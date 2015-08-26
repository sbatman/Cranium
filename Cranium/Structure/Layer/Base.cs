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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace Cranium.Lib.Structure.Layer
{
    /// <summary>
    ///     This is the base Layer structure of the Neural Network, It is used to house collections of nodes and provide the
    ///     linking structure of these nodes with other groups of nodes.
    ///     The layer class also provides some of the basic functionality for back and foward propogation. This class can be
    ///     overriden to add additional functionality to a layer.
    /// </summary>
    [Serializable]
    public class Base : IDisposable, ISerializable
    {
        /// <summary>
        ///     The Layers that this layer connects to
        /// </summary>
        protected List<Base> _ForwardConnectedLayers = new List<Base>();

        /// <summary>
        ///     The ID of the layer
        /// </summary>
        protected Int32 _LayerID = -1;

        /// <summary>
        ///     The ID of the next node to be added to the layer
        /// </summary>
        protected Int32 _NextNodeID;

        /// <summary>
        ///     The Nodes within the layer
        /// </summary>
        protected List<Node.Base> _Nodes = new List<Node.Base>();

        /// <summary>
        ///     The Layers that are connected to this layer
        /// </summary>
        protected List<Base> _ReverseConnectedLayers = new List<Base>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Base" /> class.
        /// </summary>
        public Base() { }

        /// <summary>
        ///     Sets the nodes that are present in this layer, the previous list of nodes is purged.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetNodes(List<Node.Base> nodes)
        {
            _Nodes.Clear();
            _Nodes = null;
            _Nodes = nodes;
            foreach (Node.Base t in _Nodes)
            {
                t.SetNodeID(_NextNodeID);
                _NextNodeID++;
            }
        }

        /// <summary>
        ///     Returns the current list of nodes that are present within the layer.
        /// </summary>
        /// <returns>
        ///     The nodes.
        /// </returns>
        public virtual ReadOnlyCollection<Node.Base> GetNodes()
        {
            return _Nodes.AsReadOnly();
        }

        /// <summary>
        ///     Returns the number of nodes present in the layer.
        /// </summary>
        /// <returns>
        ///     The node count.
        /// </returns>
        public virtual Int32 GetNodeCount()
        {
            return _Nodes.Count;
        }

        /// <summary>
        ///     Adds a layer to the list of layers that are connected forward
        /// </summary>
        /// <param name='layer'>
        ///     Layer.
        /// </param>
        public virtual void ConnectFowardLayer(Base layer)
        {
            _ForwardConnectedLayers.Add(layer);
            layer._ReverseConnectedLayers.Add(this);
        }

        /// <summary>
        ///     Returns the list of layers that are connected forward
        /// </summary>
        /// <returns>
        ///     The forward connected layers.
        /// </returns>
        public virtual List<Base> GetForwardConnectedLayers()
        {
            return _ForwardConnectedLayers;
        }

        /// <summary>
        ///     Adds a layer to the list of layers that are connected reverse.
        /// </summary>
        /// <param name='layer'>
        ///     Layer.
        /// </param>
        public virtual void ConnectReverseLayer(Base layer)
        {
            _ReverseConnectedLayers.Add(layer);
            layer._ForwardConnectedLayers.Add(this);
        }

        /// <summary>
        ///     Returns the list of layers that are connected reverse
        /// </summary>
        /// <returns>
        ///     The forward connected layers.
        /// </returns>
        public virtual List<Base> GetReverseConnectedLayers()
        {
            return _ReverseConnectedLayers;
        }

        /// <summary>
        ///     Uses the current forward and reverse layers to populate the node connections (aka building weights)
        /// </summary>
        public virtual void PopulateNodeConnections()
        {
            PurgeNodeConnections();
            foreach (Base l in _ForwardConnectedLayers) foreach (Node.Base n in _Nodes) foreach (Node.Base fn in l.GetNodes()) n.ConnectToNode(fn, Weight.Base.ConnectionDirection.FORWARD, 0);
        }

        /// <summary>
        ///     Removes all connections on this layers nodes (useful if deleting or modifiying the layer
        /// </summary>
        public virtual void PurgeNodeConnections()
        {
            foreach (Node.Base n in _Nodes) n.DestroyAllConnections();
        }

        /// <summary>
        ///     Triggers a calculate value call on all nodes withint he layer and then recursively calls this function on all
        ///     foward connected layers.
        /// </summary>
        public virtual void ForwardPass()
        {
            foreach (Node.Base n in _Nodes) n.CalculateValue();
            foreach (Base l in _ForwardConnectedLayers) l.ForwardPass();
        }

        /// <summary>
        ///     Performs the defualt reverse pass logic.
        /// </summary>
        /// <param name='learningRate'>
        ///     Learning rate.
        /// </param>
        /// <param name='momentum'>
        ///     Momentum.
        /// </param>
        /// <param name='recurseDownward'>
        ///     Recurse downward, if set to false this well not call ReversePass on any layers below this one.
        /// </param>
        public virtual void ReversePass(Double learningRate, Double momentum, Boolean recurseDownward = true)
        {
            foreach (Node.Base n in _Nodes) n.CalculateError();
            foreach (Node.Base n in _Nodes) n.AdjustWeights(learningRate);
            foreach (Node.Base n in _Nodes) n.UpdateWeights(momentum);
            if (recurseDownward) foreach (Base l in _ReverseConnectedLayers) l.ReversePass(learningRate, momentum);
        }

        /// <summary>
        ///     Returns the ID of the layer
        /// </summary>
        /// <returns>
        ///     The ID
        /// </returns>
        public virtual Int32 GetID()
        {
            return _LayerID;
        }

        /// <summary>
        ///     Sets the ID of the layer
        /// </summary>
        /// <param name='id'>
        ///     Identifier.
        /// </param>
        public virtual void SetID(Int32 id)
        {
            _LayerID = id;
        }

        /// <summary>
        ///     Updates any extra logic required, This is used when pre/post epoc logic needs to run on the layer
        /// </summary>
        public virtual void UpdateExtra() { }

        /// <summary>
        ///     Gets a node within the layer by ID
        /// </summary>
        /// <returns>
        ///     The Node, or null if unable to find
        /// </returns>
        /// <param name='id'>
        ///     Identifier.
        /// </param>
        public virtual Node.Base GetNodeByID(Int32 id)
        {
	        //look for a node with matching ID if we can find it return it else return null
	        return _Nodes.FirstOrDefault(n => n.GetID() == id);
        }

	    #region IDisposable implementation

        public void Dispose()
        {
            PurgeNodeConnections();
            _ReverseConnectedLayers.Clear();
            _ReverseConnectedLayers = null;
            _ForwardConnectedLayers.Clear();
            _ForwardConnectedLayers = null;
            foreach (Node.Base n in _Nodes) n.Dispose();
            _Nodes.Clear();
            _Nodes = null;
        }

        #endregion

        #region ISerializable implementation

        /// <summary>
        ///     Initializes a new instance of the <see cref="Base" /> class. Used by the Serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Base(SerializationInfo info, StreamingContext context)
        {
            _Nodes = (List<Node.Base>) info.GetValue("_Nodes", typeof (List<Node.Base>));
            _LayerID = info.GetInt32("_LayerID");
            _NextNodeID = info.GetInt32("_NextNodeID");
            _ForwardConnectedLayers = (List<Base>) info.GetValue("_ForwardConnectedLayers", typeof (List<Base>));
            _ReverseConnectedLayers = (List<Base>) info.GetValue("_ReverseConnectedLayers", typeof (List<Base>));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Nodes", _Nodes, _Nodes.GetType());
            info.AddValue("_ForwardConnectedLayers", _ForwardConnectedLayers, typeof (List<Base>));
            info.AddValue("_ReverseConnectedLayers", _ReverseConnectedLayers, typeof (List<Base>));
            info.AddValue("_LayerID", _LayerID);
            info.AddValue("_NextNodeID", _NextNodeID);
        }

        #endregion
    }
}