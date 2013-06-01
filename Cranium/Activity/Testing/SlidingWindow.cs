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
using RecurrentContext = Cranium.Structure.Layer.RecurrentContext;
using System.Runtime.Serialization;
using System;

#endregion

namespace Cranium.Activity.Testing
{
    [Serializable]
    public class SlidingWindow : Base, ISerializable
    {

        /// <summary>
        /// Returned results class for the SlidingWindow testing activity
        /// </summary>
        public class SlidingWindowTestResults : TestResults
        {
            public double[][] ActualOutputs;
            public double[][] ExpectedOutputs;
            public double[][] OutputErrors;

            /// <summary>
            /// Initializes a new instance of the <see cref="Cranium.Activity.Testing.SlidingWindow.SlidingWindowTestResults" /> class.
            /// </summary>
            public SlidingWindowTestResults()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Cranium.Activity.Testing.SlidingWindow.SlidingWindowTestResults" /> class, used by the serializer.
            /// </summary>
            public SlidingWindowTestResults(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
                ActualOutputs = (double[][])info.GetValue("ActualOutputs", ActualOutputs.GetType());
                ExpectedOutputs = (double[][])info.GetValue("ActualOutputs", ExpectedOutputs.GetType());
                OutputErrors = (double[][])info.GetValue("ActualOutputs", OutputErrors.GetType());
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("ActualOutputs", ActualOutputs, ActualOutputs.GetType());
                info.AddValue("ExpectedOutputs", ExpectedOutputs, ExpectedOutputs.GetType());
                info.AddValue("OutputErrors", OutputErrors, OutputErrors.GetType());
            }
        }

        protected double[][] _ActualOutputs;
        protected int _DistanceToForcastHorrison;
        protected double[][] _ExpectedOutputs;
        protected double[][][] _InputSequences;
        protected double[][] _OutputErrors;
        protected int _PortionOfDatasetReserved;
        protected int _SequenceCount;
        protected int _WindowWidth;
        protected double[][] _WorkingDataset;

        public SlidingWindow()
        {
        }

        public SlidingWindow(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _ActualOutputs = (double[][])info.GetValue("_ActualOutputs", _ActualOutputs.GetType());
            _DistanceToForcastHorrison = info.GetInt32("_DistanceToForcastHorrison");
            _ExpectedOutputs = (double[][])info.GetValue("_ExpectedOutputs", _ExpectedOutputs.GetType());
            _InputSequences = (double[][][])info.GetValue("_InputSequences", _InputSequences.GetType());
            _OutputErrors = (double[][])info.GetValue("_OutputErrors", _OutputErrors.GetType());
            _PortionOfDatasetReserved = info.GetInt32("_PortionOfDatasetReserved");
            _SequenceCount = info.GetInt32("_SequenceCount");
            _WindowWidth = info.GetInt32("_WindowWidth");
            _WorkingDataset = (double[][])info.GetValue("_WorkingDataset", _WorkingDataset.GetType());
        }

        /// <summary>
        /// Sets teh width of the sliding window used for testing
        /// </summary>
        /// <param name="windowWidth"></param>
        public virtual void SetWindowWidth(int windowWidth)
        {
            _WindowWidth = windowWidth;
        }

        /// <summary>
        /// Sets the distance to prediction from the end of the presented window
        /// </summary>
        /// <param name="distance"></param>
        public virtual void SetDistanceToForcastHorrison(int distance)
        {
            _DistanceToForcastHorrison = distance;
        }

        /// <summary>
        /// Sets the ammount of data from the end of the dataset to to be used during testing
        /// </summary>
        /// <param name="reservedPortion"></param>
        public virtual void SetDatasetReservedLength(int reservedPortion)
        {
            _PortionOfDatasetReserved = reservedPortion;
        }

        /// <summary>
        /// sets the current dataset used for this test
        /// </summary>
        /// <param name="dataset"></param>
        public virtual void SetWorkingDataset(double[][] dataset)
        {
            _WorkingDataset = dataset;
        }

