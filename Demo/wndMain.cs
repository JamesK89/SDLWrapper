using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SDL2;
using static SDL2.SDL;
using SDLWrapper;

namespace SDLWrapper.Demo
{
	public class wndMain : Window
	{
		private Starfield[] _starfieds =
			new Starfield[4];

		public wndMain(Program program)
			: base()
		{
			Program = program;
			Size = new Size(800, 600);
			IsResizable = false;
			Title = "Demo";
			Background = Color.Black;
			InitStarfields();
			Show();
		}

		private void InitStarfields()
		{
			Rectangle bounds = new Rectangle(
				Point.Empty, Size);

			_starfieds[0] = new Starfield(
				bounds,
				200,
				0.25f,
				Color.DarkGray);
			
			_starfieds[1] = new Starfield(
				bounds,
				150,
				0.50f,
				Color.Gray);
			
			_starfieds[2] = new Starfield(
				bounds,
				100,
				0.75f,
				Color.LightGray);
			
			_starfieds[3] = new Starfield(
				bounds,
				50,
				1.00f,
				Color.White);
		}

		public Color Background
		{
			get;
			set;
		}

		public Program Program
		{
			get;
			private set;
		}

		protected override void OnClose()
		{
			Program.Quit = true;
			base.OnClose();
		}

		public void Draw()
		{
			Renderer.Color = Background;
			Renderer.Clear();

			for (int i = 0; i < _starfieds.Length; i++)
			{
				_starfieds[i].Update(1.0f);
				_starfieds[i].Draw(Renderer);
			}

			Renderer.Present();
		}
	}
}
