// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

#endregion

namespace Cranium.Lib.Structure.ActivationFunction
{
	/// <summary>
	///    A base class for activation functions, Exstend this when implementing new activation functions
	/// </summary>
	[Serializable]
	public abstract class AF : IDisposable, ISerializable
	{
		/// <summary>
		///    Initializes a new instance of the <see cref="AF" /> class.
		/// </summary>
		protected AF()
		{
		}

		#region IDisposable implementation

		public abstract void Dispose();

		#endregion

		/// <summary>
		///    Returns the input after running through the activation function.
		/// </summary>
		/// <param name='input'>
		///    The value to pass to the activation function
		/// </param>
		[Pure]
		public abstract Double Compute(Double input);

		/// <summary>
		///    Computes the derivative using the activation function.
		/// </summary>
		/// <returns>
		///    The derivative.
		/// </returns>
		/// <param name='input'>
		///    Input.
		/// </param>
		[Pure]
		public abstract Double ComputeDerivative(Double input);

		#region ISerializable implementation

		protected AF(SerializationInfo info, StreamingContext context)
		{
		}

		public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

		#endregion
	}
}