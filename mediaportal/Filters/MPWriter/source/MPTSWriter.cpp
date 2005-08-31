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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "MPTSWriter.h"

//maxium timeshifting length
#define MAX_FILE_LENGTH (2000LL*1024LL*1024LL)  // 2 gigabyte
//#define MAX_FILE_LENGTH (10LL*1024LL*1024LL)  // 50MB

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
    &MEDIATYPE_NULL,            // Major type
    &MEDIASUBTYPE_NULL          // Minor type
};

const AMOVIESETUP_PIN sudPins =
{
    L"Input",                   // Pin string name
    FALSE,                      // Is it rendered
    FALSE,                      // Is it an output
    FALSE,                      // Allowed none
    FALSE,                      // Likewise many
    &CLSID_NULL,                // Connects to filter
    L"Output",                  // Connects to pin
    1,                          // Number of types
    &sudPinTypes                // Pin information
};

const AMOVIESETUP_FILTER sudDump =
{
    &CLSID_MPTSWriter,          // Filter CLSID
    L"MediaPortal TS Writer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};

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

	FILE* fp = fopen("MPTSWriter.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetSystemTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
#endif
};

//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
    L"MediaPortal TS Writer", &CLSID_MPTSWriter, CDump::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


// Constructor

CDumpFilter::CDumpFilter(CDump *pDump,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("WSFileWriter"), pUnk, pLock, CLSID_MPTSWriter),
    m_pDump(pDump)
{
}


//
// GetPin
//
CBasePin * CDumpFilter::GetPin(int n)
{
    if (n == 0) {
        return m_pDump->m_pPin;
    } else {
        return NULL;
    }
}


//
// GetPinCount
//
int CDumpFilter::GetPinCount()
{
    return 1;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CDumpFilter::Stop()
{
    CAutoLock cObjectLock(m_pLock);
	
	LogDebug("graph Stop() called");
	m_pDump->Log(TEXT("graph Stop() called"),true);

    if (m_pDump)
        m_pDump->CloseFile();
    
    return CBaseFilter::Stop();
}


//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CDumpFilter::Pause()
{
    CAutoLock cObjectLock(m_pLock);

    if (m_pDump)
    {
        // GraphEdit calls Pause() before calling Stop() for this filter.
        // If we have encountered a write error (such as disk full),
        // then stopping the graph could cause our log to be deleted
        // (because the current log file handle would be invalid).
        // 
        // To preserve the log, don't open/create the log file on pause
        // if we have previously encountered an error.  The write error
        // flag gets cleared when setting a new log file name or
        // when restarting the graph with Run().
        if (!m_pDump->m_fWriteError)
        {
            m_pDump->OpenFile();
        }
    }

    return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CDumpFilter::Run(REFERENCE_TIME tStart)
{
    CAutoLock cObjectLock(m_pLock);

    // Clear the global 'write error' flag that would be set
    // if we had encountered a problem writing the previous dump file.
    // (eg. running out of disk space).
    //
    // Since we are restarting the graph, a new file will be created.
    m_pDump->m_fWriteError = FALSE;

    if (m_pDump)
        m_pDump->OpenFile();
	

    return CBaseFilter::Run(tStart);
}


//
//  Definition of CDumpInputPin
//
CDumpInputPin::CDumpInputPin(CDump *pDump,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CDumpInputPin"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"Input"),                 // Pin name
    m_pReceiveLock(pReceiveLock),
    m_pDump(pDump),
    m_tLast(0)
{
	ResetPids();
	m_restBufferLen=0;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CDumpInputPin::CheckMediaType(const CMediaType *)
{
    return S_OK;
}

HRESULT CDumpInputPin::SetVideoPid(int videoPid)
{
	m_videoPid=videoPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetAudioPid(int audioPid)
{
	m_audio1Pid=audioPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetAudioPid2(int audioPid)
{
	m_audio2Pid=audioPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetAC3Pid(int ac3Pid)
{
	m_ac3Pid=ac3Pid;
	return S_OK;
}
HRESULT CDumpInputPin::SetTeletextPid(int ttxtPid)
{
	m_ttxtPid=ttxtPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetSubtitlePid(int subtitlePid)
{
	m_subtitlePid=subtitlePid;
	return S_OK;
}
HRESULT CDumpInputPin::SetPMTPid(int pmtPid)
{
	m_pmtPid=pmtPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetPCRPid(int pcrPid)
{
	m_pcrPid=pcrPid;
	return S_OK;
}


int CDumpInputPin::GetVideoPid()
{
	return m_videoPid;
}
int CDumpInputPin::GetAudioPid()
{
	return m_audio1Pid;
}
int CDumpInputPin::GetAudioPid2()
{
	return m_audio2Pid;
}
int CDumpInputPin::GetAC3Pid()
{
	return m_ac3Pid;
}
int CDumpInputPin::GetTeletextPid()
{
	return m_ttxtPid;
}
int CDumpInputPin::GetSubtitlePid()
{
	return m_subtitlePid;
}
int CDumpInputPin::GetPMTPid()
{
	return m_pmtPid;
}
int CDumpInputPin::GetPCRPid()
{
	return m_pcrPid;
}

//
// BreakConnect
//
// Break a connection
//
HRESULT CDumpInputPin::BreakConnect()
{
    if (m_pDump->m_pPosition != NULL) {
        m_pDump->m_pPosition->ForceRefresh();
    }

    return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CDumpInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CDumpInputPin::Receive(IMediaSample *pSample)
{
    CheckPointer(pSample,E_POINTER);
/*
	//TESTTEST
	ULONGLONG duration;
	m_pDump->TimeShiftBufferDuration(&duration);
	if (duration >=30LL*10000000LL)
	{
		if (false==true)
		{
			m_pDump->SetRecordingFileName("D:\\erwin\\media\\videos\\rec.ts");
			m_pDump->StartRecord(15LL*10000000LL);
		}
	}
*/
    CAutoLock lock(m_pReceiveLock);
    PBYTE pbData;

    // Has the filter been stopped yet?
    if (m_pDump->m_hFile == INVALID_HANDLE_VALUE) {
        return NOERROR;
    }

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    m_tLast = tStart;

    // Copy the data to the file

    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) {
        return hr;
    }

	if (m_pDump->IsCopyingRecordingFile())
	{
		for (int i=0; i < 100; ++i)
			m_pDump->CopyRecordingFile();
	}

	if (m_restBufferLen>0)
	{
		//LogDebug("copy last %d bytes",  (188-m_restBufferLen));
		memcpy(&m_restBuffer[m_restBufferLen], pbData, (188-m_restBufferLen));
		if(m_restBuffer[0]==0x47)
		{
			int pid=((m_restBuffer[1] & 0x1F) <<8)+m_restBuffer[2];
			if(IsPidValid(pid)==true)
			{
				hr=m_pDump->WriteRecordingFile(m_restBuffer,188);
				hr=m_pDump->WriteTimeshiftFile(m_restBuffer,188);
			}
		}

	}
	int off=0;
	for (int i=0; i < pSample->GetActualDataLength()-2*188;++i)
	{
		if (pbData[i]==0x47 && pbData[i+188]==0x47 && pbData[i+2*188]==0x47)
		{
			off=i;
			break;
		}
	}
	for(DWORD t=0;t<(DWORD)pSample->GetActualDataLength()-off;t+=188)
	{
		if(pbData[t+off]==0x47)
		{
			int pid=((pbData[t+1+off] & 0x1F) <<8)+pbData[t+2+off];
			if(IsPidValid(pid)==true)
			{
				hr=m_pDump->WriteRecordingFile(pbData+t+off,188);
				hr=m_pDump->WriteTimeshiftFile(pbData+t+off,188);
				if(FAILED(hr))
					break;
			}

		}
	}
	
	m_restBufferLen=(pSample->GetActualDataLength()-off)/188;
	m_restBufferLen *=188;
	m_restBufferLen=(pSample->GetActualDataLength()-off)-m_restBufferLen;
	if (m_restBufferLen>0 && m_restBufferLen < 188)
		memcpy(&m_restBuffer,&pbData[pSample->GetActualDataLength()-m_restBufferLen],m_restBufferLen);
	//LogDebug("copy %d bytes off:%d len:%d", m_restBufferLen,off,pSample->GetActualDataLength());
	
	m_pDump->UpdateInfoFile(false);
	m_pDump->Flush();
    return NOERROR;
}
void CDumpInputPin::ResetPids()
{
	m_videoPid=m_audio1Pid=m_audio2Pid=m_ac3Pid=m_ttxtPid=m_subtitlePid=m_pmtPid=m_pcrPid=-1;
}
bool CDumpInputPin::IsPidValid(int pid)
{
	if(pid==0 || pid==1 || pid==0x11||pid==m_videoPid || pid==m_audio1Pid ||
		pid==m_audio2Pid || pid==m_ac3Pid || pid==m_ttxtPid || pid==m_subtitlePid || 
		pid==m_pmtPid|| pid==m_pcrPid)
		return true;
	return false;
}
//
// EndOfStream
//
STDMETHODIMP CDumpInputPin::EndOfStream(void)
{
	m_restBufferLen=0;
    CAutoLock lock(m_pReceiveLock);
    return CRenderedInputPin::EndOfStream();

} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CDumpInputPin::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
    m_tLast = 0;
    return S_OK;

} // NewSegment


//
//  CDump class
//
CDump::CDump(LPUNKNOWN pUnk, HRESULT *phr) :
    CUnknown(NAME("CDump"), pUnk),
    m_pFilter(NULL),
    m_pPin(NULL),
    m_pPosition(NULL),
    m_hFile(INVALID_HANDLE_VALUE),
    m_pFileName(0),
    m_fWriteError(0),
	m_currentFilePosition(0)
{
	m_pesPid=0;
	m_hInfoFile=INVALID_HANDLE_VALUE;
	m_hFile=INVALID_HANDLE_VALUE;
	m_recHandle=INVALID_HANDLE_VALUE;

	DeleteFile("MPTSWriter.log");
    ASSERT(phr);
	m_logFileHandle=INVALID_HANDLE_VALUE;
	m_hInfoFile=INVALID_HANDLE_VALUE;
    
    m_pFilter = new CDumpFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

    m_pPin = new CDumpInputPin(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pPin == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

	m_pesStart=0LL;
	m_pesNow=0LL;
	strcpy(m_strRecordingFileName,"");
	m_recHandle=INVALID_HANDLE_VALUE;
	m_recStartPosition=0;
	m_recState=Idle;
	m_pCopyBuffer=new byte[1000*200];
}


STDMETHODIMP CDump::SetFileName(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
    // Is this a valid filename supplied

    CheckPointer(pszFileName,E_POINTER);
    if(wcslen(pszFileName) > MAX_PATH)
        return ERROR_FILENAME_EXCED_RANGE;

    // Take a copy of the filename

    m_pFileName = new WCHAR[1+lstrlenW(pszFileName)];
    if (m_pFileName == 0)
        return E_OUTOFMEMORY;

    wcscpy(m_pFileName,pszFileName);

    // Clear the global 'write error' flag that would be set
    // if we had encountered a problem writing the previous dump file.
    // (eg. running out of disk space).
    m_fWriteError = FALSE;

    // Create the file then close it



    HRESULT hr = OpenFile();
    CloseFile();
	m_pesPid=0;
/*
	//TESTTESTTEST
	//for debugging
	SetAudioPid(0x24);
	SetVideoPid(0x21);
	SetPMTPid(0x20);
*/
    return hr;

} // SetFileName


//
// GetCurFile
//
// Implemented for IFileSinkFilter support
//
STDMETHODIMP CDump::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
{
    CheckPointer(ppszFileName, E_POINTER);
    *ppszFileName = NULL;

    if (m_pFileName != NULL) 
    {
        *ppszFileName = (LPOLESTR)
        QzTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(m_pFileName)));

        if (*ppszFileName != NULL) 
        {
            wcscpy(*ppszFileName, m_pFileName);
        }
    }

    if(pmt) 
    {
        ZeroMemory(pmt, sizeof(*pmt));
        pmt->majortype = MEDIATYPE_NULL;
        pmt->subtype = MEDIASUBTYPE_NULL;
    }

    return S_OK;

} // GetCurFile


// Destructor

CDump::~CDump()
{
    CloseFile();

    delete m_pPin;
    delete m_pFilter;
    delete m_pPosition;
    delete m_pFileName;
	delete [] m_pCopyBuffer;

	if(m_logFileHandle!=INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_logFileHandle);
		m_logFileHandle=INVALID_HANDLE_VALUE;
	}
	if (m_recHandle!=INVALID_HANDLE_VALUE)
	{	
		CloseHandle(m_recHandle);
		m_recHandle=INVALID_HANDLE_VALUE;
	}
	
	if (m_hFile!=INVALID_HANDLE_VALUE)
	{	
		CloseHandle(m_hFile);
		m_hFile=INVALID_HANDLE_VALUE;
	}
	
	if (m_hInfoFile!=INVALID_HANDLE_VALUE)
	{	
		CloseHandle(m_hInfoFile);
		m_hInfoFile=INVALID_HANDLE_VALUE;
	}
}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CDump::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    CDump *pNewObject = new CDump(punk, phr);
    if (pNewObject == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return pNewObject;

} // CreateInstance


//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CDump::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv,E_POINTER);
    CAutoLock lock(&m_Lock);

    // Do we have this interface
	if (riid == IID_IMPTSWriter)
	{
		return GetInterface((IMPTSWriter*)this, ppv);
	}
	if (riid == IID_IMPTSRecord)
	{
		return GetInterface((IMPTSRecord*)this, ppv);
	}
    if (riid == IID_IFileSinkFilter) {
        return GetInterface((IFileSinkFilter *) this, ppv);
    } 
    else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) {
        return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
    } 
    else if (riid == IID_IMediaPosition || riid == IID_IMediaSeeking) {
        if (m_pPosition == NULL) 
        {

            HRESULT hr = S_OK;
            m_pPosition = new CPosPassThru(NAME("Dump Pass Through"),
                                           (IUnknown *) GetOwner(),
                                           (HRESULT *) &hr, m_pPin);
            if (m_pPosition == NULL) 
                return E_OUTOFMEMORY;

            if (FAILED(hr)) 
            {
                delete m_pPosition;
                m_pPosition = NULL;
                return hr;
            }
        }

        return m_pPosition->NonDelegatingQueryInterface(riid, ppv);
    } 

    return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface
STDMETHODIMP CDump::SetVideoPid(int pid)
{
	if (pid== m_pPin->GetVideoPid()) return S_OK;
	Log(TEXT("SetVideoPid ="),false);
	Log((__int64)pid,true);
	LogDebug("SetVideoPid:0x%x",pid);
	m_pesPid=0;
	m_pPin->SetVideoPid(pid);
	UpdateInfoFile(true);
	return S_OK;

}
STDMETHODIMP CDump::SetAudioPid(int pid)
{
	if (pid== m_pPin->GetAudioPid()) return S_OK;
	m_pesPid=0;
	Log(TEXT("SetAudioPid ="),false);
	Log((__int64)pid,true);
	m_pPin->SetAudioPid(pid);
	LogDebug("SetAudioPid:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;

}
STDMETHODIMP CDump::SetAudioPid2(int pid)
{
	if (pid== m_pPin->GetAudioPid2()) return S_OK;
	m_pesPid=0;
	Log(TEXT("SetAudioPid2 ="),false);
	Log((__int64)pid,true);
	m_pPin->SetAudioPid2(pid);
	LogDebug("SetAudioPid2:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;

}
STDMETHODIMP CDump::SetAC3Pid(int pid)
{
	if (pid== m_pPin->GetAC3Pid()) return S_OK;
	m_pesPid=0;
	Log(TEXT("SetAC3Pid ="),false);
	Log((__int64)pid,true);
	m_pPin->SetAC3Pid(pid);
	LogDebug("SetAC3Pid:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;

}
STDMETHODIMP CDump::SetTeletextPid(int pid)
{
	if (pid== m_pPin->GetTeletextPid()) return S_OK;
	Log(TEXT("SetTeletextPid ="),false);
	Log((__int64)pid,true);
	m_pPin->SetTeletextPid(pid);
	LogDebug("SetTeletextPid:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;

}
STDMETHODIMP CDump::SetSubtitlePid(int pid)
{
	if (pid== m_pPin->GetSubtitlePid()) return S_OK;
	Log(TEXT("SetSubtitlePid ="),false);
	Log((__int64)pid,true);
	m_pPin->SetSubtitlePid(pid);
	LogDebug("SetSubtitlePid:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;

}
STDMETHODIMP CDump::SetPMTPid(int pid)
{
	if (pid== m_pPin->GetPMTPid()) return S_OK;
	Log(TEXT("SetPMTPid ="),false);
	Log((__int64)pid,true);
	m_pPin->SetPMTPid(pid);
	LogDebug("SetPMTPid:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;
}

//return the amount of time present in the timeshiftbuffer in references time
STDMETHODIMP CDump::TimeShiftBufferDuration(ULONGLONG* duration)
{
	__int64 durationInPes = m_pesNow-m_pesStart;
	PTSTime time;
	PTSToPTSTime(durationInPes,&time);
	*duration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);

	return S_OK;
}

STDMETHODIMP CDump::SetPCRPid(int pid)
{
	if (pid== m_pPin->GetPCRPid()) return S_OK;
	Log(TEXT("SetPCRPid ="),false);
	Log((__int64)pid,true);
	m_pPin->SetPCRPid(pid);
	LogDebug("SetPCRPid:0x%x",pid);
	UpdateInfoFile(true);
	return S_OK;
}

STDMETHODIMP CDump::ResetPids()
{
	LogDebug("Reset Pids");
	m_pFilter->Stop();
	LONG val;
	m_pPin->ResetPids();
	m_currentFilePosition=0;
	SetFilePointer(m_hFile,0,&val,FILE_BEGIN);
	SetEndOfFile(m_hFile);
	OpenFile();
	m_pFilter->Run(0);
	return S_OK;
}
//
// OpenFile
//
// Opens the file ready for dumping
//
HRESULT CDump::OpenFile()
{
	LogDebug("OpenFile");
    TCHAR *pFileName = NULL;

    // Is the file already opened
    if (m_hFile != INVALID_HANDLE_VALUE) {
        return NOERROR;
    }

    // Has a filename been set yet
    if (m_pFileName == NULL) {
        return ERROR_INVALID_NAME;
    }

    // Convert the UNICODE filename if necessary

#if defined(WIN32) && !defined(UNICODE)
    char convert[MAX_PATH];

    if(!WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,convert,MAX_PATH,0,0))
        return ERROR_INVALID_NAME;

    pFileName = convert;
#else
    pFileName = m_pFileName;
#endif

    // Try to open the file
	DeleteFile((LPCTSTR) pFileName);
    m_hFile = CreateFile((LPCTSTR) pFileName,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ ,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);

    if (m_hFile == INVALID_HANDLE_VALUE) 
    {
        DWORD dwErr = GetLastError();
        return HRESULT_FROM_WIN32(dwErr);
    }
	
	TCHAR logFile[512];
	strcpy(logFile, pFileName);
	strcat(logFile,"w.log");
#if DEBUG
	DeleteFile((LPCTSTR) logFile);
	m_logFileHandle=CreateFile((LPCTSTR)logFile,GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ ,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);
	logFilePos=0;
#endif

	TCHAR infoFile[512];
	strcpy(infoFile, pFileName);
	strcat(infoFile, ".info");

	DeleteFile((LPCTSTR) infoFile);
    m_hInfoFile = CreateFile((LPCTSTR) infoFile,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);



	UpdateInfoFile(true);
	return S_OK;

} // Open


//
// CloseFile
//
// Closes any dump file we have opened
//
HRESULT CDump::CloseFile()
{
    // Must lock this section to prevent problems related to
    // closing the file while still receiving data in Receive()
    CAutoLock lock(&m_Lock);

    if (m_hFile != INVALID_HANDLE_VALUE)
	{
		LogDebug("CloseFile()");

		Log(TEXT("CloseFile called"),true);
		LARGE_INTEGER li;
		li.QuadPart = m_currentFilePosition;

		SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);

		SetEndOfFile(m_hFile);

		m_currentFilePosition= 0;
		CloseHandle(m_hFile);
		m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file 
	}

    if (m_hInfoFile != INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_hInfoFile);
		m_hInfoFile = INVALID_HANDLE_VALUE;
	}

	if (m_recHandle != INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_recHandle);
		m_recHandle = INVALID_HANDLE_VALUE;
	}


	TCHAR *pFileName = NULL;
#if defined(WIN32) && !defined(UNICODE)
    char convert[MAX_PATH];

    if(!WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,convert,MAX_PATH,0,0))
        return ERROR_INVALID_NAME;

    pFileName = convert;
#else
    pFileName = m_pFileName;
#endif

	DeleteFile((LPCSTR)m_pFileName);

	TCHAR infoFile[512];
	strcpy(infoFile, pFileName);
	strcat(infoFile, ".info");

	DeleteFile(infoFile);

    return NOERROR;

} // Open

bool CDump::IsCopyingRecordingFile()
{
	if (m_recHandle==INVALID_HANDLE_VALUE) return false;
	if (m_recState!=Copying) return false;
	return true;
}

HRESULT CDump::CopyRecordingFile()
{
	if (m_recHandle==INVALID_HANDLE_VALUE) return S_OK;
	if (m_recState!=Copying) return S_OK;

	//copy next 100K
	DWORD         dwBytesRead;
	DWORD		  dwBytesToRead=1000*188;
	LARGE_INTEGER li;

	//get the filesize
	LARGE_INTEGER	liFileSize;
	::GetFileSizeEx(m_hFile,&liFileSize);
	__int64         fileSize=liFileSize.QuadPart;

	//set filepointer
	li.QuadPart = m_recStartPosition;
	HRESULT hr=SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);

	//determine how many bytes we can copy
	if (m_recStartPosition < m_currentFilePosition)
	{
		if (m_recStartPosition+dwBytesToRead >= m_currentFilePosition)
		{
			dwBytesToRead = (DWORD)(m_currentFilePosition-m_recStartPosition);
			m_recState=Following;
		}
	}
	else 
	{
		if (fileSize>=MAX_FILE_LENGTH)
		{
			if (m_recStartPosition+dwBytesToRead >= fileSize)
				dwBytesToRead = (DWORD)(fileSize-m_recStartPosition);
		}
	}

	//copy bytes from timeshift file->recording file
	if (ReadFile(m_hFile,m_pCopyBuffer,dwBytesToRead,&dwBytesRead,NULL))
	{
		DWORD dwBytesWritten;
		if (WriteFile(m_recHandle,m_pCopyBuffer,dwBytesRead,&dwBytesWritten,NULL))
		{
			m_recStartPosition+=dwBytesWritten;
			if (m_recStartPosition>=fileSize && fileSize >=MAX_FILE_LENGTH)
				m_recStartPosition=0;
		}
	}

	//restore file pointer
	li.QuadPart=m_currentFilePosition;
	SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);
	return S_OK;
}

// Write
//
// Write raw data to the recording file
//
HRESULT CDump::WriteRecordingFile(PBYTE pbData, LONG lDataLength)
{
	
	DWORD written = 0;
	if (m_recHandle==INVALID_HANDLE_VALUE) return S_OK;
	if (lDataLength<=0) return S_OK;
	if (pbData==NULL) return S_OK;
	if (m_recState!=Following) return S_OK;
	WriteFile(m_recHandle, pbData, lDataLength, &written, NULL);
	return S_OK;
}

// Write
//
// Write raw data to the timeshift file
//
HRESULT CDump::WriteTimeshiftFile(PBYTE pbData, LONG lDataLength)
{
	if (lDataLength<=0) return S_OK;
	if (pbData==NULL) return S_OK;
	DWORD written = 0;
    //write to live.ts file
    if (m_hFile == INVALID_HANDLE_VALUE)
	{
        Log(TEXT("Write: m_hFile is invalid"),true);
		return S_FALSE;
    }
	HRESULT hr = S_OK;
	LARGE_INTEGER li,listart;
	li.QuadPart = m_currentFilePosition;
	listart.QuadPart = m_currentFilePosition;

//	hr=LockFile(m_hFile,listart.LowPart,listart.HighPart,lDataLength,0);
//	if (FAILED(hr)) LogDebug("failed to lock file at %x size:%x", m_currentFilePosition,lDataLength);
	hr=SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);
	if (FAILED(hr)) LogDebug("failed to set filepointer at %x size:%x", m_currentFilePosition,lDataLength);
	hr=WriteFile(m_hFile, pbData, lDataLength, &written, NULL);
	if (FAILED(hr)) LogDebug("failed to write %x size:%x", m_currentFilePosition,lDataLength);
//	hr=UnlockFile(m_hFile,listart.LowPart,listart.HighPart,lDataLength,0);
//	if (FAILED(hr)) LogDebug("failed to unlock file at %x size:%x", m_currentFilePosition,lDataLength);

	if (written!=lDataLength) LogDebug("only wrote %x of %x bytes", written,lDataLength);
	m_currentFilePosition+=written;

	if (m_currentFilePosition> MAX_FILE_LENGTH)
	{
		LogDebug("end of file reached, back to 0");
		m_currentFilePosition=0;
	}

	//update PES
	TSHeader header;
	GetTSHeader(pbData,&header);
	int pidToCheck = ( m_pPin->GetAudioPid()>0 ? m_pPin->GetAudioPid():m_pPin->GetAC3Pid() );
	if (pidToCheck==header.Pid || (header.Pid>0 && header.Pid==m_pPin->GetVideoPid() ) || (header.Pid>0 && header.Pid==m_pPin->GetPCRPid() ) )
	{
		int offset=4;
		if(header.AdaptionControl==1 || header.AdaptionControl==3)
			offset+=pbData[4];
		//LogDebug("pes packet check adpt:%x sync:%x %x %x %x %d",header.AdaptionControl,header.SyncByte,pbData[offset],pbData[offset+1],pbData[offset+2],offset);
		
		if(header.SyncByte==0x47 && pbData[offset]==0 && pbData[offset+1]==0 && pbData[offset+2]==1)
		{
			PESHeader pes;
			GetPESHeader(&pbData[offset+6],&pes);
			
			if(pes.Reserved==0x02) // valid header
			{
				if(pes.PTSFlags==0x02)
				{
					if (m_pesPid==0)
						m_pesPid=header.Pid;
					if (m_pesPid==header.Pid)
					{
						// audio pes found
						ULONGLONG ptsValue =0;
						GetPTS(&pbData[offset+9],&ptsValue);
						m_pesNow=ptsValue;
						if (m_pesStart==0) m_pesStart=ptsValue;
					}
				}	
			}
		}
	}
	return S_OK;
}
void CDump::GetPTS(BYTE *data,ULONGLONG *pts)
{
	*pts= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));
}
void CDump::GetTSHeader(BYTE *data,TSHeader *header)
{
	header->SyncByte=data[0];
	header->TransportError=(data[1] & 0x80)>0?true:false;
	header->PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	header->TransportPriority=(data[1] & 0x20)>0?true:false;
	header->Pid=((data[1] & 0x1F) <<8)+data[2];
	header->TScrambling=data[3] & 0xC0;
	header->AdaptionControl=(data[3]>>4) & 0x3;
	header->ContinuityCounter=data[3] & 0x0F;
}

void CDump::GetPESHeader(BYTE *data,PESHeader *header)
{
	header->Reserved=(data[0] & 0xC0)>>6;
	header->ScramblingControl=(data[0] &0x30)>>4;
	header->Priority=(data[0] & 0x08)>>3;
	header->dataAlignmentIndicator=(data[0] & 0x04)>>2;
	header->Copyright=(data[0] & 0x02)>>1;
	header->Original=data[0] & 0x01;
	header->PTSFlags=(data[1] & 0xC0)>>6;
	header->ESCRFlag=(data[1] & 0x20)>>5;
	header->ESRateFlag=(data[1] & 0x10)>>4;
	header->DSMTrickModeFlag=(data[1] & 0x08)>>3;
	header->AdditionalCopyInfoFlag=(data[1] & 0x04)>>2;
	header->PESCRCFlag=(data[1] & 0x02)>>1;
	header->PESExtensionFlag=data[1] & 0x01;
	header->PESHeaderDataLength=data[2];
}

HRESULT CDump::UpdateInfoFile(bool pids)
{
	if (m_hInfoFile==INVALID_HANDLE_VALUE) 
	{
		LogDebug("UpdatePids() filehandle=closed");
		return S_OK;
	}

	__int64 key = (m_currentFilePosition/188) / 100;
	key++;
	imapPES it= m_mapPES.find(key);
	if (it != m_mapPES.end())
		m_pesStart = it->second;

	//update the info file
	LARGE_INTEGER li;
	DWORD written = 0;
	li.QuadPart = 0;
	//LockFile(m_hInfoFile,0,0,8+8*sizeof(int),0);
	SetFilePointer(m_hInfoFile,li.LowPart,&li.HighPart,FILE_BEGIN);
	WriteFile(m_hInfoFile, &m_currentFilePosition, sizeof(m_currentFilePosition), &written, NULL);
	WriteFile(m_hInfoFile, &m_pesStart, sizeof(m_pesStart), &written, NULL);
	WriteFile(m_hInfoFile, &m_pesNow, sizeof(m_pesNow), &written, NULL);
	if (pids)
	{
		LogDebug("UpdatePids() ac3:0x%x audio:0x%x audio2:0x%x video:0x%x ttx:0x%x pmt:0x%x subtitle:0x%x pcr:0x%x",
			m_pPin->GetAC3Pid(),m_pPin->GetAudioPid(),m_pPin->GetAudioPid2(),m_pPin->GetVideoPid(),
			m_pPin->GetTeletextPid(),m_pPin->GetPMTPid(),m_pPin->GetSubtitlePid(),m_pPin->GetPCRPid());

		int pid=m_pPin->GetAC3Pid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetAudioPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetAudioPid2();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetVideoPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetTeletextPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetPMTPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetSubtitlePid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		pid=m_pPin->GetPCRPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
		m_currentFilePosition=0;
		LARGE_INTEGER li;
		li.QuadPart = 0;
		SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);
		SetEndOfFile(m_hFile);
		m_pesStart=m_pesNow=0;
	}
	//UnlockFile(m_hInfoFile,0,0,8+8*sizeof(int),0);
	return S_OK;
}



