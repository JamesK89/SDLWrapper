﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	public class Renderer : IDisposable
	{
		private static Dictionary<IntPtr, WeakReference<Renderer>> _renderers
			= new Dictionary<IntPtr, WeakReference<Renderer>>();

		private Dictionary<IntPtr, WeakReference<Texture>> _textures;

		internal Renderer(IntPtr handle)
		{
			Handle = handle;

			_textures = new Dictionary<IntPtr, WeakReference<Texture>>();

			if (!_renderers.ContainsKey(handle))
			{
				_renderers.Add(handle, new WeakReference<Renderer>(this));
			}
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		public Texture Target
		{
			get
			{
				return FetchTexture(SDL_GetRenderTarget(Handle));
			}
			set
			{
				SDL_SetRenderTarget(Handle, value?.Handle ?? IntPtr.Zero);
			}
		}
		
		public Size LogicalSize
		{
			get
			{
				SDL_RenderGetLogicalSize(Handle, out int w, out int h);
				return new Size(w, h);
			}
			set
			{
				SDL_RenderSetLogicalSize(Handle, value.Width, value.Height);
			}
		}

		public SizeF Scale
		{
			get
			{
				SDL_RenderGetScale(Handle, out float x, out float y);
				return new SizeF(x, y);
			}
			set
			{
				SDL_RenderSetScale(Handle, value.Width, value.Height);
			}
		}

		public bool ClippingEnabled
		{
			get
			{
				return SDL_RenderIsClipEnabled(Handle) == SDL_bool.SDL_TRUE;
			}
		}

		public Rectangle ClippingRectangle
		{
			get
			{
				SDL_RenderGetClipRect(Handle, out SDL_Rect r);
				return new Rectangle(r.x, r.y, r.w, r.h);
			}
			set
			{
				SDL_Rect r = value.ToSDL();
				SDL_RenderSetClipRect(Handle, ref r);
			}
		}

		public Rectangle Viewport
		{
			get
			{
				SDL_RenderGetViewport(Handle, out SDL_Rect r);
				return new Rectangle(r.x, r.y, r.w, r.h);
			}
			set
			{
				SDL_Rect r = value.ToSDL();
				SDL_RenderSetViewport(Handle, ref r);
			}
		}

		public bool HasTargetSupport
		{
			get
			{
				return SDL_RenderTargetSupported(Handle) == SDL_bool.SDL_TRUE;
			}
		}

		public Color Color
		{
			get
			{
				SDL_GetRenderDrawColor(Handle,
					out byte r, out byte g, out byte b, out byte a);

				return Color.FromArgb(a, r, g, b);
			}
			set
			{
				SDL_SetRenderDrawColor(Handle,
					value.R, value.G, value.B, value.A);
			}
		}

		public Texture CreateTextureFromSurface(Surface surface)
		{
			Texture result = null;

			IntPtr ptr = 
				SDL_CreateTextureFromSurface(Handle, surface.Handle);

			if (ptr != IntPtr.Zero)
			{
				result = new Texture(ptr);
				_textures.Add(ptr, new WeakReference<Texture>(result));
			}

			return result;
		}

		public void Clear()
		{
			SDL_RenderClear(Handle);
		}

		public void Present()
		{
			SDL_RenderPresent(Handle);
		}

		public void Point(Point p)
		{
			SDL_RenderDrawPoint(Handle, p.X, p.Y);
		}

		public void Point(IEnumerable<Point> points)
		{
			SDL_Point[] nativePoints = 
				points.Select(o => o.ToSDL()).ToArray();

			SDL_RenderDrawPoints(Handle,
				nativePoints, nativePoints.Length);
		}

		public void Point(PointF p)
		{
			SDL_RenderDrawPointF(Handle, p.X, p.Y);
		}
		
		public void Point(IEnumerable<PointF> points)
		{
			SDL_FPoint[] nativePoints = 
				points.Select(o => o.ToSDL()).ToArray();

			SDL_RenderDrawPointsF(Handle,
				nativePoints, nativePoints.Length);
		}

		public void Line(Point a, Point b)
		{
			SDL_RenderDrawLine(
				Handle,
				a.X, a.Y,
				b.X, b.Y);
		}

		public void Line(IEnumerable<Point> points)
		{
			SDL_Point[] nativePoints = 
				points.Select(o => o.ToSDL()).ToArray();

			SDL_RenderDrawLines(Handle,
				nativePoints, nativePoints.Length);
		}

		public void Line(PointF a, PointF b)
		{
			SDL_RenderDrawLineF(
				Handle,
				a.X, a.Y,
				b.X, b.Y);
		}
		
		public void Line(IEnumerable<PointF> points)
		{
			SDL_FPoint[] nativePoints = 
				points.Select(o => o.ToSDL()).ToArray();

			SDL_RenderDrawLinesF(Handle,
				nativePoints, nativePoints.Length);
		}

		public void Rectangle(Rectangle rect)
		{
			SDL_Rect r = rect.ToSDL();
			SDL_RenderDrawRect(Handle, ref r);
		}

		public void Rectangle(IEnumerable<Rectangle> rects)
		{
			SDL_Rect[] nativeRects =
				rects.Select(o => o.ToSDL()).ToArray();

			SDL_RenderDrawRects(Handle,
				nativeRects, nativeRects.Length);
		}

		public void Rectangle(RectangleF rect)
		{
			SDL_FRect r = rect.ToSDL();
			SDL_RenderDrawRectF(Handle, ref r);
		}

		public void Rectangle(IEnumerable<RectangleF> rects)
		{
			SDL_FRect[] nativeRects =
				rects.Select(o => o.ToSDL()).ToArray();

			SDL_RenderDrawRectsF(Handle,
				nativeRects, nativeRects.Length);
		}

		public void FillRectangle(Rectangle rect)
		{
			SDL_Rect r = rect.ToSDL();
			SDL_RenderFillRect(Handle, ref r);
		}

		public void FillRectangle(IEnumerable<Rectangle> rects)
		{
			SDL_Rect[] nativeRects =
				rects.Select(o => o.ToSDL()).ToArray();

			SDL_RenderFillRects(Handle,
				nativeRects, nativeRects.Length);
		}

		public void FillRectangle(RectangleF rect)
		{
			SDL_FRect r = rect.ToSDL();
			SDL_RenderFillRectF(Handle, ref r);
		}

		public void FillRectangle(IEnumerable<RectangleF> rects)
		{
			SDL_FRect[] nativeRects =
				rects.Select(o => o.ToSDL()).ToArray();

			SDL_RenderFillRectsF(Handle,
				nativeRects, nativeRects.Length);
		}

		private Texture FetchTexture(IntPtr handle)
		{
			Texture result = null;

			if (handle != IntPtr.Zero)
			{
				foreach (KeyValuePair<IntPtr, WeakReference<Renderer>> kvp
					in _renderers)
				{
					if (kvp.Value.TryGetTarget(out Renderer target))
					{
						if (target._textures.ContainsKey(handle))
						{
							if (!target._textures[handle]
								.TryGetTarget(out result))
							{
								target._textures.Remove(handle);
							}
						}
					}
				}
			}

			return result;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (_textures != null)
				{
					foreach (KeyValuePair<IntPtr, WeakReference<Texture>> kvp 
						in _textures)
					{
						if (kvp.Value.TryGetTarget(out Texture target))
						{
							target?.Dispose();
						}
					}

					_textures.Clear();
					_textures = null;
				}

				if (Handle != IntPtr.Zero)
				{
					if (_renderers.ContainsKey(Handle))
					{
						_renderers.Remove(Handle);
					}

					SDL_DestroyRenderer(Handle);
					Handle = IntPtr.Zero;
				}

				disposedValue = true;
			}
		}
		
		~Renderer()
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
