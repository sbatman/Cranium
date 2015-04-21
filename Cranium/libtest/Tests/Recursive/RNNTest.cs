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
using RecurrentContext = Cranium.Lib.Structure.Layer.RecurrentContext;

#endregion

namespace Cranium.Lib.Test.Tests.Recursive
{
    /// <summary>
    ///     This test shows a neural network that can demonstate the functionality of an 2 input Xor gate using only one input
    ///     and recursive context nodes
    /// </summary>
    public static class RnnTest
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
        ///     The current hidden layer of the neural network structure that is being tested
        /// </summary>
        private static Base _HiddenLayer;

        /// <summary>
        ///     The recursive context layer used in this neural network structure.
        /// </summary>
        private static RecurrentContext _ContextLayer;

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
            _TestNetworkStructure.RandomiseWeights(1);
            PrepData();
            Int32 epoch = 0;
            Boolean Continue = true;
            while (Continue)
            {
                Continue = false;
                epoch++;

                // No need to update the screen constantly
                if (epoch%100 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("RNNTest - Stopping conditions are in the code");
                }
                for (Int32 x = 0; x < 4; x++)
                {
                    foreach (Structure.Node.Base n in _ContextLayer.GetNodes()) n.SetValue(0);
                    for (Int32 i = 0; i < 2; i++)
                    {
                        _InputLayer.GetNodes()[0].SetValue(_InputData[(x*2) + i]);
                        ForwardPass();
                        _ContextLayer.UpdateExtra();
                    }
                    ReversePass(x);

                    //Stopping conditions
                    if (x == 0 && _OutputLayer.GetNodes()[0].GetValue() > 0.01f) Continue = true;
                    if ((x == 1 || x == 2) && _OutputLayer.GetNodes()[0].GetValue() < 0.99f) Continue = true;
                    if (x == 3 && _OutputLayer.GetNodes()[0].GetValue() > 0.01f) Continue = true;
                    //

                    if (epoch%100 == 0) Console.WriteLine(_InputData[x*2] + "-" + _InputData[(x*2) + 1] + "  -  " + Math.Round(_OutputLayer.GetNodes()[0].GetValue(), 3));
                }
            }
            Console.WriteLine("Training complete in " + epoch + " epochs");
            Console.ReadKey();
        }

        /// <summary>
        ///     Builds the structure for this nerual network test and training.
        /// </summary>
        public static void BuildStructure()
        {
            _InputLayer = new Base();
            List<Structure.Node.Base> inputLayerNodes = new List<Structure.Node.Base>();
            for (Int32 i = 0; i < 1; i++) inputLayerNodes.Add(new Structure.Node.Base(_InputLayer, new Tanh()));
            _InputLayer.SetNodes(inputLayerNodes);

            _HiddenLayer = new Base();
            List<Structure.Node.Base> hiddenLayerNodes = new List<Structure.Node.Base>();
            for (Int32 i = 0; i < 3; i++) hiddenLayerNodes.Add(new Structure.Node.Base(_HiddenLayer, new Tanh()));
            _HiddenLayer.SetNodes(hiddenLayerNodes);

            _ContextLayer = new RecurrentContext(4, new Tanh());

            _OutputLayer = new Base();
            List<Structure.Node.Base> ouputLayerNodes = new List<Structure.Node.Base>();
            for (Int32 i = 0; i < 1; i++) ouputLayerNodes.Add(new Output(_OutputLayer, new Tanh()));
            _OutputLayer.SetNodes(ouputLayerNodes);

            _ContextLayer.AddSourceNodes(inputLayerNodes);
            //ContextLayer.AddSourceNodes (HiddenLayerNodes);

            _InputLayer.ConnectFowardLayer(_HiddenLayer);
            _HiddenLayer.ConnectFowardLayer(_OutputLayer);
            _ContextLayer.ConnectFowardLayer(_HiddenLayer);

            _TestNetworkStructure.AddLayer(_InputLayer);
            _TestNetworkStructure.AddLayer(_HiddenLayer);
            _TestNetworkStructure.AddLayer(_ContextLayer);
            _TestNetworkStructure.AddLayer(_OutputLayer);

            foreach (Base layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
        }

        /// <summary>
        ///     Preps the data for both training and testing this nerual network structure.
        /// </summary>
        public static void PrepData()
        {
            _InputData = new Int32[8];
            _OutputData = new Int32[4];

            Int32 i = 0;
            Int32 o = 0;

            _InputData[i++] = 0;
            _InputData[i++] = 0;
            _OutputData[o++] = 0;

            _InputData[i++] = 1;
            _InputData[i++] = 0;
            _OutputData[o++] = 1;

            _InputData[i++] = 0;
            _InputData[i++] = 1;
            _OutputData[o++] = 1;

            _InputData[i++] = 1;
            _InputData[i] = 1;
            _OutputData[o] = 0;
        }

        /// <summary>
        ///     Performs the foward pass on the neural network
        /// </summary>
        public static void ForwardPass() { _TestNetworkStructure.FowardPass(); }

        /// <summary>
        ///     Performs the reverse pass on the neural network with the row of prepared training data provided and the given
        ///     momentum
        /// </summary>
        /// <param name='row'>
        ///     Row.
        /// </param>
        public static void ReversePass(Int32 row)
        {
            Output outputNode = (Output) (_OutputLayer.GetNodes()[0]);
            outputNode.SetTargetValue(_OutputData[row]);
            _OutputLayer.ReversePass(0.3, 0.95);
        }
    }
}