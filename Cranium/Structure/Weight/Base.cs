// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Structure.Weight
{
	public class Base : IDisposable
	{
		public enum ConnectionDirection
		{
			Reverse,
			Forward
		};
		
		protected Nodes.Base _NodeA;
		protected Nodes.Base _NodeB;
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
			_InitialValue=weight;
			_PendingWeightChange=0;
			_PendingWeightChangeCount = 0;
			_PastWeightChange=0;
		}
		
		public virtual Node.Base GetNodeA ()
		{
			return _NodeA;
		}

		public virtual Node.Base GetNodeB ()
		{
			return _NodeB;
		}

		public virtual Double GetWeight ()
		{
			return _Weight;	
		}

		public virtual Double GetTotalChange ()
		{
			return _Weight - _InitialValue;
		}

		public virtual void AddWeightChange (double weightModification)
		{
			_PendingWeightChange += weightModification;
			_PendingWeightChangeCount++;
		}

		public virtual void SetWeight (double newWeight)
		{
			_Weight = newWeight;	
		}

		public virtual void ApplyPendingWeightChanges ()
		{
			_PastWeightChange = (_PendingWeightChange / _PendingWeightChangeCount);
			_Weight +=_PastWeightChange;	
			_PendingWeightChange=0;
			_PendingWeightChangeCount=0;
		}

		#region IDisposable implementation
		void IDisposable.Dispose ()
		{
			_NodeA = null;
			_NodeB = null;
		}
		#endregion
	}
}

