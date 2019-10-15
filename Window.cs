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
	public class Window : IDisposable
	{
		public delegate void WindowEventHandler(
			object sender, EventArgs e);

		public delegate void WindowTextInputEventHandler(
			object sender, TextInputEventArgs e);

		public delegate void WindowKeyboardEventHandler(
			object sender, KeyboardEventArgs e);

		public delegate void WindowMouseEventHandler(
			object sender, MouseEventArgs e);

		public event WindowEventHandler Load;
		public event WindowEventHandler Hidden;
		public event WindowEventHandler Shown;
		public event WindowEventHandler Resized;
		public event WindowEventHandler Closed;
		public event WindowEventHandler Focused;
		public event WindowEventHandler Blurred;
		public event WindowEventHandler Moved;
		public event WindowEventHandler Maximized;
		public event WindowEventHandler Minimized;
		public event WindowEventHandler Restored;
		public event WindowEventHandler SizeChanged;

		public event WindowTextInputEventHandler TextInput;
		public event WindowTextInputEventHandler TextEdit;

		public event WindowKeyboardEventHandler KeyDown;
		public event WindowKeyboardEventHandler KeyUp;
		public event WindowKeyboardEventHandler KeyRepeat;

		public event WindowMouseEventHandler MouseDown;
		public event WindowMouseEventHandler MouseUp;
		public event WindowMouseEventHandler MouseMove;
		public event WindowMouseEventHandler MouseWheel;
		public event WindowEventHandler MouseEnter;
		public event WindowEventHandler MouseLeave;

		private bool _loaded;

		internal Window()
		{
			_loaded = false;

			Handle = SDL_CreateWindow(
				string.Empty,
				SDL_WINDOWPOS_UNDEFINED,
				SDL_WINDOWPOS_UNDEFINED,
				0,
				0,
				SDL_WindowFlags.SDL_WINDOW_HIDDEN);

			SDL_SysWMinfo info = new SDL_SysWMinfo();
			SDL_VERSION(out info.version);
			SDL_GetWindowWMInfo(Handle, ref info);

			NativeHandle = info.info.win.window;

			Renderer = new Renderer(
				SDL_CreateRenderer(
					Handle,
					0,
					SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
					SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC |
					SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE));

			ID = SDL_GetWindowID(Handle);
		}

		public uint ID
		{
			get;
			private set;
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		public IntPtr NativeHandle
		{
			get;
			private set;
		}

		public Renderer Renderer
		{
			get;
			private set;
		}

		public WindowMode Mode
		{
			get
			{
				WindowMode result = WindowMode.Normal;

				if (WindowHasFlag(
					SDL_WindowFlags.SDL_WINDOW_FULLSCREEN))
				{
					result = WindowMode.Exclusive;
				}
				else if (WindowHasFlag(
					SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP))
				{
					result = WindowMode.Fullscreen;
				}

				return result;
			}
			set
			{
				switch (value)
				{
					case WindowMode.Fullscreen:
						SDL_SetWindowFullscreen(Handle,
							(uint)SDL_WindowFlags.
								SDL_WINDOW_FULLSCREEN_DESKTOP);
						break;
					case WindowMode.Exclusive:
						SDL_SetWindowFullscreen(Handle,
							(uint)SDL_WindowFlags.
								SDL_WINDOW_FULLSCREEN);
						break;
					case WindowMode.Normal:
					default:
						SDL_SetWindowFullscreen(Handle,
							(uint)0);
						break;
				}
			}
		}

		public Size MaximumSize
		{
			get
			{
				SDL_GetWindowMaximumSize(Handle, out int w, out int h);
				return new Size(w, h);
			}
			set
			{
				SDL_SetWindowMaximumSize(Handle, value.Width, value.Height);
			}
		}
		
		public Size MinimumSize
		{
			get
			{
				SDL_GetWindowMinimumSize(Handle, out int w, out int h);
				return new Size(w, h);
			}
			set
			{
				SDL_SetWindowMinimumSize(Handle, value.Width, value.Height);
			}
		}

		public (int top, int right, int bottom, int left) BorderSize
		{
			get
			{
				SDL_GetWindowBordersSize(
					Handle,
					out int top,
					out int left,
					out int bottom,
					out int right);

				return (top, right, bottom, left);
			}
		}

		public Size Size
		{
			get
			{
				SDL_GetWindowSize(Handle, out int w, out int h);
				return new Size(w, h);
			}
			set
			{
				SDL_SetWindowSize(Handle, value.Width, value.Height);
			}
		}

		public Point Position
		{
			get
			{
				SDL_GetWindowPosition(Handle, out int x, out int y);
				return new Point(x, y);
			}
			set
			{
				SDL_SetWindowPosition(Handle, value.X, value.Y);
			}
		}

		public string Title
		{
			get
			{
				return SDL_GetWindowTitle(Handle);
			}
			set
			{
				SDL_SetWindowTitle(Handle, value);
			}
		}

		public bool IsVisible
		{
			get
			{
				return !WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_HIDDEN);
			}
			set
			{
				if (value)
				{
					if (!IsVisible)
					{
						Show();
					}
				}
				else
				{
					if (IsVisible)
					{
						Hide();
					}
				}
			}
		}

		public float Opacity
		{
			get
			{
				SDL_GetWindowOpacity(Handle, out float opacity);
				return opacity;
			}
			set
			{
				SDL_SetWindowOpacity(Handle, value);
			}
		}

		public bool Grab
		{
			get
			{
				return SDL_GetWindowGrab(Handle) == SDL_bool.SDL_TRUE;
			}
			set
			{
				SDL_SetWindowGrab(
					Handle,
					value ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);
			}
		}

		public (ushort r, ushort g, ushort b)[] GammaRamp
		{
			get
			{
				ushort[] r = new ushort[256];
				ushort[] g = new ushort[256];
				ushort[] b = new ushort[256];

				(ushort r, ushort g, ushort b)[] result = 
					new (ushort, ushort, ushort)[256];

				SDL_GetWindowGammaRamp(Handle, r, g, b);

				for (int i = 0; i < result.Length; i++)
				{
					result[i].r = r[i];
					result[i].g = g[i];
					result[i].b = b[i];
				}

				return result;
			}
			set
			{
				ushort[] r = new ushort[256];
				ushort[] g = new ushort[256];
				ushort[] b = new ushort[256];

				for (int i = 0; i < value.Length; i++)
				{
					r[i] = value[i].r;
					g[i] = value[i].g;
					b[i] = value[i].b;
				}

				SDL_SetWindowGammaRamp(Handle, r, g, b);
			}
		}

		public bool IsMaximized
		{
			get
			{
				return WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED);
			}
			set
			{
				if (value)
				{
					SDL_MaximizeWindow(Handle);
				}
				else if (WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED))
				{
					SDL_RestoreWindow(Handle);
				}
			}
		}

		public bool IsMinimized
		{
			get
			{
				return WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_MINIMIZED);
			}
			set
			{
				if (value)
				{
					SDL_MinimizeWindow(Handle);
				}
				else if  (WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_MINIMIZED))
				{
					SDL_RestoreWindow(Handle);
				}
			}
		}

		public bool HasBorder
		{
			get
			{
				return !WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_BORDERLESS);
			}
			set
			{
				SDL_SetWindowBordered(Handle,
					value ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);
			}
		}

		public bool IsResizable
		{
			get
			{
				return WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
			}
			set
			{
				SDL_SetWindowResizable(Handle,
					value ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);
			}
		}

		public static bool IsCursorVisible
		{
			get
			{
				return SDL_ShowCursor(SDL_QUERY) == SDL_ENABLE;
			}
			set
			{
				SDL_ShowCursor(value ? SDL_ENABLE : SDL_DISABLE);
			}
		}

		public static bool IsTextInputActive
		{
			get
			{
				return SDL_IsTextInputActive() == SDL_bool.SDL_TRUE;
			}
			set
			{
				if (value)
				{
					SDL_StartTextInput();
				}
				else
				{
					SDL_StopTextInput();
				}
			}
		}

		public static Rectangle TextInputRectangle
		{
			set
			{
				SDL_Rect r = new SDL_Rect();

				r.x = value.X;
				r.y = value.Y;
				r.w = value.Width;
				r.h = value.Height;

				SDL_SetTextInputRect(ref r);
			}
		}

		public bool HasInputFocus
		{
			get
			{
				return WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);
			}
		}
		
		public bool HasMouseFocus
		{
			get
			{
				return WindowHasFlag(SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS);
			}
		}

		public virtual void Focus()
		{
			SDL_RaiseWindow(Handle);
		}

		public virtual void Show()
		{
			if (!IsVisible)
			{
				SDL_ShowWindow(Handle);
			}
		}

		public virtual void Hide()
		{
			if (IsVisible)
			{
				SDL_HideWindow(Handle);
			}
		}

		protected virtual void OnLoad()
		{
			if (!_loaded)
			{
				_loaded = true;
				Load?.Invoke(this, new EventArgs());
			}
		}

		protected virtual void OnMove()
		{
			Moved?.Invoke(this, new EventArgs());
		}

		protected virtual void OnShow()
		{
			if (!_loaded)
			{
				OnLoad();
			}

			Shown?.Invoke(this, new EventArgs());
		}

		protected virtual void OnHide()
		{
			Hidden?.Invoke(this, new EventArgs());
		}

		protected virtual void OnResize()
		{
			Resized?.Invoke(this, new EventArgs());
		}

		protected virtual void OnSizeChanged()
		{
			SizeChanged?.Invoke(this, new EventArgs());
		}

		protected virtual void OnClose()
		{
			Closed?.Invoke(this, new EventArgs());
		}

		protected virtual void OnKeyDown(KeyboardEventArgs e)
		{
			KeyDown?.Invoke(this, e);
		}

		protected virtual void OnKeyUp(KeyboardEventArgs e)
		{
			KeyUp?.Invoke(this, e);
		}

		protected virtual void OnKeyRepeat(KeyboardEventArgs e)
		{
			KeyRepeat?.Invoke(this, e);
		}

		protected virtual void OnTextInput(TextInputEventArgs e)
		{
			TextInput?.Invoke(this, e);
		}

		protected virtual void OnTextEdit(TextInputEventArgs e)
		{
			TextEdit?.Invoke(this, e);
		}

		protected virtual void OnMouseDown(MouseEventArgs e)
		{
			MouseDown?.Invoke(this, e);
		}

		protected virtual void OnMouseUp(MouseEventArgs e)
		{
			MouseUp?.Invoke(this, e);
		}

		protected virtual void OnMouseMove(MouseEventArgs e)
		{
			MouseMove?.Invoke(this, e);
		}

		protected virtual void OnMouseWheel(MouseEventArgs e)
		{
			MouseWheel?.Invoke(this, e);
		}

		protected virtual void OnMouseEnter()
		{
			MouseEnter?.Invoke(this, new EventArgs());
		}
		
		protected virtual void OnMouseLeave()
		{
			MouseLeave?.Invoke(this, new EventArgs());
		}

		protected virtual void OnFocus()
		{
			Focused?.Invoke(this, new EventArgs());
		}

		protected virtual void OnBlur()
		{
			Blurred?.Invoke(this, new EventArgs());
		}

		protected virtual void OnMaximized()
		{
			Maximized?.Invoke(this, new EventArgs());
		}

		protected virtual void OnMinimized()
		{
			Minimized?.Invoke(this, new EventArgs());
		}

		protected virtual void OnRestored()
		{
			Restored?.Invoke(this, new EventArgs());
		}

