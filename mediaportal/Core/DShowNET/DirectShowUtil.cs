using System;
using System.IO;
using System.Runtime.InteropServices; 
using DShowNET;
namespace DShowNET
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectShowUtil
	{
		public DirectShowUtil()
		{
		}

    static public IBaseFilter AddFilterToGraph(IGraphBuilder graphBuilder, string strFilterName)
    {
      IBaseFilter NewFilter=null;
      DebugWrite("add filter:{0} to graph", strFilterName);
      Filters filters = new Filters();
      foreach (Filter filter in filters.LegacyFilters)
      {
        if (String.Compare(filter.Name,strFilterName,true) ==0)
        {
          NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );
          int hr = graphBuilder.AddFilter( NewFilter, strFilterName );
          if( hr < 0 ) 
          {
            DebugWrite("failed:unable to add filter:{0} to graph", strFilterName);
            NewFilter=null;
          }
          else
          {
            DebugWrite("added filter:{0} to graph", strFilterName);
          }
          break;
        }
      }
      if (NewFilter==null)
      {
        DebugWrite("failed filter:{0} not found", strFilterName);
      }
      return NewFilter;
    }

    static public void RemoveAudioRendererFromGraph(IGraphBuilder graphBuilder)
    {
      int hr;
      IBaseFilter NewFilter=null;
      Filters filters = new Filters();
      
      // first remove all audio renderers
      bool bAllRemoved=false;
      IEnumFilters enumFilters;
      hr=graphBuilder.EnumFilters(out enumFilters);
      if (hr>=0 && enumFilters!=null)
      {
        uint iFetched;
        enumFilters.Reset();
        while(!bAllRemoved)
        {
          IBaseFilter pBasefilter=null;
          hr=enumFilters.Next(1,out pBasefilter,out iFetched);
          if (hr<0 || iFetched!=1 || pBasefilter==null) break;

          foreach (Filter filter in filters.AudioRenderers)
          {
            Guid classId1;
            Guid classId2;
            pBasefilter.GetClassID(out classId1);            
            
            NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );
            NewFilter.GetClassID(out classId2);
            Marshal.ReleaseComObject( NewFilter );

            if (classId1.Equals(classId2))
            { 
              DebugWrite("remove "+ filter.Name + " from graph");
              graphBuilder.RemoveFilter(pBasefilter);
              bAllRemoved=true;
              break;
            }
          }
          Marshal.ReleaseComObject( pBasefilter );
        }
      }
    }
    static public IBaseFilter AddAudioRendererToGraph(IGraphBuilder graphBuilder,string strFilterName)
    {
			try
			{
				int hr;
				IPin pinOut=null;
				IBaseFilter NewFilter=null;
				DebugWrite("add filter:{0} to graph", strFilterName);
				Filters filters = new Filters();
	      
				//check first if audio renderer exists!
				bool bRendererExists=false;
				foreach (Filter filter in filters.AudioRenderers)
				{
					if (String.Compare(filter.Name,strFilterName,true) ==0)
					{
						bRendererExists=true;
					}
				}
				if (!bRendererExists) 
				{
					DirectShowUtil.DebugWrite("FAILED: audio renderer:{0} doesnt exists", strFilterName);
					return null;
				}

				// first remove all audio renderers
				bool bAllRemoved=false;
				bool bNeedAdd=true;
				IEnumFilters enumFilters;
				hr=graphBuilder.EnumFilters(out enumFilters);
				if (hr>=0 && enumFilters!=null)
				{
					uint iFetched;
					enumFilters.Reset();
					while(!bAllRemoved)
					{
						IBaseFilter pBasefilter=null;
						hr=enumFilters.Next(1,out pBasefilter,out iFetched);
						if (hr<0 || iFetched!=1 || pBasefilter==null) break;

						foreach (Filter filter in filters.AudioRenderers)
						{
							Guid classId1;
							Guid classId2;
							pBasefilter.GetClassID(out classId1);            
	            
							NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );
							NewFilter.GetClassID(out classId2);
							Marshal.ReleaseComObject( NewFilter );

							if (classId1.Equals(classId2))
							{ 
								if (filter.Name== strFilterName)
								{
									DebugWrite("filter already in graph");
									Marshal.ReleaseComObject( pBasefilter );
									bNeedAdd=false;
									break;
								}
								else
								{
									DebugWrite("remove "+ filter.Name + " from graph");
									pinOut=FindSourcePinOf(pBasefilter);
									graphBuilder.RemoveFilter(pBasefilter);
									bAllRemoved=true;
									break;
								}
							}
						}
						Marshal.ReleaseComObject( pBasefilter );
					}
				}

				if (!bNeedAdd) return null;
				// next add the new one...
				foreach (Filter filter in filters.AudioRenderers)
				{
					if (String.Compare(filter.Name,strFilterName,true) ==0)
					{
						NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );
						hr = graphBuilder.AddFilter( NewFilter, strFilterName );
						if( hr < 0 ) 
						{
							DebugWrite("failed:unable to add filter:{0} to graph", strFilterName);
							NewFilter=null;
						}
						else
						{
							DebugWrite("added filter:{0} to graph", strFilterName);
							if (pinOut!=null)
							{
								hr=graphBuilder.Render(pinOut);
								if (hr==0) DebugWrite(" pinout rendererd");
								else DebugWrite(" failed: pinout render");
							}
							return NewFilter;
						}
					}
				}
				if (NewFilter==null)
				{
					DebugWrite("failed filter:{0} not found", strFilterName);
				}
			}
			catch(Exception ex)
			{
				DebugWrite("DirectshowUtil. Failed to add filter:{0} to graph :{0} {1} [2}", 
							strFilterName,ex.Message,ex.Source,ex.StackTrace);
			}
      return null;
    }

    static public void DebugWrite(string strFormat, params object[] arg)
    {
      try
      {
        using (StreamWriter writer = new StreamWriter(@"log\capture.log",true))
        {
          writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
          writer.Write(DateTime.Now.ToShortDateString()+ " "+DateTime.Now.ToLongTimeString()+ " ");
          writer.WriteLine(strFormat,arg);
          writer.Close();
        }
        using (StreamWriter writer = new StreamWriter(@"log\mediaportal.log",true))
        {
          writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
          writer.Write(DateTime.Now.ToShortDateString()+ " "+DateTime.Now.ToLongTimeString()+ " ");
          writer.WriteLine(strFormat,arg);
          writer.Close();
        }
      }
      catch(Exception)
      {
      }
    }
    static public IPin FindPin (IBaseFilter filter, PinDirection dir,string strPinName)
    {
      int hr=0;
      IEnumPins pinEnum;
      hr=filter.EnumPins(out pinEnum);
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
            if (pinDir==dir)
            {
              PinInfo info;
              pins[0].QueryPinInfo(out info);
              if ( String.Compare(info.name,strPinName)==0 )
              {
                return pins[0];
              }
            }
            Marshal.ReleaseComObject( pins[0] );
          }
        }
        while( hr == 0 );
      }
      return null;
    }

    static public IPin FindPinNr (IBaseFilter filter, PinDirection dir,int iPinNr)
    {
      if (filter==null ) return null;
      int iCurrentPinNr=0;
      int hr=0;
      IEnumPins pinEnum;
      hr=filter.EnumPins(out pinEnum);
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
            if (pinDir==dir)
            {
              PinInfo info;
              pins[0].QueryPinInfo(out info);
              if ( iCurrentPinNr==iPinNr )
              {
                return pins[0];
              }
              iCurrentPinNr++;
            }
            Marshal.ReleaseComObject( pins[0] );
          }
        }
        while( hr == 0 );
      }
      return null;
    }

    static public IPin FindSourcePinOf(IBaseFilter filter)
    {
      int hr=0;
      IEnumPins pinEnum;
      hr=filter.EnumPins(out pinEnum);
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
              IPin pSourcePin=null;
              hr=pins[0].ConnectedTo(out pSourcePin);
              if (hr>=0)
              {
                return pSourcePin;
              }
            }
            Marshal.ReleaseComObject( pins[0] );
          }
        }
        while( hr == 0 );
      }
      return null;
    }

		static public bool RenderOutputPins(IGraphBuilder graphBuilder,IBaseFilter filter)
		{
			return RenderOutputPins(graphBuilder,filter,100);
		}
    static public bool RenderOutputPins(IGraphBuilder graphBuilder,IBaseFilter filter, int maxPinsToRender)
    {
			int  pinsRendered=0;
      bool bAllConnected=true;
      IEnumPins pinEnum;
      int hr=filter.EnumPins(out pinEnum);
      if( (hr == 0) && (pinEnum != null) )
      {
        DebugWrite("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo=0;
        do
        {
          // Get the next pin
          //DebugWrite("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next( 1, pins, out iFetched );
          if( hr == 0 )
          {
            if (iFetched==1 && pins[0]!=null) 
            {
              PinInfo pinInfo = new PinInfo();
              hr=pins[0].QueryPinInfo(out pinInfo);
              if (hr>=0)
                DebugWrite("  got pin#{0}:{1}",iPinNo-1,pinInfo.name);
              else
                DebugWrite("  got pin:?");
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir==PinDirection.Output)
              {
                IPin pConnectPin=null;
                hr=pins[0].ConnectedTo(out pConnectPin);  
                if (hr< 0 || pConnectPin==null)
                {
                  hr=graphBuilder.Render(pins[0]);
                  if (hr==0) DebugWrite("  render ok");
                  else 
                  {
                    DebugWrite("  render failed:{0:x}",hr);
                    bAllConnected=false;
                  }
									pinsRendered++;
                }
                //else DebugWrite("pin is already connected");
              }
              Marshal.ReleaseComObject( pins[0] );
            }
            else 
            {
              iFetched=0;
              DebugWrite("no pins?");
              break;
            }
          }
          else iFetched=0;
        }while( iFetched==1 && pinsRendered < maxPinsToRender);
      }
      return bAllConnected;
    }

    static public void DisconnectOutputPins(IGraphBuilder graphBuilder,IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr=filter.EnumPins(out pinEnum);
      if( (hr == 0) && (pinEnum != null) )
      {
        //DebugWrite("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo=0;
        do
        {
          // Get the next pin
          //DebugWrite("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next( 1, pins, out iFetched );
          if( hr == 0 )
          {
            if (iFetched==1 && pins[0]!=null) 
            {
              //DebugWrite("  find pin info");
              PinInfo pinInfo = new PinInfo();
              hr=pins[0].QueryPinInfo(out pinInfo);
              if (hr>=0)
                DebugWrite("  got pin#{0}:{1}",iPinNo-1,pinInfo.name);
              else
                DebugWrite("  got pin:?");
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir==PinDirection.Output)
              {
                //DebugWrite("  is output");
                IPin pConnectPin=null;
                hr=pins[0].ConnectedTo(out pConnectPin);  
                if (hr==0 && pConnectPin!=null)
                {
                  //DebugWrite("  pin is connected ");
                  hr=pins[0].Disconnect();
                  if (hr==0) DebugWrite("  disconnected ok");
                  else 
                  {
                    DebugWrite("  disconnected failed");
                  }
                }
                //else DebugWrite("pin is already connected");
              }
              Marshal.ReleaseComObject( pins[0] );
            }
            else 
            {
              iFetched=0;
              DebugWrite("no pins?");
              break;
            }
          }
          else iFetched=0;
        }while( iFetched==1 );
      }
    }

    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    static public void SetARMode(IGraphBuilder graphBuilder, AmAspectRatioMode ARRatioMode)
    {
      int hr;
      IBaseFilter overlay;
      graphBuilder.FindFilterByName("Overlay Mixer2",out overlay);
        
      if (overlay!=null)
      {
        IPin iPin;
        overlay.FindPin("Input0", out iPin);
        if (iPin!=null)
        {
          IMixerPinConfig pMC = iPin as IMixerPinConfig ;
          if (pMC!=null)
          {
            AmAspectRatioMode mode;
            hr=pMC.SetAspectRatioMode(ARRatioMode);
            hr=pMC.GetAspectRatioMode(out mode);
          }
        }
      }
        

      IEnumFilters enumFilters;
      hr=graphBuilder.EnumFilters(out enumFilters);
      if (hr>=0 && enumFilters!=null)
      {
        uint iFetched;
        enumFilters.Reset();
        IBaseFilter pBasefilter=null;
        do
        {
          pBasefilter=null;
          hr=enumFilters.Next(1,out pBasefilter,out iFetched);
          if (hr==0 && iFetched==1 &&  pBasefilter!=null)
          {

            IVMRAspectRatioControl pARC = pBasefilter as IVMRAspectRatioControl;
            if (pARC!=null)
            {
              pARC.SetAspectRatioMode( (uint)VmrAspectRatioMode.VMR_ARMODE_NONE);
            }
            IVMRAspectRatioControl9 pARC9 = pBasefilter as IVMRAspectRatioControl9;
            if (pARC9!=null)
            {
              pARC9.SetAspectRatioMode((uint)VmrAspectRatioMode.VMR_ARMODE_NONE);
            }

            IEnumPins pinEnum;
            hr=pBasefilter.EnumPins(out pinEnum);
            if( (hr == 0) && (pinEnum != null) )
            {
              pinEnum.Reset();
              IPin[] pins = new IPin[1];
              int f;
              do
              {
                // Get the next pin
                hr = pinEnum.Next( 1, pins, out f );
                if(f==1&& hr == 0 && pins[0] != null )
                {
                  IMixerPinConfig pMC = pins[0] as IMixerPinConfig ;
                  if (null!=pMC)
                  {
                    pMC.SetAspectRatioMode(ARRatioMode);
                  }
                  Marshal.ReleaseComObject( pins[0] );
                }
              } while( f ==1);
            }
          }
        } while (iFetched==1 && pBasefilter!=null);
      }
    }

		static bool IsInterlaced(uint x) 
		{
			return ((x) & AmInterlace.IsInterlaced)!=0;
		}
		static bool IsSingleField(uint x) 
		{
			return ((x) & AmInterlace.OneFieldPerSample)!=0;
		}
		static bool  IsField1First(uint x)
		{
			return ((x) & AmInterlace.Field1First)!=0;
		}

		static VMR9_SampleFormat ConvertInterlaceFlags(uint dwInterlaceFlags)
		{
			if (IsInterlaced(dwInterlaceFlags)) 
			{
				if (IsSingleField(dwInterlaceFlags)) 
				{
					if (IsField1First(dwInterlaceFlags)) 
					{
						return VMR9_SampleFormat.VMR9_SampleFieldSingleEven;
					}
					else 
					{
						return VMR9_SampleFormat.VMR9_SampleFieldSingleOdd;
					}
				}
				else 
				{
					if (IsField1First(dwInterlaceFlags)) 
					{
						return VMR9_SampleFormat.VMR9_SampleFieldInterleavedEvenFirst;
					}
					else 
					{
						return VMR9_SampleFormat.VMR9_SampleFieldInterleavedOddFirst;
					}
				}
			}
			else 
			{
				return VMR9_SampleFormat.VMR9_SampleProgressiveFrame;  // Not interlaced.
			}
		}
    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    static public void EnableDeInterlace(IGraphBuilder graphBuilder)
    {
			DirectShowUtil.DebugWrite("EnableDeInterlace()");
      int hr;
      IBaseFilter overlay;
      graphBuilder.FindFilterByName("Overlay Mixer2",out overlay);
        
      if (overlay!=null)
      {
        IAMOverlayFX overlayFX = overlay as IAMOverlayFX;
        if (overlayFX!=null)
        {
          overlayFX.SetOverlayFX((uint)AMOVERLAYFX.DEINTERLACE);
        }
      }
        

			DirectShowUtil.DebugWrite("EnableDeInterlace() enum filters");
      IEnumFilters enumFilters;
      hr=graphBuilder.EnumFilters(out enumFilters);
      if (hr>=0 && enumFilters!=null)
      {
        uint iFetched;
        enumFilters.Reset();
        IBaseFilter pBasefilter=null;
        do
        {
          pBasefilter=null;
          hr=enumFilters.Next(1,out pBasefilter,out iFetched);
					if (hr==0 && iFetched==1 &&  pBasefilter!=null)
					{
						//DirectShowUtil.DebugWrite("got filter");

						IAMOverlayFX overlayFX = pBasefilter as IAMOverlayFX;
						if (overlayFX!=null)
						{
							DirectShowUtil.DebugWrite("enable overlay deinterlace");
							hr=overlayFX.SetOverlayFX((uint)AMOVERLAYFX.DEINTERLACE);
							if (hr!=0) DirectShowUtil.DebugWrite("Unable to set overlay deinterlace ");
						}
          
						IVMRDeinterlaceControl9 vmrdeinterlace9 = pBasefilter as IVMRDeinterlaceControl9;
						if (vmrdeinterlace9!=null)
						{
							DirectShowUtil.DebugWrite("VMR9:got IVMRDeinterlaceControl9");
							VMR9VideoDesc videodesc=new VMR9VideoDesc();
							IPin pinIn;
							pinIn=FindPinNr(pBasefilter,PinDirection.Input,0);
							if (pinIn!=null)
							{
								DirectShowUtil.DebugWrite("VMR9:got pin0");
								IntPtr ptrPMT=Marshal.AllocCoTaskMem(1000);
								hr=pinIn.ConnectionMediaType(ptrPMT);
								if (hr==0)
								{
									DirectShowUtil.DebugWrite("VMR9:got mediatype ");
									AMMediaType mediaType;
									mediaType=(AMMediaType)Marshal.PtrToStructure(ptrPMT,typeof(AMMediaType));
									
									if (mediaType.formatType==FormatType.VideoInfo)
									{
										DirectShowUtil.DebugWrite("VMR9:got mediatype :videoInfo");
										VideoInfoHeader vidInfo;
										vidInfo=(VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr,typeof(VideoInfoHeader));
										DirectShowUtil.DebugWrite("VMR9:ok");
										videodesc.dwFourCC=(uint)vidInfo.BmiHeader.Compression;
										videodesc.dwSampleWidth=(uint)vidInfo.BmiHeader.Width;
										videodesc.dwSampleHeight=(uint)vidInfo.BmiHeader.Height;
										videodesc.SampleFormat=VMR9_SampleFormat.VMR9_SampleFieldInterleavedEvenFirst;
										videodesc.InputSampleFreq.dwDenominator=(uint)vidInfo.AvgTimePerFrame;
										videodesc.InputSampleFreq.dwNumerator=10000000;
										
										videodesc.OutputFrameFreq.dwDenominator=(uint)vidInfo.AvgTimePerFrame;
										videodesc.OutputFrameFreq.dwNumerator=videodesc.InputSampleFreq.dwNumerator;
										if (videodesc.SampleFormat != VMR9_SampleFormat.VMR9_SampleProgressiveFrame)
										{
											videodesc.OutputFrameFreq.dwNumerator=2*videodesc.InputSampleFreq.dwNumerator;
										}

									}
									if (mediaType.formatType==FormatType.VideoInfo2)
									{
										VideoInfoHeader2 vidInfo2;
										DirectShowUtil.DebugWrite("VMR9:got mediatype :videoInfo2");
										vidInfo2=(VideoInfoHeader2)Marshal.PtrToStructure(mediaType.formatPtr,typeof(VideoInfoHeader2));
										DirectShowUtil.DebugWrite("VMR9:ok");
										videodesc.dwFourCC=vidInfo2.bmpInfoHdr.biCompression;
										videodesc.dwSampleWidth=(uint)vidInfo2.bmpInfoHdr.biWidth;
										videodesc.dwSampleHeight=(uint)vidInfo2.bmpInfoHdr.biHeight;
										videodesc.SampleFormat=ConvertInterlaceFlags(vidInfo2.dwInterlaceFlags);
										videodesc.InputSampleFreq.dwDenominator=(uint)vidInfo2.AvgTimePerFrame;
										videodesc.InputSampleFreq.dwNumerator=10000000;
										
										videodesc.OutputFrameFreq.dwDenominator=(uint)vidInfo2.AvgTimePerFrame;
										videodesc.OutputFrameFreq.dwNumerator=videodesc.InputSampleFreq.dwNumerator;
										if (videodesc.SampleFormat != VMR9_SampleFormat.VMR9_SampleProgressiveFrame)
										{
											videodesc.OutputFrameFreq.dwNumerator=2*videodesc.InputSampleFreq.dwNumerator;
										}
										DirectShowUtil.DebugWrite("src {0},{1}-{2},{3} dst {4},{5}-{6},{7} avgt:{8} ar {9}:{10}",
												vidInfo2.rcsource.left,vidInfo2.rcsource.top,
												vidInfo2.rcsource.right,vidInfo2.rcsource.bottom,
												vidInfo2.rctarget.left,vidInfo2.rctarget.top,
												vidInfo2.rctarget.right,vidInfo2.rctarget.bottom,
												vidInfo2.AvgTimePerFrame,
											vidInfo2.dwPictAspectRatioX,vidInfo2.dwPictAspectRatioY);


									}
								}
								else DirectShowUtil.DebugWrite("VMR9:Unable to get media type for pin0");
								Marshal.FreeCoTaskMem(ptrPMT);
							}
							else DirectShowUtil.DebugWrite("VMR9:Unable to get pin0");

							Guid guidDeint =Deinterlace.DXVA_DeinterlaceBobDevice;
							videodesc.dwSize = (uint)Marshal.SizeOf(videodesc);
							
							string strFormat="";
							strFormat+=(char)(videodesc.dwFourCC&0xff);
							strFormat+=(char)((videodesc.dwFourCC>>8)&0xff);
							strFormat+=(char)((videodesc.dwFourCC>>16)&0xff);
							strFormat+=(char)((videodesc.dwFourCC>>24)&0xff);
							DirectShowUtil.DebugWrite("VMR9 Get best deinterlace mode for {0}x{1} format:{2} 4cc:{3} size:{4}", 
								videodesc.dwSampleWidth, videodesc.dwSampleHeight, 
								videodesc.SampleFormat.ToString(),
								strFormat,
								videodesc.dwSize);
							uint uiNumberOfDeinterlaceModes=100;
							Guid[] guidsDeint = new Guid[100];
							hr=vmrdeinterlace9.GetNumberOfDeinterlaceModes( ref videodesc, ref uiNumberOfDeinterlaceModes,ref guidsDeint);
							if (hr==0)
							{
								guidDeint = guidsDeint[0];
								DirectShowUtil.DebugWrite("VMR9 supports {0} interlace modes",uiNumberOfDeinterlaceModes);
								DirectShowUtil.DebugWrite("Set VMR9 deinterlace mode to:{0}", guidDeint.ToString());
								hr=vmrdeinterlace9.SetDeinterlaceMode(0xFFFFFFFF,ref guidDeint);
								if (hr!=0) DirectShowUtil.DebugWrite("Unable to set deinterlace mode:0x{0:X}",hr);

								hr=vmrdeinterlace9.GetActualDeinterlaceMode (0,out guidDeint);
								if (hr!=0 ) 
									hr=vmrdeinterlace9.GetActualDeinterlaceMode (0,out guidDeint);
								if (hr!=0 ) 
									DirectShowUtil.DebugWrite("VMR9 Unable to get current deinterlace mode:0x{0:X}",hr);
								else
									DirectShowUtil.DebugWrite("VMR9 uses deinterlace guid:{0}",guidDeint.ToString());
							}
							else
							{
								DirectShowUtil.DebugWrite("unable to get VMR9 interlace modes:0x{0:X}",hr);
							}

							DirectShowUtil.DebugWrite("set VMR9 deinterlace preferences to next best");
							hr=vmrdeinterlace9.SetDeinterlacePrefs((uint)VMR9DeinterlacePrefs.DeinterlacePref9_NextBest);
							if (hr!=0) DirectShowUtil.DebugWrite("Unable to set deinterlace preferences to next best:0x{0:X}",hr);


						}
					}
        } while (iFetched==1 && pBasefilter!=null);
      }
    }

    static public IPin FindVideoPort(ref ICaptureGraphBuilder2 captureGraphBuilder ,ref IBaseFilter videoDeviceFilter,ref Guid mediaType)
    {
      IPin pPin;
      Guid cat = PinCategory.VideoPort;
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
      if (hr>=0 && pPin!=null)
        DebugWrite("Found videoport pin");
      return pPin;
    }

    static public IPin FindPreviewPin(ref ICaptureGraphBuilder2 captureGraphBuilder ,ref IBaseFilter videoDeviceFilter,ref Guid mediaType)
    {
      IPin pPin;
      Guid cat = PinCategory.Preview;
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
      if (hr>=0 && pPin!=null)
        DebugWrite("Found preview pin");
      return pPin;
    }

    static public IPin FindCapturePin(ref ICaptureGraphBuilder2 captureGraphBuilder ,ref IBaseFilter videoDeviceFilter,ref Guid mediaType)
    {
      IPin pPin=null;
      Guid cat = PinCategory.Capture;
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
      if (hr>=0 && pPin!=null)
        DebugWrite("Found capture pin");
      return pPin;
    }

    public static  void ResetHardware(IAMVacCtrlProp pHardware)
    {
      int hr = 0;
      uint hardRev, pRom;

      DebugWrite("ResetHardware()");
      //hr = pHardware.reset_Msp4448G();
      hr = pHardware.put_Msp4448G(0x100030,0x2003);  //modus
      hr = pHardware.get_Msp4448G(0x13001E,out hardRev);  //modus
      hr = pHardware.get_Msp4448G(0x13001F,out pRom);  //modus

      hr = pHardware.put_Msp4448G(0x12000A,0x220);   //scart1 source per line in
      hr = pHardware.put_Msp4448G(0x12000E,0x2402);   //fmAmPrescale
      hr = pHardware.put_Msp4448G(0x120010,0x5800);   //NicAmPrescale
      hr = pHardware.put_Msp4448G(0x12000D,0x1700);   //ScartPrescale
      hr = pHardware.put_Msp4448G(0x120007,0x7301);     //scartVolume
      hr = pHardware.put_Msp4448G(0x120013,0xc00);   //abc swithces
      hr = pHardware.put_Msp4448G(0x110200,0x000);   //status
    }

    public static void SetDvdQuality(IAMVacCtrlProp pHardware)
    {
      DebugWrite("SetDVDQuality()");
      int hr;
      VideoBitRate bitrate;

      hr = pHardware.put_AudioCRC(0);
      hr = pHardware.put_AudioOutputMode(0);
      hr = pHardware.put_AudioSamplingRate(1);
      hr = pHardware.put_AudioDataRate(11);
      hr = pHardware.put_OutputType(0);

      //hr = pHardware.get_Bitrate(out bitrate);
      bitrate.bEncodingMode = BitRateMode.RC_VIDEOENCODINGMODE_VBR;
      bitrate.wBitRate = 3500;
      bitrate.dwPeak = 0;
      //hr = pHardware.put_Bitrate(bitrate);
      //hr = pHardware.get_Bitrate(out bitrate);


      hr = pHardware.put_VideoResolution(0);
      hr = pHardware.put_GOPSize(15);
      hr = pHardware.put_ClosedGop(0);
      hr = pHardware.put_InverseTelecine(0);
      hr = pHardware.put_CopyProtection(0);
      ResetHardware(pHardware);
    }

	}
}
