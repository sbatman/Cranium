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

namespace Cranium.LibTest.Tests.Linear
{
	public class XOR3Test
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
			_TestNetworkStructure.RandomiseWeights ( 0.01d );
			PrepData ();
			int epoch = 0;
			bool Continue = true;
			while (Continue)
			{
				Continue = false;
				epoch++;
				
				// No need to fully clear the screen constantly
				if ( epoch % 200 == 0 )
				{
					Console.Clear ();
				}
				Console.SetCursorPosition ( 0, 0 );
				Console.WriteLine ( "XOR3Test" );
				for (int x=0; x<8; x++)
				{
					PresentData ( x );
					ForwardPass ();
					ReversePass ( x, 0 );		
					if ( x == 0 && _OutputLayer.GetNodes () [0].GetValue () > 0.02f )
					{
						Continue = true;
					}
					if ( x > 0 && x < 7 && _OutputLayer.GetNodes () [0].GetValue () < 0.98f )
					{
						Continue = true;
					}					
					if ( x == 7 && _OutputLayer.GetNodes () [0].GetValue () > 0.02f )
					{
						Continue = true;
					}		
					Console.WriteLine ( _InputLayer.GetNodes () [0].GetValue () + "-" + _InputLayer.GetNodes () [1].GetValue () + "-" + _InputLayer.GetNodes () [2].GetValue () + "  -  " + Math.Round ( _OutputLayer.GetNodes () [0].GetValue (), 2 ) );
				}
			}
			Console.WriteLine ( "Training complete in " + epoch + " epochs" );
			Console.ReadKey ();
		}
		
		/// <summary>
		/// Builds the structure of the neural network.
		/// </summary>
		public static void BuildStructure ( )
		{
			_InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
			{
				InputLayerNodes.Add ( new Cranium.Structure.Node.Base ( _InputLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}
			
			_InputLayer.SetNodes ( InputLayerNodes );
			
			_HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<4; i++)
			{
				HiddenLayerNodes.Add ( new Cranium.Structure.Node.Base ( _HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}
			_HiddenLayer.SetNodes ( HiddenLayerNodes );
			
			_OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
			{
				OuputLayerNodes.Add ( new Cranium.Structure.Node.Output ( _OutputLayer, new Cranium.Structure.ActivationFunction.Tanh () ) );
			}
			_OutputLayer.SetNodes ( OuputLayerNodes );
			
			_InputLayer.ConnectFowardLayer ( _HiddenLayer );
			_HiddenLayer.ConnectFowardLayer ( _OutputLayer );
			
			_TestNetworkStructure.AddLayer ( _InputLayer );			
			_TestNetworkStructure.AddLayer ( _HiddenLayer );	
			_TestNetworkStructure.AddLayer ( _OutputLayer );
			
			foreach ( Cranium.Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers() )
				layer.PopulateNodeConnections ();
			
									
		}
		
		/// <summary>
		/// Prepares the training data
		/// </summary>
		public static void PrepData ( )
		{
			_InputData = new Int32[24];
			_OutputData = new Int32[8];
			
			int i = 0;
			int o = 0;
			
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_OutputData [o++] = 0;

		}
		
		/// <summary>
		/// Presents the specified row of prepared training data to the network.
		/// </summary>
		/// <param name='row'>
		/// Row.
		/// </param>
		public static void PresentData ( int row )
		{
			_InputLayer.GetNodes () [0].SetValue ( _InputData [( row * 3 )] );
			_InputLayer.GetNodes () [1].SetValue ( _InputData [( row * 3 ) + 1] );
			_InputLayer.GetNodes () [2].SetValue ( _InputData [( row * 3 ) + 2] );
		}
		
		/// <summary>
		/// Performs the foward pass on the network
		/// </summary>
		public static void ForwardPass ( )
		{
			_TestNetworkStructure.FowardPass ();	
		}
		
		/// <summary>
		/// Performs the reverse pass ont he network with the given row of prepared training data and the given weight momentum
		/// </summary>
		/// <param name='row'>
		/// Row.
		/// </param>
		/// <param name='momentum'>
		/// Momentum.
		/// </param>
		public static void ReversePass ( int row, Double momentum )
		{			
			( ( Structure.Node.Output )_OutputLayer.GetNodes () [0] ).SetTargetValue ( _OutputData [row] );			
			_OutputLayer.ReversePass ( 0.1, 0.0 );
		}
	}
}

