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
	public class Starfield
	{
		private static Random _rand;
		private List<PointF> _stars;

		public Starfield(
			Rectangle bounds,
			int numStars,
			float speed,
			Color color)
		{
			if (_rand == null)
			{
				_rand = new Random();
			}
			
			Bounds = bounds;
			Speed = speed;
			Color = color;

			InitializeStars(numStars);
		}

		public Rectangle Bounds
		{
			get;
			set;
		}

		public float Speed
		{
			get;
			set;
		}

		public int Count
		{
			get;
			private set;
		}

		public IEnumerable<PointF> Stars
		{
			get
			{
				return _stars;
			}
		}

		public Color Color
		{
			get;
			set;
		}

		private void InitializeStars(int count)
		{
			if (_stars == null)
			{
				_stars = new List<PointF>(count);
			}

			_stars.Clear();

			for (int i = 0; i < count; i++)
			{
				PointF star = GetRandomStarStartingPoint(false);
				_stars.Add(star);
			}
		}

		private PointF GetRandomStarStartingPoint(bool outsideBounds = true)
		{
			PointF result = new PointF();

			result.X = (_rand.Next() % Bounds.Right) + 
				(outsideBounds ? Bounds.Right : 0);
			result.Y = (_rand.Next() % Bounds.Bottom) + Bounds.Top;

			return result;
		}

		public void Update(float delta)
		{
			for (int i = 0; i < _stars.Count; i++)
			{
				UpdateStar(delta, i);
			}
		}

		private void UpdateStar(float delta, int idx)
		{
			PointF star = _stars[idx];

			star.X -= (Speed * delta);

			if ((int)star.X < 0)
			{
				star = GetRandomStarStartingPoint();
			}

			_stars[idx] = star;
		}

		public void Draw(Renderer renderer)
		{
			renderer.Color = Color;
			renderer.Point(Stars);
		}
	}
}
