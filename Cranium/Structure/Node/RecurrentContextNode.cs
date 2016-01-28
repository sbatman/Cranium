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

#region Usings

using System;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;

#endregion

namespace Cranium.Lib.Structure.Node
{
    /// <summary>
    ///     This recursive node acts differntly from as standard node as it has a source node from which it generates its
    ///     value,
    ///     this is based on the Rate of update pased as the constructor.
    /// </summary>
    [Serializable]
    public class RecurrentContextNode : BaseNode
    {
        /// <summary>
        ///     The the persentage of the source nodes value that is used in calcualate the nodes new value
        /// </summary>
        protected Double _RateOfUpdate;

        /// <summary>
        ///     The node which this node uses to calculate its value
        /// </summary>
        protected BaseNode _SourceNode;

        /// <summary>
        ///     The initial value of the node.
        /// </summary>
        protected Double _StartValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecurrentContextNode" /> class.
        /// </summary>
        /// <param name='sourceNode'>
        ///     Source node.
        /// </param>
        /// <param name='rateOfUpdate'>
        ///     Rate of update.
        /// </param>
        /// <param name='parentLayer'>
        ///     Parent layer.
        /// </param>
        /// <param name='activationFunction'>
        ///     Activation function.
        /// </param>
        public RecurrentContextNode(BaseNode sourceNode, Double rateOfUpdate, Layer.Layer parentLayer, AF activationFunction) : base(parentLayer, activationFunction)
        {
            _Value = 0.0f;
            _SourceNode = sourceNode;
            _RateOfUpdate = rateOfUpdate;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecurrentContextNode" /> class. Used by the Serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public RecurrentContextNode(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _SourceNode = (BaseNode) info.GetValue("_SourceNode", typeof (BaseNode));
            _RateOfUpdate = info.GetDouble("_RateOfUpdate");
            _StartValue = info.GetDouble("_StartValue");
        }

        /// <summary>
        ///     Update this nodes value.
        /// </summary>
        public virtual void Update()
        {
            _Value = _Value * (1 - _RateOfUpdate) + _SourceNode.GetValue() * _RateOfUpdate;
        }

        /// <summary>
        ///     Sets the start value of this node.
        /// </summary>
        /// <param name='startValue'>
        ///     Start value.
        /// </param>
        public virtual void SetStartValue(Double startValue)
        {
            _StartValue = startValue;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_SourceNode", _SourceNode, _SourceNode.GetType());
            info.AddValue("_RateOfUpdate", _RateOfUpdate);
            info.AddValue("_StartValue", _StartValue);
        }

        public virtual void OverrideRateOfUpdate(Double newValue)
        {
            _RateOfUpdate = newValue;
        }
    }
}