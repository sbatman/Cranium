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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Cranium.Lib.Activity;
using Sbatman.Networking.Client;
using Sbatman.Serialize;

namespace Cranium.Lobe.Worker
{
    public class Worker : IDisposable
    {
        /// <summary>
        ///     This is a list of completed jobs, these will need ot be uploaded to the lobe manager when possible
        /// </summary>
        protected readonly List<Base> CompletedWork = new List<Base>();

        /// <summary>
        ///     The current connection to the lobe manager, a connection is not required for work to be completed however for the
        ///     manager to recieve work or
        ///     for this worker lobe to get further work it will be required.
        /// </summary>
        protected readonly BaseClient ConnectionToLobeManager = new BaseClient();

        /// <summary>
        ///     This is a list of packets recieved from the lobe manager that needs to be processed when possible
        /// </summary>
        protected readonly List<Packet> PacketsToBeProcessed = new List<Packet>();

        /// <summary>
        ///     A list of all the current pending work that needs to be processed. This will commonly contain
        ///     only one pending job unless settings are changed to specify otherwise.
        /// </summary>
        protected readonly List<Base> PendingWork = new List<Base>();

        /// <summary>
        ///     The time spent without communication from the lobe manager after we consider the communcations down and attempt to
        ///     reconnect  (for thoes odd tcp stak times)
        /// </summary>
        protected readonly TimeSpan TimeBeforeManagerConsideredLost = new TimeSpan(0, 0, 1, 0);

        /// <summary>
        ///     A list containing all the active worker services
        /// </summary>
        protected List<WorkerThread> _ActiveWorkerServices = new List<WorkerThread>();

        protected Boolean _CanRequestWork;

        private Int64 _CompletedJobs;
        private Int32 _CurrentThreads;

        /// <summary>
        ///     States wether the system is running and when set to false acts as a kill switch
        /// </summary>
        protected Boolean _Running;

        protected Settings _Settings = new Settings();
        private Int32 _TargetThreads;

        /// <summary>
        ///     The timestamp of the last received comunications from the lobe manager.
        /// </summary>
        protected DateTime _TimeOfLastManagerComms;

        public delegate void MessageCallBack(String s);

        public void Dispose()
        {
            ConnectionToLobeManager.Disconnect();
            _ActiveWorkerServices.Clear();
            PendingWork.Clear();
            CompletedWork.Clear();
            PacketsToBeProcessed.Clear();
        }

        public event MessageCallBack HandelMessage;

        public void AnnounceStatus(String msg)
        {
            lock (this) HandelMessage?.Invoke(msg);
        }

