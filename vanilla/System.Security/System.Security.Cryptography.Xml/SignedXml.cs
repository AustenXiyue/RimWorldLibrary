using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Unity;

namespace System.Security.Cryptography.Xml;

/// <summary>Provides a wrapper on a core XML signature object to facilitate creating XML signatures.</summary>
public class SignedXml
{
	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard namespace for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigNamespaceUrl = "http://www.w3.org/2000/09/xmldsig#";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard minimal canonicalization algorithm for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigMinimalCanonicalizationUrl = "http://www.w3.org/2000/09/xmldsig#minimal";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard canonicalization algorithm for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigCanonicalizationUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard canonicalization algorithm for XML digital signatures and includes comments. This field is constant.</summary>
	public const string XmlDsigCanonicalizationWithCommentsUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard <see cref="T:System.Security.Cryptography.SHA1" /> digest method for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigSHA1Url = "http://www.w3.org/2000/09/xmldsig#sha1";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard <see cref="T:System.Security.Cryptography.DSA" /> algorithm for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigDSAUrl = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard <see cref="T:System.Security.Cryptography.RSA" /> signature method for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigRSASHA1Url = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the standard <see cref="T:System.Security.Cryptography.HMACSHA1" /> algorithm for XML digital signatures. This field is constant.</summary>
	public const string XmlDsigHMACSHA1Url = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";

	public const string XmlDsigSHA256Url = "http://www.w3.org/2001/04/xmlenc#sha256";

	public const string XmlDsigRSASHA256Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

	public const string XmlDsigSHA384Url = "http://www.w3.org/2001/04/xmldsig-more#sha384";

	public const string XmlDsigRSASHA384Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";

	public const string XmlDsigSHA512Url = "http://www.w3.org/2001/04/xmlenc#sha512";

	public const string XmlDsigRSASHA512Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the Canonical XML transformation. This field is constant.</summary>
	public const string XmlDsigC14NTransformUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the Canonical XML transformation, with comments. This field is constant.</summary>
	public const string XmlDsigC14NWithCommentsTransformUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";

	/// <summary>Represents the Uniform Resource Identifier (URI) for exclusive XML canonicalization. This field is constant.</summary>
	public const string XmlDsigExcC14NTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#";

	/// <summary>Represents the Uniform Resource Identifier (URI) for exclusive XML canonicalization, with comments. This field is constant.</summary>
	public const string XmlDsigExcC14NWithCommentsTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the base 64 transformation. This field is constant.</summary>
	public const string XmlDsigBase64TransformUrl = "http://www.w3.org/2000/09/xmldsig#base64";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the XML Path Language (XPath). This field is constant.</summary>
	public const string XmlDsigXPathTransformUrl = "http://www.w3.org/TR/1999/REC-xpath-19991116";

	/// <summary>Represents the Uniform Resource Identifier (URI) for XSLT transformations. This field is constant.</summary>
	public const string XmlDsigXsltTransformUrl = "http://www.w3.org/TR/1999/REC-xslt-19991116";

	/// <summary>Represents the Uniform Resource Identifier (URI) for enveloped signature transformation. This field is constant.</summary>
	public const string XmlDsigEnvelopedSignatureTransformUrl = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the XML mode decryption transformation. This field is constant.</summary>
	public const string XmlDecryptionTransformUrl = "http://www.w3.org/2002/07/decrypt#XML";

	/// <summary>Represents the Uniform Resource Identifier (URI) for the license transform algorithm used to normalize XrML licenses for signatures.</summary>
	public const string XmlLicenseTransformUrl = "urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform";

	private EncryptedXml encryptedXml;

	/// <summary>Represents the <see cref="T:System.Security.Cryptography.Xml.Signature" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object. </summary>
	protected Signature m_signature;

	private AsymmetricAlgorithm key;

	/// <summary>Represents the name of the installed key to be used for signing the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object. </summary>
	protected string m_strSigningKeyName;

	private XmlDocument envdoc;

	private IEnumerator pkEnumerator;

	private XmlElement signatureElement;

	private Hashtable hashes;

	internal XmlResolver _xmlResolver = new XmlUrlResolver();

	private bool _bResolverSet = true;

