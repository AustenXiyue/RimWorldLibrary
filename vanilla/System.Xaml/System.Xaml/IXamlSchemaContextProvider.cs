namespace System.Xaml;

/// <summary>Represents a service that provides XAML schema context information to type converters and markup extensions.</summary>
public interface IXamlSchemaContextProvider
{
	/// <summary>Gets the <see cref="T:System.Xaml.XamlSchemaContext" /> that is reported by the service context.</summary>
	/// <returns>The XAML schema context that is reported by the service context.</returns>
	XamlSchemaContext SchemaContext { get; }
}
