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

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Lastest NSIS version from http://nsis.sourceforge.net/Download
#       2. The xml-plugin from http://nsis.sourceforge.net/XML_plug-in
#
#**********************************************************************************************************#
Name "MediaPortal"
SetCompressor /SOLID lzma

!ifdef SVN_BUILD
  !define MEDIAPORTAL.BASE "E:\compile\compare_mp1_test"
  !define MEDIAPORTAL.FILTERBIN "..\..\DirectShowFilters\bin\Release"
  !define MEDIAPORTAL.XBMCBIN "..\xbmc\bin\Release"
!else
  !define MEDIAPORTAL.BASE "..\MediaPortal.Base"
  !define MEDIAPORTAL.FILTERBIN "..\..\DirectShowFilters\bin\Release"
  !define MEDIAPORTAL.XBMCBIN "..\xbmc\bin\Release"
!endif
!define BUILD_TYPE "Release"
;!define BUILD_TYPE "Debug"

#!define INSTALL_LOG_FILE "$DESKTOP\install_$(^Name).log"

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup  ; Holds the Startmenu\Programs folder
; variables for commandline parameters for Installer
Var noGabest
Var noDesktopSC
Var noStartMenuSC
; variables for commandline parameters for UnInstaller
Var RemoveAll       ; Set, when the user decided to uninstall everything

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define WEB_REQUIREMENTS "http://wiki.team-mediaportal.com/MediaPortalRequirements"


!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\MediaPortal"

!define VER_MAJOR       0
!define VER_MINOR       9
!define VER_REVISION    1
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif
!if ${VER_BUILD} == 0       # it's a stable release
    !define VERSION "1.0 RC2"
!else                       # it's an svn re�ease
    !define VERSION "1.0 RC1 SVN build ${VER_BUILD} for TESTING ONLY"
!endif
BrandingText "$(^Name) ${VERSION} by ${COMPANY}"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh
!include Library.nsh
!include FileFunc.nsh
!include WinVer.nsh
!include Memento.nsh

!include setup-AddRemovePage.nsh
!include setup-CommonMPMacros.nsh
!include setup-languages.nsh

; FileFunc macros
!insertmacro GetParameters
!insertmacro GetOptions
!insertmacro un.GetParameters
!insertmacro un.GetOptions
!insertmacro GetParent
!insertmacro RefreshShellIcons
!insertmacro un.RefreshShellIcons

#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "images\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!if ${VER_BUILD} == 0       # it's a stable release
    !define MUI_HEADERIMAGE_BITMAP          "images\header.bmp"
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "images\wizard.bmp"
!else                       # it's an svn re�ease
    !define MUI_HEADERIMAGE_BITMAP          "images\header-svn.bmp"
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard-svn.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "images\wizard-svn.bmp"
!endif
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\MediaPortal"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$MPdir.Base\Configuration.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal Configuration"
#!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt
#!define MUI_FINISHPAGE_SHOWREADME_TEXT "View Readme"
#!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_LINK "Donate to MediaPortal"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.team-mediaportal.com/donate.html"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME

!ifndef SVN_BUILD
Page custom PageReinstall PageLeaveReinstall
!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
!endif

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup

!define MUI_PAGE_CUSTOMFUNCTION_PRE InstFilePre
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
; UnInstaller Interface
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE un.WelcomeLeave
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro MUI_LANGUAGE English

#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
!if ${VER_BUILD} == 0
  OutFile "Release\package-mediaportal.exe"
!else
  OutFile "Release\MediaPortal-svn-.exe"
!endif
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal"
InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName       "${NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion    "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName       "${COMPANY}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite    "${URL}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion       "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription   ""
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright    ""
ShowUninstDetails show

#---------------------------------------------------------------------------
# USEFUL MACROS
#--------------------------------------------------------------------------- 
!macro SectionList MacroName
    ; This macro used to perform operation on multiple sections.
    ; List all of your components in following manner here.
    !insertmacro "${MacroName}" "SecGabest"
!macroend

#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
!ifdef SVN_BUILD     # optional Section which could create a backup
Section "Backup current installation status" SecBackup

  !insertmacro GET_BACKUP_POSTFIX $R0

  DetailPrint "Creating backup of installation dir, this might take some minutes."
  CreateDirectory "$MPdir.Base_$R0"
  CopyFiles /SILENT "$MPdir.Base\*.*" "$MPdir.Base_$R0"

  DetailPrint "Creating backup of configuration dir, this might take some minutes."
  CreateDirectory "$MPdir.Config_$R0"
  CopyFiles /SILENT "$MPdir.Config\*.*" "$MPdir.Config_$R0"

SectionEnd
!endif

