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
	public class XOR3Test
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
			TestNetworkStructure.RandomiseWeights (0.5d);
			PrepData ();
			int epoch = 0;
			int time = 0;
			while (true) {
				epoch++;
				time++;
				if (time % 100 == 0) {
					Console.Clear ();
					Console.SetCursorPosition (0, 0);
				}
		
				for (int x=0; x<8; x++) {
					
					PresentData (x);
					ForwardPass ();
					ReversePass (x, 0);				

					
					if (time % 100 == 0)
						Console.WriteLine (InputLayer.GetNodes () [0].GetValue () + "-" + InputLayer.GetNodes () [1].GetValue () + "-" + InputLayer.GetNodes () [2].GetValue () + "  -  " + Math.Round (OutputLayer.GetNodes () [0].GetValue (), 3) + "  -  " + Math.Round (OutputLayer.GetNodes () [1].GetValue (), 3) + "  -  " + Math.Round (OutputLayer.GetNodes () [2].GetValue (), 3));
				}
			}
			Console.ReadKey ();
		}

		public static void BuildStructureXOR ()
		{
			InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
				InputLayerNodes.Add (new Cranium.Structure.Node.Base (InputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			
			InputLayer.SetNodes (InputLayerNodes);
			
			HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<6; i++)
				HiddenLayerNodes.Add (new Cranium.Structure.Node.Base (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			//HiddenLayerNodes.Add (new Cranium.Structure.Node.Bias (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			HiddenLayer.SetNodes (HiddenLayerNodes);
			
			HiddenLayer2 = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes2 = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
				HiddenLayerNodes2.Add (new Cranium.Structure.Node.Base (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			//HiddenLayerNodes.Add (new Cranium.Structure.Node.Bias (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			HiddenLayer2.SetNodes (HiddenLayerNodes2);
			
			OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<3; i++)
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
			InputData = new Int32[24];
			OutputData = new Int32[24];
			
			int i = 0;
			int o = 0;
			
			InputData [i++] = 0;
			InputData [i++] = 0;
			InputData [i++] = 0;
			OutputData [o++] = 0;
			OutputData [o++] = 0;
			OutputData [o++] = 0;
			
			InputData [i++] = 1;
			InputData [i++] = 0;
			InputData [i++] = 0;
			OutputData [o++] = 1;
			OutputData [o++] = 0;
			OutputData [o++] = 0;
			
			InputData [i++] = 0;
			InputData [i++] = 1;
			InputData [i++] = 0;
			OutputData [o++] = 1;
			OutputData [o++] = 0;
			OutputData [o++] = 0;
			
			InputData [i++] = 0;
			InputData [i++] = 0;
			InputData [i++] = 1;
			OutputData [o++] = 1;
			OutputData [o++] = 0;
			OutputData [o++] = 0;
			
			InputData [i++] = 1;
			InputData [i++] = 1;
			InputData [i++] = 0;
			OutputData [o++] = 1;
			OutputData [o++] = 1;
			OutputData [o++] = 0;
			
			InputData [i++] = 0;
			InputData [i++] = 1;
			InputData [i++] = 1;
			OutputData [o++] = 1;
			OutputData [o++] = 0;
			OutputData [o++] = 1;
			
			InputData [i++] = 1;
			InputData [i++] = 0;
			InputData [i++] = 1;
			OutputData [o++] = 1;
			OutputData [o++] = 0;
			OutputData [o++] = 0;
			
			InputData [i++] = 1;
			InputData [i++] = 1;
			InputData [i++] = 1;
			OutputData [o++] = 0;
			OutputData [o++] = 1;
			OutputData [o++] = 1;

		}
		
		public static void PresentData (int row)
		{
			InputLayer.GetNodes () [0].SetValue (InputData [(row * 3)]);
			InputLayer.GetNodes () [1].SetValue (InputData [(row * 3) + 1]);
			InputLayer.GetNodes () [2].SetValue (InputData [(row * 3) + 2]);
		}
		
		public static void ForwardPass ()
		{
			TestNetworkStructure.FowardPass ();	
		}
		
		public static void ReversePass (int row, Double momentum)
		{
			for (int x=0; x<3; x++) {
				((Structure.Node.Output)OutputLayer.GetNodes () [x]).SetTargetValue (OutputData [(row * 3) + x]);
			}
			OutputLayer.ReversePass ();
		}
	}
}

