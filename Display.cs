using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;
using System.Collections;

namespace SDLWrapper
{
	public class Display
	{
		public class Mode
		{
			internal Mode(SDL_DisplayMode mode)
			{
				SDLMode = mode;
			}

			internal SDL_DisplayMode SDLMode
			{
				get;
				set;
			}

			public int RefreshRate
			{
				get
				{
					return SDLMode.refresh_rate;
				}
			}

			public Size Resolution
			{
				get
				{
					return new Size(SDLMode.w, SDLMode.h);
				}
			}

			public PixelFormat Format
			{
				get
				{
					return SDLMode.format.ToPixelFormat();
				}
			}

			public int BitsPerPixel
			{
				get
				{
					if (SDL_PixelFormatEnumToMasks(
							SDLMode.format,
							out int bpp,
							out uint r,
							out uint g,
							out uint b,
							out uint a) != SDL_bool.SDL_TRUE)
					{
						throw new SDLException();
					}

					return bpp;
				}
			}

			public override string ToString()
			{
				return $"{Resolution.Width}x{Resolution.Height} " +
					   $"{BitsPerPixel}Bpp @ {RefreshRate}Hz";
			}
		}

		public class ModeCollectionEnumerator 
			: IEnumerator<Mode>
		{
			private Mode[] Modes
			{
				get;
				set;
			}

			private int Index
			{
				get;
				set;
			}

			internal ModeCollectionEnumerator(Mode[] modes)
			{
				Modes = modes;
				Index = -1;
			}

			public Mode Current => 
				Modes[Index];

			object IEnumerator.Current =>
				this.Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return (++Index < Modes.Length);
			}

			public void Reset()
			{
				Index = -1;
			}
		}

		public class ModeCollection 
			: IEnumerable<Mode>
		{
			private Mode[] Modes
			{
				get;
				set;
			}

			public int Count => 
				Modes.Length;

			public Mode this[int index] =>
				Modes[index];

			internal ModeCollection(Mode[] modes)
			{
				Modes = modes;
			}

			public IEnumerator<Mode> GetEnumerator()
			{
				return new ModeCollectionEnumerator(Modes);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}

		private Display(int index)
		{
			Index = index;

			int numModes = 0;

			if ((numModes = SDL_GetNumDisplayModes(Index)) < 1)
			{
				throw new SDLException();
			}

			List<Mode> modes = new List<Mode>();

			for (int i = 0; i < numModes; i++)
			{
				if (SDL_GetDisplayMode(
					 Index,
					 i,
					 out SDL_DisplayMode currentMode) != 0)
				{
					throw new SDLException();
				}

				modes.Add(new Mode(currentMode));
			}

			Modes = new ModeCollection(modes.ToArray());
		}

		private static SDL_DisplayMode GetCurrentMode(int index)
		{
			if (SDL_GetCurrentDisplayMode(
					index,
					out SDL_DisplayMode mode) != 0)
			{
				throw new SDLException();
			}

			return mode;
		}

		private static SDL_DisplayMode GetDefaultDisplayMode(int index)
		{
			if (SDL_GetDesktopDisplayMode(
					index,
					out SDL_DisplayMode mode) != 0)
			{
				throw new SDLException();
			}

			return mode;
		}

		public static int Count
		{
			get
			{
				int n = 0;

				if ((n = SDL_GetNumVideoDisplays()) < 1)
				{
					throw new SDLException();
				}

				return n;
			}
		}

		public string Name
		{
			get
			{
				return SDL_GetDisplayName(Index);
			}
		}

		public int Index
		{
			get;
			private set;
		}

		public Rectangle Bounds
		{
			get
			{
				if (SDL_GetDisplayBounds(Index, out SDL_Rect r) != 0)
				{
					throw new SDLException();
				}

				return r.ToRectangle();
			}
		}

		public Rectangle UsableBounds
		{
			get
			{
				if (SDL_GetDisplayUsableBounds(Index, out SDL_Rect r) != 0)
				{
					throw new SDLException();
				}

				return r.ToRectangle();
			}
		}

		public Mode CurrentMode
		{
			get
			{
				return new Mode(GetCurrentMode(Index));
			}
		}

		public Mode DefaultMode
		{
			get
			{
				return new Mode(GetDefaultDisplayMode(Index));
			}
		}

		public Mode GetClosestMode(
			Size resolution,
			int refreshRate = 0,
			PixelFormat format = PixelFormat.Unknown)
		{
			SDL_DisplayMode desired = new SDL_DisplayMode();

			desired.w = resolution.Width;
			desired.h = resolution.Height;
			desired.refresh_rate = refreshRate;
			desired.format = format.ToSDL();

			if (SDL_GetClosestDisplayMode(
				 Index,
				 ref desired,
				 out SDL_DisplayMode closest) == IntPtr.Zero)
			{
				throw new SDLException();
			}

			return new Mode(closest);
		}

		public ModeCollection Modes
		{
			get;
			private set;
		}

		public static Display Get(int index)
		{
			return new Display(index);
		}
	}
}
