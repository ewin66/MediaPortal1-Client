using System;
using System.IO;
using System.Management;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices; 
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using DShowNET;
using DShowNET.Device;
using DirectX.Capture;
using MediaPortal.TV.Recording;
using MediaPortal.GUI.Library;
using TVCapture;
using MediaPortal.TV.Database;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for EditCaptureCardForm.
	/// </summary>
	public class EditCaptureCardForm : System.Windows.Forms.Form
	{
		static CaptureCardDefinitions mCaptureCardDefinitions = CaptureCardDefinitions.Instance;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cardComboBox;
		private System.Windows.Forms.CheckBox useRecordingCheckBox;
		private System.Windows.Forms.CheckBox useWatchingCheckBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//
		// Private members
		//
		ArrayList captureFormats = new ArrayList();
		private System.Windows.Forms.ComboBox frameSizeComboBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox frameRateTextBox;
		private System.Windows.Forms.Label label6;
		ArrayList propertyPages = new ArrayList();
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox videoCompressorComboBox;
		private System.Windows.Forms.ComboBox audioCompressorComboBox;
		private System.Windows.Forms.Button setupButton;
		private System.Windows.Forms.ComboBox filterComboBox;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox audioDeviceComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox comboBoxLineInput;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TrackBar trackRecording;
		private System.Windows.Forms.Label lblRecordingLevel;
		private Size m_size = new Size(0,0);
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.TextBox textBoxName;
		
		bool acceptuserinput=false;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.ComboBox comboBox3Audio;
		private System.Windows.Forms.ComboBox comboBox3Video;
		private System.Windows.Forms.ComboBox comboBox2Audio;
		private System.Windows.Forms.ComboBox comboBox2Video;
		private System.Windows.Forms.ComboBox comboBox1Audio;
		private System.Windows.Forms.ComboBox comboBox1Video;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.ComboBox lnbkind1;
		private System.Windows.Forms.ComboBox lnbconfig1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TextBox circularMHZ;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.TextBox cbandMHZ;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox lnb1MHZ;
		private System.Windows.Forms.Label lnb1;
		private System.Windows.Forms.TextBox lnbswMHZ;
		private System.Windows.Forms.Label switchMHZ;
		private System.Windows.Forms.TextBox lnb0MHZ;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Button button18;
		private System.Windows.Forms.Button button17;
		private System.Windows.Forms.ProgressBar progressBar2;
		private System.Windows.Forms.Label chName;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.Button button16;
		private System.Windows.Forms.Button button15;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.TreeView treeView5;
		private System.Windows.Forms.CheckBox checkBoxHiQuality;
		bool m_stopEPGGrab;
		private System.Windows.Forms.NumericUpDown updownPrio;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.ComboBox comboBoxQuality;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox lnbkind4;
		private System.Windows.Forms.ComboBox lnbkind3;
		private System.Windows.Forms.ComboBox lnbkind2;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.ComboBox lnbconfig4;
		private System.Windows.Forms.ComboBox lnbconfig3;
		private System.Windows.Forms.ComboBox lnbconfig2;
		private System.Windows.Forms.ComboBox diseqcd;
		private System.Windows.Forms.ComboBox diseqcc;
		private System.Windows.Forms.ComboBox diseqcb;
		private System.Windows.Forms.ComboBox diseqca;
		private System.Windows.Forms.CheckBox useLNB1;
		private System.Windows.Forms.CheckBox useLNB2;
		private System.Windows.Forms.CheckBox useLNB3;
		private System.Windows.Forms.CheckBox useLNB4;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TabPage tabPage7;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.TextBox tbRecordingFolder;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.TrackBar trackBar1;
		private System.Windows.Forms.CheckBox checkBoxDeleteLow;
		private System.Windows.Forms.Label labelPercent;
		private System.Windows.Forms.Label label33;
		private System.Windows.Forms.Button btnRadio;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Label channelsCount;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.ComboBox cbRgbVideo;
		private System.Windows.Forms.ComboBox cbRgbAudio;
		private System.Windows.Forms.Label addsToDB;
		private System.Windows.Forms.Label label37;
		int  CardId;
		
		/// <summary>
		/// 
		/// </summary>
		public EditCaptureCardForm(int cardId, bool addNewCard,TVCaptureDevice deviceToEdit)
		{
			ArrayList captureCards= new ArrayList();
			if (addNewCard)
			{
				try
				{
					using(FileStream fileStream = new FileStream("capturecards.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{

						SoapFormatter formatter = new SoapFormatter();
						captureCards = (ArrayList)formatter.Deserialize(fileStream);
						for (int i=0; i < captureCards.Count; i++)
						{
							((TVCaptureDevice)captureCards[i]).ID=(i+1);
							((TVCaptureDevice)captureCards[i]).LoadDefinitions();
						}
						//
						// Finally close our file stream
						//
						fileStream.Close();
					}
				}
				catch
				{
				}
			}

			CardId=cardId;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			comboBox1Audio.Items.Add("Audio-in #1");
			comboBox1Audio.Items.Add("Audio-in #2");
			comboBox1Audio.Items.Add("Audio-in #3");

			comboBox2Audio.Items.Add("Audio-in #1");
			comboBox2Audio.Items.Add("Audio-in #2");
			comboBox2Audio.Items.Add("Audio-in #3");
			
			comboBox3Audio.Items.Add("Audio-in #1");
			comboBox3Audio.Items.Add("Audio-in #2");
			comboBox3Audio.Items.Add("Audio-in #3");

			comboBox1Video.Items.Add("CVBS #1");
			comboBox1Video.Items.Add("CVBS #2");
			comboBox1Video.Items.Add("CVBS #3");

			comboBox2Video.Items.Add("CVBS #1");
			comboBox2Video.Items.Add("CVBS #2");
			comboBox2Video.Items.Add("CVBS #3");

			comboBox3Video.Items.Add("SVHS #1");
			comboBox3Video.Items.Add("SVHS #2");
			comboBox3Video.Items.Add("SVHS #3");

			cbRgbAudio.Items.Add("Audio-in #1");
			cbRgbAudio.Items.Add("Audio-in #2");
			cbRgbAudio.Items.Add("Audio-in #3");

			cbRgbVideo.Items.Add("RGB #1");
			cbRgbVideo.Items.Add("RGB #2");
			cbRgbVideo.Items.Add("RGB #3");
			//
			// Setup combo boxes and controls
			//
			ArrayList availableVideoDevices = FilterHelper.GetVideoInputDevices();
			ArrayList availableVideoDeviceMonikers	= FilterHelper.GetVideoInputDeviceMonikers();
			ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
			ArrayList availableVideoCompressors = FilterHelper.GetVideoCompressors();
			ArrayList availableAudioCompressors = FilterHelper.GetAudioCompressors();

			/* below is used for testing only
			
			availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
			availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
			availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
			availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_e8170070&rev_01#5&267465cb&0&4828f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}");
			availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_e8170070&rev_01#5&e6752e3&0&4820f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}");
			availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_e8070070&rev_01#5&e6752e3&0&4020f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}");		
		*/
			FilterHelper.GetMPEG2VideoEncoders( availableVideoCompressors);
			FilterHelper.GetMPEG2AudioEncoders(availableAudioCompressors);
			for (int i=0; i < availableVideoDevices.Count;++i)
			{
					Log.Write("device:{0} id:{1}", availableVideoDevices[i].ToString(), availableVideoDeviceMonikers[i].ToString());
			}


			audioDeviceComboBox.Items.AddRange(availableAudioDevices.ToArray());
			videoCompressorComboBox.Items.AddRange(availableVideoCompressors.ToArray());
			audioCompressorComboBox.Items.AddRange(availableAudioCompressors.ToArray());

			comboBoxLineInput.Items.Clear();

			if(availableVideoDevices.Count == 0)
			{
				MessageBox.Show("No video device was found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
				useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = cardComboBox.Enabled = okButton.Enabled = setupButton.Enabled = audioCompressorComboBox.Enabled = audioDeviceComboBox.Enabled = videoCompressorComboBox.Enabled = false;
				comboBoxLineInput.Enabled	= false;
				trackRecording.Enabled		= false;
				acceptuserinput						= false;
			}
			else
				acceptuserinput=true;

			// #MW#
			// Load capture card definitions, and only display those cards that are supported by MP
			// This might mean that altough capture cards are present, they're not supported, and thus
			// not displayed ;-)
			//
			// So:
			//	match cards & put in list
			//	if list is empty, inform user and block further input
			//	else continue


			//array indicating if we should show the 'general card' for each video capture device on this system
			bool[] addGeneral=new bool[availableVideoDevices.Count];
			for (int i=0; i < addGeneral.Length;++i)
				addGeneral[i]=true;


			//if capturecardsdefinition.xml contains a definition for 1 or more cards on this system
			//then we wont add the general s/w card, general h/w card and general MCE card choices
			//for this device. Instead we only present the definition from the .xml file
			foreach (CaptureCardDefinition ccd in CaptureCardDefinitions.CaptureCards)
			{
				for (int i = 0; i < availableVideoDevices.Count; i++)
				{
					if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
						((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1 )) addGeneral[i]=false;
				}
			}
			for (int i = 0; i < availableVideoDevices.Count; i++)
			{
				Log.Write("Found capture #{0} card:{1} moniker:{2}",
					i,
					((string)(availableVideoDevices[i])),
					((string)(availableVideoDeviceMonikers[i])) );
			}
			//enum all cards known in capturedefinitions.xml
			foreach (CaptureCardDefinition ccd  in CaptureCardDefinitions.CaptureCards)
			{
				//enum all video capture devices on this system
				for (int i = 0; i < availableVideoDevices.Count; i++)
				{
					//treat the SSE2 DVB-S card as a general H/W card
					if( ((string)(availableVideoDevices[i])) == "B2C2 MPEG-2 Source")
					{
						if (ccd.CommercialName=="General S/W encoding card" ) continue;
						if (ccd.CommercialName=="General MCE card" ) continue;
					}

					bool add=false;
					if (ccd.CaptureName==String.Empty) 
					{
						if (addGeneral[i])
							add=true;
					}
					else
					{
						if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
							((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1 )) add=true;
					}
					if (add)
					{
						TVCaptureDevice cd		= new TVCaptureDevice();
						cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
						if (ccd.CaptureName!=String.Empty) 
						{
							cd.VideoDevice				= ccd.CaptureName;
							cd.CommercialName			= ccd.CommercialName;
							cd.LoadDefinitions();
							cd.IsBDACard					= ccd.Capabilities.IsBDADevice;
							cd.IsMCECard					= ccd.Capabilities.IsMceDevice;
							cd.SupportsMPEG2			= ccd.Capabilities.IsMpeg2Device;
							cd.DeviceId						= ccd.DeviceId;
							cd.DeviceType					= ccd.DeviceId;
						}
						else
						{
							bool bHardware=false;
							bool bMCE=false;
							if (ccd.CommercialName.IndexOf("General MCE card")>=0)
							{
								bHardware=true;
								bMCE=true;
							}
							if (ccd.CommercialName.IndexOf("General H/W encoding card")>=0)
								bHardware=true;
							cd.VideoDevice				= (string)availableVideoDevices[i];
							cd.CommercialName			= ccd.CommercialName;
							cd.CaptureName			  = cd.VideoDevice;
							cd.DeviceId						= ccd.DeviceId;
							cd.DeviceType					= ccd.DeviceId;
							cd.IsBDACard					= false;
							cd.IsMCECard					= bMCE;
							cd.SupportsMPEG2			= bMCE|bHardware;
						}
						Log.Write("Adding name:{0} capture:{1} id:{2} bda:{3} mce:{4} mpeg2:{5}", 
							cd.CommercialName,
							cd.CaptureName,
							cd.DeviceId,
							cd.IsBDACard,cd.IsMCECard,cd.SupportsMPEG2);
						ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(cd);
						int nr=1;
						foreach (ComboBoxCaptureCard cb in cardComboBox.Items)
						{
							if (cb.CaptureDevice.CommercialName==cbcc.CaptureDevice.CommercialName) nr++;
						}
						cbcc.Number=nr;
						cbcc.MaxCards=nr;
						foreach (ComboBoxCaptureCard cb in cardComboBox.Items)
						{
							if (cb.CaptureDevice.CommercialName==cbcc.CaptureDevice.CommercialName) 
							{
								cb.MaxCards=nr;
							}
						}
						if (!addNewCard)
						{
							if (deviceToEdit.DeviceType!=null)
							{
								if (cbcc.CaptureDevice.DeviceId==deviceToEdit.DeviceType)
								{
									if (deviceToEdit.VideoDeviceMoniker==cbcc.VideoDeviceMoniker &&
										deviceToEdit.VideoDevice    ==cbcc.CaptureDevice.CaptureName)
									{
										cardComboBox.Items.Add(cbcc); 
									}
								}
							}
						}
						else
						{
							bool alreadyAdded=false;
							foreach (TVCaptureDevice dev in captureCards)
							{
								if (dev.DeviceType!=null)
								{
									if (cbcc.CaptureDevice.DeviceId==dev.DeviceType)
									{
										if (dev.VideoDeviceMoniker==cbcc.VideoDeviceMoniker &&
											dev.VideoDevice    ==cbcc.CaptureDevice.CaptureName)
										{
											alreadyAdded=true;
										}
									}
								}
							}
							if (!alreadyAdded)
							{
								cardComboBox.Items.Add(cbcc); 
								if (cardComboBox.SelectedItem==null)
									cardComboBox.SelectedIndex=0;
							}
						}
					}
				}
			}

			if (cardComboBox.Items.Count == 0)
			{
				MessageBox.Show("No video capture card(s) where found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
				useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = cardComboBox.Enabled = okButton.Enabled = setupButton.Enabled = audioCompressorComboBox.Enabled = audioDeviceComboBox.Enabled = videoCompressorComboBox.Enabled = false;
				comboBoxLineInput.Enabled = false;
				trackRecording.Enabled    = false;
				acceptuserinput           = false;
			}
			else
				acceptuserinput						= true;

			SetupCaptureFormats();
			frameSizeComboBox.Items.AddRange(captureFormats.ToArray());

			textBoxName.Text=String.Format("card{0}", cardId);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.checkBoxHiQuality = new System.Windows.Forms.CheckBox();
			this.lblRecordingLevel = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.trackRecording = new System.Windows.Forms.TrackBar();
			this.label10 = new System.Windows.Forms.Label();
			this.comboBoxLineInput = new System.Windows.Forms.ComboBox();
			this.audioDeviceComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.setupButton = new System.Windows.Forms.Button();
			this.filterComboBox = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.audioCompressorComboBox = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.videoCompressorComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.frameRateTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.frameSizeComboBox = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.useRecordingCheckBox = new System.Windows.Forms.CheckBox();
			this.useWatchingCheckBox = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cardComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label24 = new System.Windows.Forms.Label();
			this.updownPrio = new System.Windows.Forms.NumericUpDown();
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.label34 = new System.Windows.Forms.Label();
			this.channelsCount = new System.Windows.Forms.Label();
			this.label28 = new System.Windows.Forms.Label();
			this.chName = new System.Windows.Forms.Label();
			this.progressBar2 = new System.Windows.Forms.ProgressBar();
			this.label26 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.button15 = new System.Windows.Forms.Button();
			this.button16 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.label18 = new System.Windows.Forms.Label();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.button18 = new System.Windows.Forms.Button();
			this.button17 = new System.Windows.Forms.Button();
			this.label29 = new System.Windows.Forms.Label();
			this.treeView5 = new System.Windows.Forms.TreeView();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.cbRgbAudio = new System.Windows.Forms.ComboBox();
			this.cbRgbVideo = new System.Windows.Forms.ComboBox();
			this.label35 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.comboBox3Audio = new System.Windows.Forms.ComboBox();
			this.comboBox3Video = new System.Windows.Forms.ComboBox();
			this.comboBox2Audio = new System.Windows.Forms.ComboBox();
			this.comboBox2Video = new System.Windows.Forms.ComboBox();
			this.comboBox1Audio = new System.Windows.Forms.ComboBox();
			this.comboBox1Video = new System.Windows.Forms.ComboBox();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.label13 = new System.Windows.Forms.Label();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.label25 = new System.Windows.Forms.Label();
			this.comboBoxQuality = new System.Windows.Forms.ComboBox();
			this.tabPage7 = new System.Windows.Forms.TabPage();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			this.checkBoxDeleteLow = new System.Windows.Forms.CheckBox();
			this.labelPercent = new System.Windows.Forms.Label();
			this.label33 = new System.Windows.Forms.Label();
			this.buttonBrowse = new System.Windows.Forms.Button();
			this.tbRecordingFolder = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.useLNB4 = new System.Windows.Forms.CheckBox();
			this.useLNB3 = new System.Windows.Forms.CheckBox();
			this.useLNB2 = new System.Windows.Forms.CheckBox();
			this.useLNB1 = new System.Windows.Forms.CheckBox();
			this.lnbkind4 = new System.Windows.Forms.ComboBox();
			this.lnbkind3 = new System.Windows.Forms.ComboBox();
			this.lnbkind2 = new System.Windows.Forms.ComboBox();
			this.label30 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.label32 = new System.Windows.Forms.Label();
			this.lnbconfig4 = new System.Windows.Forms.ComboBox();
			this.lnbconfig3 = new System.Windows.Forms.ComboBox();
			this.lnbconfig2 = new System.Windows.Forms.ComboBox();
			this.diseqcd = new System.Windows.Forms.ComboBox();
			this.diseqcc = new System.Windows.Forms.ComboBox();
			this.diseqcb = new System.Windows.Forms.ComboBox();
			this.diseqca = new System.Windows.Forms.ComboBox();
			this.lnbkind1 = new System.Windows.Forms.ComboBox();
			this.lnbconfig1 = new System.Windows.Forms.ComboBox();
			this.label23 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.circularMHZ = new System.Windows.Forms.TextBox();
			this.label20 = new System.Windows.Forms.Label();
			this.cbandMHZ = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.lnb1MHZ = new System.Windows.Forms.TextBox();
			this.lnb1 = new System.Windows.Forms.Label();
			this.lnbswMHZ = new System.Windows.Forms.TextBox();
			this.switchMHZ = new System.Windows.Forms.Label();
			this.lnb0MHZ = new System.Windows.Forms.TextBox();
			this.label22 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.btnRadio = new System.Windows.Forms.Button();
			this.addsToDB = new System.Windows.Forms.Label();
			this.label37 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.trackRecording)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.updownPrio)).BeginInit();
			this.tabPage6.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.tabPage7.SuspendLayout();
			this.groupBox6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.tabPage4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxHiQuality
			// 
			this.checkBoxHiQuality.Location = new System.Drawing.Point(24, 24);
			this.checkBoxHiQuality.Name = "checkBoxHiQuality";
			this.checkBoxHiQuality.Size = new System.Drawing.Size(368, 16);
			this.checkBoxHiQuality.TabIndex = 7;
			this.checkBoxHiQuality.Text = "Use quality profile for Hauppauge PVR cards";
			// 
			// lblRecordingLevel
			// 
			this.lblRecordingLevel.Location = new System.Drawing.Point(264, 368);
			this.lblRecordingLevel.Name = "lblRecordingLevel";
			this.lblRecordingLevel.Size = new System.Drawing.Size(64, 23);
			this.lblRecordingLevel.TabIndex = 50;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(8, 368);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(88, 32);
			this.label11.TabIndex = 49;
			this.label11.Text = "Audio input recording level";
			// 
			// trackRecording
			// 
			this.trackRecording.Location = new System.Drawing.Point(112, 360);
			this.trackRecording.Maximum = 100;
			this.trackRecording.Name = "trackRecording";
			this.trackRecording.Size = new System.Drawing.Size(136, 45);
			this.trackRecording.TabIndex = 11;
			this.trackRecording.TickFrequency = 10;
			this.trackRecording.Value = 80;
			this.trackRecording.ValueChanged += new System.EventHandler(this.trackRecording_ValueChanged);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(8, 64);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(104, 16);
			this.label10.TabIndex = 47;
			this.label10.Text = "Line input";
			// 
			// comboBoxLineInput
			// 
			this.comboBoxLineInput.Enabled = false;
			this.comboBoxLineInput.Location = new System.Drawing.Point(112, 56);
			this.comboBoxLineInput.Name = "comboBoxLineInput";
			this.comboBoxLineInput.Size = new System.Drawing.Size(320, 21);
			this.comboBoxLineInput.TabIndex = 2;
			// 
			// audioDeviceComboBox
			// 
			this.audioDeviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioDeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioDeviceComboBox.Enabled = false;
			this.audioDeviceComboBox.ItemHeight = 13;
			this.audioDeviceComboBox.Location = new System.Drawing.Point(112, 32);
			this.audioDeviceComboBox.Name = "audioDeviceComboBox";
			this.audioDeviceComboBox.Size = new System.Drawing.Size(320, 21);
			this.audioDeviceComboBox.TabIndex = 1;
			this.audioDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.audioDeviceComboBox_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 40);
			this.label2.Name = "label2";
			this.label2.TabIndex = 43;
			this.label2.Text = "Audio capture card";
			// 
			// setupButton
			// 
			this.setupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.setupButton.Enabled = false;
			this.setupButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.setupButton.Location = new System.Drawing.Point(368, 144);
			this.setupButton.Name = "setupButton";
			this.setupButton.Size = new System.Drawing.Size(56, 21);
			this.setupButton.TabIndex = 4;
			this.setupButton.Text = "Setup";
			this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
			// 
			// filterComboBox
			// 
			this.filterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.filterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.filterComboBox.Enabled = false;
			this.filterComboBox.Location = new System.Drawing.Point(112, 144);
			this.filterComboBox.Name = "filterComboBox";
			this.filterComboBox.Size = new System.Drawing.Size(248, 21);
			this.filterComboBox.TabIndex = 3;
			this.filterComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 152);
			this.label9.Name = "label9";
			this.label9.TabIndex = 40;
			this.label9.Text = "Device property";
			// 
			// audioCompressorComboBox
			// 
			this.audioCompressorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioCompressorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioCompressorComboBox.Enabled = false;
			this.audioCompressorComboBox.ItemHeight = 13;
			this.audioCompressorComboBox.Location = new System.Drawing.Point(128, 88);
			this.audioCompressorComboBox.Name = "audioCompressorComboBox";
			this.audioCompressorComboBox.Size = new System.Drawing.Size(272, 20);
			this.audioCompressorComboBox.TabIndex = 6;
			this.audioCompressorComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.audioCompressorComboBox_KeyPress);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(24, 88);
			this.label5.Name = "label5";
			this.label5.TabIndex = 38;
			this.label5.Text = "Audio codec";
			// 
			// videoCompressorComboBox
			// 
			this.videoCompressorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.videoCompressorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoCompressorComboBox.Enabled = false;
			this.videoCompressorComboBox.ItemHeight = 13;
			this.videoCompressorComboBox.Location = new System.Drawing.Point(128, 56);
			this.videoCompressorComboBox.Name = "videoCompressorComboBox";
			this.videoCompressorComboBox.Size = new System.Drawing.Size(272, 20);
			this.videoCompressorComboBox.TabIndex = 5;
			this.videoCompressorComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.videoCompressorComboBox_KeyPress);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 64);
			this.label3.Name = "label3";
			this.label3.TabIndex = 36;
			this.label3.Text = "Video codec";
			// 
			// frameRateTextBox
			// 
			this.frameRateTextBox.Enabled = false;
			this.frameRateTextBox.Location = new System.Drawing.Point(112, 336);
			this.frameRateTextBox.MaxLength = 3;
			this.frameRateTextBox.Name = "frameRateTextBox";
			this.frameRateTextBox.Size = new System.Drawing.Size(40, 20);
			this.frameRateTextBox.TabIndex = 27;
			this.frameRateTextBox.Text = "25";
			this.frameRateTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frameRateTextBox_KeyPress);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 336);
			this.label6.Name = "label6";
			this.label6.TabIndex = 20;
			this.label6.Text = "Framerate";
			// 
			// frameSizeComboBox
			// 
			this.frameSizeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.frameSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.frameSizeComboBox.Enabled = false;
			this.frameSizeComboBox.ItemHeight = 13;
			this.frameSizeComboBox.Location = new System.Drawing.Point(112, 304);
			this.frameSizeComboBox.Name = "frameSizeComboBox";
			this.frameSizeComboBox.Size = new System.Drawing.Size(176, 21);
			this.frameSizeComboBox.TabIndex = 10;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 304);
			this.label7.Name = "label7";
			this.label7.TabIndex = 18;
			this.label7.Text = "Framesize";
			// 
			// useRecordingCheckBox
			// 
			this.useRecordingCheckBox.Checked = true;
			this.useRecordingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useRecordingCheckBox.Enabled = false;
			this.useRecordingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useRecordingCheckBox.Location = new System.Drawing.Point(32, 224);
			this.useRecordingCheckBox.Name = "useRecordingCheckBox";
			this.useRecordingCheckBox.Size = new System.Drawing.Size(168, 16);
			this.useRecordingCheckBox.TabIndex = 9;
			this.useRecordingCheckBox.Text = "Use this card for recording TV";
			// 
			// useWatchingCheckBox
			// 
			this.useWatchingCheckBox.Checked = true;
			this.useWatchingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useWatchingCheckBox.Enabled = false;
			this.useWatchingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useWatchingCheckBox.Location = new System.Drawing.Point(32, 200);
			this.useWatchingCheckBox.Name = "useWatchingCheckBox";
			this.useWatchingCheckBox.Size = new System.Drawing.Size(168, 24);
			this.useWatchingCheckBox.TabIndex = 8;
			this.useWatchingCheckBox.Text = "Use this card for viewing TV";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 184);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "Purpose";
			// 
			// cardComboBox
			// 
			this.cardComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cardComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cardComboBox.Location = new System.Drawing.Point(112, 8);
			this.cardComboBox.Name = "cardComboBox";
			this.cardComboBox.Size = new System.Drawing.Size(320, 21);
			this.cardComboBox.TabIndex = 0;
			this.cardComboBox.SelectedIndexChanged += new System.EventHandler(this.cardComboBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "TV Capture card";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 96);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(424, 40);
			this.label8.TabIndex = 45;
			this.label8.Text = "To configure properties that are specific for your capture device, select the pro" +
				"perty in the dropdown list below and press the \'Setup\' button. [Note: it depends" +
				" on your TV-card if these settings are saved!!]";
			// 
			// textBoxName
			// 
			this.textBoxName.Location = new System.Drawing.Point(0, 0);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.TabIndex = 0;
			this.textBoxName.Text = "";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(0, 0);
			this.label12.Name = "label12";
			this.label12.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(381, 450);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(301, 450);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage6);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Controls.Add(this.tabPage7);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(456, 432);
			this.tabControl1.TabIndex = 5;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label24);
			this.tabPage1.Controls.Add(this.updownPrio);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.cardComboBox);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.label10);
			this.tabPage1.Controls.Add(this.comboBoxLineInput);
			this.tabPage1.Controls.Add(this.audioDeviceComboBox);
			this.tabPage1.Controls.Add(this.useRecordingCheckBox);
			this.tabPage1.Controls.Add(this.useWatchingCheckBox);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.frameRateTextBox);
			this.tabPage1.Controls.Add(this.label6);
			this.tabPage1.Controls.Add(this.frameSizeComboBox);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.lblRecordingLevel);
			this.tabPage1.Controls.Add(this.label11);
			this.tabPage1.Controls.Add(this.trackRecording);
			this.tabPage1.Controls.Add(this.setupButton);
			this.tabPage1.Controls.Add(this.filterComboBox);
			this.tabPage1.Controls.Add(this.label9);
			this.tabPage1.Controls.Add(this.label8);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(448, 406);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Capture card";
			// 
			// label24
			// 
			this.label24.Location = new System.Drawing.Point(8, 272);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(136, 16);
			this.label24.TabIndex = 52;
			this.label24.Text = "Priority (1=low,10=high)";
			// 
			// updownPrio
			// 
			this.updownPrio.Location = new System.Drawing.Point(152, 272);
			this.updownPrio.Maximum = new System.Decimal(new int[] {
																	   10,
																	   0,
																	   0,
																	   0});
			this.updownPrio.Minimum = new System.Decimal(new int[] {
																	   1,
																	   0,
																	   0,
																	   0});
			this.updownPrio.Name = "updownPrio";
			this.updownPrio.Size = new System.Drawing.Size(56, 20);
			this.updownPrio.TabIndex = 51;
			this.updownPrio.Value = new System.Decimal(new int[] {
																	 1,
																	 0,
																	 0,
																	 0});
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.groupBox5);
			this.tabPage6.Controls.Add(this.button3);
			this.tabPage6.Controls.Add(this.button2);
			this.tabPage6.Controls.Add(this.label18);
			this.tabPage6.Controls.Add(this.treeView1);
			this.tabPage6.Controls.Add(this.button18);
			this.tabPage6.Controls.Add(this.button17);
			this.tabPage6.Controls.Add(this.label29);
			this.tabPage6.Controls.Add(this.treeView5);
			this.tabPage6.Location = new System.Drawing.Point(4, 22);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.tabPage6.Size = new System.Drawing.Size(448, 406);
			this.tabPage6.TabIndex = 5;
			this.tabPage6.Text = "EPG-Grabber";
			this.tabPage6.Click += new System.EventHandler(this.tabPage6_Click);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.label37);
			this.groupBox5.Controls.Add(this.addsToDB);
			this.groupBox5.Controls.Add(this.label34);
			this.groupBox5.Controls.Add(this.channelsCount);
			this.groupBox5.Controls.Add(this.label28);
			this.groupBox5.Controls.Add(this.chName);
			this.groupBox5.Controls.Add(this.progressBar2);
			this.groupBox5.Controls.Add(this.label26);
			this.groupBox5.Controls.Add(this.label27);
			this.groupBox5.Controls.Add(this.button15);
			this.groupBox5.Controls.Add(this.button16);
			this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox5.Location = new System.Drawing.Point(8, 240);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(432, 160);
			this.groupBox5.TabIndex = 35;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Grabbing-Progress";
			// 
			// label34
			// 
			this.label34.Location = new System.Drawing.Point(8, 72);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(112, 16);
			this.label34.TabIndex = 33;
			this.label34.Text = "MHW-EPG Channels";
			// 
			// channelsCount
			// 
			this.channelsCount.Location = new System.Drawing.Point(128, 72);
			this.channelsCount.Name = "channelsCount";
			this.channelsCount.Size = new System.Drawing.Size(32, 16);
			this.channelsCount.TabIndex = 32;
			this.channelsCount.Text = "0";
			this.channelsCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label28
			// 
			this.label28.Location = new System.Drawing.Point(8, 24);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(96, 16);
			this.label28.TabIndex = 24;
			this.label28.Text = "Current Channel:";
			// 
			// chName
			// 
			this.chName.Location = new System.Drawing.Point(128, 24);
			this.chName.Name = "chName";
			this.chName.Size = new System.Drawing.Size(296, 16);
			this.chName.TabIndex = 25;
			this.chName.Text = "Stopped.";
			// 
			// progressBar2
			// 
			this.progressBar2.Location = new System.Drawing.Point(8, 130);
			this.progressBar2.Name = "progressBar2";
			this.progressBar2.Size = new System.Drawing.Size(320, 16);
			this.progressBar2.TabIndex = 26;
			// 
			// label26
			// 
			this.label26.Location = new System.Drawing.Point(8, 48);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(80, 16);
			this.label26.TabIndex = 30;
			this.label26.Text = "Events found:";
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(128, 48);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(104, 16);
			this.label27.TabIndex = 29;
			this.label27.Text = "0";
			this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// button15
			// 
			this.button15.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button15.Location = new System.Drawing.Point(336, 104);
			this.button15.Name = "button15";
			this.button15.Size = new System.Drawing.Size(88, 21);
			this.button15.TabIndex = 22;
			this.button15.Text = "Start Grab";
			this.button15.Click += new System.EventHandler(this.button15_Click);
			// 
			// button16
			// 
			this.button16.Enabled = false;
			this.button16.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button16.Location = new System.Drawing.Point(336, 128);
			this.button16.Name = "button16";
			this.button16.Size = new System.Drawing.Size(88, 21);
			this.button16.TabIndex = 23;
			this.button16.Text = "Stop";
			this.button16.Click += new System.EventHandler(this.button16_Click);
			// 
			// button3
			// 
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button3.Location = new System.Drawing.Point(216, 208);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(48, 21);
			this.button3.TabIndex = 34;
			this.button3.Text = "None";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button2
			// 
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(160, 208);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(48, 21);
			this.button2.TabIndex = 33;
			this.button2.Text = "All";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(160, 8);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(152, 16);
			this.label18.TabIndex = 32;
			this.label18.Text = "Only grab those Languages:";
			// 
			// treeView1
			// 
			this.treeView1.CheckBoxes = true;
			this.treeView1.ImageIndex = -1;
			this.treeView1.Location = new System.Drawing.Point(160, 32);
			this.treeView1.Name = "treeView1";
			this.treeView1.SelectedImageIndex = -1;
			this.treeView1.Size = new System.Drawing.Size(152, 168);
			this.treeView1.TabIndex = 31;
			// 
			// button18
			// 
			this.button18.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button18.Location = new System.Drawing.Point(64, 208);
			this.button18.Name = "button18";
			this.button18.Size = new System.Drawing.Size(48, 21);
			this.button18.TabIndex = 28;
			this.button18.Text = "None";
			this.button18.Click += new System.EventHandler(this.button18_Click);
			// 
			// button17
			// 
			this.button17.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button17.Location = new System.Drawing.Point(8, 208);
			this.button17.Name = "button17";
			this.button17.Size = new System.Drawing.Size(48, 21);
			this.button17.TabIndex = 27;
			this.button17.Text = "All";
			this.button17.Click += new System.EventHandler(this.button17_Click);
			// 
			// label29
			// 
			this.label29.Location = new System.Drawing.Point(8, 8);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(128, 16);
			this.label29.TabIndex = 21;
			this.label29.Text = "TV-Channels:";
			// 
			// treeView5
			// 
			this.treeView5.CheckBoxes = true;
			this.treeView5.HideSelection = false;
			this.treeView5.ImageIndex = -1;
			this.treeView5.Location = new System.Drawing.Point(8, 32);
			this.treeView5.Name = "treeView5";
			this.treeView5.SelectedImageIndex = -1;
			this.treeView5.Size = new System.Drawing.Size(144, 168);
			this.treeView5.Sorted = true;
			this.treeView5.TabIndex = 20;
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.cbRgbAudio);
			this.tabPage3.Controls.Add(this.cbRgbVideo);
			this.tabPage3.Controls.Add(this.label35);
			this.tabPage3.Controls.Add(this.label14);
			this.tabPage3.Controls.Add(this.comboBox3Audio);
			this.tabPage3.Controls.Add(this.comboBox3Video);
			this.tabPage3.Controls.Add(this.comboBox2Audio);
			this.tabPage3.Controls.Add(this.comboBox2Video);
			this.tabPage3.Controls.Add(this.comboBox1Audio);
			this.tabPage3.Controls.Add(this.comboBox1Video);
			this.tabPage3.Controls.Add(this.label15);
			this.tabPage3.Controls.Add(this.label16);
			this.tabPage3.Controls.Add(this.label17);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(448, 406);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Audio mapping";
			// 
			// cbRgbAudio
			// 
			this.cbRgbAudio.Location = new System.Drawing.Point(72, 264);
			this.cbRgbAudio.Name = "cbRgbAudio";
			this.cbRgbAudio.Size = new System.Drawing.Size(121, 21);
			this.cbRgbAudio.TabIndex = 21;
			// 
			// cbRgbVideo
			// 
			this.cbRgbVideo.Location = new System.Drawing.Point(72, 240);
			this.cbRgbVideo.Name = "cbRgbVideo";
			this.cbRgbVideo.Size = new System.Drawing.Size(121, 21);
			this.cbRgbVideo.TabIndex = 20;
			// 
			// label35
			// 
			this.label35.Location = new System.Drawing.Point(16, 248);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(40, 23);
			this.label35.TabIndex = 19;
			this.label35.Text = "RGB";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(16, 16);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(328, 40);
			this.label14.TabIndex = 18;
			this.label14.Text = "Map the video/audio inputs of your TV card to the CVBS1, CVBS2 and SVHS channels";
			// 
			// comboBox3Audio
			// 
			this.comboBox3Audio.Location = new System.Drawing.Point(72, 208);
			this.comboBox3Audio.Name = "comboBox3Audio";
			this.comboBox3Audio.Size = new System.Drawing.Size(121, 21);
			this.comboBox3Audio.TabIndex = 16;
			// 
			// comboBox3Video
			// 
			this.comboBox3Video.Location = new System.Drawing.Point(72, 184);
			this.comboBox3Video.Name = "comboBox3Video";
			this.comboBox3Video.Size = new System.Drawing.Size(121, 21);
			this.comboBox3Video.TabIndex = 15;
			// 
			// comboBox2Audio
			// 
			this.comboBox2Audio.Location = new System.Drawing.Point(72, 144);
			this.comboBox2Audio.Name = "comboBox2Audio";
			this.comboBox2Audio.Size = new System.Drawing.Size(121, 21);
			this.comboBox2Audio.TabIndex = 14;
			// 
			// comboBox2Video
			// 
			this.comboBox2Video.Location = new System.Drawing.Point(72, 120);
			this.comboBox2Video.Name = "comboBox2Video";
			this.comboBox2Video.Size = new System.Drawing.Size(121, 21);
			this.comboBox2Video.TabIndex = 13;
			// 
			// comboBox1Audio
			// 
			this.comboBox1Audio.Location = new System.Drawing.Point(72, 88);
			this.comboBox1Audio.Name = "comboBox1Audio";
			this.comboBox1Audio.Size = new System.Drawing.Size(121, 21);
			this.comboBox1Audio.TabIndex = 10;
			// 
			// comboBox1Video
			// 
			this.comboBox1Video.Location = new System.Drawing.Point(72, 64);
			this.comboBox1Video.Name = "comboBox1Video";
			this.comboBox1Video.Size = new System.Drawing.Size(121, 21);
			this.comboBox1Video.TabIndex = 8;
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(16, 200);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(40, 23);
			this.label15.TabIndex = 12;
			this.label15.Text = "SVHS";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(8, 136);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(48, 16);
			this.label16.TabIndex = 11;
			this.label16.Text = "CVBS#2";
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(8, 80);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(56, 16);
			this.label17.TabIndex = 9;
			this.label17.Text = "CVBS #1";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.label13);
			this.tabPage2.Controls.Add(this.audioCompressorComboBox);
			this.tabPage2.Controls.Add(this.label5);
			this.tabPage2.Controls.Add(this.videoCompressorComboBox);
			this.tabPage2.Controls.Add(this.label3);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(448, 406);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Codecs";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(16, 16);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(408, 40);
			this.label13.TabIndex = 39;
			this.label13.Text = "Specify which codecs should be used when recording.  You only need to specify cod" +
				"ecs if your card is a s/w encoding card";
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.label25);
			this.tabPage5.Controls.Add(this.comboBoxQuality);
			this.tabPage5.Controls.Add(this.checkBoxHiQuality);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(448, 406);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "Quality";
			// 
			// label25
			// 
			this.label25.Location = new System.Drawing.Point(40, 48);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(48, 16);
			this.label25.TabIndex = 9;
			this.label25.Text = "Quality:";
			// 
			// comboBoxQuality
			// 
			this.comboBoxQuality.Items.AddRange(new object[] {
																 "Low (VBR 2-4.5 MBPS)",
																 "Medium (VBR 4-6 MBPS)",
																 "High (VBR 8-12 MBPS)"});
			this.comboBoxQuality.Location = new System.Drawing.Point(96, 48);
			this.comboBoxQuality.Name = "comboBoxQuality";
			this.comboBoxQuality.Size = new System.Drawing.Size(121, 21);
			this.comboBoxQuality.TabIndex = 8;
			// 
			// tabPage7
			// 
			this.tabPage7.Controls.Add(this.groupBox6);
			this.tabPage7.Controls.Add(this.buttonBrowse);
			this.tabPage7.Controls.Add(this.tbRecordingFolder);
			this.tabPage7.Controls.Add(this.label19);
			this.tabPage7.Location = new System.Drawing.Point(4, 22);
			this.tabPage7.Name = "tabPage7";
			this.tabPage7.Size = new System.Drawing.Size(448, 406);
			this.tabPage7.TabIndex = 6;
			this.tabPage7.Text = "Recordings";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.trackBar1);
			this.groupBox6.Controls.Add(this.checkBoxDeleteLow);
			this.groupBox6.Controls.Add(this.labelPercent);
			this.groupBox6.Controls.Add(this.label33);
			this.groupBox6.Location = new System.Drawing.Point(8, 72);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(428, 120);
			this.groupBox6.TabIndex = 59;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Diskspace management";
			// 
			// trackBar1
			// 
			this.trackBar1.Location = new System.Drawing.Point(144, 24);
			this.trackBar1.Maximum = 100;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new System.Drawing.Size(104, 45);
			this.trackBar1.TabIndex = 5;
			this.trackBar1.TickFrequency = 10;
			this.trackBar1.Value = 50;
			this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
			// 
			// checkBoxDeleteLow
			// 
			this.checkBoxDeleteLow.Location = new System.Drawing.Point(32, 80);
			this.checkBoxDeleteLow.Name = "checkBoxDeleteLow";
			this.checkBoxDeleteLow.Size = new System.Drawing.Size(360, 16);
			this.checkBoxDeleteLow.TabIndex = 4;
			this.checkBoxDeleteLow.Text = "Automaticly delete oldest recordings when low on free diskspace";
			// 
			// labelPercent
			// 
			this.labelPercent.Location = new System.Drawing.Point(272, 32);
			this.labelPercent.Name = "labelPercent";
			this.labelPercent.Size = new System.Drawing.Size(128, 23);
			this.labelPercent.TabIndex = 2;
			this.labelPercent.Text = "%";
			// 
			// label33
			// 
			this.label33.Location = new System.Drawing.Point(24, 24);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(112, 32);
			this.label33.TabIndex = 0;
			this.label33.Text = "Limit diskspace used for recordings to:";
			// 
			// buttonBrowse
			// 
			this.buttonBrowse.Location = new System.Drawing.Point(368, 24);
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.Size = new System.Drawing.Size(56, 23);
			this.buttonBrowse.TabIndex = 58;
			this.buttonBrowse.Text = "Browse";
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click_1);
			// 
			// tbRecordingFolder
			// 
			this.tbRecordingFolder.Location = new System.Drawing.Point(112, 24);
			this.tbRecordingFolder.Name = "tbRecordingFolder";
			this.tbRecordingFolder.Size = new System.Drawing.Size(248, 20);
			this.tbRecordingFolder.TabIndex = 57;
			this.tbRecordingFolder.Text = "";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(8, 32);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(100, 16);
			this.label19.TabIndex = 56;
			this.label19.Text = "Recording folder:";
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.groupBox1);
			this.tabPage4.Controls.Add(this.groupBox2);
			this.tabPage4.Controls.Add(this.label23);
			this.tabPage4.Controls.Add(this.groupBox4);
			this.tabPage4.Controls.Add(this.groupBox3);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(448, 406);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "DVB-S LNB";
			this.tabPage4.Click += new System.EventHandler(this.tabPage4_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBox1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(24, 344);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(144, 56);
			this.groupBox1.TabIndex = 33;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Use SkyStar2 MD-Plugins";
			// 
			// checkBox1
			// 
			this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBox1.Location = new System.Drawing.Point(32, 24);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(88, 16);
			this.checkBox1.TabIndex = 0;
			this.checkBox1.Text = "Use Plugins";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.useLNB4);
			this.groupBox2.Controls.Add(this.useLNB3);
			this.groupBox2.Controls.Add(this.useLNB2);
			this.groupBox2.Controls.Add(this.useLNB1);
			this.groupBox2.Controls.Add(this.lnbkind4);
			this.groupBox2.Controls.Add(this.lnbkind3);
			this.groupBox2.Controls.Add(this.lnbkind2);
			this.groupBox2.Controls.Add(this.label30);
			this.groupBox2.Controls.Add(this.label31);
			this.groupBox2.Controls.Add(this.label32);
			this.groupBox2.Controls.Add(this.lnbconfig4);
			this.groupBox2.Controls.Add(this.lnbconfig3);
			this.groupBox2.Controls.Add(this.lnbconfig2);
			this.groupBox2.Controls.Add(this.diseqcd);
			this.groupBox2.Controls.Add(this.diseqcc);
			this.groupBox2.Controls.Add(this.diseqcb);
			this.groupBox2.Controls.Add(this.diseqca);
			this.groupBox2.Controls.Add(this.lnbkind1);
			this.groupBox2.Controls.Add(this.lnbconfig1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(24, 192);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(408, 144);
			this.groupBox2.TabIndex = 32;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "DiSeqC (Skystar2) / LNB-Config";
			this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
			// 
			// useLNB4
			// 
			this.useLNB4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useLNB4.Location = new System.Drawing.Point(312, 112);
			this.useLNB4.Name = "useLNB4";
			this.useLNB4.Size = new System.Drawing.Size(80, 16);
			this.useLNB4.TabIndex = 31;
			this.useLNB4.Text = "In Use";
			// 
			// useLNB3
			// 
			this.useLNB3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useLNB3.Location = new System.Drawing.Point(312, 88);
			this.useLNB3.Name = "useLNB3";
			this.useLNB3.Size = new System.Drawing.Size(80, 16);
			this.useLNB3.TabIndex = 30;
			this.useLNB3.Text = "In Use";
			// 
			// useLNB2
			// 
			this.useLNB2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useLNB2.Location = new System.Drawing.Point(312, 64);
			this.useLNB2.Name = "useLNB2";
			this.useLNB2.Size = new System.Drawing.Size(80, 16);
			this.useLNB2.TabIndex = 29;
			this.useLNB2.Text = "In Use";
			// 
			// useLNB1
			// 
			this.useLNB1.Checked = true;
			this.useLNB1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useLNB1.Enabled = false;
			this.useLNB1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useLNB1.Location = new System.Drawing.Point(312, 40);
			this.useLNB1.Name = "useLNB1";
			this.useLNB1.Size = new System.Drawing.Size(80, 16);
			this.useLNB1.TabIndex = 28;
			this.useLNB1.Text = "In Use";
			// 
			// lnbkind4
			// 
			this.lnbkind4.Items.AddRange(new object[] {
														  "Ku-Band",
														  "C-Band",
														  "Circular"});
			this.lnbkind4.Location = new System.Drawing.Point(232, 109);
			this.lnbkind4.Name = "lnbkind4";
			this.lnbkind4.Size = new System.Drawing.Size(72, 21);
			this.lnbkind4.TabIndex = 26;
			// 
			// lnbkind3
			// 
			this.lnbkind3.Items.AddRange(new object[] {
														  "Ku-Band",
														  "C-Band",
														  "Circular"});
			this.lnbkind3.Location = new System.Drawing.Point(232, 85);
			this.lnbkind3.Name = "lnbkind3";
			this.lnbkind3.Size = new System.Drawing.Size(72, 21);
			this.lnbkind3.TabIndex = 25;
			// 
			// lnbkind2
			// 
			this.lnbkind2.Items.AddRange(new object[] {
														  "Ku-Band",
														  "C-Band",
														  "Circular"});
			this.lnbkind2.Location = new System.Drawing.Point(232, 61);
			this.lnbkind2.Name = "lnbkind2";
			this.lnbkind2.Size = new System.Drawing.Size(72, 21);
			this.lnbkind2.TabIndex = 24;
			// 
			// label30
			// 
			this.label30.Location = new System.Drawing.Point(232, 21);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(56, 16);
			this.label30.TabIndex = 22;
			this.label30.Text = "LNB:";
			// 
			// label31
			// 
			this.label31.Location = new System.Drawing.Point(136, 21);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(64, 16);
			this.label31.TabIndex = 21;
			this.label31.Text = "LNBSelect:";
			// 
			// label32
			// 
			this.label32.Location = new System.Drawing.Point(16, 21);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(80, 16);
			this.label32.TabIndex = 20;
			this.label32.Text = "DiSEqC:";
			// 
			// lnbconfig4
			// 
			this.lnbconfig4.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig4.Location = new System.Drawing.Point(136, 109);
			this.lnbconfig4.Name = "lnbconfig4";
			this.lnbconfig4.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig4.TabIndex = 19;
			// 
			// lnbconfig3
			// 
			this.lnbconfig3.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig3.Location = new System.Drawing.Point(136, 85);
			this.lnbconfig3.Name = "lnbconfig3";
			this.lnbconfig3.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig3.TabIndex = 18;
			// 
			// lnbconfig2
			// 
			this.lnbconfig2.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig2.Location = new System.Drawing.Point(136, 61);
			this.lnbconfig2.Name = "lnbconfig2";
			this.lnbconfig2.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig2.TabIndex = 17;
			// 
			// diseqcd
			// 
			this.diseqcd.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqcd.Location = new System.Drawing.Point(16, 109);
			this.diseqcd.Name = "diseqcd";
			this.diseqcd.Size = new System.Drawing.Size(104, 21);
			this.diseqcd.TabIndex = 14;
			this.diseqcd.Text = "None";
			this.diseqcd.SelectedIndexChanged += new System.EventHandler(this.diseqcd_SelectedIndexChanged);
			// 
			// diseqcc
			// 
			this.diseqcc.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqcc.Location = new System.Drawing.Point(16, 85);
			this.diseqcc.Name = "diseqcc";
			this.diseqcc.Size = new System.Drawing.Size(104, 21);
			this.diseqcc.TabIndex = 13;
			this.diseqcc.Text = "None";
			this.diseqcc.SelectedIndexChanged += new System.EventHandler(this.diseqcc_SelectedIndexChanged);
			// 
			// diseqcb
			// 
			this.diseqcb.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqcb.Location = new System.Drawing.Point(16, 61);
			this.diseqcb.Name = "diseqcb";
			this.diseqcb.Size = new System.Drawing.Size(104, 21);
			this.diseqcb.TabIndex = 12;
			this.diseqcb.Text = "None";
			this.diseqcb.SelectedIndexChanged += new System.EventHandler(this.diseqcb_SelectedIndexChanged);
			// 
			// diseqca
			// 
			this.diseqca.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqca.Location = new System.Drawing.Point(16, 37);
			this.diseqca.Name = "diseqca";
			this.diseqca.Size = new System.Drawing.Size(104, 21);
			this.diseqca.TabIndex = 1;
			this.diseqca.Text = "None";
			this.diseqca.SelectedIndexChanged += new System.EventHandler(this.diseqca_SelectedIndexChanged);
			// 
			// lnbkind1
			// 
			this.lnbkind1.Items.AddRange(new object[] {
														  "Ku-Band",
														  "C-Band",
														  "Circular"});
			this.lnbkind1.Location = new System.Drawing.Point(232, 37);
			this.lnbkind1.Name = "lnbkind1";
			this.lnbkind1.Size = new System.Drawing.Size(72, 21);
			this.lnbkind1.TabIndex = 27;
			// 
			// lnbconfig1
			// 
			this.lnbconfig1.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig1.Location = new System.Drawing.Point(136, 37);
			this.lnbconfig1.Name = "lnbconfig1";
			this.lnbconfig1.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig1.TabIndex = 24;
			// 
			// label23
			// 
			this.label23.Location = new System.Drawing.Point(24, 16);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(352, 32);
			this.label23.TabIndex = 31;
			this.label23.Text = "Specify your LNB settings. Please note this is only necessary for DVB-S (sattelit" +
				"e) tv capture cards!";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.circularMHZ);
			this.groupBox4.Controls.Add(this.label20);
			this.groupBox4.Controls.Add(this.cbandMHZ);
			this.groupBox4.Controls.Add(this.label21);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(256, 72);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(176, 96);
			this.groupBox4.TabIndex = 29;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "C-Band / Circular Config:";
			// 
			// circularMHZ
			// 
			this.circularMHZ.Location = new System.Drawing.Point(96, 45);
			this.circularMHZ.Name = "circularMHZ";
			this.circularMHZ.Size = new System.Drawing.Size(64, 20);
			this.circularMHZ.TabIndex = 3;
			this.circularMHZ.Text = "10750";
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(16, 48);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(80, 16);
			this.label20.TabIndex = 2;
			this.label20.Text = "Circular (MHz)";
			// 
			// cbandMHZ
			// 
			this.cbandMHZ.Location = new System.Drawing.Point(96, 21);
			this.cbandMHZ.Name = "cbandMHZ";
			this.cbandMHZ.Size = new System.Drawing.Size(64, 20);
			this.cbandMHZ.TabIndex = 1;
			this.cbandMHZ.Text = "5150";
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(16, 24);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(80, 16);
			this.label21.TabIndex = 0;
			this.label21.Text = "C-Band (MHz)";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.lnb1MHZ);
			this.groupBox3.Controls.Add(this.lnb1);
			this.groupBox3.Controls.Add(this.lnbswMHZ);
			this.groupBox3.Controls.Add(this.switchMHZ);
			this.groupBox3.Controls.Add(this.lnb0MHZ);
			this.groupBox3.Controls.Add(this.label22);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(24, 72);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(176, 96);
			this.groupBox3.TabIndex = 28;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Ku-Band Config:";
			// 
			// lnb1MHZ
			// 
			this.lnb1MHZ.Location = new System.Drawing.Point(104, 69);
			this.lnb1MHZ.Name = "lnb1MHZ";
			this.lnb1MHZ.Size = new System.Drawing.Size(56, 20);
			this.lnb1MHZ.TabIndex = 7;
			this.lnb1MHZ.Text = "10600";
			// 
			// lnb1
			// 
			this.lnb1.Location = new System.Drawing.Point(16, 72);
			this.lnb1.Name = "lnb1";
			this.lnb1.Size = new System.Drawing.Size(72, 16);
			this.lnb1.TabIndex = 6;
			this.lnb1.Text = "LNB1 (Mhz)";
			// 
			// lnbswMHZ
			// 
			this.lnbswMHZ.Location = new System.Drawing.Point(104, 45);
			this.lnbswMHZ.Name = "lnbswMHZ";
			this.lnbswMHZ.Size = new System.Drawing.Size(56, 20);
			this.lnbswMHZ.TabIndex = 3;
			this.lnbswMHZ.Text = "11700";
			// 
			// switchMHZ
			// 
			this.switchMHZ.Location = new System.Drawing.Point(16, 48);
			this.switchMHZ.Name = "switchMHZ";
			this.switchMHZ.Size = new System.Drawing.Size(80, 16);
			this.switchMHZ.TabIndex = 2;
			this.switchMHZ.Text = "Switch (MHz)";
			// 
			// lnb0MHZ
			// 
			this.lnb0MHZ.Location = new System.Drawing.Point(104, 21);
			this.lnb0MHZ.Name = "lnb0MHZ";
			this.lnb0MHZ.Size = new System.Drawing.Size(56, 20);
			this.lnb0MHZ.TabIndex = 1;
			this.lnb0MHZ.Text = "9750";
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(16, 24);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(72, 16);
			this.label22.TabIndex = 0;
			this.label22.Text = "LNB0 (Mhz)";
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(192, 450);
			this.button1.Name = "button1";
			this.button1.TabIndex = 6;
			this.button1.Text = "Autotune";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// btnRadio
			// 
			this.btnRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRadio.Location = new System.Drawing.Point(80, 450);
			this.btnRadio.Name = "btnRadio";
			this.btnRadio.Size = new System.Drawing.Size(96, 23);
			this.btnRadio.TabIndex = 7;
			this.btnRadio.Text = "Autotune Radio";
			this.btnRadio.Click += new System.EventHandler(this.btnRadio_Click);
			// 
			// addsToDB
			// 
			this.addsToDB.Location = new System.Drawing.Point(128, 96);
			this.addsToDB.Name = "addsToDB";
			this.addsToDB.Size = new System.Drawing.Size(64, 16);
			this.addsToDB.TabIndex = 34;
			this.addsToDB.Text = "0";
			// 
			// label37
			// 
			this.label37.Location = new System.Drawing.Point(8, 96);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(112, 16);
			this.label37.TabIndex = 35;
			this.label37.Text = "MHW Events found:";
			// 
			// EditCaptureCardForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(474, 482);
			this.Controls.Add(this.btnRadio);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = new System.Drawing.Size(480, 296);
			this.Name = "EditCaptureCardForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit properties of your TV card";
			this.Load += new System.EventHandler(this.EditCaptureCardForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackRecording)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.updownPrio)).EndInit();
			this.tabPage6.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage5.ResumeLayout(false);
			this.tabPage7.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.tabPage4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		void FillInDefaultRecordingPath()
		{
			if (tbRecordingFolder.Text!=String.Empty) return;
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				for (int i = 0; i < 20; i++)
				{
					string strSharePath = String.Format("sharepath{0}",i);
					string path=xmlreader.GetValueAsString("movies", strSharePath, "");
					if (path!="" && Util.Utils.IsHD(path))
					{
						tbRecordingFolder.Text=path;
						UpdatePercentageLabel();
						break;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			if (tbRecordingFolder.Text==String.Empty)
			{
				tabControl1.SelectedTab=tabPage7;
				MessageBox.Show("No recording folder specified", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			
			string filename=String.Format(@"database\card_{0}.xml",CaptureCard.FriendlyName);
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml(filename))
			{
				xmlwriter.SetValue("mapping", "audio1", comboBox1Audio.SelectedIndex);
				xmlwriter.SetValue("mapping", "audio2", comboBox2Audio.SelectedIndex);
				xmlwriter.SetValue("mapping", "audio3", comboBox3Audio.SelectedIndex);
				xmlwriter.SetValue("mapping", "audio4", cbRgbAudio.SelectedIndex);
																								 
																								 
				xmlwriter.SetValue("mapping", "video1", comboBox1Video.SelectedIndex);
				xmlwriter.SetValue("mapping", "video2", comboBox2Video.SelectedIndex);
				xmlwriter.SetValue("mapping", "video3", comboBox3Video.SelectedIndex);
				xmlwriter.SetValue("mapping", "video4", cbRgbVideo.SelectedIndex);
			}
			SaveDVBSSettings();

			if (videoCompressorComboBox.Enabled)
			{
				if (videoCompressorComboBox.Items.Count>0 && videoCompressorComboBox.SelectedItem==null)
				{
					MessageBox.Show("No video compressor selected.Please choose a video compressor", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			if (audioCompressorComboBox.Enabled)
			{
				if (audioCompressorComboBox.Items.Count>0 && audioCompressorComboBox.SelectedItem==null)
				{
					tabControl1.SelectedTab=tabPage2;
					MessageBox.Show("No audio compressor selected.Please choose an audio compressor", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			if (audioDeviceComboBox.Enabled)
			{
				if (audioDeviceComboBox.Items.Count>0 && audioDeviceComboBox.SelectedItem==null)
				{
					tabControl1.SelectedTab=tabPage1;
					MessageBox.Show("No audio device selected.Please choose an audio device", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			if (comboBoxLineInput.Enabled)
			{
				if (comboBoxLineInput.Items.Count>0 && comboBoxLineInput.SelectedItem==null)
				{
					tabControl1.SelectedTab=tabPage1;
					MessageBox.Show("No line input selected.Please choose a line input", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}


		private void SetupPropertyPages(TVCaptureDevice capture )
		{
			//
			// Clear any previous items
			//
			filterComboBox.Items.Clear();

			if (capture != null) 
			{
				if(capture.PropertyPages != null)
				{
					foreach(PropertyPage page in capture.PropertyPages)
					{
						filterComboBox.Items.Add(page.Name);
					}
				}
			}
		}

		private void SetupCaptureFormats()
		{
			int[][] resolution = new int[][]{ new int[] { 320, 240 },
												new int[] { 352, 240 },
												new int[] { 352, 288 },
												new int[] { 640, 240 },
												new int[] { 640, 288 },
												new int[] { 640, 480 },
												new int[] { 704, 576 },
												new int[] { 720, 240 },
												new int[] { 720, 288 },
												new int[] { 720, 480 },
												new int[] { 720, 576 } };

			captureFormats.Clear();
			for(int index = 0; index < resolution.Length; index++)
			{
				CaptureFormat format = new CaptureFormat();
				format.Width = resolution[index][0]; 
				format.Height = resolution[index][1];
				format.Description = String.Format("{0}x{1}", format.Width, format.Height);
				captureFormats.Add(format);
			}
		}

		private void frameRateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}		
		}

		private void cardComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (!acceptuserinput) return;
			SetDefaultCodecs();
			SetDefaultAudioDevice();

			if (!FillInAll()) 
			{
				MessageBox.Show("Unable to create graph for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		bool isBDACard
		{
			get
			{
				if (cardComboBox.SelectedItem!=null)
				{
					TVCaptureDevice card = (cardComboBox.SelectedItem as ComboBoxCaptureCard).CaptureDevice;
					if (card!=null)
					{
						if (card.IsBDACard )
							return true;
					}
				}
				return false;
			}
		}


		/// <summary>
		/// #MW#
		/// Fill in all the appropriate fields. The selected card is already checked as a card that
		/// is supported, so definitions might already been loaded.
		/// The createcapturedevice() method builds up a graph, without the connections in order to
		/// display the property pages belonging to the required filters.
		/// 
		/// This was done using the Capture class, which did a mix of the (My)SinkGraph class used by the
		/// TvCapture stuff. For the time being, a kind of hack is used to re-use parts of the sinkgraph
		/// code that builds the graph according to the cards definitions. In the future, a redesign should
		/// be done to remove all obsolete code and refactor the whole capture tv/radio thingy...
		/// 
		/// Depending on the cards capabilities, the rest of the fields should be enabled/disabled
		/// 
		/// Display should give the descriptive name, ie PVR150MCE, PVR350 etc.
		/// </summary>
		bool FillInAll()
		{
			bool result=true;
			try
			{
				btnRadio.Visible=false;
				//
				// Setup frame sizes
				//
				TVCaptureDevice capture = CaptureCard;

				//
				// Update controls
				//
				if(capture != null)
				{
					trackRecording.Enabled=frameSizeComboBox.Enabled = frameRateTextBox.Enabled = audioDeviceComboBox.Enabled = audioCompressorComboBox.Enabled = videoCompressorComboBox.Enabled = frameRateTextBox.Enabled = frameSizeComboBox.Enabled = audioCompressorComboBox.Enabled=comboBoxLineInput.Enabled=!CaptureCard.SupportsMPEG2;
				}
				else
				{
					trackRecording.Enabled=frameSizeComboBox.Enabled = frameRateTextBox.Enabled = audioDeviceComboBox.Enabled = audioCompressorComboBox.Enabled = videoCompressorComboBox.Enabled = frameRateTextBox.Enabled = frameSizeComboBox.Enabled = audioCompressorComboBox.Enabled=comboBoxLineInput.Enabled=false;
					if(cardComboBox.SelectedIndex!=-1) 
						if(cardComboBox.SelectedItem.ToString()=="B2C2 MPEG-2 Source")
						{
							frameSizeComboBox.Enabled =true;
						}
				}

				useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = setupButton.Enabled = cardComboBox.Text.Length > 0;

				if(capture != null)
				{
					if (capture.CreateGraph())
					{
						if (capture.Network!=NetworkType.DVBC &&
							capture.Network!=NetworkType.DVBS &&
							capture.Network!=NetworkType.DVBT)
						{
							btnRadio.Visible=true;
						}
						//
						// Clear combo box
						//
						comboBoxLineInput.Items.Clear();
						frameSizeComboBox.Items.Clear();

						//
						// Loop through available frame sizes and try to assign them to the card, if we succeed we
						// know the card supports the size.
						//
					
						SetupPropertyPages(capture);
						foreach(CaptureFormat format in captureFormats)
						{
							Size frameSize = new Size(format.Width, format.Height);
							if (capture.SupportsFrameSize(frameSize))
							{	
								//
								// Card supports the current frame size
								//
								frameSizeComboBox.Items.Add(format);
							}
						}

						IBaseFilter audioDevice=capture.AudiodeviceFilter;
						if (audioDevice!=null)
						{
							int hr=0;
							IEnumPins pinEnum;
							hr=audioDevice.EnumPins(out pinEnum);
							if( (hr == 0) && (pinEnum != null) )
							{
								pinEnum.Reset();
								IPin[] pins = new IPin[1];
								int f;
								do
								{
									// Get the next pin
									hr = pinEnum.Next( 1, pins, out f );
									if( (hr == 0) && (pins[0] != null) )
									{
										PinDirection pinDir;
										pins[0].QueryDirection(out pinDir);
										if (pinDir==PinDirection.Input)
										{
											PinInfo info;
											pins[0].QueryPinInfo(out info);
											comboBoxLineInput.Items.Add(info.name);
										}
										Marshal.ReleaseComObject( pins[0] );
									}
								}
								while( hr == 0 );
							}
						}
						
						capture.DeleteGraph();
					}
					else result=false;
				}

				//select correct audio line input
				if (comboBoxLineInput.Items.Count>0)
					comboBoxLineInput.SelectedIndex=0;

				for (int i=0; i < comboBoxLineInput.Items.Count;++i)
				{
					string input = (string)comboBoxLineInput.Items[i];
					if (capture.AudioInputPin==input)
					{
						comboBoxLineInput.SelectedIndex = i;
						break;
					}
				}

				// select the correct framesize
				if (frameSizeComboBox.Items.Count>0)
					frameSizeComboBox.SelectedIndex=0;

				for (int i=0; i < frameSizeComboBox.Items.Count;++i)
				{
					CaptureFormat fmt =(CaptureFormat)frameSizeComboBox.Items[i];
					if (m_size.Width==fmt.Width && m_size.Height==fmt.Height)
					{
						frameSizeComboBox.SelectedIndex = i;
						break;
					}
				}
				DVBSections dvbSections=new DVBSections();
				ArrayList codes=new ArrayList();
				codes=dvbSections.GetLanguageCodes();
				int n=0;
				treeView1.Nodes.Clear();
				foreach(string code in codes)
				{
					TreeNode tn=new TreeNode(dvbSections.GetLanguageFromCode(code)+" ("+code+")");
					tn.Tag=code;
					n++;
					treeView1.Nodes.Add(tn);
				}
			}
				//
				//

				//
				//
			catch(Exception ex)
			{
				Log.Write("FillInAll exception:{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
			return result;
		}

		private void setupButton_Click(object sender, System.EventArgs e)
		{
			try
			{
				if(filterComboBox.SelectedItem != null)
				{
					string propertyPageName = (string)filterComboBox.SelectedItem;
					TVCaptureDevice capture = CaptureCard;
					if(capture != null)
					{
						if (capture.CreateGraph())
						{
							if(capture.PropertyPages != null)
							{
								foreach(PropertyPage page in capture.PropertyPages)
								{
									if(propertyPageName.Equals(page.Name))
									{
										//
										// Display property page
										//
										page.Show(this);

										//
										// Save settings
										//
										//capture.SaveSettings(cardId);
										break;
									}
								}
							}

							capture.DeleteGraph();
						}
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write("btnSetup exception:{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}

		private void filterComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			setupButton.Enabled = filterComboBox.SelectedItem != null;
		}

		private void videoCompressorComboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Back)
			{
				videoCompressorComboBox.SelectedItem = null;
				videoCompressorComboBox.Text = String.Empty;
			}
		}

		private void audioCompressorComboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Back)
			{
				audioCompressorComboBox.SelectedItem = null;
				audioCompressorComboBox.Text = String.Empty;
			}    
		}

		private void EditCaptureCardForm_Load(object sender, System.EventArgs e)
		{
			bool result=FillInAll();    
			
			if (CaptureCard!=null)
			{
				string filename=String.Format(@"database\card_{0}.xml",CaptureCard.FriendlyName);
				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(filename))
				{
					
					
					comboBox1Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio1", 0);
					comboBox2Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio2", 1);
					comboBox3Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio3", 0);
					cbRgbAudio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio4", 0);

					
					comboBox1Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video1", 0);
					comboBox2Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video2", 1);
					comboBox3Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video3", 0);
					cbRgbVideo.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video4", 0);

				}
			}
			LoadDVBSSettings();
			if (!result) 
			{
				MessageBox.Show("Unable to create graph for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			if (CaptureCard!=null)
			{
				if(CaptureCard.VideoDevice!="B2C2 MPEG-2 Source")
					checkBox1.Enabled=false;
			}

			FillInDefaultRecordingPath();
		}
  
		private void audioDeviceComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (!acceptuserinput) return;
			FillInAll();        
		}

		private void trackRecording_ValueChanged(object sender, System.EventArgs e)
		{
			lblRecordingLevel.Text = String.Format("{0}%", trackRecording.Value);
		}




		public TVCaptureDevice CaptureCard
		{
			get 
			{
				if (cardComboBox.SelectedItem==null) return null;

				ComboBoxCaptureCard combo = cardComboBox.SelectedItem as ComboBoxCaptureCard;
				TVCaptureDevice card = (combo).CaptureDevice;

				Log.Write("selected {0} bda:{1} mce:{2} mpeg2:{3}", 
					card.CommercialName,card.IsBDACard,card.IsMCECard,card.SupportsMPEG2);

				
				card.MaxSizeLimit=trackBar1.Value;
				card.DeleteOnLowDiskspace=checkBoxDeleteLow.Checked;
				card.RecordingPath=tbRecordingFolder.Text;
				card.UseForRecording = useRecordingCheckBox.Checked;
				card.UseForTV	= useWatchingCheckBox.Checked;
				card.ID=CardId;
				card.Priority = (int)updownPrio.Value;
				
				if(frameSizeComboBox.SelectedItem != null)
				{
					CaptureFormat fmt = (CaptureFormat)frameSizeComboBox.SelectedItem;
					card.FrameSize = new Size(fmt.Width, fmt.Height);

					if(frameRateTextBox.Text.Length > 0)
					{
						card.FrameRate = Int32.Parse(frameRateTextBox.Text);
					}
				}

				card.VideoCompressor = videoCompressorComboBox.Text;
				card.AudioCompressor = audioCompressorComboBox.Text;
				card.AudioDevice = audioDeviceComboBox.Text;
				card.AudioInputPin = comboBoxLineInput.Text;


				card.RecordingLevel = trackRecording.Value;
				card.FriendlyName   = textBoxName.Text;

				
				if (!checkBoxHiQuality.Checked) 
				{
					card.Quality=-1;
				}
				else
				{
					if (comboBoxQuality.SelectedIndex<0 ) card.Quality=-1;
					else card.Quality=comboBoxQuality.SelectedIndex;
				}
				return card;
			}

			set
			{
				acceptuserinput=false;
				TVCaptureDevice card = value as TVCaptureDevice;

				if(card != null)
				{
					if (card.Quality < 0) 
					{
						checkBoxHiQuality.Checked=false;
					}
					else
					{
						checkBoxHiQuality.Checked=true;
						comboBoxQuality.SelectedIndex=card.Quality;
					}
					tbRecordingFolder.Text=card.RecordingPath;
					if (tbRecordingFolder.Text=="")
					{
						FillInDefaultRecordingPath();
					}
						
					UpdatePercentageLabel();	
					trackBar1.Value						= card.MaxSizeLimit;
					checkBoxDeleteLow.Checked = card.DeleteOnLowDiskspace;

					textBoxName.Text=card.FriendlyName;
					for (int i=0; i < cardComboBox.Items.Count;++i)
					{
						ComboBoxCaptureCard cd= (ComboBoxCaptureCard)cardComboBox.Items[i];
						if (card.DeviceType!=null)
						{
							if (cd.CaptureDevice.DeviceId==card.DeviceType)
							{
								if (card.VideoDeviceMoniker==cd.VideoDeviceMoniker &&
									card.VideoDevice    ==cd.CaptureDevice.CaptureName)
								{
									cardComboBox.SelectedIndex=i;
									break;
								}
							}
						}
					}

					audioDeviceComboBox.SelectedItem  = card.AudioDevice          ;
					useRecordingCheckBox.Checked = card.UseForRecording;
					useWatchingCheckBox.Checked = card.UseForTV;
					frameRateTextBox.Text = card.FrameRate.ToString();
					trackRecording.Value=card.RecordingLevel  ;

					videoCompressorComboBox.SelectedItem = card.VideoCompressor;
					audioCompressorComboBox.SelectedItem = card.AudioCompressor;
					audioDeviceComboBox.SelectedItem = card.AudioDevice;
					
					updownPrio.Value=card.Priority ;
          
					comboBoxLineInput.Text = card.AudioInputPin;
					trackRecording_ValueChanged(null,null);
					m_size=new Size(card.FrameSize.Width,card.FrameSize.Height);

					FillInAll(); // fill all framerates & audio in types...


					comboBoxLineInput.Text = card.AudioInputPin;
					trackRecording_ValueChanged(null,null);
          
					// select the correct framesize
					for (int i=0; i < frameSizeComboBox.Items.Count;++i)
					{
						CaptureFormat fmt =(CaptureFormat)frameSizeComboBox.Items[i];
            
						if (card.FrameSize.Width==fmt.Width && card.FrameSize.Height==fmt.Height)
						{
							frameSizeComboBox.SelectedIndex = i;
							break;
						}
					}
					//select correct audio line input
					for (int i=0; i < comboBoxLineInput.Items.Count;++i)
					{
						string input = (string)comboBoxLineInput.Items[i];
						if (card.AudioInputPin==input)
						{
							comboBoxLineInput.SelectedIndex = i;
							break;
						}
					}

				}
				acceptuserinput=true;
			}
		}
		void SetDefaultAudioDevice()
		{
			ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
			if (availableAudioDevices.Count>0)
			{
				audioDeviceComboBox.SelectedItem = (string)availableAudioDevices[0];
			}
		}
		void SetDefaultCodecs()
		{
			ArrayList availableVideoCompressors = FilterHelper.GetVideoCompressors();
			ArrayList availableAudioCompressors = FilterHelper.GetAudioCompressors();

			foreach (string filter in availableVideoCompressors)
			{
				if (filter.ToLower().IndexOf("xvid")>=0)
				{
					videoCompressorComboBox.SelectedItem = filter;
					break;
				}
				if (filter.ToLower().IndexOf("divx")>=0)
				{
					videoCompressorComboBox.SelectedItem = filter;
					break;
				}
			}
			foreach (string filter in availableAudioCompressors)
			{
				if (filter.ToLower().IndexOf("mp3")>=0)
				{
					audioCompressorComboBox.SelectedItem = filter;
					break;
				}
				if (filter.ToLower().IndexOf("mpeg layer-3")>=0)
				{
					audioCompressorComboBox.SelectedItem = filter;
					break;
				}
			}
		}
		
		void LoadDVBSSettings()
		{
			if (CaptureCard==null) return;
			string filename=String.Format(@"database\card_{0}.xml",CaptureCard.FriendlyName);
			
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(Application.StartupPath+@"\MediaPortal.xml"))
			{
				checkBox1.Checked=xmlreader.GetValueAsBool("dvb_ts_cards","enablePlugins",false);
			}
			
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
			{
				lnb0MHZ.Text=xmlreader.GetValueAsInt("dvbs","LNB0",9750).ToString();
				lnb1MHZ.Text=xmlreader.GetValueAsInt("dvbs","LNB1",10600).ToString();
				lnbswMHZ.Text=xmlreader.GetValueAsInt("dvbs","Switch",11700).ToString();
				cbandMHZ.Text=xmlreader.GetValueAsInt("dvbs","CBand",5150).ToString();
				circularMHZ.Text=xmlreader.GetValueAsInt("dvbs","Circular",10750).ToString();
				useLNB1.Checked=true;
				useLNB2.Checked=xmlreader.GetValueAsBool("dvbs","useLNB2",false);
				useLNB3.Checked=xmlreader.GetValueAsBool("dvbs","useLNB3",false);
				useLNB4.Checked=xmlreader.GetValueAsBool("dvbs","useLNB4",false);

				int lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb",44);
				switch (lnbKhz)
				{
					case 0: lnbconfig1.SelectedItem="0 KHz";break;
					case 22: lnbconfig1.SelectedItem="22 KHz";break;
					case 33: lnbconfig1.SelectedItem="33 KHz";break;
					case 44: lnbconfig1.SelectedItem="44 KHz";break;
				}
				lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb2",44);
				switch (lnbKhz)
				{
					case 0: lnbconfig2.SelectedItem="0 KHz";break;
					case 22: lnbconfig2.SelectedItem="22 KHz";break;
					case 33: lnbconfig2.SelectedItem="33 KHz";break;
					case 44: lnbconfig2.SelectedItem="44 KHz";break;
				}
				lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb3",44);
				switch (lnbKhz)
				{
					case 0: lnbconfig3.SelectedItem="0 KHz";break;
					case 22: lnbconfig3.SelectedItem="22 KHz";break;
					case 33: lnbconfig3.SelectedItem="33 KHz";break;
					case 44: lnbconfig3.SelectedItem="44 KHz";break;
				}
				lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb4",44);
				switch (lnbKhz)
				{
					case 0: lnbconfig4.SelectedItem="0 KHz";break;
					case 22: lnbconfig4.SelectedItem="22 KHz";break;
					case 33: lnbconfig4.SelectedItem="33 KHz";break;
					case 44: lnbconfig4.SelectedItem="44 KHz";break;
				}

				int lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind",0);
				switch (lnbKind)
				{
					case 0: lnbkind1.SelectedItem="Ku-Band";break;
					case 1: lnbkind1.SelectedItem="C-Band";break;
					case 2: lnbkind1.SelectedItem="Circular";break;
				}
				lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind2",0);
				switch (lnbKind)
				{
					case 0: lnbkind2.SelectedItem="Ku-Band";break;
					case 1: lnbkind2.SelectedItem="C-Band";break;
					case 2: lnbkind2.SelectedItem="Circular";break;
				}
				lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind3",0);
				switch (lnbKind)
				{
					case 0: lnbkind3.SelectedItem="Ku-Band";break;
					case 1: lnbkind3.SelectedItem="C-Band";break;
					case 2: lnbkind3.SelectedItem="Circular";break;
				}
				lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind4",0);
				switch (lnbKind)
				{
					case 0: lnbkind4.SelectedItem="Ku-Band";break;
					case 1: lnbkind4.SelectedItem="C-Band";break;
					case 2: lnbkind4.SelectedItem="Circular";break;
				}
				int diseqc=xmlreader.GetValueAsInt("dvbs","diseqc",0);
				switch (diseqc)
				{
					case 0: diseqca.SelectedItem="None";break;
					case 1: diseqca.SelectedItem="Simple A";break;
					case 2: diseqca.SelectedItem="Simple B";break;
					case 3: diseqca.SelectedItem="Level 1 A/A";break;
					case 4: diseqca.SelectedItem="Level 1 B/A";break;
					case 5: diseqca.SelectedItem="Level 1 A/B";break;
					case 6: diseqca.SelectedItem="Level 1 B/B";break;

				}
				diseqc=xmlreader.GetValueAsInt("dvbs","diseqc2",0);
				switch (diseqc)
				{
					case 0: diseqcb.SelectedItem="None";break;
					case 1: diseqcb.SelectedItem="Simple A";break;
					case 2: diseqcb.SelectedItem="Simple B";break;
					case 3: diseqcb.SelectedItem="Level 1 A/A";break;
					case 4: diseqcb.SelectedItem="Level 1 B/A";break;
					case 5: diseqcb.SelectedItem="Level 1 A/B";break;
					case 6: diseqcb.SelectedItem="Level 1 B/B";break;

				}
				diseqc=xmlreader.GetValueAsInt("dvbs","diseqc3",0);
				switch (diseqc)
				{
					case 0: diseqcc.SelectedItem="None";break;
					case 1: diseqcc.SelectedItem="Simple A";break;
					case 2: diseqcc.SelectedItem="Simple B";break;
					case 3: diseqcc.SelectedItem="Level 1 A/A";break;
					case 4: diseqcc.SelectedItem="Level 1 B/A";break;
					case 5: diseqcc.SelectedItem="Level 1 A/B";break;
					case 6: diseqcc.SelectedItem="Level 1 B/B";break;

				}
				diseqc=xmlreader.GetValueAsInt("dvbs","diseqc4",0);
				switch (diseqc)
				{
					case 0: diseqcd.SelectedItem="None";break;
					case 1: diseqcd.SelectedItem="Simple A";break;
					case 2: diseqcd.SelectedItem="Simple B";break;
					case 3: diseqcd.SelectedItem="Level 1 A/A";break;
					case 4: diseqcd.SelectedItem="Level 1 B/A";break;
					case 5: diseqcd.SelectedItem="Level 1 A/B";break;
					case 6: diseqcd.SelectedItem="Level 1 B/B";break;

				}


			}
		}

		void SaveDVBSSettings()
		{
			string filename=String.Format(@"database\card_{0}.xml",CaptureCard.FriendlyName);
			// save settings
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml(Application.StartupPath+@"\MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("dvb_ts_cards","enablePlugins",checkBox1.Checked);
			}

			using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml(filename))
			{
				xmlwriter.SetValue("dvbs","LNB0",lnb0MHZ.Text);
				xmlwriter.SetValue("dvbs","LNB1",lnb1MHZ.Text);
				xmlwriter.SetValue("dvbs","Switch",lnbswMHZ.Text);
				xmlwriter.SetValue("dvbs","CBand",cbandMHZ.Text);
				xmlwriter.SetValue("dvbs","Circular",circularMHZ.Text);
				xmlwriter.SetValueAsBool("dvbs","useLNB1",true);				
				xmlwriter.SetValueAsBool("dvbs","useLNB2",useLNB2.Checked);				
				xmlwriter.SetValueAsBool("dvbs","useLNB3",useLNB3.Checked);
				xmlwriter.SetValueAsBool("dvbs","useLNB4",useLNB4.Checked);

				if(diseqca.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc",diseqca.SelectedIndex); 
				}
				if(diseqcb.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc2",diseqcb.SelectedIndex); 
				}
				if(diseqcc.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc3",diseqcc.SelectedIndex); 
				}
				if(diseqcd.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc4",diseqcd.SelectedIndex); 
				}
				if (lnbconfig1.SelectedIndex>=0)
				{
					switch (lnbconfig1.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb",44); break;
					}
				}
				if (lnbconfig2.SelectedIndex>=0)
				{
					switch (lnbconfig2.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb2",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb2",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb2",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb2",44); break;
					}
				}
				if (lnbconfig3.SelectedIndex>=0)
				{
					switch (lnbconfig3.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb3",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb3",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb3",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb3",44); break;
					}
				}
				if (lnbconfig4.SelectedIndex>=0)
				{
					switch (lnbconfig4.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb4",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb4",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb4",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb4",44); break;
					}
				}

				if (lnbkind1.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind",lnbkind1.SelectedIndex); 
				}
				if (lnbkind2.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind2",lnbkind2.SelectedIndex); 
				}
				if (lnbkind3.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind3",lnbkind3.SelectedIndex); 
				}
				if (lnbkind4.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind4",lnbkind4.SelectedIndex); 
				}
			}
		}

		private void tabPage4_Click(object sender, System.EventArgs e)
		{
		
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			// save settings for card
			if (CaptureCard!=null)
			{
				if(CaptureCard.VideoDevice=="B2C2 MPEG-2 Source")
				{
					string filename=String.Format(@"database\card_{0}.xml",CaptureCard.FriendlyName);
					// save settings for get the filename in mp.xml
					using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml(Application.StartupPath+@"\MediaPortal.xml"))
					{
						xmlwriter.SetValue("dvb_ts_cards","filename",filename);
					}
				}
			}

			AnalogTVTuningForm dialog = new AnalogTVTuningForm();
			ITuning tuning=GraphFactory.CreateTuning(CaptureCard);
			if (tuning!=null)
			{
				dialog.Tuning=tuning;
				dialog.Card=CaptureCard;
				dialog.ShowDialog(this);
			}
			else
			{
				MessageBox.Show("This device does not support auto tuning", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void button17_Click(object sender, System.EventArgs e)
		{
			
			foreach(TreeNode tn in treeView5.Nodes)
			{
				tn.Checked=true;
			}
		}

		private void button18_Click(object sender, System.EventArgs e)
		{
		
			foreach(TreeNode tn in treeView5.Nodes)
			{
				tn.Checked=false;
			}
		}
		int CountSelectedNodes()
		{
			int retCount=0;
			foreach(TreeNode tn in treeView5.Nodes)
			{
				if(tn.Checked==true)
					retCount++;
			}

			return retCount;

		}

		private void button15_Click(object sender, System.EventArgs e)
		{
			string langCodes="";
			TVChannel oldChannel=new TVChannel();

			foreach(TreeNode tn in treeView1.Nodes)
			{
				if(tn.Checked==true)
					langCodes+=((string)tn.Tag)+"/";
			}
			Log.Write("epg-grabbing: start grab for languages {0}",langCodes);
			try
			{
				progressBar2.Step=1;
				progressBar2.Maximum=CountSelectedNodes();
				progressBar2.Minimum=0;
				progressBar2.Value=0;
				m_stopEPGGrab=false;
				button16.Enabled=true;
				button15.Enabled=false;
				int counter=0;
				TVCaptureDevice capture = CaptureCard;
				if(capture == null)
				{
					MessageBox.Show("Unable to create directshow graph", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				if (!capture.CreateGraph())
				{
					MessageBox.Show("Unable to create directshow graph", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				DVBEPG	ssEPG;
				if(capture.VideoDevice!="B2C2 MPEG-2 Source")
					ssEPG=new DVBEPG((int)DVBEPG.EPGCard.BDACards, capture.Network);
				else
					ssEPG=new DVBEPG((int)DVBEPG.EPGCard.TechnisatStarCards);
				ssEPG.Languages=langCodes;
				GUIGraphicsContext.VideoWindow = new Rectangle(337,90,110,110);
				IntPtr prevFormPtr=GUIGraphicsContext.ActiveForm;
				Form prevForm=GUIGraphicsContext.form;
				GUIGraphicsContext.ActiveForm = this.Handle;
				GUIGraphicsContext.form=this;
				capture.View=true;
				ArrayList tvchannels=new ArrayList();
				foreach(TreeNode tn in treeView5.Nodes)
				{
					if(tn.Checked==true)
					{
						TVChannel ch=(TVChannel)tn.Tag;
						tvchannels.Add(ch);
					}
				}

				GC.Collect();
				progressBar2.Value=progressBar2.Minimum;
				foreach(TVChannel ch in tvchannels)
				{
					if(m_stopEPGGrab==true)
						break;
					chName.Text=ch.Name;
					capture.TVChannel=ch.Name;
					if (!capture.SignalPresent()) continue;
					ssEPG.LabelChannels=channelsCount;
					ssEPG.LabelTitles=addsToDB;
					int dvbAdds=ssEPG.GetEPG(capture.Mpeg2DataFilter,0/*all services*/);
					if(dvbAdds<0) dvbAdds=0;
					counter+=dvbAdds;
					label27.Text=String.Format("{0}",counter);
					try
					{
						progressBar2.Value+=1;
					}
					catch
					{
						progressBar2.Value=progressBar2.Minimum;
					}
					Application.DoEvents();
					Application.DoEvents();
				}
				Log.Write("EPG Stopped...");
				chName.Text="Stopped";
				progressBar2.Value=0;
				button15.Enabled=true;
				button16.Enabled=false;
				label27.Text="0";
				capture.View=false;
				capture.DeleteGraph();
				capture=null;
				GC.Collect();
				GUIGraphicsContext.ActiveForm = prevFormPtr;
				GUIGraphicsContext.form=prevForm;
				TVDatabase.RemoveOverlappingPrograms();
			}
			catch(Exception ex)
			{
				Log.Write("epggrabber exception:{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}

		private void button16_Click(object sender, System.EventArgs e)
		{
			m_stopEPGGrab=true;
			button16.Enabled=false;
			chName.Text="Stopping. DONT close Window until 'Stopped' appears!";
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			try
			{
				if(tabControl1.SelectedTab==tabPage4)
				{	
					
					TVCaptureDevice capture = CaptureCard;
					if (capture==null)
					{
						MessageBox.Show("Please fill in the capture card details first", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						tabControl1.SelectedIndex=0;
						return;
					}
					
					if (!capture.CreateGraph())
					{
						MessageBox.Show("Unable to create directshow graph", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						tabControl1.SelectedIndex=0;
						return;
					}
					if (capture.Network != NetworkType.DVBS)
					{
						capture.DeleteGraph();
						MessageBox.Show("This section is only for DVB-S cards", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						tabControl1.SelectedIndex=0;
						return;
					}
					capture.DeleteGraph();
				}

				if(tabControl1.SelectedTab==tabPage6)
				{	
					TVCaptureDevice capture = CaptureCard;
					if (capture==null)
					{
						MessageBox.Show("Please fill in the capture card details first", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						tabControl1.SelectedIndex=0;
						return;
					}
					if (!capture.CreateGraph())
					{
						MessageBox.Show("Unable to create directshow graph", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						tabControl1.SelectedIndex=0;
						return;
					}
					if (capture.Network != NetworkType.DVBC &&
						capture.Network != NetworkType.DVBS &&
						capture.Network != NetworkType.DVBT)
					{
						capture.DeleteGraph();
						MessageBox.Show("EPG grabber only supports DVB-T / DVB-S / DVB-T cards", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						tabControl1.SelectedIndex=0;
						return;
					}
					capture.DeleteGraph();
					ArrayList channelList=new ArrayList();
					TVDatabase.GetChannels(ref channelList);
					treeView5.Nodes.Clear();
					
					foreach (TVChannel chan in channelList)
					{
						TreeNode node=new TreeNode(chan.Name);
						node.Tag=chan;
						node.Checked=true;
						treeView5.Nodes.Add(node);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write("Switch tabs exception:{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}

		private void groupBox2_Enter(object sender, System.EventArgs e)
		{
		
		}

		private void diseqca_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void button5_Click(object sender, System.EventArgs e)
		{
		
		}

		private void diseqcb_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		private void diseqcc_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		private void diseqcd_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			foreach(TreeNode tn in treeView1.Nodes)
				tn.Checked=true;
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			foreach(TreeNode tn in treeView1.Nodes)
				tn.Checked=false;
		}


		private void trackBar1_ValueChanged(object sender, System.EventArgs e)
		{
			UpdatePercentageLabel();
		}

		void UpdatePercentageLabel()
		{
			if (tbRecordingFolder.Text.Length<=0) return;

			try
			{
				string cmd=String.Format( "win32_logicaldisk.deviceid=\"{0}:\"" , tbRecordingFolder.Text[0]);
				using (ManagementObject disk = new ManagementObject(cmd))
				{
					disk.Get();
					long diskSize=Int64.Parse(disk["Size"].ToString());
					long size= (long) ( ((float)diskSize) * ( ((float)trackBar1.Value) / 100f ));
					labelPercent.Text=String.Format("{0}% {1}", trackBar1.Value, Util.Utils.GetSize(size));
				}
			}
			catch(Exception){}
		}

		private void buttonBrowse_Click_1(object sender, System.EventArgs e)
		{
		
			using(FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where recordings should be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = tbRecordingFolder.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					tbRecordingFolder.Text = folderBrowserDialog.SelectedPath;
					UpdatePercentageLabel();
				}
			}
		}

		private void btnRadio_Click(object sender, System.EventArgs e)
		{
			TVCaptureDevice dev=CaptureCard;
			if (dev==null) return;
			if (dev.CreateGraph())
			{
				if (dev.Network==NetworkType.DVBC ||
					dev.Network==NetworkType.DVBS ||
					dev.Network==NetworkType.DVBT) return;
				dev.DeleteGraph();
			}

			RadioAutoTuningForm dialog = new RadioAutoTuningForm(dev);
			dialog.ShowDialog(this);
		}


		private void tabPage6_Click(object sender, System.EventArgs e)
		{
		
		}

	}
	public class CaptureFormat
	{
		public int Width;
		public int Height;
		public string Description;

		public override string ToString()
		{
			return Description;
		}
	}
}
public class ComboBoxCaptureCard
{
	private TVCaptureDevice _mCaptureDevice;
	private int m_iNumber=1;
	private int m_iMax=1;

	public ComboBoxCaptureCard(TVCaptureDevice pCaptureDevice)
	{
		this._mCaptureDevice = pCaptureDevice;
	}

	public TVCaptureDevice CaptureDevice
	{
		get {return _mCaptureDevice;}
	}

	public string VideoDeviceMoniker
	{
		get {return _mCaptureDevice.VideoDeviceMoniker;}
	}

	public string CaptureName
	{
		get {return _mCaptureDevice.CaptureName;}
	}
	public int Number
	{
		get {return m_iNumber;}
		set {m_iNumber=value;}
	}
	public int MaxCards
	{
		get {return m_iMax;}
		set {m_iMax=value;}
	}

	// Display a more readable name by adding the commercial name of the card to the capture device
	public override string ToString()
	{
			return String.Format("{1} ({2})",m_iNumber,CaptureName,_mCaptureDevice.CommercialName );
	}

	public override bool Equals(object obj)
	{
//		return this.ToString().Equals((obj as ComboBoxCaptureCard).ToString());
		return VideoDeviceMoniker.Equals((obj as ComboBoxCaptureCard).VideoDeviceMoniker);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