Section "MediaPortal core files (required)" SecCore
  SectionIn RO
  DetailPrint "Installing MediaPortal core files..."

  DetailPrint "Terminating processes ..."
  ${KILLPROCESS} "MediaPortal.exe"
  ${KILLPROCESS} "configuration.exe"

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "MPTestTool2.exe"
  ${KILLPROCESS} "MusicShareWatcher.exe"
  ${KILLPROCESS} "TVGuideScheduler.exe"
  ${KILLPROCESS} "WebEPG.exe"
  ${KILLPROCESS} "WebEPG-conf.exe"

  SetOverwrite on

  #filters are installed seperatly and are always include in SVN and FINAL releases
  !define EXCLUDED_FILTERS "\
    /x cdxareader.ax \
    /x CLDump.ax \
    /x MPSA.ax \
    /x PDMpgMux.ax \
    /x shoutcastsource.ax \
    /x TsReader.ax \
    /x TTPremiumSource.ax \
    /x MpaDecFilter.ax \
    /x Mpeg2DecFilter.ax \
    "

  #CONFIG FILES ARE ALWAYS INSTALLED by SVN and FINAL releases, BECAUSE of the config dir location
  !define EXCLUDED_CONFIG_FILES "\
    /x CaptureCardDefinitions.xml \
    /x 'eHome Infrared Transceiver List XP.xml' \
    /x ISDNCodes.xml \
    /x keymap.xml \
    /x MusicVideoSettings.xml \
    /x wikipedia.xml \
    /x yac-area-codes.xml \
    "

  # Files which were diffed before including in installer
  # means all of them are in full installer, but only the changed and new ones are in svn installer 
  #We can not use the complete mediaportal.base dir recoursivly , because the plugins, thumbs, weather need to be extracted to their special MPdir location
  # exluding only the folders does not work because /x plugins won't extract the \plugins AND musicplayer\plugins directory
  SetOutPath "$MPdir.Base"
  File /nonfatal /x .svn ${EXCLUDED_FILTERS} ${EXCLUDED_CONFIG_FILES}  "${MEDIAPORTAL.BASE}\*"
  SetOutPath "$MPdir.Base\MusicPlayer"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\MusicPlayer\*"
  SetOutPath "$MPdir.Base\osdskin-media"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\osdskin-media\*"
  SetOutPath "$MPdir.Base\Profiles"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Profiles\*"
  SetOutPath "$MPdir.Base\scripts"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\scripts\*"
  SetOutPath "$MPdir.Base\TTPremiumBoot"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\TTPremiumBoot\*"
  SetOutPath "$MPdir.Base\Tuningparameters"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Tuningparameters\*"
  SetOutPath "$MPdir.Base\WebEPG"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\WebEPG\*"
  SetOutPath "$MPdir.Base\Wizards"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Wizards\*"
  SetOutPath "$MPdir.Base\xmltv"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\xmltv\*"
  ; Doc
  SetOutPath "$MPdir.Base\Docs"
  File "..\Docs\BASS License.txt"
  File "..\Docs\MediaPortal License.rtf"
  #File "..\Docs\LICENSE.rtf"
  #File "..\Docs\SQLite Database Browser.exe"

  # COMMON CONFIG files for SVN and FINAL RELEASES
  SetOutPath "$MPdir.Config"
  File /nonfatal "${MEDIAPORTAL.BASE}\CaptureCardDefinitions.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\eHome Infrared Transceiver List XP.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\ISDNCodes.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\keymap.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\MusicVideoSettings.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\wikipedia.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\yac-area-codes.xml"

  SetOutPath "$MPdir.Database"  
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\database\*"
  SetOutPath "$MPdir.CustomInputDefault"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\InputDeviceMappings\defaults\*"
  SetOutPath "$MPdir.Language"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\language\*"
  SetOutPath "$MPdir.Plugins"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\plugins\*"
  SetOutPath "$MPdir.Skin"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\skin\*"
  SetOutPath "$MPdir.Thumbs"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\thumbs\*"
  SetOutPath "$MPdir.Weather"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\weather\*"


  SetOutPath "$MPdir.Base"
!ifdef SVN_BUILD
  SetOverwrite off
  File MediaPortalDirs.xml
  SetOverwrite on
!else
  File MediaPortalDirs.xml
