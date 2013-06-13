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

        public bool ConnectToWorker(string ipAddress, int port)
        {
            return _ConnectionToManager.Connect(ipAddress, port);
        }

        public bool SendJob(Cranium.Lib.Activity.Base activity)
        {
            if (!_ConnectionToManager.IsConnected()) return false;
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
                            recievedResponce = true;
                        }
                    }
                }
                Thread.Sleep(100);
            }
            if (!recievedResponce) return false;
            return true;
        }
    }
}
