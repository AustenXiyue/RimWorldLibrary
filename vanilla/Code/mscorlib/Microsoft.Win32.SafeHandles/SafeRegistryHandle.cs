using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.SafeHandles;

/// <summary>Represents a safe handle to the Windows registry.</summary>
[SecurityCritical]
public sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	[SecurityCritical]
	internal SafeRegistryHandle()
		: base(ownsHandle: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeRegistryHandle" /> class. </summary>
	/// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
	/// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release.</param>
	[SecurityCritical]
	public SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle)
		: base(ownsHandle)
	{
		SetHandle(preexistingHandle);
	}

	[SecurityCritical]
	protected override bool ReleaseHandle()
	{
		return RegCloseKey(handle) == 0;
	}

	[DllImport("advapi32.dll")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SuppressUnmanagedCodeSecurity]
	internal static extern int RegCloseKey(IntPtr hKey);
}
