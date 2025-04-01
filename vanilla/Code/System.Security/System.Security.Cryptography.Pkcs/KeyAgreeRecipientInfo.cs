namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo" /> class defines key agreement recipient information. Key agreement algorithms typically use the Diffie-Hellman key agreement algorithm, in which the two parties that establish a shared cryptographic key both take part in its generation and, by definition, agree on that key. This is in contrast to key transport algorithms, in which one party generates the key unilaterally and sends, or transports it, to the other party.</summary>
[System.MonoTODO]
public sealed class KeyAgreeRecipientInfo : RecipientInfo
{
	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.Date" /> property retrieves the date and time of the start of the key agreement protocol by the originator.</summary>
	/// <returns>The date and time of the start of the key agreement protocol by the originator.</returns>
	/// <exception cref="T:System.InvalidOperationException">The recipient identifier type is not a subject key identifier.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DateTime Date => DateTime.MinValue;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.EncryptedKey" /> property retrieves the encrypted recipient keying material.</summary>
	/// <returns>An array of byte values that contain the encrypted recipient keying material.</returns>
	public override byte[] EncryptedKey => null;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.KeyEncryptionAlgorithm" /> property retrieves the algorithm used to perform the key agreement.</summary>
	/// <returns>The value of the algorithm used to perform the key agreement.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override AlgorithmIdentifier KeyEncryptionAlgorithm => null;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.OriginatorIdentifierOrKey" /> property retrieves information about the originator of the key agreement for key agreement algorithms that warrant it.</summary>
	/// <returns>An object that contains information about the originator of the key agreement.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SubjectIdentifierOrKey OriginatorIdentifierOrKey => null;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.OtherKeyAttribute" /> property retrieves attributes of the keying material.</summary>
	/// <returns>The attributes of the keying material.</returns>
	/// <exception cref="T:System.InvalidOperationException">The recipient identifier type is not a subject key identifier.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CryptographicAttributeObject OtherKeyAttribute => null;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.RecipientIdentifier" /> property retrieves the identifier of the recipient.</summary>
	/// <returns>The identifier of the recipient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override SubjectIdentifier RecipientIdentifier => null;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo.Version" /> property retrieves the version of the key agreement recipient. This is automatically set for  objects in this class, and the value  implies that the recipient is taking part in a key agreement algorithm.</summary>
	/// <returns>The version of the <see cref="T:System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo" /> object.</returns>
	public override int Version => 0;

	internal KeyAgreeRecipientInfo()
		: base(RecipientInfoType.KeyAgreement)
	{
	}
}
