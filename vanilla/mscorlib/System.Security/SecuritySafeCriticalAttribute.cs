namespace System.Security;

/// <summary>Identifies types or members as security-critical and safely accessible by transparent code.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
public sealed class SecuritySafeCriticalAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecuritySafeCriticalAttribute" /> class. </summary>
	public SecuritySafeCriticalAttribute()
	{
	}
}