        /// <summary>
        /// Perpares any data that is required for testing
        /// </summary>
        public override void PrepareData()
        {
            _SequenceCount = ((_WorkingDataset[0].GetLength(0) - _PortionOfDatasetReserved) - _WindowWidth) -
                             _DistanceToForcastHorrison;
            int inputCount = _InputNodes.Count;
            int outputCount = _OutputNodes.Count;

            _InputSequences = new double[_SequenceCount][][];
            for (int i = 0; i < _SequenceCount; i++)
            {
                _InputSequences[i] = new double[_WindowWidth][];
                for (int y = 0; y < _WindowWidth; y++) _InputSequences[i][y] = new double[inputCount];
            }

            _ExpectedOutputs = new double[_SequenceCount][];
            for (int i = 0; i < _SequenceCount; i++) _ExpectedOutputs[i] = new double[outputCount];

            _ActualOutputs = new double[_SequenceCount][];
            for (int i = 0; i < _SequenceCount; i++) _ActualOutputs[i] = new double[outputCount];

            _OutputErrors = new double[_SequenceCount][];
            for (int i = 0; i < _SequenceCount; i++) _OutputErrors[i] = new double[outputCount];

            for (int i = 0; i < _SequenceCount; i++)
            {
                for (int j = 0; j < _WindowWidth; j++)
                {
                    for (int k = 0; k < inputCount; k++) _InputSequences[i][j][k] = _WorkingDataset[k][i + j];
                    for (int l = 0; l < outputCount; l++) _ExpectedOutputs[i][l] = _WorkingDataset[l][i + j + _DistanceToForcastHorrison];
                }
            }
        }

        /// <summary>
        /// Tests the provided network
        /// </summary>
        /// <param name="network">The network that requires testing</param>
        /// <returns>Returns acopy of the test results class (or derived class depending on class functionality)</returns>
        public override TestResults TestNetwork(Network network)
        {
            PrepareData();
            //Ensure that the networks state is clean

            int errorCount = 0;
            double rmse = 0;
            for (int s = 0; s < _SequenceCount; s++)
            {
                foreach (Structure.Layer.Base layer in network.GetCurrentLayers()) foreach (Structure.Node.Base node in layer.GetNodes()) node.SetValue(0);
                for (int i = 0; i < _WindowWidth; i++)
                {
                    for (int x = 0; x < _InputNodes.Count; x++) _InputNodes[x].SetValue(_InputSequences[s][i][x]);
                    network.FowardPass();
                    if (_Recurrentlayers != null) foreach (RecurrentContext layer in _Recurrentlayers) layer.UpdateExtra();
                }
                for (int x = 0; x < _OutputNodes.Count; x++)
                {
                    _ActualOutputs[s][x] = _OutputNodes[x].GetValue();
                    _OutputErrors[s][x] = _ExpectedOutputs[s][x] - _ActualOutputs[s][x];
                    errorCount++;
                    rmse += _OutputErrors[s][x] * _OutputErrors[s][x];
                }
            }
            //All the sequewnces have been run through and the outputs and their erros collected
            SlidingWindowTestResults result = new SlidingWindowTestResults()
                {
                    ExpectedOutputs = _ExpectedOutputs,
                    ActualOutputs = _ActualOutputs,
                    OutputErrors = _OutputErrors,
                    RMSE = rmse / errorCount
                };
            return result;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_ActualOutputs", _ActualOutputs, _ActualOutputs.GetType());
            info.AddValue("_DistanceToForcastHorrison", _DistanceToForcastHorrison);
            info.AddValue("_ExpectedOutputs", _ExpectedOutputs, _ExpectedOutputs.GetType());
            info.AddValue("_InputSequences", _InputSequences, _InputSequences.GetType());
            info.AddValue("_OutputErrors", _OutputErrors, _OutputErrors.GetType());
            info.AddValue("_PortionOfDatasetReserved", _ActualOutputs);
            info.AddValue("_SequenceCount", _SequenceCount);
            info.AddValue("_WindowWidth", _WindowWidth);
            info.AddValue("_WorkingDataset", _WorkingDataset, _WorkingDataset.GetType());
        }
    }
}