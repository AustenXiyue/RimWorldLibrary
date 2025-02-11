using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using MS.Internal.IO.Packaging;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

/// <summary>Represents a digital signature that is applied to a set of package parts and relationships.</summary>
public class PackageDigitalSignature
{
	private PackageDigitalSignatureManager _manager;

	private XmlDigitalSignatureProcessor _processor;

	private CertificatePart _certificatePart;

	private ReadOnlyCollection<Uri> _signedParts;

	private ReadOnlyCollection<PackageRelationshipSelector> _signedRelationshipSelectors;

	private bool _alreadyLookedForCertPart;

	private bool _invalid;

	/// <summary>Gets a collection of all the <see cref="T:System.IO.Packaging.PackagePart" /> objects signed with the signature. </summary>
	/// <returns>A collection that contains all the package parts signed with the signature.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public ReadOnlyCollection<Uri> SignedParts
	{
		get
		{
			ThrowIfInvalidated();
			if (_signedParts == null)
			{
				_signedParts = new ReadOnlyCollection<Uri>(_processor.PartManifest);
			}
			return _signedParts;
		}
	}

	/// <summary>Gets the list of the <see cref="T:System.IO.Packaging.PackageRelationship" /> parts that have been signed with the signature.</summary>
	/// <returns>The list of the <see cref="T:System.IO.Packaging.PackageRelationship" /> parts that have been signed with the signature.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public ReadOnlyCollection<PackageRelationshipSelector> SignedRelationshipSelectors
	{
		get
		{
			ThrowIfInvalidated();
			if (_signedRelationshipSelectors == null)
			{
				_signedRelationshipSelectors = new ReadOnlyCollection<PackageRelationshipSelector>(_processor.RelationshipManifest);
			}
			return _signedRelationshipSelectors;
		}
	}

	/// <summary>Gets the <see cref="T:System.IO.Packaging.PackagePart" /> that contains the signature. </summary>
	/// <returns>The package part that contains the signature.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public PackagePart SignaturePart
	{
		get
		{
			ThrowIfInvalidated();
			return _processor.SignaturePart;
		}
	}

	/// <summary>Gets the X.509 certificate of the signer. </summary>
	/// <returns>The digital certificate of the signer, or null if the certificate is not stored in the <see cref="T:System.IO.Packaging.Package" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public X509Certificate Signer
	{
		get
		{
			ThrowIfInvalidated();
			return _processor.Signer;
		}
	}

	/// <summary>Gets the date and time that the signature was created. </summary>
	/// <returns>The date and time that the signature was created.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public DateTime SigningTime
	{
		get
		{
			ThrowIfInvalidated();
			return _processor.SigningTime;
		}
	}

	/// <summary>Gets the format of the date and time returned by the <see cref="P:System.IO.Packaging.PackageDigitalSignature.SigningTime" /> property. </summary>
	/// <returns>The format of the date and time returned by the <see cref="P:System.IO.Packaging.PackageDigitalSignature.SigningTime" /> property.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public string TimeFormat
	{
		get
		{
			ThrowIfInvalidated();
			return _processor.TimeFormat;
		}
	}

	/// <summary>Gets the encrypted hash value of the <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" />. </summary>
	/// <returns>A byte array that contains the encrypted hash value of the <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public byte[] SignatureValue
	{
		get
		{
			ThrowIfInvalidated();
			return _processor.SignatureValue;
		}
	}

	/// <summary>Gets the URI string that identifies the signature type. </summary>
	/// <returns>A URI string that identifies the signature type. The default is <see cref="F:System.Security.Cryptography.Xml.SignedXml.XmlDsigC14NTransformUrl" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public string SignatureType
	{
		get
		{
			ThrowIfInvalidated();
			return XmlDigitalSignatureProcessor.ContentType.ToString();
		}
	}

	/// <summary>Gets or sets the XML digital signature.</summary>
	/// <returns>The XML digital signature.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public Signature Signature
	{
		get
		{
			ThrowIfInvalidated();
			return _processor.Signature;
		}
		set
		{
			ThrowIfInvalidated();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_processor.Signature = value;
		}
	}

	/// <summary>Gets the X.509 certificate embedding option. </summary>
	/// <returns>One of the <see cref="T:System.IO.Packaging.CertificateEmbeddingOption" /> values that specifies the option for the digital signature.</returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public CertificateEmbeddingOption CertificateEmbeddingOption
	{
		get
		{
			ThrowIfInvalidated();
			if (GetCertificatePart() == null)
			{
				if (Signer == null)
				{
					return CertificateEmbeddingOption.NotEmbedded;
				}
				return CertificateEmbeddingOption.InSignaturePart;
			}
			return CertificateEmbeddingOption.InCertificatePart;
		}
	}

