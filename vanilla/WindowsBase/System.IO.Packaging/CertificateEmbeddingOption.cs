namespace System.IO.Packaging;

/// <summary>Specifies the location where the X.509 certificate that is used in signing is stored.</summary>
public enum CertificateEmbeddingOption
{
	/// <summary>The certificate is embedded in its own <see cref="T:System.IO.Packaging.PackagePart" />.</summary>
	InCertificatePart,
	/// <summary>The certificate is embedded in the <see cref="P:System.IO.Packaging.PackageDigitalSignature.SignaturePart" /> that is created for the signature being added.</summary>
	InSignaturePart,
	/// <summary>The certificate in not embedded in the package.</summary>
	NotEmbedded
}
