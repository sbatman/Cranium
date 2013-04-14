// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Cranium.Structure
{
	public class Network
	{
		protected List<Layer.Base> _CurrentLayers;
		protected List<Layer.Base> _DetectedTopLayers;
		protected List<Layer.Base> _DetectedBottomLayers;
		
		public Network ()
		{			
			_CurrentLayers = new List<Layer.Base> ();
		}
		
		/// <summary>
		/// Adds a layer to the NeuralNetwork Structure causing a structure update 
		/// </summary>
		/// <param name='newLayer'>
		/// New layer.
		/// </param>
		public void AddLayer (Layer.Base newLayer)
		{
			if (_CurrentLayers.Contains (newLayer))
				return;
			_CurrentLayers.Add (newLayer);	
			StructureUpdate ();
		}
		
		/// <summary>
		/// Removes a layer from the neuralNetwork Structure causing a structure update 
		/// </summary>
		/// <param name='layer'>
		/// Layer.
		/// </param>
		public void RemoveLayer (Layer.Base layer)
		{
			if (!_CurrentLayers.Contains (layer))
				return;
			_CurrentLayers.Remove (layer);
			StructureUpdate ();
		}
		
		/// <summary>
		/// Called when a change is detected to this level of the nerual netowrks structure
		/// </summary>
		protected virtual void StructureUpdate ()
		{
			foreach (Layer.Base l in _CurrentLayers) {
				if (l.GetForwardConnectedLayers ().Count == 0)
					_DetectedTopLayers.Add (l);
				if (l.GetReverseConnectedLayers ().Count == 0)
					_DetectedBottomLayers.Add (l);
			}
		}
		
		/// <summary>
		/// Populates the current layers of the neural network with connections for the nodes
		/// This is required to build the weightings between nodes within the network.
		/// </summary>
		protected virtual void BuildNodeConnections ()
		{
			foreach (Layer.Base l in _CurrentLayers)		
				l.PopulateNodeConnections ();			
		}
		
		/// <summary>
		/// Returns a read only list of the layers in the network structure
		/// </summary>
		/// <returns>
		/// The current layers.
		/// </returns>
		public virtual ReadOnlyCollection<Layer.Base> GetCurrentLayers ()
		{
			return _CurrentLayers.AsReadOnly ();
		}
		
		/// <summary>
		/// returns a read only list of the detected top layers in the network.
		/// </summary>
		/// <returns>
		/// The detected top layers.
		/// </returns>
		public virtual ReadOnlyCollection<Layer.Base> GetDetectedTopLayers ()
		{
			return _DetectedTopLayers.AsReadOnly ();
		}
		
		/// <summary>
		/// Returns a read only list of the detected bottom layers in the network
		/// </summary>
		/// <returns>
		/// The detected bottom layers.
		/// </returns>
		public virtual ReadOnlyCollection<Layer.Base> GetDetectedBottomLayers ()
		{
			return _DetectedBottomLayers.AsReadOnly ();
		}
		
		public virtual void RandomiseWeights (double varianceFromZero)
		{
			Random rnd = new Random ();
			foreach (Layer.Base l in _CurrentLayers)
			{
				foreach (Node.Base n in l.GetNodes())
				{
					foreach (Weight.Base w in n.GetFowardWeights())
					{
						w.SetWeight (((rnd.NextDouble () * 2) - 1) * varianceFromZero);
					}
				}
			}
		}
	}
}

