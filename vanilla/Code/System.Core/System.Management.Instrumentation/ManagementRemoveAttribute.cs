using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementRemoveAttribute is used to indicate that a method cleans up an instance of a managed entity.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementRemoveAttribute : ManagementMemberAttribute
{
	/// <summary>Gets or sets a value that defines the type of output that the object that is marked with the ManagementRemove attribute will output.</summary>
	/// <returns>A <see cref="T:System.Type" /> value that indicates the type of output that the object marked with the Remove attribute will output.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementRemoveAttribute" /> class. This is the default constructor.</summary>
	public ManagementRemoveAttribute()
	{
	}
}
