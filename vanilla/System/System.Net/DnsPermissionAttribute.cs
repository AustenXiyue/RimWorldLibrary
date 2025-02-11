using System.Security;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Specifies permission to request information from Domain Name Servers.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DnsPermissionAttribute : CodeAccessSecurityAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Net.DnsPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" /> value.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="action" /> parameter is not a valid <see cref="T:System.Security.Permissions.SecurityAction" />. </exception>
	public DnsPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new instance of the <see cref="T:System.Net.DnsPermission" /> class.</summary>
	/// <returns>A <see cref="T:System.Net.DnsPermission" /> that corresponds to the security declaration.</returns>
	public override IPermission CreatePermission()
	{
		return new DnsPermission(base.Unrestricted ? PermissionState.Unrestricted : PermissionState.None);
	}
}
