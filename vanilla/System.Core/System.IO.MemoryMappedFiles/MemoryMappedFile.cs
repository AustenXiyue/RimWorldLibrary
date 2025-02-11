using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles;

/// <summary>Represents a memory-mapped file. </summary>
public class MemoryMappedFile : IDisposable
{
	private FileStream stream;

	private bool keepOpen;

	private SafeMemoryMappedFileHandle handle;

	/// <summary>Gets the file handle of a memory-mapped file.</summary>
	/// <returns>The handle to the memory-mapped file.</returns>
	public SafeMemoryMappedFileHandle SafeMemoryMappedFileHandle => handle;

	/// <summary>Creates a memory-mapped file from a file on disk.</summary>
	/// <returns>A memory-mapped file.</returns>
	/// <param name="path">The path to file to map.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string, contains only white space, or has one or more invalid characters, as defined by the <see cref="M:System.IO.Path.GetInvalidFileNameChars" /> method. -or-<paramref name="path" /> refers to an invalid device. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the maximum length defined by the operating system. In Windows, paths must contain fewer than 248 characters, and file names must contain fewer than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions for the file.</exception>
	public static MemoryMappedFile CreateFromFile(string path)
	{
		return CreateFromFile(path, FileMode.Open, null, 0L, MemoryMappedFileAccess.ReadWrite);
	}

