using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoMod.Utils.Interop;

internal static class Unix
{
	public struct LinuxAuxvEntry
	{
		public nint Key;

		public nint Value;
	}

	public enum DlopenFlags
	{
		RTLD_LAZY = 1,
		RTLD_NOW = 2,
		RTLD_LOCAL = 0,
		RTLD_GLOBAL = 256
	}

	public const string LibC = "libc";

	public const string DL1 = "dl";

	public const string DL2 = "libdl.so.2";

	public const int AT_PLATFORM = 15;

	private static int dlVersion = 1;

	[DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "uname", SetLastError = true)]
	public unsafe static extern int Uname(byte* buf);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen")]
	private unsafe static extern IntPtr DL1dlopen(byte* filename, DlopenFlags flags);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlclose")]
	private static extern int DL1dlclose(IntPtr handle);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym")]
	private unsafe static extern IntPtr DL1dlsym(IntPtr handle, byte* symbol);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlerror")]
	private static extern IntPtr DL1dlerror();

	[DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen")]
	private unsafe static extern IntPtr DL2dlopen(byte* filename, DlopenFlags flags);

	[DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlclose")]
	private static extern int DL2dlclose(IntPtr handle);

	[DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym")]
	private unsafe static extern IntPtr DL2dlsym(IntPtr handle, byte* symbol);

	[DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlerror")]
	private static extern IntPtr DL2dlerror();

	internal static byte[]? MarshalToUtf8(string? str)
	{
		if (str == null)
		{
			return null;
		}
		int byteCount = Encoding.UTF8.GetByteCount(str);
		byte[] array = ArrayPool<byte>.Shared.Rent(byteCount + 1);
		array.AsSpan().Clear();
		Encoding.UTF8.GetBytes(str, 0, str.Length, array, 0);
		return array;
	}

	internal static void FreeMarshalledArray(byte[]? arr)
	{
		if (arr != null)
		{
			ArrayPool<byte>.Shared.Return(arr);
		}
	}

	public unsafe static IntPtr DlOpen(string? filename, DlopenFlags flags)
	{
		byte[] array = MarshalToUtf8(filename);
		try
		{
			while (true)
			{
				try
				{
					fixed (byte* filename2 = array)
					{
						int num = dlVersion;
						if (num != 0 && num == 1)
						{
							return DL2dlopen(filename2, flags);
						}
						return DL1dlopen(filename2, flags);
					}
				}
				catch (DllNotFoundException) when (dlVersion > 0)
				{
					dlVersion--;
				}
			}
		}
		finally
		{
			FreeMarshalledArray(array);
		}
	}

	public static bool DlClose(IntPtr handle)
	{
		while (true)
		{
			try
			{
				int num = dlVersion;
				if (num != 0 && num == 1)
				{
					return DL2dlclose(handle) == 0;
				}
				return DL1dlclose(handle) == 0;
			}
			catch (DllNotFoundException) when (dlVersion > 0)
			{
				dlVersion--;
			}
		}
	}

	public unsafe static IntPtr DlSym(IntPtr handle, string symbol)
	{
		byte[] array = MarshalToUtf8(symbol);
		try
		{
			while (true)
			{
				try
				{
					fixed (byte* symbol2 = array)
					{
						int num = dlVersion;
						if (num != 0 && num == 1)
						{
							return DL2dlsym(handle, symbol2);
						}
						return DL1dlsym(handle, symbol2);
					}
				}
				catch (DllNotFoundException) when (dlVersion > 0)
				{
					dlVersion--;
				}
			}
		}
		finally
		{
			FreeMarshalledArray(array);
		}
	}

	public static IntPtr DlError()
	{
		while (true)
		{
			try
			{
				int num = dlVersion;
				if (num != 0 && num == 1)
				{
					return DL2dlerror();
				}
				return DL1dlerror();
			}
			catch (DllNotFoundException) when (dlVersion > 0)
			{
				dlVersion--;
			}
		}
	}
}
