using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies that a property or type should be treated as ambient. The ambient concept relates to how XAML processors determine type owners of members.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
public sealed class AmbientAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.AmbientAttribute" /> class.</summary>
	public AmbientAttribute()
	{
	}
}
