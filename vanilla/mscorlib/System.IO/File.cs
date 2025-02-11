using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;

namespace System.IO;

/// <summary>Provides static methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of <see cref="T:System.IO.FileStream" /> objects.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
public static class File
{
	private static DateTime? defaultLocalFileTime;

	private static DateTime DefaultLocalFileTime
	{
		get
		{
			if (!defaultLocalFileTime.HasValue)
			{
				defaultLocalFileTime = new DateTime(1601, 1, 1).ToLocalTime();
			}
			return defaultLocalFileTime.Value;
		}
	}

	/// <summary>Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.</summary>
	/// <param name="path">The file to append the specified string to. </param>
	/// <param name="contents">The string to append to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn’t exist or it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void AppendAllText(string path, string contents)
	{
		using TextWriter textWriter = new StreamWriter(path, append: true);
		textWriter.Write(contents);
	}

	/// <summary>Appends the specified string to the file, creating the file if it does not already exist.</summary>
	/// <param name="path">The file to append the specified string to. </param>
	/// <param name="contents">The string to append to the file. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn’t exist or it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void AppendAllText(string path, string contents, Encoding encoding)
	{
		using TextWriter textWriter = new StreamWriter(path, append: true, encoding);
		textWriter.Write(contents);
	}

	/// <summary>Creates a <see cref="T:System.IO.StreamWriter" /> that appends UTF-8 encoded text to an existing file, or to a new file if the specified file does not exist.</summary>
	/// <returns>A stream writer that appends UTF-8 encoded text to the specified file or to a new file.</returns>
	/// <param name="path">The path to the file to append to. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn’t exist or it is on an unmapped drive). </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StreamWriter AppendText(string path)
	{
		return new StreamWriter(path, append: true);
	}

	/// <summary>Copies an existing file to a new file. Overwriting a file of the same name is not allowed.</summary>
	/// <param name="sourceFileName">The file to copy. </param>
	/// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="sourceFileName" /> or <paramref name="destFileName" /> specifies a directory. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceFileName" /> was not found. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destFileName" /> exists.-or- An I/O error has occurred. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Copy(string sourceFileName, string destFileName)
	{
		Copy(sourceFileName, destFileName, overwrite: false);
	}

