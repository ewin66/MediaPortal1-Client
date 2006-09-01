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
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Epg;

namespace TvLibrary.Interfaces
{
  #region delegates
  /// <summary>
  /// delegate callback which for the OnEpgReceived event
  /// </summary>
  /// <param name="sender">sender</param>
  /// <param name="epg">list containing all epg information</param>
  public delegate void EpgReceivedHandler(object sender, List<EpgChannel> epg);
  #endregion

  /// <summary>
  /// interface for dvb epg grabbing
  /// </summary>
  public interface ITVEPG
  {
    #region events
    /// <summary>
    /// Event which gets fired when epg has been received
    /// </summary>
    event EpgReceivedHandler OnEpgReceived;
    #endregion

    /// <summary>
    /// Starts the EPG grabber.
    /// When the epg has been received the OnEpgReceived event will be fired
    /// </summary>
    void GrabEpg();

  }
}
