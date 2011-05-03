#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Gentle.Framework;
using MediaPortal.CoreServices;

namespace TvDatabase
{
  /// <summary>
  /// Instances of this class represent the properties and methods of a row in the table <b>ChannelLinkageMap</b>.
  /// </summary>
  [TableName("ChannelLinkageMap")]
  public class ChannelLinkageMap : Persistent
  {
    #region Members

    private bool isChanged;
    [TableColumn("idMapping", NotNull = true), PrimaryKey(AutoGenerated = true)] private int idMapping;
    [TableColumn("idPortalChannel", NotNull = true)] private int idPortalChannel;
    [TableColumn("idLinkedChannel", NotNull = true)] private int idLinkedChannel;
    [TableColumn("displayName", NotNull = true)] private string displayName;

    #endregion

    #region Constructors

    /// <summary> 
    /// Create a new object by specifying all fields (except the auto-generated primary key field). 
    /// </summary> 
    public ChannelLinkageMap(int idPortalChannel, int idLinkedChannel, string displayName)
    {
      isChanged = true;
      this.idPortalChannel = idPortalChannel;
      this.idLinkedChannel = idLinkedChannel;
      this.displayName = displayName;
    }

    /// <summary> 
    /// Create an object from an existing row of data. This will be used by Gentle to 
    /// construct objects from retrieved rows. 
    /// </summary> 
    public ChannelLinkageMap(int idMapping, int idPortalChannel, int idLinkedChannel, string displayName)
    {
      this.idMapping = idMapping;
      this.idPortalChannel = idPortalChannel;
      this.idLinkedChannel = idLinkedChannel;
      this.displayName = displayName;
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
      set
      {
        isChanged |= IdPortalChannel != value;
        IdPortalChannel = value;
      }
    }

    /// <summary>
    /// Property relating to database column idLinkedChannel
    /// </summary>
    public int IdLinkedChannel
    {
      get { return idLinkedChannel; }
      set
      {
        isChanged |= idLinkedChannel != value;
        idLinkedChannel = value;
      }
    }

    /// <summary>
    /// Property relating to database column displayName
    /// </summary>
    public string DisplayName
    {
      get { return displayName; }
      set
      {
        isChanged |= displayName != value;
        displayName = value;
      }
    }

    #endregion

    #region Storage and Retrieval

    /// <summary>
    /// Static method to retrieve all instances that are stored in the database in one call
    /// </summary>
    public static IList<ChannelLinkageMap> ListAll()
    {
      return Broker.RetrieveList<ChannelLinkageMap>();
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
      Key key = new Key(typeof (ChannelLinkageMap), true, "idMapping", id);
      return Broker.RetrieveInstance<ChannelLinkageMap>(key);
    }

    /// <summary>
    /// Retrieves an entity given it's id, using Gentle.Framework.Key class.
    /// This allows retrieval based on multi-column keys.
    /// </summary>
    public static ChannelLinkageMap Retrieve(Key key)
    {
      return Broker.RetrieveInstance<ChannelLinkageMap>(key);
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
          GlobalServiceProvider.Instance.Get<ILogger>().Error("Exception in ChannelLinkageMap.Persist() with Message {0}", ex.Message);
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