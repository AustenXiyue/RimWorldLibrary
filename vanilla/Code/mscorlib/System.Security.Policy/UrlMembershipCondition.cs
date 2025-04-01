using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using Mono.Security;

namespace System.Security.Policy;

/// <summary>Determines whether an assembly belongs to a code group by testing its URL. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class UrlMembershipCondition : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
{
	private readonly int version = 1;

	private Url url;

	private string userUrl;

	/// <summary>Gets or sets the URL for which the membership condition tests.</summary>
	/// <returns>The URL for which the membership condition tests.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> to null. </exception>
	/// <exception cref="T:System.ArgumentException">Value is not an absolute URL.</exception>
	public string Url
	{
		get
		{
			if (userUrl == null)
			{
				userUrl = url.Value;
			}
			return userUrl;
		}
		set
		{
			url = new Url(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.UrlMembershipCondition" /> class with the URL that determines membership.</summary>
	/// <param name="url">The URL for which to test. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="url" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="url" /> must be an absolute URL.</exception>
	public UrlMembershipCondition(string url)
	{
		if (url == null)
		{
			throw new ArgumentNullException("url");
		}
		CheckUrl(url);
		userUrl = url;
		this.url = new Url(url);
	}

	internal UrlMembershipCondition(Url url, string userUrl)
	{
		this.url = (Url)url.Copy();
		this.userUrl = userUrl;
	}

	/// <summary>Determines whether the specified evidence satisfies the membership condition.</summary>
	/// <returns>true if the specified evidence satisfies the membership condition; otherwise, false.</returns>
	/// <param name="evidence">The evidence set against which to make the test. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> property is null. </exception>
	public bool Check(Evidence evidence)
	{
		if (evidence == null)
		{
			return false;
		}
		string value = url.Value;
		int num = value.LastIndexOf("*");
		if (num == -1)
		{
			num = value.Length;
		}
		IEnumerator hostEnumerator = evidence.GetHostEnumerator();
		while (hostEnumerator.MoveNext())
		{
			if (hostEnumerator.Current is Url && string.Compare(value, 0, (hostEnumerator.Current as Url).Value, 0, num, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates an equivalent copy of the membership condition.</summary>
	/// <returns>A new, identical copy of the current membership condition.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> property is null. </exception>
	public IMembershipCondition Copy()
	{
		return new UrlMembershipCondition(url, userUrl);
	}

	/// <summary>Determines whether the URL from the specified object is equivalent to the URL contained in the current <see cref="T:System.Security.Policy.UrlMembershipCondition" />.</summary>
	/// <returns>true if the URL from the specified object is equivalent to the URL contained in the current <see cref="T:System.Security.Policy.UrlMembershipCondition" />; otherwise, false.</returns>
	/// <param name="o">The object to compare to the current <see cref="T:System.Security.Policy.UrlMembershipCondition" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> property of the current object or the specified object is null. </exception>
	public override bool Equals(object o)
	{
		UrlMembershipCondition urlMembershipCondition = o as UrlMembershipCondition;
		if (o == null)
		{
			return false;
		}
		string value = url.Value;
		int num = value.Length;
		if (value[num - 1] == '*')
		{
			num--;
			if (value[num - 1] == '/')
			{
				num--;
			}
		}
		return string.Compare(value, 0, urlMembershipCondition.Url, 0, num, ignoreCase: true, CultureInfo.InvariantCulture) == 0;
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
	/// <param name="level">The policy level context, used to resolve named permission set references. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="e" /> parameter is not a valid membership condition element. </exception>
	public void FromXml(SecurityElement e, PolicyLevel level)
	{
		MembershipConditionHelper.CheckSecurityElement(e, "e", version, version);
		string text = e.Attribute("Url");
		if (text != null)
		{
			CheckUrl(text);
			url = new Url(text);
		}
		else
		{
			url = null;
		}
		userUrl = text;
	}

	/// <summary>Gets the hash code for the current membership condition.</summary>
	/// <returns>The hash code for the current membership condition.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return url.GetHashCode();
	}

	/// <summary>Creates and returns a string representation of the membership condition.</summary>
	/// <returns>A string representation of the state of the membership condition.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> property is null. </exception>
	public override string ToString()
	{
		return "Url - " + Url;
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	public SecurityElement ToXml()
	{
		return ToXml(null);
	}

	/// <summary>Creates an XML encoding of the security object and its current state with the specified <see cref="T:System.Security.Policy.PolicyLevel" />.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <param name="level">The policy level context for resolving named permission set references. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.UrlMembershipCondition.Url" /> property is null. </exception>
	public SecurityElement ToXml(PolicyLevel level)
	{
		SecurityElement securityElement = MembershipConditionHelper.Element(typeof(UrlMembershipCondition), version);
		securityElement.AddAttribute("Url", userUrl);
		return securityElement;
	}

	internal void CheckUrl(string url)
	{
		if (new Uri((url.IndexOf(Uri.SchemeDelimiter) < 0) ? ("file://" + url) : url, dontEscape: false, reduce: false).Host.IndexOf('*') >= 1)
		{
			throw new ArgumentException(Locale.GetText("Invalid * character in url"), "name");
		}
	}
}
