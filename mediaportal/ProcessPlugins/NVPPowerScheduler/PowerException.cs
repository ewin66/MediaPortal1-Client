/* 
 *	Copyright (C) 2005 Team MediaPortal
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

namespace MediaPortal.PowerScheduler
{
	/// <summary>
	/// Reports error from calls to the Windows Power Management API.
	/// </summary>
	public class PowerException : Exception
	{
		/// <summary>
		/// Create a new instance of the exception.
		/// </summary>
		/// <param name="sReason">Describes the management call which failed.</param>
		public PowerException(string sReason) : base(sReason)
		{
		}
	}
}
