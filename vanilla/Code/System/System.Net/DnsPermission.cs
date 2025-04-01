using System.Security;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Controls rights to access Domain Name System (DNS) servers on the network.</summary>
[Serializable]
public sealed class DnsPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private const int version = 1;

	private bool m_noRestriction;

	/// <summary>Creates a new instance of the <see cref="T:System.Net.DnsPermission" /> class that either allows unrestricted DNS access or disallows DNS access.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid <see cref="T:System.Security.Permissions.PermissionState" /> value. </exception>
	public DnsPermission(PermissionState state)
	{
		m_noRestriction = state == PermissionState.Unrestricted;
	}

	/// <summary>Creates an identical copy of the current permission instance.</summary>
	/// <returns>A new instance of the <see cref="T:System.Net.DnsPermission" /> class that is an identical copy of the current instance.</returns>
	public override IPermission Copy()
	{
		return new DnsPermission(m_noRestriction ? PermissionState.Unrestricted : PermissionState.None);
	}

	/// <summary>Creates a permission instance that is the intersection of the current permission instance and the specified permission instance.</summary>
	/// <returns>A <see cref="T:System.Net.DnsPermission" /> instance that represents the intersection of the current <see cref="T:System.Net.DnsPermission" /> instance with the specified <see cref="T:System.Net.DnsPermission" /> instance, or null if the intersection is empty. If both the current instance and <paramref name="target" /> are unrestricted, this method returns a new <see cref="T:System.Net.DnsPermission" /> instance that is unrestricted; otherwise, it returns null.</returns>
	/// <param name="target">The <see cref="T:System.Net.DnsPermission" /> instance to intersect with the current instance. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is neither a <see cref="T:System.Net.DnsPermission" /> nor null. </exception>
	public override IPermission Intersect(IPermission target)
	{
		DnsPermission dnsPermission = Cast(target);
		if (dnsPermission == null)
		{
			return null;
		}
		if (IsUnrestricted() && dnsPermission.IsUnrestricted())
		{
			return new DnsPermission(PermissionState.Unrestricted);
		}
		return null;
	}

	/// <summary>Determines whether the current permission instance is a subset of the specified permission instance.</summary>
	/// <returns>false if the current instance is unrestricted and <paramref name="target" /> is either null or unrestricted; otherwise, true.</returns>
	/// <param name="target">The second <see cref="T:System.Net.DnsPermission" /> instance to be tested for the subset relationship. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is neither a <see cref="T:System.Net.DnsPermission" /> nor null. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		DnsPermission dnsPermission = Cast(target);
		if (dnsPermission == null)
		{
			return IsEmpty();
		}
		if (!dnsPermission.IsUnrestricted())
		{
			return m_noRestriction == dnsPermission.m_noRestriction;
		}
		return true;
	}

	/// <summary>Checks the overall permission state of the object.</summary>
	/// <returns>true if the <see cref="T:System.Net.DnsPermission" /> instance was created with <see cref="F:System.Security.Permissions.PermissionState.Unrestricted" />; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return m_noRestriction;
	}

	/// <summary>Creates an XML encoding of a <see cref="T:System.Net.DnsPermission" /> instance and its current state.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> instance that contains an XML-encoded representation of the security object, including state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = PermissionHelper.Element(typeof(DnsPermission), 1);
		if (m_noRestriction)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		return securityElement;
	}

	/// <summary>Reconstructs a <see cref="T:System.Net.DnsPermission" /> instance from an XML encoding.</summary>
	/// <param name="securityElement">The XML encoding to use to reconstruct the <see cref="T:System.Net.DnsPermission" /> instance. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="securityElement" /> is not a <see cref="T:System.Net.DnsPermission" /> element. </exception>
	public override void FromXml(SecurityElement securityElement)
	{
		PermissionHelper.CheckSecurityElement(securityElement, "securityElement", 1, 1);
		if (securityElement.Tag != "IPermission")
		{
			throw new ArgumentException("securityElement");
		}
		m_noRestriction = PermissionHelper.IsUnrestricted(securityElement);
	}

	/// <summary>Creates a permission instance that is the union of the current permission instance and the specified permission instance.</summary>
	/// <returns>A <see cref="T:System.Net.DnsPermission" /> instance that represents the union of the current <see cref="T:System.Net.DnsPermission" /> instance with the specified <see cref="T:System.Net.DnsPermission" /> instance. If <paramref name="target" /> is null, this method returns a copy of the current instance. If the current instance or <paramref name="target" /> is unrestricted, this method returns a <see cref="T:System.Net.DnsPermission" /> instance that is unrestricted; otherwise, it returns a <see cref="T:System.Net.DnsPermission" /> instance that is restricted.</returns>
	/// <param name="target">The <see cref="T:System.Net.DnsPermission" /> instance to combine with the current instance. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is neither a <see cref="T:System.Net.DnsPermission" /> nor null. </exception>
	public override IPermission Union(IPermission target)
	{
		DnsPermission dnsPermission = Cast(target);
		if (dnsPermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || dnsPermission.IsUnrestricted())
		{
			return new DnsPermission(PermissionState.Unrestricted);
		}
		return new DnsPermission(PermissionState.None);
	}

	private bool IsEmpty()
	{
		return !m_noRestriction;
	}

	private DnsPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		DnsPermission obj = target as DnsPermission;
		if (obj == null)
		{
			PermissionHelper.ThrowInvalidPermission(target, typeof(DnsPermission));
		}
		return obj;
	}
}
