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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using InsaneDev.Networking;
using InsaneDev.Networking.Client;

namespace Cranium.Lobe.Worker
{
    public class Worker : IDisposable
    {
        public delegate void MessageCallBack(String s);

        /// <summary>
        ///     The current connection to the lobe manager, a connection is not required for work to be completed however for the
        ///     manager to recieve work or
        ///     for this worker lobe to get further work it will be required.
        /// </summary>
        protected readonly Base _ConnectionToLobeManager = new Base();

        /// <summary>
        ///     A list containing all the active worker services
        /// </summary>
        protected readonly List<WorkerThread> _ActiveWorkerServices = new List<WorkerThread>();

        /// <summary>
        ///     A list of all the current pending work that needs to be processed. This will commonly contain
        ///     only one pending job unless settings are changed to specify otherwise.
        /// </summary>
        protected readonly List<Lib.Activity.Base> _PendingWork = new List<Lib.Activity.Base>();

        /// <summary>
        ///     This is a list of completed jobs, these will need ot be uploaded to the lobe manager when possible
        /// </summary>
        protected readonly List<Lib.Activity.Base> _CompletedWork = new List<Lib.Activity.Base>();

        /// <summary>
        ///     This is a list of packets recieved from the lobe manager that needs to be processed when possible
        /// </summary>
        protected readonly List<Packet> _PacketsToBeProcessed = new List<Packet>();

        /// <summary>
        ///     The timestamp of the last received comunications from the lobe manager.
        /// </summary>
        protected DateTime _TimeOfLastManagerComms;

        /// <summary>
        ///     The time spent without communication from the lobe manager after we consider the communcations down and attempt to
        ///     reconnect  (for thoes odd tcp stak times)
        /// </summary>
        protected readonly TimeSpan _TimeBeforeManagerConsideredLost = new TimeSpan(0, 0, 1, 0);

        /// <summary>
        ///     States wether the system is running and when set to false acts as a kill switch
        /// </summary>
        protected bool _Running;

        protected Settings _Settings = new Settings();

        public event MessageCallBack HandelMessage;

        public void AnnounceStatus(string msg)
        {
            lock (this)
            {
                if (HandelMessage != null) HandelMessage(msg);
            }
        }

