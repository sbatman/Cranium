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
	/// <inheritdoc />
	/// <summary>
	///    The Elliott activations functions acts as a computationaly cheaper version of tanH although with known problems
	///    with reaching the lowest
	///    errors and becoming trapped in local minima. This activation function is good for prototyping network structures,
	///    however in many cases
	///    it should not be used for practival implementations of networks. http://drum.lib.umd.edu/handle/1903/5355
	/// </summary>
	[Serializable]
	public class ElliottAF : AF
	{
		private Double _Scale = 1;

		/// <summary>
		///    A uniform scale applied to all values passing through the activation function
		/// </summary>
		public Double Scale
		{
			[Pure] get => _Scale;
			set => _Scale = value;
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="ElliottAF" /> class.
		/// </summary>
		public ElliottAF(Double scale = 1)
		{
			Scale = scale;
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="ElliottAF" /> class. Used by the serializer.
		/// </summary>
		/// <param name='info'>
		///    Info.
		/// </param>
		/// <param name='context'>
		///    Context.
		/// </param>
		public ElliottAF(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_Scale = info.GetDouble("_Scale");
		}

		#region implemented abstract members of Cranium.Structure.ActivationFunction.Base

		/// <summary>Returns the input after running through the activation function. </summary>
		/// <param name='input'>The value to pass to the activation function </param>
		public override Double Compute(Double input)
		{
			return input * _Scale / (1 + Math.Abs(input * _Scale));
		}

		/// <summary>     Computes the derivative using the activation function. </summary>
		/// <returns>     The derivative. </returns>
		/// <param name='input'>     Input. </param>
		public override Double ComputeDerivative(Double input)
		{
			return _Scale / Math.Pow(1.0d + Math.Abs(input * _Scale), 2);
		}

		/// <summary>
		///    Disposes of the activation function and any disposable objects owned
		/// </summary>
		public override void Dispose()
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_Scale", _Scale);
		}

		#endregion
	}
}