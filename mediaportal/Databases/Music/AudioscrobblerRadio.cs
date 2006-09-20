#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Threading;

using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TagReader;

namespace MediaPortal.Music.Database
{
  public enum StreamPlaybackState : int
  {
    offline = 0,
    initialized = 1,
    starting = 2,
    streaming = 3,
    paused = 4,
    nocontent = 5,
  }

  public enum StreamControls : int
  {
    skiptrack = 0,
    lovetrack = 1,
  }

  public class AudioscrobblerRadio //: IPlayer
  {
    // constructor
    public AudioscrobblerRadio()
    {
      LoadSettings();
    }

    protected AudioscrobblerUtils InfoScrobbler;

    private string _currentRadioURL = String.Empty;
    private string _currentSession = String.Empty;
    private string _currentUser = String.Empty;
    private bool _isSubscriber = false;
    private bool _recordToProfile = true;
    private bool _discoveryMode = false;

    private MusicTag CurrentSongTag;
    private System.Timers.Timer _nowPlayingTimer;
    private StreamPlaybackState _currentState = StreamPlaybackState.offline;

    // TO DO: 
    // Steps to get a stream:
    // 1. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://globaltags/alternative%20rock,ebm,progressive%20rock
    // or http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://artist/Subway%20To%20Sally/similarartists
    // or http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://user/f1n4rf1n/personal
    // or http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://user/f1n4rf1n/neighbours
    // or http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://user/f1n4rf1n/recommended
    // or http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://group/MediaPortal%20Users

    // 2. http.request.uri = Request URI: http://streamer1.last.fm/last.mp3?Session=e5b0c80f5b5d0937d407fb77a913cb6a
    // 3. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/control.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&command=rtp
    // 4. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://settings/discovery/off

    // TASKS:
    // Stopwatch and Parser for nowplaying
    // 5. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/np.php?session=e5b0c80f5b5d0937d407fb77a913cb6a
    // 6. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/artistmetadata.php?artist=Sportfreunde%20Stiller&lang=en
    // 7. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/metadata.php?artist=Sportfreunde%20Stiller&track=Alles%20Das&album=Macht%20doch%20was%20ihr%20wollt%20-%20Ich%20geh%2527%20jetzt%2521

    // SKIP Button
    // 8. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/control.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&command=skip

    //price=
    //shopname=
    //clickthrulink=
    //streaming=true
    //discovery=0
    //station=Global Tag Radio: metal, viking metal, Melodic Death Metal
    //artist=Sonata Arctica
    //artist_url=http://www.last.fm/music/Sonata+Arctica
    //track=8th Commandment
    //track_url=http://www.last.fm/music/Sonata+Arctica/_/8th+Commandment
    //album=Ecliptica
    //album_url=http://www.last.fm/music/Sonata+Arctica/Ecliptica
    //albumcover_small=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //albumcover_medium=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //albumcover_large=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //trackduration=222
    //radiomode=1
    //recordtoprofile=1

    private void LoadSettings()
    {
      CurrentSongTag = new MusicTag();
      _nowPlayingTimer = new System.Timers.Timer();
      _nowPlayingTimer.Interval =  180000;
      _nowPlayingTimer.Elapsed += new ElapsedEventHandler(OnTimerTick);

      InfoScrobbler = AudioscrobblerUtils.Instance;

      _currentUser = AudioscrobblerBase.Username;

      if (_currentUser.Length > 0)
      {
        _currentSession = AudioscrobblerBase.RadioSession;

        if (_currentSession != String.Empty)
        {
          _isSubscriber = AudioscrobblerBase.Subscriber;
          _currentRadioURL = "http://streamer1.last.fm/last.mp3?Session=" + _currentSession;
          _currentState = StreamPlaybackState.initialized;

          //List<String> MyTags = new List<string>();
          //MyTags.Add("cover");
          //MyTags.Add("melodic death metal");
          //TuneIntoTags(MyTags);
          //TuneIntoPersonalRadio();
          //TuneIntoPersonalRadio(_currentUser);  <-- subscriber only
          //TuneIntoGroupRadio("MediaPortal Users");
          TuneIntoRecommendedRadio(_currentUser);
        }
      }
    }

    /// <summary>
    /// URL for playback with buffering audioplayers
    /// </summary>
    public string CurrentStream
    {
      get { return _currentRadioURL; }
    }

    public StreamPlaybackState CurrentStreamState
    {
      get { return _currentState; }

      set
      {
        if (value != _currentState)
          _currentState = value;
        Log.Debug("AudioscrobblerRadio: Setting CurrentStreamState to {0}", _currentState.ToString());
      }
    }

    /// <summary>
    /// Get/Set if you like your radio songs to appear in your last.fm profile
    /// </summary>
    public bool SubmitRadioSongs
    {
      get { return _recordToProfile; }

      set
      {
        if (value != _recordToProfile)
        {
          ToggleRecordToProfile(value);
        }
      }
    }

