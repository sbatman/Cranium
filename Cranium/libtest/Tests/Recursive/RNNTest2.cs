// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Collections.Generic;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Test.Tests.Recursive
{
	/// <summary>
	///    This test shows a neural network that can demonstate the functionality of a three input Xor gate using only one
	///    input and recursive context nodes
	/// </summary>
	public static class RnnTest2
	{
		/// <summary>
		///    The network structure to test.
		/// </summary>
		private static Network _TestNetworkStructure;

		/// <summary>
		///    The current input layer of the neural network structure that is being tested
		/// </summary>
		private static Layer _InputLayer;

		/// <summary>
		///    The first hidden layer of the neural network structure that is being tested
		/// </summary>
		private static Layer _HiddenLayer;

		/// <summary>
		///    The Recursive context layer of the neural network.
		/// </summary>
		private static RecurrentContext _ContextLayer;

		/// <summary>
		///    The second hidden layer of the neural network structure that is being tested
		/// </summary>
		private static Layer _HiddenLayer2;

		/// <summary>
		///    The _ output layer of the neural network structure that is being tested
		/// </summary>
		private static Layer _OutputLayer;

		/// <summary>
		///    The Input data to be presented to the network during the foward pass
		/// </summary>
		private static Int32[] _InputData;

		/// <summary>
		///    The output data to be tested against during the reverse pass
		/// </summary>
		private static Int32[] _OutputData;

		/// <summary>
		///    Run this instance.
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
				if (epoch % 100 == 0)
				{
					Console.Clear();
					Console.WriteLine("RNNTest2");
				}

				for (Int32 x = 0; x < 8; x++)
				{
					foreach (BaseNode n in _ContextLayer.GetNodes()) n.SetValue(0);

					for (Int32 i = 0; i < 3; i++)
					{
						_InputLayer.GetNodes()[0].SetValue(_InputData[x * 3 + i]);
						ForwardPass();
						_ContextLayer.UpdateExtra();
					}

					ReversePass(x);

					if (x == 0 && _OutputLayer.GetNodes()[0].GetValue() > 0.02f) Continue = true;
					if (x > 0 && x < 7 && _OutputLayer.GetNodes()[0].GetValue() < 0.98f) Continue = true;
					if (x == 7 && _OutputLayer.GetNodes()[0].GetValue() > 0.02f) Continue = true;

					if (epoch % 100 == 0) Console.WriteLine(_InputData[x * 3] + "-" + _InputData[x * 3 + 1] + "-" + _InputData[x * 3 + 2] + "  -  " + Math.Round(_OutputLayer.GetNodes()[0].GetValue(), 2));
				}
			}

			Console.WriteLine("Training complete in " + epoch + " epochs");
			Console.ReadKey();
		}

		/// <summary>
		///    Builds the neural networks structure for testing and training
		/// </summary>
		public static void BuildStructure()
		{
			_InputLayer = new Layer();
			List<BaseNode> inputLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 1; i++) inputLayerNodes.Add(new BaseNode(_InputLayer, new TanhAF()));
			_InputLayer.SetNodes(inputLayerNodes);

			_HiddenLayer = new Layer();
			List<BaseNode> hiddenLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 5; i++) hiddenLayerNodes.Add(new BaseNode(_HiddenLayer, new TanhAF()));
			_HiddenLayer.SetNodes(hiddenLayerNodes);

			_HiddenLayer2 = new Layer();
			List<BaseNode> hiddenLayerNodes2 = new List<BaseNode>();
			for (Int32 i = 0; i < 3; i++) hiddenLayerNodes2.Add(new BaseNode(_HiddenLayer, new TanhAF()));
			_HiddenLayer2.SetNodes(hiddenLayerNodes2);

			_ContextLayer = new RecurrentContext(2, new TanhAF());

			_OutputLayer = new Layer();
			List<BaseNode> ouputLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 1; i++) ouputLayerNodes.Add(new OutputNode(_OutputLayer, new TanhAF()));
			_OutputLayer.SetNodes(ouputLayerNodes);

			_ContextLayer.AddSourceNodes(inputLayerNodes);
			//ContextLayer.AddSourceNodes (HiddenLayerNodes);

			_InputLayer.ConnectForwardLayer(_HiddenLayer);
			_HiddenLayer.ConnectForwardLayer(_HiddenLayer2);
			_HiddenLayer2.ConnectForwardLayer(_OutputLayer);
			_ContextLayer.ConnectForwardLayer(_HiddenLayer);

			_TestNetworkStructure.AddLayer(_InputLayer);
			_TestNetworkStructure.AddLayer(_HiddenLayer);
			_TestNetworkStructure.AddLayer(_HiddenLayer2);
			_TestNetworkStructure.AddLayer(_ContextLayer);
			_TestNetworkStructure.AddLayer(_OutputLayer);

			//Iterate through all the layers and cause them to build the weights between nddes
			foreach (Layer layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
		}

		/// <summary>
		///    Preps the training and testing data
		/// </summary>
		public static void PrepData()
		{
			_InputData = new Int32[24];
			_OutputData = new Int32[8];

			Int32 i = 0;

			_InputData[i++] = 0;
			_InputData[i++] = 0;
			_InputData[i++] = 0;
			_OutputData[0] = 0;

			_InputData[i++] = 1;
			_InputData[i++] = 0;
			_InputData[i++] = 0;
			_OutputData[1] = 1;

			_InputData[i++] = 0;
			_InputData[i++] = 1;
			_InputData[i++] = 0;
			_OutputData[2] = 1;

			_InputData[i++] = 0;
			_InputData[i++] = 0;
			_InputData[i++] = 1;
			_OutputData[3] = 1;

			_InputData[i++] = 1;
			_InputData[i++] = 0;
			_InputData[i++] = 1;
			_OutputData[4] = 1;

			_InputData[i++] = 1;
			_InputData[i++] = 1;
			_InputData[i++] = 0;
			_OutputData[5] = 1;

			_InputData[i++] = 0;
			_InputData[i++] = 1;
			_InputData[i++] = 1;
			_OutputData[6] = 1;

			_InputData[i++] = 1;
			_InputData[i++] = 1;
			_InputData[i] = 1;
			_OutputData[7] = 0;
		}

		/// <summary>
		///    Performs a foward pass on the neural netowork
		/// </summary>
		public static void ForwardPass()
		{
			_TestNetworkStructure.FowardPass();
		}

		/// <summary>
		///    Performs the reverse pass on the neural network with the row of prepared training data provided and the given
		///    momentum
		/// </summary>
		/// <param name='row'>
		///    Row.
		/// </param>
		public static void ReversePass(Int32 row)
		{
			OutputNode outputNode = (OutputNode) _OutputLayer.GetNodes()[0];
			outputNode.SetTargetValue(_OutputData[row]);
			_OutputLayer.ReversePass(0.2, 0.9);
		}
	}
}