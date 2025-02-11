using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.IO.Packaging.Extensions;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

/// <summary>Provides a utility class for the creation and verification of digital signatures in a <see cref="T:System.IO.Packaging.Package" />.    </summary>
public sealed class PackageDigitalSignatureManager
{
	private class StringMatchPredicate
	{
		private string _id;

		public StringMatchPredicate(string id)
		{
			_id = id;
		}

		public bool Match(string id)
		{
			return string.CompareOrdinal(_id, id) == 0;
		}
	}

	private delegate bool RelationshipOperation(PackageRelationship r, object context);

	private CertificateEmbeddingOption _certificateEmbeddingOption;

	private Package _container;

	private nint _parentWindow;

	private static Uri _defaultOriginPartName = PackUriHelper.CreatePartUri(new Uri("/package/services/digital-signature/origin.psdsor", UriKind.Relative));

	private Uri _originPartName = _defaultOriginPartName;

	private PackagePart _originPart;

	private string _hashAlgorithmString = DefaultHashAlgorithm;

	private string _signatureTimeFormat = XmlSignatureProperties.DefaultDateTimeFormat;

	private List<PackageDigitalSignature> _signatures;

	private Dictionary<string, string> _transformDictionary;

	private bool _originSearchConducted;

	private bool _originPartExists;

	private ReadOnlyCollection<PackageDigitalSignature> _signatureList;

	private static readonly ContentType _originPartContentType = new ContentType("application/vnd.openxmlformats-package.digital-signature-origin");

	private static readonly string _guidStorageFormatString = "N";

	private static readonly string _defaultHashAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";

	private static readonly string _originRelationshipType = "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/origin";

	private static readonly string _originToSignatureRelationshipType = "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/signature";

	private static readonly string _defaultSignaturePartNamePrefix = "/package/services/digital-signature/xml-signature/";

	private static readonly string _defaultSignaturePartNameExtension = ".psdsxs";

	/// <summary>Gets a value that indicates whether the package contains any signatures. </summary>
	/// <returns>true if the package contains signatures; otherwise, false.</returns>
	public bool IsSigned
	{
		get
		{
			EnsureSignatures();
			return _signatures.Count > 0;
		}
	}

	/// <summary>Gets a collection of all the signatures contained in the package. </summary>
	/// <returns>A collection of all the <see cref="T:System.IO.Packaging.PackageDigitalSignature" /> objects.</returns>
	public ReadOnlyCollection<PackageDigitalSignature> Signatures
	{
		get
		{
			EnsureSignatures();
			if (_signatureList == null)
			{
				_signatureList = new ReadOnlyCollection<PackageDigitalSignature>(_signatures);
			}
			return _signatureList;
		}
	}

	/// <summary>Gets a dictionary that contains each defined <see cref="P:System.IO.Packaging.PackagePart.ContentType" /> and its associated XML <see cref="T:System.Security.Cryptography.Xml.Transform" />.<see cref="P:System.Security.Cryptography.Xml.Transform.Algorithm" /> identifier. </summary>
	/// <returns>A dictionary that contains each defined <see cref="P:System.IO.Packaging.PackagePart.ContentType" /> and its associated XML <see cref="T:System.Security.Cryptography.Xml.Transform" />.<see cref="P:System.Security.Cryptography.Xml.Transform.Algorithm" /> identifier.</returns>
	public Dictionary<string, string> TransformMapping => _transformDictionary;

	/// <summary>Gets or sets a handle to the parent window for displaying a certificate selection dialog box. </summary>
	/// <returns>The handle of the parent window to use to display the certificate selection dialog box.</returns>
	public nint ParentWindow
	{
		get
		{
			return _parentWindow;
		}
		set
		{
			_parentWindow = value;
		}
	}

