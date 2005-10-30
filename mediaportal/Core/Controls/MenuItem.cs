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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Automation;

namespace MediaPortal.Controls
{
	public class MenuItem : HeaderedItemsControl, /* IAutomationPatternProvider, IExpandCollapseProvider, */ IInvokeProvider //, IToggleProvider
	{
		#region Constructors

		public MenuItem()
		{
		}

		#endregion Constructors

		#region Methods

		void IInvokeProvider.Invoke()
		{
			// async
		}

		#endregion Methods

		#region Properties

		[BindableAttribute(true)] 
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		[BindableAttribute(true)] 
		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(MenuItem)); 
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(MenuItem));

		#endregion Properties (Dependency)
	}
}
