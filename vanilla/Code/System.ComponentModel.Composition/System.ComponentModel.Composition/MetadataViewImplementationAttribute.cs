namespace System.ComponentModel.Composition;

/// <summary>Specifies the type used to implement a metadata view.</summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class MetadataViewImplementationAttribute : Attribute
{
	/// <summary>Gets the type of the metadata view.</summary>
	/// <returns>The type of the metadata view.</returns>
	public Type ImplementationType { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.MetadataViewImplementationAttribute" /> class.</summary>
	/// <param name="implementationType">The type of the metadata view.</param>
	public MetadataViewImplementationAttribute(Type implementationType)
	{
		ImplementationType = implementationType;
	}
}