        public void Start(Settings settings)
        {
            _Settings = settings;
            _CanRequestWork = true;
            AnnounceStatus("Lobe Worker Launching");
            _Running = true;
            _TimeOfLastManagerComms = DateTime.Now;
            _TargetThreads = _Settings.WorkerThreadCount;

            //Prepare the worker threads
            AnnounceStatus("Preparing Workers");
            for (Int32 i = 0; i < _TargetThreads; i++)
            {
                lock (_ActiveWorkerServices)
                {
                    _ActiveWorkerServices.Add(new WorkerThread(this));
                }
            }

            AnnounceStatus("Connecting To Manager");
            lock (PacketsToBeProcessed)
            {
                if (!ConnectionToLobeManager.Connect(_Settings.CommsManagerIp, _Settings.CommsManagerPort, 204800 )) AnnounceStatus("Unable to communicate with specified lobe manager, aborting!");
            }

            AnnounceStatus("Lobe Worker Online");
            while (_Running || _ActiveWorkerServices.Count > 0)
            {
                lock (this)
                {
                    _ActiveWorkerServices = _ActiveWorkerServices.Where(a => a.IsRunning()).ToList();
                    if (_CurrentThreads < _TargetThreads && _Running)
                    {
                        _ActiveWorkerServices.Add(new WorkerThread(this));
                    }
                    if (!ConnectionToLobeManager.IsConnected())
                    {
                        String error = ConnectionToLobeManager.GetError();
                        if (!String.IsNullOrEmpty(error)) Console.WriteLine("Net Error " + error);
                        AnnounceStatus("Unable to communicate with specified lobe manager, Attempting to reconnect");
                        if (ConnectionToLobeManager.Connect(_Settings.CommsManagerIp, _Settings.CommsManagerPort, 204800 )) AnnounceStatus("Connection Re-established");
                        _CanRequestWork = true;
                    }
                    else
                    {
                        lock (CompletedWork)
                        {
                            if (CompletedWork.Count > 0)
                            {
                                using (Base job = CompletedWork[0])
                                {
                                    CompletedWork.RemoveAt(0);
                                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                                    using (MemoryStream datapackage = new MemoryStream())
                                    {
                                        binaryFormatter.Serialize(datapackage, job);
                                        _CompletedJobs++;
                                        Packet responsePacket = new Packet(400);
                                        responsePacket.Add(datapackage.ToArray(), true);
                                        ConnectionToLobeManager.SendPacket(responsePacket);

                                        if (File.Exists($"{_Settings.CompletedWorkDirectory}/{job.GetGuid()}"))
                                        {
                                            File.Delete($"{_Settings.CompletedWorkDirectory}/{job.GetGuid()}");
                                        }
                                    }
                                }
                            }
                        }
                        lock (PendingWork)
                        {
                            if (PendingWork.Count < _Settings.WorkBufferCount && _Running && _CanRequestWork)
                            {
                                Packet p = new Packet(300); //Generate a work request packet
                                ConnectionToLobeManager.SendPacket(p);
                                _CanRequestWork = false;
                            }
                        }
                        lock (ConnectionToLobeManager)
                        {
                            if (ConnectionToLobeManager.GetPacketsToProcessCount() > 0)
                            {
                                //manager is communicating
                                _TimeOfLastManagerComms = DateTime.Now;
                                lock (PacketsToBeProcessed)
                                {
                                    PacketsToBeProcessed.AddRange(ConnectionToLobeManager.GetPacketsToProcess());
                                }
                            }
                            if (DateTime.Now - _TimeOfLastManagerComms > TimeBeforeManagerConsideredLost)
                            {
                                //Disconnect as it seems the manager isnt there , this will cause an attempted reconnect above
                                ConnectionToLobeManager.Disconnect();
                            }
                        }
                        lock (PacketsToBeProcessed)
                        {
                            foreach (Packet p in PacketsToBeProcessed) HandelIncomingPacket(p);
                            PacketsToBeProcessed.Clear();
                        }
                    }
                }
                Thread.Sleep(100);
            }
            lock (_ActiveWorkerServices)
            {
                foreach (WorkerThread aWs in _ActiveWorkerServices)
                {
                    aWs.StopGracefully();
                }
            }
            AnnounceStatus("Lobe Worker Exiting");
            Dispose();
        }

        public void KillWorkers()
        {
            lock (this) foreach (WorkerThread thread in _ActiveWorkerServices) thread.StopForcefully();
        }

        public void Stop()
        {
            lock (this) _Running = false;
        }

        public void Start()
        {
            //Attempt to load the settings
            if (!_Settings.LoadSettings("Settings.ini")) return;
            AnnounceStatus("Setting Load Successful");

            Start(_Settings);
        }

        /// <summary>
        ///     Basic function for calling packet specific functions on a given packet
        /// </summary>
        /// <param name="p"></param>
        private void HandelIncomingPacket(Packet p)
        {
            switch (p.Type)
            {
                case 200:
                    HandelA200Packet();
                    break;
                case 301:
                    HandelA301Packet();
                    break;
                case 302:
                    HandelA302Packet(p);
                    break;
            }
            p.Dispose();
        }

