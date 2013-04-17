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

namespace Cranium.Structure.ActivationFunction
{
	/// <summary>
	/// This activation function returns exactly what it is fed, this is extreamly useful fordata being 
	/// fed in that has already been cooked external of the network
	/// </summary>
	public class Linear : Base
	{
		public override Double Compute (Double input)
		{
			return input;
		}

		public override Double ComputeDerivative (Double input)
		{
			return 1;
		}

		public override void Dispose ()
		{
			
		}
	}
}

