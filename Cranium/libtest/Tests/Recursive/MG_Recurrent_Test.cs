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
using Cranium.Structure.Node;
using Base = Cranium.Structure.Layer.Base;
using RecurrentContext = Cranium.Structure.Layer.RecurrentContext;

#endregion

namespace Cranium.LibTest.Tests.Recursive
{
    /// <summary>
    ///     This is a test of a neural network that can show successful learning of the Mackey-Glass time series dataset using a standard
    ///     recursive context node bank for memory.
    /// </summary>
    public class MG_Recurrent_Test
    {
        /// <summary>
        ///     The Neural Network structure that is being tested
        /// </summary>
        private static Network _TestNetworkStructure;

        /// <summary>
        ///     The SlidingWindow training activity that will be used to train the neural network structure
        /// </summary>
        private static SlidingWindow _SlidingWindowTraining;

        /// <summary>
        ///     The input layer of the nerual network
        /// </summary>
        private static Base _InputLayer;

        /// <summary>
        ///     The hidden layer of the nerual network.
        /// </summary>
        private static Base _HiddenLayer;

        /// <summary>
        ///     The recurvie context layer of the nerual network
        /// </summary>
        private static RecurrentContext _ContextLayer;

        /// <summary>
        ///     The output layer of the neural network
        /// </summary>
        private static Base _OutputLayer;

        /// <summary>
        ///     A list of the input nodes present in this neural network structure
        /// </summary>
        private static List<Structure.Node.Base> _InputLayerNodes;

        /// <summary>
        ///     A list of the output nodes present in the neural network structure
        /// </summary>
        private static List<Structure.Node.Base> _OuputLayerNodes;

        /// <summary>
        ///     Run this instance.
        /// </summary>
        public static void Run()
        {
            //Build Network
            _TestNetworkStructure = new Network();
            BuildStructure();
            _TestNetworkStructure.RandomiseWeights(0.01d);
            //PrepData
            double[][] dataSet = StandardDeviationVariance.ProduceDataset("TestData/Mackey-Glass-Pure.csv").DataSet;
            //Prepare training activity
            _SlidingWindowTraining = new SlidingWindow();
            _SlidingWindowTraining.SetTargetNetwork(_TestNetworkStructure);
            // the target network for the training to take place on
            _SlidingWindowTraining.SetMomentum(0.7f);
            // The ammount of the previous weight change applied to current weight change - google if u need to know more
            _SlidingWindowTraining.SetLearningRate(0.004f);
            // The rate at which the neural entwork learns (the more agressive this is the harded itll be for the network)			
            _SlidingWindowTraining.SetDatasetReservedLength(0);
            // How many elements off the end of the dataset should not be used for training
            _SlidingWindowTraining.SetDistanceToForcastHorrison(3);
            // How far beyond the window should be be trying to predict 
            _SlidingWindowTraining.SetWindowWidth(12);
            // The window of elements that should be presented before the backward pass is performed
            _SlidingWindowTraining.SetMaximumEpochs(50); // The maximum number of epochs the network can train for
            _SlidingWindowTraining.SetInputNodes(_InputLayerNodes); // Setting the nodes that are used for input
            _SlidingWindowTraining.SetOutputNodes(_OuputLayerNodes); // Setting the nodes that are generating output
            _SlidingWindowTraining.SetWorkingDataset(dataSet); // Setting the working dataset for the training phase
            _SlidingWindowTraining.SetDynamicLearningRateDelegate(DynamicLearningRate);

            // Sets the contect layers that are used as part of the training (have to updates)
            List<Base> contextLayers = new List<Base>();
            contextLayers.Add(_ContextLayer);
            _SlidingWindowTraining.SetRecurrentConextLayers(contextLayers);

            ////////////////////////////////////////////////
            ////////////////////////////////////////////////

            Console.WriteLine("Starting Training");
            _SlidingWindowTraining.Start();
            Thread.Sleep(1000);
            while (_SlidingWindowTraining.IsRunning()) Thread.Sleep(20);

            ////////////////////////////////////////////////
            ////////////////////////////////////////////////

            Console.WriteLine("Starting Testing");

            Activity.Testing.SlidingWindow _SlidingWindowTesting = new Activity.Testing.SlidingWindow();
            _SlidingWindowTesting.SetDatasetReservedLength(0);
            _SlidingWindowTesting.SetInputNodes(_InputLayerNodes);
            _SlidingWindowTesting.SetOutputNodes(_OuputLayerNodes);
            _SlidingWindowTesting.SetRecurrentConextLayers(contextLayers);
            _SlidingWindowTesting.SetWorkingDataset(dataSet);
            _SlidingWindowTesting.SetWindowWidth(12);
            _SlidingWindowTesting.SetDistanceToForcastHorrison(3);
            Activity.Testing.SlidingWindow.TestResults Result = _SlidingWindowTesting.TestNetwork(_TestNetworkStructure);

            Console.WriteLine(Result.RMSE);
            UsefulFunctions.PrintArrayToFile(Result.ActualOutputs, "ActualOutputs.csv");
            UsefulFunctions.PrintArrayToFile(Result.ExpectedOutputs, "ExpectedOutputs.csv");
            Console.WriteLine("Comparing Against Random Walk 3 Step");
            Console.WriteLine(
                Math.Round(
                    RandomWalkComparison.CalculateErrorAgainstRandomWalk(Result.ExpectedOutputs, Result.ActualOutputs, 3)
                        [0]*100, 3));
            Console.WriteLine("Comparing Against Random Walk 2 Step");
            Console.WriteLine(
                Math.Round(
                    RandomWalkComparison.CalculateErrorAgainstRandomWalk(Result.ExpectedOutputs, Result.ActualOutputs, 2)
                        [0]*100, 3));
            Console.WriteLine("Comparing Against Random Walk 1 Step");
            Console.WriteLine(
                Math.Round(
                    RandomWalkComparison.CalculateErrorAgainstRandomWalk(Result.ExpectedOutputs, Result.ActualOutputs, 1)
                        [0]*100, 3));

            Console.ReadKey();
        }

