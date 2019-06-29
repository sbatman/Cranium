// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
	internal class Ball : IDisposable
	{
		private Arena _ParentArena;
		private Single _XVelo;
		private Single _YVelo;

		public Single X { get; private set; }

		public Single Y { get; private set; }

		public Ball(Arena parentArena, Single spawnX, Single spawnY, Single xVelo, Single yVelo)
		{
			_ParentArena = parentArena;
			X = spawnX;
			Y = spawnY;
			_XVelo = xVelo;
			_YVelo = yVelo;
		}

		public virtual void Dispose()
		{
			_ParentArena = null;
		}

		public virtual void Update()
		{
			if (!InBounds()) return;

			if (X + _XVelo <= 0 && Y > _ParentArena.LeftPaddle.Y - _ParentArena.LeftPaddle.HalfHeight && Y < _ParentArena.LeftPaddle.Y + _ParentArena.LeftPaddle.HalfHeight)
			{
				_XVelo = -_XVelo;
				_ParentArena.LeftPaddle.TriggerOnHit();
				return;
			}

			if (X + _XVelo >= _ParentArena.Width && Y > _ParentArena.RightPaddle.Y - _ParentArena.RightPaddle.HalfHeight && Y < _ParentArena.RightPaddle.Y + _ParentArena.RightPaddle.HalfHeight)
			{
				_XVelo = -_XVelo;
				_ParentArena.RightPaddle.TriggerOnHit();
				return;
			}

			if (Y + _YVelo <= 0 || Y + _YVelo > _ParentArena.Height) _YVelo = -+_YVelo;

			X += _XVelo;
			Y += _YVelo;
		}

		public virtual Boolean InBounds()
		{
			return X >= 0 && X <= _ParentArena.Width;
		}
	}
}