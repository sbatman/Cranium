using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Cranium.Lobe.Manager
{
    static class SettingsLoader
    {
        public static int WorkerThreadCount = -1;
        public static string CommsClientLocalIP = "";
        public static int CommsClientPort = 0;
        public static string CommsWorkerLocalIP = "";
        public static int CommsWorkerPort = 0;
        /// <summary>
        /// Loads in the settings file for the lobe Manager, this will build a dictionary of variables to be used.
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

            if (dictionaryOfSettings.ContainsKey("ClientIP"))
            {
                if (dictionaryOfSettings["ClientIP"].Length == 0) throw (new Exception("ClientIP not correctly specified"));
                CommsClientLocalIP = dictionaryOfSettings["ClientIP"];
            }

            if (dictionaryOfSettings.ContainsKey("ClientPort"))
            {
                int port = 0;
                if (!int.TryParse(dictionaryOfSettings["ClientPort"], out port)) throw (new Exception("Error parsing Client Port"));
                if (port < 1000 || port > 36000) throw (new Exception("Invalid Client Port specified, must be within 1000-36000"));
                CommsClientPort = port;
            }

            if (dictionaryOfSettings.ContainsKey("WorkerIP"))
            {
                if (dictionaryOfSettings["WorkerIP"].Length == 0) throw (new Exception("WorkerIP not correctly specified"));
                CommsWorkerLocalIP = dictionaryOfSettings["WorkerIP"];
            }

            if (dictionaryOfSettings.ContainsKey("WorkerPort"))
            {
                int port = 0;
                if (!int.TryParse(dictionaryOfSettings["WorkerPort"], out port)) throw (new Exception("Error parsing WorkerPort"));
                if (port < 1000 || port > 36000) throw (new Exception("Invalid WorkerPort specified, must be within 1000-36000"));
                CommsWorkerPort = port;
            }

            return true;
        }
    }
}
