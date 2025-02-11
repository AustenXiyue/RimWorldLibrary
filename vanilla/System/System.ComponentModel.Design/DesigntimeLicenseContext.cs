using System.Collections;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents a design-time license context that can support a license provider at design time.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class DesigntimeLicenseContext : LicenseContext
{
	internal Hashtable savedLicenseKeys = new Hashtable();

	/// <summary>Gets the license usage mode.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.LicenseUsageMode" /> indicating the licensing mode for the context.</returns>
	public override LicenseUsageMode UsageMode => LicenseUsageMode.Designtime;

	/// <summary>Gets a saved license key.</summary>
	/// <returns>The saved license key that matches the specified type.</returns>
	/// <param name="type">The type of the license key. </param>
	/// <param name="resourceAssembly">The assembly to get the key from. </param>
	public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly)
	{
		return null;
	}

	/// <summary>Sets a saved license key.</summary>
	/// <param name="type">The type of the license key. </param>
	/// <param name="key">The license key. </param>
	public override void SetSavedLicenseKey(Type type, string key)
	{
		savedLicenseKeys[type.AssemblyQualifiedName] = key;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesigntimeLicenseContext" /> class. </summary>
	public DesigntimeLicenseContext()
	{
	}
}
