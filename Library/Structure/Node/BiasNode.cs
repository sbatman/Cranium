// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;

#endregion

namespace Cranium.Lib.Structure.Node
{
	/// <summary>
	///    A Bias node retains a static value at all times and is useful in some neural network structures.
	/// </summary>
	[Serializable]
	public class BiasNode : BaseNode
	{
		protected Double _BiasValue = 1;

		/// <summary>
		///    Initializes a new instance of the <see cref="BiasNode" /> class.
		/// </summary>
		/// <param name='parentLayer'>
		///    Parent layer.
		/// </param>
		/// <param name='activationFunction'>
		///    Activation function.
		/// </param>
		public BiasNode(Layer.Layer parentLayer, AF activationFunction) : base(parentLayer, activationFunction)
		{
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="BiasNode" /> class. Used by the Serializer.
		/// </summary>
		/// <param name='info'>
		///    Info.
		/// </param>
		/// <param name='context'>
		///    Context.
		/// </param>
		public BiasNode(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_BiasValue = info.GetDouble("_BiasValue");
		}

		/// <summary>
		///    Returns the static value of the node
		/// </summary>
		/// <returns>
		///    The value.
		/// </returns>
		[Pure]
		public override Double GetValue()
		{
			return _BiasValue;
		}

		/// <summary>
		///    Returns the error of the node, which in the case of the bias node is always 0
		/// </summary>
		/// <returns>
		///    The error.
		/// </returns>
		[Pure]
		public override Double GetError()
		{
			return 0;
		}

		/// <summary>
		///    Sets the new static value of this node
		/// </summary>
		/// <param name='newValue'>
		///    New value.
		/// </param>
		public override void SetValue(Double newValue)
		{
			_BiasValue = newValue;
		}

		/// <summary>
		///    Connects a second node to this one, building the correct weight and adding it to the list of weights that are
		///    updated when required
		/// </summary>
		/// <param name='nodeToConnect'>
		///    Node to connect.
		/// </param>
		/// <param name='connectionDirectionToNode'>
		///    Connection direction to node.
		/// </param>
		/// <param name='startingWeight'>
		///    Starting weight.
		/// </param>
		public override void ConnectToNode(BaseNode nodeToConnect, Weight.Weight.ConnectionDirection connectionDirectionToNode, Single startingWeight)
		{
			if (connectionDirectionToNode == Weight.Weight.ConnectionDirection.REVERSE) return;
			base.ConnectToNode(nodeToConnect, connectionDirectionToNode, startingWeight);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("_BiasValue", _BiasValue);
		}
	}
}