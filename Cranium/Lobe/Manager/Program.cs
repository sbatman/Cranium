// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Lobe.Manager
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Cranium.Lib.Activity;
using Sbatman.Networking.Server;

namespace Cranium.Lobe.Manager
{
    internal class Program
    {
        private static BaseServer _CommsServerClient;
        private static BaseServer _CommsServerWorker;
        private static readonly List<Guid> _PendingWork = new List<Guid>();
        private static readonly List<Tuple<Base, DateTime>> _WorkBeingProcessed = new List<Tuple<Base, DateTime>>();
        private static readonly List<Guid> _CompleteWork = new List<Guid>();
        private static Boolean _Running;

        private static void Main()
        {
            _Running = true;

            if (!SettingsLoader.LoadSettings("Settings.ini")) return;
            //Online The Comms system
            Console.WriteLine("Starting Comms Server for clients");
            _CommsServerClient = new BaseServer();
            _CommsServerClient.Init(SettingsLoader.CommsClientLocalIp.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsClientPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsClientLocalIp), SettingsLoader.CommsClientPort), typeof(ConnectedClient));
            Console.WriteLine("Comms Server for clients Online at " + SettingsLoader.CommsClientLocalIp + ":" + SettingsLoader.CommsClientPort);

            Console.WriteLine("Starting Comms Server for workers");
            _CommsServerWorker = new BaseServer();
            _CommsServerWorker.Init(SettingsLoader.CommsWorkerLocalIp.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsWorkerPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsWorkerLocalIp), SettingsLoader.CommsWorkerPort), typeof(ConnectedWorker));
            Console.WriteLine("Comms Server for workers Online at {0}:{1}", SettingsLoader.CommsWorkerLocalIp, SettingsLoader.CommsWorkerPort);

            Console.WriteLine("Loading Pending Work");
            if (Directory.Exists("Pending"))
            {
                String[] files = Directory.GetFiles("Pending");
                Console.WriteLine("Possible {0} jobs found, loading ..", files.Length);
                foreach (String file in files)
                {
                    try
                    {
                        FileStream stream = File.OpenRead(file);
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        Base work = (Base)binaryFormatter.Deserialize(stream);
                        stream.Close();

                        lock (_PendingWork) _PendingWork.Add(work.ActivityInstanceIdentifier);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            Console.WriteLine("Loading Pending Completed");

            _CommsServerClient.StartListening();
            _CommsServerWorker.StartListening();

            while (_Running)
            {
                Console.Title = "Pending:" + _PendingWork.Count + " Processing:" + _WorkBeingProcessed.Count + " Complete:" + _CompleteWork.Count;
                lock (_WorkBeingProcessed)
                {
                    List<Tuple<Base, DateTime>> lostWork = _WorkBeingProcessed.Where(a => DateTime.Now - a.Item2 > SettingsLoader.WorkLostAfterTime).ToList();
                    foreach (Tuple<Base, DateTime> tuple in lostWork)
                    {
                        _WorkBeingProcessed.Remove(tuple);
                        AddJob(tuple.Item1);
                        Console.WriteLine("Job lost Rescheduling " + tuple.Item1.ActivityInstanceIdentifier);
                    }
                }

                Thread.Sleep(500);
            }
        }

        public static void AddJob(Base work)
        {
            lock (_PendingWork)
            {
                _PendingWork.Add(work.ActivityInstanceIdentifier);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                if (!Directory.Exists("Pending")) Directory.CreateDirectory("Pending");
                FileStream stream = File.Create("Pending/" + work.ActivityInstanceIdentifier + ".dat");
                binaryFormatter.Serialize(stream, work);
                stream.Close();
            }
        }

        /// <summary>
        ///    Gets a single piece of pending work, if the there is none this will return null
        /// </summary>
        /// <returns>A piece of pending work or null</returns>
        public static Base GetPendingJob()
        {
            lock (_PendingWork)
            {
                if (_PendingWork.Count <= 0) return null;
                if (!Directory.Exists("Pending")) Directory.CreateDirectory("Pending");
                FileStream stream = File.OpenRead("Pending/" + _PendingWork[0] + ".dat");
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                Base work = (Base)binaryFormatter.Deserialize(stream);
                stream.Close();

                _PendingWork.RemoveAt(0);
                lock (_WorkBeingProcessed) _WorkBeingProcessed.Add(new Tuple<Base, DateTime>(work, DateTime.Now));
                return work;
            }
        }

        public static void RegisterCompletedWork(Base completedWork)
        {
            lock (_CompleteWork)
            {
                lock (_WorkBeingProcessed)
                {
                    if (_WorkBeingProcessed.Count(a => a.Item1.ActivityInstanceIdentifier == completedWork.ActivityInstanceIdentifier) <= 0) return;
                    _CompleteWork.Add(completedWork.ActivityInstanceIdentifier);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    if (!Directory.Exists("Completed")) Directory.CreateDirectory("Completed");

                    String filename = completedWork.ActivityInstanceIdentifier + ".dat";

                    if (File.Exists($"Pending/{filename}")) File.Delete($"Pending/{filename}");

                    FileStream stream = File.Create($"Completed/{filename}");
                    binaryFormatter.Serialize(stream, completedWork);
                    stream.Close();
                    _WorkBeingProcessed.RemoveAll(a => a.Item1.ActivityInstanceIdentifier == completedWork.ActivityInstanceIdentifier);
                    Console.WriteLine("Completed Job Registered " + completedWork.ActivityInstanceIdentifier);
                }
            }
        }

        public static Base GetCompletedJobByGuid(Guid jobGuid)
        {
            lock (_CompleteWork)
            {
                String filename = $"Completed/{jobGuid}.dat";

                if (!_CompleteWork.Contains(jobGuid))
                {
                    if (File.Exists(filename)) _CompleteWork.Add(jobGuid);
                }

                if (_CompleteWork.Contains(jobGuid))
                {
                    FileStream stream = File.OpenRead(filename);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Base work = (Base)binaryFormatter.Deserialize(stream);
                    stream.Close();
                    return work;
                }

                return null;
            }
        }
    }
}