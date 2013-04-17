// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Cranium.Structure.Weight
{
	public class Base : IDisposable
	{
		public enum ConnectionDirection
		{
			Reverse,
			Forward
		};
		
		protected Node.Base _NodeA;
		protected Node.Base _NodeB;
		protected Double _Weight;
		protected Double _InitialValue;
		protected Double _PendingWeightChange;
		protected Double _PendingWeightChangeCount;
		protected Double _PastWeightChange;
		
		public Base (Node.Base nodeA, Node.Base nodeB, Double weight)
		{
			_NodeA = nodeA;
			_NodeB = nodeB;
			_Weight = weight;
			_InitialValue = weight;
			_PendingWeightChange = 0;
			_PendingWeightChangeCount = 0;
			_PastWeightChange = 0;
		}
		
		/// <summary>
		/// Gets the first node in the weighting.
		/// </summary>
		/// <returns>
		/// The node a.
		/// </returns>
		public virtual Node.Base GetNodeA ()
		{
			return _NodeA;
		}

		/// <summary>
		/// Gets the second node in the weighting.
		/// </summary>
		/// <returns>
		/// The node b.
		/// </returns>
		public virtual Node.Base GetNodeB ()
		{
			return _NodeB;
		}
		
		/// <summary>
		/// Gets the current weight.
		/// </summary>
		/// <returns>
		/// The weight.
		/// </returns>
		public virtual Double GetWeight ()
		{
			return _Weight;	
		}

		/// <summary>
		/// Gets the total change from the initial value the weight was set with.
		/// </summary>
		/// <returns>
		/// The total change.
		/// </returns>
		public virtual Double GetTotalChange ()
		{
			return _Weight - _InitialValue;
		}
		
		/// <summary>
		/// Adds a pending weight change
		/// </summary>
		/// <param name='weightModification'>
		/// Weight modification.
		/// </param>
		public virtual void AddWeightChange (Double weightModification)
		{
			if (Double.IsNaN (_Weight) || Double.IsInfinity (weightModification))
				throw(new Exception ("Weight Error"));
			_PendingWeightChange += weightModification;
			_PendingWeightChangeCount++;
		}
		
		/// <summary>
		/// Sets the current weight.
		/// </summary>
		/// <param name='newWeight'>
		/// New weight.
		/// </param>
		public virtual void SetWeight (Double newWeight)
		{
			_Weight = newWeight;	
		}
		
		public virtual double GetPastWeightChange ()
		{
			return _PastWeightChange;	
		}
		
		/// <summary>
		/// Applies all pending weightchanges and clears the pending change.
		/// </summary>
		public virtual void ApplyPendingWeightChanges ()
		{
			_PastWeightChange = (_PendingWeightChange / _PendingWeightChangeCount) ;
			_Weight += _PastWeightChange;	
									

			if (Double.IsNaN (_Weight) || Double.IsInfinity (_Weight))
				throw(new Exception ("Weight Error"));
			_PendingWeightChange = 0;
			_PendingWeightChangeCount = 0;
		}
		
		/// <summary>
		/// Clears the pending weight changes.
		/// </summary>
		public virtual void ClearPendingWeightChanges ()
		{
			_PendingWeightChange = 0;
			_PendingWeightChangeCount = 0;	
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			_NodeA = null;
			_NodeB = null;
		}
		#endregion
	}
}

