using System;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    internal class Paddle : IDisposable
    {
        public Single Height;
        public Single HalfHeight;

        private readonly Single _X;
        private Single _Y;
        private Arena _ParentArena;

        public Single X => _X;

        public Action OnHit;

        public Single Y
        {
            get { return _Y; }
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

        public void Move(Single distance)
        {
            Y += distance;
        }

        public void Dispose()
        {
            _ParentArena = null;
        }

        public void TriggerOnHit()
        {
            OnHit?.Invoke();
        }
    }
}
