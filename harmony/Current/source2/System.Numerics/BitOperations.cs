using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Numerics;

internal static class BitOperations
{
	private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
	{
		0, 1, 28, 2, 29, 14, 24, 3, 30, 22,
		20, 15, 25, 17, 4, 8, 31, 27, 13, 23,
		21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
		10, 9
	};

	private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
	{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
		16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
		15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
		4, 31
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int LeadingZeroCount(uint value)
	{
		if (value == 0)
		{
			return 32;
		}
		return 0x1F ^ Log2SoftwareFallback(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int LeadingZeroCount(ulong value)
	{
		uint num = (uint)(value >> 32);
		if (num == 0)
		{
			return 32 + LeadingZeroCount((uint)value);
		}
		return LeadingZeroCount(num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int Log2(uint value)
	{
		value |= 1;
		return Log2SoftwareFallback(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int Log2(ulong value)
	{
		value |= 1;
		uint num = (uint)(value >> 32);
		if (num == 0)
		{
			return Log2((uint)value);
		}
		return 32 + Log2(num);
	}

	private static int Log2SoftwareFallback(uint value)
	{
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(Log2DeBruijn), (IntPtr)(int)(value * 130329821 >> 27));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int Log2Ceiling(uint value)
	{
		int num = Log2(value);
		if (PopCount(value) != 1)
		{
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int Log2Ceiling(ulong value)
	{
		int num = Log2(value);
		if (PopCount(value) != 1)
		{
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int PopCount(uint value)
	{
		return SoftwareFallback(value);
		static int SoftwareFallback(uint value)
		{
			value -= (value >> 1) & 0x55555555;
			value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
			value = ((value + (value >> 4)) & 0xF0F0F0F) * 16843009 >> 24;
			return (int)value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int PopCount(ulong value)
	{
		if (IntPtr.Size == 8)
		{
			return PopCount((uint)value) + PopCount((uint)(value >> 32));
		}
		return SoftwareFallback(value);
		static int SoftwareFallback(ulong value)
		{
			value -= (value >> 1) & 0x5555555555555555L;
			value = (value & 0x3333333333333333L) + ((value >> 2) & 0x3333333333333333L);
			value = ((value + (value >> 4)) & 0xF0F0F0F0F0F0F0FL) * 72340172838076673L >> 56;
			return (int)value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(int value)
	{
		return TrailingZeroCount((uint)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int TrailingZeroCount(uint value)
	{
		if (value == 0)
		{
			return 32;
		}
		return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn), (IntPtr)(int)((value & (0 - value)) * 125613361 >> 27));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(long value)
	{
		return TrailingZeroCount((ulong)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static int TrailingZeroCount(ulong value)
	{
		uint num = (uint)value;
		if (num == 0)
		{
			return 32 + TrailingZeroCount((uint)(value >> 32));
		}
		return TrailingZeroCount(num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static uint RotateLeft(uint value, int offset)
	{
		return (value << offset) | (value >> 32 - offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static ulong RotateLeft(ulong value, int offset)
	{
		return (value << offset) | (value >> 64 - offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static uint RotateRight(uint value, int offset)
	{
		return (value >> offset) | (value << 32 - offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public static ulong RotateRight(ulong value, int offset)
	{
		return (value >> offset) | (value << 64 - offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ResetLowestSetBit(uint value)
	{
		return value & (value - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ResetBit(uint value, int bitPos)
	{
		return value & (uint)(~(1 << bitPos));
	}
}
