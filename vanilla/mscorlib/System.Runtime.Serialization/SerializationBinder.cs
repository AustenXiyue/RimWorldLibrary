using System.Runtime.InteropServices;

namespace System.Runtime.Serialization;

/// <summary>Allows users to control class loading and mandate what class to load.</summary>
[Serializable]
[ComVisible(true)]
public abstract class SerializationBinder
{
	/// <summary>When overridden in a derived class, controls the binding of a serialized object to a type.</summary>
	/// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
	/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object. </param>
	/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object. </param>
	public virtual void BindToName(Type serializedType, out string assemblyName, out string typeName)
	{
		assemblyName = null;
		typeName = null;
	}

	/// <summary>When overridden in a derived class, controls the binding of a serialized object to a type.</summary>
	/// <returns>The type of the object the formatter creates a new instance of.</returns>
	/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object. </param>
	/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object. </param>
	public abstract Type BindToType(string assemblyName, string typeName);

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SerializationBinder" /> class.</summary>
	protected SerializationBinder()
	{
	}
}
