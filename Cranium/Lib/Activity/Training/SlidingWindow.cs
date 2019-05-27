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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Activity.Training
{
    /// <summary>
    ///     This training activity presents the given data as a series of windows, this type of of training activity is best
    ///     suited to networks
    ///     with forms of recursive memory. You need to specify the data set, how wide the window much be and the range from
    ///     the end of the
    ///     presented window to the desired prediction
    /// </summary>
    [Serializable]
    public class SlidingWindow : Base
    {
        /// <summary>
        ///     How far beyond the last presented value are we attempting to predict
        /// </summary>
        protected Int32 _DistanceToForcastHorizon;

        /// <summary>
        ///     The outputs that are expected
        /// </summary>
        protected Double[,] _ExpectedOutputs;

        /// <summary>
        ///     A collection of the current input nodes
        /// </summary>
        protected List<BaseNode> _InputNodes;

        /// <summary>
        ///     Thre sequences of inputs to be presented during training
        /// </summary>
        protected Double[,,] _InputSequences;

        /// <summary>
        ///     The average error experianced during the last pass
        /// </summary>
        protected Double _LastPassAverageError;

        /// <summary>
        ///     A stream setup for logging the training information
        /// </summary>
        protected StreamWriter _LogStream;

        /// <summary>
        ///     A collection of the current output nodes
        /// </summary>
        protected List<BaseNode> _OutputNodes;

        /// <summary>
        ///     Th number of data entries to not use when building the trainign dataset, this allows testing and training against
        ///     different sets
        /// </summary>
        protected Int32 _PortionOfDatasetReserved;

        /// <summary>
        ///     Any recurrent layers that are present in the network structure
        /// </summary>
        protected List<Layer> _Recurrentlayers = new List<Layer>();

        /// <summary>
        ///     The RND used during the netowrk setup when required
        /// </summary>
        protected Random _Rnd;

        /// <summary>
        ///     The number of sequences that weill be tested
        /// </summary>
        protected Int32 _SequenceCount;

        /// <summary>
        /// Sets the output value of the node to the target
        /// </summary>
        protected Boolean _SetOuputToTarget;

        /// <summary>
        ///     The width of the presented window
        /// </summary>
        protected Int32 _WindowWidth;

        public SlidingWindow()
        {
            _Rnd = new Random();
        }

        public SlidingWindow(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _DistanceToForcastHorizon = info.GetInt32("_DistanceToForcastHorizon");
            _InputNodes = (List<BaseNode>)info.GetValue("_InputNodes", typeof(List<BaseNode>));
            _LastPassAverageError = info.GetDouble("_LastPassAverageError");
            _OutputNodes = (List<BaseNode>)info.GetValue("_OutputNodes", typeof(List<BaseNode>));
            _PortionOfDatasetReserved = info.GetInt32("_PortionOfDatasetReserved");
            _Rnd = (Random)info.GetValue("_RND", typeof(Random));
            _Recurrentlayers = (List<Layer>)info.GetValue("_UpdatingLayers", typeof(List<Layer>));
            _WindowWidth = info.GetInt32("_WindowWidth");
            _SetOuputToTarget = info.GetBoolean("_SetOuputToTarget");
        }

        /// <summary>
        ///     How far beyond the last presented value are we attempting to predict
        /// </summary>
        public Int32 DistanceToForcastHorizon
        {
            get { return _DistanceToForcastHorizon; }
            set { _DistanceToForcastHorizon = value; }
        }

        /// <summary>
        ///     The width of the presented window
        /// </summary>
        public Int32 WindowWidth
        {
            get { return _WindowWidth; }
            set { _WindowWidth = value; }
        }

        /// <summary>
        ///     The average error experianced during the last pass
        /// </summary>
        public double LastPassAverageError
        {
            get { return _LastPassAverageError; }
        }

        public virtual void SetOutputToTarget(Boolean state)
        {
            _SetOuputToTarget = state;
        }

        /// <summary>
        ///     Sets the length of the dataset that is reserved from training.
        /// </summary>
        /// <param name='reservedPortion'>
        ///     Reserved portion.
        /// </param>
        public virtual void SetDatasetReservedLength(Int32 reservedPortion)
        {
            _PortionOfDatasetReserved = reservedPortion;
        }

        /// <summary>
        ///     Sets the input nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetInputNodes(List<BaseNode> nodes)
        {
            _InputNodes = nodes;
        }

        /// <summary>
        ///     Sets the output nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetOutputNodes(List<BaseNode> nodes)
        {
            _OutputNodes = nodes;
        }

        /// <summary>
        ///     Sets the recurrent conext layers.
        /// </summary>
        /// <param name='layers'>
        ///     Layers.
        /// </param>
        public virtual void SetRecurrentConextLayers(List<Layer> layers)
        {
            _Recurrentlayers = layers;
        }

        /// <summary>
        ///     Sets the learning rate.
        /// </summary>
        /// <param name='rate'>
        ///     Rate.
        /// </param>
        public virtual void SetLearningRate(Double rate)
        {
            if (_TargetNetwork == null) throw new Exception("Target Network must be defined first");
            _TargetNetwork.LearningRate=(rate);
        }

        /// <summary>
        ///     Sets the momentum.
        /// </summary>
        /// <param name='momentum'>
        ///     Momentum.
        /// </param>
        public virtual void SetMomentum(Double momentum)
        {
            if (_TargetNetwork == null) throw new Exception("Target Network must be defined first");
            _TargetNetwork.Momentum=(momentum);
        }

        /// <summary>
        ///     Prepares the data before training.
        /// </summary>
        public virtual void PrepareData()
        {
            _SequenceCount = _WorkingDataset[0].GetLength(0) - _PortionOfDatasetReserved - WindowWidth - DistanceToForcastHorizon;
            Int32 inputCount = _InputNodes.Count;
            Int32 outputCount = _OutputNodes.Count;
            _InputSequences = new Double[_SequenceCount, WindowWidth, inputCount];
            _ExpectedOutputs = new Double[_SequenceCount, outputCount];
            for (Int32 i = 0; i < _SequenceCount; i++)
            {
                for (Int32 j = 0; j < WindowWidth; j++)
                {
                    for (Int32 k = 0; k < inputCount; k++) _InputSequences[i, j, k] = _WorkingDataset[k][i + j];
                    for (Int32 l = 0; l < outputCount; l++) _ExpectedOutputs[i, l] = _WorkingDataset[l][i + j + DistanceToForcastHorizon];
                }
            }
        }

        public override void Dispose()
        {
            _InputNodes?.Clear();
            _LogStream?.Dispose();
            _OutputNodes?.Clear();
            _Recurrentlayers?.Clear();
            _Rnd = null;
            base.Dispose();
        }

        #region implemented abstract members of Cranium.Activity.Training.Base

        /// <summary>
        ///     This function is called repeatedly untill trainingas been instructed to stop or untill the stopping criteria has
        ///     been met
        /// </summary>
        /// <returns>
        ///     The tick.
        /// </returns>
        protected override Boolean _Tick()
        {
            if (CurrentEpoch >= _MaxEpochs) return false;
            Double error = 0;

            // if the Dynamic Learning Rate delegate is set call it
            if (_DynamicLearningRate != null) _TargetNetwork.LearningRate=(_DynamicLearningRate(CurrentEpoch, _LastPassAverageError));
            // if the Dynamic Momentum delegate is set call it
            if (_DynamicMomentum != null) _TargetNetwork.Momentum=(_DynamicMomentum(CurrentEpoch, _LastPassAverageError));

            List<Int32> sequencyList = new List<Int32>(_SequenceCount);

            for (Int32 s = 0; s < _SequenceCount; s++) sequencyList.Add(s);

            while (sequencyList.Count > 0)
            {
                //This needs to be booled so it can be turned off
                Int32 s = sequencyList[_Rnd.Next(0, sequencyList.Count)];
                sequencyList.Remove(s);

                foreach (BaseNode node in _TargetNetwork.GetCurrentLayers().SelectMany(layer => layer.GetNodes())) node.SetValue(0);

                for (Int32 i = 0; i < WindowWidth; i++)
                {
                    for (Int32 x = 0; x < _InputNodes.Count; x++) _InputNodes[x].SetValue(_InputSequences[s, i, x]);
                    _TargetNetwork.FowardPass();

                    foreach (RecurrentContext layer in _Recurrentlayers.Cast<RecurrentContext>()) layer.UpdateExtra();
                }
                for (Int32 x = 0; x < _OutputNodes.Count; x++)
                {
                    OutputNode output = _OutputNodes[x] as OutputNode;
                    output?.SetTargetValue(_ExpectedOutputs[s, x]);
                }

                _TargetNetwork.ReversePass();

                if (_SetOuputToTarget)
                {
                    for (Int32 x = 0; x < _OutputNodes.Count; x++)
                    {
                        OutputNode output = _OutputNodes[x] as OutputNode;
                        output?.SetValue(_ExpectedOutputs[s, x]);
                    }
                }

                //Calculate the current error
                Double passError = _OutputNodes.OfType<OutputNode>().Sum(output => output.GetError());
                passError /= _OutputNodes.Count;
                error += passError * passError;
            }
            _LastPassAverageError = error / _SequenceCount;
            _LogStream?.WriteLine(_LastPassAverageError);
            _LogStream?.Flush();
            return true;
        }

        /// <summary>
        ///     Called as this training instance starts
        /// </summary>
        protected override void Starting()
        {
            PrepareData();
            _LastPassAverageError = 0;
        }

        /// <summary>
        ///     Called if this instance is stopped/finished
        /// </summary>
        protected override void Stopping()
        {
            _LogStream?.Close();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_DistanceToForcastHorizon", DistanceToForcastHorizon);
            info.AddValue("_InputNodes", _InputNodes, typeof(List<BaseNode>));
            info.AddValue("_LastPassAverageError", _LastPassAverageError);
            info.AddValue("_OutputNodes", _OutputNodes, typeof(List<BaseNode>));
            info.AddValue("_PortionOfDatasetReserved", _PortionOfDatasetReserved);
            info.AddValue("_RND", _Rnd, typeof(Random));
            info.AddValue("_UpdatingLayers", _Recurrentlayers, typeof(List<Layer>));
            info.AddValue("_WindowWidth", WindowWidth);
            info.AddValue("_SetOuputToTarget", WindowWidth);
        }

        #endregion
    }
}