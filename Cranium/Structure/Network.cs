// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;

namespace Structure
{
	public class Network
	{
		protected List<Layer.Base> _CurrentLayers;
		
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
			_CurrentLayers.add (newLayer);	
			StructureUpdate();
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
			StructureUpdate();
		}
		
		/// <summary>
		/// Called when a change is detected to this level of the nerual netowrks structure
		/// </summary>
		protected virtual void StructureUpdate()
		{
			
		}
	}
}

