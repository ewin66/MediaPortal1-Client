/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Agree
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

#include <streams.h>
#include <bdaiface.h>
#include "SplitterSetup.h"
#include <commctrl.h>
extern void Log(const char *fmt, ...) ;

SplitterSetup::SplitterSetup(Sections *pSections) :
m_demuxSetupComplete(FALSE),m_pSectionsPin(NULL)
{
	m_pSections = pSections;
}

SplitterSetup::~SplitterSetup()
{
	if(m_pSectionsPin!=NULL)
		m_pSectionsPin->Release();
}
HRESULT SplitterSetup::SetDemuxPins(IFilterGraph *pGraph)
{

	if(m_demuxSetupComplete==TRUE)
		return S_FALSE;

	if(pGraph==NULL)
		return S_FALSE;

	HRESULT hr;
	IGraphBuilder *pGB=NULL;

	if(FAILED(pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGB)))
	{
		return S_FALSE;
	}

	IBaseFilter *pDemuxer;
	hr=pGB->FindFilterByName(L"MPEG-2 Demultiplexer",&pDemuxer);
	if(FAILED(hr))
	{
		pGB->Release();
		return hr;
	}

	hr=SetupDemuxer(pDemuxer);
	pGB->Release();
	pDemuxer->Release();

	return NOERROR;
}
HRESULT SplitterSetup::SetupDemuxer(IBaseFilter *demuxFilter)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	HRESULT hr=0;

	if(demuxFilter==NULL)
		return S_FALSE;

	IMpeg2Demultiplexer *demuxer=NULL;

	hr=demuxFilter->QueryInterface(IID_IMpeg2Demultiplexer,(void**)&demuxer);
	if(FAILED(hr))
		return hr;

	m_pSectionsPin=NULL;
	m_demuxSetupComplete=true;
	demuxer->Release();
	return S_OK;
}

HRESULT SplitterSetup::SetupPins(IPin *pPin)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	int				maxCounter;
	
	HRESULT hr=0;

	
			
	Log("Setup pins");

	// video

	if(pPin==NULL)
		return S_FALSE;

	hr=pPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr) || pPidEnum==NULL)
		return 7;
		
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		if (count!=1) break;
			
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
		{	
			Log("failed to unmap pids");
			return 8;
		}
	}
	pPidEnum->Release();
	
	Log("map pid 0x0");
	pid = (ULONG)0;// pat
	hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
	if(FAILED(hr))
	{
		Log("failed to map pid 0x0");
		return 4;
	}
	Log("map pid 0x11");
	pid = (ULONG)0x11;// sdt
	hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
	if(FAILED(hr))
	{
		Log("failed to map pid 0x11");
		return 4;
	}
	Log("map pid 0x1ffb");
	pid = (ULONG)0x1ffb;// ATSC
	hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); 
	if(FAILED(hr))
	{	
		Log("failed to map pid 0x1ffb");
		return 4;
	}
	pMap->Release();
	return S_OK;
}
HRESULT SplitterSetup::MapAdditionalPID(ULONG pid)
{
	IMPEG2PIDMap	*pMap=NULL;
	
	HRESULT hr=0;

	if(m_pSectionsPin==NULL)
		return S_FALSE;

	hr=m_pSectionsPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
	if(FAILED(hr))
		return 4;

	pMap->Release();
	return S_OK;
}
HRESULT SplitterSetup::MapAdditionalPayloadPID(ULONG pid)
{
	IMPEG2PIDMap	*pMap=NULL;
	
	HRESULT hr=0;

	if(m_pSectionsPin==NULL)
		return S_FALSE;

	hr=m_pSectionsPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->MapPID(1,&pid,MEDIA_TRANSPORT_PAYLOAD); // tv
	if(FAILED(hr))
		return 4;

	pMap->Release();
	return S_OK;
}
bool SplitterSetup::PinIsNULL()
{
	return (m_pSectionsPin==NULL);
}
HRESULT SplitterSetup::SetPin(IPin *ppin)
{
	if(m_pSectionsPin==NULL)
		m_pSectionsPin=ppin;
	return S_OK;
}
HRESULT SplitterSetup::UnMapAllPIDs()
{
	if(m_pSectionsPin==NULL)
	{
		return S_FALSE;
	}

	SetupPins(m_pSectionsPin);
	return S_OK;
}
HRESULT SplitterSetup::GetPSIMedia(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_MPEG2_SECTIONS;
	pintype->subtype = MEDIASUBTYPE_DVB_SI;

	return S_OK;
}