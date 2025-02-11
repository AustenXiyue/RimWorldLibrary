using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementBind attribute indicates that a method is used to return the instance of a WMI class associated with a specific key value.</summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementBindAttribute : ManagementNewInstanceAttribute
{
	/// <summary>Gets or sets a value that defines the type of output that the method that is marked with the ManagementEnumerator attribute will output.</summary>
	/// <returns>A <see cref="T:System.Type" /> value that indicates the type of output that the method marked with the <see cref="ManagementBind" /> attribute will output.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementBindAttribute" /> class. This is the default constructor.</summary>
	public ManagementBindAttribute()
	{
	}
}
