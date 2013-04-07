// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Structure.ActivationFunction
{
	public class Base
	{
		public abstract Double Comput(Double input);
		public abstract Double ComputeDerivative(Double input);
	}
}

