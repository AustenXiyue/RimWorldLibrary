using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Indicates which property of a type is the XAML content property. A XAML processor uses this information when processing XAML child elements of XAML representations of the attributed type.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class ContentPropertyAttribute : Attribute
{
	/// <summary>Gets the name of the property that is the content property.</summary>
	/// <returns>The name of the property that is the content property.</returns>
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ContentPropertyAttribute" /> class.</summary>
	public ContentPropertyAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ContentPropertyAttribute" /> class, by using the specified name.</summary>
	/// <param name="name">The property name for the property that is the content property.</param>
	public ContentPropertyAttribute(string name)
	{
		Name = name;
	}
}
