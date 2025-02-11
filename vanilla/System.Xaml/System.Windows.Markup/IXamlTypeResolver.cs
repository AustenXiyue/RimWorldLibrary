using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Represents a service that resolves from named elements in XAML markup to the appropriate CLR type.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface IXamlTypeResolver
{
	/// <summary>Resolves a named XAML type to the corresponding CLR <see cref="T:System.Type" />.</summary>
	/// <returns>The <see cref="T:System.Type" /> that <paramref name="qualifiedTypeName" /> resolves to </returns>
	/// <param name="qualifiedTypeName">The XAML type name to resolve. The type name is optionally qualified by the prefix for a XML namespace. Otherwise the current default XML namespace is assumed.</param>
	Type Resolve(string qualifiedTypeName);
}