	/// <summary>Gets or sets the URI identifier for the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> instance used to create and verify signatures. </summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> URI identifier for the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> instance used to create and verify signatures.</returns>
	/// <exception cref="T:System.ArgumentNullException">The string for the URI to set is null.</exception>
	/// <exception cref="T:System.ArgumentException">The string for the URI to set is empty.</exception>
	public string HashAlgorithm
	{
		get
		{
			return _hashAlgorithmString;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(SR.UnsupportedHashAlgorithm, "value");
			}
			_hashAlgorithmString = value;
		}
	}

	/// <summary>Gets or sets the X.509 certificate embedding option used by the <see cref="M:System.IO.Packaging.PackageDigitalSignatureManager.Sign(System.Collections.Generic.IEnumerable{System.Uri})" /> method to digitally sign package parts. </summary>
	/// <returns>One of the <see cref="T:System.IO.Packaging.CertificateEmbeddingOption" /> values. </returns>
	public CertificateEmbeddingOption CertificateOption
	{
		get
		{
			return _certificateEmbeddingOption;
		}
		set
		{
			if (value < CertificateEmbeddingOption.InCertificatePart || value > CertificateEmbeddingOption.NotEmbedded)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_certificateEmbeddingOption = value;
		}
	}

	/// <summary>Gets or sets the date/time format used to create a signature <see cref="P:System.IO.Packaging.PackageDigitalSignature.SigningTime" />. </summary>
	/// <returns>The date/time format used to create a signature <see cref="P:System.IO.Packaging.PackageDigitalSignature.SigningTime" />.</returns>
	public string TimeFormat
	{
		get
		{
			return _signatureTimeFormat;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (XmlSignatureProperties.LegalFormat(value))
			{
				_signatureTimeFormat = value;
				return;
			}
			throw new FormatException(SR.BadSignatureTimeFormatString);
		}
	}

	/// <summary>Gets the uniform resource identifier (URI) of the signature origin part.</summary>
	/// <returns>The URI of the signature origin part.</returns>
	public Uri SignatureOrigin
	{
		get
		{
			OriginPartExists();
			return _originPartName;
		}
	}

	/// <summary>Gets the type of default signature origin relationship.</summary>
	/// <returns>The type of default signature origin relationship.</returns>
	public static string SignatureOriginRelationshipType => _originRelationshipType;

	/// <summary>Gets a URI string that identifies the default hash algorithm used to create and verify signatures. </summary>
	/// <returns>A URI string that identifies the default hash algorithm used to create and verify signatures.</returns>
	public static string DefaultHashAlgorithm
	{
		get
		{
			if (!BaseAppContextSwitches.UseSha1AsDefaultHashAlgorithmForDigitalSignatures)
			{
				return _defaultHashAlgorithm;
			}
			return "http://www.w3.org/2000/09/xmldsig#sha1";
		}
	}

	internal Package Package => _container;

	private bool ReadOnly => _container.FileOpenAccess == FileAccess.Read;

	private PackagePart OriginPart
	{
		get
		{
			if (_originPart == null && !OriginPartExists())
			{
				_originPart = _container.CreatePart(_originPartName, _originPartContentType.ToString());
				_container.CreateRelationship(_originPartName, TargetMode.Internal, _originRelationshipType);
			}
			return _originPart;
		}
	}

	/// <summary>Occurs when <see cref="M:System.IO.Packaging.PackageDigitalSignatureManager.VerifySignatures(System.Boolean)" /> encounters an invalid signature. </summary>
	public event InvalidSignatureEventHandler InvalidSignatureEvent;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Packaging.PackageDigitalSignatureManager" /> class for use with a specified <see cref="T:System.IO.Packaging.Package" />. </summary>
	/// <param name="package">The package associated with this signature manager.</param>
	public PackageDigitalSignatureManager(Package package)
	{
		if (package == null)
		{
			throw new ArgumentNullException("package");
		}
		_parentWindow = IntPtr.Zero;
		_container = package;
		_transformDictionary = new Dictionary<string, string>(4);
		_transformDictionary[PackagingUtilities.RelationshipPartContentType.ToString()] = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
		_transformDictionary[XmlDigitalSignatureProcessor.ContentType.ToString()] = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
	}

	/// <summary>Prompts the user for an X.509 certificate, which is then used to digitally sign a specified list of package parts.</summary>
	/// <returns>The digital signature used to sign the list of <paramref name="parts" />.</returns>
	/// <param name="parts">The list of uniform resource identifiers (URIs) for the <see cref="T:System.IO.Packaging.PackagePart" /> elements to sign.</param>
	public PackageDigitalSignature Sign(IEnumerable<Uri> parts)
	{
		X509Certificate x509Certificate = PromptForSigningCertificate(ParentWindow);
		if (x509Certificate == null)
		{
			return null;
		}
		return Sign(parts, x509Certificate);
	}

	/// <summary>Signs a list of package parts with a given X.509 certificate. </summary>
	/// <returns>The digital signature used to sign the given list of <paramref name="parts" />; or null if no certificate could be found or the user clicked "Cancel" in the certificate selection dialog box.</returns>
	/// <param name="parts">The list of uniform resource identifiers (URIs) for the <see cref="T:System.IO.Packaging.PackagePart" /> elements to sign.</param>
	/// <param name="certificate">The X.509 certificate to use to digitally sign each of the specified <paramref name="parts" />.</param>
	public PackageDigitalSignature Sign(IEnumerable<Uri> parts, X509Certificate certificate)
	{
		return Sign(parts, certificate, null);
	}

	/// <summary>Signs a list of package parts and package relationships with a given X.509 certificate. </summary>
	/// <returns>The digital signature used to sign the elements specified in the <paramref name="parts" /> and <paramref name="relationshipSelectors" /> lists.</returns>
	/// <param name="parts">The list of uniform resource identifiers (URIs) for the <see cref="T:System.IO.Packaging.PackagePart" /> objects to sign.</param>
	/// <param name="certificate">The X.509 certificate to use to digitally sign each of the specified parts and relationships.</param>
	/// <param name="relationshipSelectors">The list of <see cref="T:System.IO.Packaging.PackageRelationship" /> objects to sign.</param>
	/// <exception cref="T:System.ArgumentException">Neither <paramref name="parts" /> nor <paramref name="relationshipSelectors" /> specify any objects to sign.</exception>
	public PackageDigitalSignature Sign(IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors)
	{
		return Sign(parts, certificate, relationshipSelectors, XTable.Get(XTable.ID.OpcSignatureAttrValue));
	}

	/// <summary>Signs a list of package parts and package relationships with a given X.509 certificate and identifier (ID).</summary>
	/// <returns>The digital signature used to sign the elements specified in the <paramref name="parts" /> and <paramref name="relationshipSelectors" /> lists.</returns>
	/// <param name="parts">The list of uniform resource identifiers (URIs) for the <see cref="T:System.IO.Packaging.PackagePart" /> objects to sign.</param>
	/// <param name="certificate">The X.509 certificate to use to digitally sign each of the specified parts and relationships.</param>
	/// <param name="relationshipSelectors">The list of <see cref="T:System.IO.Packaging.PackageRelationship" /> objects to sign.</param>
	/// <param name="signatureId">An identification string to associate with the signature.</param>
	/// <exception cref="T:System.ArgumentException">Neither <paramref name="parts" /> nor <paramref name="relationshipSelectors" /> specify any elements to sign.</exception>
	public PackageDigitalSignature Sign(IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors, string signatureId)
	{
		if (parts == null && relationshipSelectors == null)
		{
			throw new ArgumentException(SR.NothingToSign);
		}
		return Sign(parts, certificate, relationshipSelectors, signatureId, null, null);
	}

	/// <summary>Signs a list of package parts, package relationships, or custom objects with a specified X.509 certificate and signature identifier (ID).</summary>
	/// <returns>The digital signature used to sign the elements specified in the <paramref name="parts" /> and <paramref name="relationshipSelectors" /> lists.</returns>
	/// <param name="parts">The list of uniform resource identifiers (URIs) for the <see cref="T:System.IO.Packaging.PackagePart" /> objects to sign.</param>
	/// <param name="certificate">The X.509 certificate to use to digitally sign each of the specified parts and relationships.</param>
	/// <param name="relationshipSelectors">The list of <see cref="T:System.IO.Packaging.PackageRelationship" /> objects to sign.</param>
	/// <param name="signatureId">An identification string to associate with the signature.</param>
	/// <param name="signatureObjects">A list of custom data objects to sign.</param>
	/// <param name="objectReferences">A list of references to custom objects to sign.</param>
	/// <exception cref="T:System.ArgumentException">Neither <paramref name="parts" />, <paramref name="relationshipSelectors" />, <paramref name="signatureObjects" />, nor <paramref name="objectReferences" /> specify any elements to sign.</exception>
	/// <exception cref="T:System.InvalidOperationException">A <see cref="P:System.IO.Packaging.PackagePart.ContentType" /> of a part being signed references an empty, null, or undefined <see cref="P:System.IO.Packaging.PackageDigitalSignatureManager.TransformMapping" />.</exception>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="signatureId" /> is not null and is not a valid XML schema ID (for example, begins with a leading numeric digit).</exception>
	public PackageDigitalSignature Sign(IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors, string signatureId, IEnumerable<DataObject> signatureObjects, IEnumerable<Reference> objectReferences)
	{
		if (ReadOnly)
		{
			throw new InvalidOperationException(SR.CannotSignReadOnlyFile);
		}
		VerifySignArguments(parts, certificate, relationshipSelectors, signatureId, signatureObjects, objectReferences);
		if (string.IsNullOrEmpty(signatureId))
		{
			signatureId = "packageSignature";
		}
		EnsureSignatures();
		Uri uri = GenerateSignaturePartName();
		if (_container.PartExists(uri))
		{
			throw new ArgumentException(SR.DuplicateSignature);
		}
		OriginPart.CreateRelationship(uri, TargetMode.Internal, _originToSignatureRelationshipType);
		_container.Flush();
		VerifyPartsExist(parts);
		bool embedCertificate = _certificateEmbeddingOption == CertificateEmbeddingOption.InSignaturePart;
		X509Certificate2 x509Certificate = certificate as X509Certificate2;
		if (x509Certificate == null)
		{
			x509Certificate = new X509Certificate2(certificate.Handle);
		}
		PackageDigitalSignature packageDigitalSignature = null;
		PackagePart packagePart = null;
		try
		{
			packagePart = _container.CreatePart(uri, XmlDigitalSignatureProcessor.ContentType.ToString());
			packageDigitalSignature = XmlDigitalSignatureProcessor.Sign(this, packagePart, parts, relationshipSelectors, x509Certificate, signatureId, embedCertificate, signatureObjects, objectReferences);
		}
		catch (InvalidOperationException)
		{
			InternalRemoveSignature(uri, _signatures.Count);
			_container.Flush();
			throw;
		}
		catch (IOException)
		{
			InternalRemoveSignature(uri, _signatures.Count);
			_container.Flush();
			throw;
		}
		catch (CryptographicException)
		{
			InternalRemoveSignature(uri, _signatures.Count);
			_container.Flush();
			throw;
		}
		_signatures.Add(packageDigitalSignature);
		if (_certificateEmbeddingOption == CertificateEmbeddingOption.InCertificatePart)
		{
			Uri uri2 = PackUriHelper.CreatePartUri(new Uri(CertificatePart.PartNamePrefix + x509Certificate.SerialNumber + CertificatePart.PartNameExtension, UriKind.Relative));
			CertificatePart certificatePart = new CertificatePart(_container, uri2);
			certificatePart.SetCertificate(x509Certificate);
			packagePart.CreateRelationship(uri2, TargetMode.Internal, CertificatePart.RelationshipType);
			packageDigitalSignature.SetCertificatePart(certificatePart);
		}
		_container.Flush();
		return packageDigitalSignature;
	}

	/// <summary>Countersigns all the signatures in the package with a user-selected X.509 certificate.</summary>
	/// <returns>The signature that was added as a countersign; or null if no certificate could be located or the user canceled the certificate selection dialog.</returns>
	public PackageDigitalSignature Countersign()
	{
		if (!IsSigned)
		{
			throw new InvalidOperationException(SR.NoCounterSignUnsignedContainer);
		}
		X509Certificate x509Certificate = PromptForSigningCertificate(ParentWindow);
		if (x509Certificate == null)
		{
			return null;
		}
		return Countersign(x509Certificate);
	}

	/// <summary>Countersigns all the signatures in the package with a specified X.509 certificate. </summary>
	/// <returns>The signature that was added as a countersign.</returns>
	/// <param name="certificate">The X.509 certificate to add as a countersign signature.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null</exception>
	public PackageDigitalSignature Countersign(X509Certificate certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (!IsSigned)
		{
			throw new InvalidOperationException(SR.NoCounterSignUnsignedContainer);
		}
		List<Uri> list = new List<Uri>(_signatures.Count);
		for (int i = 0; i < _signatures.Count; i++)
		{
			list.Add(_signatures[i].SignaturePart.Uri);
		}
		return Sign(list, certificate);
	}

	/// <summary>Countersigns a list of signatures with a given X.509 certificate.</summary>
	/// <returns>The digital signature used to countersign each of the <paramref name="signatures" />.</returns>
	/// <param name="certificate">The X.509 certificate to countersign each of the specified <paramref name="signatures" />.</param>
	/// <param name="signatures">The list of signatures to countersign.</param>
	/// <exception cref="T:System.ArgumentNullException">Either the <paramref name="certificate" /> or <paramref name="signatures" /> parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The package contains no signed <see cref="T:System.IO.Packaging.PackagePart" /> objects.</exception>
	/// <exception cref="T:System.ArgumentException">The package contains no <see cref="T:System.IO.Packaging.PackageDigitalSignature" /> parts.</exception>
	public PackageDigitalSignature Countersign(X509Certificate certificate, IEnumerable<Uri> signatures)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (signatures == null)
		{
			throw new ArgumentNullException("signatures");
		}
		if (!IsSigned)
		{
			throw new InvalidOperationException(SR.NoCounterSignUnsignedContainer);
		}
		foreach (Uri signature in signatures)
		{
			if (!_container.GetPart(signature).ValidatedContentType().AreTypeAndSubTypeEqual(XmlDigitalSignatureProcessor.ContentType))
			{
				throw new ArgumentException(SR.Format(SR.CanOnlyCounterSignSignatureParts, signatures));
			}
		}
		return Sign(signatures, certificate);
	}

	/// <summary>Verifies the signatures on all signed parts within the package. </summary>
	/// <returns>
	///   <see cref="F:System.IO.Packaging.VerifyResult.Success" /> (value 0) if all signatures are verified successfully; otherwise, an enumeration that identifies the error.</returns>
	/// <param name="exitOnFailure">true to exit on first failure; otherwise, false to continue and check all signatures.</param>
	public VerifyResult VerifySignatures(bool exitOnFailure)
	{
		EnsureSignatures();
		VerifyResult result;
		if (_signatures.Count == 0)
		{
			result = VerifyResult.NotSigned;
		}
		else
		{
			result = VerifyResult.Success;
			for (int i = 0; i < _signatures.Count; i++)
			{
				VerifyResult verifyResult = _signatures[i].Verify();
				if (verifyResult != 0)
				{
					result = verifyResult;
					if (this.InvalidSignatureEvent != null)
					{
						this.InvalidSignatureEvent(this, new SignatureVerificationEventArgs(_signatures[i], verifyResult));
					}
					if (exitOnFailure)
					{
						break;
					}
				}
			}
		}
		return result;
	}

	/// <summary>Removes the digital signature with a given signature uniform resource identifier (URI). </summary>
	/// <param name="signatureUri">The URI of the <see cref="T:System.IO.Packaging.PackageDigitalSignature" /> to remove.</param>
	public void RemoveSignature(Uri signatureUri)
	{
		if (ReadOnly)
		{
			throw new InvalidOperationException(SR.CannotRemoveSignatureFromReadOnlyFile);
		}
		if (signatureUri == null)
		{
			throw new ArgumentNullException("signatureUri");
		}
		if (!IsSigned)
		{
			return;
		}
		int signatureIndex = GetSignatureIndex(signatureUri);
		if (signatureIndex < 0)
		{
			return;
		}
		try
		{
			InternalRemoveSignature(signatureUri, _signatures.Count - 1);
			_signatures[signatureIndex].Invalidate();
		}
		finally
		{
			_signatures.RemoveAt(signatureIndex);
		}
	}

	/// <summary>Removes all digital signatures from the package. </summary>
	public void RemoveAllSignatures()
	{
		if (ReadOnly)
		{
			throw new InvalidOperationException(SR.CannotRemoveSignatureFromReadOnlyFile);
		}
		EnsureSignatures();
		try
		{
			for (int i = 0; i < _signatures.Count; i++)
			{
				PackagePart signaturePart = _signatures[i].SignaturePart;
				foreach (PackageRelationship item in signaturePart.GetRelationshipsByType(CertificatePart.RelationshipType))
				{
					if (item.TargetMode == TargetMode.Internal)
					{
						_container.DeletePart(PackUriHelper.ResolvePartUri(item.SourceUri, item.TargetUri));
					}
				}
				_container.DeletePart(signaturePart.Uri);
				_signatures[i].Invalidate();
			}
			DeleteOriginPart();
		}
		finally
		{
			_signatures.Clear();
		}
	}

	/// <summary>Returns the digital signature for a given signature uniform resource identifier (URI). </summary>
	/// <returns>The digital signature for the specified <paramref name="signatureUri" />, or null if a signature for the given <paramref name="signatureUri" /> cannot be found.</returns>
	/// <param name="signatureUri">The URI of the digital signature to return.</param>
	public PackageDigitalSignature GetSignature(Uri signatureUri)
	{
		if (signatureUri == null)
		{
			throw new ArgumentNullException("signatureUri");
		}
		int signatureIndex = GetSignatureIndex(signatureUri);
		if (signatureIndex < 0)
		{
			return null;
		}
		return _signatures[signatureIndex];
	}

	/// <summary>Verifies a given X.509 certificate. </summary>
	/// <returns>
	///   <see cref="F:System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError" /> (value 0) if the certificate verified successfully; otherwise, a bitwise enumeration of error flags.</returns>
	/// <param name="certificate">The X.509 certificate to verify.</param>
	public static X509ChainStatusFlags VerifyCertificate(X509Certificate certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		X509ChainStatusFlags x509ChainStatusFlags = X509ChainStatusFlags.NoError;
		X509Chain x509Chain = new X509Chain();
		if (!x509Chain.Build(new X509Certificate2(certificate.Handle)))
		{
			X509ChainStatus[] chainStatus = x509Chain.ChainStatus;
			if (chainStatus.Length == 0)
			{
				return X509ChainStatusFlags.NotValidForUsage;
			}
			for (int i = 0; i < chainStatus.Length; i++)
			{
				x509ChainStatusFlags |= chainStatus[i].Status;
			}
		}
		return x509ChainStatusFlags;
	}

	internal static X509Certificate PromptForSigningCertificate(nint hwndParent)
	{
		X509Certificate2 result = null;
		X509Store x509Store = new X509Store(StoreLocation.CurrentUser);
		x509Store.Open(OpenFlags.OpenExistingOnly);
		X509Certificate2Collection certificates = x509Store.Certificates;
		certificates = certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, validOnly: true);
		certificates = certificates.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, validOnly: false);
		for (int num = certificates.Count - 1; num >= 0; num--)
		{
			if (!certificates[num].HasPrivateKey)
			{
				certificates.RemoveAt(num);
			}
		}
		if (certificates.Count > 0)
		{
			certificates = X509Certificate2UI.SelectFromCollection(certificates, SR.CertSelectionDialogTitle, SR.CertSelectionDialogMessage, X509SelectionFlag.SingleSelection, hwndParent);
			if (certificates.Count > 0)
			{
				result = certificates[0];
			}
		}
		return result;
	}

	private void VerifyPartsExist(IEnumerable<Uri> parts)
	{
		if (parts == null)
		{
			return;
		}
		foreach (Uri part in parts)
		{
			if (!_container.PartExists(part))
			{
				if (_signatures.Count == 0)
				{
					DeleteOriginPart();
				}
				throw new ArgumentException(SR.PartToSignMissing, "parts");
			}
		}
	}

	private void VerifySignArguments(IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors, string signatureId, IEnumerable<DataObject> signatureObjects, IEnumerable<Reference> objectReferences)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (EnumeratorEmptyCheck(parts) && EnumeratorEmptyCheck(relationshipSelectors) && EnumeratorEmptyCheck(signatureObjects) && EnumeratorEmptyCheck(objectReferences))
		{
			throw new ArgumentException(SR.NothingToSign);
		}
		if (signatureObjects != null)
		{
			List<string> list = new List<string>();
			foreach (DataObject signatureObject in signatureObjects)
			{
				if (string.CompareOrdinal(signatureObject.Id, XTable.Get(XTable.ID.OpcAttrValue)) == 0)
				{
					throw new ArgumentException(SR.SignaturePackageObjectTagMustBeUnique, "signatureObjects");
				}
				if (list.Exists(new StringMatchPredicate(signatureObject.Id).Match))
				{
					throw new ArgumentException(SR.SignatureObjectIdMustBeUnique, "signatureObjects");
				}
				list.Add(signatureObject.Id);
			}
		}
		if (!string.IsNullOrEmpty(signatureId))
		{
			try
			{
				XmlConvert.VerifyNCName(signatureId);
			}
			catch (XmlException innerException)
			{
				throw new ArgumentException(SR.Format(SR.NotAValidXmlIdString, signatureId), "signatureId", innerException);
			}
		}
	}

	private bool EnumeratorEmptyCheck(IEnumerable enumerable)
	{
		if (enumerable == null)
		{
			return true;
		}
		if (enumerable is ICollection collection)
		{
			return collection.Count == 0;
		}
		IEnumerator enumerator = enumerable.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return false;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return true;
	}

	private void InternalRemoveSignature(Uri signatureUri, int countOfSignaturesRemaining)
	{
		if (countOfSignaturesRemaining == 0)
		{
			DeleteOriginPart();
		}
		else
		{
			SafeVisitRelationships(OriginPart.GetRelationshipsByType(_originToSignatureRelationshipType), DeleteRelationshipToSignature, signatureUri);
		}
		SafeVisitRelationships(_container.GetPart(signatureUri).GetRelationshipsByType(CertificatePart.RelationshipType), DeleteCertificateIfReferenceCountBecomesZeroVisitor);
		_container.DeletePart(signatureUri);
	}

	private void SafeVisitRelationships(PackageRelationshipCollection relationships, RelationshipOperation visit)
	{
		SafeVisitRelationships(relationships, visit, null);
	}

	private void SafeVisitRelationships(PackageRelationshipCollection relationships, RelationshipOperation visit, object context)
	{
		List<PackageRelationship> list = new List<PackageRelationship>(relationships);
		for (int i = 0; i < list.Count && visit(list[i], context); i++)
		{
		}
	}

	private bool DeleteCertificateIfReferenceCountBecomesZeroVisitor(PackageRelationship r, object context)
	{
		if (r.TargetMode != 0)
		{
			throw new FileFormatException(SR.PackageSignatureCorruption);
		}
		Uri uri = PackUriHelper.ResolvePartUri(r.SourceUri, r.TargetUri);
		if (CertificatePartReferenceCount(uri) == 1)
		{
			_container.DeletePart(uri);
		}
		return true;
	}

	private bool DeleteRelationshipOfTypePackageToOriginVisitor(PackageRelationship r, object context)
	{
		if (r.TargetMode != 0)
		{
			throw new FileFormatException(SR.PackageSignatureCorruption);
		}
		if (PackUriHelper.ComparePartUri(PackUriHelper.ResolvePartUri(r.SourceUri, r.TargetUri), _originPartName) == 0)
		{
			_container.DeleteRelationship(r.Id);
		}
		return true;
	}

	private bool DeleteRelationshipToSignature(PackageRelationship r, object signatureUri)
	{
		Uri secondPartUri = signatureUri as Uri;
		if (r.TargetMode != 0)
		{
			throw new FileFormatException(SR.PackageSignatureCorruption);
		}
		if (PackUriHelper.ComparePartUri(PackUriHelper.ResolvePartUri(r.SourceUri, r.TargetUri), secondPartUri) == 0)
		{
			OriginPart.DeleteRelationship(r.Id);
		}
		return true;
	}

	private void DeleteOriginPart()
	{
		try
		{
			SafeVisitRelationships(_container.GetRelationshipsByType(_originRelationshipType), DeleteRelationshipOfTypePackageToOriginVisitor);
			_container.DeletePart(_originPartName);
		}
		finally
		{
			_originPartExists = false;
			_originSearchConducted = true;
			_originPart = null;
		}
	}

	private int GetSignatureIndex(Uri uri)
	{
		EnsureSignatures();
		for (int i = 0; i < _signatures.Count; i++)
		{
			if (PackUriHelper.ComparePartUri(uri, _signatures[i].SignaturePart.Uri) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	private int CertificatePartReferenceCount(Uri certificatePartUri)
	{
		int num = 0;
		for (int i = 0; i < _signatures.Count; i++)
		{
			if (_signatures[i].GetCertificatePart() != null && PackUriHelper.ComparePartUri(certificatePartUri, _signatures[i].GetCertificatePart().Uri) == 0)
			{
				num++;
			}
		}
		return num;
	}

	private Uri GenerateSignaturePartName()
	{
		return PackUriHelper.CreatePartUri(new Uri(_defaultSignaturePartNamePrefix + Guid.NewGuid().ToString(_guidStorageFormatString, null) + _defaultSignaturePartNameExtension, UriKind.Relative));
	}

	private void EnsureSignatures()
	{
		if (_signatures != null)
		{
			return;
		}
		_signatures = new List<PackageDigitalSignature>();
		if (!OriginPartExists())
		{
			return;
		}
		foreach (PackageRelationship item2 in _originPart.GetRelationshipsByType(_originToSignatureRelationshipType))
		{
			if (item2.TargetMode != 0)
			{
				throw new FileFormatException(SR.PackageSignatureCorruption);
			}
			Uri partUri = PackUriHelper.ResolvePartUri(_originPart.Uri, item2.TargetUri);
			if (!_container.PartExists(partUri))
			{
				throw new FileFormatException(SR.PackageSignatureCorruption);
			}
			PackagePart part = _container.GetPart(partUri);
			if (part.ValidatedContentType().AreTypeAndSubTypeEqual(XmlDigitalSignatureProcessor.ContentType))
			{
				PackageDigitalSignature item = new PackageDigitalSignature(this, part);
				_signatures.Add(item);
			}
		}
	}

	private bool OriginPartExists()
	{
		if (!_originSearchConducted)
		{
			try
			{
				foreach (PackageRelationship item in _container.GetRelationshipsByType(_originRelationshipType))
				{
					if (item.TargetMode != 0)
					{
						throw new FileFormatException(SR.PackageSignatureCorruption);
					}
					Uri uri = PackUriHelper.ResolvePartUri(item.SourceUri, item.TargetUri);
					if (!_container.PartExists(uri))
					{
						throw new FileFormatException(SR.SignatureOriginNotFound);
					}
					PackagePart part = _container.GetPart(uri);
					if (part.ValidatedContentType().AreTypeAndSubTypeEqual(_originPartContentType))
					{
						if (_originPartExists)
						{
							throw new FileFormatException(SR.MultipleSignatureOrigins);
						}
						_originPartName = uri;
						_originPart = part;
						_originPartExists = true;
					}
				}
			}
			finally
			{
				_originSearchConducted = true;
			}
		}
		return _originPartExists;
	}
}
