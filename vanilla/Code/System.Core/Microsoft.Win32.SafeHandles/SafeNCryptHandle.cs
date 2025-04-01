using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.SafeHandles;

/// <summary>Provides a safe handle that can be used by Cryptography Next Generation (CNG) objects.</summary>
public abstract class SafeNCryptHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	public override bool IsInvalid
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeNCryptHandle" /> class.</summary>
	protected SafeNCryptHandle()
		: base(ownsHandle: true)
	{
	}

	protected SafeNCryptHandle(IntPtr handle, SafeHandle parentHandle)
		: base(ownsHandle: false)
	{
		throw new NotImplementedException();
	}

	/// <summary>Releases a handle used by a Cryptography Next Generation (CNG) object.</summary>
	/// <returns>true if the handle is released successfully; otherwise, false.</returns>
	protected override bool ReleaseHandle()
	{
		return false;
	}

	/// <summary>Releases a native handle used by a Cryptography Next Generation (CNG) object.</summary>
	/// <returns>true if the handle is released successfully; otherwise, false.</returns>
	protected abstract bool ReleaseNativeHandle();
}
