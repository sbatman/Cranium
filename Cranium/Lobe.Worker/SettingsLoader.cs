﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Cranium.Lobe.Worker
{
    static class SettingsLoader
    {
        public static int WorkerThreadCount = -1;
        public static string CommsLocalIP;
        public static int CommsLocalPort;
        public static string CompletedWorkDirectory;
        public static string PendingWorkDirectory;
        /// <summary>
        /// Loads in the settings file for the worker agent, this will build a dictionary of variables to be used.
        /// </summary>
        /// <param name="fileName"></param>
        public static bool LoadSettings(string fileName)
        {
            Console.WriteLine("Loading Settings from file "+fileName);
            Dictionary<string, string> dictionaryOfSettings = new Dictionary<string, string>();
            if (!File.Exists(fileName)) throw (new Exception("Settings file " + fileName + " not found"));
            using (StreamReader settingsFile = File.OpenText(fileName))
            {
                List<string> fileContents = new List<string>();
                while (!settingsFile.EndOfStream) fileContents.Add(settingsFile.ReadLine());
                foreach (string[] parts in fileContents.Where(line => !line.StartsWith("#")).Select(line => line.Split("=".ToCharArray())).Where(parts => parts.Length > 1))
                {
                    dictionaryOfSettings.Add(parts[0], parts[1]);
                }
            }

            if (dictionaryOfSettings.Count == 0) throw (new Exception("No settings present in file"));

            if (dictionaryOfSettings.ContainsKey("WorkerThreadCount"))
            {
                int count;
                if (!int.TryParse(dictionaryOfSettings["WorkerThreadCount"], out count)) throw (new Exception("Error parsing WorkerThreadCount"));
                if (count < 1 || count > 255) throw (new Exception("Invalid WorkerThreadCount specified"));
                WorkerThreadCount = count;
            }
            else throw (new Exception("No WorkerThreadCount specified"));

            if (dictionaryOfSettings.ContainsKey("LocalIP"))
            {
                if (dictionaryOfSettings["LocalIP"].Length ==0 ) throw (new Exception("LocalIP not correctly specified"));
                CommsLocalIP = dictionaryOfSettings["LocalIP"];
            }
            else throw (new Exception("No LocalIP specified"));

            if (dictionaryOfSettings.ContainsKey("Port"))
            {
                int port;
                if (!int.TryParse(dictionaryOfSettings["Port"], out port)) throw (new Exception("Error parsing Port"));
                if (port < 1000 || port > 36000) throw (new Exception("Invalid Port specified, must be within 1000-36000"));
                CommsLocalPort = port;
            }
            else throw (new Exception("No Port specified"));

            if (dictionaryOfSettings.ContainsKey("CompletedWorkDirectory"))
            {
                CompletedWorkDirectory = dictionaryOfSettings ["CompletedWorkDirectory"];
            }
            else throw (new Exception("No CompletedWorkDirectory specified"));


            if (dictionaryOfSettings.ContainsKey("PendingdWorkDirectory"))
            {
                PendingWorkDirectory = dictionaryOfSettings["PendingdWorkDirectory"];
            }
            else throw (new Exception("No Pending Wor kDirectory specified"));

            return true;
        }
    }
}
