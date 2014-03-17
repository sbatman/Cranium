using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using InsaneDev.Networking.Server;

namespace Cranium.Lobe.Manager
{
    internal class Program
    {
        private static Base _CommsServerClient;
        private static Base _CommsServerWorker;
        private static readonly List<Guid> PendingWork = new List<Guid>();
        private static readonly List<Tuple<Lib.Activity.Base, DateTime>> WorkBeingProcessed = new List<Tuple<Lib.Activity.Base, DateTime>>();
        private static readonly List<Guid> CompleteWork = new List<Guid>();
        private static bool _Running;

        private static void Main()
        {
            _Running = true;

            if (!SettingsLoader.LoadSettings("Settings.ini")) return;
            //Online The Comms system
            Console.WriteLine("Starting Comms Server for clients");
            _CommsServerClient = new Base();
            _CommsServerClient.Init(SettingsLoader.CommsClientLocalIP.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsClientPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsClientLocalIP), SettingsLoader.CommsClientPort), typeof(ConnectedClient));
            Console.WriteLine("Comms Server for clients Online at " + SettingsLoader.CommsClientLocalIP + ":" + SettingsLoader.CommsClientPort);

            Console.WriteLine("Starting Comms Server for workers");
            _CommsServerWorker = new Base();
            _CommsServerWorker.Init(SettingsLoader.CommsWorkerLocalIP.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsWorkerPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsWorkerLocalIP), SettingsLoader.CommsWorkerPort), typeof(ConnectedWorker));
            Console.WriteLine("Comms Server for workers Online at " + SettingsLoader.CommsWorkerLocalIP + ":" + SettingsLoader.CommsWorkerPort);

            _CommsServerClient.StartListening();
            _CommsServerWorker.StartListening();

            while (_Running)
            {
                Console.Title = "Pending:" + PendingWork.Count + " Processing:" + WorkBeingProcessed.Count + " Complete:" + CompleteWork.Count;
                lock (WorkBeingProcessed)
                {
                    List<Tuple<Lib.Activity.Base, DateTime>> lostwork = WorkBeingProcessed.Where(A => DateTime.Now - A.Item2 > SettingsLoader.WorkLostAfterTime).ToList();
                    foreach (Tuple<Lib.Activity.Base, DateTime> tuple in lostwork)
                    {
                        WorkBeingProcessed.Remove(tuple);
                        AddJob(tuple.Item1);
                        Console.WriteLine("Job lost Reschedualing " + tuple.Item1.GetGUID());
                    }
                }
                Thread.Sleep(500);

            }
        }

        public static void AddJob(Lib.Activity.Base work)
        {
            lock (PendingWork)
            {
                PendingWork.Add(work.GetGUID());
                var binaryFormatter = new BinaryFormatter();
                if (!Directory.Exists("Pending")) Directory.CreateDirectory("Pending");
                FileStream stream = File.Create("Pending/" + work.GetGUID() + ".dat");
                binaryFormatter.Serialize(stream, work);
                stream.Close();
            }
        }

        /// <summary>
        ///     Gets a single piece of pending work, if the there is none this will return null
        /// </summary>
        /// <returns>A piece of pending work or null</returns>
        public static Lib.Activity.Base GetPendingJob()
        {
            lock (PendingWork)
            {
                if (PendingWork.Count > 0)
                {
                    if (!Directory.Exists("Pending")) Directory.CreateDirectory("Pending");
                    FileStream stream = File.OpenRead("Pending/" + PendingWork[0] + ".dat");
                    var binaryFormatter = new BinaryFormatter();
                    Lib.Activity.Base work = (Lib.Activity.Base)binaryFormatter.Deserialize(stream);
                    stream.Close();
                    File.Delete("Pending/" + PendingWork[0] + ".dat");
                    PendingWork.RemoveAt(0);
                    lock (WorkBeingProcessed) WorkBeingProcessed.Add(new Tuple<Lib.Activity.Base, DateTime>(work, DateTime.Now));
                    return work;
                }
                return null;
            }
        }

        public static void RegisterCompletedWork(Lib.Activity.Base completedWork)
        {
            lock (CompleteWork)
            {
                lock (WorkBeingProcessed)
                {
                    if (WorkBeingProcessed.Count(a => a.Item1.GetGUID() == completedWork.GetGUID()) <= 0) return;
                    CompleteWork.Add(completedWork.GetGUID());
                    var binaryFormatter = new BinaryFormatter();
                    if (!Directory.Exists("Completed")) Directory.CreateDirectory("Completed");
                    FileStream stream = File.Create("Completed/" + completedWork.GetGUID() + ".dat");
                    binaryFormatter.Serialize(stream, completedWork);
                    stream.Close();
                    WorkBeingProcessed.RemoveAll(a => a.Item1.GetGUID() == completedWork.GetGUID());
                    Console.WriteLine("Completed Job Registered " + completedWork.GetGUID());
                }
            }
        }

        public static Lib.Activity.Base GetCompletedJobByGUID(Guid jobGuid)
        {
            lock (CompleteWork)
            {
                if (!CompleteWork.Contains(jobGuid))
                {
                    if(File.Exists("Completed/" + jobGuid + ".dat"))CompleteWork.Add(jobGuid);
                }
                if (CompleteWork.Contains(jobGuid))
                {
                    FileStream stream = File.OpenRead("Completed/" + jobGuid + ".dat");
                    var binaryFormatter = new BinaryFormatter();
                    Lib.Activity.Base work = (Lib.Activity.Base)binaryFormatter.Deserialize(stream);
                    stream.Close();
                    return work;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}