    public bool DiscoveryMode
    {
      get { return _discoveryMode; }

      set
      {
        if (value != _discoveryMode)
        {
          ToggleDiscoveryMode(value);
        }
      }
    }



    #region Control functions

    public bool PlayStream()
    {
      _currentState = StreamPlaybackState.starting;
      // often the buffer is too slow for the playback to start
      for (int i = 0; i < 3; i++)
      {
        if (g_Player.Play(_currentRadioURL))
        {
          _currentState = StreamPlaybackState.streaming;
          Thread.Sleep(50);
          ToggleRecordToProfile(_recordToProfile);
          Thread.Sleep(50);
          ToggleDiscoveryMode(_discoveryMode);
          Thread.Sleep(50);
          _nowPlayingTimer.Start();
          Thread.Sleep(50);
          UpdateNowPlaying();

          return true;
        }
      }
      return false;
    }

    //public override bool Playing
    //{
    //  get
    //  {
    //    return _currentState == StreamPlaybackState.streaming;
    //  }
    //}

    //public override string CurrentFile
    //{
    //  get
    //  {
    //    return _currentRadioURL;
    //  }
    //}

    //public override double Duration
    //{
    //  get
    //  {
    //    double tmpDuration = 0;
    //    if (CurrentSongTag != null)
    //    {
    //      tmpDuration = Convert.ToDouble(CurrentSongTag.Duration);
    //    }
    //    return tmpDuration;
    //  }
    //}

    //public override bool HasVideo
    //{
    //  get
    //  {
    //    return false;
    //  }
    //}

    //public override bool IsRadio
    //{
    //  get
    //  {
    //    return true;
    //  }
    //}

    //public override bool Paused
    //{
    //  get
    //  {
    //    return _currentState == StreamPlaybackState.paused;
    //  }
    //}

    public void UpdateNowPlaying()
    {
      SendCommandRequest(@"http://ws.audioscrobbler.com/radio/np.php?session=" + _currentSession);
    }

    public bool ToggleRecordToProfile(bool submitTracks_)
    {
      bool success = false;

      if (submitTracks_)
      {
        if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=rtp"))
        {
          success = true;
          _recordToProfile = true;
          Log.Info("AudioscrobblerRadio: Enabled submitting of radio tracks to profile");
        }
      }
      else
        if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=nortp"))
        {
          success = true;
          _recordToProfile = false;
          Log.Info("AudioscrobblerRadio: Disabled submitting of radio tracks to profile");
        }
      return success;
    }

    public bool ToggleDiscoveryMode(bool enableDiscovery_)
    {
      bool success = false;
      string actionCommand = enableDiscovery_ ? "on" : "off";

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://settings/discovery/" + actionCommand))
      {
        success = true;
        _discoveryMode = enableDiscovery_;
        Log.Info("AudioscrobblerRadio: Toggled discovery mode {0}", actionCommand);
      }

      return success;
    }

    public bool SendControlCommand(StreamControls command_)
    {
      bool success = false;
      if (_currentState == StreamPlaybackState.streaming)
      {
        string baseUrl = @"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession;

        switch (command_)
        {
          case StreamControls.skiptrack:
            if (SendCommandRequest(baseUrl + @"&command=skip"))
            {
              Thread.Sleep(750);
              Log.Info("AudioscrobblerRadio: Successfully send skip command");
              Thread.Sleep(750);
              success = true;
              Thread.Sleep(750);
              // the website has to refresh therefore we wait a little bit
              Thread.Sleep(750);              
              UpdateNowPlaying();
            }
            break;
          case StreamControls.lovetrack:
            if (SendCommandRequest(baseUrl + @"&command=love"))
            {
              Log.Info("AudioscrobblerRadio: Track added to loved tracks list");
              success = true;
            }
            break;
        }
      }
      else
        Log.Info("AudioscrobblerRadio: Currently not streaming - ignoring command");

      return success;
    }
    #endregion

