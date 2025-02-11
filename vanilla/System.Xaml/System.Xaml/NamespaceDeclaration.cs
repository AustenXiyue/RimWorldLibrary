using System.Diagnostics;

namespace System.Xaml;

/// <summary>Declares the identifier and the prefix of a XAML namespace by storing these string values as separate properties.</summary>
[DebuggerDisplay("Prefix={Prefix} Namespace={Namespace}")]
public class NamespaceDeclaration
{
	private string prefix;

	private string ns;

	/// <summary>Gets the prefix that is used as the XAML namespace mapping prefix for a <see cref="T:System.Xaml.NamespaceDeclaration" />.</summary>
	/// <returns>The prefix string for this <see cref="T:System.Xaml.NamespaceDeclaration" />.</returns>
	public string Prefix => prefix;

	/// <summary>Gets the identifier component of a <see cref="T:System.Xaml.NamespaceDeclaration" />.</summary>
	/// <returns>The identifier of the XAML namespace declaration.</returns>
	public string Namespace => ns;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.NamespaceDeclaration" /> class using initial property values.</summary>
	/// <param name="ns">The XAML namespace identifier, as a string. </param>
	/// <param name="prefix">The string prefix that is used for the namespace in prefix mappings.</param>
	public NamespaceDeclaration(string ns, string prefix)
	{
		this.ns = ns;
		this.prefix = prefix;
	}
}
