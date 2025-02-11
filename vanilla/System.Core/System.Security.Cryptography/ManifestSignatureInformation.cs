using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides information for a manifest signature. </summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManifestSignatureInformation
{
	/// <summary>Gets the Authenticode signature information for a manifest. </summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.AuthenticodeSignatureInformation" /> object that contains Authenticode signature information for the manifest, or null if there is no signature.</returns>
	public AuthenticodeSignatureInformation AuthenticodeSignature
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the type of a manifest.</summary>
	/// <returns>One of the <see cref="T:System.Security.ManifestKinds" /> values.</returns>
	public ManifestKinds Manifest
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(ManifestKinds);
		}
	}

	/// <summary>Gets the details of the strong name signature of a manifest.</summary>
	/// <returns>A <see cref="P:System.Security.Cryptography.ManifestSignatureInformation.StrongNameSignature" /> object that contains the signature, or null if there is no strong name signature.</returns>
	public StrongNameSignatureInformation StrongNameSignature
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	internal ManifestSignatureInformation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Gathers and verifies information about the signatures of manifests that belong to a specified activation context.</summary>
	/// <returns>A collection that contains a <see cref="T:System.Security.Cryptography.ManifestSignatureInformation" /> object for each manifest that is verified.</returns>
	/// <param name="application">The activation context of the manifest. Activation contexts belong to an application and contain multiple manifests.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="application" /> parameter is null.</exception>
	public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Gathers and verifies information about the signatures of manifests that belong to a specified activation context and manifest type.</summary>
	/// <returns>A collection that contains a <see cref="T:System.Security.Cryptography.ManifestSignatureInformation" /> object for each manifest that is verified.</returns>
	/// <param name="application">The activation context of the manifest. Activation contexts belong to an application and contain multiple manifests.</param>
	/// <param name="manifests">The type of manifest. This parameter specifies which manifests in the activation context you want to verify.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="application" /> parameter is null.</exception>
	public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application, ManifestKinds manifests)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Gathers and verifies information about the signatures of manifests that belong to a specified activation context and manifest type, and allows certificates to be selected for revocation.</summary>
	/// <returns>A collection that contains a <see cref="T:System.Security.Cryptography.ManifestSignatureInformation" /> object for each manifest that is verified.</returns>
	/// <param name="application">The application context of the manifests. Activation contexts belong to an application and contain multiple manifests.</param>
	/// <param name="manifests">The type of manifest. This parameter specifies which manifests in the activation context you want to verify.</param>
	/// <param name="revocationFlag">One of the enumeration values that specifies which certificates in the chain are checked for revocation. The default is <see cref="F:System.Security.Cryptography.X509Certificates.X509RevocationFlag.ExcludeRoot" />.</param>
	/// <param name="revocationMode">Determines whether the X.509 verification should look online for revocation lists. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="application" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">A value specified for the <paramref name="revocationFlag" /> or <paramref name="revocationMode" /> parameter is invalid.</exception>
	[SecuritySafeCritical]
	public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application, ManifestKinds manifests, X509RevocationFlag revocationFlag, X509RevocationMode revocationMode)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
