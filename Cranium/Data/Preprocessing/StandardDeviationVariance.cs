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
using System.Collections.Generic;
using System.IO;

#endregion

namespace Cranium.Data.Preprocessing
{
    public struct Data_Preprocessed_StandardDeviationVariance
    {
        public double[] Average;
        public double[][] DataSet;
        public double[] Scale;
        public double[] StandardDeviation;
    }

    /// <summary>
    ///     This class is designed as a Dataset Preprocessor. It will take in the data provided, calculate the average and standard deviation
    ///     and then return the data as a comparison against variance of the standeviation from the average. So a value of 2 would signify
    ///     that it is twice the standard deviation higher than the average.
    /// </summary>
    public static class StandardDeviationVariance
    {
        /// <summary>
        ///     Loads the provided file and pulls the data into a table, this is then pre-processed and returned along with the average and the standard deviation.
        ///     The file must be in CSV format with each line representing one set of inputs (seperated by commas)
        /// </summary>
        /// <returns>
        ///     The dataset.
        /// </returns>
        /// <param name='fileName'>
        ///     File name.
        /// </param>
        public static Data_Preprocessed_StandardDeviationVariance ProduceDataset(string fileName)
        {
            if (fileName.Length == 0 || !File.Exists(fileName))
            {
                throw (new Exception("Bad filename provided"));
            }

            //try
            //{
            StreamReader fileStream = File.OpenText(fileName);

            List<string> data = new List<string>();
            while (!fileStream.EndOfStream)
            {
                data.Add(fileStream.ReadLine());
            }
            int columnCount = data[0].Split(new[] {(char) 44}).Length;
            double[][] workingDataSet = new double[columnCount][];
            for (int i = 0; i < columnCount; i++) workingDataSet[i] = new double[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                string[] currentLine = data[i].Split(new[] {(char) 44});
                for (int x = 0; x < columnCount; x++)
                {
                    workingDataSet[x][i] = Double.Parse(currentLine[x]);
                }
            }
            fileStream.Close();
            Data_Preprocessed_StandardDeviationVariance returnResult = new Data_Preprocessed_StandardDeviationVariance
                {
                    DataSet = workingDataSet
                };
            ProcessData(ref returnResult);
            return returnResult;
            //	}
            //	catch ( Exception e )
            //	{
            //	throw( new Exception ( "Data pre-processing failed :" + e.Message ) );				
            //	}
        }

        /// <summary>
        ///     Processes the provided dataset returning the preprocessed dataset, the average and standard deviation.
        /// </summary>
        /// <returns>
        ///     The dataset.
        /// </returns>
        /// <param name='inputData'>
        ///     Input data.
        /// </param>
        public static Data_Preprocessed_StandardDeviationVariance ProduceDataset(double[][] inputData)
        {
            try
            {
                Data_Preprocessed_StandardDeviationVariance returnResult =
                    new Data_Preprocessed_StandardDeviationVariance {DataSet = inputData};
                ProcessData(ref returnResult);
                return returnResult;
            }
            catch (Exception e)
            {
                throw (new Exception("Data pre-processing failed :" + e.Message));
            }
        }

        /// <summary>
        ///     Does the actual pre-processing.
        /// </summary>
        /// <param name='inputData'>
        ///     Input data.
        /// </param>
        private static void ProcessData(ref Data_Preprocessed_StandardDeviationVariance inputData)
        {
            int colCount = inputData.DataSet.GetLength(0);
            int rowCount = inputData.DataSet[0].GetLength(0);
            inputData.Average = new double[colCount];
            inputData.StandardDeviation = new double[colCount];
            inputData.Scale = new double[colCount];

            for (int x = 0; x < colCount; x++)
            {
                //Calculate the Average
                double avg = 0;
                for (int y = 0; y < rowCount; y++)
                {
                    avg += inputData.DataSet[x][y];
                }
                avg /= rowCount;

                //Calculate the StandardDeviation
                Double stdv = 0;
                for (int y = 0; y < rowCount; y++)
                {
                    stdv += Math.Pow(inputData.DataSet[x][y] - avg, 2);
                }
                stdv = Math.Sqrt(stdv/rowCount);

                double min = 0;
                double max = 0;
                //Processing The Data
                for (int y = 0; y < rowCount; y++)
                {
                    inputData.DataSet[x][y] = (inputData.DataSet[x][y] - avg)/stdv;
                    if (inputData.DataSet[x][y] < min)
                    {
                        min = inputData.DataSet[x][y];
                    }
                    if (inputData.DataSet[x][y] > max)
                    {
                        max = inputData.DataSet[x][y];
                    }
                }
                inputData.Average[x] = avg;
                inputData.StandardDeviation[x] = stdv;

                //Need to bring the data within the -1 to 1 range for maximising the effectiveness of most non-linear activation functions present at this time
                double scale = max;
                if (0 - min > scale)
                {
                    scale = -min;
                }
                if (1 > scale)
                {
                    scale = 1;
                }
                if (Math.Abs(scale - 1) > 0.00001)
                {
                    for (int y = 0; y < rowCount; y++)
                    {
                        inputData.DataSet[x][y] /= scale;
                    }
                    inputData.Scale[x] = scale;
                }
            }
        }
    }
}