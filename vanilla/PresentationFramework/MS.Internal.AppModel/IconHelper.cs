using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MS.Internal.Interop;
using MS.Win32;

namespace MS.Internal.AppModel;

internal static class IconHelper
{
	private static Size s_smallIconSize;

	private static Size s_iconSize;

	private static int s_systemBitDepth;

	private static void EnsureSystemMetrics()
	{
		if (s_systemBitDepth != 0)
		{
			return;
		}
		HandleRef hDC = new HandleRef(null, MS.Win32.UnsafeNativeMethods.GetDC(default(HandleRef)));
		try
		{
			int deviceCaps = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(hDC, 12);
			deviceCaps *= MS.Win32.UnsafeNativeMethods.GetDeviceCaps(hDC, 14);
			if (deviceCaps == 8)
			{
				deviceCaps = 4;
			}
			int systemMetrics = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CXSMICON);
			int systemMetrics2 = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CYSMICON);
			int systemMetrics3 = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CXICON);
			int systemMetrics4 = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CYICON);
			s_smallIconSize = new Size(systemMetrics, systemMetrics2);
			s_iconSize = new Size(systemMetrics3, systemMetrics4);
			s_systemBitDepth = deviceCaps;
		}
		finally
		{
			MS.Win32.UnsafeNativeMethods.ReleaseDC(default(HandleRef), hDC);
		}
	}

	public static void GetDefaultIconHandles(out MS.Win32.NativeMethods.IconHandle largeIconHandle, out MS.Win32.NativeMethods.IconHandle smallIconHandle)
	{
		largeIconHandle = null;
		smallIconHandle = null;
		MS.Win32.UnsafeNativeMethods.ExtractIconEx(MS.Win32.UnsafeNativeMethods.GetModuleFileName(default(HandleRef)), 0, out largeIconHandle, out smallIconHandle, 1);
	}

	public static void GetIconHandlesFromImageSource(ImageSource image, out MS.Win32.NativeMethods.IconHandle largeIconHandle, out MS.Win32.NativeMethods.IconHandle smallIconHandle)
	{
		EnsureSystemMetrics();
		largeIconHandle = CreateIconHandleFromImageSource(image, s_iconSize);
		smallIconHandle = CreateIconHandleFromImageSource(image, s_smallIconSize);
	}

	public static MS.Win32.NativeMethods.IconHandle CreateIconHandleFromImageSource(ImageSource image, Size size)
	{
		EnsureSystemMetrics();
		bool flag = false;
		BitmapFrame bitmapFrame = image as BitmapFrame;
		if (bitmapFrame?.Decoder?.Frames != null)
		{
			bitmapFrame = GetBestMatch(bitmapFrame.Decoder.Frames, size);
			flag = bitmapFrame.Decoder is IconBitmapDecoder || ((double)bitmapFrame.PixelWidth == size.Width && (double)bitmapFrame.PixelHeight == size.Height);
			image = bitmapFrame;
		}
		if (!flag)
		{
			bitmapFrame = BitmapFrame.Create(GenerateBitmapSource(image, size));
		}
		return CreateIconHandleFromBitmapFrame(bitmapFrame);
	}

	private static BitmapSource GenerateBitmapSource(ImageSource img, Size renderSize)
	{
		Rect rectangle = new Rect(0.0, 0.0, renderSize.Width, renderSize.Height);
		double num = renderSize.Width / renderSize.Height;
		double num2 = img.Width / img.Height;
		if (img.Width <= renderSize.Width && img.Height <= renderSize.Height)
		{
			rectangle = new Rect((renderSize.Width - img.Width) / 2.0, (renderSize.Height - img.Height) / 2.0, img.Width, img.Height);
		}
		else if (num > num2)
		{
			double num3 = img.Width / img.Height * renderSize.Width;
			rectangle = new Rect((renderSize.Width - num3) / 2.0, 0.0, num3, renderSize.Height);
		}
		else if (num < num2)
		{
			double num4 = img.Height / img.Width * renderSize.Height;
			rectangle = new Rect(0.0, (renderSize.Height - num4) / 2.0, renderSize.Width, num4);
		}
		DrawingVisual drawingVisual = new DrawingVisual();
		DrawingContext drawingContext = drawingVisual.RenderOpen();
		drawingContext.DrawImage(img, rectangle);
		drawingContext.Close();
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)renderSize.Width, (int)renderSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(drawingVisual);
		return renderTargetBitmap;
	}

	private static MS.Win32.NativeMethods.IconHandle CreateIconHandleFromBitmapFrame(BitmapFrame sourceBitmapFrame)
	{
		Invariant.Assert(sourceBitmapFrame != null, "sourceBitmapFrame cannot be null here");
		BitmapSource bitmapSource = sourceBitmapFrame;
		if (bitmapSource.Format != PixelFormats.Bgra32 && bitmapSource.Format != PixelFormats.Pbgra32)
		{
			bitmapSource = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0.0);
		}
		int pixelWidth = bitmapSource.PixelWidth;
		int pixelHeight = bitmapSource.PixelHeight;
		int num = (bitmapSource.Format.BitsPerPixel * pixelWidth + 31) / 32 * 4;
		byte[] array = new byte[num * pixelHeight];
		bitmapSource.CopyPixels(array, num, 0);
		return CreateIconCursor(array, pixelWidth, pixelHeight, 0, 0, isIcon: true);
	}

	internal static MS.Win32.NativeMethods.IconHandle CreateIconCursor(byte[] colorArray, int width, int height, int xHotspot, int yHotspot, bool isIcon)
	{
		MS.Win32.NativeMethods.BitmapHandle bitmapHandle = null;
		MS.Win32.NativeMethods.BitmapHandle bitmapHandle2 = null;
		try
		{
			MS.Win32.NativeMethods.BITMAPINFO bitmapInfo = new MS.Win32.NativeMethods.BITMAPINFO(width, -height, 32);
			bitmapInfo.bmiHeader_biCompression = 0;
			nint ppvBits = IntPtr.Zero;
			bitmapHandle = MS.Win32.UnsafeNativeMethods.CreateDIBSection(new HandleRef(null, IntPtr.Zero), ref bitmapInfo, 0, ref ppvBits, null, 0);
			if (bitmapHandle.IsInvalid || ppvBits == IntPtr.Zero)
			{
				return MS.Win32.NativeMethods.IconHandle.GetInvalidIcon();
			}
			Marshal.Copy(colorArray, 0, ppvBits, colorArray.Length);
			byte[] array = GenerateMaskArray(width, height, colorArray);
			Invariant.Assert(array != null);
			bitmapHandle2 = MS.Win32.UnsafeNativeMethods.CreateBitmap(width, height, 1, 1, array);
			if (bitmapHandle2.IsInvalid)
			{
				return MS.Win32.NativeMethods.IconHandle.GetInvalidIcon();
			}
			return MS.Win32.UnsafeNativeMethods.CreateIconIndirect(new MS.Win32.NativeMethods.ICONINFO
			{
				fIcon = isIcon,
				xHotspot = xHotspot,
				yHotspot = yHotspot,
				hbmMask = bitmapHandle2,
				hbmColor = bitmapHandle
			});
		}
		finally
		{
			if (bitmapHandle != null)
			{
				bitmapHandle.Dispose();
				bitmapHandle = null;
			}
			if (bitmapHandle2 != null)
			{
				bitmapHandle2.Dispose();
				bitmapHandle2 = null;
			}
		}
	}

	private static byte[] GenerateMaskArray(int width, int height, byte[] colorArray)
	{
		int num = width * height;
		int num2 = AlignToBytes(width, 2) / 8;
		byte[] array = new byte[num2 * height];
		for (int i = 0; i < num; i++)
		{
			int num3 = i % width;
			int num4 = i / width;
			int num5 = num3 / 8;
			byte b = (byte)(128 >> num3 % 8);
			if (colorArray[i * 4 + 3] == 0)
			{
				array[num5 + num2 * num4] |= b;
			}
			else
			{
				array[num5 + num2 * num4] &= (byte)(~b);
			}
			if (num3 == width - 1 && width == 8)
			{
				array[1 + num2 * num4] = byte.MaxValue;
			}
		}
		return array;
	}

	internal static int AlignToBytes(double original, int nBytesCount)
	{
		int num = 8 << nBytesCount - 1;
		return ((int)Math.Ceiling(original) + (num - 1)) / num * num;
	}

	private static int MatchImage(BitmapFrame frame, Size size, int bpp)
	{
		return 2 * MyAbs(bpp, s_systemBitDepth, fPunish: false) + MyAbs(frame.PixelWidth, (int)size.Width, fPunish: true) + MyAbs(frame.PixelHeight, (int)size.Height, fPunish: true);
	}

	private static int MyAbs(int valueHave, int valueWant, bool fPunish)
	{
		int num = valueHave - valueWant;
		if (num < 0)
		{
			num = (fPunish ? (-2) : (-1)) * num;
		}
		return num;
	}

	private static BitmapFrame GetBestMatch(ReadOnlyCollection<BitmapFrame> frames, Size size)
	{
		Invariant.Assert(size.Width != 0.0, "input param width should not be zero");
		Invariant.Assert(size.Height != 0.0, "input param height should not be zero");
		int num = int.MaxValue;
		int num2 = 0;
		int index = 0;
		bool flag = frames[0].Decoder is IconBitmapDecoder;
		for (int i = 0; i < frames.Count; i++)
		{
			if (num == 0)
			{
				break;
			}
			int num3 = (flag ? frames[i].Thumbnail.Format.BitsPerPixel : frames[i].Format.BitsPerPixel);
			if (num3 == 0)
			{
				num3 = 8;
			}
			int num4 = MatchImage(frames[i], size, num3);
			if (num4 < num)
			{
				index = i;
				num2 = num3;
				num = num4;
			}
			else if (num4 == num && num2 < num3)
			{
				index = i;
				num2 = num3;
			}
		}
		return frames[index];
	}
}
