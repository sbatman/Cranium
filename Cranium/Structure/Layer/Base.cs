// ///////////////////////
// 
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// //////////////////////
using System;
using System.Collections.Generic;

namespace Structure.Layer
{
	public class Base
	{
		protected List<Node.Base> _Nodes;
		protected List<Layer.Base> _ForwardConnectedLayers;
		protected List<Layer.Base> _ReverseConnectedLayers;
		
		public virtual void SetNodes (List<Node.Base> nodes)
		{
			_Nodes = nodes;	
		}
		
		public virtual List<Node.Base> GetNodes()
		{
			return _Nodes;	
		}

		public virtual void ConnectFowardLayer (Layer.Base layer)
		{
			_ForwardConnectedLayers.Add (layer);
		}
		
		public virtual List<Layer.Base> GetForwardConnectedLayers ()
		{
			return _ForwardConnectedLayers;	
		}

		public virtual void ConnectReverseLayer (Layer.Base layer)
		{
			_ReverseConnectedLayers.Add (layer);
		}
		
		public virtual List<Layer.Base> GetReverseConnectedLayers ()
		{
			return _ReverseConnectedLayers;
		}
	}
}

