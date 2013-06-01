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
using System.Threading;
using Cranium.Activity.Training;
using Cranium.Data;
using Cranium.Data.PostProcessing;
using Cranium.Data.Preprocessing;
using Cranium.Structure;
using Cranium.Structure.ActivationFunction;
using Cranium.Structure.Layer;
using Cranium.Structure.Node;
using Base = Cranium.Structure.Layer.Base;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.IO;

#endregion

namespace Cranium.LibTest.Tests.Reservoir
{
    /// <summary>
    /// This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
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
            _SlidingWindowTraining.SetWindowWidth(12);
            _SlidingWindowTraining.SetMaximumEpochs(5);
            _SlidingWindowTraining.SetInputNodes(_InputLayerNodes);
            _SlidingWindowTraining.SetOutputNodes(_OuputLayerNodes);
            _SlidingWindowTraining.SetWorkingDataset(dataSet);
            _SlidingWindowTraining.SetRecurrentConextLayers(new List<Base>());

            ////////////////////////////////////////////////
            ////////////////////////////////////////////////

            Console.WriteLine("Starting Training");
            _SlidingWindowTraining.Start();
            Thread.Sleep(1000);
            while (_SlidingWindowTraining.IsRunning()) Thread.Sleep(20);

            Console.WriteLine("Complete Training");

            ////////////////////////////////////////////////
            ////////////////////////////////////////////////

            Console.WriteLine("Starting Testing");

            Activity.Testing.SlidingWindow slidingWindowTesting = new Activity.Testing.SlidingWindow();
            slidingWindowTesting.SetDatasetReservedLength(0);
            slidingWindowTesting.SetInputNodes(_InputLayerNodes);
            slidingWindowTesting.SetOutputNodes(_OuputLayerNodes);
            slidingWindowTesting.SetRecurrentConextLayers(new List<Base>());
            slidingWindowTesting.SetWorkingDataset(dataSet);
            slidingWindowTesting.SetWindowWidth(12);
            slidingWindowTesting.SetDistanceToForcastHorrison(3);
            slidingWindowTesting.SetTargetNetwork(_TestNetworkStructure);

            BinaryFormatter formatter = new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple };
            FileStream stream = File.Create("Test.dat");
            formatter.Serialize(stream, slidingWindowTesting);
            stream.Close();

            slidingWindowTesting = null;

            stream = File.Open("Test.dat",FileMode.Open);
            slidingWindowTesting = (Activity.Testing.SlidingWindow)formatter.Deserialize(stream);
            stream.Close();

            Activity.Testing.SlidingWindow.SlidingWindowTestResults result = (Activity.Testing.SlidingWindow.SlidingWindowTestResults)slidingWindowTesting.TestNetwork();



            Console.WriteLine(result.RMSE);
            Functions.PrintArrayToFile(result.ActualOutputs, "ActualOutputs.csv");
            Functions.PrintArrayToFile(result.ExpectedOutputs, "ExpectedOutputs.csv");
            Console.WriteLine("Complete Testing");
            Console.WriteLine("Comparing Against Random Walk 3 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 3) [0]*100, 3));
            Console.WriteLine("Comparing Against Random Walk 2 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 2) [0]*100, 3));
            Console.WriteLine("Comparing Against Random Walk 1 Step");
            Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 1) [0]*100, 3));

            Console.ReadKey();
        }

        /// <summary>
        /// Builds the structure of the neural network ready for training and testing
        /// </summary>
        public static void BuildStructure()
        {
            _InputLayer = new Base();
            _InputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 1; i++) _InputLayerNodes.Add(new Structure.Node.Base(_InputLayer, new Elliott()));
            _InputLayer.SetNodes(_InputLayerNodes);

            Echo_Reservoir echoLayer = new Echo_Reservoir(130, 0.4f, 0, 5, new Elliott());

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