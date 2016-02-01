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

namespace Cranium.Lobe.Worker
{
    internal class Program
    {
        public static void Main()
        {
            using (Worker w = new Worker())
            {
                Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
                w.HandelMessage += Console.WriteLine;
                w.Start();
            }
        }
    }
}