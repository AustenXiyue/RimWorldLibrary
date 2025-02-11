using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> around a file, supporting both synchronous and asynchronous read and write operations.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
public class FileStream : Stream
{
	private delegate int ReadDelegate(byte[] buffer, int offset, int count);

	private delegate void WriteDelegate(byte[] buffer, int offset, int count);

	internal const int DefaultBufferSize = 4096;

	private static byte[] buf_recycle;

	private static readonly object buf_recycle_lock = new object();

	private byte[] buf;

	private string name = "[Unknown]";

	private SafeFileHandle safeHandle;

	private bool isExposed;

	private long append_startpos;

	private FileAccess access;

	private bool owner;

	private bool async;

	private bool canseek;

	private bool anonymous;

	private bool buf_dirty;

	private int buf_size;

	private int buf_length;

	private int buf_offset;

	private long buf_start;

	/// <summary>Gets a value indicating whether the current stream supports reading.</summary>
	/// <returns>true if the stream supports reading; false if the stream is closed or was opened with write-only access.</returns>
	/// <filterpriority>1</filterpriority>
	public override bool CanRead
	{
		get
		{
			if (access != FileAccess.Read)
			{
				return access == FileAccess.ReadWrite;
			}
			return true;
		}
	}

	/// <summary>Gets a value indicating whether the current stream supports writing.</summary>
	/// <returns>true if the stream supports writing; false if the stream is closed or was opened with read-only access.</returns>
	/// <filterpriority>1</filterpriority>
	public override bool CanWrite
	{
		get
		{
			if (access != FileAccess.Write)
			{
				return access == FileAccess.ReadWrite;
			}
			return true;
		}
	}

	/// <summary>Gets a value indicating whether the current stream supports seeking.</summary>
	/// <returns>true if the stream supports seeking; false if the stream is closed or if the FileStream was constructed from an operating-system handle such as a pipe or output to the console.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanSeek => canseek;

	/// <summary>Gets a value indicating whether the FileStream was opened asynchronously or synchronously.</summary>
	/// <returns>true if the FileStream was opened asynchronously; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsAsync => async;

	/// <summary>Gets the name of the FileStream that was passed to the constructor.</summary>
	/// <returns>A string that is the name of the FileStream.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string Name => name;

	/// <summary>Gets the length in bytes of the stream.</summary>
	/// <returns>A long value representing the length of the stream in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.FileStream.CanSeek" /> for this stream is false. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as the file being closed, occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public override long Length
	{
		get
		{
			if (safeHandle.IsClosed)
			{
				throw new ObjectDisposedException("Stream has been closed");
			}
			if (!CanSeek)
			{
				throw new NotSupportedException("The stream does not support seeking");
			}
			FlushBufferIfDirty();
			MonoIOError error;
			long length = MonoIO.GetLength(safeHandle, out error);
			if (error != 0)
			{
				throw MonoIO.GetException(GetSecureFileName(name), error);
			}
			return length;
		}
	}

