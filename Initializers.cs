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
	internal static class Initializers
	{
		public static bool IsGlobalInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeGlobal()
		{
			if (!IsGlobalInitialized)
			{
				SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

				IsGlobalInitialized = true;
			}
		}

		public static bool IsEventsInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeEvents()
		{
			if (!IsEventsInitialized)
			{
				IsEventsInitialized =
					Initialize(SDL_INIT_EVENTS);
			}
		}

		public static bool IsJoystickInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeJoystick()
		{
			if (!IsJoystickInitialized)
			{
				IsJoystickInitialized =
					Initialize(SDL_INIT_JOYSTICK);
			}
		}

		public static bool IsHapticInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeHaptic()
		{
			if (!IsHapticInitialized)
			{
				IsHapticInitialized =
					Initialize(SDL_INIT_HAPTIC);
			}
		}

		public static bool IsControllerInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeController()
		{
			if (!IsControllerInitialized)
			{
				IsControllerInitialized =
					Initialize(SDL_INIT_GAMECONTROLLER);
			}
		}

		public static bool IsSensorInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeSensor()
		{
			if (!IsSensorInitialized)
			{
				IsSensorInitialized =
					Initialize(SDL_INIT_GAMECONTROLLER);
			}
		}

		public static bool IsVideoInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeVideo()
		{
			if (!IsVideoInitialized)
			{
				IsVideoInitialized =
					Initialize(SDL_INIT_VIDEO);
			}
		}

		public static bool IsTimerInitialized
		{
			get;
			private set;
		} = false;

		public static void InitializeTimer()
		{
			if (!IsTimerInitialized)
			{
				IsTimerInitialized =
					Initialize(SDL_INIT_TIMER);
			}
		}

		private static bool Initialize(uint flags)
		{
			InitializeGlobal();

			if (SDL_InitSubSystem(flags) != 0)
			{
				throw new SDLException();
			}

			return true;
		}
	}
}
