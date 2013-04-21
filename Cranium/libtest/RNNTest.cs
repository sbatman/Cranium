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
		private static Cranium.Structure.Network TestNetworkStructure;
		private static  Cranium.Structure.Layer.Base InputLayer;
		private static  Cranium.Structure.Layer.Base HiddenLayer;
		private static  Cranium.Structure.Layer.Recurrent_Context ContextLayer;
		private static  Cranium.Structure.Layer.Base OutputLayer;
		private static  Int32[] InputData;
		private static  Int32[] OutputData;

		public static void Run ()
		{			
			TestNetworkStructure = new Network ();
			BuildStructure ();
			TestNetworkStructure.RandomiseWeights (0.001d);
			PrepData ();
			int epoch = 0;
			int time = 0;
			while (true) {
				epoch++;
				time++;
				if (time % 100 == 0) {
					Console.Clear ();
					Console.WriteLine ("RNNTest");
				}
		
				for (int x=0; x<4; x++) {
					foreach (Cranium.Structure.Node.Base n in ContextLayer.GetNodes())					
						n.SetValue (0);						
					for (int i=0; i<2; i++) {
						InputLayer.GetNodes () [0].SetValue (InputData [(x * 2) + i]);						
						ForwardPass ();
						ContextLayer.Update ();
					}
					ReversePass (x, 0);					
					if (time % 100 == 0)
						Console.WriteLine (InputData [x * 2] + "-" + InputData [(x * 2) + 1] + "  -  " + Math.Round (OutputLayer.GetNodes () [0].GetValue (), 3));
				}
			}
			Console.ReadKey ();
		}

		public static void BuildStructure ()
		{
			InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
				InputLayerNodes.Add (new Cranium.Structure.Node.Base (InputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));			
			InputLayer.SetNodes (InputLayerNodes);		
			
			HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<10; i++)
				HiddenLayerNodes.Add (new Cranium.Structure.Node.Base (HiddenLayer, new Cranium.Structure.ActivationFunction.Tanh ()));			
			HiddenLayer.SetNodes (HiddenLayerNodes);	
			
			ContextLayer = new Cranium.Structure.Layer.Recurrent_Context (4);
			
			OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
				OuputLayerNodes.Add (new Cranium.Structure.Node.Output (OutputLayer, new Cranium.Structure.ActivationFunction.Tanh ()));
			OutputLayer.SetNodes (OuputLayerNodes);
			
			ContextLayer.AddSourceNodes (InputLayerNodes);
			//ContextLayer.AddSourceNodes (HiddenLayerNodes);
			
			InputLayer.ConnectFowardLayer (HiddenLayer);			
			HiddenLayer.ConnectFowardLayer (OutputLayer);
			ContextLayer.ConnectFowardLayer (HiddenLayer);
			
			TestNetworkStructure.AddLayer (InputLayer);			
			TestNetworkStructure.AddLayer (HiddenLayer);	
			TestNetworkStructure.AddLayer (ContextLayer);	
			TestNetworkStructure.AddLayer (OutputLayer);
			
			foreach (Cranium.Structure.Layer.Base layer in TestNetworkStructure.GetCurrentLayers())
				layer.PopulateNodeConnections ();									
		}
		
		public static void PrepData ()
		{
			InputData = new Int32[8];
			OutputData = new Int32[4];
			
			int i=0;
			int o=0;
			
			InputData [i++] = 0;
			InputData [i++] = 0;
			OutputData [o++] = 0;
			
			InputData [i++] = 1;
			InputData [i++] = 0;
			OutputData [o++] = 1;
			
			InputData [i++] = 0;
			InputData [i++] = 1;
			OutputData [o++] = 1;
			
			InputData [i++] = 1;
			InputData [i++] = 1;
			OutputData [o++] = 0;
		}
	
		public static void ForwardPass ()
		{
			TestNetworkStructure.FowardPass ();	
		}
		
		public static void ReversePass (int row, Double momentum)
		{
			Structure.Node.Output outputNode = (Structure.Node.Output)(OutputLayer.GetNodes () [0]);
			outputNode.SetTargetValue (OutputData [row]);
			OutputLayer.ReversePass (0.006, 0.7);
		}
	}
}

