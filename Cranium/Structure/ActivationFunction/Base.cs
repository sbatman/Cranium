// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Structure.ActivationFunction
{
	public abstract class Base
	{
		/// <summary>
		/// Returns the input after running through the activation function.
		/// </summary>
		/// <param name='input'>
		/// The value to pass to the activation function
		/// </param>
		public abstract Double Compute(Double input);
		/// <summary>
		/// Computes the derivative using the activation function.
		/// </summary>
		/// <returns>
		/// The derivative.
		/// </returns>
		/// <param name='input'>
		/// Input.
		/// </param>
		public abstract Double ComputeDerivative(Double input);
	}
}

