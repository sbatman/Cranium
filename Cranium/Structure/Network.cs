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
		
		public void AddLayer (Layer.Base newLayer)
		{
			if (_CurrentLayers.Contains (newLayer))
				return;
			_CurrentLayers.add (newLayer);	
		}
		
		public void RemoveLayer (Layer.Base layer)
		{
			if (!_CurrentLayers.Contains (layer))
				return;
			_CurrentLayers.Remove (layer);
		}
	}
}

