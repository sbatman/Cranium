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

namespace Cranium.Data.PostProcessing
{
	/// <summary>
	/// This class contains functions for testing a dataset and a neural networks learnt knowledge of the dataset against a random walk of the dataset.
	/// The random walk offers a benchmark by providing a compareable error. The random walk error is calculated by determining the error between the
	/// expected value and that of the value N steps before. So a step of 3 would assume that the ransom walk would predict the value of the signal
	/// in 3 steps is the same as the value now.
	/// </summary>
	public  static class RandomWalkComparison
	{
		/// <summary>
		/// Calculates the error of the actual values compared to the expected values against he error of the ranom walk against thw expected values.
		/// This in most cases is a viable method of benchmarking error levels of a network.
		/// </summary>
		/// <returns>
		/// The error against random walk.
		/// </returns>
		/// <param name='expectedValues'>
		/// Expected values.
		/// </param>
		/// <param name='actualValues'>
		/// Actual values.
		/// </param>
		/// <param name='distanceOffsetOfRandomWalk'>
		/// Distance offset of random walk.
		/// </param>
		public static double CalculateErrorAgainstRandomWalk ( double[] expectedValues, double[] actualValues, int distanceOffsetOfRandomWalk )
		{
			double[] randomWalkValues = new double[expectedValues.Length];
			for (int x=distanceOffsetOfRandomWalk; x< expectedValues.Length; x++)
			{
				randomWalkValues [x] = expectedValues [x - distanceOffsetOfRandomWalk];
			}
			
			double[] randomWalkErrors = new double[expectedValues.Length - distanceOffsetOfRandomWalk];
			double[] actualErrors = new double[expectedValues.Length - distanceOffsetOfRandomWalk];
			
			double totalRandomWalkError = 0;
			double totalActualError = 0;
			
			for (int x=distanceOffsetOfRandomWalk; x< expectedValues.Length; x++)
			{
				randomWalkErrors [x - distanceOffsetOfRandomWalk] = Math.Pow ( randomWalkValues [x] - expectedValues [x], 2 );
				totalRandomWalkError += randomWalkErrors [x - distanceOffsetOfRandomWalk];
				actualErrors [x - distanceOffsetOfRandomWalk] = Math.Pow ( actualValues [x] - expectedValues [x], 2 );
				totalActualError += actualErrors [x - distanceOffsetOfRandomWalk];
			}
			
			double avgRandomWalkError = totalRandomWalkError / ( expectedValues.Length - distanceOffsetOfRandomWalk );
			double avgActualError = totalActualError / ( expectedValues.Length - distanceOffsetOfRandomWalk );
			
			return ( avgActualError - avgRandomWalkError ) / avgRandomWalkError;
		}

		public static double[] 	 CalculateErrorAgainstRandomWalk ( double[][] expectedValues, double[][] actualValues, int distanceOffsetOfRandomWalk )
		{
			int comparisonSets = expectedValues [0].GetLength ( 0 );
			double[] results = new double[comparisonSets];
			for (int i=0; i<comparisonSets; i++)
			{
				double[] expected = new double[expectedValues.Length];
				double[] actual = new double[expectedValues.Length];
				for (int x=0; x<expectedValues.Length; x++)
				{
					expected [x] = expectedValues [x] [i];
					actual [x] = actualValues [x] [i];
				}
				results [i] = CalculateErrorAgainstRandomWalk ( expected, actual, distanceOffsetOfRandomWalk );
					
			}
			return results;
		}
	}
}

