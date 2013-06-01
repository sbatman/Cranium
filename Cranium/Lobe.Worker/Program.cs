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

using System.Collections.Generic;

namespace Cranium.Lobe.Worker
{
    internal class Program
    {
        /// <summary>
        /// A list containing all the active worker services
        /// </summary>
        private static readonly List<WorkerThread> ActiveWorkerServices = new List<WorkerThread>();
        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            //Attempt to load the settings
            if (!SettingsLoader.LoadSettings("Settings.ini")) return;

            //Prepare the workers
            for (int i = 0; i < SettingsLoader.WorkerThreadCount; i++)
            {
                ActiveWorkerServices.Add(new WorkerThread());
            }
        }
    }
}