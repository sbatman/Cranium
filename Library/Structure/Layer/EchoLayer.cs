// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Linq;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Structure.Layer
{
    /// <summary>
    ///    This is an implementation of the echo reservoir found in EchoState networks. It provides a form of recursive memory
    ///    as each node within the layer is randomly connected to a number of other nodes. When presented with data over a
    ///    number
    ///    of iterations this causes a RNN style memory behaviour. However due to the chaotic nature of the reservoirs
    ///    construction,
    ///    accuracy and learning limits of this type of network can vary heavily. Further information can be sourced
    ///    here http://www.scholarpedia.org/article/Echo_state_network
    /// </summary>
    [Serializable]
    public class EchoReservoir : Layer
    {
        /// <summary>
        ///    The Activation to use for all the nodes created within the Reservoir
        /// </summary>
        protected AF _ActivationFunction;

        /// <summary>
        ///    The connectivity is calculated as chances = max-min, for each chance the levelOfConnectivity is compared to a
        ///    random double, if levelOfConnectivity is higher then an additional connection is made on top of the original Min
        /// </summary>
        protected Double _LevelOfConnectivity;

        /// <summary>
        ///    The Maximum connections per node
        /// </summary>
        protected Int32 _MaximumConnections;

        /// <summary>
        ///    The minimum connections per node.
        /// </summary>
        protected Int32 _MinimumConnections;

        /// <summary>
        ///    The number of nodes present in the Reservoir
        /// </summary>
        protected Int32 _NodeCount;

        /// <summary>
        ///    The random used for building connections
        /// </summary>
        protected Random _Rnd = new Random();

        /// <summary>
        ///    Initializes a new instance of the <see cref="EchoReservoir" /> class.
        /// </summary>
        /// <param name='nodeCount'>
        ///    The number of nodes in the Reservoir
        /// </param>
        /// <param name='levelOfConnectivity'>
        ///    The connectivity is calculated as chances = max-min, for each chance the levelOfConnectivity is compared to a
        ///    random double, if levelOfConnectivity is higher
        ///    then an additional connection is made on top of the original Min
        /// </param>
        /// <param name='minimumConnections'>
        ///    unused
        /// </param>
        /// <param name='maximumConnections'>
        ///    unused
        /// </param>
        /// <param name='activationFunction'>
        ///    Activation function.
        /// </param>
        public EchoReservoir(Int32 nodeCount, Double levelOfConnectivity, Int32 minimumConnections, Int32 maximumConnections, AF activationFunction)
        {
            _NodeCount = nodeCount;
            _LevelOfConnectivity = levelOfConnectivity;
            _MinimumConnections = minimumConnections;
            _MaximumConnections = maximumConnections;
            _ActivationFunction = activationFunction;
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref="EchoReservoir" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///    Info.
        /// </param>
        /// <param name='context'>
        ///    Context.
        /// </param>
        public EchoReservoir(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _NodeCount = info.GetInt32("_NodeCount");
            _LevelOfConnectivity = info.GetDouble("_LevelOfConnectivity");
            _ActivationFunction = (AF)info.GetValue("_ActivationFunction", typeof(AF));
            _Rnd = (Random)info.GetValue("_Rnd", typeof(Random));
        }

		/// <summary>
        /// THe minimum number of connections per node requested
        /// </summary>
        public virtual Int32 MinimumConnections => _MinimumConnections;

		/// <summary>
        /// The maximum number of connections per node requested
        /// </summary>
        public virtual Int32 MaximumConnections => _MaximumConnections;

		/// <summary>
        /// the level of connectivity per node requested
        /// </summary>
        public virtual Double LevelOfConnectivity => _LevelOfConnectivity;

        /// <summary>
        ///    Populates the node connections aka builds the weights.
        /// </summary>
        public override void PopulateNodeConnections()
        {
            PurgeNodeConnections();
            BuildNodeBank();
            foreach (Layer l in _ForwardConnectedLayers)
                foreach (BaseNode n in _Nodes)
                    foreach (BaseNode fn in l.GetNodes())
                        n.ConnectToNode(fn, Weight.Weight.ConnectionDirection.FORWARD, 0);
            foreach (Layer l in _ReverseConnectedLayers)
                foreach (BaseNode n in _Nodes)
                    foreach (BaseNode fn in l.GetNodes())
                        n.ConnectToNode(fn, Weight.Weight.ConnectionDirection.REVERSE, 0);
        }

        /// <summary>
        ///    Creates all the nodes and inter node connections in this layer
        /// </summary>
        public virtual void BuildNodeBank()
        {
            for (Int32 x = 0; x < _NodeCount; x++) _Nodes.Add(new BaseNode(this, _ActivationFunction));
            foreach (BaseNode node in _Nodes)
            {
                foreach (BaseNode node2 in _Nodes.Where(node2 => _Rnd.NextDouble() < _LevelOfConnectivity))
                {
                    node.ConnectToNode(node2, Weight.Weight.ConnectionDirection.FORWARD, 0);
                }
            }
        }

        /// <summary>
        ///    Reverses the pass, and prevents the call to recurse downwards, as the inter-node connections will not have their
        ///    weights changed.
        /// </summary>
        /// <param name='learningRate'>
        ///    Learning rate.
        /// </param>
        /// <param name='momentum'>
        ///    Momentum.
        /// </param>
        /// <param name='recurseDownward'>
        ///    Recurse downward.
        /// </param>
        /// <param name="delayWeightUpdate">If this is passed as true then weight updating will need to be performed manually</param>
        public override void ReversePass(Double learningRate, Double momentum, Boolean recurseDownward = true, Boolean delayWeightUpdate = false)
        {
            base.ReversePass(learningRate, momentum, false, delayWeightUpdate);
        }

		/// <summary>
        /// FOr serializing the object
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_NodeCount", _NodeCount);
            info.AddValue("_LevelOfConnectivity", _LevelOfConnectivity);
            info.AddValue("_ActivationFunction", _ActivationFunction, _ActivationFunction.GetType());
            info.AddValue("_Rnd", _Rnd, typeof(Random));
        }
    }
}