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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "subtitlepin.h"
#include "tsfileSeek.h"

void LogDebug(const char *fmt, ...) 
{
#ifdef DEBUG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("tsreader.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);

		char buf[1000];
		sprintf(buf,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		::OutputDebugString(buf);
	}
#endif
};


const AMOVIESETUP_MEDIATYPE acceptAudioPinTypes =
{
	&MEDIATYPE_Audio,                  // major type
  &MEDIASUBTYPE_MPEG1Audio      // minor type
};
const AMOVIESETUP_MEDIATYPE acceptVideoPinTypes =
{
	&MEDIATYPE_Video,                  // major type
	&MEDIASUBTYPE_MPEG2_VIDEO      // minor type
};

const AMOVIESETUP_MEDIATYPE acceptSubtitlePinTypes =
{
  &MEDIATYPE_Stream,           // major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT      // minor type
};

const AMOVIESETUP_PIN audioVideoPin[] =
{
	{L"Audio",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptAudioPinTypes},
	{L"Video",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptVideoPinTypes},
	{L"Subtitle",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptSubtitlePinTypes}
};

const AMOVIESETUP_FILTER TSReader =
{
  &CLSID_TSReader,L"MediaPortal File Reader",MERIT_NORMAL+1000,2,audioVideoPin
};

CFactoryTemplate g_Templates[] =
{
	{L"MediaPortal File Reader",&CLSID_TSReader,CTsReaderFilter::CreateInstance,NULL,&TSReader},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);





CUnknown * WINAPI CTsReaderFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);
	CTsReaderFilter *pNewObject = new CTsReaderFilter(punk, phr);

	if (pNewObject == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
	}
	return pNewObject;
}


// Constructor
CTsReaderFilter::CTsReaderFilter(IUnknown *pUnk, HRESULT *phr) :
	CSource(NAME("CTsReaderFilter"), pUnk, CLSID_TSReader),
	m_pAudioPin(NULL),
  m_demultiplexer( m_duration, *this)
{
  m_fileReader=NULL;
  m_fileDuration=NULL;

	LogDebug("CTsReaderFilter::ctor");
	m_pAudioPin = new CAudioPin(GetOwner(), this, phr,&m_section);
	m_pVideoPin = new CVideoPin(GetOwner(), this, phr,&m_section);
  m_pSubtitlePin = new CSubtitlePin(GetOwner(), this, phr,&m_section);
  m_referenceClock= new CBaseReferenceClock("refClock",GetOwner(), phr);
  m_bSeeking=false;

	if (m_pAudioPin == NULL) 
	{
		*phr = E_OUTOFMEMORY;
		return;
	}
	wcscpy(m_fileName,L"");
  m_dwGraphRegister=0;
}

CTsReaderFilter::~CTsReaderFilter()
{
	LogDebug("CTsReaderFilter::dtor");
	HRESULT hr=m_pAudioPin->Disconnect();
	delete m_pAudioPin;

	hr=m_pVideoPin->Disconnect();
	delete m_pVideoPin;
  
	hr=m_pSubtitlePin->Disconnect();
	delete m_pSubtitlePin;
  if (m_fileReader!=NULL)
    delete m_fileReader;
  if (m_fileDuration!=NULL)
    delete m_fileDuration;
}

STDMETHODIMP CTsReaderFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	if (riid == IID_IAMFilterMiscFlags)
	{
		return GetInterface((IAMFilterMiscFlags*)this, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)this, ppv);
	}
  if (riid == IID_IAMStreamSelect)
	{
		return GetInterface((IAMStreamSelect*)this, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);

}

CBasePin * CTsReaderFilter::GetPin(int n)
{
    if (n == 0) 
		{
			return m_pAudioPin;
    } 
		else  if (n==1)
		{
        return m_pVideoPin;
    }
		//else  if (n==2)
		//{
    //    return m_pSubtitlePin;
    //}  
		else 
		{
        return NULL;
    }
}


