using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Indicates that a collection type should be processed as being whitespace significant by a XAML processor.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class WhitespaceSignificantCollectionAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.WhitespaceSignificantCollectionAttribute" /> class.</summary>
	public WhitespaceSignificantCollectionAttribute()
	{
	}
}
