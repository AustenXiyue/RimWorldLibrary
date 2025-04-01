namespace System.Security;

/// <summary>Identifies which of the nonpublic <see cref="T:System.Security.SecurityCriticalAttribute" /> members are accessible by transparent code within the assembly. </summary>
[Obsolete("SecurityTreatAsSafe is only used for .NET 2.0 transparency compatibility.  Please use the SecuritySafeCriticalAttribute instead.")]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
public sealed class SecurityTreatAsSafeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityTreatAsSafeAttribute" /> class. </summary>
	public SecurityTreatAsSafeAttribute()
	{
	}
}
