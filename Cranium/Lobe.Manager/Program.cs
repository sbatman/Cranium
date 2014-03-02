using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using InsaneDev.Networking.Server;

namespace Cranium.Lobe.Manager
{
    internal class Program
    {
        private static Base _CommsServerClient;
        private static Base _CommsServerWorker;
        private static readonly List<Lib.Activity.Base> PendingWork = new List<Lib.Activity.Base>();
        private static readonly List<Lib.Activity.Base> WorkBeingProcessed = new List<Lib.Activity.Base>();
        private static readonly List<Lib.Activity.Base> CompleteWork = new List<Lib.Activity.Base>();
        private static bool _Running;

        private static void Main()
        {
            _Running = true;

            if (!SettingsLoader.LoadSettings("Settings.ini")) return;
            //Online The Comms system
            Console.WriteLine("Starting Comms Server for clients");
            _CommsServerClient = new Base();
            _CommsServerClient.Init(SettingsLoader.CommsClientLocalIP.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsClientPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsClientLocalIP), SettingsLoader.CommsClientPort), typeof (ConnectedClient));
            Console.WriteLine("Comms Server for clients Online at " + SettingsLoader.CommsClientLocalIP + ":" + SettingsLoader.CommsClientPort);

            Console.WriteLine("Starting Comms Server for workers");
            _CommsServerWorker = new Base();
            _CommsServerWorker.Init(SettingsLoader.CommsWorkerLocalIP.Equals("any", StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsWorkerPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsWorkerLocalIP), SettingsLoader.CommsWorkerPort), typeof (ConnectedWorker));
            Console.WriteLine("Comms Server for workers Online at " + SettingsLoader.CommsWorkerLocalIP + ":" + SettingsLoader.CommsWorkerPort);

            _CommsServerClient.StartListening();
            _CommsServerWorker.StartListening();

            while (_Running) Thread.Sleep(200);
        }

        public static void AddJob(Lib.Activity.Base work)
        {
            lock (PendingWork)
            {
                PendingWork.Add(work);
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
                    Lib.Activity.Base work = PendingWork[0];
                    PendingWork.RemoveAt(0);
                    lock (WorkBeingProcessed) WorkBeingProcessed.Add(work);
                    return work;
                }
                return null;
            }
        }

        public static void RegisterCompletedWork(Lib.Activity.Base completedWork)
        {
            lock (CompleteWork)
            {
                CompleteWork.Add(completedWork);
                lock (WorkBeingProcessed) WorkBeingProcessed.Remove(completedWork);
                Console.WriteLine("Completed Job Registered " + completedWork.GetGUID());
            }
        }

        public static Lib.Activity.Base GetCompletedJobByGUID(Guid jobGuid)
        {
            lock (CompleteWork)
            {
                return CompleteWork.FirstOrDefault(job => job.GetGUID() == jobGuid);
            }
        }
    }
}