int CTsReaderFilter::GetPinCount()
{
    return 2;
}

STDMETHODIMP CTsReaderFilter::Run(REFERENCE_TIME tStart)
{
  StartThread();
	CRefTime runTime=tStart;
	double msec=(double)runTime.Millisecs();
	msec/=1000.0;
	LogDebug("CTsReaderFilter::Run(%05.2f)",msec);
  CAutoLock cObjectLock(m_pLock);
		
  //Set our StreamTime Reference offset to zero
	HRESULT hr= CSource::Run(tStart);

  LogDebug("CTsReaderFilter::Run(%05.2f) done",msec);
  return hr;
}

STDMETHODIMP CTsReaderFilter::Stop()
{
  StopThread();
	CAutoLock cObjectLock(m_pLock);

	LogDebug("CTsReaderFilter::Stop()");
	HRESULT hr=CSource::Stop();
	return hr;
}

STDMETHODIMP CTsReaderFilter::Pause()
{
  StopThread();
	LogDebug("CTsReaderFilter::Pause()");
  CAutoLock cObjectLock(m_pLock);


  return CSource::Pause();
}

STDMETHODIMP CTsReaderFilter::GetDuration(REFERENCE_TIME *dur)
{
	if(!dur)
		return E_INVALIDARG;

	CAutoLock lock (&m_CritSecDuration);
  *dur = (REFERENCE_TIME)m_duration.Duration();

	return NOERROR;
}
STDMETHODIMP CTsReaderFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
  if (m_fileReader!=NULL)
    delete m_fileReader;
  if (m_fileDuration!=NULL)
    delete m_fileDuration;

	LogDebug("CTsReaderFilter::Load()");
  m_bSeeking=false;
	wcscpy(m_fileName,pszFileName);
  char url[MAX_PATH];
  WideCharToMultiByte(CP_ACP,0,m_fileName,-1,url,MAX_PATH,0,0);
  int length=strlen(url);	
  if ((length < 9) || (_strcmpi(&url[length-9], ".tsbuffer") != 0))
  {
    m_fileReader = new FileReader();
    m_fileDuration = new FileReader();
  }
  else
  {
    m_fileReader = new MultiFileReader();
    m_fileDuration = new MultiFileReader();
  }
	m_fileReader->SetFileName(url);
	m_fileReader->OpenFile();

  m_fileDuration->SetFileName(url);
	m_fileDuration->OpenFile();
  m_demultiplexer.SetFileReader(m_fileReader);
  m_demultiplexer.Start();

  m_duration.SetFileReader(m_fileDuration);
  m_duration.UpdateDuration();

	m_fileReader->SetFilePointer(0LL,FILE_BEGIN);

  AddGraphToRot(GetFilterGraph());

	return S_OK;
}


STDMETHODIMP CTsReaderFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
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
		pmt->subtype = MEDIASUBTYPE_MPEG2_PROGRAM;
	}
	return S_OK;
}


double CTsReaderFilter::UpdateDuration()
{
	return 0;
}

// IAMFilterMiscFlags

ULONG CTsReaderFilter::GetMiscFlags()
{
	return AM_FILTER_MISC_FLAGS_IS_SOURCE;
}


CDeMultiplexer& CTsReaderFilter::GetDemultiplexer()
{
	return m_demultiplexer;
}

double CTsReaderFilter::GetStartTime()
{
	CAutoLock lock (&m_CritSecDuration);
	return 0;
}

void CTsReaderFilter::Seek(CRefTime& seekTime)
{
  m_bSeeking=true;
  CTsFileSeek seek(m_duration);
  seek.SetFileReader(m_fileReader);
  seek.Seek(seekTime);

}


bool CTsReaderFilter::IsSeeking()
{
  return m_bSeeking;
}
void CTsReaderFilter::SeekDone()
{
  m_demultiplexer.Flush();
  m_bSeeking=false;
}
void CTsReaderFilter::SeekStart()
{
  m_bSeeking=true;
}

