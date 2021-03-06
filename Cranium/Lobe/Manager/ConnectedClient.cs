﻿// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Lobe.Manager
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Cranium.Lib.Activity;
using Sbatman.Networking.Server;
using Sbatman.Serialize;

namespace Cranium.Lobe.Manager
{
	internal class ConnectedClient : ClientConnection
	{
		public ConnectedClient(BaseServer server, TcpClient incomingSocket)
			: base(server, incomingSocket, 204800)
		{
			_ClientUpdateInterval = TimeSpan.FromMilliseconds(1);
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

		protected override void OnDisconnect()
		{
		}

		/// <summary>
		///    Handels a packet of type 1000, this packet should be used to send a work request
		/// </summary>
		/// <param name="p"></param>
		protected void HandelA1000(Packet p)
		{
			Object[] packetObjects = p.GetObjects();
			Guid jobGuid = Guid.NewGuid();
			Byte[] jobData = (Byte[]) packetObjects[0];

			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				Base activity = (Base) binaryFormatter.Deserialize(new MemoryStream(jobData));
				activity.ActivityInstanceIdentifier = jobGuid;

				Packet returnPacket = new Packet(1001);
				returnPacket.Add(jobGuid.ToByteArray());
				SendPacket(returnPacket);
				Program.AddJob(activity);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		protected void HandelA1100(Packet p)
		{
			Object[] dataObjects = p.GetObjects();
			Guid jobLookupID = new Guid((Byte[]) dataObjects[0]);
			Base activity = Program.GetCompletedJobByGuid(jobLookupID);
			if (activity == null) SendPacket(new Packet(1101));
			else
			{
				Packet returnPacket = new Packet(1102);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream datapackage = new MemoryStream();
				binaryFormatter.Serialize(datapackage, activity);
				returnPacket.Add(datapackage.ToArray());
				SendPacket(returnPacket);
			}
		}

		protected override void HandelException(Exception e)
		{
		}
	}
}