using System;
using System.Runtime.InteropServices;

namespace WinRT;

internal class MarshalString
{
	public struct HStringHeader
	{
		public unsafe fixed byte Reserved[24];
	}

	internal struct MarshalerArray
	{
		public nint _array;

		public MarshalString[] _marshalers;

		public void Dispose()
		{
			if (_marshalers != null)
			{
				MarshalString[] marshalers = _marshalers;
				for (int i = 0; i < marshalers.Length; i++)
				{
					marshalers[i]?.Dispose();
				}
			}
			if (_array != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(_array);
			}
		}
	}

	public HStringHeader _header;

	public GCHandle _gchandle;

	public nint _handle;

	public void Dispose()
	{
		_gchandle.Dispose();
	}

	public unsafe static MarshalString CreateMarshaler(string value)
	{
		if (value == null)
		{
			return null;
		}
		MarshalString m = new MarshalString();
		Func<bool> func = delegate
		{
			m.Dispose();
			return false;
		};
		try
		{
			m._gchandle = GCHandle.Alloc(value, GCHandleType.Pinned);
			fixed (char* sourceString = value)
			{
				fixed (HStringHeader* header = &m._header)
				{
					void* hstring_header = header;
					fixed (nint* handle = &m._handle)
					{
						void* hstring = handle;
						Marshal.ThrowExceptionForHR(Platform.WindowsCreateStringReference(sourceString, value.Length, (nint*)hstring_header, (nint*)hstring));
					}
				}
			}
			return m;
		}
		catch (Exception) when (func())
		{
			return null;
		}
	}

	public static nint GetAbi(MarshalString m)
	{
		return m?._handle ?? IntPtr.Zero;
	}

	public static nint GetAbi(object box)
	{
		if (box != null)
		{
			return ((MarshalString)box)._handle;
		}
		return IntPtr.Zero;
	}

	public static void DisposeMarshaler(MarshalString m)
	{
		m?.Dispose();
	}

	public static void DisposeMarshaler(object box)
	{
		if (box != null)
		{
			DisposeMarshaler((MarshalString)box);
		}
	}

	public static void DisposeAbi(nint hstring)
	{
		if (hstring != IntPtr.Zero)
		{
			Platform.WindowsDeleteString(hstring);
		}
	}

	public static void DisposeAbi(object abi)
	{
		if (abi != null)
		{
			DisposeAbi((nint)abi);
		}
	}

	public unsafe static string FromAbi(nint value)
	{
		if (value == IntPtr.Zero)
		{
			return "";
		}
		uint length = default(uint);
		return new string(Platform.WindowsGetStringRawBuffer(value, &length), 0, (int)length);
	}

	public unsafe static nint FromManaged(string value)
	{
		if (value == null)
		{
			return IntPtr.Zero;
		}
		nint result = default(nint);
		Marshal.ThrowExceptionForHR(Platform.WindowsCreateString(value, value.Length, &result));
		return result;
	}

	public unsafe static MarshalerArray CreateMarshalerArray(string[] array)
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
			m._array = Marshal.AllocCoTaskMem(num * Marshal.SizeOf<nint>());
			m._marshalers = new MarshalString[num];
			nint* ptr = (nint*)((IntPtr)m._array).ToPointer();
			for (int i = 0; i < num; i++)
			{
				m._marshalers[i] = CreateMarshaler(array[i]);
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
		MarshalString[] marshalers = marshalerArray._marshalers;
		return (length: (marshalers != null) ? marshalers.Length : 0, data: marshalerArray._array);
	}

	public unsafe static string[] FromAbiArray(object box)
	{
		if (box == null)
		{
			return null;
		}
		(int, nint) tuple = ((int, nint))box;
		string[] array = new string[tuple.Item1];
		nint* ptr = (nint*)((IntPtr)tuple.Item2).ToPointer();
		for (int i = 0; i < tuple.Item1; i++)
		{
			array[i] = FromAbi(ptr[i]);
		}
		return array;
	}

	public unsafe static void CopyAbiArray(string[] array, object box)
	{
		(int, nint) tuple = ((int, nint))box;
		nint* ptr = (nint*)((IntPtr)tuple.Item2).ToPointer();
		for (int i = 0; i < tuple.Item1; i++)
		{
			array[i] = FromAbi(ptr[i]);
		}
	}

	public unsafe static (int length, nint data) FromManagedArray(string[] array)
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
			data = Marshal.AllocCoTaskMem(num * Marshal.SizeOf<nint>());
			nint* ptr = (nint*)data;
			for (i = 0; i < num; i++)
			{
				ptr[i] = FromManaged(array[i]);
			}
		}
		catch (Exception) when (func())
		{
			return default((int, nint));
		}
		return (length: i, data: data);
	}

	public unsafe static void CopyManagedArray(string[] array, nint data)
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
			nint* ptr = (nint*)data;
			for (i = 0; i < num; i++)
			{
				ptr[i] = FromManaged(array[i]);
			}
		}
		catch (Exception) when (func())
		{
		}
	}

	public static void DisposeMarshalerArray(object box)
	{
		if (box != null)
		{
			((MarshalerArray)box).Dispose();
		}
	}

	public unsafe static void DisposeAbiArrayElements((int length, nint data) abi)
	{
		nint* item = (nint*)abi.data;
		for (int i = 0; i < abi.length; i++)
		{
			DisposeAbi(item[i]);
		}
	}

	public static void DisposeAbiArray(object box)
	{
		if (box != null)
		{
			(int, nint) abi = ((int, nint))box;
			DisposeAbiArrayElements(abi);
			Marshal.FreeCoTaskMem(abi.Item2);
		}
	}
}
