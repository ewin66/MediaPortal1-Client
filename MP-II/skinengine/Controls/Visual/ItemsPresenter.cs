#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion
using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Core.Properties;
using SkinEngine.Controls.Visuals.Styles;
using MediaPortal.Core.InputManager;

using SkinEngine;
using SkinEngine.Controls.Panels;
using SkinEngine.Controls.Bindings;
using SkinEngine.Controls.Visuals.Styles;

namespace SkinEngine.Controls.Visuals
{
  /// <summary>
  /// Used within the template of an item control to specify the place in the controlís visual tree 
  /// where the ItemsPanel defined by the ItemsControl is to be added.
  /// http://msdn2.microsoft.com/en-us/library/system.windows.controls.itemspresenter.aspx
  /// </summary>
  public class ItemsPresenter : Control
  {
    public ItemsPresenter()
    {
    }
    public ItemsPresenter(ItemsPresenter p)
      :base(p)
    {
    }
    public override object Clone()
    {
      return new ItemsPresenter(this);
    }

    public void ApplyTemplate(FrameworkTemplate template)
    {
      ControlTemplate ct = new ControlTemplate();
      ct.AddChild(template.LoadContent());
      this.Template = ct;
    }
  }
}
