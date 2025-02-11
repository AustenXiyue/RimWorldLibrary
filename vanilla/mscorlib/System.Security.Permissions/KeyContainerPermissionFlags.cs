using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Specifies the type of key container access allowed.</summary>
[Serializable]
[Flags]
[ComVisible(true)]
public enum KeyContainerPermissionFlags
{
	/// <summary>No access to a key container.</summary>
	NoFlags = 0,
	/// <summary>Create a key container.</summary>
	Create = 1,
	/// <summary>Open a key container and use the public key.</summary>
	Open = 2,
	/// <summary>Delete a key container.</summary>
	Delete = 4,
	/// <summary>Import a key into a key container.</summary>
	Import = 0x10,
	/// <summary>Export a key from a key container.</summary>
	Export = 0x20,
	/// <summary>Sign a file using a key.</summary>
	Sign = 0x100,
	/// <summary>Decrypt a key container.</summary>
	Decrypt = 0x200,
	/// <summary>View the access control list (ACL) for a key container.</summary>
	ViewAcl = 0x1000,
	/// <summary>Change the access control list (ACL) for a key container. </summary>
	ChangeAcl = 0x2000,
	/// <summary>Create, decrypt, delete, and open a key container; export and import a key; sign files using a key; and view and change the access control list for a key container.</summary>
	AllFlags = 0x3337
}
