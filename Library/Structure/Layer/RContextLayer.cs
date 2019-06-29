// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Node;

#endregion

namespace Cranium.Lib.Structure.Layer
{
	/// <summary>
	///    This type of layer offers a form of recursive memory in the form of gradually lower momentum recursive stores.
	///    This is described in detail in Ulbrich, C., 1994. Multi-Recurrent networks for Traffic Forecasting. Vienna:
	///    Austrian Research Institute for Artificial Intelligence.
	///    The white paper can be found here http://www.aaai.org/Papers/AAAI/1994/AAAI94-135.pdf
	/// </summary>
	[Serializable]
	public class RecurrentContext : Layer
	{
		/// <summary>
		///    The Activation function that should be used for all nodes within the layer
		/// </summary>
		protected AF _ActivationFunction;

		/// <summary>
		///    How many context nodes should be created per source node
		/// </summary>
		protected Int32 _LevelOfContext;

		/// <summary>
		///    The source ndoes used when building the recurrent context
		/// </summary>
		protected List<BaseNode> _SourceNodes;

		/// <summary>
		///    Initializes a new instance of the <see cref="RecurrentContext" /> class.
		/// </summary>
		/// <param name='levelOfContext'>
		///    Level of context.
		/// </param>
		/// <param name='activationFunction'>
		///    Activation function to be used for the context nodes.
		/// </param>
		public RecurrentContext(Int32 levelOfContext, AF activationFunction)
		{
			_ActivationFunction = activationFunction;
			_SourceNodes = new List<BaseNode>();
			_LevelOfContext = levelOfContext;
		}

		/// <summary>
		///    Initializes a new instance of the <see cref="RecurrentContext" /> class. Used by Serializer
		/// </summary>
		/// <param name='info'>
		///    Info.
		/// </param>
		/// <param name='context'>
		///    Context.
		/// </param>
		public RecurrentContext(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_SourceNodes = (List<BaseNode>) info.GetValue("_SourceNodes", typeof(List<BaseNode>));
			_LevelOfContext = info.GetInt32("_LevelOfContext");
			_ActivationFunction = (AF) info.GetValue("_ActivationFunction", typeof(AF));
		}

		/// <summary>
		///    Populates the node connections aka the weights between nodes.
		/// </summary>
		public override void PopulateNodeConnections()
		{
			BuildNodeBank();
			base.PopulateNodeConnections();
		}

		/// <summary>
		///    Builds the node bank that creates recursion, The number of banks (and thus recursive steps) is set during the
		///    contructor of the layer.
		/// </summary>
		public virtual void BuildNodeBank()
		{
			Double step = 1d / _LevelOfContext;
			for (Int32 x = 0; x < _LevelOfContext; x++)
				foreach (BaseNode n in _SourceNodes)
					_Nodes.Add(new RecurrentContextNode(n, step * x, this, _ActivationFunction));
		}

		/// <summary>
		///    Adds the source nodes from which the node banks will be built and connected.
		/// </summary>
		/// <param name='nodes'>
		///    Nodes.
		/// </param>
		public virtual void AddSourceNodes(List<BaseNode> nodes)
		{
			_SourceNodes.AddRange(nodes);
		}

		/// <summary>
		///    Performs any extra update required on child nodes
		/// </summary>
		public override void UpdateExtra()
		{
			foreach (RecurrentContextNode n in _Nodes.Cast<RecurrentContextNode>()) n.Update();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("_SourceNodes", _SourceNodes, _SourceNodes.GetType());
			info.AddValue("_LevelOfContext", _LevelOfContext);
			info.AddValue("_ActivationFunction", _ActivationFunction, _ActivationFunction.GetType());
		}
	}
}