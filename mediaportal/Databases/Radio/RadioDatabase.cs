using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using SQLite.NET;
using MediaPortal.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabase
  {
    static SQLiteClient m_db=null;

    // singleton. Dont allow any instance of this class
    private RadioDatabase()
    {
    }

		static RadioDatabase()
		{
			Open();
		}
		static void Open()
		{
      try 
      {
        // Open database
        Log.Write("open radiodatabase");
				try
				{
        System.IO.Directory.CreateDirectory("database");
				}
				catch(Exception){}
				m_db = new SQLiteClient(@"database\RadioDatabase2.db");
        CreateTables();

        if (m_db!=null)
        {
          m_db.Execute("PRAGMA cache_size=8192\n");
          m_db.Execute("PRAGMA synchronous='OFF'\n");
          m_db.Execute("PRAGMA count_changes='OFF'\n");
        }

      } 
      catch (Exception ex) 
      {
        Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
      Log.Write("Radio database opened");
    }
  
    static bool CreateTables()
    {
      if (m_db==null) return false;
      if ( DatabaseUtility.AddTable(m_db,"station","CREATE TABLE station ( idChannel integer primary key, strName text, iChannelNr integer, frequency text, URL text, genre text, bitrate int)\n"))
      {
        m_db.Execute("CREATE INDEX idxStation ON station(idChannel)");
      }
			DatabaseUtility.AddTable(m_db,"tblDVBSMapping" ,"CREATE TABLE tblDVBSMapping ( idChannel integer,sPCRPid integer,sTSID integer,sFreq integer,sSymbrate integer,sFEC integer,sLNBKhz integer,sDiseqc integer,sProgramNumber integer,sServiceType integer,sProviderName text,sChannelName text,sEitSched integer,sEitPreFol integer,sAudioPid integer,sVideoPid integer,sAC3Pid integer,sAudio1Pid integer,sAudio2Pid integer,sAudio3Pid integer,sTeletextPid integer,sScrambled integer,sPol integer,sLNBFreq integer,sNetworkID integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text,sECMPid integer,sPMTPid integer)\n");
			DatabaseUtility.AddTable(m_db,"tblDVBCMapping" ,"CREATE TABLE tblDVBCMapping ( idChannel integer, strChannel text, strProvider text, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer)\n");
			DatabaseUtility.AddTable(m_db,"tblDVBTMapping" ,"CREATE TABLE tblDVBTMapping ( idChannel integer, strChannel text, strProvider text, frequency text, bandwidth integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer)\n");
      

      return true;
    }
    static public void GetStations(ref ArrayList stations)
    {
      stations.Clear();
      if (m_db==null) return ;
      lock (m_db)
      {
        try
        {
          if (null==m_db) return ;
          string strSQL;
          strSQL=String.Format("select * from station order by iChannelNr");
          SQLiteResultSet results;
          results=m_db.Execute(strSQL);
          if (results.Rows.Count== 0) return ;
          for (int i=0; i < results.Rows.Count;++i)
          {
            RadioStation chan=new RadioStation();
            try
            {
              chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
            }catch(Exception){}
            try
            {
              chan.Channel = Int32.Parse(DatabaseUtility.Get(results,i,"iChannelNr"));
            }catch(Exception){}
            try
            {
              chan.Frequency = Int64.Parse(DatabaseUtility.Get(results,i,"frequency"));
            }catch(Exception)
            {}
            chan.Name = DatabaseUtility.Get(results,i,"strName");
            chan.URL = DatabaseUtility.Get(results,i,"URL");
            if (chan.URL.Equals("unknown")) chan.URL ="";
            try
            {
            chan.BitRate=Int32.Parse( DatabaseUtility.Get(results,i,"bitrate") );
            }
            catch(Exception){}

            chan.Genre=DatabaseUtility.Get(results,i,"genre") ;
            stations.Add(chan);
          }

          return ;
        }
        catch(Exception ex)
        {
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }
        return ;
      }
    }

    
    static public int AddStation(ref RadioStation channel)
    {
        lock (m_db)
        {
          string strSQL;
          try
          {
            string strChannel=channel.Name;
            string strURL    =channel.URL;
            string strGenre    =channel.Genre;
            DatabaseUtility.RemoveInvalidChars(ref strChannel);
            DatabaseUtility.RemoveInvalidChars(ref strURL);
            DatabaseUtility.RemoveInvalidChars(ref strGenre);

            if (null==m_db) return -1;
            SQLiteResultSet results;
            strSQL=String.Format( "select * from station where strName like '{0}'", strChannel);
            results=m_db.Execute(strSQL);
            if (results.Rows.Count==0) 
            {
              // doesnt exists, add it
              strSQL=String.Format("insert into station (idChannel, strName,iChannelNr ,frequency,URL,bitrate,genre) values ( NULL, '{0}', {1}, {2}, '{3}',{4},'{5}' )", 
                                    strChannel,channel.Channel,channel.Frequency.ToString(),strURL, channel.BitRate,strGenre);
              m_db.Execute(strSQL);
              int iNewID=m_db.LastInsertID();
              channel.ID=iNewID;
              return iNewID;
            }
            else
            {
              int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
              channel.ID=iNewID;
              return iNewID;
            }
          } 
          catch (Exception ex) 
          {
						Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
						Open();
          }

          return -1;
        }
    }

    static public int GetStationId(string strStation)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          if (null==m_db) return -1;
          SQLiteResultSet results;
          strSQL=String.Format( "select * from station where strName like '{0}'", strStation);
          results=m_db.Execute(strSQL);
          if (results.Rows.Count==0) return -1;
          int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
          return iNewID;
        } 
        catch (Exception ex) 
        {
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }

        return -1;
      }
    }


    static public void RemoveStation(string strStationName)
    {
      lock (m_db)
      {
        if (null==m_db) return ;

        int iChannelId=GetStationId(strStationName);
        if (iChannelId<0) return ;
        
        try
        {
          if (null==m_db) return ;
          string strSQL=String.Format("delete from station where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBSMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
        }
        catch(Exception ex)
        {
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }
      }
    }   
    
    static public void RemoveAllStations()
    {
      lock (m_db)
      {
        if (null==m_db) return ;
        
        try
        {
          if (null==m_db) return ;
          string strSQL=String.Format("delete from station ");
          m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBSMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping");
					m_db.Execute(strSQL);

        }
        catch(Exception ex)
        {
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }
      }
		}  
    
		static public void RemoveLocalRadioStations()
		{
			lock (m_db)
			{
				if (null==m_db) return ;
        
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from station where frequency>0");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBSMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping");
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public int MapDVBSChannel(int idChannel,int freq,int symrate,int fec,int lnbkhz,int diseqc,
			int prognum,int servicetype,string provider,string channel,int eitsched,
			int eitprefol,int audpid,int vidpid,int ac3pid,int apid1,int apid2,int apid3,
			int teltxtpid,int scrambled,int pol,int lnbfreq,int networkid,int tsid,int pcrpid,string aLangCode,string aLangCode1,string aLangCode2,string aLangCode3,int ecmPid,int pmtPid)
		{
			lock (typeof(RadioDatabase))
			{
				string strSQL;
				try
				{
					DatabaseUtility.RemoveInvalidChars(ref provider);
					DatabaseUtility.RemoveInvalidChars(ref channel);

					string strChannel=channel;
					SQLiteResultSet results=null;

					strSQL=String.Format( "select * from tblDVBSMapping ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", idChannel,servicetype);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{

						strSQL=String.Format("insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})", 
							idChannel,freq,symrate, fec,lnbkhz,diseqc,
							prognum,servicetype,provider,channel, eitsched,
							eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
							teltxtpid,scrambled, pol,lnbfreq,networkid,tsid,pcrpid,aLangCode,aLangCode1,aLangCode2,aLangCode3,ecmPid,pmtPid);
					  
						m_db.Execute(strSQL);
						return 0;
					}
					else
					{
						return -1;
					}
				} 
				catch (Exception ex) 
				{
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		static public int MapDVBTChannel(string channelName, string providerName,int idChannel, int frequency, int ONID, int TSID, int SID, int audioPid)
		{
			lock (typeof(RadioDatabase))
			{
				if (null==m_db) return -1;
				string strSQL;
				try
				{
					string strChannel=channelName;
					string strProvider=providerName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strProvider);

					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblDVBTMapping where idChannel like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBTMapping (idChannel, strChannel ,strProvider,frequency , bandwidth , ONID , TSID , SID , audioPid,Visible) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},1)",
							idChannel,strChannel,strProvider,frequency,0,ONID,TSID,SID,audioPid);
						//Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}',strProvider='{5}',audioPid={6} where idChannel like '{7}'", 
							frequency,ONID,TSID,SID,strChannel, strProvider,audioPid, idChannel);
						//	Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		static public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate,int innerFec, int modulation,int ONID, int TSID, int SID, int audioPid)
		{
			lock (typeof(RadioDatabase))
			{
				if (null==m_db) return -1;
				string strSQL;
				try
				{
					string strChannel=channelName;
					string strProvider=providerName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strProvider);

					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblDVBCMapping where idChannel like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,Visible) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},1)"
							,idChannel,strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid);
						//Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9} where idChannel like '{10}'", 
							frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,strChannel, strProvider,audioPid,idChannel);
						//Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		
		static public void GetDVBTTuneRequest(int idChannel, out string strProvider,out int frequency, out int ONID, out int TSID, out int SID, out int audioPid) 
		{
			audioPid=-1;
			strProvider="";
			frequency=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.Write("GetTuneRequest for idChannel:{0}", idChannel);
			lock (typeof(RadioDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBTMapping where idChannel={0}",idChannel);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=Int32.Parse(DatabaseUtility.Get(results,0,"frequency"));
					ONID=Int32.Parse(DatabaseUtility.Get(results,0,"ONID"));
					TSID=Int32.Parse(DatabaseUtility.Get(results,0,"TSID"));
					SID=Int32.Parse(DatabaseUtility.Get(results,0,"SID"));
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=Int32.Parse(DatabaseUtility.Get(results,0,"audioPid"));
					return ;
				}
				catch(Exception ex)
				{
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public void GetDVBCTuneRequest(int idChannel, out string strProvider,out int frequency,out int symbolrate,out int innerFec,out int modulation, out int ONID, out int TSID, out int SID, out int audioPid) 
		{
			audioPid=0;
			strProvider="";
			frequency=-1;
			symbolrate=-1;
			innerFec=-1;
			modulation=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.Write("GetTuneRequest for idChannel:{0}", idChannel);
			lock (typeof(RadioDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBCMapping where idChannel={0}",idChannel);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=Int32.Parse(DatabaseUtility.Get(results,0,"frequency"));
					symbolrate=Int32.Parse(DatabaseUtility.Get(results,0,"symbolrate"));
					innerFec=Int32.Parse(DatabaseUtility.Get(results,0,"innerFec"));
					modulation=Int32.Parse(DatabaseUtility.Get(results,0,"modulation"));
					ONID=Int32.Parse(DatabaseUtility.Get(results,0,"ONID"));
					TSID=Int32.Parse(DatabaseUtility.Get(results,0,"TSID"));
					SID=Int32.Parse(DatabaseUtility.Get(results,0,"SID"));
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=Int32.Parse(DatabaseUtility.Get(results,0,"audioPid"));
					return ;
				}
				catch(Exception ex)
				{
					Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public bool GetDVBSTuneRequest(int idChannel,int serviceType,ref DVBChannel retChannel)
		{
		  
			int freq=0;int symrate=0;int fec=0;int lnbkhz=0;int diseqc=0;
			int prognum=0;int servicetype=0;string provider="";string channel="";int eitsched=0;
			int eitprefol=0;int audpid=0;int vidpid=0;int ac3pid=0;int apid1=0;int apid2=0;int apid3=0;
			int teltxtpid=0;int scrambled=0;int pol=0;int lnbfreq=0;int networkid=0;int tsid=0;int pcrpid=0;
			string audioLang;string audioLang1;string audioLang2;string audioLang3;int ecm;int pmt;
	  
	  
			if (m_db==null) return false;
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from tblDVBSMapping where idChannel={0} and sServiceType={1}",idChannel,serviceType);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count<1) return false;
					else
					{
						//chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
						//chan.Number = Int32.Parse(DatabaseUtility.Get(results,i,"iChannelNr"));
						// sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName
						//sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,
						//sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID
						int i=0;
						freq=Int32.Parse(DatabaseUtility.Get(results,i,"sFreq"));
						symrate=Int32.Parse(DatabaseUtility.Get(results,i,"sSymbrate"));
						fec=Int32.Parse(DatabaseUtility.Get(results,i,"sFEC"));
						lnbkhz=Int32.Parse(DatabaseUtility.Get(results,i,"sLNBKhz"));
						diseqc=Int32.Parse(DatabaseUtility.Get(results,i,"sDiseqc"));
						prognum=Int32.Parse(DatabaseUtility.Get(results,i,"sProgramNumber"));
						servicetype=Int32.Parse(DatabaseUtility.Get(results,i,"sServiceType"));
						provider=DatabaseUtility.Get(results,i,"sProviderName");
						channel=DatabaseUtility.Get(results,i,"sChannelName");
						eitsched=Int32.Parse(DatabaseUtility.Get(results,i,"sEitSched"));
						eitprefol= Int32.Parse(DatabaseUtility.Get(results,i,"sEitPreFol"));
						audpid=Int32.Parse(DatabaseUtility.Get(results,i,"sAudioPid"));
						vidpid=Int32.Parse(DatabaseUtility.Get(results,i,"sVideoPid"));
						ac3pid=Int32.Parse(DatabaseUtility.Get(results,i,"sAC3Pid"));
						apid1= Int32.Parse(DatabaseUtility.Get(results,i,"sAudio1Pid"));
						apid2= Int32.Parse(DatabaseUtility.Get(results,i,"sAudio2Pid"));
						apid3=Int32.Parse(DatabaseUtility.Get(results,i,"sAudio3Pid"));
						teltxtpid=Int32.Parse(DatabaseUtility.Get(results,i,"sTeletextPid"));
						scrambled= Int32.Parse(DatabaseUtility.Get(results,i,"sScrambled"));
						pol=Int32.Parse(DatabaseUtility.Get(results,i,"sPol"));
						lnbfreq=Int32.Parse(DatabaseUtility.Get(results,i,"sLNBFreq"));
						networkid=Int32.Parse(DatabaseUtility.Get(results,i,"sNetworkID"));
						tsid=Int32.Parse(DatabaseUtility.Get(results,i,"sTSID"));
						pcrpid=Int32.Parse(DatabaseUtility.Get(results,i,"sPCRPid"));
						// sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid
						audioLang=DatabaseUtility.Get(results,i,"sAudioLang");
						audioLang1=DatabaseUtility.Get(results,i,"sAudioLang1");
						audioLang2=DatabaseUtility.Get(results,i,"sAudioLang2");
						audioLang3=DatabaseUtility.Get(results,i,"sAudioLang3");
						ecm=Int32.Parse(DatabaseUtility.Get(results,i,"sECMPid"));
						pmt=Int32.Parse(DatabaseUtility.Get(results,i,"sPMTPid"));
						retChannel=new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
							prognum, servicetype,provider, channel, eitsched,
							eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
							teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid,audioLang,audioLang1,audioLang2,audioLang3,ecm,pmt);

					}

					return true;
				}
				catch(Exception ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}

  }
}
