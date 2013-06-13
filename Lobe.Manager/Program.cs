using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cranium.Lib;

namespace Cranium.Lobe.Manager
{
    class Program
    {
        private static InsaneDev.Networking.Server.Base _CommsServerClient;
        private static InsaneDev.Networking.Server.Base _CommsServerWorker;
        private static readonly List<Lib.Activity.Base> PendingWork = new List<Lib.Activity.Base>();
        private static readonly List<Lib.Activity.Base> WorkBeingProcessed = new List<Lib.Activity.Base>();
        private static readonly List<Lib.Activity.Base> CompleteWork = new List<Lib.Activity.Base>();
        private static bool _Running;
        static void Main(string[] args)
        {
            _Running = true;
            //Online The Comms system
            Console.WriteLine("Starting Comms Server for clients");
            _CommsServerClient = new InsaneDev.Networking.Server.Base();
            _CommsServerClient.Init(SettingsLoader.CommsClientLocalIP.Equals("any", System.StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsClientPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsClientLocalIP), SettingsLoader.CommsClientPort), typeof (ConnectedClient));
            Console.WriteLine("Comms Server for clients Online at " + SettingsLoader.CommsClientLocalIP + ":" + SettingsLoader.CommsClientPort);

            Console.WriteLine("Starting Comms Server for workers");
            _CommsServerWorker = new InsaneDev.Networking.Server.Base();
            _CommsServerWorker.Init(SettingsLoader.CommsWorkerLocalIP.Equals("any", System.StringComparison.InvariantCultureIgnoreCase) ? new IPEndPoint(IPAddress.Any, SettingsLoader.CommsClientPort) : new IPEndPoint(IPAddress.Parse(SettingsLoader.CommsWorkerLocalIP), SettingsLoader.CommsWorkerPort), typeof(ConnectedClient));
            Console.WriteLine("Comms Server for workers Online at " + SettingsLoader.CommsWorkerLocalIP + ":" + SettingsLoader.CommsWorkerPort);

            _CommsServerClient.StartListening();
            _CommsServerWorker.StartListening();

            while (_Running)
            {
                Thread.Sleep(200);
            }
        }
        /// <summary>
        /// Gets a single piece of pending work, if the there is none this will return null
        /// </summary>
        /// <returns>A piece of pending work or null</returns>
        public static Lib.Activity.Base GetPendingJob()
        {
            lock (PendingWork)
            {
                if (PendingWork.Count > 0)
                {
                    Lib.Activity.Base work = PendingWork [0];
                    PendingWork.RemoveAt(0);
                    return work;
                }
                return null;
            }
        }
    }
}
