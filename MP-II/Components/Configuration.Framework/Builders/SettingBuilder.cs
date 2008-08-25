﻿
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Core.PluginManager;
using MediaPortal.Presentation.Localisation;
using MediaPortal.Configuration;


namespace Components.Configuration.Builders
{
  public class SettingBuilder : IPluginItemBuilder
  {
    #region IPluginBuilder methods

    public object BuildItem(IPluginRegisteredItem item)
    {
      ConfigBase setting;

      try
      {
        if (item.Contains("class"))
          setting = (ConfigBase)item.Plugin.CreateObject(item["class"]);
        else
          setting = new ConfigBase();
      }
      catch (Exception e)
      {
        ServiceScope.Get<ILogger>().Warn("Can't create instance for {0}  [Exception: {1}]", item["class"], e.Message);
        setting = new ConfigBase();
      }
      // All .plugin files should only contain english characters.
      setting.Id = item.Id.ToLower(new System.Globalization.CultureInfo("en"));

      if (item.Contains("text"))
        setting.Text = new StringId(item["text"]);
      else
        setting.Text = new StringId();

      if (item.Contains("help"))
        setting.Help = new StringId(item["help"]);
      else
        setting.Help = new StringId();

      if (item.Contains("iconsmall"))
        setting.IconSmall = Path.Combine(item.Plugin.PluginPath.ToString(), item["iconsmall"]).ToString();

      if (item.Contains("iconlarge"))
        setting.IconLarge = Path.Combine(item.Plugin.PluginPath.ToString(), item["iconlarge"]).ToString();

      int width = -1;
      if (item.Contains("width")) Int32.TryParse(item["width"], out width);
      setting.Width = width;

      int height = -1;
      if (item.Contains("height")) Int32.TryParse(item["height"], out height);
      setting.Height = height;

      if (item.Contains("type"))
      {
        try
        {
          setting.Type = (SettingType)Enum.Parse(typeof(SettingType), item["type"], true);
        }
        catch (Exception)
        {
          setting.Type = SettingType.Unknown;
        }
      }
      else
      {
        setting.Type = SettingType.Unknown;
      }

      if (item.Contains("listento"))
        setting.ListenItems = new List<string>(item["listento"].Replace(" ", "").Split(new char[] { '[', ']', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
      else
        setting.ListenItems = new List<string>(0);

      return setting;
    }

    #endregion
  }
}
