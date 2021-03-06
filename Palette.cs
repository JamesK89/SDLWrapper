﻿using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	public class PaletteColorEnumerator 
		: IEnumerator<Color>
	{
		internal PaletteColorEnumerator(Palette palette)
		{
			Palette = palette;
			Index = -1;
		}

		private int Index
		{
			get;
			set;
		}

		private Palette Palette
		{
			get;
			set;
		}

		public Color Current => 
			Palette[Index];

		object IEnumerator.Current => 
			Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return (++Index < Palette.Count);
		}

		public void Reset()
		{
			Index = -1;
		}
	}

	public class Palette 
		: IEnumerable<Color>, IDisposable
	{
#if SAFE_AS_POSSIBLE
		SDL_Palette _palette;
		SDL_Color[] _colors;
#endif

		internal Palette(IntPtr handle, bool owner)
		{
			Initializers.InitializeVideo();

			Handle = handle;
			IsOwner = owner;

#if SAFE_AS_POSSIBLE
			_palette = Marshal.PtrToStructure<SDL_Palette>(handle);
			_colors = GetColors();
#endif
		}

		private bool IsOwner
		{
			get;
			set;
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		public int Count
		{
			get
			{
#if !SAFE_AS_POSSIBLE
				unsafe
				{
					return ((SDL_Palette*)Handle.ToPointer())->ncolors;
				}
#else
					return _palette.ncolors;
#endif
			}
		}

		public Color this[int index]
		{
			get
			{
#if !SAFE_AS_POSSIBLE
				unsafe
				{
					SDL_Palette* palette = 
						(SDL_Palette*)Handle.ToPointer();
					SDL_Color* colors = 
						(SDL_Color*)palette->colors.ToPointer();

					return colors[index].ToColor();
				}
#else
				return _colors[index].ToColor();
#endif
			}
			set
			{
				if (SDL_SetPaletteColors(
					 Handle,
					 new SDL_Color[] { value.ToSDLColor() },
					 index, 1) != 0)
				{
					throw new SDLException();
				}
			}
		}

		private SDL_Color[] GetColors()
		{
			List<SDL_Color> result = new List<SDL_Color>();

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				int numColors = ((SDL_Palette*)Handle.ToPointer())->ncolors;

				SDL_Color* pColor = 
					((SDL_Color*)((SDL_Palette*)Handle.ToPointer())->
					colors.ToPointer());

				for (int i = 0; i < numColors; i++)
				{
					result.Add(pColor[i]);
				}
			}
#else
			IntPtr pColors = _palette.colors;
			
			for (int i = 0; i < _palette.ncolors; i++)
			{
				result.Add(Marshal.PtrToStructure<SDL_Color>(pColors));

				pColors = IntPtr.Add(pColors, 
					Marshal.SizeOf(typeof(SDL_Color)));
			}
#endif

			return result.ToArray();
		}

		private void SetColors(IEnumerable<SDL_Color> colors)
		{
			SDL_Color[] cols = colors.ToArray();
			if (SDL_SetPaletteColors(Handle, cols, 0, cols.Length) != 0)
			{
				throw new SDLException();
			}
		}

		public static implicit operator IntPtr(Palette palette)
		{
			return palette.Handle;
		}

		public static implicit operator bool(Palette palette)
		{
			return (palette != null && palette.Handle != IntPtr.Zero);
		}

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (Handle != IntPtr.Zero)
				{
					if (IsOwner)
					{
						SDL_FreePalette(Handle);
					}

					Handle = IntPtr.Zero;
				}

				disposedValue = true;
			}
		}

		~Palette()
		{
		   Dispose(false);
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
#endregion

		public IEnumerator<Color> GetEnumerator()
		{
			return new PaletteColorEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}
	}
}
