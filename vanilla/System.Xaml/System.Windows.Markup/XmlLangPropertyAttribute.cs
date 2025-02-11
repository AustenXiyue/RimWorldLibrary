using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Identifies the property to associate with the xml:lang attribute.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlLangPropertyAttribute : Attribute
{
	/// <summary>Gets the name of the property that is specified in this attribute.</summary>
	/// <returns>The name of the property.</returns>
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlLangPropertyAttribute" /> class.</summary>
	/// <param name="name">The property name to associate with the xml:lang attribute.</param>
	public XmlLangPropertyAttribute(string name)
	{
		Name = name;
	}
}
