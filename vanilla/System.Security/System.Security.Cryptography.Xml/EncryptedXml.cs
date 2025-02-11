using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the process model for implementing XML encryption.</summary>
public class EncryptedXml
{
	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for XML encryption syntax and processing. This field is constant.</summary>
	public const string XmlEncNamespaceUrl = "http://www.w3.org/2001/04/xmlenc#";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for an XML encryption element. This field is constant.</summary>
	public const string XmlEncElementUrl = "http://www.w3.org/2001/04/xmlenc#Element";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for XML encryption element content. This field is constant.</summary>
	public const string XmlEncElementContentUrl = "http://www.w3.org/2001/04/xmlenc#Content";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the XML encryption &lt;EncryptedKey&gt; element. This field is constant.</summary>
	public const string XmlEncEncryptedKeyUrl = "http://www.w3.org/2001/04/xmlenc#EncryptedKey";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the Digital Encryption Standard (DES) algorithm. This field is constant.</summary>
	public const string XmlEncDESUrl = "http://www.w3.org/2001/04/xmlenc#des-cbc";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the Triple DES algorithm. This field is constant.</summary>
	public const string XmlEncTripleDESUrl = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the 128-bit Advanced Encryption Standard (AES) algorithm (also known as the Rijndael algorithm). This field is constant.</summary>
	public const string XmlEncAES128Url = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the 256-bit Advanced Encryption Standard (AES) algorithm (also known as the Rijndael algorithm). This field is constant.</summary>
	public const string XmlEncAES256Url = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the 192-bit Advanced Encryption Standard (AES) algorithm (also known as the Rijndael algorithm). This field is constant.</summary>
	public const string XmlEncAES192Url = "http://www.w3.org/2001/04/xmlenc#aes192-cbc";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the RSA Public Key Cryptography Standard (PKCS) Version 1.5 algorithm. This field is constant.</summary>
	public const string XmlEncRSA15Url = "http://www.w3.org/2001/04/xmlenc#rsa-1_5";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the RSA Optimal Asymmetric Encryption Padding (OAEP) encryption algorithm. This field is constant.</summary>
	public const string XmlEncRSAOAEPUrl = "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the TRIPLEDES key wrap algorithm. This field is constant.</summary>
	public const string XmlEncTripleDESKeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-tripledes";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the 128-bit Advanced Encryption Standard (AES) Key Wrap algorithm (also known as the Rijndael Key Wrap algorithm). This field is constant. </summary>
	public const string XmlEncAES128KeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-aes128";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the 256-bit Advanced Encryption Standard (AES) Key Wrap algorithm (also known as the Rijndael Key Wrap algorithm). This field is constant.</summary>
	public const string XmlEncAES256KeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-aes256";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the 192-bit Advanced Encryption Standard (AES) Key Wrap algorithm (also known as the Rijndael Key Wrap algorithm). This field is constant.</summary>
	public const string XmlEncAES192KeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-aes192";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the SHA-256 algorithm. This field is constant.</summary>
	public const string XmlEncSHA256Url = "http://www.w3.org/2001/04/xmlenc#sha256";

	/// <summary>Represents the namespace Uniform Resource Identifier (URI) for the SHA-512 algorithm. This field is constant.</summary>
	public const string XmlEncSHA512Url = "http://www.w3.org/2001/04/xmlenc#sha512";

	private XmlDocument _document;

	private Evidence _evidence;

	private XmlResolver _xmlResolver;

	private const int _capacity = 4;

	private Hashtable _keyNameMapping;

	private PaddingMode _padding;

	private CipherMode _mode;

	private Encoding _encoding;

	private string _recipient;

	private int _xmlDsigSearchDepthCounter;

	private int _xmlDsigSearchDepth;

	public int XmlDSigSearchDepth
	{
		get
		{
			return _xmlDsigSearchDepth;
		}
		set
		{
			_xmlDsigSearchDepth = value;
		}
	}

