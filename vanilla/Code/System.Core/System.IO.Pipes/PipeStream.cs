using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> object around a pipe, which supports both anonymous and named pipes.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public abstract class PipeStream : Stream
{
	internal const int DefaultBufferSize = 1024;

	private PipeDirection direction;

	private PipeTransmissionMode transmission_mode;

	private PipeTransmissionMode read_trans_mode;

	private int buffer_size;

	private SafePipeHandle handle;

	private Stream stream;

	private Func<byte[], int, int, int> read_delegate;

	private Action<byte[], int, int> write_delegate;

	internal static bool IsWindows => Win32Marshal.IsWindows;

	/// <summary>Gets a value indicating whether the current stream supports read operations.</summary>
	/// <returns>true if the stream supports read operations; otherwise, false.</returns>
	public override bool CanRead => (direction & PipeDirection.In) != 0;

	/// <summary>Gets a value indicating whether the current stream supports seek operations.</summary>
	/// <returns>false in all cases.</returns>
	public override bool CanSeek => false;

	/// <summary>Gets a value indicating whether the current stream supports write operations.</summary>
	/// <returns>true if the stream supports write operations; otherwise, false.</returns>
	public override bool CanWrite => (direction & PipeDirection.Out) != 0;

	/// <summary>Gets the size, in bytes, of the inbound buffer for a pipe.</summary>
	/// <returns>An integer value that represents the inbound buffer size, in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">The stream is unreadable.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is waiting to connect.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public virtual int InBufferSize => buffer_size;

	/// <summary>Gets a value indicating whether a <see cref="T:System.IO.Pipes.PipeStream" /> object was opened asynchronously or synchronously.</summary>
	/// <returns>true if the <see cref="T:System.IO.Pipes.PipeStream" /> object was opened asynchronously; otherwise, false.</returns>
	public bool IsAsync { get; private set; }

	/// <summary>Gets or sets a value indicating whether a <see cref="T:System.IO.Pipes.PipeStream" /> object is connected.</summary>
	/// <returns>true if the <see cref="T:System.IO.Pipes.PipeStream" /> object is connected; otherwise, false.</returns>
	public bool IsConnected { get; protected set; }

	internal Stream Stream
	{
		get
		{
			if (!IsConnected)
			{
				throw new InvalidOperationException("Pipe is not connected");
			}
			if (stream == null)
			{
				stream = new FileStream(handle.DangerousGetHandle(), (!CanRead) ? FileAccess.Write : ((!CanWrite) ? FileAccess.Read : FileAccess.ReadWrite), ownsHandle: false, buffer_size, IsAsync);
			}
			return stream;
		}
		set
		{
			stream = value;
		}
	}

	/// <summary>Gets a value indicating whether a handle to a <see cref="T:System.IO.Pipes.PipeStream" /> object is exposed.</summary>
	/// <returns>true if a handle to the <see cref="T:System.IO.Pipes.PipeStream" /> object is exposed; otherwise, false.</returns>
	protected bool IsHandleExposed { get; private set; }

	/// <summary>Gets a value indicating whether there is more data in the message returned from the most recent read operation.</summary>
	/// <returns>true if there are no more characters to read in the message; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The pipe is not connected.-or-The pipe handle has not been set.-or-The pipe's <see cref="P:System.IO.Pipes.PipeStream.ReadMode" /> property value is not <see cref="F:System.IO.Pipes.PipeTransmissionMode.Message" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	[MonoTODO]
	public bool IsMessageComplete { get; private set; }

	/// <summary>Gets the size, in bytes, of the outbound buffer for a pipe.</summary>
	/// <returns>The outbound buffer size, in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">The stream is unwriteable.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is waiting to connect.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	[MonoTODO]
	public virtual int OutBufferSize => buffer_size;

	/// <summary>Gets or sets the reading mode for a <see cref="T:System.IO.Pipes.PipeStream" /> object.</summary>
	/// <returns>One of the <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> values that indicates how the <see cref="T:System.IO.Pipes.PipeStream" /> object reads from the pipe.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The supplied value is not a valid <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">The supplied value is not a supported <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> value for this pipe stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">The handle has not been set.-or-The pipe is waiting to connect with a named client.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or an I/O error occurred with a named client.</exception>
	public virtual PipeTransmissionMode ReadMode
	{
		get
		{
			CheckPipePropertyOperations();
			return read_trans_mode;
		}
		set
		{
			CheckPipePropertyOperations();
			read_trans_mode = value;
		}
	}

	/// <summary>Gets the safe handle for the local end of the pipe that the current <see cref="T:System.IO.Pipes.PipeStream" /> object encapsulates.</summary>
	/// <returns>A <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> object for the pipe that is encapsulated by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public SafePipeHandle SafePipeHandle
	{
		get
		{
			CheckPipePropertyOperations();
			return handle;
		}
	}

	/// <summary>Gets the pipe transmission mode supported by the current pipe.</summary>
	/// <returns>One of the <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> values that indicates the transmission mode supported by the current pipe.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The handle has not been set.-or-The pipe is waiting to connect in an anonymous client/server operation or with a named client. </exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public virtual PipeTransmissionMode TransmissionMode
	{
		get
		{
			CheckPipePropertyOperations();
			return transmission_mode;
		}
	}

	/// <summary>Gets the length of a stream, in bytes.</summary>
	/// <returns>0 in all cases.</returns>
	/// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets or sets the current position of the current stream.</summary>
	/// <returns>0 in all cases.</returns>
	/// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
	public override long Position
	{
		get
		{
			return 0L;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	internal Exception ThrowACLException()
	{
		return new NotImplementedException("ACL is not supported in Mono");
	}

	internal static PipeAccessRights ToAccessRights(PipeDirection direction)
	{
		return direction switch
		{
			PipeDirection.In => PipeAccessRights.ReadData, 
			PipeDirection.Out => PipeAccessRights.WriteData, 
			PipeDirection.InOut => PipeAccessRights.ReadData | PipeAccessRights.WriteData, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	internal static PipeDirection ToDirection(PipeAccessRights rights)
	{
		bool num = (rights & PipeAccessRights.ReadData) != 0;
		bool flag = (rights & PipeAccessRights.WriteData) != 0;
		if (num)
		{
			if (flag)
			{
				return PipeDirection.InOut;
			}
			return PipeDirection.In;
		}
		if (flag)
		{
			return PipeDirection.Out;
		}
		throw new ArgumentOutOfRangeException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeStream" /> class using the specified <see cref="T:System.IO.Pipes.PipeDirection" /> value and buffer size.</summary>
	/// <param name="direction">One of the <see cref="T:System.IO.Pipes.PipeDirection" /> values that indicates the direction of the pipe object.</param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than or equal to 0 that indicates the buffer size.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="bufferSize" /> is less than 0.</exception>
	protected PipeStream(PipeDirection direction, int bufferSize)
		: this(direction, PipeTransmissionMode.Byte, bufferSize)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeStream" /> class using the specified <see cref="T:System.IO.Pipes.PipeDirection" />, <see cref="T:System.IO.Pipes.PipeTransmissionMode" />, and buffer size.</summary>
	/// <param name="direction">One of the <see cref="T:System.IO.Pipes.PipeDirection" /> values that indicates the direction of the pipe object.</param>
	/// <param name="transmissionMode">One of the <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> values that indicates the transmission mode of the pipe object.</param>
	/// <param name="outBufferSize">A positive <see cref="T:System.Int32" /> value greater than or equal to 0 that indicates the buffer size.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="transmissionMode" /> is not a valid <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> value.-or-<paramref name="bufferSize" /> is less than 0.</exception>
	protected PipeStream(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize)
	{
		this.direction = direction;
		transmission_mode = transmissionMode;
		read_trans_mode = transmissionMode;
		if (outBufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize must be greater than 0");
		}
		buffer_size = outBufferSize;
	}

	/// <summary>Verifies that the pipe is in a proper state for getting or setting properties.</summary>
	[MonoTODO]
	protected internal virtual void CheckPipePropertyOperations()
	{
	}

	/// <summary>Verifies that the pipe is in a connected state for read operations.</summary>
	[MonoTODO]
	protected internal void CheckReadOperations()
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("Pipe is not connected");
		}
		if (!CanRead)
		{
			throw new NotSupportedException("The pipe stream does not support read operations");
		}
	}

	/// <summary>Verifies that the pipe is in a connected state for write operations.</summary>
	[MonoTODO]
	protected internal void CheckWriteOperations()
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("Pipe is not connected");
		}
		if (!CanWrite)
		{
			throw new NotSupportedException("The pipe stream does not support write operations");
		}
	}

	/// <summary>Initializes a <see cref="T:System.IO.Pipes.PipeStream" /> object from the specified <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> object.</summary>
	/// <param name="handle">The <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> object of the pipe to initialize.</param>
	/// <param name="isExposed">true to expose the handle; otherwise, false.</param>
	/// <param name="isAsync">true to indicate that the handle was opened asynchronously; otherwise, false.</param>
	/// <exception cref="T:System.IO.IOException">A handle cannot be bound to the pipe.</exception>
	protected void InitializeHandle(SafePipeHandle handle, bool isExposed, bool isAsync)
	{
		this.handle = handle;
		IsHandleExposed = isExposed;
		IsAsync = isAsync;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Pipes.PipeStream" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (handle != null && disposing)
		{
			handle.Dispose();
		}
	}

	/// <summary>Sets the length of the current stream to the specified value.</summary>
	/// <param name="value">The new length of the stream.</param>
	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	/// <summary>Sets the current position of the current stream to the specified value.</summary>
	/// <returns>The new position in the stream.</returns>
	/// <param name="offset">The point, relative to <paramref name="origin" />, to begin seeking from.</param>
	/// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for <paramref name="offset" />, using a value of type <see cref="T:System.IO.SeekOrigin" />.</param>
	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	/// <summary>Gets a <see cref="T:System.IO.Pipes.PipeSecurity" /> object that encapsulates the access control list (ACL) entries for the pipe described by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</summary>
	/// <returns>A <see cref="T:System.IO.Pipes.PipeSecurity" /> object that encapsulates the access control list (ACL) entries for the pipe described by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying call to set security information failed.</exception>
	public PipeSecurity GetAccessControl()
	{
		return new PipeSecurity(SafePipeHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Applies the access control list (ACL) entries specified by a <see cref="T:System.IO.Pipes.PipeSecurity" /> object to the pipe specified by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</summary>
	/// <param name="pipeSecurity">A <see cref="T:System.IO.Pipes.PipeSecurity" /> object that specifies an access control list (ACL) entry to apply to the current pipe.</param>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeSecurity" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying call to set security information failed.</exception>
	public void SetAccessControl(PipeSecurity pipeSecurity)
	{
		if (pipeSecurity == null)
		{
			throw new ArgumentNullException("pipeSecurity");
		}
		pipeSecurity.Persist(SafePipeHandle);
	}

	/// <summary>Waits for the other end of the pipe to read all sent bytes.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public void WaitForPipeDrain()
	{
	}

	/// <summary>Reads a block of bytes from a stream and writes the data to a specified buffer.</summary>
	/// <returns>The total number of bytes that are read into <paramref name="buffer" />. This might be less than the number of bytes requested if that number of bytes is not currently available, or 0 if the end of the stream is reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
	/// <param name="offset">The byte offset in the <paramref name="buffer" /> array at which the bytes that are read will be placed.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support read operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">Any I/O error occurred.</exception>
	[MonoTODO]
	public override int Read([In] byte[] buffer, int offset, int count)
	{
		CheckReadOperations();
		return Stream.Read(buffer, offset, count);
	}

	/// <summary>Reads a byte from a pipe.</summary>
	/// <returns>The byte, cast to <see cref="T:System.Int32" />, or -1 indicates the end of the stream (the pipe has been closed).</returns>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support read operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">Any I/O error occurred.</exception>
	[MonoTODO]
	public override int ReadByte()
	{
		CheckReadOperations();
		return Stream.ReadByte();
	}

	/// <summary>Writes a block of bytes to the current stream using data from a buffer.</summary>
	/// <param name="buffer">The buffer that contains data to write to the pipe.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The maximum number of bytes to write to the current stream.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	[MonoTODO]
	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckWriteOperations();
		Stream.Write(buffer, offset, count);
	}

	/// <summary>Writes a byte to the current stream.</summary>
	/// <param name="value">The byte to write to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	[MonoTODO]
	public override void WriteByte(byte value)
	{
		CheckWriteOperations();
		Stream.WriteByte(value);
	}

	/// <summary>Clears the buffer for the current stream and causes any buffered data to be written to the underlying device.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	[MonoTODO]
	public override void Flush()
	{
		CheckWriteOperations();
		Stream.Flush();
	}

	/// <summary>Begins an asynchronous read operation.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous read.</returns>
	/// <param name="buffer">The buffer to read data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin reading.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="callback">The method to call when the asynchronous read operation is completed.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support read operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (read_delegate == null)
		{
			read_delegate = Read;
		}
		return read_delegate.BeginInvoke(buffer, offset, count, callback, state);
	}

	/// <summary>Begins an asynchronous write operation.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer that contains the data to write to the current stream.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="callback">The method to call when the asynchronous write operation is completed.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (write_delegate == null)
		{
			write_delegate = Write;
		}
		return write_delegate.BeginInvoke(buffer, offset, count, callback, state);
	}

	/// <summary>Ends a pending asynchronous read request.</summary>
	/// <returns>The number of bytes that were read. A return value of 0 indicates the end of the stream (the pipe has been closed).</returns>
	/// <param name="asyncResult">The reference to the pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> did not originate from a <see cref="M:System.IO.Pipes.PipeStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> method on the current stream. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	public override int EndRead(IAsyncResult asyncResult)
	{
		return read_delegate.EndInvoke(asyncResult);
	}

	/// <summary>Ends a pending asynchronous write request.</summary>
	/// <param name="asyncResult">The reference to the pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> did not originate from a <see cref="M:System.IO.Pipes.PipeStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> method on the current stream. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		write_delegate.EndInvoke(asyncResult);
	}
}
