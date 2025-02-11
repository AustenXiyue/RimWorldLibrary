namespace System.Xaml;

/// <summary>Provides optional settings for a <see cref="T:System.Xaml.XamlSchemaContext" />.</summary>
public class XamlSchemaContextSettings
{
	/// <summary>Gets or sets a value that specifies whether a XAML schema context allows for markup extensions that have two constructors with the same arity (number of input parameters).</summary>
	/// <returns>true if the schema context allows for markup extensions that have duplicate arity; otherwise, false.</returns>
	public bool SupportMarkupExtensionsWithDuplicateArity { get; set; }

	/// <summary>Gets or sets a value that specifies whether a XAML schema and its context use fully qualified assembly names in the values that are returned by the lookup API.</summary>
	/// <returns>true if a XAML schema and its context use fully qualified assembly names in the values that are returned by the lookup APIs; otherwise, false.</returns>
	public bool FullyQualifyAssemblyNamesInClrNamespaces { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlSchemaContextSettings" /> class.</summary>
	public XamlSchemaContextSettings()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlSchemaContextSettings" /> class by copying the values of an existing <see cref="T:System.Xaml.XamlSchemaContextSettings" /> instance.</summary>
	/// <param name="settings">An existing <see cref="T:System.Xaml.XamlSchemaContextSettings" />. </param>
	public XamlSchemaContextSettings(XamlSchemaContextSettings settings)
	{
		if (settings != null)
		{
			SupportMarkupExtensionsWithDuplicateArity = settings.SupportMarkupExtensionsWithDuplicateArity;
			FullyQualifyAssemblyNamesInClrNamespaces = settings.FullyQualifyAssemblyNamesInClrNamespaces;
		}
	}
}
