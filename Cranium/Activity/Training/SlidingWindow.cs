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

namespace Cranium.Activity.Training
{
	public class SlidingWindow : Base
	{
		protected int _WindowWidth;
		protected int _SequenceCount;
		protected int _DistanceToForcastHorrison;
		protected int _PortionOfDatasetReserved;
		protected double[,,] InputSequences;
		protected double[,] ExpectedOutputs;
		protected double _LearningRate;
		protected double _Momentum;
		protected List<Cranium.Structure.Node.Base> _InputNodes;
		protected List<Cranium.Structure.Node.Base> _OutputNodes;
		protected List<Cranium.Structure.Layer.Recurrent_Context> _Recurrentlayers;
		
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
		
		public virtual void SetInputNodes (List<Structure.Node.Base> nodes)
		{
			_InputNodes = nodes;
		}
		
		public virtual void SetOutputNodes (List<Structure.Node.Base> nodes)
		{
			_OutputNodes = nodes;	
		}
		
		public virtual void SetRecurrentConextLayers (List<Structure.Layer.Recurrent_Context> layers)
		{
			_Recurrentlayers = layers;	
		}
		
		public virtual void SetLearningRate (double rate)
		{
			_LearningRate = rate;	
		}
		
		public virtual void SetMomentum (double momentum)
		{
			_Momentum = momentum;	
		}
				
		public virtual void PrepareData ()
		{
			_SequenceCount = ( ( _WorkingDataset.GetLength ( 0 ) - _PortionOfDatasetReserved ) - _WindowWidth ) - _DistanceToForcastHorrison;
			int inputCount = _WorkingDataset.GetLength ( 1 );
			int outputCount = 1;
			InputSequences = new double[_SequenceCount, _WindowWidth, inputCount];
			ExpectedOutputs = new double[_SequenceCount, outputCount];
			for ( int i=0 ; i<_SequenceCount ; i++ )
			{
				for ( int j=0 ; j<_WindowWidth ; j++ )
				{
					for ( int k=0 ; k<inputCount ; k++ )
					{
						InputSequences [ i, j, k ] = _WorkingDataset [ i + j, k ];						
					}
					for ( int l=0 ; l<outputCount ; l++ )
					{
						ExpectedOutputs [ i, l ] = _WorkingDataset [ i + j + _DistanceToForcastHorrison, l ];
					}
				}				
			}
		}
		
		#region implemented abstract members of Cranium.Activity.Training.Base
		protected override bool _Tick ()
		{
			if ( _CurrentEpoch >= _MaxEpochs )
				return false; // reached the max epoch so we are done for now
			
			for ( int s=0 ; s<_SequenceCount ; s++ )
			{
				for ( int i=0 ; i<_WindowWidth ; i++ )
				{
					for ( int x=0 ; x<_InputNodes.Count ; x++ )
					{
						_InputNodes [ x ].SetValue ( InputSequences [ s, i, x ] );
					}	
					_TargetNetwork.FowardPass ( );
					foreach (Structure.Layer.Recurrent_Context layer in _Recurrentlayers)
						layer.Update ( );
				}
				_TargetNetwork.ReversePass ( _LearningRate, _Momentum );
			}
			
			return true;
		}
	
		protected override void Starting ()
		{
			PrepareData ( );			
		}

		protected override void Stopping ()
		{
		}
		#endregion
	}
}

