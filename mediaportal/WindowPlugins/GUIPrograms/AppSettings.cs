using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using ProgramsDatabase;
using Programs.Utils;


namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for AppSettings.
	/// </summary>
	public class AppSettings : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.	
		/// </summary>
		private System.ComponentModel.Container components = null;
		protected System.Windows.Forms.OpenFileDialog dialogFile;
		protected System.Windows.Forms.FolderBrowserDialog dialogFolder;
		protected ProgramConditionChecker m_Checker = new ProgramConditionChecker();

		public event EventHandler OnFileEditClick;
		public event EventHandler OnRefreshClick;
		public event EventHandler OnUpClick;
		public event EventHandler OnDownClick;


		public AppSettings()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.dialogFile = new System.Windows.Forms.OpenFileDialog();
			this.dialogFolder = new System.Windows.Forms.FolderBrowserDialog();
			// 
			// AppSettings
			// 
			this.Name = "AppSettings";

		}
		#endregion

		public virtual bool AppObj2Form(AppItem curApp)
		{
			// virtual!
			return true;
		}

		public virtual bool Applist2Form(Applist apps)
		{
			// virtual!
			return true;
		}

		public virtual void Form2AppObj(AppItem curApp)
		{
			// virtual!
		}

		public virtual bool EntriesOK(AppItem curApp)
		{
			return true;
		}

		protected void FileButtonClicked()
		{
			if (this.OnFileEditClick != null)
			{
				OnFileEditClick(this, null);
			}
		}

		protected void RefreshButtonClicked()
		{
			if (this.OnRefreshClick != null)
			{
				OnRefreshClick(this, null);
			}
		}

		protected void UpButtonClicked()
		{
			if (this.OnUpClick != null)
			{
				OnUpClick(this, null);
			}
		}

		protected void DownButtonClicked()
		{
			if (this.OnDownClick != null)
			{
				OnDownClick(this, null);
			}
		}

	}
}
