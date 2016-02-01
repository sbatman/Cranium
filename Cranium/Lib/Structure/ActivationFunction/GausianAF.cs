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
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

#endregion

namespace Cranium.Lib.Structure.ActivationFunction
{
    /// <summary>
    ///     Gaussian Activation function acts as a bellcurve activation function
    /// </summary>
    [Serializable]
    public class Gaussian : AF
    {
        /// <summary>
        ///     The steepness of the bellcurve.
        /// </summary>
        protected Double _Steepness;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Gaussian" /> class.
        /// </summary>
        /// <param name='steepness'>
        ///     Steepness.
        /// </param>
        public Gaussian(Double steepness)
        {
            _Steepness = steepness;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Gaussian" /> class. Used by the serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Gaussian(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _Steepness = info.GetDouble("_Steepness");
        }

        #region implemented abstract members of Cranium.Structure.ActivationFunction.Base

        [Pure]
        public override Double Compute(Double input)
        {
            return Math.Exp(-Math.Pow(_Steepness * input, 2.0d));
        }

        /// <summary>
        ///     Computes the derivative using the activation function.
        /// </summary>
        /// <returns>
        ///     The derivative.
        /// </returns>
        /// <param name='input'>
        ///     Input.
        /// </param>
        [Pure]
        public override Double ComputeDerivative(Double input)
        {
            return -2 * input * _Steepness * Compute(input) * input;
        }

        public override void Dispose()
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Steepness", _Steepness);
        }

        #endregion
    }
}