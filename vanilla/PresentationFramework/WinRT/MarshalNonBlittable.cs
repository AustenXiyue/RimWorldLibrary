using System;
using System.Runtime.InteropServices;

namespace WinRT;

internal class MarshalNonBlittable<T> : MarshalGeneric<T>
{
	internal struct MarshalerArray
	{
		public nint _array;

		public object[] _marshalers;

		public void Dispose()
		{
			if (_marshalers != null)
			{
				object[] marshalers = _marshalers;
				foreach (object obj in marshalers)
				{
					Marshaler<T>.DisposeMarshaler(obj);
				}
			}
			if (_array != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(_array);
			}
		}
	}

	public unsafe static MarshalerArray CreateMarshalerArray(T[] array)
	{
		MarshalerArray m = default(MarshalerArray);
		if (array == null)
		{
			return m;
		}
		Func<bool> func = delegate
		{
			m.Dispose();
			return false;
		};
		try
		{
			int num = array.Length;
			int num2 = Marshal.SizeOf(MarshalGeneric<T>.HelperType);
			int cb = num * num2;
			m._array = Marshal.AllocCoTaskMem(cb);
			m._marshalers = new object[num];
			byte* ptr = (byte*)((IntPtr)m._array).ToPointer();
			for (int i = 0; i < num; i++)
			{
				m._marshalers[i] = Marshaler<T>.CreateMarshaler(array[i]);
				Marshaler<T>.CopyAbi(m._marshalers[i], (nint)ptr);
				ptr += num2;
			}
			return m;
		}
		catch (Exception) when (func())
		{
			return default(MarshalerArray);
		}
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		MarshalerArray marshalerArray = (MarshalerArray)box;
		object[] marshalers = marshalerArray._marshalers;
		return (length: (marshalers != null) ? marshalers.Length : 0, data: marshalerArray._array);
	}

	public unsafe static T[] FromAbiArray(object box)
	{
		if (box == null)
		{
			return null;
		}
		(int, nint) tuple = ((int, nint))box;
		T[] array = new T[tuple.Item1];
		byte* ptr = (byte*)((IntPtr)tuple.Item2).ToPointer();
		int num = Marshal.SizeOf(MarshalGeneric<T>.HelperType);
		for (int i = 0; i < tuple.Item1; i++)
		{
			object arg = Marshal.PtrToStructure((nint)ptr, MarshalGeneric<T>.HelperType);
			array[i] = Marshaler<T>.FromAbi(arg);
			ptr += num;
		}
		return array;
	}

	public unsafe static void CopyAbiArray(T[] array, object box)
	{
		(int, nint) tuple = ((int, nint))box;
		if (tuple.Item2 != IntPtr.Zero)
		{
			byte* ptr = (byte*)((IntPtr)tuple.Item2).ToPointer();
			int num = Marshal.SizeOf(MarshalGeneric<T>.HelperType);
			for (int i = 0; i < tuple.Item1; i++)
			{
				object arg = Marshal.PtrToStructure((nint)ptr, MarshalGeneric<T>.HelperType);
				array[i] = Marshaler<T>.FromAbi(arg);
				ptr += num;
			}
		}
	}

	public unsafe static (int length, nint data) FromManagedArray(T[] array)
	{
		if (array == null)
		{
			return (length: 0, data: IntPtr.Zero);
		}
		nint data = IntPtr.Zero;
		int i = 0;
		Func<bool> func = delegate
		{
			DisposeAbiArray((i, data));
			i = 0;
			data = IntPtr.Zero;
			return false;
		};
		try
		{
			int num = array.Length;
			int num2 = Marshal.SizeOf(MarshalGeneric<T>.HelperType);
			int cb = num * num2;
			data = Marshal.AllocCoTaskMem(cb);
			byte* ptr = (byte*)((IntPtr)data).ToPointer();
			for (i = 0; i < num; i++)
			{
				Marshaler<T>.CopyManaged(array[i], (nint)ptr);
				ptr += num2;
			}
		}
		catch (Exception) when (func())
		{
			return default((int, nint));
		}
		return (length: i, data: data);
	}

	public unsafe static void CopyManagedArray(T[] array, nint data)
	{
		if (array == null)
		{
			return;
		}
		DisposeAbiArrayElements((length: array.Length, data: data));
		int i = 0;
		Func<bool> func = delegate
		{
			DisposeAbiArrayElements((length: i, data: data));
			return false;
		};
		try
		{
			int num = array.Length;
			int num2 = Marshal.SizeOf(MarshalGeneric<T>.HelperType);
			byte* ptr = (byte*)((IntPtr)data).ToPointer();
			for (i = 0; i < num; i++)
			{
				Marshaler<T>.CopyManaged(array[i], (nint)ptr);
				ptr += num2;
			}
		}
		catch (Exception) when (func())
		{
		}
	}

	public static void DisposeMarshalerArray(object box)
	{
		((MarshalerArray)box).Dispose();
	}

	public unsafe static void DisposeAbiArrayElements((int length, nint data) abi)
	{
		byte* ptr = (byte*)((IntPtr)abi.data).ToPointer();
		int num = Marshal.SizeOf(MarshalGeneric<T>.HelperType);
		for (int i = 0; i < abi.length; i++)
		{
			object obj = Marshal.PtrToStructure((nint)ptr, MarshalGeneric<T>.HelperType);
			Marshaler<T>.DisposeAbi(obj);
			ptr += num;
		}
	}

	public static void DisposeAbiArray(object box)
	{
		if (box != null)
		{
			(int, nint) abi = ((int, nint))box;
			if (abi.Item2 != IntPtr.Zero)
			{
				DisposeAbiArrayElements(abi);
				Marshal.FreeCoTaskMem(abi.Item2);
			}
		}
	}
}
