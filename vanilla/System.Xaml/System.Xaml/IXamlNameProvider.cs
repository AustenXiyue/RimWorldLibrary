namespace System.Xaml;

/// <summary>Provides a service that is used during save and write operations to input an object and return a XAML name.</summary>
public interface IXamlNameProvider
{
	/// <summary>Retrieves the XAML name of the specified object.</summary>
	/// <returns>The XAML name value of the requested object.</returns>
	/// <param name="value">The object to get the name for.</param>
	string GetName(object value);
}
