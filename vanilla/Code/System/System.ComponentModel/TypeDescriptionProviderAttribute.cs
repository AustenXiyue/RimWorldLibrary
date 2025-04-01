namespace System.ComponentModel;

/// <summary>Specifies the custom type description provider for a class. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class TypeDescriptionProviderAttribute : Attribute
{
	private string _typeName;

	/// <summary>Gets the type name for the type description provider.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the qualified type name for the <see cref="T:System.ComponentModel.TypeDescriptionProvider" />.</returns>
	public string TypeName => _typeName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeDescriptionProviderAttribute" /> class using the specified type name.</summary>
	/// <param name="typeName">The qualified name of the type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null.</exception>
	public TypeDescriptionProviderAttribute(string typeName)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		_typeName = typeName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeDescriptionProviderAttribute" /> class using the specified type.</summary>
	/// <param name="type">The type to store in the attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public TypeDescriptionProviderAttribute(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_typeName = type.AssemblyQualifiedName;
	}
}
