#region Copyright (C) 2005-2011 Team MediaPortal

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
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using MediaPortal.ExtensionMethods;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Container for preview window - also setting video properties like title, playtime, etc for skin access
  /// </summary>
  public class GUIVideoOverlay : GUIInternalOverlayWindow, IRenderLayer
  {
    private bool _isFocused = false;
    private string _fileName = "";
    private string _program = "";

    [SkinControl(0)] protected GUIImage _videoRectangle = null;
    [SkinControl(1)] protected GUIVideoControl _videoWindow = null;
    [SkinControl(2)] protected GUILabelControl _labelPlayTime = null;
    [SkinControl(3)] protected GUIImage _imagePlayLogo = null;
    [SkinControl(4)] protected GUIImage _imagePauseLogo = null;
    [SkinControl(5)] protected GUIFadeLabel _labelInfo = null;
    [SkinControl(6)] protected GUIImage _labelBigPlayTime = null;
    [SkinControl(7)] protected GUIImage _imageFastForward = null;
    [SkinControl(8)] protected GUIImage _imageRewind = null;

    private string _thumbLogo = "";
    private bool _didRenderLastTime = false;

    public GUIVideoOverlay()
    {
      GetID = (int)Window.WINDOW_VIDEO_OVERLAY;
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(OnVideoChanged);
    }

    ~GUIVideoOverlay()
    {
      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(OnVideoChanged);
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\videoOverlay.xml");
      GetID = (int)Window.WINDOW_VIDEO_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.VideoOverlay);
      return result;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }

    public override void Render(float timePassed) {}

    private void OnUpdateState(bool render)
    {
      if (_didRenderLastTime != render)
      {
        _didRenderLastTime = render;
        if (render)
        {
          QueueAnimation(AnimationType.WindowOpen);
        }
        else
        {
          QueueAnimation(AnimationType.WindowClose);
        }
      }
    }

    public override bool DoesPostRender()
    {
      if (!g_Player.Playing)
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if ((g_Player.IsRadio || g_Player.IsMusic))
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if (!g_Player.IsVideo && !g_Player.IsDVD && !g_Player.IsTVRecording && !g_Player.IsTV)
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      if (g_Player.CurrentFile != _fileName)
      {
        _fileName = g_Player.CurrentFile;
        SetCurrentFile(_fileName);
      }

      if (g_Player.IsTV && (_program != GUIPropertyManager.GetProperty("#TV.View.title")) && g_Player.IsTimeShifting)
      {
        _program = GUIPropertyManager.GetProperty("#TV.View.title");
        GUIPropertyManager.SetProperty("#Play.Current.Title", GUIPropertyManager.GetProperty("#TV.View.channel"));
        GUIPropertyManager.SetProperty("#Play.Current.Genre", _program);
        GUIPropertyManager.SetProperty("#Play.Current.Year", GUIPropertyManager.GetProperty("#TV.View.genre"));
        GUIPropertyManager.SetProperty("#Play.Current.Director",
                                       GUIPropertyManager.GetProperty("#TV.View.start") + " - " +
                                       GUIPropertyManager.GetProperty("#TV.View.stop"));
      }

      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // Too late to schedule a fade out animation. We are already rendering full screen video
        //OnUpdateState(false);
        //return base.IsAnimating(AnimationType.WindowClose);
        return false;
      }
      if (GUIGraphicsContext.Calibrating)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if (!GUIGraphicsContext.Overlay)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      OnUpdateState(true);
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer != 2)
      {
        return;
      }
      if (!base.IsAnimating(AnimationType.WindowClose))
      {
        if (GUIPropertyManager.GetProperty("#Play.Current.Thumb") != _thumbLogo)
        {
          _thumbLogo = GUIPropertyManager.GetProperty("#Play.Current.Thumb");
          if (g_Player.CurrentFile != _fileName)
          {
            _fileName = g_Player.CurrentFile;
            SetCurrentFile(_fileName);
          }
        }

        //        int speed = g_Player.Speed;
        //        double pos = g_Player.CurrentPosition;    // Should not called from this thread. !
        if (_imagePlayLogo != null)
        {
          _imagePlayLogo.Visible = (g_Player.Paused == false);
        }

        if (_imagePauseLogo != null)
        {
          _imagePauseLogo.Visible = false; // (g_Player.Paused == true);
        }

        if (_imageFastForward != null)
        {
          _imageFastForward.Visible = false; // (g_Player.Speed>1);
        }

        if (_imageRewind != null)
        {
          _imageRewind.Visible = false; // (g_Player.Speed<0);
        }

        if (_videoRectangle != null)
        {
          if (g_Player.Playing)
          {
            _videoRectangle.Visible = GUIGraphicsContext.ShowBackground;
          }
          else
          {
            _videoRectangle.Visible = false;
          }
        }
      }
      base.Render(timePassed);
    }


    private void OnVideoChanged()
    {
      if (_videoWindow == null)
      {
        return;
      }

      if (GUIGraphicsContext.Overlay == true && GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.IsPlaying)
        //&& GUIGraphicsContext.IsPlayingVideo && !GUIGraphicsContext.IsFullScreenVideo && !g_Player.FullScreen)
      {
        if (_videoWindow.Visible == false)
        {
          _videoWindow.Visible = true;
        }
        return;
      }
      if (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.IsPlaying)
        // && GUIGraphicsContext.IsPlayingVideo && !GUIGraphicsContext.IsFullScreenVideo && !g_Player.FullScreen)
      {
        if (_videoWindow.Visible == true)
        {
          _videoWindow.Visible = false;
        }
        return;
      }
    }

    /// <summary>
    /// Examines the current playing movie and fills in all the #tags for the skin.
    /// For movies it will look in the video database for any IMDB info
    /// For record TV programs it will look in the TVDatabase for recording info 
    /// </summary>
    /// <param name="fileName">Filename of the current playing movie</param>
    /// <remarks>
    /// Function will fill in the following tags for TV programs
    /// #Play.Current.Title, #Play.Current.Plot, #Play.Current.PlotOutline #Play.Current.File, #Play.Current.Thumb, #Play.Current.Year, #Play.Current.Channel,
    /// 
    /// Function will fill in the following tags for movies
    /// #Play.Current.Title, #Play.Current.Plot, #Play.Current.PlotOutline #Play.Current.File, #Play.Current.Thumb, #Play.Current.Year
    /// #Play.Current.Director, #cast, #dvdlabel, #imdbnumber, #Play.Current.Plot, #Play.Current.PlotOutline, #rating, #tagline, #votes, #credits
    /// </remarks>
    private void SetCurrentFile(string fileName)
    {
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.Title", Util.Utils.GetFilename(fileName));
      GUIPropertyManager.SetProperty("#Play.Current.File", Path.GetFileName(fileName));
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", "");
      GUIPropertyManager.SetProperty("#Play.Current.VideoCodec.Texture", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.VideoResolution", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.AudioCodec.Texture", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.AudioChannels", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.HasSubtitles", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.AspectRatio", string.Empty);

      if ((g_Player.IsVideo || g_Player.IsDVD) && !g_Player.IsTV && g_Player.MediaInfo != null)
      {
        GUIPropertyManager.SetProperty("#Play.Current.VideoCodec.Texture",
                                       Util.Utils.MakeFileName(g_Player.MediaInfo.VideoCodec));
        GUIPropertyManager.SetProperty("#Play.Current.VideoResolution", g_Player.MediaInfo.VideoResolution);
        GUIPropertyManager.SetProperty("#Play.Current.AudioCodec.Texture",
                                       Util.Utils.MakeFileName(g_Player.MediaInfo.AudioCodec));
        GUIPropertyManager.SetProperty("#Play.Current.AudioChannels", g_Player.MediaInfo.AudioChannelsFriendly);
        GUIPropertyManager.SetProperty("#Play.Current.HasSubtitles", g_Player.MediaInfo.HasSubtitles.ToString());
        GUIPropertyManager.SetProperty("#Play.Current.AspectRatio", g_Player.MediaInfo.AspectRatio);
      }

      if (g_Player.IsDVD)
      {
        // for dvd's the file is in the form c:\media\movies\the matrix\video_ts\video_ts.ifo
        // first strip the \video_ts\video_ts.ifo
        string lowPath = fileName.ToLower();
        int index = lowPath.IndexOf("video_ts/");
        if (index < 0)
        {
          index = lowPath.IndexOf(@"video_ts\");
        }
        if (index >= 0)
        {
          fileName = fileName.Substring(0, index);
          fileName = Util.Utils.RemoveTrailingSlash(fileName);

          // get the name by stripping the first part : c:\media\movies
          string strName = fileName;
          int pos = fileName.LastIndexOfAny(new char[] {'\\', '/'});
          if (pos >= 0 && pos + 1 < fileName.Length - 1)
          {
            strName = fileName.Substring(pos + 1);
          }
          GUIPropertyManager.SetProperty("#Play.Current.Title", strName);
          GUIPropertyManager.SetProperty("#Play.Current.File", strName);

          // construct full filename as imdb info is stored...
          fileName += @"\VIDEO_TS\VIDEO_TS.IFO";
        }
      }

      bool isLive = g_Player.IsTimeShifting;
      string extension = Path.GetExtension(fileName).ToLower();
      if (extension.Equals(".sbe") || extension.Equals(".dvr-ms") ||
          (extension.Equals(".ts") && !isLive || g_Player.IsTVRecording))
      {
        // this is a recorded movie.
        // check the TVDatabase for the description,genre,title,...
        if (g_Player.currentTitle != "")
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", g_Player.currentTitle);
          GUIPropertyManager.SetProperty("#Play.Current.Plot",
                                         g_Player.currentTitle + "\n" + g_Player.currentDescription);
          GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", g_Player.currentDescription);
        }
      }

      /*if (fileName.Substring(0, 4) == "rtsp")
      {
          GUIPropertyManager.SetProperty("#Play.Current.Title", g_Player.currentTitle);
          GUIPropertyManager.SetProperty("#Play.Current.Plot", g_Player.currentTitle + "\n" + g_Player.currentDescription);
          GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", g_Player.currentDescription);
      }*/


      IMDBMovie movieDetails = new IMDBMovie();
      bool bMovieInfoFound = false;

      if (!g_Player.IsTVRecording)
      {
        if (VideoDatabase.HasMovieInfo(fileName))
        {
          VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
          bMovieInfoFound = true;
        }
        else if (File.Exists(Path.ChangeExtension(fileName, ".xml")))
        {
          MatroskaTagInfo info = MatroskaTagHandler.Fetch(Path.ChangeExtension(fileName, ".xml"));
          movieDetails.Title = info.title;
          movieDetails.Plot = info.description;
          movieDetails.Genre = info.genre;
          GUIPropertyManager.SetProperty("#Play.Current.Channel", info.channelName);
          string logo = Util.Utils.GetCoverArt(Thumbs.TVChannel, info.channelName);
          if (!Util.Utils.FileExistsInCache(logo))
          {
            logo = "defaultVideoBig.png";
          }
          GUIPropertyManager.SetProperty("#Play.Current.Thumb", logo);
          _thumbLogo = logo;
          bMovieInfoFound = true;
        }
        if (bMovieInfoFound)
        {
          movieDetails.SetPlayProperties();
        }
        else
        {
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Path = fileName;
          Util.Utils.SetThumbnails(ref item);
          GUIPropertyManager.SetProperty("#Play.Current.Thumb", item.ThumbnailImage);
        }
      }
      else if (g_Player.IsTV && g_Player.IsTimeShifting)
      {
        GUIPropertyManager.SetProperty("#Play.Current.Title", GUIPropertyManager.GetProperty("#TV.View.channel"));
        GUIPropertyManager.SetProperty("#Play.Current.Genre", GUIPropertyManager.GetProperty("#TV.View.title"));
      }
      else
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = false;
        item.Path = fileName;
        Util.Utils.SetThumbnails(ref item);
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", item.ThumbnailImage);
      }
      _thumbLogo = GUIPropertyManager.GetProperty("#Play.Current.Thumb");
    }

    public override bool Focused
    {
      get { return _isFocused; }
      set
      {
        _isFocused = value;
        if (_isFocused)
        {
          if (_videoWindow != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int)_videoWindow.GetID,
                                            0, 0, null);
            OnMessage(msg);
          }
        }
        else
        {
          foreach (GUIControl control in controlList)
          {
            control.Focus = false;
          }
        }
      }
    }

    protected override bool ShouldFocus(Action action)
    {
      return (action.wID == Action.ActionType.ACTION_MOVE_DOWN);
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
      if ((action.wID == Action.ActionType.ACTION_MOVE_UP) ||
          (action.wID == Action.ActionType.ACTION_MOVE_RIGHT))
      {
        Focused = false;
      }
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return DoesPostRender();
    }

    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 2);
    }

    #endregion

    public override void Dispose()
    {
      // this is causing Mantis 3128: No video preview in MyVideos in some situations.
      //GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(OnVideoChanged);

      _videoRectangle.SafeDispose();
      _videoWindow.SafeDispose();
      _labelPlayTime.SafeDispose();
      _imagePlayLogo.SafeDispose();
      _imagePauseLogo.SafeDispose();
      _labelInfo.SafeDispose();
      _labelBigPlayTime.SafeDispose();
      _imageFastForward.SafeDispose();
      _imageRewind.SafeDispose();

      base.Dispose();
    }
  }
}