using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Indicates to XAML processors that the whitespace surrounding elements of the type in markup should be trimmed when serializing.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class TrimSurroundingWhitespaceAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.TrimSurroundingWhitespaceAttribute" /> class.</summary>
	public TrimSurroundingWhitespaceAttribute()
	{
	}
}
