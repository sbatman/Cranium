﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cranium.Lib.Activity
{
    [Serializable]
    public abstract class Base : ISerializable
    {
        protected Guid _ActivityInstanceIdentifier;

        public Base() { }

        public Base(SerializationInfo info, StreamingContext context)
        {
            _ActivityInstanceIdentifier = (Guid) info.GetValue("_ActivityInstanceIdentifier", typeof (Guid));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ActivityInstanceIdentifier", _ActivityInstanceIdentifier, typeof (Guid));
        }

        public Guid GetGUID()
        {
            return _ActivityInstanceIdentifier;
        }

        public virtual void SetGUID(Guid newGuid)
        {
            _ActivityInstanceIdentifier = newGuid;
        }

        public virtual void SaveToDisk(string filename)
        {
            var binaryFormatter = new BinaryFormatter();
            using (FileStream dataFile = File.Create(filename)) binaryFormatter.Serialize(dataFile, this);
        }

        public static Base LoadFromDisk(string filename)
        {
            var binaryFormatter = new BinaryFormatter();
            using (FileStream dataFile = File.Open(filename, FileMode.Open)) return (Base) binaryFormatter.Deserialize(dataFile);
        }
    }
}