#include <streams.h>
#include <bdatypes.h>
#include <time.h>
#include ".\atscparser.h"

extern void Log(const char *fmt, ...) ;
ATSCParser::ATSCParser(void)
{
	m_demuxer=NULL;
	masterGuideTableDecoded=false;
}

ATSCParser::~ATSCParser(void)
{
}
void ATSCParser::Reset()
{
	masterGuideTableDecoded=false;
	if (m_demuxer!=NULL) 
	{
		m_demuxer->UnMapSectionPIDs();
	}
}

void ATSCParser::SetDemuxer(SplitterSetup* demuxer)
{
	m_demuxer=demuxer;
}

void ATSCParser::ATSCDecodeMasterGuideTable(byte* buf, int len,int* channelsFound)
{	
	int table_id = buf[0];
	if (table_id!=0xc7) return;

	int section_syntax_indicator = (buf[1]>>7) & 1;
	int private_indicator = (buf[1]>>6) & 1;
	int section_length = ((buf[1]& 0xF)<<8) + buf[2];
	int transport_stream_id = (buf[3]<<8)+buf[4];
	int version_number = ((buf[5]>>1)&0x1F);
	int current_next_indicator = buf[5] & 1;
	int section_number = buf[6];
	int last_section_number = buf[7];
	int protocol_version = buf[8];
	int tables_defined = (buf[9]<<8) + buf[10];

	if (masterGuideTableDecoded) 
	{
		if (section_length==mgSectionLength &&
			section_number==mgSectionNumber &&
			last_section_number==mgLastSectionNumber)
		{
			return ;
		}
	}
			
	if (m_demuxer!=NULL) m_demuxer->UnMapSectionPIDs();
	mgLastSectionNumber=last_section_number;
	mgSectionNumber=section_number;
	mgSectionLength=section_length;
	masterGuideTableDecoded=true;
	*channelsFound=0;

	//decode tables...
	int start=11;
	// 16------ -------- 3--13--- -------- 3--5---- 32------ -------- -------- -------- 4---12-- --------
	// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
	//    0        1        2         3        4       5       6         7        8       9        10
	for (int i=0; i < tables_defined; ++i)
	{
		//table type		description
		//0x0000 			Terrestrial VCT with current_next_indicator=1
		//0x0001 			Terrestrial VCT with current_next_indicator=0
		//0x0002 			Cable VCT with current_next_indicator=1
		//0x0003 			Cable VCT with current_next_indicator=0
		//0x0004 			Channel ETT
		//0x0005 			DCCSCT
		//0x0006-0x00FF 	[Reserved for future ATSC use]
		//0x0100-0x017F 	EIT-0 to EIT-127
		//0x0180-0x01FF 	[Reserved for future ATSC use]
		//0x0200-0x027F 	Event ETT-0 to event ETT-127
		//0x0280-0x0300 	[Reserved for future ATSC use]
		//0x0301-0x03FF 	RRT with rating_region 1-255
		//0x0400-0x0FFF 	[User private]
		//0x1000-0x13FF 	[Reserved for future ATSC use]
		//0x1400-0x14FF 	DCCT with dcc_id 0x00 � 0xFF
		//0x1500-0xFFFF 	[Reserved for future ATSC use]
		int table_type				=  (buf[start]<<8) + (buf[start+1]);
		int table_type_PID			= ((buf[start+2]&0x1f)<<8) + (buf[start+3]);
		int table_type_version		=   buf[start+4] & 0x1f;
		int number_of_bytes			=  (buf[start+5]<<24) + (buf[start+6]<<16) + (buf[start+7]<<8)+ buf[start+8];
		int table_type_descriptors_len = ((buf[start+9]&0xf)<<8) + buf[start+10];
		int pos=0;
		int ofs=start+11;

		if (m_demuxer!=NULL) 
		{
			if (table_type >=0x100 && table_type  <= 0x17f) 
				m_demuxer->MapAdditionalPID(table_type_PID);
			else if (table_type >=0x200 && table_type  <= 0x27F) 
				m_demuxer->MapAdditionalPID(table_type_PID);
			else if (table_type >=0x0301 && table_type  <= 0x03FF) 
				m_demuxer->MapAdditionalPID(table_type_PID);
			else if (table_type == 0x0004) 
				m_demuxer->MapAdditionalPID(table_type_PID);
		}
		while (pos < table_type_descriptors_len)
		{
			int descriptor_tag = buf[ofs];
			int descriptor_len = buf[ofs+1];
			switch (descriptor_tag)
			{
				case 0x80: //stuffing
					break;
				case 0x81: //AC3 audio descriptor
					break;
				case 0x86: //caption service descriptor
					break;
				case 0x87: //content advisory descriptor
					break;
				case 0xa0: //extended channel name descriptor
					break;
				case 0xa1: //service location descriptor
					break;
				case 0xa2: //time-shifted service descriptor
					break;
				case 0xa3: //component name descriptor
					break;
				case 0xa8: //DCC departing request descriptor
					break;
				case 0xa9: //DCC arriving request descriptor
					break;
				case 0xaa: //redistribution control descriptor
					break;
			}
			pos += (2+descriptor_len);
			ofs += (2+descriptor_len);
		}
		start= start + 11 + table_type_descriptors_len;
	}
}

