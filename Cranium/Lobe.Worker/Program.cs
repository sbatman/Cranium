#region info

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
using System.Net;
using System.Threading;

namespace Cranium.Lobe.Worker
{
    internal class Program
    {
        /// <summary>
        /// A list containing all the active worker services
        /// </summary>
        protected static InsaneDev.Networking.Client.Base _ConnectionToLobeManager = new InsaneDev.Networking.Client.Base();
        protected static readonly List<WorkerThread> _ActiveWorkerServices = new List<WorkerThread>();
        protected static bool _Running = false;
        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Lobe Worker Launching");
            _Running = true;

            //Attempt to load the settings
            if (!SettingsLoader.LoadSettings("Settings.ini")) return;
            Console.WriteLine("Setting Load Successful");

            //Prepare the worker threads
            Console.WriteLine("Preparing Workers");
            for (int i = 0; i < SettingsLoader.WorkerThreadCount; i++)
            {
                _ActiveWorkerServices.Add(new WorkerThread());
            }

            Console.WriteLine("Connecting To Manager");
            _ConnectionToLobeManager.Connect(SettingsLoader.CommsLocalIP, SettingsLoader.CommsLocalPort);
            if (!_ConnectionToLobeManager.IsConnected())
            {
                Console.WriteLine("Unable to communicate with specified lobe manager, aborting!");
            }

            Console.WriteLine("Lobe Worker Online");
            while (_Running)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Lobe Worker Exiting");
        }
    }
}