using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Cryptography;

namespace System.Security.Permissions;

/// <summary>Represents the identity of a software publisher. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class PublisherIdentityPermission : CodeAccessPermission, IBuiltInPermission
{
	private const int version = 1;

	private X509Certificate x509;

	/// <summary>Gets or sets an Authenticode X.509v3 certificate that represents the identity of the software publisher.</summary>
	/// <returns>An X.509 certificate representing the identity of the software publisher.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Permissions.PublisherIdentityPermission.Certificate" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Permissions.PublisherIdentityPermission.Certificate" /> is not a valid certificate. </exception>
	/// <exception cref="T:System.NotSupportedException">The property cannot be set because the identity is ambiguous.</exception>
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
				throw new ArgumentNullException("X509Certificate");
			}
			x509 = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PublisherIdentityPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public PublisherIdentityPermission(PermissionState state)
	{
		CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PublisherIdentityPermission" /> class with the specified Authenticode X.509v3 certificate.</summary>
	/// <param name="certificate">An X.509 certificate representing the software publisher's identity. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="certificate" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="certificate" /> parameter is not a valid certificate. </exception>
	public PublisherIdentityPermission(X509Certificate certificate)
	{
		Certificate = certificate;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		PublisherIdentityPermission publisherIdentityPermission = new PublisherIdentityPermission(PermissionState.None);
		if (x509 != null)
		{
			publisherIdentityPermission.Certificate = x509;
		}
		return publisherIdentityPermission;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		if (esd.Attributes["X509v3Certificate"] is string hex)
		{
			byte[] data = CryptoConvert.FromHex(hex);
			x509 = new X509Certificate(data);
		}
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		PublisherIdentityPermission publisherIdentityPermission = Cast(target);
		if (publisherIdentityPermission == null)
		{
			return null;
		}
		if (x509 != null && publisherIdentityPermission.x509 != null && x509.GetRawCertDataString() == publisherIdentityPermission.x509.GetRawCertDataString())
		{
			return new PublisherIdentityPermission(publisherIdentityPermission.x509);
		}
		return null;
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		PublisherIdentityPermission publisherIdentityPermission = Cast(target);
		if (publisherIdentityPermission == null)
		{
			return false;
		}
		if (x509 == null)
		{
			return true;
		}
		if (publisherIdentityPermission.x509 == null)
		{
			return false;
		}
		return x509.GetRawCertDataString() == publisherIdentityPermission.x509.GetRawCertDataString();
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (x509 != null)
		{
			securityElement.AddAttribute("X509v3Certificate", x509.GetRawCertDataString());
		}
		return securityElement;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. -or-The two permissions are not equal.</exception>
	public override IPermission Union(IPermission target)
	{
		PublisherIdentityPermission publisherIdentityPermission = Cast(target);
		if (publisherIdentityPermission == null)
		{
			return Copy();
		}
		if (x509 != null && publisherIdentityPermission.x509 != null)
		{
			if (x509.GetRawCertDataString() == publisherIdentityPermission.x509.GetRawCertDataString())
			{
				return new PublisherIdentityPermission(x509);
			}
		}
		else
		{
			if (x509 == null && publisherIdentityPermission.x509 != null)
			{
				return new PublisherIdentityPermission(publisherIdentityPermission.x509);
			}
			if (x509 != null && publisherIdentityPermission.x509 == null)
			{
				return new PublisherIdentityPermission(x509);
			}
		}
		return null;
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 10;
	}

	private PublisherIdentityPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		PublisherIdentityPermission obj = target as PublisherIdentityPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(PublisherIdentityPermission));
		}
		return obj;
	}
}
