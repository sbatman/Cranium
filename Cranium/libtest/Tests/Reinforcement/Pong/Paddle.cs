// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
	internal class Paddle : IDisposable
	{
		private readonly Single _X;
		private Arena _ParentArena;
		private Single _Y;
		public Single HalfHeight;
		public Single Height;

		public Action OnHit;

		public Single X => _X;

		public Single Y
		{
			get => _Y;
			set
			{
				_Y = value;
				if (Y < HalfHeight) Y = HalfHeight;
				if (Y > _ParentArena.Height - HalfHeight) Y = _ParentArena.Height - HalfHeight;
			}
		}

		public Paddle(Arena parentArena, Single startX, Single startY, Single height)
		{
			Height = height;
			HalfHeight = Height / 2;

			_ParentArena = parentArena;
			_X = startX;
			_Y = startY + HalfHeight;
		}

		public void Dispose()
		{
			_ParentArena = null;
		}

		public void Move(Single distance)
		{
			Y += distance;
		}

		public void TriggerOnHit()
		{
			OnHit?.Invoke();
		}
	}
}