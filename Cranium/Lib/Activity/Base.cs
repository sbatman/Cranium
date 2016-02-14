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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cranium.Lib.Activity
{
    [Serializable]
    public abstract class Base : ISerializable, IDisposable
    {
        protected Guid _ActivityInstanceIdentifier;
        protected String _ActivityNotes;

        protected Base()
        {
        }

        protected Base(SerializationInfo info, StreamingContext context)
        {
            ActivityInstanceIdentifier = (Guid) info.GetValue("_ActivityInstanceIdentifier", typeof (Guid));
            _ActivityNotes = info.GetString("_ActivityNotes");
        }

        public String ActivityNotes
        {
            get { return _ActivityNotes; }
            set { _ActivityNotes = value; }
        }
        public Guid ActivityInstanceIdentifier
        {
            get { return _ActivityInstanceIdentifier; }
            set { _ActivityInstanceIdentifier = value; }
        }

        public virtual void Dispose()
        {
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ActivityInstanceIdentifier", ActivityInstanceIdentifier, typeof(Guid));
            info.AddValue("_ActivityNotes", ActivityNotes, typeof(String));
        }

        public virtual void SaveToDisk(String filename)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream dataFile = File.Create(filename)) binaryFormatter.Serialize(dataFile, this);
        }

        public static Base LoadFromDisk(String filename)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream dataFile = File.Open(filename, FileMode.Open)) return (Base) binaryFormatter.Deserialize(dataFile);
        }
    }
}