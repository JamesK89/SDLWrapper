using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	public class TextInputEventArgs : EventArgs
	{
		public string Text
		{
			get;
			set;
		}

		public int Start
		{
			get;
			set;
		}

		public int Length
		{
			get;
			set;
		}
	}

	public class MouseEventArgs : EventArgs
	{
		public Point Position
		{
			get;
			set;
		}

		public Point Delta
		{
			get;
			set;
		}

		public MouseButton Button
		{
			get;
			set;
		}

		public Point Wheel
		{
			get;
			set;
		}

		public bool Double
		{
			get;
			set;
		}
	}

	public class KeyboardEventArgs : EventArgs
	{
		public Key Key
		{
			get;
			set;
		}

		public KeyModifier Modifier
		{
			get;
			set;
		}
	}

	public class TimerEventArgs : EventArgs
	{
		public uint Interval
		{
			get;
			set;
		}
	}
	
	public class RWopCloseEventArgs : EventArgs
	{
		public IntPtr Context
		{
			get;
			set;
		} = IntPtr.Zero;
		
		public RWopEventOverrideMode Override
		{
			get;
			set;
		} = RWopEventOverrideMode.Continue;

		public int Result
		{
			get;
			set;
		} = 0;
	}
	
	public class RWopSeekEventArgs : EventArgs
	{
		public IntPtr Context
		{
			get;
			set;
		} = IntPtr.Zero;

		public RWopEventOverrideMode Override
		{
			get;
			set;
		} = RWopEventOverrideMode.Continue;

		public long Result
		{
			get;
			set;
		} = 0;

		public long Offset
		{
			get;
			set;
		} = 0;

		public System.IO.SeekOrigin Origin
		{
			get;
			set;
		} = System.IO.SeekOrigin.Begin;
	}
	
	public class RWopSizeEventArgs : EventArgs
	{
		public IntPtr Context
		{
			get;
			set;
		} = IntPtr.Zero;
		
		public RWopEventOverrideMode Override
		{
			get;
			set;
		} = RWopEventOverrideMode.Continue;

		public long Result
		{
			get;
			set;
		} = 0;
	}

	public class RWopReadEventArgs : EventArgs
	{
		public IntPtr Context
		{
			get;
			set;
		} = IntPtr.Zero;
		
		public RWopEventOverrideMode Override
		{
			get;
			set;
		} = RWopEventOverrideMode.Continue;

		public uint Result
		{
			get;
			set;
		} = 0;

		public IntPtr Data
		{
			get;
			set;
		} = IntPtr.Zero;

		public uint Size
		{
			get;
			set;
		} = 0;

		public uint Count
		{
			get;
			set;
		} = 0;
	}
	
	public class RWopWriteEventArgs : EventArgs
	{
		public IntPtr Context
		{
			get;
			set;
		} = IntPtr.Zero;
		
		public RWopEventOverrideMode Override
		{
			get;
			set;
		} = RWopEventOverrideMode.Continue;

		public uint Result
		{
			get;
			set;
		} = 0;

		public IntPtr Data
		{
			get;
			set;
		} = IntPtr.Zero;

		public uint Size
		{
			get;
			set;
		} = 0;

		public uint Count
		{
			get;
			set;
		} = 0;
	}
}
