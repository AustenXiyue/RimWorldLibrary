namespace System.Security.AccessControl;

/// <summary>Specifies the cryptographic key operation for which an authorization rule controls access or auditing.</summary>
[Flags]
public enum CryptoKeyRights
{
	/// <summary>Read the key data.</summary>
	ReadData = 1,
	/// <summary>Write key data.</summary>
	WriteData = 2,
	/// <summary>Read extended attributes of the key.</summary>
	ReadExtendedAttributes = 8,
	/// <summary>Write extended attributes of the key.</summary>
	WriteExtendedAttributes = 0x10,
	/// <summary>Read attributes of the key.</summary>
	ReadAttributes = 0x80,
	/// <summary>Write attributes of the key.</summary>
	WriteAttributes = 0x100,
	/// <summary>Delete the key.</summary>
	Delete = 0x10000,
	/// <summary>Read permissions for the key.</summary>
	ReadPermissions = 0x20000,
	/// <summary>Change permissions for the key.</summary>
	ChangePermissions = 0x40000,
	/// <summary>Take ownership of the key.</summary>
	TakeOwnership = 0x80000,
	/// <summary>Use the key for synchronization.</summary>
	Synchronize = 0x100000,
	/// <summary>Full control of the key.</summary>
	FullControl = 0x1F019B,
	/// <summary>A combination of <see cref="F:System.Security.AccessControl.CryptoKeyRights.GenericRead" /> and <see cref="F:System.Security.AccessControl.CryptoKeyRights.GenericWrite" />.</summary>
	GenericAll = 0x10000000,
	/// <summary>Not used.</summary>
	GenericExecute = 0x20000000,
	/// <summary>Write the key data, extended attributes of the key, attributes of the key, and permissions for the key.</summary>
	GenericWrite = 0x40000000,
	/// <summary>Read the key data, extended attributes of the key, attributes of the key, and permissions for the key.</summary>
	GenericRead = int.MinValue
}
