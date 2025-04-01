using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Mono.Security;
using Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents an X.509 certificate.  </summary>
[Serializable]
public class X509Certificate2 : X509Certificate
{
	private string friendlyName = string.Empty;

	private static byte[] signedData = new byte[9] { 42, 134, 72, 134, 247, 13, 1, 7, 2 };

	internal new X509Certificate2Impl Impl
	{
		get
		{
			X509Certificate2Impl result = base.Impl as X509Certificate2Impl;
			X509Helper2.ThrowIfContextInvalid(result);
			return result;
		}
	}

	/// <summary>Gets or sets a value indicating that an X.509 certificate is archived.</summary>
	/// <returns>true if the certificate is archived, false if the certificate is not archived.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public bool Archived
	{
		get
		{
			return Impl.Archived;
		}
		set
		{
			Impl.Archived = true;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Security.Cryptography.X509Certificates.X509Extension" /> objects.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public X509ExtensionCollection Extensions => Impl.Extensions;

	/// <summary>Gets or sets the associated alias for a certificate.</summary>
	/// <returns>The certificate's friendly name.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public string FriendlyName
	{
		get
		{
			ThrowIfContextInvalid();
			return friendlyName;
		}
		set
		{
			ThrowIfContextInvalid();
			friendlyName = value;
		}
	}

	/// <summary>Gets a value that indicates whether an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object contains a private key. </summary>
	/// <returns>true if the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object contains a private key; otherwise, false. </returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public bool HasPrivateKey => Impl.HasPrivateKey;

	/// <summary>Gets the distinguished name of the certificate issuer.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> object that contains the name of the certificate issuer.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public X500DistinguishedName IssuerName => Impl.IssuerName;

	/// <summary>Gets the date in local time after which a certificate is no longer valid.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that represents the expiration date for the certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public DateTime NotAfter => Impl.GetValidUntil().ToLocalTime();

	/// <summary>Gets the date in local time on which a certificate becomes valid.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that represents the effective date of the certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public DateTime NotBefore => Impl.GetValidFrom().ToLocalTime();

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object that represents the private key associated with a certificate.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object, which is either an RSA or DSA cryptographic service provider.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key value is not an RSA or DSA key, or the key is unreadable. </exception>
	/// <exception cref="T:System.ArgumentNullException">The value being set for this property is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The key algorithm for this private key is not supported.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The X.509 keys do not match.</exception>
	/// <exception cref="T:System.ArgumentException">The cryptographic service provider key is null.</exception>
	public AsymmetricAlgorithm PrivateKey
	{
		get
		{
			return Impl.PrivateKey;
		}
		set
		{
			Impl.PrivateKey = value;
		}
	}

	/// <summary>Gets a <see cref="P:System.Security.Cryptography.X509Certificates.X509Certificate2.PublicKey" /> object associated with a certificate.</summary>
	/// <returns>A <see cref="P:System.Security.Cryptography.X509Certificates.X509Certificate2.PublicKey" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key value is not an RSA or DSA key, or the key is unreadable. </exception>
	public PublicKey PublicKey => Impl.PublicKey;

	/// <summary>Gets the raw data of a certificate.</summary>
	/// <returns>The raw data of the certificate as a byte array.</returns>
	public byte[] RawData => GetRawCertData();

	/// <summary>Gets the serial number of a certificate.</summary>
	/// <returns>The serial number of the certificate.</returns>
	public string SerialNumber => GetSerialNumberString();

	/// <summary>Gets the algorithm used to create the signature of a certificate.</summary>
	/// <returns>Returns the object identifier (<see cref="T:System.Security.Cryptography.Oid" />) of the signature algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public Oid SignatureAlgorithm => Impl.SignatureAlgorithm;

	/// <summary>Gets the subject distinguished name from a certificate.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> object that represents the name of the certificate subject.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public X500DistinguishedName SubjectName => Impl.SubjectName;

	/// <summary>Gets the thumbprint of a certificate.</summary>
	/// <returns>The thumbprint of the certificate.</returns>
	public string Thumbprint => GetCertHashString();

	/// <summary>Gets the X.509 format version of a certificate.</summary>
	/// <returns>The certificate format.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public int Version => Impl.Version;

	[System.MonoTODO("See comment in X509Helper2.GetMonoCertificate().")]
	internal Mono.Security.X509.X509Certificate MonoCertificate => X509Helper2.GetMonoCertificate(this);

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class.</summary>
	public X509Certificate2()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using information from a byte array.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData)
	{
		Import(rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData, string password)
	{
		Import(rawData, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData, SecureString password)
	{
		Import(rawData, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName)
	{
		Import(fileName, string.Empty, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name and a password used to access the certificate.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, string password)
	{
		Import(fileName, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name and a password.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, SecureString password)
	{
		Import(fileName, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name, a password used to access the certificate, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(fileName, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name, a password, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(fileName, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using an unmanaged handle.</summary>
	/// <param name="handle">A pointer to a certificate context in unmanaged code. The C structure is called PCCERT_CONTEXT.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(IntPtr handle)
		: base(handle)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</summary>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(X509Certificate certificate)
		: base(X509Helper2.Import(certificate))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using the specified serialization and stream context information. </summary>
	/// <param name="info">The serialization information required to deserialize the new <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" />.</param>
	/// <param name="context">Contextual information about the source of the stream to be deserialized.</param>
	protected X509Certificate2(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal X509Certificate2(X509Certificate2Impl impl)
		: base(impl)
	{
	}

	/// <summary>Gets the subject and issuer names from a certificate.</summary>
	/// <returns>The name of the certificate.</returns>
	/// <param name="nameType">The <see cref="T:System.Security.Cryptography.X509Certificates.X509NameType" /> value for the subject. </param>
	/// <param name="forIssuer">true to include the issuer name; otherwise, false. </param>
	[System.MonoTODO("always return String.Empty for UpnName, DnsFromAlternativeName and UrlName")]
	public string GetNameInfo(X509NameType nameType, bool forIssuer)
	{
		return Impl.GetNameInfo(nameType, forIssuer);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with data from a byte array.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	public override void Import(byte[] rawData)
	{
		Import(rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object using data from a byte array, a password, and flags for determining how to import the private key.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	[System.MonoTODO("missing KeyStorageFlags support")]
	public override void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		X509Certificate2Impl x509Certificate2Impl = X509Helper2.Import(rawData, password, keyStorageFlags);
		ImportHandle(x509Certificate2Impl);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object using data from a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	[System.MonoTODO("SecureString is incomplete")]
	public override void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(rawData, (string)null, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with information from a certificate file.</summary>
	/// <param name="fileName">The name of a certificate. </param>
	public override void Import(string fileName)
	{
		byte[] rawData = File.ReadAllBytes(fileName);
		Import(rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with information from a certificate file, a password, and a <see cref="T:System.Security.Cryptography.X509Certificates.X509KeyStorageFlags" /> value.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	[System.MonoTODO("missing KeyStorageFlags support")]
	public override void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		byte[] rawData = File.ReadAllBytes(fileName);
		Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with information from a certificate file, a password, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	[System.MonoTODO("SecureString is incomplete")]
	public override void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		byte[] rawData = File.ReadAllBytes(fileName);
		Import(rawData, (string)null, keyStorageFlags);
	}

	[System.MonoTODO("X509ContentType.SerializedCert is not supported")]
	public override byte[] Export(X509ContentType contentType, string password)
	{
		return Impl.Export(contentType, password);
	}

	/// <summary>Resets the state of an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object.</summary>
	public override void Reset()
	{
		friendlyName = string.Empty;
		base.Reset();
	}

	/// <summary>Displays an X.509 certificate in text format.</summary>
	/// <returns>The certificate information.</returns>
	public override string ToString()
	{
		if (!base.IsValid)
		{
			return "System.Security.Cryptography.X509Certificates.X509Certificate2";
		}
		return base.ToString(fVerbose: true);
	}

	/// <summary>Displays an X.509 certificate in text format.</summary>
	/// <returns>The certificate information.</returns>
	/// <param name="verbose">true to display the public key, private key, extensions, and so forth; false to display information that is similar to the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class, including thumbprint, serial number, subject and issuer names, and so on. </param>
	public override string ToString(bool verbose)
	{
		if (!base.IsValid)
		{
			return "System.Security.Cryptography.X509Certificates.X509Certificate2";
		}
		if (!verbose)
		{
			return base.ToString(fVerbose: true);
		}
		string newLine = Environment.NewLine;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("[Version]{0}  V{1}{0}{0}", newLine, Version);
		stringBuilder.AppendFormat("[Subject]{0}  {1}{0}{0}", newLine, base.Subject);
		stringBuilder.AppendFormat("[Issuer]{0}  {1}{0}{0}", newLine, base.Issuer);
		stringBuilder.AppendFormat("[Serial Number]{0}  {1}{0}{0}", newLine, SerialNumber);
		stringBuilder.AppendFormat("[Not Before]{0}  {1}{0}{0}", newLine, NotBefore);
		stringBuilder.AppendFormat("[Not After]{0}  {1}{0}{0}", newLine, NotAfter);
		stringBuilder.AppendFormat("[Thumbprint]{0}  {1}{0}{0}", newLine, Thumbprint);
		stringBuilder.AppendFormat("[Signature Algorithm]{0}  {1}({2}){0}{0}", newLine, SignatureAlgorithm.FriendlyName, SignatureAlgorithm.Value);
		AsymmetricAlgorithm key = PublicKey.Key;
		stringBuilder.AppendFormat("[Public Key]{0}  Algorithm: ", newLine);
		if (key is RSA)
		{
			stringBuilder.Append("RSA");
		}
		else if (key is DSA)
		{
			stringBuilder.Append("DSA");
		}
		else
		{
			stringBuilder.Append(key.ToString());
		}
		stringBuilder.AppendFormat("{0}  Length: {1}{0}  Key Blob: ", newLine, key.KeySize);
		AppendBuffer(stringBuilder, PublicKey.EncodedKeyValue.RawData);
		stringBuilder.AppendFormat("{0}  Parameters: ", newLine);
		AppendBuffer(stringBuilder, PublicKey.EncodedParameters.RawData);
		stringBuilder.Append(newLine);
		return stringBuilder.ToString();
	}

	private static void AppendBuffer(StringBuilder sb, byte[] buffer)
	{
		if (buffer == null)
		{
			return;
		}
		for (int i = 0; i < buffer.Length; i++)
		{
			sb.Append(buffer[i].ToString("x2"));
			if (i < buffer.Length - 1)
			{
				sb.Append(" ");
			}
		}
	}

	/// <summary>Performs a X.509 chain validation using basic validation policy.</summary>
	/// <returns>true if the validation succeeds; false if the validation fails.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	[System.MonoTODO("by default this depends on the incomplete X509Chain")]
	public bool Verify()
	{
		return Impl.Verify(this);
	}

	/// <summary>Indicates the type of certificate contained in a byte array.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> object.</returns>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rawData" /> has a zero length or is null. </exception>
	[System.MonoTODO("Detection limited to Cert, Pfx, Pkcs12, Pkcs7 and Unknown")]
	public static X509ContentType GetCertContentType(byte[] rawData)
	{
		if (rawData == null || rawData.Length == 0)
		{
			throw new ArgumentException("rawData");
		}
		X509ContentType result = X509ContentType.Unknown;
		try
		{
			ASN1 aSN = new ASN1(rawData);
			if (aSN.Tag != 48)
			{
				throw new CryptographicException(global::Locale.GetText("Unable to decode certificate."));
			}
			if (aSN.Count == 0)
			{
				return result;
			}
			if (aSN.Count == 3)
			{
				switch (aSN[0].Tag)
				{
				case 48:
					if (aSN[1].Tag == 48 && aSN[2].Tag == 3)
					{
						result = X509ContentType.Cert;
					}
					break;
				case 2:
					if (aSN[1].Tag == 48 && aSN[2].Tag == 48)
					{
						result = X509ContentType.Pfx;
					}
					break;
				}
			}
			if (aSN[0].Tag == 6 && aSN[0].CompareValue(signedData))
			{
				result = X509ContentType.Pkcs7;
			}
		}
		catch (Exception inner)
		{
			throw new CryptographicException(global::Locale.GetText("Unable to decode certificate."), inner);
		}
		return result;
	}

	/// <summary>Indicates the type of certificate contained in a file.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> object.</returns>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	[System.MonoTODO("Detection limited to Cert, Pfx, Pkcs12 and Unknown")]
	public static X509ContentType GetCertContentType(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (fileName.Length == 0)
		{
			throw new ArgumentException("fileName");
		}
		return GetCertContentType(File.ReadAllBytes(fileName));
	}
}
