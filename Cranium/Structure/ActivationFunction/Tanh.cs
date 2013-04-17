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
		public override Double Compute (Double input)
		{
			Double D = (Math.Exp(input * 2.0) - 1.0) / (Math.Exp(input * 2.0) + 1.0);
			if(Double.IsNaN(D) || Double.IsInfinity(D)) throw(new Exception("Activation Function Error"));
			return D;
		}
		public override Double ComputeDerivative (Double input)
		{	
			Double D = 1-Math.Pow((input),2);
			if(Double.IsNaN(D) || Double.IsInfinity(D)) throw(new Exception("Activation Function Error"));
			return D;
		}
		public override void Dispose ()
		{
			
		}
	}
}