	/// <summary>Gets or sets the evidence of the <see cref="T:System.Xml.XmlDocument" /> object from which the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object is constructed.</summary>
	/// <returns>An <see cref="T:System.Security.Policy.Evidence" /> object.</returns>
	public Evidence DocumentEvidence
	{
		get
		{
			return _evidence;
		}
		set
		{
			_evidence = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.XmlResolver" /> object used by the Document Object Model (DOM) to resolve external XML references.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlResolver" /> object.</returns>
	public XmlResolver Resolver
	{
		get
		{
			return _xmlResolver;
		}
		set
		{
			_xmlResolver = value;
		}
	}

	/// <summary>Gets or sets the padding mode used for XML encryption.</summary>
	/// <returns>One of the <see cref="T:System.Security.Cryptography.PaddingMode" /> values that specifies the type of padding used for encryption.</returns>
	public PaddingMode Padding
	{
		get
		{
			return _padding;
		}
		set
		{
			_padding = value;
		}
	}

	/// <summary>Gets or sets the cipher mode used for XML encryption.</summary>
	/// <returns>One of the <see cref="T:System.Security.Cryptography.CipherMode" /> values.</returns>
	public CipherMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	/// <summary>Gets or sets the encoding used for XML encryption.</summary>
	/// <returns>An <see cref="T:System.Text.Encoding" /> object.</returns>
	public Encoding Encoding
	{
		get
		{
			return _encoding;
		}
		set
		{
			_encoding = value;
		}
	}

	/// <summary>Gets or sets the recipient of the encrypted key information.</summary>
	/// <returns>The recipient of the encrypted key information.</returns>
	public string Recipient
	{
		get
		{
			if (_recipient == null)
			{
				_recipient = string.Empty;
			}
			return _recipient;
		}
		set
		{
			_recipient = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> class.</summary>
	public EncryptedXml()
		: this(new XmlDocument())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> class using the specified XML document.</summary>
	/// <param name="document">An <see cref="T:System.Xml.XmlDocument" /> object used to initialize the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object.</param>
	public EncryptedXml(XmlDocument document)
		: this(document, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> class using the specified XML document and evidence.</summary>
	/// <param name="document">An <see cref="T:System.Xml.XmlDocument" /> object used to initialize the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object.</param>
	/// <param name="evidence">An <see cref="T:System.Security.Policy.Evidence" /> object associated with the <see cref="T:System.Xml.XmlDocument" /> object.</param>
	public EncryptedXml(XmlDocument document, Evidence evidence)
	{
		_document = document;
		_evidence = evidence;
		_xmlResolver = null;
		_padding = PaddingMode.ISO10126;
		_mode = CipherMode.CBC;
		_encoding = Encoding.UTF8;
		_keyNameMapping = new Hashtable(4);
		_xmlDsigSearchDepth = 20;
	}

	private bool IsOverXmlDsigRecursionLimit()
	{
		if (_xmlDsigSearchDepthCounter > XmlDSigSearchDepth)
		{
			return true;
		}
		return false;
	}

	private byte[] GetCipherValue(CipherData cipherData)
	{
		if (cipherData == null)
		{
			throw new ArgumentNullException("cipherData");
		}
		WebResponse webResponse = null;
		Stream stream = null;
		if (cipherData.CipherValue != null)
		{
			return cipherData.CipherValue;
		}
		if (cipherData.CipherReference != null)
		{
			if (cipherData.CipherReference.CipherValue != null)
			{
				return cipherData.CipherReference.CipherValue;
			}
			Stream stream2 = null;
			if (cipherData.CipherReference.Uri.Length == 0)
			{
				string baseUri = ((_document == null) ? null : _document.BaseURI);
				stream2 = cipherData.CipherReference.TransformChain.TransformToOctetStream(_document, _xmlResolver, baseUri);
			}
			else
			{
				if (cipherData.CipherReference.Uri[0] != '#')
				{
					throw new CryptographicException("Unable to resolve Uri {0}.", cipherData.CipherReference.Uri);
				}
				string idValue = Utils.ExtractIdFromLocalUri(cipherData.CipherReference.Uri);
				stream = new MemoryStream(_encoding.GetBytes(GetIdElement(_document, idValue).OuterXml));
				string baseUri2 = ((_document == null) ? null : _document.BaseURI);
				stream2 = cipherData.CipherReference.TransformChain.TransformToOctetStream(stream, _xmlResolver, baseUri2);
			}
			byte[] array = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Utils.Pump(stream2, memoryStream);
				array = memoryStream.ToArray();
				webResponse?.Close();
				stream?.Close();
				stream2.Close();
			}
			cipherData.CipherReference.CipherValue = array;
			return array;
		}
		throw new CryptographicException("Cipher data is not specified.");
	}

	/// <summary>Determines how to resolve internal Uniform Resource Identifier (URI) references.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlElement" /> object that contains an ID indicating how internal Uniform Resource Identifiers (URIs) are to be resolved.</returns>
	/// <param name="document">An <see cref="T:System.Xml.XmlDocument" /> object that contains an element with an ID value.</param>
	/// <param name="idValue">A string that represents the ID value.</param>
	public virtual XmlElement GetIdElement(XmlDocument document, string idValue)
	{
		return SignedXml.DefaultGetIdElement(document, idValue);
	}

	/// <summary>Retrieves the decryption initialization vector (IV) from an <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object.</summary>
	/// <returns>A byte array that contains the decryption initialization vector (IV).</returns>
	/// <param name="encryptedData">The <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object that contains the initialization vector (IV) to retrieve.</param>
	/// <param name="symmetricAlgorithmUri">The Uniform Resource Identifier (URI) that describes the cryptographic algorithm associated with the <paramref name="encryptedData" /> value.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="encryptedData" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <paramref name="encryptedData" /> parameter has an <see cref="P:System.Security.Cryptography.Xml.EncryptedType.EncryptionMethod" />  property that is null.-or-The value of the <paramref name="symmetricAlgorithmUrisymAlgUri" /> parameter is not a supported algorithm.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual byte[] GetDecryptionIV(EncryptedData encryptedData, string symmetricAlgorithmUri)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		int num = 0;
		if (symmetricAlgorithmUri == null)
		{
			if (encryptedData.EncryptionMethod == null)
			{
				throw new CryptographicException("Symmetric algorithm is not specified.");
			}
			symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
		}
		switch (symmetricAlgorithmUri)
		{
		case "http://www.w3.org/2001/04/xmlenc#des-cbc":
		case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
			num = 8;
			break;
		case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
		case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
		case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
			num = 16;
			break;
		default:
			throw new CryptographicException(" The specified Uri is not supported.");
		}
		byte[] array = new byte[num];
		Buffer.BlockCopy(GetCipherValue(encryptedData.CipherData), 0, array, 0, array.Length);
		return array;
	}

	/// <summary>Retrieves the decryption key from the specified <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.SymmetricAlgorithm" /> object associated with the decryption key.</returns>
	/// <param name="encryptedData">The <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object that contains the decryption key to retrieve.</param>
	/// <param name="symmetricAlgorithmUri">The size of the decryption key to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="encryptedData" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The encryptedData parameter has an <see cref="P:System.Security.Cryptography.Xml.EncryptedType.EncryptionMethod" /> property that is null.-or-The encrypted key cannot be retrieved using the specified parameters.</exception>
	public virtual SymmetricAlgorithm GetDecryptionKey(EncryptedData encryptedData, string symmetricAlgorithmUri)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		if (encryptedData.KeyInfo == null)
		{
			return null;
		}
		IEnumerator enumerator = encryptedData.KeyInfo.GetEnumerator();
		EncryptedKey encryptedKey = null;
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is KeyInfoName { Value: var value })
			{
				if ((SymmetricAlgorithm)_keyNameMapping[value] != null)
				{
					return (SymmetricAlgorithm)_keyNameMapping[value];
				}
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(_document.NameTable);
				xmlNamespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
				XmlNodeList xmlNodeList = _document.SelectNodes("//enc:EncryptedKey", xmlNamespaceManager);
				if (xmlNodeList == null)
				{
					break;
				}
				foreach (XmlNode item in xmlNodeList)
				{
					XmlElement value2 = item as XmlElement;
					EncryptedKey encryptedKey2 = new EncryptedKey();
					encryptedKey2.LoadXml(value2);
					if (encryptedKey2.CarriedKeyName == value && encryptedKey2.Recipient == Recipient)
					{
						encryptedKey = encryptedKey2;
						break;
					}
				}
				break;
			}
			if (enumerator.Current is KeyInfoRetrievalMethod keyInfoRetrievalMethod)
			{
				string idValue = Utils.ExtractIdFromLocalUri(keyInfoRetrievalMethod.Uri);
				encryptedKey = new EncryptedKey();
				encryptedKey.LoadXml(GetIdElement(_document, idValue));
				break;
			}
			if (enumerator.Current is KeyInfoEncryptedKey keyInfoEncryptedKey)
			{
				encryptedKey = keyInfoEncryptedKey.EncryptedKey;
				break;
			}
		}
		if (encryptedKey != null)
		{
			if (symmetricAlgorithmUri == null)
			{
				if (encryptedData.EncryptionMethod == null)
				{
					throw new CryptographicException("Symmetric algorithm is not specified.");
				}
				symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
			}
			byte[] array = DecryptEncryptedKey(encryptedKey);
			if (array == null)
			{
				throw new CryptographicException("Unable to retrieve the decryption key.");
			}
			SymmetricAlgorithm obj = (SymmetricAlgorithm)CryptoHelpers.CreateFromName(symmetricAlgorithmUri);
			obj.Key = array;
			return obj;
		}
		return null;
	}

