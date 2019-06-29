// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    internal class Test
    {
        private readonly Arena _Arena;
        private readonly Random _RND = new Random();
        private RecurrentContext _ContextLayer;
        private List<BaseNode> _InputLayerNodes;
        private Int32 _InputNodeCount;
        private Network _NeuralNetwork;
        private List<BaseNode> _OutputLayerNodes;
        private Double _RollingAveragePerformance = -1;

        private Int32 _Streak;

        private Boolean _TeachingEnabled = true;
        private Int32 _TimesHit;

        private Int32 _TimesMissed;
        private Visualizer _Visualizer;
        public Int32 Epoch => _TimesHit + _TimesMissed;

        private int _ConsoleSkip = 100;

        public Test()
        {
            _Arena = new Arena(800, 300, new[] { 100.0f, 150.0f });
            _Arena.LeftPaddle.OnHit += OnPaddleHit;
            Reset();
            GenerateNeuralNetwork();
        }

        private List<Double> GetEnvironmentState()
        {
            return new List<Double>
            {
                (_Arena.LeftPaddle.Y * 2)-_Arena.Height / _Arena.Height,
                _Arena.Ball.X / _Arena.Width,
	            (_Arena.Ball.Y * 2)-_Arena.Height / _Arena.Height
            };
        }

        private List<ActionInstance> GenerateActionOptions()
        {
            List<ActionInstance> returnList = new List<ActionInstance>();
            List<Double> envState = GetEnvironmentState();

            foreach (PossibleActions pa in Enum.GetValues(typeof(PossibleActions)))
            {
                returnList.Add(new ActionInstance
                {
                    Action = pa,
                    EnvironmentState = envState
                });
            }

            return returnList;
        }

        private void GenerateNeuralNetwork()
        {
            _NeuralNetwork?.Dispose();
            _NeuralNetwork = new Network { LearningRate = 0.06f, Momentum = 0.6f };
            _InputNodeCount = GetEnvironmentState().Count + Enum.GetValues(typeof(PossibleActions)).Length;

            Layer inputLayer = new Layer();
            _InputLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < _InputNodeCount; i++) _InputLayerNodes.Add(new BaseNode(inputLayer, new LinearAF()));
            inputLayer.SetNodes(_InputLayerNodes);

            Layer hiddenLayer = new Layer();
            List<BaseNode> hiddenLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < 10; i++) hiddenLayerNodes.Add(new BaseNode(hiddenLayer, new TanhAF()));
            hiddenLayer.SetNodes(hiddenLayerNodes);

            _ContextLayer = new RecurrentContext(8, new TanhAF());

            Layer outputLayer = new Layer();
            _OutputLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < 1; i++) _OutputLayerNodes.Add(new OutputNode(outputLayer, new LinearAF()));
            outputLayer.SetNodes(_OutputLayerNodes);

            _ContextLayer.AddSourceNodes(_InputLayerNodes);
            _ContextLayer.AddSourceNodes(_OutputLayerNodes);
            //  _ContextLayer.AddSourceNodes(hiddenLayerNodes);

            inputLayer.ConnectForwardLayer(hiddenLayer);
            hiddenLayer.ConnectForwardLayer(outputLayer);
            _ContextLayer.ConnectForwardLayer(hiddenLayer);


            _NeuralNetwork.AddLayer(inputLayer);
            _NeuralNetwork.AddLayer(hiddenLayer);
            _NeuralNetwork.AddLayer(_ContextLayer);
            _NeuralNetwork.AddLayer(outputLayer);

            foreach (Layer layer in _NeuralNetwork.GetCurrentLayers()) layer.PopulateNodeConnections();
            _NeuralNetwork.RandomiseWeights(0.2f);
        }

        private List<ActionInstance> TestActionOptions()
        {
            List<ActionInstance> returnList = new List<ActionInstance>();
            foreach (ActionInstance actionInstance in GenerateActionOptions())
            {
                PresentActionInstance(actionInstance);
                actionInstance.Value = _OutputLayerNodes[0].GetValue();
                returnList.Add(actionInstance);
            }

            return returnList;
        }

        private void PresentActionInstance(ActionInstance actionInstance)
        {
            Int32 node = 0;
            foreach (Double variable in actionInstance.EnvironmentState)
            {
                _InputLayerNodes[node++].SetValue(variable);
            }

            foreach (PossibleActions pa in Enum.GetValues(typeof(PossibleActions)))
            {
                _InputLayerNodes[node++].SetValue(pa == actionInstance.Action ? 1 : 0);
            }

            _NeuralNetwork.FowardPass();
        }

        public void Update()
        {
            TestActionOptions();

            if (!_TeachingEnabled)
            {
                Console.Clear();
                Console.WriteLine($"Hit :\t\t{_TimesHit}");
                Console.WriteLine($"Missed :\t{_TimesMissed}");
                Console.WriteLine($"Performance :\t{Math.Round(_RollingAveragePerformance, 3)}");
                Console.WriteLine($"Streak :\t{_Streak}");
                Console.WriteLine("------");
                Console.WriteLine("Left \t{0},\t{1}", _Arena.LeftPaddle.X, _Arena.LeftPaddle.Y);
                Console.WriteLine("Right \t{0},\t{1}", _Arena.RightPaddle.X, _Arena.RightPaddle.Y);
                Console.WriteLine("Ball \t{0},\t{1}", _Arena.Ball.X, _Arena.Ball.Y);
                Console.WriteLine("-------");
                _Visualizer?.SetBallPosition(_Arena.Ball.X, _Arena.Ball.Y);
                _Visualizer?.SetLPaddlePosition(_Arena.LeftPaddle.X, _Arena.LeftPaddle.Y);
                _Visualizer?.SetRPaddlePosition(_Arena.RightPaddle.X, _Arena.RightPaddle.Y);
            }

            List<ActionInstance> weightedOptions = TestActionOptions();
            if (!_TeachingEnabled)
            {
                foreach (ActionInstance action in weightedOptions)
                {
                    Console.WriteLine($"{action.Action} \t {action.Value}");
                }
            }

            ActionInstance actionToPerform;


            Double chanceRatio = 1.0 / (Epoch / 1000.0);

            if (_RND.NextDouble() < chanceRatio) // lets explore rather than exploit
            {
                //Todo: Check this is only happening on the first pass, then policy from there onwards. :D
                actionToPerform = weightedOptions[_RND.Next(weightedOptions.Count)]; //random pick if we are attempting to explore this round
            }
            else
            {
                actionToPerform = weightedOptions.OrderByDescending(a => a.Value).ToList()[0];
            }


            switch (actionToPerform.Action)
            {
                case PossibleActions.MOVE_UP:
                    _Arena.LeftPaddle.Y -= 20;
                    break;
                case PossibleActions.MOVE_DOWN:
                    _Arena.LeftPaddle.Y += 20;
                    break;
                case PossibleActions.HOLD:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PresentActionInstance(actionToPerform);
            _ContextLayer.UpdateExtra();

            _Arena.RightPaddle.Y = _Arena.Ball.Y;

            _Arena.Update();

            if (!_Arena.Ball.InBounds())
            {
                Teach(-(Math.Abs(_Arena.LeftPaddle.Y - _Arena.Ball.Y) / _Arena.Height));
                _TimesMissed++;
                _Streak = 0;
            }

            if (_Visualizer != null) Application.DoEvents();
            if (!_TeachingEnabled) Thread.Sleep(33);
        }

        public void Teach(Double value)
        {
            if (_TeachingEnabled)
            {
                if (_RollingAveragePerformance > 0.5f)
                {
                    _NeuralNetwork.LearningRate = 0.04f;
                }

                if (_RollingAveragePerformance > 0.7f)
                {
                    _NeuralNetwork.LearningRate = 0.02f;
                }

                if (_RollingAveragePerformance > 0.9f)
                {
                    _NeuralNetwork.LearningRate = 0.01f;
                }

                if (_RollingAveragePerformance <= 0.5f)
                {
                    _NeuralNetwork.LearningRate = 0.08f;
                }

                if (_RollingAveragePerformance > 0.85f)
                {
                    _TeachingEnabled = false;
                    _Visualizer = new Visualizer(_Arena);
                    _Visualizer.Show();
                    _Visualizer.ResetEvent += Reset;
                }

                ((OutputNode)_OutputLayerNodes[0]).SetTargetValue(value);
                _NeuralNetwork.ReversePass();
                foreach (BaseNode node in _ContextLayer.GetNodes()) node.SetValue(0);
            }


            _RollingAveragePerformance = _RollingAveragePerformance * 0.98 + value * 0.02;
            Reset();
        }

        public void Reset()
        {
            _Arena.SpawnBall(-25f, ((Single)_RND.NextDouble() - 0.5f) * 40);
            _Arena.LeftPaddle.Y = (Single)_RND.NextDouble() * _Arena.Height;
            if (_TeachingEnabled && _ConsoleSkip-- <0)
            {
	            _ConsoleSkip = 100;
                Console.Clear();
                Console.WriteLine($"Hit :\t\t{_TimesHit}");
                Console.WriteLine($"Missed :\t{_TimesMissed}");
                Console.WriteLine($"Performance :\t{Math.Round(_RollingAveragePerformance, 3)}");
            }
        }

        public void OnPaddleHit()
        {
            _Streak++;
            Single accuracy = 1 - Math.Abs((_Arena.LeftPaddle.Y - _Arena.Ball.Y) / 250);
            accuracy = ((accuracy + 1) * 0.5f);
            if (_TeachingEnabled) Teach(accuracy);
            _TimesHit++;
        }

        public static void Run()
        {
            Console.Clear();

            Console.WriteLine("The network will now proceed to use reinforcement learning to teach its self how to play pong, once complete further information will be available");
            Console.WriteLine("Press any key to begin");
            Console.ReadKey();
            Test t = new Test();
            while (true)
            {
                t.Update();
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private enum PossibleActions
        {
            MOVE_UP,
            MOVE_DOWN,
            HOLD
        }

        private class ActionInstance
        {
            public PossibleActions Action;
            public List<Double> EnvironmentState;
            public Double Value;
        }
    }
}