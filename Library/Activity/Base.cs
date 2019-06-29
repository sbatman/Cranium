// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

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

		public String ActivityNotes
		{
			get => _ActivityNotes;
			set => _ActivityNotes = value;
		}

		public Guid ActivityInstanceIdentifier
		{
			get => _ActivityInstanceIdentifier;
			set => _ActivityInstanceIdentifier = value;
		}

		protected Base()
		{
		}

		protected Base(SerializationInfo info, StreamingContext context)
		{
			ActivityInstanceIdentifier = (Guid) info.GetValue("_ActivityInstanceIdentifier", typeof(Guid));
			_ActivityNotes = info.GetString("_ActivityNotes");
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