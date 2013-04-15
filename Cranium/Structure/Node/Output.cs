// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using Cranium.Structure.Node;

namespace Structure.Node
{
	public class Output : Base
	{
		protected double _TargetValue;
		
		public override void CalculateError ()
		{
			_Error =  _Value - _TargetValue;
		}	
	}
}

