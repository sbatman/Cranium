using Cranium.Lib.Structure;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cranium.Lib.Activity.Testing
{
    /// <summary>
    /// A Base class for the Testing Activity. Testing a network can many differnt tasks so creating derivatives of this class to allow that is intended.
    /// This base class is provided as a structure guide, point of refrence for serialisation and for distribution.
    /// </summary>
    [Serializable]
    public abstract class Base : Activity.Base
    {
        /// <summary>
        /// Basse Class for formatting and delivering test results
        /// </summary>
        [Serializable]
        public class TestResults : ISerializable
        {
            public double RMSE;

            /// <summary>
            /// Initializes a new instance of the <see>
            ///                                       <cref>Activity.Testing.Base.TestResults</cref>
            ///                                   </see>
            ///     class.
            /// </summary>
            public TestResults()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see>
            ///                                       <cref>Activity.Testing.Base.TestResults</cref>
            ///                                   </see>
            ///     class, for use by the serialiser.
            /// </summary>
            public TestResults(SerializationInfo info, StreamingContext context)
            {
                RMSE = info.GetDouble("RMSE");
            }

            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("RMSE", RMSE);
            }
        }

        /// <summary>
        /// The current input nodes that has been assigned for use during this test
        /// </summary>
        protected List<Structure.Node.Base> _InputNodes;
        /// <summary>
        /// The current output nodes that has been assigned for use during this test
        /// </summary>
        protected List<Structure.Node.Base> _OutputNodes;
        /// <summary>
        /// The current Recurrent layers that has been assigned for use during this test
        /// </summary>
        protected List<Structure.Layer.Base> _Recurrentlayers;
        /// <summary>
        /// The network that requires testing
        /// </summary>
        protected Network _TargetNetwork;

        /// <summary>
        ///     Sets the input nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetInputNodes(List<Structure.Node.Base> nodes)
        {
            _InputNodes = nodes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity.Testing.Base" /> class.
        /// </summary>
        protected Base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity.Testing.Base" /> class, for use by the serialiser.
        /// </summary>
        protected Base(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _InputNodes = (List<Structure.Node.Base>)info.GetValue("_InputNodes", typeof(List<Structure.Node.Base>));
            _OutputNodes = (List<Structure.Node.Base>)info.GetValue("_OutputNodes", typeof(List<Structure.Node.Base>));
            _Recurrentlayers = (List<Structure.Layer.Base>)info.GetValue("_Recurrentlayers", typeof(List<Structure.Layer.Base>));
            _TargetNetwork = (Network)info.GetValue("_TargetNetwork", typeof(Network));
        }

        /// <summary>
        /// Sets the output nodes.
        /// </summary>
        /// <param name='nodes'>
        /// Nodes.
        /// </param>
        public virtual void SetOutputNodes(List<Structure.Node.Base> nodes)
        {
            _OutputNodes = nodes;
        }

        /// <summary>
        /// Sets the current layers that require additional update logic during testing
        /// </summary>
        /// <param name="layers"></param>
        public virtual void SetRecurrentConextLayers(List<Structure.Layer.Base> layers)
        {
            _Recurrentlayers = layers;
        }

        /// <summary>
        /// Sets the current target network for the testing activity, this must be set before testNetwork is called
        /// </summary>
        /// <param name="targetNetwork"></param>
        public virtual void SetTargetNetwork(Network targetNetwork)
        {
            _TargetNetwork = targetNetwork;
        }

        /// <summary>
        /// Returns the current target network for the activity
        /// </summary>
        /// <returns></returns>
        public virtual Network GetTargetNetwork()
        {
            return _TargetNetwork;
        }

        /// <summary>
        /// Perpares any data that is required for testing
        /// </summary>
        public abstract void PrepareData();

        /// <summary>
        /// Tests the provided network
        /// </summary>
        /// <returns>Returns acopy of the test results class (or derived class depending on class functionality)</returns>
        public abstract TestResults TestNetwork();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_InputNodes", _InputNodes, _InputNodes.GetType());
            info.AddValue("_OutputNodes", _OutputNodes, _OutputNodes.GetType());
            info.AddValue("_Recurrentlayers", _Recurrentlayers, _Recurrentlayers.GetType());
            info.AddValue("_TargetNetwork", _TargetNetwork, typeof(Network));
        }
    }
}
