namespace System.IO.Packaging;

/// <summary>Specifies the result of a certificate or signature verification.</summary>
public enum VerifyResult
{
	/// <summary>The verification was successful.</summary>
	Success,
	/// <summary>The signature is not valid.</summary>
	InvalidSignature,
	/// <summary>The X.509 certificate is not available to verify the signature.</summary>
	CertificateRequired,
	/// <summary>The X.509 certificate is not valid.</summary>
	InvalidCertificate,
	/// <summary>A reference relationship to the signature was not found.</summary>
	ReferenceNotFound,
	/// <summary>The specified package or part has no signature.</summary>
	NotSigned
}