void ATSCParser::ATSCDecodeEIT(byte* buf, int len)
{
}
void ATSCParser::ATSCDecodeETT(byte* buf, int len)
{
}
void ATSCParser::ATSCDecodeRTT(byte* buf, int len)
{
}
void ATSCParser::ATSCDecodeChannelEIT(byte* buf, int len)
{
}
void ATSCParser::ATSCDecodeEPG(byte* buf, int len)
{
	int table_id = buf[0];
	if (table_id >=0x100 && table_id <=0x17f)
	{
		//Decode EIT-0 - EIT-127
		ATSCDecodeEIT(buf,len);
	}
	else if (table_id >=0x200 && table_id <=0x27f)
	{
		//Decode ETT-0 - ETT-127
		ATSCDecodeETT(buf,len);
	}
	else if (table_id >=0x300 && table_id <=0x3ff)
	{
		//Decode RTT with region 1-255
		ATSCDecodeRTT(buf,len);
	}
	else if (table_id==0x04)
	{
		//decode Channel ETT
		ATSCDecodeChannelEIT(buf,len);
	}
}
void ATSCParser::ATSCDecodeChannelTable(BYTE *buf,ChannelInfo *ch, int* channelsFound)
{
	int table_id = buf[0];
	if (table_id!=0xc8 && table_id != 0xc9) return;
	//dump table!
	*channelsFound=0;
	Log("ATSCDecodeChannelTable()");
	int section_syntax_indicator = (buf[1]>>7) & 1;
	int private_indicator = (buf[1]>>6) & 1;
	int section_length = ((buf[1]& 0xF)<<8) + buf[2];
	int transport_stream_id = (buf[3]<<8)+buf[4];
	int version_number = ((buf[5]>>1)&0x1F);
	int current_next_indicator = buf[5] & 1;
	int section_number = buf[6];
	int last_section_number = buf[7];
	int protocol_version = buf[8];
	int num_channels_in_section = buf[9];
	if (num_channels_in_section <= 0) return;
/*
	FILE* fp = fopen("table.dat","wb+");
	if (fp!=NULL)
	{
		fwrite(buf,1,section_length,fp);
		fclose(fp);
	}*/
	Log("  table id:0x%x section length:%d channels:%d (%d)", table_id,section_length,num_channels_in_section, (*channelsFound));
	int start=10;
	for (int i=0; i < num_channels_in_section;i++)
	{
		Log("  decode channel:%d", i);
		char shortName[127];
		strcpy(shortName,"unknown");
		try
		{
			//shortname 7*16 bits (14 bytes) in UTF-16
			for (int count=0; count < 7; count++)
			{
				shortName[count] = buf[1+start+count*2];
				shortName[count+1]=0; 
			}
		}
		catch(...)
		{
		}
		
		Log("  channel:%d shortname:%s", i,shortName);

		start+= 7*2;
		// 4---10-- ------10 -------- 8------- 32------ -------- -------- -------- 16------ -------- 16------ -------- 2-111113 --6----- 16------ -------- 6-----10 --------
		// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
		//    112      113      114       115      116    117      118       119     120     121       123      124      125      126      127      128      129      130
		//     0        1        2         3        4      5        6         7       8       9        10       11       12       13       14       15       16       17 
		//  ++++++++ ++++++++ --+-++-	
		// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
		int major_channel    		 =((buf[start  ]&0xf)<<8) + (buf[start+1]>>2);
		int minor_channel    		 =((buf[start+1]&0x3)<<8) + buf[start+2];
		int modulation_mode  		 = buf[start+3];
		int carrier_frequency		 = (buf[start+4]<<24) + (buf[start+5]<<16) + (buf[start+6]<<8) + (buf[start+7]);
		int channel_TSID			 = ((buf[start+8])<<8) + buf[start+9];
		int program_number			 = ((buf[start+10])<<8) + buf[start+11];
		int ETM_location			 = ((buf[start+12]>>6)&0x3);
		int access_controlled		 = ((buf[start+12]>>4)&0x1);
		int hidden          		 = ((buf[start+12]>>3)&0x1);
		int path_select     		 = ((buf[start+12]>>2)&0x1);
		int out_of_band     		 = ((buf[start+12]>>1)&0x1);
		int hide_guide     		     = ((buf[start+12]   )&0x1);
		int service_type             = ((buf[start+13]   )&0x3f);
		int source_id				 = ((buf[start+14])<<8) + buf[start+15];
		int descriptors_length		 = ((buf[start+16]&0x3)<<8) + buf[start+17];

		if (major_channel==0 && minor_channel==0 && channel_TSID==0 && service_type==0 )
		{
			*channelsFound=0;
			return;
		}
		if (modulation_mode < 0 || modulation_mode > 5)
		{
			*channelsFound=0;
			return;
		}
		Log("  channel:%d major:%d minor:%d modulation:%d frequency:%d tsid:%d program:%d servicetype:%d descriptor len:%d", 
						i,major_channel,minor_channel,modulation_mode,carrier_frequency, channel_TSID, program_number,service_type, descriptors_length);
		ChannelInfo* channelInfo = &ch[*channelsFound];
		memset(channelInfo->ProviderName,0,255);
		memset(channelInfo->ServiceName,0,255);
		strcpy((char*)channelInfo->ProviderName,"unknown");
		strcpy((char*)channelInfo->ServiceName,shortName);
		channelInfo->MinorChannel = minor_channel;
		channelInfo->MajorChannel = major_channel;
		channelInfo->Frequency    = carrier_frequency;
		channelInfo->ProgrammNumber= program_number;
		channelInfo->TransportStreamID = channel_TSID;		
		channelInfo->Pids.Teletext=-1;
		channelInfo->Pids.AudioPid1=-1;
		channelInfo->Pids.AudioPid2=-1;
		channelInfo->Pids.AudioPid3=-1;
		channelInfo->Pids.AC3=-1;
		channelInfo->Pids.Subtitles=-1;
		channelInfo->Pids.VideoPid=-1;
		channelInfo->Pids.Lang1_1=0;
		channelInfo->Pids.Lang1_2=0;
		channelInfo->Pids.Lang1_3=0;
		channelInfo->Pids.Lang2_1=0;
		channelInfo->Pids.Lang2_2=0;
		channelInfo->Pids.Lang2_3=0;
		channelInfo->Pids.Lang3_1=0;
		channelInfo->Pids.Lang3_2=0;
		channelInfo->Pids.Lang3_3=0;
		channelInfo->EITPreFollow=0;
		channelInfo->EITSchedule=0;
		channelInfo->ProgrammPMTPID=-1;
		channelInfo->NetworkID    =-1;
		channelInfo->PMTReady	  = 1;
		channelInfo->SDTReady	  = 1;
		if (service_type==1||service_type==2) channelInfo->ServiceType=1;//ATSC video
		if (service_type==3) channelInfo->ServiceType=2;//ATSC audio
		switch (modulation_mode)
		{
			case 0: //reserved
				channelInfo->Modulation   = BDA_MOD_NOT_SET;
			break;
			case 1: //analog
				channelInfo->Modulation   = BDA_MOD_ANALOG_FREQUENCY;
			break;
			case 2: //QAM64
				channelInfo->Modulation   = BDA_MOD_64QAM;
			break;
			case 3: //QAM256
				channelInfo->Modulation   = BDA_MOD_256QAM;
			break;
			case 4: //8 VSB
				channelInfo->Modulation   = BDA_MOD_8VSB;
			break;
			case 5: //16 VSB
				channelInfo->Modulation   = BDA_MOD_16VSB;
			break;
			default: //
				channelInfo->Modulation   = BDA_MOD_NOT_SET;
			break;

		}

		start += 18;
		int len=0;
		if (descriptors_length<=0)
		{
			*channelsFound=0;
			return;
		}
		while (len < descriptors_length)
		{
			int descriptor_tag = buf[start+len];
			int descriptor_len = buf[start+len+1];
			if (descriptor_len==0 || descriptor_len+start > section_length)
			{
				*channelsFound=0;
				return;
			}			
			Log("    decode descriptor start:%d len:%d tag:%x", start, descriptor_len, descriptor_tag);
			switch (descriptor_tag)
			{
				case 0xa1:
					DecodeServiceLocationDescriptor( buf,start+len, channelInfo);
				break;
				case 0xa0:
					DecodeExtendedChannelNameDescriptor( buf,start+len,channelInfo);
				break;
			}
			len += (descriptor_len+2);
		}
		start += descriptors_length;
		*channelsFound=*channelsFound+1;
	}
	Log("ATSCDecodeChannelTable() done, found %d channels", (*channelsFound));
}

