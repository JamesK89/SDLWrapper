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
}
