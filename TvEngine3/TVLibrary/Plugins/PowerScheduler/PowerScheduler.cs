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

#region Usings
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using TvControl;
using TvDatabase;
using TvEngine;
using TvEngine.Interfaces;
using TvEngine.PowerScheduler.Handlers;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using System.Runtime.InteropServices;
using System.Collections;
#endregion

namespace TvEngine.PowerScheduler
{
  /// <summary>
  /// PowerScheduler: tvservice plugin which controls power management
  /// </summary>
  public class PowerScheduler : MarshalByRefObject, IPowerScheduler, IPowerController
  {
    #region Variables
    public event PowerSchedulerEventHandler OnPowerSchedulerEvent;
    /// <summary>
    /// PowerScheduler single instance
    /// </summary>
    static PowerScheduler _powerScheduler;
    /// <summary>
    /// mutex lock object to ensure only one instance of the PowerScheduler object
    /// is created.
    /// </summary>
    static readonly object _mutex = new object();
    /// <summary>
    /// Reference to tvservice's TVController
    /// </summary>
    IController _controller;
    /// <summary>
    /// Factory for creating various IStandbyHandlers/IWakeupHandlers
    /// </summary>
    PowerSchedulerFactory _factory;
    /// <summary>
    /// Manages setting the according thread execution state
    /// </summary>
    PowerManager _powerManager;
    /// <summary>
    /// List of registered standby handlers ("disable standby" plugins)
    /// </summary>
    List<IStandbyHandler> _standbyHandlers;
    /// <summary>
    /// List of registered wakeup handlers ("enable wakeup" plugins)
    /// </summary>
    List<IWakeupHandler> _wakeupHandlers;
    /// <summary>
    /// IStandbyHandler for the client in singleseat setups
    /// </summary>
    GenericStandbyHandler _clientStandbyHandler;
    /// <summary>
    /// IWakeupHandler for the client in singleseat setups
    /// </summary>
    GenericWakeupHandler _clientWakeupHandler;
    /// <summary>
    /// Timer for executing periodic checks (should we enter standby..)
    /// </summary>
    System.Timers.Timer _timer;
    /// <summary>
    /// Timer with support for waking up the system
    /// </summary>
    WaitableTimer _wakeupTimer;
    /// <summary>
    /// Last time any activity by the user was detected.
    /// </summary>
    DateTime _lastUserTime;
    /// <summary>
    /// If this is true, the station is unattended.
    /// </summary>
    bool _unattended;
    /// <summary>
    /// Global indicator if the PowerScheduler thinks the system is idle
    /// </summary>
    bool _idle = false;
    /// <summary>
    /// Indicating whether the PowerScheduler is in standby-mode.
    /// </summary>
    bool _standby = false;
    /// <summary>
    /// All PowerScheduler related settings are stored here
    /// </summary>
    PowerSettings _settings;
    /// <summary>
    /// Indicator if remoting has been setup
    /// </summary>
    bool _remotingStarted = false;
    /// <summary>
    /// Indicator if the TVController should be reinitialized
    /// (or if this has already been done)
    /// </summary>
    bool _reinitializeController = false;
    /// <summary>
    /// Indicator if the cards have been stopped
    /// </summary>
    bool _cardsStopped = false;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new PowerScheduler plugin and performs the one-time initialization
    /// </summary>
    PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();
      _clientStandbyHandler = new GenericStandbyHandler();
      _clientWakeupHandler = new GenericWakeupHandler();
      _lastUserTime = DateTime.Now;
      _unattended = false;
      _idle = false;

      // register to power events generated by the system
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
        Log.Debug("PowerScheduler: Registered PowerScheduler as PowerEventHandler to tvservice");
      }
      else
      {
        Log.Error("PowerScheduler: Unable to register power event handler!");
      }

