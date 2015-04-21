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

namespace Cranium.Lib.Structure.ActivationFunction
{
    /// <summary>
    ///     This activation function will return 1 when >= threshold else 0, its not recomended you use this in a backprop
    ///     network
    /// </summary>
    [Serializable]
    public class Step : Base
    {
        /// <summary>
        ///     The Point at which the value must be at or above to for the activation function to return one else zero
        /// </summary>
        protected Double _ActivationPoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Step" /> class.
        /// </summary>
        /// <param name='activationPoint'>
        ///     Activation point.
        /// </param>
        public Step(Double activationPoint) { _ActivationPoint = activationPoint; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Step" /> class.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Step(SerializationInfo info, StreamingContext context) : base(info, context) { _ActivationPoint = info.GetDouble("_ActivationPoint"); }

        /// <summary>
        ///     Returns the input after running through the activation function.
        /// </summary>
        /// <param name='input'>
        ///     The value to pass to the activation function
        /// </param>
        public override Double Compute(Double input) { return input >= _ActivationPoint ? 1 : 0; }

        /// <summary>
        ///     Computes the derivative using the activation function.
        /// </summary>
        /// <returns>
        ///     The derivative.
        /// </returns>
        /// <param name='input'>
        ///     Input.
        /// </param>
        public override Double ComputeDerivative(Double input) { return 1; }

        public override void Dispose() { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { info.AddValue("_ActivationPoint", _ActivationPoint); }
    }
}