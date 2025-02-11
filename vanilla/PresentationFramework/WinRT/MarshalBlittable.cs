using System;
using System.Runtime.InteropServices;

namespace WinRT;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct MarshalBlittable<T>
{
	internal struct MarshalerArray
	{
		public GCHandle _gchandle;

		public MarshalerArray(Array array)
		{
			_gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		}

		public void Dispose()
		{
			_gchandle.Dispose();
		}
	}

	public static MarshalerArray CreateMarshalerArray(Array array)
	{
		return new MarshalerArray(array);
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		MarshalerArray marshalerArray = (MarshalerArray)box;
		return (length: ((Array)marshalerArray._gchandle.Target).Length, data: marshalerArray._gchandle.AddrOfPinnedObject());
	}

	public unsafe static T[] FromAbiArray(object box)
	{
		if (box == null)
		{
			return null;
		}
		(int, nint) tuple = ((int, nint))box;
		return new ReadOnlySpan<T>(((IntPtr)tuple.Item2).ToPointer(), tuple.Item1).ToArray();
	}

	public static (int length, nint data) FromManagedArray(Array array)
	{
		if (array == null)
		{
			return (length: 0, data: IntPtr.Zero);
		}
		int length = array.Length;
		nint num = Marshal.AllocCoTaskMem(length * Marshal.SizeOf<T>());
		CopyManagedArray(array, num);
		return (length: length, data: num);
	}

	public unsafe static void CopyManagedArray(Array array, nint data)
	{
		if (array != null)
		{
			int num = array.Length * Marshal.SizeOf<T>();
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Buffer.MemoryCopy(((IntPtr)gCHandle.AddrOfPinnedObject()).ToPointer(), ((IntPtr)data).ToPointer(), num, num);
			gCHandle.Free();
		}
	}

	public static void DisposeMarshalerArray(object box)
	{
		if (box != null)
		{
			((MarshalerArray)box).Dispose();
		}
	}

	public static void DisposeAbiArray(object box)
	{
		if (box != null)
		{
			Marshal.FreeCoTaskMem((((int, nint))box).Item2);
		}
	}
}
