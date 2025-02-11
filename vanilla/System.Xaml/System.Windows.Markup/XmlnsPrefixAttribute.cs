using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Identifies a recommended prefix to associate with a XAML namespace for XAML usage, when writing elements and attributes in a XAML file (serialization) or when interacting with a design environment that has XAML editing features.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlnsPrefixAttribute : Attribute
{
	/// <summary>Gets the XAML namespace identifier associated with this attribute.</summary>
	/// <returns>The XAML namespace identifier. </returns>
	public string XmlNamespace { get; }

	/// <summary>Gets the recommended prefix associated with this attribute.</summary>
	/// <returns>The recommended prefix string.</returns>
	public string Prefix { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlnsPrefixAttribute" /> class.</summary>
	/// <param name="xmlNamespace">The XAML namespace indentifier.</param>
	/// <param name="prefix">The recommended prefix string.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlNamespace" /> or <paramref name="prefix" /> is null.</exception>
	public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
	{
		XmlNamespace = xmlNamespace ?? throw new ArgumentNullException("xmlNamespace");
		Prefix = prefix ?? throw new ArgumentNullException("prefix");
	}
}
