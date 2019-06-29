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
	///    This activation function will return 1 when >= threshold else 0, its not recommended you use this in a back propagation
	///    network
	/// </summary>
	[Serializable]
	public class StepAF : AF
	{
		/// <summary>
		///    The Point at which the value must be at or above to for the activation function to return one else zero
		/// </summary>
		protected Double _ActivationPoint;

		/// <summary>
		///    Initializes a new instance of the <see cref="StepAF" /> class.
		/// </summary>
		/// <param name='activationPoint'>
		///    Activation point.
		/// </param>
		public StepAF(Double activationPoint)
		{
			_ActivationPoint = activationPoint;
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="StepAF" /> class.
		/// </summary>
		/// <param name='info'>
		///    Info.
		/// </param>
		/// <param name='context'>
		///    Context.
		/// </param>
		public StepAF(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_ActivationPoint = info.GetDouble("_ActivationPoint");
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
			return input >= _ActivationPoint ? 1 : 0;
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
			return 1;
		}

		public override void Dispose()
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_ActivationPoint", _ActivationPoint);
		}
	}
}