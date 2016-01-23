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
using Cranium.Lobe.Client;

#endregion

namespace Cranium.Lib.Test.Tests.Reservoir
{
    /// <summary>
    ///     This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
    /// </summary>
    public static class MgEchoStateTestLobe
    {
        /// <summary>
        ///     Run this instance.
        /// </summary>
        public static void Run()
        {
            Double[][] dataSet = StandardDeviationVariance.ProduceDataset("TestData/Mackey-Glass-Pure.csv").DataSet;
            List<Guid> outstandingWork = new List<Guid>();
            CommsClient lobeConnection = new CommsClient();
            lobeConnection.ConnectToManager("localhost", 17432);
            for (Int32 x = 0; x < 20; x++)
            {
                Network testNetworkStructure;
                SlidingWindow slidingWindowTraining;
                Layer inputLayer = new Layer();
                ;
                Layer outputLayer = new Layer();
                ;
                List<BaseNode> inputLayerNodes = new List<BaseNode>();
                List<BaseNode> ouputLayerNodes = new List<BaseNode>();
                //Build Network
                testNetworkStructure = new Network();
                BuildStructure(inputLayer, outputLayer, inputLayerNodes, ouputLayerNodes, testNetworkStructure);
                testNetworkStructure.SaveToFile("test.dat");
                testNetworkStructure.RandomiseWeights(1.1d);
                //PrepData


                //Prepare training activity
                slidingWindowTraining = new SlidingWindow();
                slidingWindowTraining.SetTargetNetwork(testNetworkStructure);
                slidingWindowTraining.SetMomentum(0.5f);
                slidingWindowTraining.SetLearningRate(0.004f);
                slidingWindowTraining.SetDatasetReservedLength(0);
                slidingWindowTraining.SetDistanceToForcastHorrison(3);
                slidingWindowTraining.SetWindowWidth(12);
                slidingWindowTraining.SetMaximumEpochs(100);
                slidingWindowTraining.SetInputNodes(inputLayerNodes);
                slidingWindowTraining.SetOutputNodes(ouputLayerNodes);
                slidingWindowTraining.SetWorkingDataset(dataSet);
                slidingWindowTraining.SetRecurrentConextLayers(new List<Layer>());


                outstandingWork.Add(lobeConnection.SendJob(slidingWindowTraining));
            }
            while (outstandingWork.Count > 0)
            {
                Thread.Sleep(1000);
                List<Guid> tempList = new List<Guid>(outstandingWork);
                foreach (Guid guid in tempList)
                {
                    SlidingWindow work = (SlidingWindow) lobeConnection.GetCompletedWork(guid);
                    if (work == null) continue;
                    outstandingWork.Remove(guid);
                    Console.WriteLine("Starting Testing");

                    Activity.Testing.SlidingWindow slidingWindowTesting = new Activity.Testing.SlidingWindow();
                    slidingWindowTesting.SetDatasetReservedLength(0);
                    slidingWindowTesting.SetInputNodes(work.GetTargetNetwork().GetDetectedBottomLayers()[0].GetNodes().ToList());
                    slidingWindowTesting.SetOutputNodes(work.GetTargetNetwork().GetDetectedTopLayers()[0].GetNodes().ToList());
                    slidingWindowTesting.SetRecurrentConextLayers(new List<Layer>());
                    slidingWindowTesting.SetWorkingDataset(dataSet);
                    slidingWindowTesting.SetWindowWidth(12);
                    slidingWindowTesting.SetDistanceToForcastHorrison(3);
                    slidingWindowTesting.SetTargetNetwork(work.GetTargetNetwork());

                    Activity.Testing.SlidingWindow.SlidingWindowTestResults result = (Activity.Testing.SlidingWindow.SlidingWindowTestResults) slidingWindowTesting.TestNetwork();

                    Console.WriteLine(result.Rmse);
                    Functions.PrintArrayToFile(result.ActualOutputs, "ActualOutputs.csv");
                    Functions.PrintArrayToFile(result.ExpectedOutputs, "ExpectedOutputs.csv");
                    Console.WriteLine("Complete Testing");
                    Console.WriteLine("Comparing Against Random Walk 3 Step");
                    Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 3)[0]*100, 3));
                    Console.WriteLine("Comparing Against Random Walk 2 Step");
                    Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 2)[0]*100, 3));
                    Console.WriteLine("Comparing Against Random Walk 1 Step");
                    Console.WriteLine(Math.Round(RandomWalkCompare.CalculateError(result.ExpectedOutputs, result.ActualOutputs, 1)[0]*100, 3));
                }
            }

            ////////////////////////////////////////////////
            ////////////////////////////////////////////////
            Console.WriteLine("all Jobs Done");
            Console.ReadKey();
        }

        /// <summary>
        ///     Builds the structure of the neural network ready for training and testing
        /// </summary>
        public static void BuildStructure(Layer inputLayer, Layer outputLayer, List<BaseNode> inputLayerNodes, List<BaseNode> ouputLayerNodes, Network testNetworkStructure)
        {
            for (Int32 i = 0; i < 1; i++) inputLayerNodes.Add(new BaseNode(inputLayer, new ElliottAF()));

            inputLayer.SetNodes(inputLayerNodes);

            EchoReservoir echoLayer = new EchoReservoir(130, 0.4f, 0, 5, new ElliottAF());


            for (Int32 i = 0; i < 1; i++) ouputLayerNodes.Add(new OutputNode(outputLayer, new ElliottAF()));
            outputLayer.SetNodes(ouputLayerNodes);

            inputLayer.ConnectFowardLayer(echoLayer);
            echoLayer.ConnectFowardLayer(outputLayer);

            testNetworkStructure.AddLayer(inputLayer);
            testNetworkStructure.AddLayer(echoLayer);
            testNetworkStructure.AddLayer(outputLayer);

            foreach (Layer layer in testNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }
    }
}