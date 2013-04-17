// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using Cranium.Structure.Node;
using Cranium.Structure;

namespace Cranium.Structure.Node
{
	public class Output : Base
	{
		protected Double _TargetValue;
		
		public Output (Cranium.Structure.Layer.Base parentLayer, Cranium.Structure.ActivationFunction.Base activationFunction):base(parentLayer,activationFunction)
		{
			
		}
		
		public override void CalculateError ()
		{
			_Error = ((1 - _Value) * (1 + _Value)) * (_TargetValue - _Value);

		}

		public virtual void SetTargetValue (Double targetValue)
		{
			_TargetValue = targetValue;
		}
	}
}

