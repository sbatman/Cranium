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
	public class Recurrent_Context : Base
	{
		protected Node.Base _SourceNode;
		protected double _RateOfUpdate;
		protected double _StartValue = 0.5f;

		public Recurrent_Context ( Node.Base sourceNode, double rateOfUpdate, Layer.Base parentLayer, ActivationFunction.Base activationFunction ) : base(parentLayer,activationFunction)
		{
			_Value = 0.5f;
			_SourceNode = sourceNode;
			_RateOfUpdate = rateOfUpdate;
		}

		public override void CalculateValue ( )
		{
			return;
		}

		public virtual void Update ( )
		{
			_Value = ( _Value * ( 1 - _RateOfUpdate ) ) + ( _SourceNode.GetValue () * _RateOfUpdate );
		}

		public virtual void SetStartValue ( double startValue )
		{
			_StartValue = startValue;
		}
	}
}

