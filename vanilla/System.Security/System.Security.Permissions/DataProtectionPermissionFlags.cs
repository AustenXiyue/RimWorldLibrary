namespace System.Security.Permissions;

/// <summary>Specifies the access permissions for encrypting data and memory.</summary>
[Serializable]
[Flags]
public enum DataProtectionPermissionFlags
{
	/// <summary>No protection abilities.</summary>
	NoFlags = 0,
	/// <summary>The ability to encrypt data.</summary>
	ProtectData = 1,
	/// <summary>The ability to unencrypt data.</summary>
	UnprotectData = 2,
	/// <summary>The ability to encrypt memory.</summary>
	ProtectMemory = 4,
	/// <summary>The ability to unencrypt memory.</summary>
	UnprotectMemory = 8,
	/// <summary>The ability to encrypt data, encrypt memory, unencrypt data, and unencrypt memory.</summary>
	AllFlags = 0xF
}
