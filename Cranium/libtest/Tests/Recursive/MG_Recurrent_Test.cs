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

namespace Cranium.LibTest.Tests.Recursive
{
	public class MG_Recurrent_Test
	{
		/// <summary>
		/// The Neural Network structure that is being tested
		/// </summary>
		private static Cranium.Structure.Network _TestNetworkStructure;
		/// <summary>
		/// The SlidingWindow training activity that will be used to train the neural network structure
		/// </summary>
		private static Cranium.Activity.Training.SlidingWindow _SlidingWindowTraining;
		/// <summary>
		/// The input layer of the nerual network
		/// </summary>
		private static Cranium.Structure.Layer.Base _InputLayer;
		/// <summary>
		/// The hidden layer of the nerual network.
		/// </summary>
		private static Cranium.Structure.Layer.Base _HiddenLayer;
		/// <summary>
		/// The recurvie context layer of the nerual network
		/// </summary>
		private static Cranium.Structure.Layer.Recurrent_Context _ContextLayer;
		/// <summary>
		/// The output layer of the neural network
		/// </summary>
		private static Cranium.Structure.Layer.Base _OutputLayer;
		/// <summary>
		/// A list of the input nodes present in this neural network structure
		/// </summary>
		private static List<Cranium.Structure.Node.Base> _InputLayerNodes;
		/// <summary>
		/// A list of the output nodes present in the neural network structure
		/// </summary>
		private static List<Cranium.Structure.Node.Base> _OuputLayerNodes;
		
		/// <summary>
		/// Run this instance.
		/// </summary>
		public static void Run ( )
		{			
			//Build Network
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.RandomiseWeights ( 0.01d );
			//PrepData
			double[][] dataSet = Cranium.Data.Preprocessing.StandardDeviationVariance.ProduceDataset ( "TestData/Mackey-Glass-Pure.csv" ).DataSet;
			
			//Prepare training activity
			_SlidingWindowTraining = new Cranium.Activity.Training.SlidingWindow ();
			_SlidingWindowTraining.SetMomentum ( 0.7f ); // The ammount of the previous weight change applied to current weight change - google if u need to know more
			_SlidingWindowTraining.SetLearningRate ( 0.004f ); // The rate at which the neural entwork learns (the more agressive this is the harded itll be for the network)
			_SlidingWindowTraining.SetTargetNetwork ( _TestNetworkStructure ); // the target network for the training to take place on
			_SlidingWindowTraining.SetDatasetReservedLength ( 0 ); // How many elements off the end of the dataset should not be used for training
			_SlidingWindowTraining.SetDistanceToForcastHorrison ( 3 ); // How far beyond the window should be be trying to predict 
			_SlidingWindowTraining.SetWindowWidth ( 12 ); // The window of elements that should be presented before the backward pass is performed
			_SlidingWindowTraining.SetMaximumEpochs ( 900 ); // The maximum number of epochs the network can train for
			_SlidingWindowTraining.SetInputNodes ( _InputLayerNodes ); // Setting the nodes that are used for input
			_SlidingWindowTraining.SetOutputNodes ( _OuputLayerNodes ); // Setting the nodes that are generating output
			_SlidingWindowTraining.SetWorkingDataset ( dataSet ); // Setting the working dataset for the training phase
			
			// Sets the contect layers that are used as part of the training (have to updates)
			List<Structure.Layer.Base> contextLayers = new List<Structure.Layer.Base> ();
			contextLayers.Add ( _ContextLayer );
			_SlidingWindowTraining.SetRecurrentConextLayers ( contextLayers );
						
			////////////////////////////////////////////////
			////////////////////////////////////////////////
			
			Console.WriteLine ( "Starting Training" );
			_SlidingWindowTraining.Start ();
			Thread.Sleep ( 1000 );
			while (_SlidingWindowTraining.IsRunning())
			{
				Thread.Sleep ( 20 );
			}
			
			Console.WriteLine ( "Complete Training" );
			
			////////////////////////////////////////////////
			////////////////////////////////////////////////
			
			Console.WriteLine ( "Starting Testing" );
			
			Activity.Testing.SlidingWindow _SlidingWindowTesting = new Activity.Testing.SlidingWindow ();
			_SlidingWindowTesting.SetDatasetReservedLength ( 0 );
			_SlidingWindowTesting.SetInputNodes ( _InputLayerNodes );
			_SlidingWindowTesting.SetOutputNodes ( _OuputLayerNodes );
			_SlidingWindowTesting.SetRecurrentConextLayers ( contextLayers );
			_SlidingWindowTesting.SetWorkingDataset ( dataSet );
			_SlidingWindowTesting.SetWindowWidth ( 12 );
			_SlidingWindowTesting.SetDistanceToForcastHorrison ( 3 );
			Activity.Testing.SlidingWindow.TestResults Result = _SlidingWindowTesting.TestNetwork ( _TestNetworkStructure );
			
			Console.WriteLine ( Result.RMSE );
			Data.UsefulFunctions.PrintArrayToFile ( Result.ActualOutputs, "ActualOutputs.csv" );
			Data.UsefulFunctions.PrintArrayToFile ( Result.ExpectedOutputs, "ExpectedOutputs.csv" );
			Console.WriteLine ( "Complete Testing" );
			
			Console.ReadKey ();
		}

		/// <summary>
		/// Builds the structure of the neural netowrk to undergo training and testing.
		/// </summary>
		public static void BuildStructure ( )
		{
			// Input layer construction
			_InputLayer = new Cranium.Structure.Layer.Base ();
			_InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
			{
				_InputLayerNodes.Add ( new Cranium.Structure.Node.Base ( _InputLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}			
			_InputLayer.SetNodes ( _InputLayerNodes );		
			
			// Hidden layer construction
			_HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<5; i++)
			{
				HiddenLayerNodes.Add ( new Cranium.Structure.Node.Base ( _HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}	
			_HiddenLayer.SetNodes ( HiddenLayerNodes );	
			
			// Conext layer construction
			_ContextLayer = new Cranium.Structure.Layer.Recurrent_Context ( 4 );			
			
			//Output layer construction
			_OutputLayer = new Cranium.Structure.Layer.Base ();
			_OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
			{
				_OuputLayerNodes.Add ( new Cranium.Structure.Node.Output ( _OutputLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}
			_OutputLayer.SetNodes ( _OuputLayerNodes );
			
			// Add the nodes of the output and hidden layers to the context layer (so it generates context codes)
			_ContextLayer.AddSourceNodes ( _OuputLayerNodes );
			_ContextLayer.AddSourceNodes ( HiddenLayerNodes );
			
			// Connecting the layers of the neural network together
			_InputLayer.ConnectFowardLayer ( _HiddenLayer );			
			_HiddenLayer.ConnectFowardLayer ( _OutputLayer );
			_ContextLayer.ConnectFowardLayer ( _HiddenLayer );
			
			// Adding the layers to the neural network
			_TestNetworkStructure.AddLayer ( _InputLayer );			
			_TestNetworkStructure.AddLayer ( _HiddenLayer );	
			_TestNetworkStructure.AddLayer ( _ContextLayer );	
			_TestNetworkStructure.AddLayer ( _OutputLayer );
			
			// Generate all the node to node links
			foreach ( Cranium.Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers() )
				layer.PopulateNodeConnections ();									
		}
	}
}

