using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.IO.IsolatedStorage;

/// <summary>Exposes a file within isolated storage. </summary>
[ComVisible(true)]
public class IsolatedStorageFileStream : FileStream
{
	/// <summary>Gets a Boolean value indicating whether the file can be read.</summary>
	/// <returns>true if an <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object can be read; otherwise, false.</returns>
	public override bool CanRead => base.CanRead;

	/// <summary>Gets a Boolean value indicating whether seek operations are supported.</summary>
	/// <returns>true if an <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object supports seek operations; otherwise, false.</returns>
	public override bool CanSeek => base.CanSeek;

	/// <summary>Gets a Boolean value indicating whether you can write to the file.</summary>
	/// <returns>true if an <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object can be written; otherwise, false.</returns>
	public override bool CanWrite => base.CanWrite;

	/// <summary>Gets a <see cref="T:Microsoft.Win32.SafeHandles.SafeFileHandle" /> object that represents the operating system file handle for the file that the current <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object encapsulates.</summary>
	/// <returns>A <see cref="T:Microsoft.Win32.SafeHandles.SafeFileHandle" /> object that represents the operating system file handle for the file that the current <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object encapsulates.</returns>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">The <see cref="P:System.IO.IsolatedStorage.IsolatedStorageFileStream.SafeFileHandle" /> property always generates this exception. </exception>
	public override SafeFileHandle SafeFileHandle
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		get
		{
			throw new IsolatedStorageException(Locale.GetText("Information is restricted"));
		}
	}

	/// <summary>Gets the file handle for the file that the current <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object encapsulates. Accessing this property is not permitted on an <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object, and throws an <see cref="T:System.IO.IsolatedStorage.IsolatedStorageException" />.</summary>
	/// <returns>The file handle for the file that the current <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object encapsulates.</returns>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">The <see cref="P:System.IO.IsolatedStorage.IsolatedStorageFileStream.Handle" /> property always generates this exception.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[Obsolete("Use SafeFileHandle - once available")]
	public override IntPtr Handle
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		get
		{
			throw new IsolatedStorageException(Locale.GetText("Information is restricted"));
		}
	}

	/// <summary>Gets a Boolean value indicating whether the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object was opened asynchronously or synchronously.</summary>
	/// <returns>true if the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object supports asynchronous access; otherwise, false.</returns>
	public override bool IsAsync => base.IsAsync;

	/// <summary>Gets the length of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object.</summary>
	/// <returns>The length of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object in bytes.</returns>
	public override long Length => base.Length;

	/// <summary>Gets or sets the current position of the current <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object.</summary>
	/// <returns>The current position of this <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The position cannot be set to a negative number. </exception>
	public override long Position
	{
		get
		{
			return base.Position;
		}
		set
		{
			base.Position = value;
		}
	}

	[ReflectionPermission(SecurityAction.Assert, TypeInformation = true)]
	private static string CreateIsolatedPath(IsolatedStorageFile isf, string path, FileMode mode)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (!Enum.IsDefined(typeof(FileMode), mode))
		{
			throw new ArgumentException("mode");
		}
		if (isf == null)
		{
			isf = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, IsolatedStorageFile.GetDomainIdentityFromEvidence(AppDomain.CurrentDomain.Evidence), IsolatedStorageFile.GetAssemblyIdentityFromEvidence(new StackFrame(3).GetMethod().ReflectedType.Assembly.UnprotectedGetEvidence()));
		}
		if (isf.IsDisposed)
		{
			throw new ObjectDisposedException("IsolatedStorageFile");
		}
		if (isf.IsClosed)
		{
			throw new InvalidOperationException("Storage needs to be open for this operation.");
		}
		FileInfo fileInfo = new FileInfo(isf.Root);
		if (!fileInfo.Directory.Exists)
		{
			fileInfo.Directory.Create();
		}
		if (Path.IsPathRooted(path))
		{
			string pathRoot = Path.GetPathRoot(path);
			path = path.Remove(0, pathRoot.Length);
		}
		string text = Path.Combine(isf.Root, path);
		Path.GetFullPath(text);
		if (!Path.GetFullPath(text).StartsWith(isf.Root))
		{
			throw new IsolatedStorageException();
		}
		fileInfo = new FileInfo(text);
		if (!fileInfo.Directory.Exists)
		{
			throw new DirectoryNotFoundException(string.Format(Locale.GetText("Could not find a part of the path \"{0}\"."), path));
		}
		return text;
	}

	/// <summary>Initializes a new instance of an <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object giving access to the file designated by <paramref name="path" /> in the specified <paramref name="mode" />.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The directory in <paramref name="path" /> does not exist. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" /></exception>
	public IsolatedStorageFileStream(string path, FileMode mode)
		: this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, 4096, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" />, in the specified <paramref name="mode" />, with the kind of <paramref name="access" /> requested.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access)
		: this(path, mode, access, (access != FileAccess.Write) ? FileShare.Read : FileShare.None, 4096, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" />, in the specified <paramref name="mode" />, with the specified file <paramref name="access" />, using the file sharing mode specified by <paramref name="share" />.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <param name="share">A bitwise combination of the <see cref="T:System.IO.FileShare" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share)
		: this(path, mode, access, share, 4096, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" />, in the specified <paramref name="mode" />, with the specified file <paramref name="access" />, using the file sharing mode specified by <paramref name="share" />, with the <paramref name="buffersize" /> specified.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <param name="share">A bitwise combination of the <see cref="T:System.IO.FileShare" /> values. </param>
	/// <param name="bufferSize">The <see cref="T:System.IO.FileStream" /> buffer size. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
		: this(path, mode, access, share, bufferSize, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" />, in the specified <paramref name="mode" />, with the specified file <paramref name="access" />, using the file sharing mode specified by <paramref name="share" />, with the <paramref name="buffersize" /> specified, and in the context of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> specified by <paramref name="isf" />.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <param name="share">A bitwise combination of the <see cref="T:System.IO.FileShare" /> values </param>
	/// <param name="bufferSize">The <see cref="T:System.IO.FileStream" /> buffer size. </param>
	/// <param name="isf">The <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> in which to open the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">
	///   <paramref name="isf" /> does not have a quota. </exception>
	[FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
	public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, IsolatedStorageFile isf)
		: base(CreateIsolatedPath(isf, path, mode), mode, access, share, bufferSize, isAsync: false, anonymous: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" />, in the specified <paramref name="mode" />, with the specified file <paramref name="access" />, using the file sharing mode specified by <paramref name="share" />, and in the context of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> specified by <paramref name="isf" />.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <param name="share">A bitwise combination of the <see cref="T:System.IO.FileShare" /> values. </param>
	/// <param name="isf">The <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> in which to open the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">
	///   <paramref name="isf" /> does not have a quota. </exception>
	public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
		: this(path, mode, access, share, 4096, isf)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" /> in the specified <paramref name="mode" />, with the specified file <paramref name="access" />, and in the context of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> specified by <paramref name="isf" />.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <param name="isf">The <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> in which to open the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The isolated store is closed.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">
	///   <paramref name="isf" /> does not have a quota. </exception>
	public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
		: this(path, mode, access, (access != FileAccess.Write) ? FileShare.Read : FileShare.None, 4096, isf)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> class giving access to the file designated by <paramref name="path" />, in the specified <paramref name="mode" />, and in the context of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> specified by <paramref name="isf" />.</summary>
	/// <param name="path">The relative path of the file within isolated storage. </param>
	/// <param name="mode">One of the <see cref="T:System.IO.FileMode" /> values. </param>
	/// <param name="isf">The <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> in which to open the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> is badly formed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">No file was found and the <paramref name="mode" /> is set to <see cref="F:System.IO.FileMode.Open" />. </exception>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">
	///   <paramref name="isf" /> does not have a quota. </exception>
	public IsolatedStorageFileStream(string path, FileMode mode, IsolatedStorageFile isf)
		: this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, 4096, isf)
	{
	}

	/// <summary>Begins an asynchronous read.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that represents the asynchronous read, which is possibly still pending. This <see cref="T:System.IAsyncResult" /> must be passed to this stream's <see cref="M:System.IO.IsolatedStorage.IsolatedStorageFileStream.EndRead(System.IAsyncResult)" /> method to determine how many bytes were read. This can be done either by the same code that called <see cref="M:System.IO.IsolatedStorage.IsolatedStorageFileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> or in a callback passed to <see cref="M:System.IO.IsolatedStorage.IsolatedStorageFileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />.</returns>
	/// <param name="buffer">The buffer to read data into. </param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin reading. </param>
	/// <param name="numBytes">The maximum number of bytes to read. </param>
	/// <param name="userCallback">The method to call when the asynchronous read operation is completed. This parameter is optional. </param>
	/// <param name="stateObject">The status of the asynchronous read. </param>
	/// <exception cref="T:System.IO.IOException">An asynchronous read was attempted past the end of the file. </exception>
	public override IAsyncResult BeginRead(byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		return base.BeginRead(buffer, offset, numBytes, userCallback, stateObject);
	}

	/// <summary>Begins an asynchronous write.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that represents the asynchronous write, which is possibly still pending. This <see cref="T:System.IAsyncResult" /> must be passed to this stream's <see cref="M:System.IO.Stream.EndWrite(System.IAsyncResult)" /> method to ensure that the write is complete, then frees resources appropriately. This can be done either by the same code that called <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> or in a callback passed to <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />.</returns>
	/// <param name="buffer">The buffer to write data to. </param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing. </param>
	/// <param name="numBytes">The maximum number of bytes to write. </param>
	/// <param name="userCallback">The method to call when the asynchronous write operation is completed. This parameter is optional. </param>
	/// <param name="stateObject">The status of the asynchronous write. </param>
	/// <exception cref="T:System.IO.IOException">An asynchronous write was attempted past the end of the file. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		return base.BeginWrite(buffer, offset, numBytes, userCallback, stateObject);
	}

	/// <summary>Ends a pending asynchronous read request.</summary>
	/// <returns>The number of bytes read from the stream, between zero and the number of requested bytes. Streams will only return zero at the end of the stream. Otherwise, they will block until at least one byte is available.</returns>
	/// <param name="asyncResult">The pending asynchronous request. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> is null. </exception>
	public override int EndRead(IAsyncResult asyncResult)
	{
		return base.EndRead(asyncResult);
	}

	/// <summary>Ends an asynchronous write.</summary>
	/// <param name="asyncResult">The pending asynchronous I/O request to end. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> parameter is null. </exception>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		base.EndWrite(asyncResult);
	}

	/// <summary>Clears buffers for this stream and causes any buffered data to be written to the file.</summary>
	public override void Flush()
	{
		base.Flush();
	}

	/// <summary>Clears buffers for this stream and causes any buffered data to be written to the file, and also clears all intermediate file buffers.</summary>
	/// <param name="flushToDisk">true to flush all intermediate file buffers; otherwise, false.</param>
	public override void Flush(bool flushToDisk)
	{
		base.Flush(flushToDisk);
	}

	/// <summary>Copies bytes from the current buffered <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object to an array.</summary>
	/// <returns>The total number of bytes read into the <paramref name="buffer" />. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the end of the stream is reached.</returns>
	/// <param name="buffer">The buffer to read. </param>
	/// <param name="offset">The offset in the buffer at which to begin writing. </param>
	/// <param name="count">The maximum number of bytes to read. </param>
	public override int Read(byte[] buffer, int offset, int count)
	{
		return base.Read(buffer, offset, count);
	}

	/// <summary>Reads a single byte from the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object in isolated storage.</summary>
	/// <returns>The 8-bit unsigned integer value read from the isolated storage file.</returns>
	public override int ReadByte()
	{
		return base.ReadByte();
	}

	/// <summary>Sets the current position of this <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object to the specified value.</summary>
	/// <returns>The new position in the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object.</returns>
	/// <param name="offset">The new position of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object. </param>
	/// <param name="origin">One of the <see cref="T:System.IO.SeekOrigin" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="origin" /> must be one of the <see cref="T:System.IO.SeekOrigin" /> values. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override long Seek(long offset, SeekOrigin origin)
	{
		return base.Seek(offset, origin);
	}

	/// <summary>Sets the length of this <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object to the specified <paramref name="value" />.</summary>
	/// <param name="value">The new length of the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is a negative number.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override void SetLength(long value)
	{
		base.SetLength(value);
	}

	/// <summary>Writes a block of bytes to the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object using data read from a byte array.</summary>
	/// <param name="buffer">The buffer to write. </param>
	/// <param name="offset">The byte offset in buffer from which to begin. </param>
	/// <param name="count">The maximum number of bytes to write. </param>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">The write attempt exceeds the quota for the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override void Write(byte[] buffer, int offset, int count)
	{
		base.Write(buffer, offset, count);
	}

	/// <summary>Writes a single byte to the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object.</summary>
	/// <param name="value">The byte value to write to the isolated storage file. </param>
	/// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">The write attempt exceeds the quota for the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> object. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override void WriteByte(byte value)
	{
		base.WriteByte(value);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFileStream" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources </param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
