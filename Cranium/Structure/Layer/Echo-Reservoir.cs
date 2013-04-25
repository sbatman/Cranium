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

namespace Cranium.Structure.Layer
{
	public class Echo_Reservoir : Base
	{
		protected static Random rnd = new Random ();
		protected int _NodeCount;
		protected double _LevelOfConnectivity;
		protected int _MinimumConnections;
		protected int _MaximumConnections;

		public Echo_Reservoir ( int nodeCount, double levelOfConnectivity, int minimumConnections, int maximumConnections )
		{
			_NodeCount = nodeCount;
			_LevelOfConnectivity = levelOfConnectivity;
			_MinimumConnections = minimumConnections;
			_MaximumConnections = maximumConnections;
		}
		
		public override void PopulateNodeConnections ( )
		{
			PurgeNodeConnections ();
			BuildNodeBank ();
			foreach ( Layer.Base l in _ForwardConnectedLayers )
			{
				foreach ( Node.Base n in _Nodes )
				{
					foreach ( Node.Base fn in l.GetNodes() )
					{
						n.ConnectToNode ( fn, Weight.Base.ConnectionDirection.Forward, 0 );
					}
				}
			}	
			foreach ( Layer.Base l in _ReverseConnectedLayers )
			{
				foreach ( Node.Base n in _Nodes )
				{
					foreach ( Node.Base fn in l.GetNodes() )
					{
						n.ConnectToNode ( fn, Weight.Base.ConnectionDirection.Reverse, 0 );
					}
				}
			}	
		}
		
		public virtual void BuildNodeBank ( )
		{	
			for (int x=0; x<_NodeCount; x++)
			{
				_Nodes.Add ( new Node.Base ( this, new ActivationFunction.Tanh () ) );
			}
			foreach ( Node.Base node in _Nodes )
			{
				int connections = _MinimumConnections;
				for (int x=0; x< _MaximumConnections-_MinimumConnections; x++)
				{
					connections += rnd.NextDouble () > _LevelOfConnectivity ? 0 : 1;
				}
				for (int i=0; i<connections; i++)
				{
					node.ConnectToNode ( _Nodes [rnd.Next ( 0, _Nodes.Count )], Weight.Base.ConnectionDirection.Forward, 0 );	
				}
			}
		}
		
		public override void ForwardPass ( )
		{
			base.ForwardPass ();
		}

		public override void ReversePass ( double learningRate, double momentum, bool recurseDownward = true )
		{
			base.ReversePass ( learningRate, momentum, false );
		}
	}
}

