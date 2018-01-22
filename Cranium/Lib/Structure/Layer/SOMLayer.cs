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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Node;

namespace Cranium.Lib.Structure.Layer
{
    /// <summary>
    ///     The SOM layer offers self organising map functionalty, the layer contains a square 2 dimentional collection of
    ///     nodes which can be used for topological feature recognition further information about this netowrk type can
    ///     be sourced here : https://en.wikipedia.org/wiki/Self-organizing_map
    /// </summary>
    public class SOMLayer : Layer
    {
        public Double MaxmimumLearningDistance { get; set; }

        public Double MinimumLearningDistance { get; set; }

        public Double CurrentDistanceSupression { get; set; }

        public Int32 NodeGridSize { get; }

        public SOMLayer(Int32 gridSize)
        {
            NodeGridSize = gridSize;
            for (Int32 i = 0; i < gridSize * gridSize; i++)
            {
                _Nodes.Add(new SOMNode(this, new LinearAF()));
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SOMLayer" /> class. Used by the Serializer.
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public SOMLayer(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            MaxmimumLearningDistance = info.GetDouble("_MaxmimumLearningDistance");
            MinimumLearningDistance = info.GetDouble("_MinimumLearningDistance");
            CurrentDistanceSupression = info.GetDouble("_CurrentDistanceSupression");
            NodeGridSize = info.GetInt32("_NodeGridSize");
        }

        [Pure]
        private static SOMNode GetNodeAtLocation(IReadOnlyList<BaseNode> nodes, Int32 x, Int32 y, Int32 widthHeight)
        {
            if (x < 0 || y < 0 || x >= widthHeight || y >= widthHeight) return null;
            return (SOMNode) nodes[y * widthHeight + x];
        }

        /// <summary>
        ///     Performs the defualt reverse pass logic.
        /// </summary>
        /// <param name='learningRate'>
        ///     Learning rate.
        /// </param>
        /// <param name='momentum'>
        ///     Momentum.
        /// </param>
        /// <param name='recurseDownward'>
        ///     Recurse downward, if set to false this well not call ReversePass on any layers below this one.
        /// </param>
        /// <param name="delayWeightUpdate">If this is passed as true then weight updating will need to be perfomed manually</param>
        public override void ReversePass(Double learningRate, Double momentum, Boolean recurseDownward = true, Boolean delayWeightUpdate = false)
        {
            Int32 totalNodes = _Nodes.Count;
            Int32 widthHeight = (Int32) Math.Sqrt(totalNodes);

            Double lowestDiff = Double.MaxValue;
            Int32 lowestDiffX = 0;
            Int32 lowestDiffY = 0;
            for (Int32 y = 0; y < widthHeight; y++)
            {
                for (Int32 x = 0; x < widthHeight; x++)
                {
                    SOMNode node = GetNodeAtLocation(_Nodes, x, y, widthHeight);

                    node.CalculateError();

                    if (!(node.GetError() < lowestDiff)) continue;

                    lowestDiff = node.GetError();
                    lowestDiffX = x;
                    lowestDiffY = y;
                }
            }

            Int32 range = (Int32) Math.Ceiling(MaxmimumLearningDistance);

            for (Int32 y = -range; y < range + 1; y++)
            {
                for (Int32 x = -range; x < range + 1; x++)
                {
                    Double distanceFromCentre = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) + 1;

                    Double acceptedDistance = (MaxmimumLearningDistance - MinimumLearningDistance) * CurrentDistanceSupression + MinimumLearningDistance;

                    if (distanceFromCentre > acceptedDistance) continue;

                    BaseNode node = GetNodeAtLocation(_Nodes, lowestDiffX + x, lowestDiffY + y, widthHeight);
                    if (node == null) continue;

                    foreach (Weight.Weight w in node.GetReverseWeights())
                    {
                        Double diff = w.NodeA.GetValue() - w.Value;
                        w.AddWeightChange(diff * learningRate / distanceFromCentre);
                    }
                }
            }

            base.ReversePass(learningRate, momentum, false, delayWeightUpdate);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("_MaxmimumLearningDistance", MaxmimumLearningDistance);
            info.AddValue("_MinimumLearningDistance", MinimumLearningDistance);
            info.AddValue("_CurrentDistanceSupression", CurrentDistanceSupression);
            info.AddValue("_NodeGridSize", NodeGridSize);
        }

        [Pure]
        public SOMNode GetNodeAtLocation(Int32 x, Int32 y)
        {
            if (x < 0 || y < 0 || x >= NodeGridSize || y >= NodeGridSize) return null;
            return (SOMNode) _Nodes[y * NodeGridSize + x];
        }
    }
}