using Microsoft.Win32.SafeHandles;
using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class SafeReversePInvokeWrapper : SafeHandleZeroOrMinusOneIsInvalid
{
	internal SafeReversePInvokeWrapper()
		: base(ownsHandle: true)
	{
	}

	internal SafeReversePInvokeWrapper(nint delegatePtr)
		: base(ownsHandle: true)
	{
		HRESULT.Check(UnsafeNativeMethods.MilCoreApi.MilCreateReversePInvokeWrapper(delegatePtr, out var reversePInvokeWrapper));
		SetHandle(reversePInvokeWrapper);
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero)
		{
			UnsafeNativeMethods.MilCoreApi.MilReleasePInvokePtrBlocking(handle);
		}
		UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref handle);
		return true;
	}
}
