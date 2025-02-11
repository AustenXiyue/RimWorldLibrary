using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Cryptography;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.PublisherIdentityPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class PublisherIdentityPermissionAttribute : CodeAccessSecurityAttribute
{
	private string certFile;

	private string signedFile;

	private string x509data;

	/// <summary>Gets or sets a certification file containing an Authenticode X.509v3 certificate.</summary>
	/// <returns>The file path of an X.509 certificate file (usually has the extension.cer).</returns>
	public string CertFile
	{
		get
		{
			return certFile;
		}
		set
		{
			certFile = value;
		}
	}

	/// <summary>Gets or sets a signed file from which to extract an Authenticode X.509v3 certificate.</summary>
	/// <returns>The file path of a file signed with the Authenticode signature.</returns>
	public string SignedFile
	{
		get
		{
			return signedFile;
		}
		set
		{
			signedFile = value;
		}
	}

	/// <summary>Gets or sets an Authenticode X.509v3 certificate that identifies the publisher of the calling code.</summary>
	/// <returns>A hexadecimal representation of the X.509 certificate.</returns>
	public string X509Certificate
	{
		get
		{
			return x509data;
		}
		set
		{
			x509data = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PublisherIdentityPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public PublisherIdentityPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new instance of <see cref="T:System.Security.Permissions.PublisherIdentityPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.PublisherIdentityPermission" /> that corresponds to this attribute.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new PublisherIdentityPermission(PermissionState.Unrestricted);
		}
		if (x509data != null)
		{
			return new PublisherIdentityPermission(new X509Certificate(CryptoConvert.FromHex(x509data)));
		}
		if (certFile != null)
		{
			return new PublisherIdentityPermission(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile(certFile));
		}
		if (signedFile != null)
		{
			return new PublisherIdentityPermission(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(signedFile));
		}
		return new PublisherIdentityPermission(PermissionState.None);
	}
}
