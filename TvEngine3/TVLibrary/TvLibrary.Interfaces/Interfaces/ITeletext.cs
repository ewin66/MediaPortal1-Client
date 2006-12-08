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
using System.Drawing;
using TvLibrary.Interfaces;

namespace TvLibrary.Teletext
{
  /// <summary>
  /// delegate which is called when a teletext page has been received,modified or deleted
  /// </summary>
  /// <param name="pageNumber">pagenumber (100-899)</param>
  /// <param name="subPageNumber">subpagenumber (0-79)</param>
  public delegate void PageEventHandler(int pageNumber, int subPageNumber);

  public interface IVbiCallback
  {
    void OnVbiData( IntPtr data, int len,bool analog);
  }
  /// <summary>
  /// teletext decoder interface
  /// </summary>
  public interface ITeletext
  {
    #region events
    /// <summary>
    /// event which gets fired when the current selected pagenumber is updated
    /// </summary>
    event PageEventHandler OnPageUpdated;
    /// <summary>
    /// event which gets fired when the current selected pagenumber is added
    /// </summary>
    event PageEventHandler OnPageAdded;
    /// <summary>
    /// event which gets fired when the current selected pagenumber is removed
    /// </summary>
    event PageEventHandler OnPageDeleted;
    #endregion

    #region methods
    /// <summary>
    /// returns the channel name found in packet 8/30
    /// </summary>
    /// <returns>string containing the channel name</returns>
    string GetTeletextChannelName();

    /// <summary>
    /// clears the teletext channel name
    /// </summary>
    void ClearTeletextChannelName();

    /// <summary>
    /// sets the width/height of the bitmap generated by GetPage()
    /// </summary>
    /// <param name="renderWidth">width in pixels</param>
    /// <param name="renderHeight">height in pixels</param>
    void SetPageSize(int renderWidth, int renderHeight);

    /// <summary>
    /// Gets the teletext page and renders it to a Bitmap
    /// </summary>
    /// <param name="page">pagenumber (0x100-0x899)</param>
    /// <param name="subpage">subpagenumber (0x0-0x79)</param>
    /// <returns>bitmap (or null if page is not found)</returns>
    Bitmap GetPage(int page, int subpage);

    /// <summary>
    /// Gets the raw teletext page.
    /// </summary>
    /// <param name="page">pagenumber (0x100-0x899)</param>
    /// <param name="subpage">subpagenumber (0x0-0x79)</param>
    /// <returns>raw teletext page (or null if page is not found)</returns>
    byte[] GetRawPage(int page, int subpage);

    /// <summary>
    /// returns the total number of subpages for a pagnumber
    /// </summary>
    /// <param name="currentPageNumber">pagenumber 0x100-0x899</param>
    /// <returns>number of subpages for this pagenumber</returns>
    int NumberOfSubpages(int currentPageNumber);

    /// <summary>
    /// returns the rotation time for the page.
    /// </summary>
    /// <param name="currentPageNumber">The current page number.</param>
    /// <returns>timespan contain the rotation time</returns>
    TimeSpan RotationTime(int currentPageNumber);
    #endregion

    #region fasttext
    /// <summary>
    /// returns the pagenumber for the red button
    /// </summary>
    int PageRed { get;}
    /// <summary>
    /// returns the pagenumber for the green button
    /// </summary>
    int PageGreen { get;}
    /// <summary>
    /// returns the pagenumber for the yellow button
    /// </summary>
    int PageYellow { get;}
    /// <summary>
    /// returns the pagenumber for the blue button
    /// </summary>
    int PageBlue { get;}

    /// <summary>
    /// Gets the page select text.
    /// </summary>
    /// <value>The page select text.</value>
    string PageSelectText { get;}
    /// <summary>
    /// turns on/off the conceal (hidden) mode
    /// </summary>
    bool HiddenMode { get;set;}
    /// <summary>
    /// turns on/off transparent mode. In transparent mode the
    /// teletext page is rendered on transparent background
    /// </summary>
    bool TransparentMode { get;set;}
    #endregion
  }
}
