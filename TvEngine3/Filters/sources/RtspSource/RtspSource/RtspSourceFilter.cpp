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
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "RtspSourceFilter.h"
#include "outputpin.h"
#include "ChannelInfo.h"
extern void Log(const char *fmt, ...) ;

#define BUFFER_BEFORE_PLAY_SIZE (1024L*200) //200Kb

const AMOVIESETUP_MEDIATYPE acceptOutputPinTypes =
{
	&MEDIATYPE_Stream,                  // major type
	&MEDIASUBTYPE_MPEG2_PROGRAM      // minor type
};

const AMOVIESETUP_PIN outputPin[] =
{
	{L"Out",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptOutputPinTypes},
};

const AMOVIESETUP_FILTER RtspReader =
{
	&CLSID_RtspSource,L"MediaPortal RTSP Source filter",MERIT_DO_NOT_USE,1,outputPin
};

CFactoryTemplate g_Templates[] =
{
	{L"MediaPortal RTSP Source filter",&CLSID_RtspSource,CRtspSourceFilter::CreateInstance,NULL,&RtspReader},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);




CUnknown * WINAPI CRtspSourceFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);
	CRtspSourceFilter *pNewObject = new CRtspSourceFilter(punk, phr);

	if (pNewObject == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
	}
	return pNewObject;
}

CRtspSourceFilter::CRtspSourceFilter(IUnknown *pUnk, HRESULT *phr) 
:	CSource(NAME("CRtspSource"), pUnk, CLSID_RtspSource)
,m_client(m_buffer)
,m_FilterRefList(NAME("MyFilterRefList"))
{
	wcscpy(m_fileName,L"");
  m_pOutputPin = new COutputPin(GetOwner(), this, phr, &m_section);
	m_pDemux = new Demux(&m_pids, this, &m_FilterRefList);
	m_rtStartFrom=0;
}

CRtspSourceFilter::~CRtspSourceFilter(void)
{
	m_pOutputPin->Disconnect();
	delete m_pOutputPin;
  delete m_pDemux;
}

STDMETHODIMP CRtspSourceFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	if (riid == IID_IAMFilterMiscFlags)
	{
		return GetInterface((IAMFilterMiscFlags*)this, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)this, ppv);
	}
	if ((riid == IID_IMediaPosition || riid == IID_IMediaSeeking))
	{
		return m_pOutputPin->NonDelegatingQueryInterface(riid, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);

}

CBasePin * CRtspSourceFilter::GetPin(int n)
{
    if (n == 0) 
		{
			return m_pOutputPin;
    } 
		else 
		{
        return NULL;
    }
}


int CRtspSourceFilter::GetPinCount()
{
    return 1;
}

void CRtspSourceFilter::ResetStreamTime()
{
	CRefTime cTime;
	StreamTime(cTime);
	m_tStart = REFERENCE_TIME(m_tStart) + REFERENCE_TIME(cTime);
}

HRESULT CRtspSourceFilter::OnConnect()
{
  m_buffer.SetCallback(this);
  m_patParser.Reset();
  if (m_client.Play(0.0f))
  {
    while (m_patParser.Count()==0)
    {
      Sleep(10);
    }
    CChannelInfo info;
    m_patParser.GetChannel(0,info);
    CPidTable pids=info.PidTable;
    m_pids.aud=pids.AudioPid1;
    m_pids.aud2=pids.AudioPid2;
    m_pids.ac3=pids.AC3Pid;
    m_pids.vid=pids.VideoPid;
    m_pids.pcr=pids.PcrPid;
    m_pids.pmt=pids.PmtPid;
  }
  else
  {
    return E_FAIL;
  }
  m_client.Stop();
  m_pDemux->set_ClockMode(1);
  m_pDemux->set_Auto(TRUE);
  m_pDemux->set_FixedAspectRatio(TRUE);
  m_pDemux->set_MPEG2Audio2Mode(TRUE);
  m_pDemux->AOnConnect();
  m_pDemux->SetRefClock();
  return S_OK;
}

STDMETHODIMP CRtspSourceFilter::Run(REFERENCE_TIME tStart)
{
	float milliSecs=m_rtStartFrom.Millisecs();
	milliSecs/=1000.0f;

	m_buffer.Clear();
  if (m_client.Play(milliSecs))
	{
		m_pOutputPin->UpdateStopStart();
		m_client.FillBuffer( BUFFER_BEFORE_PLAY_SIZE);
	}
	else return E_FAIL;
	m_client.Run();
	return CSource::Run(tStart);
}

STDMETHODIMP CRtspSourceFilter::Stop()
{
	HRESULT hr=CSource::Stop();
  m_client.Stop();
  m_buffer.Clear();
	return hr;
}

STDMETHODIMP CRtspSourceFilter::Pause()
{
	m_client.Pause();
  return CSource::Pause();
}

BOOL CRtspSourceFilter::is_Active(void)
{
	return ((m_State == State_Paused) || (m_State == State_Running));
}

void CRtspSourceFilter::GetStartStop(CRefTime &m_rtStart,CRefTime  &m_rtStop)
{
	m_rtStop= CRefTime(m_client.Duration());
}

void CRtspSourceFilter::Seek(CRefTime start)
{
	m_rtStartFrom=start;
}

STDMETHODIMP CRtspSourceFilter::GetDuration(REFERENCE_TIME *dur)
{
	CRefTime reftime(m_client.Duration());
	if(!dur)
		return E_INVALIDARG;
	return NOERROR;
}

STDMETHODIMP CRtspSourceFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
	wcscpy(m_fileName,pszFileName);
  //wcscpy(m_fileName,L"rtsp://192.168.1.58/test");

  if (m_client.Initialize())
  {
	  char url[MAX_PATH];
	  WideCharToMultiByte(CP_ACP,0,m_fileName,-1,url,MAX_PATH,0,0);
    if (m_client.OpenStream(url))
    {
			m_pOutputPin->UpdateStopStart();

    }
		else return E_FAIL;
  }
	else return E_FAIL;
	return S_OK;
}
STDMETHODIMP CRtspSourceFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
{
	CheckPointer(ppszFileName, E_POINTER);
	*ppszFileName = NULL;

	if (lstrlenW(m_fileName)>0)
	{
		*ppszFileName = (LPOLESTR)QzTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(m_fileName)));
		wcscpy(*ppszFileName,m_fileName);
	}
	if(pmt)
	{
		ZeroMemory(pmt, sizeof(*pmt));
		pmt->majortype = MEDIATYPE_Stream;
    pmt->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;
	}
	return S_OK;
}
ULONG CRtspSourceFilter::GetMiscFlags()
{
	return AM_FILTER_MISC_FLAGS_IS_SOURCE;
}

LONG CRtspSourceFilter::GetData(BYTE* pData, long size)
{
	if (!m_client.IsRunning()) return 0;
  DWORD bytesRead= m_buffer.ReadFromBuffer(pData, size, 0);
  return bytesRead;
}


void CRtspSourceFilter::OnTsPacket(byte* tsPacket)
{
  m_patParser.OnTsPacket(tsPacket);
}

void CRtspSourceFilter::OnRawDataReceived(BYTE *pbData, long lDataLength)
{
	OnRawData(pbData, lDataLength);
}
////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration 
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

//
// DllRegisterSever
//
// Handle the registration of this filter
//
STDAPI DllRegisterServer()
{
    return AMovieDllRegisterServer2( TRUE );

} // DllRegisterServer


//
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
    return AMovieDllRegisterServer2( FALSE );

} // DllUnregisterServer


//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

