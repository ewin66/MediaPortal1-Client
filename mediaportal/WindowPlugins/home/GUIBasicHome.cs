#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Drawing;
using MediaPortal.GUI.Library;

namespace WindowPlugins.home
{
  /// <summary>
  /// Summary description for GUIBasicHome.
  /// </summary>
  public class GUIBasicHome : GUIInternalWindow
  {
    [SkinControl(99)] protected GUIVideoControl _videoWindow = null;

    public GUIBasicHome()
    {
      GetID = (int)Window.WINDOW_SECOND_HOME;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\BasicHome.xml"));
    }

    protected override void OnPageLoad()
    {
      GUIControl ctl = GetControl(GetFocusControlId());

      if (ctl != null)
      {
        ctl.Focus = false;
        ctl.Focus = true; // this will update the skin property #highlightedbutton
      }
      base.OnPageLoad();

      //set video window position
      if (_videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition,
                                                       _videoWindow.Width, _videoWindow.Height);
      }
    }
  }
}