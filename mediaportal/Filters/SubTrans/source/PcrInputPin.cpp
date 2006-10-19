/*
 *	Copyright (C) 2005-2006 Team MediaPortal
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

#include <windows.h>
#include <commdlg.h>
#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include <commctrl.h>
#include <initguid.h>

#include "PcrInputPin.h"
#include "PatParser\TsHeader.h"

extern void Log( const char *fmt, ... );

const int TSPacketSize = 188;

CPcrInputPin::CPcrInputPin( CSubTransform *m_pTransform,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
								HRESULT *phr ) :

    CRenderedInputPin(NAME( "CPcrInputPin" ),
					pFilter,						// Filter
					pLock,							// Locking
					phr,							  // Return code
					L"Pcr" ),					  // Pin name
					m_pReceiveLock( pReceiveLock ),
					m_pTransform( m_pTransform )
{
	Reset();
	Log( "Pcr: Pin created" );
}

CPcrInputPin::~CPcrInputPin()
{
}


//
// CheckMediaType
//
HRESULT CPcrInputPin::CheckMediaType( const CMediaType *pmt )
{
	Log("Audio pin: CheckMediaType()");
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}


HRESULT CPcrInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  m_pDemuxerPin = pPin;
  if( m_pcrPid == -1 )
    return hr;  // PID is mapped later when we have it

  hr = MapPidToDemuxer( m_pcrPid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );

  return hr;
}


//
// Receive
//
STDMETHODIMP CPcrInputPin::Receive( IMediaSample *pSample )
{
	CAutoLock lock( m_pReceiveLock );

  if( m_pcrPid == -1 )
    return S_OK;  // Nothing to be done yet

	CheckPointer( pSample, E_POINTER );

	PBYTE pbData = NULL;

	long lDataLen = 0;

	HRESULT hr = pSample->GetPointer( &pbData );
	if( FAILED(hr) )
	{
		Log( "Pcr pin: Receive() err = %d", hr );
		return hr;
	}
	lDataLen = pSample->GetActualDataLength();

	OnRawData( pbData, lDataLen );

  return S_OK;
}


void CPcrInputPin::Reset()
{
  m_currentPTS = 0;
}

void CPcrInputPin::SetPcrPid( LONG pPid )
{
	m_pcrPid = pPid;
  MapPidToDemuxer( m_pcrPid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );
}


STDMETHODIMP CPcrInputPin::BeginFlush(void)
{
	Reset();
	return CRenderedInputPin::BeginFlush();
}


STDMETHODIMP CPcrInputPin::EndFlush(void)
{
	Reset();
	return CRenderedInputPin::EndFlush();
}


ULONGLONG CPcrInputPin::GetCurrentPTS()
{
	return m_currentPTS;
}

void CPcrInputPin::OnTsPacket( byte* tsPacket )
{
	__int64 pcrHigh;
	int     pcrLow;

  if (m_pcrPid==-1) return;
	CTsHeader header(tsPacket);
	if (header.Pid != m_pcrPid) return;
//  if (!header.PayloadUnitStart) return;

	int afc, len, flags;
	byte *p;
	unsigned int v;

	afc = (tsPacket[3] >> 4) & 3;
	if (afc <= 1)
	{
		return ;
	}
	p = tsPacket + 4;
	len = p[0];
	p++;
	if (len == 0)
	{
			return ;
	}
	flags = *p++;
	len--;
	if (!(flags & 0x10))
	{
			return ;
	}
	if (len < 6)
	{
			return ;
	}
	v = (p[0] << 24) | (p[1] << 16) | (p[2] << 8) | p[3];
	pcrHigh = ((__int64)v << 1) | (p[4] >> 7);			//33 bits
	pcrLow = ((p[4] & 1) << 8) | p[5];							//9	 bits

  m_currentPTS = pcrHigh;
}