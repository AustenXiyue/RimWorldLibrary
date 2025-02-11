using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Cryptography;

namespace System.Security.Policy;

/// <summary>Determines whether an assembly belongs to a code group by testing its software publisher's Authenticode X.509v3 certificate. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class PublisherMembershipCondition : IConstantMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
{
	private readonly int version = 1;

	private X509Certificate x509;

	/// <summary>Gets or sets the Authenticode X.509v3 certificate for which the membership condition tests.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> for which the membership condition tests.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property value is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public X509Certificate Certificate
	{
		get
		{
			return x509;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			x509 = value;
		}
	}

	internal PublisherMembershipCondition()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.PublisherMembershipCondition" /> class with the Authenticode X.509v3 certificate that determines membership.</summary>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> that contains the software publisher's public key. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="certificate" /> parameter is null. </exception>
	public PublisherMembershipCondition(X509Certificate certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (certificate.GetHashCode() == 0)
		{
			throw new ArgumentException("certificate");
		}
		x509 = certificate;
	}

	/// <summary>Determines whether the specified evidence satisfies the membership condition.</summary>
	/// <returns>true if the specified evidence satisfies the membership condition; otherwise, false.</returns>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> against which to make the test. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public bool Check(Evidence evidence)
	{
		if (evidence == null)
		{
			return false;
		}
		IEnumerator hostEnumerator = evidence.GetHostEnumerator();
		while (hostEnumerator.MoveNext())
		{
			if (hostEnumerator.Current is Publisher && x509.Equals((hostEnumerator.Current as Publisher).Certificate))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates an equivalent copy of the membership condition.</summary>
	/// <returns>A new, identical copy of the current membership condition.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public IMembershipCondition Copy()
	{
		return new PublisherMembershipCondition(x509);
	}

	/// <summary>Determines whether the publisher certificate from the specified object is equivalent to the publisher certificate contained in the current <see cref="T:System.Security.Policy.PublisherMembershipCondition" />.</summary>
	/// <returns>true if the publisher certificate from the specified object is equivalent to the publisher certificate contained in the current <see cref="T:System.Security.Policy.PublisherMembershipCondition" />; otherwise, false.</returns>
	/// <param name="o">The object to compare to the current <see cref="T:System.Security.Policy.PublisherMembershipCondition" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override bool Equals(object o)
	{
		if (!(o is PublisherMembershipCondition publisherMembershipCondition))
		{
			return false;
		}
		return x509.Equals(publisherMembershipCondition.Certificate);
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
		string text = e.Attribute("X509Certificate");
		if (text != null)
		{
			byte[] data = CryptoConvert.FromHex(text);
			x509 = new X509Certificate(data);
		}
	}

	/// <summary>Gets the hash code for the current membership condition.</summary>
	/// <returns>The hash code for the current membership condition.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return x509.GetHashCode();
	}

	/// <summary>Creates and returns a string representation of the <see cref="T:System.Security.Policy.PublisherMembershipCondition" />.</summary>
	/// <returns>A representation of the <see cref="T:System.Security.Policy.PublisherMembershipCondition" />.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "Publisher - " + x509.GetPublicKeyString();
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public SecurityElement ToXml()
	{
		return ToXml(null);
	}

	/// <summary>Creates an XML encoding of the security object and its current state with the specified <see cref="T:System.Security.Policy.PolicyLevel" />.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context, which is used to resolve <see cref="T:System.Security.NamedPermissionSet" /> references. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Policy.PublisherMembershipCondition.Certificate" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public SecurityElement ToXml(PolicyLevel level)
	{
		SecurityElement securityElement = MembershipConditionHelper.Element(typeof(PublisherMembershipCondition), version);
		securityElement.AddAttribute("X509Certificate", x509.GetRawCertDataString());
		return securityElement;
	}
}