void CDump::Flush()
{
	__int64 key;
	key = (m_currentFilePosition/188) / 100;
	m_mapPES[key] = m_pesNow;
}


HRESULT CDump::HandleWriteFailure(void)
{
    DWORD dwErr = GetLastError();

    if (dwErr == ERROR_DISK_FULL)
    {
        // Close the dump file and stop the filter, 
        // which will prevent further write attempts
        Log(TEXT("error_disk_full happend"),true);
		m_pFilter->Stop();

        // Set a global flag to prevent accidental deletion of the dump file
        m_fWriteError = TRUE;

        // Display a message box to inform the developer of the write failure
    }

    return HRESULT_FROM_WIN32(dwErr);
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
STDMETHODIMP CDump::Log(__int64 value,bool crlf)
{
	char buffer[100];
	return Log(_i64toa(value,buffer,10),crlf);
}
STDMETHODIMP CDump::Log(char* text,bool crlf)
{
	CAutoLock lock(&m_Lock);
	if(m_logFileHandle==INVALID_HANDLE_VALUE)
		return S_FALSE;

	char _crlf[2];
	_crlf[0]=(char)13;
	_crlf[1]=(char)10;

	DWORD written=0;
	DWORD len=strlen(text);
	LARGE_INTEGER li;
	li.QuadPart = (LONGLONG)logFilePos;
	SetFilePointer(m_logFileHandle,li.LowPart,&li.HighPart,FILE_BEGIN);
	WriteFile(m_logFileHandle, text, len, &written, NULL);
	logFilePos+=(__int64)written;
	if(crlf)
	{
		written=0;
		WriteFile(m_logFileHandle, _crlf, 2, &written, NULL);
		logFilePos+=(__int64)written;
	}
	return S_OK;
}


STDMETHODIMP CDump::SetRecordingFileName(char* pszFileName)
{
	strcpy(m_strRecordingFileName,pszFileName);
	return S_OK;
}

void CDump::WriteAttributesToRecordingFile()
{
	if (m_recHandle!=INVALID_HANDLE_VALUE) return;
	if (m_mapAttributes.size()==0) return;//no attributes

	byte buf[200];
	buf[0]=0x47; //syncbyte
	buf[1]=0x00; //TransportError/TransportPriority/Pid Hi
	buf[2]=0x12; //pid Lo
	buf[3]=0;	 //TScrambling/AdaptionControl/ContinuityCounter
	buf[4]=0xFF; //special 
	buf[5]=0;    //len
	int pos=6;
	imapAttributes it = m_mapAttributes.begin();
	DWORD written;
	while (it != m_mapAttributes.end())
	{
		int attribNo	  =it->first;
		string attribValue=it->second;
		if (pos+10 < 188)
		{
			int sizeValue=attribValue.size();
			buf[pos  ]=((attribNo>>24)&0xff);
			buf[pos+1]=((attribNo>>16)&0xff);
			buf[pos+2]=((attribNo>>8) &0xff);
			buf[pos+3]=((attribNo   ) &0xff);
			buf[pos+4]=(sizeValue>>8) &0xff;
			buf[pos+5]=(sizeValue   ) &0xff;
			pos+=6;
			for (int x=0; x < sizeValue;++x)
			{
				buf[pos]= (attribValue.c_str())[x];
				pos++;
				if (pos>=188)
				{
					buf[5]=pos;

					//write
					WriteFile(m_recHandle, buf, 188, &written, NULL);
					pos=6;
				}
			}
		}
		else
		{
			buf[5]=pos;
			//write
			WriteFile(m_recHandle, buf, 188, &written, NULL);
			pos=6;
		}
		++it;
	}
	if (pos>6)
	{
		buf[5]=pos;
		//write
		WriteFile(m_recHandle, buf, 188, &written, NULL);
	}
}

STDMETHODIMP CDump::StartRecord( ULONGLONG timeFromTimeshiftBuffer)
{
	if (m_recHandle!=INVALID_HANDLE_VALUE) return E_FAIL;
	if (strlen(m_strRecordingFileName)==0) return E_FAIL;

	LogDebug("Start Recording:'%s'",m_strRecordingFileName);
	m_recHandle = CreateFile((LPCTSTR) m_strRecordingFileName,
								GENERIC_READ | GENERIC_WRITE,
								FILE_SHARE_READ | FILE_SHARE_WRITE,
								NULL,
								CREATE_ALWAYS,
								FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
								NULL);

	if (m_recHandle==INVALID_HANDLE_VALUE) 
	{
		LogDebug("Start Record unable to create file:%d",GetLastError());
		return E_FAIL;
	}


	WriteAttributesToRecordingFile();
	
	//get the total seconds present in the timeshift buffer
	ULONGLONG duration;
	TimeShiftBufferDuration(&duration);

	if (duration>0)
	{
		//get the filesize
		LARGE_INTEGER	liFileSize;
		::GetFileSizeEx(m_hFile,&liFileSize);
		__int64         fileSize=liFileSize.QuadPart;

		//calculate start position
		__int64			position=0;
		position=(fileSize/100LL)* ( ( (duration-timeFromTimeshiftBuffer)*100LL)/ duration);
		if (fileSize>=MAX_FILE_LENGTH)
			position += m_currentFilePosition;
		if(position>fileSize)
			position -= fileSize;
		
		if(position<1)
			position=0;

		if(position>0) position=(position/188)*188;
		m_recStartPosition=position;
		m_recState=Copying;
	}
	else
	{
		m_recState=Following;
	}
	
	return S_OK;

}	
STDMETHODIMP CDump::StopRecord( ULONGLONG startTime)
{
	if (m_recHandle==INVALID_HANDLE_VALUE) return S_OK;

	LogDebug("Stop Recording:'%s'",m_strRecordingFileName);
	CloseHandle(m_recHandle);
	m_recHandle = INVALID_HANDLE_VALUE;
	m_recState=Idle;
	m_recStartPosition=0;
	strcpy(m_strRecordingFileName,"");
	m_mapAttributes.clear();
	return S_OK;
}

STDMETHODIMP CDump::SetAttribute(int attribNo, char* attribValue)
{
	m_mapAttributes[attribNo]= attribValue;
	return S_OK;
}

void CDump::PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime)
{
	PTSTime time;
	ULONG  _90khz = (ULONG)(pts/90);
	time.h=(_90khz/(1000*60*60));
	time.m=(_90khz/(1000*60))-(time.h*60);
	time.s=(_90khz/1000)-(time.h*3600)-(time.m*60);
	time.u=_90khz-(time.h*1000*60*60)-(time.m*1000*60)-(time.s*1000);
	*ptsTime=time;
}