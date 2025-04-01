using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>This class is used by the WMI.NET Provider Extensions framework. It is the base class for all the management attributes that can be applied to members.</summary>
[AttributeUsage(AttributeTargets.All)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public abstract class ManagementMemberAttribute : Attribute
{
	/// <summary>Gets or sets the name of the management attribute.</summary>
	/// <returns>Returns a string which is the name of the management attribute.</returns>
	public string Name
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

	/// <summary>Initializes a new instance of <see cref="T:System.Management.ManagementMemberAttribute" /> the class. This is the default constructor.</summary>
	protected ManagementMemberAttribute()
	{
	}
}
