using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> around a named pipe, supporting both synchronous and asynchronous read and write operations.</summary>
[MonoTODO("working only on win32 right now")]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class NamedPipeServerStream : PipeStream
{
	/// <summary>Represents the maximum number of server instances that the system resources allow.</summary>
	public const int MaxAllowedServerInstances = -1;

	private INamedPipeServer impl;

	private Action wait_connect_delegate;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported. </exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName)
		: this(pipeName, PipeDirection.InOut)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name and pipe direction.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction)
		: this(pipeName, direction, 1)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, and maximum number of server instances.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-A non-negative number is required. -or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<see cref="F:System.IO.HandleInheritability.None" />  or <see cref="F:System.IO.HandleInheritability.Inheritable" />  is required.-or-Access rights is limited to the <see cref="F:System.IO.Pipes.PipeAccessRights.ChangePermissions" /> , <see cref="F:System.IO.Pipes.PipeAccessRights.TakeOwnership" /> , and <see cref="F:System.IO.Pipes.PipeAccessRights.AccessSystemSecurity" />  flags.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances)
		: this(pipeName, direction, maxNumberOfServerInstances, PipeTransmissionMode.Byte)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, and transmission mode.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, PipeOptions.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, and pipe options.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, 1024, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, and recommended in and out buffer sizes.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">A positive value greater than 0 that indicates the input buffer size.</param>
	/// <param name="outBufferSize">A positive value greater than 0 that indicates the output buffer size.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, recommended in and out buffer sizes, and pipe security.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">A positive value greater than 0 that indicates the input buffer size.</param>
	/// <param name="outBufferSize">A positive value greater than 0 that indicates the output buffer size.</param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, recommended in and out buffer sizes, pipe security, and inheritability mode.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">A positive value greater than 0 that indicates the input buffer size.</param>
	/// <param name="outBufferSize">A positive value greater than 0 that indicates the output buffer size.</param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, inheritability, PipeAccessRights.ReadData | PipeAccessRights.WriteData)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, recommended in and out buffer sizes, pipe security, inheritability mode, and pipe access rights.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">The input buffer size.</param>
	/// <param name="outBufferSize">The output buffer size.</param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes.</param>
	/// <param name="additionalAccessRights">One of the enumeration values that specifies the access rights of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.-or-<paramref name="additionalAccessRights" /> is not a valid <see cref="T:System.IO.Pipes.PipeAccessRights" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	[MonoTODO]
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
		: base(direction, transmissionMode, outBufferSize)
	{
		PipeAccessRights rights = PipeStream.ToAccessRights(direction) | additionalAccessRights;
		if (PipeStream.IsWindows)
		{
			impl = new Win32NamedPipeServer(this, pipeName, maxNumberOfServerInstances, transmissionMode, rights, options, inBufferSize, outBufferSize, pipeSecurity, inheritability);
		}
		else
		{
			impl = new UnixNamedPipeServer(this, pipeName, maxNumberOfServerInstances, transmissionMode, rights, options, inBufferSize, outBufferSize, inheritability);
		}
		InitializeHandle(impl.Handle, isExposed: false, (options & PipeOptions.Asynchronous) != 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class from the specified pipe handle.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="isAsync">true to indicate that the handle was opened asynchronously; otherwise, false.</param>
	/// <param name="isConnected">true to indicate that the pipe is connected; otherwise, false.</param>
	/// <param name="safePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object will encapsulate.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="safePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="safePipeHandle" /> is an invalid handle.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="safePipeHandle" /> is not a valid pipe handle.-or-The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
		: base(direction, 1024)
	{
		if (PipeStream.IsWindows)
		{
			impl = new Win32NamedPipeServer(this, safePipeHandle);
		}
		else
		{
			impl = new UnixNamedPipeServer(this, safePipeHandle);
		}
		base.IsConnected = isConnected;
		InitializeHandle(safePipeHandle, isExposed: true, isAsync);
	}

	~NamedPipeServerStream()
	{
	}

	/// <summary>Disconnects the current connection.</summary>
	/// <exception cref="T:System.InvalidOperationException">No pipe connections have been made yet.-or-The connected pipe has already disconnected.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public void Disconnect()
	{
		impl.Disconnect();
	}

	/// <summary>Calls a delegate while impersonating the client.</summary>
	/// <param name="impersonationWorker">The delegate that specifies a method to call.</param>
	/// <exception cref="T:System.InvalidOperationException">No pipe connections have been made yet.-or-The connected pipe has already disconnected.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.-or-An I/O error occurred.</exception>
	[MonoTODO]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
	public void RunAsClient(PipeStreamImpersonationWorker impersonationWorker)
	{
		throw new NotImplementedException();
	}

	/// <summary>Waits for a client to connect to this <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object.</summary>
	/// <exception cref="T:System.InvalidOperationException">A pipe connection has already been established.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.</exception>
	public void WaitForConnection()
	{
		impl.WaitForConnection();
		base.IsConnected = true;
	}

	public Task WaitForConnectionAsync()
	{
		return WaitForConnectionAsync(CancellationToken.None);
	}

	[MonoTODO]
	public Task WaitForConnectionAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the user name of the client on the other end of the pipe.</summary>
	/// <returns>The user name of the client on the other end of the pipe.</returns>
	/// <exception cref="T:System.InvalidOperationException">No pipe connections have been made yet.-or-The connected pipe has already disconnected.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.-or-The user name of the client is longer than 19 characters.</exception>
	[MonoTODO]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
	public string GetImpersonationUserName()
	{
		throw new NotImplementedException();
	}

	/// <summary>Begins an asynchronous operation to wait for a client to connect.</summary>
	/// <returns>An object that references the asynchronous request.</returns>
	/// <param name="callback">The method to call when a client connects to the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous request from other requests.</param>
	/// <exception cref="T:System.InvalidOperationException">The pipe was not opened asynchronously.-or-A pipe connection has already been established.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginWaitForConnection(AsyncCallback callback, object state)
	{
		if (wait_connect_delegate == null)
		{
			wait_connect_delegate = WaitForConnection;
		}
		return wait_connect_delegate.BeginInvoke(callback, state);
	}

	/// <summary>Ends an asynchronous operation to wait for a client to connect.</summary>
	/// <param name="asyncResult">The pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe was not opened asynchronously.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public void EndWaitForConnection(IAsyncResult asyncResult)
	{
		wait_connect_delegate.EndInvoke(asyncResult);
	}
}