    #region Tuning functions
    public bool TuneIntoPersonalRadio(string username_)
    {
      string TuneUser = InfoScrobbler.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/personal"))
      {
        Log.Info("AudioscrobblerRadio: Tune into personal station of: {0}", username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoLovedTracks(string username_)
    {
      string TuneUser = InfoScrobbler.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/loved"))
      {
        Log.Info("AudioscrobblerRadio: Tune into loved tracks of: {0}", username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoGroupRadio(string groupname_)
    {
      string TuneGroup = InfoScrobbler.getValidURLLastFMString(groupname_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://group/" + TuneGroup))
      {
        Log.Info("AudioscrobblerRadio: Tune into group radio for: {0}", groupname_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoRecommendedRadio(string username_)
    {
      string TuneUser = InfoScrobbler.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/recommended"))
      {
        Log.Info("AudioscrobblerRadio: Tune into recommended station for: {0}", username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoArtist(string artist_)
    {
      string TuneArtist = InfoScrobbler.getValidURLLastFMString(artist_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://artist/" + TuneArtist + "/similarartists"))
      {
        Log.Info("AudioscrobblerRadio: Tune into artists similar to: {0}", artist_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoTags(List<String> tags_)
    {
      string TuneTags = String.Empty;

      foreach (string singleTag in tags_)
      {
        TuneTags += InfoScrobbler.getValidURLLastFMString(singleTag) + ",";
      }
      // remove trailing comma
      TuneTags = TuneTags.Remove(TuneTags.Length - 1);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://globaltags/" + TuneTags))
      {
        Log.Info("AudioscrobblerRadio: Tune into tags: {0}", TuneTags);
        return true;
      }
      else
        return false;
    }
    #endregion

    #region Network related
    private bool SendCommandRequest(string url_)
    {
      HttpWebRequest request = null;

      // send the command
      try
      {
        
        request = (HttpWebRequest)WebRequest.Create(url_);
        request.Timeout = 20000;
        if (request == null)
          throw (new Exception());
      }
      catch (Exception e)
      {
        Log.Error("AudioscrobblerRadio: SendCommandRequest failed - {0}", e.Message);
        return false;
      }

      StreamReader reader = null;
      HttpStatusCode responseCode = new HttpStatusCode();

      // get the response
      try
      {

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        if (response == null)
          throw (new Exception());

        reader = new StreamReader(response.GetResponseStream());
        responseCode = response.StatusCode;
      }
      catch (Exception e)
      {
        Log.Error("AudioscrobblerRadio: SendCommandRequest: Response failed {0}", e.Message);
        return false;
      }

      // parse the response
      try
      {
        string responseMessage = reader.ReadLine();

        if (responseCode == HttpStatusCode.OK)
        {
          if (responseMessage.StartsWith("response=OK"))
            return true;

          if (responseMessage.StartsWith("price="))
          {
            ParseNowPlaying(reader);
            return true;
          }
        }
        else
        {
          string logmessage = "AudioscrobblerRadio: ***** Unknown response! - " + responseMessage;
          while ((responseMessage = reader.ReadLine()) != null)
            logmessage += "\n" + responseMessage;

          if (logmessage.Contains("Not enough content"))
          {
            _currentState = StreamPlaybackState.nocontent;
            Log.Warn("AudioscrobblerRadio: Not enough content left to play this station");
            return false;
          }
          else
            Log.Warn(logmessage);
        }
      }
      catch (Exception e)
      {
        Log.Error("AudioscrobblerRadio: SendCommandRequest: Parsing response failed {0}", e.Message);
        return false;
      }

      return false;
    }
    # endregion

    #region Response parser
    private void ParseNowPlaying(StreamReader responseStream_)
    {
      List<String> NowPlayingInfo = new List<string>();
      String tmpString = String.Empty;
      CurrentSongTag.Clear();

      try
      {
        while ((tmpString = responseStream_.ReadLine()) != null)
          NowPlayingInfo.Add(tmpString);

        foreach (String token in NowPlayingInfo)
        {
          if (token.StartsWith("artist="))
          {
            if (token.Length > 7)
              CurrentSongTag.Artist = token.Substring(7);
          }
          else if (token.StartsWith("album="))
          {
            if (token.Length > 6)
              CurrentSongTag.Album = token.Substring(6);
          }
          else if (token.StartsWith("track="))
          {
            if (token.Length > 6)
              CurrentSongTag.Title = token.Substring(6);
          }
          else if (token.StartsWith("station="))
          {
            if (token.Length > 8)
              CurrentSongTag.Genre = token.Substring(8);
          }
          else if (token.StartsWith("albumcover_large="))
          {
            if (token.Length > 17)
              CurrentSongTag.Comment = token.Substring(17);
          }
          else if (token.StartsWith("trackduration="))
          {
            if (token.Length > 14)
            {
              int trackLength = Convert.ToInt32(token.Substring(14));
              CurrentSongTag.Duration = trackLength;

              if (trackLength > 0)
              {
                _nowPlayingTimer.Stop();
                _nowPlayingTimer.Interval = trackLength * 1000;                
                _nowPlayingTimer.Start();
              }
            }
          }
        }
        GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentSongTag.Artist);
        GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentSongTag.Album);
        GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentSongTag.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentSongTag.Genre);
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", CurrentSongTag.Comment);
        GUIPropertyManager.SetProperty("#trackduration", Convert.ToString(CurrentSongTag.Duration));


        Log.Info("AudioscrobblerRadio: Current track: {0} [{1}] - {2} ({3})", CurrentSongTag.Artist, CurrentSongTag.Album, CurrentSongTag.Title, Util.Utils.SecondsToHMSString(CurrentSongTag.Duration));
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerRadio: Error parsing now playing info: {0}", ex.Message);
      }
    }
    #endregion

    #region Utils
    private void OnTimerTick(object trash_, ElapsedEventArgs args_)
    {     
      UpdateNowPlaying();
    }
    #endregion
  }
}
