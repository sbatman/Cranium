using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using Cranium.Lib.Genetics;
using Cranium.Lib.Genetics.Genes;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    internal class TestGenetics
    {

        private static List<DNA> _DNAProfilesToRun = new List<DNA>();
        private static List<DNA> _DNAScores = new List<DNA>();
        private static StreamWriter _OutputStream;

        private enum PossibleActions
        {
            MOVEUP,
            MOVEDOWN,
            HOLD
        }

        private class ActionInstance
        {
            public PossibleActions Action;
            public List<Double> EnvironmentState;
            public Double Value;
        }

        private readonly Arena _Arena;
        private Network _NeuralNetwork;
        private List<BaseNode> _InputLayerNodes;
        private List<BaseNode> _OuputLayerNodes;
        private RecurrentContext _ContextLayer;
        private Int32 _InputNodeCount;
        private Random _RND = new Random();

        private Int32 _TimesMissed;
        private Int32 _TimesHit;
        public Int32 Epoch => _TimesHit + _TimesMissed;
        private Double _RollingAveragePerformance = -1;

        private Boolean _TeachingEnabled = true;
        private Boolean _InitialPass;

        private Int32 _Streak;
        private Visualizer _Visualizer;

        public DNA DNAProfile;

        public TestGenetics(DNA dnaProfile)
        {
            DNAProfile = dnaProfile;
            _Arena = new Arena(1200, 600, new[] { 100.0f, 150.0f });
            _Arena.LeftPaddle.OnHit += OnPaddleHit;
            Reset();
            GenerateNeuralNetwork();
        }

        private List<Double> GetEnvironmentState()
        {
            return new List<Double>
            {
                _Arena.LeftPaddle.Y / _Arena.Height,
                _Arena.Ball.X / _Arena.Width,
                _Arena.Ball.Y / _Arena.Height
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
            _NeuralNetwork = new Network();
            _NeuralNetwork.LearningRate = ((SingleGene)DNAProfile.GetGene("LearningRate")).CurrentValue;
            _NeuralNetwork.Momentum = 0.0;
            _InputNodeCount = GetEnvironmentState().Count + Enum.GetValues(typeof(PossibleActions)).Length;

            Layer inputLayer = new Layer();
            _InputLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < _InputNodeCount; i++) _InputLayerNodes.Add(new BaseNode(inputLayer, new LinearAF()));
            inputLayer.SetNodes(_InputLayerNodes);

            Layer hiddenLayer = new Layer();
            List<BaseNode> hiddenLayerNodes = new List<BaseNode>();

            AF hlActivationFunction = null;
            AF conActivationFunction = null;

            if (((BooleanGene)DNAProfile.GetGene("AFHLTan")).CurrentValue) hlActivationFunction = new TanhAF(); else hlActivationFunction = new LinearAF();
            if (((BooleanGene)DNAProfile.GetGene("AFCONTan")).CurrentValue) conActivationFunction = new TanhAF(); else conActivationFunction = new LinearAF();

            for (Int32 i = 0; i < ((Int32Gene)DNAProfile.GetGene("HiddenNodeCount")).CurrentValue; i++) hiddenLayerNodes.Add(new BaseNode(hiddenLayer, hlActivationFunction));
            hiddenLayer.SetNodes(hiddenLayerNodes);

            _ContextLayer = new RecurrentContext(((Int32Gene)DNAProfile.GetGene("Recurrency")).CurrentValue, conActivationFunction);

            Layer outputLayer = new Layer();
            _OuputLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < 1; i++) _OuputLayerNodes.Add(new OutputNode(outputLayer, new LinearAF()));
            outputLayer.SetNodes(_OuputLayerNodes);

            _ContextLayer.AddSourceNodes(_InputLayerNodes);
            if (((BooleanGene)DNAProfile.GetGene("ContextConnectOutputLayers")).CurrentValue) _ContextLayer.AddSourceNodes(_OuputLayerNodes);
            if (((BooleanGene)DNAProfile.GetGene("ContextConnectHiddenLayers")).CurrentValue) _ContextLayer.AddSourceNodes(hiddenLayerNodes);

            inputLayer.ConnectFowardLayer(hiddenLayer);
            hiddenLayer.ConnectFowardLayer(outputLayer);
            _ContextLayer.ConnectFowardLayer(hiddenLayer);


            _NeuralNetwork.AddLayer(inputLayer);
            _NeuralNetwork.AddLayer(hiddenLayer);
            _NeuralNetwork.AddLayer(_ContextLayer);
            _NeuralNetwork.AddLayer(outputLayer);

            foreach (Layer layer in _NeuralNetwork.GetCurrentLayers()) layer.PopulateNodeConnections();
            _NeuralNetwork.RandomiseWeights(((SingleGene)DNAProfile.GetGene("RandomWeight")).CurrentValue);
        }

        private List<ActionInstance> TestActionOptions()
        {
            List<ActionInstance> returnList = new List<ActionInstance>();
            foreach (ActionInstance actionInstance in GenerateActionOptions())
            {
                PresentActionInstance(actionInstance);
                actionInstance.Value = _OuputLayerNodes[0].GetValue();
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
                Console.WriteLine($"Streak :\t{ _Streak}");
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
                case PossibleActions.MOVEUP:
                    _Arena.LeftPaddle.Y -= 20;
                    break;
                case PossibleActions.MOVEDOWN:
                    _Arena.LeftPaddle.Y += 20;
                    break;
                    case PossibleActions.HOLD:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PresentActionInstance(actionToPerform);
            _ContextLayer.UpdateExtra();

            _InitialPass = false;

            _Arena.RightPaddle.Y = _Arena.Ball.Y;

            _Arena.Update();

            if (!_Arena.Ball.InBounds())
            {
                Teach(-(Math.Abs(_Arena.LeftPaddle.Y - _Arena.Ball.Y) / _Arena.Height));
                Reset();
                _TimesMissed++;
                _Streak = 0;
            }
            if (_Visualizer != null) Application.DoEvents();
            if (!_TeachingEnabled) Thread.Sleep(33);
        }

        public void Teach(Double value)
        {

            (_OuputLayerNodes[0] as OutputNode).SetTargetValue(value);
            _NeuralNetwork.ReversePass();
            foreach (BaseNode node in _ContextLayer.GetNodes()) node.SetValue(0);


            _RollingAveragePerformance = (_RollingAveragePerformance * 0.98) + (value * 0.02);
            Reset();
        }

        public void ShowVisualizer()
        {
            _Visualizer = new Visualizer(_Arena);
            _Visualizer.Show();
            _Visualizer.ResetEvent += Reset;
        }

        public void Reset()
        {
            _InitialPass = true;
            _Arena.SpawnBall(-25f, ((Single)_RND.NextDouble() - 0.5f) * 40);
            _Arena.LeftPaddle.Y = (Single)_RND.NextDouble() * _Arena.Height;
            if (_TeachingEnabled)
            {
                //  Console.Clear();
                //   Console.WriteLine($"Hit :\t\t{_TimesHit}");
                //   Console.WriteLine($"Missed :\t{_TimesMissed}");
                //   Console.WriteLine($"Performance :\t{Math.Round(_RollingAveragePerformance, 3)}");
            }
        }

        public void OnPaddleHit()
        {
            _Streak++;
            Single accuracy = 1 - Math.Abs((_Arena.LeftPaddle.Y - _Arena.Ball.Y) / 250);
            if (_TeachingEnabled) Teach(accuracy);
            _TimesHit++;
        }

        public static void Run()
        {
            Console.Clear();
            _OutputStream = File.CreateText("Output.txt");

            DNA baseDNAProfile = new DNA(new List<Gene>()
            {
                new SingleGene("RandomWeight",0.2f,0.01f,1f),
                new SingleGene("LearningRate",0.02f,0.02f,0.8f),
                 new Int32Gene("HiddenNodeCount",5,1,30),
                  new Int32Gene("Recurrency",3,1,10),
                  new BooleanGene("ContextConnectOutputLayers",false),
                        new BooleanGene("ContextConnectHiddenLayers",false),
                        new BooleanGene("AFHLTan",true),
                        new BooleanGene("AFCONTan",true)
            });

            for (Int32 i = 0; i < 60; i++)
            {
                DNA profile = baseDNAProfile.Copy();
                profile.Mutate(1.0f);
                _DNAProfilesToRun.Add(profile);
            }

            for (int generation = 0; generation < 10; generation++)
            {
                Console.WriteLine($"Starting generation {generation}");
                List<Thread> activeThreads = new List<Thread>();
                for (Int32 i = 0; i < Environment.ProcessorCount; i++)
                {
                    Thread t = new Thread(DNARunnerThread);
                    activeThreads.Add(t);
                    t.Start();
                }

                while (activeThreads.Any(a => a.ThreadState != ThreadState.Stopped))
                {
                    Thread.Sleep(10);
                }

                _DNAScores = _DNAScores.OrderByDescending(a => a.Score).ToList();
                List<DNA> chosenDNA = _DNAScores.Take(10).Select(a => a.Copy()).ToList();

                foreach (DNA dna in _DNAScores.Take(10).Select(a => a.Copy()))
                {
                    dna.Mutate(0.5f);
                    chosenDNA.Add(dna);
                }

                foreach (DNA dna in _DNAScores.Take(8).Select(a => a.Copy()))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        DNA t = dna.Copy();
                        t.Cross(_DNAScores[i]);
                        chosenDNA.Add(t);
                    }
                }
                _DNAProfilesToRun.AddRange(chosenDNA);
                Console.WriteLine($"Generation {generation} complete");
                Console.WriteLine("Top 5 Performers");
                foreach (DNA dna in _DNAScores.OrderByDescending(a => a.Score).Take(5))
                {
                    Console.WriteLine($"{dna.ToString()} Score : {dna.Score}");
                }
            }
            _OutputStream.Flush();
            _OutputStream.Dispose();
            _OutputStream = null;
            Console.WriteLine("Completed");


            Console.ReadKey();
            Console.ReadKey();

        }

        private static void DNARunnerThread()
        {
            while (_DNAProfilesToRun.Count > 0)
            {
                Monitor.Enter(_DNAProfilesToRun);
                DNA currentProfile = _DNAProfilesToRun[0];
                _DNAProfilesToRun.RemoveAt(0);
                Monitor.Exit(_DNAProfilesToRun);

                List<Single> scores = new List<Single>();
                for (Int32 i = 0; i < 4; i++)
                {
                    TestGenetics t = new TestGenetics(currentProfile);
                    while (t.Epoch < 10000)
                    {
                        t.Update();
                    }
                    scores.Add((Single)t._RollingAveragePerformance);
                }
                Monitor.Enter(_DNAScores);
                currentProfile.Score = scores.Average();
                _DNAScores.Add(currentProfile);
                String output = $"{currentProfile.ToString()} Score : {scores.Average()}";
                Console.WriteLine(output);
                _OutputStream.WriteLine(output);

                Monitor.Exit(_DNAScores);
            }
        }
    }
}
