using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
