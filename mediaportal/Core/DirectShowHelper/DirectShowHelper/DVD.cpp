/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Frodo
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

#include "stdafx.h"
#include "DVD.h"
#include ".\dvd.h"
#include "resetdvd.h"
#include <comutil.h>
// CDVD


STDMETHODIMP CDVD::Reset(BSTR strPath)
{
	_bstr_t bstrPath(strPath,true);
	CResetDVD* dvd = new CResetDVD((char*)bstrPath);
	delete dvd;
	return S_OK;
}
