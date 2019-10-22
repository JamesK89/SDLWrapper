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
	public class SDLException : Exception
	{
		public SDLException()
			: base(SDL_GetError())
		{
			SDL_ClearError();
		}
	}
}
