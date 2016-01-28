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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Cranium.Lib.Activity;
using Sbatman.Networking.Client;
using Sbatman.Serialize;

namespace Cranium.Lobe.Client
{
    public class CommsClient
    {
        protected Int32 _CommsTimeout = 50000;
        protected BaseClient _ConnectionToManager = new BaseClient();
        protected String _IpAddress;
        protected Int32 _Port;

        public Boolean ConnectToManager(String ipAddress, Int32 port)
        {
            _IpAddress = ipAddress;
            _Port = port;
            return _ConnectionToManager.Connect(ipAddress, port, 204800 * 1024);
        }

        public void DisconnectFromManager()
        {
            _ConnectionToManager.Disconnect();
            _ConnectionToManager = null;
        }

        public Base GetCompletedWork(Guid jobGuid)
        {
            if (_ConnectionToManager == null || !_ConnectionToManager.IsConnected()) throw new Exception("Not connected to the manager");

            while (true)
            {
                Packet p = new Packet(1100);
                p.Add(jobGuid.ToByteArray(), true);
                _ConnectionToManager.SendPacket(p);
                Stopwatch sendTime = new Stopwatch();
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
                                    Object[] packetObjects = packet.GetObjects();
                                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                                    return (Base) binaryFormatter.Deserialize(new MemoryStream((Byte[]) packetObjects[0]));
                            }
                        }
                        Thread.Sleep(100);
                    }
                }
                if (_ConnectionToManager.IsConnected()) _ConnectionToManager.Disconnect();
                _ConnectionToManager.Connect(_IpAddress, _Port, 204800 * 1024);
            }
        }

        public Guid SendJob(Base activity)
        {
            if (_ConnectionToManager == null || !_ConnectionToManager.IsConnected()) throw new Exception("Not connected to the manager");

            while (true)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream datastream = new MemoryStream();
                binaryFormatter.Serialize(datastream, activity);
                Packet p = new Packet(1000);
                Byte[] data = datastream.ToArray();
                p.Add(data, true);
                _ConnectionToManager.SendPacket(p);
                Stopwatch sendTime = new Stopwatch();
                sendTime.Start();
                while (sendTime.ElapsedMilliseconds < _CommsTimeout)
                {
                    if (_ConnectionToManager.GetPacketsToProcessCount() > 0)
                    {
                        foreach (Guid jobGuid in from packet in _ConnectionToManager.GetPacketsToProcess() where packet.Type == 1001 select new Guid((Byte[]) packet.GetObjects()[0]))
                        {
                            return jobGuid;
                        }
                    }
                    Thread.Sleep(1);
                }
                if (_ConnectionToManager.IsConnected()) _ConnectionToManager.Disconnect();
                _ConnectionToManager.Connect(_IpAddress, _Port, 204800 * 1024);
            }
        }
    }
}