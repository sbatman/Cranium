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

namespace Cranium.Structure.ActivationFunction
{
	/// <summary>
	/// This activation function will return 1 when >= threshold else 0, its not recomended you use this in a backprop netowrk
	/// </summary>
	public class Step : Base
	{
		protected double _ActivationPoint;
		
		public Step ( double activationPoint )
		{
			_ActivationPoint = activationPoint;	
		}

		public override Double Compute ( Double input )
		{
			return input >= _ActivationPoint ? 1 : 0;
		}

		public override Double ComputeDerivative ( Double input )
		{
			return 1;
		}

		public override void Dispose ( )
		{
			
		}
	}
}

