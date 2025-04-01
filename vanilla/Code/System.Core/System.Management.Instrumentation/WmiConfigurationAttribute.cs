using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The WmiConfiguration attribute indicates that an assembly contains code that implements a WMI provider by using the WMI.NET Provider Extensions model. The attribute accepts parameters that establish the high-level configuration of the implemented WMI provider. </summary>
[AttributeUsage(AttributeTargets.Assembly)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class WmiConfigurationAttribute : Attribute
{
	/// <summary>Gets or sets the hosting group for the WMI provider.</summary>
	/// <returns>A <see cref="T:System.String" /> value that indicates the hosting group for the WMI provider.</returns>
	public string HostingGroup
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

	/// <summary>Gets or sets the hosting model for the WMI provider.</summary>
	/// <returns>A <see cref="T:System.Management.Instrumentation.ManagementHostingModel" /> value that indicates the hosting model of the WMI provider.</returns>
	public ManagementHostingModel HostingModel
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(ManagementHostingModel);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a value that specifies whether the WMI provider can impersonate its callers. If the value is false, the provider cannot impersonate, and if the value is true, the provider can impersonate.</summary>
	/// <returns>A Boolean value that indicates whether a provider can or cannot impersonate its callers. If the value is false, the provider cannot impersonate, and if the value is true, the provider can impersonate.</returns>
	public bool IdentifyLevel
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a Security Descriptor Definition Language (SDDL) string that specifies the security descriptor on the namespace in which the provider exposes management objects.</summary>
	/// <returns>An SDDL string that represents the security descriptor on the namespace in which the provider exposes management objects.</returns>
	public string NamespaceSecurity
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

	/// <summary>Gets or sets the WMI namespace in which the WMI provider exposes classes.</summary>
	/// <returns>A <see cref="T:System.String" /> value that indicates the namespace in which the WMI provider exposes classes.</returns>
	public string Scope
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets or sets a security descriptor for the WMI provider. For more information, see the SecurityDescriptor property information in the "__Win32Provider" topic in the MSDN online library at http://www.msdn.com. </summary>
	/// <returns>A <see cref="T:System.String" /> value that contains the security descriptor for the WMI provider.</returns>
	public string SecurityRestriction
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.WmiConfigurationAttribute" /> class that specifies the WMI namespace in which the WMI provider will expose classes.</summary>
	/// <param name="scope">The WMI namespace in which the provider will expose classes. For example, "root\MyProviderNamespace".</param>
	public WmiConfigurationAttribute(string scope)
	{
	}
}
