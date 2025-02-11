namespace System.Runtime.CompilerServices;

/// <summary>Specifies that a type contains an unmanaged array that might potentially overflow. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Struct)]
public sealed class UnsafeValueTypeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.UnsafeValueTypeAttribute" /> class.</summary>
	public UnsafeValueTypeAttribute()
	{
	}
}
