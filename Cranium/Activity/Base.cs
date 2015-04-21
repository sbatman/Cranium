using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Sbatman.Serialize;

namespace Cranium.Lib.Activity
{
    [Serializable]
    public abstract class Base : ISerializable ,IDisposable
    {
        private const UInt16 PACKETIDENTIFIER=585;
        protected Guid _ActivityInstanceIdentifier;

        public Base() { }

        public Base(SerializationInfo info, StreamingContext context)
        {
            _ActivityInstanceIdentifier = (Guid) info.GetValue("_ActivityInstanceIdentifier", typeof (Guid));
        }

        public Base(Packet p)
        {
            if (p.Type != PACKETIDENTIFIER) throw new Exception("Inforrect packet identifer");
            _ActivityInstanceIdentifier = new Guid((Byte[])p.GetObjects()[0]);
            p.Dispose();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ActivityInstanceIdentifier", _ActivityInstanceIdentifier, typeof (Guid));
        }

        public virtual Packet ToPacket()
        {
            Packet p = new Packet(PACKETIDENTIFIER);
            p.Add(_ActivityInstanceIdentifier.ToByteArray(),true);
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

        public virtual void Dispose()
        {
           
        }
    }
}