!endif

  ; ========================================
  ; MediaPortalEXE
  ;should be           , but because of postbuild.bat there are too much matching files
  ;File "..\xbmc\bin\Release\${BUILD_TYPE}\MediaPortal.*"
  File "..\xbmc\bin\${BUILD_TYPE}\MediaPortal.exe"
  File "..\xbmc\bin\${BUILD_TYPE}\MediaPortal.exe.config"
  ; Configuration
  File "..\Configuration\bin\${BUILD_TYPE}\Configuration.*"

  ; ========================================
  ; Core
  File "..\core\bin\${BUILD_TYPE}\Core.*"
  File "..\core\bin\${BUILD_TYPE}\DirectShowLib.*"

  #those files are moved to MediaPortal.Base
  #File "..\core\directshowhelper\directshowhelper\Release\dshowhelper.dll"
  #File "..\core\DXUtil\Release\DXUtil.dll"
  #File "..\core\fontengine\fontengine\${BUILD_TYPE}\fontengine.*"

  ; Utils
  File "..\Utils\bin\${BUILD_TYPE}\Utils.dll"
  ; Support
  File "..\MediaPortal.Support\bin\${BUILD_TYPE}\MediaPortal.Support.*"
  ; Databases
  File "..\databases\bin\${BUILD_TYPE}\databases.*"
  ; TvCapture
  File "..\tvcapture\bin\${BUILD_TYPE}\tvcapture.*"
  ; TvGuideScheduler
  File "..\TVGuideScheduler\bin\${BUILD_TYPE}\TVGuideScheduler.*"

  ; ========================================
  ; MusicShareWatcher
  File "..\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\${BUILD_TYPE}\MusicShareWatcherHelper.*"
  File "..\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\${BUILD_TYPE}\MusicShareWatcher.exe"
  ; MPInstaller
  File "..\MPInstaller\bin\${BUILD_TYPE}\MPInstaller.*"
  File "..\MPInstaller\bin\${BUILD_TYPE}\MPInstaller.Library.*"
  ; MPTestTool2
  File "..\MPTestTool2\bin\${BUILD_TYPE}\MPTestTool2.exe"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\DaggerLib.dll"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\DaggerLib.DSGraphEdit.dll"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\DirectShowLib-2005.dll"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\MediaFoundation.dll"
  ; WebEPG
  File "..\WebEPG\WebEPG\bin\${BUILD_TYPE}\WebEPG.dll"
  File /oname=WebEPG.exe "..\WebEPG\WebEPG-xmltv\bin\${BUILD_TYPE}\WebEPG-xmltv.exe"
  File "..\WebEPG\WebEPG-conf\bin\${BUILD_TYPE}\WebEPG-conf.exe"

  ; ========================================
  ; Plugins
  File "..\RemotePlugins\bin\${BUILD_TYPE}\RemotePlugins.*"
  File "..\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\${BUILD_TYPE}\HCWHelper.*"
  File "..\RemotePlugins\Remotes\X10Remote\Interop.X10.dll"

  SetOutPath "$MPdir.Plugins\ExternalPlayers"
  File "..\ExternalPlayers\bin\${BUILD_TYPE}\ExternalPlayers.*"
  SetOutPath "$MPdir.Plugins\process"
  File "..\ProcessPlugins\bin\${BUILD_TYPE}\ProcessPlugins.*"
  SetOutPath "$MPdir.Plugins\subtitle"
  File "..\SubtitlePlugins\bin\${BUILD_TYPE}\SubtitlePlugins.*"
  SetOutPath "$MPdir.Plugins\Windows"
  File "..\Dialogs\bin\${BUILD_TYPE}\Dialogs.*"
  File "..\WindowPlugins\bin\${BUILD_TYPE}\WindowPlugins.*"

  ; MyBurner plugin dependencies
  SetOutPath "$MPdir.Base"
  File "..\XPImapiBurner\bin\${BUILD_TYPE}\XPBurnComponent.dll"

  ; ========================================
  ; Wizards
  SetOutPath "$MPdir.Base\Wizards"
  File "..\Configuration\Wizards\*.*"

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  SetOutPath "$MPdir.Base"
  ;filter used for SVCD and VCD playback
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\cdxareader.ax"       "$MPdir.Base\cdxareader.ax" "$MPdir.Base"
  ##### MAYBE used by VideoEditor
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\CLDump.ax"           "$MPdir.Base\CLDump.ax" "$MPdir.Base"
  ; used for scanning in tve2
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MPSA.ax"             "$MPdir.Base\MPSA.ax" "$MPdir.Base"
  ;filter for analog tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\PDMpgMux.ax"         "$MPdir.Base\PDMpgMux.ax" "$MPdir.Base"
  ; used for shoutcast
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\shoutcastsource.ax"  "$MPdir.Base\shoutcastsource.ax" "$MPdir.Base"
  ; used for digital tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TsReader.ax"         "$MPdir.Base\TsReader.ax" "$MPdir.Base"
  ##### not sure for what this is used
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TTPremiumSource.ax"  "$MPdir.Base\TTPremiumSource.ax" "$MPdir.Base"
SectionEnd
!macro Remove_${SecCore}
  DetailPrint "Uninstalling MediaPortal core files..."

  DetailPrint "Terminating processes ..."
  ${KILLPROCESS} "MediaPortal.exe"
  ${KILLPROCESS} "configuration.exe"

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "MPTestTool2.exe"
  ${KILLPROCESS} "MusicShareWatcher.exe"
  ${KILLPROCESS} "TVGuideScheduler.exe"
  ${KILLPROCESS} "WebEPG.exe"
  ${KILLPROCESS} "WebEPG-conf.exe"

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ;filter used for SVCD and VCD playback
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\cdxareader.ax"
  ##### MAYBE used by VideoEditor
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\CLDump.ax"
  ; used for scanning in tve2
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MPSA.ax"
  ;filter for analog tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\PDMpgMux.ax"
  ; used for shoutcast
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\shoutcastsource.ax"
  ; used for digital tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\TsReader.ax"
  ##### not sure for what this is used
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\TTPremiumSource.ax"

  ; Config Files
  Delete /REBOOTOK "$MPdir.Config\CaptureCardDefinitions.xml"
  Delete /REBOOTOK "$MPdir.Config\eHome Infrared Transceiver List XP.xml"
  Delete /REBOOTOK "$MPdir.Config\ISDNCodes.xml"
  Delete /REBOOTOK "$MPdir.Config\keymap.xml"
  Delete /REBOOTOK "$MPdir.Config\MusicVideoSettings.xml"
  Delete /REBOOTOK "$MPdir.Config\wikipedia.xml"
  Delete /REBOOTOK "$MPdir.Config\yac-area-codes.xml"

  ; Remove the Folders
  RMDir /r /REBOOTOK "$MPdir.Base\MusicPlayer"
  RMDir /r /REBOOTOK "$MPdir.Base\osdskin-media"
  RMDir /r /REBOOTOK "$MPdir.Base\Profiles"
  RMDir /r /REBOOTOK "$MPdir.Base\scripts"
  RMDir /r /REBOOTOK "$MPdir.Base\TTPremiumBoot"
  RMDir /r /REBOOTOK "$MPdir.Base\Tuningparameters"
  RMDir /r /REBOOTOK "$MPdir.Base\WebEPG"
  RMDir /r /REBOOTOK "$MPdir.Base\Wizards"

  RMDir /r /REBOOTOK "$MPdir.BurnerSupport"
  RMDir /r /REBOOTOK "$MPdir.Cache"
  RMDir /r /REBOOTOK "$MPdir.CustomInputDefault"
  RMDir /r /REBOOTOK "$MPdir.Language"
  RMDir /r /REBOOTOK "$MPdir.Weather"

  ; Doc
  Delete /REBOOTOK "$MPdir.Base\Docs\BASS License.txt"
  Delete /REBOOTOK "$MPdir.Base\Docs\MediaPortal License.rtf"
  #Delete /REBOOTOK "$INSTDIR\Docs\LICENSE.rtf"
  #Delete /REBOOTOK "$INSTDIR\Docs\SQLite Database Browser.exe"
  RMDir "$MPdir.Base\Docs"

  ; WebEPG
  RMDir /r /REBOOTOK "$MPdir.Base\WebEPG\channels"
  RMDir /r /REBOOTOK "$MPdir.Base\WebEPG\grabbers"
  RMDir "$MPdir.Base\WebEPG"

  ; xmltv
  Delete /REBOOTOK "$MPdir.Base\xmltv\ReadMe.txt"
  Delete /REBOOTOK "$MPdir.Base\xmltv\xmltv.dtd"
  RMDir "$MPdir.Base\xmltv"

  ; database
  RMDir /r /REBOOTOK "$MPdir.Database\convert"
  RMDir "$MPdir.Database"

  ; plugins
  Delete /REBOOTOK "$MPdir.Plugins\ExternalPlayers\ExternalPlayers.dll"
  RMDir "$MPdir.Plugins\ExternalPlayers"

  RMDir /r /REBOOTOK "$MPdir.Plugins\process\LCDDrivers"
  Delete /REBOOTOK "$MPdir.Plugins\process\ProcessPlugins.dll"
  Delete /REBOOTOK "$MPdir.Plugins\process\PowerSchedulerClientPlugin.dll"
  RMDir "$MPdir.Plugins\process"

  Delete /REBOOTOK "$MPdir.Plugins\subtitle\SubtitlePlugins.dll"
  RMDir "$MPdir.Plugins\subtitle"

  Delete /REBOOTOK "$MPdir.Plugins\Windows\Dialogs.dll"
  Delete /REBOOTOK "$MPdir.Plugins\Windows\WindowPlugins.dll"
  Delete /REBOOTOK "$MPdir.Plugins\Windows\XihSolutions.DotMSN.dll"
  Delete /REBOOTOK "$MPdir.Plugins\Windows\TvPlugin.dll"
  RMDir "$MPdir.Plugins\Windows"

  RMDir "$MPdir.Plugins"

  ; skins
  RMDir /r /REBOOTOK "$MPdir.Skin\BlueTwo"
  RMDir /r /REBOOTOK "$MPdir.Skin\BlueTwo wide"
  RMDir "$MPdir.Skin"

  ; Remove Files in MP Root Directory
  Delete /REBOOTOK "$MPdir.Base\AppStart.exe"
  Delete /REBOOTOK "$MPdir.Base\AppStart.exe.config"
  Delete /REBOOTOK "$MPdir.Base\AxInterop.WMPLib.dll"
  Delete /REBOOTOK "$MPdir.Base\BallonRadio.ico"
  Delete /REBOOTOK "$MPdir.Base\bass.dll"
  Delete /REBOOTOK "$MPdir.Base\Bass.Net.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_fx.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_vis.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_vst.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_wadsp.dll"
  Delete /REBOOTOK "$MPdir.Base\bassasio.dll"
  Delete /REBOOTOK "$MPdir.Base\bassmix.dll"
  Delete /REBOOTOK "$MPdir.Base\BassRegistration.dll"
  Delete /REBOOTOK "$MPdir.Base\Configuration.exe"
  Delete /REBOOTOK "$MPdir.Base\Configuration.exe.config"
  Delete /REBOOTOK "$MPdir.Base\Core.dll"
  Delete /REBOOTOK "$MPdir.Base\CSScriptLibrary.dll"
  Delete /REBOOTOK "$MPdir.Base\d3dx9_30.dll"
  Delete /REBOOTOK "$MPdir.Base\DaggerLib.dll"
  Delete /REBOOTOK "$MPdir.Base\DaggerLib.DSGraphEdit.dll"
  Delete /REBOOTOK "$MPdir.Base\Databases.dll"
  Delete /REBOOTOK "$MPdir.Base\defaultMusicViews.xml"
  Delete /REBOOTOK "$MPdir.Base\defaultVideoViews.xml"
  Delete /REBOOTOK "$MPdir.Base\DirectShowLib-2005.dll"
  Delete /REBOOTOK "$MPdir.Base\DirectShowLib.dll"
  Delete /REBOOTOK "$MPdir.Base\dlportio.dll"
  Delete /REBOOTOK "$MPdir.Base\dshowhelper.dll"
  Delete /REBOOTOK "$MPdir.Base\dvblib.dll"
  Delete /REBOOTOK "$MPdir.Base\dxerr9.dll"
  Delete /REBOOTOK "$MPdir.Base\DXUtil.dll"
  Delete /REBOOTOK "$MPdir.Base\edtftpnet-1.2.2.dll"
  Delete /REBOOTOK "$MPdir.Base\FastBitmap.dll"
  Delete /REBOOTOK "$MPdir.Base\fontEngine.dll"
  Delete /REBOOTOK "$MPdir.Base\FTD2XX.DLL"
  Delete /REBOOTOK "$MPdir.Base\hauppauge.dll"
  Delete /REBOOTOK "$MPdir.Base\HcwHelper.exe"
  Delete /REBOOTOK "$MPdir.Base\HelpReferences.xml"
  Delete /REBOOTOK "$MPdir.Base\ICSharpCode.SharpZipLib.dll"
  Delete /REBOOTOK "$MPdir.Base\inpout32.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.GIRDERLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.iTunesLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.TunerLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.WMEncoderLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.WMPLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.X10.dll"
  Delete /REBOOTOK "$MPdir.Base\KCS.Utilities.dll"
  Delete /REBOOTOK "$MPdir.Base\lame_enc.dll"
  Delete /REBOOTOK "$MPdir.Base\LibDriverCoreClient.dll"
  Delete /REBOOTOK "$MPdir.Base\log4net.dll"
  Delete /REBOOTOK "$MPdir.Base\madlldlib.dll"
  Delete /REBOOTOK "$MPdir.Base\MediaFoundation.dll"
  Delete /REBOOTOK "$MPdir.Base\MediaPadLayer.dll"
  Delete /REBOOTOK "$MPdir.Base\MediaPortalDirs.xml"
  Delete /REBOOTOK "$MPdir.Base\MediaPortal.exe"
  Delete /REBOOTOK "$MPdir.Base\MediaPortal.exe.config"
  Delete /REBOOTOK "$MPdir.Base\MediaPortal.Support.dll"
  Delete /REBOOTOK "$MPdir.Base\menu.bin"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ApplicationUpdater.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ApplicationUpdater.Interfaces.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ExceptionManagement.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ExceptionManagement.Interfaces.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.Direct3D.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.Direct3DX.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.DirectDraw.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.DirectInput.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.Office.Interop.Outlook.dll"
  Delete /REBOOTOK "$MPdir.Base\MPInstaller.exe"
  Delete /REBOOTOK "$MPdir.Base\MPInstaller.Library.dll"
  Delete /REBOOTOK "$MPdir.Base\mplogo.gif"
  Delete /REBOOTOK "$MPdir.Base\MPTestTool2.exe"
  Delete /REBOOTOK "$MPdir.Base\mpviz.dll"
  Delete /REBOOTOK "$MPdir.Base\MusicShareWatcher.exe"
  Delete /REBOOTOK "$MPdir.Base\MusicShareWatcherHelper.dll"
  Delete /REBOOTOK "$MPdir.Base\RemotePlugins.dll"
  Delete /REBOOTOK "$MPdir.Base\restart.vbs"
  Delete /REBOOTOK "$MPdir.Base\SG_VFD.dll"
  Delete /REBOOTOK "$MPdir.Base\SG_VFDv5.dll"
  Delete /REBOOTOK "$MPdir.Base\sqlite.dll"
  Delete /REBOOTOK "$MPdir.Base\taglib-sharp.dll"
  Delete /REBOOTOK "$MPdir.Base\TaskScheduler.dll"
  Delete /REBOOTOK "$MPdir.Base\ttBdaDrvApi_Dll.dll"
  Delete /REBOOTOK "$MPdir.Base\ttdvbacc.dll"
  Delete /REBOOTOK "$MPdir.Base\TVCapture.dll"
  Delete /REBOOTOK "$MPdir.Base\TVGuideScheduler.exe"
  Delete /REBOOTOK "$MPdir.Base\Utils.dll"
  Delete /REBOOTOK "$MPdir.Base\WebEPG.dll"
  Delete /REBOOTOK "$MPdir.Base\WebEPG.exe"
  Delete /REBOOTOK "$MPdir.Base\WebEPG-conf.exe"
  Delete /REBOOTOK "$MPdir.Base\X10Unified.dll"
  Delete /REBOOTOK "$MPdir.Base\xAPMessage.dll"
  Delete /REBOOTOK "$MPdir.Base\xAPTransport.dll"
  Delete /REBOOTOK "$MPdir.Base\XPBurnComponent.dll"
