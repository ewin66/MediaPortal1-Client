/* 
 *	Copyright (C) 2005 Team MediaPortal
 *  Author: tourettes
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

#pragma warning( disable: 4995 4996 )

#include "DVBSub.h"
#include "SubtitleInputPin.h"
#include "SubtitleOutputPin.h"

const int subtitleSizeInBytes = 720 * 576 * 3;

extern void Log(const char *fmt, ...);

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypesSubtitle =
{
	&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_DVB_SI 
};

const AMOVIESETUP_PIN sudPins[2] =
{
	{
		L"Subtitle",				  // Pin string name
		FALSE,						    // Is it rendered
		FALSE,						    // Is it an output
		FALSE,						    // Allowed none
		FALSE,						    // Likewise many
		&CLSID_NULL,				  // Connects to filter
		L"Subtitle",				  // Connects to pin
		1,							      // Number of types
		&sudPinTypesSubtitle  // Pin information
	},
	{
		L"Subtitle",				  // Pin string name
		FALSE,						    // Is it rendered
		TRUE,						      // Is it an output
		FALSE,						    // Allowed none
		FALSE,						    // Likewise many
		&CLSID_NULL,				  // Connects to filter
		L"Subtitle",				  // Connects to pin
		1,							      // Number of types
		&sudPinTypesSubtitle  // Pin information
	}
};
//
// Constructor
//
CDVBSub::CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock ) :
  CBaseFilter( NAME("MediaPortal DVBSub"), pUnk, &m_Lock, CLSID_DVBSub ),
  m_pSubtitleInputPin( NULL ),
	m_pSubDecoder( NULL ),
	m_pSubtitle( NULL )
{
	// Create subtitle decoder
	m_pSubDecoder = new CDVBSubDecoder();
	
	if( m_pSubDecoder == NULL ) 
	{
    if( phr )
	  {
      *phr = E_OUTOFMEMORY;
	  }
    return;
  }

	// Create subtitle input pin
	m_pSubtitleInputPin = new CSubtitleInputPin( this,
								GetOwner(),
								this,
								&m_Lock,
								&m_ReceiveLock,
								m_pSubDecoder, 
								phr );
    
	if ( m_pSubtitleInputPin == NULL ) 
	{
    if( phr )
		{
      *phr = E_OUTOFMEMORY;
		}
    return;
  }

	// Create subtitle output pin
	m_pSubtitleOutputPin = new CSubtitleOutputPin(
                this,
								this,
								&m_ReceiveLock,
								phr );
    
	if ( m_pSubtitleOutputPin == NULL ) 
	{
    if( phr )
		{
      *phr = E_OUTOFMEMORY;
		}
    return;
  }

	m_curSubtitleData = NULL;
	m_pSubDecoder->SetObserver( this );
	//m_pSubtitleInputPin->SetSubtitlePID( 0xfbb );
	m_pSubtitleInputPin->SetSubtitlePID( 0x403 );
}

CDVBSub::~CDVBSub()
{
	m_pSubDecoder->SetObserver( NULL );
	delete m_pSubDecoder;
	delete m_pSubtitleInputPin;
  delete m_pSubtitleOutputPin;
}

//
// GetPin
//
CBasePin * CDVBSub::GetPin( int n )
{
	if( n == 0 )
	{
		return m_pSubtitleInputPin;
	}
  if( n == 1 )
  {
    return m_pSubtitleOutputPin;
  }

  return NULL;
}

int CDVBSub::GetPinCount()
{
	return 2; // subtitle in & out
}

STDMETHODIMP CDVBSub::Run( REFERENCE_TIME tStart )
{
  CAutoLock cObjectLock( m_pLock );
	Reset();
	return CBaseFilter::Run( tStart );
}

STDMETHODIMP CDVBSub::Pause()
{
  CAutoLock cObjectLock( m_pLock );
	//Reset();
	return CBaseFilter::Pause();
}

STDMETHODIMP CDVBSub::Stop()
{
  CAutoLock cObjectLock( m_pLock );
	Reset();
	return CBaseFilter::Stop();
}

void CDVBSub::Reset()
{
	CAutoLock cObjectLock( m_pLock );

	m_pSubDecoder->Reset();
	m_pSubtitle = NULL;	// NULL the local pointer, as cache is deleted

	m_pSubtitleInputPin->Reset();
  m_pSubtitleOutputPin->Reset();

	m_firstPTS = -1;
}

void CDVBSub::Notify()
{
  // Process the subtitle ( DVB -> VOBSUB conversion & notify the output Pin )

  if( m_pSubtitleOutputPin->IsConnected() )
  {
    CComPtr<IMediaSample> pSample;
    m_pSubtitleOutputPin->GetDeliveryBuffer( &pSample, NULL, NULL, 0 );
    BYTE* pData = NULL;
    pSample->GetPointer( &pData );
    //pSample->SetActualDataLength( 999999 );
    m_pSubtitleOutputPin->Deliver( pSample );
  }
}

//
// Interface methods
//
STDMETHODIMP CDVBSub::SetSubtitlePID( ULONG pPID )
{
	if( m_pSubtitleInputPin )
	{
		m_pSubtitleInputPin->SetSubtitlePID( pPID );
    return S_OK;
	}
  else
  {
    return S_FALSE;  
  }
}

//
// CreateInstance
//
CUnknown * WINAPI CDVBSub::CreateInstance( LPUNKNOWN punk, HRESULT *phr )
{
  ASSERT( phr );
    
  CDVBSub *pFilter = new CDVBSub( punk, phr, NULL );
  if( pFilter == NULL ) 
	{
    if (phr)
		{
      *phr = E_OUTOFMEMORY;
		}
  }
  return pFilter;
}