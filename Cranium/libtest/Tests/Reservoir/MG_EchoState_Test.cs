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
using Base = Cranium.Lib.Structure.Layer.Base;

#endregion

namespace Cranium.Lib.Test.Tests.Reservoir
{
    /// <summary>
    ///     This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
    /// </summary>
    public static class MG_EchoState_Test
    {
        private static Network _TestNetworkStructure;
        private static SlidingWindow _SlidingWindowTraining;
        private static Base _InputLayer;
        private static Base _OutputLayer;
        private static List<Structure.Node.Base> _InputLayerNodes;
        private static List<Structure.Node.Base> _OuputLayerNodes;

        /// <summary>
        ///     Run this instance.
        /// </summary>
        public static void Run()
        {
            //Build Network
            _TestNetworkStructure = new Network();
            BuildStructure();
            _TestNetworkStructure.SaveToFile("testtttt.dat");
            _TestNetworkStructure.RandomiseWeights(1.1d);
            //PrepData
            double[][] dataSet = StandardDeviationVariance.ProduceDataset("TestData/Mackey-Glass-Pure.csv").DataSet;

            //Prepare training activity
            _SlidingWindowTraining = new SlidingWindow();
            _SlidingWindowTraining.SetTargetNetwork(_TestNetworkStructure);
            _SlidingWindowTraining.SetMomentum(0.5f);
            _SlidingWindowTraining.SetLearningRate(0.004f);
            _SlidingWindowTraining.SetDatasetReservedLength(0);
            _SlidingWindowTraining.SetDistanceToForcastHorrison(3);
            _SlidingWindowTraining.SetWindowWidth(6);
            _SlidingWindowTraining.SetMaximumEpochs(5);
            _SlidingWindowTraining.SetInputNodes(_InputLayerNodes);
            _SlidingWindowTraining.SetOutputNodes(_OuputLayerNodes);
            _SlidingWindowTraining.SetWorkingDataset(dataSet);
            _SlidingWindowTraining.SetRecurrentConextLayers(new List<Base>());

            Console.WriteLine("Starting Training");
            _SlidingWindowTraining.Start();
            Thread.Sleep(1000);
            while (_SlidingWindowTraining.IsRunning()) Thread.Sleep(20);

            Console.WriteLine("Complete Training");


            Console.WriteLine("Starting Testing");

            var slidingWindowTesting = new Activity.Testing.SlidingWindow();
            slidingWindowTesting.SetDatasetReservedLength(0);
            slidingWindowTesting.SetInputNodes(_SlidingWindowTraining.GetTargetNetwork().GetDetectedBottomLayers()[0].GetNodes().ToList());
            slidingWindowTesting.SetOutputNodes(_SlidingWindowTraining.GetTargetNetwork().GetDetectedTopLayers()[0].GetNodes().ToList());
            slidingWindowTesting.SetRecurrentConextLayers(new List<Base>());
            slidingWindowTesting.SetWorkingDataset(dataSet);
            slidingWindowTesting.SetWindowWidth(6);
            slidingWindowTesting.SetDistanceToForcastHorrison(3);
            slidingWindowTesting.SetTargetNetwork(_SlidingWindowTraining.GetTargetNetwork());

            var result = (Activity.Testing.SlidingWindow.SlidingWindowTestResults) slidingWindowTesting.TestNetwork();

            Console.WriteLine(result.RMSE);
            Functions.PrintArrayToFile(result.ActualOutputs, "ActualOutputs.csv");
            Functions.PrintArrayToFile(result.ExpectedOutputs, "ExpectedOutputs.csv");
            Console.WriteLine("Complete Testing");
            Console.WriteLine("Comparing Against Random Walk 3 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 3)[0]*100, 3));
            Console.WriteLine("Comparing Against Random Walk 2 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 2)[0]*100, 3));
            Console.WriteLine("Comparing Against Random Walk 1 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 1)[0]*100, 3));

            Console.ReadKey();
        }

        /// <summary>
        ///     Builds the structure of the neural network ready for training and testing
        /// </summary>
        public static void BuildStructure()
        {
            _InputLayer = new Base();
            _InputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 1; i++) _InputLayerNodes.Add(new Structure.Node.Base(_InputLayer, new Elliott()));
            _InputLayer.SetNodes(_InputLayerNodes);

            var echoLayer = new Echo_Reservoir(130, 0.4f, 0, 5, new Elliott());

            _OutputLayer = new Base();
            _OuputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 1; i++) _OuputLayerNodes.Add(new Output(_OutputLayer, new Elliott()));
            _OutputLayer.SetNodes(_OuputLayerNodes);

            _InputLayer.ConnectFowardLayer(echoLayer);
            echoLayer.ConnectFowardLayer(_OutputLayer);

            _TestNetworkStructure.AddLayer(_InputLayer);
            _TestNetworkStructure.AddLayer(echoLayer);
            _TestNetworkStructure.AddLayer(_OutputLayer);

            foreach (Base layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }
    }
}