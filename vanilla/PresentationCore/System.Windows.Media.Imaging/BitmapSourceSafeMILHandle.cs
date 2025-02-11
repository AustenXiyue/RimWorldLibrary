using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

internal class BitmapSourceSafeMILHandle : SafeMILHandle
{
	private static Guid _uuidBitmap;

	static BitmapSourceSafeMILHandle()
	{
		_uuidBitmap = MILGuidData.IID_IWICBitmapSource;
	}

	internal BitmapSourceSafeMILHandle()
	{
	}

	internal BitmapSourceSafeMILHandle(nint handle)
	{
		SetHandle(handle);
	}

	internal BitmapSourceSafeMILHandle(nint handle, SafeMILHandle copyMemoryPressureFrom)
		: this(handle)
	{
		CopyMemoryPressure(copyMemoryPressureFrom);
	}

	internal void CalculateSize()
	{
		UpdateEstimatedSize(ComputeEstimatedSize(handle));
	}

	private static long ComputeEstimatedSize(nint bitmapObject)
	{
		long result = 0L;
		if (bitmapObject != IntPtr.Zero && UnsafeNativeMethods.MILUnknown.QueryInterface(bitmapObject, ref _uuidBitmap, out var ppvObject) == 0)
		{
			SafeMILHandle tHIS_PTR = new SafeMILHandle(ppvObject);
			uint puiWidth = 0u;
			uint puiHeight = 0u;
			if (UnsafeNativeMethods.WICBitmapSource.GetSize(tHIS_PTR, out puiWidth, out puiHeight) == 0 && UnsafeNativeMethods.WICBitmapSource.GetPixelFormat(tHIS_PTR, out var pPixelFormatEnum) == 0)
			{
				PixelFormat pixelFormat = new PixelFormat(pPixelFormatEnum);
				long num = puiWidth * pixelFormat.InternalBitsPerPixel / 8;
				if (num < 1073741824)
				{
					result = puiHeight * num;
				}
			}
		}
		return result;
	}

	protected override bool ReleaseHandle()
	{
		return base.ReleaseHandle();
	}
}
