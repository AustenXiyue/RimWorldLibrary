using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementEnumerator attribute marks a method that returns all the instances of a WMI class.</summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementEnumeratorAttribute : ManagementNewInstanceAttribute
{
	/// <summary>Gets or sets a value that defines the type of output that the method that is marked with the ManagementEnumerator attribute will output.</summary>
	/// <returns>A <see cref="T:System.Type" /> value that indicates the type of output that the method marked with the <see cref="ManagementEnumerator" /> attribute will output.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementEnumeratorAttribute" /> class.</summary>
	public ManagementEnumeratorAttribute()
	{
	}
}