        /// <summary>
        ///     Builds the structure of the neural network to undergo training and testing.
        /// </summary>
        public static void BuildStructure()
        {
            // Input layer construction
            _InputLayer = new Base();
            _InputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 1; i++) _InputLayerNodes.Add(new Structure.Node.Base(_InputLayer, new Tanh()));
            _InputLayer.SetNodes(_InputLayerNodes);

            // Hidden layer construction
            _HiddenLayer = new Base();
            List<Structure.Node.Base> HiddenLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 10; i++) HiddenLayerNodes.Add(new Structure.Node.Base(_HiddenLayer, new Tanh()));
            _HiddenLayer.SetNodes(HiddenLayerNodes);

            // Conext layer construction
            _ContextLayer = new RecurrentContext(6, new Tanh());

            //Output layer construction
            _OutputLayer = new Base();
            _OuputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 1; i++) _OuputLayerNodes.Add(new Output(_OutputLayer, new Tanh()));
            _OutputLayer.SetNodes(_OuputLayerNodes);

            // Add the nodes of the output and hidden layers to the context layer (so it generates context codes)
            _ContextLayer.AddSourceNodes(_OuputLayerNodes);
            _ContextLayer.AddSourceNodes(HiddenLayerNodes);

            // Connecting the layers of the neural network together
            _InputLayer.ConnectFowardLayer(_HiddenLayer);
            _HiddenLayer.ConnectFowardLayer(_OutputLayer);
            _ContextLayer.ConnectFowardLayer(_HiddenLayer);

            // Adding the layers to the neural network
            _TestNetworkStructure.AddLayer(_InputLayer);
            _TestNetworkStructure.AddLayer(_HiddenLayer);
            _TestNetworkStructure.AddLayer(_ContextLayer);
            _TestNetworkStructure.AddLayer(_OutputLayer);

            // Generate all the node to node links
            foreach (Base layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }

        /// <summary>
        ///     Function that is passed as a delate that dynamiclay calculates the learning rate
        /// </summary>
        /// <returns>
        ///     The learning rate.
        /// </returns>
        /// <param name='x'>
        ///     X.
        /// </param>
        /// <param name='y'>
        ///     Y.
        /// </param>
        public static double DynamicLearningRate(int x, double y)
        {
            return 0.004f;
        }
    }
}