// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Lobe.Worker
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

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