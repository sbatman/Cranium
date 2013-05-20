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
using Cranium.Structure;
using System.Collections.Generic;

namespace Cranium.LibTest.Tests.Recursive
{
	/// <summary>
	/// This test shows a neural network that can demonstate the functionality of an 2 input Xor gate using only one input and recursive context nodes
	/// </summary>
	public class RNNTest
	{		
		/// <summary>
		/// The network structure to test.
		/// </summary>
		private static Cranium.Structure.Network _TestNetworkStructure;
		/// <summary>
		/// The current input layer of the neural network structure that is being tested
		/// </summary>
		private static  Cranium.Structure.Layer.Base _InputLayer;
		/// <summary>
		/// The current hidden layer of the neural network structure that is being tested
		/// </summary>
		private static  Cranium.Structure.Layer.Base _HiddenLayer;
		/// <summary>
		/// The recursive context layer used in this neural network structure.
		/// </summary>
		private static  Cranium.Structure.Layer.Recurrent_Context _ContextLayer;
		/// <summary>
		/// The _ output layer of the neural network structure that is being tested
		/// </summary>
		private static  Cranium.Structure.Layer.Base _OutputLayer;
		/// <summary>
		/// The Input data to be presented to the network during the foward pass
		/// </summary>
		private static  Int32[] _InputData;
		/// <summary>
		/// The output data to be tested against during the reverse pass
		/// </summary>
		private static  Int32[] _OutputData;		
		
		/// <summary>
		/// Run this instance.
		/// </summary>
		public static void Run ( )
		{			
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.RandomiseWeights ( 0.001d );
			PrepData ();
			int epoch = 0;
			bool Continue = true;
			while (Continue)
			{
				Continue = false;
				epoch++;
				
				// No need to update the screen constantly
				if ( epoch % 100 == 0 )
				{
					Console.Clear ();
					Console.WriteLine ( "RNNTest - Stopping conditions are in the code" );
				}	
				for (int x=0; x<4; x++)
				{
					foreach ( Cranium.Structure.Node.Base n in _ContextLayer.GetNodes() )					
						n.SetValue ( 0 );						
					for (int i=0; i<2; i++)
					{
						_InputLayer.GetNodes () [0].SetValue ( _InputData [( x * 2 ) + i] );						
						ForwardPass ();
						_ContextLayer.UpdateExtra ();
					}
					ReversePass ( x, 0 );		
					
					//Stopping conditions
					if ( x == 0 && _OutputLayer.GetNodes () [0].GetValue () > 0.05f )
					{
						Continue = true;
					}
					if ( ( x == 1 || x == 2 ) && _OutputLayer.GetNodes () [0].GetValue () < 0.95f )
					{
						Continue = true;
					}					
					if ( x == 3 && _OutputLayer.GetNodes () [0].GetValue () > 0.05f )
					{
						Continue = true;
					}					
					//
					
					if ( epoch % 100 == 0 )
					{
						Console.WriteLine ( _InputData [x * 2] + "-" + _InputData [( x * 2 ) + 1] + "  -  " + Math.Round ( _OutputLayer.GetNodes () [0].GetValue (), 3 ) );
					}
				}
			}
			Console.WriteLine ( "Training complete in " + epoch + " epochs" );
			Console.ReadKey ();
		}		
		
		/// <summary>
		/// Builds the structure for this nerual network test and training.
		/// </summary>
		public static void BuildStructure ( )
		{
			_InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
			{
				InputLayerNodes.Add ( new Cranium.Structure.Node.Base ( _InputLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}			
			_InputLayer.SetNodes ( InputLayerNodes );		
			
			_HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<10; i++)
			{
				HiddenLayerNodes.Add ( new Cranium.Structure.Node.Base ( _HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}			
			_HiddenLayer.SetNodes ( HiddenLayerNodes );	
			
			_ContextLayer = new Cranium.Structure.Layer.Recurrent_Context ( 4 , new Cranium.Structure.ActivationFunction.Tanh());
			
			_OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
			{
				OuputLayerNodes.Add ( new Cranium.Structure.Node.Output ( _OutputLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}
			_OutputLayer.SetNodes ( OuputLayerNodes );
			
			_ContextLayer.AddSourceNodes ( InputLayerNodes );
			//ContextLayer.AddSourceNodes (HiddenLayerNodes);
			
			_InputLayer.ConnectFowardLayer ( _HiddenLayer );			
			_HiddenLayer.ConnectFowardLayer ( _OutputLayer );
			_ContextLayer.ConnectFowardLayer ( _HiddenLayer );
			
			_TestNetworkStructure.AddLayer ( _InputLayer );			
			_TestNetworkStructure.AddLayer ( _HiddenLayer );	
			_TestNetworkStructure.AddLayer ( _ContextLayer );	
			_TestNetworkStructure.AddLayer ( _OutputLayer );
			
			foreach ( Cranium.Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers() )
				layer.PopulateNodeConnections ();									
		}
		
		/// <summary>
		/// Preps the data for both training and testing this nerual network structure.
		/// </summary>
		public static void PrepData ( )
		{
			_InputData = new Int32[8];
			_OutputData = new Int32[4];
			
			int i = 0;
			int o = 0;
			
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_OutputData [o++] = 0;
		}
	
		/// <summary>
		/// Performs the foward pass on the neural network
		/// </summary>
		public static void ForwardPass ( )
		{
			_TestNetworkStructure.FowardPass ();	
		}
		
		/// <summary>
		/// Performs the reverse pass on the neural network with the row of prepared training data provided and the given momentum
		/// </summary>
		/// <param name='row'>
		/// Row.
		/// </param>
		/// <param name='momentum'>
		/// Momentum.
		/// </param>
		public static void ReversePass ( int row, Double momentum )
		{
			Structure.Node.Output outputNode = ( Structure.Node.Output )( _OutputLayer.GetNodes () [0] );
			outputNode.SetTargetValue ( _OutputData [row] );
			_OutputLayer.ReversePass ( 0.06, 0.7 );
		}
	}
}

