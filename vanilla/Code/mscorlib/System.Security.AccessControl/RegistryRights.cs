namespace System.Security.AccessControl;

/// <summary>Specifies the access control rights that can be applied to registry objects.</summary>
[Flags]
public enum RegistryRights
{
	/// <summary>The right to query the name/value pairs in a registry key.</summary>
	QueryValues = 1,
	/// <summary>The right to create, delete, or set name/value pairs in a registry key.</summary>
	SetValue = 2,
	/// <summary>The right to create subkeys of a registry key.</summary>
	CreateSubKey = 4,
	/// <summary>The right to list the subkeys of a registry key.</summary>
	EnumerateSubKeys = 8,
	/// <summary>The right to request notification of changes on a registry key.</summary>
	Notify = 0x10,
	/// <summary>Reserved for system use.</summary>
	CreateLink = 0x20,
	/// <summary>The right to delete a registry key.</summary>
	Delete = 0x10000,
	/// <summary>The right to open and copy the access rules and audit rules for a registry key.</summary>
	ReadPermissions = 0x20000,
	/// <summary>The right to create, delete, and set the name/value pairs in a registry key, to create or delete subkeys, to request notification of changes, to enumerate its subkeys, and to read its access rules and audit rules.</summary>
	WriteKey = 0x20006,
	/// <summary>The right to query the name/value pairs in a registry key, to request notification of changes, to enumerate its subkeys, and to read its access rules and audit rules.</summary>
	ReadKey = 0x20019,
	/// <summary>Same as <see cref="F:System.Security.AccessControl.RegistryRights.ReadKey" />.</summary>
	ExecuteKey = 0x20019,
	/// <summary>The right to change the access rules and audit rules associated with a registry key.</summary>
	ChangePermissions = 0x40000,
	/// <summary>The right to change the owner of a registry key.</summary>
	TakeOwnership = 0x80000,
	/// <summary>The right to exert full control over a registry key, and to modify its access rules and audit rules.</summary>
	FullControl = 0xF003F
}
