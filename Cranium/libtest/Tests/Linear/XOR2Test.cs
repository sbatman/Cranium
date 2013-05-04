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
	public static class XOR2Test
	{
		private static Cranium.Structure.Network _TestNetworkStructure;
		private static  Cranium.Structure.Layer.Base _InputLayer;
		private static  Cranium.Structure.Layer.Base _HiddenLayer;
		private static  Cranium.Structure.Layer.Base _HiddenLayer2;
		private static  Cranium.Structure.Layer.Base _OutputLayer;
		private static  Int32[] _InputData;
		private static  Int32[] _OutputData;

		public static void Run ( )
		{			
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.RandomiseWeights ( 0.4d );
			PrepData ();
			int epoch = 0;
			bool Continue = true;
			while (Continue)
			{
				Continue = false;
				epoch++;
				Console.Clear ();
				Console.WriteLine ( "XOR2Test" );		
		
				for (int x=0; x<4; x++)
				{					
					PresentData ( x );
					ForwardPass ();
					ReversePass ( x, 0 );		
					
					if ( x == 0 && _OutputLayer.GetNodes () [0].GetValue () > 0.02f )
					{
						Continue = true;
					}
					if ( ( x == 1 || x == 2 ) && _OutputLayer.GetNodes () [0].GetValue () < 0.98f )
					{
						Continue = true;
					}					
					if ( x == 3 && _OutputLayer.GetNodes () [0].GetValue () > 0.02f )
					{
						Continue = true;
					}
					Console.WriteLine ( _InputLayer.GetNodes () [0].GetValue () + "-" + _InputLayer.GetNodes () [1].GetValue () + "  -  " + Math.Round ( _OutputLayer.GetNodes () [0].GetValue (), 3 ) );
				}
			}
			Console.WriteLine ( "Training complete in " + epoch + " epochs" );
			Console.ReadKey ();
		}

		public static void BuildStructure ( )
		{
			_InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<2; i++)
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
			for (int i=0; i<1; i++)
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
		
		public static void PrepData ( )
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
		
		public static void PresentData ( int row )
		{
			_InputLayer.GetNodes () [0].SetValue ( _InputData [( row * 2 )] );
			_InputLayer.GetNodes () [1].SetValue ( _InputData [( row * 2 ) + 1] );
		}
		
		public static void ForwardPass ( )
		{
			_TestNetworkStructure.FowardPass ();	
		}
		
		public static void ReversePass ( int row, Double momentum )
		{
			Structure.Node.Output outputNode = ( Structure.Node.Output )( _OutputLayer.GetNodes () [0] );
			outputNode.SetTargetValue ( _OutputData [row] );
			_OutputLayer.ReversePass ( 0.3, 0.0 );
		}
	}
}


