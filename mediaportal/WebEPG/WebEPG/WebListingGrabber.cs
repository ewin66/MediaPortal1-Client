#region Copyright (C) 2006 Team MediaPortal
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
#endregion

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using MediaPortal.Services;
using MediaPortal.Webepg.Profile;
using MediaPortal.TV.Database;
using MediaPortal.WebEPG;
using MediaPortal.Utils.Web;
using MediaPortal.Utils.Time;
using MediaPortal.Utils.Services;
using MediaPortal.EPG.config;
using MediaPortal.WebEPG.Parser;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG
{
  /// <summary>
  /// Summary description for Class1
  /// </summary>
  public class WebListingGrabber
  {
    #region Variables
    WorldTimeZone _siteTimeZone = null;
    ListingTimeControl _timeControl;
    RequestData _reqData;
    RequestBuilder _reqBuilder;
    GrabberConfigFile _grabber;
    DateTime _grabStart;
    string _strID = string.Empty;
    string _strBaseDir = string.Empty;
    bool _grabLinked;
    bool _dblookup = true;
    int _linkStart;
    int _linkEnd;

    IParser _parser;
    ArrayList _programs;
    ArrayList _dbPrograms;

    int _dbLastProg;
    int _maxGrabDays;
    ILog _log;
    #endregion

    #region Constructors/Destructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maxGrabDays">The number of days to grab</param>
    /// <param name="baseDir">The baseDir for grabber files</param>
    public WebListingGrabber(int maxGrabDays, string baseDir)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _maxGrabDays = maxGrabDays;
      _strBaseDir = baseDir;
    }
    #endregion

    #region Public Methods
    public bool Initalise(string File)
    {
      _log.Info(LogType.WebEPG, "WebEPG: Opening {0}", File);

      try
      {
        //_grabber = new GrabberConfig(_strBaseDir + File);

        XmlSerializer s = new XmlSerializer(typeof(GrabberConfigFile));

        TextReader r = new StreamReader(_strBaseDir + File);
        _grabber = (GrabberConfigFile)s.Deserialize(r);
      }
      catch (ArgumentException ex)
      {
        _log.Error(LogType.WebEPG, "WebEPG: Config Error {0}: {1}", File, ex.Message);
        return false;
      }

      if (_grabber.Listing.SearchParameters == null)
        _grabber.Listing.SearchParameters = new RequestData();

      _reqData = _grabber.Listing.SearchParameters;


      if (_grabber.Info.TimeZone != string.Empty)
      {
        //_timeAdjustOnly = _xmlreader.GetValueAsBool("Info", "TimeAdjustOnly", false);
        _log.Info(LogType.WebEPG, "WebEPG: TimeZone, Local: {0}", TimeZone.CurrentTimeZone.StandardName);
        try
        {
          _log.Info(LogType.WebEPG, "WebEPG: TimeZone, Site : {0}", _grabber.Info.TimeZone);
          //_log.Info(false, "[Debug] WebEPG: TimeZone, debug: {0}", _timeAdjustOnly);
          _siteTimeZone = new WorldTimeZone(_grabber.Info.TimeZone);
        }
        catch (ArgumentException)
        {
          _log.Error(LogType.WebEPG, "WebEPG: TimeZone Not valid");
          _siteTimeZone = null;
        }
      }
      else
      {
        _siteTimeZone = new WorldTimeZone(TimeZone.CurrentTimeZone.StandardName);
      }

      switch (_grabber.Listing.listingType)
      {
        case ListingInfo.Type.Xml:
          _parser = new XmlParser(_grabber.Listing.XmlTemplate);
          break;

        case ListingInfo.Type.Data:

          if (_grabber.Listing.DataTemplate.Template == null)
          {
            _log.Error("WebEPG: {0}: No Template", File);
            return false;
          }
          _parser = new DataParser(_grabber.Listing.DataTemplate);
          break;

        case ListingInfo.Type.Html:
          HtmlParserTemplate defaultTemplate = _grabber.Listing.HtmlTemplate.GetTemplate("default");
          if (defaultTemplate == null ||
            defaultTemplate.SectionTemplate == null ||
            defaultTemplate.SectionTemplate.Template == null)
          {
            _log.Error(LogType.WebEPG, "WebEPG: {0}: No Template", File);
            return false;
          }
          _parser = new WebParser(_grabber.Listing.HtmlTemplate);
          if (_grabber.Info.GrabDays < _maxGrabDays)
          {
            _log.Warn(LogType.WebEPG, "WebEPG: GrabDays {0} more than GuideDays {0}, limiting grab days", _grabber.Info.GrabDays, _maxGrabDays);
            _maxGrabDays = _grabber.Info.GrabDays;
          }

          break;
      }

      return true;
    }

    public ArrayList GetGuide(string strChannelID, bool Linked, int linkStart, int linkEnd)
    {
      return GetGuide(strChannelID, Linked, linkStart, linkEnd, DateTime.Now);
    }

    public ArrayList GetGuide(string strChannelID, bool Linked, int linkStart, int linkEnd, DateTime startDateTime)
    {
      _strID = strChannelID;
      _grabLinked = Linked;
      _linkStart = linkStart;
      _linkEnd = linkEnd;
      //int offset = 0;

      _reqData.ChannelId = _grabber.GetChannel(strChannelID);
      if (_reqData.ChannelId == string.Empty)
      {
        _log.Info(LogType.WebEPG, "WebEPG: ChannelId: {0} not found!", strChannelID);
        return null;
      }

      //_removeProgramsList = _grabber.GetRemoveProgramList(strChannelID); // <--- !!!

      _programs = new ArrayList();

      _log.Info(LogType.WebEPG, "WebEPG: ChannelId: {0}", strChannelID);

      //_GrabDay = 0;
      if (_grabber.Listing.Request.Delay < 500)
        _grabber.Listing.Request.Delay = 500;
      _reqBuilder = new RequestBuilder(_grabber.Listing.Request, startDateTime, _reqData);
      _grabStart = startDateTime;

      _log.Debug(LogType.WebEPG, "WebEPG: Grab Start {0} {1}", startDateTime.ToShortTimeString(), startDateTime.ToShortDateString());
      int requestedStartDay = startDateTime.Subtract(DateTime.Now).Days;
      if (requestedStartDay > 0)
      {
        if (requestedStartDay > _grabber.Info.GrabDays)
        {
          _log.Error(LogType.WebEPG, "WebEPG: Trying to grab past guide days");
          return null;
        }

        if (requestedStartDay + _maxGrabDays > _grabber.Info.GrabDays)
        {
          _maxGrabDays = _grabber.Info.GrabDays - requestedStartDay;
          _log.Warn(LogType.WebEPG, "WebEPG: Grab days more than Guide days, limiting to {0}", _maxGrabDays);
        }

        //_GrabDay = requestedStartDay;
        _reqBuilder.DayOffset = requestedStartDay;
        if (_reqBuilder.DayOffset > _maxGrabDays) //_GrabDay > _maxGrabDays)
          _maxGrabDays = _reqBuilder.DayOffset + _maxGrabDays; // _GrabDay + _maxGrabDays;
      }

      //TVDatabase.BeginTransaction();
      //TVDatabase.ClearCache();
      //TVDatabase.RemoveOldPrograms();

      int dbChannelId;
      string dbChannelName;
      _dbPrograms = new ArrayList();
      _dbLastProg = 0;

      try
      {
        if (TVDatabase.GetEPGMapping(strChannelID, out dbChannelId, out dbChannelName)) // (nodeId.InnerText, out idTvChannel, out strTvChannel);
        {
          TVDatabase.GetProgramsPerChannel(dbChannelName, ref _dbPrograms);
        }
      }
      catch (Exception)
      {
        _log.Error(LogType.WebEPG, "WebEPG: Database failed, disabling db lookup");
        _dblookup = false;
      }

      _timeControl = new ListingTimeControl(_siteTimeZone.FromLocalTime(startDateTime));
      while (_reqBuilder.DayOffset < _maxGrabDays)
      {
        _reqBuilder.Offset = 0;

        bool error;
        while (GetListing(out error))
        {
          if (_grabber.Listing.SearchParameters.MaxListingCount == 0)
            break;
          _reqBuilder.Offset++;
        }

        if (error)
        {
          _log.Error(LogType.WebEPG, "WebEPG: ChannelId: {0} grabber error", strChannelID);
          break;
        }

        //_GrabDay++;
        if (_reqBuilder.HasDate()) // < here
        {
          _reqBuilder.AddDays(1);
        }
        else
        {
          if (!_reqBuilder.HasList()) // < here
            break;
          _reqBuilder.AddDays(_timeControl.GrabDay);
        }
      }

      return _programs;
    }
    #endregion

    #region Private Methods
    private TVProgram dbProgram(string Title, long Start)
    {
      if (_dbPrograms.Count > 0)
      {
        for (int i = _dbLastProg; i < _dbPrograms.Count; i++)
        {
          TVProgram prog = (TVProgram)_dbPrograms[i];

          if (prog.Title == Title && prog.Start == Start)
          {
            _dbLastProg = i;
            return prog;
          }
        }

        for (int i = 0; i < _dbLastProg; i++)
        {
          TVProgram prog = (TVProgram)_dbPrograms[i];

          if (prog.Title == Title && prog.Start == Start)
          {
            _dbLastProg = i;
            return prog;
          }
        }
      }
      return null;
    }

    private TVProgram GetProgram(int index)
    {
      ProgramData guideData = (ProgramData)_parser.GetData(index);

      if (guideData == null ||
        guideData.StartTime == null || guideData.Title == string.Empty)
      {
        return null;
      }

      // Set ChannelId
      guideData.ChannelId = _strID;

      if (_grabber.Actions != null && guideData.IsRemoved(_grabber.Actions))
        return null;

      //_log.Debug(LogType.WebEPG, "WebEPG: Guide, Program title: {0}", guideData.Title);
      //_log.Debug(LogType.WebEPG, "WebEPG: Guide, Program start: {0}:{1} - {2}/{3}/{4}", guideData.StartTime.Hour, guideData.StartTime.Minute, guideData.StartTime.Day, guideData.StartTime.Month, guideData.StartTime.Year);
      //if (guideData.EndTime != null)
      //  _log.Debug(LogType.WebEPG, "WebEPG: Guide, Program end  : {0}:{1} - {2}/{3}/{4}", guideData.EndTime.Hour, guideData.EndTime.Minute, guideData.EndTime.Day, guideData.EndTime.Month, guideData.EndTime.Year);
      //_log.Debug(LogType.WebEPG, "WebEPG: Guide, Program desc.: {0}", guideData.Description);
      //_log.Debug(LogType.WebEPG, "WebEPG: Guide, Program genre: {0}", guideData.Genre);

      // Adjust Time
      if (guideData.StartTime.Day == 0 || guideData.StartTime.Month == 0 || guideData.StartTime.Year == 0)
      {
        if (!_timeControl.CheckAdjustTime(ref guideData))
          return null;
      }

      //Set TimeZone
      guideData.StartTime.TimeZone = _siteTimeZone;
      if (guideData.EndTime != null)
      {
        guideData.EndTime.TimeZone = _siteTimeZone;
        _log.Info(LogType.WebEPG, "WebEPG: Guide, Program Info: {0} / {1} - {2}", guideData.StartTime.ToLocalLongDateTime(), guideData.EndTime.ToLocalLongDateTime(), guideData.Title);
      }
      else
      {
        _log.Info(LogType.WebEPG, "WebEPG: Guide, Program Info: {0} - {1}", guideData.StartTime.ToLocalLongDateTime(), guideData.Title);
      }

      if (guideData.StartTime.ToLocalTime() < _grabStart.AddHours(-2))
      {
        _log.Info(LogType.WebEPG, "WebEPG: Program starts in the past, ignoring it.");
        return null;
      }

      // Check TV db if program exists
      if (_dblookup)
      {
        TVProgram dbProg = dbProgram(guideData.Title, guideData.StartTime.ToLocalLongDateTime());
        if (dbProg != null)
        {
          _log.Info(LogType.WebEPG, "WebEPG: Program already in db");
          dbProg.Channel = _strID;
          return dbProg;
        }
      }

      // SubLink
      if (guideData.HasSublink())
      {
        if (_parser is WebParser)
        {
          WebParser webParser = (WebParser)_parser;

          if (!webParser.GetLinkedData(ref guideData))
            _log.Warn(LogType.WebEPG, "WebEPG: Getting sublinked data failed");
          else
            _log.Debug(LogType.WebEPG, "WebEPG: Getting sublinked data sucessful");

        }
      }

      if (_grabber.Actions != null)
        guideData.Replace(_grabber.Actions);

      return guideData.ToTvProgram();
    }

    private bool GetListing(out bool error)
    {
      int listingCount = 0;
      bool bMore = false;
      error = false;

      HTTPRequest request = _reqBuilder.GetRequest();

      _log.Info(LogType.WebEPG, "WebEPG: Reading {0}", request.ToString());

      listingCount = _parser.ParseUrl(request);

      if (listingCount == 0) // && _maxListingCount == 0)
      {
        if (_grabber.Listing.SearchParameters.MaxListingCount == 0 || (_grabber.Listing.SearchParameters.MaxListingCount != 0 && _reqBuilder.Offset == 0))
        {
          _log.Info(LogType.WebEPG, "WebEPG: No Listings Found");
          _reqBuilder.AddDays(1); // _GrabDay++;
          error = true;
        }
        else
        {
          _log.Info(LogType.WebEPG, "WebEPG: Listing Count 0");
        }
        //_GrabDay++;
      }
      else
      {
        _log.Info(LogType.WebEPG, "WebEPG: Listing Count {0}", listingCount);

        if (listingCount == _grabber.Listing.SearchParameters.MaxListingCount) // || _pageStart + offset < _pageEnd)
          bMore = true;

        for (int i = 0; i < listingCount; i++)
        {
          TVProgram program = GetProgram(i);
          if (program != null)
          {
            _programs.Add(program);
          }
        }

        if (_timeControl.GrabDay > _maxGrabDays) //_GrabDay > _maxGrabDays)
          bMore = false;
      }

      return bMore;
    }
    #endregion
  }
}
