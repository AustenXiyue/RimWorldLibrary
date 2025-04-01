using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Defines the identity permission for the zone from which the code originates. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class ZoneIdentityPermission : CodeAccessPermission, IBuiltInPermission
{
	private const int version = 1;

	private SecurityZone zone;

	/// <summary>Gets or sets the zone represented by the current <see cref="T:System.Security.Permissions.ZoneIdentityPermission" />.</summary>
	/// <returns>One of the <see cref="T:System.Security.SecurityZone" /> values.</returns>
	/// <exception cref="T:System.ArgumentException">The parameter value is not a valid value of <see cref="T:System.Security.SecurityZone" />. </exception>
	public SecurityZone SecurityZone
	{
		get
		{
			return zone;
		}
		set
		{
			if (!Enum.IsDefined(typeof(SecurityZone), value))
			{
				throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), value), "SecurityZone");
			}
			zone = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.ZoneIdentityPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public ZoneIdentityPermission(PermissionState state)
	{
		CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: false);
		zone = SecurityZone.NoZone;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.ZoneIdentityPermission" /> class to represent the specified zone identity.</summary>
	/// <param name="zone">The zone identifier. </param>
	public ZoneIdentityPermission(SecurityZone zone)
	{
		SecurityZone = zone;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		return new ZoneIdentityPermission(zone);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null, this permission does not represent the <see cref="F:System.Security.SecurityZone.NoZone" /> security zone, and the specified permission is not equal to the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		ZoneIdentityPermission zoneIdentityPermission = Cast(target);
		if (zoneIdentityPermission == null)
		{
			return zone == SecurityZone.NoZone;
		}
		if (zone != SecurityZone.NoZone)
		{
			return zone == zoneIdentityPermission.zone;
		}
		return true;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. -or-The two permissions are not equal and the current permission does not represent the <see cref="F:System.Security.SecurityZone.NoZone" /> security zone.</exception>
	public override IPermission Union(IPermission target)
	{
		ZoneIdentityPermission zoneIdentityPermission = Cast(target);
		if (zoneIdentityPermission == null)
		{
			if (zone != SecurityZone.NoZone)
			{
				return Copy();
			}
			return null;
		}
		if (zone == zoneIdentityPermission.zone || zoneIdentityPermission.zone == SecurityZone.NoZone)
		{
			return Copy();
		}
		if (zone == SecurityZone.NoZone)
		{
			return zoneIdentityPermission.Copy();
		}
		throw new ArgumentException(Locale.GetText("Union impossible"));
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		ZoneIdentityPermission zoneIdentityPermission = Cast(target);
		if (zoneIdentityPermission == null || zone == SecurityZone.NoZone)
		{
			return null;
		}
		if (zone == zoneIdentityPermission.zone)
		{
			return Copy();
		}
		return null;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		string text = esd.Attribute("Zone");
		if (text == null)
		{
			zone = SecurityZone.NoZone;
		}
		else
		{
			zone = (SecurityZone)Enum.Parse(typeof(SecurityZone), text);
		}
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (zone != SecurityZone.NoZone)
		{
			securityElement.AddAttribute("Zone", zone.ToString());
		}
		return securityElement;
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 14;
	}

	private ZoneIdentityPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		ZoneIdentityPermission obj = target as ZoneIdentityPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(ZoneIdentityPermission));
		}
		return obj;
	}
}
