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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;

#endregion

namespace Cranium.Lib.Structure.Node
{
    /// <summary>
    ///     The base node class is a core part of the Neural network framework and represents a neuron that is placed within
    ///     layers in the network.
    ///     This class can be derived to add additional functionality to a node such as adding recurive memory.
    /// </summary>
    [Serializable]
    public class BaseNode : IDisposable, ISerializable
    {
        /// <summary>
        ///     The Activation function of this node.
        /// </summary>
        protected AF _ActivationFunction;

        /// <summary>
        ///     Current Error
        /// </summary>
        protected Double _Error;

        /// <summary>
        ///     A list of foward weights on this node, where this node is NodeA on the weight
        /// </summary>
        protected List<Weight.Weight> _ForwardWeights = new List<Weight.Weight>();

        /// <summary>
        ///     The Nodes current ID.
        /// </summary>
        protected Int32 _NodeID;

        /// <summary>
        ///     The parent layer.
        /// </summary>
        protected Layer.Layer _ParentLayer;

        /// <summary>
        ///     A list of reverse weights on this node, where this node is NodeB on the weight
        /// </summary>
        protected List<Weight.Weight> _ReverseWeights = new List<Weight.Weight>();

        /// <summary>
        ///     A baked copy of the foward weights, updated automaticaly
        /// </summary>
        protected Weight.Weight[] _TFowardWeights;

        /// <summary>
        ///     A baked copy of the reverse weights, updated automaticaly
        /// </summary>
        protected Weight.Weight[] _TReverseWeights;

        /// <summary>
        ///     Current Value
        /// </summary>
        protected Double _Value;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see>
        ///         <cref>Cranium.Structure.Node.Base</cref>
        ///     </see>
        ///     class for use by the serializer.
        /// </summary>
        /// <param name='layer'>
        ///     Layer.
        /// </param>
        /// <param name='activationFunction'>
        ///     Activation function.
        /// </param>
        public BaseNode(Layer.Layer layer, AF activationFunction)
        {
            _ParentLayer = layer;
            _ActivationFunction = activationFunction;
        }

        /// <summary>
        ///     Causes the node to calucate its new value as a sum of the weights by values of reverse connected nodes and then
        ///     passes this value by the Activation function assigned
        /// </summary>
        public virtual void CalculateValue()
        {
            if (_TFowardWeights == null || _TReverseWeights == null) BakeLists();
            if (_TReverseWeights.Length == 0) return;
            _Value = 0;
            if (_Value > 1 || _Value < -1) Debugger.Break();
            foreach (Weight.Weight w in _TReverseWeights)
            {
                _Value += w.NodeA._Value * w.Value;
            }
            Double k = _Value;
            _Value = _ActivationFunction.Compute(_Value);
            if (_Value > 1 || _Value < -1) Debugger.Break();

        }

        /// <summary>
        ///     Returns the last calucated value of the node
        /// </summary>
        /// <returns>
        ///     The value.
        /// </returns>
        public virtual Double GetValue()
        {
            return _Value;
        }

        /// <summary>
        ///     Bakes down the forward and reverse list of weights for optimisation sake. This is performed autmaticaly before any
        ///     node functions that requires the weights
        /// </summary>
        public virtual void BakeLists()
        {
            _TFowardWeights = _ForwardWeights.ToArray();
            _TReverseWeights = _ReverseWeights.ToArray();
        }

        /// <summary>
        ///     Returns the currently assigned forward weights
        /// </summary>
        /// <returns>
        ///     The foward weights.
        /// </returns>
        public virtual Weight.Weight[] GetFowardWeights()
        {
            if (_TFowardWeights == null || _TReverseWeights == null) BakeLists();
            return _TFowardWeights;
        }

        /// <summary>
        ///     Calculates the error of the node based on its contibution to the error of forward nodes.
        /// </summary>
        public virtual void CalculateError()
        {
            Double tempError = 0;
            Int32 count = 0;
            foreach (Weight.Weight w in _TFowardWeights)
            {
                count++;
                tempError += w.Value * w.NodeB.GetError();
            }
            tempError /= count;
            _Error = _ActivationFunction.ComputeDerivative(_Value) * tempError;
        }

        /// <summary>
        ///     Causes a spending weight change based on learning rate and errors
        /// </summary>
        /// <param name='learningRate'>
        ///     Learning rate.
        /// </param>
        public virtual void AdjustWeights(Double learningRate)
        {
            foreach (Weight.Weight w in _ForwardWeights) w.AddWeightChange(_Value * w.NodeB._Error * learningRate);
        }

        /// <summary>
        ///     Triggers the addition of the momentum and the application of all pending weights
        /// </summary>
        /// <param name='momentum'>
        ///     Momentum.
        /// </param>
        public virtual void UpdateWeights(Double momentum)
        {
            foreach (Weight.Weight w in _ForwardWeights)
            {
                w.SetWeight(w.Value + (w.GetPastWeightChange() * momentum));
                w.ApplyPendingWeightChanges();
            }
        }

        /// <summary>
        ///     Returns the error of the node, used when back propogating
        /// </summary>
        /// <returns>
        ///     The error.
        /// </returns>
        public virtual Double GetError()
        {
            return _Error;
        }

