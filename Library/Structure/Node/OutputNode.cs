// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;

#endregion

namespace Cranium.Lib.Structure.Node
{
	/// <summary>
	///    The output node functions differntly from normal nodes as its error is calcuated from the targetvalue rather than
	///    the error of foward nodes.
	/// </summary>
	[Serializable]
	public class OutputNode : BaseNode
	{
		protected Double _TargetValue;

		/// <summary>
		///    Initializes a new instance of the <see cref="OutputNode" /> class.
		/// </summary>
		/// <param name='parentLayer'>
		///    Parent layer.
		/// </param>
		/// <param name='activationFunction'>
		///    Activation function.
		/// </param>
		public OutputNode(Layer.Layer parentLayer, AF activationFunction) : base(parentLayer, activationFunction)
		{
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="OutputNode" /> class. Used by the Serializer
		/// </summary>
		/// <param name='info'>
		///    Info.
		/// </param>
		/// <param name='context'>
		///    Context.
		/// </param>
		public OutputNode(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_TargetValue = info.GetDouble("_TargetValue");
		}

		/// <summary>
		///    Calculates the error of the node based on its distance from the target value
		/// </summary>
		public override void CalculateError()
		{
			_Error = _TargetValue - _Value;
		}

		/// <summary>
		///    Sets the target value, used for error calculation
		/// </summary>
		/// <param name='targetValue'>
		///    Target value.
		/// </param>
		public virtual void SetTargetValue(Double targetValue)
		{
			_TargetValue = targetValue;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("_TargetValue", _TargetValue);
		}
	}
}