using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Describes a service that can return a XAML namespace that is based on its prefix as it is mapped in XAML markup.</summary>
public interface IXamlNamespaceResolver
{
	/// <summary>Retrieves a XAML namespace identifier for the specified prefix string.</summary>
	/// <returns>The requested XAML namespace identifier, as a string.</returns>
	/// <param name="prefix">The prefix for which to retrieve the XAML namespace information.</param>
	string GetNamespace(string prefix);

	/// <summary>Returns all the possible prefix-to-XAML namespace mappings (<see cref="T:System.Xaml.NamespaceDeclaration" /> values) that are available in the active XAML schema context.</summary>
	/// <returns>An enumerable set of <see cref="T:System.Xaml.NamespaceDeclaration" /> values. To get all the prefix strings, get the <see cref="P:System.Xaml.NamespaceDeclaration.Prefix" /> value from each value in the returned set. To get prefixes for specific XAML namespaces, process any return value that has the desired <see cref="P:System.Xaml.NamespaceDeclaration.Namespace" /> value.</returns>
	IEnumerable<NamespaceDeclaration> GetNamespacePrefixes();
}
