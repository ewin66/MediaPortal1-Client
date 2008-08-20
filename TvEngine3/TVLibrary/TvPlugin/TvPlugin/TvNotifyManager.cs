#region Copyright (C) 2005-2008 Team MediaPortal
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Text;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Configuration;

using Gentle.Common;
using Gentle.Framework;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;

namespace TvPlugin
{
  public class TvNotifyManager
  {
    System.Windows.Forms.Timer _timer;
    // flag indicating that notifies have been added/changed/removed
    static bool _notifiesListChanged;
    static bool _enableNotification;
    int _preNotifyConfig;
    //list of all notifies (alert me n minutes before program starts)
    IList _notifiesList;
    IList _notifiedRecordings;
    User _dummyuser;

    public TvNotifyManager()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _preNotifyConfig = xmlreader.GetValueAsInt("movieplayer", "notifyTVBefore", 300);
        _enableNotification = xmlreader.GetValueAsBool("mytv", "enableTvNotifier", true);
      }
     
      
      _timer = new System.Windows.Forms.Timer();

      // check every 15 seconds for notifies
      _dummyuser = new User();
      _dummyuser.IsAdmin = false;
      _dummyuser.Name = "Free channel checker";
      _timer.Interval = 15000;
      _timer.Enabled = true;
      _timer.Tick += new EventHandler(_timer_Tick);
    }

    public static void OnNotifiesChanged()
    {
      Log.Info("TvNotify:OnNotifiesChanged");
      _notifiesListChanged = true;
    }

    void LoadNotifies()
    {
      try
      {
        Log.Info("TvNotify:LoadNotifies");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
        sb.AddConstraint(Operator.Equals, "notify", 1);
        SqlStatement stmt = sb.GetStatement(true);
        _notifiesList = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
        _notifiedRecordings = new ArrayList();
        if (_notifiesList != null)
        {
          Log.Info("TvNotify: {0} notifies", _notifiesList.Count);
        }
      }
      catch (Exception)
      {
      }
    }


    void _timer_Tick(object sender, EventArgs e)
    {

      if (!_enableNotification) { return; };
      if (_notifiesListChanged)
      {
        LoadNotifies();
        _notifiesListChanged = false;
      }
      DateTime preNotifySecs = DateTime.Now.AddSeconds(_preNotifyConfig);
      if (_notifiesList != null && _notifiesList.Count > 0)
      {
        foreach (Program program in _notifiesList)
        {
          if (preNotifySecs > program.StartTime)
          {
            Log.Info("Notify {0} on {1} start {2}", program.Title, program.ReferencedChannel().DisplayName, program.StartTime);
            program.Notify = false;
            program.Persist();

            MediaPortal.TV.Database.TVProgram tvProg = new MediaPortal.TV.Database.TVProgram();
            tvProg.Channel = program.ReferencedChannel().DisplayName;
            tvProg.Title = program.Title;
            tvProg.Description = program.Description;
            tvProg.Genre = program.Genre;
            tvProg.Start = Utils.datetolong(program.StartTime);
            tvProg.End = Utils.datetolong(program.EndTime);

            _notifiesList.Remove(program);
            Log.Info("send notify");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
            msg.Object = tvProg;
            GUIGraphicsContext.SendMessage(msg);
            msg = null;
            Log.Info("send notify done");
            return;
          }
        }
      }
      //Log.Debug("TVPlugIn: Notifier checking for recording to start at {0}", preNotifySecs);
      if (g_Player.IsTV && TVHome.Card.IsTimeShifting && g_Player.Playing )
      {
        if (TVHome.TvServer.IsTimeToRecord(preNotifySecs))
        {
          try
          {
            IList schedulesList = Schedule.ListAll();
            foreach (Schedule rec in schedulesList)
            {
              //Check if alerady notified user
              foreach (Schedule notifiedRec in _notifiedRecordings)
              {
                if (rec == notifiedRec)
                {
                  return;

                }
              }
              //Check if timing it's time 
              Log.Debug("TVPlugIn: Notifier checking program {0}", rec.ProgramName);
              if (TVHome.TvServer.IsTimeToRecord(preNotifySecs, rec.IdSchedule))
              {
                //check if freecard is available. 
                //Log.Debug("TVPlugIn: Notify verified program {0} about to start recording. {1} / {2}", rec.ProgramName, rec.StartTime, preNotifySecs);
                if (TVHome.Navigator.Channel.IdChannel != rec.IdChannel && (int)TVHome.TvServer.GetChannelState(rec.IdChannel, _dummyuser) == 0) //not tunnable
                {
                  Log.Debug("TVPlugIn: No free card available for {0}. Notifying user.", rec.ProgramName);
                  GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
                  if (pDlgNotify != null)
                  {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TV, 0, 0, 0, 0, 0, null);
                    string logo = Utils.GetCoverArt(Thumbs.TVChannel, TVHome.Navigator.CurrentChannel);
                    GUIGraphicsContext.SendMessage(msg); //Send the message so the miniguide 
                    //msg.Object = tvProg;
                    pDlgNotify.Reset();
                    pDlgNotify.ClearAll();
                    pDlgNotify.SetImage(logo);
                    _notifiedRecordings.Add(rec);
                    pDlgNotify.SetHeading(1004);//About to start recording
                    pDlgNotify.SetText(String.Format("{0}. {1}", rec.ProgramName, GUILocalizeStrings.Get(200055))); //TvViewing might be disrupted. 
                    pDlgNotify.TimeOut = 10;
                    pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            Log.Debug("Tv NotifyManager: Exception at recording notification {0}", ex.ToString());
          }
        }
      }
    }
  }
}
