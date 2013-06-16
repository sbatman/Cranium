using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cranium.Lib.Activity
{
    [Serializable]
    public abstract class Base : ISerializable
    {
        protected Guid _ActivityInstanceIdentifier = new Guid();
        public Base()
        {
        }

        public Base(SerializationInfo info, StreamingContext context)
        {
            _ActivityInstanceIdentifier =(Guid)info.GetValue("_ActivityInstanceIdentifier", typeof (Guid));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ActivityInstanceIdentifier", _ActivityInstanceIdentifier,typeof(Guid));
        }

        public virtual Guid GetGUID()
        {
            return _ActivityInstanceIdentifier;
        }

        public virtual void SetGUID(Guid newGuid)
        {
            _ActivityInstanceIdentifier = newGuid;
        }

        public virtual void SaveToDisk(string filename)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream dataFile = File.Create(filename))
            {
                binaryFormatter.Serialize(dataFile,this);
            }
        }

        public static Activity.Base LoadFromDisk(string filename)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream dataFile = File.Open(filename, FileMode.Open))
            {
                return (Lib.Activity.Base)binaryFormatter.Deserialize(dataFile);
            }
        }
    }
}
