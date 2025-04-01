using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.SafeHandles;

/// <summary>Provides a safe handle that represents a key (NCRYPT_KEY_HANDLE).</summary>
public sealed class SafeNCryptKeyHandle : SafeNCryptHandle
{
	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeNCryptKeyHandle" /> class.</summary>
	public SafeNCryptKeyHandle()
	{
	}

	public SafeNCryptKeyHandle(IntPtr handle, SafeHandle parentHandle)
		: base(handle, parentHandle)
	{
	}

	protected override bool ReleaseNativeHandle()
	{
		return false;
	}
}
