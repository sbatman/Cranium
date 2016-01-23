#region info

// //////////////////////
//
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
//
// This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
//
// //////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cranium.Lib.Activity.Training;
using Cranium.Lib.Data;
using Cranium.Lib.Data.PostProcessing;
using Cranium.Lib.Data.Preprocessing;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Test.Tests.Reservoir
{
    /// <summary>
    ///     This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
    /// </summary>
    public static class MgEchoStateTest
    {
        private static Network _TestNetworkStructure;
        private static SlidingWindow _SlidingWindowTraining;
        private static Layer _InputLayer;
        private static Layer _OutputLayer;
        private static List<BaseNode> _InputLayerNodes;
        private static List<BaseNode> _OuputLayerNodes;

        /// <summary>
        ///     Run this instance.
        /// </summary>
        public static void Run()
        {
            //Build Network
            _TestNetworkStructure = new Network();
            BuildStructure();
            _TestNetworkStructure.SaveToFile("test.dat");
            _TestNetworkStructure.RandomiseWeights(1.1d);
            //PrepData
            Double[][] dataSet = StandardDeviationVariance.ProduceDataset("TestData/Mackey-Glass-Pure.csv").DataSet;

            //Prepare training activity
            _SlidingWindowTraining = new SlidingWindow();
            _SlidingWindowTraining.SetTargetNetwork(_TestNetworkStructure);
            _SlidingWindowTraining.SetMomentum(0.5f);
            _SlidingWindowTraining.SetLearningRate(0.004f);
            _SlidingWindowTraining.SetDatasetReservedLength(120);
            _SlidingWindowTraining.SetDistanceToForcastHorrison(3);
            _SlidingWindowTraining.SetWindowWidth(12);
            _SlidingWindowTraining.SetMaximumEpochs(450);
            _SlidingWindowTraining.SetInputNodes(_InputLayerNodes);
            _SlidingWindowTraining.SetOutputNodes(_OuputLayerNodes);
            _SlidingWindowTraining.SetWorkingDataset(dataSet);
            _SlidingWindowTraining.SetRecurrentConextLayers(new List<Layer>());

            Console.WriteLine("Starting Training");
            _SlidingWindowTraining.Start();
            Thread.Sleep(1000);
            while (_SlidingWindowTraining.Running)
            {
                Console.WriteLine(_SlidingWindowTraining.CurrentEpoch);
                Thread.Sleep(20);
            }

            Console.WriteLine("Complete Training");


            Console.WriteLine("Starting Testing");

            Activity.Testing.SlidingWindow slidingWindowTesting = new Activity.Testing.SlidingWindow();
            slidingWindowTesting.SetDatasetReservedLength(0);
            slidingWindowTesting.SetInputNodes(_SlidingWindowTraining.GetTargetNetwork().GetDetectedBottomLayers()[0].GetNodes().ToList());
            slidingWindowTesting.SetOutputNodes(_SlidingWindowTraining.GetTargetNetwork().GetDetectedTopLayers()[0].GetNodes().ToList());
            slidingWindowTesting.SetRecurrentConextLayers(new List<Layer>());
            slidingWindowTesting.SetWorkingDataset(dataSet);
            slidingWindowTesting.SetWindowWidth(6);
            slidingWindowTesting.SetDistanceToForcastHorrison(3);
            slidingWindowTesting.SetTargetNetwork(_SlidingWindowTraining.GetTargetNetwork());

            Activity.Testing.SlidingWindow.SlidingWindowTestResults result = (Activity.Testing.SlidingWindow.SlidingWindowTestResults)slidingWindowTesting.TestNetwork();

            Console.WriteLine(result.Rmse);
            Functions.PrintArrayToFile(result.ActualOutputs, "ActualOutputs.csv");
            Functions.PrintArrayToFile(result.ExpectedOutputs, "ExpectedOutputs.csv");
            Console.WriteLine("Complete Testing");
            Console.WriteLine("Comparing Against Random Walk 3 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 3)[0] * 100, 3));
            Console.WriteLine("Comparing Against Random Walk 2 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 2)[0] * 100, 3));
            Console.WriteLine("Comparing Against Random Walk 1 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 1)[0] * 100, 3));

            Console.ReadKey();
        }

        /// <summary>
        ///     Builds the structure of the neural network ready for training and testing
        /// </summary>
        public static void BuildStructure()
        {
            _InputLayer = new Layer();
            _InputLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < 1; i++) _InputLayerNodes.Add(new BaseNode(_InputLayer, new TanhAF()));
            _InputLayer.SetNodes(_InputLayerNodes);

            EchoReservoir echoLayer = new EchoReservoir(130, 0.4f, 0, 5, new TanhAF());

            _OutputLayer = new Layer();
            _OuputLayerNodes = new List<BaseNode>();
            for (Int32 i = 0; i < 1; i++) _OuputLayerNodes.Add(new OutputNode(_OutputLayer, new TanhAF()));
            _OutputLayer.SetNodes(_OuputLayerNodes);

            _InputLayer.ConnectFowardLayer(echoLayer);
            echoLayer.ConnectFowardLayer(_OutputLayer);

            _TestNetworkStructure.AddLayer(_InputLayer);
            _TestNetworkStructure.AddLayer(echoLayer);
            _TestNetworkStructure.AddLayer(_OutputLayer);

            foreach (Layer layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }
    }
}