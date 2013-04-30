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

namespace Cranium.Activity.Testing
{
	public class SlidingWindow
	{
		public struct TestResults
		{
			public double[,] ExpectedOutputs;
			public double[,] ActualOutputs;
			public double[,] OutputErrors;
			public double RMSE;
		}
		protected int _WindowWidth;
		protected int _SequenceCount;
		protected int _DistanceToForcastHorrison;
		protected int _PortionOfDatasetReserved;
		protected double[,] _WorkingDataset;
		protected double[,,] InputSequences;
		protected double[,] _ExpectedOutputs;
		protected double[,] _ActualOutputs;
		protected double[,] _OutputErrors;
		protected double _LastPassAverageError;
		protected List<Cranium.Structure.Node.Base> _InputNodes;
		protected List<Cranium.Structure.Node.Base> _OutputNodes;
		protected List<Cranium.Structure.Layer.Base> _Recurrentlayers;
		
		public virtual void SetWindowWidth ( int windowWidth )
		{
			_WindowWidth = windowWidth;	
		}

		public virtual void SetDistanceToForcastHorrison ( int distance )
		{
			_DistanceToForcastHorrison = distance;	
		}

		public virtual void SetDatasetReservedLength ( int reservedPortion )
		{
			_PortionOfDatasetReserved = reservedPortion;
		}
		
		public virtual void SetWorkingDataset ( double[,] dataset )
		{
			_WorkingDataset = dataset;	
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
		
		public virtual void SetRecurrentConextLayers ( List<Structure.Layer.Base> layers )
		{
			_Recurrentlayers = layers;	
		}
		
		/// <summary>
		/// Prepares the data before training.
		/// </summary>
		public virtual void PrepareData ( )
		{
			_SequenceCount = ( ( _WorkingDataset.GetLength ( 1 ) - _PortionOfDatasetReserved ) - _WindowWidth ) - _DistanceToForcastHorrison;
			int inputCount = _WorkingDataset.GetLength ( 0 );
			int outputCount = 1;
			InputSequences = new double[_SequenceCount, _WindowWidth, inputCount];
			_ExpectedOutputs = new double[_SequenceCount, outputCount];
			_ActualOutputs = new double[_SequenceCount, outputCount];
			_OutputErrors = new double[_SequenceCount, outputCount];
			for (int i=0; i<_SequenceCount; i++)
			{
				for (int j=0; j<_WindowWidth; j++)
				{
					for (int k=0; k<inputCount; k++)
					{
						InputSequences [i, j, k] = _WorkingDataset [k, i + j];						
					}
					for (int l=0; l<outputCount; l++)
					{
						_ExpectedOutputs [i, l] = _WorkingDataset [l, i + j + _DistanceToForcastHorrison];
					}
				}				
			}
		}
		
		public virtual TestResults TestNetwork ( Structure.Network network )
		{
			PrepareData ();
			//Ensure that the networks state is clean
			foreach ( Structure.Layer.Base layer in network.GetCurrentLayers() )
				foreach ( Structure.Node.Base node in layer.GetNodes() )
					node.SetValue ( 0 );
			
			int errorCount = 0;
			double RMSE = 0;
			for (int s=0; s<_SequenceCount; s++)
			{
				for (int i=0; i<_WindowWidth; i++)
				{
					for (int x=0; x<_InputNodes.Count; x++)
					{
						_InputNodes [x].SetValue ( InputSequences [s, i, x] );
					}					
					network.FowardPass ();
					foreach ( Structure.Layer.Recurrent_Context layer in _Recurrentlayers )
						layer.UpdateExtra ();
				}
				for (int x=0; x<_OutputNodes.Count; x++)
				{
					_ActualOutputs [s, x] = _OutputNodes [x].GetValue ();
					_OutputErrors [s, x] = _ExpectedOutputs [s, x] - _ActualOutputs [s, x];
					errorCount++;
					RMSE += _OutputErrors [s, x] * _OutputErrors [s, x];
				}				
			}
			//All the sequewnces have been run through and the outputs and their erros collected
			TestResults result = new TestResults ();
			result.ExpectedOutputs = _ExpectedOutputs;
			result.ActualOutputs = _ActualOutputs;
			result.OutputErrors = _OutputErrors;
			result.RMSE = RMSE / ( double )errorCount;
			return result;
		}
	}
}

