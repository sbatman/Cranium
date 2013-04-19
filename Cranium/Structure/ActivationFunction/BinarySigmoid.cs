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
	public class BinarySigmoid :Base
	{
		public override Double Compute (Double input)
		{
			Double d = (Double)1d / (1d + Math.Exp (0d - (Double)input));
			if (Double.IsNaN (d) || Double.IsInfinity (d))
				throw(new Exception ("Activation Function Error"));
			return d;
		}

		public override Double ComputeDerivative (Double input)
		{
			Double d = (Double)(Compute (input) * (1d - Compute (input)));
			if (Double.IsNaN (d) || Double.IsInfinity (d))
				throw(new Exception ("Activation Function Error"));
			return d;
		}

		public override void Dispose ()
		{
			
		}
	}
}

