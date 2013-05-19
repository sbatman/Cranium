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
using System.Runtime.Serialization;

namespace Cranium.Structure.ActivationFunction
{
	/// <summary>
	/// A base class for activation functions, Exstend this when implementing new activation functions
	/// </summary>
	[Serializable]
	public abstract class Base :IDisposable, ISerializable
	{
		/// <summary>
		/// Returns the input after running through the activation function.
		/// </summary>
		/// <param name='input'>
		/// The value to pass to the activation function
		/// </param>
		public abstract Double Compute ( Double input );
		/// <summary>
		/// Computes the derivative using the activation function.
		/// </summary>
		/// <returns>
		/// The derivative.
		/// </returns>
		/// <param name='input'>
		/// Input.
		/// </param>
		public abstract Double ComputeDerivative ( Double input );

		#region IDisposable implementation
		public abstract void Dispose ( );
		#endregion5

		#region ISerializable implementation
		public Base(SerializationInfo info, StreamingContext context )
		{
		}
		public abstract void GetObjectData ( SerializationInfo info, StreamingContext context );
		#endregion
	}
}

