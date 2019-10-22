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
	internal static class Extensions
	{
		private static Dictionary<SDL_Keycode, Key> _keyMap =
			new Dictionary<SDL_Keycode, Key>()
			{
				{ SDL_Keycode.SDLK_0, Key.Numeric_Zero },
				{ SDL_Keycode.SDLK_1, Key.Numeric_One },
				{ SDL_Keycode.SDLK_2, Key.Numeric_Two },
				{ SDL_Keycode.SDLK_3, Key.Numeric_Three },
				{ SDL_Keycode.SDLK_4, Key.Numeric_Four },
				{ SDL_Keycode.SDLK_5, Key.Numeric_Five },
				{ SDL_Keycode.SDLK_6, Key.Numeric_Six },
				{ SDL_Keycode.SDLK_7, Key.Numeric_Seven },
				{ SDL_Keycode.SDLK_8, Key.Numeric_Eight },
				{ SDL_Keycode.SDLK_9, Key.Numeric_Nine },

				{ SDL_Keycode.SDLK_NUMLOCKCLEAR, Key.Numpad_Clear },
				{ SDL_Keycode.SDLK_KP_0, Key.Numeric_Zero },
				{ SDL_Keycode.SDLK_KP_1, Key.Numeric_One },
				{ SDL_Keycode.SDLK_KP_2, Key.Numeric_Two },
				{ SDL_Keycode.SDLK_KP_3, Key.Numeric_Three },
				{ SDL_Keycode.SDLK_KP_4, Key.Numeric_Four },
				{ SDL_Keycode.SDLK_KP_5, Key.Numeric_Five },
				{ SDL_Keycode.SDLK_KP_6, Key.Numeric_Six },
				{ SDL_Keycode.SDLK_KP_7, Key.Numeric_Seven },
				{ SDL_Keycode.SDLK_KP_8, Key.Numeric_Eight },
				{ SDL_Keycode.SDLK_KP_9, Key.Numeric_Nine },
				{ SDL_Keycode.SDLK_KP_ENTER, Key.Numpad_Enter },
				{ SDL_Keycode.SDLK_KP_DIVIDE, Key.Numpad_SlashForward },
				{ SDL_Keycode.SDLK_KP_PLUS, Key.Numpad_Plus },
				{ SDL_Keycode.SDLK_KP_MINUS, Key.Numpad_Minus },
				{ SDL_Keycode.SDLK_KP_MULTIPLY, Key.Numpad_Asterisk },
				{ SDL_Keycode.SDLK_KP_PERIOD, Key.Numpad_Period },

				{ SDL_Keycode.SDLK_F1, Key.F1 },
				{ SDL_Keycode.SDLK_F2, Key.F2 },
				{ SDL_Keycode.SDLK_F3, Key.F3 },
				{ SDL_Keycode.SDLK_F4, Key.F4 },
				{ SDL_Keycode.SDLK_F5, Key.F5 },
				{ SDL_Keycode.SDLK_F6, Key.F6 },
				{ SDL_Keycode.SDLK_F7, Key.F7 },
				{ SDL_Keycode.SDLK_F8, Key.F8 },
				{ SDL_Keycode.SDLK_F9, Key.F9 },
				{ SDL_Keycode.SDLK_F10, Key.F10 },
				{ SDL_Keycode.SDLK_F11, Key.F11 },
				{ SDL_Keycode.SDLK_F12, Key.F12 },
				{ SDL_Keycode.SDLK_F13, Key.F13 },
				{ SDL_Keycode.SDLK_F14, Key.F14 },

				{ SDL_Keycode.SDLK_INSERT, Key.Insert },
				{ SDL_Keycode.SDLK_HOME, Key.Home },
				{ SDL_Keycode.SDLK_PAGEUP, Key.PageUp },
				{ SDL_Keycode.SDLK_PAGEDOWN, Key.PageDown },
				{ SDL_Keycode.SDLK_DELETE, Key.Delete },
				{ SDL_Keycode.SDLK_END, Key.End },
				{ SDL_Keycode.SDLK_PAUSE, Key.Pause },
				{ SDL_Keycode.SDLK_SCROLLLOCK, Key.ScrollLock },
				{ SDL_Keycode.SDLK_PRINTSCREEN, Key.PrintScreen },

				{ SDL_Keycode.SDLK_RETURN, Key.Return },
				{ SDL_Keycode.SDLK_BACKSPACE, Key.Backspace },
				{ SDL_Keycode.SDLK_SPACE, Key.Space },

				{ SDL_Keycode.SDLK_APPLICATION, Key.Application },
				{ SDL_Keycode.SDLK_TAB, Key.Tab },
				{ SDL_Keycode.SDLK_CAPSLOCK, Key.Capslock },
				{ SDL_Keycode.SDLK_LSHIFT, Key.ShiftLeft },
				{ SDL_Keycode.SDLK_RSHIFT, Key.ShiftRight },
				{ SDL_Keycode.SDLK_LALT, Key.AltLeft },
				{ SDL_Keycode.SDLK_RALT, Key.AltRight },
				{ SDL_Keycode.SDLK_LCTRL, Key.ControlLeft },
				{ SDL_Keycode.SDLK_RCTRL, Key.ControlRight },
				{ SDL_Keycode.SDLK_ESCAPE, Key.Escape },

				{ SDL_Keycode.SDLK_EXCLAIM, Key.Symbol_Exclamation },
				{ SDL_Keycode.SDLK_AT, Key.Symbol_At },
				{ SDL_Keycode.SDLK_HASH, Key.Symbol_Pound },
				{ SDL_Keycode.SDLK_DOLLAR, Key.Symbol_Dollar },
				{ SDL_Keycode.SDLK_PERCENT, Key.Symbol_Percent },
				{ SDL_Keycode.SDLK_POWER, Key.Symbol_Circumflex },
				{ SDL_Keycode.SDLK_AMPERSAND, Key.Symbol_Ampersand },
				{ SDL_Keycode.SDLK_ASTERISK, Key.Symbol_Asterisk },
				{ SDL_Keycode.SDLK_LEFTPAREN, Key.Symbol_ParenthesisLeft },
				{ SDL_Keycode.SDLK_RIGHTPAREN, Key.Symbol_ParenthesisRight },
				{ SDL_Keycode.SDLK_MINUS, Key.Symbol_Minus },
				{ SDL_Keycode.SDLK_PLUS, Key.Symbol_Plus },
				{ SDL_Keycode.SDLK_EQUALS, Key.Symbol_Equal },
				{ SDL_Keycode.SDLK_UNDERSCORE, Key.Symbol_Underscore },
				{ SDL_Keycode.SDLK_LEFTBRACKET, Key.Symbol_LeftBracket },
				{ SDL_Keycode.SDLK_RIGHTBRACKET, Key.Symbol_RightBracket },
				{ SDL_Keycode.SDLK_COLON, Key.Symbol_Colon },
				{ SDL_Keycode.SDLK_SEMICOLON, Key.Symbol_ColonSemi },
				{ SDL_Keycode.SDLK_QUOTE, Key.Symbol_QuoteSingle },
				{ SDL_Keycode.SDLK_QUOTEDBL, Key.Symbol_QuoteDouble },
				{ SDL_Keycode.SDLK_COMMA, Key.Symbol_Comma },
				{ SDL_Keycode.SDLK_PERIOD, Key.Symbol_Period },
				{ SDL_Keycode.SDLK_LESS, Key.Symbol_Less },
				{ SDL_Keycode.SDLK_GREATER, Key.Symbol_Greater },
				{ SDL_Keycode.SDLK_SLASH, Key.Symbol_SlashForward },
				{ SDL_Keycode.SDLK_BACKSLASH, Key.Symbol_SlashBack },
				{ SDL_Keycode.SDLK_BACKQUOTE, Key.Symbol_Grave },

				{ SDL_Keycode.SDLK_a, Key.A },
				{ SDL_Keycode.SDLK_b, Key.B },
				{ SDL_Keycode.SDLK_c, Key.C },
				{ SDL_Keycode.SDLK_d, Key.D },
				{ SDL_Keycode.SDLK_e, Key.E },
				{ SDL_Keycode.SDLK_f, Key.F },
				{ SDL_Keycode.SDLK_g, Key.G },
				{ SDL_Keycode.SDLK_h, Key.H },
				{ SDL_Keycode.SDLK_i, Key.I },
				{ SDL_Keycode.SDLK_j, Key.J },
				{ SDL_Keycode.SDLK_k, Key.K },
				{ SDL_Keycode.SDLK_l, Key.L },
				{ SDL_Keycode.SDLK_m, Key.M },
				{ SDL_Keycode.SDLK_n, Key.N },
				{ SDL_Keycode.SDLK_o, Key.O },
				{ SDL_Keycode.SDLK_p, Key.P },
				{ SDL_Keycode.SDLK_q, Key.Q },
				{ SDL_Keycode.SDLK_r, Key.R },
				{ SDL_Keycode.SDLK_s, Key.S },
				{ SDL_Keycode.SDLK_t, Key.T },
				{ SDL_Keycode.SDLK_u, Key.U },
				{ SDL_Keycode.SDLK_v, Key.V },
				{ SDL_Keycode.SDLK_w, Key.W },
				{ SDL_Keycode.SDLK_x, Key.X },
				{ SDL_Keycode.SDLK_y, Key.Y },
				{ SDL_Keycode.SDLK_z, Key.Z }
			};

		public static Key ToKey(this SDL_Keycode code)
		{
			return _keyMap[code];
		}

		public static KeyModifier ToKeyModifier(
			this SDL_Keymod km)
		{
			KeyModifier result = KeyModifier.None;
			
			if ((km & SDL_Keymod.KMOD_LALT) == SDL_Keymod.KMOD_LALT)
			{
				result |= KeyModifier.AltLeft;
			}

			if ((km & SDL_Keymod.KMOD_RALT) == SDL_Keymod.KMOD_RALT)
			{
				result |= KeyModifier.AltRight;
			}

			if ((km & SDL_Keymod.KMOD_LCTRL) == SDL_Keymod.KMOD_LCTRL)
			{
				result |= KeyModifier.ControlLeft;
			}

			if ((km & SDL_Keymod.KMOD_RCTRL) == SDL_Keymod.KMOD_RCTRL)
			{
				result |= KeyModifier.ControlRight;
			}
			
			if ((km & SDL_Keymod.KMOD_LSHIFT) == SDL_Keymod.KMOD_LSHIFT)
			{
				result |= KeyModifier.ShiftLeft;
			}

			if ((km & SDL_Keymod.KMOD_RSHIFT) == SDL_Keymod.KMOD_RSHIFT)
			{
				result |= KeyModifier.ShiftRight;
			}

			return result;
		}

		public static SDL_Point ToSDL(this Point point)
		{
			SDL_Point p = new SDL_Point();

			p.x = point.X;
			p.y = point.Y;

			return p;
		}

		public static Point ToDrawing(this SDL_Point point)
		{
			return new Point(point.x, point.y);
		}

		public static SDL_FPoint ToSDL(this PointF point)
		{
			SDL_FPoint p = new SDL_FPoint();

			p.x = point.X;
			p.y = point.Y;

			return p;
		}

		public static PointF ToDrawing(this SDL_FPoint point)
		{
			return new PointF(point.x, point.y);
		}

		public static SDL_Rect ToSDL(this Rectangle rect)
		{
			SDL_Rect r = new SDL_Rect();

			r.x = rect.X;
			r.y = rect.Y;
			r.w = rect.Width;
			r.h = rect.Height;

			return r;
		}

		public static Rectangle ToDrawing(this SDL_Rect rect)
		{
			return new Rectangle(rect.x, rect.y, rect.w, rect.h);
		}

		public static SDL_FRect ToSDL(this RectangleF rect)
		{
			SDL_FRect r = new SDL_FRect();

			r.x = rect.X;
			r.y = rect.Y;
			r.w = rect.Width;
			r.h = rect.Height;

			return r;
		}

		public static RectangleF ToDrawing(this SDL_FRect rect)
		{
			return new RectangleF(rect.x, rect.y, rect.w, rect.h);
		}

		public static Color ToDrawing(this SDL_Color color)
		{
			return Color.FromArgb(color.a, color.r, color.g, color.b);
		}

		public static SDL_Color ToSDL(this Color color)
		{
			SDL_Color result = new SDL_Color();

			result.r = color.R;
			result.g = color.G;
			result.b = color.B;
			result.a = color.A;

			return result;
		}

		public static uint ToSDL(this Color color, IntPtr surface)
		{
			uint pixel = 0;

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				pixel = 
					SDL_MapRGBA(
						((SDL_Surface*)surface.ToPointer())->format,
						color.R,
						color.G,
						color.B,
						color.A);
			}
#else
			SDL_Surface surf = Marshal.PtrToStructure<SDL_Surface>(surface);
			pixel = color.ToSDL(ref surf);
#endif

			return pixel;
		}

		public static uint ToSDL(this Color color, ref SDL_Surface surface)
		{
			return SDL_MapRGBA(
						surface.format,
						color.R,
						color.G,
						color.B,
						color.A);
		}

		public static Color ToColorFromSDL(this uint pixel, IntPtr surface)
		{
			Color result = Color.Black;

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_GetRGBA(
					pixel,
					((SDL_Surface*)surface.ToPointer())->format,
					out byte r,
					out byte g,
					out byte b,
					out byte a);

				result = Color.FromArgb(a, r, g, b);
			}
