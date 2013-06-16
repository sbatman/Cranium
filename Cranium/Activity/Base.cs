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
        }

        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

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
