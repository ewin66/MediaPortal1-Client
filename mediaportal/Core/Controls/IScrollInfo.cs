#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

#endregion

using MediaPortal.Drawing;

namespace MediaPortal.Controls
{
	public interface IScrollInfo
	{
		#region Methods

		void LineDown();
		void LineLeft();
		void LineRight();
		void LineUp();
//		Rect MakeVisible(Visual visual, Rect rectangle);
		void MouseWheelDown();
		void MouseWheelLeft();
		void MouseWheelRight();
		void MouseWheelUp();
		void PageDown();
		void PageLeft();
		void PageRight();
		void PageUp();

		// are these document incorrectly?
		void SetHorizontalOffset(double offset);
		void SetVerticalOffset(double offset);

		#endregion Methods

		#region Properties

		bool CanScrollHorizontally
		{
			get;
			set;
		}

		bool CanScrollVertically
		{
			get;
			set;
		}

		double ExtentHeight
		{
			get;
		}

		double ExtentWidth
		{
			get;
		}

		double HorizontalOffset
		{
			get;
		}

		Orientation Orientation
		{
			get;
			set;
		}

//		ScrollViewer ScrollOwner
//		{
//			get;
//			set;
//		}

		double VerticalOffset
		{
			get;
		}

		double ViewportHeight
		{
			get;
		}

		double ViewportWidth
		{
			get;
		}

		#endregion Properties
	}
}
