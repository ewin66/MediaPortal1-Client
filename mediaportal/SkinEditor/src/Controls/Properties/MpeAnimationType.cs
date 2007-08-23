#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

using System.Drawing.Design;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Mpe.Controls.Properties
{
  //public enum MpeAnimationType
  //{
  //  None,
  //  FlyInFromLeft,
  //  FlyInFromRight,
  //  FlyInFromTop,
  //  FlyInFromBottom
  //} ;

  public class MpeAnimationType
  {
    public List<MpeAnimationBaseType> Animation;

    public MpeAnimationType()
    {
      Animation = new List<MpeAnimationBaseType>();
      for (int i=0; i < 6; i++)
        Animation.Add(new MpeAnimationBaseType());
    }
  }

  public enum MpeAnimationEfect
  {
    None,
    fade, 
    slide,
    rotate, 
    rotatex,
    rotatey,
    zoom 
  };

  public enum MpeAnimationTween
  {
    None,
    elastic, 
    bounce,
    circle,
    back,
    sine,
    cubic,
    quadratic,
    linear
  }

  public enum MpeAnimationEasing
  {
  None,
  Out,
  inout,
  In
  }

  public class MpeAnimationBaseType
  {
    protected MpeAnimationEfect _efect;
    protected bool _enabled;
    protected int _time;
    protected int _delay;
    protected string _start;
    protected string _end;
    protected int _acceleration;
    protected Point _center;
    protected string _condition;
    protected bool _reversible;
    protected bool _pulse;
    protected MpeAnimationTween _tween;
    protected MpeAnimationEasing _easing;
    
    public MpeAnimationBaseType()
    {
      Enabled = false;
    }
    public MpeAnimationEfect Efect
    {
      get { return _efect; }
      set { _efect = value; }
    }
    
    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }

    [Description("Specifies the length of time that the animation will run, in milliseconds")]  
    public int Time
    {
      get { return _time; }
      set { _time = value; }
    }

    [Description("The time to delay the transistion before starting it, in milliseconds")]
    public int Delay
    {
      get { return _delay; }
      set { _delay = value; }
    }

    [Description("The start state of the control for this transistion. For fades, this is the opaqueness as a percentage (ie start=\"100\" is fully opaque, start=\"0\" is fully transparent. For slides, this is the relative coordinate offset to start the control at (ie start=\"50,60\" will start the control off at 50 pixels to the right, and 60 pixels below it's normal viewing position. For rotates, this is the starting degree offset from the horizontal. (ie start=\"30\" will start the control off on an angle of 30 degrees from the horizontal). For zooms, this is the starting size as a percentage. (ie start=\"50,60\" will start the control at 50% of it's horizontal size and 60% of it's vertical size)")]
    public string Start
    {
      get { return _start; }
      set { _start = value; }
    }

    [Description("The end state of the control for this transistion. Similar to the start state, except that the end state is always kept after the animation is finished, and until the control changes its state.")]
    public string End
    {
      get { return _end; }
      set { _end = value; }
    }

    [Description("Amount to accelerate or decelerate during a �slide�, �zoom� or �rotate� transistion. For deceleration, use a negative value. A value of -1 will cause the control to come to rest at its end coordinates. Defaults to 0")]
    public int Acceleration
    {
      get { return _acceleration; }
      set { _acceleration = value; }
    }
    
    [Description("Center of the rotation or zoom to perform with a �rotate� or �zoom� transistion. This is the coordinates about which the rotation or zoom will take place. eg center=\"30,50� will mean that all points will revolve around (or zoom from) the (30,50) pixel location.")]
    public Point Center
    {
      get { return _center; }
      set { _center = value; }
    }

    [Description("The conditions under which this animation should be performed. Defaults to being always performed. See here for a list of valid conditionals")]
    public string Condition
    {
      get { return _condition; }
      set { _condition = value; }
    }

    [Description("If �false� the animation is not reversed if it is interrupted when it is finished. For instance a Visible animation will normally be reversed (instead of running the Hidden animation) if the control becomes hidden before the visible animation has finished. Setting reversible=\"false� prevents this behaviour (the Hidden animation will take its place). Defaults to true")]
    public bool Reversible
    {
      get { return _reversible; }
      set { _reversible = value; }
    }

    [Description("If �true� will make your fade animation loop")]
    public bool Pulse
    {
      get { return _pulse; }
      set { _pulse = value; }
    }

    [Description("Tween is like an advanced acceleration attribute that can be applied to all animations. Instead of a steady acceleration or deceleration, you can specify curves that the animation should follow")]
    public MpeAnimationTween Tween
    {
      get { return _tween; }
      set { _tween = value; }
    }

    [Description("Easing basically defines the direction of the tween and can be one of �out�, �inout� and �in�. The default value is �out�")]
    public MpeAnimationEasing Easing
    {
      get { return _easing; }
      set { _easing = value; }
    }
  }
}