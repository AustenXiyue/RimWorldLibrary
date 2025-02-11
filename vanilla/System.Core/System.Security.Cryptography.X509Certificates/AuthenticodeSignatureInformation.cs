using System.Security.Permissions;
using Unity;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Provides information about an Authenticode signature for a manifest. </summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class AuthenticodeSignatureInformation
{
	/// <summary>Gets the description of the signing certificate.</summary>
	/// <returns>The description of the signing certificate.</returns>
	public string Description
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the description URL of the signing certificate.</summary>
	/// <returns>The description URL of the signing certificate.</returns>
	public Uri DescriptionUrl
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the hash algorithm used to compute the signature.</summary>
	/// <returns>The hash algorithm used to compute the signature.</returns>
	public string HashAlgorithm
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the HRESULT value from verifying the signature.</summary>
	/// <returns>The HRESULT value from verifying the signature.</returns>
	public int HResult
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	/// <summary>Gets the chain of certificates used to verify the Authenticode signature.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> object that contains the certificate chain.</returns>
	public X509Chain SignatureChain
	{
		[SecuritySafeCritical]
		[StorePermission(SecurityAction.Demand, OpenStore = true, EnumerateCertificates = true)]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the certificate that signed the manifest.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the certificate.</returns>
	public X509Certificate2 SigningCertificate
	{
		[SecuritySafeCritical]
		[StorePermission(SecurityAction.Demand, OpenStore = true, EnumerateCertificates = true)]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the time stamp that was applied to the Authenticode signature.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.X509Certificates.TimestampInformation" /> object that contains the signature time stamp.</returns>
	public TimestampInformation Timestamp
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the trustworthiness of the Authenticode signature.</summary>
	/// <returns>One of the <see cref="T:System.Security.Cryptography.X509Certificates.TrustStatus" /> values. </returns>
	public TrustStatus TrustStatus
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TrustStatus);
		}
	}

	/// <summary>Gets the result of verifying the Authenticode signature.</summary>
	/// <returns>One of the <see cref="T:System.Security.Cryptography.SignatureVerificationResult" /> values.</returns>
	public SignatureVerificationResult VerificationResult
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(SignatureVerificationResult);
		}
	}

	internal AuthenticodeSignatureInformation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
