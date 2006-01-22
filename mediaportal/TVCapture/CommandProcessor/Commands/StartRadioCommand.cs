/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion

namespace MediaPortal.TV.Recording
{
  public class StartRadioCommand : CardCommand
  {
    string _stationName;

    public StartRadioCommand(string radioStation)
    {
      RadioStation = radioStation;
    }

    public string RadioStation
    {
      get { return _stationName; }
      set { _stationName = value; }
    }

    public override void Execute(CommandProcessor handler)
    {
      Log.WriteFile(Log.LogType.Recorder, "Command:Start radio:{0}", RadioStation);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msg);
      TurnTvOff(handler,-1);
      RadioStation radiostation;
      if (!RadioDatabase.GetStation(RadioStation, out radiostation))
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio()  unknown station:{0}", RadioStation);
        return;
      }

      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        TVCaptureDevice tvcard = handler.TVCards[i];
        if (!tvcard.IsRecording)
        {
          if (RadioDatabase.CanCardTuneToStation(RadioStation, tvcard.ID) || handler.TVCards.Count == 1)
          {
            for (int x = 0; x < handler.TVCards.Count; ++x)
            {
              TVCaptureDevice dev = handler.TVCards[x];
              if (i != x)
              {
                if (dev.IsRadio)
                {
                  dev.Stop();
                }
              }
            }
            handler.CurrentCardIndex = i;
            Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio()  start on card:{0} station:{1}", tvcard.ID, RadioStation);
            tvcard.StartRadio(radiostation);
            /*if (tvcard.IsTimeShifting)
            {
              string strTimeShiftFileName=GetTimeShiftFileNameByCardId(tvcard.ID);

              Log.WriteFile(Log.LogType.Recorder,"Recorder:  currentfile:{0} newfile:{1}", g_Player.CurrentFile,strTimeShiftFileName);
              g_Player.Play(strTimeShiftFileName);
            }*/
            return;
          }
        }
      }
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio()  no free card which can listen to radio channel:{0}", RadioStation);
    }

    void TurnTvOff(CommandProcessor handler, int exceptCard)
    {
      StopTvCommand cmd = new StopTvCommand(exceptCard);
      cmd.Execute(handler);
    }
  }
}
