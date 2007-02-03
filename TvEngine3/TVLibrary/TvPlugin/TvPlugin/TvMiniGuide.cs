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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;

using TvDatabase;


using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  /// <summary>
  /// GUIMiniGuide
  /// </summary>
  /// 
  public class TvMiniGuide : GUIWindow, IRenderLayer
  {
    // Member variables                                  
    [SkinControlAttribute(34)]
    protected GUIButtonControl cmdExit = null;
    [SkinControlAttribute(35)]
    protected GUIListControl lstChannels = null;
    [SkinControlAttribute(36)]
    protected GUISpinControl spinGroup = null;

    bool m_bRunning = false;
    bool _altLayout = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;
    List<Channel> tvChannelList = null;
    List<ChannelGroup> ChannelGroupList = null;
    Channel _selectedChannel;
    bool _zap = true;

    /// <summary>
    /// Constructor
    /// </summary>
    public TvMiniGuide()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MINI_GUIDE;
    }
    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_MINI_GUIDE, this);
      Restore();
      PreInit();
      ResetAllControls();
    }
    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is tv.
    /// </summary>
    /// <value><c>true</c> if this instance is tv; otherwise, <c>false</c>.</value>
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Gets or sets the selected channel.
    /// </summary>
    /// <value>The selected channel.</value>
    public Channel SelectedChannel
    {
      get
      {
        return _selectedChannel;
      }
      set
      {
        _selectedChannel = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [auto zap].
    /// </summary>
    /// <value><c>true</c> if [auto zap]; otherwise, <c>false</c>.</value>
    public bool AutoZap
    {
      get
      {
        return _zap;
      }
      set
      {
        _zap = value;
      }
    }

    /// <summary>
    /// Init method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _altLayout = xmlreader.GetValueAsBool("mytv", "altminiguide", true);
      }
      bool bResult = Load(GUIGraphicsContext.Skin + @"\TVMiniGuide.xml");

      GetID = (int)GUIWindow.Window.WINDOW_MINI_GUIDE;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      return bResult;
    }


    /// <summary>
    /// Renderer
    /// </summary>
    /// <param name="timePassed"></param>
    public override void Render(float timePassed)
    {
      base.Render(timePassed);		// render our controls to the screen
    }

    /// <summary>
    /// On close
    /// </summary>
    void Close()
    {
      Log.Debug("miniguide:close()");
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        m_bRunning = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    /// <summary>
    /// On Message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (message.SenderControlId == 35) // listbox
            {
              if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                // switching logic
                SelectedChannel = (Channel)lstChannels.SelectedListItem.MusicTag;
                if (AutoZap)
                {
                  string selectedChan = (string)lstChannels.SelectedListItem.TVTag;
                  if (TVHome.Navigator.CurrentChannel != selectedChan)
                  {
                    TVHome.Navigator.ZapToChannel(tvChannelList[lstChannels.SelectedListItemIndex], false);
                    TVHome.Navigator.ZapNow();
                  }
                }
                Close();
              }
            }
            else if (message.SenderControlId == 36) // spincontrol
            {
              // switch group
              TVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
              FillChannelList();
            }
            else if (message.SenderControlId == 34) // exit button
            {
              // exit
              Close();
            }
            break;
          }
      }
      return base.OnMessage(message);
    }

    /// <summary>
    /// On action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_CONTEXT_MENU:
          //m_bRunning = false;
          Close();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          //m_bRunning = false;
          Close();
          return;
        case Action.ActionType.ACTION_MOVE_LEFT:
          // switch group
          spinGroup.MoveUp();
          TVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
          FillChannelList();
          return;
        case Action.ActionType.ACTION_MOVE_RIGHT:
          // switch group
          spinGroup.MoveDown();
          TVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
          FillChannelList();
          return;
      }
      base.OnAction(action);
    }

    /// <summary>
    /// Page gets destroyed
    /// </summary>
    /// <param name="new_windowId"></param>
    protected override void OnPageDestroy(int new_windowId)
    {
      Log.Debug("miniguide OnPageDestroy");
      base.OnPageDestroy(new_windowId);
      m_bRunning = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {
      Log.Debug("miniguide onpageload");
      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      AllocResources();
      ResetAllControls();							// make sure the controls are positioned relevant to the OSD Y offset
      FillChannelList();
      FillGroupList();
      base.OnPageLoad();
    }

    /// <summary>
    /// Fill up the list with groups
    /// </summary>
    public void FillGroupList()
    {
      ChannelGroup current = null;
      ChannelGroupList = TVHome.Navigator.Groups;
      // empty list of channels currently in the 
      // spin control
      spinGroup.Reset();
      // start to fill them up again
      for (int i = 0; i < ChannelGroupList.Count; i++)
      {
        current = ChannelGroupList[i];
        spinGroup.AddLabel(current.GroupName, i);
        // set selected
        if (current.GroupName.CompareTo(TVHome.Navigator.CurrentGroup.GroupName) == 0)
          spinGroup.Value = i;
      }
    }

    /// <summary>
    /// Fill the list with channels
    /// </summary>
    public void FillChannelList()
    {
      Log.Info("FillChannelList#1");
      tvChannelList = new List<Channel>();
      foreach (GroupMap map in TVHome.Navigator.CurrentGroup.ReferringGroupMap())
      {
        Channel ch = map.ReferencedChannel();
        if (ch.VisibleInGuide && ch.IsTv)
          tvChannelList.Add(ch);
      }
      Log.Info("FillChannelList#2");
      TvBusinessLayer layer = new TvBusinessLayer();
      Dictionary<int, NowAndNext> listNowNext = layer.GetNowAndNext();
      Log.Info("FillChannelList#3");
      lstChannels.Clear();
      Channel current = null;
      GUIListItem item = null;
      string logo = "";
      int selected = 0;

      for (int i = 0; i < tvChannelList.Count; i++)
      {
        current = tvChannelList[i];
        if (current.VisibleInGuide)
        {
          NowAndNext prog;
          if (listNowNext.ContainsKey(current.IdChannel) != false)
          {
            prog = listNowNext[current.IdChannel];
          }
          else
          {
            prog = new NowAndNext(current.IdChannel, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), DateTime.Now.AddHours(2), DateTime.Now.AddHours(3), "no information", "no information", -1, -1);
          }

          StringBuilder sb = new StringBuilder();
          item = new GUIListItem("");
          // store here as it is not needed right now - please beat me later..
          item.TVTag = current.Name;
          item.MusicTag = current;
          if (!_altLayout)
            item.Label2 = current.Name;
          logo = Utils.GetCoverArt(Thumbs.TVChannel, current.Name);

          // if we are watching this channel mark it
          if (TVHome.Navigator.Channel.IdChannel == tvChannelList[i].IdChannel)
          {
            item.IsRemote = true;
            selected = lstChannels.Count;
          }

          if (System.IO.File.Exists(logo))
          {
            item.IconImageBig = logo;
            item.IconImage = logo;
          }
          else
          {
            item.IconImageBig = string.Empty;
            item.IconImage = string.Empty;
          }

          item.Label2 = prog.TitleNow;
          //                    item.Label3 = prog.Title + " [" + prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "-" + prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "]";
          if (_altLayout)
          {
            item.Label3 = GUILocalizeStrings.Get(789) + prog.TitleNow;
            sb.Append(current.Name);
            sb.Append(" - ");
            sb.Append(CalculateProgress(prog.NowStartTime, prog.NowEndTime).ToString());
            sb.Append("%");
            item.Label2 = sb.ToString();
          }
          else
            item.Label3 = prog.TitleNow + ": " + CalculateProgress(prog.NowStartTime, prog.NowEndTime).ToString() + "%";

          
          if (!_altLayout)
            item.Label = prog.TitleNext;
          else
            item.Label = GUILocalizeStrings.Get(790) + prog.TitleNext;

          lstChannels.Add(item);
        }
      }
      Log.Info("FillChannelList#4");
      lstChannels.SelectedListItemIndex = selected;
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
    /// Do this modal
    /// </summary>
    /// <param name="dwParentId"></param>
    public void DoModal(int dwParentId)
    {
      Log.Debug("miniguide domodal");
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        Log.Debug("parentwindow=0");
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      m_bRunning = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
        if (!GUIGraphicsContext.Vmr9Active)
          System.Threading.Thread.Sleep(50);
      }
      GUILayerManager.UnRegisterLayer(this);

      Log.Debug("miniguide closed");
    }

    // Overlay IRenderLayer members
    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      if (m_bRunning)
        Render(timePassed);
    }
    #endregion
  }
}