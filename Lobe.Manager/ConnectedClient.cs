using InsaneDev.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cranium.Lobe.Manager
{
    class ConnectedClient : InsaneDev.Networking.Server.ClientConnection
    {
        public ConnectedClient(TcpClient incomingSocket)
        : base(incomingSocket)
        {
        }

        protected override void ClientUpdateLogic()
        {
            if (GetOutStandingProcessingPacketsCount() == 0) return;

            List<Packet> packetstoProcess = GetOutStandingProcessingPackets();
            foreach (Packet p in packetstoProcess)
            {
                switch (p.Type)
                {
                case 1000:
                    HandelA1000(p);
                    break;
                }
            }
        }

        protected override void OnConnect()
        {
            Console.WriteLine("New Client Connected");
            SendPacket(new Packet(200)); //Lets say hello
        }

        protected override void OnDisconnect()
        {

        }

        /// <summary>
        /// Handels a packet of type 1000, this packet should be used to sent a work request
        /// </summary>
        /// <param name="p"></param>
        protected void HandelA1000(Packet p)
        {
            object[] packetObjects = p.GetObjects();
            Guid JobGUID = Guid.NewGuid();
            byte[] JobData = (byte[])packetObjects[0];

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Cranium.Lib.Activity.Base Activity = (Cranium.Lib.Activity.Base)binaryFormatter.Deserialize(new MemoryStream(JobData));

        }
    }
}
