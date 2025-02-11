using System;
using System.Runtime.InteropServices;
using MS.Win32;

namespace MS.Internal.Controls;

internal class EnumUnknown : MS.Win32.UnsafeNativeMethods.IEnumUnknown
{
	private object[] arr;

	private int loc;

	private int size;

	internal EnumUnknown(object[] arr)
	{
		this.arr = arr;
		loc = 0;
		size = ((arr != null) ? arr.Length : 0);
	}

	private EnumUnknown(object[] arr, int loc)
		: this(arr)
	{
		this.loc = loc;
	}

	unsafe int MS.Win32.UnsafeNativeMethods.IEnumUnknown.Next(int celt, nint rgelt, nint pceltFetched)
	{
		if (pceltFetched != IntPtr.Zero)
		{
			Marshal.WriteInt32(pceltFetched, 0, 0);
		}
		if (celt < 0)
		{
			return -2147024809;
		}
		int num = 0;
		if (loc >= size)
		{
			num = 0;
		}
		else
		{
			while (loc < size && num < celt)
			{
				if (arr[loc] != null)
				{
					Marshal.WriteIntPtr(rgelt, Marshal.GetIUnknownForObject(arr[loc]));
					rgelt = (nint)((long)rgelt + (long)sizeof(nint));
					num++;
				}
				loc++;
			}
		}
		if (pceltFetched != IntPtr.Zero)
		{
			Marshal.WriteInt32(pceltFetched, 0, num);
		}
		if (num != celt)
		{
			return 1;
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IEnumUnknown.Skip(int celt)
	{
		loc += celt;
		if (loc >= size)
		{
			return 1;
		}
		return 0;
	}

	void MS.Win32.UnsafeNativeMethods.IEnumUnknown.Reset()
	{
		loc = 0;
	}

	void MS.Win32.UnsafeNativeMethods.IEnumUnknown.Clone(out MS.Win32.UnsafeNativeMethods.IEnumUnknown ppenum)
	{
		ppenum = new EnumUnknown(arr, loc);
	}
}
