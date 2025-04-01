namespace System.Security.AccessControl;

/// <summary>Specifies the access control rights that can be applied to named system mutex objects.</summary>
[Flags]
public enum MutexRights
{
	/// <summary>The right to release a named mutex.</summary>
	Modify = 1,
	/// <summary>The right to delete a named mutex.</summary>
	Delete = 0x10000,
	/// <summary>The right to open and copy the access rules and audit rules for a named mutex.</summary>
	ReadPermissions = 0x20000,
	/// <summary>The right to change the security and audit rules associated with a named mutex.</summary>
	ChangePermissions = 0x40000,
	/// <summary>The right to change the owner of a named mutex.</summary>
	TakeOwnership = 0x80000,
	/// <summary>The right to wait on a named mutex.</summary>
	Synchronize = 0x100000,
	/// <summary>The right to exert full control over a named mutex, and to modify its access rules and audit rules.</summary>
	FullControl = 0x1F0001
}
