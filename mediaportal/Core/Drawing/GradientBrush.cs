#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;

namespace MediaPortal.Drawing
{
	public abstract class GradientBrush : Brush
	{
		#region Constructors

		public GradientBrush()
		{
		}

		public GradientBrush(GradientStopCollection gradientStops)
		{
			_gradientStops = gradientStops;
		}

		#endregion Constructors

		#region Methods

		public void AddStop(System.Drawing.Color color, double offset)
		{
			GradientStops.Add(new GradientStop(color, offset));
		}

		#endregion Methods

		#region Properties

//		public ColorInterpolationMode ColorInterpolationMode
//		{
//			get { return _colorInterpolationMode; }
//			set { if(_colorInterpolationMode.Equals(_colorInterpolationMode) == false) { _colorInterpolationMode = value; _isDirty = true; } } 
//		}

		public GradientStopCollection GradientStops
		{
			get { if(_gradientStops == null) _gradientStops = new GradientStopCollection(); return _gradientStops; }
		}

		public GradientSpreadMethod SpreadMethod
		{
			get { return _spreadMethod; }
			set { _spreadMethod = value; }
		}

		#endregion Properties

		#region Fields

//		ColorInterpolationMode				_colorInterpolationMode = ColorInterpolationMode.PhysicallyLinearGamma;
		GradientStopCollection				_gradientStops;
		GradientSpreadMethod				_spreadMethod;

		#endregion Fields
	}
}
