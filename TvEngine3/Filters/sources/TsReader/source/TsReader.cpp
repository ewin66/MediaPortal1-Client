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
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"
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
	&MEDIASUBTYPE_MPEG2_AUDIO      // minor type
};
const AMOVIESETUP_MEDIATYPE acceptVideoPinTypes =
{
	&MEDIATYPE_Audio,                  // major type
	&MEDIASUBTYPE_MPEG2_VIDEO      // minor type
};

const AMOVIESETUP_PIN audioVideoPin[] =
{
	{L"Audio",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptAudioPinTypes},
	{L"Video",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptVideoPinTypes}
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
  m_referenceClock= new CBaseReferenceClock("refClock",GetOwner(), phr);
  m_bSeeking=false;

	if (m_pAudioPin == NULL) 
	{
		*phr = E_OUTOFMEMORY;
		return;
	}
	wcscpy(m_fileName,L"");
}

CTsReaderFilter::~CTsReaderFilter()
{
	LogDebug("CTsReaderFilter::dtor");
	HRESULT hr=m_pAudioPin->Disconnect();
	delete m_pAudioPin;

	hr=m_pVideoPin->Disconnect();
	delete m_pVideoPin;
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
	/*if ((riid == IID_IMediaPosition || riid == IID_IMediaSeeking))
	{
    if (m_pAudioPin->IsConnected())
    {
		  return m_pAudioPin->NonDelegatingQueryInterface(riid, ppv);
    }
    else
    {
		  return m_pVideoPin->NonDelegatingQueryInterface(riid, ppv);
    }
	}
  if (riid == IID_IReferenceClock || riid == IID_IReferenceClock2)
	{
    return m_referenceClock->NonDelegatingQueryInterface(riid, ppv);
	}*/

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

  m_duration.SetFileReader(m_fileDuration);
  m_duration.UpdateDuration();

	m_fileReader->SetFilePointer(0LL,FILE_BEGIN);
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

void CTsReaderFilter::ThreadProc()
{
  while (!ThreadIsStopping(100))
	{
    CTsDuration duration;
    duration.SetFileReader(m_fileDuration);
    duration.UpdateDuration();
    m_duration.Set(duration.StartPcr(), duration.EndPcr());
    Sleep(1000);
  }
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
