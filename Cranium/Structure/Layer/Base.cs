// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using System.Collections.Generic;

namespace Cranium.Structure.Layer
{
	public class Base : IDisposable
	{
		protected List<Node.Base> _Nodes;
		protected List<Layer.Base> _ForwardConnectedLayers;
		protected List<Layer.Base> _ReverseConnectedLayers;
		
		/// <summary>
		/// Sets the nodes that are present in this layer, the previous list of nodes is purged.
		/// </summary>
		/// <param name='nodes'>
		/// Nodes.
		/// </param>
		public virtual void SetNodes (List<Node.Base> nodes)
		{
			_Nodes.Clear ();
			_Nodes = null;
			_Nodes = nodes;	
		}
		
		/// <summary>
		/// Returns the current list of nodes that are present within the layer.
		/// </summary>
		/// <returns>
		/// The nodes.
		/// </returns>
		public virtual List<Node.Base> GetNodes ()
		{
			return _Nodes;	
		}

		/// <summary>
		/// Adds a layer to the list of layers that are connected forward
		/// </summary>
		/// <param name='layer'>
		/// Layer.
		/// </param>
		public virtual void ConnectFowardLayer (Layer.Base layer)
		{
			_ForwardConnectedLayers.Add (layer);
			layer._ReverseConnectedLayers.Add (this);
		}
		
		/// <summary>
		/// Returns the list of layers that are connected forward
		/// </summary>
		/// <returns>
		/// The forward connected layers.
		/// </returns>
		public virtual List<Layer.Base> GetForwardConnectedLayers ()
		{
			return _ForwardConnectedLayers;	
		}
		
		/// <summary>
		/// Adds a layer to the list of layers that are connected reverse.
		/// </summary>
		/// <param name='layer'>
		/// Layer.
		/// </param>
		public virtual void ConnectReverseLayer (Layer.Base layer)
		{
			_ReverseConnectedLayers.Add (layer);
			layer._ForwardConnectedLayers.Add (this);
		}
		
		/// <summary>
		/// Returns the list of layers that are connected reverse
		/// </summary>
		/// <returns>
		/// The forward connected layers.
		/// </returns>
		public virtual List<Layer.Base> GetReverseConnectedLayers ()
		{
			return _ReverseConnectedLayers;
		}
		
		//uses the current forward and reverse layers to populate the node connections
		public virtual void PopulateNodeConnections ()
		{
			PurgeNodeConnections ();
			foreach (Layer.Base l in _ForwardConnectedLayers) {
				foreach (Node.Base n in _Nodes) {
					foreach (Node.Base fn in l.GetNodes()) {
						n.ConnectToNode (fn, Weight.Base.ConnectionDirection.Forward, 0);
					}
				}
			}
		}
		
		/// <summary>
		/// Removes all connections on this layers nodes (useful if deleting or modifiying the layer
		/// </summary>
		public virtual void PurgeNodeConnections ()
		{
			foreach (Node.Base n in _Nodes)
				n.DestroyAllConnections ();
		}
		
		public virtual void ForwardPass ()
		{
			foreach (Node.Base n in _Nodes) {
				n.CalculateValue ();	
			}	
			foreach (Layer.Base l in _ForwardConnectedLayers) {
				l.ForwardPass ();	
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			PurgeNodeConnections ();
			_ReverseConnectedLayers.Clear ();
			_ReverseConnectedLayers = null;
			_ForwardConnectedLayers.Clear ();
			_ForwardConnectedLayers = null;
			foreach (Node.Base n in _Nodes)
				n.Dispose ();
			_Nodes.Clear ();
			_Nodes = null;
		}
		#endregion
	}
}

