/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DShowNET;

namespace MediaPortal.Player 
{
  public class TStreamBufferPlayer9 : BaseTStreamBufferPlayer
  {

		[ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
			class MPEG2Demultiplexer {}
		VMR9Util Vmr9 = null;
    public TStreamBufferPlayer9()
    {
    }
		protected override void OnInitialized()
		{
			if (Vmr9!=null)
			{
				Vmr9.Enable(true);
				m_bUpdateNeeded=true;
				SetVideoWindow();
			}
		}
		public override void SetVideoWindow()
		{
			if (GUIGraphicsContext.IsFullScreenVideo!= m_bFullScreen)
			{
				m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
				m_bUpdateNeeded=true;
			}

			if (!m_bUpdateNeeded) return;
      
			m_bUpdateNeeded=false;
			m_bStarted=true;

		}


    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
		  Speed=1;	
			//Log.Write("TStreamBufferPlayer9: GetInterfaces()");
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
			
	//		Log.Write("TStreamBufferPlayer9: switch to fullscreen mode");
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);
//Log.Write("TStreamBufferPlayer9: build graph");

      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
				Vmr9= new VMR9Util("mytv");
				Vmr9.AddVMR9(graphBuilder);			
				Vmr9.Enable(false);	
				int hr;

				MPEG2Demultiplexer m_MPEG2Demuxer=null;
				IBaseFilter m_mpeg2Multiplexer=null;
				Log.WriteFile(Log.LogType.Capture,"mpeg2:add new MPEG2 Demultiplexer to graph");
				try
				{
					m_MPEG2Demuxer=new MPEG2Demultiplexer();
					m_mpeg2Multiplexer = (IBaseFilter) m_MPEG2Demuxer;
				}
				catch(Exception){}
				//m_mpeg2Multiplexer = DirectShowUtil.AddFilterToGraph(m_graphBuilder,"MPEG-2 Demultiplexer");
				if (m_mpeg2Multiplexer==null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to create mpeg2 demuxer");
					return false;
				}
				 hr=graphBuilder.AddFilter(m_mpeg2Multiplexer,"MPEG-2 Demultiplexer");
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to add mpeg2 demuxer to graph:0x{0:X}",hr);
					return false;
				}


				bufferSource = new MPTransportStreamReader();
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "MP TS Reader");
		
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				 hr = fileSource.Load(filename, IntPtr.Zero);
				// add preferred video & audio codecs
				string strVideoCodec="";
        string strAudioCodec="";
				string strAudioRenderer="";
        bool   bAddFFDshow=false;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
					strAudioRenderer=xmlreader.GetValueAsString("mytv","audiorenderer","");
					string strValue=xmlreader.GetValueAsString("mytv","defaultar","normal");
					if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
					if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
					if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
					if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
					if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
					if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;

				}
				IBaseFilter videoCodec=null;
				if (strVideoCodec.Length>0) 
					videoCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (strAudioRenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudioRenderer,false);
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				// render output pins of SBE
        DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
				m_mediaSeeking = graphBuilder as IMediaSeeking;
        
				hasVideo=true;
				if ( !Vmr9.IsVMR9Connected )
				{
					hasVideo=false;
					if (videoCodec!=null)
						graphBuilder.RemoveFilter(videoCodec);
					if (Vmr9!=null)
						Vmr9.RemoveVMR9();
					Vmr9=null;
				}

				return true;
      }
      catch( Exception  ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }




    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
		{
			Cleanup();
		}

		void Cleanup()
		{
				if (graphBuilder==null) return;

        int hr;
        //Log.Write("TStreamBufferPlayer9:cleanup DShow graph {0}",GUIGraphicsContext.InVmr9Render);
        try 
        {
					if(Vmr9!=null)
						Vmr9.Enable(false);

					int counter=0;
					while (GUIGraphicsContext.InVmr9Render)
					{
						counter++;
						System.Threading.Thread.Sleep(1);
						if (counter >200) break;
					}

          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
						mediaCtrl=null;
          }
          
					mediaEvt = null;

					if(Vmr9!=null)
					{
						Vmr9.RemoveVMR9();
						Vmr9.Release();
						Vmr9=null;
					}
					
					basicAudio	= null;
					basicVideo	= null;
					m_mediaSeeking=null;
					bufferSource=null;
					videoWin=null;

					DsUtils.RemoveFilters(graphBuilder);

					if( rotCookie != 0 )
						DsROT.RemoveGraphFromRot( ref rotCookie );
					rotCookie=0;

					if( graphBuilder != null )
					{
						while ((hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
						graphBuilder = null;
					}

				GUIGraphicsContext.form.Invalidate(true);
				m_state = PlayState.Init;
				GC.Collect();GC.Collect();GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }

//Log.Write("TStreamBufferPlayer9:switch");
			//switch back to directx windowed mode
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
			GUIWindowManager.SendMessage(msg);
			Log.Write("TStreamBufferPlayer9:cleanup done");
		}

    protected override void OnProcess()
		{
			if (Vmr9!=null)
			{
				m_iVideoWidth=Vmr9.VideoWidth;
				m_iVideoHeight=Vmr9.VideoHeight;
			}
			if((GUIGraphicsContext.Vmr9Active && Vmr9!=null))
			{
				Vmr9.Process();
				if (GUIGraphicsContext.Vmr9FPS < 1f)
				{
					Vmr9.Repaint();// repaint vmr9
				}
			}
    }


  }
}
