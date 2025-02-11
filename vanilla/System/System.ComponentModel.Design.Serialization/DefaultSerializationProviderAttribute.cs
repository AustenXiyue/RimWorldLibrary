namespace System.ComponentModel.Design.Serialization;

/// <summary>The <see cref="T:System.ComponentModel.Design.Serialization.DefaultSerializationProviderAttribute" /> attribute is placed on a serializer to indicate the class to use as a default provider of that type of serializer. </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DefaultSerializationProviderAttribute : Attribute
{
	private string _providerTypeName;

	/// <summary>Gets the type name of the serialization provider.</summary>
	/// <returns>A string containing the name of the provider.</returns>
	public string ProviderTypeName => _providerTypeName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.DefaultSerializationProviderAttribute" /> class with the given provider type.</summary>
	/// <param name="providerType">The <see cref="T:System.Type" /> of the serialization provider.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="providerType" /> is null.</exception>
	public DefaultSerializationProviderAttribute(Type providerType)
	{
		if (providerType == null)
		{
			throw new ArgumentNullException("providerType");
		}
		_providerTypeName = providerType.AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.DefaultSerializationProviderAttribute" /> class with the named provider type.</summary>
	/// <param name="providerTypeName">The name of the serialization provider type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="providerTypeName" /> is null.</exception>
	public DefaultSerializationProviderAttribute(string providerTypeName)
	{
		if (providerTypeName == null)
		{
			throw new ArgumentNullException("providerTypeName");
		}
		_providerTypeName = providerTypeName;
	}
}
