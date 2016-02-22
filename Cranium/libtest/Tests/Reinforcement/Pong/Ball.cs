using System;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    internal class Ball : IDisposable
    {
        private Arena _ParentArena;
        private Single _X;
        private Single _Y;
        private Single _XVelo;
        private Single _YVelo;

        public Single X => _X;
        public Single Y => _Y;

        public Ball(Arena parentArena, Single spawnX, Single spawnY, Single xVelo, Single yVelo)
        {
            _ParentArena = parentArena;
            _X = spawnX;
            _Y = spawnY;
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

            if ((_X+ _XVelo) <= 0 && _Y > _ParentArena.LeftPaddle.Y - _ParentArena.LeftPaddle.HalfHeight && _Y < _ParentArena.LeftPaddle.Y + _ParentArena.LeftPaddle.HalfHeight)
            {
                _XVelo = -_XVelo;
                _ParentArena.LeftPaddle.TriggerOnHit();
                return;
            }
            if ((_X + _XVelo) >= _ParentArena.Width && _Y > _ParentArena.RightPaddle.Y - _ParentArena.RightPaddle.HalfHeight && _Y < _ParentArena.RightPaddle.Y + _ParentArena.RightPaddle.HalfHeight)
            {
                _XVelo = -_XVelo;
                _ParentArena.RightPaddle.TriggerOnHit();
                return;
            }

            if (_Y + _YVelo <= 0 || _Y + _YVelo > _ParentArena.Height) _YVelo = -+_YVelo;

            _X += _XVelo;
            _Y += _YVelo;
        }

        public virtual Boolean InBounds()
        {
            return _X >= 0 && _X <= _ParentArena.Width;
        }
    }
}
