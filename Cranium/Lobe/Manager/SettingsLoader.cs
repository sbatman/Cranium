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

namespace Cranium.Lobe.Manager
{
    internal static class SettingsLoader
    {
        public static String CommsClientLocalIp = "";
        public static Int32 CommsClientPort;
        public static String CommsWorkerLocalIp = "";
        public static Int32 CommsWorkerPort;
        public static TimeSpan WorkLostAfterTime = new TimeSpan(0, 60, 0);

        /// <summary>
        ///     Loads in the settings file for the lobe Manager, this will build a dictionary of variables to be used.
        /// </summary>
        /// <param name="fileName"></param>
        public static Boolean LoadSettings(String fileName)
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

            if (dictionaryOfSettings.ContainsKey("ClientIP"))
            {
                if (dictionaryOfSettings["ClientIP"].Length == 0) throw new Exception("ClientIP not correctly specified");
                CommsClientLocalIp = dictionaryOfSettings["ClientIP"];
            }

            if (dictionaryOfSettings.ContainsKey("JobLossTimeMin"))
            {
                Int32 time = 0;
                if (!Int32.TryParse(dictionaryOfSettings["JobLossTimeMin"], out time)) throw new Exception("Error parsing JobLossTimeMin");
                if (time < 1 || time > 1000) throw new Exception("Invalid JobLossTimeMin specified, must be within 1 - 1000");
                WorkLostAfterTime = new TimeSpan(0, time, 0);
            }

            if (dictionaryOfSettings.ContainsKey("ClientPort"))
            {
                Int32 port = 0;
                if (!Int32.TryParse(dictionaryOfSettings["ClientPort"], out port)) throw new Exception("Error parsing Client Port");
                if (port < 1000 || port > 36000) throw new Exception("Invalid Client Port specified, must be within 1000-36000");
                CommsClientPort = port;
            }

            if (dictionaryOfSettings.ContainsKey("WorkerIP"))
            {
                if (dictionaryOfSettings["WorkerIP"].Length == 0) throw new Exception("WorkerIP not correctly specified");
                CommsWorkerLocalIp = dictionaryOfSettings["WorkerIP"];
            }

            if (dictionaryOfSettings.ContainsKey("WorkerPort"))
            {
                Int32 port = 0;
                if (!Int32.TryParse(dictionaryOfSettings["WorkerPort"], out port)) throw new Exception("Error parsing WorkerPort");
                if (port < 1000 || port > 36000) throw new Exception("Invalid WorkerPort specified, must be within 1000-36000");
                CommsWorkerPort = port;
            }

            return true;
        }
    }
}