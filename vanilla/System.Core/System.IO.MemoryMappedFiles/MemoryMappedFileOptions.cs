namespace System.IO.MemoryMappedFiles;

/// <summary>Provides memory allocation options for memory-mapped files.</summary>
[Serializable]
[Flags]
public enum MemoryMappedFileOptions
{
	/// <summary>No memory allocation options are applied.</summary>
	None = 0,
	/// <summary>Memory allocation is delayed until a view is created with either the <see cref="M:System.IO.MemoryMappedFiles.MemoryMappedFile.CreateViewAccessor" /> or <see cref="M:System.IO.MemoryMappedFiles.MemoryMappedFile.CreateViewStream" /> method.</summary>
	DelayAllocatePages = 0x4000000
}
