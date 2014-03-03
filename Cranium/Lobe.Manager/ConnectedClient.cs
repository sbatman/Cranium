﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using InsaneDev.Networking;
using InsaneDev.Networking.Server;
using Base = Cranium.Lib.Activity.Base;

namespace Cranium.Lobe.Manager
{
    internal class ConnectedClient : ClientConnection
    {
        public ConnectedClient(TcpClient incomingSocket) : base(incomingSocket)
        {
            _ClientUpdateInterval = new TimeSpan(0, 0, 0, 0, 1);
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
                    case 1100:
                        HandelA1100(p);
                        break;
                }
            }
        }

        protected override void OnConnect()
        {
            Console.WriteLine("New Client Connected");
            SendPacket(new Packet(200)); //Lets say hello
        }

        protected override void OnDisconnect() { }

        /// <summary>
        ///     Handels a packet of type 1000, this packet should be used to sent a work request
        /// </summary>
        /// <param name="p"></param>
        protected void HandelA1000(Packet p)
        {
            object[] packetObjects = p.GetObjects();
            Guid jobGUID = Guid.NewGuid();
            var jobData = (byte[]) packetObjects[0];

            var binaryFormatter = new BinaryFormatter();
            var activity = (Base) binaryFormatter.Deserialize(new MemoryStream(jobData));
            activity.SetGUID(jobGUID);

            var returnPacket = new Packet(1001);
            returnPacket.AddBytePacketCompressed(jobGUID.ToByteArray());
            SendPacket(returnPacket);
            Program.AddJob(activity);
        }

        protected void HandelA1100(Packet p)
        {
            Object[] dataObjects = p.GetObjects();
            var jobLookupID = new Guid((byte[]) dataObjects[0]);
            Base activity = Program.GetCompletedJobByGUID(jobLookupID);
            if (activity == null) SendPacket(new Packet(1101));
            else
            {
                var returnPacket = new Packet(1102);
                var binaryFormatter = new BinaryFormatter();
                var datapackage = new MemoryStream();
                binaryFormatter.Serialize(datapackage, activity);
                returnPacket.AddBytePacketCompressed(datapackage.ToArray());
                SendPacket(returnPacket);
            }
        }
    }
}