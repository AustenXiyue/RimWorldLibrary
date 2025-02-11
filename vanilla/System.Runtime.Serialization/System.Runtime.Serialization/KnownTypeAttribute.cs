namespace System.Runtime.Serialization;

/// <summary>Specifies types that should be recognized by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" /> when serializing or deserializing a given type. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
public sealed class KnownTypeAttribute : Attribute
{
	private string methodName;

	private Type type;

	/// <summary>Gets the name of a method that will return a list of types that should be recognized during serialization or deserialization. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name of the method on the type defined by the <see cref="T:System.Runtime.Serialization.KnownTypeAttribute" /> class. </returns>
	public string MethodName => methodName;

	/// <summary>Gets the type that should be recognized during serialization or deserialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />. </summary>
	/// <returns>The <see cref="T:System.Type" /> that is used during serialization or deserialization. </returns>
	public Type Type => type;

	private KnownTypeAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.KnownTypeAttribute" /> class with the specified type. </summary>
	/// <param name="type">The <see cref="T:System.Type" /> that is included as a known type when serializing or deserializing data.</param>
	public KnownTypeAttribute(Type type)
	{
		this.type = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.KnownTypeAttribute" /> class with the name of a method that returns an <see cref="T:System.Collections.IEnumerable" /> of known types. </summary>
	/// <param name="methodName">The name of the method that returns an <see cref="T:System.Collections.IEnumerable" /> of types used when serializing or deserializing data.</param>
	public KnownTypeAttribute(string methodName)
	{
		this.methodName = methodName;
	}
}
