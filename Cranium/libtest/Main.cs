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

#region Usings

using System;
using Cranium.Lib.Test.Tests.Linear;
using Cranium.Lib.Test.Tests.Recursive;
using Cranium.Lib.Test.Tests.Reservoir;
using Cranium.LibTest.Tests.Recursive;

#endregion

namespace Cranium.Lib.Test
{
    internal class MainClass
    {
        /// <summary>
        ///     The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        ///     The command-line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Which test would you like to run");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("1 -  XOR2Test");
            Console.WriteLine("2 -  XOR3Test");
            Console.WriteLine("3 -  RNNTest");
            Console.WriteLine("4 -  RNNTest2");
            Console.WriteLine("5 -  MG Recurrent Test");
            Console.WriteLine("6 -  MG Echo State Test");
            Console.WriteLine("7 -  MG Echo State Test on Lobe");

            ConsoleKey pressedKey = Console.ReadKey().Key;
            Console.Clear();
            switch (pressedKey)
            {
                case ConsoleKey.D1:
                    XOR2Test.Run();
                    break;
                case ConsoleKey.D2:
                    XOR3Test.Run();
                    break;
                case ConsoleKey.D3:
                    RNNTest.Run();
                    break;
                case ConsoleKey.D4:
                    RNNTest2.Run();
                    break;
                case ConsoleKey.D5:
                    MG_Recurrent_Test.Run();
                    break;
                case ConsoleKey.D6:
                    MG_EchoState_Test.Run();
                    break;
                case ConsoleKey.D7:
                    MG_EchoState_Test_Lobe.Run();
                    break;
            }
        }
    }
}