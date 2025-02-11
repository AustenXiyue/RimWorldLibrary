using System;
using System.IO;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles;

/// <summary>Represents a wrapper class for a pipe handle. </summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
public sealed class SafePipeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> class.</summary>
	/// <param name="preexistingHandle">An <see cref="T:System.IntPtr" /> object that represents the pre-existing handle to use.</param>
	/// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).</param>
	public SafePipeHandle(IntPtr preexistingHandle, bool ownsHandle)
		: base(ownsHandle)
	{
		handle = preexistingHandle;
	}

	protected override bool ReleaseHandle()
	{
		MonoIOError error;
		return MonoIO.Close(handle, out error);
	}
}
