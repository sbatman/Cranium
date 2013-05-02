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

namespace Cranium.Activity.Training
{
	public class SlidingWindow : Base
	{
		protected Random _RND;
		protected int _WindowWidth;
		protected int _SequenceCount;
		protected int _DistanceToForcastHorrison;
		protected int _PortionOfDatasetReserved;
		protected double[,,] InputSequences;
		protected double[,] ExpectedOutputs;
		protected double _LearningRate;
		protected double _Momentum;
		protected double _LastPassAverageError;
		protected List<Cranium.Structure.Node.Base> _InputNodes;
		protected List<Cranium.Structure.Node.Base> _OutputNodes;
		protected List<Cranium.Structure.Layer.Base> _Recurrentlayers;
		protected StreamWriter _LogStream;
		
		/// <summary>
		/// Sets the width of the sliding window for data fed to the network before it is trained.
		/// </summary>
		/// <param name='windowWidth'>
		/// Window width.
		/// </param>
		public virtual void SetWindowWidth ( int windowWidth )
		{
			_WindowWidth = windowWidth;	
		}
		
		/// <summary>
		/// Sets the number of intervals ahead to predict
		/// </summary>
		/// <param name='distance'>
		/// Distance.
		/// </param>
		public virtual void SetDistanceToForcastHorrison ( int distance )
		{
			_DistanceToForcastHorrison = distance;	
		}
		
		/// <summary>
		/// Sets the length of the dataset that is reserved from training.
		/// </summary>
		/// <param name='reservedPortion'>
		/// Reserved portion.
		/// </param>
		public virtual void SetDatasetReservedLength ( int reservedPortion )
		{
			_PortionOfDatasetReserved = reservedPortion;
		}
		
		/// <summary>
		/// Sets the input nodes.
		/// </summary>
		/// <param name='nodes'>
		/// Nodes.
		/// </param>
		public virtual void SetInputNodes ( List<Structure.Node.Base> nodes )
		{
			_InputNodes = nodes;
		}
		
		/// <summary>
		/// Sets the output nodes.
		/// </summary>
		/// <param name='nodes'>
		/// Nodes.
		/// </param>
		public virtual void SetOutputNodes ( List<Structure.Node.Base> nodes )
		{
			_OutputNodes = nodes;	
		}
		
		/// <summary>
		/// Sets the recurrent conext layers.
		/// </summary>
		/// <param name='layers'>
		/// Layers.
		/// </param>
		public virtual void SetRecurrentConextLayers ( List<Structure.Layer.Base> layers )
		{
			_Recurrentlayers = layers;	
		}
		
		/// <summary>
		/// Sets the learning rate.
		/// </summary>
		/// <param name='rate'>
		/// Rate.
		/// </param>
		public virtual void SetLearningRate ( double rate )
		{
			_LearningRate = rate;	
		}
		
		/// <summary>
		/// Sets the momentum.
		/// </summary>
		/// <param name='momentum'>
		/// Momentum.
		/// </param>
		public virtual void SetMomentum ( double momentum )
		{
			_Momentum = momentum;	
		}
				
		/// <summary>
		/// Prepares the data before training.
		/// </summary>
		public virtual void PrepareData ( )
		{
			_SequenceCount = ( ( _WorkingDataset[0].GetLength (0 ) - _PortionOfDatasetReserved ) - _WindowWidth ) - _DistanceToForcastHorrison;
			int inputCount = _WorkingDataset.GetLength ( 0 );
			int outputCount = 1;
			InputSequences = new double[_SequenceCount, _WindowWidth, inputCount];
			ExpectedOutputs = new double[_SequenceCount, outputCount];
			for (int i=0; i<_SequenceCount; i++)
			{
				for (int j=0; j<_WindowWidth; j++)
				{
					for (int k=0; k<inputCount; k++)
					{
						InputSequences [i, j, k] = _WorkingDataset [k][ i + j];						
					}
					for (int l=0; l<outputCount; l++)
					{
						ExpectedOutputs [i, l] = _WorkingDataset [l][ i + j + _DistanceToForcastHorrison];
					}
				}				
			}
		}
		
		#region implemented abstract members of Cranium.Activity.Training.Base
		protected override bool _Tick ( )
		{
			if ( _CurrentEpoch >= _MaxEpochs )
			{
				return false;
			} // reached the max epoch so we are done for now
			double error = 0;
			
			List<int> sequencyList = new List<int> ( _SequenceCount );
			
			for (int s=0; s<_SequenceCount; s++)
			{
				sequencyList.Add ( s );
			}
			
			while (sequencyList.Count>0)
			{
				//This needs to be booled so it can be turned off
				int s = sequencyList [_RND.Next ( 0, sequencyList.Count )];
				sequencyList.Remove ( s );
				
				foreach ( Structure.Layer.Base layer in _TargetNetwork.GetCurrentLayers() )
					foreach ( Structure.Node.Base node in layer.GetNodes() )
						node.SetValue ( 0 );
				
				for (int i=0; i<_WindowWidth; i++)
				{
					for (int x=0; x<_InputNodes.Count; x++)
					{
						_InputNodes [x].SetValue ( InputSequences [s, i, x] );
					}					
					_TargetNetwork.FowardPass ();
					foreach ( Structure.Layer.Recurrent_Context layer in _Recurrentlayers )
						layer.UpdateExtra ();
				}
				for (int x=0; x<_OutputNodes.Count; x++)
				{
					( _OutputNodes [x] as Structure.Node.Output ).SetTargetValue ( ExpectedOutputs [s, x] );
				}				
				_TargetNetwork.ReversePass ( _LearningRate, _Momentum );
				
	
				//Calculate the current error				
				double passError = 0;
				for (int x=0; x<_OutputNodes.Count; x++)
				{
					passError += ( _OutputNodes [x] as Structure.Node.Output ).GetError ();
				}				
				passError /= _OutputNodes.Count;
				error += passError * passError;
			}
			_LastPassAverageError = error / _SequenceCount;
			Console.WriteLine ( _LastPassAverageError );
			_LogStream.WriteLine ( _LastPassAverageError );
			_LogStream.Flush ();
			return true;
		}
	
		protected override void Starting ( )
		{
			PrepareData ();		
			_LastPassAverageError = 0;
			_LogStream = File.CreateText ( "log.txt" );
			_RND = new Random ();
		}

		protected override void Stopping ( )
		{
			_LogStream.Close ();
		}
		#endregion
	}
}

