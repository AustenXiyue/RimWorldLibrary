using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementTask attribute indicates that the target method implements a WMI method.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementTaskAttribute : ManagementMemberAttribute
{
	/// <summary>Gets or sets a value that defines the type of output that the method that is marked with the ManagementTask attribute will output.</summary>
	/// <returns>A <see cref="T:System.Type" /> value that indicates the type of output that the method that is marked with the ManagementTask attribute will output.</returns>
	public Type Schema
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementTaskAttribute" /> class. This is the default constructor.</summary>
	public ManagementTaskAttribute()
	{
	}
}
