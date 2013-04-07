// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using System.Collections.Generic;

namespace Structure.Node
{
	public class Base
	{
		protected Double _Value;
		protected Layer.Base _ParentLayer;
		protected List<Weight.Base> _FowardWeights;
		protected List<Weight.Base> _ReverseWeights;
		protected ActivationFunction.Base _ActivationFunction;
		
		//Baked or Temp fields
		protected Weight.Base[] _T_FowardWeights;
		protected Weight.Base[] _T_ReverseWeights;
		
		public Base (Layer.Base layer, ActivationFunction.Base activationFunction)
		{
			_ParentLayer = layer;
			_ActivationFunction = activationFunction;
		}

		public virtual void CalculateValue ()
		{
			if (_T_FowardWeights == null || _T_ReverseWeights == null)
				BakeLists ();
			_Value = 0;
			//Calculate new value
			_Value = _ActivationFunction.Compute (_Value);
		}

		public virtual Double GetValue ()
		{
			return _Value;	
		}

		public virtual void BakeLists ()
		{
			_T_FowardWeights = _FowardWeights.ToArray ();
			_T_ReverseWeights = _ReverseWeights.ToArray ();
		}

		public virtual Weight.Base[] GetFowardWeights ()
		{
			if (_T_FowardWeights == null || _T_ReverseWeights == null)
				BakeLists ();
			return _T_FowardWeights;
		}

		public virtual Weight.Base[] GetReverseWeights ()
		{
			if (_T_FowardWeights == null || _T_ReverseWeights == null)
				BakeLists ();
			return _T_ReverseWeights;	
		}

		public virtual void ConnectToNode (Base nodeToConnect, Weight.Base.ConnectionDirection connectionDirectionToNode, float startingWeight)
		{
			Weight.Base theNewWeight;
			switch (connectionDirectionToNode) {
				case Weight.Base.ConnectionDirection.Forward:
				
					break;
				case Weight.Base.ConnectionDirection.Reverse:
				
					break;
			}
		}
	}
}

