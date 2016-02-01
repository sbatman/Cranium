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
using Sbatman.Serialize;

namespace Cranium.Lib.Activity
{
    [Serializable]
    public abstract class Base : ISerializable, IDisposable
    {
        private const UInt16 PACKETIDENTIFIER = 585;
        protected Guid _ActivityInstanceIdentifier;
        protected String _ActivityNotes;

        protected Base()
        {
        }

        protected Base(SerializationInfo info, StreamingContext context)
        {
            _ActivityInstanceIdentifier = (Guid) info.GetValue("_ActivityInstanceIdentifier", typeof (Guid));
            ActivityNotes = info.GetString("_ActivityNotes");
        }

        protected Base(Packet p)
        {
            if (p.Type != PACKETIDENTIFIER) throw new Exception("Inforrect packet identifer");
            _ActivityInstanceIdentifier = new Guid((Byte[]) p.GetObjects()[0]);
            ActivityNotes = (String)p.GetObjects()[1];
            p.Dispose();
        }

        public String ActivityNotes
        {
            get { return _ActivityNotes; }
            set { _ActivityNotes = value; }
        }

        public virtual void Dispose()
        {
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ActivityInstanceIdentifier", _ActivityInstanceIdentifier, typeof(Guid));
            info.AddValue("_ActivityNotes", ActivityNotes, typeof(String));
        }

        public virtual Packet ToPacket()
        {
            Packet p = new Packet(PACKETIDENTIFIER);
            p.Add(_ActivityInstanceIdentifier.ToByteArray(), true);
            return p;
        }

        public Guid GetGuid()
        {
            return _ActivityInstanceIdentifier;
        }

        public virtual void SetGuid(Guid newGuid)
        {
            _ActivityInstanceIdentifier = newGuid;
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