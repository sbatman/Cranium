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
	/// This activation function scales the input into the range of -1 to 1 which is very important for the majority
	/// or neural network structures that wish to learn signal data.
	/// </summary>
	public class Tanh : Base
	{
		public override double Compute (double input)
		{
			Double D = Math.Tanh(input);
			if(Double.IsNaN(D) || Double.IsInfinity(D)) throw(new Exception("Activation Function Error"));
			return D;
		}
		public override double ComputeDerivative (double input)
		{
			Double D = 1/ Math.Pow(Math.Cosh(input),2);
			if(Double.IsNaN(D) || Double.IsInfinity(D)) throw(new Exception("Activation Function Error"));
			return D;
		}
		public override void Dispose ()
		{
			
		}
	}
}

