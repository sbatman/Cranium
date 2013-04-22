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
		public static void Main (string[] args)
		{			
			
			Console.WriteLine ("Which test would you like to run");
			Console.WriteLine ("-------------------------------------------------");
			Console.WriteLine ("1 -  XOR2Test");
			Console.WriteLine ("2 -  XOR3Test");
			Console.WriteLine ("3 -  RNNTest");
			Console.WriteLine ("4 -  RNNTest2");
			Console.WriteLine ("5 -  MG Recurrent Test");
			
			ConsoleKey PressedKey = Console.ReadKey ().Key;
			Console.Clear ();
			if (PressedKey == ConsoleKey.D1)			
				XOR2Test.Run ();
			if (PressedKey == ConsoleKey.D2)			
				XOR3Test.Run ();
			if (PressedKey == ConsoleKey.D3)			
				RNNTest.Run ();
			if (PressedKey == ConsoleKey.D4)			
				RNNTest2.Run ();
			if (PressedKey == ConsoleKey.D5)			
				MG_Recurrent_Test.Run ();
		}
	}
}