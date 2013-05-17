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

namespace Cranium.Structure.Layer
{
	/// <summary>
	/// This type of layer offers a form of recursive memory in the form of gradually lower momentum recursive stores.
	/// This is described in detail in Ulbrich, C., 1994. Multi-Recurrent Netowrks for Traffic Forecasting. Vienna: Austrian Research Institute for Artificial Intelligence.
	/// The white paper can be found here http://www.aaai.org/Papers/AAAI/1994/AAAI94-135.pdf
	/// </summary>
	[Serializable]
	public class Recurrent_Context : Base
	{
		/// <summary>
		/// The source ndoes used when building the recurrent context
		/// </summary>
		protected List<Node.Base> _SourceNodes;
		/// <summary>
		/// How many context nodes should be created per source node
		/// </summary>
		protected int _LevelOfContext = 1;
		/// <summary>
		/// The Activation function that should be used for all nodes within the layer
		/// </summary>
		protected ActivationFunction.Base _ActivationFunction;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Cranium.Structure.Layer.Recurrent_Context"/> class.
		/// </summary>
		/// <param name='levelOfContext'>
		/// Level of context.
		/// </param>
		/// <param name='activationFunction'>
		/// Activation function to be used for the context nodes.
		/// </param>
		public Recurrent_Context ( int levelOfContext, ActivationFunction.Base activationFunction )
		{
			_ActivationFunction = activationFunction;
			_SourceNodes = new List<Node.Base> ();
			_LevelOfContext = levelOfContext;
		}
		
		/// <summary>
		/// Populates the node connections aka the weights between nodes.
		/// </summary>
		public override void PopulateNodeConnections ( )
		{
			BuildNodeBank ();
			base.PopulateNodeConnections ();
		}
		
		/// <summary>
		/// Builds the node bank that creates recursion, The number of banks (and thus recursive steps) is set during the contructor of the layer.
		/// </summary>
		public virtual void BuildNodeBank ( )
		{
			double step = 1d / _LevelOfContext;
			for (int x=0; x<_LevelOfContext; x++)
			{
				foreach ( Node.Base n in _SourceNodes )
				{
					_Nodes.Add ( new Node.Recurrent_Context ( n, step * x, this, _ActivationFunction ) );
				}
			}
		}

		/// <summary>
		/// Adds the source nodes from which the node banks will be built and connected.
		/// </summary>
		/// <param name='nodes'>
		/// Nodes.
		/// </param>
		public virtual void AddSourceNodes ( List<Node.Base> nodes )
		{
			_SourceNodes.AddRange ( nodes );
		}
		
		/// <summary>
		/// Performs any extra update required on child nodes
		/// </summary>
		public override void UpdateExtra ( )
		{
			foreach ( Node.Recurrent_Context n in _Nodes )
				n.Update ();	
		}
		
		public override void GetObjectData ( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			base.GetObjectData ( info, context );
			info.AddValue ( "_SourceNodes", _SourceNodes, _SourceNodes.GetType () );
			info.AddValue ( "_LevelOfContext", _LevelOfContext );
			info.AddValue ( "_ActivationFunction", _ActivationFunction, _ActivationFunction.GetType () );
		}
	}
}

