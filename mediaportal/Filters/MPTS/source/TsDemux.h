#pragma once
#include <map>
using namespace std;
#define AV_NOPTS_VALUE __int64(0x8000000000000000)
#define TS_PACKET_SIZE 188
#define PES_START_SIZE 9
#define MAX_PES_HEADER_SIZE (9 + 255)

class TsDemux
{
private:

	enum MpegTSState {
		MPEGTS_HEADER = 0,
		MPEGTS_PESHEADER_FILL,
		MPEGTS_PAYLOAD,
		MPEGTS_SKIP,
	};
	class MpegTSFilter 
	{
		public:
			int pid;
			int last_cc; /* last cc code (-1 if first packet) */
			MpegTSState state;
			int data_index;
			int total_size;
			int pes_header_size;
			__int64 pts, dts;
			byte header[MAX_PES_HEADER_SIZE];
			
	 };

public:
	TsDemux(void);
	virtual ~TsDemux(void);
	void ParsePacket(byte* tsPacket);

private:
	__int64 get_pts(const byte *p);
	void DecodePesPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void ParsePacket(MpegTSFilter* tss,const byte *buf, int buf_size, int is_start);
	map<int , MpegTSFilter*> m_mapFilters;
	typedef map<int , MpegTSFilter*>::iterator imapFilters;
};