        /// <summary>
        ///     Returns the currently assigned list of reverse weights
        /// </summary>
        /// <returns>
        ///     The reverse weights.
        /// </returns>
        public virtual Weight.Weight[] GetReverseWeights()
        {
            if (_TFowardWeights == null || _TReverseWeights == null) BakeLists();
            return _TReverseWeights;
        }

        /// <summary>
        ///     Connects a second node to this one, building the correct weight and adding it to the list of weights that are
        ///     updated when required
        /// </summary>
        /// <param name='nodeToConnect'>
        ///     Node to connect.
        /// </param>
        /// <param name='connectionDirectionToNode'>
        ///     Connection direction to node.
        /// </param>
        /// <param name='startingWeight'>
        ///     Starting weight.
        /// </param>
        public virtual void ConnectToNode(BaseNode nodeToConnect, Weight.Weight.ConnectionDirection connectionDirectionToNode, Single startingWeight)
        {
            Weight.Weight theNewWeight;
            switch (connectionDirectionToNode)
            {
                case Weight.Weight.ConnectionDirection.FORWARD:
                    _TFowardWeights = null;
                    theNewWeight = new Weight.Weight(this, nodeToConnect, startingWeight);
                    _ForwardWeights.Add(theNewWeight);
                    nodeToConnect._ReverseWeights.Add(theNewWeight);
                    break;
                case Weight.Weight.ConnectionDirection.REVERSE:
                    _TReverseWeights = null;
                    theNewWeight = new Weight.Weight(nodeToConnect, this, startingWeight);
                    _ReverseWeights.Add(theNewWeight);
                    nodeToConnect._ForwardWeights.Add(theNewWeight);
                    break;
            }
        }

        /// <summary>
        ///     Sets the value of the node.
        /// </summary>
        /// <param name='newValue'>
        ///     New value.
        /// </param>
        public virtual void SetValue(Double newValue)
        {
            _Value = newValue;
        }

        /// <summary>
        ///     Destroies all the foward and reverse weights connected to this node.
        /// </summary>
        public virtual void DestroyAllConnections()
        {
            if (_TFowardWeights != null)
            {
                foreach (Weight.Weight w in _TFowardWeights)
                {
                    if (w.NodeB != null)
                    {
                        w.NodeB._TReverseWeights = null;
                        if (w.NodeB._ReverseWeights != null) w.NodeB._ReverseWeights.Remove(w);
                    }
                    w.Dispose();
                }
                if (_ForwardWeights != null) _ForwardWeights.Clear();
                _TFowardWeights = null;
            }
            if (_TReverseWeights != null)
            {
                foreach (Weight.Weight w in _TReverseWeights)
                {

                    if (w.NodeB != null)
                    {
                        w.NodeB._TFowardWeights = null;
                        w.NodeB._ForwardWeights.Remove(w);
                    }
                    w.Dispose();
                }
                if (_ReverseWeights != null) _ReverseWeights.Clear();
                _TReverseWeights = null;
            }
        }

        /// <summary>
        ///     Returns the current nodes ID
        /// </summary>
        /// <returns>
        ///     The I.
        /// </returns>
        public virtual Int32 GetID()
        {
            return _NodeID;
        }

        /// <summary>
        ///     Sets the current nodes ID
        /// </summary>
        /// <param name='newID'>
        ///     New I.
        /// </param>
        public virtual void SetNodeID(Int32 newID)
        {
            _NodeID = newID;
        }

        #region IDisposable implementation

        public virtual void Dispose()
        {
            _ParentLayer = null;
            _ForwardWeights.Clear();
            _ForwardWeights = null;
            _ReverseWeights.Clear();
            _ReverseWeights = null;
            _ActivationFunction.Dispose();
            _ActivationFunction = null;
        }

        #endregion

        #region ISerializable implementation

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see>
        ///         <cref>Cranium.Structure.Node.Base</cref>
        ///     </see>
        ///     class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public BaseNode(SerializationInfo info, StreamingContext context)
        {
            _Value = info.GetDouble("_Value");
            _Error = info.GetDouble("_Error");
            _ParentLayer = (Layer.Layer)info.GetValue("_ParentLayer", typeof(Layer.Layer));
            _ForwardWeights = (List<Weight.Weight>)info.GetValue("_ForwardWeights", typeof(List<Weight.Weight>));
            _ReverseWeights = (List<Weight.Weight>)info.GetValue("_ReverseWeights", typeof(List<Weight.Weight>));
            _ActivationFunction = (AF)info.GetValue("_ActivationFunction", typeof(AF));
            _NodeID = info.GetInt32("_NodeID");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Value", _Value);
            info.AddValue("_Error", _Error);
            info.AddValue("_ParentLayer", _ParentLayer, typeof(Layer.Layer));
            info.AddValue("_ForwardWeights", _ForwardWeights, typeof(List<Weight.Weight>));
            info.AddValue("_ReverseWeights", _ReverseWeights, typeof(List<Weight.Weight>));
            info.AddValue("_ActivationFunction", _ActivationFunction, _ActivationFunction.GetType());
            info.AddValue("_NodeID", _NodeID);
        }

        #endregion
    }
}