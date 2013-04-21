// // ///////////////////////
// // 
// // Cranium - A neural network framework for C#
// // https://github.com/sbatman/Cranium.git
// // 
// // This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// // More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// // If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
// //
// // //////////////////////
using System;
using System.Threading;
using Cranium.Structure;
using System.Collections.Generic;

namespace Cranium.LibTest
{
	public class MG_Recurrent_Test
	{
		private static Cranium.Structure.Network _TestNetworkStructure;
		private static Cranium.Activity.Training.SlidingWindow _SlidingWindowTest;
		private static  Cranium.Structure.Layer.Base _InputLayer;
		private static  Cranium.Structure.Layer.Base _HiddenLayer;
		private static  Cranium.Structure.Layer.Recurrent_Context _ContextLayer;
		private static  Cranium.Structure.Layer.Base _OutputLayer;
		private static List<Cranium.Structure.Node.Base> _InputLayerNodes;
		private static List<Cranium.Structure.Node.Base> _OuputLayerNodes;

		public static void Run ()
		{			
			//Build Network
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.RandomiseWeights (0.01d);
			//PrepData
			double[,] dataSet = Cranium.DataPreperation.StandardDeviationVariance.ProduceDataset("TestData/Mackey-Glass-Pure.csv").DataSet;
			
			//Prepare training activity
			_SlidingWindowTest = new Cranium.Activity.Training.SlidingWindow();
			_SlidingWindowTest.SetMomentum(0.7f);
			_SlidingWindowTest.SetLearningRate(0.04f);
			_SlidingWindowTest.SetTargetNetwork(_TestNetworkStructure);
			_SlidingWindowTest.SetDatasetReservedLength(100);
			_SlidingWindowTest.SetDistanceToForcastHorrison(3);
			_SlidingWindowTest.SetWindowWidth(12);
			_SlidingWindowTest.SetMaximumEpochs(1000);
			_SlidingWindowTest.SetInputNodes(_InputLayerNodes);
			_SlidingWindowTest.SetOutputNodes(_OuputLayerNodes);
			_SlidingWindowTest.SetWorkingDataset(dataSet);
			
			List<Structure.Layer.Recurrent_Context> contextLayers = new List<Structure.Layer.Recurrent_Context>();
			contextLayers.Add(_ContextLayer);
			_SlidingWindowTest.SetRecurrentConextLayers(contextLayers);
			
			Console.WriteLine("Starting");
			_SlidingWindowTest.Start();
			Thread.Sleep(1000);
			while(_SlidingWindowTest.IsRunning())Thread.Sleep(20);
			
			Console.WriteLine("Complete");
			
			Console.ReadKey ();
		}

		public static void BuildStructure ()
		{
			_InputLayer = new Cranium.Structure.Layer.Base ();
			 _InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
				_InputLayerNodes.Add (new Cranium.Structure.Node.Base (_InputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));			
			_InputLayer.SetNodes (_InputLayerNodes);		
			
			_HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<6; i++)
				HiddenLayerNodes.Add (new Cranium.Structure.Node.Base (_HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));			
			_HiddenLayer.SetNodes (HiddenLayerNodes);	
			
			_ContextLayer = new Cranium.Structure.Layer.Recurrent_Context (4);
			
			_OutputLayer = new Cranium.Structure.Layer.Base ();
			_OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
				_OuputLayerNodes.Add (new Cranium.Structure.Node.Output (_OutputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			_OutputLayer.SetNodes (_OuputLayerNodes);
			
			_ContextLayer.AddSourceNodes (_InputLayerNodes);
			_ContextLayer.AddSourceNodes (HiddenLayerNodes);
			
			_InputLayer.ConnectFowardLayer (_HiddenLayer);			
			_HiddenLayer.ConnectFowardLayer (_OutputLayer);
			_ContextLayer.ConnectFowardLayer (_HiddenLayer);
			
			_TestNetworkStructure.AddLayer (_InputLayer);			
			_TestNetworkStructure.AddLayer (_HiddenLayer);	
			_TestNetworkStructure.AddLayer (_ContextLayer);	
			_TestNetworkStructure.AddLayer (_OutputLayer);
			
			foreach (Cranium.Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers())
				layer.PopulateNodeConnections ();									
		}
	}
}

