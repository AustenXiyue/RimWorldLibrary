using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.KeyTransRecipientInfo" /> class defines key transport recipient information.        Key transport algorithms typically use the RSA algorithm, in which  an originator establishes a shared cryptographic key with a recipient by generating that key and  then transporting it to the recipient. This is in contrast to key agreement algorithms, in which the two parties that will be using a cryptographic key both take part in its generation, thereby mutually agreeing to that key.</summary>
public sealed class KeyTransRecipientInfo : RecipientInfo
{
	private byte[] _encryptedKey;

	private AlgorithmIdentifier _keyEncryptionAlgorithm;

	private SubjectIdentifier _recipientIdentifier;

	private int _version;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyTransRecipientInfo.EncryptedKey" /> property retrieves the encrypted key for this key transport recipient.</summary>
	/// <returns>An array of byte values that represents the encrypted key.</returns>
	public override byte[] EncryptedKey => _encryptedKey;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyTransRecipientInfo.KeyEncryptionAlgorithm" /> property retrieves the key encryption algorithm used to encrypt the content encryption key.</summary>
	/// <returns> An  <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" />  object that stores the key encryption algorithm identifier.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override AlgorithmIdentifier KeyEncryptionAlgorithm => _keyEncryptionAlgorithm;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyTransRecipientInfo.RecipientIdentifier" /> property retrieves the subject identifier associated with the encrypted content.</summary>
	/// <returns>A   <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifier" />  object that  stores the identifier of the recipient taking part in the key transport.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override SubjectIdentifier RecipientIdentifier => _recipientIdentifier;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyTransRecipientInfo.Version" /> property retrieves the version of the key transport recipient. The version of the key transport recipient is automatically set for  objects in this class, and the value  implies that the recipient is taking part in a key transport algorithm.</summary>
	/// <returns>An int value that represents the version of the key transport <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object.</returns>
	public override int Version => _version;

	internal KeyTransRecipientInfo(byte[] encryptedKey, AlgorithmIdentifier keyEncryptionAlgorithm, SubjectIdentifier recipientIdentifier, int version)
		: base(RecipientInfoType.KeyTransport)
	{
		_encryptedKey = encryptedKey;
		_keyEncryptionAlgorithm = keyEncryptionAlgorithm;
		_recipientIdentifier = recipientIdentifier;
		_version = version;
	}

	internal KeyTransRecipientInfo()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