!macroend

${MementoSection} "Gabest MPA/MPV decoder" SecGabest
  DetailPrint "Installing Gabest MPA/MPV decoder..."

  SetOutPath "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpaDecFilter.ax"   "$MPdir.Base\MpaDecFilter.ax" "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\Mpeg2DecFilter.ax" "$MPdir.Base\Mpeg2DecFilter.ax" "$MPdir.Base"

  ; Write Default Values for Filter into the registry
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AAC Downmix" 1
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3 Dynamic Range" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3 LFE" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3 Speaker Config" 2
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3Decoder" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "Boost" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTS Dynamic Range" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTS LFE" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTS Speaker Config" 2
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTSDecoder" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "Normalize" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "Output Format" 0

  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Brightness" 128
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Contrast" 100
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Deinterlace" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Enable Planar YUV Modes" 1
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Forced Subtitles" 1
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Hue" 180
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Saturation" 100

  ; adjust the merit of this directshow filter
  SetOutPath "$MPdir.Base"
  File "Resources\SetMerit.exe"

  ${LOG_TEXT} "INFO" "set merit for MPA"
  nsExec::ExecToLog '"$MPdir.Base\SetMerit.exe" {3D446B6F-71DE-4437-BE15-8CE47174340F} 00600000'
  ${LOG_TEXT} "INFO" "set merit for MPV"
  nsExec::ExecToLog '"$MPdir.Base\SetMerit.exe" {39F498AF-1A09-4275-B193-673B0BA3D478} 00600000'
