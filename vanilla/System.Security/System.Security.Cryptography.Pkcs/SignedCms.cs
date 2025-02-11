using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using Mono.Security;
using Mono.Security.X509;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class enables signing and verifying of CMS/PKCS #7 messages. </summary>
public sealed class SignedCms
{
	private ContentInfo _content;

	private bool _detached;

	private SignerInfoCollection _info;

	private X509Certificate2Collection _certs;

	private SubjectIdentifierType _type;

	private int _version;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.Certificates" /> property retrieves the certificates associated with the encoded CMS/PKCS #7 message.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents the set of certificates for the encoded CMS/PKCS #7 message.</returns>
	public X509Certificate2Collection Certificates => _certs;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.ContentInfo" /> property retrieves the inner contents of the encoded CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the contents of the encoded CMS/PKCS #7 message.</returns>
	public ContentInfo ContentInfo
	{
		get
		{
			if (_content == null)
			{
				Oid contentType = new Oid("1.2.840.113549.1.7.1");
				_content = new ContentInfo(contentType, new byte[0]);
			}
			return _content;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.Detached" /> property retrieves whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature. If this property is true, the signature is detached. If this property is false, the signature is not detached.</returns>
	public bool Detached => _detached;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.SignerInfos" /> property retrieves the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection associated with the CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> object that represents the signer information for the CMS/PKCS #7 message.</returns>
	public SignerInfoCollection SignerInfos => _info;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.Version" /> property retrieves the version of the CMS/PKCS #7 message.</summary>
	/// <returns>An int value that represents the CMS/PKCS #7 message version.</returns>
	public int Version => _version;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class.</summary>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms()
	{
		_certs = new X509Certificate2Collection();
		_info = new SignerInfoCollection();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified content information as the inner content.</summary>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(ContentInfo contentInfo)
		: this(contentInfo, detached: false)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo,System.Boolean)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified content information as the inner content and by using the detached state.</summary>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <param name="detached">A <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature. If <paramref name="detached" /> is true, the signature is detached. If <paramref name="detached" /> is false, the signature is not detached.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(ContentInfo contentInfo, bool detached)
		: this()
	{
		if (contentInfo == null)
		{
			throw new ArgumentNullException("contentInfo");
		}
		_content = contentInfo;
		_detached = detached;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified subject identifier type as the default subject identifier type for signers.</summary>
	/// <param name="signerIdentifierType">A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> member that specifies the default subject identifier type for signers.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(SubjectIdentifierType signerIdentifierType)
		: this()
	{
		_type = signerIdentifierType;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified subject identifier type as the default subject identifier type for signers and content information as the inner content.</summary>
	/// <param name="signerIdentifierType">A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> member that specifies the default subject identifier type for signers.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(SubjectIdentifierType signerIdentifierType, ContentInfo contentInfo)
		: this(contentInfo, detached: false)
	{
		_type = signerIdentifierType;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo,System.Boolean)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified subject identifier type as the default subject identifier type for signers, the content information as the inner content, and by using the detached state.</summary>
	/// <param name="signerIdentifierType">A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> member that specifies the default subject identifier type for signers.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <param name="detached">A <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature. If <paramref name="detached" /> is true, the signature is detached. If detached is false, the signature is not detached.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	public SignedCms(SubjectIdentifierType signerIdentifierType, ContentInfo contentInfo, bool detached)
		: this(contentInfo, detached)
	{
		_type = signerIdentifierType;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Boolean)" /> method verifies the digital signatures on the signed CMS/PKCS #7 message and, optionally, validates the signers' certificates.</summary>
	/// <param name="verifySignatureOnly">A <see cref="T:System.Boolean" /> value that specifies whether only the digital signatures are verified without the signers' certificates being validated. If <paramref name="verifySignatureOnly" /> is true, only the digital signatures are verified. If it is false, the digital signatures are verified, the signers' certificates are validated, and the purposes of the certificates are validated. The purposes of a certificate are considered valid if the certificate has no key usage or if the key usage supports digital signatures or nonrepudiation.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException"> A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void CheckSignature(bool verifySignatureOnly)
	{
		SignerInfoEnumerator enumerator = _info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.CheckSignature(verifySignatureOnly);
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" /> method verifies the digital signatures on the signed CMS/PKCS #7 message by using the specified collection of certificates and, optionally, validates the signers' certificates.</summary>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object that can be used to validate the certificate chain. If no additional certificates are to be used to validate the certificate chain, use <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Boolean)" /> instead of <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" />.</param>
	/// <param name="verifySignatureOnly">A <see cref="T:System.Boolean" /> value that specifies whether only the digital signatures are verified without the signers' certificates being validated. If <paramref name="verifySignatureOnly" /> is true, only the digital signatures are verified. If it is false, the digital signatures are verified, the signers' certificates are validated, and the purposes of the certificates are validated. The purposes of a certificate are considered valid if the certificate has no key usage or if the key usage supports digital signatures or nonrepudiation.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void CheckSignature(X509Certificate2Collection extraStore, bool verifySignatureOnly)
	{
		SignerInfoEnumerator enumerator = _info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.CheckSignature(extraStore, verifySignatureOnly);
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckHash" /> method verifies the data integrity of the CMS/PKCS #7 message. <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckHash" /> is a specialized method used in specific security infrastructure applications that only wish to check the hash of the CMS message, rather than perform a full digital signature verification. <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckHash" /> does not authenticate the author nor sender of the message because this method does not involve verifying a digital signature. For general-purpose checking of the integrity and authenticity of a CMS/PKCS #7 message, use the <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Boolean)" /> or <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" /> methods.</summary>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void CheckHash()
	{
		throw new InvalidOperationException("");
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.ComputeSignature" /> method prompts the user to select a signing certificate, creates a signature, and adds the signature to the CMS/PKCS #7 message.</summary>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.StorePermission, System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Flags="CreateStore, DeleteStore, OpenStore, EnumerateCertificates" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void ComputeSignature()
	{
		throw new CryptographicException("");
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.ComputeSignature(System.Security.Cryptography.Pkcs.CmsSigner)" /> method creates a signature using the specified signer and adds the signature to the CMS/PKCS #7 message.</summary>
	/// <param name="signer">A <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object that represents the signer.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void ComputeSignature(CmsSigner signer)
	{
		ComputeSignature();
	}

	/// <summary>Creates a signature using the specified signer and adds the signature to the CMS/PKCS #7 message. If the value of the silent parameter is false and the <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.Certificate" /> property of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object specified by the signer parameter is not set to a valid certificate, this method prompts the user to select a signing certificate.</summary>
	/// <param name="signer">A <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object that represents the signer.</param>
	/// <param name="silent">false to prompt the user to select a signing certificate.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The value of the <paramref name="silent" /> parameter is true and a signing certificate is not specified.</exception>
	[System.MonoTODO]
	public void ComputeSignature(CmsSigner signer, bool silent)
	{
		ComputeSignature();
	}

	private string ToString(byte[] array, bool reverse)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (reverse)
		{
			for (int num = array.Length - 1; num >= 0; num--)
			{
				stringBuilder.Append(array[num].ToString("X2"));
			}
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("X2"));
			}
		}
		return stringBuilder.ToString();
	}

	private byte[] GetKeyIdentifier(Mono.Security.X509.X509Certificate x509)
	{
		Mono.Security.X509.X509Extension x509Extension = x509.Extensions["2.5.29.14"];
		if (x509Extension != null)
		{
			return new ASN1(x509Extension.Value.Value).Value;
		}
		ASN1 aSN = new ASN1(48);
		ASN1 aSN2 = aSN.Add(new ASN1(48));
		aSN2.Add(new ASN1(CryptoConfig.EncodeOID(x509.KeyAlgorithm)));
		aSN2.Add(new ASN1(x509.KeyAlgorithmParameters));
		byte[] publicKey = x509.PublicKey;
		byte[] array = new byte[publicKey.Length + 1];
		Array.Copy(publicKey, 0, array, 1, publicKey.Length);
		aSN.Add(new ASN1(3, array));
		return SHA1.Create().ComputeHash(aSN.GetBytes());
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.Decode(System.Byte[])" /> method decodes an encoded <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message. Upon successful decoding, the decoded information can be retrieved from the properties of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object.</summary>
	/// <param name="encodedMessage">Array of byte values that represents the encoded CMS/PKCS #7 message to be decoded.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO("incomplete - missing attributes")]
	public void Decode(byte[] encodedMessage)
	{
		PKCS7.ContentInfo contentInfo = new PKCS7.ContentInfo(encodedMessage);
		if (contentInfo.ContentType != "1.2.840.113549.1.7.2")
		{
			throw new Exception("");
		}
		PKCS7.SignedData signedData = new PKCS7.SignedData(contentInfo.Content);
		SubjectIdentifierType type = SubjectIdentifierType.Unknown;
		object o = null;
		X509Certificate2 certificate = null;
		if (signedData.SignerInfo.Certificate != null)
		{
			certificate = new X509Certificate2(signedData.SignerInfo.Certificate.RawData);
		}
		else if (signedData.SignerInfo.IssuerName != null && signedData.SignerInfo.SerialNumber != null)
		{
			byte[] serialNumber = signedData.SignerInfo.SerialNumber;
			Array.Reverse(serialNumber);
			type = SubjectIdentifierType.IssuerAndSerialNumber;
			X509IssuerSerial x509IssuerSerial = default(X509IssuerSerial);
			x509IssuerSerial.IssuerName = signedData.SignerInfo.IssuerName;
			x509IssuerSerial.SerialNumber = ToString(serialNumber, reverse: true);
			o = x509IssuerSerial;
			foreach (Mono.Security.X509.X509Certificate certificate2 in signedData.Certificates)
			{
				if (certificate2.IssuerName == signedData.SignerInfo.IssuerName && ToString(certificate2.SerialNumber, reverse: true) == x509IssuerSerial.SerialNumber)
				{
					certificate = new X509Certificate2(certificate2.RawData);
					break;
				}
			}
		}
		else if (signedData.SignerInfo.SubjectKeyIdentifier != null)
		{
			string text = ToString(signedData.SignerInfo.SubjectKeyIdentifier, reverse: false);
			type = SubjectIdentifierType.SubjectKeyIdentifier;
			o = text;
			foreach (Mono.Security.X509.X509Certificate certificate3 in signedData.Certificates)
			{
				if (ToString(GetKeyIdentifier(certificate3), reverse: false) == text)
				{
					certificate = new X509Certificate2(certificate3.RawData);
					break;
				}
			}
		}
		SignerInfo signer = new SignerInfo(signedData.SignerInfo.HashName, certificate, type, o, signedData.SignerInfo.Version);
		_info.Add(signer);
		ASN1 content = signedData.ContentInfo.Content;
		Oid contentType = new Oid(signedData.ContentInfo.ContentType);
		if (!_detached || _content == null)
		{
			if (content[0] == null)
			{
				throw new ArgumentException("ContentInfo has no content. Detached signature ?");
			}
			_content = new ContentInfo(contentType, content[0].Value);
		}
		foreach (Mono.Security.X509.X509Certificate certificate4 in signedData.Certificates)
		{
			_certs.Add(new X509Certificate2(certificate4.RawData));
		}
		_version = signedData.Version;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.Encode" /> method encodes the information in the object into a CMS/PKCS #7 message.</summary>
	/// <returns>An array of byte values that represents the encoded message. The encoded message can be decoded by the <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.Decode(System.Byte[])" /> method.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public byte[] Encode()
	{
		return null;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.RemoveSignature(System.Security.Cryptography.Pkcs.SignerInfo)" /> method removes the signature for the specified <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object.</summary>
	/// <param name="signerInfo">A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object that represents the countersignature being removed.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void RemoveSignature(SignerInfo signerInfo)
	{
	}

	/// <summary>Removes the signature at the specified index of the <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.SignerInfos" /> collection. </summary>
	/// <param name="index">The zero-based index of the signature to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">A CMS/PKCS #7 message is not signed, and <paramref name="index" /> is invalid.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than the signature count minus 1.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The signature could not be removed.-or-An internal cryptographic error occurred.</exception>
	[System.MonoTODO]
	public void RemoveSignature(int index)
	{
	}
}
