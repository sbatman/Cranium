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
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace Cranium.Lib.Structure.Node
{
    /// <summary>
    ///     The base node class is a core part of the Neural network framework and represents a neuron that is placed within
    ///     layers in the network.
    ///     This class can be derived to add additional functionality to a node such as adding recurive memory.
    /// </summary>
    [Serializable]
    public class Base : IDisposable, ISerializable
    {
        /// <summary>
        ///     The Activation function of this node.
        /// </summary>
        protected ActivationFunction.Base _ActivationFunction;

        /// <summary>
        ///     Current Error
        /// </summary>
        protected Double _Error;

        /// <summary>
        ///     A list of foward weights on this node, where this node is NodeA on the weight
        /// </summary>
        protected List<Weight.Base> _ForwardWeights = new List<Weight.Base>();

        /// <summary>
        ///     The Nodes current ID.
        /// </summary>
        protected int _NodeID;

        /// <summary>
        ///     The parent layer.
        /// </summary>
        protected Layer.Base _ParentLayer;

        /// <summary>
        ///     A list of reverse weights on this node, where this node is NodeB on the weight
        /// </summary>
        protected List<Weight.Base> _ReverseWeights = new List<Weight.Base>();

        /// <summary>
        ///     A baked copy of the foward weights, updated automaticaly
        /// </summary>
        protected Weight.Base[] _TFowardWeights;

        /// <summary>
        ///     A baked copy of the reverse weights, updated automaticaly
        /// </summary>
        protected Weight.Base[] _TReverseWeights;

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
        public Base(Layer.Base layer, ActivationFunction.Base activationFunction)
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

            Debug.Assert(_TReverseWeights != null, "_TReverseWeights != null");
            if (_TReverseWeights.Length == 0) return;

            _Value = 0;

            foreach (Weight.Base w in _TReverseWeights) _Value += w.NodeA._Value*w.Weight;

            _Value = _ActivationFunction.Compute(_Value);
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
        public virtual Weight.Base[] GetFowardWeights()
        {
            if (_TFowardWeights == null || _TReverseWeights == null) BakeLists();
            return _TFowardWeights;
        }

        /// <summary>
        ///     Calculates the error of the node based on its contibution to the error of forward nodes.
        /// </summary>
        public virtual void CalculateError()
        {
            Double tempError = _TFowardWeights.Sum(w => w.Weight*w.NodeB.GetError());
            _Error = _ActivationFunction.ComputeDerivative(_Value)*tempError;
            // if (Double.IsNaN(_Error) || Double.IsInfinity(_Error)) throw (new Exception("Weight Error"));
        }

        /// <summary>
        ///     Causes a spending weight change based on learning rate and errors
        /// </summary>
        /// <param name='learningRate'>
        ///     Learning rate.
        /// </param>
        public virtual void AdjustWeights(Double learningRate)
        {
            foreach (Weight.Base w in _ForwardWeights) w.AddWeightChange(_Value*w.NodeB._Error*learningRate);
        }

        /// <summary>
        ///     Triggers the addition of the momentum and the application of all pending weights
        /// </summary>
        /// <param name='momentum'>
        ///     Momentum.
        /// </param>
        public virtual void UpdateWeights(double momentum)
        {
            foreach (Weight.Base w in _ForwardWeights)
            {
                w.SetWeight(w.Weight + (w.GetPastWeightChange()*momentum));
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
            //if (Double.IsNaN(_Error) || Double.IsInfinity(_Error)) throw (new Exception("Weight Error"));
            return _Error;
        }

        /// <summary>
        ///     Returns the currently assigned list of reverse weights
        /// </summary>
        /// <returns>
        ///     The reverse weights.
        /// </returns>
        public virtual Weight.Base[] GetReverseWeights()
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
        public virtual void ConnectToNode(Base nodeToConnect, Weight.Base.ConnectionDirection connectionDirectionToNode, float startingWeight)
        {
            Weight.Base theNewWeight;
            switch (connectionDirectionToNode)
            {
                case Weight.Base.ConnectionDirection.Forward:
                    _TFowardWeights = null;
                    theNewWeight = new Weight.Base(this, nodeToConnect, startingWeight);
                    _ForwardWeights.Add(theNewWeight);
                    nodeToConnect._ReverseWeights.Add(theNewWeight);
                    break;
                case Weight.Base.ConnectionDirection.Reverse:
                    _TReverseWeights = null;
                    theNewWeight = new Weight.Base(nodeToConnect, this, startingWeight);
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
                foreach (Weight.Base w in _TFowardWeights)
                {
                    w.NodeB._TReverseWeights = null;
                    if (w.NodeB._ReverseWeights != null) w.NodeB._ReverseWeights.Remove(w);
                    w.Dispose();
                }
                _ForwardWeights.Clear();
                _TFowardWeights = null;
            }
            if (_TReverseWeights != null)
            {
                foreach (Weight.Base w in _TReverseWeights)
                {
                    w.Dispose();
                    w.NodeB._TFowardWeights = null;
                    w.NodeB._ForwardWeights.Remove(w);
                }
                _ReverseWeights.Clear();
                _TReverseWeights = null;
            }
        }

        /// <summary>
        ///     Returns the current nodes ID
        /// </summary>
        /// <returns>
        ///     The I.
        /// </returns>
        public virtual int GetID()
        {
            return _NodeID;
        }

        /// <summary>
        ///     Sets the current nodes ID
        /// </summary>
        /// <param name='newID'>
        ///     New I.
        /// </param>
        public virtual void SetNodeID(int newID)
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
        public Base(SerializationInfo info, StreamingContext context)
        {
            _Value = info.GetDouble("_Value");
            _Error = info.GetDouble("_Error");
            _ParentLayer = (Layer.Base) info.GetValue("_ParentLayer", typeof (Layer.Base));
            _ForwardWeights = (List<Weight.Base>) info.GetValue("_ForwardWeights", typeof (List<Weight.Base>));
            _ReverseWeights = (List<Weight.Base>) info.GetValue("_ReverseWeights", typeof (List<Weight.Base>));
            _ActivationFunction = (ActivationFunction.Base) info.GetValue("_ActivationFunction", typeof (ActivationFunction.Base));
            _NodeID = info.GetInt32("_NodeID");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Value", _Value);
            info.AddValue("_Error", _Error);
            info.AddValue("_ParentLayer", _ParentLayer, typeof (Layer.Base));
            info.AddValue("_ForwardWeights", _ForwardWeights, typeof (List<Weight.Base>));
            info.AddValue("_ReverseWeights", _ReverseWeights, typeof (List<Weight.Base>));
            info.AddValue("_ActivationFunction", _ActivationFunction, _ActivationFunction.GetType());
            info.AddValue("_NodeID", _NodeID);
        }

        #endregion
    }
}