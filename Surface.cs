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
	public class Surface : IDisposable
	{
#if SAFE_AS_POSSIBLE
		private SDL_Surface _surface;
		private SDL_PixelFormat _format;
#endif

		internal Surface(IntPtr handle)
		{
			Handle = handle;

#if SAFE_AS_POSSIBLE
			_surface = 
				Marshal.PtrToStructure<SDL_Surface>(Handle);
			_format = 
				Marshal.PtrToStructure<SDL_PixelFormat>(_surface.format);
#endif
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		public Color ColorKey
		{
			get
			{
				SDL_GetColorKey(Handle, out uint pixel);
				return pixel.ToColorFromSDL(Handle);
			}
			set
			{
				SDL_SetColorKey(Handle, 1, value.ToSDL(Handle));
			}
		}

		public bool IsColorKeyEnabled
		{
			get
			{
				return SDL_GetColorKey(Handle, out uint key) != -1;
			}
			set
			{
				SDL_GetColorKey(Handle, out uint key);
				SDL_SetColorKey(Handle, value ? 1 : 0, key);
			}
		}

		public Rectangle ClippingRectangle
		{
			get
			{
				SDL_GetClipRect(Handle, out SDL_Rect r);
				return r.ToDrawing();
			}
			set
			{
				SDL_Rect r = value.ToSDL();
				SDL_SetClipRect(Handle, ref r);
			}
		}

		public Size Size
		{
			get
			{
#if !SAFE_AS_POSSIBLE
				unsafe
				{
					SDL_Surface* pSurface = 
						(SDL_Surface*)Handle.ToPointer();

					return new Size(
						pSurface->w,
						pSurface->h);
				}
#else
				return new Size(
					_surface.w,
					_surface.h);
#endif
			}
		}

		public int Pitch
		{
			get
			{
#if !SAFE_AS_POSSIBLE
				unsafe
				{
					return ((SDL_Surface*)Handle.ToPointer())->pitch;
				}
#else
				return _surface.pitch;
#endif
			}
		}

		public static Surface FromBitmap(string fileName)
		{
			return new Surface(SDL_LoadBMP(fileName));
		}

		public void GetPixels(out byte[] pixels)
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_Surface* pSurface = 
					(SDL_Surface*)Handle.ToPointer();

				pixels = new byte[pSurface->pitch * pSurface->h];
				Marshal.Copy(pSurface->pixels, pixels, 0, pixels.Length);
			}
#else
			pixels = new byte[_surface.pitch * _surface.h];
			Marshal.Copy(
				_surface.pixels,
				pixels,
				0,
				pixels.Length);
#endif
		}

		public void GetPixels(out IntPtr pixels)
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_Surface* pSurface = 
					(SDL_Surface*)Handle.ToPointer();

				pixels = pSurface->pixels;
			}
#else
			pixels = _surface.pixels;
#endif
		}

#if !SAFE_AS_POSSIBLE
		public unsafe byte* GetPixels()
		{
			unsafe
			{
				SDL_Surface* pSurface = 
					(SDL_Surface*)Handle.ToPointer();

				return (byte*)pSurface->pixels.ToPointer();
			}
		}
#endif

		public void SetPixels(byte[] pixels)
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_Surface* pSurface =
					(SDL_Surface*)Handle.ToPointer();

				Marshal.Copy(
					pixels,
					0,
					pSurface->pixels,
					pSurface->pitch * pSurface->h);
			}
#else
			Marshal.Copy(
				pixels, 
				0,
				_surface.pixels,
				_surface.pitch * _surface.h);
#endif
		}

		public void SetPixels(IntPtr pixels)
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_Surface* pSurface =
					(SDL_Surface*)Handle.ToPointer();
				
				long length = pSurface->pitch * pSurface->h;
				
				Buffer.MemoryCopy(
					pixels.ToPointer(),
					pSurface->pixels.ToPointer(),
					length,
					length);
			}
#else
			throw new NotImplementedException();
#endif
		}

#if !SAFE_AS_POSSIBLE
		public unsafe void SetPixels(byte* pixels)
		{
			unsafe
			{
				SDL_Surface* pSurface =
					(SDL_Surface*)Handle.ToPointer();

				long length = pSurface->pitch * pSurface->h;

				Buffer.MemoryCopy(
					(void*)pixels,
					pSurface->pixels.ToPointer(),
					length,
					length);
			}
		}
#endif

		public void Lock()
		{
			SDL_LockSurface(Handle);
		}

		public void Unlock()
		{
			SDL_UnlockSurface(Handle);
		}

		public void SaveBitmap(string fileName)
		{
			SDL_SaveBMP(Handle, fileName);
		}

		public void FillRectangle(Rectangle rect, Color color)
		{
			SDL_Rect r = rect.ToSDL();
			SDL_FillRect(Handle, ref r, color.ToSDL(Handle));
		}

		public void FillRectangles(IEnumerable<Rectangle> rects, Color color)
		{
			SDL_Rect[] nativeRects = rects.Select(o => o.ToSDL()).ToArray();
			SDL_FillRects(
				Handle,
				nativeRects, nativeRects.Length,
				color.ToSDL(Handle));
		}

		public void Blit(
			Rectangle sourceRect,
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect s = sourceRect.ToSDL();
			SDL_Rect d = destinationRect.ToSDL();

			SDL_BlitSurface(
				Handle,
				ref s,
				destination.Handle,
				ref d);
		}
		
		public void Blit(
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect d = destinationRect.ToSDL();
			
			SDL_BlitSurface(
				Handle,
				IntPtr.Zero,
				destination.Handle,
				ref d);
		}

		public void Blit(
			Rectangle sourceRect,
			Surface destination)
		{
			SDL_Rect s = sourceRect.ToSDL();

			SDL_BlitSurface(
				Handle,
				ref s,
				destination.Handle,
				IntPtr.Zero);
		}
		
		public void Blit(
			Surface destination)
		{
			SDL_BlitSurface(
				Handle,
				IntPtr.Zero,
				destination.Handle,
				IntPtr.Zero);
		}
		
		public void BlitScaled(
			Rectangle sourceRect,
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect s = sourceRect.ToSDL();
			SDL_Rect d = destinationRect.ToSDL();

			SDL_BlitScaled(
				Handle,
				ref s,
				destination.Handle,
				ref d);
		}

		public void BlitScaled (
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect d = destinationRect.ToSDL();
			
			SDL_BlitScaled(
				Handle,
				IntPtr.Zero,
				destination.Handle,
				ref d);
		}

		public void BlitScaled(
			Rectangle sourceRect,
			Surface destination)
		{
			SDL_Rect s = sourceRect.ToSDL();

			SDL_BlitScaled(
				Handle,
				ref s,
				destination.Handle,
				IntPtr.Zero);
		}
		
		public void BlitScaled(
			Surface destination)
		{
			SDL_BlitScaled(
				Handle,
				IntPtr.Zero,
				destination.Handle,
				IntPtr.Zero);
		}

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (Handle != IntPtr.Zero)
				{
					SDL_FreeSurface(Handle);
					Handle = IntPtr.Zero;
				}

				disposedValue = true;
			}
		}
		
		 ~Surface()
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
