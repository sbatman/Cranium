using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Node;

namespace Cranium.Lib.Structure.Layer
{
    /// <summary>
    /// The SOM layer offers self organising map functionalty, the layer contains a square 2 dimentional collection of
    /// nodes which can be used for topological feature recognition further information about this netowrk type can
    /// be sourced here : https://en.wikipedia.org/wiki/Self-organizing_map
    /// </summary>
    public class SOMLayer : Layer
    {
        private Double _MaxmimumLearningDistance;
        private Double _MinimumLearningDistance;
        private Double _CurrentDistanceSupression;
        private readonly Int32 _NodeGridSize;

        public SOMLayer(Int32 gridSize)
        {
            _NodeGridSize = gridSize;
            for (int i = 0; i < gridSize* gridSize; i++)
            {
                _Nodes.Add(new SOMNode(this, new LinearAF()));
            }
        }

        public Double MaxmimumLearningDistance
        {
            get { return _MaxmimumLearningDistance; }
            set { _MaxmimumLearningDistance = value; }
        }

        public Double MinimumLearningDistance
        {
            get { return _MinimumLearningDistance; }
            set { _MinimumLearningDistance = value; }
        }

        public Double CurrentDistanceSupression
        {
            get { return _CurrentDistanceSupression; }
            set { _CurrentDistanceSupression = value; }
        }

        public Int32 NodeGridSize
        {
            get { return _NodeGridSize; }
        }

        [Pure]
        private static SOMNode GetNodeAtLocation(IReadOnlyList<BaseNode> nodes, Int32 x, Int32 y, Int32 widthHeight)
        {
            if (x < 0 || y < 0 || x >= widthHeight || y >= widthHeight) return null;
            return (SOMNode)nodes[(y * widthHeight) + x];
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
            Int32 widthHeight = (Int32)Math.Sqrt(totalNodes);

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

            Int32 range = (Int32)Math.Ceiling(_MaxmimumLearningDistance);

            for (Int32 y = -range; y < range + 1; y++)
            {
                for (Int32 x = -range; x < range + 1; x++)
                {

                    Double distanceFromCentre = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) + 1;

                    Double acceptedDistance = ((_MaxmimumLearningDistance - _MinimumLearningDistance) * _CurrentDistanceSupression) + _MinimumLearningDistance;

                    if (distanceFromCentre > acceptedDistance) continue;

                    BaseNode node = GetNodeAtLocation(_Nodes, lowestDiffX + x, lowestDiffY + y, widthHeight);
                    if (node == null) continue;

                    foreach (Weight.Weight w in node.GetReverseWeights())
                    {
                        Double diff = ((w.NodeA.GetValue() - w.Value));
                        w.AddWeightChange((diff * learningRate) / distanceFromCentre);
                    }
                }
            }

            base.ReversePass(learningRate, momentum, false, delayWeightUpdate);
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
            _Nodes = (List<BaseNode>)info.GetValue("_Nodes", typeof(List<BaseNode>));
            _LayerID = info.GetInt32("_LayerID");
            _NextNodeID = info.GetInt32("_NextNodeID");
            _ForwardConnectedLayers = (List<Layer>)info.GetValue("_ForwardConnectedLayers", typeof(List<Layer>));
            _ReverseConnectedLayers = (List<Layer>)info.GetValue("_ReverseConnectedLayers", typeof(List<Layer>));


            _MaxmimumLearningDistance = info.GetDouble("_MaxmimumLearningDistance");
            _MinimumLearningDistance = info.GetDouble("_MinimumLearningDistance");
            _CurrentDistanceSupression = info.GetDouble("_CurrentDistanceSupression");
            _NodeGridSize = info.GetInt32("_NodeGridSize");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("_MaxmimumLearningDistance", _MaxmimumLearningDistance);
            info.AddValue("_MinimumLearningDistance", _MinimumLearningDistance);
            info.AddValue("_CurrentDistanceSupression", _CurrentDistanceSupression);
            info.AddValue("_NodeGridSize", _NodeGridSize);
        }

        [Pure]
        public SOMNode GetNodeAtLocation(Int32 x, Int32 y)
        {
            if (x < 0 || y < 0 || x >= _NodeGridSize || y >= _NodeGridSize) return null;
            return (SOMNode)_Nodes[(y * _NodeGridSize) + x];
        }

    }
}
