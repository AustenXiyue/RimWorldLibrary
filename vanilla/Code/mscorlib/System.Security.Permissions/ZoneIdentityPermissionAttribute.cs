using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.ZoneIdentityPermission" /> to be applied to code using declarative security. This class cannot be inherited. </summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class ZoneIdentityPermissionAttribute : CodeAccessSecurityAttribute
{
	private SecurityZone zone;

	/// <summary>Gets or sets membership in the content zone specified by the property value.</summary>
	/// <returns>One of the <see cref="T:System.Security.SecurityZone" /> values.</returns>
	public SecurityZone Zone
	{
		get
		{
			return zone;
		}
		set
		{
			zone = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.ZoneIdentityPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public ZoneIdentityPermissionAttribute(SecurityAction action)
		: base(action)
	{
		zone = SecurityZone.NoZone;
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.ZoneIdentityPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.ZoneIdentityPermission" /> that corresponds to this attribute.</returns>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new ZoneIdentityPermission(PermissionState.Unrestricted);
		}
		return new ZoneIdentityPermission(zone);
	}
}
