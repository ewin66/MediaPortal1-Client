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
	/// <summary>
	/// Instances of this class represent the properties and methods of a row in the table <b>Recording</b>.
	/// </summary>
	[TableName("Recording")]
	public class Recording : Persistent
	{
		#region Members
		private bool isChanged;
		[TableColumn("idRecording", NotNull=true), PrimaryKey(AutoGenerated=true)]
		private int idRecording;
		[TableColumn("idChannel", NotNull = true), ForeignKey("Channel", "idChannel")]
		private int idChannel;
		[TableColumn("startTime", NotNull=true)]
		private DateTime startTime;
		[TableColumn("endTime", NotNull=true)]
		private DateTime endTime;
		[TableColumn("title", NotNull=true)]
		private string title;
		[TableColumn("description", NotNull=true)]
		private string description;
		[TableColumn("genre", NotNull=true)]
		private string genre;
		[TableColumn("fileName", NotNull=true)]
		private string fileName;
		[TableColumn("keepUntil", NotNull=true)]
		private int keepUntil;
		[TableColumn("keepUntilDate", NotNull=true)]
		private DateTime keepUntilDate;
		[TableColumn("timesWatched", NotNull=true)]
		private int timesWatched;
		[TableColumn("idServer", NotNull = true), ForeignKey("Server", "idServer")]
		private int idServer;
		#endregion
			
		#region Constructors
		/// <summary> 
		/// Create a new object by specifying all fields (except the auto-generated primary key field). 
		/// </summary> 
		public Recording(int idChannel, DateTime startTime, DateTime endTime, string title, string description, string genre, string fileName, int keepUntil, DateTime keepUntilDate, int timesWatched, int idServer)
		{
			isChanged = true;
			this.idChannel = idChannel;
			this.startTime = startTime;
			this.endTime = endTime;
			this.title = title;
			this.description = description;
			this.genre = genre;
			this.fileName = fileName;
			this.keepUntil = keepUntil;
			this.keepUntilDate = keepUntilDate;
			this.timesWatched = timesWatched;
			this.idServer = idServer;
		}
			
		/// <summary> 
		/// Create an object from an existing row of data. This will be used by Gentle to 
		/// construct objects from retrieved rows. 
		/// </summary> 
		public Recording(int idRecording, int idChannel, DateTime startTime, DateTime endTime, string title, string description, string genre, string fileName, int keepUntil, DateTime keepUntilDate, int timesWatched, int idServer)
		{
			this.idRecording = idRecording;
			this.idChannel = idChannel;
			this.startTime = startTime;
			this.endTime = endTime;
			this.title = title;
			this.description = description;
			this.genre = genre;
			this.fileName = fileName;
			this.keepUntil = keepUntil;
			this.keepUntilDate = keepUntilDate;
			this.timesWatched = timesWatched;
			this.idServer = idServer;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Indicates whether the entity is changed and requires saving or not.
		/// </summary>
		public bool IsChanged
		{
			get	{ return isChanged; }
		}

		/// <summary>
		/// Property relating to database column idRecording
		/// </summary>
		public int IdRecording
		{
			get { return idRecording; }
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
		/// Property relating to database column title
		/// </summary>
		public string Title
		{
			get { return title; }
			set { isChanged |= title != value; title = value; }
		}

		/// <summary>
		/// Property relating to database column description
		/// </summary>
		public string Description
		{
			get { return description; }
			set { isChanged |= description != value; description = value; }
		}

		/// <summary>
		/// Property relating to database column genre
		/// </summary>
		public string Genre
		{
			get { return genre; }
			set { isChanged |= genre != value; genre = value; }
		}

		/// <summary>
		/// Property relating to database column fileName
		/// </summary>
		public string FileName
		{
      get { return this.fileName; }
      set { isChanged |= this.fileName != value; this.fileName = value; }
		}

		/// <summary>
		/// Property relating to database column keepUntil
		/// </summary>
		public int KeepUntil
		{
			get { return keepUntil; }
			set { isChanged |= keepUntil != value; keepUntil = value; }
		}

		/// <summary>
		/// Property relating to database column keepUntilDate
		/// </summary>
		public DateTime KeepUntilDate
		{
			get { return keepUntilDate; }
			set { isChanged |= keepUntilDate != value; keepUntilDate = value; }
		}

		/// <summary>
		/// Property relating to database column timesWatched
		/// </summary>
		public int TimesWatched
		{
			get { return timesWatched; }
			set { isChanged |= timesWatched != value; timesWatched = value; }
		}

		/// <summary>
		/// Property relating to database column idServer
		/// </summary>
		public int IdServer
		{
			get { return idServer; }
			set { isChanged |= idServer != value; idServer = value; }
		}
		#endregion

		#region Storage and Retrieval
	
		/// <summary>
		/// Static method to retrieve all instances that are stored in the database in one call
		/// </summary>
		public static IList ListAll()
		{
			return Broker.RetrieveList(typeof(Recording));
		}

		/// <summary>
		/// Retrieves an entity given it's id.
		/// </summary>
		public static Recording Retrieve(int id)
		{
			// Return null if id is smaller than seed and/or increment for autokey
			if(id<1) 
			{
				return null;
			}
			Key key = new Key(typeof(Recording), true, "idRecording", id);
			return Broker.RetrieveInstance(typeof(Recording), key) as Recording;
		}
		
		/// <summary>
		/// Retrieves an entity given it's id, using Gentle.Framework.Key class.
		/// This allows retrieval based on multi-column keys.
		/// </summary>
		public static Recording Retrieve(Key key)
		{
			return Broker.RetrieveInstance(typeof(Recording), key) as Recording;
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
		///
		/// </summary>
		public Channel ReferencedChannel()
		{
			return Channel.Retrieve(IdChannel);
		}
		/// <summary>
		///
		/// </summary>
		public Server ReferencedServer()
		{
			return Server.Retrieve(IdServer);
		}
		#endregion

    public bool ShouldBeDeleted
    {
      get
      {
        if (KeepUntil != (int)KeepMethodType.TillDate) return false;
        if (KeepUntilDate.Date > DateTime.Now.Date) return false;
        return true;
      }
    }
    public void Delete()
    {
      Remove();
    }
	}
}
