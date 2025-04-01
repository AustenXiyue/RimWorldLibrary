namespace System.Runtime.CompilerServices;

/// <summary>Indicates that a native enumeration is not qualified by the enumeration type name. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Enum)]
public sealed class ScopelessEnumAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.ScopelessEnumAttribute" /> class. </summary>
	public ScopelessEnumAttribute()
	{
	}
}
