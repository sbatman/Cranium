﻿using System;
using System.Diagnostics;
using System.Threading;
using Cranium.Lib.Activity;

namespace Cranium.Lobe.Worker
{
    public class WorkerThread
    {
        protected Base _CurrentWork;
        protected Thread _InternalThread;
        protected Object _LockingObject = new Object();
        protected bool _Running;
        protected Worker _ParentWorker;

        public WorkerThread(Worker parentWorker)
        {
            if (parentWorker == null) throw new ArgumentNullException("parentWorker");
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
                        _ParentWorker.AnnounceStatus("Worker service starting job " + _CurrentWork.GetGUID());
                        if (_CurrentWork is Lib.Activity.Training.Base)
                        {
                            var trainingWork = (Lib.Activity.Training.Base) _CurrentWork;
                            trainingWork.StartSynchronous();
                            _ParentWorker.AddToCompletedWork(trainingWork);
                        }
                        _ParentWorker.AnnounceStatus("Worker service Completed job " + _CurrentWork.GetGUID());
                        _CurrentWork = null;
                    }
                    Thread.Sleep(100);
                }
            }
            catch(Exception e)
            {
                _ParentWorker.AnnounceStatus("Worker exception");
                _ParentWorker.AnnounceStatus(e.ToString());
            }
            _Running = false;
            _ParentWorker.DecrementCurrentThreads();
        }

        public bool IsRunning()
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