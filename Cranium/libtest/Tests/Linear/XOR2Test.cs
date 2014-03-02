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
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Node;
using Base = Cranium.Lib.Structure.Layer.Base;

#endregion

namespace Cranium.Lib.Test.Tests.Linear
{
    /// <summary>
    ///     This is a test showing a neural network that is capable if learning the functionality of a two input xor gate
    /// </summary>
    public static class XOR2Test
    {
        /// <summary>
        ///     The network structure to test.
        /// </summary>
        private static Network _TestNetworkStructure;

        /// <summary>
        ///     The current input layer of the neural network structure that is being tested
        /// </summary>
        private static Base _InputLayer;

        /// <summary>
        ///     The first hidden layer of the neural network structure that is being tested
        /// </summary>
        private static Base _HiddenLayer;

        /// <summary>
        ///     The _ output layer of the neural network structure that is being tested
        /// </summary>
        private static Base _OutputLayer;

        /// <summary>
        ///     The Input data to be presented to the network during the foward pass
        /// </summary>
        private static Int32[] _InputData;

        /// <summary>
        ///     The output data to be tested against during the reverse pass
        /// </summary>
        private static Int32[] _OutputData;

        /// <summary>
        ///     Run this instance.
        /// </summary>
        public static void Run()
        {
            _TestNetworkStructure = new Network();
            BuildStructure();
            _TestNetworkStructure.RandomiseWeights(0.4d);
            PrepData();
            int epoch = 0;
            bool Continue = true;
            while (Continue)
            {
                Continue = false;
                epoch++;
                Console.Clear();
                Console.WriteLine("XOR2Test");

                for (int x = 0; x < 4; x++)
                {
                    PresentData(x);
                    ForwardPass();
                    ReversePass(x);

                    if (x == 0 && _OutputLayer.GetNodes() [0].GetValue() > 0.02f) Continue = true;
                    if ((x == 1 || x == 2) && _OutputLayer.GetNodes() [0].GetValue() < 0.98f) Continue = true;
                    if (x == 3 && _OutputLayer.GetNodes() [0].GetValue() > 0.02f) Continue = true;
                    Console.WriteLine(_InputLayer.GetNodes() [0].GetValue() + "-" + _InputLayer.GetNodes() [1].GetValue() +
                                      "  -  " + Math.Round(_OutputLayer.GetNodes() [0].GetValue(), 3));
                }
            }
            Console.WriteLine("Training complete in " + epoch + " epochs");
            Console.ReadKey();
        }

        /// <summary>
        ///     Builds the structure of the neural network.
        /// </summary>
        private static void BuildStructure()
        {
            _InputLayer = new Base();
            List<Structure.Node.Base> inputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 2; i++) inputLayerNodes.Add(new Structure.Node.Base(_InputLayer, new Tanh()));

            _InputLayer.SetNodes(inputLayerNodes);

            _HiddenLayer = new Base();
            List<Structure.Node.Base> hiddenLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 4; i++) hiddenLayerNodes.Add(new Structure.Node.Base(_HiddenLayer, new Tanh()));
            _HiddenLayer.SetNodes(hiddenLayerNodes);

            _OutputLayer = new Base();
            List<Structure.Node.Base> ouputLayerNodes = new List<Structure.Node.Base>();
            for (int i = 0; i < 1; i++) ouputLayerNodes.Add(new Output(_OutputLayer, new Tanh()));
            _OutputLayer.SetNodes(ouputLayerNodes);

            _InputLayer.ConnectFowardLayer(_HiddenLayer);
            _HiddenLayer.ConnectFowardLayer(_OutputLayer);

            _TestNetworkStructure.AddLayer(_InputLayer);
            _TestNetworkStructure.AddLayer(_HiddenLayer);
            _TestNetworkStructure.AddLayer(_OutputLayer);

            foreach (Base layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }

        /// <summary>
        ///     Prepares the training data
        /// </summary>
        private static void PrepData()
        {
            _InputData = new Int32[8];
            _OutputData = new Int32[4];

            _InputData [0] = 0;
            _InputData [1] = 0;
            _OutputData [0] = 0;

            _InputData [2] = 1;
            _InputData [3] = 0;
            _OutputData [1] = 1;

            _InputData [4] = 0;
            _InputData [5] = 1;
            _OutputData [2] = 1;

            _InputData [6] = 1;
            _InputData [7] = 1;
            _OutputData [3] = 0;
        }

        /// <summary>
        ///     Presents the specified row of prepared training data to the network.
        /// </summary>
        /// <param name='row'>
        ///     Row.
        /// </param>
        private static void PresentData(int row)
        {
            _InputLayer.GetNodes() [0].SetValue(_InputData [(row*2)]);
            _InputLayer.GetNodes() [1].SetValue(_InputData [(row*2) + 1]);
        }

        /// <summary>
        ///     Performs the foward pass on the network
        /// </summary>
        private static void ForwardPass()
        {
            _TestNetworkStructure.FowardPass();
        }

        /// <summary>
        ///     Performs the reverse pass ont he network with the given row of prepared training data and the given weight momentum
        /// </summary>
        /// <param name='row'>
        ///     Row.
        /// </param>
        private static void ReversePass(int row)
        {
            Output outputNode = (Output) (_OutputLayer.GetNodes() [0]);
            outputNode.SetTargetValue(_OutputData [row]);
            _OutputLayer.ReversePass(0.3, 0.0);
        }
    }
}