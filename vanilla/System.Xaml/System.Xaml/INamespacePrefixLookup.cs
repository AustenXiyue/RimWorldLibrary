namespace System.Xaml;

/// <summary>Represents a service that can return the recommended prefix for a XAML namespace mapping to consumers. Consumers might include design environments or serializers.</summary>
public interface INamespacePrefixLookup
{
	/// <summary>Returns the recommended prefix for a specified XAML namespace identifier.</summary>
	/// <returns>The recommended prefix.</returns>
	/// <param name="ns">The XAML namespace identifier string for which to obtain a prefix.</param>
	string LookupPrefix(string ns);
}
