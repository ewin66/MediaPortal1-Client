using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Dialogs;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using DirectShowLib;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>

  public partial class TvHome : System.Windows.Controls.Page//System.Windows.Window
  {
    #region variables
    private delegate void StartTimeShiftingDelegate(Channel channel);
    private delegate void EndTimeShiftingDelegate(TvResult result, VirtualCard card);
    private delegate void SeekToEndDelegate();
    private delegate void MediaPlayerErrorDelegate();
    private delegate void ConnectToServerDelegate();
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvHome"/> class.
    /// </summary>
    public TvHome()
    {
      this.ShowsNavigationUI = false;
      WindowTaskbar.Show();
      InitializeComponent();
    }
    #endregion


    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {

      // Sets keyboard focus on the first Button in the sample.
      Keyboard.Focus(buttonTvGuide);

      //try to connect to server in background...
      ConnectToServerDelegate starter = new ConnectToServerDelegate(this.ConnectToServer);
      starter.BeginInvoke(null, null);
    }

    /// <summary>
    /// background worker. Connects to server.
    /// on success call OnSucceededToConnectToServer() via dispatcher
    /// if failed call OnFailedToConnectToServer() via dispatcher
    /// </summary>
    void ConnectToServer()
    {
      try
      {
        RemoteControl.HostName = UserSettings.GetString("tv", "serverHostName");

        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

        XmlDocument doc = new XmlDocument();
        doc.Load("gentle.config");
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        node.InnerText = connectionString;
        nodeProvider.InnerText = provider;
        doc.Save("gentle.config");
        ChannelNavigator.Instance.Initialize();

        int cards = RemoteControl.Instance.Cards;
        IList channels = Channel.ListAll();
      }
      catch (Exception)
      {
        buttonTvGuide.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new ConnectToServerDelegate(OnFailedToConnectToServer));
        return;
      }
      buttonTvGuide.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new ConnectToServerDelegate(OnSucceededToConnectToServer));
    }

    /// <summary>
    /// Called when we failed to connect to server.
    /// navigate to tv-setup window
    /// </summary>
    void OnFailedToConnectToServer()
    {
      this.NavigationService.Navigate(new Uri("TvSetup.xaml", UriKind.Relative));
    }

    /// <summary>
    /// Called when we succeeded in connecting to the tvserver
    /// update infobox and show video
    /// </summary>
    void OnSucceededToConnectToServer()
    {
      WindowMediaPlayerCheck check = new WindowMediaPlayerCheck();
      if (!check.IsInstalled)
      {
        MpDialogOk dlg = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = "Error";
        dlg.Content = "Infinity needs Windows Media Player 10 or higher to playback video!";
        dlg.ShowDialog();
        return;
      }
      TsReaderCheck checkReader = new TsReaderCheck();
      {
        if (!checkReader.IsInstalled)
        {
          MpDialogOk dlg = new MpDialogOk();
          Window w = Window.GetWindow(this);
          dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlg.Owner = w;
          dlg.Title = "";
          dlg.Header = "Error";
          dlg.Content = "Infinity needs TsReader.ax to be registered!";
          dlg.ShowDialog();
          return;
        }
      }
      UpdateInfoBox();

      if (TvPlayerCollection.Instance.Count > 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        videoWindow.Fill = videoBrush;
      }
    }

    /// <summary>
    /// Called when mouse enters a button
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
      }
    }

    /// <summary>
    /// Called when tv guide button is clicked.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnTvGuideClicked(object sender, EventArgs args)
    {
      if (ChannelNavigator.Instance.CurrentGroup != null)
      {
        this.NavigationService.Navigate(new Uri("TvGuide.xaml", UriKind.Relative));
      }
    }

    /// <summary>
    /// Called when recordnow button gets clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnRecordClicked(object sender, EventArgs args)
    {
      /*  Window window = (Window)this.Parent;
        if (window.WindowState == System.Windows.WindowState.Maximized)
        {
          window.ShowInTaskbar = true;
          WindowTaskbar.Show(); ;
          window.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
          window.WindowState = System.Windows.WindowState.Normal;
        }
        else
        {
          window.ShowInTaskbar = false;
          window.WindowStyle = System.Windows.WindowStyle.None;
          WindowTaskbar.Hide(); ;
          window.WindowState = System.Windows.WindowState.Maximized;
        }*/

      //record now.
      //Are we recording this channel already?
      TvBusinessLayer layer = new TvBusinessLayer();
      TvServer server = new TvServer();
      VirtualCard card;
      Channel channel = ChannelNavigator.Instance.SelectedChannel;
      if (channel == null) return;
      if (false == server.IsRecording(channel.Name, out card))
      {
        //no then start recording
        Program prog = channel.CurrentProgram;
        if (prog != null)
        {
          MpMenu dlgMenu = new MpMenu();
          Window w = Window.GetWindow(this);
          dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgMenu.Owner = w;
          dlgMenu.Items.Clear();
          dlgMenu.Header = "Record";
          dlgMenu.Items.Add(new DialogMenuItem("current program"));
          dlgMenu.Items.Add(new DialogMenuItem("until manual stopped"));
          dlgMenu.ShowDialog();
          switch (dlgMenu.SelectedIndex)
          {
            case 0:
              {
                Schedule newSchedule = new Schedule(channel.IdChannel, channel.CurrentProgram.Title,
                          channel.CurrentProgram.StartTime, channel.CurrentProgram.EndTime);
                newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                newSchedule.RecommendedCard = ChannelNavigator.Instance.Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                newSchedule.Persist();
                server.OnNewSchedule();
              }
              break;

            case 1:
              {
                Schedule newSchedule = new Schedule(channel.IdChannel, "Manual (" + channel.Name + ")",
                                            DateTime.Now, DateTime.Now.AddDays(1));
                newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                newSchedule.RecommendedCard = ChannelNavigator.Instance.Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                newSchedule.Persist();
                server.OnNewSchedule();
              }
              break;
          }
        }
        else
        {
          //manual record
          Schedule newSchedule = new Schedule(channel.IdChannel, "Manual (" + channel.Name + ")",
                                      DateTime.Now, DateTime.Now.AddDays(1));
          newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
          newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
          newSchedule.RecommendedCard = ChannelNavigator.Instance.Card.Id;

          newSchedule.Persist();
          server.OnNewSchedule();
        }
      }
      else
      {
        server.StopRecordingSchedule(ChannelNavigator.Instance.Card.RecordingScheduleId);
      }
    }

    /// <summary>
    /// Called when tv on/off button gets clicked
    /// 
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnTvOnOff(object sender, EventArgs args)
    {
      if (TvPlayerCollection.Instance.Count != 0)
      {
        videoWindow.Fill = new SolidColorBrush(Color.FromArgb(0xff, 0, 0, 0));
        TvPlayerCollection.Instance.DisposeAll();
      }
      else
      {
        if (ChannelNavigator.Instance.SelectedChannel != null)
        {
          ViewChannel(ChannelNavigator.Instance.SelectedChannel);
        }
      }
    }

    /// <summary>
    /// Get current tv program
    /// </summary>
    /// <param name="prog"></param>
    /// <returns></returns>
    private double CalculateProgress(DateTime start, DateTime end)
    {
      TimeSpan length = end - start;
      TimeSpan passed = DateTime.Now - start;
      if (length.TotalMinutes > 0)
      {
        double fprogress = (passed.TotalMinutes / length.TotalMinutes) * 100;
        fprogress = Math.Floor(fprogress);
        if (fprogress > 100.0f)
          return 100.0f;
        return fprogress;
      }
      else
        return 0;
    }
    /// <summary>
    /// Called when Channel button gets clicked.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnChannelClicked(object sender, EventArgs args)
    {
      if (ChannelNavigator.Instance.CurrentGroup == null) return;
      //show dialog menu showing all channels of current tvgroup
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      TvBusinessLayer layer = new TvBusinessLayer();
      IList groups = ChannelNavigator.Instance.CurrentGroup.ReferringGroupMap();
      List<Channel> _tvChannelList = new List<Channel>();
      foreach (GroupMap map in groups)
      {
        Channel ch = map.ReferencedChannel();
        if (ch.VisibleInGuide)
        {
          _tvChannelList.Add(ch);
        }
      }
      Dictionary<int, NowAndNext> listNowNext = layer.GetNowAndNext();

      bool checkChannelState = true;
      List<int> channelsRecording = null;
      List<int> channelsTimeshifting = null;
      TvServer server = new TvServer();
      server.GetAllRecordingChannels(out channelsRecording, out channelsTimeshifting);

      if (channelsRecording.Count == 0)
      {
        // not using cards at all - assume tuneability (why else should the user have this channel added..)
        if (channelsTimeshifting.Count == 0)
          checkChannelState = false;
        else
        {
          // note: it could be possible we're watching a stream another user is timeshifting...
          // TODO: add user check
          if (channelsTimeshifting.Count == 1 && TvPlayerCollection.Instance.Count != 0)
          {
            checkChannelState = false;
          }
        }
      }
      int selected = 0;
      ChannelState currentChannelState = ChannelState.tunable;
      for (int i = 0; i < _tvChannelList.Count; i++)
      {
        Channel currentChannel = _tvChannelList[i];
        if (checkChannelState)
          currentChannelState = (ChannelState)server.GetChannelState(currentChannel.IdChannel);
        else
          currentChannelState = ChannelState.tunable;

        if (channelsRecording.Contains(currentChannel.IdChannel))
          currentChannelState = ChannelState.recording;
        else
          if (channelsTimeshifting.Contains(currentChannel.IdChannel))
            currentChannelState = ChannelState.timeshifting;

        if (currentChannel == ChannelNavigator.Instance.SelectedChannel) selected = i;
        NowAndNext prog;
        if (listNowNext.ContainsKey(currentChannel.IdChannel) != false)
          prog = listNowNext[currentChannel.IdChannel];
        else
          prog = new NowAndNext(currentChannel.IdChannel, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), DateTime.Now.AddHours(2), DateTime.Now.AddHours(3), "No data available", "No data available", -1, -1);

        string percent = String.Format("{0}-{1}%", currentChannel.Name, CalculateProgress(prog.NowStartTime, prog.NowEndTime).ToString());
        string now = String.Format("Now:{0}", prog.TitleNow);
        string next = String.Format("Next:{0}", prog.TitleNext);


        switch (currentChannelState)
        {
          case ChannelState.nottunable:
            percent = "(unavailable) " + percent;
            break;
          case ChannelState.timeshifting:
            percent = "(timeshifting) " + percent;
            break;
          case ChannelState.recording:
            percent = "(recording) " + percent;
            break;
        }
        string channelLogoFileName = Thumbs.GetLogoFileName(currentChannel.Name);
        if (System.IO.File.Exists(channelLogoFileName))
        {
          dlgMenu.Items.Add(new DialogMenuItem(channelLogoFileName, now, next, percent));
        }
        else
        {
          dlgMenu.Items.Add(new DialogMenuItem("", now, next, percent));
        }
      }

      dlgMenu.Header = "On Now";
      dlgMenu.SubTitle = DateTime.Now.ToString("HH:mm");
      dlgMenu.SelectedIndex = selected;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected

      //get the selected tv channel
      ChannelNavigator.Instance.SelectedChannel = _tvChannelList[dlgMenu.SelectedIndex];

      //and view it
      ViewChannel(ChannelNavigator.Instance.SelectedChannel);
    }

    /// <summary>
    /// Called when scheduled button gets clicked].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnScheduledClicked(object sender, EventArgs args)
    {
    }
    /// <summary>
    /// Called when recorded button gets clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnRecordedClicked(object sender, EventArgs args)
    {
      this.NavigationService.Navigate(new Uri("TvRecorded.xaml", UriKind.Relative));
    }
    /// <summary>
    /// Called when search button gets clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnSearchClicked(object sender, EventArgs args)
    {
      this.Opacity = 0;
    }

    /// <summary>
    /// Start viewing the tv channel 
    /// </summary>
    /// <param name="channel">The channel.</param>
    void ViewChannel(Channel channel)
    {
      //tell server to start timeshifting the channel
      //we do this in the background so GUI stays responsive...
      StartTimeShiftingDelegate starter = new StartTimeShiftingDelegate(this.StartTimeShiftingBackGroundWorker);
      starter.BeginInvoke(channel, null, null);
    }

    /// <summary>
    /// Starts the timeshifting 
    /// this is done in the background so the GUI stays responsive
    /// </summary>
    /// <param name="channel">The channel.</param>
    private void StartTimeShiftingBackGroundWorker(Channel channel)
    {
      TvServer server = new TvServer();
      VirtualCard card;

      User user = new User();
      TvResult succeeded = TvResult.Succeeded;
      succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);

      // Schedule the update function in the UI thread.
      buttonTvGuide.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new EndTimeShiftingDelegate(OnStartTimeShiftingResult), succeeded, card);
    }
    /// <summary>
    /// Called from dispatcher when StartTimeShiftingBackGroundWorker() has a result for us
    /// we check the result and if needed start a new media player to playback the tv timeshifting file
    /// </summary>
    /// <param name="succeeded">The result.</param>
    /// <param name="card">The card.</param>
    private void OnStartTimeShiftingResult(TvResult succeeded, VirtualCard card)
    {
      videoWindow.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
      if (succeeded == TvResult.Succeeded)
      {
        //timeshifting worked, now view the channel
        ChannelNavigator.Instance.Card = card;
        //do we already have a media player ?
        if (TvPlayerCollection.Instance.Count != 0)
        {
          TvPlayerCollection.Instance.DisposeAll();
        }

        //create a new media player 
        MediaPlayer player = TvPlayerCollection.Instance.Get(card, card.TimeShiftFileName);
        player.MediaFailed += new EventHandler<ExceptionEventArgs>(_mediaPlayer_MediaFailed);
        player.MediaOpened += new EventHandler(_mediaPlayer_MediaOpened);

        //create video drawing which draws the video in the video window
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        videoWindow.Fill = videoBrush;
        videoDrawing.Player.Play();

      }
      else
      {
        //close media player
        if (TvPlayerCollection.Instance.Count != 0)
        {
          TvPlayerCollection.Instance.DisposeAll();
        }
        //tun tv button off
        buttonTvOnOff.IsChecked = false;

        //show error to user
        MpDialogOk dlg = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "Failed to start TV";
        dlg.Header = "Error";
        switch (succeeded)
        {
          case TvResult.AllCardsBusy:
            dlg.Content = "All cards are currently busy";
            break;
          case TvResult.CardIsDisabled:
            dlg.Content = "Card is disabled";
            break;
          case TvResult.ChannelIsScrambled:
            dlg.Content = "Channel is scrambled";
            break;
          case TvResult.ChannelNotMappedToAnyCard:
            dlg.Content = "Channel is not mapped to any tv card";
            break;
          case TvResult.ConnectionToSlaveFailed:
            dlg.Content = "Failed to connect to slave server";
            break;
          case TvResult.NotTheOwner:
            dlg.Content = "Card is owned by another user";
            break;
          case TvResult.NoTuningDetails:
            dlg.Content = "Channel does not have tuning information";
            break;
          case TvResult.NoVideoAudioDetected:
            dlg.Content = "No Video/Audio streams detected";
            break;
          case TvResult.UnableToStartGraph:
            dlg.Content = "Unable to start graph";
            break;
          case TvResult.UnknownChannel:
            dlg.Content = "Unknown channel";
            break;
          case TvResult.UnknownError:
            dlg.Content = "Unknown error occured";
            break;
        }
        dlg.ShowDialog();
      }
    }
    void OnTvStreamsClicked(object sender, EventArgs args)
    {
      int selected = 0;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "Streams";
      IList cards = TvDatabase.Card.ListAll();
      List<Channel> channels = new List<Channel>();
      int count = 0;
      TvServer server = new TvServer();
      List<User> _users = new List<User>();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
        User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          bool isRecording;
          bool isTimeShifting;
          VirtualCard tvcard = new VirtualCard(user, RemoteControl.HostName);
          isRecording = tvcard.IsRecording;
          isTimeShifting = tvcard.IsTimeShifting;
          if (isTimeShifting || (isRecording && !isTimeShifting))
          {
            int idChannel = tvcard.IdChannel;
            user = tvcard.User;
            Channel ch = Channel.Retrieve(idChannel);
            channels.Add(ch);
            string logo = Thumbs.GetLogoFileName(ch.Name);
            if (!System.IO.File.Exists(logo))
            {
              logo = "";
            }
            dlgMenu.Items.Add(new DialogMenuItem(logo, ch.Name, user.Name));
            //item.IconImage = strLogo;
            //if (isRecording)
            //  item.PinImage = Thumbs.TvRecordingIcon;
            //else
            //  item.PinImage = "";

            _users.Add(user);
            if (ChannelNavigator.Instance.Card != null && ChannelNavigator.Instance.Card.IdChannel == idChannel)
            {
              selected = count;
            }
            count++;
          }
        }
      }
      if (channels.Count == 0)
      {
        MpDialogOk dlgError = new MpDialogOk();
        Window win = Window.GetWindow(this);
        dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgError.Owner = win;
        dlgError.Title = "";
        dlgError.Header = "Streams";
        dlgError.Content = "No active streams";
        dlgError.ShowDialog();
        return;
      }
      dlgMenu.SelectedIndex = selected;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;
      ChannelNavigator.Instance.Card = new VirtualCard(_users[dlgMenu.SelectedIndex], RemoteControl.HostName);
      videoWindow.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
      string fileName = "";
      TvPlayerCollection.Instance.DisposeAll();
      if (ChannelNavigator.Instance.Card.IsRecording )
      {
        fileName = ChannelNavigator.Instance.Card.RecordingFileName; 
      }
      else
      {
        fileName = ChannelNavigator.Instance.Card.TimeShiftFileName; 
      }

      //create a new media player 
      MediaPlayer player = TvPlayerCollection.Instance.Get(ChannelNavigator.Instance.Card, ChannelNavigator.Instance.Card.TimeShiftFileName);
      player.MediaFailed += new EventHandler<ExceptionEventArgs>(_mediaPlayer_MediaFailed);
      player.MediaOpened += new EventHandler(_mediaPlayer_MediaOpened);

      //create video drawing which draws the video in the video window
      VideoDrawing videoDrawing = new VideoDrawing();
      videoDrawing.Player = player;
      videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
      DrawingBrush videoBrush = new DrawingBrush();
      videoBrush.Drawing = videoDrawing;
      videoWindow.Fill = videoBrush;
      videoDrawing.Player.Play();

      ChannelNavigator.Instance.Card.User.Name = new User().Name;
    }

    #region media player events & dispatcher methods
    void OnSeekToEnd()
    {
      if (TvPlayerCollection.Instance.Count != 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        if (player.NaturalDuration.HasTimeSpan)
        {
          TimeSpan duration = player.NaturalDuration.TimeSpan;
          player.Position = duration;
        }
        //set tv button on
        buttonTvOnOff.IsChecked = true;

        //update screen
        UpdateInfoBox();
      }
    }

    /// <summary>
    /// Called when media player has an error condition
    /// show messagebox to user and close media playback
    /// </summary>
    void OnMediaPlayerError()
    {

      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
      videoWindow.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
      if (player.HasError)
      {
        MpDialogOk dlgError = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgError.Owner = w;
        dlgError.Title = "Cannot open file";
        dlgError.Header = "Error";
        dlgError.Content = "Unable to open the timeshifting file " + player.ErrorMessage;
        dlgError.ShowDialog();
      }
      TvPlayerCollection.Instance.DisposeAll();

      //set tv button on
      buttonTvOnOff.IsChecked = false;

      //update screen
      UpdateInfoBox();
    }
    /// <summary>
    /// Handles the MediaOpened event of the _mediaPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void _mediaPlayer_MediaOpened(object sender, EventArgs e)
    {
      //media is opened, seek to end (via dispatcher)
      buttonTvGuide.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new SeekToEndDelegate(OnSeekToEnd));
    }

    /// <summary>
    /// Handles the MediaFailed event of the _mediaPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Media.ExceptionEventArgs"/> instance containing the event data.</param>
    void _mediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      // media player failed to open file
      // show error dialog (via dispatcher)
      buttonTvGuide.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
    }

    #endregion

    /// <summary>
    /// Updates the info like program title,description,time start/end and progress bar on screen.
    /// </summary>
    void UpdateInfoBox()
    {
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      if (ChannelNavigator.Instance.SelectedChannel == null)
      {
        labelStart.Content = "";
        labelTitle.Content = "";
        labelChannel.Content = "";
        labelDescription.Text = "";
        progressBar.Value = 0;
        return;
      }
      Program program = ChannelNavigator.Instance.SelectedChannel.CurrentProgram;
      if (program == null)
      {
        labelStart.Content = "";
        labelTitle.Content = "";
        labelChannel.Content = "";
        labelDescription.Text = "";
        progressBar.Value = 0;
        return;
      }
      labelStart.Content = String.Format("{0}-{1}", program.StartTime.ToString("HH:mm"), program.EndTime.ToString("HH:mm"));
      labelTitle.Content = program.Title;
      labelChannel.Content = ChannelNavigator.Instance.SelectedChannel.Name;
      labelDescription.Text = program.Description;

      TimeSpan duration = program.EndTime - program.StartTime;
      TimeSpan passed = DateTime.Now - program.StartTime;
      float percent = (float)(passed.TotalMinutes / duration.TotalMinutes);
      progressBar.Value = (int)(percent * 100);
    }

  }
}