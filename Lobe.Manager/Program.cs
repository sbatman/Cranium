using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cranium.Lobe.Manager
{
    class Program
    {
        private static InsaneDev.Networking.Server.Base _CommsServer;
        static void Main(string[] args)
        {

            //Online The Comms system
            Console.WriteLine("Starting Comms Server");
            _CommsServer = new InsaneDev.Networking.Server.Base();
            if (SettingsLoader.Comms_Client_LocalIP.Equals("any", System.StringComparison.InvariantCultureIgnoreCase))
            {
                _CommsServer.Init(new System.Net.IPEndPoint(IPAddress.Any, SettingsLoader.Comms_Client_LocalIP), typeof(ConnectedClient));
            }
            else
            {
                _CommsServer.Init(new System.Net.IPEndPoint(IPAddress.Parse(SettingsLoader.Comms_Client_LocalIP), SettingsLoader.Comms_Client_LocalIP), typeof(ConnectedClient));
            }
            Console.WriteLine("Comss Server Online at " + SettingsLoader.Comms_Client_LocalIP + ":" + SettingsLoader.Comms_Client_LocalIP);
        }
    }
}
