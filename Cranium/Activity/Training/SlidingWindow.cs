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

namespace Cranium.Activity.Training
{
	public class SlidingWindow : Base
	{
		protected int _WindowWidth;
		protected int _DistanceToForcastHorrison;
		protected int _PortionOfDatasetReserved;
		protected double[,,] InputSequences;
		protected double[,] ExpectedOutputs;
		
		/// <summary>
		/// Sets the width of the sliding window for data fed to the network before it is trained.
		/// </summary>
		/// <param name='windowWidth'>
		/// Window width.
		/// </param>
		public virtual void SetWindowWidth (int windowWidth)
		{
			_WindowWidth = windowWidth;	
		}
		
		/// <summary>
		/// Sets the number of intervals ahead to predict
		/// </summary>
		/// <param name='distance'>
		/// Distance.
		/// </param>
		public virtual void SetDistanceToForcastHorrison (int distance)
		{
			_DistanceToForcastHorrison = distance;	
		}
		
		public virtual void SetDatasetReservedLength (int reservedPortion)
		{
			_PortionOfDatasetReserved = reservedPortion;
		}
		
		public virtual void PrepareData ()
		{
			int sequencesToProduce = ( ( _WorkingDataset.GetLength ( 0 ) - _PortionOfDatasetReserved ) - _WindowWidth ) - _DistanceToForcastHorrison;
			int inputCount = _WorkingDataset.GetLength ( 1 );
			int outputCount = 1;
			InputSequences = new double[sequencesToProduce, _WindowWidth, inputCount];
			ExpectedOutputs = new double[sequencesToProduce,outputCount];
			for ( int i=0 ; i<sequencesToProduce ; i++ )
			{
				for ( int j=0 ; j<_WindowWidth ; j++ )
				{
					for ( int k=0 ; k<inputCount ; k++ )
					{
						InputSequences [ i, j, k ] = _WorkingDataset [ i + j, k ];						
					}
					for ( int l=0 ; l<outputCount ; l++ )
					{
							ExpectedOutputs[i,l]= _WorkingDataset[i+j+_DistanceToForcastHorrison,l];
					}
				}				
			}
		}
		
		#region implemented abstract members of Cranium.Activity.Training.Base
		protected override bool _Tick ()
		{
			return true;
		}
	
		protected override void Starting ()
		{
		}

		protected override void Stopping ()
		{
		}
		#endregion
	}
}

