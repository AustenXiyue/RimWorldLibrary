using System.Security.Permissions;

namespace System.Management.Instrumentation;

/// <summary>The ManagementKey attribute identifies the key properties of a WMI class.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementKeyAttribute : ManagementMemberAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementKeyAttribute" />  class. This is the default constructor.</summary>
	public ManagementKeyAttribute()
	{
	}
}
