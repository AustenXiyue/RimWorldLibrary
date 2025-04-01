using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>Marks a type definition as discardable.</summary>
[ComVisible(true)]
public class DiscardableAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DiscardableAttribute" /> class with default values.</summary>
	public DiscardableAttribute()
	{
	}
}
