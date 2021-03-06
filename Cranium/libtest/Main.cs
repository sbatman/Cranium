// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Globalization;
using System.Threading;
using Cranium.Lib.Test.Tests.Linear;
using Cranium.Lib.Test.Tests.Recursive;
using Cranium.Lib.Test.Tests.Reinforcement.Pong;
using Cranium.Lib.Test.Tests.Reservoir;
using Cranium.Lib.Test.Tests.SOM;

#endregion

namespace Cranium.Lib.Test
{
	internal class MainClass
	{
		/// <summary>
		///    The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		///    The command-line arguments.
		/// </param>
		public static void Main(String[] args)
		{
			//Fix at the request of Github:jjaskulowski
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine("Which test would you like to run");
			Console.WriteLine("-------------------------------------------------");
			Console.WriteLine("1 -  XOR2Test");
			Console.WriteLine("2 -  XOR3Test");
			Console.WriteLine("3 -  RNNTest");
			Console.WriteLine("4 -  RNNTest2");
			Console.WriteLine("5 -  MG Recurrent Test");
			Console.WriteLine("6 -  MG Echo State Test");
			Console.WriteLine("7 -  MG Echo State Test on Lobe");
			Console.WriteLine("8 -  Echo State Sin Generator Test");
			Console.WriteLine("9 -  SOM Digit recognition Test");
			Console.WriteLine("0 -  Reinforcement Learning - Pong");
			Console.WriteLine("A -  Reinforcement Learning - Pong - Genetics");

			ConsoleKey pressedKey = Console.ReadKey().Key;
			Console.Clear();
			switch (pressedKey)
			{
				case ConsoleKey.D1:
					Xor2Test.Run();
					break;
				case ConsoleKey.D2:
					Xor3Test.Run();
					break;
				case ConsoleKey.D3:
					RnnTest.Run();
					break;
				case ConsoleKey.D4:
					RnnTest2.Run();
					break;
				case ConsoleKey.D5:
					MgRecurrentTest.Run();
					break;
				case ConsoleKey.D6:
					MgEchoStateTest.Run();
					break;
				case ConsoleKey.D7:
					MgEchoStateTestLobe.Run();
					break;
				case ConsoleKey.D8:
					EchoStateTestSinGenerator.Run();
					break;
				case ConsoleKey.D9:
					SOMCharRecTets.Run();
					break;
				case ConsoleKey.D0:
					Tests.Reinforcement.Pong.Test.Run();
					break;
				case ConsoleKey.A:
					TestGenetics.Run();
					break;
			}
		}
	}
}