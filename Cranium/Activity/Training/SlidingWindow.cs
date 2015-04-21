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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.Node;
using RecurrentContext = Cranium.Lib.Structure.Layer.RecurrentContext;

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
        protected Int32 _DistanceToForcastHorrison;

        /// <summary>
        ///     The outputs that are expected
        /// </summary>
        protected Double[,] _ExpectedOutputs;

        /// <summary>
        ///     A collection of the current input nodes
        /// </summary>
        protected List<Structure.Node.Base> _InputNodes;

        /// <summary>
        ///     Thre sequences of inputs to be presented during training
        /// </summary>
        protected Double[, ,] _InputSequences;

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
        protected List<Structure.Node.Base> _OutputNodes;

        /// <summary>
        ///     Th number of data entries to not use when building the trainign dataset, this allows testing and training against
        ///     different sets
        /// </summary>
        protected Int32 _PortionOfDatasetReserved;

        /// <summary>
        ///     The RND used during the netowrk setup when required
        /// </summary>
        protected Random _RND;

        /// <summary>
        ///     Any recurrent layers that are present in the network structure
        /// </summary>
        protected List<Structure.Layer.Base> _Recurrentlayers;

        /// <summary>
        ///     The number of sequences that weill be tested
        /// </summary>
        protected Int32 _SequenceCount;

        /// <summary>
        ///     The width of the presented window
        /// </summary>
        protected Int32 _WindowWidth;

        protected Boolean _SetOuputToTarget;

        public SlidingWindow()
        {
            _RND = new Random();
        }

        public SlidingWindow(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _DistanceToForcastHorrison = info.GetInt32("_DistanceToForcastHorrison");
            _InputNodes = (List<Structure.Node.Base>)info.GetValue("_InputNodes", typeof(List<Structure.Node.Base>));
            _LastPassAverageError = info.GetDouble("_LastPassAverageError");
            _OutputNodes = (List<Structure.Node.Base>)info.GetValue("_OutputNodes", typeof(List<Structure.Node.Base>));
            _PortionOfDatasetReserved = info.GetInt32("_PortionOfDatasetReserved");
            _RND = (Random)info.GetValue("_RND", typeof(Random));
            _Recurrentlayers = (List<Structure.Layer.Base>)info.GetValue("_Recurrentlayers", typeof(List<Structure.Layer.Base>));
            _WindowWidth = info.GetInt32("_WindowWidth");
        }

        public virtual void SetOutputToTarget(Boolean state)
        {
            _SetOuputToTarget = state;
        }

        /// <summary>
        ///     Sets the width of the sliding window for data fed to the network before it is trained.
        /// </summary>
        /// <param name='windowWidth'>
        ///     Window width.
        /// </param>
        public virtual void SetWindowWidth(Int32 windowWidth)
        {
            _WindowWidth = windowWidth;
        }

        public virtual Int32 GetWindowWidth()
        {
            return _WindowWidth;
        }

        /// <summary>
        ///     Sets the number of intervals ahead to predict
        /// </summary>
        /// <param name='distance'>
        ///     Distance.
        /// </param>
        public virtual void SetDistanceToForcastHorrison(Int32 distance)
        {
            _DistanceToForcastHorrison = distance;
        }

        public virtual Int32 GetDistanceToForcastHorrison()
        {
            return _DistanceToForcastHorrison;
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
        public virtual void SetInputNodes(List<Structure.Node.Base> nodes)
        {
            _InputNodes = nodes;
        }

        /// <summary>
        ///     Sets the output nodes.
        /// </summary>
        /// <param name='nodes'>
        ///     Nodes.
        /// </param>
        public virtual void SetOutputNodes(List<Structure.Node.Base> nodes)
        {
            _OutputNodes = nodes;
        }

        /// <summary>
        ///     Sets the recurrent conext layers.
        /// </summary>
        /// <param name='layers'>
        ///     Layers.
        /// </param>
        public virtual void SetRecurrentConextLayers(List<Structure.Layer.Base> layers)
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
            if (_TargetNetwork == null) throw (new Exception("Target Network must be defined first"));
            _TargetNetwork.SetLearningRate(rate);
        }

        /// <summary>
        ///     Sets the momentum.
        /// </summary>
        /// <param name='momentum'>
        ///     Momentum.
        /// </param>
        public virtual void SetMomentum(Double momentum)
        {
            if (_TargetNetwork == null) throw (new Exception("Target Network must be defined first"));
            _TargetNetwork.SetMomentum(momentum);
        }

        /// <summary>
        ///     Prepares the data before training.
        /// </summary>
        public virtual void PrepareData()
        {
            _SequenceCount = ((_WorkingDataset[0].GetLength(0) - _PortionOfDatasetReserved) - _WindowWidth) - _DistanceToForcastHorrison;
            Int32 inputCount = _InputNodes.Count;
            Int32 outputCount = _OutputNodes.Count;
            _InputSequences = new Double[_SequenceCount, _WindowWidth, inputCount];
            _ExpectedOutputs = new Double[_SequenceCount, outputCount];
            for (Int32 i = 0; i < _SequenceCount; i++)
            {
                for (Int32 j = 0; j < _WindowWidth; j++)
                {
                    for (Int32 k = 0; k < inputCount; k++) _InputSequences[i, j, k] = _WorkingDataset[k][i + j];
                    for (Int32 l = 0; l < outputCount; l++) _ExpectedOutputs[i, l] = _WorkingDataset[l][i + j + _DistanceToForcastHorrison];
                }
            }
        }

        public override void Dispose()
        {
            if (_InputNodes != null) _InputNodes.Clear();
            if (_LogStream != null) _LogStream.Dispose();
            if (_OutputNodes != null) _OutputNodes.Clear();
            if (_Recurrentlayers != null) _Recurrentlayers.Clear();
            _RND = null;
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
            if (_CurrentEpoch >= _MaxEpochs) return false;
            Double error = 0;

            // if the Dynamic Learning Rate delegate is set call it
            if (_DynamicLearningRate != null) _TargetNetwork.SetLearningRate(_DynamicLearningRate(_CurrentEpoch, _LastPassAverageError));
            // if the Dynamic Momentum delegate is set call it
            if (_DynamicMomentum != null) _TargetNetwork.SetMomentum(_DynamicMomentum(_CurrentEpoch, _LastPassAverageError));

            List<Int32> sequencyList = new List<Int32>(_SequenceCount);

            for (Int32 s = 0; s < _SequenceCount; s++) sequencyList.Add(s);

            while (sequencyList.Count > 0)
            {
                //This needs to be booled so it can be turned off
                Int32 s = sequencyList[_RND.Next(0, sequencyList.Count)];
                sequencyList.Remove(s);

                foreach (Structure.Node.Base node in _TargetNetwork.GetCurrentLayers().SelectMany(layer => layer.GetNodes())) node.SetValue(0);

                for (Int32 i = 0; i < _WindowWidth; i++)
                {
                    for (Int32 x = 0; x < _InputNodes.Count; x++) _InputNodes[x].SetValue(_InputSequences[s, i, x]);
                    _TargetNetwork.FowardPass();
                    foreach (RecurrentContext layer in _Recurrentlayers) layer.UpdateExtra();
                }
                for (Int32 x = 0; x < _OutputNodes.Count; x++)
                {
                    Output output = _OutputNodes[x] as Output;
                    if (output != null) output.SetTargetValue(_ExpectedOutputs[s, x]);
                }

                _TargetNetwork.ReversePass();

                if (_SetOuputToTarget)
                {
                    for (Int32 x = 0; x < _OutputNodes.Count; x++)
                    {
                        Output output = _OutputNodes[x] as Output;
                        if (output != null) output.SetValue(_ExpectedOutputs[s, x]);
                    }
                }

                //Calculate the current error
                Double passError = _OutputNodes.OfType<Output>().Sum(output => output.GetError());
                passError /= _OutputNodes.Count;
                error += passError * passError;
            }
            _LastPassAverageError = error / _SequenceCount;
            Console.WriteLine(_LastPassAverageError);
            if (_LogStream != null) _LogStream.WriteLine(_LastPassAverageError);
            if (_LogStream != null) _LogStream.Flush();
            return true;
        }

        /// <summary>
        ///     Called as this training instance starts
        /// </summary>
        protected override void Starting()
        {
            //todo: some real logging would be nice
            PrepareData();
            _LastPassAverageError = 0;
            try
            {
                if (_LogStream == null) _LogStream = File.CreateText("log.txt");
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        ///     Called if this instance is stopped/finished
        /// </summary>
        protected override void Stopping()
        {
            if (_LogStream != null) _LogStream.Close();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_DistanceToForcastHorrison", _DistanceToForcastHorrison);
            info.AddValue("_InputNodes", _InputNodes, typeof(List<Structure.Node.Base>));
            info.AddValue("_LastPassAverageError", _LastPassAverageError);
            info.AddValue("_OutputNodes", _OutputNodes, typeof(List<Structure.Node.Base>));
            info.AddValue("_PortionOfDatasetReserved", _PortionOfDatasetReserved);
            info.AddValue("_RND", _RND, typeof(Random));
            info.AddValue("_Recurrentlayers", _Recurrentlayers, typeof(List<Structure.Layer.Base>));
            info.AddValue("_WindowWidth", _WindowWidth);
        }

        #endregion
    }
}