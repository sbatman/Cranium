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

namespace Cranium.Structure.Layer
{
	public class Recurrent_Context : Base
	{
		protected List<Node.Base> _SourceNodes;
		protected int _LevelOfContext = 1;
		
		public Recurrent_Context (int levelOfContext)
		{
			_SourceNodes = new List<Node.Base> ();
			_LevelOfContext = levelOfContext;
		}

		public override void PopulateNodeConnections ()
		{
			BuildNodeBank ();
			base.PopulateNodeConnections ();
		}

		public virtual void BuildNodeBank ()
		{
			double Step = 1 / _LevelOfContext;
			for (int x=0; x<_LevelOfContext; x++) {
				foreach (Node.Base n in _SourceNodes) {
					_Nodes.Add(new Node.Recurrent_Context(n,Step*x,this,new ActivationFunction.Tanh()));
				}
			}
		}

		public virtual void AddSourceNodes (List<Node.Base> nodes)
		{
			_SourceNodes.AddRange (nodes);
		}
	}
}

