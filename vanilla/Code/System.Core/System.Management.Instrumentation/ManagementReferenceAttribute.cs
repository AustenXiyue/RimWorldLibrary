using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementReferenceAttribute marks a class member, property or method parameter as a reference to another management object or class.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementReferenceAttribute : Attribute
{
	/// <summary>Gets or sets the name of the referenced type.</summary>
	/// <returns>A string containing the name of the referenced type.</returns>
	public string Type
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementReferenceAttribute" /> class. This is the default constructor.</summary>
	public ManagementReferenceAttribute()
	{
	}
}
