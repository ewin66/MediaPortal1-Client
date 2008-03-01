using System;
using System.Collections;
using Gentle.Common;
using Gentle.Framework;
using TvLibrary.Log;

namespace TvDatabase
{
  /// <summary>
  /// Instances of this class represent the properties and methods of a row in the table <b>ChannelLinkageMap</b>.
  /// </summary>
  [TableName("ChannelLinkageMap")]
  public class ChannelLinkageMap: Persistent
  {
    #region Members
    private bool isChanged;
    [TableColumn("idMapping", NotNull = true), PrimaryKey(AutoGenerated = true)]
    private int idMapping;
    [TableColumn("idPortalChannel", NotNull = true)]
    private int idPortalChannel;
    [TableColumn("idLinkedChannel", NotNull = true)]
    private int idLinkedChannel;
    #endregion

    #region Constructors
    /// <summary> 
    /// Create a new object by specifying all fields (except the auto-generated primary key field). 
    /// </summary> 
    public ChannelLinkageMap(int idPortalChannel, int idLinkedChannel)
    {
      isChanged = true;
      this.idPortalChannel = idPortalChannel;
      this.idLinkedChannel = idLinkedChannel;
    }

    /// <summary> 
    /// Create an object from an existing row of data. This will be used by Gentle to 
    /// construct objects from retrieved rows. 
    /// </summary> 
    public ChannelLinkageMap(int idMapping, int idPortalChannel, int idLinkedChannel)
    {
      this.idMapping = idMapping;
      this.idPortalChannel = idPortalChannel;
      this.idLinkedChannel = idLinkedChannel;
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
    /// Property relating to database column idMapping
    /// </summary>
    public int IdMapping
    {
      get { return idMapping; }
    }

    /// <summary>
    /// Property relating to database column idPortalChannel
    /// </summary>
    public int IdPortalChannel
    {
      get { return idPortalChannel; }
      set { isChanged |= IdPortalChannel != value; IdPortalChannel = value; }
    }
    /// <summary>
    /// Property relating to database column idLinkedChannel
    /// </summary>
    public int IdLinkedChannel
    {
      get { return idLinkedChannel; }
      set { isChanged |= idLinkedChannel != value; idLinkedChannel = value; }
    }
    #endregion

    #region Storage and Retrieval

    /// <summary>
    /// Static method to retrieve all instances that are stored in the database in one call
    /// </summary>
    public static IList ListAll()
    {
      return Broker.RetrieveList(typeof(ChannelLinkageMap));
    }

    /// <summary>
    /// Retrieves an entity given it's id.
    /// </summary>
    public static ChannelLinkageMap Retrieve(int id)
    {
      // Return null if id is smaller than seed and/or increment for autokey
      if (id < 1)
      {
        return null;
      }
      Key key = new Key(typeof(ChannelLinkageMap), true, "idMapping", id);
      return Broker.RetrieveInstance(typeof(ChannelLinkageMap), key) as ChannelLinkageMap;
    }

    /// <summary>
    /// Retrieves an entity given it's id, using Gentle.Framework.Key class.
    /// This allows retrieval based on multi-column keys.
    /// </summary>
    public static ChannelLinkageMap Retrieve(Key key)
    {
      return Broker.RetrieveInstance(typeof(ChannelLinkageMap), key) as ChannelLinkageMap;
    }

    /// <summary>
    /// Persists the entity if it was never persisted or was changed.
    /// </summary>
    public override void Persist()
    {
      if (IsChanged || !IsPersisted)
      {
        try
        {
          base.Persist();
        }
        catch (Exception ex)
        {
          Log.Error("Exception in ChannelLinkageMap.Persist() with Message {0}", ex.Message);
          return;
        }
        isChanged = false;
      }
    }

    #endregion

    #region Relations

    public Channel ReferringPortalChannel()
    {
      return Channel.Retrieve(idPortalChannel);
    }
    public Channel ReferringLinkedChannel()
    {
      return Channel.Retrieve(idLinkedChannel);
    }
    #endregion
  }
}
