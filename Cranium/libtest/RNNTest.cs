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

namespace Cranium.LibTest
{
	public class RNNTest
	{
		private static Cranium.Structure.Network _TestNetworkStructure;
		private static  Cranium.Structure.Layer.Base _InputLayer;
		private static  Cranium.Structure.Layer.Base _HiddenLayer;
		private static  Cranium.Structure.Layer.Recurrent_Context _ContextLayer;
		private static  Cranium.Structure.Layer.Base _OutputLayer;
		private static  Int32[] _InputData;
		private static  Int32[] _OutputData;

		public static void Run ( )
		{			
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.RandomiseWeights ( 0.001d );
			PrepData ();
			int epoch = 0;
			int time = 0;
			while (epoch<1000)
			{
				epoch++;
				time++;
				if ( time % 100 == 0 )
				{
					Console.Clear ();
					Console.WriteLine ( "RNNTest" );
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
					if ( time % 100 == 0 )
					{
						Console.WriteLine ( _InputData [x * 2] + "-" + _InputData [( x * 2 ) + 1] + "  -  " + Math.Round ( _OutputLayer.GetNodes () [0].GetValue (), 3 ) );
					}
				}
			}
			Console.ReadKey ();
		}

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
			
			_ContextLayer = new Cranium.Structure.Layer.Recurrent_Context ( 4 );
			
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
	
		public static void ForwardPass ( )
		{
			_TestNetworkStructure.FowardPass ();	
		}
		
		public static void ReversePass ( int row, Double momentum )
		{
			Structure.Node.Output outputNode = ( Structure.Node.Output )( _OutputLayer.GetNodes () [0] );
			outputNode.SetTargetValue ( _OutputData [row] );
			_OutputLayer.ReversePass ( 0.006, 0.7 );
		}
	}
}

