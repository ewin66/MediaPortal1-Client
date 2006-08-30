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
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
  Guid("59f8d617-92fd-48d5-8f6d-a97bfd95c448"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsVideoAnalyzer
  {
    [PreserveSig]
    int SetVideoPid(short videoPid);

    [PreserveSig]
    int GetVideoPid(out short videoPid);

    [PreserveSig]
    int SetAudioPid(short audioPid);

    [PreserveSig]
    int GetAudioPid(out short audioPid);

    [PreserveSig]
    int IsVideoEncrypted(out short yesNo);

    [PreserveSig]
    int IsAudioEncrypted(out short yesNo);

    [PreserveSig]
    int Reset();
  }
}
