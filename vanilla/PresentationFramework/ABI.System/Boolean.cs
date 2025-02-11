using System;

namespace ABI.System;

internal struct Boolean
{
	private byte value;

	public static bool CreateMarshaler(bool value)
	{
		return value;
	}

	public static Boolean GetAbi(bool value)
	{
		Boolean result = default(Boolean);
		result.value = (value ? ((byte)1) : ((byte)0));
		return result;
	}

	public static bool FromAbi(Boolean abi)
	{
		return abi.value != 0;
	}

	public unsafe static void CopyAbi(bool value, nint dest)
	{
		*(byte*)((IntPtr)dest).ToPointer() = GetAbi(value).value;
	}

	public static Boolean FromManaged(bool value)
	{
		return GetAbi(value);
	}

	public unsafe static void CopyManaged(bool arg, nint dest)
	{
		*(byte*)((IntPtr)dest).ToPointer() = FromManaged(arg).value;
	}

	public static void DisposeMarshaler(bool m)
	{
	}

	public static void DisposeAbi(byte abi)
	{
	}
}
