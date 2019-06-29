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
	///    This activation function scales the input into the range of -1 to 1 which is very important for the majority
	///    or neural network structures that wish to learn signal data.
	/// </summary>
	[Serializable]
	public class TanhAF : AF
	{
		/// <summary>
		///    Initializes a new instance of the <see cref="TanhAF" /> class.
		/// </summary>
		public TanhAF()
		{
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="TanhAF" /> class. Used by the serializer
		/// </summary>
		/// <param name='info'>
		///    Info.
		/// </param>
		/// <param name='context'>
		///    Context.
		/// </param>
		public TanhAF(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		///    Returns the input after running through the activation function.
		/// </summary>
		/// <param name='input'>
		///    The value to pass to the activation function
		/// </param>
		[Pure]
		public override Double Compute(Double input)
		{
			Double temp = Math.Exp(input * 2.0);
			return (temp - 1.0) / (temp + 1.0);
		}

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
		public override Double ComputeDerivative(Double input)
		{
			return 1 - Math.Pow(input, 2);
		}

		public override void Dispose()
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
	}
}