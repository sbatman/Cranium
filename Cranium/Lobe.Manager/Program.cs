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
            _CommsServerClient.Init(SettingsLoader.CommsClientLocalIp.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsClientPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsClientLocalIp), SettingsLoader.CommsClientPort), typeof (ConnectedClient));
            Console.WriteLine("Comms Server for clients Online at " + SettingsLoader.CommsClientLocalIp + ":" + SettingsLoader.CommsClientPort);

            Console.WriteLine("Starting Comms Server for workers");
            _CommsServerWorker = new BaseServer();
            _CommsServerWorker.Init(SettingsLoader.CommsWorkerLocalIp.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsWorkerPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsWorkerLocalIp), SettingsLoader.CommsWorkerPort), typeof (ConnectedWorker));
            Console.WriteLine("Comms Server for workers Online at " + SettingsLoader.CommsWorkerLocalIp + ":" + SettingsLoader.CommsWorkerPort);

            Console.WriteLine("Loading Pending Work");
            if (Directory.Exists("Pending"))
            {
                String[] files = Directory.GetFiles("Pending");
                Console.WriteLine("Possible " + files.Length + " jobs found, loading ..");
                foreach (String file in files)
                {
                    try
                    {
                        FileStream stream = File.OpenRead(file);
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        Base work = (Base) binaryFormatter.Deserialize(stream);
                        stream.Close();
                        lock (_PendingWork)
                        {
                            _PendingWork.Add(work.GetGuid());
                        }
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
                    List<Tuple<Base, DateTime>> lostwork = _WorkBeingProcessed.Where(a => DateTime.Now - a.Item2 > SettingsLoader.WorkLostAfterTime).ToList();
                    foreach (Tuple<Base, DateTime> tuple in lostwork)
                    {
                        _WorkBeingProcessed.Remove(tuple);
                        AddJob(tuple.Item1);
                        Console.WriteLine("Job lost Reschedualing " + tuple.Item1.GetGuid());
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static void AddJob(Base work)
        {
            lock (_PendingWork)
            {
                _PendingWork.Add(work.GetGuid());
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                if (!Directory.Exists("Pending")) Directory.CreateDirectory("Pending");
                FileStream stream = File.Create("Pending/" + work.GetGuid() + ".dat");
                binaryFormatter.Serialize(stream, work);
                stream.Close();
            }
        }

        /// <summary>
        ///     Gets a single piece of pending work, if the there is none this will return null
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
                Base work = (Base) binaryFormatter.Deserialize(stream);
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
                    if (_WorkBeingProcessed.Count(a => a.Item1.GetGuid() == completedWork.GetGuid()) <= 0) return;
                    _CompleteWork.Add(completedWork.GetGuid());
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    if (!Directory.Exists("Completed")) Directory.CreateDirectory("Completed");
                    if (File.Exists("Pending/" + completedWork.GetGuid() + ".dat")) File.Delete("Pending/" + completedWork.GetGuid() + ".dat");
                    FileStream stream = File.Create("Completed/" + completedWork.GetGuid() + ".dat");
                    binaryFormatter.Serialize(stream, completedWork);
                    stream.Close();
                    _WorkBeingProcessed.RemoveAll(a => a.Item1.GetGuid() == completedWork.GetGuid());
                    Console.WriteLine("Completed Job Registered " + completedWork.GetGuid());
                }
            }
        }

        public static Base GetCompletedJobByGuid(Guid jobGuid)
        {
            lock (_CompleteWork)
            {
                if (!_CompleteWork.Contains(jobGuid))
                {
                    if (File.Exists("Completed/" + jobGuid + ".dat")) _CompleteWork.Add(jobGuid);
                }
                if (_CompleteWork.Contains(jobGuid))
                {
                    FileStream stream = File.OpenRead("Completed/" + jobGuid + ".dat");
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Base work = (Base) binaryFormatter.Deserialize(stream);
                    stream.Close();
                    return work;
                }
                return null;
            }
        }
    }
}