        /// <summary>
        ///     Handels and incoming packet with ID 200, this is a hello packet from the lobe manager to which we repsond
        ///     with the number of worker threads we ahve running
        /// </summary>
        private void HandelA200Packet()
        {
            Packet responsePacket = new Packet(201);
            responsePacket.Add(_ActiveWorkerServices.Count);
            ConnectionToLobeManager.SendPacket(responsePacket);
        }

        /// <summary>
        ///     Handels an incoming packet with ID 301, this is a No work avaliable packet
        /// </summary>
        private void HandelA301Packet()
        {
            Console.WriteLine("Servers got no work");
            _CanRequestWork = true;
        }

        /// <summary>
        ///     Handels an incoming packet with ID 302, this is a work packet
        /// </summary>
        /// <param name="p"></param>
        private void HandelA302Packet(Packet p)
        {
            AnnounceStatus("Recieved Job");
            _CanRequestWork = true;
            Object[] dataPackage = p.GetObjects();
            Byte[] serialisedAcitvity = (Byte[]) dataPackage[0];
            Base activity;
            using (MemoryStream datastream = new MemoryStream(serialisedAcitvity))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                activity = (Base) binaryFormatter.Deserialize(datastream);
            }
            if (!Directory.Exists(_Settings.PendingWorkDirectory)) Directory.CreateDirectory(_Settings.PendingWorkDirectory);
            activity.SaveToDisk(_Settings.PendingWorkDirectory + "/" + activity.GetGuid());
            lock (PendingWork)
            {
                PendingWork.Add(activity);
            }
        }

        /// <summary>
        ///     Used by the owrker threads to get a piece of work to use, this function will return null if there is no work
        ///     avaliable.
        /// </summary>
        /// <returns></returns>
        public Base GetPendingWork()
        {
            lock (PendingWork)
            {
                if (PendingWork.Count <= 0) return null;
                Base work = PendingWork[0];
                PendingWork.RemoveAt(0);
                return work;
            }
        }

        /// <summary>
        ///     adds a peice of work to the work completed list ready to be sent to the manager
        /// </summary>
        /// <param name="work"></param>
        public void AddToCompletedWork(Base work)
        {
            lock (CompletedWork)
            {
                CompletedWork.Add(work);
                if (!Directory.Exists(_Settings.CompletedWorkDirectory)) Directory.CreateDirectory(_Settings.CompletedWorkDirectory);
                work.SaveToDisk(_Settings.CompletedWorkDirectory + "/" + work.GetGuid());
            }
        }

        public Boolean IsRunning()
        {
            lock (this) return _Running;
        }

        public Boolean IsConnectedToManager()
        {
            lock (this) return ConnectionToLobeManager.IsConnected();
        }

        public Int32 GetWorkerThreadCount()
        {
            lock (this) return _ActiveWorkerServices.Count;
        }

        public Int32 GetPendingWorkCount()
        {
            lock (this) return PendingWork.Count;
        }

        public Int32 GetWorkCompletedCount()
        {
            lock (this) return CompletedWork.Count;
        }

        public Int64 GetCompletedJobCount()
        {
            lock (this) return _CompletedJobs;
        }

        public void IncrementCurrentThreads()
        {
            lock (this) _CurrentThreads++;
        }

        public void DecrementCurrentThreads()
        {
            lock (this) _CurrentThreads--;
        }

        public Boolean ThreadCountCheck()
        {
            lock (this) return _CurrentThreads > _TargetThreads;
        }

        public void SetCoreUsage(Int32 cores, Int32 max)
        {
            lock (this)
            {
                _TargetThreads = cores;
                Int64 affinityMask = 0;
                for (Int32 x = max - cores; x < max; x++)
                {
                    affinityMask |= 1 << x;
                }
                Process proc = Process.GetCurrentProcess();
                foreach (ProcessThread thread in proc.Threads.Cast<ProcessThread>().Where(thread => thread.Id != Thread.CurrentThread.ManagedThreadId))
                {
                    thread.ProcessorAffinity = (IntPtr) affinityMask;
                }
            }
        }
    }
}