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
    ///     The Elliott activations functions acts as a computationaly cheaper version of tanH although with known problems
    ///     with reaching the lowest
    ///     errors and becoming trapped in local minima. This activation function is good for prototyping network structures,
    ///     however in many cases
    ///     it should not be used for practival implementations of networks. http://drum.lib.umd.edu/handle/1903/5355
    /// </summary>
    [Serializable]
    public class ElliottAF : AF
    {
        private Double _Scale = 1;

        public Double Scale
        {
            [Pure] get { return _Scale; }
            set { _Scale = value; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ElliottAF" /> class.
        /// </summary>
        public ElliottAF()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ElliottAF" /> class. Used by the serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public ElliottAF(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _Scale = info.GetDouble("_Scale");
        }

        #region implemented abstract members of Cranium.Structure.ActivationFunction.Base

        /// <summary>
        ///     Returns the input after running through the activation function.
        /// </summary>
        /// <param name='input'>
        ///     The value to pass to the activation function
        /// </param>
        public override Double Compute(Double input)
        {
            return input * _Scale / (1 + Math.Abs(input * _Scale));
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
        public override Double ComputeDerivative(Double input)
        {
            return _Scale / Math.Pow(1.0d + Math.Abs(input * _Scale), 2);
        }

        public override void Dispose()
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Scale", _Scale);
        }

        #endregion
    }
}