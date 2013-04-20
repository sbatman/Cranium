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
using System.Collections.Generic;

namespace Cranium.DataPreperation
{
	public struct Data_Preprocessed_StandardDeviationVariance
	{
		public double[,] DataSet;
		public double[] Average;
		public double[] StandardDeviation;
	}
	
	/// <summary>
	/// This class is designed as a Dataset Preprocessor. It will take in the data provided, calculate teh average and standard deviation
	/// and then return the data as a comparrison against variance of the standeviation from the average. So a value of 2 would signify
	/// that it is twice the standard deviation higher than the average.
	/// </summary>
	public static class StandardDeviationVariance
	{
		public static Data_Preprocessed_StandardDeviationVariance ProduceDataset (string fileName)
		{
			StreamReader fileStream = File.OpenText ( fileName );
			
			List<string> Data = new List<string> ( );
			while ( !fileStream.EndOfStream )	
				Data.Add ( fileStream.ReadLine ( ) );	
			int columnCount = Data [ 0 ].Split ( new char[]{(char)44} ).Length;			
			double[,] WorkingDataSet = new double[columnCount, Data.Count];
			for ( int i=0 ; i<Data.Count ; i++ )
			{
				string[] currentLine = Data [ i ].Split ( new char[]{(char)44} );
				for ( int x=0 ; x<columnCount ; x++ )
				{
					WorkingDataSet [ x, i ] = Double.Parse ( currentLine [ x ] );
				}
			}
			Data_Preprocessed_StandardDeviationVariance ReturnResult = new Data_Preprocessed_StandardDeviationVariance ( );
			ReturnResult.DataSet = WorkingDataSet;
			ProcessData ( ref ReturnResult );
			return ReturnResult;
		}

		public static double[,] ProduceDataset (double[,] inputData)
		{
			return new double[1, 1];
		}
		
		private  static void ProcessData (ref Data_Preprocessed_StandardDeviationVariance inputData)
		{
			int colCount = inputData.DataSet.GetLength ( 0 );
			int rowCount = inputData.DataSet.GetLength ( 1 );
			inputData.Average = new double[colCount];
			inputData.StandardDeviation = new double[colCount];
			
			for ( int x=0 ; x<colCount ; x++ )
			{
				
				//Calculate the Average
				double avg = 0;
				for ( int y=0 ; y<rowCount ; y++ )				
					avg += inputData.DataSet [ x, y ];				
				avg /= rowCount;
				
				//Calculate the StandardDeviation
				Double stdv = 0;
				for ( int y=0 ; y<rowCount ; y++ )				
					stdv += Math.Pow ( inputData.DataSet [ x, y ] - avg, 2 );
				stdv = Math.Sqrt ( stdv / rowCount );
				
				//Processing The Data
				for ( int y=0 ; y<rowCount ; y++ )	
					inputData.DataSet [ x, y ] = ( inputData.DataSet [ x, y ] - avg ) / stdv;
				inputData.Average [ x ] = avg;
				inputData.StandardDeviation [ x ] = stdv;
			}		
			
		}
	}
}

