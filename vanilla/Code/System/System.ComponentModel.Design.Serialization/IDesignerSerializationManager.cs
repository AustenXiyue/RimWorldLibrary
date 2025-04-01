using System.Collections;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides an interface that can manage design-time serialization.</summary>
public interface IDesignerSerializationManager : IServiceProvider
{
	/// <summary>Gets a stack-based, user-defined storage area that is useful for communication between serializers.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.Design.Serialization.ContextStack" /> that stores data.</returns>
	ContextStack Context { get; }

	/// <summary>Indicates custom properties that can be serializable with available serializers.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> containing the properties to be serialized.</returns>
	PropertyDescriptorCollection Properties { get; }

	/// <summary>Occurs when <see cref="M:System.ComponentModel.Design.Serialization.IDesignerSerializationManager.GetName(System.Object)" /> cannot locate the specified name in the serialization manager's name table.</summary>
	event ResolveNameEventHandler ResolveName;

	/// <summary>Occurs when serialization is complete.</summary>
	event EventHandler SerializationComplete;

	/// <summary>Adds the specified serialization provider to the serialization manager.</summary>
	/// <param name="provider">The serialization provider to add. </param>
	void AddSerializationProvider(IDesignerSerializationProvider provider);

	/// <summary>Creates an instance of the specified type and adds it to a collection of named instances.</summary>
	/// <returns>The newly created object instance.</returns>
	/// <param name="type">The data type to create. </param>
	/// <param name="arguments">The arguments to pass to the constructor for this type. </param>
	/// <param name="name">The name of the object. This name can be used to access the object later through <see cref="M:System.ComponentModel.Design.Serialization.IDesignerSerializationManager.GetInstance(System.String)" />. If null is passed, the object is still created but cannot be accessed by name. </param>
	/// <param name="addToContainer">If true, this object is added to the design container. The object must implement <see cref="T:System.ComponentModel.IComponent" /> for this to have any effect. </param>
	object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer);

	/// <summary>Gets an instance of a created object of the specified name, or null if that object does not exist.</summary>
	/// <returns>An instance of the object with the given name, or null if no object by that name can be found.</returns>
	/// <param name="name">The name of the object to retrieve. </param>
	object GetInstance(string name);

	/// <summary>Gets the name of the specified object, or null if the object has no name.</summary>
	/// <returns>The name of the object, or null if the object is unnamed.</returns>
	/// <param name="value">The object to retrieve the name for. </param>
	string GetName(object value);

	/// <summary>Gets a serializer of the requested type for the specified object type.</summary>
	/// <returns>An instance of the requested serializer, or null if no appropriate serializer can be located.</returns>
	/// <param name="objectType">The type of the object to get the serializer for. </param>
	/// <param name="serializerType">The type of the serializer to retrieve. </param>
	object GetSerializer(Type objectType, Type serializerType);

	/// <summary>Gets a type of the specified name.</summary>
	/// <returns>An instance of the type, or null if the type cannot be loaded.</returns>
	/// <param name="typeName">The fully qualified name of the type to load. </param>
	Type GetType(string typeName);

	/// <summary>Removes a custom serialization provider from the serialization manager.</summary>
	/// <param name="provider">The provider to remove. This object must have been added using <see cref="M:System.ComponentModel.Design.Serialization.IDesignerSerializationManager.AddSerializationProvider(System.ComponentModel.Design.Serialization.IDesignerSerializationProvider)" />. </param>
	void RemoveSerializationProvider(IDesignerSerializationProvider provider);

	/// <summary>Reports an error in serialization.</summary>
	/// <param name="errorInformation">The error to report. This information object can be of any object type. If it is an exception, the message of the exception is extracted and reported to the user. If it is any other type, <see cref="M:System.Object.ToString" /> is called to display the information to the user. </param>
	void ReportError(object errorInformation);

	/// <summary>Sets the name of the specified existing object.</summary>
	/// <param name="instance">The object instance to name. </param>
	/// <param name="name">The name to give the instance. </param>
	void SetName(object instance, string name);
}
