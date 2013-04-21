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
		
		/// <summary>
		/// Sets the width of the sliding window for data fed to the network before it is trained.
		/// </summary>
		/// <param name='windowWidth'>
		/// Window width.
		/// </param>
		public virtual void SetWindowWidth(int windowWidth)
		{
		_WindowWidth = windowWidth;	
		}
		
		/// <summary>
		/// Sets the number of intervals ahead to predict
		/// </summary>
		/// <param name='distance'>
		/// Distance.
		/// </param>
		public virtual void SetDistanceToForcastHorrison(int distance)
		{
		_DistanceToForcastHorrison = distance;	
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

