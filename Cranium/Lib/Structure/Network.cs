// //////////////////////
//  
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
// 
// //////////////////////

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace Cranium.Lib.Structure
{
    /// <summary>
    ///     A base network class, This primarily acts as a structure container of the network and used at a later stage for a
    ///     large ammount of the networks IO
    /// </summary>
    [Serializable]
    public class Network : IDisposable, ISerializable, IDeserializationCallback
    {
        protected List<Layer.Layer> _CurrentLayers;
        protected List<Layer.Layer> _DetectedBottomLayers = new List<Layer.Layer>();
        protected List<Layer.Layer> _DetectedTopLayers = new List<Layer.Layer>();
        protected Int32 _LastIssuedLayerID;
        protected Double _LearningRate;
        protected Double _Momentum;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Network" /> class.
        /// </summary>
        public Network()
        {
            _CurrentLayers = new List<Layer.Layer>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Network" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Network(SerializationInfo info, StreamingContext context)
        {
            _CurrentLayers = (List<Layer.Layer>)info.GetValue("_CurrentLayers", typeof(List<Layer.Layer>));
            _LearningRate = info.GetDouble("_LearningRate");
            _Momentum = info.GetDouble("_Momenum");
            _LastIssuedLayerID = info.GetInt32("_LastIssuedLayerID");
            _DetectedBottomLayers = (List<Layer.Layer>)info.GetValue("_DetectedBottomLayers", typeof(List<Layer.Layer>));
            _DetectedTopLayers = (List<Layer.Layer>)info.GetValue("_DetectedTopLayers", typeof(List<Layer.Layer>));
        }

        /// <summary>
        /// The learning rate of the network, This is a mutiplier against the learning change of the network to
        /// control the learning speed. The smaller the number (0.004f for example) the slower the network learns
        /// </summary>
        public Double LearningRate
        {
            [Pure]
            get { return _LearningRate; }
            set { _LearningRate = value; }
        }

        /// <summary>
        /// The percentage of the last learning pass's change to apply to the current learning pass
        /// </summary>
        public Double Momentum
        {
            [Pure]
            get { return _Momentum; }
            set { _Momentum = value; }
        }

        #region IDeserializationCallback implementation

        public void OnDeserialization(Object sender)
        {
            StructureUpdate();
        }

        #endregion

        #region IDisposable implementation

        public virtual void Dispose()
        {
            _DetectedTopLayers.Clear();
            _DetectedTopLayers = null;
            _DetectedBottomLayers.Clear();
            _DetectedBottomLayers = null;
            foreach (Layer.Layer l in _CurrentLayers) l.Dispose();
            _CurrentLayers.Clear();
            _CurrentLayers = null;
        }

        #endregion

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //Right lets serialise this thing
            info.AddValue("_CurrentLayers", _CurrentLayers, typeof(List<Layer.Layer>));
            info.AddValue("_LearningRate", _LearningRate);
            info.AddValue("_Momenum", _Momentum);
            info.AddValue("_LastIssuedLayerID", _LastIssuedLayerID);
            info.AddValue("_DetectedBottomLayers", _DetectedBottomLayers, typeof(List<Layer.Layer>));
            info.AddValue("_DetectedTopLayers", _DetectedTopLayers, typeof(List<Layer.Layer>));
        }

        #endregion

        /// <summary>
        ///     Adds a layer to the NeuralNetwork Structure causing a structure update
        /// </summary>
        /// <param name='newLayer'>
        ///     New layer.
        /// </param>
        public void AddLayer(Layer.Layer newLayer)
        {
            if (_CurrentLayers.Contains(newLayer)) return;
            _CurrentLayers.Add(newLayer);
            StructureUpdate();
        }

        /// <summary>
        ///     Removes a layer from the neuralNetwork Structure causing a structure update
        /// </summary>
        /// <param name='layer'>
        ///     Layer.
        /// </param>
        public void RemoveLayer(Layer.Layer layer)
        {
            if (!_CurrentLayers.Contains(layer)) return;
            _CurrentLayers.Remove(layer);
            StructureUpdate();
        }

        /// <summary>
        ///     Called when a change is detected to this level of the nerual networks structure
        /// </summary>
        protected virtual void StructureUpdate()
        {
            _DetectedTopLayers.Clear();
            _DetectedBottomLayers.Clear();
            foreach (Layer.Layer l in _CurrentLayers)
            {
                if (l.GetForwardConnectedLayers().Count == 0) _DetectedTopLayers.Add(l);
                if (l.GetReverseConnectedLayers().Count == 0) _DetectedBottomLayers.Add(l);

                if (l.GetID() != -1) continue;
                _LastIssuedLayerID++;
                l.SetID(_LastIssuedLayerID);
            }
        }

        /// <summary>
        ///     Populates the current layers of the neural network with connections for the nodes
        ///     This is required to build the weightings between nodes within the network.
        /// </summary>
        protected virtual void BuildNodeConnections()
        {
            foreach (Layer.Layer l in _CurrentLayers) l.PopulateNodeConnections();
        }

        /// <summary>
        ///     Returns a read only list of the layers in the network structure
        /// </summary>
        /// <returns>
        ///     The current layers.
        /// </returns>
        [Pure]
        public virtual ReadOnlyCollection<Layer.Layer> GetCurrentLayers()
        {
            return _CurrentLayers.AsReadOnly();
        }

        /// <summary>
        ///     returns a read only list of the detected top layers in the network.
        /// </summary>
        /// <returns>
        ///     The detected top layers.
        /// </returns>
        [Pure]
        public virtual ReadOnlyCollection<Layer.Layer> GetDetectedTopLayers()
        {
            return _DetectedTopLayers.AsReadOnly();
        }

        /// <summary>
        ///     Returns a read only list of the detected bottom layers in the network
        /// </summary>
        /// <returns>
        ///     The detected bottom layers.
        /// </returns>
        [Pure]
        public virtual ReadOnlyCollection<Layer.Layer> GetDetectedBottomLayers()
        {
            return _DetectedBottomLayers.AsReadOnly();
        }

        /// <summary>
        ///     Randomises the weights for all nodes within the network.
        /// </summary>
        /// <param name='varianceFromZero'>     Variance from zero. </param>
        /// <param name="positiveOnly">If true random weights will only be positive else positvite or negative around 0</param>
        public virtual void RandomiseWeights(Double varianceFromZero, Boolean positiveOnly = false)
        {
            Random rnd = new Random();

            foreach (
                Weight.Weight w in
                    from l in _CurrentLayers from n in l.GetNodes() from w in n.GetFowardWeights() select w)
            {
                Double val = positiveOnly ? rnd.NextDouble() : rnd.NextDouble() * 2 - 1;
                w.SetWeight(val * varianceFromZero);
            }
        }

        [Pure]
        public virtual Int32 GetWeightCount()
        {
            return _CurrentLayers.Sum(a => a.GetNodes().Sum(b => b.GetFowardWeights().Length));
        }

        /// <summary>
        ///     Performs a recursive foward pass across the network causing the update of all values of all nodes that have reverse
        ///     weights.
        /// </summary>
        public virtual void FowardPass()
        {
            foreach (Layer.Layer l in _DetectedBottomLayers) l.ForwardPass();
        }

        /// <summary>
        ///     Performs a recursive foward pass across the network
        /// </summary>
        public virtual void ReversePass(Boolean delayWeightUpdate = false)
        {
            foreach (Layer.Layer l in _DetectedTopLayers) l.ReversePass(LearningRate, _Momentum, delayWeightUpdate: delayWeightUpdate);
        }

        public void SaveToFile(String fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple };
            using (FileStream atextwriter = File.Create(fileName))
            {
                GZipStream compressionStream = new GZipStream(atextwriter, CompressionMode.Compress);
                formatter.Serialize(compressionStream, this);
                compressionStream.Close();
            }
        }

        public static Network LoadFromFile(String filename)
        {
            Network returnNetwork;
            using (FileStream loadedFile = File.OpenRead(filename))
            {
                GZipStream compressionStream = new GZipStream(loadedFile, CompressionMode.Decompress);
                BinaryFormatter formatter = new BinaryFormatter();
                returnNetwork = (Network)formatter.Deserialize(compressionStream);
                compressionStream.Close();
            }
            return returnNetwork;
        }
    }
}