        public void Start(Settings settings)
        {
            _Settings = settings;
            AnnounceStatus("Lobe Worker Launching");
            _Running = true;
            _TimeOfLastManagerComms = DateTime.Now;


            //Prepare the worker threads
            AnnounceStatus("Preparing Workers");
            for (int i = 0; i < _Settings.WorkerThreadCount; i++) _ActiveWorkerServices.Add(new WorkerThread(this));

            AnnounceStatus("Connecting To Manager");
            if (!_ConnectionToLobeManager.Connect(_Settings.CommsManagerIP, _Settings.CommsManagerPort)) AnnounceStatus("Unable to communicate with specified lobe manager, aborting!");

            AnnounceStatus("Lobe Worker Online");
            while (_Running)
            {
                lock (this)
                {
                    if (!_ConnectionToLobeManager.IsConnected())
                    {
                        AnnounceStatus("Unable to communicate with specified lobe manager, Attempting to reconnect");
                        if (_ConnectionToLobeManager.Connect(_Settings.CommsManagerIP, _Settings.CommsManagerPort)) AnnounceStatus("Connection Re-established");
                    }
                    else
                    {
                        lock (_CompletedWork)
                        {
                            if (_CompletedWork.Count > 0)
                            {
                                Lib.Activity.Base job = _CompletedWork[0];
                                _CompletedWork.RemoveAt(0);
                                var binaryFormatter = new BinaryFormatter();
                                var datapackage = new MemoryStream();
                                binaryFormatter.Serialize(datapackage, job);

                                var responsePacket = new Packet(400);
                                responsePacket.AddBytePacket(datapackage.ToArray());
                                _ConnectionToLobeManager.SendPacket(responsePacket);
                            }
                        }
                        lock (_PendingWork)
                        {
                            if (_PendingWork.Count < _Settings.WorkBufferCount)
                            {
                                var p = new Packet(300); //Generate a work request packet
                                _ConnectionToLobeManager.SendPacket(p);
                            }
                        }
                        lock (_ConnectionToLobeManager)
                        {
                            if (_ConnectionToLobeManager.GetPacketsToProcessCount() > 0)
                            {
                                //manager is communicating
                                _TimeOfLastManagerComms = DateTime.Now;
                                lock (_PacketsToBeProcessed)
                                {
                                    _PacketsToBeProcessed.AddRange(_ConnectionToLobeManager.GetPacketsToProcess());
                                }
                            }
                            if (DateTime.Now - _TimeOfLastManagerComms > _TimeBeforeManagerConsideredLost)
                            {
                                //Disconnect as it seems the manager isnt there , this will cause an attempted reconnect above
                                _ConnectionToLobeManager.Disconnect();
                            }
                        }
                        lock (_PacketsToBeProcessed)
                        {
                            foreach (Packet p in _PacketsToBeProcessed) HandelIncomingPacket(p);
                            _PacketsToBeProcessed.Clear();
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            lock (_ActiveWorkerServices)
            {
                foreach (var aWS in _ActiveWorkerServices)
                {
                    aWS.StopGracefully();
                }
            }
            AnnounceStatus("Lobe Worker Exiting");
            Dispose();
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
            var responsePacket = new Packet(201);
            responsePacket.AddInt(_ActiveWorkerServices.Count);
            _ConnectionToLobeManager.SendPacket(responsePacket);
        }

        /// <summary>
        ///     Handels an incoming packet with ID 301, this is a No work avaliable packet
        /// </summary>
        private void HandelA301Packet()
        {
            Console.WriteLine("Servers got no work");
        }

        /// <summary>
        ///     Handels an incoming packet with ID 302, this is a work packet
        /// </summary>
        /// <param name="p"></param>
        private void HandelA302Packet(Packet p)
        {
            AnnounceStatus("Recieved Job");
            object[] dataPackage = p.GetObjects();
            var serialisedAcitvity = (byte[])dataPackage[0];
            var datastream = new MemoryStream(serialisedAcitvity);
            var binaryFormatter = new BinaryFormatter();
            var activity = (Lib.Activity.Base)binaryFormatter.Deserialize(datastream);
            if (!Directory.Exists(_Settings.PendingWorkDirectory)) Directory.CreateDirectory(_Settings.PendingWorkDirectory);
            activity.SaveToDisk(_Settings.PendingWorkDirectory + "/" + activity.GetGUID());
            lock (_PendingWork)
            {
                _PendingWork.Add(activity);
            }
        }

        /// <summary>
        ///     Used by the owrker threads to get a piece of work to use, this function will return null if there is no work
        ///     avaliable.
        /// </summary>
        /// <returns></returns>
        public Lib.Activity.Base GetPendingWork()
        {
            lock (_PendingWork)
            {
                if (_PendingWork.Count > 0)
                {
                    Lib.Activity.Base work = _PendingWork[0];
                    _PendingWork.RemoveAt(0);
                    return work;
                }
                return null;
            }
        }

        /// <summary>
        ///     adds a peice of work to the work completed list ready to be sent to the manager
        /// </summary>
        /// <param name="work"></param>
        public void AddToCompletedWork(Lib.Activity.Base work)
        {
            lock (_CompletedWork)
            {
                _CompletedWork.Add(work);
            }
            if (!Directory.Exists(_Settings.CompletedWorkDirectory)) Directory.CreateDirectory(_Settings.CompletedWorkDirectory);
            work.SaveToDisk(_Settings.CompletedWorkDirectory + "/" + work.GetGUID());
        }

        public void Dispose()
        {
            _ConnectionToLobeManager.Disconnect();
            _ActiveWorkerServices.Clear();
            _PendingWork.Clear();
            _CompletedWork.Clear();
            _PacketsToBeProcessed.Clear();
        }

        public bool IsRunning()
        {
            lock (this) return _Running;
        }

        public bool isConnectedToManager()
        {
            lock (this) return _ConnectionToLobeManager.IsConnected();
        }

        public int GetWorkerThreadCount()
        {
            lock (this) return _ActiveWorkerServices.Count;
        }

        public int GetPendingWorkCount()
        {
            lock (this) return _PendingWork.Count;
        }

        public int GetWorkCompletedCount()
        {
            lock (this) return _CompletedWork.Count;
        }
    }
}