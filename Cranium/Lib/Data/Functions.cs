// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion

namespace Cranium.Lib.Data
{
	/// <summary>
	///    A collection of usefull functions for interacting with a loaded dataset. These include thing like prointing the
	///    double array to disk etc.
	///    This section is prone to change as new useful functions are described and as these functions are categorised.
	/// </summary>
	public static class Functions
	{
		/// <summary>
		///    Prints the provided array to a csv file with the given filename
		/// </summary>
		/// <param name='data'>
		///    Data.
		/// </param>
		/// <param name='fileName'>
		///    File name.
		/// </param>
		public static void PrintArrayToFile(IEnumerable<Double> data, String fileName)
		{
			using (StreamWriter stream = File.CreateText(fileName))
				foreach (Double t in data)
					stream.WriteLine(t);
		}

		/// <summary>
		///    Prints the provided multi-dimentional array to a csv file with the given filename
		/// </summary>
		/// <param name='data'>
		///    Data.
		/// </param>
		/// <param name='fileName'>
		///    File name.
		/// </param>
		public static void PrintArrayToFile(Double[][] data, String fileName)
		{
			using (StreamWriter stream = File.CreateText(fileName))
			{
				for (Int32 x = 0; x < data.GetLength(0); x++)
				{
					for (Int32 y = 0; y < data[x].GetLength(0); y++)
					{
						if (y + 1 < data[x].GetLength(0)) stream.Write(data[x][y] + ",");
						else stream.Write(data[x][y]);
					}

					stream.WriteLine("");
				}
			}
		}

		public static String PrintArrayToString(Double[][] data)
		{
			StringBuilder theString = new StringBuilder();

			for (Int32 x = 0; x < data.GetLength(0); x++)
			{
				for (Int32 y = 0; y < data[x].GetLength(0); y++)
				{
					if (y + 1 < data[x].GetLength(0)) theString.AppendFormat("{0},", data[x][y]);
					else theString.Append(data[x][y]);
				}

				theString.AppendLine();
			}

			return theString.ToString();
		}
	}
}