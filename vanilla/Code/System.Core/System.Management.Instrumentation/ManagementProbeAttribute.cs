using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementProbe attribute indicates that a property or field represents a read-only WMI property.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementProbeAttribute : ManagementMemberAttribute
{
	/// <summary>Gets or sets a value that defines the type of output that the property that is marked with the ManagementProbe attribute will output.</summary>
	/// <returns>A <see cref="T:System.Type" /> value that indicates the type of output that the property that is marked with the ManagementProbe attribute will output.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementProbeAttribute" /> class. This is the default constructor for the class.</summary>
	public ManagementProbeAttribute()
	{
	}
}
