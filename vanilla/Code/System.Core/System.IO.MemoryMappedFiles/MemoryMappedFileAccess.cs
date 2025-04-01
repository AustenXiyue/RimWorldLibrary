namespace System.IO.MemoryMappedFiles;

/// <summary>Specifies access capabilities and restrictions for a memory-mapped file or view. </summary>
[Serializable]
public enum MemoryMappedFileAccess
{
	/// <summary>Read and write access to the file.</summary>
	ReadWrite,
	/// <summary>Read-only access to the file.</summary>
	Read,
	/// <summary>Write-only access to file.</summary>
	Write,
	/// <summary>Read and write access to the file, with the restriction that any write operations will not be seen by other processes. </summary>
	CopyOnWrite,
	/// <summary>Read access to the file that can store and run executable code.</summary>
	ReadExecute,
	/// <summary>Read and write access to the file that can can store and run executable code.</summary>
	ReadWriteExecute
}
