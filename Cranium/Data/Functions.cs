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
using System.IO;

namespace Cranium.Data
{
	public static class UsefulFunctions
	{
		public static void PrintArrayToFile ( double[] data, string fileName )
		{
			StreamWriter stream = File.CreateText ( fileName );
			for (int x=0; x<data.Length; x++)
			{
				stream.WriteLine ( data [x] );
			}
			stream.Flush ();
			stream.Close ();
		}

		public static void PrintArrayToFile ( double[,] data, string fileName )
		{
			StreamWriter stream = File.CreateText ( fileName );
			for (int x=0; x<data.GetLength(0); x++)
			{
				for (int y=0; y<data.GetLength(1); y++)
				{
					if ( y + 1 < data.GetLength ( 1 ) )
					{
						stream.Write ( data [x, y] + "," );
					}
					else
					{
						stream.Write ( data [x, y] );
					}
				}
				stream.WriteLine("");
			}
			stream.Flush ();
			stream.Close ();
		}
	}
}