${MementoSectionEnd}
!macro Remove_${SecGabest}
  DetailPrint "Uninstalling Gabest MPA/MPV decoder..."

  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MpaDecFilter.ax"
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\Mpeg2DecFilter.ax"

  ; remove the tool to adjust the merit
  Delete /REBOOTOK "$MPdir.Base\SetMerit.exe"
!macroend

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
  DetailPrint "Doing post installation stuff..."

  ;Removes unselected components
  !insertmacro SectionList "FinishSection"

  ;writes component status to registry
  ${MementoSectionSave}

  SetOverwrite on
  SetOutPath "$MPdir.Base"

  ${If} $noDesktopSC != 1
    CreateShortCut "$DESKTOP\MediaPortal.lnk"               "$MPdir.Base\MediaPortal.exe"      "" "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
    CreateShortCut "$DESKTOP\MediaPortal Configuration.lnk" "$MPdir.Base\Configuration.exe"    "" "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
  ${EndIf}

  ${If} $noStartMenuSC != 1
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
      ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
      CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk"                            "$MPdir.Base\MediaPortal.exe"   ""      "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"              "$MPdir.Base\Configuration.exe" ""      "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug-Mode.lnk"                 "$MPdir.Base\MPTestTool2.exe"   "-auto" "$MPdir.Base\MPTestTool2.exe"   0 "" "" "MediaPortal Debug-Mode"
      CreateDirectory "$MPdir.Log"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"                  "$MPdir.Log"                    ""      "$MPdir.Log"                    0 "" "" "MediaPortal Log-Files"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Plugins-Skins Installer.lnk"    "$MPdir.Base\MPInstaller.exe"   ""      "$MPdir.Base\MPInstaller.exe"   0 "" "" "MediaPortal Plugins-Skins Installer"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TestTool.lnk"                   "$MPdir.Base\MPTestTool2.exe"   ""      "$MPdir.Base\MPTestTool2.exe"   0 "" "" "MediaPortal TestTool"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"                  "$MPdir.Base\uninstall-mp.exe"
      WriteINIStr "$SMPROGRAMS\$StartMenuGroup\web site.url" "InternetShortcut" "URL" "${URL}"
    !insertmacro MUI_STARTMENU_WRITE_END
  ${EndIf}

  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

  ; Write Uninstall Information
  WriteRegStr HKLM "${REG_UNINSTALL}" InstallPath        "$MPdir.Base"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "$(^Name)"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${COMPANY}"
  WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${URL}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$MPdir.Base\MediaPortal.exe,0"
  WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$MPdir.Base\uninstall-mp.exe"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1

  WriteUninstaller "$MPdir.Base\uninstall-mp.exe"

  ; Associate .mpi files with MPInstaller
  !define Index "Line${__LINE__}"
  ; backup the association, if it already exsists
  ReadRegStr $1 HKCR ".mpi" ""
  StrCmp $1 "" "${Index}-NoBackup"
  StrCmp $1 "MediaPortal.Installer" "${Index}-NoBackup"
  WriteRegStr HKCR ".mpi" "backup_val" $1

  "${Index}-NoBackup:"
  WriteRegStr HKCR ".mpi" "" "MediaPortal.Installer"
  WriteRegStr HKCR "MediaPortal.Installer" "" "MediaPortal Installer"
  WriteRegStr HKCR "MediaPortal.Installer\shell" "" "open"
  WriteRegStr HKCR "MediaPortal.Installer\DefaultIcon" "" "$MPdir.Base\MPInstaller.exe,0"
  WriteRegStr HKCR "MediaPortal.Installer\shell\open\command" "" '$MPdir.Base\MPInstaller.exe "%1"'

  ${RefreshShellIcons}
  # [OBSOLETE] System::Call 'Shell32::SHChangeNotify(i 0x8000000, i 0, i 0, i 0)'
  !undef Index
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
  ;First removes all optional components
  !insertmacro SectionList "RemoveSection"
  ;now also remove core component
  !insertmacro Remove_${SecCore}

  ; remove registry key
  DeleteRegValue HKLM "${REG_UNINSTALL}" "UninstallString"

  ; remove Start Menu shortcuts
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug-Mode.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Plugins-Skins Installer.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal TestTool.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\web site.url"
  RMDir "$SMPROGRAMS\$StartMenuGroup"

  ; remove Desktop shortcuts
  Delete "$DESKTOP\MediaPortal.lnk"
  Delete "$DESKTOP\MediaPortal Configuration.lnk"

  ; remove last files and instdir
  Delete /REBOOTOK "$MPdir.Base\uninstall-mp.exe"
  RMDir "$MPdir.Base"

  ; do we need to deinstall everything? Then remove also the CommonAppData and InstDir
  ${If} $RemoveAll == 1
    DetailPrint "Removing User Settings"
    DeleteRegKey HKLM "${REG_UNINSTALL}"
    RMDir /r /REBOOTOK "$MPdir.Config"
    RMDir /r /REBOOTOK "$MPdir.Database"
    RMDir /r /REBOOTOK "$MPdir.Plugins"
    RMDir /r /REBOOTOK "$MPdir.Skin"
    RMDir /r /REBOOTOK "$MPdir.Base"
  ${EndIf}

  ; Remove File Association for .mpi files
  !define Index "Line${__LINE__}"
  ReadRegStr $1 HKCR ".mpi" ""
  StrCmp $1 "MediaPortal.Installer" 0 "${Index}-NoOwn" ; only do this if we own it
  ReadRegStr $1 HKCR ".mpi" "backup_val"
  StrCmp $1 "" 0 "${Index}-Restore" ; if backup="" then delete the whole key
  DeleteRegKey HKCR ".mpi"
  Goto "${Index}-NoOwn"

  "${Index}-Restore:"
  WriteRegStr HKCR ".mpi" "" $1
  DeleteRegValue HKCR ".mpi" "backup_val"

  DeleteRegKey HKCR "MediaPortal.Installer" ;Delete key with association settings

  ${un.RefreshShellIcons}

  "${Index}-NoOwn:"
  !undef Index
