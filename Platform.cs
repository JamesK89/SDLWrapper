using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	public static class Platform
	{
		public static string Name =>
			SDL_GetPlatform();

		public static int Memory =>
			SDL_GetSystemRAM();

		public static class CPU
		{
			public static int Count =>
				SDL_GetCPUCount();

			public static int L1CacheSize =>
				SDL_GetCPUCacheLineSize();

			public static bool Has3DNow =>
				SDL_Has3DNow() == SDL_bool.SDL_TRUE;

			public static bool HasMMX =>
				SDL_HasMMX() == SDL_bool.SDL_TRUE;

			public static bool HasSSE =>
				SDL_HasSSE() == SDL_bool.SDL_TRUE;
			public static bool HasSSE2 =>
				SDL_HasSSE2() == SDL_bool.SDL_TRUE;
			public static bool HasSSE3 =>
				SDL_HasSSE3() == SDL_bool.SDL_TRUE;
			public static bool HasSSE41 =>
				SDL_HasSSE41() == SDL_bool.SDL_TRUE;
			public static bool HasSSE42 =>
				SDL_HasSSE42() == SDL_bool.SDL_TRUE;

			public static bool HasAVX =>
				SDL_HasAVX() == SDL_bool.SDL_TRUE;
			public static bool HasAVX2 =>
				SDL_HasAVX2() == SDL_bool.SDL_TRUE;
			public static bool HasAVX512F =>
				SDL_HasAVX512F() == SDL_bool.SDL_TRUE;

			public static bool HasAltiVec =>
				SDL_HasAltiVec() == SDL_bool.SDL_TRUE;

			public static bool HasRDTSC =>
				SDL_HasRDTSC() == SDL_bool.SDL_TRUE;
			
			public static bool HasNEON =>
				SDL_HasNEON() == SDL_bool.SDL_TRUE;
		}
	}
}
