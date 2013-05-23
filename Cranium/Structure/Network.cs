#region info

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

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Cranium.Structure.Layer;

#endregion

namespace Cranium.Structure
{
    /// <summary>
    ///     A base network class, This primarily acts as a structure container of the network and used at a later stage for a large ammount of the networks IO
    /// </summary>
    [Serializable]
    public class Network : IDisposable, ISerializable, IDeserializationCallback
    {
        protected List<Base> _CurrentLayers = new List<Base>();
        protected List<Base> _DetectedBottomLayers = new List<Base>();
        protected List<Base> _DetectedTopLayers = new List<Base>();
        protected int _LastIssuedLayerID = 0;
        protected double _LearningRate = 0;
        protected double _Momenum = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cranium.Structure.Network" /> class.
        /// </summary>
        public Network()
        {
            _CurrentLayers = new List<Base>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cranium.Structure.Network" /> class. Used by the Serializer
        /// </summary>
        /// <param name='info'>
        ///     Info.
        /// </param>
        /// <param name='context'>
        ///     Context.
        /// </param>
        public Network(SerializationInfo info, StreamingContext context)
        {
            _CurrentLayers = (List<Base>) info.GetValue("CurrentLayers", typeof (List<Base>));
            _LearningRate = info.GetDouble("_LearningRate");
            _Momenum = info.GetDouble("_Momenum");
            _LastIssuedLayerID = info.GetInt32("_LastIssuedLayerID");
            //	StructureUpdate ();
        }

        /// <summary>
        ///     Adds a layer to the NeuralNetwork Structure causing a structure update
        /// </summary>
        /// <param name='newLayer'>
        ///     New layer.
        /// </param>
        public void AddLayer(Base newLayer)
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
        public void RemoveLayer(Base layer)
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
            foreach (Base l in _CurrentLayers)
            {
                if (l.GetForwardConnectedLayers().Count == 0) _DetectedTopLayers.Add(l);
                if (l.GetReverseConnectedLayers().Count == 0) _DetectedBottomLayers.Add(l);
                if (l.GetID() == -1)
                {
                    _LastIssuedLayerID++;
                    l.SetID(_LastIssuedLayerID);
                }
            }
        }

        /// <summary>
        ///     Populates the current layers of the neural network with connections for the nodes
        ///     This is required to build the weightings between nodes within the network.
        /// </summary>
        protected virtual void BuildNodeConnections()
        {
            foreach (Base l in _CurrentLayers) l.PopulateNodeConnections();
        }

        /// <summary>
        ///     Returns a read only list of the layers in the network structure
        /// </summary>
        /// <returns>
        ///     The current layers.
        /// </returns>
        public virtual ReadOnlyCollection<Base> GetCurrentLayers()
        {
            return _CurrentLayers.AsReadOnly();
        }

        /// <summary>
        ///     returns a read only list of the detected top layers in the network.
        /// </summary>
        /// <returns>
        ///     The detected top layers.
        /// </returns>
        public virtual ReadOnlyCollection<Base> GetDetectedTopLayers()
        {
            return _DetectedTopLayers.AsReadOnly();
        }

        /// <summary>
        ///     Returns a read only list of the detected bottom layers in the network
        /// </summary>
        /// <returns>
        ///     The detected bottom layers.
        /// </returns>
        public virtual ReadOnlyCollection<Base> GetDetectedBottomLayers()
        {
            return _DetectedBottomLayers.AsReadOnly();
        }

        /// <summary>
        ///     Randomises the weights for all nodes within the network.
        /// </summary>
        /// <param name='varianceFromZero'>
        ///     Variance from zero.
        /// </param>
        public virtual void RandomiseWeights(Double varianceFromZero)
        {
            Random rnd = new Random();
            foreach (
                Weight.Base w in from l in _CurrentLayers from n in l.GetNodes() from w in n.GetFowardWeights() select w
                ) w.SetWeight(((rnd.NextDouble()*2) - 1)*varianceFromZero);
        }

        /// <summary>
        ///     Performs a recursive foward pass across the network causing the update of all values of all nodes that have reverse weights.
        /// </summary>
        public virtual void FowardPass()
        {
            foreach (Base l in  _DetectedBottomLayers) l.ForwardPass();
        }

        /// <summary>
        ///     Performs a recursive foward pass across the network
        /// </summary>
        public virtual void ReversePass()
        {
            foreach (Base l in _DetectedTopLayers) l.ReversePass(_LearningRate, _Momenum);
        }

        /// <summary>
        ///     Gets the current learning rate.
        /// </summary>
        /// <returns>
        ///     The learning rate.
        /// </returns>
        public virtual double GetLearningRate()
        {
            return _LearningRate;
        }

        /// <summary>
        ///     Gets the current momentum.
        /// </summary>
        /// <returns>
        ///     The momentum.
        /// </returns>
        public virtual double GetMomentum()
        {
            return _Momenum;
        }

        /// <summary>
        ///     Sets the current learning rate.
        /// </summary>
        /// <param name='newLearningRate'>
        ///     New learning rate.
        /// </param>
        public virtual void SetLearningRate(double newLearningRate)
        {
            _LearningRate = newLearningRate;
        }

        /// <summary>
        ///     Sets the current momentum.
        /// </summary>
        /// <param name='newMomentum'>
        ///     New momentum.
        /// </param>
        public virtual void SetMomentum(double newMomentum)
        {
            _Momenum = newMomentum;
        }

        public void SaveToFile(string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};
            FileStream atextwriter = File.Create(fileName);
            GZipStream compressionStream = new GZipStream(atextwriter, CompressionMode.Compress);
            formatter.Serialize(compressionStream, this);

            compressionStream.Close();
            atextwriter.Close();
        }

        public static Network LoadFromFile(string filename)
        {
            FileStream loadedFile = File.OpenRead(filename);
            GZipStream compressionStream = new GZipStream(loadedFile, CompressionMode.Decompress);
            BinaryFormatter formatter = new BinaryFormatter();
            Network returnNetwork = (Network) formatter.Deserialize(compressionStream);
            compressionStream.Close();
            loadedFile.Close();
            return returnNetwork;
        }

        #region IDisposable implementation

        public virtual void Dispose()
        {
            _DetectedTopLayers.Clear();
            _DetectedTopLayers = null;
            _DetectedBottomLayers.Clear();
            _DetectedBottomLayers = null;
            foreach (Base l in _CurrentLayers) l.Dispose();
            _CurrentLayers.Clear();
            _CurrentLayers = null;
        }

        #endregion

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //Right lets serialise this thing
            info.AddValue("CurrentLayers", _CurrentLayers, typeof (List<Base>));
            info.AddValue("_LearningRate", _LearningRate);
            info.AddValue("_Momenum", _Momenum);
            info.AddValue("_LastIssuedLayerID", _LastIssuedLayerID);
        }

        #endregion

        #region IDeserializationCallback implementation

        public void OnDeserialization(object sender)
        {
            StructureUpdate();
        }

        #endregion
    }
}