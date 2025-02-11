using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows;
using System.Xml;
using MS.Internal.IO.Packaging.Extensions;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal class XmlDigitalSignatureProcessor
{
	private PackagePart _signaturePart;

	private X509Certificate2 _certificate;

	private bool _lookForEmbeddedCert;

	private PackageDigitalSignatureManager _manager;

	private PackageDigitalSignature _signature;

	private SignedXml _signedXml;

	private string _hashAlgorithmName;

	private bool _dataObjectParsed;

	private DateTime _signingTime;

	private string _signingTimeFormat;

	private List<Uri> _partManifest;

	private List<PartManifestEntry> _partEntryManifest;

	private List<PackageRelationshipSelector> _relationshipManifest;

	private static readonly ContentType _xmlSignaturePartType = new ContentType("application/vnd.openxmlformats-package.digital-signature-xmlsignature+xml");

	private static readonly Dictionary<string, string> _rsaSigMethodLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "http://www.w3.org/2001/04/xmlenc#sha256", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" },
		{ "http://www.w3.org/2001/04/xmldsig-more#sha384", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384" },
		{ "http://www.w3.org/2001/04/xmlenc#sha512", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512" }
	};

	internal static ContentType ContentType => _xmlSignaturePartType;

	internal PackagePart SignaturePart => _signaturePart;

	internal List<Uri> PartManifest
	{
		get
		{
			ParsePackageDataObject();
			return _partManifest;
		}
	}

	internal List<PackageRelationshipSelector> RelationshipManifest
	{
		get
		{
			ParsePackageDataObject();
			return _relationshipManifest;
		}
	}

	internal X509Certificate2 Signer
	{
		get
		{
			if (_certificate == null)
			{
				if (PackageSignature.GetCertificatePart() != null)
				{
					_certificate = PackageSignature.GetCertificatePart().GetCertificate();
				}
				else if (_lookForEmbeddedCert)
				{
					IEnumerator enumerator = EnsureXmlSignatureParsed().KeyInfo.GetEnumerator(typeof(KeyInfoX509Data));
					while (enumerator.MoveNext())
					{
						{
							IEnumerator enumerator2 = ((KeyInfoX509Data)enumerator.Current).Certificates.GetEnumerator();
							try
							{
								if (enumerator2.MoveNext())
								{
									X509Certificate2 certificate = (X509Certificate2)enumerator2.Current;
									_certificate = certificate;
								}
							}
							finally
							{
								IDisposable disposable = enumerator2 as IDisposable;
								if (disposable != null)
								{
									disposable.Dispose();
								}
							}
						}
						if (_certificate != null)
						{
							break;
						}
					}
					_lookForEmbeddedCert = false;
				}
			}
			return _certificate;
		}
	}

	internal byte[] SignatureValue => EnsureXmlSignatureParsed().SignatureValue;

	internal Signature Signature
	{
		get
		{
			return EnsureXmlSignatureParsed().Signature;
		}
		set
		{
			UpdatePartFromSignature(value);
		}
	}

	internal PackageDigitalSignature PackageSignature => _signature;

	internal DateTime SigningTime
	{
		get
		{
			ParsePackageDataObject();
			return _signingTime;
		}
	}

	internal string TimeFormat
	{
		get
		{
			ParsePackageDataObject();
			return _signingTimeFormat;
		}
	}

	internal XmlDigitalSignatureProcessor(PackageDigitalSignatureManager manager, PackagePart signaturePart, PackageDigitalSignature packageSignature)
		: this(manager, signaturePart)
	{
		_signature = packageSignature;
	}

	internal static PackageDigitalSignature Sign(PackageDigitalSignatureManager manager, PackagePart signaturePart, IEnumerable<Uri> parts, IEnumerable<PackageRelationshipSelector> relationshipSelectors, X509Certificate2 signer, string signatureId, bool embedCertificate, IEnumerable<DataObject> signatureObjects, IEnumerable<Reference> objectReferences)
	{
		return new XmlDigitalSignatureProcessor(manager, signaturePart).Sign(parts, relationshipSelectors, signer, signatureId, embedCertificate, signatureObjects, objectReferences);
	}

	internal bool Verify()
	{
		return Verify(Signer);
	}

	internal bool Verify(X509Certificate2 signer)
	{
		Invariant.Assert(signer != null);
		SignedXml signedXml = EnsureXmlSignatureParsed();
		bool flag = false;
		ValidateReferences(signedXml.SignedInfo.References, allowPackageSpecificReferences: true);
		flag = signedXml.CheckSignature(signer, verifySignatureOnly: true);
		if (flag)
		{
			HashAlgorithm hashAlgorithm = null;
			string text = string.Empty;
			try
			{
				try
				{
					ParsePackageDataObject();
				}
				catch (XmlException)
				{
					return false;
				}
				foreach (PartManifestEntry item in _partEntryManifest)
				{
					Stream stream = null;
					if (item.IsRelationshipEntry)
					{
						stream = GetRelationshipStream(item);
					}
					else
					{
						PackagePart part = _manager.Package.GetPart(item.Uri);
						if (string.CompareOrdinal(item.ContentType.OriginalString, part.ValidatedContentType().OriginalString) != 0)
						{
							flag = false;
							break;
						}
						stream = part.GetSeekableStream(FileMode.Open, FileAccess.Read);
					}
					using (stream)
					{
						if ((hashAlgorithm != null && !hashAlgorithm.CanReuseTransform) || string.CompareOrdinal(item.HashAlgorithm, text) != 0)
						{
							((IDisposable)hashAlgorithm)?.Dispose();
							text = item.HashAlgorithm;
							hashAlgorithm = GetHashAlgorithm(text);
							if (hashAlgorithm == null)
							{
								flag = false;
								break;
							}
						}
						if (string.CompareOrdinal(GenerateDigestValue(stream, item.Transforms, hashAlgorithm), item.HashValue) != 0)
						{
							flag = false;
							break;
						}
					}
				}
			}
			finally
			{
				((IDisposable)hashAlgorithm)?.Dispose();
			}
		}
		return flag;
	}

	internal List<string> GetPartTransformList(Uri partName)
	{
		ParsePackageDataObject();
		List<string> list = null;
		foreach (PartManifestEntry item in _partEntryManifest)
		{
			if (System.IO.Packaging.PackUriHelper.ComparePartUri(item.Uri, partName) == 0)
			{
				list = item.Transforms;
				break;
			}
		}
		if (list == null)
		{
			list = new List<string>();
		}
		return list;
	}

	internal static string GenerateDigestValue(Stream s, string transformName, HashAlgorithm hashAlgorithm)
	{
		List<string> list = null;
		if (transformName != null)
		{
			list = new List<string>(1);
			list.Add(transformName);
		}
		return GenerateDigestValue(s, list, hashAlgorithm);
	}

	internal static string GenerateDigestValue(Stream s, List<string> transforms, HashAlgorithm hashAlgorithm)
	{
		s.Seek(0L, SeekOrigin.Begin);
		Stream stream = new IgnoreFlushAndCloseStream(s);
		List<Stream> list = null;
		if (transforms != null)
		{
			list = new List<Stream>(transforms.Count);
			list.Add(stream);
			foreach (string transform in transforms)
			{
				if (transform.Length != 0 && string.CompareOrdinal(transform, XTable.Get(XTable.ID.RelationshipsTransformName)) != 0)
				{
					stream = TransformXml(StringToTransform(transform) ?? throw new XmlException(SR.UnsupportedTransformAlgorithm), stream);
					list.Add(stream);
				}
			}
		}
		string result = Convert.ToBase64String(HashStream(hashAlgorithm, stream));
		if (list != null)
		{
			foreach (Stream item in list)
			{
				item.Close();
			}
		}
		return result;
	}

	internal static Stream GenerateRelationshipNodeStream(IEnumerable<System.IO.Packaging.PackageRelationship> relationships)
	{
		Stream stream = new MemoryStream();
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(new IgnoreFlushAndCloseStream(stream), Encoding.UTF8);
		xmlTextWriter.WriteStartElement(XTable.Get(XTable.ID.RelationshipsTagName), PackagingUtilities.RelationshipNamespaceUri);
		InternalRelationshipCollection.WriteRelationshipsAsXml(xmlTextWriter, relationships, alwaysWriteTargetModeAttribute: true, inStreamingProduction: false);
		xmlTextWriter.WriteEndElement();
		return stream;
	}

	internal static HashAlgorithm GetHashAlgorithm(string hashAlgorithmName)
	{
		object obj = CryptoConfig.CreateFromName(hashAlgorithmName);
		HashAlgorithm obj2 = obj as HashAlgorithm;
		if (obj2 == null && obj != null && obj is IDisposable disposable)
		{
			disposable.Dispose();
		}
		return obj2;
	}

	private static Transform StringToTransform(string transformName)
	{
		Invariant.Assert(transformName != null);
		if (string.CompareOrdinal(transformName, "http://www.w3.org/TR/2001/REC-xml-c14n-20010315") == 0)
		{
			return new XmlDsigC14NTransform();
		}
		if (string.CompareOrdinal(transformName, "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments") == 0)
		{
			return new XmlDsigC14NWithCommentsTransform();
		}
		return null;
	}

	internal static bool IsValidXmlCanonicalizationTransform(string transformName)
	{
		Invariant.Assert(transformName != null);
		if (string.CompareOrdinal(transformName, "http://www.w3.org/TR/2001/REC-xml-c14n-20010315") == 0 || string.CompareOrdinal(transformName, "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments") == 0)
		{
			return true;
		}
		return false;
	}

	private SignedXml EnsureXmlSignatureParsed()
	{
		if (_signedXml == null)
		{
			_signedXml = new CustomSignedXml();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.PreserveWhitespace = true;
			using Stream input = SignaturePart.GetSeekableStream();
			using XmlTextReader xmlTextReader = new XmlTextReader(input);
			xmlTextReader.ProhibitDtd = true;
			PackagingUtilities.PerformInitailReadAndVerifyEncoding(xmlTextReader);
			xmlDocument.Load(xmlTextReader);
			XmlNodeList childNodes = xmlDocument.ChildNodes;
			if (childNodes == null || childNodes.Count == 0 || childNodes.Count > 2)
			{
				throw new XmlException(SR.PackageSignatureCorruption);
			}
			XmlNode xmlNode = childNodes[0];
			if (childNodes.Count == 2)
			{
				if (childNodes[0].NodeType != XmlNodeType.XmlDeclaration)
				{
					throw new XmlException(SR.PackageSignatureCorruption);
				}
				xmlNode = childNodes[1];
			}
			if (xmlNode.NodeType != XmlNodeType.Element || string.CompareOrdinal(xmlNode.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") != 0 || string.CompareOrdinal(xmlNode.LocalName, XTable.Get(XTable.ID.SignatureTagName)) != 0)
			{
				throw new XmlException(SR.PackageSignatureCorruption);
			}
			_signedXml.LoadXml((XmlElement)xmlNode);
		}
		if (!IsValidXmlCanonicalizationTransform(_signedXml.SignedInfo.CanonicalizationMethod))
		{
			throw new XmlException(SR.UnsupportedCanonicalizationMethod);
		}
		if (_signedXml.Signature.Id != null)
		{
			try
			{
				XmlConvert.VerifyNCName(_signedXml.Signature.Id);
			}
			catch (XmlException)
			{
				throw new XmlException(SR.PackageSignatureCorruption);
			}
		}
		return _signedXml;
	}

	private XmlDigitalSignatureProcessor(PackageDigitalSignatureManager manager, PackagePart signaturePart)
	{
		Invariant.Assert(manager != null);
		Invariant.Assert(signaturePart != null);
		_signaturePart = signaturePart;
		_manager = manager;
		_lookForEmbeddedCert = true;
	}

	private PackageDigitalSignature Sign(IEnumerable<Uri> parts, IEnumerable<PackageRelationshipSelector> relationshipSelectors, X509Certificate2 signer, string signatureId, bool embedCertificate, IEnumerable<DataObject> signatureObjects, IEnumerable<Reference> objectReferences)
	{
		_hashAlgorithmName = _manager.HashAlgorithm;
		if (_manager.CertificateOption == CertificateEmbeddingOption.NotEmbedded)
		{
			_lookForEmbeddedCert = false;
		}
		else
		{
			_certificate = signer;
		}
		AsymmetricAlgorithm asymmetricAlgorithm = null;
		using (AsymmetricAlgorithm asymmetricAlgorithm2 = ((!signer.HasPrivateKey) ? GetPrivateKeyForSigning(signer) : GetPrivateKey(signer)))
		{
			_signedXml = new CustomSignedXml();
			_signedXml.SigningKey = asymmetricAlgorithm2;
			_signedXml.Signature.Id = signatureId;
			if (BaseCompatibilityPreferences.MatchPackageSignatureMethodToPackagePartDigestMethod)
			{
				_signedXml.SignedInfo.SignatureMethod = SelectSignatureMethod(asymmetricAlgorithm2);
			}
			bool flag = _signedXml.SignedInfo.SignatureMethod != null;
			if (embedCertificate)
			{
				_signedXml.KeyInfo = GenerateKeyInfo(asymmetricAlgorithm2, signer);
			}
			using (HashAlgorithm hashAlgorithm = GetHashAlgorithm(_hashAlgorithmName))
			{
				if (hashAlgorithm == null)
				{
					throw new InvalidOperationException(SR.UnsupportedHashAlgorithm);
				}
				_signedXml.AddObject(GenerateObjectTag(hashAlgorithm, parts, relationshipSelectors, signatureId));
			}
			Reference reference = new Reference(XTable.Get(XTable.ID.OpcLinkAttrValue));
			reference.Type = XTable.Get(XTable.ID.W3CSignatureNamespaceRoot) + "Object";
			reference.DigestMethod = _hashAlgorithmName;
			_signedXml.AddReference(reference);
			AddCustomObjectTags(signatureObjects, objectReferences);
			SignedXml signedXml = _signedXml;
			try
			{
				signedXml.ComputeSignature();
			}
			catch (CryptographicException) when (flag)
			{
				BaseCompatibilityPreferences.MatchPackageSignatureMethodToPackagePartDigestMethod = false;
				signedXml.SignedInfo.SignatureMethod = null;
				signedXml.ComputeSignature();
			}
			UpdatePartFromSignature(_signedXml.Signature);
		}
		_signature = new PackageDigitalSignature(_manager, this);
		return _signature;
	}

	private string SelectSignatureMethod(AsymmetricAlgorithm key)
	{
		string value = null;
		if (key is RSA)
		{
			_rsaSigMethodLookup.TryGetValue(_manager.HashAlgorithm, out value);
		}
		return value;
	}

	private static AsymmetricAlgorithm GetPrivateKey(X509Certificate2 cert)
	{
		AsymmetricAlgorithm rSAPrivateKey = cert.GetRSAPrivateKey();
		if (rSAPrivateKey != null)
		{
			return rSAPrivateKey;
		}
		rSAPrivateKey = cert.GetDSAPrivateKey();
		if (rSAPrivateKey != null)
		{
			return rSAPrivateKey;
		}
		rSAPrivateKey = cert.GetECDsaPrivateKey();
		if (rSAPrivateKey != null)
		{
			return rSAPrivateKey;
		}
		return cert.PrivateKey;
	}

	private Stream GetRelationshipStream(PartManifestEntry partEntry)
	{
		SortedDictionary<string, System.IO.Packaging.PackageRelationship> sortedDictionary = new SortedDictionary<string, System.IO.Packaging.PackageRelationship>(StringComparer.Ordinal);
		foreach (PackageRelationshipSelector relationshipSelector in partEntry.RelationshipSelectors)
		{
			foreach (System.IO.Packaging.PackageRelationship item in relationshipSelector.Select(_manager.Package))
			{
				if (!sortedDictionary.ContainsKey(item.Id))
				{
					sortedDictionary.Add(item.Id, item);
				}
			}
		}
		return GenerateRelationshipNodeStream(sortedDictionary.Values);
	}

	private void AddCustomObjectTags(IEnumerable<DataObject> signatureObjects, IEnumerable<Reference> objectReferences)
	{
		Invariant.Assert(_signedXml != null);
		if (objectReferences != null)
		{
			ValidateReferences(objectReferences, allowPackageSpecificReferences: false);
			foreach (Reference objectReference in objectReferences)
			{
				objectReference.DigestMethod = _hashAlgorithmName;
				_signedXml.AddReference(objectReference);
			}
		}
		if (signatureObjects == null)
		{
			return;
		}
		foreach (DataObject signatureObject in signatureObjects)
		{
			_signedXml.AddObject(signatureObject);
		}
	}

	private void UpdatePartFromSignature(Signature sig)
	{
		using (Stream w = SignaturePart.GetSeekableStream(FileMode.Create, FileAccess.Write))
		{
			using XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.UTF8);
			xmlTextWriter.WriteStartDocument(standalone: true);
			sig.GetXml().WriteTo(xmlTextWriter);
			xmlTextWriter.WriteEndDocument();
		}
		_signedXml = null;
	}

	private static byte[] HashStream(HashAlgorithm hashAlgorithm, Stream s)
	{
		s.Seek(0L, SeekOrigin.Begin);
		hashAlgorithm.Initialize();
		return hashAlgorithm.ComputeHash(s);
	}

	private static Stream TransformXml(Transform xForm, object source)
	{
		xForm.LoadInput(source);
		return (Stream)xForm.GetOutput();
	}

	private void ParsePackageDataObject()
	{
		if (_dataObjectParsed)
		{
			return;
		}
		EnsureXmlSignatureParsed();
		XmlNodeList data = GetPackageDataObject().Data;
		if (data.Count != 2)
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		XmlReader xmlReader = new XmlNodeReader(data[0].ParentNode);
		xmlReader.Read();
		if (string.CompareOrdinal(xmlReader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") != 0)
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		string strB = XTable.Get(XTable.ID.SignaturePropertiesTagName);
		string strB2 = XTable.Get(XTable.ID.ManifestTagName);
		bool flag = false;
		bool flag2 = false;
		while (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Element)
		{
			if (xmlReader.MoveToContent() == XmlNodeType.Element && string.CompareOrdinal(xmlReader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") == 0 && xmlReader.Depth == 1)
			{
				if (!flag && string.CompareOrdinal(xmlReader.LocalName, strB) == 0)
				{
					flag = true;
					_signingTime = XmlSignatureProperties.ParseSigningTime(xmlReader, _signedXml.Signature.Id, out _signingTimeFormat);
					continue;
				}
				if (!flag2 && string.CompareOrdinal(xmlReader.LocalName, strB2) == 0)
				{
					flag2 = true;
					XmlSignatureManifest.ParseManifest(_manager, xmlReader, out _partManifest, out _partEntryManifest, out _relationshipManifest);
					continue;
				}
			}
			throw new XmlException(SR.XmlSignatureParseError);
		}
		if (!(flag && flag2))
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		_dataObjectParsed = true;
	}

	private DataObject GetPackageDataObject()
	{
		EnsureXmlSignatureParsed();
		string strB = XTable.Get(XTable.ID.OpcAttrValue);
		DataObject dataObject = null;
		foreach (DataObject @object in _signedXml.Signature.ObjectList)
		{
			if (string.CompareOrdinal(@object.Id, strB) == 0)
			{
				if (dataObject != null)
				{
					throw new XmlException(SR.SignatureObjectIdMustBeUnique);
				}
				dataObject = @object;
			}
		}
		if (dataObject != null)
		{
			return dataObject;
		}
		throw new XmlException(SR.PackageSignatureObjectTagRequired);
	}

	private KeyInfo GenerateKeyInfo(AsymmetricAlgorithm key, X509Certificate2 signer)
	{
		KeyInfo keyInfo = new KeyInfo();
		KeyInfoName keyInfoName = new KeyInfoName();
		keyInfoName.Value = signer.Subject;
		keyInfo.AddClause(keyInfoName);
		if (key is RSA)
		{
			keyInfo.AddClause(new RSAKeyValue((RSA)key));
		}
		else
		{
			if (!(key is DSA))
			{
				throw new ArgumentException(SR.CertificateKeyTypeNotSupported, "signer");
			}
			keyInfo.AddClause(new DSAKeyValue((DSA)key));
		}
		keyInfo.AddClause(new KeyInfoX509Data(signer));
		return keyInfo;
	}

	private DataObject GenerateObjectTag(HashAlgorithm hashAlgorithm, IEnumerable<Uri> parts, IEnumerable<PackageRelationshipSelector> relationshipSelectors, string signatureId)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "root", "namespace"));
		xmlDocument.DocumentElement.AppendChild(XmlSignatureManifest.GenerateManifest(_manager, xmlDocument, hashAlgorithm, parts, relationshipSelectors));
		xmlDocument.DocumentElement.AppendChild(XmlSignatureProperties.AssembleSignatureProperties(xmlDocument, DateTime.Now, _manager.TimeFormat, signatureId));
		return new DataObject
		{
			Data = xmlDocument.DocumentElement.ChildNodes,
			Id = XTable.Get(XTable.ID.OpcAttrValue)
		};
	}

	private static AsymmetricAlgorithm GetPrivateKeyForSigning(X509Certificate2 signer)
	{
		Invariant.Assert(!signer.HasPrivateKey);
		X509Store x509Store = new X509Store(StoreLocation.CurrentUser);
		try
		{
			x509Store.Open(OpenFlags.OpenExistingOnly);
			X509Certificate2Collection certificates = x509Store.Certificates;
			certificates = certificates.Find(X509FindType.FindByThumbprint, signer.Thumbprint, validOnly: true);
			if (certificates.Count <= 0)
			{
				throw new CryptographicException(SR.DigSigCannotLocateCertificate);
			}
			if (certificates.Count > 1)
			{
				throw new CryptographicException(SR.DigSigDuplicateCertificate);
			}
			signer = certificates[0];
		}
		finally
		{
			x509Store.Close();
		}
		return GetPrivateKey(signer);
	}

	private void ValidateReferences(IEnumerable references, bool allowPackageSpecificReferences)
	{
		bool flag = false;
		foreach (Reference reference in references)
		{
			if (reference.Uri.StartsWith("#", StringComparison.Ordinal))
			{
				if (string.CompareOrdinal(reference.Uri, XTable.Get(XTable.ID.OpcLinkAttrValue)) == 0)
				{
					if (!allowPackageSpecificReferences)
					{
						throw new ArgumentException(SR.PackageSpecificReferenceTagMustBeUnique);
					}
					if (flag)
					{
						throw new XmlException(SR.MoreThanOnePackageSpecificReference);
					}
					flag = true;
				}
				TransformChain transformChain = reference.TransformChain;
				for (int i = 0; i < transformChain.Count; i++)
				{
					if (!IsValidXmlCanonicalizationTransform(transformChain[i].Algorithm))
					{
						throw new XmlException(SR.UnsupportedTransformAlgorithm);
					}
				}
				continue;
			}
			throw new XmlException(SR.InvalidUriAttribute);
		}
		if (allowPackageSpecificReferences && !flag)
		{
			throw new XmlException(SR.PackageSignatureReferenceTagRequired);
		}
	}
}
