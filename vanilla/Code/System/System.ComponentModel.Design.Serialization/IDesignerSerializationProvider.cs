namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides an interface that enables access to a serializer.</summary>
public interface IDesignerSerializationProvider
{
	/// <summary>Gets a serializer using the specified attributes.</summary>
	/// <returns>An instance of a serializer of the type requested, or null if the request cannot be satisfied.</returns>
	/// <param name="manager">The serialization manager requesting the serializer. </param>
	/// <param name="currentSerializer">An instance of the current serializer of the specified type. This can be null if no serializer of the specified type exists. </param>
	/// <param name="objectType">The data type of the object to serialize. </param>
	/// <param name="serializerType">The data type of the serializer to create. </param>
	object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType);
}
