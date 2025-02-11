using Microsoft.Win32.SafeHandles;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class SafeMILHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private SafeMILHandleMemoryPressure _gcPressure;

	internal SafeMILHandle()
		: base(ownsHandle: true)
	{
	}

	internal SafeMILHandle(nint handle)
		: base(ownsHandle: true)
	{
		SetHandle(handle);
	}

	internal void UpdateEstimatedSize(long estimatedSize)
	{
		if (_gcPressure != null)
		{
			_gcPressure.Release();
		}
		if (estimatedSize > 0)
		{
			_gcPressure = new SafeMILHandleMemoryPressure(estimatedSize);
			_gcPressure.AddRef();
		}
	}

	internal void CopyMemoryPressure(SafeMILHandle original)
	{
		_gcPressure = original._gcPressure;
		if (_gcPressure != null)
		{
			_gcPressure.AddRef();
		}
	}

	protected override bool ReleaseHandle()
	{
		UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref handle);
		if (_gcPressure != null)
		{
			_gcPressure.Release();
			_gcPressure = null;
		}
		return true;
	}
}
