#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Plugins
{
  /// <summary>
  /// A <see cref="IPluginManager"/> implementation that uses .plugin files to find
  /// what plug-ins are available
  /// </summary>
  public class PluginManager : IPluginManager
  {
    #region Variables
    private List<string> _pluginFiles;
    private List<string> _disabledPlugins;
    private PluginTree _pluginTree;
    #endregion

    #region Constructors/Destructors
    public PluginManager()
    {
      ServiceScope.Get<IMessageBroker>().Register(this);
      LoadPlugins();
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Triggers the PluginStarted message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPluginStarted(PluginStartStopEventArgs e)
    {
      //if (PluginStarted != null)
      //{
      //  PluginStarted(this, e);
      //}
    }

    /// <summary>
    /// Triggers the PluginStopped message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPluginStopped(PluginStartStopEventArgs e)
    {
      //if (PluginStopped != null)
      //{
      //  PluginStopped(this, e);
      //}
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loads all the available plugins
    /// </summary>
    /// <remarks>
    /// Note that by using an attribute to hold the plugin name and description, we 
    /// do not need to actually start the plugin to get it's name and description
    /// </remarks>
    private void LoadPlugins()
    {
      //Test data -> get from config in future
      _pluginFiles = new List<string>();
      _disabledPlugins = new List<string>();
      _pluginFiles.Add("Plugins/MyTv.plugin");
      _pluginFiles.Add("Plugins/Menu.plugin");
      _pluginFiles.Add("Plugins/MyVideos.plugin");
      _pluginFiles.Add("Plugins/MyPictures.plugin");
      _pluginFiles.Add("Plugins/MyWeather.plugin");
      _pluginTree = new PluginTree();
      _pluginTree.Load(_pluginFiles, _disabledPlugins);

      ServiceScope.Add<IPluginTree>(_pluginTree);
    }
    #endregion

    #region IPluginManager Members

    public event EventHandler<PluginStartStopEventArgs> PluginStarted;
    public event EventHandler<PluginStartStopEventArgs> PluginStopped;

    public List<T> BuildItems<T>(string treePath)
    {
      return _pluginTree.BuildItems<T>(treePath, null, false);
    }

    /// <summary>
    /// Gets an enumerable list of available plugins
    /// </summary>
    /// <returns>An <see cref="IEnumerable<IPlugin>"/> list.</returns>
    /// <remarks>A configuration program can use this list to present the user a list of available plugins that he can (de)activate.</remarks>
    public IEnumerable<IPluginInfo> GetAvailablePlugins()
    {
      return null; // pluginInfo.Values;
    }

    /// <summary>
    /// Stops the given plug-in.
    /// </summary>
    /// <param name="pluginName">name of the plug-in to stop</param>
    public void Stop(string pluginName)
    {
      //if (!runningPlugins.ContainsKey(pluginName))
      //{
      //  return; //Plugin was not running
      //}
      //IPlugin plugin = runningPlugins[pluginName];
      //runningPlugins.Remove(pluginName);
      //plugin.Dispose();
      //OnPluginStopped(new PluginStartStopEventArgs(pluginName));
    }

    /// <summary>
    /// Stops all plug-ins
    /// </summary>
    public void StopAll()
    {
      //foreach (IPlugin plugin in runningPlugins.Values)
      //{
      //  plugin.Dispose();
      //}
      //runningPlugins.Clear();
    }

    /// <summary>
    /// Starts the given plug-in.
    /// </summary>
    /// <param name="pluginName">name of the plug-in to start</param>
    public void Start(string pluginName)
    {
      ////TODO: add support for starting the same plug-in more than once.
      //if (!pluginTypes.ContainsKey(pluginName))
      //{
      //  throw new ArgumentException(string.Format("{0} Plug-in does not exist", pluginName), "pluginName");
      //}
      //IPlugin plugin = (IPlugin) Activator.CreateInstance(pluginTypes[pluginName]);
      //plugin.Initialize();
      //if (!runningPlugins.ContainsKey(pluginName))
      //{
      //  runningPlugins.Add(pluginName, plugin);
      //}
      //OnPluginStarted(new PluginStartStopEventArgs(pluginName));
    }


    /// <summary>
    /// Starts all plug-ins that are activated by the user.
    /// </summary>
    public void StartAll()
    {
      foreach (IPlugin plugin in _pluginTree.BuildItems<IPlugin>("/Infinity/AutoStart", null, false))
      {
        plugin.Initialize();
      }
    }

    #endregion
  }
}