	/// <summary>Gets or sets the current position of this stream.</summary>
	/// <returns>The current position of this stream.</returns>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. - or -The position was set to a very large value beyond the end of the stream in Windows 98 or earlier.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Attempted to set the position to a negative value. </exception>
	/// <exception cref="T:System.IO.EndOfStreamException">Attempted seeking past the end of a stream that does not support this. </exception>
	/// <filterpriority>1</filterpriority>
	public override long Position
	{
		get
		{
			if (safeHandle.IsClosed)
			{
				throw new ObjectDisposedException("Stream has been closed");
			}
			if (!CanSeek)
			{
				throw new NotSupportedException("The stream does not support seeking");
			}
			if (!isExposed)
			{
				return buf_start + buf_offset;
			}
			MonoIOError error;
			long result = MonoIO.Seek(safeHandle, 0L, SeekOrigin.Current, out error);
			if (error != 0)
			{
				throw MonoIO.GetException(GetSecureFileName(name), error);
			}
			return result;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Non-negative number required."));
			}
			Seek(value, SeekOrigin.Begin);
		}
	}

	/// <summary>Gets the operating system file handle for the file that the current FileStream object encapsulates.</summary>
	/// <returns>The operating system file handle for the file encapsulated by this FileStream object, or -1 if the FileStream has been closed.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[Obsolete("Use SafeFileHandle instead")]
	public virtual IntPtr Handle
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
		get
		{
			IntPtr result = safeHandle.DangerousGetHandle();
			if (!isExposed)
			{
				ExposeHandle();
			}
			return result;
		}
	}

	/// <summary>Gets a <see cref="T:Microsoft.Win32.SafeHandles.SafeFileHandle" /> object that represents the operating system file handle for the file that the current <see cref="T:System.IO.FileStream" /> object encapsulates.</summary>
	/// <returns>An object that represents the operating system file handle for the file that the current <see cref="T:System.IO.FileStream" /> object encapsulates.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual SafeFileHandle SafeFileHandle
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
		get
		{
			if (!isExposed)
			{
				ExposeHandle();
			}
			return safeHandle;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission.</summary>
	/// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
	/// <param name="access">A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="access" /> is not a field of <see cref="T:System.IO.FileAccess" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	[Obsolete("Use FileStream(SafeFileHandle handle, FileAccess access) instead")]
	public FileStream(IntPtr handle, FileAccess access)
		: this(handle, access, ownsHandle: true, 4096, isAsync: false, isConsoleWrapper: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission and FileStream instance ownership.</summary>
	/// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
	/// <param name="access">A constant that gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <param name="ownsHandle">true if the file handle will be owned by this FileStream instance; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="access" /> is not a field of <see cref="T:System.IO.FileAccess" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	[Obsolete("Use FileStream(SafeFileHandle handle, FileAccess access) instead")]
	public FileStream(IntPtr handle, FileAccess access, bool ownsHandle)
		: this(handle, access, ownsHandle, 4096, isAsync: false, isConsoleWrapper: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission, FileStream instance ownership, and buffer size.</summary>
	/// <param name="handle">A file handle for the file that this FileStream object will encapsulate. </param>
	/// <param name="access">A constant that gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <param name="ownsHandle">true if the file handle will be owned by this FileStream instance; otherwise, false. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	[Obsolete("Use FileStream(SafeFileHandle handle, FileAccess access, int bufferSize) instead")]
	public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
		: this(handle, access, ownsHandle, bufferSize, isAsync: false, isConsoleWrapper: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission, FileStream instance ownership, buffer size, and synchronous or asynchronous state.</summary>
	/// <param name="handle">A file handle for the file that this FileStream object will encapsulate. </param>
	/// <param name="access">A constant that gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <param name="ownsHandle">true if the file handle will be owned by this FileStream instance; otherwise, false. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes.</param>
	/// <param name="isAsync">true if the handle was opened asynchronously (that is, in overlapped I/O mode); otherwise, false. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="access" /> is less than FileAccess.Read or greater than FileAccess.ReadWrite or <paramref name="bufferSize" /> is less than or equal to 0. </exception>
	/// <exception cref="T:System.ArgumentException">The handle is invalid. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	[Obsolete("Use FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) instead")]
	public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync)
		: this(handle, access, ownsHandle, bufferSize, isAsync, isConsoleWrapper: false)
	{
	}

	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	internal FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync, bool isConsoleWrapper)
	{
		if (handle == MonoIO.InvalidHandle)
		{
			throw new ArgumentException("handle", Locale.GetText("Invalid."));
		}
		Init(new SafeFileHandle(handle, ownsHandle: false), access, ownsHandle, bufferSize, isAsync, isConsoleWrapper);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path and creation mode.</summary>
	/// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
	/// <param name="mode">A constant that determines how to open or create the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred.-or-The stream has been closed. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	public FileStream(string path, FileMode mode)
		: this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, 4096, anonymous: false, FileOptions.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, and read/write permission.</summary>
	/// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
	/// <param name="mode">A constant that determines how to open or create the file. </param>
	/// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	public FileStream(string path, FileMode mode, FileAccess access)
		: this(path, mode, access, (access != FileAccess.Write) ? FileShare.Read : FileShare.None, 4096, isAsync: false, anonymous: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, read/write permission, and sharing permission.</summary>
	/// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
	/// <param name="mode">A constant that determines how to open or create the file. </param>
	/// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file. </param>
	/// <param name="share">A constant that determines how the file will be shared by processes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred. -or-The system is running Windows 98 or Windows 98 Second Edition and <paramref name="share" /> is set to FileShare.Delete.-or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
		: this(path, mode, access, share, 4096, anonymous: false, FileOptions.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, read/write and sharing permission, and buffer size.</summary>
	/// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
	/// <param name="mode">A constant that determines how to open or create the file. </param>
	/// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file. </param>
	/// <param name="share">A constant that determines how the file will be shared by processes. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> contain an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred. -or-The system is running Windows 98 or Windows 98 Second Edition and <paramref name="share" /> is set to FileShare.Delete.-or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
		: this(path, mode, access, share, bufferSize, anonymous: false, FileOptions.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, read/write and sharing permission, buffer size, and synchronous or asynchronous state.</summary>
	/// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
	/// <param name="mode">A constant that determines how to open or create the file. </param>
	/// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file. </param>
	/// <param name="share">A constant that determines how the file will be shared by processes. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes. </param>
	/// <param name="useAsync">Specifies whether to use asynchronous I/O or synchronous I/O. However, note that the underlying operating system might not support asynchronous I/O, so when specifying true, the handle might be opened synchronously depending on the platform. When opened asynchronously, the <see cref="M:System.IO.FileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> and <see cref="M:System.IO.FileStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> methods perform better on large reads or writes, but they might be much slower for small reads or writes. If the application is designed to take advantage of asynchronous I/O, set the <paramref name="useAsync" /> parameter to true. Using asynchronous I/O correctly can speed up applications by as much as a factor of 10, but using it without redesigning the application for asynchronous I/O can decrease performance by as much as a factor of 10. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> contain an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred.-or- The system is running Windows 98 or Windows 98 Second Edition and <paramref name="share" /> is set to FileShare.Delete.-or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
		: this(path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.</summary>
	/// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
	/// <param name="mode">A constant that determines how to open or create the file. </param>
	/// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file. </param>
	/// <param name="share">A constant that determines how the file will be shared by processes. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes. </param>
	/// <param name="options">A value that specifies additional file options.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> contain an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred.-or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. -or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" />, but file encryption is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
		: this(path, mode, access, share, bufferSize, anonymous: false, options)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission. </summary>
	/// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
	/// <param name="access">A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="access" /> is not a field of <see cref="T:System.IO.FileAccess" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	public FileStream(SafeFileHandle handle, FileAccess access)
		: this(handle, access, 4096, isAsync: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission, and buffer size.</summary>
	/// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> constant that gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="handle" /> parameter is an invalid handle.-or-The <paramref name="handle" /> parameter is a synchronous handle and it was used asynchronously. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="bufferSize" /> parameter is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed.  </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize)
		: this(handle, access, bufferSize, isAsync: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission, buffer size, and synchronous or asynchronous state.</summary>
	/// <param name="handle">A file handle for the file that this FileStream object will encapsulate. </param>
	/// <param name="access">A constant that gets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object. </param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes.</param>
	/// <param name="isAsync">true if the handle was opened asynchronously (that is, in overlapped I/O mode); otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="handle" /> parameter is an invalid handle.-or-The <paramref name="handle" /> parameter is a synchronous handle and it was used asynchronously. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="bufferSize" /> parameter is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed.  </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and the file handle is set for read-only access. </exception>
	public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
	{
		Init(handle, access, ownsHandle: false, bufferSize, isAsync, isConsoleWrapper: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, access rights and sharing permission, the buffer size, and additional file options.</summary>
	/// <param name="path">A relative or absolute path for the file that the current <see cref="T:System.IO.FileStream" /> object will encapsulate.</param>
	/// <param name="mode">A constant that determines how to open or create the file.</param>
	/// <param name="rights">A constant that determines the access rights to use when creating access and audit rules for the file.</param>
	/// <param name="share">A constant that determines how the file will be shared by processes.</param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes.</param>
	/// <param name="options">A constant that specifies additional file options.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> contain an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. -or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" />, but file encryption is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	[MonoLimitation("This ignores the rights parameter")]
	public FileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options)
		: this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, share, bufferSize, anonymous: false, options)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path, creation mode, access rights and sharing permission, the buffer size, additional file options, access control and audit security.</summary>
	/// <param name="path">A relative or absolute path for the file that the current <see cref="T:System.IO.FileStream" /> object will encapsulate.</param>
	/// <param name="mode">A constant that determines how to open or create the file.</param>
	/// <param name="rights">A constant that determines the access rights to use when creating access and audit rules for the file.</param>
	/// <param name="share">A constant that determines how the file will be shared by processes.</param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. For <paramref name="bufferSize" /> values between one and eight, the actual buffer size is set to eight bytes.</param>
	/// <param name="options">A constant that specifies additional file options.</param>
	/// <param name="fileSecurity">A constant that determines the access control and audit security for the file.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> contain an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode" /> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or ReadWrite and the file or directory is set for read-only access. -or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" />, but file encryption is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	[MonoLimitation("This ignores the rights and fileSecurity parameters")]
	public FileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity)
		: this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, share, bufferSize, anonymous: false, options)
	{
	}

	internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, string msgPath, bool bFromProxy, bool useLongPath = false, bool checkHost = false)
		: this(path, mode, access, share, bufferSize, anonymous: false, options)
	{
	}

	internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool isAsync, bool anonymous)
		: this(path, mode, access, share, bufferSize, anonymous, isAsync ? FileOptions.Asynchronous : FileOptions.None)
	{
	}

	internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool anonymous, FileOptions options)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Path is empty");
		}
		this.anonymous = anonymous;
		share &= ~FileShare.Inheritable;
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
		}
		if (mode < FileMode.CreateNew || mode > FileMode.Append)
		{
			throw new ArgumentOutOfRangeException("mode", "Enum value was out of legal range.");
		}
		if (access < FileAccess.Read || access > FileAccess.ReadWrite)
		{
			throw new ArgumentOutOfRangeException("access", "Enum value was out of legal range.");
		}
		if ((share < FileShare.None) || share > (FileShare.ReadWrite | FileShare.Delete))
		{
			throw new ArgumentOutOfRangeException("share", "Enum value was out of legal range.");
		}
		if (path.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("Name has invalid chars");
		}
		path = Path.InsecureGetFullPath(path);
		if (Directory.Exists(path))
		{
			throw new UnauthorizedAccessException(string.Format(Locale.GetText("Access to the path '{0}' is denied."), GetSecureFileName(path, full: false)));
		}
		if (mode == FileMode.Append && (access & FileAccess.Read) == FileAccess.Read)
		{
			throw new ArgumentException("Append access can be requested only in write-only mode.");
		}
		if ((access & FileAccess.Write) == 0 && mode != FileMode.Open && mode != FileMode.OpenOrCreate)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Combining FileMode: {0} with FileAccess: {1} is invalid."), access, mode));
		}
		string directoryName = Path.GetDirectoryName(path);
		if (directoryName.Length > 0 && !Directory.Exists(Path.GetFullPath(directoryName)))
		{
			throw new DirectoryNotFoundException(string.Format(Locale.GetText("Could not find a part of the path \"{0}\"."), anonymous ? directoryName : Path.GetFullPath(path)));
		}
		if (!anonymous)
		{
			name = path;
		}
		MonoIOError error;
		IntPtr intPtr = MonoIO.Open(path, mode, access, share, options, out error);
		if (intPtr == MonoIO.InvalidHandle)
		{
			throw MonoIO.GetException(GetSecureFileName(path), error);
		}
		safeHandle = new SafeFileHandle(intPtr, ownsHandle: false);
		this.access = access;
		owner = true;
		if (MonoIO.GetFileType(safeHandle, out error) == MonoFileType.Disk)
		{
			canseek = true;
			async = (options & FileOptions.Asynchronous) != 0;
		}
		else
		{
			canseek = false;
			async = false;
		}
		if (access == FileAccess.Read && canseek && bufferSize == 4096)
		{
			long length = Length;
			if (bufferSize > length)
			{
				bufferSize = (int)((length < 1000) ? 1000 : length);
			}
		}
		InitBuffer(bufferSize, isZeroSize: false);
		if (mode == FileMode.Append)
		{
			Seek(0L, SeekOrigin.End);
			append_startpos = Position;
		}
		else
		{
			append_startpos = 0L;
		}
	}

	private void Init(SafeFileHandle safeHandle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync, bool isConsoleWrapper)
	{
		if (!isConsoleWrapper && safeHandle.IsInvalid)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid handle."), "handle");
		}
		if (access < FileAccess.Read || access > FileAccess.ReadWrite)
		{
			throw new ArgumentOutOfRangeException("access");
		}
		if (!isConsoleWrapper && bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
		}
		MonoIOError error;
		MonoFileType fileType = MonoIO.GetFileType(safeHandle, out error);
		if (error != 0)
		{
			throw MonoIO.GetException(name, error);
		}
		switch (fileType)
		{
		case MonoFileType.Unknown:
			throw new IOException("Invalid handle.");
		case MonoFileType.Disk:
			canseek = true;
			break;
		default:
			canseek = false;
			break;
		}
		this.safeHandle = safeHandle;
		ExposeHandle();
		this.access = access;
		owner = ownsHandle;
		async = isAsync;
		anonymous = false;
		if (canseek)
		{
			buf_start = MonoIO.Seek(safeHandle, 0L, SeekOrigin.Current, out error);
			if (error != 0)
			{
				throw MonoIO.GetException(name, error);
			}
		}
		append_startpos = 0L;
	}

	private void ExposeHandle()
	{
		isExposed = true;
		FlushBuffer();
		InitBuffer(0, isZeroSize: true);
	}

	/// <summary>Reads a byte from the file and advances the read position one byte.</summary>
	/// <returns>The byte, cast to an <see cref="T:System.Int32" />, or -1 if the end of the stream has been reached.</returns>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public override int ReadByte()
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (!CanRead)
		{
			throw new NotSupportedException("Stream does not support reading");
		}
		if (buf_size == 0)
		{
			if (ReadData(safeHandle, buf, 0, 1) == 0)
			{
				return -1;
			}
			return buf[0];
		}
		if (buf_offset >= buf_length)
		{
			RefillBuffer();
			if (buf_length == 0)
			{
				return -1;
			}
		}
		return buf[buf_offset++];
	}

	/// <summary>Writes a byte to the current position in the file stream.</summary>
	/// <param name="value">A byte to write to the stream. </param>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
	/// <filterpriority>1</filterpriority>
	public override void WriteByte(byte value)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (!CanWrite)
		{
			throw new NotSupportedException("Stream does not support writing");
		}
		if (buf_offset == buf_size)
		{
			FlushBuffer();
		}
		if (buf_size == 0)
		{
			buf[0] = value;
			buf_dirty = true;
			buf_length = 1;
			FlushBuffer();
			return;
		}
		buf[buf_offset++] = value;
		if (buf_offset > buf_length)
		{
			buf_length = buf_offset;
		}
		buf_dirty = true;
	}

	/// <summary>Reads a block of bytes from the stream and writes the data in a given buffer.</summary>
	/// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
	/// <param name="array">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1<paramref name=")" /> replaced by the bytes read from the current source. </param>
	/// <param name="offset">The byte offset in <paramref name="array" /> at which the read bytes will be placed. </param>
	/// <param name="count">The maximum number of bytes to read. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> and <paramref name="count" /> describe an invalid range in <paramref name="array" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Read([In][Out] byte[] array, int offset, int count)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (!CanRead)
		{
			throw new NotSupportedException("Stream does not support reading");
		}
		int num = array.Length;
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "< 0");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "< 0");
		}
		if (offset > num)
		{
			throw new ArgumentException("destination offset is beyond array size");
		}
		if (offset > num - count)
		{
			throw new ArgumentException("Reading would overrun buffer");
		}
		if (async)
		{
			IAsyncResult asyncResult = BeginRead(array, offset, count, null, null);
			return EndRead(asyncResult);
		}
		return ReadInternal(array, offset, count);
	}

	private int ReadInternal(byte[] dest, int offset, int count)
	{
		int num = ReadSegment(dest, offset, count);
		if (num == count)
		{
			return count;
		}
		int num2 = num;
		count -= num;
		if (count > buf_size)
		{
			FlushBuffer();
			num = ReadData(safeHandle, dest, offset + num, count);
			buf_start += num;
		}
		else
		{
			RefillBuffer();
			num = ReadSegment(dest, offset + num2, count);
		}
		return num2 + num;
	}

	/// <summary>Begins an asynchronous read operation. (Consider using <see cref="M:System.IO.FileStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <returns>An object that references the asynchronous read.</returns>
	/// <param name="array">The buffer to read data into. </param>
	/// <param name="offset">The byte offset in <paramref name="array" /> at which to begin reading. </param>
	/// <param name="numBytes">The maximum number of bytes to read. </param>
	/// <param name="userCallback">The method to be called when the asynchronous read operation is completed. </param>
	/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous read request from other requests. </param>
	/// <exception cref="T:System.ArgumentException">The array length minus <paramref name="offset" /> is less than <paramref name="numBytes" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="numBytes" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An asynchronous read was attempted past the end of the file. </exception>
	/// <filterpriority>2</filterpriority>
	public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (!CanRead)
		{
			throw new NotSupportedException("This stream does not support reading");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (numBytes < 0)
		{
			throw new ArgumentOutOfRangeException("numBytes", "Must be >= 0");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
		}
		if (numBytes > array.Length - offset)
		{
			throw new ArgumentException("Buffer too small. numBytes/offset wrong.");
		}
		if (!async)
		{
			return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
		}
		return new ReadDelegate(ReadInternal).BeginInvoke(array, offset, numBytes, userCallback, stateObject);
	}

	/// <summary>Waits for the pending asynchronous read operation to complete. (Consider using <see cref="M:System.IO.FileStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <returns>The number of bytes read from the stream, between 0 and the number of bytes you requested. Streams only return 0 at the end of the stream, otherwise, they should block until at least 1 byte is available.</returns>
	/// <param name="asyncResult">The reference to the pending asynchronous request to wait for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">This <see cref="T:System.IAsyncResult" /> object was not created by calling <see cref="M:System.IO.FileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> on this class. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.IO.FileStream.EndRead(System.IAsyncResult)" /> is called multiple times. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	/// <filterpriority>2</filterpriority>
	public override int EndRead(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!async)
		{
			return base.EndRead(asyncResult);
		}
		return ((((asyncResult as AsyncResult) ?? throw new ArgumentException("Invalid IAsyncResult", "asyncResult")).AsyncDelegate as ReadDelegate) ?? throw new ArgumentException("Invalid IAsyncResult", "asyncResult")).EndInvoke(asyncResult);
	}

	/// <summary>Writes a block of bytes to the file stream.</summary>
	/// <param name="array">The buffer containing data to write to the stream.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="array" /> from which to begin copying bytes to the stream. </param>
	/// <param name="count">The maximum number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> and <paramref name="count" /> describe an invalid range in <paramref name="array" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. - or -Another thread may have caused an unexpected change in the position of the operating system's file handle. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The current stream instance does not support writing. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Write(byte[] array, int offset, int count)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "< 0");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "< 0");
		}
		if (offset > array.Length - count)
		{
			throw new ArgumentException("Reading would overrun buffer");
		}
		if (!CanWrite)
		{
			throw new NotSupportedException("Stream does not support writing");
		}
		if (async)
		{
			IAsyncResult asyncResult = BeginWrite(array, offset, count, null, null);
			EndWrite(asyncResult);
		}
		else
		{
			WriteInternal(array, offset, count);
		}
	}

	private void WriteInternal(byte[] src, int offset, int count)
	{
		if (count > buf_size)
		{
			FlushBuffer();
			MonoIOError error;
			if (CanSeek && !isExposed)
			{
				MonoIO.Seek(safeHandle, buf_start, SeekOrigin.Begin, out error);
				if (error != 0)
				{
					throw MonoIO.GetException(GetSecureFileName(name), error);
				}
			}
			int num = count;
			while (num > 0)
			{
				int num2 = MonoIO.Write(safeHandle, src, offset, num, out error);
				if (error != 0)
				{
					throw MonoIO.GetException(GetSecureFileName(name), error);
				}
				num -= num2;
				offset += num2;
			}
			buf_start += count;
			return;
		}
		int num3 = 0;
		while (count > 0)
		{
			int num4 = WriteSegment(src, offset + num3, count);
			num3 += num4;
			count -= num4;
			if (count != 0)
			{
				FlushBuffer();
				continue;
			}
			break;
		}
	}

	/// <summary>Begins an asynchronous write operation. (Consider using <see cref="M:System.IO.FileStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <returns>An object that references the asynchronous write.</returns>
	/// <param name="array">The buffer containing data to write to the current stream.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="array" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="numBytes">The maximum number of bytes to write. </param>
	/// <param name="userCallback">The method to be called when the asynchronous write operation is completed. </param>
	/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous write request from other requests. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> length minus <paramref name="offset" /> is less than <paramref name="numBytes" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="numBytes" /> is negative. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>2</filterpriority>
	public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (!CanWrite)
		{
			throw new NotSupportedException("This stream does not support writing");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (numBytes < 0)
		{
			throw new ArgumentOutOfRangeException("numBytes", "Must be >= 0");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
		}
		if (numBytes > array.Length - offset)
		{
			throw new ArgumentException("array too small. numBytes/offset wrong.");
		}
		if (!async)
		{
			return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
		}
		new FileStreamAsyncResult(userCallback, stateObject)
		{
			BytesRead = -1,
			Count = numBytes,
			OriginalCount = numBytes
		};
		return new WriteDelegate(WriteInternal).BeginInvoke(array, offset, numBytes, userCallback, stateObject);
	}

	/// <summary>Ends an asynchronous write operation and blocks until the I/O operation is complete. (Consider using <see cref="M:System.IO.FileStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead; see the Remarks section.)</summary>
	/// <param name="asyncResult">The pending asynchronous I/O request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">This <see cref="T:System.IAsyncResult" /> object was not created by calling <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> on this class. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.IO.FileStream.EndWrite(System.IAsyncResult)" /> is called multiple times. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	/// <filterpriority>2</filterpriority>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!async)
		{
			base.EndWrite(asyncResult);
		}
		else
		{
			((((asyncResult as AsyncResult) ?? throw new ArgumentException("Invalid IAsyncResult", "asyncResult")).AsyncDelegate as WriteDelegate) ?? throw new ArgumentException("Invalid IAsyncResult", "asyncResult")).EndInvoke(asyncResult);
		}
	}

	/// <summary>Sets the current position of this stream to the given value.</summary>
	/// <returns>The new position in the stream.</returns>
	/// <param name="offset">The point relative to <paramref name="origin" /> from which to begin seeking. </param>
	/// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for <paramref name="origin" />, using a value of type <see cref="T:System.IO.SeekOrigin" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the FileStream is constructed from a pipe or console output. </exception>
	/// <exception cref="T:System.ArgumentException">Seeking is attempted before the beginning of the stream. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>1</filterpriority>
	public override long Seek(long offset, SeekOrigin origin)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (!CanSeek)
		{
			throw new NotSupportedException("The stream does not support seeking");
		}
		long num = origin switch
		{
			SeekOrigin.End => Length + offset, 
			SeekOrigin.Current => Position + offset, 
			SeekOrigin.Begin => offset, 
			_ => throw new ArgumentException("origin", "Invalid SeekOrigin"), 
		};
		if (num < 0)
		{
			throw new IOException("Attempted to Seek before the beginning of the stream");
		}
		if (num < append_startpos)
		{
			throw new IOException("Can't seek back over pre-existing data in append mode");
		}
		FlushBuffer();
		buf_start = MonoIO.Seek(safeHandle, num, SeekOrigin.Begin, out var error);
		if (error != 0)
		{
			throw MonoIO.GetException(GetSecureFileName(name), error);
		}
		return buf_start;
	}

	/// <summary>Sets the length of this stream to the given value.</summary>
	/// <param name="value">The new length of the stream. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Attempted to set the <paramref name="value" /> parameter to less than 0. </exception>
	/// <filterpriority>2</filterpriority>
	public override void SetLength(long value)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (!CanSeek)
		{
			throw new NotSupportedException("The stream does not support seeking");
		}
		if (!CanWrite)
		{
			throw new NotSupportedException("The stream does not support writing");
		}
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value is less than 0");
		}
		FlushBuffer();
		MonoIO.SetLength(safeHandle, value, out var error);
		if (error != 0)
		{
			throw MonoIO.GetException(GetSecureFileName(name), error);
		}
		if (Position > value)
		{
			Position = value;
		}
	}

	/// <summary>Clears buffers for this stream and causes any buffered data to be written to the file.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Flush()
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		FlushBuffer();
	}

	/// <summary>Clears buffers for this stream and causes any buffered data to be written to the file, and also clears all intermediate file buffers.</summary>
	/// <param name="flushToDisk">true to flush all intermediate file buffers; otherwise, false. </param>
	public virtual void Flush(bool flushToDisk)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		FlushBuffer();
		if (flushToDisk)
		{
			MonoIO.Flush(safeHandle, out var _);
		}
	}

	/// <summary>Prevents other processes from reading from or writing to the <see cref="T:System.IO.FileStream" />.</summary>
	/// <param name="position">The beginning of the range to lock. The value of this parameter must be equal to or greater than zero (0). </param>
	/// <param name="length">The range to be locked. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> or <paramref name="length" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The file is closed. </exception>
	/// <exception cref="T:System.IO.IOException">The process cannot access the file because another process has locked a portion of the file.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Lock(long position, long length)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position must not be negative");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length must not be negative");
		}
		MonoIO.Lock(safeHandle, position, length, out var error);
		if (error != 0)
		{
			throw MonoIO.GetException(GetSecureFileName(name), error);
		}
	}

	/// <summary>Allows access by other processes to all or part of a file that was previously locked.</summary>
	/// <param name="position">The beginning of the range to unlock. </param>
	/// <param name="length">The range to be unlocked. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> or <paramref name="length" /> is negative. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Unlock(long position, long length)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position must not be negative");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length must not be negative");
		}
		MonoIO.Unlock(safeHandle, position, length, out var error);
		if (error != 0)
		{
			throw MonoIO.GetException(GetSecureFileName(name), error);
		}
	}

	/// <summary>Ensures that resources are freed and other cleanup operations are performed when the garbage collector reclaims the FileStream.</summary>
	~FileStream()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.FileStream" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		Exception ex = null;
		if (safeHandle != null && !safeHandle.IsClosed)
		{
			try
			{
				FlushBuffer();
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			if (owner)
			{
				MonoIO.Close(safeHandle.DangerousGetHandle(), out var error);
				if (error != 0)
				{
					throw MonoIO.GetException(GetSecureFileName(name), error);
				}
				safeHandle.DangerousRelease();
			}
		}
		canseek = false;
		access = (FileAccess)0;
		if (disposing && buf != null)
		{
			if (buf.Length == 4096 && buf_recycle == null)
			{
				lock (buf_recycle_lock)
				{
					if (buf_recycle == null)
					{
						buf_recycle = buf;
					}
				}
			}
			buf = null;
			GC.SuppressFinalize(this);
		}
		if (ex != null)
		{
			throw ex;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control list (ACL) entries for the file described by the current <see cref="T:System.IO.FileStream" /> object.</summary>
	/// <returns>An object that encapsulates the access control settings for the file described by the current <see cref="T:System.IO.FileStream" /> object.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">This operation is not supported on the current platform.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public FileSecurity GetAccessControl()
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		return new FileSecurity(SafeFileHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Applies access control list (ACL) entries described by a <see cref="T:System.Security.AccessControl.FileSecurity" /> object to the file described by the current <see cref="T:System.IO.FileStream" /> object.</summary>
	/// <param name="fileSecurity">An object that describes an ACL entry to apply to the current file.</param>
	/// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileSecurity" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found or modified.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The current process does not have access to open the file.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetAccessControl(FileSecurity fileSecurity)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		if (fileSecurity == null)
		{
			throw new ArgumentNullException("fileSecurity");
		}
		fileSecurity.PersistModifications(SafeFileHandle);
	}

	/// <summary>Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests. </summary>
	/// <returns>A task that represents the asynchronous flush operation. </returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		if (safeHandle.IsClosed)
		{
			throw new ObjectDisposedException("Stream has been closed");
		}
		return base.FlushAsync(cancellationToken);
	}

	/// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
	/// <param name="buffer">The buffer to write the data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		return base.ReadAsync(buffer, offset, count, cancellationToken);
	}

	/// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests. </summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer to write data from. </param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation. </exception>
	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		return base.WriteAsync(buffer, offset, count, cancellationToken);
	}

	private int ReadSegment(byte[] dest, int dest_offset, int count)
	{
		count = Math.Min(count, buf_length - buf_offset);
		if (count > 0)
		{
			Buffer.InternalBlockCopy(buf, buf_offset, dest, dest_offset, count);
			buf_offset += count;
		}
		return count;
	}

	private int WriteSegment(byte[] src, int src_offset, int count)
	{
		if (count > buf_size - buf_offset)
		{
			count = buf_size - buf_offset;
		}
		if (count > 0)
		{
			Buffer.BlockCopy(src, src_offset, buf, buf_offset, count);
			buf_offset += count;
			if (buf_offset > buf_length)
			{
				buf_length = buf_offset;
			}
			buf_dirty = true;
		}
		return count;
	}

	private void FlushBuffer()
	{
		if (buf_dirty)
		{
			MonoIOError error;
			if (CanSeek && !isExposed)
			{
				MonoIO.Seek(safeHandle, buf_start, SeekOrigin.Begin, out error);
				if (error != 0)
				{
					throw MonoIO.GetException(GetSecureFileName(name), error);
				}
			}
			int num = buf_length;
			int num2 = 0;
			while (num > 0)
			{
				int num3 = MonoIO.Write(safeHandle, buf, num2, buf_length, out error);
				if (error != 0)
				{
					throw MonoIO.GetException(GetSecureFileName(name), error);
				}
				num -= num3;
				num2 += num3;
			}
		}
		buf_start += buf_offset;
		buf_offset = (buf_length = 0);
		buf_dirty = false;
	}

	private void FlushBufferIfDirty()
	{
		if (buf_dirty)
		{
			FlushBuffer();
		}
	}

	private void RefillBuffer()
	{
		FlushBuffer();
		buf_length = ReadData(safeHandle, buf, 0, buf_size);
	}

	private int ReadData(SafeHandle safeHandle, byte[] buf, int offset, int count)
	{
		int num = 0;
		num = MonoIO.Read(safeHandle, buf, offset, count, out var error);
		switch (error)
		{
		case MonoIOError.ERROR_BROKEN_PIPE:
			num = 0;
			break;
		default:
			throw MonoIO.GetException(GetSecureFileName(name), error);
		case MonoIOError.ERROR_SUCCESS:
			break;
		}
		if (num == -1)
		{
			throw new IOException();
		}
		return num;
	}

	private void InitBuffer(int size, bool isZeroSize)
	{
		if (isZeroSize)
		{
			size = 0;
			buf = new byte[1];
		}
		else
		{
			if (size <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
			}
			size = Math.Max(size, 8);
			if (size <= 4096 && buf_recycle != null)
			{
				lock (buf_recycle_lock)
				{
					if (buf_recycle != null)
					{
						buf = buf_recycle;
						buf_recycle = null;
					}
				}
			}
			if (buf == null)
			{
				buf = new byte[size];
			}
			else
			{
				Array.Clear(buf, 0, size);
			}
		}
		buf_size = size;
	}

	private string GetSecureFileName(string filename)
	{
		if (!anonymous)
		{
			return Path.GetFullPath(filename);
		}
		return Path.GetFileName(filename);
	}

	private string GetSecureFileName(string filename, bool full)
	{
		if (!anonymous)
		{
			if (!full)
			{
				return filename;
			}
			return Path.GetFullPath(filename);
		}
		return Path.GetFileName(filename);
	}
}
