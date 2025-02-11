using System;

namespace ABI.System;

internal struct Char
{
	private ushort value;

	public static char CreateMarshaler(char value)
	{
		return value;
	}

	public static Char GetAbi(char value)
	{
		Char result = default(Char);
		result.value = value;
		return result;
	}

	public static char FromAbi(Char abi)
	{
		return (char)abi.value;
	}

	public unsafe static void CopyAbi(char value, nint dest)
	{
		*(ushort*)((IntPtr)dest).ToPointer() = GetAbi(value).value;
	}

	public static Char FromManaged(char value)
	{
		return GetAbi(value);
	}

	public unsafe static void CopyManaged(char arg, nint dest)
	{
		*(ushort*)((IntPtr)dest).ToPointer() = FromManaged(arg).value;
	}

	public static void DisposeMarshaler(char m)
	{
	}

	public static void DisposeAbi(Char abi)
	{
	}
}
