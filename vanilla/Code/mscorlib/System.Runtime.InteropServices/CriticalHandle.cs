using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Permissions;

namespace System.Runtime.InteropServices;

/// <summary>Represents a wrapper class for handle resources.</summary>
[SecurityCritical]
[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
public abstract class CriticalHandle : CriticalFinalizerObject, IDisposable
{
	/// <summary>Specifies the handle to be wrapped.</summary>
	protected IntPtr handle;

	private bool _isClosed;

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
			return _isClosed;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the handle value is invalid.</summary>
	/// <returns>true if the handle is valid; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public abstract bool IsInvalid
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.CriticalHandle" /> class with the specified invalid handle value.</summary>
	/// <param name="invalidHandleValue">The value of an invalid handle (usually 0 or -1).</param>
	/// <exception cref="T:System.TypeLoadException">The derived class resides in an assembly without unmanaged code access permission.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	protected CriticalHandle(IntPtr invalidHandleValue)
	{
		handle = invalidHandleValue;
		_isClosed = false;
	}

	/// <summary>Frees all resources associated with the handle.</summary>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	~CriticalHandle()
	{
		Dispose(disposing: false);
	}

	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	private void Cleanup()
	{
		if (IsClosed)
		{
			return;
		}
		_isClosed = true;
		if (!IsInvalid)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (!ReleaseHandle())
			{
				FireCustomerDebugProbe();
			}
			Marshal.SetLastWin32Error(lastWin32Error);
			GC.SuppressFinalize(this);
		}
	}

	private static void FireCustomerDebugProbe()
	{
	}

	/// <summary>Sets the handle to the specified pre-existing handle.</summary>
	/// <param name="handle">The pre-existing handle to use.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	protected void SetHandle(IntPtr handle)
	{
		this.handle = handle;
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

	/// <summary>Releases all resources used by the <see cref="T:System.Runtime.InteropServices.CriticalHandle" />. </summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Runtime.InteropServices.CriticalHandle" /> class specifying whether to perform a normal dispose operation.</summary>
	/// <param name="disposing">true for a normal dispose operation; false to finalize the handle.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecurityCritical]
	protected virtual void Dispose(bool disposing)
	{
		Cleanup();
	}

	/// <summary>Marks a handle as invalid.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void SetHandleAsInvalid()
	{
		_isClosed = true;
		GC.SuppressFinalize(this);
	}

	/// <summary>When overridden in a derived class, executes the code required to free the handle.</summary>
	/// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.</returns>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	protected abstract bool ReleaseHandle();
}
