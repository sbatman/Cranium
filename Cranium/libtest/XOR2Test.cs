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

namespace Cranium.libtest
{
	public  static class XOR2Test
	{
		private static Cranium.Structure.Network TestNetworkStructure;
		private static  Cranium.Structure.Layer.Base InputLayer;
		private static  Cranium.Structure.Layer.Base HiddenLayer;
		private static  Cranium.Structure.Layer.Base HiddenLayer2;
		private static  Cranium.Structure.Layer.Base OutputLayer;
		private static  Int32[] InputData;
		private static  Int32[] OutputData;

		public static void Run ()
		{			
			TestNetworkStructure = new Network ();
			BuildStructureXOR ();
			TestNetworkStructure.RandomiseWeights (0.1d);
			PrepData ();
			int epoch = 0;
			int time = 0;
			while (true) {
				epoch++;
				time++;
				if (time % 100 == 0) {
					Console.Clear ();
					Console.WriteLine("XOR2Test");
				}
		
				for (int x=0; x<4; x++) {
					
					PresentData (x);
					ForwardPass ();
					ReversePass (x, 0);
					
					if (time % 100 == 0)
						Console.WriteLine (InputLayer.GetNodes () [0].GetValue () + "-" + InputLayer.GetNodes () [1].GetValue () + "  -  " + Math.Round (OutputLayer.GetNodes () [0].GetValue (), 3));
				}
			}
			Console.ReadKey ();
		}

		public static void BuildStructureXOR ()
		{
			InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<2; i++)
				InputLayerNodes.Add (new Cranium.Structure.Node.Base (InputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			
			InputLayer.SetNodes (InputLayerNodes);
			
			HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<2; i++)
				HiddenLayerNodes.Add (new Cranium.Structure.Node.Base (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			//HiddenLayerNodes.Add (new Cranium.Structure.Node.Bias (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			HiddenLayer.SetNodes (HiddenLayerNodes);
			
			HiddenLayer2 = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes2 = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<2; i++)
				HiddenLayerNodes2.Add (new Cranium.Structure.Node.Base (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			//HiddenLayerNodes.Add (new Cranium.Structure.Node.Bias (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			HiddenLayer2.SetNodes (HiddenLayerNodes2);
			
			OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
				OuputLayerNodes.Add (new Cranium.Structure.Node.Output (OutputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			OutputLayer.SetNodes (OuputLayerNodes);
			
			InputLayer.ConnectFowardLayer (HiddenLayer);
			HiddenLayer.ConnectFowardLayer (HiddenLayer2);
			HiddenLayer2.ConnectFowardLayer (OutputLayer);
			
			TestNetworkStructure.AddLayer (InputLayer);			
			TestNetworkStructure.AddLayer (HiddenLayer);	
			TestNetworkStructure.AddLayer (HiddenLayer2);	
			TestNetworkStructure.AddLayer (OutputLayer);
			
			foreach (Cranium.Structure.Layer.Base layer in TestNetworkStructure.GetCurrentLayers())
				layer.PopulateNodeConnections ();
			
									
		}
		
		public static void PrepData ()
		{
			InputData = new Int32[8];
			OutputData = new Int32[4];
			
			InputData [0] = 0;
			InputData [1] = 0;
			OutputData [0] = 0;
			
			InputData [2] = 1;
			InputData [3] = 0;
			OutputData [1] = 1;
			
			InputData [4] = 0;
			InputData [5] = 1;
			OutputData [2] = 1;
			
			InputData [6] = 1;
			InputData [7] = 1;
			OutputData [3] = 0;
		}
		
		public static void PresentData (int row)
		{
			InputLayer.GetNodes () [0].SetValue (InputData [(row * 2)]);
			InputLayer.GetNodes () [1].SetValue (InputData [(row * 2) + 1]);
		}
		
		public static void ForwardPass ()
		{
			TestNetworkStructure.FowardPass ();	
		}
		
		public static void ReversePass (int row, Double momentum)
		{
			Structure.Node.Output outputNode = (Structure.Node.Output)(OutputLayer.GetNodes () [0]);
			outputNode.SetTargetValue (OutputData [row]);
			OutputLayer.ReversePass (0.06, 0.1);
		}
	}
}


