using System;
using System.Windows;
using ProjectInfinity.Messaging;
using ProjectInfinity.Messaging.MusicMessages;
using ProjectInfinity.Music;
using ProjectInfinity.Windows;

namespace ProjectInfinity
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IMainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      playButton.Click += PlayClicked;
      stopButton.Click += StopClicked;

      ServiceScope.Get<IMessageBroker>().Register(this);
    }

    private static void PlayClicked(object sender, RoutedEventArgs e)
    {
      try
      {
        IMusicPlayer player = ServiceScope.Get<IMusicPlayer>();
        player.Play(@"..\..\The Infinity Project-Mystical Experiences-8-Blue Aura.mp3");
      }
      catch (ServiceNotFoundException)
      {
        MessageBox.Show("MusicPlayer is not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private static void StopClicked(object sender, RoutedEventArgs e)
    {
      IMusicPlayer player = ServiceScope.Get<IMusicPlayer>(false);
      if (player != null)
      {
        player.Stop();
      }
    }

    [MessageSubscription(typeof (MusicStartMessage))]
    private void MusicStarted(object sender, MusicStartMessage args)
    {
      musicLabel.Content =
        string.Format("Playing {0} from {1}: track {2} from the album {3}", args.Title, args.Artist, args.TrackNo,
                      args.Album);
      //Check if the message was sent by the ExtendedPlayer.  We know that it passes an 
      //ExtendedMusicStartEventArgs instance (which inherits from MusicStartEventArgs)
      ExtendedMusicStartMessage extended = args as ExtendedMusicStartMessage;
      if (extended != null)
      {
        ratingLabel.Content = "Rating = " + extended.Rating;
      }
      else
      {
        ratingLabel.Content = null;
      }
    }

    [MessageSubscription(typeof (Stop))]
    private void MusicStopped(object sender, EventArgs args)
    {
      musicLabel.Content = null;
      ratingLabel.Content = null;
    }
  }
}