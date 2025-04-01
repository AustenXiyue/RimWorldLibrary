using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.InteropServices;

/// <summary>Represents a wrapper class for operating system handles. This class must be inherited.</summary>
[StructLayout(LayoutKind.Sequential)]
[SecurityCritical]
[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
public abstract class SafeHandle : CriticalFinalizerObject, IDisposable
{
	private enum State
	{
		Closed = 1,
		Disposed
	}

	/// <summary>Specifies the handle to be wrapped.</summary>
	protected IntPtr handle;

	private int _state;

	private bool _ownsHandle;

	private bool _fullyInitialized;

	private const int RefCount_Mask = 2147483644;

	private const int RefCount_One = 4;

	/// <summary>Gets a value indicating whether the handle is closed.</summary>
	/// <returns>true if the handle is closed; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public bool IsClosed
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return (_state & 1) == 1;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the handle value is invalid.</summary>
	/// <returns>true if the handle value is invalid; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public abstract bool IsInvalid
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.SafeHandle" /> class with the specified invalid handle value.</summary>
	/// <param name="invalidHandleValue">The value of an invalid handle (usually 0 or -1).  Your implementation of <see cref="P:System.Runtime.InteropServices.SafeHandle.IsInvalid" /> should return true for this value.</param>
	/// <param name="ownsHandle">true to reliably let <see cref="T:System.Runtime.InteropServices.SafeHandle" /> release the handle during the finalization phase; otherwise, false (not recommended). </param>
	/// <exception cref="T:System.TypeLoadException">The derived class resides in an assembly without unmanaged code access permission. </exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	protected SafeHandle(IntPtr invalidHandleValue, bool ownsHandle)
	{
		handle = invalidHandleValue;
		_state = 4;
		_ownsHandle = ownsHandle;
		if (!ownsHandle)
		{
			GC.SuppressFinalize(this);
		}
		_fullyInitialized = true;
	}

	/// <summary>Frees all resources associated with the handle.</summary>
	[SecuritySafeCritical]
	~SafeHandle()
	{
		Dispose(disposing: false);
	}

	/// <summary>Sets the handle to the specified pre-existing handle.</summary>
	/// <param name="handle">The pre-existing handle to use. </param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	protected void SetHandle(IntPtr handle)
	{
		this.handle = handle;
	}

	/// <summary>Returns the value of the <see cref="F:System.Runtime.InteropServices.SafeHandle.handle" /> field.</summary>
	/// <returns>An IntPtr representing the value of the <see cref="F:System.Runtime.InteropServices.SafeHandle.handle" /> field. If the handle has been marked invalid with <see cref="M:System.Runtime.InteropServices.SafeHandle.SetHandleAsInvalid" />, this method still returns the original handle value, which can be a stale value.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public IntPtr DangerousGetHandle()
	{
		return handle;
	}

	/// <summary>Marks the handle for releasing and freeing resources.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Runtime.InteropServices.SafeHandle" /> class.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Runtime.InteropServices.SafeHandle" /> class specifying whether to perform a normal dispose operation.</summary>
	/// <param name="disposing">true for a normal dispose operation; false to finalize the handle.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecurityCritical]
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			InternalDispose();
		}
		else
		{
			InternalFinalize();
		}
	}

	/// <summary>When overridden in a derived class, executes the code required to free the handle.</summary>
	/// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.</returns>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	protected abstract bool ReleaseHandle();

	/// <summary>Marks a handle as no longer used.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void SetHandleAsInvalid()
	{
		try
		{
		}
		finally
		{
			int state;
			int value;
			do
			{
				state = _state;
				value = state | 1;
			}
			while (Interlocked.CompareExchange(ref _state, value, state) != state);
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>Manually increments the reference counter on <see cref="T:System.Runtime.InteropServices.SafeHandle" /> instances.</summary>
	/// <param name="success">true if the reference counter was successfully incremented; otherwise, false.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public void DangerousAddRef(ref bool success)
	{
		try
		{
		}
		finally
		{
			if (!_fullyInitialized)
			{
				throw new InvalidOperationException();
			}
			int state;
			int value;
			do
			{
				state = _state;
				if ((state & 1) != 0)
				{
					throw new ObjectDisposedException(null, "Safe handle has been closed");
				}
				value = state + 4;
			}
			while (Interlocked.CompareExchange(ref _state, value, state) != state);
			success = true;
		}
	}

	/// <summary>Manually decrements the reference counter on a <see cref="T:System.Runtime.InteropServices.SafeHandle" /> instance.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void DangerousRelease()
	{
		DangerousReleaseInternal(dispose: false);
	}

	private void InternalDispose()
	{
		if (!_fullyInitialized)
		{
			throw new InvalidOperationException();
		}
		DangerousReleaseInternal(dispose: true);
		GC.SuppressFinalize(this);
	}

	private void InternalFinalize()
	{
		if (_fullyInitialized)
		{
			DangerousReleaseInternal(dispose: true);
		}
	}

	private void DangerousReleaseInternal(bool dispose)
	{
		try
		{
		}
		finally
		{
			if (!_fullyInitialized)
			{
				throw new InvalidOperationException();
			}
			bool flag = false;
			int state;
			int num;
			do
			{
				state = _state;
				if (dispose && (state & 2) != 0)
				{
					flag = false;
					break;
				}
				if ((state & 0x7FFFFFFC) == 0)
				{
					throw new ObjectDisposedException(null, "Safe handle has been closed");
				}
				flag = (state & 0x7FFFFFFC) == 4 && (state & 1) == 0 && _ownsHandle && !IsInvalid;
				num = (state & 0x7FFFFFFC) - 4;
				if ((state & 0x7FFFFFFC) == 4)
				{
					num |= 1;
				}
				if (dispose)
				{
					num |= 2;
				}
			}
			while (Interlocked.CompareExchange(ref _state, num, state) != state);
			if (flag)
			{
				ReleaseHandle();
			}
		}
	}
}
