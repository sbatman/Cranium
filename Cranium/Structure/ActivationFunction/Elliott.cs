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
	/// The Elliott activations functions acts as a computationaly cheaper version of tanH although with known problems with reaching the lowest
	/// errors and becoming trapped in local minima. This activation function is good for prototyping netowrk structures, however in many cases
	/// it should not be used for practival implementations of networks. http://drum.lib.umd.edu/handle/1903/5355
	/// </summary>
	[Serializable]
	public class Elliott :Base
	{
		private double _Scale = 1;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.ActivationFunction.Elliott"/> class.
		/// </summary>
		public Elliott () : base()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.ActivationFunction.Elliott"/> class. Used by the serializer.
		/// </summary>
		/// <param name='info'>
		/// Info.
		/// </param>
		/// <param name='context'>
		/// Context.
		/// </param>
		public Elliott ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ) : base()
		{
			_Scale = info.GetDouble ( "_Scale" );	
		}

		#region implemented abstract members of Cranium.Structure.ActivationFunction.Base
		/// <summary>
		/// Returns the input after running through the activation function.
		/// </summary>
		/// <param name='input'>
		/// The value to pass to the activation function
		/// </param>
		public override double Compute ( double input )
		{
			return  ( input * _Scale ) / ( 1 + Math.Abs ( input * _Scale ) ); 
		}
		
		/// <summary>
		/// Computes the derivative using the activation function.
		/// </summary>
		/// <returns>
		/// The derivative.
		/// </returns>
		/// <param name='input'>
		/// Input.
		/// </param>
		public override double ComputeDerivative ( double input )
		{
			
			return  _Scale / ( Math.Pow ( ( 1.0d + Math.Abs ( input * _Scale ) ), 2 ) );
		}

		public override void Dispose ( )
		{
			
		}

		public override void GetObjectData ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			info.AddValue ( "_Scale", _Scale );
		}
		#endregion
	}
}

