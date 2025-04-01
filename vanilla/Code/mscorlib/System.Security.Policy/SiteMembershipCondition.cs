using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Policy;

/// <summary>Determines whether an assembly belongs to a code group by testing the site from which it originated. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SiteMembershipCondition : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
{
	private readonly int version = 1;

	private string _site;

	/// <summary>Gets or sets the site for which the membership condition tests.</summary>
	/// <returns>The site for which the membership condition tests.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> to null. </exception>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> to an invalid <see cref="T:System.Security.Policy.Site" />. </exception>
	public string Site
	{
		get
		{
			return _site;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("site");
			}
			if (!System.Security.Policy.Site.IsValid(value))
			{
				throw new ArgumentException("invalid site");
			}
			_site = value;
		}
	}

	internal SiteMembershipCondition()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.SiteMembershipCondition" /> class with name of the site that determines membership.</summary>
	/// <param name="site">The site name or wildcard expression. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="site" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="site" /> parameter is not a valid <see cref="T:System.Security.Policy.Site" />. </exception>
	public SiteMembershipCondition(string site)
	{
		Site = site;
	}

	/// <summary>Determines whether the specified evidence satisfies the membership condition.</summary>
	/// <returns>true if the specified evidence satisfies the membership condition; otherwise, false.</returns>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> against which to make the test. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property is null. </exception>
	public bool Check(Evidence evidence)
	{
		if (evidence == null)
		{
			return false;
		}
		IEnumerator hostEnumerator = evidence.GetHostEnumerator();
		while (hostEnumerator.MoveNext())
		{
			if (!(hostEnumerator.Current is Site))
			{
				continue;
			}
			string[] array = _site.Split('.');
			string[] array2 = (hostEnumerator.Current as Site).origin_site.Split('.');
			int num = array.Length - 1;
			int num2 = array2.Length - 1;
			while (num >= 0)
			{
				if (num == 0)
				{
					return string.Compare(array[0], "*", ignoreCase: true, CultureInfo.InvariantCulture) == 0;
				}
				if (string.Compare(array[num], array2[num2], ignoreCase: true, CultureInfo.InvariantCulture) != 0)
				{
					return false;
				}
				num--;
				num2--;
			}
			return true;
		}
		return false;
	}

	/// <summary>Creates an equivalent copy of the membership condition.</summary>
	/// <returns>A new, identical copy of the current membership condition.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property is null. </exception>
	public IMembershipCondition Copy()
	{
		return new SiteMembershipCondition(_site);
	}

	/// <summary>Determines whether the site from the specified <see cref="T:System.Security.Policy.SiteMembershipCondition" /> object is equivalent to the site contained in the current <see cref="T:System.Security.Policy.SiteMembershipCondition" />.</summary>
	/// <returns>true if the site from the specified <see cref="T:System.Security.Policy.SiteMembershipCondition" /> object is equivalent to the site contained in the current <see cref="T:System.Security.Policy.SiteMembershipCondition" />; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Security.Policy.SiteMembershipCondition" /> object to compare to the current <see cref="T:System.Security.Policy.SiteMembershipCondition" />. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property for the current object or the specified object is null. </exception>
	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		if (o is SiteMembershipCondition)
		{
			return new Site((o as SiteMembershipCondition)._site).Equals(new Site(_site));
		}
		return false;
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the security object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="e" /> parameter is not a valid membership condition element. </exception>
	public void FromXml(SecurityElement e)
	{
		FromXml(e, null);
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the security object. </param>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context, used to resolve <see cref="T:System.Security.NamedPermissionSet" /> references. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="e" /> parameter is not a valid membership condition element. </exception>
	public void FromXml(SecurityElement e, PolicyLevel level)
	{
		MembershipConditionHelper.CheckSecurityElement(e, "e", version, version);
		_site = e.Attribute("Site");
	}

	/// <summary>Gets the hash code for the current membership condition.</summary>
	/// <returns>The hash code for the current membership condition.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return _site.GetHashCode();
	}

	/// <summary>Creates and returns a string representation of the membership condition.</summary>
	/// <returns>A string representation of the membership condition.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property is null. </exception>
	public override string ToString()
	{
		return "Site - " + _site;
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property is null. </exception>
	public SecurityElement ToXml()
	{
		return ToXml(null);
	}

	/// <summary>Creates an XML encoding of the security object and its current state with the specified <see cref="T:System.Security.Policy.PolicyLevel" />.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context, used to resolve <see cref="T:System.Security.NamedPermissionSet" /> references. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.SiteMembershipCondition.Site" /> property is null. </exception>
	public SecurityElement ToXml(PolicyLevel level)
	{
		SecurityElement securityElement = MembershipConditionHelper.Element(typeof(SiteMembershipCondition), version);
		securityElement.AddAttribute("Site", _site);
		return securityElement;
	}
}
