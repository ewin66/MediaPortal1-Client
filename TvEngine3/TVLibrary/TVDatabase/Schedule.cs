//========================================================================
// This file was generated using the MyGeneration tool in combination
// with the Gentle.NET Business Entity template, $Rev: 965 $
//========================================================================
using System;
using System.Collections;
using Gentle.Common;
using Gentle.Framework;

namespace TvDatabase
{
  public enum KeepMethodType
  {
    UntilSpaceNeeded,
    UntilWatched,
    TillDate,
    Always
  }
  public enum ScheduleRecordingType
  {
    Once,
    Daily,
    Weekly,
    EveryTimeOnThisChannel,
    EveryTimeOnEveryChannel,
    Weekends,
    WorkingDays
  }
  /// <summary>
  /// Instances of this class represent the properties and methods of a row in the table <b>Schedule</b>.
  /// </summary>
  [TableName("Schedule")]
  public class Schedule : Persistent
  {
    public enum QualityType
    {
      NotSet,
      Portable,
      Low,
      Medium,
      High
    }
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    static public readonly int HighestPriority = Int32.MaxValue;
    static public readonly int LowestPriority = 0;
    bool _isSeries = false;
    #region Members
    private bool isChanged;
    [TableColumn("id_Schedule", NotNull = true), PrimaryKey(AutoGenerated = true)]
    private int idSchedule;
    [TableColumn("idChannel", NotNull = true), ForeignKey("Channel", "idChannel")]
    private int idChannel;
    [TableColumn("scheduleType", NotNull = true)]
    private int scheduleType;
    [TableColumn("programName", NotNull = true)]
    private string programName;
    [TableColumn("startTime", NotNull = true)]
    private DateTime startTime;
    [TableColumn("endTime", NotNull = true)]
    private DateTime endTime;
    [TableColumn("maxAirings", NotNull = true)]
    private int maxAirings;
    [TableColumn("priority", NotNull = true)]
    private int priority;
    [TableColumn("directory", NotNull = true)]
    private string directory;
    [TableColumn("quality", NotNull = true)]
    private int quality;
    [TableColumn("keepMethod", NotNull = true)]
    private int keepMethod;
    [TableColumn("keepDate", NotNull = true)]
    private DateTime keepDate;
    [TableColumn("preRecordInterval", NotNull = true)]
    private int preRecordInterval;
    [TableColumn("postRecordInterval", NotNull = true)]
    private int postRecordInterval;
    [TableColumn("canceled", NotNull = true)]
    private DateTime canceled;
    [TableColumn("recommendedCard", NotNull = true)]
    private int recommendedCard;
    #endregion

    #region Constructors

    public Schedule(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      isChanged = true;
      this.idChannel = idChannel;
      ProgramName = programName;
      Canceled = MinSchedule;
      Directory = "";
      EndTime = endTime;
      KeepDate = MinSchedule;
      KeepMethod = (int)KeepMethodType.UntilSpaceNeeded;
      MaxAirings = 5;
      PostRecordInterval = 0;
      PreRecordInterval = 0;
      Priority = 0;
      Quality = 0;
      ScheduleType = (int)ScheduleRecordingType.Once;
      Series = false;
      StartTime = startTime;
      this.recommendedCard = -1;
    }

