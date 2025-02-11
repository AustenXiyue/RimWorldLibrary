using System.Security.Cryptography.X509Certificates;
using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> class represents a signer associated with a <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object that represents a CMS/PKCS #7 message.</summary>
public sealed class SignerInfo
{
	private SubjectIdentifier _signer;

	private X509Certificate2 _certificate;

	private Oid _digest;

	private SignerInfoCollection _counter;

	private CryptographicAttributeObjectCollection _signed;

	private CryptographicAttributeObjectCollection _unsigned;

	private int _version;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.SignedAttributes" /> property retrieves the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection of signed attributes that is associated with the signer information. Signed attributes are signed along with the rest of the message content.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the signed attributes. If there are no signed attributes, the property is an empty collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CryptographicAttributeObjectCollection SignedAttributes => _signed;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.Certificate" /> property retrieves the signing certificate associated with the signer information.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public X509Certificate2 Certificate => _certificate;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.CounterSignerInfos" /> property retrieves the set of counter signers associated with the signer information. </summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection that represents the counter signers for the signer information. If there are no counter signers, the property is an empty collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SignerInfoCollection CounterSignerInfos => _counter;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.DigestAlgorithm" /> property retrieves the <see cref="T:System.Security.Cryptography.Oid" /> object that represents the hash algorithm used in the computation of the signatures.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" /> object that represents the hash algorithm used with the signature.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Oid DigestAlgorithm => _digest;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.SignerIdentifier" /> property retrieves the certificate identifier of the signer associated with the signer information.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifier" /> object that uniquely identifies the certificate associated with the signer information.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SubjectIdentifier SignerIdentifier => _signer;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.UnsignedAttributes" /> property retrieves the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection of unsigned attributes that is associated with the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> content. Unsigned attributes can be modified without invalidating the signature.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the unsigned attributes. If there are no unsigned attributes, the property is an empty collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CryptographicAttributeObjectCollection UnsignedAttributes => _unsigned;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.Version" /> property retrieves the signer information version.</summary>
	/// <returns>An int value that specifies the signer information version.</returns>
	public int Version => _version;

	internal SignerInfo(string hashName, X509Certificate2 certificate, SubjectIdentifierType type, object o, int version)
	{
		_digest = new Oid(CryptoConfig.MapNameToOID(hashName));
		_certificate = certificate;
		_counter = new SignerInfoCollection();
		_signed = new CryptographicAttributeObjectCollection();
		_unsigned = new CryptographicAttributeObjectCollection();
		_signer = new SubjectIdentifier(type, o);
		_version = version;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckHash" /> method verifies the data integrity of the CMS/PKCS #7 message signer information. <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckHash" /> is a specialized method used in specific security infrastructure applications in which the subject uses the HashOnly member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration when setting up a <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object. <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckHash" /> does not authenticate the signer information because this method does not involve verifying a digital signature. For general-purpose checking of the integrity and authenticity of CMS/PKCS #7 message signer information and countersignatures, use the <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckSignature(System.Boolean)" /> or <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" /> methods.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void CheckHash()
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckSignature(System.Boolean)" /> method verifies the digital signature of the message and, optionally, validates the certificate.</summary>
	/// <param name="verifySignatureOnly">A bool value that specifies whether only the digital signature is verified. If <paramref name="verifySignatureOnly" /> is true, only the signature is verified. If <paramref name="verifySignatureOnly" /> is false, the digital signature is verified, the certificate chain is validated, and the purposes of the certificates are validated. The purposes of the certificate are considered valid if the certificate has no key usage or if the key usage supports digital signature or nonrepudiation.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void CheckSignature(bool verifySignatureOnly)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" /> method verifies the digital signature of the message by using the specified collection of certificates and, optionally, validates the certificate.</summary>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object that can be used to validate the chain. If no additional certificates are to be used to validate the chain, use <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckSignature(System.Boolean)" /> instead of <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" />.</param>
	/// <param name="verifySignatureOnly">A bool value that specifies whether only the digital signature is verified. If <paramref name="verifySignatureOnly" /> is true, only the signature is verified. If <paramref name="verifySignatureOnly" /> is false, the digital signature is verified, the certificate chain is validated, and the purposes of the certificates are validated. The purposes of the certificate are considered valid if the certificate has no key usage or if the key usage supports digital signature or nonrepudiation.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void CheckSignature(X509Certificate2Collection extraStore, bool verifySignatureOnly)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.ComputeCounterSignature" /> method prompts the user to select a signing certificate, creates a countersignature, and adds the signature to the CMS/PKCS #7 message. Countersignatures are restricted to one level.</summary>
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
	public void ComputeCounterSignature()
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.ComputeCounterSignature(System.Security.Cryptography.Pkcs.CmsSigner)" /> method creates a countersignature by using the specified signer and adds the signature to the CMS/PKCS #7 message. Countersignatures are restricted to one level.</summary>
	/// <param name="signer">A <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object that represents the counter signer.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void ComputeCounterSignature(CmsSigner signer)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.RemoveCounterSignature(System.Security.Cryptography.Pkcs.SignerInfo)" /> method removes the countersignature for the specified <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object.</summary>
	/// <param name="counterSignerInfo">A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object that represents the countersignature being removed.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void RemoveCounterSignature(SignerInfo counterSignerInfo)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfo.RemoveCounterSignature(System.Int32)" /> method removes the countersignature at the specified index of the <see cref="P:System.Security.Cryptography.Pkcs.SignerInfo.CounterSignerInfos" /> collection.</summary>
	/// <param name="index">The zero-based index of the countersignature to remove.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void RemoveCounterSignature(int index)
	{
	}

	internal SignerInfo()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
