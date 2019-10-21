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
	public class Timer : IDisposable
	{
		private uint _interval;
		SDL_TimerCallback _callback;
		GCHandle _gcHandle;

		public delegate void TickEventHandler(object sender, EventArgs e);

		public event TickEventHandler Tick;

		public Timer(uint interval)
		{
			_interval = interval;
			_callback = new SDL_TimerCallback(TickCallback);
			_gcHandle = GCHandle.Alloc(_callback);

			if (_interval > 0)
			{
				Handle = new IntPtr(
					SDL_AddTimer(interval, _callback, IntPtr.Zero));

				if (Handle == IntPtr.Zero)
				{
					throw new SDLException();
				}
			}
		}

		public static uint Ticks
		{
			get
			{
				return SDL_GetTicks();
			}
		}

		public static void Delay(uint milliseconds)
		{
			SDL_Delay(milliseconds);
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		public uint Interval
		{
			get
			{
				return _interval;
			}
			set
			{
				if (value != _interval)
				{
					if (Handle != IntPtr.Zero)
					{
						SDL_RemoveTimer(Handle.ToInt32());
						Handle = IntPtr.Zero;
					}

					if (value > 0)
					{
						Handle = new IntPtr(
							SDL_AddTimer(value, _callback, IntPtr.Zero));

						if (Handle == IntPtr.Zero)
						{
							throw new SDLException();
						}
					}
				}
			}
		}

		protected virtual void OnTick(TimerEventArgs e)
		{
			Tick?.Invoke(this, e);
		}

		private uint TickCallback(uint interval, IntPtr param)
		{
			TimerEventArgs e = new TimerEventArgs();
			e.Interval = Interval;

			OnTick(e);
			
			return (_interval = e.Interval);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (Handle != IntPtr.Zero)
				{
					SDL_RemoveTimer(Handle.ToInt32());
					Handle = IntPtr.Zero;
				}
				
				if (_callback != null)
				{
					_gcHandle.Free();
					_callback = null;
				}

				disposedValue = true;
			}
		}

		~Timer()
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
