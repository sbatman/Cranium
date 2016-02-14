﻿// //////////////////////
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
using System.Threading;
using Cranium.Lib.Activity;

namespace Cranium.Lobe.Worker
{
    public class WorkerThread
    {
        protected Base _CurrentWork;
        protected Thread _InternalThread;
        protected Object _LockingObject = new Object();
        protected Worker _ParentWorker;
        protected Boolean _Running;

        public WorkerThread(Worker parentWorker)
        {
            if (parentWorker == null) throw new ArgumentNullException(nameof(parentWorker));
            _Running = true;
            _ParentWorker = parentWorker;
            _ParentWorker.IncrementCurrentThreads();
            _InternalThread = new Thread(LogicLoop);
            _InternalThread.Start();
        }

        protected void LogicLoop()
        {
            try
            {
                while (_Running)
                {
                    if (_CurrentWork == null)
                    {
                        if (_ParentWorker.ThreadCountCheck())
                        {
                            _Running = false;
                            break;
                        }
                        _CurrentWork = _ParentWorker.GetPendingWork(); //Gets pending work from the main program
                    }
                    else
                    {
                        _ParentWorker.AnnounceStatus("Worker service starting job " + _CurrentWork.ActivityInstanceIdentifier);
                        Lib.Activity.Training.Base work = _CurrentWork as Lib.Activity.Training.Base;
                        if (work != null)
                        {
                            Lib.Activity.Training.Base trainingWork = work;
                            trainingWork.StartSynchronous();
                            _ParentWorker.AddToCompletedWork(trainingWork);
                        }
                        _ParentWorker.AnnounceStatus("Worker service Completed job " + _CurrentWork.ActivityInstanceIdentifier);
                        _CurrentWork = null;
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                _ParentWorker.AnnounceStatus("Worker exception");
                _ParentWorker.AnnounceStatus(e.ToString());
            }
            _Running = false;
            _ParentWorker.DecrementCurrentThreads();
        }

        public Boolean IsRunning()
        {
            return _Running;
        }

        public void StopGracefully()
        {
            _Running = false;
        }

        public void StopForcefully()
        {
            lock (_LockingObject)
            {
                _InternalThread.Abort();
                _Running = false;
            }
        }
    }
}