	/// <summary>Returns an ordered list of the <see cref="T:System.Security.Cryptography.Xml.Transform" /> operations applied to a given part. </summary>
	/// <returns>An ordered list of URI strings, such as <see cref="F:System.Security.Cryptography.Xml.SignedXml.XmlDsigCanonicalizationUrl" /> or <see cref="F:System.Security.Cryptography.Xml.SignedXml.XmlDsigC14NTransformUrl" />, that represents the canonical XML transformations applied to the part with the given <paramref name="partName" /> URI.</returns>
	/// <param name="partName">The <see cref="T:System.Uri" /> of the <see cref="T:System.IO.Packaging.PackagePart" /> to return the transform list for.</param>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public List<string> GetPartTransformList(Uri partName)
	{
		ThrowIfInvalidated();
		return _processor.GetPartTransformList(partName);
	}

	/// <summary>Verifies the digital signature against an X.509 certificate. </summary>
	/// <returns>
	///   <see cref="F:System.IO.Packaging.VerifyResult.Success" /> if the verification succeeded; otherwise, one of the <see cref="T:System.IO.Packaging.VerifyResult" /> values that identifies a problem. </returns>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public VerifyResult Verify()
	{
		ThrowIfInvalidated();
		if (Signer == null)
		{
			return VerifyResult.CertificateRequired;
		}
		return Verify(Signer);
	}

	/// <summary>Verifies the digital signature against a given X.509 certificate. </summary>
	/// <returns>
	///   <see cref="F:System.IO.Packaging.VerifyResult.Success" /> if the verification succeeded; otherwise, one of the <see cref="T:System.IO.Packaging.VerifyResult" /> values that identifies a problem.</returns>
	/// <param name="signingCertificate">The signer's X.509 certificate to verify the digital signature against.</param>
	/// <exception cref="T:System.InvalidOperationException">The digital <see cref="P:System.IO.Packaging.PackageDigitalSignature.Signature" /> has been deleted.</exception>
	public VerifyResult Verify(X509Certificate signingCertificate)
	{
		ThrowIfInvalidated();
		VerifyResult verifyResult = VerifyResult.NotSigned;
		if (signingCertificate == null)
		{
			throw new ArgumentNullException("signingCertificate");
		}
		foreach (Uri signedPart in SignedParts)
		{
			if (!_manager.Package.PartExists(signedPart))
			{
				return VerifyResult.ReferenceNotFound;
			}
		}
		X509Certificate2 x509Certificate = signingCertificate as X509Certificate2;
		if (x509Certificate == null)
		{
			x509Certificate = new X509Certificate2(signingCertificate.Handle);
		}
		if (_processor.Verify(x509Certificate))
		{
			return VerifyResult.Success;
		}
		return VerifyResult.InvalidSignature;
	}

	internal PackageDigitalSignature(PackageDigitalSignatureManager manager, XmlDigitalSignatureProcessor processor)
	{
		_manager = manager;
		_processor = processor;
	}

	internal PackageDigitalSignature(PackageDigitalSignatureManager manager, PackagePart signaturePart)
	{
		_manager = manager;
		_processor = new XmlDigitalSignatureProcessor(manager, signaturePart, this);
	}

	internal void Invalidate()
	{
		_invalid = true;
	}

	internal CertificatePart GetCertificatePart()
	{
		if (_certificatePart == null && !_alreadyLookedForCertPart)
		{
			foreach (PackageRelationship item in SignaturePart.GetRelationshipsByType(CertificatePart.RelationshipType))
			{
				if (item.TargetMode != 0)
				{
					throw new FileFormatException(SR.PackageSignatureCorruption);
				}
				Uri uri = PackUriHelper.ResolvePartUri(SignaturePart.Uri, item.TargetUri);
				if (_manager.Package.PartExists(uri))
				{
					_certificatePart = new CertificatePart(_manager.Package, uri);
					break;
				}
			}
			_alreadyLookedForCertPart = true;
		}
		return _certificatePart;
	}

	internal void SetCertificatePart(CertificatePart certificatePart)
	{
		_certificatePart = certificatePart;
	}

	private void ThrowIfInvalidated()
	{
		if (_invalid)
		{
			throw new InvalidOperationException(SR.SignatureDeleted);
		}
	}
}