    /// <summary> 
    /// Create a new object by specifying all fields (except the auto-generated primary key field). 
    /// </summary> 
    public Schedule(int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
    {
      isChanged = true;
      this.idChannel = idChannel;
      this.scheduleType = scheduleType;
      this.programName = programName;
      this.startTime = startTime;
      this.endTime = endTime;
      this.maxAirings = maxAirings;
      this.priority = priority;
      this.directory = directory;
      this.quality = quality;
      this.keepMethod = keepMethod;
      this.keepDate = keepDate;
      this.preRecordInterval = preRecordInterval;
      this.postRecordInterval = postRecordInterval;
      this.canceled = canceled;
      this.recommendedCard = -1;
    }

    /// <summary> 
    /// Create an object from an existing row of data. This will be used by Gentle to 
    /// construct objects from retrieved rows. 
    /// </summary> 
    public Schedule(int idSchedule, int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
    {
      this.idSchedule = idSchedule;
      this.idChannel = idChannel;
      this.scheduleType = scheduleType;
      this.programName = programName;
      this.startTime = startTime;
      this.endTime = endTime;
      this.maxAirings = maxAirings;
      this.priority = priority;
      this.directory = directory;
      this.quality = quality;
      this.keepMethod = keepMethod;
      this.keepDate = keepDate;
      this.preRecordInterval = preRecordInterval;
      this.postRecordInterval = postRecordInterval;
      this.canceled = canceled;
      this.recommendedCard = -1;
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Indicates whether the entity is changed and requires saving or not.
    /// </summary>
    public bool IsChanged
    {
      get { return isChanged; }
    }

    /// <summary>
    /// Property relating to database column id_Schedule
    /// </summary>
    public int IdSchedule
    {
      get { return idSchedule; }
    }

    /// <summary>
    /// Property to get/set the card id recommended by ConflictsManager plugin
    /// </summary>
    public int RecommendedCard
    {
      get { return recommendedCard; }
      set { isChanged |= recommendedCard != value; recommendedCard = value; }
    }

    /// <summary>
    /// Property relating to database column idChannel
    /// </summary>
    public int IdChannel
    {
      get { return idChannel; }
      set { isChanged |= idChannel != value; idChannel = value; }
    }

    /// <summary>
    /// Property relating to database column scheduleType
    /// </summary>
    public int ScheduleType
    {
      get { return scheduleType; }
      set { isChanged |= scheduleType != value; scheduleType = value; }
    }

    /// <summary>
    /// Property relating to database column programName
    /// </summary>
    public string ProgramName
    {
      get { return programName; }
      set { isChanged |= programName != value; programName = value; }
    }

    /// <summary>
    /// Property relating to database column startTime
    /// </summary>
    public DateTime StartTime
    {
      get { return startTime; }
      set { isChanged |= startTime != value; startTime = value; }
    }

    /// <summary>
    /// Property relating to database column endTime
    /// </summary>
    public DateTime EndTime
    {
      get { return endTime; }
      set { isChanged |= endTime != value; endTime = value; }
    }

    /// <summary>
    /// Property relating to database column maxAirings
    /// </summary>
    public int MaxAirings
    {
      get { return maxAirings; }
      set { isChanged |= maxAirings != value; maxAirings = value; }
    }

    /// <summary>
    /// Property relating to database column priority
    /// </summary>
    public int Priority
    {
      get { return priority; }
      set { isChanged |= priority != value; priority = value; }
    }

    /// <summary>
    /// Property relating to database column directory
    /// </summary>
    public string Directory
    {
      get { return directory; }
      set { isChanged |= directory != value; directory = value; }
    }

    /// <summary>
    /// Property relating to database column quality
    /// </summary>
    public int Quality
    {
      get { return quality; }
      set { isChanged |= quality != value; quality = value; }
    }

    /// <summary>
    /// Property relating to database column keepMethod
    /// </summary>
    public int KeepMethod
    {
      get { return keepMethod; }
      set { isChanged |= keepMethod != value; keepMethod = value; }
    }

    /// <summary>
    /// Property relating to database column keepDate
    /// </summary>
    public DateTime KeepDate
    {
      get { return keepDate; }
      set { isChanged |= keepDate != value; keepDate = value; }
    }

    /// <summary>
    /// Property relating to database column preRecordInterval
    /// </summary>
    public int PreRecordInterval
    {
      get { return preRecordInterval; }
      set { isChanged |= preRecordInterval != value; preRecordInterval = value; }
    }

    /// <summary>
    /// Property relating to database column postRecordInterval
    /// </summary>
    public int PostRecordInterval
    {
      get { return postRecordInterval; }
      set { isChanged |= postRecordInterval != value; postRecordInterval = value; }
    }

    /// <summary>
    /// Property relating to database column canceled
    /// </summary>
    public DateTime Canceled
    {
      get { return canceled; }
      set { isChanged |= canceled != value; canceled = value; }
    }
    #endregion

    #region Storage and Retrieval

    /// <summary>
    /// Static method to retrieve all instances that are stored in the database in one call
    /// </summary>
    public static IList ListAll()
    {
      return Broker.RetrieveList(typeof(Schedule));
    }

    /// <summary>
    /// Retrieves an entity given it's id.
    /// </summary>
    public static Schedule Retrieve(int id)
    {
      // Return null if id is smaller than seed and/or increment for autokey
      if (id < 1)
      {
        return null;
      }
      Key key = new Key(typeof(Schedule), true, "id_Schedule", id);
      return Broker.RetrieveInstance(typeof(Schedule), key) as Schedule;
    }

    /// <summary>
    /// Retrieves an entity given it's id, using Gentle.Framework.Key class.
    /// This allows retrieval based on multi-column keys.
    /// </summary>
    public static Schedule Retrieve(Key key)
    {
      return Broker.RetrieveInstance(typeof(Schedule), key) as Schedule;
    }

    /// <summary>
    /// Persists the entity if it was never persisted or was changed.
    /// </summary>
    public override void Persist()
    {
      if (IsChanged || !IsPersisted)
      {
        base.Persist();
        isChanged = false;
      }
    }

    #endregion

    #region Relations

    /// <summary>
    /// Get a list of CanceledSchedule referring to the current entity.
    /// </summary>
    public IList ReferringCanceledSchedule()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(CanceledSchedule));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idSchedule", idSchedule);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection(typeof(CanceledSchedule), stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(CanceledSchedule), this );
    }
    /// <summary>
    /// Get a list of Conflicts referring to the current entity.
    /// </summary>
    public IList ReferringConflicts()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Conflict));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idSchedule", idSchedule);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection(typeof(Conflict), stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(CanceledSchedule), this );
    }

    /// <summary>
    /// Get a list of Conflicts referring to the current entity.
    /// </summary>
    public IList ConflictingSchedules()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Conflict));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idConflictingSchedule", idSchedule);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection(typeof(Conflict), stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(CanceledSchedule), this );
    }


    /// <summary>
    ///
    /// </summary>
    public Channel ReferencedChannel()
    {
      return Channel.Retrieve(IdChannel);
    }
    #endregion

    public bool IsSerieIsCanceled(DateTime startTime)
    {
      foreach (CanceledSchedule schedule in ReferringCanceledSchedule())
      {
        if (schedule.CancelDateTime == startTime) return true;
      }
      return false;
    }
    public void UnCancelSerie(DateTime startTime)
    {
      foreach (CanceledSchedule schedule in ReferringCanceledSchedule())
      {
        if (schedule.CancelDateTime == startTime)
        {
          schedule.Remove();
          return;
        }
      }
      return;
    }

    /// <summary>
    /// Checks if the recording should record the specified tvprogram
    /// </summary>
    /// <param name="program">TVProgram to check</param>
    /// <returns>true if the specified tvprogram should be recorded</returns>
    /// <param name="filterCanceledRecordings">(true/false)
    /// if true then  we'll return false if recording has been canceled for this program
    /// if false then we'll return true if recording has been not for this program</param>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingProgram(Program program, bool filterCanceledRecordings)
    {
      ScheduleRecordingType scheduleType = (ScheduleRecordingType)this.ScheduleType;
      switch (scheduleType)
      {
        case ScheduleRecordingType.Once:
          {
            if (program.StartTime == StartTime && program.EndTime == EndTime && program.IdChannel == IdChannel)
            {
              if (filterCanceledRecordings)
              {
                if (this.ReferringCanceledSchedule().Count > 0) return false;
              }
              return true;
            }
          }
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          if (program.Title == ProgramName)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          if (program.Title == ProgramName && program.IdChannel == IdChannel)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
        case ScheduleRecordingType.Daily:
        case ScheduleRecordingType.WorkingDays:
        case ScheduleRecordingType.Weekends:
        case ScheduleRecordingType.Weekly:
          if (IsInFuzzyTimeSlot(program.IdChannel, program.Title, program.StartTime))
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
      }
      return false;
    }

    public bool DoesUseEpisodeManagement
    {
      get
      {
        if (ScheduleType == (int)ScheduleRecordingType.Once) return false;
        if (MaxAirings == Int32.MaxValue) return false;
        if (MaxAirings < 1) return false;
        return true;
      }
    }
    /// <summary>
    /// Checks whether this recording is finished and can be deleted
    /// 
    /// </summary>
    /// <returns>true:Recording is finished can be deleted
    ///          false:Recording is not done yet, or needs to be done multiple times
    /// </returns>
    public bool IsDone()
    {
      if (ScheduleType != (int)ScheduleRecordingType.Once) return false;
      if (DateTime.Now > EndTime) return true;
      return false;
    }

    public void Delete()
    {
      IList list = ReferringConflicts();
      foreach (Conflict conflict in list)
        conflict.Remove();

      list = ConflictingSchedules();
      foreach (Conflict conflict in list)
        conflict.Remove();

      list = ReferringCanceledSchedule();
      foreach (CanceledSchedule schedule in list)
        schedule.Remove();
      Remove();
    }

    public bool Series
    {
      get
      {
        return _isSeries;
      }
      set
      {
        _isSeries = value;
      }
    }
    public Schedule Clone()
    {
      Schedule schedule = new Schedule(IdChannel, scheduleType, ProgramName, StartTime, EndTime, MaxAirings, Priority, Directory, Quality, KeepMethod, KeepDate, PreRecordInterval, PostRecordInterval, Canceled);

      schedule._isSeries = _isSeries;
      schedule.idSchedule = idSchedule;
      schedule.isChanged = false;
      return schedule;
    }

    public bool IsOverlapping(Schedule schedule)
    {
      DateTime Start1, Start2, End1, End2;

      Start1 = this.StartTime.AddMinutes(-this.preRecordInterval);
      Start2 = schedule.StartTime.AddMinutes(-schedule.preRecordInterval);
      End1 = this.EndTime.AddMinutes(this.postRecordInterval);
      End2 = schedule.EndTime.AddMinutes(schedule.postRecordInterval);

      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if ((Start2 >= Start1 && Start2 < End1) ||
          (Start2 <= Start1 && End2 >= End1) ||
          (End2 > Start1 && End2 <= End1)) return true;
      return false;
    }

    /// <summary>
    /// checks if 2 schedules have a common Transponder
    /// depending on tuningdetails of their respective channels
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns>True if a common transponder exists</returns>
    public bool isSameTransponder(Schedule schedule)
    {
      IList tuningList1 = this.ReferencedChannel().ReferringTuningDetail();
      IList tuningList2 = schedule.ReferencedChannel().ReferringTuningDetail();
      foreach (TuningDetail tun1 in tuningList1)
      {
        foreach (TuningDetail tun2 in tuningList2)
        {
          if (tun1.Frequency == tun2.Frequency) return true;
        }
      }
      return false;
      
    }

    /// <summary>
    /// Get the schedule's start and end-times nearest to the specified time.
    /// </summary>
    /// <param name="date">The date to get the recording times on.</param>
    /// <returns>False if the schedule is not valid for the given date.</returns>
    public bool GetTimesNearestTo(DateTime time, out DateTime recStartTime, out DateTime recEndTime)
    {
      // Get the schedule's times on the date of the requested time, the day before and the day after.
      DateTime recToday = new DateTime(time.Year, time.Month, time.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
      DateTime recYesterday = recToday.AddDays(-1);
      DateTime recTomorrow = recToday.AddDays(1);

      // Now find the schedule time that is closest to the requested time.
      double todayDiff = Math.Abs(time.Subtract(recToday).TotalMinutes);
      double yesterdayDiff = Math.Abs(time.Subtract(recYesterday).TotalMinutes);
      double tomorrowDiff = Math.Abs(time.Subtract(recTomorrow).TotalMinutes);
      if (tomorrowDiff < todayDiff)
      {
        recStartTime = recTomorrow;
      }
      else if (yesterdayDiff < todayDiff)
      {
        recStartTime = recYesterday;
      }
      else
      {
        recStartTime = recToday;
      }

      // Finally check if the schedule is valid on this day.
      bool recordOnDate = false;
      if (this.ScheduleType == (int)ScheduleRecordingType.Daily)
      {
        recordOnDate = true;
      }
      else if (this.ScheduleType == (int)ScheduleRecordingType.Weekends)
      {
        recordOnDate = (recStartTime.DayOfWeek == DayOfWeek.Saturday || recStartTime.DayOfWeek == DayOfWeek.Sunday);
      }
      else if (this.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        recordOnDate = (recStartTime.DayOfWeek >= DayOfWeek.Monday && recStartTime.DayOfWeek <= DayOfWeek.Friday);
      }
      else if (this.ScheduleType == (int)ScheduleRecordingType.Weekly)
      {
        recordOnDate = (this.StartTime.DayOfWeek == recStartTime.DayOfWeek);
      }
      else
      {
        throw new ArgumentException("GetTimesNearestTo(): ScheduleRecordingType must be Daily, Weekly, Weekends or WorkingDays");
      }
      if (recordOnDate)
      {
        // We found a valid schedule. Calculate the end time and return success.
        recEndTime = recStartTime.Add(this.EndTime - this.StartTime);
        return true;
      }
      else
      {
        recStartTime = DateTime.MinValue;
        recEndTime = DateTime.MinValue;
        return false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="programTitle"></param>
    /// <param name="programTime"></param>
    /// <returns></returns>
    public bool IsInFuzzyTimeSlot(int channelId, string programTitle, DateTime programTime)
    {
      DateTime recStartTime;
      DateTime recEndTime;
      if (GetTimesNearestTo(programTime, out recStartTime, out recEndTime))
      {
        if (this.IdChannel == channelId
            && this.programName == programTitle)
        {
          TimeSpan diffSpan = recStartTime.Subtract(programTime);
          TimeSpan recordingLength = this.EndTime - this.StartTime;
          // Check as far as one minute below running time.
          int maxMinutesDiff = Math.Max((int)recordingLength.TotalMinutes - 1, 1);
          // Don't go overboard and never check more than an approx hour out of sync.
          maxMinutesDiff = Math.Min(maxMinutesDiff, 59);
          if (Math.Abs(diffSpan.TotalMinutes) <= maxMinutesDiff)
          {
            return true;
          }
        }
      }
      return false;
    }

    public void GetRecordingDetails(DateTime currentTime, out Channel channel, out DateTime startTime, out DateTime endTime, out bool isDue)
    {
      Program current = null;
      Program next = null;
      Program target = null;

      channel = ReferencedChannel();
      startTime = DateTime.MinValue;
      endTime = DateTime.MinValue;
      isDue = false;

      ScheduleRecordingType type = (ScheduleRecordingType)scheduleType;

      switch (type)
      {
        case ScheduleRecordingType.Once:
          startTime = this.startTime;
          endTime = this.endTime;
          if (currentTime >= startTime.AddMinutes(-preRecordInterval) &&
            currentTime <= endTime.AddMinutes(postRecordInterval))
          {
            isDue = true;
          }
          break;
        case ScheduleRecordingType.Daily:
        case ScheduleRecordingType.Weekends:
        case ScheduleRecordingType.WorkingDays:
        case ScheduleRecordingType.Weekly:
          current = ReferencedChannel().CurrentProgram;
          next = ReferencedChannel().NextProgram;
          if (current != null)
            target = current;
          else if (next != null)
            target = next;

          if (target != null)
          {
            channel = target.ReferencedChannel();
            startTime = target.StartTime;
            endTime = target.EndTime;
            if (currentTime >= target.StartTime.AddMinutes(-preRecordInterval) && currentTime <= target.EndTime.AddMinutes(postRecordInterval))
            {
              if (IsInFuzzyTimeSlot(current.IdChannel, target.Title, target.StartTime))
              {
                if (!IsSerieIsCanceled(target.StartTime))
                {
                  isDue = true;
                }
              }
            }
          }
          else
          {
            // If there is no guide information available, simply check the schedule's set time.
            channel = next.ReferencedChannel();
            if (GetTimesNearestTo(currentTime, out startTime, out endTime))
            {
              if (currentTime >= startTime.AddMinutes(-preRecordInterval) &&
                  currentTime <= endTime.AddMinutes(postRecordInterval))
              {
                isDue = true;
              }
            }
          }
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          current = ReferencedChannel().CurrentProgram;
          next = ReferencedChannel().NextProgram;
          if (current != null)
            target = current;
          else if (next != null)
            target = next;

          if (target != null)
          {
            channel = target.ReferencedChannel();
            startTime = target.StartTime;
            endTime = target.EndTime;
            if (currentTime >= target.StartTime.AddMinutes(-preRecordInterval) && currentTime <= target.EndTime.AddMinutes(postRecordInterval))
            {
              if (String.Compare(current.Title, programName, true) == 0)
              {
                if (!IsSerieIsCanceled(target.StartTime))
                {
                  isDue = true;
                }
              }
            }
          }
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          foreach (Channel chan in Channel.ListAll())
          {
            current = chan.CurrentProgram;
            next = chan.NextProgram;
            if (current != null)
              target = current;
            else if (next != null)
              target = next;

            if (target != null)
            {
              channel = target.ReferencedChannel();
              startTime = target.StartTime;
              endTime = target.EndTime;
              if (currentTime >= target.StartTime.AddMinutes(-preRecordInterval) && currentTime <= target.EndTime.AddMinutes(postRecordInterval))
              {
                if (String.Compare(target.Title, programName, true) == 0)
                {
                  if (!IsSerieIsCanceled(target.StartTime))
                  {
                    isDue = true;
                    break;
                  }
                }
              }
            }
          }
          break;
      }
    }

    public override string ToString()
    {
      return String.Format("{0} on {1} {2} - {3}", ProgramName, IdChannel, StartTime, EndTime);
    }
  }
}
