namespace System.IO.Pipes;

/// <summary>Defines the access rights to use when you create access and audit rules. </summary>
[Flags]
public enum PipeAccessRights
{
	/// <summary>Specifies the right to read data from the pipe. This does not include the right to read file system attributes, extended file system attributes, or access and audit rules.</summary>
	ReadData = 1,
	/// <summary>Specifies the right to write data to a pipe. This does not include the right to write file system attributes or extended file system attributes.</summary>
	WriteData = 2,
	/// <summary>Specifies the right to create a new pipe. Setting this right also sets the <see cref="F:System.IO.Pipes.PipeAccessRights.Synchronize" /> right.</summary>
	CreateNewInstance = 4,
	/// <summary>Specifies the right to read extended file system attributes from a pipe. This does not include the right to read data, file system attributes, or access and audit rules.</summary>
	ReadExtendedAttributes = 8,
	/// <summary>Specifies the right to write extended file system attributes to a pipe. This does not include the right to write file attributes or data.</summary>
	WriteExtendedAttributes = 0x10,
	/// <summary>Specifies the right to read file system attributes from a pipe. This does not include the right to read data, extended file system attributes, or access and audit rules.</summary>
	ReadAttributes = 0x80,
	/// <summary>Specifies the right to write file system attributes to a pipe. This does not include the right to write data or extended file system attributes.</summary>
	WriteAttributes = 0x100,
	/// <summary>Specifies the right to delete a pipe.</summary>
	Delete = 0x10000,
	/// <summary>Specifies the right to read access and audit rules from the pipe. This does not include the right to read data, file system attributes, or extended file system attributes.</summary>
	ReadPermissions = 0x20000,
	/// <summary>Specifies the right to change the security and audit rules that are associated with a pipe.</summary>
	ChangePermissions = 0x40000,
	/// <summary>Specifies the right to change the owner of a pipe. Note that owners of a pipe have full access to that resource.</summary>
	TakeOwnership = 0x80000,
	/// <summary>Specifies whether the application can wait for a pipe handle to synchronize with the completion of an I/O operation.</summary>
	Synchronize = 0x100000,
	/// <summary>Specifies the right to make changes to the system access control list (SACL).</summary>
	AccessSystemSecurity = 0x1000000,
	/// <summary>Specifies the right to read from the pipe. This right includes the <see cref="F:System.IO.Pipes.PipeAccessRights.ReadAttributes" />, <see cref="F:System.IO.Pipes.PipeAccessRights.ReadData" />, <see cref="F:System.IO.Pipes.PipeAccessRights.ReadExtendedAttributes" />, and <see cref="F:System.IO.Pipes.PipeAccessRights.ReadPermissions" /> rights.</summary>
	Read = 0x20089,
	/// <summary>Specifies the right to write to the pipe. This right includes the <see cref="F:System.IO.Pipes.PipeAccessRights.WriteAttributes" />, <see cref="F:System.IO.Pipes.PipeAccessRights.WriteData" />, and <see cref="F:System.IO.Pipes.PipeAccessRights.WriteExtendedAttributes" /> rights.</summary>
	Write = 0x112,
	/// <summary>Specifies the right to read and write from the pipe. This right includes the <see cref="F:System.IO.Pipes.PipeAccessRights.ReadAttributes" />, <see cref="F:System.IO.Pipes.PipeAccessRights.ReadData" />, <see cref="F:System.IO.Pipes.PipeAccessRights.ReadExtendedAttributes" />, <see cref="F:System.IO.Pipes.PipeAccessRights.ReadPermissions" />, <see cref="F:System.IO.Pipes.PipeAccessRights.WriteAttributes" />, <see cref="F:System.IO.Pipes.PipeAccessRights.WriteData" />, and <see cref="F:System.IO.Pipes.PipeAccessRights.WriteExtendedAttributes" /> rights.</summary>
	ReadWrite = 0x2019B,
	/// <summary>Specifies the right to exert full control over a pipe, and to modify access control and audit rules. This value represents the combination of all rights in this enumeration.</summary>
	FullControl = 0x1F019F
}
