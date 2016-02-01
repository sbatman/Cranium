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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Cranium.Lib.Structure;

#endregion

namespace Cranium.Lib.Activity.Training
{
    [Serializable]
    public abstract class Base : Activity.Base
    {
        private Int32 _CurrentEpoch;
        protected DynamicVariable _DynamicLearningRate;
        protected DynamicVariable _DynamicMomentum;
        protected ReaderWriterLockSlim _Lock = new ReaderWriterLockSlim();
        private Thread _LoopThread;
        protected Int32 _MaxEpochs;
        private Boolean _Running;
        private Boolean _Stopping;
        protected Network _TargetNetwork;
        protected Double[][] _WorkingDataset;

        public delegate Double DynamicVariable(Int32 epoch, Double currentRmse);

        public Int32 CurrentEpoch
        {
            get
            {
                _Lock.EnterReadLock();
                Int32 returnValue = _CurrentEpoch;
                _Lock.ExitReadLock();
                return returnValue;
            }
        }

        /// <summary>
        ///     Determines whether this training activity is running.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </returns>
        public Boolean Running
        {
            get
            {
                _Lock.EnterReadLock();
                Boolean returnValue = _Running;
                _Lock.ExitReadLock();
                return returnValue;
            }
            protected set
            {
                _Lock.EnterWriteLock();
                _Running = value;
                _Lock.ExitWriteLock();
            }
        }

        protected Base()
        {
        }

        protected Base(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _CurrentEpoch = info.GetInt32("_CurrentEpoch");
            _DynamicLearningRate = (DynamicVariable) info.GetValue("_DynamicLearningRate", typeof (DynamicVariable));
            _DynamicMomentum = (DynamicVariable) info.GetValue("_DynamicMomentum", typeof (DynamicVariable));
            _MaxEpochs = info.GetInt32("_MaxEpochs");
            _TargetNetwork = (Network) info.GetValue("_TargetNetwork", typeof (Network));
            _WorkingDataset = (Double[][]) info.GetValue("_WorkingDataset", typeof (Double[][]));
        }

        /// <summary>
        ///     Start this training activity (launched in a sperate thread).
        /// </summary>
        public void Start()
        {
            _CurrentEpoch = 0;
            if (_LoopThread != null)
            {
                Debug.Assert(_LoopThread != null, "Calling Start on a training activity that has already been started");
                _LoopThread.Abort();
                _LoopThread = null;
            }
            _LoopThread = new Thread(_UpdateLoop);
            _LoopThread.Start();
        }

        public void StartSynchronous()
        {
            _CurrentEpoch = 0;

            Running = true;

            Starting();
            while (_Tick() && !_Stopping)
            {
                _Lock.EnterWriteLock();
                _CurrentEpoch++;
                _Lock.ExitWriteLock();
            }
            Stopping();

            Running = false;
        }

        /// <summary>
        ///     Sets the target network.
        /// </summary>
        /// <param name='targetNetwork'>
        ///     Target network.
        /// </param>
        public virtual void SetTargetNetwork(Network targetNetwork)
        {
            _TargetNetwork = targetNetwork;
        }

        /// <summary>
        ///     Returns the current target network
        /// </summary>
        /// <returns></returns>
        public virtual Network GetTargetNetwork()
        {
            return _TargetNetwork;
        }

        /// <summary>
        ///     Sets the working dataset.
        /// </summary>
        /// <param name='workingDataset'>
        ///     Working dataset.
        /// </param>
        public virtual void SetWorkingDataset(Double[][] workingDataset)
        {
            _WorkingDataset = workingDataset;
        }

        public virtual Double[][] GetWorkingDataset()
        {
            return _WorkingDataset;
        }

        /// <summary>
        ///     Sets the maximum epochs.
        /// </summary>
        /// <param name='epochs'>
        ///     Epochs.
        /// </param>
        public virtual void SetMaximumEpochs(Int32 epochs)
        {
            _MaxEpochs = epochs;
        }

        public virtual Int32 GetMaximumEpcohs()
        {
            return _MaxEpochs;
        }

        /// <summary>
        ///     Stop this training activity.
        /// </summary>
        public void Stop()
        {
            _Stopping = true;
        }

        /// <summary>
        ///     This function is called repeatedly untill trainingas been instructed to stop or untill the stopping criteria has
        ///     been met
        /// </summary>
        /// <returns>
        ///     The tick.
        /// </returns>
        protected abstract Boolean _Tick();

        /// <summary>
        ///     Called as this training instance starts
        /// </summary>
        protected abstract void Starting();

        /// <summary>
        ///     Called if this instance is stopped/finished
        /// </summary>
        protected abstract void Stopping();

        /// <summary>
        ///     Logic loop that is operated on another thread
        /// </summary>
        private void _UpdateLoop()
        {
            Running = true;
            Starting();
            while (_Tick() && !_Stopping) _CurrentEpoch++;
            Stopping();
            Running = false;
        }

        /// <summary>
        ///     Sets the dynamic learning rate delegate, passing null will switch back to static learning rate.
        /// </summary>
        /// <param name='function'>
        ///     Function.
        /// </param>
        public virtual void SetDynamicLearningRateDelegate(DynamicVariable function)
        {
            _DynamicLearningRate = function;
        }

        /// <summary>
        ///     Sets the dynamic momenum delegate passing null will switch back to static momentum.
        /// </summary>
        /// <param name='function'>
        ///     Function.
        /// </param>
        public virtual void SetDynamicMomenumDelegate(DynamicVariable function)
        {
            _DynamicMomentum = function;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_CurrentEpoch", _CurrentEpoch);
            info.AddValue("_DynamicLearningRate", _DynamicLearningRate, typeof (DynamicVariable));
            info.AddValue("_DynamicMomentum", _DynamicMomentum, typeof (DynamicVariable));
            info.AddValue("_MaxEpochs", _MaxEpochs);
            info.AddValue("_TargetNetwork", _TargetNetwork, _TargetNetwork.GetType());
            info.AddValue("_WorkingDataset", _WorkingDataset, _WorkingDataset.GetType());
        }

        public override void Dispose()
        {
            _TargetNetwork?.Dispose();
            _DynamicLearningRate = null;
            _DynamicMomentum = null;
            _LoopThread?.Abort();

            base.Dispose();
        }
    }
}