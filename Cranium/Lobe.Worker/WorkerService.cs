using System;
using System.Threading;
using Cranium.Lib.Activity;

namespace Cranium.Lobe.Worker
{
    internal class WorkerThread
    {
        protected Base _CurrentWork;
        protected Thread _InternalThread;
        protected Object _LockingObject = new Object();
        protected bool _Running;

        public WorkerThread()
        {
            _Running = true;
            _InternalThread = new Thread(LogicLoop);
            _InternalThread.Start();
        }

        protected void LogicLoop()
        {
            Console.WriteLine("Worker Thread Starting");
            while (_Running)
            {
                if (_CurrentWork == null) _CurrentWork = Program.GetPendingWork(); //Gets pending work from the main program
                else
                {
                    Console.WriteLine("Worker service starting job " + _CurrentWork.GetGUID());
                    if (_CurrentWork is Lib.Activity.Training.Base)
                    {
                        var trainingWork = (Lib.Activity.Training.Base) _CurrentWork;
                        trainingWork.StartSynchronous();
                        Program.AddToCompletedWork(trainingWork);
                    }
                    Console.WriteLine("Worker service Completed job " + _CurrentWork.GetGUID());
                    _CurrentWork = null;
                }
                Thread.Sleep(1000);
            }
            Console.WriteLine("Worker Thread Exiting");
        }

        public bool IsRunning() { return _Running; }

        public void StopGracefully() { _Running = false; }

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