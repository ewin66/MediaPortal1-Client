/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#include <streams.h>
#include "tsreader.h"
#include "audiopin.h"

byte MPEG1AudioFormat[] = 
{
  0x50, 0x00,				//wFormatTag
  0x02, 0x00,				//nChannels
  0x80, 0xBB,	0x00, 0x00, //nSamplesPerSec
  0x00, 0x7D,	0x00, 0x00, //nAvgBytesPerSec
  0x00, 0x03,				//nBlockAlign
  0x00, 0x00,				//wBitsPerSample
  0x16, 0x00,				//cbSize
  0x02, 0x00,				//wValidBitsPerSample
  0x00, 0xE8,				//wSamplesPerBlock
  0x03, 0x00,				//wReserved
  0x01, 0x00,	0x01,0x00,  //dwChannelMask
  0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00

};
extern void LogDebug(const char *fmt, ...) ;

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
	m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinAudio"),pUnk,phr,section),
	m_section(section)
{
	m_refStartTime=m_rtStart;
	m_bDropPackets=false;
  m_bDropSeek=false;
  m_bConnected=false;
	m_rtStart=0;
	m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute	|
	AM_SEEKING_CanSeekForwards	|
	AM_SEEKING_CanSeekBackwards	|
	AM_SEEKING_CanGetStopPos	|
	AM_SEEKING_CanGetDuration	;//|
	//AM_SEEKING_Source;
}

CAudioPin::~CAudioPin()
{
	LogDebug("pin:dtor()");
}
STDMETHODIMP CAudioPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	if (riid == IID_IAsyncReader)
  {
		int x=1;
	}
  if (riid == IID_IMediaSeeking)
  {
    return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  if (riid == IID_IMediaPosition)
  {
		return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CAudioPin::GetMediaType(CMediaType *pmt)
{

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Audio);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();
	pmt->SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));

	return S_OK;
}

HRESULT CAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;


	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
			pRequest->cBuffers = 1;
	}

	pRequest->cbBuffer = 0x10000;


	ALLOCATOR_PROPERTIES Actual;
	hr = pAlloc->SetProperties(pRequest, &Actual);
	if (FAILED(hr))
	{
			return hr;
	}

	if (Actual.cbBuffer < pRequest->cbBuffer)
	{
			return E_FAIL;
	}

	return S_OK;
}

HRESULT CAudioPin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("pin:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		LogDebug("pin:CompleteConnect() done");
    m_bConnected=true;
	}
	else
	{
		LogDebug("pin:CompleteConnect() failed:%x",hr);
	}
  REFERENCE_TIME refTime;
  m_pTsReaderFilter->GetDuration(&refTime);
  m_rtDuration=CRefTime(refTime);
	return hr;
}

HRESULT CAudioPin::BreakConnect()
{
  m_bConnected=false;
  return CSourceStream::BreakConnect();
}

HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
  if (m_pTsReaderFilter->IsSeeking())
	{
		Sleep(1);
		return NOERROR;
	}
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CBuffer* buffer=demux.GetAudio();
  if (m_bDiscontinuity)
  {
    LogDebug("aud:set discontinuity");
    pSample->SetDiscontinuity(TRUE);
    m_bDiscontinuity=FALSE;
  }
  if (buffer!=NULL)
  {
    BYTE* pSampleBuffer;
    CRefTime cRefTime;
    if (buffer->MediaTime(cRefTime))
    {
      cRefTime-=m_rtStart;
      REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
      pSample->SetTime(&refTime,NULL);  
      pSample->SetSyncPoint(TRUE);
      float fTime=(float)cRefTime.Millisecs();
      fTime/=1000.0f;
      LogDebug("aud:%f", fTime);
    }
	  pSample->SetActualDataLength(buffer->Length());
    pSample->GetPointer(&pSampleBuffer);
    memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
    delete buffer;
    return NOERROR;
  }
  else
  {
    LogDebug("aud:no buffer");
	  pSample->SetActualDataLength(0);
  }
}


bool CAudioPin::IsConnected()
{
  return m_bConnected;
}
// CMediaSeeking
HRESULT CAudioPin::ChangeStart()
{
    UpdateFromSeek();
	return S_OK;
}
HRESULT CAudioPin::ChangeStop()
{
    UpdateFromSeek();
	return S_OK;
}
HRESULT CAudioPin::ChangeRate()
{
	return S_OK;
}


HRESULT CAudioPin::OnThreadStartPlay()
{    
  m_bDiscontinuity=TRUE;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;
  LogDebug("aud:OnThreadStartPlay(%f)", fStart);
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	return CSourceStream::OnThreadStartPlay( );
}

void CAudioPin::SetStart(CRefTime rtStartTime)
{
}
STDMETHODIMP CAudioPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{/*
	REFERENCE_TIME rtStop = *pStop;
	REFERENCE_TIME rtCurrent = *pCurrent;
	if (CurrentFlags & AM_SEEKING_RelativePositioning)
	{
		rtCurrent += m_rtStart;
		CurrentFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
		CurrentFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
	}
	if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
	{
		m_rtStart = rtCurrent;
	}
  
	if (StopFlags & AM_SEEKING_RelativePositioning)
	{
		rtStop += m_rtStop;
		StopFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
		StopFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
	}
	if (!(CurrentFlags & AM_SEEKING_NoFlush) && (CurrentFlags & AM_SEEKING_PositioningBitsMask))
  {
    m_pTsReaderFilter->SeekStart();
    CRefTime rtSeek=rtCurrent;
    float seekTime=rtSeek.Millisecs();
    seekTime/=1000.0f;
    LogDebug("seek to %f", seekTime);
  				
    m_rtStart = rtCurrent;
    
	  if (m_pTsReaderFilter->IsActive())
	  {
		  DeliverBeginFlush();
	  }

	  CSourceStream::Stop();

    m_pTsReaderFilter->Seek(CRefTime(rtCurrent));
    
		if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
		{
			m_rtStart = rtCurrent;
		}
	  if (m_pTsReaderFilter->IsActive())
	  {
		  DeliverEndFlush();
	  }
    m_pTsReaderFilter->SeekDone();

    m_bDiscontinuity=TRUE;
    CSourceStream::Run();

	  if (CurrentFlags & AM_SEEKING_ReturnTime)
    {
      *pCurrent=rtCurrent;
    }
			
    return CSourceSeeking::SetPositions(&rtCurrent, CurrentFlags, pStop, StopFlags);

  }*/
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

void CAudioPin::UpdateFromSeek()
{
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);
    CRefTime rtSeek=m_rtStart;
    float seekTime=rtSeek.Millisecs();
    seekTime/=1000.0f;
    LogDebug("aud seek to %f", seekTime);
    if (ThreadExists()) 
    {
        // next time around the loop, the worker thread will
        // pick up the position change.
        // We need to flush all the existing data - we must do that here
        // as our thread will probably be blocked in GetBuffer otherwise
        
        m_pTsReaderFilter->SeekStart();
        DeliverBeginFlush();
        // make sure we have stopped pushing
        Stop();
        m_pTsReaderFilter->Seek(CRefTime(m_rtStart));

        // complete the flush
        DeliverEndFlush();
        m_pTsReaderFilter->SeekDone();
  
        // restart
        m_rtStart=rtSeek;
        Run();
    }
}
