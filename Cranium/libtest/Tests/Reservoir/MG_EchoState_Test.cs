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
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cranium.Lib.Data;
using Cranium.Lib.Data.PostProcessing;
using Cranium.Lib.Data.Preprocessing;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.IO;
using Base = Cranium.Lib.Structure.Node.Base;

#endregion

namespace Cranium.Lib.Test.Tests.Reservoir
{
    /// <summary>
    /// This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
    /// </summary>
    public static class MG_EchoState_Test
    {


        /// <summary>
        ///     Run this instance.
        /// </summary>
        public static void Run()
        {
            double[][] dataSet = StandardDeviationVariance.ProduceDataset("TestData/Mackey-Glass-Pure.csv").DataSet;
            List<Guid> OutstandingWork = new List<Guid>();
            Lobe.Client.CommsClient lobeConnection = new Lobe.Client.CommsClient();
            lobeConnection.ConnectToManager("localhost", 17432);
            for (int x = 0; x < 20; x++)
            {
                Network _TestNetworkStructure;
                Activity.Training.SlidingWindow _SlidingWindowTraining;
                Structure.Layer.Base _InputLayer = new Structure.Layer.Base(); ;
                Structure.Layer.Base _OutputLayer = new Structure.Layer.Base(); ;
                List<Structure.Node.Base> _InputLayerNodes = new List<Base>();
                List<Structure.Node.Base> _OuputLayerNodes = new List<Base>();
                //Build Network
                _TestNetworkStructure = new Network();
                BuildStructure(_InputLayer, _OutputLayer, _InputLayerNodes, _OuputLayerNodes, _TestNetworkStructure);
                _TestNetworkStructure.SaveToFile("test.dat");
                _TestNetworkStructure.RandomiseWeights(1.1d);
                //PrepData


                //Prepare training activity
                _SlidingWindowTraining = new Activity.Training.SlidingWindow();
                _SlidingWindowTraining.SetTargetNetwork(_TestNetworkStructure);
                _SlidingWindowTraining.SetMomentum(0.5f);
                _SlidingWindowTraining.SetLearningRate(0.004f);
                _SlidingWindowTraining.SetDatasetReservedLength(0);
                _SlidingWindowTraining.SetDistanceToForcastHorrison(3);
                _SlidingWindowTraining.SetWindowWidth(12);
                _SlidingWindowTraining.SetMaximumEpochs(100);
                _SlidingWindowTraining.SetInputNodes(_InputLayerNodes);
                _SlidingWindowTraining.SetOutputNodes(_OuputLayerNodes);
                _SlidingWindowTraining.SetWorkingDataset(dataSet);
                _SlidingWindowTraining.SetRecurrentConextLayers(new List<Structure.Layer.Base>());


                OutstandingWork.Add(lobeConnection.SendJob(_SlidingWindowTraining));

            }
            while (OutstandingWork.Count > 0)
            {
                Thread.Sleep(1000);
                List<Guid> tempList = new List<Guid>(OutstandingWork);
                foreach (Guid guid in tempList)
                {
                    Activity.Training.SlidingWindow work = (Activity.Training.SlidingWindow)lobeConnection.GetCompletedWork(guid);
                    if (work == null) continue;
                    OutstandingWork.Remove(guid);
                    Console.WriteLine("Starting Testing");

                    Activity.Testing.SlidingWindow slidingWindowTesting = new Activity.Testing.SlidingWindow();
                    slidingWindowTesting.SetDatasetReservedLength(0);
                    slidingWindowTesting.SetInputNodes(work.GetTargetNetwork().GetDetectedBottomLayers()[0].GetNodes().ToList());
                    slidingWindowTesting.SetOutputNodes(work.GetTargetNetwork().GetDetectedTopLayers()[0].GetNodes().ToList());
                    slidingWindowTesting.SetRecurrentConextLayers(new List<Structure.Layer.Base>());
                    slidingWindowTesting.SetWorkingDataset(dataSet);
                    slidingWindowTesting.SetWindowWidth(12);
                    slidingWindowTesting.SetDistanceToForcastHorrison(3);
                    slidingWindowTesting.SetTargetNetwork(work.GetTargetNetwork());

                    Activity.Testing.SlidingWindow.SlidingWindowTestResults result = (Activity.Testing.SlidingWindow.SlidingWindowTestResults)slidingWindowTesting.TestNetwork();

                    Console.WriteLine(result.RMSE);
                    Functions.PrintArrayToFile(result.ActualOutputs, "ActualOutputs.csv");
                    Functions.PrintArrayToFile(result.ExpectedOutputs, "ExpectedOutputs.csv");
                    Console.WriteLine("Complete Testing");
                    Console.WriteLine("Comparing Against Random Walk 3 Step");
                    Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 3)[0] * 100, 3));
                    Console.WriteLine("Comparing Against Random Walk 2 Step");
                    Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 2)[0] * 100, 3));
                    Console.WriteLine("Comparing Against Random Walk 1 Step");
                    Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 1)[0] * 100, 3));
                }
            }

            ////////////////////////////////////////////////
            ////////////////////////////////////////////////
            Console.WriteLine("all Jobs Done");
            Console.ReadKey();

        }

        /// <summary>
        /// Builds the structure of the neural network ready for training and testing
        /// </summary>
        public static void BuildStructure(Structure.Layer.Base _InputLayer, Structure.Layer.Base _OutputLayer, List<Structure.Node.Base> _InputLayerNodes, List<Structure.Node.Base> _OuputLayerNodes, Network _TestNetworkStructure)
        {

            for (int i = 0; i < 1; i++) _InputLayerNodes.Add(new Structure.Node.Base(_InputLayer, new Elliott()));

            _InputLayer.SetNodes(_InputLayerNodes);

            Echo_Reservoir echoLayer = new Echo_Reservoir(130, 0.4f, 0, 5, new Elliott());


            for (int i = 0; i < 1; i++) _OuputLayerNodes.Add(new Output(_OutputLayer, new Elliott()));
            _OutputLayer.SetNodes(_OuputLayerNodes);

            _InputLayer.ConnectFowardLayer(echoLayer);
            echoLayer.ConnectFowardLayer(_OutputLayer);

            _TestNetworkStructure.AddLayer(_InputLayer);
            _TestNetworkStructure.AddLayer(echoLayer);
            _TestNetworkStructure.AddLayer(_OutputLayer);

            foreach (Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }
    }
}