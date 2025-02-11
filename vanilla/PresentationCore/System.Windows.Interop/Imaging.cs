using System.Windows.Media;
using System.Windows.Media.Imaging;
using MS.Internal;

namespace System.Windows.Interop;

/// <summary>Provides managed to unmanaged interoperation support for creating image objects.</summary>
public static class Imaging
{
	public static BitmapSource CreateBitmapSourceFromHBitmap(nint bitmap, nint palette, Int32Rect sourceRect, BitmapSizeOptions sizeOptions)
	{
		return CriticalCreateBitmapSourceFromHBitmap(bitmap, palette, sourceRect, sizeOptions, WICBitmapAlphaChannelOption.WICBitmapUseAlpha);
	}

	internal static BitmapSource CriticalCreateBitmapSourceFromHBitmap(nint bitmap, nint palette, Int32Rect sourceRect, BitmapSizeOptions sizeOptions, WICBitmapAlphaChannelOption alphaOptions)
	{
		if (bitmap == IntPtr.Zero)
		{
			throw new ArgumentNullException("bitmap");
		}
		return new InteropBitmap(bitmap, palette, sourceRect, sizeOptions, alphaOptions);
	}

	public static BitmapSource CreateBitmapSourceFromHIcon(nint icon, Int32Rect sourceRect, BitmapSizeOptions sizeOptions)
	{
		if (icon == IntPtr.Zero)
		{
			throw new ArgumentNullException("icon");
		}
		return new InteropBitmap(icon, sourceRect, sizeOptions);
	}

	public static BitmapSource CreateBitmapSourceFromMemorySection(nint section, int pixelWidth, int pixelHeight, PixelFormat format, int stride, int offset)
	{
		if (section == IntPtr.Zero)
		{
			throw new ArgumentNullException("section");
		}
		return new InteropBitmap(section, pixelWidth, pixelHeight, format, stride, offset);
	}
}
