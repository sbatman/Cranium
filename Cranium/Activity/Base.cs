using System;
using System.Runtime.Serialization;

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
    }
}
