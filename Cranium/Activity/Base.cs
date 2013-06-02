using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Cranium.Lib.Activity
{
    [Serializable]
    public abstract class Base : ISerializable
    {
        public Base()
        {
        }

        public Base(SerializationInfo info, StreamingContext context)
        {
        }

        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
