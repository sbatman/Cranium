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

using System.IO;

#endregion

namespace Cranium.Data
{
    /// <summary>
    ///     A collection of usefull functions for interacting with a loaded dataset. These include thing like prointing the double array to disk etc.
    ///     This section is prone to change as new useful functions are described and as these functions are categorised.
    /// </summary>
    public static class UsefulFunctions
    {
        /// <summary>
        ///     Prints the provided array to a csv file with the given filename
        /// </summary>
        /// <param name='data'>
        ///     Data.
        /// </param>
        /// <param name='fileName'>
        ///     File name.
        /// </param>
        public static void PrintArrayToFile(double[] data, string fileName)
        {
            StreamWriter stream = File.CreateText(fileName);
            foreach (double t in data) stream.WriteLine(t);
            stream.Flush();
            stream.Close();
        }

        /// <summary>
        ///     Prints the provided multi-dimentional array to a csv file with the given filename
        /// </summary>
        /// <param name='data'>
        ///     Data.
        /// </param>
        /// <param name='fileName'>
        ///     File name.
        /// </param>
        public static void PrintArrayToFile(double[][] data, string fileName)
        {
            StreamWriter stream = File.CreateText(fileName);
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data [x].GetLength(0); y++)
                {
                    if (y + 1 < data [x].GetLength(0)) stream.Write(data [x] [y] + ",");
                    else stream.Write(data [x] [y]);
                }
                stream.WriteLine("");
            }
            stream.Flush();
            stream.Close();
        }
    }
}