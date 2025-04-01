using System.Security.Permissions;

namespace System.Management.Instrumentation;

/// <summary>The ManagementCreateAttribute is used to indicate that a method creates a new instance of a managed entity.</summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementCreateAttribute : ManagementNewInstanceAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementCreateAttribute" /> class. This is the default constructor.</summary>
	public ManagementCreateAttribute()
	{
	}
}
