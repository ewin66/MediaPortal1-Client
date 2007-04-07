using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Logging;
using ProjectInfinity.Menu;
using ProjectInfinity.Messaging;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Themes;
using ProjectInfinity.Utilities.CommandLine;
using ProjectInfinity.Windows;
using ProjectInfinity.Localisation;
using ProjectInfinity.Players;

namespace ProjectInfinity
{
  public class Launcher
  {
    [STAThread]
    public static void Main(params string[] args)
    {
      ILogger logger = new FileLogger("ProjectInfinity.log", LogLevel.Debug);
      ServiceScope.Add(logger);
      logger.Critical("ProjectInfinity is starting...");
      //register service implementations
      ServiceScope.Add<IMessageBroker>(new MessageBroker()); //Our messagebroker
      //A pluginmanager that uses reflection to enumerate available plugins
      ServiceScope.Add<IPluginManager>(new ReflectionPluginManager());
      ServiceScope.Add<IThemeManager>(new ThemeManager());
      ServiceScope.Add<IMenuManager>(new MenuManager());
      ServiceScope.Add<INavigationService>(new NavigationService());
      ServiceScope.Add<IPlayerCollectionService>(new PlayerCollectionService());
      ServiceScope.Add<ILocalisation>(new StringManager("Language", "en"));

      ICommandLineOptions piArgs = new ProjectInfinityCommandLine();

      try
      {
        CommandLine.Parse(args, ref piArgs);
      }
      catch (ArgumentException)
      {
        piArgs.DisplayOptions();
        return;
      }
      Core.Start();

    }
  }
}