#if HIT_TESTS_ENABLED
		public bool HitTestEnabled
		{
			set
			{
				if (value)
				{
					SDL_SetWindowHitTest(Handle, NativeHitTest, IntPtr.Zero);
				}
				else
				{
					SDL_SetWindowHitTest(Handle, null, IntPtr.Zero);
				}
			}
		}

		protected virtual WindowHitTestResult OnHitTest(Point position)
		{
			Debug.WriteLine(nameof(OnHitTest));
			return WindowHitTestResult.Normal;
		}

		private SDL_HitTestResult NativeHitTest(
			IntPtr win, IntPtr area, IntPtr data)
		{
			SDL_HitTestResult result = SDL_HitTestResult.SDL_HITTEST_NORMAL;

			if (win != IntPtr.Zero && win == Handle)
			{
				SDL_Point p = new SDL_Point();

				unsafe
				{
					if (area != IntPtr.Zero)
					{
						SDL_Point* pArea = (SDL_Point*)area.ToPointer();

						p.x = pArea->x;
						p.y = pArea->y;
					}
				}

				WindowHitTestResult testResult = 
					OnHitTest(new Point(p.x, p.y));

				switch (testResult)
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
			}

			return result;
		}
#endif
		
		private bool WindowHasFlag(
			SDL_WindowFlags flag)
		{
			return (
				(SDL_WindowFlags)SDL_GetWindowFlags(Handle) &
				flag) == flag;
		}

		private bool IsEventForMe(SDL_Event e)
		{
			bool result = false;

			switch (e.type)
			{
				case SDL_EventType.SDL_KEYUP:
				case SDL_EventType.SDL_KEYDOWN:
					result = (e.key.windowID == ID);
					break;
				case SDL_EventType.SDL_MOUSEBUTTONDOWN:
				case SDL_EventType.SDL_MOUSEBUTTONUP:
					result = (e.button.windowID == ID);
					break;
				case SDL_EventType.SDL_MOUSEWHEEL:
					result = (e.button.windowID == ID);
					break;
				case SDL_EventType.SDL_MOUSEMOTION:
					result = (e.motion.windowID == ID);
					break;
				case SDL_EventType.SDL_WINDOWEVENT:
					result = (e.window.windowID == ID);
					break;
				case SDL_EventType.SDL_TEXTEDITING:
				case SDL_EventType.SDL_TEXTINPUT:
					result = (e.text.windowID == ID);
					break;
			}

			return result;
		}

		internal bool DoEvent(SDL_Event e)
		{
			bool result = IsEventForMe(e);

			if (result)
			{
				ProcessEvents(new SDL_Event[] { e });
			}

			return result;
		}

		internal int DoEvents(IEnumerable<SDL_Event> events)
		{
			int result = 0;

			foreach (SDL_Event e in events)
			{
				result += DoEvent(e) ? 1 : 0;
			}

			return result;
		}

		public int DoEvents()
		{
			SDL_Event[] events = new SDL_Event[256];

			List<SDL_Event> consumedEvents = new List<SDL_Event>(256);
			List<SDL_Event> returnedEvents = new List<SDL_Event>(256);

			int numEvents = 0;

			while ((numEvents = SDL_PeepEvents(
				events, events.Length,
				SDL_eventaction.SDL_GETEVENT,
				SDL_EventType.SDL_FIRSTEVENT, SDL_EventType.SDL_LASTEVENT))
				> 0)
			{
				for (int i = 0; i < numEvents; i++)
				{
					SDL_Event e = events[i];

					if (!IsEventForMe(e))
					{
						returnedEvents.Add(e);
					}
					else
					{
						consumedEvents.Add(e);
					}
				}
			}

			if (returnedEvents.Count > 0)
			{
				events = returnedEvents.ToArray();
				SDL_PeepEvents(
					events, events.Length,
					SDL_eventaction.SDL_ADDEVENT,
					SDL_EventType.SDL_FIRSTEVENT, SDL_EventType.SDL_LASTEVENT);
			}

			ProcessEvents(consumedEvents);

			return consumedEvents.Count;
		}

		private void ProcessEvent(SDL_Event e)
		{
			switch (e.type)
			{
				case SDL_EventType.SDL_MOUSEBUTTONDOWN:
				case SDL_EventType.SDL_MOUSEBUTTONUP:
					OnMouseButtonEvent(e);
					break;
				case SDL_EventType.SDL_KEYDOWN:
				case SDL_EventType.SDL_KEYUP:
					OnKeyEvent(e);
					break;
				case SDL_EventType.SDL_MOUSEWHEEL:
					OnMouseWheelEvent(e);
					break;
				case SDL_EventType.SDL_MOUSEMOTION:
					OnMouseMotionEvent(e);
					break;
				case SDL_EventType.SDL_WINDOWEVENT:
					OnWindowEvent(e);
					break;
				case SDL_EventType.SDL_TEXTEDITING:
				case SDL_EventType.SDL_TEXTINPUT:
					OnTextInputEvent(e);
					break;
			}
		}

		private void ProcessEvents(IEnumerable<SDL_Event> events)
		{
			foreach (SDL_Event e in events)
			{
				ProcessEvent(e);
			}
		}

		private void OnWindowEvent(SDL_Event e)
		{
			switch (e.window.windowEvent)
			{
				case SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
					OnMove();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
					OnShow();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
					OnHide();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
					OnClose();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
					OnResize();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
					OnFocus();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
					OnBlur();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
					OnMouseEnter();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
					OnMouseLeave();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
					OnMaximized();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
					OnMinimized();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
					OnRestored();
					break;
				case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
					OnSizeChanged();
					break;
			}
		}

		private void OnTextInputEvent(SDL_Event e)
		{
			TextInputEventArgs args = new TextInputEventArgs();

			args.Start = e.edit.start;
			args.Length = e.edit.length;

			switch (e.type)
			{
				case SDL_EventType.SDL_TEXTINPUT:
					unsafe
					{
						args.Text = new string((sbyte*)e.text.text);
					}

					OnTextInput(args);
					break;
				case SDL_EventType.SDL_TEXTEDITING:
					unsafe
					{
						args.Text = new string((sbyte*)e.edit.text);
					}

					OnTextEdit(args);
					break;
			}
		}

		private void OnKeyEvent(SDL_Event e)
		{
			KeyboardEventArgs args = new KeyboardEventArgs();

			args.Key = e.key.keysym.sym.ToKey();
			args.Modifier = e.key.keysym.mod.ToKeyModifier();
			
			if (e.key.repeat < 1)
			{
				switch (e.key.type)
				{
					case SDL_EventType.SDL_KEYDOWN:
						OnKeyDown(args);
						break;
					case SDL_EventType.SDL_KEYUP:
						OnKeyUp(args);
						break;
				}
			}
			else if (e.key.repeat > 0)
			{
				switch (e.key.type)
				{
					case SDL_EventType.SDL_KEYDOWN:
						OnKeyRepeat(args);
						break;
				}
			}
		}

		private void OnMouseButtonEvent(SDL_Event e)
		{
			MouseEventArgs args = new MouseEventArgs();

			args.Position = new Point(
				e.button.x,
				e.button.y);

			switch ((uint)e.button.button)
			{
				case SDL_BUTTON_LEFT:
					args.Button = MouseButton.Left;
					break;
				case SDL_BUTTON_MIDDLE:
					args.Button = MouseButton.Middle;
					break;
				case SDL_BUTTON_RIGHT:
					args.Button = MouseButton.Right;
					break;
				case SDL_BUTTON_X1:
					args.Button = MouseButton.Extra1;
					break;
				case SDL_BUTTON_X2:
					args.Button = MouseButton.Extra2;
					break;
				default:
					args.Button = MouseButton.None;
					break;
			}

			args.Double = e.button.clicks > 1;

			switch (e.button.type)
			{
				case SDL_EventType.SDL_MOUSEBUTTONDOWN:
					OnMouseDown(args);
					break;
				case SDL_EventType.SDL_MOUSEBUTTONUP:
					OnMouseUp(args);
					break;
			}
		}

		private void OnMouseMotionEvent(SDL_Event e)
		{
			MouseEventArgs args = new MouseEventArgs();

			args.Position = new Point(
				e.motion.x,
				e.motion.y);

			args.Delta = new Point(
				e.motion.xrel,
				e.motion.yrel);

			OnMouseMove(args);
		}

		protected void OnMouseWheelEvent(SDL_Event e)
		{
			MouseEventArgs args = new MouseEventArgs();

			int m =
				e.wheel.direction ==
				(uint)SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED ?
				-1 : 1;

			args.Wheel = new Point(
				e.wheel.x * m,
				e.wheel.y * m);

			OnMouseWheel(args);
		}

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (Renderer != null)
					{
						Renderer.Dispose();
						Renderer = null;
					}
				}

				if (Handle != IntPtr.Zero)
				{
					SDL_DestroyWindow(Handle);
					Handle = IntPtr.Zero;
				}

				disposedValue = true;
			}
		}

		 ~Window()
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
