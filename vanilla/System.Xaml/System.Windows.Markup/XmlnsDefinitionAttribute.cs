using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies a mapping on a per-assembly basis between a XAML namespace and a CLR namespace, which is then used for type resolution by a XAML object writer or XAML schema context.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlnsDefinitionAttribute : Attribute
{
	/// <summary>Gets the XAML namespace identifier specified in this attribute.</summary>
	/// <returns>The XAML namespace identifier.</returns>
	public string XmlNamespace { get; }

	/// <summary>Gets the string name of the CLR namespace specified in this attribute </summary>
	/// <returns>The CLR namespace, specified as a string.</returns>
	public string ClrNamespace { get; }

	/// <summary>Gets or sets the name of the assembly associated with the attribute. </summary>
	/// <returns>The assembly name.</returns>
	public string AssemblyName { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlnsDefinitionAttribute" /> class.</summary>
	/// <param name="xmlNamespace">The XAML namespace identifier.</param>
	/// <param name="clrNamespace">A string that references a CLR namespace name.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlNamespace" /> or <paramref name="clrNamespace" /> are null.</exception>
	public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
	{
		XmlNamespace = xmlNamespace ?? throw new ArgumentNullException("xmlNamespace");
		ClrNamespace = clrNamespace ?? throw new ArgumentNullException("clrNamespace");
	}
}
