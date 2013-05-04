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
	/// <summary>
	/// This is an implementation of the echo reservoir found in EchoState netowrks. It provides a form of recursive memory as each node within the layer
	/// is randomly connected to a number of other nodes. When presented with data over a number of iterations this causes a RNN style memory behaviour.
	/// However due to the chaotic nature of the revervoirs contrcution, accuracy and learning limits of this type of network can vary heavily.
	/// Further information can be sourced here http://www.scholarpedia.org/article/Echo_state_network
	/// </summary>
	public class Echo_Reservoir : Base
	{
		protected static Random rnd = new Random ();
		protected int _NodeCount;
		protected double _LevelOfConnectivity;
		protected int _MinimumConnections;
		protected int _MaximumConnections;
		protected ActivationFunction.Base _ActivationFunction;

		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.Layer.Echo_Reservoir"/> class.
		/// </summary>
		/// <param name='nodeCount'>
		/// The number of nodes in the Reservoir
		/// </param>
		/// <param name='levelOfConnectivity'>
		/// The connectivity is calculated as chances = max-min, for each chance the levelOfConnectivity is compared to a random double, if levelOfConnectivity is higher 
		/// then an additional connection is made ontop of the origional Min
		/// </param>
		/// <param name='minimumConnections'>
		/// Minimum connections.
		/// </param>
		/// <param name='maximumConnections'>
		/// Maximum connections.
		/// </param>
		/// <param name='activationFunction'>
		/// Activation function.
		/// </param>
		public Echo_Reservoir ( int nodeCount, double levelOfConnectivity, int minimumConnections, int maximumConnections, ActivationFunction.Base activationFunction )
		{
			_NodeCount = nodeCount;
			_LevelOfConnectivity = levelOfConnectivity;
			_MinimumConnections = minimumConnections;
			_MaximumConnections = maximumConnections;
			_ActivationFunction = activationFunction;
		}
		
		/// <summary>
		/// Populates the node connections aka builds the weights.
		/// </summary>
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
		
		/// <summary>
		/// Creates all the nodes and inter node connections in this layer
		/// </summary>
		public virtual void BuildNodeBank ( )
		{	
			for (int x=0; x<_NodeCount; x++)
			{
				_Nodes.Add ( new Node.Base ( this, _ActivationFunction ) );
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

		/// <summary>
		/// Reverses the pass, and prevents the call to recurse downwards, as the internode connections will not have their weights changed.
		/// </summary>
		/// <param name='learningRate'>
		/// Learning rate.
		/// </param>
		/// <param name='momentum'>
		/// Momentum.
		/// </param>
		/// <param name='recurseDownward'>
		/// Recurse downward.
		/// </param>
		public override void ReversePass ( double learningRate, double momentum, bool recurseDownward = true )
		{
			base.ReversePass ( learningRate, momentum, false );
		}
	}
}

