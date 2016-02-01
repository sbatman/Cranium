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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cranium.Lobe.Worker
{
    public class Settings
    {
        public String CommsManagerIp;
        public Int32 CommsManagerPort;
        public String CompletedWorkDirectory;
        public String PendingWorkDirectory;
        public Int32 WorkBufferCount = -1;
        public Int32 WorkerThreadCount = -1;

        /// <summary>
        ///     Loads in the settings file for the worker agent, this will build a dictionary of variables to be used.
        /// </summary>
        /// <param name="fileName"></param>
        public Boolean LoadSettings(String fileName)
        {
            Console.WriteLine("Loading Settings from file " + fileName);
            Dictionary<String, String> dictionaryOfSettings = new Dictionary<String, String>();
            if (!File.Exists(fileName)) throw new Exception("Settings file " + fileName + " not found");
            using (StreamReader settingsFile = File.OpenText(fileName))
            {
                List<String> fileContents = new List<String>();
                while (!settingsFile.EndOfStream) fileContents.Add(settingsFile.ReadLine());
                foreach (String[] parts in
                    fileContents.Where(line => !line.StartsWith("#")).Select(line => line.Split("=".ToCharArray())).Where(parts => parts.Length > 1)) dictionaryOfSettings.Add(parts[0], parts[1]);
            }

            if (dictionaryOfSettings.Count == 0) throw new Exception("No settings present in file");

            if (dictionaryOfSettings.ContainsKey("WorkerThreadCount"))
            {
                Int32 count;
                if (!Int32.TryParse(dictionaryOfSettings["WorkerThreadCount"], out count)) throw new Exception("Error parsing WorkerThreadCount");
                if (count < 1 || count > 255) throw new Exception("Invalid WorkerThreadCount specified");
                WorkerThreadCount = count;
            }
            else throw new Exception("No WorkerThreadCount specified");

            if (dictionaryOfSettings.ContainsKey("WorkBufferCount"))
            {
                Int32 count;
                if (!Int32.TryParse(dictionaryOfSettings["WorkBufferCount"], out count)) throw new Exception("Error parsing WorkBufferCount");
                if (count < 1 || count > 255) throw new Exception("Invalid WorkBufferCount specified");
                WorkBufferCount = count;
            }
            else throw new Exception("No WorkBufferCount specified");

            if (dictionaryOfSettings.ContainsKey("ManagerIP"))
            {
                if (dictionaryOfSettings["ManagerIP"].Length == 0) throw new Exception("ManagerIP not correctly specified");
                CommsManagerIp = dictionaryOfSettings["ManagerIP"];
            }
            else throw new Exception("No ManagerIP specified");

            if (dictionaryOfSettings.ContainsKey("ManagerPort"))
            {
                Int32 port;
                if (!Int32.TryParse(dictionaryOfSettings["ManagerPort"], out port)) throw new Exception("Error parsing ManagerPort");
                if (port < 1000 || port > 36000) throw new Exception("Invalid ManagerPort specified, must be within 1000-36000");
                CommsManagerPort = port;
            }
            else throw new Exception("No ManagerPort specified");

            if (dictionaryOfSettings.ContainsKey("CompletedWorkDirectory")) CompletedWorkDirectory = dictionaryOfSettings["CompletedWorkDirectory"];
            else throw new Exception("No CompletedWorkDirectory specified");

            if (dictionaryOfSettings.ContainsKey("PendingdWorkDirectory")) PendingWorkDirectory = dictionaryOfSettings["PendingdWorkDirectory"];
            else throw new Exception("No Pending Wor kDirectory specified");

            return true;
        }
    }
}