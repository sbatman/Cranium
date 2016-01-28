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

#region Usings

using System;
using Cranium.Lib.Test.Tests.Linear;
using Cranium.Lib.Test.Tests.Recursive;
using Cranium.Lib.Test.Tests.Reservoir;
using Cranium.Lib.Test.Tests.SOM;
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
        public static void Main(String[] args)
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
            Console.WriteLine("8 -  Echo State Sin Generator Test");
            Console.WriteLine("9 -  SOM Didgit recognition Test");

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
            }
        }
    }
}