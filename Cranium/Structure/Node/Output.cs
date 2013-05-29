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

namespace Cranium.Structure.Node
{
    /// <summary>
    ///     The output node functions differntly from normal nodes as its error is calcuated from the targetvalue rather than the error of foward nodes.
    /// </summary>
    [Serializable]
    public class Output : Base
    {
        protected Double _TargetValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cranium.Structure.Node.Output" /> class.
        /// </summary>
        /// <param name='parentLayer'>
        ///     Parent layer.
        /// </param>
        /// <param name='activationFunction'>
        ///     Activation function.
        /// </param>
        public Output(Layer.Base parentLayer, ActivationFunction.Base activationFunction)
            : base(parentLayer, activationFunction)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cranium.Structure.Node.Output" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Output(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _TargetValue = info.GetDouble("_TargetValue");
        }

        /// <summary>
        ///     Calculates the error of the node based on its distance from the target value
        /// </summary>
        public override void CalculateError()
        {
            _Error = ((1 - _Value)*(1 + _Value))*(_TargetValue - _Value);
        }

        /// <summary>
        ///     Sets the target value, used for error calculation
        /// </summary>
        /// <param name='targetValue'>
        ///     Target value.
        /// </param>
        public virtual void SetTargetValue(Double targetValue)
        {
            _TargetValue = targetValue;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_TargetValue", _TargetValue);
        }
    }
}