using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Mono.Security;

namespace System.Security.Policy;

/// <summary>Provides the URL from which a code assembly originates as evidence for policy evaluation. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class Url : EvidenceBase, IIdentityPermissionFactory, IBuiltInEvidence
{
	private string origin_url;

	/// <summary>Gets the URL from which the code assembly originates.</summary>
	/// <returns>The URL from which the code assembly originates.</returns>
	public string Value => origin_url;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Url" /> class with the URL from which a code assembly originates.</summary>
	/// <param name="name">The URL of origin for the associated code assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	public Url(string name)
		: this(name, validated: false)
	{
	}

	internal Url(string name, bool validated)
	{
		origin_url = (validated ? name : Prepare(name));
	}

	/// <summary>Creates a new copy of the evidence object.</summary>
	/// <returns>A new, identical copy of the evidence object.</returns>
	public object Copy()
	{
		return new Url(origin_url, validated: true);
	}

	/// <summary>Creates an identity permission corresponding to the current instance of the <see cref="T:System.Security.Policy.Url" /> evidence class.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.UrlIdentityPermission" /> for the specified <see cref="T:System.Security.Policy.Url" /> evidence.</returns>
	/// <param name="evidence">The evidence set from which to construct the identity permission. </param>
	public IPermission CreateIdentityPermission(Evidence evidence)
	{
		return new UrlIdentityPermission(origin_url);
	}

	/// <summary>Compares the current <see cref="T:System.Security.Policy.Url" /> evidence object to the specified object for equivalence.</summary>
	/// <returns>true if the two <see cref="T:System.Security.Policy.Url" /> objects are equal; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Security.Policy.Url" /> evidence object to test for equivalence with the current object. </param>
	public override bool Equals(object o)
	{
		if (!(o is Url { Value: var text }))
		{
			return false;
		}
		string text2 = origin_url;
		if (text.IndexOf(Uri.SchemeDelimiter) < 0)
		{
			text = "file://" + text;
		}
		if (text2.IndexOf(Uri.SchemeDelimiter) < 0)
		{
			text2 = "file://" + text2;
		}
		return string.Compare(text, text2, ignoreCase: true, CultureInfo.InvariantCulture) == 0;
	}

	/// <summary>Gets the hash code of the current URL.</summary>
	/// <returns>The hash code of the current URL.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		string text = origin_url;
		if (text.IndexOf(Uri.SchemeDelimiter) < 0)
		{
			text = "file://" + text;
		}
		return text.GetHashCode();
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Policy.Url" />.</summary>
	/// <returns>A representation of the current <see cref="T:System.Security.Policy.Url" />.</returns>
	public override string ToString()
	{
		SecurityElement securityElement = new SecurityElement("System.Security.Policy.Url");
		securityElement.AddAttribute("version", "1");
		securityElement.AddChild(new SecurityElement("Url", origin_url));
		return securityElement.ToString();
	}

	int IBuiltInEvidence.GetRequiredSize(bool verbose)
	{
		return ((!verbose) ? 1 : 3) + origin_url.Length;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
	{
		return 0;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
	{
		return 0;
	}

	private string Prepare(string url)
	{
		if (url == null)
		{
			throw new ArgumentNullException("Url");
		}
		if (url == string.Empty)
		{
			throw new FormatException(Locale.GetText("Invalid (empty) Url"));
		}
		if (url.IndexOf(Uri.SchemeDelimiter) > 0)
		{
			if (url.StartsWith("file://"))
			{
				url = "file://" + url.Substring(7);
			}
			url = new Uri(url, dontEscape: false, reduce: false).ToString();
		}
		int num = url.Length - 1;
		if (url[num] == '/')
		{
			url = url.Substring(0, num);
		}
		return url;
	}
}
