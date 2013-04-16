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
		protected double _TargetValue;
		
		public Output (Cranium.Structure.Layer.Base parentLayer,Cranium.Structure.ActivationFunction.Base activationFunction):base(parentLayer,activationFunction)
		{
			
		}
		
		public override void CalculateError ()
		{
			_Error =  _Value - _TargetValue;
		}	
		public virtual void SetTargetValue(double targetValue)
		{
			_TargetValue=targetValue;
		}
	}
}

