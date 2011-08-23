﻿#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Player
{
  public class BDOSDRenderer
  {
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineSetAlphaBlend(UInt32 alphaBlend);

    private static BDOSDRenderer _instance;     

    /// <summary>
    /// The coordinates of current vertex buffer
    /// </summary>
    private int _wx, _wy, _wwidth, _wheight;

    /// <summary>
    /// Vertex buffer for rendering OSD
    /// </summary>
    private VertexBuffer _vertexBuffer;

    /// <summary>
    /// Texture containing the whole OSD area (1920x1080)
    /// </summary>
    private Texture _OSDTexture;

    /// <summary>
    /// Lock for syncronising the texture update and rendering
    /// </summary>
    private object _OSDLock;

    private BDOSDRenderer()
    {
      _OSDLock = new Object();
    }

    public static BDOSDRenderer GetInstance()
    {
      if (_instance == null)
      {
        _instance = new BDOSDRenderer();
      }
      return _instance;
    }

    public static void Release()
    {
      _instance = null;
    }
    
    public void DrawItem(OSDTexture item)
    {
      try
      {
        lock (_OSDLock)
        {
          if (_OSDTexture == null || _OSDTexture.Disposed)
          {
            _OSDTexture = new Texture(GUIGraphicsContext.DX9Device, 1920, 1080, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
          }

          if (item.texture != null && item.width > 0 && item.height > 0)
          {
            Rectangle sourceRect = new Rectangle(0, 0, item.width, item.height);
            Rectangle dstRect = new Rectangle(item.x, item.y, item.width, item.height);

            Texture itemTexture = new Texture(item.texture);

            GUIGraphicsContext.DX9Device.StretchRectangle(itemTexture.GetSurfaceLevel(0), sourceRect,
              _OSDTexture.GetSurfaceLevel(0), dstRect, 0);
          }
          else
          {
            Rectangle dstRect = new Rectangle(0, 0, 1920, 1080);
            GUIGraphicsContext.DX9Device.ColorFill(_OSDTexture.GetSurfaceLevel(0), dstRect, 0x00000000);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void Render()
    {
      lock (_OSDLock)
      {
        // Store current settings so they can be restored when we are done
        VertexFormats vertexFormat = GUIGraphicsContext.DX9Device.VertexFormat;        
        
        try
        {
          if (_OSDTexture == null || _OSDTexture.Disposed)
          {
            _OSDTexture = new Texture(GUIGraphicsContext.DX9Device, 1920, 1080, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
          }

          int wx = 0, wy = 0, wwidth = 0, wheight = 0;
          float rationW = 1.0f, rationH = 1.0f;

          if (GUIGraphicsContext.IsFullScreenVideo)
          {
            rationH = (float)GUIGraphicsContext.Height / 1080.0f;
            rationW = rationH;

            wx = GUIGraphicsContext.OverScanLeft;
            wy = GUIGraphicsContext.OverScanTop;
          }
          else // Video overlay
          {
            rationH = (float)GUIGraphicsContext.VideoWindow.Height / 1080.0f;
            rationW = rationH;

            wx = GUIGraphicsContext.VideoWindow.Right - (GUIGraphicsContext.VideoWindow.Width);
            wy = GUIGraphicsContext.VideoWindow.Top;
          }

          wwidth = (int)(1920.0f * rationW);
          wheight = (int)(1080.0f * rationH);
          
          FontEngineSetAlphaBlend(1); //TRUE
          CreateVertexBuffer(wx, wy, wwidth, wheight);

          // Make sure D3D objects haven't been disposed for some reason. This would cause
          // an access violation on native side, causing Skin Engine to halt rendering
          if (!_OSDTexture.Disposed && !_vertexBuffer.Disposed)
          {
            GUIGraphicsContext.DX9Device.SetStreamSource(0, _vertexBuffer, 0);
            GUIGraphicsContext.DX9Device.SetTexture(0, _OSDTexture);
            GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
            GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
          }
          else
          {
            Log.Debug("OSD renderer: D3D resource was disposed! Not trying to render the texture");
          }
        }
        catch (Exception e)
        {
          Log.Error(e);
        }

        try
        {
          // Restore device settings
          GUIGraphicsContext.DX9Device.SetTexture(0, null);
          GUIGraphicsContext.DX9Device.VertexFormat = vertexFormat;
        }
        catch (Exception e)
        {
          Log.Error(e);
        }
      }
    }

    private void CreateVertexBuffer(int wx, int wy, int wwidth, int wheight)
    {
      if (_vertexBuffer == null)
      {
        _vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedTextured),
                                        4, GUIGraphicsContext.DX9Device,
                                        Usage.Dynamic | Usage.WriteOnly, 
                                        CustomVertex.TransformedTextured.Format,
                                        GUIGraphicsContext.GetTexturePoolType());
        _wx = _wy = _wwidth = _wheight = 0;
      }

      if (_wx != wx || _wy != wy || _wwidth != wwidth || _wheight != wheight)
      {
        CustomVertex.TransformedTextured[] verts = (CustomVertex.TransformedTextured[])_vertexBuffer.Lock(0, 0);

        // upper left
        verts[0] = new CustomVertex.TransformedTextured(wx, wy, 0, 1, 0, 0);

        // upper right
        verts[1] = new CustomVertex.TransformedTextured(wx + wwidth, wy, 0, 1, 1, 0);

        // lower left
        verts[2] = new CustomVertex.TransformedTextured(wx, wy + wheight, 0, 1, 0, 1);

        // lower right
        verts[3] = new CustomVertex.TransformedTextured(wx + wwidth, wy + wheight, 0, 1, 1, 1);

        _vertexBuffer.SetData(verts, 0, LockFlags.None);
        
        // remember what the vertexBuffer is set to
        _wy = wy;
        _wx = wx;
        _wheight = wheight;
        _wwidth = wwidth;
      }
    }
  }
}