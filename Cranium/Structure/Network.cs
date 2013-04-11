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
			foreach(Layer.Base l in _CurrentLayers)
			{
				if(l.GetForwardConnectedLayers().Count==0)_DetectedTopLayers.Add(l);
				if(l.GetReverseConnectedLayers().Count==0)_DetectedBottomLayers.Add(l);				
			}
		}
	}
}

