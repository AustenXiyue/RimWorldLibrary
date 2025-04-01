using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> class represents information about a CMS/PKCS #7 message recipient. The <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> class is an abstract class inherited by the <see cref="T:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo" /> and <see cref="T:System.Security.Cryptography.Pkcs.KeyTransRecipientInfo" /> classes.</summary>
public abstract class RecipientInfo
{
	private RecipientInfoType _type;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfo.EncryptedKey" /> abstract property retrieves the encrypted recipient keying material.</summary>
	/// <returns>An array of byte values that contain the encrypted recipient keying material.</returns>
	public abstract byte[] EncryptedKey { get; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfo.KeyEncryptionAlgorithm" /> abstract property retrieves the algorithm used to perform the key establishment.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that contains the value of the algorithm used to establish the key between the originator and recipient of the CMS/PKCS #7 message.</returns>
	public abstract AlgorithmIdentifier KeyEncryptionAlgorithm { get; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfo.RecipientIdentifier" /> abstract property retrieves the identifier of the recipient.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifier" /> object that contains the identifier of the recipient.</returns>
	public abstract SubjectIdentifier RecipientIdentifier { get; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfo.Type" /> property retrieves the type of the recipient. The type of the recipient determines which of two major protocols is used to establish a key between the originator and the recipient of a CMS/PKCS #7 message.</summary>
	/// <returns>A value of the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoType" /> enumeration that defines the type of the recipient.</returns>
	public RecipientInfoType Type => _type;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfo.Version" /> abstract property retrieves the version of the recipient information. Derived classes automatically set this property for their objects, and the value indicates whether it is using PKCS #7 or Cryptographic Message Syntax (CMS) to protect messages. The version also implies whether the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object establishes a cryptographic key by a key agreement algorithm or a key transport algorithm.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the version of the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object.</returns>
	public abstract int Version { get; }

	internal RecipientInfo(RecipientInfoType recipInfoType)
	{
		_type = recipInfoType;
	}

	internal RecipientInfo()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
