using System.Globalization;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes the client side of an anonymous pipe stream, which supports both synchronous and asynchronous read and write operations.</summary>
[MonoTODO("Anonymous pipes are not working even on win32, due to some access authorization issue")]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class AnonymousPipeClientStream : PipeStream
{
	/// <summary>Sets the reading mode for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object.</summary>
	/// <returns>The <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The transmission mode is not valid. For anonymous pipes, only <see cref="F:System.IO.Pipes.PipeTransmissionMode.Byte" /> is supported.</exception>
	/// <exception cref="T:System.NotSupportedException">The transmission mode is <see cref="F:System.IO.Pipes.PipeTransmissionMode.Message" />.</exception>
	/// <exception cref="T:System.IO.IOException">The connection is broken or another I/O error occurs.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public override PipeTransmissionMode ReadMode
	{
		set
		{
			if (value == PipeTransmissionMode.Message)
			{
				throw new NotSupportedException();
			}
		}
	}

	/// <summary>Gets the pipe transmission mode supported by the current pipe.</summary>
	/// <returns>The <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> supported by the current pipe.</returns>
	public override PipeTransmissionMode TransmissionMode => PipeTransmissionMode.Byte;

	private static SafePipeHandle ToSafePipeHandle(string pipeHandleAsString)
	{
		if (pipeHandleAsString == null)
		{
			throw new ArgumentNullException("pipeHandleAsString");
		}
		return new SafePipeHandle(new IntPtr(long.Parse(pipeHandleAsString, NumberFormatInfo.InvariantInfo)), ownsHandle: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> class with the specified string representation of the pipe handle.</summary>
	/// <param name="pipeHandleAsString">A string that represents the pipe handle.</param>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="pipeHandleAsString" /> is not a valid pipe handle.</exception>
	public AnonymousPipeClientStream(string pipeHandleAsString)
		: this(PipeDirection.In, pipeHandleAsString)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> class with the specified pipe direction and a string representation of the pipe handle.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="pipeHandleAsString">A string that represents the pipe handle.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeHandleAsString" /> is an invalid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeHandleAsString" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	public AnonymousPipeClientStream(PipeDirection direction, string pipeHandleAsString)
		: this(direction, ToSafePipeHandle(pipeHandleAsString))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> class from the specified handle.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="safePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object will encapsulate.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="safePipeHandle " />is not a valid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="safePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, has occurred.-or-The stream has been closed.</exception>
	public AnonymousPipeClientStream(PipeDirection direction, SafePipeHandle safePipeHandle)
		: base(direction, 1024)
	{
		InitializeHandle(safePipeHandle, isExposed: false, isAsync: false);
		base.IsConnected = true;
	}

	~AnonymousPipeClientStream()
	{
	}
}
