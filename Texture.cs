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
	public class TextureLockData
	{
		public TextureLockData(
			Texture texture,
			IntPtr pixels,
			int pitch,
			int length,
			byte[] data)
		{
			Pixels = pixels;
			Pitch = pitch;
			Data = data;
			Copied = (data != null);
		}

		public byte[] Data
		{
			get;
			private set;
		}

		public IntPtr Pixels
		{
			get;
			private set;
		}

		public int Pitch
		{
			get;
			private set;
		}

		public int Length
		{
			get;
			private set;
		}

		public Texture Texture
		{
			get;
			private set;
		}

		public bool Copied
		{
			get;
			private set;
		}
	}

	public class Texture : IDisposable
	{
		private WeakReference<Renderer> _renderer;

		private SDL_TextureAccess _access;

		internal Texture(IntPtr handle, Renderer renderer)
		{
			Initializers.InitializeVideo();

			Handle = handle;

			if (SDL_QueryTexture(
				Handle,
				out uint format,
				out int access,
				out int width,
				out int height) != 0)
			{
				throw new SDLException();
			}

			Size = new Size(
				width, height);

			_access = (SDL_TextureAccess)access;

			Format = format.ToPixelFormat();

			Renderer = renderer;

			LockData = null;
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

		public PixelFormat Format
		{
			get;
			private set;
		}

		public TextureLockData LockData
		{
			get;
			private set;
		}

		public bool IsStreaming
		{
			get
			{
				return
					(_access & SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING)
					== SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING;
			}
		}

		public bool IsRenderTarget
		{
			get
			{
				return
					(_access & SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET)
					== SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET;
			}
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

		public TextureLockData Lock(bool copy = false)
		{
			if (LockData != null)
			{
				throw new InvalidOperationException(
					"Texture is already locked");
			}

			if (SDL_LockTexture(Handle,
				IntPtr.Zero, out IntPtr pixels, out int pitch) != 0)
			{
				throw new SDLException();
			}

			byte[] data = null;
			int length = pitch * Size.Height;

			if (copy)
			{
				data = new byte[length];
				Marshal.Copy(pixels, data, 0, data.Length);
			}

			LockData = new TextureLockData(
				this, pixels, pitch, length, data);
			
			return LockData;
		}

		public TextureLockData Lock(Rectangle rect, bool copy = false)
		{
			if (LockData != null)
			{
				throw new InvalidOperationException(
					"Texture is already locked");
			}

			SDL_Rect r = rect.ToSDLRect();

			if (SDL_LockTexture(Handle,
				ref r, out IntPtr pixels, out int pitch) != 0)
			{
				throw new SDLException();
			}

			byte[] data = null;
			int length = pitch * rect.Height;

			if (copy)
			{
				data = new byte[length];
				Marshal.Copy(pixels, data, 0, data.Length);
			}

			LockData = new TextureLockData(
				this, pixels, pitch, length, data);
			
			return LockData;
		}
		
		public void Unlock()
		{
			if (LockData == null)
			{
				throw new InvalidOperationException(
					"Texture is not locked");
			}

			if (LockData.Copied && LockData.Data != null)
			{
				Marshal.Copy(
					LockData.Data,
					0,
					LockData.Pixels,
					LockData.Length);
			}

			SDL_UnlockTexture(Handle);
			LockData = null;
		}

		public void Update(byte[] pixels, int pitch)
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				fixed (byte* p = &pixels[0])
				{
					if (SDL_UpdateTexture(
						Handle,
						IntPtr.Zero,
						new IntPtr(p),
						pitch) != 0)
					{
						throw new SDLException();
					}
				}
			}
#else
			IntPtr p = Marshal.AllocHGlobal(pitch);

			try
			{
				Marshal.Copy(pixels, 0, p, pitch);
				if (SDL_UpdateTexture(Handle, IntPtr.Zero, p, pitch) != 0)
				{
					throw new SDLException();
				}
			}
			finally
			{
				if (p != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(p);
				}
			}
#endif
		}

		public void Update(Rectangle rect, byte[] pixels, int pitch)
		{
			SDL_Rect r = rect.ToSDLRect();

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				fixed (byte* p = &pixels[0])
				{
					if (SDL_UpdateTexture(
						Handle,
						ref r,
						new IntPtr(p),
						pitch) != 0)
					{
						throw new SDLException();
					}
				}
			}
#else
			IntPtr p = Marshal.AllocHGlobal(pitch);

			try
			{
				Marshal.Copy(pixels, 0, p, pitch);
				if (SDL_UpdateTexture(Handle, ref r, p, pitch) != 0)
				{
					throw new SDLException();
				}
			}
			finally
			{
				if (p != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(p);
				}
			}
#endif
		}
		
		public void Update(IntPtr pixels, int pitch)
		{
			if (SDL_UpdateTexture(Handle, IntPtr.Zero, pixels, pitch) != 0)
			{
				throw new SDLException();
			}
		}

		public void Update(Rectangle rect, IntPtr pixels, int pitch)
		{
			SDL_Rect r = rect.ToSDLRect();
			if (SDL_UpdateTexture(Handle, ref r, pixels, pitch) != 0)
			{
				throw new SDLException();
			}
		}

		public static implicit operator IntPtr(Texture texture)
		{
			return texture.Handle;
		}

		public static implicit operator bool(Texture texture)
		{
			return (texture != null && texture.Handle != IntPtr.Zero);
		}

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (Handle != IntPtr.Zero)
				{
					if (LockData != null)
					{
						SDL_UnlockTexture(Handle);
					}

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