	/// <summary>Determines the key represented by the <see cref="T:System.Security.Cryptography.Xml.EncryptedKey" /> element.</summary>
	/// <returns>A byte array that contains the key.</returns>
	/// <param name="encryptedKey">The <see cref="T:System.Security.Cryptography.Xml.EncryptedKey" /> object that contains the key to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="encryptedKey" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <paramref name="encryptedKey" /> parameter is not the Triple DES Key Wrap algorithm or the Advanced Encryption Standard (AES) Key Wrap algorithm (also called Rijndael). </exception>
	public virtual byte[] DecryptEncryptedKey(EncryptedKey encryptedKey)
	{
		if (encryptedKey == null)
		{
			throw new ArgumentNullException("encryptedKey");
		}
		if (encryptedKey.KeyInfo == null)
		{
			return null;
		}
		IEnumerator enumerator = encryptedKey.KeyInfo.GetEnumerator();
		EncryptedKey encryptedKey2 = null;
		bool flag = false;
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is KeyInfoName { Value: var value })
			{
				object obj = _keyNameMapping[value];
				if (obj == null)
				{
					break;
				}
				if (obj is SymmetricAlgorithm)
				{
					return DecryptKey(encryptedKey.CipherData.CipherValue, (SymmetricAlgorithm)obj);
				}
				flag = encryptedKey.EncryptionMethod != null && encryptedKey.EncryptionMethod.KeyAlgorithm == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p";
				return DecryptKey(encryptedKey.CipherData.CipherValue, (RSA)obj, flag);
			}
			if (enumerator.Current is KeyInfoX509Data keyInfoX509Data)
			{
				X509Certificate2Enumerator enumerator2 = Utils.BuildBagOfCerts(keyInfoX509Data, CertUsageType.Decryption).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					using RSA rSA = enumerator2.Current.GetRSAPrivateKey();
					if (rSA != null)
					{
						flag = encryptedKey.EncryptionMethod != null && encryptedKey.EncryptionMethod.KeyAlgorithm == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p";
						return DecryptKey(encryptedKey.CipherData.CipherValue, rSA, flag);
					}
				}
				break;
			}
			if (enumerator.Current is KeyInfoRetrievalMethod keyInfoRetrievalMethod)
			{
				string idValue = Utils.ExtractIdFromLocalUri(keyInfoRetrievalMethod.Uri);
				encryptedKey2 = new EncryptedKey();
				encryptedKey2.LoadXml(GetIdElement(_document, idValue));
				try
				{
					_xmlDsigSearchDepthCounter++;
					if (IsOverXmlDsigRecursionLimit())
					{
						throw new CryptoSignedXmlRecursionException();
					}
					return DecryptEncryptedKey(encryptedKey2);
				}
				finally
				{
					_xmlDsigSearchDepthCounter--;
				}
			}
			if (enumerator.Current is KeyInfoEncryptedKey { EncryptedKey: var encryptedKey3 })
			{
				byte[] array = DecryptEncryptedKey(encryptedKey3);
				if (array != null)
				{
					SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)CryptoHelpers.CreateFromName(encryptedKey.EncryptionMethod.KeyAlgorithm);
					symmetricAlgorithm.Key = array;
					return DecryptKey(encryptedKey.CipherData.CipherValue, symmetricAlgorithm);
				}
			}
		}
		return null;
	}

	/// <summary>Defines a mapping between a key name and a symmetric key or an asymmetric key.</summary>
	/// <param name="keyName">The name to map to <paramref name="keyObject" />.</param>
	/// <param name="keyObject">The symmetric key to map to <paramref name="keyName" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="keyName" /> parameter is null.-or-The value of the <paramref name="keyObject" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <paramref name="keyObject" /> parameter is not an RSA algorithm or a symmetric key. </exception>
	public void AddKeyNameMapping(string keyName, object keyObject)
	{
		if (keyName == null)
		{
			throw new ArgumentNullException("keyName");
		}
		if (keyObject == null)
		{
			throw new ArgumentNullException("keyObject");
		}
		if (!(keyObject is SymmetricAlgorithm) && !(keyObject is RSA))
		{
			throw new CryptographicException("The specified cryptographic transform is not supported.");
		}
		_keyNameMapping.Add(keyName, keyObject);
	}

	/// <summary>Resets all key name mapping.</summary>
	public void ClearKeyNameMappings()
	{
		_keyNameMapping.Clear();
	}

	/// <summary>Encrypts the outer XML of an element using the specified X.509 certificate.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> element that represents the encrypted XML data.</returns>
	/// <param name="inputElement">The XML element to encrypt.</param>
	/// <param name="certificate">The X.509 certificate to use for encryption.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="inputElement" /> parameter is null.-or-The value of the <paramref name="certificate" /> parameter is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The value of the <paramref name="certificate" /> parameter does not represent an RSA key algorithm.</exception>
	public EncryptedData Encrypt(XmlElement inputElement, X509Certificate2 certificate)
	{
		if (inputElement == null)
		{
			throw new ArgumentNullException("inputElement");
		}
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		using RSA rSA = certificate.GetRSAPublicKey();
		if (rSA == null)
		{
			throw new NotSupportedException("The certificate key algorithm is not supported.");
		}
		EncryptedData obj = new EncryptedData
		{
			Type = "http://www.w3.org/2001/04/xmlenc#Element",
			EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#aes256-cbc")
		};
		EncryptedKey encryptedKey = new EncryptedKey();
		encryptedKey.EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#rsa-1_5");
		encryptedKey.KeyInfo.AddClause(new KeyInfoX509Data(certificate));
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		encryptedKey.CipherData.CipherValue = EncryptKey(rijndaelManaged.Key, rSA, useOAEP: false);
		KeyInfoEncryptedKey clause = new KeyInfoEncryptedKey(encryptedKey);
		obj.KeyInfo.AddClause(clause);
		obj.CipherData.CipherValue = EncryptData(inputElement, rijndaelManaged, content: false);
		return obj;
	}

	/// <summary>Encrypts the outer XML of an element using the specified key in the key mapping table.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object that represents the encrypted XML data.</returns>
	/// <param name="inputElement">The XML element to encrypt.</param>
	/// <param name="keyName">A key name that can be found in the key mapping table.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="inputElement" /> parameter is null.-or-The value of the <paramref name="keyName" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <paramref name="keyName" /> parameter does not match a registered key name pair.-or-The cryptographic key described by the <paramref name="keyName" /> parameter is not supported. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public EncryptedData Encrypt(XmlElement inputElement, string keyName)
	{
		if (inputElement == null)
		{
			throw new ArgumentNullException("inputElement");
		}
		if (keyName == null)
		{
			throw new ArgumentNullException("keyName");
		}
		object obj = null;
		if (_keyNameMapping != null)
		{
			obj = _keyNameMapping[keyName];
		}
		if (obj == null)
		{
			throw new CryptographicException("Unable to retrieve the encryption key.");
		}
		SymmetricAlgorithm symmetricAlgorithm = obj as SymmetricAlgorithm;
		RSA rsa = obj as RSA;
		EncryptedData encryptedData = new EncryptedData();
		encryptedData.Type = "http://www.w3.org/2001/04/xmlenc#Element";
		encryptedData.EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#aes256-cbc");
		string algorithm = null;
		if (symmetricAlgorithm == null)
		{
			algorithm = "http://www.w3.org/2001/04/xmlenc#rsa-1_5";
		}
		else if (symmetricAlgorithm is TripleDES)
		{
			algorithm = "http://www.w3.org/2001/04/xmlenc#kw-tripledes";
		}
		else
		{
			if (!(symmetricAlgorithm is Rijndael) && !(symmetricAlgorithm is Aes))
			{
				throw new CryptographicException("The specified cryptographic transform is not supported.");
			}
			switch (symmetricAlgorithm.KeySize)
			{
			case 128:
				algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes128";
				break;
			case 192:
				algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes192";
				break;
			case 256:
				algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes256";
				break;
			}
		}
		EncryptedKey encryptedKey = new EncryptedKey();
		encryptedKey.EncryptionMethod = new EncryptionMethod(algorithm);
		encryptedKey.KeyInfo.AddClause(new KeyInfoName(keyName));
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		encryptedKey.CipherData.CipherValue = ((symmetricAlgorithm == null) ? EncryptKey(rijndaelManaged.Key, rsa, useOAEP: false) : EncryptKey(rijndaelManaged.Key, symmetricAlgorithm));
		KeyInfoEncryptedKey clause = new KeyInfoEncryptedKey(encryptedKey);
		encryptedData.KeyInfo.AddClause(clause);
		encryptedData.CipherData.CipherValue = EncryptData(inputElement, rijndaelManaged, content: false);
		return encryptedData;
	}

	/// <summary>Decrypts all &lt;EncryptedData&gt; elements of the XML document that were specified during initialization of the <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> class.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic key used to decrypt the document was not found. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public void DecryptDocument()
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(_document.NameTable);
		xmlNamespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
		XmlNodeList xmlNodeList = _document.SelectNodes("//enc:EncryptedData", xmlNamespaceManager);
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			XmlElement xmlElement = item as XmlElement;
			EncryptedData encryptedData = new EncryptedData();
			encryptedData.LoadXml(xmlElement);
			SymmetricAlgorithm decryptionKey = GetDecryptionKey(encryptedData, null);
			if (decryptionKey == null)
			{
				throw new CryptographicException("Unable to retrieve the decryption key.");
			}
			byte[] decryptedData = DecryptData(encryptedData, decryptionKey);
			ReplaceData(xmlElement, decryptedData);
		}
	}

	/// <summary>Encrypts data in the specified byte array using the specified symmetric algorithm.</summary>
	/// <returns>A byte array of encrypted data.</returns>
	/// <param name="plaintext">The data to encrypt.</param>
	/// <param name="symmetricAlgorithm">The symmetric algorithm to use for encryption.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="plaintext" /> parameter is null.-or-The value of the <paramref name="symmetricAlgorithm" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The data could not be encrypted using the specified parameters.</exception>
	public byte[] EncryptData(byte[] plaintext, SymmetricAlgorithm symmetricAlgorithm)
	{
		if (plaintext == null)
		{
			throw new ArgumentNullException("plaintext");
		}
		if (symmetricAlgorithm == null)
		{
			throw new ArgumentNullException("symmetricAlgorithm");
		}
		CipherMode mode = symmetricAlgorithm.Mode;
		PaddingMode padding = symmetricAlgorithm.Padding;
		byte[] array = null;
		try
		{
			symmetricAlgorithm.Mode = _mode;
			symmetricAlgorithm.Padding = _padding;
			array = symmetricAlgorithm.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);
		}
		finally
		{
			symmetricAlgorithm.Mode = mode;
			symmetricAlgorithm.Padding = padding;
		}
		byte[] array2 = null;
		if (_mode == CipherMode.ECB)
		{
			array2 = array;
		}
		else
		{
			byte[] iV = symmetricAlgorithm.IV;
			array2 = new byte[array.Length + iV.Length];
			Buffer.BlockCopy(iV, 0, array2, 0, iV.Length);
			Buffer.BlockCopy(array, 0, array2, iV.Length, array.Length);
		}
		return array2;
	}

	/// <summary>Encrypts the specified element or its contents using the specified symmetric algorithm.</summary>
	/// <returns>A byte array that contains the encrypted data.</returns>
	/// <param name="inputElement">The element or its contents to encrypt.</param>
	/// <param name="symmetricAlgorithm">The symmetric algorithm to use for encryption.</param>
	/// <param name="content">true to encrypt only the contents of the element; false to encrypt the entire element.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="inputElement" /> parameter is null.-or-The value of the <paramref name="symmetricAlgorithm" /> parameter is null.</exception>
	public byte[] EncryptData(XmlElement inputElement, SymmetricAlgorithm symmetricAlgorithm, bool content)
	{
		if (inputElement == null)
		{
			throw new ArgumentNullException("inputElement");
		}
		if (symmetricAlgorithm == null)
		{
			throw new ArgumentNullException("symmetricAlgorithm");
		}
		byte[] plaintext = (content ? _encoding.GetBytes(inputElement.InnerXml) : _encoding.GetBytes(inputElement.OuterXml));
		return EncryptData(plaintext, symmetricAlgorithm);
	}

	/// <summary>Decrypts an &lt;EncryptedData&gt; element using the specified symmetric algorithm.</summary>
	/// <returns>A byte array that contains the raw decrypted plain text.</returns>
	/// <param name="encryptedData">The data to decrypt.</param>
	/// <param name="symmetricAlgorithm">The symmetric key used to decrypt <paramref name="encryptedData" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="encryptedData" /> parameter is null.-or-The value of the <paramref name="symmetricAlgorithm" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] DecryptData(EncryptedData encryptedData, SymmetricAlgorithm symmetricAlgorithm)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		if (symmetricAlgorithm == null)
		{
			throw new ArgumentNullException("symmetricAlgorithm");
		}
		byte[] cipherValue = GetCipherValue(encryptedData.CipherData);
		CipherMode mode = symmetricAlgorithm.Mode;
		PaddingMode padding = symmetricAlgorithm.Padding;
		byte[] iV = symmetricAlgorithm.IV;
		byte[] array = null;
		if (_mode != CipherMode.ECB)
		{
			array = GetDecryptionIV(encryptedData, null);
		}
		byte[] array2 = null;
		try
		{
			int num = 0;
			if (array != null)
			{
				symmetricAlgorithm.IV = array;
				num = array.Length;
			}
			symmetricAlgorithm.Mode = _mode;
			symmetricAlgorithm.Padding = _padding;
			return symmetricAlgorithm.CreateDecryptor().TransformFinalBlock(cipherValue, num, cipherValue.Length - num);
		}
		finally
		{
			symmetricAlgorithm.Mode = mode;
			symmetricAlgorithm.Padding = padding;
			symmetricAlgorithm.IV = iV;
		}
	}

	/// <summary>Replaces an &lt;EncryptedData&gt; element with a specified decrypted sequence of bytes.</summary>
	/// <param name="inputElement">The &lt;EncryptedData&gt; element to replace.</param>
	/// <param name="decryptedData">The decrypted data to replace <paramref name="inputElement" /> with.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="inputElement" /> parameter is null.-or-The value of the <paramref name="decryptedData" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void ReplaceData(XmlElement inputElement, byte[] decryptedData)
	{
		if (inputElement == null)
		{
			throw new ArgumentNullException("inputElement");
		}
		if (decryptedData == null)
		{
			throw new ArgumentNullException("decryptedData");
		}
		XmlNode parentNode = inputElement.ParentNode;
		if (parentNode.NodeType == XmlNodeType.Document)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.PreserveWhitespace = true;
			using (StringReader input = new StringReader(_encoding.GetString(decryptedData)))
			{
				using XmlReader reader = XmlReader.Create(input, Utils.GetSecureXmlReaderSettings(_xmlResolver));
				xmlDocument.Load(reader);
			}
			XmlNode newChild = inputElement.OwnerDocument.ImportNode(xmlDocument.DocumentElement, deep: true);
			parentNode.RemoveChild(inputElement);
			parentNode.AppendChild(newChild);
			return;
		}
		XmlNode xmlNode = parentNode.OwnerDocument.CreateElement(parentNode.Prefix, parentNode.LocalName, parentNode.NamespaceURI);
		try
		{
			parentNode.AppendChild(xmlNode);
			xmlNode.InnerXml = _encoding.GetString(decryptedData);
			XmlNode xmlNode2 = xmlNode.FirstChild;
			XmlNode nextSibling = inputElement.NextSibling;
			while (xmlNode2 != null)
			{
				XmlNode nextSibling2 = xmlNode2.NextSibling;
				parentNode.InsertBefore(xmlNode2, nextSibling);
				xmlNode2 = nextSibling2;
			}
		}
		finally
		{
			parentNode.RemoveChild(xmlNode);
		}
		parentNode.RemoveChild(inputElement);
	}

	/// <summary>Replaces the specified element with the specified <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object.</summary>
	/// <param name="inputElement">The element to replace with an &lt;EncryptedData&gt; element.</param>
	/// <param name="encryptedData">The <see cref="T:System.Security.Cryptography.Xml.EncryptedData" /> object to replace the <paramref name="inputElement" /> parameter with.</param>
	/// <param name="content">true to replace only the contents of the element; false to replace the entire element.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="inputElement" /> parameter is null.-or-The value of the <paramref name="encryptedData" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void ReplaceElement(XmlElement inputElement, EncryptedData encryptedData, bool content)
	{
		if (inputElement == null)
		{
			throw new ArgumentNullException("inputElement");
		}
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		XmlElement xml = encryptedData.GetXml(inputElement.OwnerDocument);
		switch (content)
		{
		case true:
			Utils.RemoveAllChildren(inputElement);
			inputElement.AppendChild(xml);
			break;
		case false:
			inputElement.ParentNode.ReplaceChild(xml, inputElement);
			break;
		}
	}

	/// <summary>Encrypts a key using a symmetric algorithm that a recipient uses to decrypt an &lt;EncryptedData&gt; element.</summary>
	/// <returns>A byte array that represents the encrypted value of the <paramref name="keyData" /> parameter.</returns>
	/// <param name="keyData">The key to encrypt.</param>
	/// <param name="symmetricAlgorithm">The symmetric key used to encrypt <paramref name="keyData" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="keyData" /> parameter is null.-or-The value of the <paramref name="symmetricAlgorithm" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <paramref name="symmetricAlgorithm" /> parameter is not the Triple DES Key Wrap algorithm or the Advanced Encryption Standard (AES) Key Wrap algorithm (also called Rijndael). </exception>
	public static byte[] EncryptKey(byte[] keyData, SymmetricAlgorithm symmetricAlgorithm)
	{
		if (keyData == null)
		{
			throw new ArgumentNullException("keyData");
		}
		if (symmetricAlgorithm == null)
		{
			throw new ArgumentNullException("symmetricAlgorithm");
		}
		if (symmetricAlgorithm is TripleDES)
		{
			return SymmetricKeyWrap.TripleDESKeyWrapEncrypt(symmetricAlgorithm.Key, keyData);
		}
		if (symmetricAlgorithm is Rijndael || symmetricAlgorithm is Aes)
		{
			return SymmetricKeyWrap.AESKeyWrapEncrypt(symmetricAlgorithm.Key, keyData);
		}
		throw new CryptographicException("The specified cryptographic transform is not supported.");
	}

	/// <summary>Encrypts the key that a recipient uses to decrypt an &lt;EncryptedData&gt; element.</summary>
	/// <returns>A byte array that represents the encrypted value of the <paramref name="keyData" /> parameter.</returns>
	/// <param name="keyData">The key to encrypt.</param>
	/// <param name="rsa">The asymmetric key used to encrypt <paramref name="keyData" />.</param>
	/// <param name="useOAEP">A value that specifies whether to use Optimal Asymmetric Encryption Padding (OAEP).</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="keyData" /> parameter is null.-or-The value of the <paramref name="rsa" /> parameter is null.</exception>
	public static byte[] EncryptKey(byte[] keyData, RSA rsa, bool useOAEP)
	{
		if (keyData == null)
		{
			throw new ArgumentNullException("keyData");
		}
		if (rsa == null)
		{
			throw new ArgumentNullException("rsa");
		}
		if (useOAEP)
		{
			return new RSAOAEPKeyExchangeFormatter(rsa).CreateKeyExchange(keyData);
		}
		return new RSAPKCS1KeyExchangeFormatter(rsa).CreateKeyExchange(keyData);
	}

	/// <summary>Decrypts an &lt;EncryptedKey&gt; element using a symmetric algorithm.</summary>
	/// <returns>A byte array that contains the plain text key.</returns>
	/// <param name="keyData">An array of bytes that represents an encrypted &lt;EncryptedKey&gt; element.</param>
	/// <param name="symmetricAlgorithm">The symmetric key used to decrypt <paramref name="keyData" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="keyData" /> parameter is null.-or-The value of the <paramref name="symmetricAlgorithm" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <paramref name="symmetricAlgorithm" /> element is not the Triple DES Key Wrap algorithm or the Advanced Encryption Standard (AES) Key Wrap algorithm (also called Rijndael). </exception>
	public static byte[] DecryptKey(byte[] keyData, SymmetricAlgorithm symmetricAlgorithm)
	{
		if (keyData == null)
		{
			throw new ArgumentNullException("keyData");
		}
		if (symmetricAlgorithm == null)
		{
			throw new ArgumentNullException("symmetricAlgorithm");
		}
		if (symmetricAlgorithm is TripleDES)
		{
			return SymmetricKeyWrap.TripleDESKeyWrapDecrypt(symmetricAlgorithm.Key, keyData);
		}
		if (symmetricAlgorithm is Rijndael || symmetricAlgorithm is Aes)
		{
			return SymmetricKeyWrap.AESKeyWrapDecrypt(symmetricAlgorithm.Key, keyData);
		}
		throw new CryptographicException("The specified cryptographic transform is not supported.");
	}

	/// <summary>Decrypts an &lt;EncryptedKey&gt; element using an asymmetric algorithm.</summary>
	/// <returns>A byte array that contains the plain text key.</returns>
	/// <param name="keyData">An array of bytes that represents an encrypted &lt;EncryptedKey&gt; element.</param>
	/// <param name="rsa">The asymmetric key used to decrypt <paramref name="keyData" />.</param>
	/// <param name="useOAEP">A value that specifies whether to use Optimal Asymmetric Encryption Padding (OAEP).</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="keyData" /> parameter is null.-or-The value of the <paramref name="rsa" /> parameter is null.</exception>
	public static byte[] DecryptKey(byte[] keyData, RSA rsa, bool useOAEP)
	{
		if (keyData == null)
		{
			throw new ArgumentNullException("keyData");
		}
		if (rsa == null)
		{
			throw new ArgumentNullException("rsa");
		}
		if (useOAEP)
		{
			return new RSAOAEPKeyExchangeDeformatter(rsa).DecryptKeyExchange(keyData);
		}
		return new RSAPKCS1KeyExchangeDeformatter(rsa).DecryptKeyExchange(keyData);
	}
}