#else
			SDL_Surface surf = Marshal.PtrToStructure<SDL_Surface>(surface);
			result = pixel.ToColorFromSDL(ref surf);
#endif

			return result;
		}

		public static Color ToColorFromSDL(
			this uint pixel,
			ref SDL_Surface surface)
		{
			SDL_GetRGBA(
				pixel,
				surface.format,
				out byte r,
				out byte g,
				out byte b,
				out byte a);

			return Color.FromArgb(a, r, g, b);
		}

		public static SDL_RendererFlip ToSDL(this RenderFlip flip)
		{
			SDL_RendererFlip result = 
				SDL_RendererFlip.SDL_FLIP_NONE;

			if ((flip & RenderFlip.Horizontal) == RenderFlip.Horizontal)
			{
				result |= SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
			}

			if ((flip & RenderFlip.Vertical) == RenderFlip.Vertical)
			{
				result |= SDL_RendererFlip.SDL_FLIP_VERTICAL;
			}

			return result;
		}

		public static uint ToSDL(this PixelFormat format)
		{
			uint result = SDL_PIXELFORMAT_UNKNOWN;

			switch (format)
			{
				default:
				case PixelFormat.Unknown:
					result = 0;
					break;
				case PixelFormat.Indexed1BppLittle:
					result = SDL_PIXELFORMAT_INDEX1LSB;
					break;
				case PixelFormat.Indexed1BppBig:
					result = SDL_PIXELFORMAT_INDEX1MSB;
					break;
				case PixelFormat.Indexed4BppLittle:
					result = SDL_PIXELFORMAT_INDEX4LSB;
					break;
				case PixelFormat.Indexed4BppBig:
					result = SDL_PIXELFORMAT_INDEX4MSB;
					break;
				case PixelFormat.Indexed8Bpp:
					result = SDL_PIXELFORMAT_INDEX8;
					break;
				case PixelFormat.RGB332:
					result = SDL_PIXELFORMAT_RGB332;
					break;
				case PixelFormat.RGB444:
					result = SDL_PIXELFORMAT_RGB444;
					break;
				case PixelFormat.RGB555:
					result = SDL_PIXELFORMAT_RGB555;
					break;
				case PixelFormat.BGR555:
					result = SDL_PIXELFORMAT_BGR555;
					break;
				case PixelFormat.ARGB4444:
					result = SDL_PIXELFORMAT_ARGB4444;
					break;
				case PixelFormat.RGBA4444:
					result = SDL_PIXELFORMAT_RGBA4444;
					break;
				case PixelFormat.ABGR4444:
					result = SDL_PIXELFORMAT_ABGR4444;
					break;
				case PixelFormat.BGRA4444:
					result = SDL_PIXELFORMAT_BGRA4444;
					break;
				case PixelFormat.ARGB1555:
					result = SDL_PIXELFORMAT_ARGB1555;
					break;
				case PixelFormat.RGBA5551:
					result = SDL_PIXELFORMAT_RGBA5551;
					break;
				case PixelFormat.ABGR1555:
					result = SDL_PIXELFORMAT_ABGR1555;
					break;
				case PixelFormat.BGRA5551:
					result = SDL_PIXELFORMAT_BGRA5551;
					break;
				case PixelFormat.RGB565:
					result = SDL_PIXELFORMAT_RGB565;
					break;
				case PixelFormat.BGR565:
					result = SDL_PIXELFORMAT_BGR565;
					break;
				case PixelFormat.RGB24:
					result = SDL_PIXELFORMAT_RGB24;
					break;
				case PixelFormat.BGR24:
					result = SDL_PIXELFORMAT_BGR24;
					break;
				case PixelFormat.RGB888:
					result = SDL_PIXELFORMAT_RGB888;
					break;
				case PixelFormat.RGBX8888:
					result = SDL_PIXELFORMAT_RGBX8888;
					break;
				case PixelFormat.BGR888:
					result = SDL_PIXELFORMAT_BGR888;
					break;
				case PixelFormat.BGRX8888:
					result = SDL_PIXELFORMAT_BGRX8888;
					break;
				case PixelFormat.ARGB8888:
					result = SDL_PIXELFORMAT_ARGB8888;
					break;
				case PixelFormat.RGBA8888:
					result = SDL_PIXELFORMAT_RGBA8888;
					break;
				case PixelFormat.ABGR8888:
					result = SDL_PIXELFORMAT_ABGR8888;
					break;
				case PixelFormat.BGRA8888:
					result = SDL_PIXELFORMAT_BGRA8888;
					break;
				case PixelFormat.ARGB2101010:
					result = SDL_PIXELFORMAT_ARGB2101010;
					break;
				case PixelFormat.YV12:
					result = SDL_PIXELFORMAT_YV12;
					break;
				case PixelFormat.IYUV:
					result = SDL_PIXELFORMAT_IYUV;
					break;
				case PixelFormat.YUY2:
					result = SDL_PIXELFORMAT_YUY2;
					break;
				case PixelFormat.UYVY:
					result = SDL_PIXELFORMAT_UYVY;
					break;
				case PixelFormat.YVYU:
					result = SDL_PIXELFORMAT_YVYU;
					break;
			}

			return result;
		}

		public static PixelFormat ToPixelFormat(this uint format)
		{
			PixelFormat result = PixelFormat.Unknown;

			if (format == SDL_PIXELFORMAT_INDEX1LSB)
			{
				result = PixelFormat.Indexed1BppLittle;
			}
			else if (format == SDL_PIXELFORMAT_INDEX1MSB)
			{
				result = PixelFormat.Indexed1BppBig;
			}
			else if (format == SDL_PIXELFORMAT_INDEX4LSB)
			{
				result = PixelFormat.Indexed4BppLittle;
			}
			else if (format == SDL_PIXELFORMAT_INDEX4MSB)
			{
				result = PixelFormat.Indexed4BppBig;
			}
			else if (format == SDL_PIXELFORMAT_INDEX8)
			{
				result = PixelFormat.Indexed8Bpp;
			}
			else if (format == SDL_PIXELFORMAT_RGB332)
			{
				result = PixelFormat.RGB332;
			}
			else if (format == SDL_PIXELFORMAT_RGB444)
			{
				result = PixelFormat.RGB444;
			}
			else if (format == SDL_PIXELFORMAT_RGB555)
			{
				result = PixelFormat.RGB555;
			}
			else if (format == SDL_PIXELFORMAT_BGR555)
			{
				result = PixelFormat.BGR555;
			}
			else if (format == SDL_PIXELFORMAT_ARGB4444)
			{
				result = PixelFormat.ARGB4444;
			}
			else if (format == SDL_PIXELFORMAT_RGBA4444)
			{
				result = PixelFormat.RGBA4444;
			}
			else if (format == SDL_PIXELFORMAT_ABGR4444)
			{
				result = PixelFormat.ABGR4444;
			}
			else if (format == SDL_PIXELFORMAT_BGRA4444)
			{
				result = PixelFormat.BGRA4444;
			}
			else if (format == SDL_PIXELFORMAT_ARGB1555)
			{
				result = PixelFormat.ARGB1555;
			}
			else if (format == SDL_PIXELFORMAT_RGBA5551)
			{
				result = PixelFormat.RGBA5551;
			}
			else if (format == SDL_PIXELFORMAT_ABGR1555)
			{
				result = PixelFormat.ABGR1555;
			}
			else if (format == SDL_PIXELFORMAT_BGRA5551)
			{
				result = PixelFormat.BGRA5551;
			}
			else if (format == SDL_PIXELFORMAT_RGB565)
			{
				result = PixelFormat.RGB565;
			}
			else if (format == SDL_PIXELFORMAT_BGR565)
			{
				result = PixelFormat.BGR565;
			}
			else if (format == SDL_PIXELFORMAT_RGB24)
			{
				result = PixelFormat.RGB24;
			}
			else if (format == SDL_PIXELFORMAT_BGR24)
			{
				result = PixelFormat.BGR24;
			}
			else if (format == SDL_PIXELFORMAT_RGB888)
			{
				result = PixelFormat.RGB888;
			}
			else if (format == SDL_PIXELFORMAT_RGBX8888)
			{
				result = PixelFormat.RGBX8888;
			}
			else if (format == SDL_PIXELFORMAT_BGR888)
			{
				result = PixelFormat.BGR888;
			}
			else if (format == SDL_PIXELFORMAT_BGRX8888)
			{
				result = PixelFormat.BGRX8888;
			}
			else if (format == SDL_PIXELFORMAT_ARGB8888)
			{
				result = PixelFormat.ARGB8888;
			}
			else if (format == SDL_PIXELFORMAT_RGBA8888)
			{
				result = PixelFormat.RGBA8888;
			}
			else if (format == SDL_PIXELFORMAT_ABGR8888)
			{
				result = PixelFormat.ABGR8888;
			}
			else if (format == SDL_PIXELFORMAT_BGRA8888)
			{
				result = PixelFormat.BGRA8888;
			}
			else if (format == SDL_PIXELFORMAT_ARGB2101010)
			{
				result = PixelFormat.ARGB2101010;
			}
			else if (format == SDL_PIXELFORMAT_YV12)
			{
				result = PixelFormat.YV12;
			}
			else if (format == SDL_PIXELFORMAT_IYUV)
			{
				result = PixelFormat.IYUV;
			}
			else if (format == SDL_PIXELFORMAT_YUY2)
			{
				result = PixelFormat.YUY2;
			}
			else if (format == SDL_PIXELFORMAT_UYVY)
			{
				result = PixelFormat.UYVY;
			}
			else if (format == SDL_PIXELFORMAT_YVYU)
			{
				result = PixelFormat.YVYU;
			}

			return result;
		}

		public static SDL_HitTestResult ToSDL(this WindowHitTestResult value)
		{
			SDL_HitTestResult result = SDL_HitTestResult.SDL_HITTEST_NORMAL;
			
			switch (value)
			{
				case WindowHitTestResult.Drag:
					result = SDL_HitTestResult.SDL_HITTEST_DRAGGABLE;
					break;
				case WindowHitTestResult.ResizeTopLeft:
					result = SDL_HitTestResult.SDL_HITTEST_RESIZE_TOPLEFT;
					break;
				case WindowHitTestResult.ResizeTop:
					result = SDL_HitTestResult.SDL_HITTEST_RESIZE_TOP;
					break;
				case WindowHitTestResult.ResizeTopRight:
					result = SDL_HitTestResult.SDL_HITTEST_RESIZE_TOPRIGHT;
					break;
				case WindowHitTestResult.ResizeRight:
					result = SDL_HitTestResult.SDL_HITTEST_RESIZE_RIGHT;
					break;
				case WindowHitTestResult.ResizeBottomRight:
					result = 
						SDL_HitTestResult.SDL_HITTEST_RESIZE_BOTTOMRIGHT;
					break;
				case WindowHitTestResult.ResizeBottom:
					result = SDL_HitTestResult.SDL_HITTEST_RESIZE_BOTTOM;
					break;
				case WindowHitTestResult.ResizeBottomLeft:
					result = 
						SDL_HitTestResult.SDL_HITTEST_RESIZE_BOTTOMLEFT;
					break;
				case WindowHitTestResult.ResizeLeft:
					result = SDL_HitTestResult.SDL_HITTEST_RESIZE_LEFT;
					break;
				case WindowHitTestResult.Normal:
				default:
					result = SDL_HitTestResult.SDL_HITTEST_NORMAL;
					break;
			}

			return result;
		}
	}
}
