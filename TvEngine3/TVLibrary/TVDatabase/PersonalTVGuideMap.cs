//========================================================================
// This file was generated using the MyGeneration tool in combination
// with the Gentle.NET Business Entity template, $Rev: 965 $
//========================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using Gentle.Common;
using Gentle.Framework;

namespace TvDatabase
{
	/// <summary>
	/// Instances of this class represent the properties and methods of a row in the table <b>PersonalTVGuideMap</b>.
	/// Database used by PersonalTVGuide plugin
	/// </summary>
	[TableName("PersonalTVGuideMap")]
	public class PersonalTVGuideMap : Persistent
	{
		#region Members
		private bool isChanged;
		[TableColumn("idPersonalTVGuideMap", NotNull = true), PrimaryKey(AutoGenerated = true)]
		private int idPersonalTVGuideMap;
		[TableColumn("idKeyword", NotNull = true), ForeignKey("Keyword", "idKeyword")]
		private int idKeyword;
		[TableColumn("idProgram", NotNull = true), ForeignKey("Program", "idProgram")]
		private int idProgram;
		#endregion

		#region Constructors
		/// <summary> 
		/// Create a new object by specifying all fields (except the auto-generated primary key field). 
		/// </summary> 
		public PersonalTVGuideMap(int idKeyword, int idProgram)
		{
			isChanged = true;
			this.idKeyword = idKeyword;
			this.idProgram = idProgram;
		}

		/// <summary> 
		/// Create an object from an existing row of data. This will be used by Gentle to 
		/// construct objects from retrieved rows. 
		/// </summary> 
		public PersonalTVGuideMap(int idPersonalTVGuideMap, int idKeyword, int idProgram)
		{
			this.idPersonalTVGuideMap = idPersonalTVGuideMap;
			this.idKeyword = idKeyword;
			this.idProgram = idProgram;
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
		/// Property relating to database column idPersonalTVGuideMap
		/// </summary>
		public int IdPersonalTVGuideMap
		{
			get { return idPersonalTVGuideMap; }
		}

		/// <summary>
		/// Property relating to database column idKeyword
		/// </summary>
		public int IdKeyword
		{
			get { return idKeyword; }
			set { isChanged |= idKeyword != value; idKeyword = value; }
		}

		/// <summary>
		/// Property relating to database column idProgram
		/// </summary>
		public int IdProgram
		{
			get { return idProgram; }
			set { isChanged |= idProgram != value; idProgram = value; }
		}
		#endregion

		#region Storage and Retrieval

		/// <summary>
		/// Static method to retrieve all instances that are stored in the database in one call
		/// </summary>
		public static IList ListAll()
		{
			return Broker.RetrieveList(typeof(PersonalTVGuideMap));
		}

		/// <summary>
		/// Retrieves an entity given it's id.
		/// </summary>
		public static PersonalTVGuideMap Retrieve(int id)
		{
			// Return null if id is smaller than seed and/or increment for autokey
			if (id < 1)
			{
				return null;
			}
			Key key = new Key(typeof(PersonalTVGuideMap), true, "idPersonalTVGuideMap", id);
			return Broker.RetrieveInstance(typeof(PersonalTVGuideMap), key) as PersonalTVGuideMap;
		}

		/// <summary>
		/// Retrieves an entity given it's id, using Gentle.Framework.Key class.
		/// This allows retrieval based on multi-column keys.
		/// </summary>
		public static PersonalTVGuideMap Retrieve(Key key)
		{
			return Broker.RetrieveInstance(typeof(PersonalTVGuideMap), key) as PersonalTVGuideMap;
		}

    /// <summary>
    /// Retrieves a list of Program's with the same KeywordID.
    /// </summary>
    public static List<Program> RetrieveProgramList(int KeywordID)
    {
      if (KeywordID < 1)
      {
        return null;
      }
      Key key = new Key(typeof(PersonalTVGuideMap), true, "idKeyword", KeywordID);
      IList list = Broker.RetrieveList(typeof(PersonalTVGuideMap), key);
      List<Program> programList = new List<Program>();
      foreach (PersonalTVGuideMap map in list)
      {
        if (map.IdProgram > 0)
        {
          Program program = Program.Retrieve(map.IdProgram);
          if (program != null)
          {
            programList.Add(program);
          }
        }
      }
      return programList;
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
		/// Get a list of all PersonalTVGuideMap referring to the current IdKeyword.
		/// </summary>
		public IList ReferencedKeyword()
		{
			SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(PersonalTVGuideMap));
			sb.AddConstraint(Operator.Equals, "idKeyword", IdKeyword);
			SqlStatement stmt = sb.GetStatement(true);
			return ObjectFactory.GetCollection(typeof(PersonalTVGuideMap), stmt.Execute());
		}

    #endregion
	}
}