CAudioPin* CTsReaderFilter::GetAudioPin()
{
  return m_pAudioPin;
}
CVideoPin* CTsReaderFilter::GetVideoPin()
{
  return m_pVideoPin;
}
CSubtitlePin* CTsReaderFilter::GetSubtitlePin()
{
  return m_pSubtitlePin;
}


void CTsReaderFilter::ThreadProc()
{
  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_BELOW_NORMAL);
  while (!ThreadIsStopping(100))
	{
    CTsDuration duration;
    duration.SetFileReader(m_fileDuration);
    duration.UpdateDuration();
    m_duration.Set(duration.StartPcr(), duration.EndPcr());
    Sleep(1000);
  }
}
HRESULT CTsReaderFilter::AddGraphToRot(IUnknown *pUnkGraph) 
{
  CComPtr <IMoniker>              pMoniker;
  CComPtr <IRunningObjectTable>   pROT;
  WCHAR wsz[128];
  HRESULT hr;

  if (m_dwGraphRegister!=0) return S_OK;
  if (FAILED(GetRunningObjectTable(0, &pROT)))
      return E_FAIL;

  swprintf(wsz, L"FilterGraph %08x pid %08x\0", (DWORD_PTR) pUnkGraph, GetCurrentProcessId());
  hr = CreateItemMoniker(L"!", wsz, &pMoniker);
  if (SUCCEEDED(hr))
  {
    hr = pROT->Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, pUnkGraph, pMoniker, &m_dwGraphRegister);
  }
  return hr;
}
        

// Removes a filter graph from the Running Object Table
void CTsReaderFilter::RemoveGraphFromRot()
{
  
  if (m_dwGraphRegister==0) return;
  CComPtr <IRunningObjectTable> pROT;

  if (SUCCEEDED(GetRunningObjectTable(0, &pROT))) 
      pROT->Revoke(m_dwGraphRegister);
}
STDMETHODIMP CTsReaderFilter::Count(DWORD* streamCount)
{
  *streamCount=m_demultiplexer.GetAudioStreamCount();
  return S_OK;
}
STDMETHODIMP CTsReaderFilter::Enable(long index, DWORD flags)
{
  m_demultiplexer.SetAudioStream((int)index);
  return S_OK;
}
STDMETHODIMP CTsReaderFilter::Info( long lIndex,AM_MEDIA_TYPE **ppmt,DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk)
{
  if (pdwFlags) 
  {
    if (m_demultiplexer.GetAudioStream()==(int)lIndex)
      *pdwFlags=AMSTREAMSELECTINFO_EXCLUSIVE;
    else
      *pdwFlags=0;
  }
  if (plcid) *plcid=0;
  if (pdwGroup) *pdwGroup=0;
  if (ppObject) *ppObject=NULL;
  if (ppUnk) *ppUnk=NULL;
  if (ppszName)
  {
    char szName[20];
    m_demultiplexer.GetAudioStreamInfo((int)lIndex,szName);
    *ppszName = (WCHAR *)CoTaskMemAlloc(20);
    MultiByteToWideChar(CP_ACP,0,szName,-1,*ppszName,20);
  }
  if (ppmt)
  {
    CMediaType mediaType;
    m_demultiplexer.GetAudioStreamType((int)lIndex,mediaType);
    AM_MEDIA_TYPE* mType=(AM_MEDIA_TYPE*)(&mediaType);
    *ppmt=(AM_MEDIA_TYPE*)CoTaskMemAlloc(sizeof(AM_MEDIA_TYPE));
    memcpy(*ppmt, mType,sizeof(AM_MEDIA_TYPE));

    (*ppmt)->pbFormat=(BYTE*)CoTaskMemAlloc(mediaType.FormatLength());
    memcpy((*ppmt)->pbFormat,mType->pbFormat,mediaType.FormatLength());
  }
  return S_OK;
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

