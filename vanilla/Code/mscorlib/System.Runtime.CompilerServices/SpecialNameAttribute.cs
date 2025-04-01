namespace System.Runtime.CompilerServices;

/// <summary>Indicates that a type or member is treated in a special way by the runtime or tools.  This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
public sealed class SpecialNameAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.SpecialNameAttribute" /> class. </summary>
	public SpecialNameAttribute()
	{
	}
}
