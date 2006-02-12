rem Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

cd
if not exist plugins mkdir plugins
if not exist plugins\windows mkdir plugins\windows
if not exist plugins\TagReaders mkdir plugins\TagReaders
if not exist plugins\subtitle mkdir plugins\subtitle
if not exist plugins\ExternalPlayers mkdir plugins\ExternalPlayers
if not exist plugins\process mkdir plugins\process
if not exist Wizards mkdir Wizards

del /F /Q plugins\windows\*.*
del /F /Q plugins\tagreaders\*.*
del /F /Q plugins\subtitle\*.*
del /F /Q plugins\ExternalPlayers\*.*
del /F /Q plugins\process\*.*
del *.dll
del *.ax

if exist ..\..\..\lame_enc.dll copy ..\..\..\lame_enc.dll .
copy ..\..\..\MPSA.ax .
copy ..\..\..\TSFileSource.ax .
copy ..\..\..\MPTSWriter.ax .
regsvr32 /s MPSA.ax
regsvr32 /s TSFileSource.ax
regsvr32 /s MPTSWriter.ax

copy ..\..\..\MediaPortal.Support\bin\debug\MediaPortal.Support.dll .
copy ..\..\..\MediaPortal.Support\bin\debug\MediaPortal.Support.pdb .
copy ..\..\..\RemotePlugins\HCWHelper\HCWHelper\bin\debug\HCWHelper.exe .
copy ..\..\..\RemotePlugins\HCWHelper\HCWHelper\bin\debug\HCWHelper.pdb .
copy ..\..\..\RemotePlugins\X10Remote\AxInterop.X10.dll .
copy ..\..\..\RemotePlugins\X10Remote\Interop.X10.dll .
copy ..\..\..\RemotePlugins\IrTrans\IRTrans.NET.dll .
copy ..\..\..\core\directshowhelper\directshowhelper\release\dshowhelper.dll .
copy ..\..\..\core\fontengine\fontengine\debug\fontengine.dll .
if exist ..\..\..\core\fontengine\fontengine\debug\fontengine.pdb copy ..\..\..\core\fontengine\fontengine\debug\fontengine.pdb .
rem copy ..\..\..\Interop.DirectShowHelperLib.dll .
copy ..\..\..\mfc71.dll .
copy ..\..\..\msvcp71.dll .
copy ..\..\..\msvcr71.dll .
copy ..\..\..\AxInterop.MOZILLACONTROLLib.dll .
copy ..\..\..\Interop.MOZILLACONTROLLib.dll .
copy ..\..\..\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\d3dx9_26.dll .
copy ..\..\..\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\Microsoft.DirectX.dll .
copy ..\..\..\Microsoft.DirectX.DirectInput.dll .
rem ExternalDisplay plugin LCD driver DLLs
copy ..\..\..\FTD2XX.DLL .
copy ..\..\..\SG_VFD.dll .
copy ..\..\..\inpout32.dll .
if not exist LUI\. mkdir LUI
copy ..\..\..\LUI.dll LUI\.
copy ..\..\..\Communications.dll .
copy ..\..\..\Interop.GIRDERLib.dll .
copy ..\..\..\MediaPadLayer.dll .
rem 
copy ..\..\..\KCS.Utilities.dll .
rem copy ..\..\..\X10Plugin.* .
copy ..\..\..\X10Unified.* .
copy ..\..\..\xAPMessage.dll .
copy ..\..\..\xAPTransport.dll .
copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\debug\Configuration.exe .
copy ..\..\..\Configuration\bin\debug\Configuration.exe.config .
copy ..\..\..\Configuration\bin\debug\Configuration.pdb .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.exe .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.pdb .
copy ..\..\..\mbm5.dll .
copy ..\..\..\madlldlib.dll .
copy ..\..\..\ECP2Assembly.dll .
copy ..\..\..\edtftpnet-1.2.2.dll .
copy ..\..\..\dvblib.dll .
rem copy ..\..\..\*.tpl .
copy ..\..\..\Interop.WMEncoderLib.dll .
copy ..\..\..\Interop.TunerLib.dll .
copy ..\..\..\Interop.iTunesLib.dll .
copy ..\..\..\Microsoft.Office.Interop.Outlook.dll .
copy ..\..\..\XPBurnComponent.dll .

copy ..\..\..\core\bin\debug\DirectShowLib.dll .
copy ..\..\..\core\bin\debug\DirectShowLib.pdb .
copy ..\..\..\core\bin\debug\Core.dll .
copy ..\..\..\core\bin\debug\Core.pdb .
copy ..\..\..\tvcapture\bin\debug\tvcapture.dll .
copy ..\..\..\tvcapture\bin\debug\tvcapture.pdb .
copy ..\..\..\databases\bin\debug\databases.dll .
copy ..\..\..\databases\bin\debug\databases.pdb .
copy ..\..\..\SubtitlePlugins\bin\debug\SubtitlePlugins.dll plugins\subtitle
copy ..\..\..\SubtitlePlugins\bin\debug\SubtitlePlugins.pdb plugins\subtitle
copy ..\..\..\TagReaderPlugins\bin\debug\TagReaderPlugins.dll plugins\TagReaders
copy ..\..\..\TagReaderPlugins\bin\debug\TagReaderPlugins.pdb plugins\TagReaders
copy ..\..\..\ExternalPlayers\bin\debug\ExternalPlayers.dll plugins\ExternalPlayers
copy ..\..\..\ExternalPlayers\bin\debug\ExternalPlayers.pdb plugins\ExternalPlayers
copy ..\..\..\WindowPlugins\bin\debug\WindowPlugins.dll plugins\Windows
copy ..\..\..\WindowPlugins\bin\debug\WindowPlugins.pdb plugins\Windows
copy ..\..\..\XihSolutions.DotMSN.dll plugins\Windows
copy ..\..\..\Dialogs\bin\debug\Dialogs.dll plugins\Windows
copy ..\..\..\Dialogs\bin\debug\Dialogs.pdb plugins\Windows
copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.dll plugins\process\
copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.pdb  plugins\process\
copy ..\..\..\RemotePlugins\bin\debug\RemotePlugins.dll .


copy ..\..\..\sqlite.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\AxInterop.WMPLib.dll .
copy ..\..\..\Interop.WMPLib.dll .

copy ..\..\..\FireDTVKeyMap.XML .
copy ..\..\..\FireDTVKeyMap.XML.Schema .


copy ..\..\..\WebEPG\WebEPG\bin\debug\WebEPG.dll .
copy ..\..\..\Utils\bin\debug\Utils.dll .

copy ..\..\..\WebEPG\WebEPG-xmltv\bin\debug\WebEPG-xmltv.exe WebEPG.exe
copy ..\..\..\WebEPG\WebEPG-conf\bin\debug\WebEPG-conf.exe .
copy ..\..\..\WebEPG\WebEPG-channels\bin\debug\WebEPG-channels.exe .
