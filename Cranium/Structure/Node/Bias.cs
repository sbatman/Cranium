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