	internal XmlElement _context;

	private ArrayList manifests;

	private IEnumerator _x509Enumerator;

	private static readonly char[] whitespaceChars = new char[4] { ' ', '\r', '\n', '\t' };

	/// <summary>Gets or sets an <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object that defines the XML encryption processing rules.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object that defines the XML encryption processing rules.</returns>
	[ComVisible(false)]
	public EncryptedXml EncryptedXml
	{
		get
		{
			return encryptedXml;
		}
		set
		{
			encryptedXml = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public KeyInfo KeyInfo
	{
		get
		{
			if (m_signature.KeyInfo == null)
			{
				m_signature.KeyInfo = new KeyInfo();
			}
			return m_signature.KeyInfo;
		}
		set
		{
			m_signature.KeyInfo = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.Xml.Signature" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.Signature" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public Signature Signature => m_signature;

	/// <summary>Gets the length of the signature for the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The length of the signature for the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public string SignatureLength => m_signature.SignedInfo.SignatureLength;

	/// <summary>Gets the signature method of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The signature method of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public string SignatureMethod => m_signature.SignedInfo.SignatureMethod;

	/// <summary>Gets the signature value of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>A byte array that contains the signature value of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public byte[] SignatureValue => m_signature.SignatureValue;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object of the current <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public SignedInfo SignedInfo => m_signature.SignedInfo;

	/// <summary>Gets or sets the asymmetric algorithm key used for signing a <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The asymmetric algorithm key used for signing the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public AsymmetricAlgorithm SigningKey
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	/// <summary>Gets or sets the name of the installed key to be used for signing the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The name of the installed key to be used for signing the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</returns>
	public string SigningKeyName
	{
		get
		{
			return m_strSigningKeyName;
		}
		set
		{
			m_strSigningKeyName = value;
		}
	}

	/// <summary>Sets the current <see cref="T:System.Xml.XmlResolver" /> object.</summary>
	/// <returns>The current <see cref="T:System.Xml.XmlResolver" /> object. The defaults is a <see cref="T:System.Xml.XmlSecureResolver" /> object.</returns>
	public XmlResolver Resolver
	{
		set
		{
			_xmlResolver = value;
			_bResolverSet = true;
		}
	}

	internal bool ResolverSet => _bResolverSet;

	/// <summary>Gets the names of methods whose canonicalization algorithms are explicitly allowed. </summary>
	/// <returns>A collection of the names of methods that safely produce canonical XML. </returns>
	public Collection<string> SafeCanonicalizationMethods
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (Collection<string>)0;
		}
	}

	/// <summary>Gets a delegate that will be called to validate the format (not the cryptographic security) of an XML signature.</summary>
	/// <returns>true if the format is acceptable; otherwise, false.</returns>
	public Func<SignedXml, bool> SignatureFormatValidator
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (Func<SignedXml, bool>)0;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> class.</summary>
	public SignedXml()
	{
		m_signature = new Signature();
		m_signature.SignedInfo = new SignedInfo();
		hashes = new Hashtable(2);
		_context = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> class from the specified XML document.</summary>
	/// <param name="document">The <see cref="T:System.Xml.XmlDocument" /> object to use to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.SignedXml" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="document" /> parameter is null.-or-The <paramref name="document" /> parameter contains a null <see cref="P:System.Xml.XmlDocument.DocumentElement" /> property.</exception>
	public SignedXml(XmlDocument document)
		: this()
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		envdoc = document;
		_context = document.DocumentElement;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> class from the specified <see cref="T:System.Xml.XmlElement" /> object.</summary>
	/// <param name="elem">The <see cref="T:System.Xml.XmlElement" /> object to use to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.SignedXml" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="elem" /> parameter is null. </exception>
	public SignedXml(XmlElement elem)
		: this()
	{
		if (elem == null)
		{
			throw new ArgumentNullException("elem");
		}
		envdoc = new XmlDocument();
		_context = elem;
		envdoc.LoadXml(elem.OuterXml);
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object to the list of objects to be signed.</summary>
	/// <param name="dataObject">The <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object to add to the list of objects to be signed. </param>
	public void AddObject(DataObject dataObject)
	{
		m_signature.AddObject(dataObject);
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.Reference" /> object to the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object that describes a digest method, digest value, and transform to use for creating an XML digital signature.</summary>
	/// <param name="reference">The  <see cref="T:System.Security.Cryptography.Xml.Reference" /> object that describes a digest method, digest value, and transform to use for creating an XML digital signature.</param>
	public void AddReference(Reference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		m_signature.SignedInfo.AddReference(reference);
	}

	private Stream ApplyTransform(Transform t, XmlDocument input)
	{
		if (t is XmlDsigXPathTransform || t is XmlDsigEnvelopedSignatureTransform || t is XmlDecryptionTransform)
		{
			input = (XmlDocument)input.Clone();
		}
		t.LoadInput(input);
		if (t is XmlDsigEnvelopedSignatureTransform)
		{
			return CanonicalizeOutput(t.GetOutput());
		}
		object output = t.GetOutput();
		if (output is Stream)
		{
			return (Stream)output;
		}
		if (output is XmlDocument)
		{
			MemoryStream memoryStream = new MemoryStream();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
			((XmlDocument)output).WriteTo(xmlTextWriter);
			xmlTextWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		if (output == null)
		{
			throw new NotImplementedException(string.Concat("This should not occur. Transform is ", t, "."));
		}
		return CanonicalizeOutput(output);
	}

	private Stream CanonicalizeOutput(object obj)
	{
		Transform c14NMethod = GetC14NMethod();
		c14NMethod.LoadInput(obj);
		return (Stream)c14NMethod.GetOutput();
	}

	private XmlDocument GetManifest(Reference r)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		if (r.Uri[0] == '#')
		{
			if (signatureElement != null)
			{
				XmlElement idElement = GetIdElement(signatureElement.OwnerDocument, r.Uri.Substring(1));
				if (idElement == null)
				{
					throw new CryptographicException("Manifest targeted by Reference was not found: " + r.Uri.Substring(1));
				}
				xmlDocument.AppendChild(xmlDocument.ImportNode(idElement, deep: true));
				FixupNamespaceNodes(idElement, xmlDocument.DocumentElement, ignoreDefault: false);
			}
		}
		else if (_xmlResolver != null)
		{
			Stream inStream = (Stream)_xmlResolver.GetEntity(new Uri(r.Uri), null, typeof(Stream));
			xmlDocument.Load(inStream);
		}
		if (xmlDocument.FirstChild != null)
		{
			if (manifests == null)
			{
				manifests = new ArrayList();
			}
			manifests.Add(xmlDocument);
			return xmlDocument;
		}
		return null;
	}

	private void FixupNamespaceNodes(XmlElement src, XmlElement dst, bool ignoreDefault)
	{
		foreach (XmlAttribute item in src.SelectNodes("namespace::*"))
		{
			if (!(item.LocalName == "xml") && (!ignoreDefault || !(item.LocalName == "xmlns")))
			{
				dst.SetAttributeNode(dst.OwnerDocument.ImportNode(item, deep: true) as XmlAttribute);
			}
		}
	}

	private byte[] GetReferenceHash(Reference r, bool check_hmac)
	{
		Stream stream = null;
		XmlDocument xmlDocument = null;
		if (r.Uri == string.Empty)
		{
			xmlDocument = envdoc;
		}
		else if (r.Type == "http://www.w3.org/2000/09/xmldsig#Manifest")
		{
			xmlDocument = GetManifest(r);
		}
		else
		{
			xmlDocument = new XmlDocument();
			xmlDocument.PreserveWhitespace = true;
			string text = null;
			if (r.Uri.StartsWith("#xpointer"))
			{
				string text2 = string.Join("", r.Uri.Substring(9).Split(whitespaceChars));
				text2 = ((text2.Length >= 2 && text2[0] == '(' && text2[text2.Length - 1] == ')') ? text2.Substring(1, text2.Length - 2) : string.Empty);
				if (text2 == "/")
				{
					xmlDocument = envdoc;
				}
				else if (text2.Length > 6 && text2.StartsWith("id(") && text2[text2.Length - 1] == ')')
				{
					text = text2.Substring(4, text2.Length - 6);
				}
			}
			else if (r.Uri[0] == '#')
			{
				text = r.Uri.Substring(1);
			}
			else if (_xmlResolver != null)
			{
				try
				{
					Uri absoluteUri = new Uri(r.Uri);
					stream = (Stream)_xmlResolver.GetEntity(absoluteUri, null, typeof(Stream));
				}
				catch
				{
					stream = File.OpenRead(r.Uri);
				}
			}
			if (text != null)
			{
				XmlElement xmlElement = null;
				foreach (DataObject @object in m_signature.ObjectList)
				{
					if (!(@object.Id == text))
					{
						continue;
					}
					xmlElement = @object.GetXml();
					xmlElement.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
					xmlDocument.AppendChild(xmlDocument.ImportNode(xmlElement, deep: true));
					foreach (XmlNode childNode in xmlElement.ChildNodes)
					{
						if (childNode.NodeType == XmlNodeType.Element)
						{
							FixupNamespaceNodes(childNode as XmlElement, xmlDocument.DocumentElement, ignoreDefault: true);
						}
					}
					break;
				}
				if (xmlElement == null && envdoc != null)
				{
					xmlElement = GetIdElement(envdoc, text);
					if (xmlElement != null)
					{
						xmlDocument.AppendChild(xmlDocument.ImportNode(xmlElement, deep: true));
						FixupNamespaceNodes(xmlElement, xmlDocument.DocumentElement, ignoreDefault: false);
					}
				}
				if (xmlElement == null)
				{
					throw new CryptographicException($"Malformed reference object: {text}");
				}
			}
		}
		if (r.TransformChain.Count > 0)
		{
			foreach (Transform item in r.TransformChain)
			{
				if (stream == null)
				{
					stream = ApplyTransform(item, xmlDocument);
					continue;
				}
				item.LoadInput(stream);
				object output = item.GetOutput();
				stream = ((!(output is Stream)) ? CanonicalizeOutput(output) : ((Stream)output));
			}
		}
		else if (stream == null)
		{
			if (r.Uri[0] != '#')
			{
				stream = new MemoryStream();
				xmlDocument.Save(stream);
			}
			else
			{
				stream = ApplyTransform(new XmlDsigC14NTransform(), xmlDocument);
			}
		}
		return GetHash(r.DigestMethod, check_hmac)?.ComputeHash(stream);
	}

	private void DigestReferences()
	{
		foreach (Reference reference in m_signature.SignedInfo.References)
		{
			if (reference.DigestMethod == null)
			{
				reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
			}
			reference.DigestValue = GetReferenceHash(reference, check_hmac: false);
		}
	}

	private Transform GetC14NMethod()
	{
		return ((Transform)CryptoConfig.CreateFromName(m_signature.SignedInfo.CanonicalizationMethod)) ?? throw new CryptographicException("Unknown Canonicalization Method {0}", m_signature.SignedInfo.CanonicalizationMethod);
	}

	private Stream SignedInfoTransformed()
	{
		Transform c14NMethod = GetC14NMethod();
		if (signatureElement == null)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.PreserveWhitespace = true;
			xmlDocument.LoadXml(m_signature.SignedInfo.GetXml().OuterXml);
			if (envdoc != null)
			{
				foreach (XmlAttribute item in envdoc.DocumentElement.SelectNodes("namespace::*"))
				{
					if (!(item.LocalName == "xml") && !(item.Prefix == xmlDocument.DocumentElement.Prefix))
					{
						xmlDocument.DocumentElement.SetAttributeNode(xmlDocument.ImportNode(item, deep: true) as XmlAttribute);
					}
				}
			}
			c14NMethod.LoadInput(xmlDocument);
		}
		else
		{
			XmlElement xmlElement = signatureElement.GetElementsByTagName("SignedInfo", "http://www.w3.org/2000/09/xmldsig#")[0] as XmlElement;
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlTextWriter.WriteStartElement(xmlElement.Prefix, xmlElement.LocalName, xmlElement.NamespaceURI);
			foreach (XmlAttribute item2 in xmlElement.SelectNodes("namespace::*"))
			{
				if (item2.ParentNode != xmlElement && !(item2.LocalName == "xml") && !(item2.Prefix == xmlElement.Prefix))
				{
					item2.WriteTo(xmlTextWriter);
				}
			}
			foreach (XmlNode attribute in xmlElement.Attributes)
			{
				attribute.WriteTo(xmlTextWriter);
			}
			foreach (XmlNode childNode in xmlElement.ChildNodes)
			{
				childNode.WriteTo(xmlTextWriter);
			}
			xmlTextWriter.WriteEndElement();
			byte[] bytes = Encoding.UTF8.GetBytes(stringWriter.ToString());
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(bytes, 0, bytes.Length);
			memoryStream.Position = 0L;
			c14NMethod.LoadInput(memoryStream);
		}
		return (Stream)c14NMethod.GetOutput();
	}

	private HashAlgorithm GetHash(string algorithm, bool check_hmac)
	{
		HashAlgorithm hashAlgorithm = (HashAlgorithm)hashes[algorithm];
		if (hashAlgorithm == null)
		{
			hashAlgorithm = HashAlgorithm.Create(algorithm);
			if (hashAlgorithm == null)
			{
				throw new CryptographicException("Unknown hash algorithm: {0}", algorithm);
			}
			hashes.Add(algorithm, hashAlgorithm);
		}
		else
		{
			hashAlgorithm.Initialize();
		}
		if (check_hmac && hashAlgorithm is KeyedHashAlgorithm)
		{
			return null;
		}
		return hashAlgorithm;
	}

	/// <summary>Determines whether the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies using the public key in the signature.</summary>
	/// <returns>true if the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies; otherwise, false.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.AsymmetricAlgorithm.SignatureAlgorithm" /> property of the public key in the signature does not match the <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignatureMethod" /> property.-or- The signature description could not be created.-or The hash algorithm could not be created. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool CheckSignature()
	{
		return CheckSignatureInternal(null) != null;
	}

	private bool CheckReferenceIntegrity(ArrayList referenceList)
	{
		if (referenceList == null)
		{
			return false;
		}
		foreach (Reference reference in referenceList)
		{
			byte[] referenceHash = GetReferenceHash(reference, check_hmac: true);
			if (!Compare(reference.DigestValue, referenceHash))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Determines whether the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies for the specified key.</summary>
	/// <returns>true if the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies for the specified key; otherwise, false.</returns>
	/// <param name="key">The implementation of the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> property that holds the key to be used to verify the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.AsymmetricAlgorithm.SignatureAlgorithm" /> property of the <paramref name="key" /> parameter does not match the <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignatureMethod" /> property.-or- The signature description could not be created.-or The hash algorithm could not be created. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool CheckSignature(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return CheckSignatureInternal(key) != null;
	}

	private AsymmetricAlgorithm CheckSignatureInternal(AsymmetricAlgorithm key)
	{
		pkEnumerator = null;
		if (key != null)
		{
			if (!CheckSignatureWithKey(key))
			{
				return null;
			}
		}
		else
		{
			if (Signature.KeyInfo == null)
			{
				return null;
			}
			while ((key = GetPublicKey()) != null && !CheckSignatureWithKey(key))
			{
			}
			pkEnumerator = null;
			if (key == null)
			{
				return null;
			}
		}
		if (!CheckReferenceIntegrity(m_signature.SignedInfo.References))
		{
			return null;
		}
		if (manifests != null)
		{
			for (int i = 0; i < manifests.Count; i++)
			{
				Manifest manifest = new Manifest((manifests[i] as XmlDocument).DocumentElement);
				if (!CheckReferenceIntegrity(manifest.References))
				{
					return null;
				}
			}
		}
		return key;
	}

	private bool CheckSignatureWithKey(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			return false;
		}
		SignatureDescription signatureDescription = (SignatureDescription)CryptoConfig.CreateFromName(m_signature.SignedInfo.SignatureMethod);
		if (signatureDescription == null)
		{
			return false;
		}
		AsymmetricSignatureDeformatter asymmetricSignatureDeformatter = (AsymmetricSignatureDeformatter)CryptoConfig.CreateFromName(signatureDescription.DeformatterAlgorithm);
		if (asymmetricSignatureDeformatter == null)
		{
			return false;
		}
		try
		{
			asymmetricSignatureDeformatter.SetKey(key);
			asymmetricSignatureDeformatter.SetHashAlgorithm(signatureDescription.DigestAlgorithm);
			HashAlgorithm hash = GetHash(signatureDescription.DigestAlgorithm, check_hmac: true);
			MemoryStream inputStream = (MemoryStream)SignedInfoTransformed();
			byte[] rgbHash = hash.ComputeHash(inputStream);
			return asymmetricSignatureDeformatter.VerifySignature(rgbHash, m_signature.SignatureValue);
		}
		catch
		{
			return false;
		}
	}

	private bool Compare(byte[] expected, byte[] actual)
	{
		bool flag = expected != null && actual != null;
		if (flag)
		{
			int num = expected.Length;
			flag = num == actual.Length;
			if (flag)
			{
				for (int i = 0; i < num; i++)
				{
					if (expected[i] != actual[i])
					{
						return false;
					}
				}
			}
		}
		return flag;
	}

	/// <summary>Determines whether the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies for the specified message authentication code (MAC) algorithm.</summary>
	/// <returns>true if the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies for the specified MAC; otherwise, false.</returns>
	/// <param name="macAlg">The implementation of <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> that holds the MAC to be used to verify the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="macAlg" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.HashAlgorithm.HashSize" /> property of the specified <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> object is not valid.-or- The <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property is null.-or- The cryptographic transform used to check the signature could not be created. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool CheckSignature(KeyedHashAlgorithm macAlg)
	{
		if (macAlg == null)
		{
			throw new ArgumentNullException("macAlg");
		}
		pkEnumerator = null;
		Stream stream = SignedInfoTransformed();
		if (stream == null)
		{
			return false;
		}
		byte[] array = macAlg.ComputeHash(stream);
		if (m_signature.SignedInfo.SignatureLength != null)
		{
			int num = int.Parse(m_signature.SignedInfo.SignatureLength);
			if ((num & 7) != 0)
			{
				throw new CryptographicException("Signature length must be a multiple of 8 bits.");
			}
			num >>= 3;
			if (num != m_signature.SignatureValue.Length)
			{
				throw new CryptographicException("Invalid signature length.");
			}
			int num2 = Math.Max(10, array.Length / 2);
			if (num < num2)
			{
				throw new CryptographicException("HMAC signature is too small");
			}
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Buffer.BlockCopy(array, 0, array2, 0, num);
				array = array2;
			}
		}
		if (Compare(m_signature.SignatureValue, array))
		{
			return CheckReferenceIntegrity(m_signature.SignedInfo.References);
		}
		return false;
	}

	/// <summary>Determines whether the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies for the specified <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object and, optionally, whether the certificate is valid.</summary>
	/// <returns>true if the signature is valid; otherwise, false. -or-true if the signature and certificate are valid; otherwise, false. </returns>
	/// <param name="certificate">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object to use to verify the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property.</param>
	/// <param name="verifySignatureOnly">true to verify the signature only; false to verify both the signature and certificate.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="certificate" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A signature description could not be created for the <paramref name="certificate" /> parameter.</exception>
	[System.MonoTODO]
	[ComVisible(false)]
	public bool CheckSignature(X509Certificate2 certificate, bool verifySignatureOnly)
	{
		throw new NotImplementedException();
	}

	/// <summary>Determines whether the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies using the public key in the signature.</summary>
	/// <returns>true if the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property verifies using the public key in the signature; otherwise, false.</returns>
	/// <param name="signingKey">When this method returns, contains the implementation of <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> that holds the public key in the signature. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="signingKey" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.AsymmetricAlgorithm.SignatureAlgorithm" /> property of the public key in the signature does not match the <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignatureMethod" /> property.-or- The signature description could not be created.-or The hash algorithm could not be created. </exception>
	public bool CheckSignatureReturningKey(out AsymmetricAlgorithm signingKey)
	{
		signingKey = CheckSignatureInternal(null);
		return signingKey != null;
	}

	/// <summary>Computes an XML digital signature.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.SignedXml.SigningKey" /> property is null.-or- The <see cref="P:System.Security.Cryptography.Xml.SignedXml.SigningKey" /> property is not a <see cref="T:System.Security.Cryptography.DSA" /> object or <see cref="T:System.Security.Cryptography.RSA" /> object.-or- The key could not be loaded. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void ComputeSignature()
	{
		DigestReferences();
		if (key == null)
		{
			throw new CryptographicException("Signing key is not loaded.");
		}
		if (SignedInfo.SignatureMethod == null)
		{
			if (key is DSA)
			{
				SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
			}
			else
			{
				if (!(key is RSA))
				{
					throw new CryptographicException("Failed to create signing key.");
				}
				SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
			}
		}
		SignatureDescription obj = (CryptoConfig.CreateFromName(SignedInfo.SignatureMethod) as SignatureDescription) ?? throw new CryptographicException("SignatureDescription could not be created for the signature algorithm supplied.");
		HashAlgorithm hashAlgorithm = obj.CreateDigest();
		if (hashAlgorithm == null)
		{
			throw new CryptographicException("Could not create hash algorithm object.");
		}
		hashAlgorithm.ComputeHash(SignedInfoTransformed());
		AsymmetricSignatureFormatter asymmetricSignatureFormatter = obj.CreateFormatter(key);
		m_signature.SignatureValue = asymmetricSignatureFormatter.CreateSignature(hashAlgorithm);
	}

	/// <summary>Computes an XML digital signature using the specified message authentication code (MAC) algorithm.</summary>
	/// <param name="macAlg">A <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> object that holds the MAC to be used to compute the value of the <see cref="P:System.Security.Cryptography.Xml.SignedXml.Signature" /> property. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="macAlg" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> object specified by the <paramref name="macAlg" /> parameter is not an instance of <see cref="T:System.Security.Cryptography.HMACSHA1" />.-or- The <see cref="P:System.Security.Cryptography.HashAlgorithm.HashSize" /> property of the specified <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> object is not valid.-or- The cryptographic transform used to check the signature could not be created. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void ComputeSignature(KeyedHashAlgorithm macAlg)
	{
		if (macAlg == null)
		{
			throw new ArgumentNullException("macAlg");
		}
		string text = null;
		if (macAlg is HMACSHA1)
		{
			text = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";
		}
		else if (macAlg is HMACSHA256)
		{
			text = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
		}
		else if (macAlg is HMACSHA384)
		{
			text = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";
		}
		else if (macAlg is HMACSHA512)
		{
			text = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";
		}
		else if (macAlg is HMACRIPEMD160)
		{
			text = "http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160";
		}
		if (text == null)
		{
			throw new CryptographicException("unsupported algorithm");
		}
		DigestReferences();
		m_signature.SignedInfo.SignatureMethod = text;
		m_signature.SignatureValue = macAlg.ComputeHash(SignedInfoTransformed());
	}

	/// <summary>Returns the <see cref="T:System.Xml.XmlElement" /> object with the specified ID from the specified <see cref="T:System.Xml.XmlDocument" /> object.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlElement" /> object with the specified ID from the specified <see cref="T:System.Xml.XmlDocument" /> object, or null if it could not be found.</returns>
	/// <param name="document">The <see cref="T:System.Xml.XmlDocument" /> object to retrieve the <see cref="T:System.Xml.XmlElement" /> object from.</param>
	/// <param name="idValue">The ID of the <see cref="T:System.Xml.XmlElement" /> object to retrieve from the <see cref="T:System.Xml.XmlDocument" /> object.</param>
	public virtual XmlElement GetIdElement(XmlDocument document, string idValue)
	{
		if (document == null || idValue == null)
		{
			return null;
		}
		XmlElement xmlElement = document.GetElementById(idValue);
		if (xmlElement == null)
		{
			xmlElement = (XmlElement)document.SelectSingleNode("//*[@Id='" + idValue + "']");
			if (xmlElement == null)
			{
				xmlElement = (XmlElement)document.SelectSingleNode("//*[@ID='" + idValue + "']");
				if (xmlElement == null)
				{
					xmlElement = (XmlElement)document.SelectSingleNode("//*[@id='" + idValue + "']");
				}
			}
		}
		return xmlElement;
	}

	internal static XmlElement DefaultGetIdElement(XmlDocument document, string idValue)
	{
		if (document == null)
		{
			return null;
		}
		try
		{
			XmlConvert.VerifyNCName(idValue);
		}
		catch
		{
			return null;
		}
		XmlElement elementById = document.GetElementById(idValue);
		if (elementById != null)
		{
			XmlDocument xmlDocument = (XmlDocument)document.CloneNode(deep: true);
			XmlElement elementById2 = xmlDocument.GetElementById(idValue);
			if (elementById2 != null)
			{
				elementById2.Attributes.RemoveAll();
				if (xmlDocument.GetElementById(idValue) != null)
				{
					throw new CryptographicException("Malformed reference element.");
				}
			}
			return elementById;
		}
		elementById = GetSingleReferenceTarget(document, "Id", idValue);
		if (elementById != null)
		{
			return elementById;
		}
		elementById = GetSingleReferenceTarget(document, "id", idValue);
		if (elementById != null)
		{
			return elementById;
		}
		return GetSingleReferenceTarget(document, "ID", idValue);
	}

	private static XmlElement GetSingleReferenceTarget(XmlDocument document, string idAttributeName, string idValue)
	{
		string xpath = "//*[@" + idAttributeName + "=\"" + idValue + "\"]";
		XmlNodeList xmlNodeList = document.SelectNodes(xpath);
		if (xmlNodeList == null || xmlNodeList.Count == 0)
		{
			return null;
		}
		if (xmlNodeList.Count == 1)
		{
			return xmlNodeList[0] as XmlElement;
		}
		throw new CryptographicException("Malformed reference element.");
	}

	/// <summary>Returns the public key of a signature.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object that contains the public key of the signature, or null if the key cannot be found.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.SignedXml.KeyInfo" /> property is null.</exception>
	protected virtual AsymmetricAlgorithm GetPublicKey()
	{
		if (m_signature.KeyInfo == null)
		{
			return null;
		}
		if (pkEnumerator == null)
		{
			pkEnumerator = m_signature.KeyInfo.GetEnumerator();
		}
		if (_x509Enumerator != null)
		{
			if (_x509Enumerator.MoveNext())
			{
				return new X509Certificate2(((X509Certificate)_x509Enumerator.Current).GetRawCertData()).PublicKey.Key;
			}
			_x509Enumerator = null;
		}
		while (pkEnumerator.MoveNext())
		{
			AsymmetricAlgorithm asymmetricAlgorithm = null;
			KeyInfoClause keyInfoClause = (KeyInfoClause)pkEnumerator.Current;
			if (keyInfoClause is DSAKeyValue)
			{
				asymmetricAlgorithm = DSA.Create();
			}
			else if (keyInfoClause is RSAKeyValue)
			{
				asymmetricAlgorithm = RSA.Create();
			}
			if (asymmetricAlgorithm != null)
			{
				asymmetricAlgorithm.FromXmlString(keyInfoClause.GetXml().InnerXml);
				return asymmetricAlgorithm;
			}
			if (keyInfoClause is KeyInfoX509Data)
			{
				_x509Enumerator = ((KeyInfoX509Data)keyInfoClause).Certificates.GetEnumerator();
				if (_x509Enumerator.MoveNext())
				{
					return new X509Certificate2(((X509Certificate)_x509Enumerator.Current).GetRawCertData()).PublicKey.Key;
				}
			}
		}
		return null;
	}

	/// <summary>Returns the XML representation of a <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> object.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.Signature" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignedInfo" /> property is null.-or- The <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignatureValue" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		return m_signature.GetXml(envdoc);
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> state from an XML element.</summary>
	/// <param name="value">The XML element to load the <see cref="T:System.Security.Cryptography.Xml.SignedXml" /> state from. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignatureValue" /> property.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.SignedXml.SignedInfo" /> property.</exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		signatureElement = value;
		m_signature.LoadXml(value);
		if (_context == null)
		{
			_context = value;
		}
		foreach (Reference reference in m_signature.SignedInfo.References)
		{
			foreach (Transform item in reference.TransformChain)
			{
				if (item is XmlDecryptionTransform)
				{
					((XmlDecryptionTransform)item).EncryptedXml = EncryptedXml;
				}
			}
		}
	}
}
