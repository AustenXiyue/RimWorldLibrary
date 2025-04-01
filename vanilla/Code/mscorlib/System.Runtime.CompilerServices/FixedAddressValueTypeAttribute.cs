namespace System.Runtime.CompilerServices;

/// <summary>Fixes the address of a static value type field throughout its lifetime. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Field)]
public sealed class FixedAddressValueTypeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.FixedAddressValueTypeAttribute" /> class. </summary>
	public FixedAddressValueTypeAttribute()
	{
	}
}
