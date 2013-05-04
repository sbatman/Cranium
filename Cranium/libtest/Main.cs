// // ///////////////////////
// // 
// // Cranium - A neural network framework for C#
// // https://github.com/sbatman/Cranium.git
// // 
// // This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// // More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// // If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
// //
// // //////////////////////
using System;
using System.Collections.Generic;
using Cranium.Structure;
using Cranium.Data.Preprocessing;

namespace Cranium.LibTest
{
	class MainClass
	{
		public static void Main ( string[] args )
		{			
			
			Console.WriteLine ( "Which test would you like to run" );
			Console.WriteLine ( "-------------------------------------------------" );
			Console.WriteLine ( "1 -  XOR2Test" );
			Console.WriteLine ( "2 -  XOR3Test" );
			Console.WriteLine ( "3 -  RNNTest" );
			Console.WriteLine ( "4 -  RNNTest2" );
			Console.WriteLine ( "5 -  MG Recurrent Test" );
			Console.WriteLine ( "6 -  MG Echo State Test" );
			
			ConsoleKey pressedKey = Console.ReadKey ().Key;
			Console.Clear ();
			switch ( pressedKey )
			{
				case 	ConsoleKey.D1 :
					Cranium.LibTest.Tests.Linear.XOR2Test.Run ();
					break;
				case 	ConsoleKey.D2 :
					Cranium.LibTest.Tests.Linear.XOR3Test.Run ();
					break;
				case 	ConsoleKey.D3 :
					Cranium.LibTest.Tests.Recursive.RNNTest.Run ();
					break;
				case 	ConsoleKey.D4 :
					Cranium.LibTest.Tests.Recursive.RNNTest2.Run ();
					break;
				case 	ConsoleKey.D5 :
					Cranium.LibTest.Tests.Recursive.MG_Recurrent_Test.Run ();
					break;
				case 	ConsoleKey.D6 :
					Cranium.LibTest.Tests.Reservoir.MG_EchoState_Test.Run ();
					break;
			}			
		}
	}
}