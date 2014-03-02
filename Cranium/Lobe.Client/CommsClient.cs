using InsaneDev.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Cranium.Lobe.Client
{
    public class CommsClient
    {
        protected InsaneDev.Networking.Client.Base _ConnectionToManager = new InsaneDev.Networking.Client.Base();

        public bool ConnectToManager(string ipAddress, int port)
        {
            return _ConnectionToManager.Connect(ipAddress, port);
        }

        public Cranium.Lib.Activity.Base GetCompletedWork(Guid jobGuid)
        {
            if (!_ConnectionToManager.IsConnected()) throw new Exception("Not connected to the manager");
            Packet p = new Packet(1100);
            p.AddBytePacket(jobGuid.ToByteArray());
            _ConnectionToManager.SendPacket(p);
            Stopwatch sendTime = new Stopwatch();
            sendTime.Start();
            bool recievedResponce = false;
            while (sendTime.ElapsedMilliseconds < 5000 && !recievedResponce)
            {
                if (_ConnectionToManager.GetPacketsToProcessCount() > 0)
                {
                    foreach (Packet packet in _ConnectionToManager.GetPacketsToProcess())
                    {
                        if (packet.Type == 1101)
                        {
                            return null;
                        }
                        if (packet.Type == 1102)
                        {
                            object[] packetObjects = packet.GetObjects();
                            byte[] JobData = (byte[])packetObjects[0];

                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            return (Cranium.Lib.Activity.Base)binaryFormatter.Deserialize(new MemoryStream(JobData));
                        }
                    }
                }
                Thread.Sleep(100);
            }
            return null;
        }

        public Guid SendJob(Cranium.Lib.Activity.Base activity)
        {
            if (!_ConnectionToManager.IsConnected()) throw new Exception("Not connected to the manager");
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream datastream = new MemoryStream();
            binaryFormatter.Serialize(datastream, activity);
            Packet p = new Packet(1000);

            p.AddBytePacket(datastream.GetBuffer());
            _ConnectionToManager.SendPacket(p);
            Stopwatch sendTime = new Stopwatch();
            sendTime.Start();
            bool recievedResponce = false;
            while (sendTime.ElapsedMilliseconds < 5000 && !recievedResponce)
            {
                if (_ConnectionToManager.GetPacketsToProcessCount() > 0)
                {
                    foreach (Packet packet in _ConnectionToManager.GetPacketsToProcess())
                    {
                        if (packet.Type == 1001)
                        {
                            Guid jobGuid = new Guid((byte[]) packet.GetObjects()[0]);
                            Console.WriteLine("Work request sucess job registered as " + jobGuid);
                            recievedResponce = true;
                            return jobGuid;
                        }
                    }
                }
                Thread.Sleep(100);
            }
            throw new Exception("Mananger unavailable or busy");
        }
    }
}
