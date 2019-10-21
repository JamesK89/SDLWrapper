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
	public static class Time
	{
		public static uint Ticks
		{
			get
			{
				return SDL_GetTicks();
			}
		}
		
		public static ulong Counter
		{
			get
			{
				return SDL_GetPerformanceCounter();
			}
		}

		public static ulong Frequency
		{
			get
			{
				return SDL_GetPerformanceFrequency();
			}
		}

		public static bool TicksPassed(uint ticks1, uint ticks2)
		{
			return SDL_TICKS_PASSED(ticks1, Ticks);
		}

		public static void Delay(uint milliseconds)
		{
			SDL_Delay(milliseconds);
		}
	}
}