SectionEnd

#---------------------------------------------------------------------------
# FUNCTIONS
#---------------------------------------------------------------------------
Function .onInit
  ${LOG_OPEN}

  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $noGabest 0
  StrCpy $noDesktopSC 0
  StrCpy $noStartMenuSC 0

  ; gets comandline parameter
  ${GetParameters} $R0

  ClearErrors
  ${GetOptions} $R0 "/noGabest" $R1
  IfErrors +2
  StrCpy $noGabest 1

  ClearErrors
  ${GetOptions} $R0 "/noDesktopSC" $R1
  IfErrors +2
  StrCpy $noDesktopSC 1

  ClearErrors
  ${GetOptions} $R0 "/noStartMenuSC" $R1
  IfErrors +2
  StrCpy $noStartMenuSC 1
  #### END of check and parse cmdline parameter

  ; reads components status for registry
  ${MementoSectionRestore}

  ; update the component status -> commandline parameters have higher priority than registry values
  ${If} $noGabest = 1
    !insertmacro UnselectSection ${SecGabest}
  ${EndIf}

  ; check if old mp 0.2.2 is installed
  ${If} ${MP022IsInstalled}
    MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MP022)"
    Abort
  ${EndIf}

  ; check if old mp 0.2.3 RC3 is installed
  ${If} ${MP023RC3IsInstalled}
    MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MP023RC3)"
    Abort
  ${EndIf}

  ; check if old mp 0.2.3 is installed.
  ${If} ${MP023IsInstalled}
    MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MP023)"
    Abort
  ${EndIf}

  ; check if minimum Windows version is XP
  ${If} ${AtMostWin2000}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ; check if current user is admin
  UserInfo::GetOriginalAccountType
  Pop $0
  #StrCmp $0 "Admin" 0 +3
  ${IfNot} $0 == "Admin"
    MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ADMIN)"
    Abort
  ${EndIf}

  ; check if .Net is installed
  ${IfNot} ${dotNetIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ; check if VC Redist 2005 SP1 is installed
  ${IfNot} ${VCRedistIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_VCREDIST)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ; check if reboot is required
  ${If} ${FileExists} "$MPdir.Base\rebootflag"
    MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
    Abort
  ${EndIf}


  ${If} ${Silent}
    Call InstFilePre
  ${EndIf}

  SetShellVarContext all
FunctionEnd

Function .onInstFailed
  ${LOG_CLOSE}
FunctionEnd

Function .onInstSuccess
  ${LOG_CLOSE}
FunctionEnd

Function un.onInit
  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $RemoveAll 0

  ; gets comandline parameter
  ${un.GetParameters} $R0

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $RemoveAll 1
  #### END of check and parse cmdline parameter

  ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" "InstallPath"
  ${un.ReadMediaPortalDirs} "$INSTDIR"
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

  SetShellVarContext all
FunctionEnd

Function un.onUninstSuccess
  ; write a reboot flag, if reboot is needed, so the installer won't continue until reboot is done
  ${If} ${RebootFlag}
    FileOpen $0 "$MPdir.Base\rebootflag" w
    Delete /REBOOTOK "$MPdir.Base\rebootflag" ; this will not be deleted until the reboot because it is currently opened
    RMDir /REBOOTOK "$MPdir.Base"
    FileClose $0
  ${EndIf}
FunctionEnd

Function InstFilePre
  ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMajor"
  ReadRegDWORD $R2 HKLM "${REG_UNINSTALL}" "VersionMinor"

  ${IfNot} ${MPIsInstalled}
    ${If} $R1 != ${VER_MAJOR}
    ${OrIf} $R2 != ${VER_MINOR}

      !insertmacro GET_BACKUP_POSTFIX $R0

      ${If} ${FileExists} "$MPdir.Base\*.*"
        Rename "$MPdir.Base" "$MPdir.Base_$R0"
      ${EndIf}

      ${If} ${FileExists} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml"
        Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_$R0"
      ${EndIf}

    ${EndIf}
  ${EndIf}

  ${ReadMediaPortalDirs} "$INSTDIR"
FunctionEnd

Function un.WelcomeLeave
    ; This function is called, before the uninstallation process is startet

    ; It asks the user, if he wants to remove all files and settings
    StrCpy $RemoveAll 0
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL)" IDNO +3
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL_STUPID)" IDNO +2
    StrCpy $RemoveAll 1

FunctionEnd

#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGabest}  $(DESC_SecGabest)
!insertmacro MUI_FUNCTION_DESCRIPTION_END