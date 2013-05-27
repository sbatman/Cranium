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
using Cranium.Lib.Structure.Node;
using RecurrentContext = Cranium.Lib.Structure.Layer.RecurrentContext;

#endregion

namespace Cranium.Lib.Activity.Training
{
    public class SlidingWindow : Base
    {
        /// <summary>
        ///     How far beyond the last presented value are we attempting to predict
        /// </summary>
        protected int _DistanceToForcastHorrison;
        /// <summary>
        ///     The outputs that are exspected
        /// </summary>
        protected double[,] _ExpectedOutputs;
        /// <summary>
        ///     A collection of the current input nodes
        /// </summary>
        protected List<Structure.Node.Base> _InputNodes;
        /// <summary>
        ///     Thre sequences of inputs to be presented during training
        /// </summary>
        protected double[,,] _InputSequences;
        /// <summary>
        ///     The average error experianced during the last pass
        /// </summary>
        protected double _LastPassAverageError;
        /// <summary>
        ///     A stream setup for logging the training information
        /// </summary>
        protected StreamWriter _LogStream;
        /// <summary>
        ///     A collection of the current output nodes
        /// </summary>
        protected List<Structure.Node.Base> _OutputNodes;
        /// <summary>
        ///     Th number of data entries to not use when building the trainign dataset, this allows testing and training against different sets
        /// </summary>
        protected int _PortionOfDatasetReserved;
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
        protected int _SequenceCount;
        /// <summary>
        ///     The width of the presented window
        /// </summary>
        protected int _WindowWidth;

        /// <summary>
        ///     Sets the width of the sliding window for data fed to the network before it is trained.
        /// </summary>
        /// <param name='windowWidth'>
        ///     Window width.
        /// </param>
        public virtual void SetWindowWidth(int windowWidth)
        {
            _WindowWidth = windowWidth;
        }

        /// <summary>
        ///     Sets the number of intervals ahead to predict
        /// </summary>
        /// <param name='distance'>
        ///     Distance.
        /// </param>
        public virtual void SetDistanceToForcastHorrison(int distance)
        {
            _DistanceToForcastHorrison = distance;
        }

        /// <summary>
        ///     Sets the length of the dataset that is reserved from training.
        /// </summary>
        /// <param name='reservedPortion'>
        ///     Reserved portion.
        /// </param>
        public virtual void SetDatasetReservedLength(int reservedPortion)
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
        public virtual void SetLearningRate(double rate)
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
        public virtual void SetMomentum(double momentum)
        {
            if (_TargetNetwork == null) throw (new Exception("Target Network must be defined first"));
            _TargetNetwork.SetMomentum(momentum);
        }

        /// <summary>
        ///     Prepares the data before training.
        /// </summary>
        public virtual void PrepareData()
        {
            _SequenceCount = ((_WorkingDataset [0].GetLength(0) - _PortionOfDatasetReserved) - _WindowWidth) -
                             _DistanceToForcastHorrison;
            int inputCount = _InputNodes.Count;
            int outputCount = _OutputNodes.Count;
            _InputSequences = new double[_SequenceCount,_WindowWidth,inputCount];
            _ExpectedOutputs = new double[_SequenceCount,outputCount];
            for (int i = 0; i < _SequenceCount; i++)
            {
                for (int j = 0; j < _WindowWidth; j++)
                {
                    for (int k = 0; k < inputCount; k++) _InputSequences [i, j, k] = _WorkingDataset [k] [i + j];
                    for (int l = 0; l < outputCount; l++) _ExpectedOutputs [i, l] = _WorkingDataset [l] [i + j + _DistanceToForcastHorrison];
                }
            }
        }

        #region implemented abstract members of Cranium.Activity.Training.Base

        protected override bool _Tick()
        {
            if (_CurrentEpoch >= _MaxEpochs) return false;
            double error = 0;

            // if the Dynamic Learning Rate delegate is set call it
            if (_DynamicLearningRate != null) _TargetNetwork.SetLearningRate(_DynamicLearningRate(_CurrentEpoch, _LastPassAverageError));
            // if the Dynamic Momentum delegate is set call it
            if (_DynamicMomentum != null) _TargetNetwork.SetMomentum(_DynamicMomentum(_CurrentEpoch, _LastPassAverageError));

            List<int> sequencyList = new List<int>(_SequenceCount);

            for (int s = 0; s < _SequenceCount; s++) sequencyList.Add(s);

            while (sequencyList.Count > 0)
            {
                //This needs to be booled so it can be turned off
                int s = sequencyList [_RND.Next(0, sequencyList.Count)];
                sequencyList.Remove(s);

                foreach (Structure.Layer.Base layer in _TargetNetwork.GetCurrentLayers()) foreach (Structure.Node.Base node in layer.GetNodes()) node.SetValue(0);

                for (int i = 0; i < _WindowWidth; i++)
                {
                    for (int x = 0; x < _InputNodes.Count; x++) _InputNodes [x].SetValue(_InputSequences [s, i, x]);
                    _TargetNetwork.FowardPass();
                    foreach (RecurrentContext layer in _Recurrentlayers) layer.UpdateExtra();
                }
                for (int x = 0; x < _OutputNodes.Count; x++)
                {
                    Output output = _OutputNodes [x] as Output;
                    if (output != null) output.SetTargetValue(_ExpectedOutputs [s, x]);
                }

                _TargetNetwork.ReversePass();

                //Calculate the current error				
                double passError = _OutputNodes.OfType<Output>().Sum(output => output.GetError());
                passError /= _OutputNodes.Count;
                error += passError*passError;
            }
            _LastPassAverageError = error/_SequenceCount;
            Console.WriteLine(_LastPassAverageError);
            _LogStream.WriteLine(_LastPassAverageError);
            _LogStream.Flush();
            return true;
        }

        /// <summary>
        ///     Called as this training instance starts
        /// </summary>
        protected override void Starting()
        {
            PrepareData();
            _LastPassAverageError = 0;
            _LogStream = File.CreateText("log.txt");
            _RND = new Random();
        }

        /// <summary>
        ///     Called if this instance is stopped/finished
        /// </summary>
        protected override void Stopping()
        {
            _LogStream.Close();
        }

        #endregion
    }
}