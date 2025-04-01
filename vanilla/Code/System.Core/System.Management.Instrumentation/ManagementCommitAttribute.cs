using System.Security.Permissions;

namespace System.Management.Instrumentation;

/// <summary>The ManagementCommit attribute marks a method that is called when it is necessary to update a set of read-write properties in one, atomic operation.</summary>
[AttributeUsage(AttributeTargets.Method)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementCommitAttribute : ManagementMemberAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementCommitAttribute" /> class. This is the default constructor.</summary>
	public ManagementCommitAttribute()
	{
	}
}
