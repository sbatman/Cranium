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
	/// Gaussian Activation function acts as a bellcurve activation function
	/// </summary>
	[Serializable]
	public class Gaussian : Base
	{
		/// <summary>
		/// The steepness of the bellcurve.
		/// </summary>
		protected double _Steepness;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.ActivationFunction.Gaussian"/> class.
		/// </summary>
		/// <param name='steepness'>
		/// Steepness.
		/// </param>
		public Gaussian ( Double steepness )
		{
			_Steepness = steepness;	
		}
		#region implemented abstract members of Cranium.Structure.ActivationFunction.Base
		public override double Compute ( double input )
		{
			return Math.Exp ( -Math.Pow ( _Steepness * input, 2.0d ) );
		}

		/// <summary>
		/// Computes the derivative using the activation function.
		/// </summary>
		/// <returns>
		/// The derivative.
		/// </returns>
		/// <param name='input'>
		/// Input.
		/// </param>
		public override double ComputeDerivative ( double input )
		{
			return -2 * input * _Steepness * Compute ( input ) * input;
		}

		public override void Dispose ( )
		{
		
		}

		public override void GetObjectData ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			info.AddValue ( "_Steepness", _Steepness );
		}
		#endregion
	}
}

