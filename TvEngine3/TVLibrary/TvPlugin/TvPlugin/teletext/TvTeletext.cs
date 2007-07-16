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
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Configuration;
using MediaPortal.GUI.Pictures;

using TvDatabase;

using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin {
  /// <summary>
  /// Teletext window of TVE3
  /// </summary>
  public class TVTeletext : TvTeletextBase {
    #region gui components
    [SkinControlAttribute(502)]
    protected GUIButtonControl btnPage100 = null;
    [SkinControlAttribute(503)]
    protected GUIButtonControl btnPage200 = null;
    [SkinControlAttribute(504)]
    protected GUIButtonControl btnPage300 = null;
    [SkinControlAttribute(505)]
    protected GUIToggleButtonControl btnHidden = null;
    [SkinControlAttribute(506)]
    protected GUISelectButtonControl btnSubPage = null;
    [SkinControlAttribute(507)]
    protected GUIButtonControl btnFullscreen = null;
    #endregion

    #region ctor
    public TVTeletext() {
      GetID = (int)GUIWindow.Window.WINDOW_TELETEXT;
    }
    #endregion

    #region GUIWindow initializing methods
    public override bool Init() {
      return Load(GUIGraphicsContext.Skin + @"\myteletext.xml");
    }
    public override void OnAdded() {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TELETEXT, this);
      Restore();
      PreInit();
      ResetAllControls();
    }
    protected override void OnPageDestroy(int newWindowId) {
      // Save the settings and then stop the update thread. Also the teletext grabbing
      SaveSettings();
      _updateThreadStop = true;
      TVHome.Card.GrabTeletext = false;
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad() {
      base.OnPageLoad();
      // Deactivate fullscreen video and initialize the window
      GUIGraphicsContext.IsFullScreenVideo = false;
      btnSubPage.RestoreSelection = false;
      InitializeWindow(false);
      btnHidden.Selected = _hiddenMode;
    }
    #endregion

    #region OnAction
    public override void OnAction(Action action) {
      base.OnAction(action);
      switch (action.wID) {
        case Action.ActionType.ACTION_SWITCH_TELETEXT_HIDDEN:
          //Change Hidden Mode
          if (btnHidden != null) {
            btnHidden.Selected = _hiddenMode;
          }
          break;
        case Action.ActionType.ACTION_REMOTE_SUBPAGE_UP:
        case Action.ActionType.ACTION_REMOTE_SUBPAGE_DOWN:
          // Subpage up
          if (btnSubPage != null) {
            btnSubPage.SelectedItem = currentSubPageNumber;
          }
          break;
      }
    }
    #endregion

    #region OnClicked
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
      // Handle the click events, of this window
      if (control == btnPage100) {
        currentPageNumber = 0x100;
        currentSubPageNumber = 0;
        _numberOfRequestedUpdates++;
      }
      if (control == btnPage200) {
        currentPageNumber = 0x200;
        currentSubPageNumber = 0;
        _numberOfRequestedUpdates++;
      }
      if (control == btnPage300) {
        currentPageNumber = 0x300;
        currentSubPageNumber = 0;
        _numberOfRequestedUpdates++;
      }
      if (control == btnHidden) {
        if (btnHidden != null) {
          _hiddenMode = btnHidden.Selected;
          _renderer.HiddenMode = btnHidden.Selected;
          _numberOfRequestedUpdates++;
        }
      }
      if (control == btnSubPage) {
        if (btnSubPage != null) {
          currentSubPageNumber = btnSubPage.SelectedItem;
          _numberOfRequestedUpdates++;
        }
      }
      if (control == btnFullscreen) {
        // First the fullscreen video and then switch to the new window. Otherwise the teletext isn't displayed
        GUIGraphicsContext.IsFullScreenVideo = true;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
      }
      base.OnClicked(controlId, control, actionType);
    }
    #endregion

    #region Rendering method
    public override void Render(float timePassed) {
      base.Render(timePassed);
    }
    #endregion

  }// class
}// namespace
