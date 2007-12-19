﻿#region Copyright (C) 2007 Team MediaPortal

/*
    Copyright (C) 2007 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Cache;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using Microsoft.DirectX.Direct3D;
using SkinEngine.Players;
using SkinEngine.Players.Geometry;
using SkinEngine.Thumbnails;
using Geometry = SkinEngine.Players.Geometry.Geometry;

namespace SkinEngine
{
  public class TextureAsset : IAsset
  {
    #region variables

    private enum State
    {
      Unknown,
      Creating,
      Created,
      DoesNotExists
    } ;

    private Texture _texture = null;
    private int _width;
    private int _height;
    private float _maxV;
    private float _maxU;
    private readonly string _textureName;
    private DateTime _lastTimeUsed = DateTime.MinValue;
    private State _state = State.Unknown;
    private string _sourceFileName;
    private WebClient _webClient;
    private byte[] _buffer;
    private bool _useThumbnail = true;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureAsset"/> class.
    /// </summary>
    /// <param name="textureName">Name of the texture.</param>
    public TextureAsset(string textureName)
    {
      _textureName = textureName;
      //Allocate();
    }

    public TextureAsset(Texture texture, float maxU, float maxV)
    {
      _texture = texture;
      _maxU = maxU;
      _maxV = maxV;
    }

    public Texture Texture
    {
      get { return _texture; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use a thumbnail or the original image
    /// </summary>
    /// <value><c>true</c> if using thumbnail; otherwise, <c>false</c>.</value>
    public bool UseThumbNail
    {
      get { return _useThumbnail; }
      set { _useThumbnail = value; }
    }

    /// <summary>
    /// Gets the name of the texture
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get { return _textureName; }
    }

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width
    {
      get { return _width; }
    }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height
    {
      get { return _height; }
    }

    /// <summary>
    /// Gets the max U of the textire.
    /// </summary>
    /// <value>The max U.</value>
    public float MaxU
    {
      get { return _maxU; }
    }

    /// <summary>
    /// Gets the max V of the texture
    /// </summary>
    /// <value>The max V.</value>
    public float MaxV
    {
      get { return _maxV; }
    }

    public bool DoesExists
    {
      get
      {
        if (!IsAllocated)
        {
          Allocate();
        }
        return (_state == State.Created);
      }
    }

    /// <summary>
    /// Loads the specified texture from the file.
    /// </summary>
    public void Allocate()
    {
      if (_textureName == null || _textureName.Length == 0)
      {
        _state = State.DoesNotExists;
        return;
      }
      if (_state == State.DoesNotExists)
      {
        return;
      }
      byte[] thumbData = null;
      ImageInformation info = new ImageInformation();


      if (_state == State.Unknown)
      {
        if (SkinContext.Theme.HasImage(_textureName))
        {
          _sourceFileName = String.Format(@"skin\{0}\themes\{1}\media\{2}", SkinContext.SkinName, SkinContext.ThemeName, SkinContext.Theme.GetImage(_textureName));
          if (File.Exists(_sourceFileName))
          {
            _state = State.Created;
          }
          else
          {
            _sourceFileName = String.Format(@"skin\{0}\media\{1}", SkinContext.SkinName, SkinContext.Theme.GetImage(_textureName));
            if (File.Exists(_sourceFileName))
            {
              _state = State.Created;
            }
          }
        }
        else
        {
          _sourceFileName = String.Format(@"skin\{0}\themes\{1}\media\{2}", SkinContext.SkinName, SkinContext.ThemeName, _textureName);
          if (File.Exists(_sourceFileName))
          {
            _state = State.Created;
          }
          else
          {
            _sourceFileName = String.Format(@"skin\{0}\media\{1}", SkinContext.SkinName, _textureName);
            if (File.Exists(_sourceFileName))
            {
              _state = State.Created;
            }
          }
        }
        if (_state == State.Unknown)
        {
          //if (File.Exists(_textureName))
          //{
          //  _sourceFileName = _textureName;
          //  _state = State.Created;
          //}
          //else
          {
            Uri uri = null;
            if (!Uri.TryCreate(_textureName, UriKind.Absolute, out uri))
            {
              ServiceScope.Get<ILogger>().Error("Cannot open texture :{0}", _textureName);
              _state = State.DoesNotExists;
              return;
            }


            if (!uri.IsFile)
            {
              if (_state == State.Unknown)
              {
                if (_webClient == null)
                {
                  _state = State.Creating;
                  _webClient = new WebClient();
                  //_webClient.Proxy = null;
                  _webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
                  _webClient.DownloadDataCompleted += _webClient_DownloadDataCompleted;
                  _webClient.DownloadDataAsync(uri);
                  return;
                }
              }
            }
            else
            {
              _sourceFileName = uri.LocalPath;
              if (UseThumbNail)
              {
                bool exists = Generator.Instance.Exists(_sourceFileName);
                if (exists)
                {
                  thumbData = Generator.Instance.GetThumbNail(_sourceFileName);
                  _state = State.Created;
                }
                else if (Generator.Instance.IsCreating(_sourceFileName))
                {
                  _state = State.Creating;
                }
                else
                {
                  Generator.Instance.CreateThumbnail(_sourceFileName);
                  _state = State.Creating;
                }
              }
              else
              {
                _state = State.Created;
              }
            }
          }
        }
      }

      if (_state == State.Creating)
      {
        if (_webClient == null)
        {
          if (Generator.Instance.IsCreating(_sourceFileName) == false)
          {
            if (Generator.Instance.Exists(_sourceFileName))
            {
              _state = State.Created;
              thumbData = Generator.Instance.GetThumbNail(_sourceFileName);
            }
            else
            {
              _state = State.DoesNotExists;
            }
          }
        }
      }

      if (_state == State.Creating)
      {
        return;
      }
      if (_state == State.DoesNotExists)
      {
        return;
      }
      if (_webClient != null)
      {
        thumbData = _buffer;
        _buffer = null;
        _webClient.Dispose();
        _webClient = null;
      }

      if (_texture != null)
      {
        Free();
      }
      if (thumbData != null)
      {
        using (MemoryStream stream = new MemoryStream(thumbData))
        {
          try
          {
            //            ServiceScope.Get<ILogger>().Debug("TEXTURE alloc from thumbdata:{0}", _textureName);
            if (UseThumbNail)
            {
              _texture =
                TextureLoader.FromStream(GraphicsDevice.Device, stream, 0, 0, 1, Usage.None, Format.A8R8G8B8, Pool.Default,
                                         Filter.None, Filter.None, 0, ref info);
              ContentManager.TextureReferences++;
            }
            else
            {
              ImageInformation imgInfo = TextureLoader.ImageInformationFromStream(stream);
              if (imgInfo.Width > GraphicsDevice.Width || imgInfo.Height > GraphicsDevice.Height)
              {
                int destWidth = GraphicsDevice.Width;
                int destHeight = GraphicsDevice.Height;
                GeometryNormal g = new GeometryNormal();
                Geometry gem = SkinContext.Geometry;
                using (Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb))
                {
                  using (Image imgSource = Image.FromStream(stream))
                  {
                    using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
                    {
                      Rectangle rSource;
                      Rectangle rDest;
                      float sourcFrameRatio = imgSource.Width / ((float)imgSource.Height);
                      CropSettings crop = new CropSettings();
                      gem.ImageWidth = imgSource.Width;
                      gem.ImageHeight = imgSource.Height;
                      gem.ScreenWidth = GraphicsDevice.Width;
                      gem.ScreenHeight = GraphicsDevice.Height;
                      g.GetWindow(gem, sourcFrameRatio, out rSource, out rDest, crop);
                      grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                      grPhoto.DrawImage(imgSource,
                                        new Rectangle(rDest.X, rDest.Y, rDest.Width, rDest.Height),
                                        new Rectangle(rSource.X, rSource.Y, rSource.Width, rSource.Height),
                                        GraphicsUnit.Pixel);
                    }
                  }
                  using (MemoryStream streamMem = new MemoryStream())
                  {
                    bmPhoto.Save(streamMem, ImageFormat.Bmp);
                    streamMem.Seek(0, SeekOrigin.Begin);
                    _texture =
                      TextureLoader.FromStream(GraphicsDevice.Device, streamMem, 0, 0, 1, Usage.None, Format.Unknown,
                                               Pool.Managed, Filter.None, Filter.None, 0, ref info);
                    ContentManager.TextureReferences++;
                  }
                }
              }
              else
              {
                _texture =
                  TextureLoader.FromStream(GraphicsDevice.Device, stream, 0, 0, 1, Usage.None, Format.Unknown,
                                         Pool.Managed, Filter.None, Filter.None, 0, ref info);
                ContentManager.TextureReferences++;
              }
            }
          }
          catch (Exception)
          {
            _state = State.DoesNotExists;
          }
        }
      }
      else
      {
        //        ServiceScope.Get<ILogger>().Debug("TEXTURE alloc from file:{0}", _sourceFileName);
        try
        {
          if (UseThumbNail)
          {
            _texture =
              TextureLoader.FromFile(GraphicsDevice.Device, _sourceFileName, 0, 0, 1, Usage.None, Format.A8R8G8B8,
                                     Pool.Default, Filter.None, Filter.None, 0, ref info);
            ContentManager.TextureReferences++;
          }
          else
          {
            ImageInformation imgInfo = TextureLoader.ImageInformationFromFile(_sourceFileName);
            if (imgInfo.Width > GraphicsDevice.Width || imgInfo.Height > GraphicsDevice.Height)
            {
              int destWidth = GraphicsDevice.Width;
              int destHeight = GraphicsDevice.Height;
              GeometryNormal g = new GeometryNormal();
              Geometry gem = SkinContext.Geometry;
              using (Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb))
              {
                using (Image imgSource = Image.FromFile(_sourceFileName))
                {
                  using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
                  {
                    Rectangle rSource;
                    Rectangle rDest;
                    float sourcFrameRatio = imgSource.Width / ((float)imgSource.Height);
                    CropSettings crop = new CropSettings();
                    gem.ImageWidth = imgSource.Width;
                    gem.ImageHeight = imgSource.Height;
                    gem.ScreenWidth = GraphicsDevice.Width;
                    gem.ScreenHeight = GraphicsDevice.Height;
                    g.GetWindow(gem, sourcFrameRatio, out rSource, out rDest, crop);
                    grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    grPhoto.DrawImage(imgSource,
                                      new Rectangle(rDest.X, rDest.Y, rDest.Width, rDest.Height),
                                      new Rectangle(rSource.X, rSource.Y, rSource.Width, rSource.Height),
                                      GraphicsUnit.Pixel);
                  }
                }
                using (MemoryStream stream = new MemoryStream())
                {
                  bmPhoto.Save(stream, ImageFormat.Bmp);
                  stream.Seek(0, SeekOrigin.Begin);
                  _texture =
                    TextureLoader.FromStream(GraphicsDevice.Device, stream, 0, 0, 1, Usage.None, Format.Unknown,
                                             Pool.Managed, Filter.None, Filter.None, 0, ref info);
                  ContentManager.TextureReferences++;
                }
              }
            }
            else
            {
              _texture =
                TextureLoader.FromFile(GraphicsDevice.Device, _sourceFileName, 0, 0, 1, Usage.None, Format.Unknown,
                                       Pool.Managed, Filter.None, Filter.None, 0, ref info);
              ContentManager.TextureReferences++;
            }
          }
        }
        catch (Exception)
        {
          _state = State.DoesNotExists;
        }
      }
      if (_texture != null)
      {
        SurfaceDescription desc = _texture.GetLevelDescription(0);
        _width = info.Width;
        _height = info.Height;
        _maxU = info.Width / ((float)desc.Width);
        _maxV = info.Height / ((float)desc.Height);
      }
    }

    private void _webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        ServiceScope.Get<ILogger>().Error("Contentmanager:Failed to download {0}", _textureName);
        ServiceScope.Get<ILogger>().Error(e.Error.Message);
        _webClient.Dispose();
        _webClient = null;
        _state = State.DoesNotExists;
        return;
      }
      Trace.WriteLine("downloaded " + _textureName);
      _buffer = e.Result;
      _state = State.Created;
    }

    /// <summary>
    /// Draws the image.
    /// </summary>
    public void Draw(int streamNumber)
    {
      if (!IsAllocated)
      {
        Allocate();
      }
      if (!IsAllocated)
      {
        return;
      }
      lock (_texture)
      {
        GraphicsDevice.Device.SetTexture(streamNumber, _texture);
        GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
      }
      _lastTimeUsed = SkinContext.Now;
    }

    /// <summary>
    /// Draws the image.
    /// </summary>
    public void Set(int streamNumber)
    {
      if (!IsAllocated)
      {
        Allocate();
      }
      if (!IsAllocated)
      {
        return;
      }

      GraphicsDevice.Device.SetTexture(streamNumber, _texture);
      _lastTimeUsed = SkinContext.Now;
    }

    #region IAsset Members

    /// <summary>
    /// Gets a value indicating the asset is allocated
    /// </summary>
    /// <value><c>true</c> if this asset is allocated; otherwise, <c>false</c>.</value>
    public bool IsAllocated
    {
      get { return (_texture != null); }
    }

    /// <summary>
    /// Gets a value indicating whether this asset can be deleted.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this asset can be deleted; otherwise, <c>false</c>.
    /// </value>
    public bool CanBeDeleted
    {
      get
      {
        if (!IsAllocated)
        {
          return false;
        }
        TimeSpan ts = SkinContext.Now - _lastTimeUsed;
        if (UseThumbNail)
        {
          if (ts.TotalSeconds >= 5)
          {
            return true;
          }
        }
        if (ts.TotalSeconds >= 1)
        {
          return true;
        }

        return false;
      }
    }

    /// <summary>
    /// Frees this asset.
    /// </summary>
    public void Free()
    {
      //      Trace.WriteLine(String.Format("  Dispose texture:{0}", _textureName));
      if (_texture != null)
      {
        lock (_texture)
        {
          //ServiceScope.Get<ILogger>().Debug("TEXTURE dispose from {0}", _textureName);
          _texture.Dispose();
          _texture = null;

          ContentManager.TextureReferences--;
        }
      }
      _state = State.Unknown;
    }

    #endregion
  }
}
