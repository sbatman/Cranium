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
	/// <summary>
	/// This is the base weight class and acts as a standard weight between two nodes. weight changes can be applied immediatly
	/// or added to a pending list and applied at a later stage.
	/// </summary>
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
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.Weight.Base"/> class.
		/// </summary>
		/// <param name='nodeA'>
		/// Node a.
		/// </param>
		/// <param name='nodeB'>
		/// Node b.
		/// </param>
		/// <param name='weight'>
		/// Initial Weight.
		/// </param>
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
		
		/// <summary>
		/// Gets the previous total weight change caused by ApplyPendingWeightChanges
		/// </summary>
		/// <returns>
		/// The past weight change.
		/// </returns>
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
		public virtual void Dispose ( )
		{
			NodeA = null;
			NodeB = null;
		}
		#endregion
	}
}

