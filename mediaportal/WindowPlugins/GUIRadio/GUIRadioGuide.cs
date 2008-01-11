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

#region usings
using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Radio.Database;
#endregion


namespace MediaPortal.GUI.Radio
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIRadioGuide : GUIRadioGuideBase
  {

    public GUIRadioGuide() : base()
    {
      GetID = (int)GUIWindow.Window.WINDOW_RADIO_GUIDE;
    }
    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\myradioguide.xml");
      Initialize();
      return result;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();    }

    protected override void OnPageDestroy(int newWindowId)
    {

      base.OnPageDestroy(newWindowId);
    }
  }
}
