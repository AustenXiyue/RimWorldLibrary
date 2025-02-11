using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Standard;

internal static class Utility
{
	private static readonly Version _osVersion = Environment.OSVersion.Version;

	private static readonly Version _presentationFrameworkVersion = Assembly.GetAssembly(typeof(Window)).GetName().Version;

	private static int s_bitDepth;

	public static bool IsOSVistaOrNewer => _osVersion >= new Version(6, 0);

	public static bool IsOSWindows7OrNewer => _osVersion >= new Version(6, 1);

	public static bool IsPresentationFrameworkVersionLessThan4 => _presentationFrameworkVersion < new Version(4, 0);

	public static Color ColorFromArgbDword(uint color)
	{
		return Color.FromArgb((byte)((color & 0xFF000000u) >> 24), (byte)((color & 0xFF0000) >> 16), (byte)((color & 0xFF00) >> 8), (byte)(color & 0xFF));
	}

	public static int GET_X_LPARAM(nint lParam)
	{
		return LOWORD((int)((IntPtr)lParam).ToInt64());
	}

	public static int GET_Y_LPARAM(nint lParam)
	{
		return HIWORD((int)((IntPtr)lParam).ToInt64());
	}

	public static int HIWORD(int i)
	{
		return (short)(i >> 16);
	}

	public static int LOWORD(int i)
	{
		return (short)(i & 0xFFFF);
	}

	public static bool IsFlagSet(int value, int mask)
	{
		return (value & mask) != 0;
	}

	public static bool IsFlagSet(uint value, uint mask)
	{
		return (value & mask) != 0;
	}

	public static bool IsFlagSet(long value, long mask)
	{
		return (value & mask) != 0;
	}

	public static bool IsFlagSet(ulong value, ulong mask)
	{
		return (value & mask) != 0;
	}

	public static BitmapFrame GetBestMatch(IList<BitmapFrame> frames, int width, int height)
	{
		return _GetBestMatch(frames, _GetBitDepth(), width, height);
	}

	private static int _MatchImage(BitmapFrame frame, int bitDepth, int width, int height, int bpp)
	{
		return 2 * _WeightedAbs(bpp, bitDepth, fPunish: false) + _WeightedAbs(frame.PixelWidth, width, fPunish: true) + _WeightedAbs(frame.PixelHeight, height, fPunish: true);
	}

	private static int _WeightedAbs(int valueHave, int valueWant, bool fPunish)
	{
		int num = valueHave - valueWant;
		if (num < 0)
		{
			num = (fPunish ? (-2) : (-1)) * num;
		}
		return num;
	}

	private static BitmapFrame _GetBestMatch(IList<BitmapFrame> frames, int bitDepth, int width, int height)
	{
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
			int num4 = _MatchImage(frames[i], bitDepth, width, height, num3);
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

	private static int _GetBitDepth()
	{
		if (s_bitDepth == 0)
		{
			using SafeDC hdc = SafeDC.GetDesktop();
			s_bitDepth = NativeMethods.GetDeviceCaps(hdc, DeviceCap.BITSPIXEL) * NativeMethods.GetDeviceCaps(hdc, DeviceCap.PLANES);
		}
		return s_bitDepth;
	}

	public static void SafeDeleteObject(ref nint gdiObject)
	{
		nint num = gdiObject;
		gdiObject = IntPtr.Zero;
		if (IntPtr.Zero != num)
		{
			NativeMethods.DeleteObject(num);
		}
	}

	public static void SafeDestroyWindow(ref nint hwnd)
	{
		nint hwnd2 = hwnd;
		hwnd = IntPtr.Zero;
		if (NativeMethods.IsWindow(hwnd2))
		{
			NativeMethods.DestroyWindow(hwnd2);
		}
	}

	public static void SafeRelease<T>(ref T comObject) where T : class
	{
		T val = comObject;
		comObject = null;
		if (val != null)
		{
			Marshal.ReleaseComObject(val);
		}
	}

	public static void AddDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
	{
		if (component != null)
		{
			DependencyPropertyDescriptor.FromProperty(property, component.GetType()).AddValueChanged(component, listener);
		}
	}

	public static void RemoveDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
	{
		if (component != null)
		{
			DependencyPropertyDescriptor.FromProperty(property, component.GetType()).RemoveValueChanged(component, listener);
		}
	}

	public static bool IsThicknessNonNegative(Thickness thickness)
	{
		if (!IsDoubleFiniteAndNonNegative(thickness.Top))
		{
			return false;
		}
		if (!IsDoubleFiniteAndNonNegative(thickness.Left))
		{
			return false;
		}
		if (!IsDoubleFiniteAndNonNegative(thickness.Bottom))
		{
			return false;
		}
		if (!IsDoubleFiniteAndNonNegative(thickness.Right))
		{
			return false;
		}
		return true;
	}

	public static bool IsCornerRadiusValid(CornerRadius cornerRadius)
	{
		if (!IsDoubleFiniteAndNonNegative(cornerRadius.TopLeft))
		{
			return false;
		}
		if (!IsDoubleFiniteAndNonNegative(cornerRadius.TopRight))
		{
			return false;
		}
		if (!IsDoubleFiniteAndNonNegative(cornerRadius.BottomLeft))
		{
			return false;
		}
		if (!IsDoubleFiniteAndNonNegative(cornerRadius.BottomRight))
		{
			return false;
		}
		return true;
	}

	public static bool IsDoubleFiniteAndNonNegative(double d)
	{
		if (double.IsNaN(d) || double.IsInfinity(d) || d < 0.0)
		{
			return false;
		}
		return true;
	}
}
