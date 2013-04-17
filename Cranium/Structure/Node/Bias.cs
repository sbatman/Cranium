// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using Cranium.Structure.Node;

namespace Cranium.Structure.Node
{
	public class Bias : Base
	{
		public Bias (Layer.Base parentLayer, ActivationFunction.Base activationFunction) : base(parentLayer,activationFunction)
		{
		}

		public override Double GetValue ()
		{
			return 1;
		}

		public override double GetError ()
		{
			return 0;
		}

		public override void ConnectToNode (Base nodeToConnect, Weight.Base.ConnectionDirection connectionDirectionToNode, float startingWeight)
		{
			if (connectionDirectionToNode == Weight.Base.ConnectionDirection.Reverse)
				return;
			base.ConnectToNode (nodeToConnect, connectionDirectionToNode, startingWeight);
		}
	}
}

