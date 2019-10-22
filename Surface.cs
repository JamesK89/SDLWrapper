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
	public class Surface : IDisposable, ICloneable
	{
#if SAFE_AS_POSSIBLE
		private SDL_Surface _surface;
		private SDL_PixelFormat _format;
#endif

		private Palette _palette;

		internal Surface(IntPtr handle, bool owner)
		{
			Initializers.InitializeVideo();

			Handle = handle;
			
			IsOwner = owner;

			Initialize();
		}

		public Surface(Size size, int bpp, PixelFormat format)
		{
			Initializers.InitializeVideo();

			Handle = SDL_CreateRGBSurfaceWithFormat(
				0,
				size.Width, size.Height,
				bpp,
				format.ToSDL());

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			IsOwner = true;

			Initialize();
		}

		public Surface(Size size, PixelFormat format)
		{
			Initializers.InitializeVideo();

			SDL_PixelFormatEnumToMasks(
				format.ToSDL(),
				out int bpp,
				out uint rmask,
				out uint gmask,
				out uint bmask,
				out uint amask);

			Handle = SDL_CreateRGBSurfaceWithFormat(
				0,
				size.Width, size.Height,
				bpp,
				format.ToSDL());

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}
			
			IsOwner = true;

			Initialize();
		}

		public Surface(string fileName)
		{
			Initializers.InitializeVideo();

			Handle = SDL_LoadBMP(fileName);

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			IsOwner = true;

			Initialize();
		}

		private void Initialize()
		{
			_palette = null;
			
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_Surface* pSurface = 
					(SDL_Surface*)Handle.ToPointer();
				SDL_PixelFormat* pFormat = 
					(SDL_PixelFormat*)pSurface->format;

				if (pFormat->palette != IntPtr.Zero)
				{
					_palette = new Palette(
						pFormat->palette,
						false);
				}
			}
