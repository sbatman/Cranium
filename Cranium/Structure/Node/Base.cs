// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using System.Collections.Generic;

namespace Cranium.Structure.Node
{
	public class Base : IDisposable
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
		
		/// <summary>
		/// Causes the node to calucate its new value as a sum of the weights by values of reverse connected nodes and then passes this value by the Activation function assigned
		/// </summary>
		public virtual void CalculateValue ()
		{
			if (_T_FowardWeights == null || _T_ReverseWeights == null)
				BakeLists ();
			_Value = 0;
			foreach (Weight.Base W in _T_ReverseWeights)
				_Value += W.GetNodeA ().GetValue () * W.GetWeight ();			
			_Value = _ActivationFunction.Compute (_Value);
		}
		
		/// <summary>
		/// Returns the last calucated value of the node
		/// </summary>
		/// <returns>
		/// The value.
		/// </returns>
		public virtual Double GetValue ()
		{
			return _Value;	
		}
		
		/// <summary>
		/// Bakes down the forward and reverse list of weights for optimisation sake. This is performed autmaticaly before any node functions that requires the weights
		/// </summary>
		public virtual void BakeLists ()
		{
			_T_FowardWeights = _FowardWeights.ToArray ();
			_T_ReverseWeights = _ReverseWeights.ToArray ();
		}
		
		
		/// <summary>
		/// Returns the currently assigned forward weights
		/// </summary>
		/// <returns>
		/// The foward weights.
		/// </returns>
		public virtual Weight.Base[] GetFowardWeights ()
		{
			if (_T_FowardWeights == null || _T_ReverseWeights == null)
				BakeLists ();
			return _T_FowardWeights;
		}
		
		
		/// <summary>
		/// Returns the currently assigned list of reverse weights
		/// </summary>
		/// <returns>
		/// The reverse weights.
		/// </returns>
		public virtual Weight.Base[] GetReverseWeights ()
		{
			if (_T_FowardWeights == null || _T_ReverseWeights == null)
				BakeLists ();
			return _T_ReverseWeights;	
		}

		
		/// <summary>
		/// Connects a second node to this one, building the correct weight and adding it to the list of weights that are updated when required
		/// </summary>
		/// <param name='nodeToConnect'>
		/// Node to connect.
		/// </param>
		/// <param name='connectionDirectionToNode'>
		/// Connection direction to node.
		/// </param>
		/// <param name='startingWeight'>
		/// Starting weight.
		/// </param>
		public virtual void ConnectToNode (Base nodeToConnect, Weight.Base.ConnectionDirection connectionDirectionToNode, float startingWeight)
		{
			Weight.Base theNewWeight;
			switch (connectionDirectionToNode) {
			case Weight.Base.ConnectionDirection.Forward:
				_T_FowardWeights = null;
				theNewWeight = new Structure.Weight.Base (this, nodeToConnect, startingWeight);
				_FowardWeights.Add (theNewWeight);
				nodeToConnect._ReverseWeights.Add (theNewWeight);
				break;
			case Weight.Base.ConnectionDirection.Reverse:
				_T_ReverseWeights = null;
				theNewWeight = new Structure.Weight.Base (nodeToConnect, this, startingWeight);
				_ReverseWeights.Add (theNewWeight);
				nodeToConnect._FowardWeights.Add (theNewWeight);
				break;
			}
		}
		
		public virtual void SetValue (Double newValue)
		{
			_Value = newValue;	
		}
		
		public virtual void DestroyAllConnections ()
		{
			foreach (Weight.Base w in _T_FowardWeights)
				w.Dispose ();	
			_FowardWeights.Clear ();
			_T_FowardWeights = null;
			foreach (Weight.Base w in _T_ReverseWeights)
				w.Dispose ();
			_ReverseWeights.Clear ();
			_T_ReverseWeights = null;
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			_ParentLayer = null;
			_FowardWeights.Clear ();
			_FowardWeights = null;
			_ReverseWeights.Clear ();
			_ReverseWeights = null;
			_ActivationFunction.Dispose ();
			_ActivationFunction = null;
		}
		#endregion
	}
}

