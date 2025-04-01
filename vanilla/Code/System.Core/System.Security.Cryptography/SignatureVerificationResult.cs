namespace System.Security.Cryptography;

/// <summary>Specifies most of the result codes for signature verification. </summary>
public enum SignatureVerificationResult
{
	/// <summary>The identity of the assembly specified in the /asm:assembly/asm:assemblyIdentity node of the manifest does not match the identity of the assembly in the Authenticode signature in the /asm:assembly/ds:signature/ds:KeyInfo/msrel:RelData/r:license/r:grant/as:ManifestInformation/as:assemblyIdentity node.</summary>
	AssemblyIdentityMismatch = 1,
	/// <summary>The digital signature of the object did not verify.</summary>
	BadDigest = -2146869232,
	/// <summary>The signature format is invalid.</summary>
	BadSignatureFormat = -2146762749,
	/// <summary>The basic constraint extension of a certificate has not been observed.</summary>
	BasicConstraintsNotObserved = -2146869223,
	/// <summary>The certificate has expired.</summary>
	CertificateExpired = -2146762495,
	/// <summary>The certificate was explicitly marked as not trusted by the user.</summary>
	CertificateExplicitlyDistrusted = -2146762479,
	/// <summary>The certificate is missing or has an empty value for an important field, such as a subject or issuer name.</summary>
	CertificateMalformed = -2146762488,
	/// <summary>The certificate is not trusted explicitly.</summary>
	CertificateNotExplicitlyTrusted = -2146762748,
	/// <summary>The certificate has been revoked.</summary>
	CertificateRevoked = -2146762484,
	/// <summary>The certificate cannot be used for signing and verification.</summary>
	CertificateUsageNotAllowed = -2146762490,
	/// <summary>The strong name signature does not verify in the <see cref="T:System.Security.Cryptography.X509Certificates.AuthenticodeSignatureInformation" /> object. Because the strong name signature wraps the Authenticode signature, someone could replace the Authenticode signature with a signature of their choosing. To prevent this, this error code is returned if the strong name does not verify because substituting a part of the strong name signature will invalidate it.</summary>
	ContainingSignatureInvalid = 2,
	/// <summary>The chain could not be built.</summary>
	CouldNotBuildChain = -2146762486,
	/// <summary>There is a general trust failure with the certificate.</summary>
	GenericTrustFailure = -2146762485,
	/// <summary>The certificate has an invalid name. The name is either not included in the permitted list or is explicitly excluded.</summary>
	InvalidCertificateName = -2146762476,
	/// <summary>The certificate has an invalid policy.</summary>
	InvalidCertificatePolicy = -2146762477,
	/// <summary>The certificate has an invalid role.</summary>
	InvalidCertificateRole = -2146762493,
	/// <summary>The signature of the certificate cannot be verified.</summary>
	InvalidCertificateSignature = -2146869244,
	/// <summary>The certificate has an invalid usage.</summary>
	InvalidCertificateUsage = -2146762480,
	/// <summary>One of the counter signatures is invalid.</summary>
	InvalidCountersignature = -2146869245,
	/// <summary>The certificate for the signer of the message is invalid or not found.</summary>
	InvalidSignerCertificate = -2146869246,
	/// <summary>A certificate was issued after the issuing certificate has expired.</summary>
	InvalidTimePeriodNesting = -2146762494,
	/// <summary>The time stamp signature or certificate could not be verified or is malformed.</summary>
	InvalidTimestamp = -2146869243,
	/// <summary>A parent of a given certificate did not issue that child certificate.</summary>
	IssuerChainingError = -2146762489,
	/// <summary>The signature is missing.</summary>
	MissingSignature = -2146762496,
	/// <summary>A path length constraint in the certification chain has been violated.</summary>
	PathLengthConstraintViolated = -2146762492,
	/// <summary>The public key token from the manifest identity in the /asm:assembly/asm:AssemblyIdentity node does not match the public key token of the key that is used to sign the manifest.</summary>
	PublicKeyTokenMismatch = 3,
	/// <summary>The publisher name from /asm:assembly/asmv2:publisherIdentity does not match the subject name of the signing certificate, or the issuer key hash from the same publisherIdentity node does not match the key hash of the signing certificate.</summary>
	PublisherMismatch = 4,
	/// <summary>The revocation check failed.</summary>
	RevocationCheckFailure = -2146762482,
	/// <summary>A system-level error occurred while verifying trust.</summary>
	SystemError = -2146869247,
	/// <summary>A certificate contains an unknown extension that is marked critical.</summary>
	UnknownCriticalExtension = -2146762491,
	/// <summary>The certificate has an unknown trust provider.</summary>
	UnknownTrustProvider = -2146762751,
	/// <summary>The certificate has an unknown verification action.</summary>
	UnknownVerificationAction = -2146762750,
	/// <summary>The certification chain processed correctly, but one of the CA certificates is not trusted by the policy provider.</summary>
	UntrustedCertificationAuthority = -2146762478,
	/// <summary>The root certificate is not trusted.</summary>
	UntrustedRootCertificate = -2146762487,
	/// <summary>The test root certificate is not trusted.</summary>
	UntrustedTestRootCertificate = -2146762483,
	/// <summary>The certificate verification result is valid.</summary>
	Valid = 0
}
