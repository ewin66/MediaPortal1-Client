using System;
using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TvDatabase;
using TvControl;
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  public class TvBaseViewModel : INotifyPropertyChanged
  {
    #region variables
    Window _window;
    Page _page;
    ICommand _fullScreenTvCommand;
    ICommand _fullScreenCommand;
    ICommand _playCommand;
    ICommand _timeShiftCommand;
    ICommand _tvStreamsCommand;
    ICommand _miniEpgCommand;
    ICommand _tvGuideCommand;
    ICommand _scheduledCommand;
    ICommand _recordedCommand;
    ICommand _searchCommand;
    ICommand _turnTvOnOffCommand;
    ICommand _recordNowCommand;
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvBaseViewModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public TvBaseViewModel(Page page)
    {

      //store page & window
      _page = page;
      _window = Window.GetWindow(_page);
      ServiceScope.Get<ITvChannelNavigator>().PropertyChanged += new PropertyChangedEventHandler(OnChannelChanged);
    }

    void OnChannelChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SelectedChannel")
      {
        ChangeProperty("ProgramPercent");
        ChangeProperty("ProgramTitle");
        ChangeProperty("ProgramGenre");
        ChangeProperty("ProgramPercent");
        ChangeProperty("ProgramDescription");
        ChangeProperty("ProgramStartEnd");
        ChangeProperty("ProgramChannelName");
      }
      if (e.PropertyName == "IsRecording")
      {
        ChangeProperty("IsRecordingLogo");
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Notifies subscribers that property has been changed
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public void ChangeProperty(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets the recording logo.
    /// </summary>
    /// <value>The is recording logo.</value>
    public string IsRecordingLogo
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().IsRecording)
          return String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvRecordingIcon);
        else return "";
      }
    }
    /// <summary>
    /// Gets the window.
    /// </summary>
    /// <value>The window.</value>
    public Window Window
    {
      get
      {
        return _window;
      }
    }
    /// <summary>
    /// Gets the current Page.
    /// </summary>
    /// <value>The page.</value>
    public Page Page
    {
      get
      {
        return _page;
      }
      set
      {
        _page = value;
        _window = Window.GetWindow(_page);
      }
    }

    #region properties for current tuned channel
    /// <summary>
    /// Returns percentage how far current program is done
    /// </summary>
    /// <value>The program percent.</value>
    public double ProgramPercent
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel == null) return 0;
        Program program = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.CurrentProgram;
        if (program == null) return 0;

        TimeSpan duration = program.EndTime - program.StartTime;
        TimeSpan passed = DateTime.Now - program.StartTime;
        float percent = (float)(passed.TotalMinutes / duration.TotalMinutes);
        return (int)(percent * 100);
      }
    }
    /// <summary>
    /// Gets the program title.
    /// </summary>
    /// <value>The program title.</value>
    public string ProgramTitle
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel == null) return "";
        Program program = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.CurrentProgram;
        if (program == null) return "";
        return program.Title;
      }
    }
    /// <summary>
    /// Gets the program genre.
    /// </summary>
    /// <value>The program genre.</value>
    public string ProgramGenre
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel == null) return "";
        Program program = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.CurrentProgram;
        if (program == null) return "";
        return program.Genre;
      }
    }
    /// <summary>
    /// Gets the program description.
    /// </summary>
    /// <value>The program description.</value>
    public string ProgramDescription
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel == null) return "";
        Program program = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.CurrentProgram;
        if (program == null) return "";
        return program.Description;
      }
    }
    /// <summary>
    /// Gets the program start-end.
    /// </summary>
    /// <value>The program start-end.</value>
    public string ProgramStartEnd
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel == null) return "";
        Program program = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.CurrentProgram;
        if (program == null) return "";
        return String.Format("{0}-{1}", program.StartTime.ToString("HH:mm"), program.EndTime.ToString("HH:mm")); ;
      }
    }
    /// <summary>
    /// Gets the program start-end.
    /// </summary>
    /// <value>The program start-end.</value>
    public string ProgramChannelName
    {
      get
      {
        if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel == null) return "";
        Program program = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.CurrentProgram;
        if (program == null) return "";
        return program.ReferencedChannel().Name;
      }
    }
    #endregion

    #region button label properties
    public string DateLabel
    {
      get
      {
        return DateTime.Now.ToString("dd-MM HH:mm");
      }
    }
    public string TvGuideLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 0);//TvGuide
      }
    }
    public string RecordNowLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 1);//Record now
      }
    }
    public string ChannelLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 2);//Channel
      }
    }
    public string TvStreamsLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 3);//TvStreams
      }
    }
    public string TvOnOffLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 4);//TvOnOff
      }
    }
    public string ScheduledLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 5);//Scheduled
      }
    }
    public string RecordedLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 6);//Recorded
      }
    }
    public virtual string SearchLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 7);//Search
      }
    }
    public string TeletextLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 8);//Teletext
      }
    }
    public virtual string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 9);//television
      }
    }
    public bool? TvOnOff
    {
      get
      {
        return (ServiceScope.Get<IPlayerCollectionService>().Count > 0);
      }
    }
    #endregion

    #region video properties
    /// <summary>
    /// Returns whether video is present or not.
    /// </summary>
    /// <value>Visibility.Visible when video is present otherwise Visibility.Collapsed</value>
    public Visibility IsVideoPresent
    {
      get
      {
        return (ServiceScope.Get<IPlayerCollectionService>().Count != 0) ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    /// <summary>
    /// Gets the video brush.
    /// </summary>
    /// <value>The video brush.</value>
    public Brush VideoBrush
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          MediaPlayer player = (MediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0].UnderlyingPlayer;
          VideoDrawing videoDrawing = new VideoDrawing();
          videoDrawing.Player = player;
          videoDrawing.Rect = new Rect(0, 0, player.NaturalVideoWidth, player.NaturalVideoHeight);
          DrawingBrush videoBrush = new DrawingBrush();
          videoBrush.Drawing = videoDrawing;
          return videoBrush;
        }
        return new SolidColorBrush(Color.FromArgb(0xff, 0, 0, 0));
      }
    }
    #endregion
    #endregion

    #region commands
    /// <summary>
    /// Returns a ICommand for cleaning up watched recordings
    /// </summary>
    /// <value>The command.</value>
    public ICommand FullScreenTv
    {
      get
      {
        if (_fullScreenTvCommand == null)
        {
          _fullScreenTvCommand = new FullScreenTvCommand(this);
        }
        return _fullScreenTvCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for toggeling between fullscreen mode and windowed mode
    /// </summary>
    /// <value>The command.</value>
    public ICommand FullScreen
    {
      get
      {
        if (_fullScreenCommand == null)
        {
          _fullScreenCommand = new FullScreenCommand(this);
        }
        return _fullScreenCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for showing the context menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand Play
    {
      get
      {
        if (_playCommand == null)
        {
          _playCommand = new PlayCommand(this);
        }
        return _playCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for showing the context menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand TimeShift
    {
      get
      {
        if (_timeShiftCommand == null)
        {
          _timeShiftCommand = new TimeShiftCommand(this);
        }
        return _timeShiftCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for showing the tvstreams menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand TvStreams
    {
      get
      {
        if (_tvStreamsCommand == null)
        {
          _tvStreamsCommand = new TvStreamsCommand(this);
        }
        return _tvStreamsCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for showing the miniepg menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand MiniEpg
    {
      get
      {
        if (_miniEpgCommand == null)
        {
          _miniEpgCommand = new MiniEpgCommand(this);
        }
        return _miniEpgCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for navigating to the tvguide
    /// </summary>
    /// <value>The command.</value>
    public ICommand TvGuide
    {
      get
      {
        if (_tvGuideCommand == null)
        {
          _tvGuideCommand = new TvGuideCommand(this);
        }
        return _tvGuideCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for navigating to the recorded tv
    /// </summary>
    /// <value>The command.</value>
    public ICommand RecordedTv
    {
      get
      {
        if (_recordedCommand == null)
        {
          _recordedCommand = new RecordedCommand(this);
        }
        return _recordedCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for navigating to the recorded tv
    /// </summary>
    /// <value>The command.</value>
    public ICommand ScheduledTv
    {
      get
      {
        if (_scheduledCommand == null)
        {
          _scheduledCommand = new ScheduledCommand(this);
        }
        return _scheduledCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for navigating to the search tv
    /// </summary>
    /// <value>The command.</value>
    public ICommand Search
    {
      get
      {
        if (_searchCommand == null)
        {
          _searchCommand = new SearchCommand(this);
        }
        return _searchCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for turning tv on/off
    /// </summary>
    /// <value>The command.</value>
    public ICommand TurnTvOnOff
    {
      get
      {
        if (_turnTvOnOffCommand == null)
        {
          _turnTvOnOffCommand = new TurnTvOnOffCommand(this);
        }
        return _turnTvOnOffCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for recording now
    /// </summary>
    /// <value>The command.</value>
    public ICommand RecordNow
    {
      get
      {
        if (_recordNowCommand == null)
        {
          _recordNowCommand = new RecordNowCommand(this);
        }
        return _recordNowCommand;
      }
    }
    #endregion

    #region Commands subclasses
    #region base command class
    public abstract class TvBaseCommand : ICommand
    {
      protected TvBaseViewModel _viewModel;
      public event EventHandler CanExecuteChanged;

      public TvBaseCommand(TvBaseViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      public abstract void Execute(object parameter);

      public virtual bool CanExecute(object parameter)
      {
        return true;
      }

      protected void OnCanExecuteChanged()
      {
        if (this.CanExecuteChanged != null)
        {
          this.CanExecuteChanged(this, EventArgs.Empty);
        }
      }
    }
    #endregion

    #region FullScreenCommand  class
    /// <summary>
    /// FullScreenCommand will toggle application between normal and fullscreen mode
    /// </summary> 
    public class FullScreenCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="FullScreenCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public FullScreenCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        Window window = _viewModel.Window;
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
        }
      }
    }
    #endregion

    #region Play command class
    /// <summary>
    /// Play command will start playing a recording
    /// </summary>
    public class PlayCommand : TvBaseCommand
    {
      public class PlayParameter
      {
        public string FileName;
        public VirtualCard Card;
        public bool StartAtBeginning;
        public PlayParameter(string filename, VirtualCard card)
        {
          FileName = filename;
          Card = card;
          StartAtBeginning = false;
        }
        public PlayParameter(string filename, VirtualCard card, bool startAtBeginning)
        {
          FileName = filename;
          Card = card;
          StartAtBeginning = startAtBeginning;
        }
      }
      private delegate void MediaPlayerErrorDelegate();
      private delegate void MediaPlayerOpenDelegate();
      PlayParameter _playParameter;
      /// <summary>
      /// Initializes a new instance of the <see cref="PlayCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public PlayCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          ServiceScope.Get<IPlayerCollectionService>().Clear();
          _viewModel.ChangeProperty("VideoBrush");
          _viewModel.ChangeProperty("FullScreen");
          _viewModel.ChangeProperty("IsVideoPresent");
          _viewModel.ChangeProperty("TvOnOff");
        }
        _playParameter = parameter as PlayParameter;
        TvMediaPlayer player = new TvMediaPlayer(_playParameter.Card, _playParameter.FileName);
        ServiceScope.Get<IPlayerCollectionService>().Add(player);
        player.MediaFailed += new EventHandler<MediaExceptionEventArgs>(_mediaPlayer_MediaFailed);
        player.MediaOpened += new EventHandler(player_MediaOpened);
        player.Open(PlayerMediaType.TvLive,_playParameter.FileName);
        player.Play();
      }

      void player_MediaOpened(object sender, EventArgs e)
      {
        _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerOpenDelegate(OnMediaOpened));
      }
      void OnMediaOpened()
      {
        _viewModel.ChangeProperty("VideoBrush");
        _viewModel.ChangeProperty("FullScreen");
        _viewModel.ChangeProperty("IsVideoPresent");
        _viewModel.ChangeProperty("TvOnOff");
        if (_playParameter.StartAtBeginning)
        {
          IPlayer player = ServiceScope.Get<IPlayerCollectionService>()[0];
          player.Position = new TimeSpan(0, 0, 0, 0);
        }
      }
      void _mediaPlayer_MediaFailed(object sender, MediaExceptionEventArgs e)
      {
        _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
      }
      void OnMediaPlayerError()
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
          if (player.HasError)
          {
            MpDialogOk dlgError = new MpDialogOk();
            dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlgError.Owner = _viewModel.Window;
            dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
            dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
            dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 38)/*Unable to open the file*/+ " " + player.ErrorMessage;
            dlgError.ShowDialog();
          }
        }
        ServiceScope.Get<IPlayerCollectionService>().Clear();
      }
    }
    #endregion

    #region TimeShift command class
    /// <summary>
    /// Fullscreen command will navigate to fullscreen window
    /// </summary>
    public class TimeShiftCommand : TvBaseCommand
    {
      private delegate void MediaPlayerErrorDelegate();
      private delegate void StartTimeShiftingDelegate(Channel channel);
      private delegate void EndTimeShiftingDelegate(TvResult result, VirtualCard card);
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public TimeShiftCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        Channel channel = parameter as Channel;
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
        ServiceScope.Get<ILogger>().Info("Tv:  start timeshifting channel:{0}", channel.Name);
        TvServer server = new TvServer();
        VirtualCard card;

        User user = new User();
        TvResult succeeded = TvResult.Succeeded;
        ServiceScope.Get<ITvChannelNavigator>().SelectedChannel = channel;
        succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);

        // Schedule the update function in the UI thread.
        _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new EndTimeShiftingDelegate(OnStartTimeShiftingResult), succeeded, card);
      }
      /// <summary>
      /// Called from dispatcher when StartTimeShiftingBackGroundWorker() has a result for us
      /// we check the result and if needed start a new media player to playback the tv timeshifting file
      /// </summary>
      /// <param name="succeeded">The result.</param>
      /// <param name="card">The card.</param>
      private void OnStartTimeShiftingResult(TvResult succeeded, VirtualCard card)
      {
        ServiceScope.Get<ILogger>().Info("Tv:  timeshifting channel:{0} result:{1}", ServiceScope.Get<ITvChannelNavigator>().SelectedChannel.Name, succeeded);
        if (succeeded == TvResult.Succeeded)
        {
          //timeshifting worked, now view the channel
          ServiceScope.Get<ITvChannelNavigator>().Card = card;

          //do we already have a media player ?
          if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
          {
            if (ServiceScope.Get<IPlayerCollectionService>()[0].FileName != card.TimeShiftFileName)
            {
              _viewModel.ChangeProperty("VideoBrush");
              _viewModel.ChangeProperty("FullScreen");
              _viewModel.ChangeProperty("IsVideoPresent");
              _viewModel.ChangeProperty("TvOnOff");
              ServiceScope.Get<IPlayerCollectionService>().Clear();
            }
          }
          if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
          {
            TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
            player.SeekToEnd();
            return;
          }

          ICommand cmd = _viewModel.Play;
          cmd.Execute(new TvBaseViewModel.PlayCommand.PlayParameter(card.TimeShiftFileName, card));
        }
        else
        {
          //close media player
          if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
          {
            _viewModel.ChangeProperty("VideoBrush");
            _viewModel.ChangeProperty("FullScreen");
            _viewModel.ChangeProperty("IsVideoPresent");
            _viewModel.ChangeProperty("TvOnOff");
            ServiceScope.Get<IPlayerCollectionService>().Clear();
          }

          //show error to user
          MpDialogOk dlg = new MpDialogOk();
          dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlg.Owner = _viewModel.Window;
          dlg.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 23);//"Failed to start TV;
          dlg.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);/*(Error)*/
          switch (succeeded)
          {
            case TvResult.AllCardsBusy:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 24); //"All cards are currently busy";
              break;
            case TvResult.CardIsDisabled:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 25);// "Card is disabled";
              break;
            case TvResult.ChannelIsScrambled:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 26);//"Channel is scrambled";
              break;
            case TvResult.ChannelNotMappedToAnyCard:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 27);//"Channel is not mapped to any tv card";
              break;
            case TvResult.ConnectionToSlaveFailed:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 28);//"Failed to connect to slave server";
              break;
            case TvResult.NotTheOwner:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 29);//"Card is owned by another user";
              break;
            case TvResult.NoTuningDetails:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 30);//"Channel does not have tuning information";
              break;
            case TvResult.NoVideoAudioDetected:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 31);//"No Video/Audio streams detected";
              break;
            case TvResult.UnableToStartGraph:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 32);//"Unable to start graph";
              break;
            case TvResult.UnknownChannel:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 33);//"Unknown channel";
              break;
            case TvResult.UnknownError:
              dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 34);//"Unknown error occured";
              break;
          }
          dlg.ShowDialog();
        }
      }

    }
    #endregion

    #region TvStreamsCommand  class
    /// <summary>
    /// TvStreamsCommand will show the tvstreams dialog
    /// </summary> 
    public class TvStreamsCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="TvStreamsCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public TvStreamsCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        int selected = 0;
        MpMenuWithLogo dlgMenu = new MpMenuWithLogo();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 35);// "Streams";
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
              string logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(ch.Name));
              if (!System.IO.File.Exists(logo))
              {
                logo = "";
              }
              dlgMenu.Items.Add(new DialogMenuItem(logo, ch.Name, "", user.Name));
              //item.IconImage = strLogo;
              //if (isRecording)
              //  item.PinImage = Thumbs.TvRecordingIcon;
              //else
              //  item.PinImage = "";

              _users.Add(user);
              if (ServiceScope.Get<ITvChannelNavigator>().Card != null && ServiceScope.Get<ITvChannelNavigator>().Card.IdChannel == idChannel)
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
          dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgError.Owner = _viewModel.Window;
          dlgError.Title = "";
          dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 35);// "Streams";
          dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 36);//"No active streams";
          dlgError.ShowDialog();
          return;
        }
        dlgMenu.SelectedIndex = selected;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;
        ServiceScope.Get<ITvChannelNavigator>().Card = new VirtualCard(_users[dlgMenu.SelectedIndex], RemoteControl.HostName);


        string fileName = "";
        if (ServiceScope.Get<IPlayerCollectionService>() != null)
        {
          _viewModel.ChangeProperty("VideoBrush");
          _viewModel.ChangeProperty("FullScreen");
          _viewModel.ChangeProperty("IsVideoPresent");
          _viewModel.ChangeProperty("TvOnOff");
        }
        ServiceScope.Get<IPlayerCollectionService>().Clear();
        if (ServiceScope.Get<ITvChannelNavigator>().Card.IsRecording)
        {
          fileName = ServiceScope.Get<ITvChannelNavigator>().Card.RecordingFileName;
        }
        else
        {
          fileName = ServiceScope.Get<ITvChannelNavigator>().Card.TimeShiftFileName;
        }

        //create a new media player 
        ICommand cmd = _viewModel.Play;
        cmd.Execute(new TvBaseViewModel.PlayCommand.PlayParameter(ServiceScope.Get<ITvChannelNavigator>().Card.TimeShiftFileName, ServiceScope.Get<ITvChannelNavigator>().Card));
        ServiceScope.Get<ITvChannelNavigator>().Card.User.Name = new User().Name;
      }
    }
    #endregion

    #region MiniEpgCommand  class
    /// <summary>
    /// MiniEpgCommand will show the mini epg
    /// </summary> 
    public class MiniEpgCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="FullScreenCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public MiniEpgCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
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
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (ServiceScope.Get<ITvChannelNavigator>().CurrentGroup == null) return;
        ServiceScope.Get<ILogger>().Info("MyTv: OnChannelClicked");

        //show dialog menu showing all channels of current tvgroup
        DialogMenuItemCollection menuItems = new DialogMenuItemCollection();
        ServiceScope.Get<ILogger>().Info("MyTv:   get channels");
        TvBusinessLayer layer = new TvBusinessLayer();
        IList groups = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup.ReferringGroupMap();
        IList channelList = Channel.ListAll();
        List<Channel> _tvChannelList = new List<Channel>();
        ServiceScope.Get<ILogger>().Info("MyTv:   get channels2");
        foreach (GroupMap map in groups)
        {
          foreach (Channel ch in channelList)
          {
            if (ch.IdChannel == map.IdChannel)
            {
              if (ch.VisibleInGuide && ch.IsTv)
              {
                _tvChannelList.Add(ch);
              }
              break;
            }
          }
        }
        ServiceScope.Get<ILogger>().Info("MyTv:   get now&next");
        Dictionary<int, NowAndNext> listNowNext = layer.GetNowAndNext();

        ServiceScope.Get<ILogger>().Info("MyTv:   get recording channels");
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
            if (channelsTimeshifting.Count == 1 && ServiceScope.Get<IPlayerCollectionService>().Count != 0)
            {
              checkChannelState = false;
            }
          }
        }

        ServiceScope.Get<ILogger>().Info("MyTv:   {0} channels recording", channelsRecording.Count);
        ServiceScope.Get<ILogger>().Info("MyTv:   {0} channels timeshifting", channelsTimeshifting.Count);
        ServiceScope.Get<ILogger>().Info("MyTv:   checkChannelState:{0}", checkChannelState);
        ServiceScope.Get<ILogger>().Info("MyTv:   add {0} channels", _tvChannelList.Count);
        int selected = 0;
        ChannelState currentChannelState = ChannelState.tunable;
        string nowLocalize = ServiceScope.Get<ILocalisation>().ToString("mytv", 17);/*Now*/
        string nextLocalize = ServiceScope.Get<ILocalisation>().ToString("mytv", 18);/*Next*/
        string currentFolder = System.IO.Directory.GetCurrentDirectory();
        for (int i = 0; i < _tvChannelList.Count; i++)
        {
          //  ServiceScope.Get<ILogger>().Info("MyTv:   add {0} ", i);
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

          if (currentChannel == ServiceScope.Get<ITvChannelNavigator>().SelectedChannel) selected = i;
          NowAndNext prog;
          if (listNowNext.ContainsKey(currentChannel.IdChannel) != false)
            prog = listNowNext[currentChannel.IdChannel];
          else
            prog = new NowAndNext(currentChannel.IdChannel, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), DateTime.Now.AddHours(2), DateTime.Now.AddHours(3), "No data available", "No data available", -1, -1);

          string percent = String.Format("{0}-{1}%", currentChannel.Name, CalculateProgress(prog.NowStartTime, prog.NowEndTime).ToString());
          string now = String.Format("{0}:{1}", nowLocalize, prog.TitleNow);
          string next = String.Format("{0}:{1}", nextLocalize, prog.TitleNext);


          switch (currentChannelState)
          {
            case ChannelState.nottunable:
              percent = ServiceScope.Get<ILocalisation>().ToString("mytv", 19)/*(unavailable)*/  + percent;
              break;
            case ChannelState.timeshifting:
              percent = ServiceScope.Get<ILocalisation>().ToString("mytv", 20)/*(timeshifting)*/  + percent;
              break;
            case ChannelState.recording:
              percent = ServiceScope.Get<ILocalisation>().ToString("mytv", 21)/*(recording)*/  + percent;
              break;
          }
          string channelLogoFileName = String.Format(@"{0}\{1}", currentFolder, Thumbs.GetLogoFileName(currentChannel.Name));
          if (!System.IO.File.Exists(channelLogoFileName))
          {
            channelLogoFileName = "";
          }
          DialogMenuItem item = new DialogMenuItem(channelLogoFileName, now, next, percent);
          menuItems.Add(item);
        }
        ServiceScope.Get<ILogger>().Info("MyTv:   create dialog");
        MpMenuWithLogo dlgMenu = new MpMenuWithLogo(menuItems);
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 22);/*(On Now)*/
        dlgMenu.SubTitle = DateTime.Now.ToString("HH:mm");
        dlgMenu.SelectedIndex = selected;
        ServiceScope.Get<ILogger>().Info("MyTv:   show dialog");
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected

        //get the selected tv channel
        ServiceScope.Get<ITvChannelNavigator>().SelectedChannel = _tvChannelList[dlgMenu.SelectedIndex];

        //and view it
        ICommand cmd = _viewModel.TimeShift;
        cmd.Execute(ServiceScope.Get<ITvChannelNavigator>().SelectedChannel);
      }
    }
    #endregion

    #region TurnTvOnOffCommand command class
    /// <summary>
    /// TurnTvOnOffCommand command will turn tv on/off
    /// </summary>
    public class TurnTvOnOffCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="TurnTvOnOffCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public TurnTvOnOffCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
        {
          ServiceScope.Get<ILogger>().Info("Tv:  stop tv");
          ServiceScope.Get<IPlayerCollectionService>().Clear();
          _viewModel.ChangeProperty("VideoBrush");
          _viewModel.ChangeProperty("FullScreen");
          _viewModel.ChangeProperty("IsVideoPresent");
          _viewModel.ChangeProperty("TvOnOff");
          if (ServiceScope.Get<ITvChannelNavigator>().Card != null)
          {
            ServiceScope.Get<ITvChannelNavigator>().Card.StopTimeShifting();
          }
        }
        else
        {
          ServiceScope.Get<ILogger>().Info("Tv:  start tv");
          if (ServiceScope.Get<ITvChannelNavigator>().SelectedChannel != null)
          {
            ICommand cmd = _viewModel.TimeShift;
            cmd.Execute(ServiceScope.Get<ITvChannelNavigator>().SelectedChannel);
          }
        }
      }
    }
    #endregion

    #region RecordNow command class
    /// <summary>
    /// RecordNowCommand command start/stop recording
    /// </summary>
    public class RecordNowCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="RecordNowCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public RecordNowCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //Are we recording this channel already?
        TvBusinessLayer layer = new TvBusinessLayer();
        TvServer server = new TvServer();
        VirtualCard card;
        Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
        if (channel == null) return;
        if (false == server.IsRecording(channel.Name, out card))
        {
          //no then start recording
          Program prog = channel.CurrentProgram;
          if (prog != null)
          {
            MpMenu dlgMenu = new MpMenu();
            dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlgMenu.Owner = _viewModel.Window;
            dlgMenu.Items.Clear();
            dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);//Record
            dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 14)/*current program*/));
            dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 15)/*until manual stopped*/));
            dlgMenu.ShowDialog();
            switch (dlgMenu.SelectedIndex)
            {
              case 0:
                {
                  Schedule newSchedule = new Schedule(channel.IdChannel, channel.CurrentProgram.Title,
                            channel.CurrentProgram.StartTime, channel.CurrentProgram.EndTime);
                  newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                  newSchedule.RecommendedCard = ServiceScope.Get<ITvChannelNavigator>().Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

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
                  newSchedule.RecommendedCard = ServiceScope.Get<ITvChannelNavigator>().Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                  newSchedule.Persist();
                  server.OnNewSchedule();
                }
                break;
            }
          }
          else
          {
            //manual record
            string manual = ServiceScope.Get<ILocalisation>().ToString("mytv", 16);//Manual
            Schedule newSchedule = new Schedule(channel.IdChannel, manual + " (" + channel.Name + ")",
                                        DateTime.Now, DateTime.Now.AddDays(1));
            newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
            newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
            newSchedule.RecommendedCard = ServiceScope.Get<ITvChannelNavigator>().Card.Id;

            newSchedule.Persist();
            server.OnNewSchedule();
          }
        }
        else
        {
          server.StopRecordingSchedule(ServiceScope.Get<ITvChannelNavigator>().Card.RecordingScheduleId);
        }
      }
    }
    #endregion

    #region navigation commands
    #region Search command class
    /// <summary>
    /// Fullscreen command will navigate to search window
    /// </summary>
    public class SearchCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SearchCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        TvSearch.SearchMode = TvSearchViewModel.SearchType.Title;
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvSearch.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region TvGuide command class
    /// <summary>
    /// Fullscreen command will navigate to TvGuide window
    /// </summary>
    public class TvGuideCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public TvGuideCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvGuide.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region Recorded command class
    /// <summary>
    /// Fullscreen command will navigate to recorded tv window
    /// </summary>
    public class RecordedCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="RecordedCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public RecordedCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvRecorded.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region Scheduled command class
    /// <summary>
    /// ScheduledCommand command will navigate to scheduled tv window
    /// </summary>
    public class ScheduledCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduledCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public ScheduledCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvScheduled.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region FullscreenTv command class
    /// <summary>
    /// Fullscreen command will navigate to fullscreen window
    /// </summary>
    public class FullScreenTvCommand : TvBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public FullScreenTvCommand(TvBaseViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
        {
          ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
        }
      }
    }
    #endregion

    #endregion
    #endregion
  }
}

