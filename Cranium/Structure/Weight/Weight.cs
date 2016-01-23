#region info

// //////////////////////
//
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
//
// This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
//
// //////////////////////

#endregion

#region Usings

using System;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Structure.Weight
{
    /// <summary>
    ///     This is the base Value class and acts as a standard Value between two nodes. Value changes can be applied
    ///     immediatly
    ///     or added to a pending list and applied at a later stage.
    /// </summary>
    [Serializable]
    public class Weight : IDisposable, ISerializable
    {
        /// <summary>
        ///     Connection direction. Either foward or reverse, Added for code readability
        /// </summary>
        public enum ConnectionDirection
        {
            REVERSE,
            FORWARD
        }

        /// <summary>
        ///     Node a, this should be the reverse node
        /// </summary>
        public BaseNode NodeA;

        /// <summary>
        ///     Node b, this should be the forward node
        /// </summary>
        public BaseNode NodeB;

        /// <summary>
        ///     The current Value
        /// </summary>
        public Double Value;

        /// <summary>
        ///     The initial value of the Value
        /// </summary>
        protected Double _InitialValue;

        /// <summary>
        ///     The last total Value change
        /// </summary>
        protected Double _PastWeightChange;

        /// <summary>
        ///     The total pending Value change before count devision
        /// </summary>
        protected Double _PendingWeightChange;

        /// <summary>
        ///     The number of pending Value changes
        /// </summary>
        protected Double _PendingWeightChangeCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Structure.Weight.Weight" /> class.
        /// </summary>
        /// <param name='nodeA'>
        ///     Node a.
        /// </param>
        /// <param name='nodeB'>
        ///     Node b.
        /// </param>
        /// <param name='value'>
        ///     Initial Value.
        /// </param>
        public Weight(BaseNode nodeA, BaseNode nodeB, Double value)
        {
            NodeA = nodeA;
            NodeB = nodeB;
            Value = value;
            _InitialValue = value;
            _PendingWeightChange = 0;
            _PendingWeightChangeCount = 0;
            _PastWeightChange = 0;
        }

        /// <summary>
        ///     Gets the total change from the initial value the Value was set with.
        /// </summary>
        /// <returns>
        ///     The total change.
        /// </returns>
        public virtual Double GetTotalChange()
        {
            return Value - _InitialValue;
        }

        /// <summary>
        ///     Adds a pending Value change
        /// </summary>
        /// <param name='weightModification'>
        ///     Value modification.
        /// </param>
        public virtual void AddWeightChange(Double weightModification)
        {
            _PendingWeightChange += weightModification;
            _PendingWeightChangeCount++;
        }

        /// <summary>
        ///     Sets the current Value.
        /// </summary>
        /// <param name='newWeight'>
        ///     New Value.
        /// </param>
        public virtual void SetWeight(Double newWeight)
        {
            Value = newWeight;
        }

        /// <summary>
        ///     Gets the previous total Value change caused by ApplyPendingWeightChanges
        /// </summary>
        /// <returns>
        ///     The past Value change.
        /// </returns>
        public virtual Double GetPastWeightChange()
        {
            return _PastWeightChange;
        }

        /// <summary>
        ///     Applies all pending weightchanges and clears the pending change.
        /// </summary>
        public virtual void ApplyPendingWeightChanges()
        {
            if (_PendingWeightChangeCount >= 1)
            {
                _PastWeightChange = (_PendingWeightChange/_PendingWeightChangeCount);
                Value += _PastWeightChange;
            }
            else
            {
                _PastWeightChange = 0;
            }
            _PendingWeightChange = 0;
            _PendingWeightChangeCount = 0;
        }

        /// <summary>
        ///     Clears the pending Value changes.
        /// </summary>
        public virtual void ClearPendingWeightChanges()
        {
            _PendingWeightChange = 0;
            _PendingWeightChangeCount = 0;
        }

        #region IDisposable implementation

        public virtual void Dispose()
        {
            NodeA = null;
            NodeB = null;
        }

        #endregion

        #region ISerializable implementation

        /// <summary>
        ///     Initializes a new instance of the <see cref="Structure.Weight.Weight" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Weight(SerializationInfo info, StreamingContext context)
        {
            NodeA = (BaseNode) info.GetValue("NodeA", typeof (BaseNode));
            NodeB = (BaseNode) info.GetValue("NodeB", typeof (BaseNode));
            Value = info.GetDouble("Value");
            _InitialValue = info.GetDouble("_InitialValue");
            _PendingWeightChange = info.GetDouble("_PendingWeightChange");
            _PendingWeightChangeCount = info.GetDouble("_PendingWeightChangeCount");
            _PastWeightChange = info.GetDouble("_PastWeightChange");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("NodeA", NodeA, NodeA.GetType());
            info.AddValue("NodeB", NodeB, NodeB.GetType());
            info.AddValue("Value", Value);
            info.AddValue("_InitialValue", _InitialValue);
            info.AddValue("_PendingWeightChange", _PendingWeightChange);
            info.AddValue("_PendingWeightChangeCount", _PendingWeightChangeCount);
            info.AddValue("_PastWeightChange", _PastWeightChange);
        }

        #endregion
    }
}