void ATSCParser::DecodeServiceLocationDescriptor( byte* buf,int start,ChannelInfo* channelInfo)
{

	Log("DecodeServiceLocationDescriptor()");
	//  8------ 8------- 3--13--- -------- 8-------       
	// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
	//    0        1        2         3        4       5       6         7        8       9     
	int pcr_pid = ((buf[start+2]&0x1f)<<8) + buf[start+3];
	int number_of_elements = buf[start+4];
	int off=start+5;
	channelInfo->PCRPid=pcr_pid;

	if (number_of_elements==0) return;
	Log(" pcr pid:%x elements:%d", pcr_pid, number_of_elements);
	for (int i=0; i < number_of_elements;++i)
	{

		//  8------ 3--13--- -------- 24------ -------- --------
		// 76543210|76543210|76543210|76543210|76543210|76543210|
		//    0        1        2         3        4       5     
		int streamtype			  = buf[off];
		int elementary_pid		  = ((buf[off+1]&0x1f)<<8) + buf[off+2];
		int ISO_639_language_code =	(buf[off+3]<<16) +(buf[off+4]<<8) + (buf[off+5]);

		Log(" element:%d type:%d pid:%x", i,streamtype, elementary_pid);
		off+=6;
		//pmtData.data=ISO_639_language_code;
		switch (streamtype)
		{
			case 0x2: // video
				channelInfo->Pids.VideoPid=elementary_pid;
				break;
			case 0x81: // audio
				channelInfo->Pids.AudioPid1=elementary_pid;
				break;
			default:
				break;
		}
	}
	Log("DecodeServiceLocationDescriptor() done");
}
void ATSCParser::DecodeExtendedChannelNameDescriptor( byte* buf,int start,ChannelInfo* channelInfo)
{
	Log("DecodeExtendedChannelNameDescriptor() ");
	// tid   
	//  8       8------- 8-------
	// 76543210|76543210|76543210
	//    0        1        2    
	int descriptor_tag = buf[start+0];
	int descriptor_len = buf[start+1];
	Log(" tag:%x len:%d", descriptor_tag, descriptor_len);
	char* label = DecodeMultipleStrings(buf,start+2);
	if (label==NULL) return ;
	strcpy((char*)channelInfo->ServiceName,label);

	Log(" label:%s", label);
	delete [] label;
	Log("DecodeExtendedChannelNameDescriptor() done");
}
char* ATSCParser::DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes)
{
	Log("DecodeString() compression type:%d numberofbytes:%d",compression_type, mode);
	if (compression_type==0 && mode==0)
	{
		char* label = new char[number_of_bytes+1];
		memcpy(label,&buf[offset],number_of_bytes);
		label[number_of_bytes]=0;
		return (char*)label;
	}
	//string data="";
	//for (int i=0; i < number_of_bytes;++i)
	//	data += String.Format(" {0:X}", buf[offset+i]);
	Log("DecodeString() unknown type or mode");
	return NULL;
}

char* ATSCParser::DecodeMultipleStrings(byte* buf, int offset)
{
	int number_of_strings = buf[offset];
	Log("DecodeMultipleStrings() number_of_strings:%d",number_of_strings);
	

	for (int i=0; i < number_of_strings;++i)
	{
		Log("  string:%d", i);
		int ISO_639_language_code = (buf[offset+1]<<16)+(buf[offset+2]<<8)+(buf[offset+3]);
		int number_of_segments=buf[offset+4];
		int start=offset+5;
		Log("  segments:%d", number_of_segments);
		for (int k=0; k < number_of_segments;++k)
		{
			Log("  decode segment:%d", k);
			int compression_type = buf[start];
			int mode             = buf[start+1];
			int number_bytes     = buf[start+2];
			//decode text....
			char *label=DecodeString(buf, start+3, compression_type,mode,number_bytes);
			start += (number_bytes+3);
			if (label!=NULL) return label;
		}
	}
	return NULL;
}
