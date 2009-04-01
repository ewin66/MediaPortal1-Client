using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Text;

using ICSharpCode.SharpZipLib.Zip;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;



namespace MediaPortal.MPInstaller
{
  public class MPInstallerScript : IMPInstallerScript
  {
    public MPInstallerScript()
    {
      EnableWizard = true;
    }

    private MPpackageStruct currentPackage;

    public MPpackageStruct CurrentPackage
    {
      get { return currentPackage; }
      set { currentPackage = value; }
    }

    private bool enableWizard;

    public bool EnableWizard
    {
      get { return enableWizard; }
      set { enableWizard = value; }
    }

    /// <summary>
    /// Execute when the package is downloaded via GUI 
    /// </summary>
    virtual public void GUI_GetOptions()
    {
      if (CurrentPackage.InstallerInfo.SetupGroups.Count > 1)
      {
        if (CurrentPackage.InstallerInfo.ProjectProperties.SingleGroupSelect)
        {

          GUIDialogMenu dlgselect = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlgselect == null) return;
          dlgselect.Reset();
          dlgselect.SetHeading(14011); // Sort options
          foreach (GroupString gs in CurrentPackage.InstallerInfo.SetupGroups)
          {
            gs.Checked = false;
            dlgselect.Add(gs.Name);
          }
          dlgselect.DoModal(GUIWindowManager.ActiveWindow);
          if (dlgselect.SelectedLabel != -1)
            CurrentPackage.InstallerInfo.SetupGroups[dlgselect.SelectedLabel].Checked = true;
        }
        else
        {
          foreach (GroupString gs in CurrentPackage.InstallerInfo.SetupGroups)
          {
            GUIDialogYesNo dlgselect = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            dlgselect.SetHeading(14011); // Sort options
            dlgselect.SetLine(1, gs.Name);
            dlgselect.DoModal(GUIWindowManager.ActiveWindow);
            if (dlgselect.IsConfirmed)
              gs.Checked = true;
          }
        }
      }
    }

    /// <summary>
    /// Test if version is compatible and show warning
    /// This use when installing via GUI
    /// </summary>
    /// <returns></returns>
    virtual public bool GUI_Warning()
    {
      if (!TestVersion(CurrentPackage))
      {
        GUIDialogYesNo dlgcontinue = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        dlgcontinue.Reset();
        dlgcontinue.SetHeading(14011); // Sort options
        dlgcontinue.SetLine(1, string.Format(GUILocalizeStrings.Get(14012), CurrentPackage.InstallerInfo.ProjectProperties.MPMinVersion));
        dlgcontinue.SetLine(1, 14013);
        dlgcontinue.DoModal(GUIWindowManager.ActiveWindow);
        if (!dlgcontinue.IsConfirmed)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Test if version is compatible and show warning
    /// </summary>
    /// <returns></returns>
    virtual public bool Warning()
    {
      if (!TestVersion(CurrentPackage))
      {
        if (MessageBox.Show(string.Format("Not supported Mediaportal version !(Needed version {0}) Do you want continue ?", CurrentPackage.InstallerInfo.ProjectProperties.MPMinVersion), "", MessageBoxButtons.YesNo) == DialogResult.No)
          return false;
      }
      return true;
    }
    /// <summary>
    /// Inits this instance.
    /// executed when the package it is loaded
    /// </summary>
    virtual public void Init()
    {
    
    }

    /// <summary>
    /// Called when the install wizard start.
    /// </summary>
    virtual public void OnInstallStart()
    {
    }

    /// <summary>
    /// Installs the current package.
    /// </summary>
    /// <param name="pb">ProgressBar for overall progress (can bee null) </param>
    /// <param name="pb1">ProgressBar for current copied file (can bee null)</param>
    /// <param name="listbox">Listbox for file listing(can bee null) </param>
    virtual public void Install(ProgressBar pb, ProgressBar pb1, ListBox listbox)
    {
      CurrentPackage.InstallPackage(pb, pb1, listbox);
      CurrentPackage.installLanguage(listbox);
    }


    /// <summary>
    /// Called when [install file procesed].
    /// </summary>
    /// <param name="mpiFileInfo">The mpi file info.</param>
    virtual public void OnInstallFileProcesed(MPIFileList mpiFileInfo)
    {

    }

    /// <summary>
    /// Called when [install done].
    /// </summary>
    virtual public void OnInstallDone()
    {

    }


    /// <summary>
    /// Executed only if EnableWizard is false
    /// </summary>
    /// <returns>True if unistall done</returns>
    virtual public bool UnInstall()
    {
      return true;
    }

    #region Generic methods

    public bool ExtractFile(string FileName, string OutFile)
    {
      try
      {
        FastZip zipobj = new FastZip();
        string intenalFileName = MPinstallerStruct.GetZipEntry(this.CurrentPackage.InstallerInfo.FindFile(FileName));
        if (File.Exists(Path.Combine(Path.GetDirectoryName(OutFile), Path.GetFileName(OutFile))))
        {
          File.Delete(Path.Combine(Path.GetDirectoryName(OutFile), Path.GetFileName(OutFile)));
        }
        if (File.Exists(Path.Combine(Path.GetDirectoryName(OutFile), intenalFileName)))
        {
          File.Delete(Path.Combine(Path.GetDirectoryName(OutFile), intenalFileName));
        }
        zipobj.ExtractZip(this.CurrentPackage.FileName, Path.GetDirectoryName(OutFile), intenalFileName);
        File.Move(Path.Combine(Path.GetDirectoryName(OutFile), intenalFileName), Path.Combine(Path.GetDirectoryName(OutFile), Path.GetFileName(OutFile)));
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace);
        return false;
      }
      return true;
    }
    #endregion


    #region helper func's

    /// <summary>
    /// Tests the version  of package if compatible with current version of MP .
    /// </summary>
    /// <param name="package">The package.</param>
    /// <returns></returns>
    private bool TestVersion(MPpackageStruct package)
    {
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      if (VersionPharser.CompareVersions(versionInfo.FileVersion, package.InstallerInfo.ProjectProperties.MPMinVersion) < 0)
      {
        return false;
      }
      return true;
    }

#endregion
  }
}
