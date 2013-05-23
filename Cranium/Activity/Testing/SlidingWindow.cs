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

using System.Collections.Generic;
using Cranium.Structure;
using Cranium.Structure.Node;
using RecurrentContext = Cranium.Structure.Layer.RecurrentContext;

#endregion

namespace Cranium.Activity.Testing
{
    public class SlidingWindow
    {
        protected double[][] _ActualOutputs;
        protected int _DistanceToForcastHorrison;
        protected double[][] _ExpectedOutputs;
        protected List<Base> _InputNodes;
        protected double[][][] _InputSequences;
        protected double[][] _OutputErrors;
        protected List<Base> _OutputNodes;
        protected int _PortionOfDatasetReserved;
        protected List<Structure.Layer.Base> _Recurrentlayers;
        protected int _SequenceCount;
        protected int _WindowWidth;
        protected double[][] _WorkingDataset;

        public virtual void SetWindowWidth(int windowWidth)
        {
            _WindowWidth = windowWidth;
        }

        public virtual void SetDistanceToForcastHorrison(int distance)
        {
            _DistanceToForcastHorrison = distance;
        }

        public virtual void SetDatasetReservedLength(int reservedPortion)
        {
            _PortionOfDatasetReserved = reservedPortion;
        }

        public virtual void SetWorkingDataset(double[][] dataset)
        {
            _WorkingDataset = dataset;
        }

        /// <summary>
        ///     Sets the input nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetInputNodes(List<Base> nodes)
        {
            _InputNodes = nodes;
        }

        /// <summary>
        ///     Sets the output nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetOutputNodes(List<Base> nodes)
        {
            _OutputNodes = nodes;
        }

        public virtual void SetRecurrentConextLayers(List<Structure.Layer.Base> layers)
        {
            _Recurrentlayers = layers;
        }

        /// <summary>
        ///     Prepares the data before training.
        /// </summary>
        public virtual void PrepareData()
        {
            _SequenceCount = ((_WorkingDataset [0].GetLength(0) - _PortionOfDatasetReserved) - _WindowWidth) -
                             _DistanceToForcastHorrison;
            int inputCount = _WorkingDataset.GetLength(0);
            int outputCount = 1; //TODO: Fix this

            _InputSequences = new double[_SequenceCount][][];
            for (int i = 0; i < _SequenceCount; i++)
            {
                _InputSequences [i] = new double[_WindowWidth][];
                for (int y = 0; y < _WindowWidth; y++) _InputSequences [i] [y] = new double[inputCount];
            }

            _ExpectedOutputs = new double[_SequenceCount][];
            for (int i = 0; i < _SequenceCount; i++) _ExpectedOutputs [i] = new double[outputCount];

            _ActualOutputs = new double[_SequenceCount][];
            for (int i = 0; i < _SequenceCount; i++) _ActualOutputs [i] = new double[outputCount];

            _OutputErrors = new double[_SequenceCount][];
            for (int i = 0; i < _SequenceCount; i++) _OutputErrors [i] = new double[outputCount];

            for (int i = 0; i < _SequenceCount; i++)
            {
                for (int j = 0; j < _WindowWidth; j++)
                {
                    for (int k = 0; k < inputCount; k++) _InputSequences [i] [j] [k] = _WorkingDataset [k] [i + j];
                    for (int l = 0; l < outputCount; l++) _ExpectedOutputs [i] [l] = _WorkingDataset [l] [i + j + _DistanceToForcastHorrison];
                }
            }
        }

        public virtual TestResults TestNetwork(Network network)
        {
            PrepareData();
            //Ensure that the networks state is clean

            int errorCount = 0;
            double rmse = 0;
            for (int s = 0; s < _SequenceCount; s++)
            {
                foreach (Structure.Layer.Base layer in network.GetCurrentLayers()) foreach (Base node in layer.GetNodes()) node.SetValue(0);
                for (int i = 0; i < _WindowWidth; i++)
                {
                    for (int x = 0; x < _InputNodes.Count; x++) _InputNodes [x].SetValue(_InputSequences [s] [i] [x]);
                    network.FowardPass();
                    if (_Recurrentlayers != null) foreach (RecurrentContext layer in _Recurrentlayers) layer.UpdateExtra();
                }
                for (int x = 0; x < _OutputNodes.Count; x++)
                {
                    _ActualOutputs [s] [x] = _OutputNodes [x].GetValue();
                    _OutputErrors [s] [x] = _ExpectedOutputs [s] [x] - _ActualOutputs [s] [x];
                    errorCount++;
                    rmse += _OutputErrors [s] [x]*_OutputErrors [s] [x];
                }
            }
            //All the sequewnces have been run through and the outputs and their erros collected
            TestResults result = new TestResults
                {
                    ExpectedOutputs = _ExpectedOutputs,
                    ActualOutputs = _ActualOutputs,
                    OutputErrors = _OutputErrors,
                    RMSE = rmse/errorCount
                };
            return result;
        }

        public struct TestResults
        {
            public double[][] ActualOutputs;
            public double[][] ExpectedOutputs;
            public double[][] OutputErrors;
            public double RMSE;
        }
    }
}