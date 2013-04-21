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
using System.Threading;

using System;

namespace Cranium.Activity.Training
{
	public abstract class Base
	{
		private bool _Running = false;
		private bool _Stopping = false;
		private Thread _LoopThread;
		protected Structure.Network _TargetNetwork;
		protected double[,] _WorkingDataset;
		protected int _MaxEpochs;
		protected int _CurrentEpoch;

		public void Start ()
		{
			_CurrentEpoch = 0;
			_LoopThread = new Thread ( _UpdateLoop );
			_LoopThread.Start ( );
		}
		
		public virtual void SetTargetNetwork (Structure.Network targetNetwork)
		{
			_TargetNetwork = targetNetwork;	
		}

		public virtual void SetWorkingDataset (double[,] workingDataset)
		{
			_WorkingDataset = workingDataset;	
		}

		public bool IsRunning ()
		{
			return _Running;
		}
		
		public virtual void SetMaximumEpochs (int epochs)
		{
			_MaxEpochs = epochs;
		}

		public void Stop ()
		{
			_Stopping = true;
		}

		protected abstract bool _Tick ();
		/// <summary>
		/// Called as this training instance starts
		/// </summary>
		protected abstract void Starting ();
		/// <summary>
		/// Called if this instance is stopped/finished
		/// </summary>
		protected abstract void Stopping ();

		private void _UpdateLoop ()
		{
			_Running = true;
			Starting ( );
			while ( _Tick()&&!_Stopping )
			{
				_CurrentEpoch++;
			}
			Stopping ( );
			_Running = false;
		}
	}
}

