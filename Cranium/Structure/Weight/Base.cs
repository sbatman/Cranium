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

#endregion

namespace Cranium.Lib.Structure.Weight
{
    /// <summary>
    ///     This is the base weight class and acts as a standard weight between two nodes. weight changes can be applied
    ///     immediatly
    ///     or added to a pending list and applied at a later stage.
    /// </summary>
    [Serializable]
    public class Base : IDisposable, ISerializable
    {
        /// <summary>
        ///     Connection direction. Either foward or reverse, Added for code readability
        /// </summary>
        public enum ConnectionDirection
        {
            Reverse,
            Forward
        };

        /// <summary>
        ///     Node a, this should be the reverse node
        /// </summary>
        public Node.Base NodeA;

        /// <summary>
        ///     Node b, this should be the forward node
        /// </summary>
        public Node.Base NodeB;

        /// <summary>
        ///     The current weight
        /// </summary>
        public Double Weight;

        /// <summary>
        ///     The initial value of the weight
        /// </summary>
        protected Double _InitialValue;

        /// <summary>
        ///     The last total weight change
        /// </summary>
        protected Double _PastWeightChange;

        /// <summary>
        ///     The total pending weight change before count devision
        /// </summary>
        protected Double _PendingWeightChange;

        /// <summary>
        ///     The number of pending weight changes
        /// </summary>
        protected Double _PendingWeightChangeCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Base" /> class.
        /// </summary>
        /// <param name='nodeA'>
        ///     Node a.
        /// </param>
        /// <param name='nodeB'>
        ///     Node b.
        /// </param>
        /// <param name='weight'>
        ///     Initial Weight.
        /// </param>
        public Base(Node.Base nodeA, Node.Base nodeB, Double weight)
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
        ///     Gets the total change from the initial value the weight was set with.
        /// </summary>
        /// <returns>
        ///     The total change.
        /// </returns>
        public virtual Double GetTotalChange()
        {
            return Weight - _InitialValue;
        }

        /// <summary>
        ///     Adds a pending weight change
        /// </summary>
        /// <param name='weightModification'>
        ///     Weight modification.
        /// </param>
        public virtual void AddWeightChange(Double weightModification)
        {
            //if (Double.IsNaN(Weight) || Double.IsInfinity(weightModification)) throw (new Exception("Weight Error"));
            _PendingWeightChange += weightModification;
            _PendingWeightChangeCount++;
        }

        /// <summary>
        ///     Sets the current weight.
        /// </summary>
        /// <param name='newWeight'>
        ///     New weight.
        /// </param>
        public virtual void SetWeight(Double newWeight)
        {
            Weight = newWeight;
        }

        /// <summary>
        ///     Gets the previous total weight change caused by ApplyPendingWeightChanges
        /// </summary>
        /// <returns>
        ///     The past weight change.
        /// </returns>
        public virtual double GetPastWeightChange()
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
                Weight += _PastWeightChange;

                if(_PendingWeightChangeCount!=1 || Math.Abs(_PendingWeightChange)>0.1f)Console.WriteLine(_PendingWeightChange+" "+_PendingWeightChangeCount);
            }
            else
            {
                _PastWeightChange = 0;
            }

            //if (Double.IsNaN(Weight) || Double.IsInfinity(Weight)) throw (new Exception("Weight Error"));
            _PendingWeightChange = 0;
            _PendingWeightChangeCount = 0;
        }

        /// <summary>
        ///     Clears the pending weight changes.
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
        ///     Initializes a new instance of the <see cref="Base" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Base(SerializationInfo info, StreamingContext context)
        {
            NodeA = (Node.Base) info.GetValue("NodeA", typeof (Node.Base));
            NodeB = (Node.Base) info.GetValue("NodeB", typeof (Node.Base));
            Weight = info.GetDouble("Weight");
            _InitialValue = info.GetDouble("_InitialValue");
            _PendingWeightChange = info.GetDouble("_PendingWeightChange");
            _PendingWeightChangeCount = info.GetDouble("_PendingWeightChangeCount");
            _PastWeightChange = info.GetDouble("_PastWeightChange");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("NodeA", NodeA, NodeA.GetType());
            info.AddValue("NodeB", NodeB, NodeB.GetType());
            info.AddValue("Weight", Weight);
            info.AddValue("_InitialValue", _InitialValue);
            info.AddValue("_PendingWeightChange", _PendingWeightChange);
            info.AddValue("_PendingWeightChangeCount", _PendingWeightChangeCount);
            info.AddValue("_PastWeightChange", _PastWeightChange);
        }

        #endregion
    }
}