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
			_Error = ( ( 1 - _Value ) * ( 1 + _Value ) ) * ( _TargetValue - _Value );

		}

		public virtual void SetTargetValue (Double targetValue)
		{
			_TargetValue = targetValue;
		}
	}
}

