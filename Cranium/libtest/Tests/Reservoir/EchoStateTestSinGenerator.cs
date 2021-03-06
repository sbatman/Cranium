// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cranium.Lib.Activity.Training;
using Cranium.Lib.Data;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Test.Tests.Reservoir
{
	/// <summary>
	///    This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
	/// </summary>
	public static class EchoStateTestSinGenerator
	{
		private static Network _TestNetworkStructure;
		private static AdaptedSlidingWindowTraining _SlidingWindowTraining;
		private static Layer _InputLayer;
		private static Layer _OutputLayer;
		private static RecurrentContext _RecurrentLayer;
		private static List<BaseNode> _InputLayerNodes;
		private static List<BaseNode> _OuputLayerNodes;

		/// <summary>
		///    Run this instance.
		/// </summary>
		public static void Run()
		{
			//Build Network
			_TestNetworkStructure = new Network();
			BuildStructure();
			_TestNetworkStructure.RandomiseWeights(0.5f);
			//PrepData
			Double[][] dataSet = BuildDataSet(3000);

			//Prepare training activity
			_SlidingWindowTraining = new AdaptedSlidingWindowTraining();
			_SlidingWindowTraining.SetTargetNetwork(_TestNetworkStructure);
			_SlidingWindowTraining.SetMomentum(0.8f);
			_SlidingWindowTraining.SetLearningRate(0.05f);
			_SlidingWindowTraining.SetDatasetReservedLength(0);
			_SlidingWindowTraining.DistanceToForcastHorizon = 0;
			_SlidingWindowTraining.WindowWidth = 300;
			_SlidingWindowTraining.SetMaximumEpochs(1000);
			_SlidingWindowTraining.SetInputNodes(_InputLayerNodes);
			_SlidingWindowTraining.SetOutputNodes(_OuputLayerNodes);
			_SlidingWindowTraining.SetWorkingDataSet(dataSet);
			_SlidingWindowTraining.SetOutputToTarget(false);
			_SlidingWindowTraining.SetRecurrentConextLayers(new List<Layer>
			{
				_RecurrentLayer
			});

			Console.WriteLine("Starting Training");
			_SlidingWindowTraining.Start();
			Thread.Sleep(1000);
			while (_SlidingWindowTraining.Running) Thread.Sleep(20);

			Console.WriteLine("Complete Training");

			Console.WriteLine("Starting Testing");
			foreach (BaseNode node in _TestNetworkStructure.GetCurrentLayers().SelectMany(layer => layer.GetNodes())) node.SetValue(0);

			Double[] input = new Double[1000];
			Double[] output = new Double[1000];
			Single frequency = 0.5f;
			for (Int32 x = 0; x < 1000; x++)
			{
				if (x % 100 == 0) frequency = 0.5f;
				//    input[x] = frequency;
				_InputLayerNodes[0].SetValue(frequency);
				_TestNetworkStructure.FowardPass();
				_RecurrentLayer.UpdateExtra();
				output[x] = _OuputLayerNodes[0].GetValue();
			}

			Functions.PrintArrayToFile(input, "intput.csv");
			Functions.PrintArrayToFile(output, "output.csv");
			Console.WriteLine("Complete Testing");

			Console.ReadKey();
		}

		/// <summary>
		///    Builds the structure of the neural network ready for training and testing
		/// </summary>
		public static void BuildStructure()
		{
			_InputLayer = new Layer();
			_InputLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 1; i++) _InputLayerNodes.Add(new BaseNode(_InputLayer, new TanhAF()));
			_InputLayer.SetNodes(_InputLayerNodes);

			EchoReservoir echoLayer = new EchoReservoir(50, 0.4f, 0, 30, new TanhAF());

			_OutputLayer = new Layer();
			_OuputLayerNodes = new List<BaseNode>();
			for (Int32 i = 0; i < 1; i++) _OuputLayerNodes.Add(new OutputNode(_OutputLayer, new TanhAF()));
			_OutputLayer.SetNodes(_OuputLayerNodes);

			_RecurrentLayer = new RecurrentContext(2, new TanhAF());
			_RecurrentLayer.AddSourceNodes(_OuputLayerNodes);

			_InputLayer.ConnectForwardLayer(echoLayer);
			_RecurrentLayer.ConnectForwardLayer(echoLayer);

			echoLayer.ConnectForwardLayer(_OutputLayer);

			_TestNetworkStructure.AddLayer(_InputLayer);
			_TestNetworkStructure.AddLayer(_RecurrentLayer);
			_TestNetworkStructure.AddLayer(echoLayer);
			_TestNetworkStructure.AddLayer(_OutputLayer);

			foreach (Layer layer in _TestNetworkStructure.GetCurrentLayers()) layer.PopulateNodeConnections();
			((RecurrentContextNode) _RecurrentLayer.GetNodes()[0]).OverrideRateOfUpdate(1);
		}

		public static Double[][] BuildDataSet(Int32 sets)
		{
			Double[][] data = new Double[2][];
			for (Int32 i = 0; i < 2; i++) data[i] = new Double[sets];
			for (Int32 x = 0; x < sets; x++)
			{
				Double output = Math.Sin(x * 0.05f) * 0.5f;
				data[0][x] = 0;
				data[1][x] = output;
			}

			return data;
		}

		public class AdaptedSlidingWindowTraining : SlidingWindow
		{
			public Double[,,] ExpectedOutputs;

			public override void PrepareData()
			{
				_SequenceCount = (_WorkingDataset[0].GetLength(0) - _PortionOfDatasetReserved) / WindowWidth;
				Int32 inputCount = _InputNodes.Count;
				Int32 outputCount = _OutputNodes.Count;
				_InputSequences = new Double[_SequenceCount, WindowWidth, inputCount];
				ExpectedOutputs = new Double[_SequenceCount, WindowWidth, outputCount];
				for (Int32 i = 0; i < _SequenceCount; i++)
				{
					for (Int32 j = 0; j < WindowWidth; j++)
					{
						_InputSequences[i, j, 0] = _WorkingDataset[0][i * WindowWidth + j];
						ExpectedOutputs[i, j, 0] = _WorkingDataset[1][i * WindowWidth + j];
					}
				}
			}

			protected override Boolean _Tick()
			{
				if (CurrentEpoch >= _MaxEpochs) return false;
				Double error = 0;

				// if the Dynamic Learning Rate delegate is set call it
				if (_DynamicLearningRate != null) _TargetNetwork.LearningRate = _DynamicLearningRate(CurrentEpoch, _LastPassAverageError);
				// if the Dynamic Momentum delegate is set call it
				if (_DynamicMomentum != null) _TargetNetwork.Momentum = _DynamicMomentum(CurrentEpoch, _LastPassAverageError);

				List<Int32> sequencyList = new List<Int32>(_SequenceCount);

				for (Int32 s = 0; s < _SequenceCount; s++) sequencyList.Add(s);

				while (sequencyList.Count > 0)
				{
					//This needs to be booled so it can be turned off
					Int32 s = sequencyList[_Rnd.Next(0, sequencyList.Count)];
					sequencyList.Remove(s);

					foreach (BaseNode node in _TargetNetwork.GetCurrentLayers().SelectMany(layer => layer.GetNodes())) node.SetValue(0);

					for (Int32 i = 0; i < WindowWidth; i++)
					{
						//   Console.Write("present :"+_InputSequences[s, i, 0]);
						for (Int32 x = 0; x < _InputNodes.Count; x++) _InputNodes[x].SetValue(_InputSequences[s, i, x]);
						_TargetNetwork.FowardPass();

						for (Int32 x = 0; x < _OutputNodes.Count; x++)
						{
							OutputNode output = _OutputNodes[x] as OutputNode;
							output?.SetTargetValue(ExpectedOutputs[s, i, x]);
							//       Console.Write(" --- :" + _ExpectedOutputs[s, i, 0]);
						}

						if (i >= 75) _TargetNetwork.ReversePass();

						if (CurrentEpoch < 250)
						{
							for (Int32 x = 0; x < _OutputNodes.Count; x++)
							{
								OutputNode output = _OutputNodes[x] as OutputNode;
								output?.SetValue(ExpectedOutputs[s, i, x]);
								//       Console.Write(" --- :" + _ExpectedOutputs[s, i, 0]);
							}
						}

						_RecurrentLayer.UpdateExtra();
						//    Console.WriteLine(" " + _UpdatingLayers[0].GetNodes()[0].GetValue());
					}

					//Calculate the current error
					Double passError = _OutputNodes.OfType<OutputNode>().Sum(output => output.GetError());
					passError /= _OutputNodes.Count;
					error += passError * passError;
				}

				_LastPassAverageError = error / _SequenceCount;
				Console.WriteLine(_LastPassAverageError);
				_LogStream?.WriteLine(_LastPassAverageError);
				_LogStream?.Flush();
				return true;
			}
		}
	}
}