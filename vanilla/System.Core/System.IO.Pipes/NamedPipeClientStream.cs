using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> around a named pipe, which supports both synchronous and asynchronous read and write operations.</summary>
[MonoTODO("working only on win32 right now")]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class NamedPipeClientStream : PipeStream
{
	private INamedPipeClient impl;

	/// <summary>Gets the number of server instances that share the same pipe name.</summary>
	/// <returns>The number of server instances that share the same pipe name.</returns>
	/// <exception cref="T:System.InvalidOperationException">The pipe handle has not been set.-or-The current <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> object has not yet connected to a <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or an I/O error occurred.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The underlying pipe handle is closed.</exception>
	public int NumberOfServerInstances
	{
		get
		{
			CheckPipePropertyOperations();
			return impl.NumberOfServerInstances;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe name.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".</exception>
	public NamedPipeClientStream(string pipeName)
		: this(".", pipeName)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".</exception>
	public NamedPipeClientStream(string serverName, string pipeName)
		: this(serverName, pipeName, PipeDirection.InOut)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction)
		: this(serverName, pipeName, direction, PipeOptions.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction and pipe options.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options)
		: this(serverName, pipeName, direction, options, TokenImpersonationLevel.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction, pipe options, and security impersonation level.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="impersonationLevel">One of the enumeration values that determines the security impersonation level.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="impersonationLevel" /> is not a valid <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel)
		: this(serverName, pipeName, direction, options, impersonationLevel, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction, pipe options, security impersonation level, and inheritability mode.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="impersonationLevel">One of the enumeration values that determines the security impersonation level.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle will be inheritable by child processes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="impersonationLevel" /> is not a valid <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
		: this(serverName, pipeName, PipeStream.ToAccessRights(direction), options, impersonationLevel, inheritability)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class for the specified pipe handle with the specified pipe direction.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="isAsync">true to indicate that the handle was opened asynchronously; otherwise, false.</param>
	/// <param name="isConnected">true to indicate that the pipe is connected; otherwise, false.</param>
	/// <param name="safePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> object will encapsulate.</param>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="safePipeHandle" /> is not a valid handle.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="safePipeHandle" /> is not a valid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="safePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	/// <exception cref="T:System.IO.IOException">The stream has been closed. </exception>
	public NamedPipeClientStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
		: base(direction, 1024)
	{
		if (PipeStream.IsWindows)
		{
			impl = new Win32NamedPipeClient(this, safePipeHandle);
		}
		else
		{
			impl = new UnixNamedPipeClient(this, safePipeHandle);
		}
		base.IsConnected = isConnected;
		InitializeHandle(safePipeHandle, isExposed: true, isAsync);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe options, security impersonation level, and inheritability mode.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="desiredAccessRights">One of the enumeration values that specifies the desired access rights of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="impersonationLevel">One of the enumeration values that determines the security impersonation level.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle will be inheritable by child processes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="impersonationLevel" /> is not a valid <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
		: base(PipeStream.ToDirection(desiredAccessRights), 1024)
	{
		if (impersonationLevel != 0 || inheritability != 0)
		{
			throw ThrowACLException();
		}
		if (PipeStream.IsWindows)
		{
			impl = new Win32NamedPipeClient(this, serverName, pipeName, desiredAccessRights, options, inheritability);
		}
		else
		{
			impl = new UnixNamedPipeClient(this, serverName, pipeName, desiredAccessRights, options, inheritability);
		}
	}

	~NamedPipeClientStream()
	{
		Dispose(disposing: false);
	}

	/// <summary>Connects to a waiting server with an infinite time-out value.</summary>
	/// <exception cref="T:System.InvalidOperationException">The client is already connected.</exception>
	public void Connect()
	{
		impl.Connect();
		InitializeHandle(impl.Handle, isExposed: false, impl.IsAsync);
		base.IsConnected = true;
	}

	/// <summary>Connects to a waiting server within the specified time-out period.</summary>
	/// <param name="timeout">The number of milliseconds to wait for the server to respond before the connection times out.</param>
	/// <exception cref="T:System.TimeoutException">Could not connect to the server within the specified <paramref name="timeout" /> period.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is less than 0 and not set to <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The client is already connected.</exception>
	/// <exception cref="T:System.IO.IOException">The server is connected to another client and the time-out period has expired.</exception>
	public void Connect(int timeout)
	{
		impl.Connect(timeout);
		InitializeHandle(impl.Handle, isExposed: false, impl.IsAsync);
		base.IsConnected = true;
	}

	public Task ConnectAsync()
	{
		return ConnectAsync(-1, CancellationToken.None);
	}

	public Task ConnectAsync(int timeout)
	{
		return ConnectAsync(timeout, CancellationToken.None);
	}

	public Task ConnectAsync(CancellationToken cancellationToken)
	{
		return ConnectAsync(-1, cancellationToken);
	}

	public Task ConnectAsync(int timeout, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	protected internal override void CheckPipePropertyOperations()
	{
		base.CheckPipePropertyOperations();
	}
}
