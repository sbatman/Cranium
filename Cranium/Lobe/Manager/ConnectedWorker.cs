﻿// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Lobe.Manager
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Cranium.Lib.Activity;
using Sbatman.Networking.Server;
using Sbatman.Serialize;

namespace Cranium.Lobe.Manager
{
	internal class ConnectedWorker : ClientConnection
	{
		/// <summary>
		///    How often a ping packet should be sent to ensure the worker is still there
		/// </summary>
		private readonly TimeSpan _PingInterval = new TimeSpan(0, 0, 0, 5);

		/// <summary>
		///    The last time a ping packet was sent
		/// </summary>
		private DateTime _LastPing = DateTime.Now;

		public ConnectedWorker(BaseServer server, TcpClient incomingSocket) : base(server, incomingSocket, 204800)
		{
		}

		protected override void ClientUpdateLogic()
		{
			if (DateTime.Now - _LastPing > _PingInterval)
			{
				SendPacket(new Packet(9999));
				_LastPing = DateTime.Now;
			}

			if (GetOutStandingProcessingPacketsCount() == 0) return;
			List<Packet> packetstoProcess = GetOutStandingProcessingPackets();
			foreach (Packet p in packetstoProcess)
			{
				switch (p.Type)
				{
					case 201:
						HandelA201(p);
						break;
					case 300:
						HandelA300(p);
						break;
					case 400:
						HandelA400(p);
						break;
				}
			}
		}

		protected override void OnConnect()
		{
			Console.WriteLine("New Worker Connected from " + ((IPEndPoint) _AttachedSocket.Client.RemoteEndPoint).Address);
			SendPacket(new Packet(200)); //Lets say hello
		}

		protected override void OnDisconnect()
		{
			Console.WriteLine("Worker Disconnected");
		}

		/// <summary>
		///    Handels a packet of type 201, This is a response to the hello packet send by the lobe manager (200)
		///    and contains the current number of worker threads registered to that worker.
		/// </summary>
		/// <param name="p"></param>
		protected void HandelA201(Packet p)
		{
		}

		/// <summary>
		///    Handels a packet of type 300, This is a work request packet from the lobe worker, these will be recieved
		///    regulary if the worker has no work at this stage. Having the workers poll the manager better fits the
		///    parent child model and reliance pathways.
		/// </summary>
		/// <param name="p"></param>
		protected void HandelA300(Packet p)
		{
			Base work = Program.GetPendingJob();
			if (work == null) SendPacket(new Packet(301)); //got no work
			else
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream datapackage = new MemoryStream();
				binaryFormatter.Serialize(datapackage, work);

				Packet responsePacket = new Packet(302);
				responsePacket.Add(datapackage.ToArray());
				SendPacket(responsePacket);
			}
		}

		protected void HandelA400(Packet p)
		{
			Object[] packetObjects = p.GetObjects();
			Byte[] jobData = (Byte[]) packetObjects[0];

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			Base activity = (Base) binaryFormatter.Deserialize(new MemoryStream(jobData));
			Program.RegisterCompletedWork(activity);
		}

		protected override void HandelException(Exception e)
		{
		}
	}
}