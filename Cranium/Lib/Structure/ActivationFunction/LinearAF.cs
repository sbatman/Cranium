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
    ///     This activation function returns exactly what it is fed, this is extreamly useful fordata being fed in that has
    ///     already been cooked external
    ///     of the network. Lacking a derivative function however it is not suited for back progopgation networks.
    /// </summary>
    [Serializable]
    public class LinearAF : AF
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinearAF" /> class.
        /// </summary>
        public LinearAF()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinearAF" /> class. Used by the Serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public LinearAF(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        ///     Returns the input after running through the activation function.
        /// </summary>
        /// <param name='input'>
        ///     The value to pass to the activation function
        /// </param>
        [Pure]
        public override Double Compute(Double input)
        {
            return input;
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
            return 1;
        }

        public override void Dispose()
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}