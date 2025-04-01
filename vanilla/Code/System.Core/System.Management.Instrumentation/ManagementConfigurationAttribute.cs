using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementConfiguration attribute indicates that a property or field represents a read-write WMI property.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementConfigurationAttribute : ManagementMemberAttribute
{
	/// <summary>Gets or sets the mode of the property, which specifies whether changes to it are applied as soon as possible or when a commit method is called.</summary>
	/// <returns>Returns a <see cref="T:System.Management.Instrumentation.ManagementConfigurationType" /> that indicates whether the WMI property uses <see cref="F:System.Management.Instrumentation.ManagementConfigurationType.Apply" /> or <see cref="F:System.Management.Instrumentation.ManagementConfigurationType.OnCommit" /> mode.</returns>
	public ManagementConfigurationType Mode
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(ManagementConfigurationType);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a value that defines the type of output that the property that is marked with the ManagementConfiguration attribute will return.</summary>
	/// <returns>A <see cref="T:System.Type" /> value representing the type of output that the property marked with the ManagementConfiguration attribute will return.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementConfigurationAttribute" /> class. This is the default constructor.</summary>
	public ManagementConfigurationAttribute()
	{
	}
}
