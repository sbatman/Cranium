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
	/// This activation function returns exactly what it is fed, this is extreamly useful fordata being fed in that has already been cooked external
	/// of the network. Lacking a derivative function however it is not suited for back progopgation networks.
	/// </summary>
	[Serializable]
	public class Linear : Base
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.ActivationFunction.Linear"/> class.
		/// </summary>
		public Linear () :base()
		{			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.ActivationFunction.Linear"/> class. Used by the Serializer.
		/// </summary>
		/// <param name='info'>
		/// Info.
		/// </param>
		/// <param name='context'>
		/// Context.
		/// </param>
		public Linear ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ):base()
		{			
		}
		
		/// <summary>
		/// Returns the input after running through the activation function.
		/// </summary>
		/// <param name='input'>
		/// The value to pass to the activation function
		/// </param>
		public override Double Compute ( Double input )
		{
			return input;
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
		public override Double ComputeDerivative ( Double input )
		{
			return 1;
		}

		public override void Dispose ( )
		{
			
		}

		public override void GetObjectData ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			
		}
	}
}

