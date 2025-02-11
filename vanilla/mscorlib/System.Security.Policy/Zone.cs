using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Mono.Security;

namespace System.Security.Policy;

/// <summary>Provides the security zone of a code assembly as evidence for policy evaluation. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class Zone : EvidenceBase, IIdentityPermissionFactory, IBuiltInEvidence
{
	private SecurityZone zone;

	/// <summary>Gets the zone from which the code assembly originates.</summary>
	/// <returns>The zone from which the code assembly originates.</returns>
	public SecurityZone SecurityZone => zone;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Zone" /> class with the zone from which a code assembly originates.</summary>
	/// <param name="zone">The zone of origin for the associated code assembly. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="zone" /> parameter is not a valid <see cref="T:System.Security.SecurityZone" />. </exception>
	public Zone(SecurityZone zone)
	{
		if (!Enum.IsDefined(typeof(SecurityZone), zone))
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid zone {0}."), zone), "zone");
		}
		this.zone = zone;
	}

	/// <summary>Creates an equivalent copy of the evidence object.</summary>
	/// <returns>A new, identical copy of the evidence object.</returns>
	public object Copy()
	{
		return new Zone(zone);
	}

	/// <summary>Creates an identity permission that corresponds to the current instance of the <see cref="T:System.Security.Policy.Zone" /> evidence class.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.ZoneIdentityPermission" /> for the specified <see cref="T:System.Security.Policy.Zone" /> evidence.</returns>
	/// <param name="evidence">The evidence set from which to construct the identity permission. </param>
	public IPermission CreateIdentityPermission(Evidence evidence)
	{
		return new ZoneIdentityPermission(zone);
	}

	/// <summary>Creates a new zone with the specified URL.</summary>
	/// <returns>A new zone with the specified URL.</returns>
	/// <param name="url">The URL from which to create the zone. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="url" /> parameter is null. </exception>
	[MonoTODO("Not user configurable yet")]
	public static Zone CreateFromUrl(string url)
	{
		if (url == null)
		{
			throw new ArgumentNullException("url");
		}
		SecurityZone securityZone = SecurityZone.NoZone;
		if (url.Length == 0)
		{
			return new Zone(securityZone);
		}
		Uri uri = null;
		try
		{
			uri = new Uri(url);
		}
		catch
		{
			return new Zone(securityZone);
		}
		if (securityZone == SecurityZone.NoZone)
		{
			securityZone = (uri.IsFile ? ((!File.Exists(uri.LocalPath)) ? ((string.Compare("FILE://", 0, url, 0, 7, ignoreCase: true, CultureInfo.InvariantCulture) == 0) ? SecurityZone.Intranet : SecurityZone.Internet) : SecurityZone.MyComputer) : (uri.IsLoopback ? SecurityZone.Intranet : SecurityZone.Internet));
		}
		return new Zone(securityZone);
	}

	/// <summary>Compares the current <see cref="T:System.Security.Policy.Zone" /> evidence object to the specified object for equivalence.</summary>
	/// <returns>true if the two <see cref="T:System.Security.Policy.Zone" /> objects are equal; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Security.Policy.Zone" /> evidence object to test for equivalence with the current object. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="o" /> parameter is not a <see cref="T:System.Security.Policy.Zone" /> object. </exception>
	public override bool Equals(object o)
	{
		if (!(o is Zone zone))
		{
			return false;
		}
		return zone.zone == this.zone;
	}

	/// <summary>Gets the hash code of the current zone.</summary>
	/// <returns>The hash code of the current zone.</returns>
	public override int GetHashCode()
	{
		return (int)zone;
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Policy.Zone" />.</summary>
	/// <returns>A representation of the current <see cref="T:System.Security.Policy.Zone" />.</returns>
	public override string ToString()
	{
		SecurityElement securityElement = new SecurityElement("System.Security.Policy.Zone");
		securityElement.AddAttribute("version", "1");
		securityElement.AddChild(new SecurityElement("Zone", zone.ToString()));
		return securityElement.ToString();
	}

	int IBuiltInEvidence.GetRequiredSize(bool verbose)
	{
		return 3;
	}

	int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
	{
		_ = buffer[position++];
		_ = buffer[position++];
		return position;
	}

	int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
	{
		buffer[position++] = '\u0003';
		buffer[position++] = (char)((int)zone >> 16);
		buffer[position++] = (char)(zone & (SecurityZone)65535);
		return position;
	}
}
