namespace System.Security.Cryptography;

/// <summary>Identifies Windows cryptographic object identifier (OID) groups.</summary>
public enum OidGroup
{
	/// <summary>All the groups.</summary>
	All,
	/// <summary>The Windows group that is represented by CRYPT_HASH_ALG_OID_GROUP_ID.</summary>
	HashAlgorithm,
	/// <summary>The Windows group that is represented by CRYPT_ENCRYPT_ALG_OID_GROUP_ID.</summary>
	EncryptionAlgorithm,
	/// <summary>The Windows group that is represented by CRYPT_PUBKEY_ALG_OID_GROUP_ID.</summary>
	PublicKeyAlgorithm,
	/// <summary>The Windows group that is represented by CRYPT_SIGN_ALG_OID_GROUP_ID.</summary>
	SignatureAlgorithm,
	/// <summary>The Windows group that is represented by CRYPT_RDN_ATTR_OID_GROUP_ID.</summary>
	Attribute,
	/// <summary>The Windows group that is represented by CRYPT_EXT_OR_ATTR_OID_GROUP_ID.</summary>
	ExtensionOrAttribute,
	/// <summary>The Windows group that is represented by CRYPT_ENHKEY_USAGE_OID_GROUP_ID.</summary>
	EnhancedKeyUsage,
	/// <summary>The Windows group that is represented by CRYPT_POLICY_OID_GROUP_ID.</summary>
	Policy,
	/// <summary>The Windows group that is represented by CRYPT_TEMPLATE_OID_GROUP_ID.</summary>
	Template,
	/// <summary>The Windows group that is represented by CRYPT_KDF_OID_GROUP_ID.</summary>
	KeyDerivationFunction
}
