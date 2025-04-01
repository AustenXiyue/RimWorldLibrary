namespace System.ComponentModel;

/// <summary>Enables attribute redirection. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class AttributeProviderAttribute : Attribute
{
	private string _typeName;

	private string _propertyName;

	/// <summary>Gets the assembly qualified type name passed into the constructor.</summary>
	/// <returns>The assembly qualified name of the type specified in the constructor.</returns>
	public string TypeName => _typeName;

	/// <summary>Gets the name of the property for which attributes will be retrieved.</summary>
	/// <returns>The name of the property for which attributes will be retrieved.</returns>
	public string PropertyName => _propertyName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AttributeProviderAttribute" /> class with the given type name.</summary>
	/// <param name="typeName">The name of the type to specify.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null.</exception>
	public AttributeProviderAttribute(string typeName)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		_typeName = typeName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AttributeProviderAttribute" /> class with the given type name and property name.</summary>
	/// <param name="typeName">The name of the type to specify.</param>
	/// <param name="propertyName">The name of the property for which attributes will be retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="propertyName" /> is null.</exception>
	public AttributeProviderAttribute(string typeName, string propertyName)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (propertyName == null)
		{
			throw new ArgumentNullException("propertyName");
		}
		_typeName = typeName;
		_propertyName = propertyName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AttributeProviderAttribute" /> class with the given type.</summary>
	/// <param name="type">The type to specify.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public AttributeProviderAttribute(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_typeName = type.AssemblyQualifiedName;
	}
}
