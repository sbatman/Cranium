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
                w.HandelMessage += Console.WriteLine;
                w.Start();
            }
        }

    }
}
