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
using System.Collections.ObjectModel;

namespace Cranium.Structure.Layer
{
	/// <summary></summary>
	/// This is the base Layer structure of the Neural Network, It is used to house collections of nodes and provide the linking structure of these nodes with other groups of nodes.
	/// The layer class also provides some of the basic functionality for back and foward propogation. This class can be overriden to add additional functionality to a layer.
	/// </summary>
	public class Base : IDisposable
	{
		protected List<Node.Base> _Nodes = new List<Node.Base> ();
		protected List<Layer.Base> _ForwardConnectedLayers = new List<Layer.Base> ();
		protected List<Layer.Base> _ReverseConnectedLayers = new List<Layer.Base> ();
		protected int _LayerID;
		
		/// <summary>
		/// Sets the nodes that are present in this layer, the previous list of nodes is purged.
		/// </summary>
		/// <param name='nodes'>
		/// Nodes.
		/// </param>
		public virtual void SetNodes ( List<Node.Base> nodes )
		{
			_Nodes.Clear ();
			_Nodes = null;
			_Nodes = nodes;	
			for (int x=0; x<_Nodes.Count; x++)
			{
				_Nodes [x].SetPositionInParentLayer ( x );	
			}
		}
		
		/// <summary>
		/// Returns the current list of nodes that are present within the layer.
		/// </summary>
		/// <returns>
		/// The nodes.
		/// </returns>
		public virtual ReadOnlyCollection<Node.Base> GetNodes ( )
		{
			return _Nodes.AsReadOnly ();	
		}
		
		/// <summary>
		/// Returns the number of nodes present in the layer.
		/// </summary>
		/// <returns>
		/// The node count.
		/// </returns>
		public virtual int GetNodeCount ( )
		{
			return _Nodes.Count;	
		}

		/// <summary>
		/// Adds a layer to the list of layers that are connected forward
		/// </summary>
		/// <param name='layer'>
		/// Layer.
		/// </param>
		public virtual void ConnectFowardLayer ( Layer.Base layer )
		{
			_ForwardConnectedLayers.Add ( layer );
			layer._ReverseConnectedLayers.Add ( this );
		}
		
		/// <summary>
		/// Returns the list of layers that are connected forward
		/// </summary>
		/// <returns>
		/// The forward connected layers.
		/// </returns>
		public virtual List<Layer.Base> GetForwardConnectedLayers ( )
		{
			return _ForwardConnectedLayers;	
		}
		
		/// <summary>
		/// Adds a layer to the list of layers that are connected reverse.
		/// </summary>
		/// <param name='layer'>
		/// Layer.
		/// </param>
		public virtual void ConnectReverseLayer ( Layer.Base layer )
		{
			_ReverseConnectedLayers.Add ( layer );
			layer._ForwardConnectedLayers.Add ( this );
		}
		
		/// <summary>
		/// Returns the list of layers that are connected reverse
		/// </summary>
		/// <returns>
		/// The forward connected layers.
		/// </returns>
		public virtual List<Layer.Base> GetReverseConnectedLayers ( )
		{
			return _ReverseConnectedLayers;
		}
		
		//uses the current forward and reverse layers to populate the node connections
		public virtual void PopulateNodeConnections ( )
		{
			PurgeNodeConnections ();
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
		}
		
		/// <summary>
		/// Removes all connections on this layers nodes (useful if deleting or modifiying the layer
		/// </summary>
		public virtual void PurgeNodeConnections ( )
		{
			foreach ( Node.Base n in _Nodes )
				n.DestroyAllConnections ();
		}
		
		/// <summary>
		/// Triggers a calculate value call on all nodes withint he layer and then recursively calls this function on all foward connected layers.
		/// </summary>
		public virtual void ForwardPass ( )
		{
			foreach ( Node.Base n in _Nodes ) 
				n.CalculateValue ();				
			foreach ( Layer.Base l in _ForwardConnectedLayers ) 
				l.ForwardPass ();				
		}

		public virtual void ReversePass ( double learningRate, double momentum, bool recurseDownward = true )
		{
			foreach ( Node.Base n in _Nodes ) 
				n.CalculateError ();			
			foreach ( Node.Base n in _Nodes ) 
				n.AdjustWeights ( learningRate );	
			foreach ( Node.Base n in _Nodes ) 
				n.UpdateWeights ( momentum );		
			if ( recurseDownward )
			{
				foreach ( Layer.Base l in _ReverseConnectedLayers ) 
					l.ReversePass ( learningRate, momentum );
			}		
			
		}
		
		public int GetID ( )
		{
			return _LayerID;	
		}
		
		public void SetID ( int id )
		{
			_LayerID = id;
		}
		
		public virtual void UpdateExtra ( )
		{
		}

		#region IDisposable implementation
		public void Dispose ( )
		{
			PurgeNodeConnections ();
			_ReverseConnectedLayers.Clear ();
			_ReverseConnectedLayers = null;
			_ForwardConnectedLayers.Clear ();
			_ForwardConnectedLayers = null;
			foreach ( Node.Base n in _Nodes )
				n.Dispose ();
			_Nodes.Clear ();
			_Nodes = null;
		}
		#endregion
	}
}

