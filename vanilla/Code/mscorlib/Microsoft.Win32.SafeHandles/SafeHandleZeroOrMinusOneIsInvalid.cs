using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles;

/// <summary>Provides a base class for Win32 safe handle implementations in which the value of either 0 or -1 indicates an invalid handle.</summary>
[SecurityCritical]
[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
public abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
{
	/// <summary>Gets a value that indicates whether the handle is invalid.</summary>
	/// <returns>true if the handle is not valid; otherwise, false.</returns>
	public override bool IsInvalid
	{
		[SecurityCritical]
		get
		{
			if (!handle.IsNull())
			{
				return handle == new IntPtr(-1);
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid" /> class, specifying whether the handle is to be reliably released. </summary>
	/// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle)
		: base(IntPtr.Zero, ownsHandle)
	{
	}
}
