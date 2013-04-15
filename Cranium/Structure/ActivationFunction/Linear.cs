// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Cranium.Structure.ActivationFunction
{
	/// <summary>
	/// This activation function returns exactly what it is fed, this is extreamly useful fordata being 
	/// fed in that has already been cooked external of the network
	/// </summary>
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

