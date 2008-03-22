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
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  class TvPluginServerChecker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Plugin";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();
      DialogResult result = dlg.ShowDialog(Utils.GetDownloadURL("TvServer"), Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string exe = Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadFile("TvServer");
      string parameters = "/noServer";
      Process setup = Process.Start(exe, parameters);
      setup.WaitForExit();
      return true;
    }
    public bool UnInstall()
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal TV Server");
      string exe = (string)key.GetValue("UninstallString");
      key.Close();
      Process setup = Process.Start(exe, "/S /RemoveAll");
      setup.WaitForExit();
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal TV Server");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        int clientInstalled=(int)key.GetValue("MementoSection_SecClient");
        string version = (string)key.GetValue("DisplayVersion");
        key.Close();
        if (clientInstalled == 0)
          result.state = CheckState.NOT_INSTALLED;
        else
          result.state = CheckState.INSTALLED;
        //if (version == "1.0.0")
        //  result.state = CheckState.INSTALLED;
        //else
        //  result.state = CheckState.VERSION_MISMATCH;
      }
      return result;
    }
  }
}
