using System;
using System.Collections.Generic;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    internal class Arena : IDisposable
    {
        private Random _RND = new Random();

        public Single Height { get; }
        public Single Width { get; }
        public Paddle LeftPaddle { get; private set; }
        public Paddle RightPaddle { get; private set; }
        public Ball Ball { get; private set; }

        public Arena(Single width, Single height, IReadOnlyList<Single> paddleHeights)
        {
            Height = height;
            Width = width;

            LeftPaddle = new Paddle(this, 0, 0, paddleHeights[0]);
            RightPaddle = new Paddle(this, width, 0, paddleHeights[1]);
        }

        public void SpawnBall(Single xVelo, Single yVelo)
        {
            Ball?.Dispose();
            Ball = new Ball(this, Width * 0.9f, (Single)(_RND.NextDouble() * Height), xVelo, yVelo);
        }

        public virtual void Update()
        {
            Ball?.Update();
        }

        public void Dispose()
        {
            LeftPaddle?.Dispose();
            LeftPaddle = null;
            RightPaddle?.Dispose();
            RightPaddle = null;
            Ball?.Dispose();
            Ball = null;
            _RND = null;
        }
    }
}
