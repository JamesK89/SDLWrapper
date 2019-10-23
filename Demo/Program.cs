using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SDL2;
using static SDL2.SDL;
using SDLWrapper;

namespace SDLWrapper.Demo
{
	public sealed class Program
	{
		[STAThread]
		private static int Main(string[] args)
		{
			int result = 0;

			try
			{
				result = (new Program()).Run();
			}
			catch (Exception ex)
			{
				result = 1;
				Console.Error.WriteLine(ex.ToString());
			}

			return result;
		}

		private Program()
		{
		}

		public int Result
		{
			get;
			set;
		}

		public bool Quit
		{
			get;
			set;
		}

		public wndMain MainWindow
		{
			get;
			private set;
		}

		private int Run()
		{
			Result = 0;
			Quit = false;
			MainWindow = new wndMain(this);

			while (!Quit)
			{
				while (SDL_PollEvent(out SDL_Event e) > 0)
				{
					if (!MainWindow.DoEvent(e))
					{
						if (e.type == SDL_EventType.SDL_QUIT)
						{
							Quit = true;
						}
					}
				}

				MainWindow.Draw();
			}

			return Result;
		}
	}
}
