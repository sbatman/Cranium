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
	[Serializable]
	public class Echo_Reservoir : Base
	{
		/// <summary>
		/// The random used for building connections
		/// </summary>
		protected static Random rnd = new Random ();
		/// <summary>
		/// The number of nodes present in the Reservoir
		/// </summary>
		protected int _NodeCount;		
		/// <summary>
		/// The connectivity is calculated as chances = max-min, for each chance the levelOfConnectivity is compared to a random double, if levelOfConnectivity is higher 
		/// then an additional connection is made ontop of the origional Min
		/// </summary>
		protected double _LevelOfConnectivity;
		/// <summary>
		/// The minimum connections per node.
		/// </summary>
		protected int _MinimumConnections;
		/// <summary>
		/// The Maximum connections per node
		/// </summary>
		protected int _MaximumConnections;
		/// <summary>
		/// The Activation to use for all the nodes created within the Reservoir
		/// </summary>
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
		/// Minimum connections per node.
		/// </param>
		/// <param name='maximumConnections'>
		/// Maximum connections per node.
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
		
		public override void GetObjectData ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			base.GetObjectData ( info, context );
			info.AddValue ( "_NodeCount", _NodeCount );
			info.AddValue ( "_LevelOfConnectivity", _LevelOfConnectivity );
			info.AddValue ( "_MinimumConnections", _MinimumConnections );
			info.AddValue ( "_MaximumConnections", _MaximumConnections );
			info.AddValue ( "_ActivationFunction", _ActivationFunction, _ActivationFunction.GetType () );
			info.AddValue("rnd",rnd,typeof(Random));
		}
	}
}

