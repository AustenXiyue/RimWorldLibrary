using System.Collections;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides an interface that can invoke serialization and deserialization.</summary>
public interface IDesignerSerializationService
{
	/// <summary>Deserializes the specified serialization data object and returns a collection of objects represented by that data.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> of objects rebuilt from the specified serialization data object.</returns>
	/// <param name="serializationData">An object consisting of serialized data. </param>
	ICollection Deserialize(object serializationData);

	/// <summary>Serializes the specified collection of objects and stores them in a serialization data object.</summary>
	/// <returns>An object that contains the serialized state of the specified collection of objects.</returns>
	/// <param name="objects">A collection of objects to serialize. </param>
	object Serialize(ICollection objects);
}
