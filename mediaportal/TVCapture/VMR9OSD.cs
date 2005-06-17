using System;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;
using MediaPortal.GUI.Library;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Util;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung f�r VMR9OSD.
	/// </summary>
	public class VMR9OSD
	{
		#region constructor / destructor
		public VMR9OSD()
		{
			//
			// TODO: F�gen Sie hier die Konstruktorlogik hinzu
			//
			ReadSkinFile();
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(System.Windows.Forms.Application.StartupPath+@"\MediaPortal.xml"))
			{
				int alpha=xmlreader.GetValueAsInt("vmr9OSDSkin","alphaValue",10);
				if(alpha>0)
					m_renderOSDAlpha=(float)alpha/10;
				else 
					m_renderOSDAlpha=0.8f;// default
			}
		}
		#endregion
		// structs
		#region structs / enums
		enum OSD
		{
			ZapOSD=1,
			ZapList,
			VolumeOSD,
			CurrentTVShowInfo,
			OtherBitmap,
			None
		}

		struct OSDSkin
		{
			public string[] rects;
			public string chName;
			public string chNow;
			public string chNext;
			public string chProgress;
			public string sigBarQ;
			public string sigBarL;
			public string time;
			public string mute;
			public string chLogo;
			public string bg;
		}
		struct OSDChannelList
		{
			public string baseRect;
		}
		#endregion
		//
		#region globals
		DateTime m_timeDisplayed=DateTime.Now;
		bool m_muteState=false;
		string m_mediaPath=System.Windows.Forms.Application.StartupPath+@"\osdskin-media\";
		TVChannel m_actualChannel=null;
		int m_channelSNR=0;
		int m_channelLevel=0;
		// osd skin
		OSDSkin m_osdSkin;
		OSDChannelList m_osdChannels;
		bool m_bitmapIsVisible=false;
		int m_timeout=0;
		OSD m_osdRendered=OSD.None;
		Bitmap m_volumeBitmap;
		Bitmap m_muteBitmap;
		float m_renderOSDAlpha=0.8f;

		#endregion

		#region properties
		public bool Mute
		{
			get{return m_muteState;}
			set{m_muteState=value;}
		}
		#endregion

		#region osd render functions
		public void RenderUserInformation(string header,params string[] textLines)
		{
			if(textLines==null)
				return;
			if(textLines.Length<1)
				return;

			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;
			m_timeout=10000;// ten seconds
			m_osdRendered=OSD.CurrentTVShowInfo;
			// render list
			Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			if(bm==null || gr==null || m_osdChannels.baseRect==null)
				return;

			int x=60;
			int y=20;
			string[] seg=m_osdChannels.baseRect.Split(new char[]{':'});
			if(seg==null) return;
			if(seg.Length!=7) return;
			if(seg[0]!="nsrect") return;
			Color headColor=GetColor(seg[1]);
			Color nBoxColor=GetColor(seg[2]);
			Color sBoxColor=GetColor(seg[3]);
			Color textColor=GetColor(seg[4]);

			Font drawFont=new Font(seg[5],Convert.ToInt16(seg[6]));
			SolidBrush textBrush=new SolidBrush(textColor);
			RectangleF layoutRect=new RectangleF(x,y,gWidth-(x*2),gHeight-(y*2));
			//
			SizeF textSize=gr.MeasureString("AAA",drawFont);
			int textHeight=2+((int)textSize.Height);

			gr.FillRectangle(new SolidBrush(headColor),x,y,gWidth-(2*x),textHeight);
			gr.DrawString(header,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			int lineCount=textLines.Length;
			if(lineCount*textHeight>(gHeight-((y*2)+textHeight)))
				lineCount=(gHeight-((y*2)+textHeight))/textHeight;

			layoutRect.Y+=textHeight;
			layoutRect.Height-=textHeight*2;
			int pos=y+textHeight;

			for(int i=0;i<lineCount;i++)
			{
				gr.FillRectangle(new SolidBrush(nBoxColor),x,pos,gWidth-(2*x),textHeight);
				gr.DrawString(textLines[i],drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				pos+=textHeight;
				layoutRect.Y+=textHeight;
				if(pos>=(gHeight-(y*2))-textHeight)
					break;
			}
			m_bitmapIsVisible=false;
			SaveBitmap(bm,true,true,m_renderOSDAlpha);
			bm.Dispose();
			gr.Dispose();
			drawFont.Dispose();
			textBrush.Dispose();

		}
		public void RenderCurrentShowInfo()
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;

			if(m_osdRendered==OSD.CurrentTVShowInfo)
			{
				m_bitmapIsVisible=true;
				HideBitmap();
				m_timeout=0;
				return;
			}
			if(m_actualChannel==null)
				return;

			TVChannel channel=m_actualChannel;
			TVProgram prog=channel.GetProgramAt(DateTime.Now);
			
			if(prog==null)
				return;
			
			m_timeout=10000;// ten seconds
			m_osdRendered=OSD.CurrentTVShowInfo;
			// render list
			Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			int x=60;
			int y=20;
			if(bm==null || gr==null || m_osdChannels.baseRect==null)
				return;
			
			string[] seg=m_osdChannels.baseRect.Split(new char[]{':'});
			if(seg==null) return;
			if(seg.Length!=7) return;
			if(seg[0]!="nsrect") return;
			Color headColor=GetColor(seg[1]);
			Color nBoxColor=GetColor(seg[2]);
			Color sBoxColor=GetColor(seg[3]);
			Color textColor=GetColor(seg[4]);
			//
			Font drawFont=new Font(seg[5],Convert.ToInt16(seg[6]));
			SolidBrush textBrush=new SolidBrush(textColor);
			RectangleF layoutRect=new RectangleF(x,y,gWidth-(x*2),gHeight-(y*2));
			//
			SizeF textSize=gr.MeasureString("AAA",drawFont);
			int textHeight=2+((int)textSize.Height);

			string headerText=String.Format("{0}: {1} ({2}-{3})",channel.Name,prog.Title,prog.StartTime.ToShortTimeString(),prog.EndTime.ToShortTimeString());

			gr.FillRectangle(new SolidBrush(headColor),x,y,gWidth-(2*x),textHeight);
			gr.DrawString(headerText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			layoutRect.Y+=textHeight;
			layoutRect.Height-=textHeight*2;
			// draw
			gr.FillRectangle(new SolidBrush(nBoxColor),layoutRect);
			layoutRect.Width-=20;// ten pixel offset
			layoutRect.X+=10;
			gr.DrawString(prog.Description,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			// display and release
			m_bitmapIsVisible=false;
			SaveBitmap(bm,true,true,m_renderOSDAlpha);
			bm.Dispose();
			gr.Dispose();
			drawFont.Dispose();
			textBrush.Dispose();
			m_timeDisplayed=DateTime.Now;
		}

		public void RenderChannelList(TVGroup group,string currentChannel)
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;

			if(group==null)
				return;
			if(group.tvChannels.Count<2)
				return;
			int positionActChannel=0;
			int counter=0;
			bool logosFound=false;
			m_timeout=10000;
			m_osdRendered=OSD.ZapList;
			
			foreach(TVChannel chan in group.tvChannels)
			{
				string tvlogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);				
				if(System.IO.File.Exists(tvlogo))
				{
					logosFound=true;
				}
				if(chan.Name==currentChannel)
				{
					positionActChannel=counter;
					//break;
				}
				counter++;
			}

			// render list
			Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			int x=60;
			int y=20;
			if(bm==null || gr==null || m_osdChannels.baseRect==null)
				return;
			
			string[] seg=m_osdChannels.baseRect.Split(new char[]{':'});
			if(seg==null) return;
			if(seg.Length!=7) return;
			if(seg[0]!="nsrect") return;
			Color headColor=GetColor(seg[1]);
			Color nBoxColor=GetColor(seg[2]);
			Color sBoxColor=GetColor(seg[3]);
			Color textColor=GetColor(seg[4]);
			//
			Font drawFont=new Font(seg[5],Convert.ToInt16(seg[6]));
			SolidBrush textBrush=new SolidBrush(textColor);
			RectangleF layoutRect=new RectangleF(x,y,gWidth-(x*2),gHeight-(y*2));
			//
			SizeF textSize=gr.MeasureString("AAA",drawFont);
			int textHeight;
			if(logosFound)
			{
				textHeight=50;
				layoutRect.X+=50;
				layoutRect.Width-=100;
			}
			else
				textHeight=2+((int)textSize.Height);


			string headText=group.GroupName;
			
			gr.FillRectangle(new SolidBrush(headColor),x,y,gWidth-(2*x),textHeight);
			gr.DrawString(headText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			layoutRect.Y+=textHeight;
			int yMax=gHeight-(y*2);
			int channelCount=yMax/textHeight;
			channelCount--;
			int pos=y+textHeight;
			int startAt=positionActChannel-(channelCount/2);
			Log.Write("start list at={0} position={1}",startAt,positionActChannel);
			// draw
			if(group.tvChannels.Count<channelCount || positionActChannel<(channelCount/2))
				startAt=0;
			for(int i=startAt;i<group.tvChannels.Count;i++)
			{
				// stop render / continue
				if(i<0) break;
				if(i>positionActChannel+channelCount) continue;
				if(i>=group.tvChannels.Count) break;
				TVChannel chan=(TVChannel)group.tvChannels[i];
				if(chan==null) break;

				TVProgram prog=chan.GetProgramAt(DateTime.Now);
				string channelText="";
				if(prog!=null)
					channelText=chan.Name+" "+"\""+prog.Title+"\"";
				else
					channelText=chan.Name;

				if(chan.Name==currentChannel)
				{
					gr.FillRectangle(new SolidBrush(sBoxColor),x,pos,gWidth-(2*x),textHeight);
					gr.DrawString(channelText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				}
				else
				{
					gr.FillRectangle(new SolidBrush(nBoxColor),x,pos,gWidth-(2*x),textHeight);
					gr.DrawString(channelText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				}
				if(logosFound==true)
				{
					string tvlogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);				
					if(System.IO.File.Exists(tvlogo))
					{
						Bitmap logo=new Bitmap(tvlogo);
						Util.BitmapResize.Resize(ref logo,48,48,true,true);
						gr.FillRectangle(new SolidBrush(Color.FromArgb(144,144,144)),x,pos+1,48,48);
						gr.DrawImage(logo,x,pos+1,48,48);
						logo.Dispose();
					}
				}

				pos+=textHeight;
				layoutRect.Y+=textHeight;

				if(pos>=yMax-textHeight)
					break;
			}
			m_bitmapIsVisible=false;
			SaveBitmap(bm,true,true,m_renderOSDAlpha);
			bm.Dispose();
			gr.Dispose();
			drawFont.Dispose();
			textBrush.Dispose();
	
		}
		public void RenderVolumeOSD()
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;
			int max;
			int min;
			int currentVolume=AudioMixerHelper.GetMinMaxVolume(out min,out max);
			int volume=0;
			if(currentVolume>0)
			{
				volume=((currentVolume*100)/max)/10;
			}
			if(volume<1)
				m_muteState=true;
			else
				m_muteState=false;
			
			int[] drawWidth=new int[]{0,25,43,62,82,99,117,137,155,173,200};

			m_osdRendered=OSD.VolumeOSD;
			m_bitmapIsVisible=false;
			m_timeout=3000; // 3 sec for volume osd
			if(System.IO.File.Exists(m_mediaPath+String.Format("volume_level_10.png",volume))==true)
			{
				if(m_osdSkin.mute!=null)
				{
					string[] seg =m_osdSkin.mute.Split(new char[]{':'});
					if(seg!=null)
					{
						if(seg[0]=="icon" && seg.Length==4)
						{
							Bitmap osd=new Bitmap(gWidth,gHeight);
							Graphics gr=Graphics.FromImage(osd);
							
							//Bitmap gfx=new Bitmap(m_mediaPath+String.Format("volume_level_{0}.png",volume));
							//gfx.MakeTransparent(Color.White);
							int xPos=0;
							int yPos=0;

							if(seg[1].StartsWith("m"))
								xPos=GetPosition(gWidth,seg[1]);
							else
								xPos=Convert.ToInt16(seg[1]);

							if(seg[2].StartsWith("m"))
								yPos=GetPosition(gHeight,seg[2]);
							else
								yPos=Convert.ToInt16(seg[2]);
							
							if(volume>0)
							{
								if(m_volumeBitmap!=null)
									gr.DrawImage(m_volumeBitmap,xPos,yPos,new RectangleF(0f,0f,drawWidth[volume],m_volumeBitmap.Height),System.Drawing.GraphicsUnit.Pixel);
							}
							else
								if(m_muteBitmap!=null)
									gr.DrawImageUnscaled(m_muteBitmap,xPos,yPos,m_muteBitmap.Width,m_muteBitmap.Height);

							SaveBitmap(osd,true,true,0.9f);
							gr.Dispose();
							osd.Dispose();
							m_timeDisplayed=DateTime.Now;
						}
					}
				
				}
			}
		}
		public void RenderZapOSD(TVChannel channel,int signalQuality,int signalLevel)
		{
			try
			{
				int gWidth=GUIGraphicsContext.Width;
				int gHeight=GUIGraphicsContext.Height;
				Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
				Graphics gr=Graphics.FromImage(bm);
				int x=140;
				int y=0;
				if(bm==null || gr==null || channel==null)
				{
					Log.Write("end rendering zaposd: no bitmap (memory problem?)");
					return ;
				}
				m_osdRendered=OSD.ZapOSD;
				m_timeout=0;
				m_actualChannel=channel;
				m_channelSNR=signalQuality;
				m_channelLevel=signalLevel;
				// set the tvchannel data
				string serviceName=channel.Name;
				TVProgram tvNext=null;
				TVProgram tvNow=channel.GetProgramAt(DateTime.Now);
				string nowStart="";
				string nowDur="";
				string nowDescr="";
				string nowTitle="";
				string nextTitle="";
				string nextStart="";
				string nextDur="";
				double done=0;
				double signalQual=(double)signalQuality;
				double signalLev=(double)signalLevel;


				if(tvNow!=null)
				{
					tvNext=channel.GetProgramAt(tvNow.EndTime.AddMinutes(1));		
					nowStart=tvNow.StartTime.ToShortTimeString();
					nowDur=tvNow.Duration.ToString();
					double nowDone=tvNow.EndTime.Subtract(DateTime.Now).TotalMinutes;
					double nowTotal=tvNow.EndTime.Subtract(tvNow.StartTime).TotalMinutes;
					done=(nowDone*100)/nowTotal;
					nowTitle=tvNow.Title;
					nowDescr=tvNow.Description;
				}
				if(tvNext!=null)
				{
					nextStart=tvNext.StartTime.ToShortTimeString();
					nextTitle=tvNext.Title;
					nextDur=tvNext.Duration.ToString();
				}
			
				// first graphic elements and pictures
				// rects
				string skinPath=GUIGraphicsContext.Skin+@"\media\";
				// bg 2 draw?

				int width=gWidth;
				int height=210;

				int xpos=0;
				int ypos=y+5;
				int timeX=0;
				int timeY=0;
				int logoW=0;
				int logoH=0;

				if(System.IO.File.Exists(skinPath+"background.png")==true &&
					System.IO.File.Exists(skinPath+"icon_empty_focus.png")==true)

				{
					Bitmap osd=new Bitmap(skinPath+"background.png");
					xpos=(gWidth-width)/2;
					ypos=gHeight-height;
					y=ypos+10;
					gr.DrawImage(osd,new Rectangle(xpos,ypos,width,height),0,0,osd.Width,osd.Height,GraphicsUnit.Pixel);
					gr.DrawImage(osd,new Rectangle(x,y,width-(x+10),height-20),x,y,width-(x+10),height-20,GraphicsUnit.Pixel);
					osd=new Bitmap(skinPath+"icon_empty_focus.png");
					logoW=osd.Width;
					logoH=osd.Height;
					int w=width-(x+10);
					xpos=10;
					ypos+=10;
					gr.DrawImage(osd,xpos,ypos,osd.Width,osd.Height);
					timeX=xpos+10;
					timeY=ypos+osd.Height+10;
					if(nowStart!="")
						gr.FillRectangle(new SolidBrush(Color.DarkBlue),x,y+30,w,60);
					osd.Dispose();
				}

				x+=5;
				// text always gets an x-offset 40 pix.
				// tv channel logo
				xpos=10;
				ypos=y;
				string tvlogo=Utils.GetCoverArt(Thumbs.TVChannel,serviceName);				
				if(System.IO.File.Exists(tvlogo))
				{
					logoW=logoW<64?64:logoW;
					logoH=logoH<64?64:logoH;
					Bitmap logo=new Bitmap(tvlogo);
					Util.BitmapResize.Resize(ref logo,64,64,true,true);
					gr.DrawImage(logo,xpos+((logoW-64)/2),ypos+((logoH-64)/2),64,64);
					logo.Dispose();
				}
				//channel name (chName)
				gr.DrawString(serviceName,new System.Drawing.Font("Arial",16),new SolidBrush(Color.White),x,y,StringFormat.GenericTypographic);
				y+=35;
				//now on tv (chNow)

				Font drawFont=new System.Drawing.Font("Arial",14);
				Brush drawBrush=new SolidBrush(Color.White);
				SizeF xEnd=gr.MeasureString(nowDur,drawFont);
				int xPosEnd=(gWidth-70)-((int)xEnd.Width);
				gr.DrawString(nowDur,drawFont,drawBrush,xPosEnd,y,StringFormat.GenericTypographic);
				gr.DrawString(nowStart+"  "+nowTitle,drawFont,drawBrush,new RectangleF(x,y,xPosEnd-x-5,xEnd.Height),StringFormat.GenericTypographic);
				// now prog
				if(nowStart!="" && 
					System.IO.File.Exists(skinPath+"osd_progress_mid_orange.png")==true &&
					System.IO.File.Exists(skinPath+"osd_progress_background.png")==true)
				{
					Bitmap prog=new Bitmap(skinPath+"osd_progress_background.png");
					gr.DrawImage(prog,new Rectangle(x,y+26,200,prog.Height),0,0,200,prog.Height,GraphicsUnit.Pixel);
					prog=new Bitmap(skinPath+"osd_progress_mid_orange.png");
					gr.DrawImage(prog,new Rectangle(x+1,y+28,200-((int)((done/100)*200))-2,prog.Height),0,0,200-((int)((done/100)*200))-2,prog.Height,GraphicsUnit.Pixel);
					prog.Dispose();
					y+=65;
				}
				//next on tv (chNow)

				xPosEnd=(gWidth-70)-((int)xEnd.Width);
				gr.DrawString(nextDur,drawFont,drawBrush,xPosEnd,y,StringFormat.GenericTypographic);
				gr.DrawString(nextStart+"  "+nextTitle,drawFont,drawBrush,new RectangleF(x,y,xPosEnd-x-5,xEnd.Height),StringFormat.GenericTypographic);

				y+=35;

				// quality and level
				xEnd.Width=100;

				if(signalQuality>2 && 
					System.IO.File.Exists(skinPath+"osd_progress_background.png")==true &&
					System.IO.File.Exists(skinPath+"osd_progress_mid.png")==true &&
					System.IO.File.Exists(skinPath+"osd_progress_mid_red.png")==true)
				{
					gr.DrawString("Quality:",drawFont,drawBrush,x,y);
					Bitmap prog=new Bitmap(skinPath+"osd_progress_background.png");
					gr.DrawImage(prog,new Rectangle(x+((int)xEnd.Width),y,200,prog.Height),0,0,200,prog.Height,GraphicsUnit.Pixel);
					if(signalQuality>50)
						prog=new Bitmap(skinPath+"osd_progress_mid.png");
					else
						prog=new Bitmap(skinPath+"osd_progress_mid_red.png");

					gr.DrawImage(prog,new Rectangle(x+((int)xEnd.Width)+1,y+2,((int)((signalQual/100)*200))-2,prog.Height),0,0,((int)((signalQual/100)*200))-2,prog.Height,GraphicsUnit.Pixel);
					prog.Dispose();
					y+=25;
				}
				if(signalLevel>2 &&
					System.IO.File.Exists(skinPath+"osd_progress_background.png")==true	&&
					System.IO.File.Exists(skinPath+"osd_progress_mid.png")==true &&
					System.IO.File.Exists(skinPath+"osd_progress_mid_red.png")==true)
					
				{
					gr.DrawString("Level:",drawFont,drawBrush,x,y);
					Bitmap prog=new Bitmap(skinPath+"osd_progress_background.png");
					gr.DrawImage(prog,new Rectangle(x+((int)xEnd.Width),y,200,prog.Height),0,0,200,prog.Height,GraphicsUnit.Pixel);
					if(signalLevel>50)
						prog=new Bitmap(skinPath+"osd_progress_mid.png");
					else
						prog=new Bitmap(skinPath+"osd_progress_mid_red.png");

					gr.DrawImage(prog,new Rectangle(x+((int)xEnd.Width)+1,y+2,((int)((signalLev/100)*200))-2,prog.Height),0,0,((int)((signalLev/100)*200))-2,prog.Height,GraphicsUnit.Pixel);
					prog.Dispose();
				}

				gr.DrawString(DateTime.Now.ToShortTimeString(),drawFont,drawBrush,timeX,timeY);

				drawFont.Dispose();
				drawBrush.Dispose();


				m_bitmapIsVisible=true;
				SaveBitmap(bm,true,true,m_renderOSDAlpha);
			}
			catch(Exception ex)
			{
				Log.Write("exception in render-osd:{0}, {1}, {2}",ex.Message,ex.Source,ex.StackTrace);
				SaveBitmap(null,false,true,m_renderOSDAlpha);

			}
		}
		#endregion

		#region private helper functions
		int GetPosition(int baseVal,string val)
		{
			string val1=val.Substring(1,val.Length-1);
			int val2=Convert.ToInt16(val1);
			return (baseVal-val2<0)?0:baseVal-val2;
		}
		Color GetColor(string colString)
		{
			Color col=new Color();
			if(colString!=null)
			{
				string[] values=colString.Split(new char[]{','});
				if(values!=null)
				{
					if(values.Length==3)
					{
						int red=Convert.ToInt16(values[0]);
						int green=Convert.ToInt16(values[1]);
						int blue=Convert.ToInt16(values[2]);
						if(red<0 || red>255) red=0;
						if(green<0 || green>255) green=0;
						if(blue<0 || blue>255) blue=0;
						col=System.Drawing.Color.FromArgb(red,green,blue);
					}
				}
			}
			return col;
		}
		#endregion

		#region public functions
		public void RefreshCurrentChannel(int signal)
		{
			if(signal!=m_channelSNR && m_bitmapIsVisible==true)
			{
				m_channelSNR=signal;
				RenderZapOSD(m_actualChannel,m_channelSNR,m_channelLevel);
			}
		}

		public void RefreshCurrentChannel()
		{
			RenderZapOSD(m_actualChannel,m_channelSNR,m_channelLevel);
		}
		public void CheckTimeOuts()
		{
			TimeSpan ts=DateTime.Now-m_timeDisplayed;
			if(ts.TotalMilliseconds>m_timeout && m_timeout>0)
			{
				
				if(m_osdRendered==OSD.VolumeOSD)
				{
					if(m_muteState==true)
						return;
					m_bitmapIsVisible=true; // force clear
					HideBitmap();
					m_timeout=0;
				}
				else
				{
					m_bitmapIsVisible=true; // force clear
					HideBitmap();
					m_timeout=0;
				}
			}
		}
		public void ShowBitmap(Bitmap bmp)
		{
			if(bmp==null)
				return;
			m_timeout=0;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,1.0f);
		}
		public void ShowBitmap(Bitmap bmp,int timeout)
		{
			if(bmp==null)
				return;
			m_timeout=timeout;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,1.0f);
		}
		public void ShowBitmap(Bitmap bmp,float alpha,int timeout)
		{
			if(bmp==null)
				return;
			m_timeout=timeout;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,alpha);
		}

		public void ShowBitmap(Bitmap bmp,float alpha)
		{
			if(bmp==null)
				return;
			m_timeout=0;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,alpha);
		}
		public void HideBitmap()
		{
			SaveBitmap(null,false,true,0f);
		}
		#endregion

		#region private functions
		void ReadSkinFile()
		{

			// channel list
			m_osdChannels.baseRect="nsrect:14,31,215:20,20,120:243,182,16:255,255,255:Arial:14";
			// bg
			try
			{
				m_volumeBitmap=new Bitmap(m_mediaPath+"volume_level_10.png");
				m_volumeBitmap.MakeTransparent(Color.White);
				m_muteBitmap=new Bitmap(m_mediaPath+"volume_level_0.png");
				m_muteBitmap.MakeTransparent(Color.White);
			}
			catch{}
		}

		bool SaveBitmap(System.Drawing.Bitmap bitmap,bool show,bool transparent,float alphaValue)
		{
			if (VMR9Util.g_vmr9!=null)
			{
				if (show==true)
				{
					VMR9Util.g_vmr9.SaveBitmap(bitmap,show,transparent,alphaValue);
					m_timeDisplayed=DateTime.Now;
					return true;
				}
				else
				{
					if(m_bitmapIsVisible==true)
					{
						if(m_muteState==true)
						{
							RenderVolumeOSD();
						}
						else
						{
							VMR9Util.g_vmr9.SaveBitmap(bitmap,show,transparent,alphaValue);
							m_bitmapIsVisible=false;
							m_osdRendered=OSD.None;
						}
					}
				}
				// dispose
				return true;
			}


			if (VMR7Util.g_vmr7!=null)
			{
				// transparent image?
				if(bitmap!=null)
				{
					if(transparent==true)
						bitmap.MakeTransparent(Color.Black);
				}
				if (show==true)
				{
					VMR7Util.g_vmr7.SaveBitmap(bitmap,show,transparent,alphaValue);
					m_timeDisplayed=DateTime.Now;
					return true;
				}
				else
				{
					if(m_bitmapIsVisible==true)
					{
						if(m_muteState==true)
						{
							RenderVolumeOSD();
						}
						else
						{
							VMR7Util.g_vmr7.SaveBitmap(bitmap,show,transparent,alphaValue);
							m_bitmapIsVisible=false;
							m_osdRendered=OSD.None;
						}
					}
				}
				// dispose
				return true;
			}

			return false;
		}// savevmr9bitmap

		
		#endregion
	}// class
}// namespace