      // Add ourselves to the GlobalServiceProvider
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
      {
        GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
      }
      GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      Log.Debug("PowerScheduler: Registered PowerScheduler service to GlobalServiceProvider");
    }
    ~PowerScheduler()
    {
      // disable the wakeup timer
      _wakeupTimer.SecondsToWait = -1;
      _wakeupTimer.Close();
    }
    #endregion

    #region Public methods

    #region Start/Stop methods
    /// <summary>
    /// Called by the PowerSchedulerPlugin to start the PowerScheduler
    /// </summary>
    /// <param name="controller">TVController from the tvservice</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Start(IController controller)
    {
      _controller = controller;

      Register(_clientStandbyHandler);
      Register(_clientWakeupHandler);
      Log.Debug("PowerScheduler: Registered default set of standby/resume handlers to PowerScheduler");

      // Create the PowerManager that helps setting the correct thread executation state
      _powerManager = new PowerManager();

      // Create the timer that will wakeup the system after a specific amount of time after the
      // system has been put into standby
      if (_wakeupTimer == null)
      {
        _wakeupTimer = new WaitableTimer();
        _wakeupTimer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(OnWakeupTimerExpired);
      }

      // start the timer responsible for standby checking and refreshing settings
      _timer = new System.Timers.Timer();
      _timer.Interval = 60000;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
      _timer.Enabled = true;

      // Configure remoting if not already done
      StartRemoting();

      LoadSettings();

      // Create the default set of standby/resume handlers
      if (_factory == null)
        _factory = new PowerSchedulerFactory(controller);
      _factory.CreateDefaultSet();

      SendPowerSchedulerEvent(PowerSchedulerEventType.Started);

      Log.Info("Powerscheduler: started");
    }
    /// <summary>
    /// Called by the PowerSchedulerPlugin to stop the PowerScheduler
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Stop()
    {
      // stop the global timer responsible for standby checking and refreshing settings
      _timer.Enabled = false;
      _timer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimerElapsed);
      _timer.Dispose();
      _timer = null;

      // dereference the PowerManager instance
      _powerManager = null;

      // Remove the default set of standby/resume handlers
      _factory.RemoveDefaultSet();
      Unregister(_clientStandbyHandler);
      Unregister(_clientWakeupHandler);
      Log.Debug("PowerScheduler: Removed default set of standby/resume handlers to PowerScheduler");

      SendPowerSchedulerEvent(PowerSchedulerEventType.Stopped);

      Log.Info("Powerscheduler: stopped");

    }
    /// <summary>
    /// Configure remoting for power control from MP
    /// </summary>
    private void StartRemoting()
    {
      if (_remotingStarted)
        return;
      try
      {
        ChannelServices.RegisterChannel(new HttpChannel(31457), false);
      }
      catch (RemotingException) { }
      catch (System.Net.Sockets.SocketException) { }
      // RemotingConfiguration.RegisterWellKnownServiceType(typeof(PowerScheduler), "PowerControl", WellKnownObjectMode.Singleton);
      ObjRef objref = RemotingServices.Marshal(this, "PowerControl", typeof(IPowerController));
      RemotePowerControl.Clear();
      Log.Debug("PowerScheduler: Registered PowerScheduler as \"PowerControl\" remoting service");
      _remotingStarted = true;
    }
    #endregion

    #region IPowerScheduler implementation
    /// <summary>
    /// Registers a new IStandbyHandler plugin which can prevent entering standby
    /// </summary>
    /// <param name="handler">handler to register</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Register(IStandbyHandler handler)
    {
      if (!_standbyHandlers.Contains(handler))
        _standbyHandlers.Add(handler);
    }
    /// <summary>
    /// Registers a new IWakeupHandler plugin which can wakeup the system at a desired time
    /// </summary>
    /// <param name="handler">handler to register</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Register(IWakeupHandler handler)
    {
      if (!_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Add(handler);
    }
    /// <summary>
    /// Unregisters a IStandbyHandler plugin
    /// </summary>
    /// <param name="handler">handler to unregister</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unregister(IStandbyHandler handler)
    {
      if (_standbyHandlers.Contains(handler))
        _standbyHandlers.Remove(handler);
    }
    /// <summary>
    /// Unregisters a IWakeupHandler plugin
    /// </summary>
    /// <param name="handler">handler to register</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unregister(IWakeupHandler handler)
    {
      if (_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Remove(handler);
    }
    /// <summary>
    /// Checks if the given IStandbyHandler is registered
    /// </summary>
    /// <param name="handler">IStandbyHandler to check</param>
    /// <returns>is the given handler registered?</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsRegistered(IStandbyHandler handler)
    {
      return _standbyHandlers.Contains(handler);
    }
    /// <summary>
    /// Checks if the given IWakeupHandler is registered
    /// </summary>
    /// <param name="handler">IWakeupHandler to check</param>
    /// <returns>is the given handler registered?</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsRegistered(IWakeupHandler handler)
    {
      return _wakeupHandlers.Contains(handler);
    }
    /// <summary>
    /// Manually puts the system in Standby (Suspend/Hibernate depending on what is configured)
    /// </summary>
    /// <param name="source">description of the source who puts the system into standby</param>
    /// <param name="force">should we ignore PowerScheduler's current state (true) or not? (false)</param>
    /// <returns></returns>
    public bool SuspendSystem(string source, bool force)
    {
      Log.Info("PowerScheduler: Manual system suspend requested by {0}", source);
      return EnterSuspendOrHibernate(force);
    }
    #endregion

    #region IPowerController implementation
    /// <summary>
    /// Allows the PowerScheduler client plugin to register its powerstate with the tvserver PowerScheduler
    /// </summary>
    /// <param name="standbyAllowed">Is standby allowed by the client (true) or not? (false)</param>
    /// <param name="handlerName">Description of the handler preventing standby</param>
    public void SetStandbyAllowed(bool standbyAllowed, string handlerName)
    {
      LogVerbose("PowerScheduler.SetStandbyAllowed: {0} {1}", standbyAllowed, handlerName);
      _clientStandbyHandler.DisAllowShutdown = !standbyAllowed;
      _clientStandbyHandler.HandlerName = handlerName;
    }
    /// <summary>
    /// Allows the PowerScheduler client plugin to set its desired wakeup time
    /// </summary>
    /// <param name="nextWakeupTime">desired (earliest) wakeup time</param>
    /// <param name="handlerName">Description of the handler causing the system to wakeup</param>
    public void SetNextWakeupTime(DateTime nextWakeupTime, string handlerName)
    {
      LogVerbose("PowerScheduler.SetNextWakeupTime: {0} {1}", nextWakeupTime, handlerName);
      _clientWakeupHandler.Update(nextWakeupTime, handlerName);
    }
    /// <summary>
    /// Resets the idle timer of the PowerScheduler. When enough time has passed (IdleTimeout), the system
    /// is suspended as soon as possible (no handler disallows shutdown).
    /// Note that the idle timer is automatically reset when the user moves the mouse or touchs the keyboard.
    /// </summary>
    public void UserActivityDetected(DateTime when)
    {
      if (when > _lastUserTime)
      {
        _lastUserTime = when;
        _unattended = false;
        LogVerbose("PowerScheduler: User input detected at {0}, system is attended", _lastUserTime);
      }
    }

    /// <summary>
    /// Indicates whether or not the client is connected to the server (or not)
    /// </summary>
    public bool IsConnected
    {
      get { return true; }
    }
    public IPowerSettings PowerSettings
    {
      get { return _settings; }
    }

    private int _remoteTags = 0;
    private Hashtable _remoteStandbyHandlers = new Hashtable();
    private Hashtable _remoteWakeupHandlers= new Hashtable();    

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int RegisterRemote(string standbyHandlerURI, string wakeupHandlerURI)
    {
      _remoteTags++;

      LogVerbose("PowerScheduler: RegisterRemote tag: {0}, uris: {1}, {2}", _remoteTags, standbyHandlerURI, wakeupHandlerURI);
      if (standbyHandlerURI != null && standbyHandlerURI.Length > 0)
      {
        RemoteStandbyHandler hdl = new RemoteStandbyHandler(standbyHandlerURI,_remoteTags);
        Register( hdl );
        _remoteStandbyHandlers[_remoteTags]= hdl;
      }
      if (wakeupHandlerURI != null && wakeupHandlerURI.Length > 0)
      {
        RemoteWakeupHandler hdl = new RemoteWakeupHandler(wakeupHandlerURI, _remoteTags);
        Register(hdl);
        _remoteWakeupHandlers[_remoteTags] = hdl;
      }
      return _remoteTags;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void UnregisterRemote(int tag)
    {
      {
        RemoteStandbyHandler hdl = (RemoteStandbyHandler) _remoteStandbyHandlers[tag];
        if (hdl != null)
        {
          _remoteStandbyHandlers.Remove(tag);
          hdl.Close();
          LogVerbose("PowerScheduler: UnregisterRemote StandbyHandler {0}", tag);
          Unregister(hdl);
        }
      }
      {
        RemoteWakeupHandler hdl = (RemoteWakeupHandler) _remoteWakeupHandlers[tag];
        if (hdl != null)
        {
          _remoteWakeupHandlers.Remove(tag);
          hdl.Close();
          LogVerbose("PowerScheduler: UnregisterRemote WakeupHandler {0}", tag);
          Unregister(hdl);
        }
      }
    }

    #endregion

    #region MarshalByRefObject overrides
    /// <summary>
    /// Make sure SAO never expires
    /// </summary>
    /// <returns></returns>
    public override object InitializeLifetimeService()
    {
      return null;
    }
    #endregion

    #endregion

    #region Private methods
    /// <summary>
    /// Called when the wakeup timer is due (when system resumes from standby)
    /// </summary>
    private void OnWakeupTimerExpired()
    {
      Log.Debug("PowerScheduler: OnResume");
    }

    /// <summary>
    /// Periodically refreshes the standby configuration and checks if the system should enter standby
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        LoadSettings();
        CheckForStandby();
        SendPowerSchedulerEvent(PowerSchedulerEventType.Elapsed);
      }
      // explicitly catch exceptions and log them otherwise they are ignored by the Timer object
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// Refreshes the standby configuration
    /// </summary>
    private void LoadSettings()
    {
      int setting;
      bool changed = false;

      if (_settings == null)
        _settings = new PowerSettings();

      TvBusinessLayer layer = new TvBusinessLayer();

      // Check if PowerScheduler should log verbose debug messages
      if (_settings.ExtensiveLogging != Convert.ToBoolean(layer.GetSetting("PowerSchedulerExtensiveLogging", "false").Value))
      {
        _settings.ExtensiveLogging = !_settings.ExtensiveLogging;
        Log.Debug("PowerScheduler: extensive logging enabled: {0}", _settings.ExtensiveLogging);
        changed = true;
      }
      // Check if PowerScheduler should actively put the system into standby
      if (_settings.ShutdownEnabled != Convert.ToBoolean(layer.GetSetting("PowerSchedulerShutdownActive", "false").Value))
      {
        _settings.ShutdownEnabled = !_settings.ShutdownEnabled;
        LogVerbose("PowerScheduler: entering standby is enabled: {0}", _settings.ShutdownEnabled);
        changed = true;
      }
      // Check if PowerScheduler should wakeup the system automatically
      if (_settings.WakeupEnabled != Convert.ToBoolean(layer.GetSetting("PowerSchedulerWakeupActive", "false").Value))
      {
        _settings.WakeupEnabled = !_settings.WakeupEnabled;
        LogVerbose("PowerScheduler: automatic wakeup is enabled: {0}", _settings.WakeupEnabled);
        changed = true;
      }
      // Check if PowerScheduler should force the system into suspend/hibernate
      if (_settings.ForceShutdown != Convert.ToBoolean(layer.GetSetting("PowerSchedulerForceShutdown", "false").Value))
      {
        _settings.ForceShutdown = !_settings.ForceShutdown;
        LogVerbose("PowerScheduler: force shutdown enabled: {0}", _settings.ForceShutdown);
        changed = true;
      }
      // Check if PowerScheduler should reinitialize the TVController after wakeup
      PowerSetting pSetting = _settings.GetSetting("ReinitializeController");
      bool bSetting = Convert.ToBoolean(layer.GetSetting("PowerSchedulerReinitializeController", "false").Value);
      if (pSetting.Get<bool>() != bSetting)
      {
        pSetting.Set<bool>(bSetting);
        LogVerbose("PowerScheduler: Reinitialize tvservice controller on wakeup: {0}", bSetting);
        changed = true;
      }

      pSetting = _settings.GetSetting("ExternalCommand");
      string sSetting = layer.GetSetting("PowerSchedulerCommand", String.Empty).Value;
      if (!sSetting.Equals(pSetting.Get<string>()))
      {
        pSetting.Set<string>(sSetting);
        LogVerbose("PowerScheduler: Run external command before standby / after resume: {0}", sSetting);
        changed = true;
      }

      // Check configured PowerScheduler idle timeout
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerIdleTimeout", "5").Value);
      if (_settings.IdleTimeout != setting)
      {
        _settings.IdleTimeout = setting;
        LogVerbose("PowerScheduler: idle timeout set to: {0} minutes", _settings.IdleTimeout);
        changed = true;
      }
      // Check configured pre-wakeup time
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerPreWakeupTime", "60").Value);
      if (_settings.PreWakeupTime != setting)
      {
        _settings.PreWakeupTime = setting;
        LogVerbose("PowerScheduler: pre-wakeup time set to: {0} seconds", _settings.PreWakeupTime);
        changed = true;
      }

      // Check configured pre-no-shutdown time
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerPreNoShutdownTime", "120").Value);
      if (_settings.PreNoShutdownTime != setting)
      {
        _settings.PreNoShutdownTime = setting;
        LogVerbose("PowerScheduler: pre-no-shutdown time set to: {0} seconds", _settings.PreNoShutdownTime);
        changed = true;
      }

      // Check if check interval needs to be updated
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerCheckInterval", "60").Value);
      if (_settings.CheckInterval != setting)
      {
        _settings.CheckInterval = setting;
        LogVerbose("PowerScheduler: Check interval set to {0} seconds", _settings.CheckInterval);
        setting *= 1000;
        _timer.Interval = setting;
        changed = true;
      }
      // Check configured shutdown mode
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerShutdownMode", "2").Value);
      if ((int)_settings.ShutdownMode != setting)
      {
        _settings.ShutdownMode = (ShutdownMode)setting;
        LogVerbose("PowerScheduler: Shutdown mode set to {0}", _settings.ShutdownMode);
        changed = true;
      }

      // Send message in case any setting has changed
      if (changed)
      {
        PowerSchedulerEventArgs args = new PowerSchedulerEventArgs(PowerSchedulerEventType.SettingsChanged);
        args.SetData<PowerSettings>(_settings.Clone());
        SendPowerSchedulerEvent(args);
      }
    }

    /// <summary>
    /// struct for GetLastInpoutInfo
    /// </summary>
    internal struct LASTINPUTINFO
    {
      public uint cbSize;
      public uint dwTime;
    }

    /// <summary>
    /// The GetLastInputInfo function retrieves the time of the last input event.
    /// </summary>
    /// <param name="plii"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    /// <summary>
    /// Returns the current tick as uint (pref. over Environemt.TickCount which only uses int)
    /// </summary>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern uint GetTickCount();

    /// <summary>
    /// This functions returns the time of the last user input recogniized,
    /// i.e. mouse moves or keyboard inputs.
    /// </summary>
    /// <returns>Last time of user input</returns>
    DateTime GetLastInputTime()
    {
      LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
      lastInputInfo.cbSize = (uint) Marshal.SizeOf(lastInputInfo);

      if (!GetLastInputInfo(ref lastInputInfo))
      {
        Log.Error("PowerScheduler: Unable to GetLastInputInfo!");
        return DateTime.MinValue;
      }

      long lastKick= lastInputInfo.dwTime;
      long tick = GetTickCount();

      long delta = lastKick - tick;

      if (delta > 0)
      {
        // there was an overflow (restart at 0) in the tick-counter!
        delta = delta - uint.MaxValue - 1;
      }

      return DateTime.Now.AddMilliseconds( delta );
    }

    /// <summary>
    /// Checks if the system should enter standby
    /// </summary>
    private void CheckForStandby()
    {
      if (!_settings.ShutdownEnabled)
        return;

      // adjust _lastUserTime by user activity
      DateTime userInput = GetLastInputTime();
      if (userInput > _lastUserTime)
      {
        _lastUserTime = userInput;
        _unattended = false;
        LogVerbose("PowerScheduler: User input detected at {0}, system is attended", _lastUserTime);
      }

      // update _unattended
      bool dummy = Unattended;

      // is anybody disallowing shutdown?
      if (!DisAllowShutdown)
      {
        if (!_idle)
        {
          Log.Info("PowerScheduler: System changed from busy state to idle state");
          _idle = true;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemIdle);
        }

        if (_unattended)
        {
          Log.Info("PowerScheduler: System is unattended and idle - initiate suspend/hibernate");
          EnterSuspendOrHibernate();
        }
      }
      else
      {
        if (_idle)
        {
          Log.Info("PowerScheduler: System changed from idle state to busy state");
          _idle = false;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
        }
      }
    }

    /// <summary>
    /// Windows PowerEvent handler
    /// </summary>
    /// <param name="powerStatus">PowerBroadcastStatus the system is changing to</param>
    /// <returns>bool indicating if the broadcast was honoured</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private bool OnPowerEvent(System.ServiceProcess.PowerBroadcastStatus powerStatus)
    {
      switch (powerStatus)
      {
        case System.ServiceProcess.PowerBroadcastStatus.QuerySuspend:
          Log.Debug("PowerScheduler: System wants to enter standby");
          bool idle = !DisAllowShutdown;
          Log.Debug("PowerScheduler: System idle: {0}", idle);
          if (!_unattended)
          {
            // scenario: User wants to standby the system, but we disallowed him to do
            // so. We go to unattended mode.
            Log.Info("PowerScheduler: User tries to standby, system is unattended");
            _unattended = true;
          }
          return idle;
        case System.ServiceProcess.PowerBroadcastStatus.Suspend:
          _standby = true;
          _timer.Enabled = false;
          _controller.EpgGrabberEnabled = false;
          FreeTVCards();
          SendPowerSchedulerEvent(PowerSchedulerEventType.EnteringStandby, false);
          SetWakeupTimer();
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.QuerySuspendFailed:
          Log.Debug("PowerScheduler: Entering standby was disallowed (blocked)");
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.ResumeAutomatic:
          Log.Debug("PowerScheduler: System has resumed automatically from standby");
          Resume(true);
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.ResumeCritical:
          Log.Debug("PowerScheduler: System has resumed from standby after a critical suspend");
          Resume(true);
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.ResumeSuspend:
          // note: this event may not arrive unless the user has moved the mouse or hit a key
          // so, we should also handle ResumeAutomatic and ResumeCritical (as done above)
          Log.Debug("PowerScheduler: System has resumed from standby");
          Resume(true);
          return true;
      }
      return true;
    }

    private void Resume( bool resume )
    {
      if (!_standby) return;
      _standby = false;

      lock (this)
      {
        // if real resume, run command
        if( resume )
          RunExternalCommand("wakeup");

        // reinitialize TVController if system is configured to do so and not already done
        ReinitializeController();
      }
      ResetAndEnableTimer();
      if (!_controller.EpgGrabberEnabled)
        _controller.EpgGrabberEnabled = true;
      SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
    }

    /// <summary>
    /// Resets the last time the system changed from busy to idle state
    /// and re-enables the timer which periodically checks for config changes/power management
    /// </summary>
    private void ResetAndEnableTimer()
    {
      _lastUserTime = DateTime.Now;
      _unattended = false;
      if (_idle)
      {
        Log.Info("PowerScheduler: System changed from idle state to busy state");
        _idle = false;
        SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
      }
      if (_timer != null)
        _timer.Enabled = true;
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    private bool EnterSuspendOrHibernate()
    {
      return EnterSuspendOrHibernate(_settings.ForceShutdown);
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <param name="force">should the system be forced to enter standby?</param>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    private bool EnterSuspendOrHibernate(bool force)
    {
      if (DisAllowShutdown && !force)
        return false;
      // determine standby mode
      PowerState state = PowerState.Suspend;
      switch (_settings.ShutdownMode)
      {
        case ShutdownMode.Suspend:
          state = PowerState.Suspend;
          break;
        case ShutdownMode.Hibernate:
          state = PowerState.Hibernate;
          break;
        case ShutdownMode.StayOn:
          Log.Debug("PowerScheduler: Standby requested but system is configured to stay on");
          return false;
        default:
          Log.Error("PowerScheduler: unknown shutdown mode: {0}", _settings.ShutdownMode);
          return false;
      }

      // make sure we set the wakeup/resume timer before entering standby
      SetWakeupTimer();

      // activate standby
      RunExternalCommand("standby");
      Log.Info("PowerScheduler: entering {0} ; forced: {1}", state, force);
      return Application.SetSuspendState(state, force, false);
    }

    /// <summary>
    /// Sets the wakeup timer to the earliest desirable wakeup time
    /// </summary>
    private void SetWakeupTimer()
    {
      if (_settings.WakeupEnabled)
      {
        // determine next wakeup time from IWakeupHandlers
        DateTime nextWakeup = NextWakeupTime;
        bool disallow = DisAllowShutdown;
        if (nextWakeup < DateTime.MaxValue || disallow)
        {
          double delta;
          if (disallow) delta = 0; // should instantly restart
          else
          {
            nextWakeup = nextWakeup.AddSeconds(-_settings.PreWakeupTime);
            delta = nextWakeup.Subtract(DateTime.Now).TotalSeconds;
          }
          
          if( delta < 45 )
          {
            // the wake up event is too near, when we set the timer and the suspend process takes to long, i.e. the timer gets fired
            // while suspending, the system would NOT wake up!

            // so, we will in any case set the wait time to 45 seconds
            delta= 45;
          }
          _wakeupTimer.SecondsToWait = delta;
          Log.Debug("PowerScheduler: Set wakeup timer to wakeup system in {0} minutes", delta/60);
        }
        else
        {
          Log.Debug("PowerScheduler: No pending events found in the future which should wakeup the system");
          _wakeupTimer.SecondsToWait = -1;
        }
      }
    }

    #region Message handling
    /// <summary>
    /// Sends the given PowerScheduler event type to receivers 
    /// </summary>
    /// <param name="eventType">Event type to send</param>
    private void SendPowerSchedulerEvent(PowerSchedulerEventType eventType)
    {
      SendPowerSchedulerEvent(eventType, true);
    }

    private void SendPowerSchedulerEvent(PowerSchedulerEventType eventType, bool sendAsync)
    {
      PowerSchedulerEventArgs args = new PowerSchedulerEventArgs(eventType);
      SendPowerSchedulerEvent(args, sendAsync);
    }

    /// <summary>
    /// Sends the given PowerSchedulerEventArgs to receivers
    /// </summary>
    /// <param name="args">PowerSchedulerEventArgs to send</param>
    private void SendPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      SendPowerSchedulerEvent(args, true);
    }

    /// <summary>
    /// Sends the given PowerSchedulerEventArgs to receivers
    /// </summary>
    /// <param name="args">PowerSchedulerEventArgs to send</param>
    /// <param name="sendAsync">bool indicating whether or not to send it asynchronously</param>
    private void SendPowerSchedulerEvent(PowerSchedulerEventArgs args, bool sendAsync)
    {
      if (OnPowerSchedulerEvent == null)
        return;
      lock (OnPowerSchedulerEvent)
      {
        if (OnPowerSchedulerEvent == null)
          return;
        if (sendAsync)
        {
          OnPowerSchedulerEvent(args);
        }
        else
        {
          foreach (Delegate del in OnPowerSchedulerEvent.GetInvocationList())
          {
            PowerSchedulerEventHandler handler = del as PowerSchedulerEventHandler;
            handler(args);
          }
        }
      }
    }
    #endregion

    /// <summary>
    /// action: standby, wakeup, epg
    /// </summary>
    /// <param name="action"></param>
    public void RunExternalCommand(String action)
    {
      PowerSetting setting = _settings.GetSetting("ExternalCommand");
      if (setting.Get<string>().Equals(String.Empty))
        return;
      using (Process p = new Process())
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = setting.Get<string>();
        psi.UseShellExecute = true;
        psi.WindowStyle = ProcessWindowStyle.Minimized;
        psi.Arguments = action;
        p.StartInfo = psi;
        LogVerbose("Starting external command: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
        try
        {
          p.Start();
          p.WaitForExit();
        }
        catch (Exception e)
        {
          Log.Write(e);
        }
        LogVerbose("External command finished");
      }
    }

    #region Logging wrapper methods
    private void LogVerbose(string msg)
    {
      //don't just do this: LogVerbose(msg, null);!!
      if (_settings.ExtensiveLogging)
        Log.Debug(msg);
    }
    private void LogVerbose(string format, params object[] args)
    {
      if (_settings.ExtensiveLogging)
        Log.Debug(format, args);
    }
    #endregion

    /// <summary>
    /// Frees the tv tuners before entering standby
    /// </summary>
    private void FreeTVCards()
    {
      if (_cardsStopped)
        return;
      // only free tuner cards if reinitialization is enabled in settings
      if (_settings.GetSetting("ReinitializeController").Get<bool>())
      {
        TvService.TVController controller = _controller as TvService.TVController;
        if (controller != null)
        {
          Log.Debug("PowerScheduler: Stopping TVController");
          controller.DeInit();
          _cardsStopped = true;
          _reinitializeController = true;
        }
      }
    }

    /// <summary>
    /// Restarts the TVController when resumed from standby
    /// </summary>
    private void ReinitializeController()
    {
      if (!_reinitializeController)
        return;
      // only reinitialize controller if enabled in settings
      if (_settings.GetSetting("ReinitializeController").Get<bool>())
      {
        TvService.TVController controller = _controller as TvService.TVController;
        if (controller != null && _reinitializeController)
        {
          Log.Debug("PowerScheduler: reinitializing the tvservice TVController");
          controller.Restart();
          _reinitializeController = false;
          _cardsStopped = false;
        }
      }
    }

    #endregion

    private DateTime _currentNextWakeupTime = DateTime.MaxValue;
    private String _currentNextWakeupHandler = "";
    private bool _currentDisAllowShutdown = false;
    private String _currentDisAllowShutdownHandler = "";

    public void GetCurrentState(bool refresh, out bool unattended, out bool disAllowShutdown, out String disAllowShutdownHandler, out DateTime nextWakeupTime, out String nextWakeupHandler)
    {
      if (refresh)
      {
        bool dummy= DisAllowShutdown;
        DateTime dummy2= NextWakeupTime;
        dummy = Unattended;
      }

      // give state
      unattended = _unattended;
      disAllowShutdown = _currentDisAllowShutdown;
      disAllowShutdownHandler = _currentDisAllowShutdownHandler;
      nextWakeupTime = _currentNextWakeupTime;
      nextWakeupHandler = _currentNextWakeupHandler;
    }

    #region Private properties

    /// <summary>
    /// Checks whether the system is unattended, i.e. the user was idle some time.
    /// </summary>
    private bool Unattended
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        if (!_unattended && _lastUserTime <= DateTime.Now.AddMinutes(-_settings.IdleTimeout))
        {
          Log.Info("PowerScheduler: User idle since {0}, system is unattended", _lastUserTime);
          _unattended = true;
        
        }
        return _unattended;
      }

    }

    /// <summary>
    /// Checks all IStandbyHandlers if one of them wants to prevent standby;
    /// returns false if one of them does; returns true of none of them does.
    /// </summary>
    private bool DisAllowShutdown
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // check whether the next event is almost due, i.e. within PreNoShutdownTime seconds
        DateTime nextWakeupTime = NextWakeupTime;
        if (DateTime.Now >= nextWakeupTime.AddSeconds(-_settings.PreNoShutdownTime))
        {
          LogVerbose("PowerScheduler.DisAllowShutdown: some event is almost due");
          _currentDisAllowShutdownHandler= "EVENT-DUE";
          _currentDisAllowShutdown = true;
          return true;
        }

        foreach (IStandbyHandler handler in _standbyHandlers)
        {
          LogVerbose("PowerScheduler.DisAllowShutdown: inspecting handler {0}", handler.HandlerName);
          if (handler.DisAllowShutdown)
          {
            LogVerbose("PowerScheduler.DisAllowShutdown: handler {0} wants to prevent standby", handler.HandlerName);
            _currentDisAllowShutdownHandler= handler.HandlerName;
            _currentDisAllowShutdown = true;
            _powerManager.PreventStandby();
            return true;
          }
          LogVerbose("PowerScheduler.DisAllowShutdown: handler {0} inspected and allows standby", handler.HandlerName);
        }
        _currentDisAllowShutdown = false;
        _currentDisAllowShutdownHandler= "";        
        _powerManager.AllowStandby();
        return false;
      }
    }

    /// <summary>
    /// Returns the earliest desirable wakeup time from all IWakeupHandlers
    /// </summary>
    private DateTime NextWakeupTime
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // earliestWakeupTime is set to "now" in order to not miss wakeups that are almost due.
        // preWakupTime is not substracted here.
        String handlerName = "";

        DateTime nextWakeupTime = DateTime.MaxValue;
        DateTime earliestWakeupTime = DateTime.Now;
        
        //too much logging Log.Debug("PowerScheduler: earliest wakeup time: {0}", earliestWakeupTime); 
        foreach (IWakeupHandler handler in _wakeupHandlers)
        {
          DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
          if (nextTime < earliestWakeupTime) nextTime = DateTime.MaxValue;
          LogVerbose("PowerScheduler.NextWakeupTime: inspecting handler:{0} time:{1}", handler.HandlerName, nextTime);
          if (nextTime < nextWakeupTime && nextTime >= earliestWakeupTime)
          {
            //too much logging Log.Debug("PowerScheduler: found next wakeup time {0} by {1}", nextTime, handler.HandlerName);
            handlerName = handler.HandlerName;
            nextWakeupTime = nextTime;
          }
        }

        _currentNextWakeupHandler = handlerName;

        // next wake-up time changed?
        if (nextWakeupTime != _currentNextWakeupTime)
        {
          _currentNextWakeupTime = nextWakeupTime;
          
          Log.Debug("PowerScheduler: new next wakeup time {0} found by {1}", nextWakeupTime, handlerName);
        }

        return nextWakeupTime;
      }
    }

    #endregion

    #region Public properties
    public static PowerScheduler Instance
    {
      get
      {
        if (_powerScheduler == null)
        {
          lock (_mutex)
          {
            if (_powerScheduler == null)
            {
              _powerScheduler = new PowerScheduler();
            }
          }
        }
        return _powerScheduler;
      }
    }
    public PowerSettings Settings
    {
      get { return _settings; }
    }
    #endregion

  }
}
