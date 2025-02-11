using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using MS.Win32;

namespace MS.Internal.Controls;

internal class ActiveXHelper
{
	public enum ActiveXState
	{
		Passive = 0,
		Loaded = 1,
		Running = 2,
		InPlaceActive = 4,
		UIActive = 8,
		Open = 0x10
	}

	public static readonly int sinkAttached = BitVector32.CreateMask();

	public static readonly int inTransition = BitVector32.CreateMask(sinkAttached);

	public static readonly int processingKeyUp = BitVector32.CreateMask(inTransition);

	private static int logPixelsX = -1;

	private static int logPixelsY = -1;

	private const int HMperInch = 2540;

	public static int LogPixelsX
	{
		get
		{
			if (logPixelsX == -1)
			{
				nint dC = MS.Win32.UnsafeNativeMethods.GetDC(MS.Win32.NativeMethods.NullHandleRef);
				if (dC != IntPtr.Zero)
				{
					logPixelsX = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 88);
					MS.Win32.UnsafeNativeMethods.ReleaseDC(MS.Win32.NativeMethods.NullHandleRef, new HandleRef(null, dC));
				}
			}
			return logPixelsX;
		}
	}

	public static int LogPixelsY
	{
		get
		{
			if (logPixelsY == -1)
			{
				nint dC = MS.Win32.UnsafeNativeMethods.GetDC(MS.Win32.NativeMethods.NullHandleRef);
				if (dC != IntPtr.Zero)
				{
					logPixelsY = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 90);
					MS.Win32.UnsafeNativeMethods.ReleaseDC(MS.Win32.NativeMethods.NullHandleRef, new HandleRef(null, dC));
				}
			}
			return logPixelsY;
		}
	}

	private ActiveXHelper()
	{
	}

	public static int Pix2HM(int pix, int logP)
	{
		return (2540 * pix + (logP >> 1)) / logP;
	}

	public static int HM2Pix(int hm, int logP)
	{
		return (logP * hm + 1270) / 2540;
	}
}
