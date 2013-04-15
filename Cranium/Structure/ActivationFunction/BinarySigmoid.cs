// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Cranium.Structure.ActivationFunction
{
	public class BinarySigmoid :Base
	{
		public override Double Compute (Double input)
		{
			Double D = (Double)1d / (1d + Math.Exp (0d - (Double)input));
			if (Double.IsNaN (D) || Double.IsInfinity (D))
				throw(new Exception ("Activation Function Error"));
			return D;
		}

		public override Double ComputeDerivative (Double input)
		{
			Double D = (Double)(Compute (input) * (1d - Compute (input)));
			if (Double.IsNaN (D) || Double.IsInfinity (D))
				throw(new Exception ("Activation Function Error"));
			return D;
		}

		public override void Dispose ()
		{
			
		}
	}
}

