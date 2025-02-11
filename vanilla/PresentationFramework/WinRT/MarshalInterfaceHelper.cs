using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal class MarshalInterfaceHelper<T>
{
	internal struct MarshalerArray
	{
		public nint _array;

		public IObjectReference[] _marshalers;

		public void Dispose()
		{
			if (_marshalers != null)
			{
				IObjectReference[] marshalers = _marshalers;
				for (int i = 0; i < marshalers.Length; i++)
				{
					MarshalInterfaceHelper<T>.DisposeMarshaler(marshalers[i]);
				}
			}
			if (_array != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(_array);
			}
		}
	}

	public unsafe static MarshalerArray CreateMarshalerArray(T[] array, Func<T, IObjectReference> createMarshaler)
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
			int cb = num * IntPtr.Size;
			m._array = Marshal.AllocCoTaskMem(cb);
			m._marshalers = new IObjectReference[num];
			nint* ptr = (nint*)((IntPtr)m._array).ToPointer();
			for (int i = 0; i < num; i++)
			{
				m._marshalers[i] = createMarshaler(array[i]);
				ptr[i] = GetAbi(m._marshalers[i]);
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
		IObjectReference[] marshalers = marshalerArray._marshalers;
		return (length: (marshalers != null) ? marshalers.Length : 0, data: marshalerArray._array);
	}

	public unsafe static T[] FromAbiArray(object box, Func<nint, T> fromAbi)
	{
		if (box == null)
		{
			return null;
		}
		(int, nint) tuple = ((int, nint))box;
		T[] array = new T[tuple.Item1];
		nint* ptr = (nint*)((IntPtr)tuple.Item2).ToPointer();
		for (int i = 0; i < tuple.Item1; i++)
		{
			array[i] = fromAbi(ptr[i]);
		}
		return array;
	}

	public unsafe static (int length, nint data) FromManagedArray(T[] array, Func<T, nint> fromManaged)
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
			int cb = num * IntPtr.Size;
			data = Marshal.AllocCoTaskMem(cb);
			nint* ptr = (nint*)((IntPtr)data).ToPointer();
			for (i = 0; i < num; i++)
			{
				ptr[i] = fromManaged(array[i]);
			}
		}
		catch (Exception) when (func())
		{
			return default((int, nint));
		}
		return (length: i, data: data);
	}

	public unsafe static void CopyManagedArray(T[] array, nint data, Action<T, nint> copyManaged)
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
			_ = IntPtr.Size;
			byte* ptr = (byte*)((IntPtr)data).ToPointer();
			for (i = 0; i < num; i++)
			{
				copyManaged(array[i], (nint)ptr);
				ptr += IntPtr.Size;
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
		nint* ptr = (nint*)((IntPtr)abi.data).ToPointer();
		for (int i = 0; i < abi.length; i++)
		{
			DisposeAbi(ptr[i]);
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

	public static nint GetAbi(IObjectReference objRef)
	{
		return objRef?.ThisPtr ?? IntPtr.Zero;
	}

	public static void DisposeMarshaler(IObjectReference objRef)
	{
		objRef?.Dispose();
	}

	public static void DisposeAbi(nint ptr)
	{
		if (ptr != IntPtr.Zero)
		{
			ObjectReference<IUnknownVftbl>.Attach(ref ptr).Dispose();
		}
	}
}
