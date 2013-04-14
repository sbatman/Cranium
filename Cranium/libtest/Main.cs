using System;
using Cranium.Structure;

namespace Cranium.libtest
{
	class MainClass
	{
		Network TestNetworkStructure;
		Int32[] InputData;
		Int32[] OutputData;

		public static void Main (string[] args)
		{
			TestNetworkStructure = new Network ();
			BuildStructure();
			PrepData();
		}

		public static void BuildStructure ()
		{
			Cranium.Structure.Layer.Base InputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> InputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<2; i++)
				InputLayerNodes.Add (new Cranium.Structure.Node.Base (InputLayer, new Cranium.Structure.ActivationFunction.Linear ()));
			InputLayer.SetNodes (InputLayerNodes);
			
			Cranium.Structure.Layer.Base HiddenLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> HiddenLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<10; i++)
				HiddenLayerNodes.Add (new Cranium.Structure.Node.Base (HiddenLayer, new Cranium.Structure.ActivationFunction.BinarySigmoid ()));
			HiddenLayer.SetNodes (HiddenLayerNodes);
			
			Cranium.Structure.Layer.Base OutputLayer = new Cranium.Structure.Layer.Base ();
			List<Cranium.Structure.Node.Base> OuputLayerNodes = new List<Cranium.Structure.Node.Base> ();
			for (int i=0; i<1; i++)
				OuputLayerNodes.Add (new Cranium.Structure.Node.Base (OutputLayer, new Cranium.Structure.ActivationFunction.Linear ()));
			HiddenLayer.SetNodes (OuputLayerNodes);
			
			InputLayer.ConnectFowardLayer (HiddenLayer);
			HiddenLayer.ConnectFowardLayer (OutputLayer);
			
			TestNetworkStructure.AddLayer (InputLayer);			
			TestNetworkStructure.AddLayer (HiddenLayer);			
			TestNetworkStructure.AddLayer (OutputLayer);
			
			foreach (Cranium.Structure.Layer.Base layer in TestNetworkStructure.GetCurrentLayers())
				layer.PopulateNodeConnections ();
			
									
		}
		
		public static void PrepData()
		{
			InputData = new Int32[8];
			OutputData = new Int32[4];
			
			InputData[0] = 0;
			InputData[1] = 0;
			OutputData[0] = 0;
			
			InputData[2] = 1;
			InputData[3] = 0;
			OutputData[1] = 1;
			
			InputData[4] = 0;
			InputData[5] = 1;
			OutputData[2] = 1;
			
			InputData[6] = 1;
			InputData[7] = 1;
			OutputData[3] = 0;
		}
	}
}
