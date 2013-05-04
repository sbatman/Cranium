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

namespace Cranium.Structure.Weight
{
	public class Base : IDisposable
	{
		public enum ConnectionDirection
		{
			Reverse,
			Forward
		};
		
		public Node.Base NodeA;
        public Node.Base NodeB;
        public Double Weight;
		protected Double _InitialValue;
		protected Double _PendingWeightChange;
		protected Double _PendingWeightChangeCount;
		protected Double _PastWeightChange;
		
		public Base ( Node.Base nodeA, Node.Base nodeB, Double weight )
		{
			NodeA = nodeA;
			NodeB = nodeB;
			Weight = weight;
			_InitialValue = weight;
			_PendingWeightChange = 0;
			_PendingWeightChangeCount = 0;
			_PastWeightChange = 0;
		}
			
		/// <summary>
		/// Gets the total change from the initial value the weight was set with.
		/// </summary>
		/// <returns>
		/// The total change.
		/// </returns>
		public virtual Double GetTotalChange ( )
		{
			return Weight - _InitialValue;
		}
		
		/// <summary>
		/// Adds a pending weight change
		/// </summary>
		/// <param name='weightModification'>
		/// Weight modification.
		/// </param>
		public virtual void AddWeightChange ( Double weightModification )
		{
			if ( Double.IsNaN ( Weight ) || Double.IsInfinity ( weightModification ) )
			{
				throw( new Exception ( "Weight Error" ) );
			}
			_PendingWeightChange += weightModification;
			_PendingWeightChangeCount++;
		}
		
		/// <summary>
		/// Sets the current weight.
		/// </summary>
		/// <param name='newWeight'>
		/// New weight.
		/// </param>
		public virtual void SetWeight ( Double newWeight )
		{
			Weight = newWeight;	
		}
		
		public virtual double GetPastWeightChange ( )
		{
			return _PastWeightChange;	
		}
		
		/// <summary>
		/// Applies all pending weightchanges and clears the pending change.
		/// </summary>
		public virtual void ApplyPendingWeightChanges ( )
		{
			_PastWeightChange = ( _PendingWeightChange / _PendingWeightChangeCount );
			Weight += _PastWeightChange;									

			if ( Double.IsNaN ( Weight ) || Double.IsInfinity ( Weight ) )
			{
				throw( new Exception ( "Weight Error" ) );
			}
			_PendingWeightChange = 0;
			_PendingWeightChangeCount = 0;
		}
		
		/// <summary>
		/// Clears the pending weight changes.
		/// </summary>
		public virtual void ClearPendingWeightChanges ( )
		{
			_PendingWeightChange = 0;
			_PendingWeightChangeCount = 0;	
		}

		#region IDisposable implementation
		public void Dispose ( )
		{
			NodeA = null;
			NodeB = null;
		}
		#endregion
	}
}

