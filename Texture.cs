using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	public class Texture : IDisposable
	{
		private WeakReference<Renderer> _renderer;

		internal Texture(IntPtr handle, Renderer renderer)
		{
			Handle = handle;

			SDL_QueryTexture(Handle,
				out uint format,
				out  int access,
				out  int width,
				out  int height);

			Size = new Size(
				width, height);

			Renderer = renderer;
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		public Size Size
		{
			get;
			private set;
		}

		public Renderer Renderer
		{
			get
			{
				_renderer.TryGetTarget(out Renderer result);
				return result;
			}
			private set
			{
				_renderer = new WeakReference<Renderer>(value);
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (Handle != IntPtr.Zero)
				{
					Renderer.FreeTexture(Handle);
					SDL_DestroyTexture(Handle);
					Handle = IntPtr.Zero;
				}

				disposedValue = true;
			}
		}
		
		~Texture()
		{
		   Dispose(false);
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
