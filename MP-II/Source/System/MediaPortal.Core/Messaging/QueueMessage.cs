#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;

namespace MediaPortal.Core.Messaging
{
  /// <summary>
  /// Message to be sent via <see cref="IMessageBroker"/>
  /// </summary>
  public class QueueMessage
  {
    #region Protected fields

    protected string _queue;
    protected IDictionary<string, object> _metaData = new Dictionary<string, object>();

    #endregion

    #region Public properties

    /// <summary>
    /// Gets or sets the name of the queue this message is sent.
    /// </summary>
    public string MessageQueue 
    {
      get { return _queue; }
      set { _queue = value; }
    }

    /// <summary>
    /// Gets or sets the message data. The message data is a generic dictionary special
    /// data entries defined by the message queue.
    /// </summary>
    /// <value>The meta data.</value>
    public IDictionary<string, object> MessageData
    {
      get { return _metaData; }
      set { _metaData = value; }
    }

    #endregion
  }
}
