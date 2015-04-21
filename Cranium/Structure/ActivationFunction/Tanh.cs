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
    ///     This activation function scales the input into the range of -1 to 1 which is very important for the majority
    ///     or neural network structures that wish to learn signal data.
    /// </summary>
    [Serializable]
    public class Tanh : Base
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Tanh" /> class.
        /// </summary>
        public Tanh() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tanh" /> class. Used by the serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Tanh(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        ///     Returns the input after running through the activation function.
        /// </summary>
        /// <param name='input'>
        ///     The value to pass to the activation function
        /// </param>
        public override Double Compute(Double input)
        {
            Double temp = Math.Exp(input*2.0);
            return (temp - 1.0)/(temp + 1.0);
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
        public override Double ComputeDerivative(Double input) { return 1 - Math.Pow((input), 2); }

        public override void Dispose() { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}