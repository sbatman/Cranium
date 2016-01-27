using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

namespace Cranium.Lib.Activity.Testing
{
    /// <summary>
    ///     A Base class for the Testing Activity. Testing a network can many differnt tasks so creating derivatives of this
    ///     class to allow that is intended.
    ///     This base class is provided as a structure guide, point of refrence for serialisation and for distribution.
    /// </summary>
    [Serializable]
    public abstract class Base : Activity.Base
    {
        /// <summary>
        ///     The current input nodes that has been assigned for use during this test
        /// </summary>
        protected List<BaseNode> _InputNodes;

        /// <summary>
        ///     The current output nodes that has been assigned for use during this test
        /// </summary>
        protected List<BaseNode> _OutputNodes;

        /// <summary>
        ///     The current Recurrent layers that has been assigned for use during this test
        /// </summary>
        protected List<Layer> _UpdatingLayers;

        /// <summary>
        ///     The network that requires testing
        /// </summary>
        protected Network _TargetNetwork;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Activity.Testing.Base" /> class.
        /// </summary>
        protected Base() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Activity.Testing.Base" /> class, for use by the serialiser.
        /// </summary>
        protected Base(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _InputNodes = (List<BaseNode>)info.GetValue("_InputNodes", typeof(List<BaseNode>));
            _OutputNodes = (List<BaseNode>)info.GetValue("_OutputNodes", typeof(List<BaseNode>));
            _UpdatingLayers = (List<Layer>)info.GetValue("_UpdatingLayers", typeof(List<Layer>));
            _TargetNetwork = (Network)info.GetValue("_TargetNetwork", typeof(Network));
        }

        /// <summary>
        ///     Sets the input nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetInputNodes(List<BaseNode> nodes) { _InputNodes = nodes; }

        /// <summary>
        ///     Sets the output nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetOutputNodes(List<BaseNode> nodes) { _OutputNodes = nodes; }

        /// <summary>
        ///     Sets the current layers that require additional update logic during testing
        /// </summary>
        /// <param name="layers"></param>
        public virtual void SetUpdatingLayers(List<Layer> layers) { _UpdatingLayers = layers; }

        /// <summary>
        ///     Sets the current target network for the testing activity, this must be set before testNetwork is called
        /// </summary>
        /// <param name="targetNetwork"></param>
        public virtual void SetTargetNetwork(Network targetNetwork) { _TargetNetwork = targetNetwork; }

        /// <summary>
        ///     Returns the current target network for the activity
        /// </summary>
        /// <returns></returns>
        public virtual Network GetTargetNetwork() { return _TargetNetwork; }

        /// <summary>
        ///     Perpares any data that is required for testing
        /// </summary>
        public abstract void PrepareData();

        /// <summary>
        ///     Tests the provided network
        /// </summary>
        /// <returns>Returns acopy of the test results class (or derived class depending on class functionality)</returns>
        public abstract TestResults TestNetwork();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_InputNodes", _InputNodes, _InputNodes.GetType());
            info.AddValue("_OutputNodes", _OutputNodes, _OutputNodes.GetType());
            info.AddValue("_UpdatingLayers", _UpdatingLayers, _UpdatingLayers.GetType());
            info.AddValue("_TargetNetwork", _TargetNetwork, typeof(Network));
        }

        /// <summary>
        ///     Basse Class for formatting and delivering test results
        /// </summary>
        [Serializable]
        public class TestResults : ISerializable
        {
            public Double Rmse;

            /// <summary>
            ///     Initializes a new instance of the
            ///     <see>
            ///         <cref>Activity.Testing.Base.TestResults</cref>
            ///     </see>
            ///     class.
            /// </summary>
            public TestResults() { }

            /// <summary>
            ///     Initializes a new instance of the
            ///     <see>
            ///         <cref>Activity.Testing.Base.TestResults</cref>
            ///     </see>
            ///     class, for use by the serialiser.
            /// </summary>
            public TestResults(SerializationInfo info, StreamingContext context) { Rmse = info.GetDouble("RMSE"); }

            public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { info.AddValue("RMSE", Rmse); }
        }

        public override void Dispose()
        {
            _InputNodes?.Clear();
            _OutputNodes?.Clear();
            _UpdatingLayers?.Clear();
            _TargetNetwork?.Dispose();

            base.Dispose();
        }
    }
}