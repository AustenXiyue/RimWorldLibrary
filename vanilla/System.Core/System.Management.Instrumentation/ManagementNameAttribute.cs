using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementName attribute is used to override names exposed through a WMI class.</summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementNameAttribute : Attribute
{
	/// <summary>Gets or sets the user-friendly name for an object. The object can be a method parameter or properties marked with the ManagementProbe, ManagementKey, or ManagementConfiguration attributes.</summary>
	/// <returns>A <see cref="T:System.String" /> value that indicates the user friendly name for an object.</returns>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementNameAttribute" /> class that specifies a value for the <see cref="P:System.Management.ManagementNameAttribute.Name" /> property of the class.</summary>
	/// <param name="name">The user-friendly name for the object.</param>
	public ManagementNameAttribute(string name)
	{
	}
}
