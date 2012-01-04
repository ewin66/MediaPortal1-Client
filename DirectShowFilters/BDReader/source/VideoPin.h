/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
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

#pragma once

#include "bdreader.h"
#include <initguid.h>

class CVideoPin : public CSourceStream, public CSourceSeeking
{
public:

  enum VIDEO_DECODER
  {
    general = 0,
    Arcsoft,
    Cyberlink
  };

  CVideoPin(LPUNKNOWN pUnk, CBDReaderFilter* pFilter, HRESULT* phr, CCritSec* pSection, CDeMultiplexer& pDemux);
  ~CVideoPin();

  STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

  // CSourceStream
  HRESULT GetMediaType(CMediaType *pMediaType);
  HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
  HRESULT CompleteConnect(IPin *pReceivePin);
  HRESULT CheckConnect(IPin *pReceivePin);
  HRESULT FillBuffer(IMediaSample *pSample);
  HRESULT BreakConnect();

  HRESULT DoBufferProcessingLoop(void);

  // CSourceSeeking
  HRESULT ChangeStart();
  HRESULT ChangeStop();
  HRESULT ChangeRate();
  HRESULT OnThreadStartPlay();
  STDMETHODIMP SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags);
  STDMETHODIMP GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest );
  STDMETHODIMP GetDuration(LONGLONG *pDuration);
  STDMETHODIMP GetCurrentPosition(LONGLONG *pCurrent);
  STDMETHODIMP Notify(IBaseFilter * pSender, Quality q);

  HRESULT DeliverBeginFlush();
  HRESULT DeliverEndFlush();

  HRESULT DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);
  
  bool IsConnected();
  void StopWait();
  void SetInitialMediaType(const CMediaType* pmt);
  void SetVideoDecoder(int format, GUID* decoder);
  void SetVC1Override(GUID* subtype);

protected:
  DWORD ThreadProc();

  void DetectVideoDecoder();
  void CreateEmptySample(IMediaSample* pSample);
  void LogMediaType(AM_MEDIA_TYPE* pmt);
  bool CompareMediaTypes(AM_MEDIA_TYPE* lhs_pmt, AM_MEDIA_TYPE* rhs_pmt);
  
  void CheckPlaybackState();
  bool CheckVideoFormat(GUID* pFormat, CLSID* pDecoder);
  CLSID GetDecoderCLSID();

  CBDReaderFilter* const m_pFilter;
  CDeMultiplexer& m_demux;
  bool      m_bConnected;
  CCritSec* m_section;

  VIDEO_DECODER m_decoderType;

  IPin* m_pReceiver;

  CMediaType m_mtInitial;

  CLSID m_VC1decoder;
  CLSID m_H264decoder;
  CLSID m_MPEG2decoder;

  GUID m_VC1Override;

  REFERENCE_TIME m_rtStreamOffset;

  Packet* m_pCachedBuffer;

  CAMEvent* m_eFlushStart;
  bool m_bFlushing;
  bool m_bSeekDone;
  bool m_bDiscontinuity;
  bool m_bFirstSample;
  bool m_bInitDuration;
  bool m_bClipEndingNotified;
  bool m_bStopWait;
  bool m_bZeroTimeStream;

  REFERENCE_TIME m_rtPrevSample;
  REFERENCE_TIME m_rtStreamTimeOffset;
  REFERENCE_TIME m_rtTitleDuration;

  bool m_bDoFakeSeek;
  bool m_bZeroStreamOffset;
};

