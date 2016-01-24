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
using System.Linq;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;

#endregion

namespace Cranium.Lib.Structure.Node
{
    /// <summary>
    ///     The output node functions differntly from normal nodes as its error is calcuated from the targetvalue rather than
    ///     the error of foward nodes.
    /// </summary>
    [Serializable]
    public class SOMNode : BaseNode
    {
        protected Double _TargetValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputNode" /> class.
        /// </summary>
        /// <param name='parentLayer'>
        ///     Parent layer.
        /// </param>
        /// <param name='activationFunction'>
        ///     Activation function.
        /// </param>
        public SOMNode(Layer.Layer parentLayer, AF activationFunction) : base(parentLayer, activationFunction) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputNode" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public SOMNode(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _TargetValue = info.GetDouble("_TargetValue");
        }

        /// <summary>
        ///     Calculates the error of the node based on its distance from the target value
        /// </summary>
        public override void CalculateError()
        {
            _Error = GetReverseWeights().Sum(w => Math.Pow(w.Value - w.NodeA.GetValue(), 2));
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

        public override void AdjustWeights(Double learningRate)
        {
            
        }
        public override void UpdateWeights(Double momentum)
        {
            foreach (Weight.Weight w in _ReverseWeights)
            {
                w.SetWeight(w.Value + (w.GetPastWeightChange() * momentum));
                w.ApplyPendingWeightChanges();
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_TargetValue", _TargetValue);
        }
    }
}