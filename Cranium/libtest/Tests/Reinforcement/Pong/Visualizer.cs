using System;
using System.Windows.Forms;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    public partial class Visualizer : Form
    {
        public Action ResetEvent;
        private Arena _Arena;

        internal Visualizer(Arena arena)
        {
            InitializeComponent();
            Height = (Int32)arena.Height;
            Width = (Int32)arena.Width + 10;
            SetClientSizeCore(Width,Height);

            LPaddle.Height = (Int32)arena.LeftPaddle.Height;
            LPaddle.Width = 10;

            RPaddle.Height = (Int32)arena.RightPaddle.Height;
            RPaddle.Width = 10;

            _Arena = arena;
        }

        private void Visualizer_Load(Object sender, EventArgs e)
        {

        }

        public void SetBallPosition(Single x, Single y)
        {
            Ball.Left = (Int32)x;
            Ball.Top = (Int32)y;
        }

        public void SetLPaddlePosition(Single x, Single y)
        {
            LPaddle.Left = (Int32)x;
            LPaddle.Top = (Int32)(y - _Arena.LeftPaddle.HalfHeight);
        }

        public void SetRPaddlePosition(Single x, Single y)
        {
            RPaddle.Left = (Int32)x;
            RPaddle.Top = (Int32)(y - _Arena.RightPaddle.HalfHeight);
        }

        private void ResetButton_Click(Object sender, EventArgs e)
        {
            ResetEvent?.Invoke();
        }
    }
}