	/// <summary>Copies an existing file to a new file. Overwriting a file of the same name is allowed.</summary>
	/// <param name="sourceFileName">The file to copy. </param>
	/// <param name="destFileName">The name of the destination file. This cannot be a directory. </param>
	/// <param name="overwrite">true if the destination file can be overwritten; otherwise, false. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. -or-<paramref name="destFileName" /> is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="sourceFileName" /> or <paramref name="destFileName" /> specifies a directory. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceFileName" /> was not found. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destFileName" /> exists and <paramref name="overwrite" /> is false.-or- An I/O error has occurred. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (destFileName == null)
		{
			throw new ArgumentNullException("destFileName");
		}
		if (sourceFileName.Length == 0)
		{
			throw new ArgumentException("An empty file name is not valid.", "sourceFileName");
		}
		if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("The file name is not valid.");
		}
		if (destFileName.Length == 0)
		{
			throw new ArgumentException("An empty file name is not valid.", "destFileName");
		}
		if (destFileName.Trim().Length == 0 || destFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("The file name is not valid.");
		}
		if (!MonoIO.Exists(sourceFileName, out var error))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", sourceFileName), sourceFileName);
		}
		if ((GetAttributes(sourceFileName) & FileAttributes.Directory) == FileAttributes.Directory)
		{
			throw new ArgumentException(Locale.GetText("{0} is a directory", sourceFileName));
		}
		if (MonoIO.Exists(destFileName, out error))
		{
			if ((GetAttributes(destFileName) & FileAttributes.Directory) == FileAttributes.Directory)
			{
				throw new ArgumentException(Locale.GetText("{0} is a directory", destFileName));
			}
			if (!overwrite)
			{
				throw new IOException(Locale.GetText("{0} already exists", destFileName));
			}
		}
		string directoryName = Path.GetDirectoryName(destFileName);
		if (directoryName != string.Empty && !Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(Locale.GetText("Destination directory not found: {0}", directoryName));
		}
		if (!MonoIO.CopyFile(sourceFileName, destFileName, overwrite, out error))
		{
			throw MonoIO.GetException(Locale.GetText("{0}\" or \"{1}", sourceFileName, destFileName), error);
		}
	}

	internal static string InternalCopy(string sourceFileName, string destFileName, bool overwrite, bool checkHost)
	{
		string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
		string fullPathInternal2 = Path.GetFullPathInternal(destFileName);
		if (!MonoIO.CopyFile(fullPathInternal, fullPathInternal2, overwrite, out var error))
		{
			throw MonoIO.GetException(Locale.GetText("{0}\" or \"{1}", sourceFileName, destFileName), error);
		}
		return fullPathInternal2;
	}

	/// <summary>Creates or overwrites a file in the specified path.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> that provides read/write access to the file specified in <paramref name="path" />.</returns>
	/// <param name="path">The path and name of the file to create. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Create(string path)
	{
		return Create(path, 8192);
	}

	/// <summary>Creates or overwrites the specified file.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> with the specified buffer size that provides read/write access to the file specified in <paramref name="path" />.</returns>
	/// <param name="path">The name of the file. </param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Create(string path, int bufferSize)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
	}

	/// <summary>Creates or overwrites the specified file, specifying a buffer size and a <see cref="T:System.IO.FileOptions" /> value that describes how to create or overwrite the file.</summary>
	/// <returns>A new file with the specified buffer size.</returns>
	/// <param name="path">The name of the file. </param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file. </param>
	/// <param name="options">One of the <see cref="T:System.IO.FileOptions" /> values that describes how to create or overwrite the file.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. -or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" /> and file encryption is not supported on the current platform.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	[MonoLimitation("FileOptions are ignored")]
	public static FileStream Create(string path, int bufferSize, FileOptions options)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
	}

	/// <summary>Creates or overwrites the specified file with the specified buffer size, file options, and file security.</summary>
	/// <returns>A new file with the specified buffer size, file options, and file security.</returns>
	/// <param name="path">The name of the file. </param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file. </param>
	/// <param name="options">One of the <see cref="T:System.IO.FileOptions" /> values that describes how to create or overwrite the file.</param>
	/// <param name="fileSecurity">One of the <see cref="T:System.Security.AccessControl.FileSecurity" /> values that determines the access control and audit security for the file.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only.-or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" /> and file encryption is not supported on the current platform. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	[MonoLimitation("FileOptions and FileSecurity are ignored")]
	public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
	}

	/// <summary>Creates or opens a file for writing UTF-8 encoded text.</summary>
	/// <returns>A <see cref="T:System.IO.StreamWriter" /> that writes to the specified file using UTF-8 encoding.</returns>
	/// <param name="path">The file to be opened for writing. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StreamWriter CreateText(string path)
	{
		return new StreamWriter(path, append: false);
	}

	/// <summary>Deletes the specified file. </summary>
	/// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">The specified file is in use. -or-There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> is a directory.-or- <paramref name="path" /> specified a read-only file. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Delete(string path)
	{
		Path.Validate(path);
		if (Directory.Exists(path))
		{
			throw new UnauthorizedAccessException(Locale.GetText("{0} is a directory", path));
		}
		string directoryName = Path.GetDirectoryName(path);
		if (directoryName != string.Empty && !Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(Locale.GetText("Could not find a part of the path \"{0}\".", path));
		}
		if (!MonoIO.DeleteFile(path, out var error) && error != MonoIOError.ERROR_FILE_NOT_FOUND)
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Determines whether the specified file exists.</summary>
	/// <returns>true if the caller has the required permissions and <paramref name="path" /> contains the name of an existing file; otherwise, false. This method also returns false if <paramref name="path" /> is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns false regardless of the existence of <paramref name="path" />.</returns>
	/// <param name="path">The file to check. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool Exists(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.InvalidPathChars) >= 0)
		{
			return false;
		}
		if (!SecurityManager.CheckElevatedPermissions())
		{
			return false;
		}
		MonoIOError error;
		return MonoIO.ExistsFile(path, out error);
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control list (ACL) entries for a specified file.</summary>
	/// <returns>A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.     </returns>
	/// <param name="path">The path to a file containing a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes the file's access control list (ACL) information.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileSecurity GetAccessControl(string path)
	{
		return GetAccessControl(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the specified type of access control list (ACL) entries for a particular file.</summary>
	/// <returns>A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.     </returns>
	/// <param name="path">The path to a file containing a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes the file's access control list (ACL) information.</param>
	/// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> values that specifies the type of access control list (ACL) information to receive.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
	{
		return new FileSecurity(path, includeSections);
	}

	/// <summary>Gets the <see cref="T:System.IO.FileAttributes" /> of the file on the path.</summary>
	/// <returns>The <see cref="T:System.IO.FileAttributes" /> of the file on the path.</returns>
	/// <param name="path">The path to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty, contains only white spaces, or contains invalid characters. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="path" /> represents a file and is invalid, such as being on an unmapped drive, or the file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> represents a directory and is invalid, such as being on an unmapped drive, or the directory cannot be found.</exception>
	/// <exception cref="T:System.IO.IOException">This file is being used by another process.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileAttributes GetAttributes(string path)
	{
		Path.Validate(path);
		MonoIOError error;
		FileAttributes fileAttributes = MonoIO.GetFileAttributes(path, out error);
		if (error != 0)
		{
			throw MonoIO.GetException(path, error);
		}
		return fileAttributes;
	}

	/// <summary>Returns the creation date and time of the specified file or directory.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
	/// <param name="path">The file or directory for which to obtain creation date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetCreationTime(string path)
	{
		Path.Validate(path);
		if (!MonoIO.GetFileStat(path, out var stat, out var error))
		{
			if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
			{
				return DefaultLocalFileTime;
			}
			throw new IOException(path);
		}
		return DateTime.FromFileTime(stat.CreationTime);
	}

	/// <summary>Returns the creation date and time, in coordinated universal time (UTC), of the specified file or directory.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the creation date and time for the specified file or directory. This value is expressed in UTC time.</returns>
	/// <param name="path">The file or directory for which to obtain creation date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetCreationTimeUtc(string path)
	{
		return GetCreationTime(path).ToUniversalTime();
	}

	/// <summary>Returns the date and time the specified file or directory was last accessed.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
	/// <param name="path">The file or directory for which to obtain access date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastAccessTime(string path)
	{
		Path.Validate(path);
		if (!MonoIO.GetFileStat(path, out var stat, out var error))
		{
			if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
			{
				return DefaultLocalFileTime;
			}
			throw new IOException(path);
		}
		return DateTime.FromFileTime(stat.LastAccessTime);
	}

	/// <summary>Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
	/// <param name="path">The file or directory for which to obtain access date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastAccessTimeUtc(string path)
	{
		return GetLastAccessTime(path).ToUniversalTime();
	}

	/// <summary>Returns the date and time the specified file or directory was last written to.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
	/// <param name="path">The file or directory for which to obtain write date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastWriteTime(string path)
	{
		Path.Validate(path);
		if (!MonoIO.GetFileStat(path, out var stat, out var error))
		{
			if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
			{
				return DefaultLocalFileTime;
			}
			throw new IOException(path);
		}
		return DateTime.FromFileTime(stat.LastWriteTime);
	}

	/// <summary>Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last written to. This value is expressed in UTC time.</returns>
	/// <param name="path">The file or directory for which to obtain write date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastWriteTimeUtc(string path)
	{
		return GetLastWriteTime(path).ToUniversalTime();
	}

	/// <summary>Moves a specified file to a new location, providing the option to specify a new file name.</summary>
	/// <param name="sourceFileName">The name of the file to move. </param>
	/// <param name="destFileName">The new path for the file. </param>
	/// <exception cref="T:System.IO.IOException">The destination file already exists.-or-<paramref name="sourceFileName" /> was not found. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains invalid characters as defined in <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Move(string sourceFileName, string destFileName)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (destFileName == null)
		{
			throw new ArgumentNullException("destFileName");
		}
		if (sourceFileName.Length == 0)
		{
			throw new ArgumentException("An empty file name is not valid.", "sourceFileName");
		}
		if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("The file name is not valid.");
		}
		if (destFileName.Length == 0)
		{
			throw new ArgumentException("An empty file name is not valid.", "destFileName");
		}
		if (destFileName.Trim().Length == 0 || destFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("The file name is not valid.");
		}
		if (!MonoIO.Exists(sourceFileName, out var error))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", sourceFileName), sourceFileName);
		}
		string directoryName = Path.GetDirectoryName(destFileName);
		if (directoryName != string.Empty && !Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(Locale.GetText("Could not find a part of the path."));
		}
		if (!MonoIO.MoveFile(sourceFileName, destFileName, out error))
		{
			switch (error)
			{
			case MonoIOError.ERROR_ALREADY_EXISTS:
				throw MonoIO.GetException(error);
			case MonoIOError.ERROR_SHARING_VIOLATION:
				throw MonoIO.GetException(sourceFileName, error);
			default:
				throw MonoIO.GetException(error);
			}
		}
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path with read/write access.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> opened in the specified mode and path, with read/write access and not shared.</returns>
	/// <param name="path">The file to open. </param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. -or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> specified an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Open(string path, FileMode mode)
	{
		return new FileStream(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path, with the specified mode and access.</summary>
	/// <returns>An unshared <see cref="T:System.IO.FileStream" /> that provides access to the specified file, with the specified mode and access.</returns>
	/// <param name="path">The file to open. </param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten. </param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> value that specifies the operations that can be performed on the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="access" /> specified Read and <paramref name="mode" /> specified Create, CreateNew, Truncate, or Append. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only and <paramref name="access" /> is not Read.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. -or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> or <paramref name="access" /> specified an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Open(string path, FileMode mode, FileAccess access)
	{
		return new FileStream(path, mode, access, FileShare.None);
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
	/// <param name="path">The file to open. </param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten. </param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> value that specifies the operations that can be performed on the file. </param>
	/// <param name="share">A <see cref="T:System.IO.FileShare" /> value specifying the type of access other threads have to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="access" /> specified Read and <paramref name="mode" /> specified Create, CreateNew, Truncate, or Append. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only and <paramref name="access" /> is not Read.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. -or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> specified an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		return new FileStream(path, mode, access, share);
	}

	/// <summary>Opens an existing file for reading.</summary>
	/// <returns>A read-only <see cref="T:System.IO.FileStream" /> on the specified path.</returns>
	/// <param name="path">The file to be opened for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream OpenRead(string path)
	{
		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Opens an existing UTF-8 encoded text file for reading.</summary>
	/// <returns>A <see cref="T:System.IO.StreamReader" /> on the specified path.</returns>
	/// <param name="path">The file to be opened for reading. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StreamReader OpenText(string path)
	{
		return new StreamReader(path);
	}

	/// <summary>Opens an existing file or creates a new file for writing.</summary>
	/// <returns>An unshared <see cref="T:System.IO.FileStream" /> object on the specified path with <see cref="F:System.IO.FileAccess.Write" /> access.</returns>
	/// <param name="path">The file to be opened for writing. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a read-only file or directory. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream OpenWrite(string path)
	{
		return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
	}

	/// <summary>Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file.</summary>
	/// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName" />.</param>
	/// <param name="destinationFileName">The name of the file being replaced.</param>
	/// <param name="destinationBackupFileName">The name of the backup file.</param>
	/// <exception cref="T:System.ArgumentException">The path described by the <paramref name="destinationFileName" /> parameter was not of a legal form.-or-The path described by the <paramref name="destinationBackupFileName" /> parameter was not of a legal form.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationFileName" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.-or-The file described by the <paramref name="destinationBackupFileName" /> parameter could not be found. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.- or -The <paramref name="sourceFileName" /> and <paramref name="destinationFileName" /> parameters specify the same file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows 98 Second Edition or earlier and the files system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="sourceFileName" /> or <paramref name="destinationFileName" /> parameter specifies a file that is read-only.-or- This operation is not supported on the current platform.-or- Source or destination parameters specify a directory instead of a file.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
	{
		Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);
	}

	/// <summary>Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file and optionally ignores merge errors.</summary>
	/// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName" />.</param>
	/// <param name="destinationFileName">The name of the file being replaced.</param>
	/// <param name="destinationBackupFileName">The name of the backup file.</param>
	/// <param name="ignoreMetadataErrors">true to ignore merge errors (such as attributes and access control lists (ACLs)) from the replaced file to the replacement file; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">The path described by the <paramref name="destinationFileName" /> parameter was not of a legal form.-or-The path described by the <paramref name="destinationBackupFileName" /> parameter was not of a legal form.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationFileName" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.-or-The file described by the <paramref name="destinationBackupFileName" /> parameter could not be found. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.- or -The <paramref name="sourceFileName" /> and <paramref name="destinationFileName" /> parameters specify the same file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows 98 Second Edition or earlier and the files system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="sourceFileName" /> or <paramref name="destinationFileName" /> parameter specifies a file that is read-only.-or- This operation is not supported on the current platform.-or- Source or destination parameters specify a directory instead of a file.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (destinationFileName == null)
		{
			throw new ArgumentNullException("destinationFileName");
		}
		if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("sourceFileName");
		}
		if (destinationFileName.Trim().Length == 0 || destinationFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("destinationFileName");
		}
		string fullPath = Path.GetFullPath(sourceFileName);
		string fullPath2 = Path.GetFullPath(destinationFileName);
		if (MonoIO.ExistsDirectory(fullPath, out var error))
		{
			throw new IOException(Locale.GetText("{0} is a directory", sourceFileName));
		}
		if (MonoIO.ExistsDirectory(fullPath2, out error))
		{
			throw new IOException(Locale.GetText("{0} is a directory", destinationFileName));
		}
		if (!Exists(fullPath))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", sourceFileName), sourceFileName);
		}
		if (!Exists(fullPath2))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", destinationFileName), destinationFileName);
		}
		if (fullPath == fullPath2)
		{
			throw new IOException(Locale.GetText("Source and destination arguments are the same file."));
		}
		string text = null;
		if (destinationBackupFileName != null)
		{
			if (destinationBackupFileName.Trim().Length == 0 || destinationBackupFileName.IndexOfAny(Path.InvalidPathChars) != -1)
			{
				throw new ArgumentException("destinationBackupFileName");
			}
			text = Path.GetFullPath(destinationBackupFileName);
			if (MonoIO.ExistsDirectory(text, out error))
			{
				throw new IOException(Locale.GetText("{0} is a directory", destinationBackupFileName));
			}
			if (fullPath == text)
			{
				throw new IOException(Locale.GetText("Source and backup arguments are the same file."));
			}
			if (fullPath2 == text)
			{
				throw new IOException(Locale.GetText("Destination and backup arguments are the same file."));
			}
		}
		if ((GetAttributes(fullPath2) & FileAttributes.ReadOnly) != 0)
		{
			throw MonoIO.GetException(MonoIOError.ERROR_ACCESS_DENIED);
		}
		if (!MonoIO.ReplaceFile(fullPath, fullPath2, text, ignoreMetadataErrors, out error))
		{
			throw MonoIO.GetException(error);
		}
	}

	/// <summary>Applies access control list (ACL) entries described by a <see cref="T:System.Security.AccessControl.FileSecurity" /> object to the specified file.</summary>
	/// <param name="path">A file to add or remove access control list (ACL) entries from.</param>
	/// <param name="fileSecurity">A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes an ACL entry to apply to the file described by the <paramref name="path" /> parameter.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileSecurity" /> parameter is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetAccessControl(string path, FileSecurity fileSecurity)
	{
		if (fileSecurity == null)
		{
			throw new ArgumentNullException("fileSecurity");
		}
		fileSecurity.PersistModifications(path);
	}

	/// <summary>Sets the specified <see cref="T:System.IO.FileAttributes" /> of the file on the specified path.</summary>
	/// <param name="path">The path to the file. </param>
	/// <param name="fileAttributes">A bitwise combination of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty, contains only white spaces, contains invalid characters, or the file attribute is invalid. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetAttributes(string path, FileAttributes fileAttributes)
	{
		Path.Validate(path);
		if (!MonoIO.SetFileAttributes(path, fileAttributes, out var error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Sets the date and time the file was created.</summary>
	/// <param name="path">The file for which to set the creation date and time information. </param>
	/// <param name="creationTime">A <see cref="T:System.DateTime" /> containing the value to set for the creation date and time of <paramref name="path" />. This value is expressed in local time. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while performing the operation. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationTime" /> specifies a value outside the range of dates, times, or both permitted for this operation. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetCreationTime(string path, DateTime creationTime)
	{
		Path.Validate(path);
		if (!MonoIO.Exists(path, out var error))
		{
			throw MonoIO.GetException(path, error);
		}
		if (!MonoIO.SetCreationTime(path, creationTime, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the file was created.</summary>
	/// <param name="path">The file for which to set the creation date and time information. </param>
	/// <param name="creationTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the creation date and time of <paramref name="path" />. This value is expressed in UTC time. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while performing the operation. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationTime" /> specifies a value outside the range of dates, times, or both permitted for this operation. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		SetCreationTime(path, creationTimeUtc.ToLocalTime());
	}

	/// <summary>Sets the date and time the specified file was last accessed.</summary>
	/// <param name="path">The file for which to set the access date and time information. </param>
	/// <param name="lastAccessTime">A <see cref="T:System.DateTime" /> containing the value to set for the last access date and time of <paramref name="path" />. This value is expressed in local time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastAccessTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		Path.Validate(path);
		if (!MonoIO.Exists(path, out var error))
		{
			throw MonoIO.GetException(path, error);
		}
		if (!MonoIO.SetLastAccessTime(path, lastAccessTime, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the specified file was last accessed.</summary>
	/// <param name="path">The file for which to set the access date and time information. </param>
	/// <param name="lastAccessTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the last access date and time of <paramref name="path" />. This value is expressed in UTC time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastAccessTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		SetLastAccessTime(path, lastAccessTimeUtc.ToLocalTime());
	}

	/// <summary>Sets the date and time that the specified file was last written to.</summary>
	/// <param name="path">The file for which to set the date and time information. </param>
	/// <param name="lastWriteTime">A <see cref="T:System.DateTime" /> containing the value to set for the last write date and time of <paramref name="path" />. This value is expressed in local time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastWriteTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		Path.Validate(path);
		if (!MonoIO.Exists(path, out var error))
		{
			throw MonoIO.GetException(path, error);
		}
		if (!MonoIO.SetLastWriteTime(path, lastWriteTime, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.</summary>
	/// <param name="path">The file for which to set the date and time information. </param>
	/// <param name="lastWriteTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the last write date and time of <paramref name="path" />. This value is expressed in UTC time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastWriteTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		SetLastWriteTime(path, lastWriteTimeUtc.ToLocalTime());
	}

	/// <summary>Opens a binary file, reads the contents of the file into a byte array, and then closes the file.</summary>
	/// <returns>A byte array containing the contents of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static byte[] ReadAllBytes(string path)
	{
		using FileStream fileStream = OpenRead(path);
		long length = fileStream.Length;
		if (length > int.MaxValue)
		{
			throw new IOException("Reading more than 2GB with this call is not supported");
		}
		int num = 0;
		int num2 = (int)length;
		byte[] array = new byte[length];
		while (num2 > 0)
		{
			int num3 = fileStream.Read(array, num, num2);
			if (num3 == 0)
			{
				throw new IOException("Unexpected end of stream");
			}
			num += num3;
			num2 -= num3;
		}
		return array;
	}

	/// <summary>Opens a text file, reads all lines of the file, and then closes the file.</summary>
	/// <returns>A string array containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string[] ReadAllLines(string path)
	{
		using StreamReader reader = OpenText(path);
		return ReadAllLines(reader);
	}

	/// <summary>Opens a file, reads all lines of the file with the specified encoding, and then closes the file.</summary>
	/// <returns>A string array containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <param name="encoding">The encoding applied to the contents of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string[] ReadAllLines(string path, Encoding encoding)
	{
		using StreamReader reader = new StreamReader(path, encoding);
		return ReadAllLines(reader);
	}

	private static string[] ReadAllLines(StreamReader reader)
	{
		List<string> list = new List<string>();
		while (!reader.EndOfStream)
		{
			list.Add(reader.ReadLine());
		}
		return list.ToArray();
	}

	/// <summary>Opens a text file, reads all lines of the file, and then closes the file.</summary>
	/// <returns>A string containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string ReadAllText(string path)
	{
		using StreamReader streamReader = new StreamReader(path);
		return streamReader.ReadToEnd();
	}

	/// <summary>Opens a file, reads all lines of the file with the specified encoding, and then closes the file.</summary>
	/// <returns>A string containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <param name="encoding">The encoding applied to the contents of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string ReadAllText(string path, Encoding encoding)
	{
		using StreamReader streamReader = new StreamReader(path, encoding);
		return streamReader.ReadToEnd();
	}

	/// <summary>Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="bytes">The bytes to write to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null or the byte array is empty. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllBytes(string path, byte[] bytes)
	{
		using Stream stream = Create(path);
		stream.Write(bytes, 0, bytes.Length);
	}

	/// <summary>Creates a new file, write the specified string array to the file, and then closes the file. </summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string array to write to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is null.  </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllLines(string path, string[] contents)
	{
		using StreamWriter writer = new StreamWriter(path);
		WriteAllLines(writer, contents);
	}

	/// <summary>Creates a new file, writes the specified string array to the file by using the specified encoding, and then closes the file. </summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string array to write to the file. </param>
	/// <param name="encoding">An <see cref="T:System.Text.Encoding" /> object that represents the character encoding applied to the string array.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is null.  </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		using StreamWriter writer = new StreamWriter(path, append: false, encoding);
		WriteAllLines(writer, contents);
	}

	private static void WriteAllLines(StreamWriter writer, string[] contents)
	{
		foreach (string value in contents)
		{
			writer.WriteLine(value);
		}
	}

	/// <summary>Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string to write to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null or <paramref name="contents" /> is empty.  </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllText(string path, string contents)
	{
		WriteAllText(path, contents, EncodingHelper.UTF8Unmarked);
	}

	/// <summary>Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string to write to the file. </param>
	/// <param name="encoding">The encoding to apply to the string.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null or <paramref name="contents" /> is empty. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllText(string path, string contents, Encoding encoding)
	{
		using StreamWriter streamWriter = new StreamWriter(path, append: false, encoding);
		streamWriter.Write(contents);
	}

	/// <summary>Encrypts a file so that only the account used to encrypt the file can decrypt it.</summary>
	/// <param name="path">A path that describes a file to encrypt.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the <paramref name="path" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.-or-This operation is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.NotSupportedException">The file system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoLimitation("File encryption isn't supported (even on NTFS).")]
	public static void Encrypt(string path)
	{
		throw new NotSupportedException(Locale.GetText("File encryption isn't supported on any file system."));
	}

	/// <summary>Decrypts a file that was encrypted by the current account using the <see cref="M:System.IO.File.Encrypt(System.String)" /> method.</summary>
	/// <param name="path">A path that describes a file to decrypt.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the <paramref name="path" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. For example, the encrypted file is already open. -or-This operation is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.NotSupportedException">The file system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoLimitation("File encryption isn't supported (even on NTFS).")]
	public static void Decrypt(string path)
	{
		throw new NotSupportedException(Locale.GetText("File encryption isn't supported on any file system."));
	}

	/// <summary>Reads the lines of a file.</summary>
	/// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
	/// <param name="path">The file to read.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static IEnumerable<string> ReadLines(string path)
	{
		return ReadLines(OpenText(path));
	}

	/// <summary>Read the lines of a file that has a specified encoding.</summary>
	/// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
	/// <param name="path">The file to read.</param>
	/// <param name="encoding">The encoding that is applied to the contents of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		return ReadLines(new StreamReader(path, encoding));
	}

	private static IEnumerable<string> ReadLines(StreamReader reader)
	{
		using (reader)
		{
			while (true)
			{
				string text;
				string s = (text = reader.ReadLine());
				if (text == null)
				{
					break;
				}
				yield return s;
			}
		}
	}

	/// <summary>Appends lines to a file, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.</summary>
	/// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
	/// <param name="contents">The lines to append to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path " />or <paramref name="contents" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, the directory doesn’t exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to write to the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.</exception>
	public static void AppendAllLines(string path, IEnumerable<string> contents)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using TextWriter textWriter = new StreamWriter(path, append: true);
		foreach (string content in contents)
		{
			textWriter.WriteLine(content);
		}
	}

	/// <summary>Appends lines to a file by using a specified encoding, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.</summary>
	/// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
	/// <param name="contents">The lines to append to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path" />, <paramref name="contents" />, or <paramref name="encoding" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, the directory doesn’t exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using TextWriter textWriter = new StreamWriter(path, append: true, encoding);
		foreach (string content in contents)
		{
			textWriter.WriteLine(content);
		}
	}

	/// <summary>Creates a new file, writes a collection of strings to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The lines to write to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path " />or <paramref name="contents" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static void WriteAllLines(string path, IEnumerable<string> contents)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using TextWriter textWriter = new StreamWriter(path, append: false);
		foreach (string content in contents)
		{
			textWriter.WriteLine(content);
		}
	}

	/// <summary>Creates a new file by using the specified encoding, writes a collection of strings to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The lines to write to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path" />,<paramref name=" contents" />, or <paramref name="encoding" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using TextWriter textWriter = new StreamWriter(path, append: false, encoding);
		foreach (string content in contents)
		{
			textWriter.WriteLine(content);
		}
	}

	internal static int FillAttributeInfo(string path, ref MonoIOStat data, bool tryagain, bool returnErrorOnNotFound)
	{
		if (tryagain)
		{
			throw new NotImplementedException();
		}
		MonoIO.GetFileStat(path, out data, out var error);
		if (!returnErrorOnNotFound && (error == MonoIOError.ERROR_FILE_NOT_FOUND || error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_NOT_READY))
		{
			data = default(MonoIOStat);
			data.fileAttributes = (FileAttributes)(-1);
			return 0;
		}
		return (int)error;
	}
}
