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

		public void Start ()
		{
			_LoopThread = new Thread ( _UpdateLoop );
			_LoopThread.Start ( );
		}
		
		public virtual void SetTargetNeteork (Structure.Network targetNetwork)
		{
			_TargetNetwork = targetNetwork;	
		}

		public bool IsRunning ()
		{
			return _Running;
		}

		public void Stop ()
		{
			_Stopping = true;
		}

		protected abstract bool _Tick ();

		private void _UpdateLoop ()
		{
			_Running = true;
			while ( _Tick()&&!_Stopping )
				;
			_Running = false;
		}
	}
}

