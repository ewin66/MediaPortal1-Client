using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for FormOSD.
	/// </summary>
	public class FormOSD : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		int m_iLastMousePositionX = 0;
		int m_iLastMousePositionY = 0;

		Timer   Clock;
		public FormOSD()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
#if DEBUG
			//this.TopMost = false;
#endif
      SetPositionAndSize();
      GUIGraphicsContext.form.LocationChanged +=new EventHandler(form_LocationChanged);
      GUIGraphicsContext.form.Resize += new EventHandler(form_Resize);
      Clock = new Timer();
			Clock.Interval=250;
      Clock.Start();
      
			Clock.Tick+=new EventHandler(Timer_Tick);

			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
		}

		private void OnMouseDown(object sender, MouseEventArgs  e)
		{
			Action action;

			// first move mouse
			float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
			float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
			float x = (fX * ((float)m_iLastMousePositionX)) - GUIGraphicsContext.OffsetX;
			float y = (fY * ((float)m_iLastMousePositionY)) - GUIGraphicsContext.OffsetY; ;
			action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
			GUIGraphicsContext.OnAction(action);

			// right mouse button=back
			if (e.Button == MouseButtons.Right)
			{
				Key key = new Key(0, (int)Keys.Escape);
				action = new Action();
				if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
				{
					GUIGraphicsContext.OnAction(action);
					return;
				}
				return;
			}

			//middle mouse button=Y
			if (e.Button == MouseButtons.Middle)
			{
				Key key = new Key('y',0);
				action = new Action();
				if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
				{
					GUIGraphicsContext.OnAction(action);
					return;
				}
			}
	
			if (e.Button == MouseButtons.Left)
			{			
				action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, x, y);
			}

			action.MouseButton = e.Button;
			action.SoundFileName = "click.wav";

			GUIGraphicsContext.OnAction(action);
		}

		private void OnMouseMove(object sender, MouseEventArgs  e)
		{
			if (m_iLastMousePositionX != e.X || m_iLastMousePositionY != e.Y)
			{
				m_iLastMousePositionX = e.X;
				m_iLastMousePositionY = e.Y;

				float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
				float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
				float x = (fX * ((float)e.X)) - GUIGraphicsContext.OffsetX;
				float y = (fY * ((float)e.Y)) - GUIGraphicsContext.OffsetY; ;
				GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
				if (window != null)
				{
					Action action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
					action.MouseButton = e.Button;
					GUIGraphicsContext.OnAction(action);       
				}
			}
		}


    
		/// <summary>
		/// Set position & size of the OSD form
		/// based on position&size of the main form
		/// </summary>
		void SetPositionAndSize()
		{
			if (GUIGraphicsContext.form.FormBorderStyle != System.Windows.Forms.FormBorderStyle.None)
			{
				//There are 2 different "size" members in a Form: 
				//ClientSize and Size. Size is the total size including borders, 
				//whereas ClientSize is just the drawable area. For a graphical application the focus is typically on the drawable area
				int iOffX=GUIGraphicsContext.form.Size.Width-GUIGraphicsContext.form.ClientSize.Width;
				int iOffY=GUIGraphicsContext.form.Size.Height-GUIGraphicsContext.form.ClientSize.Height;
				Bounds=GUIGraphicsContext.form.Bounds;
				Location = new System.Drawing.Point(
					(iOffX/2)+GUIGraphicsContext.form.Location.X,
					(iOffY-(iOffX/2))+GUIGraphicsContext.form.Location.Y);

				ClientSize = GUIGraphicsContext.form.ClientSize;
			}
			else
			{
				// Location = GUIGraphicsContext.form.Location;
				// Bounds=GUIGraphicsContext.form.Bounds;
				// ClientSize = GUIGraphicsContext.form.ClientSize;
				Location = new System.Drawing.Point(1,1);
				Bounds=new Rectangle(1,1,GUIGraphicsContext.form.Bounds.Width-2,GUIGraphicsContext.form.Bounds.Height-2);
				ClientSize = new Size(GUIGraphicsContext.form.Bounds.Width-2, GUIGraphicsContext.form.Bounds.Height-2);
          
			}
		}

		/// <summary>
		/// Timer callback
		/// check if OSD needs an update, ifso invalidate the form
		/// so OnPaint() gets called
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eArgs"></param>
		public void Timer_Tick(object sender,EventArgs eArgs)
		{
			if(sender==Clock)
			{
				if (Focused)
				{
					Debug.WriteLine("set focus->main form");
					GUIGraphicsContext.form.Focus();
				}

				if (g_Player.Playing && GUIGraphicsContext.IsFullScreenVideo)
				{
					GUIVideoFullscreen win=(GUIVideoFullscreen)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					if ( win.NeedUpdate() )
					{ 
						Invalidate();
						/*
						int iDC=g_Player.GetHDC();
						if (iDC==0)
						{
							Invalidate();
						}
						else
						{
							using (Graphics g = Graphics.FromHdc(new IntPtr(iDC)))
							{
								GUIGraphicsContext.graphics=g;
								win.RenderForm();
								GUIGraphicsContext.graphics=null;
							}
							g_Player.ReleaseHDC(iDC);
						}*/
					}
				}
			}
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
				if (Clock!=null)
				{
					Clock.Dispose();
					Clock=null;
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
			// 
			// FormOSD
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(656, 93);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormOSD";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "FormOSD";
			this.TopMost = true;
			this.TransparencyKey = System.Drawing.Color.Black;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormOSD_Closing);
			this.Load += new System.EventHandler(this.FormOSD_Load);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
		}
		#endregion


		/// <summary>
		/// OnPaintBackground() 
		/// Dont do anything since otherwise it causes flickering
		/// </summary>
		/// <param name="pevent"></param>
		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			//Trace.WriteLine("osd:OnPaintBackground");


			//pevent.Graphics.Clear(this.BackColor);
		}

		/// <summary>
		/// OnPaint handler
		/// Just ask the the fullscreen window 2 render itself on this form
		/// for that we need to set GUIGraphicsContext.Graphics to a valid graphics device
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			//Trace.WriteLine("osd:OnPaint "+DateTime.Now.ToLongTimeString() );
			GUIGraphicsContext.graphics=e.Graphics;
			GUIVideoFullscreen win=(GUIVideoFullscreen)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
			win.RenderForm(0);
			GUIGraphicsContext.graphics=null;
		}

		/// <summary>
		/// OSD form location changed.Set position & size the same as the main form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void form_LocationChanged(object sender, EventArgs e)
		{
			//Trace.WriteLine("osd:location changed");
			SetPositionAndSize();
		}

		/// <summary>
		/// OSD Form is resizing. Set position & size the same as the main form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void form_Resize(object sender, EventArgs e)
		{
			//Trace.WriteLine("osd:size changed");
			SetPositionAndSize();
		}

		/// <summary>
		/// Form is going 2b presented.
		/// set the focus back to the main form (so that receives the keyboard input)
		/// and set the position & size of the OSD form to same size as main form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormOSD_Load(object sender, System.EventArgs e)
		{
			//Trace.WriteLine("osd:open"+DateTime.Now.ToLongTimeString() );
			SetPositionAndSize();
			GUIGraphicsContext.form.Focus();
		}

		/// <summary>
		/// Form is closing. Just stop the timer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormOSD_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//Trace.WriteLine("osd:close"+DateTime.Now.ToLongTimeString() );
			if (Clock!=null)
			{
				Clock.Stop();
				Clock.Dispose();
				Clock=null;
			}
      
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			//Debug.WriteLine("osd:onkeydown");
			base.OnKeyDown (e);
		}
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			//Debug.WriteLine("osd:onkeypress");
			base.OnKeyPress (e);
		}
		protected override void OnKeyUp(KeyEventArgs e)
		{
			//Debug.WriteLine("osd:onkeyup");
			base.OnKeyUp (e);
		}

	}
}
