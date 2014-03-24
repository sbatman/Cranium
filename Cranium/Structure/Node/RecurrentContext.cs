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
using System.Diagnostics;
using System.Runtime.Serialization;

#endregion

namespace Cranium.Lib.Structure.Node
{
    /// <summary>
    ///     This recursive node acts differntly from as standard node as it has a source node from which it generates its
    ///     value,
    ///     this is based on the Rate of update pased as the constructor.
    /// </summary>
    [Serializable]
    public class RecurrentContext : Base
    {
        /// <summary>
        ///     The the persentage of the source nodes value that is used in calcualate the nodes new value
        /// </summary>
        protected double _RateOfUpdate;

        /// <summary>
        ///     The node which this node uses to calculate its value
        /// </summary>
        protected Base _SourceNode;

        /// <summary>
        ///     The initial value of the node.
        /// </summary>
        protected double _StartValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecurrentContext" /> class.
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
        public RecurrentContext(Base sourceNode, double rateOfUpdate, Layer.Base parentLayer, ActivationFunction.Base activationFunction) : base(parentLayer, activationFunction)
        {
            _Value = 0.5f;
            _SourceNode = sourceNode;
            _RateOfUpdate = rateOfUpdate;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecurrentContext" /> class. Used by the Serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public RecurrentContext(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _SourceNode = (Base) info.GetValue("_SourceNode", typeof (Base));
            _RateOfUpdate = info.GetDouble("_RateOfUpdate");
            _StartValue = info.GetDouble("_StartValue");
        }

        /// <summary>
        ///     Update this nodes value.
        /// </summary>
        public virtual void Update()
        {
            _Value = (_Value*(1 - _RateOfUpdate)) + (_SourceNode.GetValue()*_RateOfUpdate);
            if (double.IsNaN(_Value) || Double.IsInfinity(_Value)) Debugger.Break();
            if (double.IsNaN(_SourceNode.GetValue()) || Double.IsInfinity(_SourceNode.GetValue())) Debugger.Break();
        }

        /// <summary>
        ///     Sets the start value of this node.
        /// </summary>
        /// <param name='startValue'>
        ///     Start value.
        /// </param>
        public virtual void SetStartValue(double startValue)
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

        public virtual void OverrideRateOfUpdate(double newValue)
        {
            _RateOfUpdate = newValue;
        }
    }
}