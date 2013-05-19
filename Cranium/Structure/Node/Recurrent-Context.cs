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

namespace Cranium.Structure.Node
{
	/// <summary>
	/// This recursive node acts differntly from as standard node as it has a source node from which it generates its value,
	/// this is based on the Rate of update pased as the constructor.
	/// </summary>
	[Serializable]
	public class Recurrent_Context : Base
	{
		/// <summary>
		/// The node which this node uses to calculate its value
		/// </summary>
		protected Node.Base _SourceNode;
		/// <summary>
		/// The the persentage of the source nodes value that is used in calcualate the nodes new value
		/// </summary>
		protected double _RateOfUpdate;
		/// <summary>
		/// The initial value of the node.
		/// </summary>
		protected double _StartValue = 0.0f;

		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.Node.Recurrent_Context"/> class.
		/// </summary>
		/// <param name='sourceNode'>
		/// Source node.
		/// </param>
		/// <param name='rateOfUpdate'>
		/// Rate of update.
		/// </param>
		/// <param name='parentLayer'>
		/// Parent layer.
		/// </param>
		/// <param name='activationFunction'>
		/// Activation function.
		/// </param>
		public Recurrent_Context ( Node.Base sourceNode, double rateOfUpdate, Layer.Base parentLayer, ActivationFunction.Base activationFunction ) : base(parentLayer,activationFunction)
		{
			_Value = 0.5f;
			_SourceNode = sourceNode;
			_RateOfUpdate = rateOfUpdate;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.Node.Recurrent_Context"/> class. Used by the Serializer.
		/// </summary>
		/// <param name='info'>
		/// Info.
		/// </param>
		/// <param name='context'>
		/// Context.
		/// </param>
		public Recurrent_Context ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ):base(info,context)
		{
			_SourceNode = ( Node.Base )info.GetValue ( "_SourceNode", typeof (Node.Base) );
			_RateOfUpdate = info.GetDouble ( "_RateOfUpdate" );
			_StartValue = info.GetDouble ( "_StartValue" );
		}
		
		/// <summary>
		/// Update this nodes value.
		/// </summary>
		public virtual void Update ( )
		{
			_Value = ( _Value * ( 1 - _RateOfUpdate ) ) + ( _SourceNode.GetValue () * _RateOfUpdate );
		}
		
		/// <summary>
		/// Sets the start value of this node.
		/// </summary>
		/// <param name='startValue'>
		/// Start value.
		/// </param>
		public virtual void SetStartValue ( double startValue )
		{
			_StartValue = startValue;
		}
		
		public override void GetObjectData ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			base.GetObjectData ( info, context );
			info.AddValue ( "_SourceNode", _SourceNode, _SourceNode.GetType () );
			info.AddValue ( "_RateOfUpdate", _RateOfUpdate );
			info.AddValue ( "_StartValue", _StartValue );
		}
	}
}

