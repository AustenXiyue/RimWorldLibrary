namespace System.IO.MemoryMappedFiles;

/// <summary>Specifies access rights to a memory-mapped file that is not associated with a file on disk.</summary>
[Flags]
public enum MemoryMappedFileRights
{
	/// <summary>The right to read and write to a file with the restriction that write operations will not be seen by other processes.</summary>
	CopyOnWrite = 1,
	/// <summary>The right to add data to a file or remove data from a file.</summary>
	Write = 2,
	/// <summary>The right to open and copy a file as read-only.</summary>
	Read = 4,
	/// <summary>The right to run an application file.</summary>
	Execute = 8,
	/// <summary>The right to delete a file.</summary>
	Delete = 0x10000,
	/// <summary>The right to open and copy access and audit rules from a file. This does not include the right to read data, file system attributes, or extended file system attributes.</summary>
	ReadPermissions = 0x20000,
	/// <summary>The right to change the security and audit rules associated with a file.</summary>
	ChangePermissions = 0x40000,
	/// <summary>The right to change the owner of a file.</summary>
	TakeOwnership = 0x80000,
	/// <summary>The right to open and copy a file, and the right to add data to a file or remove data from a file.</summary>
	ReadWrite = 6,
	/// <summary>The right to open and copy a folder or file as read-only, and to run application files. This right includes the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileRights.Read" /> right and the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileRights.Execute" /> right.</summary>
	ReadExecute = 0xC,
	/// <summary>The right to open and copy a file, the right to add data to a file or remove data from a file, and the right to run an application file.</summary>
	ReadWriteExecute = 0xE,
	/// <summary>The right to exert full control over a file, and to modify access control and audit rules. This value represents the right to do anything with a file and is the combination of all rights in this enumeration.</summary>
	FullControl = 0xF000F,
	/// <summary>The right to get or set permissions on a file.</summary>
	AccessSystemSecurity = 0x1000000
}
