﻿#region info

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

#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Cranium.Lib;
using MS.Internal.Xml.XPath;
using InsaneDev.Networking;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cranium.Lobe.Worker
{
    internal class Program
    {
        /// <summary>
        /// A list containing all the active worker services
        /// </summary>
        protected static InsaneDev.Networking.Client.Base _ConnectionToLobeManager = new InsaneDev.Networking.Client.Base();
        protected static readonly List<WorkerThread> _ActiveWorkerServices = new List<WorkerThread>();
        protected static readonly List<Lib.Activity.Base> _PendingWork = new List<Lib.Activity.Base>();
        protected static readonly List<Lib.Activity.Base> _CompletedWork = new List<Lib.Activity.Base>();
        protected static readonly List<Packet> _PacketsToBeProcessed = new List<Packet>();
        protected static DateTime _TimeOfLastManagerComms;
        protected static readonly TimeSpan _TimeBeforeManagerConsideredLost = new TimeSpan(0, 0, 1, 0);
        protected static bool _Running = false;

        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Lobe Worker Launching");
            _Running = true;
            _TimeOfLastManagerComms = DateTime.Now;

            //Attempt to load the settings
            if (!SettingsLoader.LoadSettings("Settings.ini"))
            {
                return;
            }
            Console.WriteLine("Setting Load Successful");

            //Prepare the worker threads
            Console.WriteLine("Preparing Workers");
            for (int i = 0; i < SettingsLoader.WorkerThreadCount; i++)
            {
                _ActiveWorkerServices.Add(new WorkerThread());
            }

            Console.WriteLine("Connecting To Manager");
            ;
            if (!_ConnectionToLobeManager.Connect(SettingsLoader.CommsLocalIP, SettingsLoader.CommsLocalPort))
            {
                Console.WriteLine("Unable to communicate with specified lobe manager, aborting!");
            }

            Console.WriteLine("Lobe Worker Online");
            while (_Running)
            {
                if (!_ConnectionToLobeManager.IsConnected())
                {
                    Console.WriteLine("Unable to communicate with specified lobe manager, Attempting to reconnect");
                    if (_ConnectionToLobeManager.Connect(SettingsLoader.CommsLocalIP, SettingsLoader.CommsLocalPort))
                    {
                        Console.WriteLine("Connection Re-established");
                    }
                }
                else
                {
                    lock (_CompletedWork)
                    {
                        if (_CompletedWork.Count > 0)
                        {
                            //Send Work back To Manager
                        }
                    }
                    lock (_PendingWork)
                    {
                        if (_PendingWork.Count == 0)
                        {
                            Packet p = new Packet(300); //Generate a work request packet
                            _ConnectionToLobeManager.SendPacket(p);
                        }
                    }
                    lock (_ConnectionToLobeManager)
                    {
                        if (_ConnectionToLobeManager.GetPacketsToProcessCount() > 0)
                        {
                            //manager is communicating
                            _TimeOfLastManagerComms = DateTime.Now;
                            lock (_PacketsToBeProcessed)
                            {
                                _PacketsToBeProcessed.AddRange(_ConnectionToLobeManager.GetPacketsToProcess());
                            }
                        }
                        if (DateTime.Now - _TimeOfLastManagerComms > _TimeBeforeManagerConsideredLost)
                        {
                            //Disconnect as it seems the manager isnt there , this will cause an attempted reconnect above
                            _ConnectionToLobeManager.Disconnect();
                        }
                    }
                    lock (_PacketsToBeProcessed)
                    {
                        foreach (Packet p in _PacketsToBeProcessed)
                        {
                            HandelIncomingPacket(p);
                        }
                        _PacketsToBeProcessed.Clear();
                    }
                }
                Thread.Sleep(100);
            }
            Console.WriteLine("Lobe Worker Exiting");
        }
        private static void HandelIncomingPacket(Packet p)
        {
            switch (p.Type)
            {
                case 200: HandelA200Packet(p); break;
                case 301: HandelA200Packet(p); break;
                case 302: HandelA200Packet(p); break;
            }
            p.Dispose();
        }
        /// <summary>
        /// Handels and incoming packet with ID 200, this is a hello packet from the lobe manager to which we repsond
        /// with the number of worker threads we ahve running
        /// </summary>
        /// <param name="p"></param>
        private static void HandelA200Packet(Packet p)
        {
            Packet responsePacket = new Packet(201);
            responsePacket.AddInt(_ActiveWorkerServices.Count);
            _ConnectionToLobeManager.SendPacket(responsePacket);
        }
        /// <summary>
        /// Handels an incoming packet with ID 301, this is a No work avaliable packet
        /// </summary>
        /// <param name="p"></param>
        private static void HandelA301Packet(Packet p)
        {
            Console.WriteLine("Servers got no work");
        }
        /// <summary>
        /// Handels an incoming packet with ID 302, this is a work packet
        /// </summary>
        /// <param name="p"></param>
        private static void HandelA302Packet(Packet p)
        {

            object[] dataPackage = p.GetObjects();
            byte[] serialisedAcitvity = (byte[])dataPackage[0];
            MemoryStream datastream = new MemoryStream(serialisedAcitvity);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Lib.Activity.Base activity = (Lib.Activity.Base)binaryFormatter.Deserialize(datastream);
            lock (_PendingWork)
            {
                _PendingWork.Add(activity);
            }
        }
    }
}