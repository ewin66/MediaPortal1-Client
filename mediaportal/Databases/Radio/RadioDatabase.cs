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
using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using SQLite.NET;
using MediaPortal.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabase : IRadioDatabase
  {
    static IRadioDatabase _database = new RadioDatabaseSqlLite();

    public void ClearAll()
    {
      _database.ClearAll();
    }
    public void GetStations(ref ArrayList stations)
    {
      _database.GetStations(ref  stations);
    }
    public bool GetStation(string radioName, out RadioStation station)
    {
      return _database.GetStation(radioName, out  station);
    }
    public void UpdateStation(RadioStation channel)
    {
      _database.UpdateStation(channel);
    }
    public int AddStation(ref RadioStation channel)
    {
      return _database.AddStation(ref  channel);
    }
    public int GetStationId(string strStation)
    {
      return _database.GetStationId(strStation);
    }
    public void RemoveStation(string strStationName)
    {
      _database.RemoveStation(strStationName);
    }
    public void RemoveAllStations()
    {
      _database.RemoveAllStations();
    }
    public void RemoveLocalRadioStations()
    {
      _database.RemoveLocalRadioStations();
    }
    public int MapDVBSChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
      int prognum, int servicetype, string provider, string channel, int eitsched,
      int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
      int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid, int pcrpid, string aLangCode, string aLangCode1, string aLangCode2, string aLangCode3, int ecmPid, int pmtPid)
    {
      return _database.MapDVBSChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc, prognum, servicetype, provider, channel, eitsched, eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3, teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, aLangCode, aLangCode1, aLangCode2, aLangCode3, ecmPid, pmtPid);

    }
    public int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID, int TSID, int SID, int audioPid, int pmtPid, int bandWidth, int pcrPid)
    {
      return _database.MapDVBTChannel(channelName, providerName, idChannel, frequency, ONID, TSID, SID, audioPid, pmtPid, bandWidth, pcrPid);
    }
    public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      return _database.MapDVBCChannel(channelName, providerName, idChannel, frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, pmtPid, pcrPid);
    }
    public int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel, string providerName, int idChannel, int frequency, int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      return _database.MapATSCChannel(channelName, physicalChannel, minorChannel, majorChannel, providerName, idChannel, frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, pmtPid, pcrPid);
    }
    public void GetDVBTTuneRequest(int idChannel, out string strProvider, out int frequency, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int bandWidth, out int pcrPid)
    {
      _database.GetDVBTTuneRequest(idChannel, out strProvider, out frequency, out ONID, out TSID, out SID, out audioPid, out pmtPid, out bandWidth, out pcrPid);
    }
    public void GetDVBCTuneRequest(int idChannel, out string strProvider, out int frequency, out int symbolrate, out int innerFec, out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid)
    {
      _database.GetDVBCTuneRequest(idChannel, out strProvider, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
    }
    public void GetATSCTuneRequest(int idChannel, out int physicalChannel, out int minorChannel, out int majorChannel, out string strProvider, out int frequency, out int symbolrate, out int innerFec, out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid)
    {
      _database.GetATSCTuneRequest(idChannel, out physicalChannel, out minorChannel, out majorChannel, out strProvider, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
    }
    public bool GetDVBSTuneRequest(int idChannel, int serviceType, ref DVBChannel retChannel)
    {
      return _database.GetDVBSTuneRequest(idChannel, serviceType, ref  retChannel);
    }
    public void MapChannelToCard(int channelId, int card)
    {
      _database.MapChannelToCard(channelId, card);
    }
    public void DeleteCard(int card)
    {
      _database.DeleteCard(card);
    }
    public void UnmapChannelFromCard(RadioStation channel, int card)
    {
      _database.UnmapChannelFromCard(channel, card);
    }
    public bool CanCardTuneToStation(string channelName, int card)
    {
      return _database.CanCardTuneToStation(channelName, card);
    }
    public void GetStationsForCard(ref ArrayList stations, int card)
    {
      _database.GetStationsForCard(ref  stations, card);
    }
    public RadioStation GetStationByStream(bool atsc, bool dvbt, bool dvbc, bool dvbs, int networkid, int transportid, int serviceid, out string provider)
    {
      return _database.GetStationByStream(atsc, dvbt, dvbc, dvbs, networkid, transportid, serviceid, out provider);
    }
    public void UpdatePids(bool isATSC, bool isDVBC, bool isDVBS, bool isDVBT, DVBChannel channel)
    {
      _database.UpdatePids(isATSC, isDVBC, isDVBS, isDVBT, channel);
    }
    public int AddProgram(TVProgram prog)
    {
      return _database.AddProgram(prog);
    }
    public int AddGenre(string strGenre1)
    {
      return _database.AddGenre(strGenre1);
    }
    public int UpdateProgram(TVProgram prog)
    {
      return _database.UpdateProgram(prog);
    }
    public void RemoveOldPrograms()
    {
      _database.RemoveOldPrograms();
    }
    public bool GetGenres(ref List<string> genres)
    {
      return _database.GetGenres(ref  genres);
    }
    public bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime, ref List<TVProgram> progs)
    {
      return _database.GetProgramsPerChannel(strChannel1, iStartTime, iEndTime, ref  progs);
    }
  }
}
