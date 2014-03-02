﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using InsaneDev.Networking;
using InsaneDev.Networking.Client;

namespace Cranium.Lobe.Client
{
    public class CommsClient
    {
        protected int _CommsTimeout = 5000;
        protected Base _ConnectionToManager = new Base();

        public bool ConnectToManager(string ipAddress, int port) { return _ConnectionToManager.Connect(ipAddress, port); }

        public void DisconnectFromManager()
        {
            _ConnectionToManager.Disconnect();
            _ConnectionToManager = null;
        }

        public Lib.Activity.Base GetCompletedWork(Guid jobGuid)
        {
            if (_ConnectionToManager == null || !_ConnectionToManager.IsConnected()) throw new Exception("Not connected to the manager");
            var p = new Packet(1100);
            p.AddBytePacket(jobGuid.ToByteArray());
            _ConnectionToManager.SendPacket(p);
            var sendTime = new Stopwatch();
            sendTime.Start();
            while (sendTime.ElapsedMilliseconds < _CommsTimeout)
            {
                if (_ConnectionToManager.GetPacketsToProcessCount() > 0)
                {
                    foreach (Packet packet in _ConnectionToManager.GetPacketsToProcess())
                    {
                        switch (packet.Type)
                        {
                            case 1101:
                                return null;
                            case 1102:
                                object[] packetObjects = packet.GetObjects();
                                var binaryFormatter = new BinaryFormatter();
                                return (Lib.Activity.Base) binaryFormatter.Deserialize(new MemoryStream((byte[]) packetObjects[0]));
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            return null;
        }

        public Guid SendJob(Lib.Activity.Base activity)
        {
            if (_ConnectionToManager == null || !_ConnectionToManager.IsConnected()) throw new Exception("Not connected to the manager");
            var binaryFormatter = new BinaryFormatter();
            var datastream = new MemoryStream();
            binaryFormatter.Serialize(datastream, activity);
            var p = new Packet(1000);

            p.AddBytePacket(datastream.GetBuffer());
            _ConnectionToManager.SendPacket(p);
            var sendTime = new Stopwatch();
            sendTime.Start();
            while (sendTime.ElapsedMilliseconds < _CommsTimeout)
            {
                if (_ConnectionToManager.GetPacketsToProcessCount() > 0)
                {
                    foreach (Guid jobGuid in from packet in _ConnectionToManager.GetPacketsToProcess() where packet.Type == 1001 select new Guid((byte[]) packet.GetObjects()[0]))
                    {
                        Console.WriteLine("Work request sucess job registered as " + jobGuid);
                        return jobGuid;
                    }
                }
                Thread.Sleep(100);
            }
            throw new Exception("Mananger unavailable or busy");
        }
    }
}