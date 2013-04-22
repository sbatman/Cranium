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
	public class XOR3Test
	{
		
		private static Cranium.Structure.Network _TestNetworkStructure;
		private static  Cranium.Structure.Layer.Base _InputLayer;
		private static  Cranium.Structure.Layer.Base _HiddenLayer;
		private static  Cranium.Structure.Layer.Base _HiddenLayer2;
		private static  Cranium.Structure.Layer.Base _OutputLayer;
		private static  Int32[] _InputData;
		private static  Int32[] _OutputData;

		public static void Run ()
		{			
			_TestNetworkStructure = new Network ();
			BuildStructure ();
			_TestNetworkStructure.RandomiseWeights (0.1d);
			PrepData ();
			int epoch = 0;
			int time = 0;
			while (epoch<1000) {
				epoch++;
				time++;
				if (time % 100 == 0) {
					Console.Clear ();
					Console.WriteLine("XOR3Test");
				}
		
				for (int x=0; x<8; x++) {
					
					PresentData (x);
					ForwardPass ();
					ReversePass (x, 0);				

					
					if (time % 100 == 0)
						Console.WriteLine (_InputLayer.GetNodes () [0].GetValue () + "-" + _InputLayer.GetNodes () [1].GetValue () + "-" + _InputLayer.GetNodes () [2].GetValue () + "  -  " + Math.Round (_OutputLayer.GetNodes () [0].GetValue (), 2) + "  -  " + Math.Round (_OutputLayer.GetNodes () [1].GetValue (), 2) + "  -  " + Math.Round (_OutputLayer.GetNodes () [2].GetValue (), 2));
				}
			}
			Console.ReadKey ();
		}

		public static void BuildStructure ()
		{
			_InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
				InputLayerNodes.Add (new Cranium.Structure.Node.Base (_InputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			
			_InputLayer.SetNodes (InputLayerNodes);
			
			_HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<6; i++)
				HiddenLayerNodes.Add (new Cranium.Structure.Node.Base (_HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			//HiddenLayerNodes.Add (new Cranium.Structure.Node.Bias (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			_HiddenLayer.SetNodes (HiddenLayerNodes);
			
			_HiddenLayer2 = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes2 = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<4; i++)
				HiddenLayerNodes2.Add (new Cranium.Structure.Node.Base (_HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			//HiddenLayerNodes.Add (new Cranium.Structure.Node.Bias (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			_HiddenLayer2.SetNodes (HiddenLayerNodes2);
			
			_OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
				OuputLayerNodes.Add (new Cranium.Structure.Node.Output (_OutputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			_OutputLayer.SetNodes (OuputLayerNodes);
			
			_InputLayer.ConnectFowardLayer (_HiddenLayer);
			_HiddenLayer.ConnectFowardLayer (_HiddenLayer2);
			_HiddenLayer2.ConnectFowardLayer (_OutputLayer);
			
			_TestNetworkStructure.AddLayer (_InputLayer);			
			_TestNetworkStructure.AddLayer (_HiddenLayer);	
			_TestNetworkStructure.AddLayer (_HiddenLayer2);	
			_TestNetworkStructure.AddLayer (_OutputLayer);
			
			foreach (Cranium.Structure.Layer.Base layer in _TestNetworkStructure.GetCurrentLayers())
				layer.PopulateNodeConnections ();
			
									
		}
		
		public static void PrepData ()
		{
			_InputData = new Int32[24];
			_OutputData = new Int32[24];
			
			int i = 0;
			int o = 0;
			
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_OutputData [o++] = 0;
			_OutputData [o++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			_OutputData [o++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			_OutputData [o++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 0;
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			_OutputData [o++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_OutputData [o++] = 1;
			_OutputData [o++] = 1;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			_OutputData [o++] = 0;
			_OutputData [o++] = 1;
			
			_InputData [i++] = 1;
			_InputData [i++] = 0;
			_InputData [i++] = 1;
			_OutputData [o++] = 1;
			_OutputData [o++] = 0;
			_OutputData [o++] = 0;
			
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_InputData [i++] = 1;
			_OutputData [o++] = 0;
			_OutputData [o++] = 1;
			_OutputData [o++] = 1;

		}
		
		public static void PresentData (int row)
		{
			_InputLayer.GetNodes () [0].SetValue (_InputData [(row * 3)]);
			_InputLayer.GetNodes () [1].SetValue (_InputData [(row * 3) + 1]);
			_InputLayer.GetNodes () [2].SetValue (_InputData [(row * 3) + 2]);
		}
		
		public static void ForwardPass ()
		{
			_TestNetworkStructure.FowardPass ();	
		}
		
		public static void ReversePass (int row, Double momentum)
		{
			for (int x=0; x<3; x++) {
				((Structure.Node.Output)_OutputLayer.GetNodes () [x]).SetTargetValue (_OutputData [(row * 3) + x]);
			}
			_OutputLayer.ReversePass (0.01,0.0);
		}
	}
}

