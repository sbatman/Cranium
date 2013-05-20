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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Cranium.LibTest.Tests.Reservoir
{
	/// <summary>
	/// This test shows an example of an echo state neural network learning the Makey-Glass time series dataset
	/// </summary>
	public class MG_EchoState_Test
	{
		private static Cranium.Structure.Network _TestNetworkStructure;
		private static Cranium.Activity.Training.SlidingWindow _SlidingWindowTraining;
		private static Cranium.Structure.Layer.Base _InputLayer;
		private static Cranium.Structure.Layer.Base _OutputLayer;
		private static List<Cranium.Structure.Node.Base> _InputLayerNodes;
		private static List<Cranium.Structure.Node.Base> _OuputLayerNodes;
		
		/// <summary>
		/// Run this instance.
		/// </summary>
		public static void Run ( )
		{			
			//Build Network
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.SaveToFile("testtttt.dat");
			_TestNetworkStructure.RandomiseWeights ( 1.1d );
			//PrepData
			double[][] dataSet = Cranium.Data.Preprocessing.StandardDeviationVariance.ProduceDataset ( "TestData/Mackey-Glass-Pure.csv" ).DataSet;
			
			//Prepare training activity
			_SlidingWindowTraining = new Cranium.Activity.Training.SlidingWindow ();
			_SlidingWindowTraining.SetTargetNetwork ( _TestNetworkStructure );
			_SlidingWindowTraining.SetMomentum ( 0.5f );
			_SlidingWindowTraining.SetLearningRate ( 0.004f );			
			_SlidingWindowTraining.SetDatasetReservedLength ( 0 );
			_SlidingWindowTraining.SetDistanceToForcastHorrison ( 3 );
			_SlidingWindowTraining.SetWindowWidth ( 12 );
			_SlidingWindowTraining.SetMaximumEpochs ( 300 );
			_SlidingWindowTraining.SetInputNodes ( _InputLayerNodes );
			_SlidingWindowTraining.SetOutputNodes ( _OuputLayerNodes );
			_SlidingWindowTraining.SetWorkingDataset ( dataSet );
			_SlidingWindowTraining.SetRecurrentConextLayers ( new List<Structure.Layer.Base> () );
						
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
			_SlidingWindowTraining.SetRecurrentConextLayers ( new List<Structure.Layer.Base> () );
			_SlidingWindowTesting.SetWorkingDataset ( dataSet );
			_SlidingWindowTesting.SetWindowWidth ( 12 );
			_SlidingWindowTesting.SetDistanceToForcastHorrison ( 3 );
			Activity.Testing.SlidingWindow.TestResults Result = _SlidingWindowTesting.TestNetwork ( _TestNetworkStructure );
			
			Console.WriteLine ( Result.RMSE );
			Data.UsefulFunctions.PrintArrayToFile ( Result.ActualOutputs, "ActualOutputs.csv" );
			Data.UsefulFunctions.PrintArrayToFile ( Result.ExpectedOutputs, "ExpectedOutputs.csv" );
			Console.WriteLine ( "Complete Testing" );
			Console.WriteLine ( "Comparing Against Random Walk 3 Step" );
			Console.WriteLine ( Math.Round ( Cranium.Data.PostProcessing.RandomWalkComparison.CalculateErrorAgainstRandomWalk ( Result.ExpectedOutputs, Result.ActualOutputs, 3 ) [0] * 100, 3 ) );
			Console.WriteLine ( "Comparing Against Random Walk 2 Step" );
			Console.WriteLine ( Math.Round ( Cranium.Data.PostProcessing.RandomWalkComparison.CalculateErrorAgainstRandomWalk ( Result.ExpectedOutputs, Result.ActualOutputs, 2 ) [0] * 100, 3 ) );
			Console.WriteLine ( "Comparing Against Random Walk 1 Step" );
			Console.WriteLine ( Math.Round ( Cranium.Data.PostProcessing.RandomWalkComparison.CalculateErrorAgainstRandomWalk ( Result.ExpectedOutputs, Result.ActualOutputs, 1 ) [0] * 100, 3 ) );
					
			Console.ReadKey ();
		}

		public static void BuildStructure ( )
		{
			_InputLayer = new Cranium.Structure.Layer.Base ();
			_InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
			{
				_InputLayerNodes.Add ( new Cranium.Structure.Node.Base ( _InputLayer, new Cranium.Structure.ActivationFunction.Elliott () ) );
			}			
			_InputLayer.SetNodes ( _InputLayerNodes );		
			
			Structure.Layer.Echo_Reservoir echoLayer = new Cranium.Structure.Layer.Echo_Reservoir ( 130, 0.4f, 0, 5,new Cranium.Structure.ActivationFunction.Elliott ()  );
				
			_OutputLayer = new Cranium.Structure.Layer.Base ();
			_OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
			{
				_OuputLayerNodes.Add ( new Cranium.Structure.Node.Output ( _OutputLayer, new Cranium.Structure.ActivationFunction.Elliott () ) );
			}
			_OutputLayer.SetNodes ( _OuputLayerNodes );			
			
			_InputLayer.ConnectFowardLayer ( echoLayer );			
			echoLayer.ConnectFowardLayer ( _OutputLayer );			
			
			_TestNetworkStructure.AddLayer ( _InputLayer );			
			_TestNetworkStructure.AddLayer ( echoLayer );			
			_TestNetworkStructure.AddLayer ( _OutputLayer );
			
			foreach ( Cranium.Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers() )
				layer.PopulateNodeConnections ();									
		}
	}
}