	/// <summary>Creates a memory-mapped file that has the specified access mode from a file on disk. </summary>
	/// <returns>A memory-mapped file that has the specified access mode.</returns>
	/// <param name="path">The path to file to map.</param>
	/// <param name="mode">Access mode; must be <see cref="F:System.IO.FileMode.Open" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string, contains only white space, or has one or more invalid characters, as defined by the <see cref="M:System.IO.Path.GetInvalidFileNameChars" /> method. -or-<paramref name="path" /> refers to an invalid device.-or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Append" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" />, <see cref="F:System.IO.FileMode.CreateNew" />, or <see cref="F:System.IO.FileMode.Truncate" />.-or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.OpenOrCreate" /> and the file on disk does not exist.-or-An I/O error occurred.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the maximum length defined by the operating system. In Windows, paths must contain fewer than 248 characters, and file names must contain fewer than 260 characters. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions for the file.</exception>
	public static MemoryMappedFile CreateFromFile(string path, FileMode mode)
	{
		long capacity = 0L;
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("path");
		}
		if (mode == FileMode.Append)
		{
			throw new ArgumentException("mode");
		}
		IntPtr preexistingHandle = MemoryMapImpl.OpenFile(path, mode, null, out capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None);
		return new MemoryMappedFile
		{
			handle = new SafeMemoryMappedFileHandle(preexistingHandle, ownsHandle: true)
		};
	}

	/// <summary>Creates a memory-mapped file that has the specified access mode and name from a file on disk.</summary>
	/// <returns>A memory-mapped file that has the specified name and access mode.</returns>
	/// <param name="path">The path to the file to map.</param>
	/// <param name="mode">Access mode; must be <see cref="F:System.IO.FileMode.Open" />.</param>
	/// <param name="mapName">A name to assign to the memory-mapped file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string, contains only white space, or has one or more invalid characters, as defined by the <see cref="M:System.IO.Path.GetInvalidFileNameChars" /> method. -or-<paramref name="path" /> refers to an invalid device.-or-<paramref name="mapName" /> is an empty string.-or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Append" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" />, <see cref="F:System.IO.FileMode.CreateNew" />, or <see cref="F:System.IO.FileMode.Truncate" />.-or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.OpenOrCreate" /> and the file on disk does not exist.-or-An I/O error occurred.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the maximum length defined by the operating system. In Windows, paths must contain fewer than 248 characters, and file names must contain fewer than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions for the file.</exception>
	public static MemoryMappedFile CreateFromFile(string path, FileMode mode, string mapName)
	{
		return CreateFromFile(path, mode, mapName, 0L, MemoryMappedFileAccess.ReadWrite);
	}

	/// <summary>Creates a memory-mapped file that has the specified access mode, name, and capacity from a file on disk.</summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="path">The path to the file to map.</param>
	/// <param name="mode">Access mode; can be any of the <see cref="T:System.IO.FileMode" /> enumeration values except <see cref="F:System.IO.FileMode.Append" />.</param>
	/// <param name="mapName">A name to assign to the memory-mapped file. </param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file. Specify 0 to set the capacity to the size of the file on disk.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string, contains only white space, or has one or more invalid characters, as defined by the <see cref="M:System.IO.Path.GetInvalidFileNameChars" /> method. -or-<paramref name="path" /> refers to an invalid device.-or-<paramref name="mapName" /> is an empty string.-or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Append" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is greater than the size of the logical address space.-or-<paramref name="capacity" /> is less than zero.-or-<paramref name="capacity" /> is less than the file size (but not zero).-or-<paramref name="capacity" /> is zero, and the size of the file on disk is also zero.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the maximum length defined by the operating system. In Windows, paths must contain fewer than 248 characters, and file names must contain fewer than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions for the file.</exception>
	public static MemoryMappedFile CreateFromFile(string path, FileMode mode, string mapName, long capacity)
	{
		return CreateFromFile(path, mode, mapName, capacity, MemoryMappedFileAccess.ReadWrite);
	}

	/// <summary>Creates a memory-mapped file that has the specified access mode, name, capacity, and access type from a file on disk.</summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="path">The path to the file to map.</param>
	/// <param name="mode">Access mode; can be any of the <see cref="T:System.IO.FileMode" /> enumeration values except <see cref="F:System.IO.FileMode.Append" />.</param>
	/// <param name="mapName">A name to assign to the memory-mapped file. </param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file. Specify 0 to set the capacity to the size of the file on disk.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.-or-<paramref name="access" /> is not an allowed value.-or-<paramref name="path" /> specifies an empty file.-or-<paramref name="access" /> is specified as <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read" /> and capacity is greater than the size of the file indicated by <paramref name="path" />.-or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Append" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="mapName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is greater than the size of the logical address space.-or-<paramref name="capacity" /> is less than zero.-or-<paramref name="capacity" /> is less than the file size (but not zero).-or-<paramref name="capacity" /> is zero, and the size of the file on disk is also zero.-or-<paramref name="access" /> is not a defined <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> value.-or-The size of the file indicated by <paramref name="path" /> is greater than <paramref name="capacity" />.</exception>
	/// <exception cref="T:System.IO.IOException">-or-An I/O error occurred.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the maximum length defined by the operating system. In Windows, paths must contain fewer than 248 characters, and file names must contain fewer than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions for the file.</exception>
	public static MemoryMappedFile CreateFromFile(string path, FileMode mode, string mapName, long capacity, MemoryMappedFileAccess access)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("path");
		}
		if (mapName != null && mapName.Length == 0)
		{
			throw new ArgumentException("mapName");
		}
		if (mode == FileMode.Append)
		{
			throw new ArgumentException("mode");
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		IntPtr preexistingHandle = MemoryMapImpl.OpenFile(path, mode, mapName, out capacity, access, MemoryMappedFileOptions.None);
		return new MemoryMappedFile
		{
			handle = new SafeMemoryMappedFileHandle(preexistingHandle, ownsHandle: true)
		};
	}

	public static MemoryMappedFile CreateFromFile(FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access, HandleInheritability inheritability, bool leaveOpen)
	{
		if (fileStream == null)
		{
			throw new ArgumentNullException("fileStream");
		}
		if (mapName != null && mapName.Length == 0)
		{
			throw new ArgumentException("mapName");
		}
		if ((!MonoUtil.IsUnix && capacity == 0L && fileStream.Length == 0L) || capacity > fileStream.Length)
		{
			throw new ArgumentException("capacity");
		}
		IntPtr preexistingHandle = MemoryMapImpl.OpenHandle(fileStream.SafeFileHandle.DangerousGetHandle(), mapName, out capacity, access, MemoryMappedFileOptions.None);
		MemoryMapImpl.ConfigureHandleInheritability(preexistingHandle, inheritability);
		return new MemoryMappedFile
		{
			handle = new SafeMemoryMappedFileHandle(preexistingHandle, ownsHandle: true),
			stream = fileStream,
			keepOpen = leaveOpen
		};
	}

	/// <summary>Creates a memory-mapped file that has the specified name, capacity, access type, security permissions, inheritability, and disposal requirement from a file on disk. </summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="fileStream">The <paramref name="fileStream" /> to the file to map.</param>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file. Specify 0 to set the capacity to the size of the file on disk.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.This parameter cannot be set to <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read" /> or <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" />. </param>
	/// <param name="memoryMappedFileSecurity">The permissions that can be granted for file access and operations on memory-mapped files.This parameter can be null.</param>
	/// <param name="inheritability">One of the enumeration values that specifies whether a handle to the memory-mapped file can be inherited by a child process. The default is <see cref="F:System.IO.HandleInheritability.None" />.</param>
	/// <param name="leaveOpen">true to not dispose <paramref name="fileStream" /> after the <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFile" /> is closed; false to dispose <paramref name="fileStream" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.-or-<paramref name="capacity" /> and the length of the file are zero.-or-<paramref name="access" /> is set to the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read" /> or <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> enumeration value, which is not allowed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileStream" /> or <paramref name="mapname" />  is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or-<paramref name="capacity" /> is less than the file size.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> enumeration value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <paramref name="fileStream" /> was closed.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="access" /> is set to <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" /> when <paramref name="fileStream" />'s access is set to <see cref="F:System.IO.FileAccess.Read" /> or <see cref="F:System.IO.FileAccess.Write" />. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="mapName" /> already exists.</exception>
	[MonoLimitation("memoryMappedFileSecurity is currently ignored")]
	public static MemoryMappedFile CreateFromFile(FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability, bool leaveOpen)
	{
		if (fileStream == null)
		{
			throw new ArgumentNullException("fileStream");
		}
		if (mapName != null && mapName.Length == 0)
		{
			throw new ArgumentException("mapName");
		}
		if ((!MonoUtil.IsUnix && capacity == 0L && fileStream.Length == 0L) || capacity > fileStream.Length)
		{
			throw new ArgumentException("capacity");
		}
		IntPtr preexistingHandle = MemoryMapImpl.OpenHandle(fileStream.SafeFileHandle.DangerousGetHandle(), mapName, out capacity, access, MemoryMappedFileOptions.None);
		MemoryMapImpl.ConfigureHandleInheritability(preexistingHandle, inheritability);
		return new MemoryMappedFile
		{
			handle = new SafeMemoryMappedFileHandle(preexistingHandle, ownsHandle: true),
			stream = fileStream,
			keepOpen = leaveOpen
		};
	}

	private static MemoryMappedFile CoreShmCreate(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability, FileMode mode)
	{
		if (mapName != null && mapName.Length == 0)
		{
			throw new ArgumentException("mapName");
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		IntPtr preexistingHandle = MemoryMapImpl.OpenFile(null, mode, mapName, out capacity, access, options);
		return new MemoryMappedFile
		{
			handle = new SafeMemoryMappedFileHandle(preexistingHandle, ownsHandle: true)
		};
	}

	/// <summary>Creates a memory-mapped file that has the specified capacity in system memory. </summary>
	/// <returns>A memory-mapped file that has the specified name and capacity.</returns>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than or equal to zero.</exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile CreateNew(string mapName, long capacity)
	{
		return CreateNew(mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, null, HandleInheritability.None);
	}

	/// <summary>Creates a memory-mapped file that has the specified capacity and access type in system memory. </summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.-or-<paramref name="access" /> is set to write-only with the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> enumeration value. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than or equal to zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile CreateNew(string mapName, long capacity, MemoryMappedFileAccess access)
	{
		return CreateNew(mapName, capacity, access, MemoryMappedFileOptions.None, null, HandleInheritability.None);
	}

	[MonoLimitation("Named mappings scope is process local; options is ignored")]
	public static MemoryMappedFile CreateNew(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, HandleInheritability inheritability)
	{
		return CreateNew(mapName, capacity, access, options, null, inheritability);
	}

	/// <summary>Creates a memory-mapped file that has the specified capacity, access type, memory allocation, security permissions, and inheritability in system memory.</summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />. </param>
	/// <param name="options">A bitwise combination of enumeration values that specifies memory allocation options for the memory-mapped file.</param>
	/// <param name="memoryMappedFileSecurity">The permissions that can be granted for file access and operations on memory-mapped files.This parameter can be null.</param>
	/// <param name="inheritability">One of the enumeration values that specifies whether a handle to the memory-mapped file can be inherited by a child process. The default is <see cref="F:System.IO.HandleInheritability.None" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.-or-<paramref name="access" /> is set to write-only with the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> enumeration value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than or equal to zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> enumeration value.</exception>
	[MonoLimitation("Named mappings scope is process local; options and memoryMappedFileSecurity are ignored")]
	public static MemoryMappedFile CreateNew(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability)
	{
		return CoreShmCreate(mapName, capacity, access, options, memoryMappedFileSecurity, inheritability, FileMode.CreateNew);
	}

	/// <summary>Creates or opens a memory-mapped file that has the specified capacity in system memory.</summary>
	/// <returns>A memory-mapped file that has the specified name and size.</returns>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is greater than the size of the logical address space.-or-<paramref name="capacity" /> is less than or equal to zero.</exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile CreateOrOpen(string mapName, long capacity)
	{
		return CreateOrOpen(mapName, capacity, MemoryMappedFileAccess.ReadWrite);
	}

	/// <summary>Creates or opens a memory-mapped file that has the specified capacity and access type in system memory. </summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.-or-<paramref name="access" /> is set to write-only with the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> enumeration value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is greater than the size of the logical address space.-or-<paramref name="capacity" /> is less than or equal to zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The operating system denied the specified access to the file; for example, access is set to <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> or <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />, but the file or directory is read-only. </exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile CreateOrOpen(string mapName, long capacity, MemoryMappedFileAccess access)
	{
		return CreateOrOpen(mapName, capacity, access, MemoryMappedFileOptions.None, null, HandleInheritability.None);
	}

	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile CreateOrOpen(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, HandleInheritability inheritability)
	{
		return CreateOrOpen(mapName, capacity, access, options, null, inheritability);
	}

	/// <summary>Creates or opens a memory-mapped file that has the specified capacity, access type, memory allocation, security permissions, and inheritability in system memory.</summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="mapName">A name to assign to the memory-mapped file.</param>
	/// <param name="capacity">The maximum size, in bytes, to allocate to the memory-mapped file.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />. </param>
	/// <param name="options">A bitwise combination of enumeration values that specifies memory allocation options for the memory-mapped file.</param>
	/// <param name="memoryMappedFileSecurity">The permissions that can be granted for file access and operations on memory-mapped files.This parameter can be null.</param>
	/// <param name="inheritability">One of the enumeration values that specifies whether a handle to the memory-mapped file can be inherited by a child process. The default is <see cref="F:System.IO.HandleInheritability.None" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string. -or-<paramref name="access" /> is set to write-only with the <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> enumeration value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is greater than the size of the logical address space.-or-<paramref name="capacity" /> is less than or equal to zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> enumeration value.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The operating system denied the specified <paramref name="access" /> to the file; for example, <paramref name="access" /> is set to <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> or <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />, but the file or directory is read-only. </exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile CreateOrOpen(string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability)
	{
		return CoreShmCreate(mapName, capacity, access, options, memoryMappedFileSecurity, inheritability, FileMode.OpenOrCreate);
	}

	/// <summary>Opens an existing memory-mapped file that has the specified name in system memory.</summary>
	/// <returns>A memory-mapped file that has the specified name. </returns>
	/// <param name="mapName">The name of the memory-mapped file to open.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified for <paramref name="mapName" /> does not exist.</exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile OpenExisting(string mapName)
	{
		return OpenExisting(mapName, MemoryMappedFileRights.ReadWrite);
	}

	/// <summary>Opens an existing memory-mapped file that has the specified name and access rights in system memory.</summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="mapName">The name of the memory-mapped file to open.</param>
	/// <param name="desiredAccessRights">One of the enumeration values that specifies the access rights to apply to the memory-mapped file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="desiredAccessRights" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileRights" /> enumeration value.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified for <paramref name="mapName" /> does not exist.</exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile OpenExisting(string mapName, MemoryMappedFileRights desiredAccessRights)
	{
		return OpenExisting(mapName, desiredAccessRights, HandleInheritability.None);
	}

	/// <summary>Opens an existing memory-mapped file that has the specified name, access rights, and inheritability in system memory.</summary>
	/// <returns>A memory-mapped file that has the specified characteristics.</returns>
	/// <param name="mapName">The name of the memory-mapped file to open.</param>
	/// <param name="desiredAccessRights">One of the enumeration values that specifies the access rights to apply to the memory-mapped file.</param>
	/// <param name="inheritability">One of the enumeration values that specifies whether a handle to the memory-mapped file can be inherited by a child process. The default is <see cref="F:System.IO.HandleInheritability.None" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mapName" /> is an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="mapName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="desiredAccessRights" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileRights" /> enumeration value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> enumeration value.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The requested access is invalid for the memory-mapped file.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified for <paramref name="mapName" /> does not exist.</exception>
	[MonoLimitation("Named mappings scope is process local")]
	public static MemoryMappedFile OpenExisting(string mapName, MemoryMappedFileRights desiredAccessRights, HandleInheritability inheritability)
	{
		return CoreShmCreate(mapName, 0L, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, null, inheritability, FileMode.Open);
	}

	/// <summary>Creates a stream that maps to a view of the memory-mapped file.  </summary>
	/// <returns>A stream of memory.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the memory-mapped file is unauthorized.</exception>
	public MemoryMappedViewStream CreateViewStream()
	{
		return CreateViewStream(0L, 0L);
	}

	/// <summary>Creates a stream that maps to a view of the memory-mapped file, and that has the specified offset and size.</summary>
	/// <returns>A stream of memory that has the specified offset and size.</returns>
	/// <param name="offset">The byte at which to start the view.</param>
	/// <param name="size">The size of the view. Specify 0 (zero) to create a view that starts at <paramref name="offset" /> and ends approximately at the end of the memory-mapped file.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="size" /> is a negative value.-or-<paramref name="size" /> is greater than the logical address space.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the memory-mapped file is unauthorized.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="size" /> is greater than the total virtual memory.</exception>
	public MemoryMappedViewStream CreateViewStream(long offset, long size)
	{
		return CreateViewStream(offset, size, MemoryMappedFileAccess.ReadWrite);
	}

	/// <summary>Creates a stream that maps to a view of the memory-mapped file, and that has the specified offset, size, and access type.</summary>
	/// <returns>A stream of memory that has the specified characteristics.</returns>
	/// <param name="offset">The byte at which to start the view.</param>
	/// <param name="size">The size of the view. Specify 0 (zero) to create a view that starts at <paramref name="offset" /> and ends approximately at the end of the memory-mapped file.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="size" /> is a negative value.-or-<paramref name="size" /> is greater than the logical address space.-or-<paramref name="access " />is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="access" /> is invalid for the memory-mapped file.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="size" /> is greater than the total virtual memory.-or-<paramref name="access" /> is invalid for the memory-mapped file.</exception>
	public MemoryMappedViewStream CreateViewStream(long offset, long size, MemoryMappedFileAccess access)
	{
		return new MemoryMappedViewStream(MemoryMappedView.Create(handle.DangerousGetHandle(), offset, size, access));
	}

	/// <summary>Creates a <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedViewAccessor" /> that maps to a view of the memory-mapped file.</summary>
	/// <returns>A randomly accessible block of memory.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the memory-mapped file is unauthorized.</exception>
	public MemoryMappedViewAccessor CreateViewAccessor()
	{
		return CreateViewAccessor(0L, 0L);
	}

	/// <summary>Creates a <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedViewAccessor" /> that maps to a view of the memory-mapped file, and that has the specified offset and size.</summary>
	/// <returns>A randomly accessible block of memory.</returns>
	/// <param name="offset">The byte at which to start the view.</param>
	/// <param name="size">The size of the view. Specify 0 (zero) to create a view that starts at <paramref name="offset" /> and ends approximately at the end of the memory-mapped file.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="size" /> is a negative value.-or-<paramref name="size" /> is greater than the logical address space.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the memory-mapped file is unauthorized.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	public MemoryMappedViewAccessor CreateViewAccessor(long offset, long size)
	{
		return CreateViewAccessor(offset, size, MemoryMappedFileAccess.ReadWrite);
	}

	/// <summary>Creates a <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedViewAccessor" /> that maps to a view of the memory-mapped file, and that has the specified offset, size, and access restrictions.</summary>
	/// <returns>A randomly accessible block of memory.</returns>
	/// <param name="offset">The byte at which to start the view.</param>
	/// <param name="size">The size of the view. Specify 0 (zero) to create a view that starts at <paramref name="offset" /> and ends approximately at the end of the memory-mapped file.</param>
	/// <param name="access">One of the enumeration values that specifies the type of access allowed to the memory-mapped file. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="size" /> is a negative value.-or-<paramref name="size" /> is greater than the logical address space.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="access" /> is invalid for the memory-mapped file.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	public MemoryMappedViewAccessor CreateViewAccessor(long offset, long size, MemoryMappedFileAccess access)
	{
		return new MemoryMappedViewAccessor(MemoryMappedView.Create(handle.DangerousGetHandle(), offset, size, access));
	}

	private MemoryMappedFile()
	{
	}

	/// <summary>Releases all resources used by the <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFile" />. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFile" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && stream != null)
		{
			if (!keepOpen)
			{
				stream.Close();
			}
			stream = null;
		}
		if (handle != null)
		{
			handle.Dispose();
			handle = null;
		}
	}

	/// <summary>Gets the access control to the memory-mapped file resource.</summary>
	/// <returns>The permissions that can be granted for file access and operations on memory-mapped files.</returns>
	/// <exception cref="T:System.InvalidOperationException">An underlying call to set security information failed.</exception>
	/// <exception cref="T:System.NotSupportedException">An underlying call to set security information failed.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The memory-mapped file is closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current platform is Windows 98 or earlier.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">An underlying call to set security information failed.-or-The memory-mapped file was opened as <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Write" /> only.</exception>
	[MonoTODO]
	public MemoryMappedFileSecurity GetAccessControl()
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the access control to the memory-mapped file resource.</summary>
	/// <param name="memoryMappedFileSecurity">The permissions that can be granted for file access and operations on memory-mapped files.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="memoryMappedFileSecurity" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">An underlying call to set security information failed.</exception>
	/// <exception cref="T:System.NotSupportedException">An underlying call to set security information failed.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">An underlying call to set security information failed.</exception>
	[MonoTODO]
	public void SetAccessControl(MemoryMappedFileSecurity memoryMappedFileSecurity)
	{
		throw new NotImplementedException();
	}

	internal static FileAccess GetFileAccess(MemoryMappedFileAccess access)
	{
		return access switch
		{
			MemoryMappedFileAccess.Read => FileAccess.Read, 
			MemoryMappedFileAccess.Write => FileAccess.Write, 
			MemoryMappedFileAccess.ReadWrite => FileAccess.ReadWrite, 
			MemoryMappedFileAccess.CopyOnWrite => FileAccess.ReadWrite, 
			MemoryMappedFileAccess.ReadExecute => FileAccess.Read, 
			MemoryMappedFileAccess.ReadWriteExecute => FileAccess.ReadWrite, 
			_ => throw new ArgumentOutOfRangeException("access"), 
		};
	}
}
