using System.Runtime.CompilerServices;

namespace System.Numerics;

internal static class BitOperationsEx
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPow2(int value)
	{
		if ((value & (value - 1)) == 0)
		{
			return value > 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static bool IsPow2(uint value)
	{
		if ((value & (value - 1)) == 0)
		{
			return value != 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPow2(long value)
	{
		if ((value & (value - 1)) == 0L)
		{
			return value > 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static bool IsPow2(ulong value)
	{
		if ((value & (value - 1)) == 0L)
		{
			return value != 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPow2(nint value)
	{
		if ((value & (value - 1)) == 0)
		{
			return value > 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static bool IsPow2(nuint value)
	{
		if ((value & (value - 1)) == 0)
		{
			return value != 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static uint RoundUpToPowerOf2(uint value)
	{
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		return value + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static ulong RoundUpToPowerOf2(ulong value)
	{
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		value |= value >> 32;
		return value + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static nuint RoundUpToPowerOf2(nuint value)
	{
		if (IntPtr.Size == 8)
		{
			return (nuint)RoundUpToPowerOf2((ulong)value);
		}
		return RoundUpToPowerOf2((uint)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int LeadingZeroCount(uint value)
	{
		return BitOperations.LeadingZeroCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int LeadingZeroCount(ulong value)
	{
		return BitOperations.LeadingZeroCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int LeadingZeroCount(nuint value)
	{
		if (IntPtr.Size == 8)
		{
			return LeadingZeroCount((ulong)value);
		}
		return LeadingZeroCount((uint)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int Log2(uint value)
	{
		return BitOperations.Log2(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int Log2(ulong value)
	{
		return BitOperations.Log2(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int Log2(nuint value)
	{
		if (IntPtr.Size == 8)
		{
			return Log2((ulong)value);
		}
		return Log2((uint)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int PopCount(uint value)
	{
		return BitOperations.PopCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int PopCount(ulong value)
	{
		return BitOperations.PopCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int PopCount(nuint value)
	{
		if (IntPtr.Size == 8)
		{
			return PopCount((ulong)value);
		}
		return PopCount((uint)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(int value)
	{
		return BitOperations.TrailingZeroCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int TrailingZeroCount(uint value)
	{
		return BitOperations.TrailingZeroCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(long value)
	{
		return BitOperations.TrailingZeroCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int TrailingZeroCount(ulong value)
	{
		return BitOperations.TrailingZeroCount(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(nint value)
	{
		if (IntPtr.Size == 8)
		{
			return TrailingZeroCount((long)value);
		}
		return TrailingZeroCount((int)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int TrailingZeroCount(nuint value)
	{
		if (IntPtr.Size == 8)
		{
			return TrailingZeroCount((ulong)value);
		}
		return TrailingZeroCount((uint)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static uint RotateLeft(uint value, int offset)
	{
		return BitOperations.RotateLeft(value, offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static ulong RotateLeft(ulong value, int offset)
	{
		return BitOperations.RotateLeft(value, offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static nuint RotateLeft(nuint value, int offset)
	{
		if (IntPtr.Size == 8)
		{
			return (nuint)RotateLeft((ulong)value, offset);
		}
		return RotateLeft((uint)value, offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static uint RotateRight(uint value, int offset)
	{
		return BitOperations.RotateRight(value, offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static ulong RotateRight(ulong value, int offset)
	{
		return BitOperations.RotateRight(value, offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static nuint RotateRight(nuint value, int offset)
	{
		if (IntPtr.Size == 8)
		{
			return (nuint)RotateRight((ulong)value, offset);
		}
		return RotateRight((uint)value, offset);
	}
}
