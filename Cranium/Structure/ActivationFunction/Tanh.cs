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
	/// This activation function scales the input into the range of -1 to 1 which is very important for the majority
	/// or neural network structures that wish to learn signal data.
	/// </summary>
	public class Tanh : Base
	{
		public override Double Compute (Double input)
		{
			Double d = (Math.Exp (input * 2.0) - 1.0) / (Math.Exp (input * 2.0) + 1.0);
			if (Double.IsNaN (d) || Double.IsInfinity (d))
				throw(new Exception ("Activation Function Error"));
			return d;
		}

		public override Double ComputeDerivative (Double input)
		{	
			Double d = 1 - Math.Pow ((input), 2);
			if (Double.IsNaN (d) || Double.IsInfinity (d))
				throw(new Exception ("Activation Function Error"));
			return d;
		}

		public override void Dispose ()
		{
			
		}
	}
}

