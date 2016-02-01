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
using System.Linq;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Activity.Testing
{
    [Serializable]
    public class SlidingWindow : Base
    {
        /// <summary>
        ///     Returned results class for the SlidingWindow testing activity
        /// </summary>
        public class SlidingWindowTestResults : TestResults
        {
            public Double[][] ActualOutputs;
            public Double[][] ExpectedOutputs;
            public Double[][] OutputErrors;

            /// <summary>
            ///     Initializes a new instance of the <see cref="Activity.Testing.SlidingWindow.SlidingWindowTestResults" /> class.
            /// </summary>
            public SlidingWindowTestResults()
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="Activity.Testing.SlidingWindow.SlidingWindowTestResults" /> class,
            ///     used by the serializer.
            /// </summary>
            public SlidingWindowTestResults(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                ActualOutputs = (Double[][])info.GetValue("ActualOutputs", ActualOutputs.GetType());
                ExpectedOutputs = (Double[][])info.GetValue("ActualOutputs", ExpectedOutputs.GetType());
                OutputErrors = (Double[][])info.GetValue("ActualOutputs", OutputErrors.GetType());
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("ActualOutputs", ActualOutputs, ActualOutputs.GetType());
                info.AddValue("ExpectedOutputs", ExpectedOutputs, ExpectedOutputs.GetType());
                info.AddValue("OutputErrors", OutputErrors, OutputErrors.GetType());
            }
        }

        protected Double[][] _ActualOutputs;
        protected Int32 _DistanceToForcastHorrison;
        protected Double[][] _ExpectedOutputs;
        protected Double[][][] _InputSequences;
        protected Double[][] _OutputErrors;
        protected Int32 _PortionOfDatasetReserved;
        protected Int32 _SequenceCount;
        protected Int32 _WindowWidth;
        protected Double[][] _WorkingDataset;

        public SlidingWindow()
        {
        }

        public SlidingWindow(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _ActualOutputs = (Double[][])info.GetValue("_ActualOutputs", typeof(Double[][]));
            _DistanceToForcastHorrison = info.GetInt32("_DistanceToForcastHorrison");
            _ExpectedOutputs = (Double[][])info.GetValue("_ExpectedOutputs", typeof(Double[][]));
            _InputSequences = (Double[][][])info.GetValue("_InputSequences", typeof(Double[][][]));
            _OutputErrors = (Double[][])info.GetValue("_OutputErrors", typeof(Double[][]));
            _PortionOfDatasetReserved = info.GetInt32("_PortionOfDatasetReserved");
            _SequenceCount = info.GetInt32("_SequenceCount");
            _WindowWidth = info.GetInt32("_WindowWidth");
            _WorkingDataset = (Double[][])info.GetValue("_WorkingDataset", typeof(Double[][]));
        }

        /// <summary>
        ///     Sets teh width of the sliding window used for testing
        /// </summary>
        /// <param name="windowWidth"></param>
        public virtual void SetWindowWidth(Int32 windowWidth)
        {
            _WindowWidth = windowWidth;
        }

        /// <summary>
        ///     Sets the distance to prediction from the end of the presented window
        /// </summary>
        /// <param name="distance"></param>
        public virtual void SetDistanceToForcastHorrison(Int32 distance)
        {
            _DistanceToForcastHorrison = distance;
        }

        /// <summary>
        ///     Sets the ammount of data from the end of the dataset to to be used during testing
        /// </summary>
        /// <param name="reservedPortion"></param>
        public virtual void SetDatasetReservedLength(Int32 reservedPortion)
        {
            _PortionOfDatasetReserved = reservedPortion;
        }

        /// <summary>
        ///     sets the current dataset used for this test
        /// </summary>
        /// <param name="dataset"></param>
        public virtual void SetWorkingDataset(Double[][] dataset)
        {
            _WorkingDataset = dataset;
        }

        public virtual Double[][] GetWorkingDataset()
        {
            return _WorkingDataset;
        }

        /// <summary>
        ///     Perpares any data that is required for testing
        /// </summary>
        public override void PrepareData()
        {
            _SequenceCount = _WorkingDataset[0].GetLength(0) - _PortionOfDatasetReserved - _WindowWidth - _DistanceToForcastHorrison;
            Int32 inputCount = _InputNodes.Count;
            Int32 outputCount = _OutputNodes.Count;

            _InputSequences = new Double[_SequenceCount][][];
            for (Int32 i = 0; i < _SequenceCount; i++)
            {
                _InputSequences[i] = new Double[_WindowWidth][];
                for (Int32 y = 0; y < _WindowWidth; y++) _InputSequences[i][y] = new Double[inputCount];
            }

            _ExpectedOutputs = new Double[_SequenceCount][];
            for (Int32 i = 0; i < _SequenceCount; i++) _ExpectedOutputs[i] = new Double[outputCount];

            _ActualOutputs = new Double[_SequenceCount][];
            for (Int32 i = 0; i < _SequenceCount; i++) _ActualOutputs[i] = new Double[outputCount];

            _OutputErrors = new Double[_SequenceCount][];
            for (Int32 i = 0; i < _SequenceCount; i++) _OutputErrors[i] = new Double[outputCount];

            for (Int32 i = 0; i < _SequenceCount; i++)
            {
                for (Int32 j = 0; j < _WindowWidth; j++)
                {
                    for (Int32 k = 0; k < inputCount; k++) _InputSequences[i][j][k] = _WorkingDataset[k][i + j];
                    for (Int32 l = 0; l < outputCount; l++) _ExpectedOutputs[i][l] = _WorkingDataset[l][i + j + _DistanceToForcastHorrison];
                }
            }
        }

        /// <summary>
        ///     Tests the provided network
        /// </summary>
        /// <returns>Returns acopy of the test results class (or derived class depending on class functionality)</returns>
        public override TestResults TestNetwork()
        {
            PrepareData();
            //Ensure that the networks state is clean

            Int32 errorCount = 0;
            Double rmse = 0;
            for (Int32 s = 0; s < _SequenceCount; s++)
            {
                foreach (BaseNode node in _TargetNetwork.GetCurrentLayers().SelectMany(layer => layer.GetNodes())) node.SetValue(0);
                for (Int32 i = 0; i < _WindowWidth; i++)
                {
                    for (Int32 x = 0; x < _InputNodes.Count; x++) _InputNodes[x].SetValue(_InputSequences[s][i][x]);
                    _TargetNetwork.FowardPass();
                    if (_UpdatingLayers != null) foreach (RecurrentContext layer in _UpdatingLayers.Cast<RecurrentContext>()) layer.UpdateExtra();
                }
                for (Int32 x = 0; x < _OutputNodes.Count; x++)
                {
                    _ActualOutputs[s][x] = _OutputNodes[x].GetValue();
                    _OutputErrors[s][x] = _ExpectedOutputs[s][x] - _ActualOutputs[s][x];
                    errorCount++;
                    rmse += _OutputErrors[s][x] * _OutputErrors[s][x];
                }
            }
            //All the sequewnces have been run through and the outputs and their erros collected
            SlidingWindowTestResults result = new SlidingWindowTestResults { ExpectedOutputs = _ExpectedOutputs, ActualOutputs = _ActualOutputs, OutputErrors = _OutputErrors, Rmse = rmse / errorCount };
            return result;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_ActualOutputs", _ActualOutputs, typeof(Double[][]));
            info.AddValue("_DistanceToForcastHorrison", _DistanceToForcastHorrison);
            info.AddValue("_ExpectedOutputs", _ExpectedOutputs, typeof(Double[][]));
            info.AddValue("_InputSequences", _InputSequences, typeof(Double[][][]));
            info.AddValue("_OutputErrors", _OutputErrors, typeof(Double[][]));
            info.AddValue("_PortionOfDatasetReserved", _PortionOfDatasetReserved);
            info.AddValue("_SequenceCount", _SequenceCount);
            info.AddValue("_WindowWidth", _WindowWidth);
            info.AddValue("_WorkingDataset", _WorkingDataset, typeof(Double[][]));
        }
    }
}