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
	public enum WindowMode
	{
		/// <summary>
		/// Normal windowed mode
		/// </summary>
		Normal = 0,
		/// <summary>
		/// Occupy full area of desktop
		/// </summary>
		Fullscreen,
		/// <summary>
		/// Change resolution to size of window
		/// </summary>
		Exclusive
	}
	
	public enum WindowHitTestResult
	{
		Normal = 0,
		Drag,
		ResizeTopLeft,
		ResizeTop,
		ResizeTopRight,
		ResizeRight,
		ResizeBottomRight,
		ResizeBottom,
		ResizeBottomLeft,
		ResizeLeft
	}

	[Flags]
	public enum MouseButton
	{
		None = 0,
		Left = 1,
		Right = 2,
		Middle = 4,
		Extra1 = 8,
		Extra2 = 16
	}

	public enum Key
	{
		None = 0,

		Numeric_Zero,
		Numeric_One,
		Numeric_Two,
		Numeric_Three,
		Numeric_Four,
		Numeric_Five,
		Numeric_Six,
		Numeric_Seven,
		Numeric_Eight,
		Numeric_Nine,
		
		Symbol_Exclamation,
		Symbol_At,
		Symbol_Pound,
		Symbol_Dollar,
		Symbol_Percent,
		Symbol_Circumflex,
		Symbol_Ampersand,
		Symbol_Asterisk,
		Symbol_ParenthesisLeft,
		Symbol_ParenthesisRight,
		Symbol_Minus,
		Symbol_Plus,
		Symbol_Underscore,
		Symbol_Equal,
		Symbol_LeftBracket,
		Symbol_RightBracket,
		Symbol_BraceLeft,
		Symbol_BraceRight,
		Symbol_SlashForward,
		Symbol_SlashBack,
		Symbol_Pipe,
		Symbol_Colon,
		Symbol_ColonSemi,
		Symbol_QuoteDouble,
		Symbol_QuoteSingle,
		Symbol_Question,
		Symbol_Less,
		Symbol_Greater,
		Symbol_Comma,
		Symbol_Period,
		Symbol_Tilde, // TODO
		Symbol_Grave, // TODO
		
		Numpad_Clear,
		Numpad_SlashForward,
		Numpad_Asterisk,
		Numpad_Minus,
		Numpad_Plus,
		Numpad_Period,
		Numpad_Enter,
		Numpad_Zero,
		Numpad_One,
		Numpad_Two,
		Numpad_Three,
		Numpad_Four,
		Numpad_Five,
		Numpad_Six,
		Numpad_Seven,
		Numpad_Eight,
		Numpad_Nine,

		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
		F13,
		F14,

		Insert,
		Home,
		End,
		PageUp,
		PageDown,
		Delete,
		Escape,
		ScrollLock,
		PrintScreen,
		Pause,
		
		Backspace,
		Return,
		Space,

		Up,
		Right,
		Down,
		Left,

		Tab,
		Capslock,
		ShiftLeft,
		ShiftRight,
		ControlLeft,
		ControlRight,
		AltLeft,
		AltRight,
		Application,

		A, B, C, D, E, F, G, H, I, J, K,
		L, M, N, O, P, Q, R, S, T, U, V,
		W, X, Y, Z
	}

	[Flags]
	public enum KeyModifier
	{
		None = 0,

		ControlLeft = 1,
		ControlRight = 2,

		AltLeft = 4,
		AltRight = 8,

		ShiftLeft = 16,
		ShiftRight = 32,

		Control = (ControlLeft | ControlRight),
		Alt = (AltLeft | AltRight),
		Shift = (ShiftLeft | ShiftRight)
	}

	[Flags]
	public enum RenderFlip
	{
		None = 0,
		Horizontal = 1,
		Vertical = 2
	}

	public enum PixelFormat
	{
		Unknown = 0,
		Indexed1BppLittle,
		Indexed1BppBig,
		Indexed4BppLittle,
		Indexed4BppBig,
		Indexed8Bpp,
		RGB332,
		RGB444,
		RGB555,
		BGR555,
		ARGB4444,
		RGBA4444,
		ABGR4444,
		BGRA4444,
		ARGB1555,
		RGBA5551,
		ABGR1555,
		BGRA5551,
		RGB565,
		BGR565,
		RGB24,
		BGR24,
		RGB888,
		RGBX8888,
		BGR888,
		BGRX8888,
		ARGB8888,
		RGBA8888,
		ABGR8888,
		BGRA8888,
		ARGB2101010,
		RGBA32,
		ARGB32,
		BGRA32,
		ABGR32,
		YV12,
		IYUV,
		YUY2,
		UYVY,
		YVYU,
		NV12,
		NV21
	}
}
