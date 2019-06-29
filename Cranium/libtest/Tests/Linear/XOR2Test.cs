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

namespace Cranium.Lib.Test.Tests.Linear
{
	/// <summary>
	///    This is a test showing a neural network that is capable if learning the functionality of a two input xor gate
	/// </summary>
	public static class Xor2Test
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
			_TestNetworkStructure.RandomiseWeights(0.9d);
			PrepData();
			Int32 epoch = 0;
			Boolean Continue = true;
			while (Continue)
			{
				Continue = false;
				epoch++;
				Console.Clear();
				Console.WriteLine("XOR2Test");

				for (Int32 x = 0; x < 4; x++)
				{
					PresentData(x);
					ForwardPass();
					ReversePass(x);

					if (x == 0 && _OutputLayer.GetNodes()[0].GetValue() > 0.02d) Continue = true;
					if ((x == 1 || x == 2) && _OutputLayer.GetNodes()[0].GetValue() < 0.98d) Continue = true;
					if (x == 3 && _OutputLayer.GetNodes()[0].GetValue() > 0.02d) Continue = true;
					Console.WriteLine(_InputLayer.GetNodes()[0].GetValue() + "-" + _InputLayer.GetNodes()[1].GetValue() + "  -  " + Math.Round(_OutputLayer.GetNodes()[0].GetValue(), 3));
				}
			}

			Console.WriteLine("Training complete in " + epoch + " epochs");
			Console.ReadKey();
		}

		/// <summary>
		///    Builds the structure of the neural network.
		/// </summary>
		private static void BuildStructure()
		{
			_InputLayer = new Layer();
			List<BaseNode> inputLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 2; i++) inputLayerNodes.Add(new BaseNode(_InputLayer, new TanhAF()));

			_InputLayer.SetNodes(inputLayerNodes);

			_HiddenLayer = new Layer();
			List<BaseNode> hiddenLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 3; i++) hiddenLayerNodes.Add(new BaseNode(_HiddenLayer, new TanhAF()));
			_HiddenLayer.SetNodes(hiddenLayerNodes);

			_OutputLayer = new Layer();
			List<BaseNode> ouputLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 1; i++) ouputLayerNodes.Add(new OutputNode(_OutputLayer, new TanhAF()));
			_OutputLayer.SetNodes(ouputLayerNodes);

			_InputLayer.ConnectForwardLayer(_HiddenLayer);
			_HiddenLayer.ConnectForwardLayer(_OutputLayer);

			_TestNetworkStructure.AddLayer(_InputLayer);
			_TestNetworkStructure.AddLayer(_HiddenLayer);
			_TestNetworkStructure.AddLayer(_OutputLayer);

			foreach (Layer layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
		}

		/// <summary>
		///    Prepares the training data
		/// </summary>
		private static void PrepData()
		{
			_InputData = new Int32[8];
			_OutputData = new Int32[4];

			_InputData[0] = 0;
			_InputData[1] = 0;
			_OutputData[0] = 0;

			_InputData[2] = 1;
			_InputData[3] = 0;
			_OutputData[1] = 1;

			_InputData[4] = 0;
			_InputData[5] = 1;
			_OutputData[2] = 1;

			_InputData[6] = 1;
			_InputData[7] = 1;
			_OutputData[3] = 0;
		}

		/// <summary>
		///    Presents the specified row of prepared training data to the network.
		/// </summary>
		/// <param name='row'>
		///    Row.
		/// </param>
		private static void PresentData(Int32 row)
		{
			_InputLayer.GetNodes()[0].SetValue(_InputData[row * 2]);
			_InputLayer.GetNodes()[1].SetValue(_InputData[row * 2 + 1]);
		}

		/// <summary>
		///    Performs the foward pass on the network
		/// </summary>
		private static void ForwardPass()
		{
			_TestNetworkStructure.FowardPass();
		}

		/// <summary>
		///    Performs the reverse pass ont he network with the given row of prepared training data and the given weight momentum
		/// </summary>
		/// <param name='row'>
		///    Row.
		/// </param>
		private static void ReversePass(Int32 row)
		{
			OutputNode outputNode = (OutputNode) _OutputLayer.GetNodes()[0];
			outputNode.SetTargetValue(_OutputData[row]);
			_OutputLayer.ReversePass(0.6, 0.1);
		}
	}
}