#else
			_surface = 
				Marshal.PtrToStructure<SDL_Surface>(Handle);
			_format = 
				Marshal.PtrToStructure<SDL_PixelFormat>(_surface.format);

			if (_format.palette != IntPtr.Zero)
			{
				_palette = new Palette(_format.palette, false);
			}
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

		public Palette Palette
		{
			get
			{
				return _palette;
			}
			set
			{
				if (SDL_SetSurfacePalette(Handle, value.Handle) != 0)
				{
					throw new SDLException();
				}
			}
		}

		public Color ColorKey
		{
			get
			{
				if (SDL_GetColorKey(Handle, out uint pixel) != 0)
				{
					throw new SDLException();
				}

				return pixel.ToColorFromSDL(Handle);
			}
			set
			{
				if (SDL_SetColorKey(Handle, 1, value.ToSDL(Handle)) != 0)
				{
					throw new SDLException();
				}
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

				if (SDL_SetColorKey(Handle, value ? 1 : 0, key) != 0)
				{
					throw new SDLException();
				}
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

		public IntPtr Pixels
		{
			get
			{
#if !SAFE_AS_POSSIBLE
				unsafe
				{
					return ((SDL_Surface*)Handle.ToPointer())->pixels;
				}
#else
				return _surface.pixels;
#endif
			}
		}

		public byte[] Data
		{
			get
			{
				GetPixels(out byte[] result);
				return result;
			}
			set
			{
				SetPixels(value);
			}
		}

		public PixelFormat Format
		{
			get
			{
#if !SAFE_AS_POSSIBLE
				unsafe
				{
					SDL_Surface* pSurface =
						(SDL_Surface*)Handle.ToPointer();

					SDL_PixelFormat* pFormat =
						(SDL_PixelFormat*)pSurface->format.ToPointer();

					return pFormat->format.ToPixelFormat();
				}
#else
				return _format.format.ToPixelFormat();
#endif
			}
		}

		public static Surface FromBitmap(string fileName)
		{
			return new Surface(fileName);
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
			if (SDL_LockSurface(Handle) != 0)
			{
				throw new SDLException();
			}
		}

		public void Unlock()
		{
			SDL_UnlockSurface(Handle);
		}

		public void SaveBitmap(string fileName)
		{
			if (SDL_SaveBMP(Handle, fileName) != 0)
			{
				throw new SDLException();
			}
		}

		public void FillRectangle(Rectangle rect, Color color)
		{
			SDL_Rect r = rect.ToSDL();

			if (SDL_FillRect(Handle, ref r, color.ToSDL(Handle)) != 0)
			{
				throw new SDLException();
			}
		}

		public void FillRectangles(IEnumerable<Rectangle> rects, Color color)
		{
			SDL_Rect[] nativeRects = rects.Select(o => o.ToSDL()).ToArray();

			if (SDL_FillRects(
				 Handle,
				 nativeRects, nativeRects.Length,
				 color.ToSDL(Handle)) != 0)
			{
				throw new SDLException();
			}
		}

		public void Blit(
			Rectangle sourceRect,
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect s = sourceRect.ToSDL();
			SDL_Rect d = destinationRect.ToSDL();

			if (SDL_BlitSurface(
				 Handle,
				 ref s,
				 destination.Handle,
				 ref d) != 0)
			{
				throw new SDLException();
			}
		}
		
		public void Blit(
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect d = destinationRect.ToSDL();

			if (SDL_BlitSurface(
				 Handle,
				 IntPtr.Zero,
				 destination.Handle,
				 ref d) != 0)
			{
				throw new SDLException();
			}
		}

		public void Blit(
			Rectangle sourceRect,
			Surface destination)
		{
			SDL_Rect s = sourceRect.ToSDL();

			if (SDL_BlitSurface(
				 Handle,
				 ref s,
				 destination.Handle,
				 IntPtr.Zero) != 0)
			{
				throw new SDLException();
			}
		}
		
		public void Blit(
			Surface destination)
		{
			if (SDL_BlitSurface(
				 Handle,
				 IntPtr.Zero,
				 destination.Handle,
				 IntPtr.Zero) != 0)
			{
				throw new SDLException();
			}
		}
		
		public void BlitScaled(
			Rectangle sourceRect,
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect s = sourceRect.ToSDL();
			SDL_Rect d = destinationRect.ToSDL();

			if (SDL_BlitScaled(
				 Handle,
				 ref s,
				 destination.Handle,
				 ref d) != 0)
			{
				throw new SDLException();
			}
		}

		public void BlitScaled (
			Surface destination,
			Rectangle destinationRect)
		{
			SDL_Rect d = destinationRect.ToSDL();

			if (SDL_BlitScaled(
				 Handle,
				 IntPtr.Zero,
				 destination.Handle,
				 ref d) != 0)
			{
				throw new SDLException();
			}
		}

		public void BlitScaled(
			Rectangle sourceRect,
			Surface destination)
		{
			SDL_Rect s = sourceRect.ToSDL();

			if (SDL_BlitScaled(
				 Handle,
				 ref s,
				 destination.Handle,
				 IntPtr.Zero) != 0)
			{
				throw new SDLException();
			}
		}
		
		public void BlitScaled(
			Surface destination)
		{
			if (SDL_BlitScaled(
				 Handle,
				 IntPtr.Zero,
				 destination.Handle,
				 IntPtr.Zero) != 0)
			{
				throw new SDLException();
			}
		}

		private static IntPtr Clone(IntPtr handle)
		{
			IntPtr result = IntPtr.Zero;

			IntPtr pixels = IntPtr.Zero;

			uint format = 0;
			int width = 0;
			int height = 0;
			int depth = 0;
			int pitch = 0;

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_Surface* pSurface = 
					(SDL_Surface*)handle.ToPointer();
				SDL_PixelFormat* pFormat = 
					(SDL_PixelFormat*)pSurface->format.ToPointer();

				format = pFormat->format;
				depth = pFormat->BitsPerPixel;
				width = pSurface->w;
				height = pSurface->h;
				pixels = pSurface->pixels;
				pitch = pSurface->pitch;
			}
#else
			SDL_Surface surface =
				Marshal.PtrToStructure<SDL_Surface>(handle);
			SDL_PixelFormat pixelFormat =
				Marshal.PtrToStructure<SDL_PixelFormat>(surface.format);
			
			format = pixelFormat.format;
			pixels = surface.pixels;
			depth = pixelFormat.BitsPerPixel;
			pitch = surface.pitch;
			width = surface.w;
			height = surface.h;
#endif

			result = SDL_CreateRGBSurfaceWithFormatFrom(
				pixels,
				width,
				height,
				depth,
				pitch,
				format);

			if (result == IntPtr.Zero)
			{
				throw new SDLException();
			}

			return result;
		}

		public Surface Clone()
		{
			Surface result = new Surface(Clone(Handle), true);

			return result;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (Handle != IntPtr.Zero && IsOwner)
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
