// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Structure.ActivationFunction
{
	public class Linear : Base
	{
		public override double Compute (double input)
		{
			return input;
		}
		public override double ComputeDerivative (double input)
		{
			return 1;
		}
		public override void Dispose ()
		{
			
		}
	}
}

