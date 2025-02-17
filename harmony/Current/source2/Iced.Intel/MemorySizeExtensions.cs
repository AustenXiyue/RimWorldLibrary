using System;

namespace Iced.Intel;

internal static class MemorySizeExtensions
{
	internal static readonly MemorySizeInfo[] MemorySizeInfos = GetMemorySizeInfos();

	public static bool IsBroadcast(this MemorySize memorySize)
	{
		return memorySize >= MemorySize.Broadcast32_Float16;
	}

	private static MemorySizeInfo[] GetMemorySizeInfos()
	{
		ReadOnlySpan<byte> readOnlySpan = new byte[480]
		{
			0, 0, 0, 1, 33, 0, 2, 66, 0, 3,
			99, 0, 4, 165, 0, 5, 165, 0, 6, 8,
			1, 7, 74, 1, 8, 140, 1, 9, 33, 128,
			10, 66, 128, 11, 99, 128, 12, 165, 128, 13,
			8, 129, 14, 74, 129, 15, 140, 129, 16, 99,
			0, 17, 132, 0, 18, 198, 0, 19, 66, 0,
			20, 99, 0, 21, 165, 0, 2, 67, 0, 3,
			101, 0, 24, 165, 0, 25, 8, 1, 26, 132,
			0, 27, 198, 0, 28, 66, 128, 29, 99, 128,
			30, 165, 128, 31, 198, 128, 32, 8, 129, 33,
			66, 128, 34, 231, 0, 35, 41, 1, 36, 173,
			1, 37, 206, 1, 38, 239, 1, 39, 239, 1,
			40, 0, 0, 41, 0, 0, 42, 198, 128, 43,
			140, 1, 44, 0, 0, 45, 198, 0, 46, 107,
			1, 47, 140, 1, 1, 34, 0, 9, 34, 128,
			1, 35, 0, 9, 35, 128, 2, 67, 0, 10,
			67, 128, 28, 67, 128, 33, 67, 128, 1, 37,
			0, 9, 37, 128, 2, 69, 0, 10, 69, 128,
			3, 101, 0, 11, 101, 128, 28, 69, 128, 29,
			101, 128, 1, 40, 0, 9, 40, 128, 2, 72,
			0, 10, 72, 128, 3, 104, 0, 11, 104, 128,
			4, 168, 0, 5, 168, 0, 12, 168, 128, 28,
			72, 128, 29, 104, 128, 30, 168, 128, 54, 104,
			128, 55, 104, 128, 1, 42, 0, 9, 42, 128,
			2, 74, 0, 10, 74, 128, 3, 106, 0, 11,
			106, 128, 4, 170, 0, 5, 170, 0, 12, 170,
			128, 6, 10, 1, 13, 10, 129, 28, 74, 128,
			29, 106, 128, 30, 170, 128, 32, 10, 129, 54,
			106, 128, 55, 106, 128, 1, 44, 0, 9, 44,
			128, 2, 76, 0, 10, 76, 128, 3, 108, 0,
			11, 108, 128, 4, 172, 0, 5, 172, 0, 12,
			172, 128, 6, 12, 1, 28, 76, 128, 29, 108,
			128, 30, 172, 128, 54, 108, 128, 55, 108, 128,
			28, 66, 128, 3, 99, 0, 11, 99, 128, 28,
			66, 128, 29, 99, 128, 10, 66, 0, 2, 66,
			0, 3, 99, 0, 11, 99, 128, 4, 165, 0,
			5, 165, 0, 12, 165, 128, 28, 66, 128, 29,
			99, 128, 30, 165, 128, 53, 99, 128, 61, 165,
			128, 60, 165, 0, 54, 99, 128, 55, 99, 128,
			10, 66, 0, 2, 66, 0, 3, 99, 0, 11,
			99, 128, 4, 165, 0, 5, 165, 0, 12, 165,
			128, 28, 66, 128, 29, 99, 128, 30, 165, 128,
			53, 99, 128, 61, 165, 128, 60, 165, 0, 54,
			99, 128, 55, 99, 128, 10, 66, 0, 2, 66,
			0, 3, 99, 0, 11, 99, 128, 4, 165, 0,
			5, 165, 0, 12, 165, 128, 28, 66, 128, 29,
			99, 128, 30, 165, 128, 54, 99, 128, 53, 99,
			128, 60, 165, 0, 61, 165, 128, 55, 99, 128
		};
		ushort[] array = new ushort[16]
		{
			0, 1, 2, 4, 6, 8, 10, 14, 16, 28,
			32, 48, 64, 94, 108, 512
		};
		MemorySizeInfo[] array2 = new MemorySizeInfo[160];
		int num = 0;
		int num2 = 0;
		while (num < array2.Length)
		{
			MemorySize elementType = (MemorySize)readOnlySpan[num2];
			uint num3 = (uint)((readOnlySpan[num2 + 2] << 8) | readOnlySpan[num2 + 1]);
			ushort size = array[num3 & 0x1F];
			ushort elementSize = array[(num3 >> 5) & 0x1F];
			array2[num] = new MemorySizeInfo((MemorySize)num, size, elementSize, elementType, (num3 & 0x8000) != 0, num >= 110);
			num++;
			num2 += 3;
		}
		return array2;
	}

	public static MemorySizeInfo GetInfo(this MemorySize memorySize)
	{
		MemorySizeInfo[] memorySizeInfos = MemorySizeInfos;
		if ((uint)memorySize >= (uint)memorySizeInfos.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_memorySize();
		}
		return memorySizeInfos[(int)memorySize];
	}

	public static int GetSize(this MemorySize memorySize)
	{
		return memorySize.GetInfo().Size;
	}

	public static int GetElementSize(this MemorySize memorySize)
	{
		return memorySize.GetInfo().ElementSize;
	}

	public static MemorySize GetElementType(this MemorySize memorySize)
	{
		return memorySize.GetInfo().ElementType;
	}

	public static MemorySizeInfo GetElementTypeInfo(this MemorySize memorySize)
	{
		return memorySize.GetInfo().ElementType.GetInfo();
	}

	public static bool IsSigned(this MemorySize memorySize)
	{
		return memorySize.GetInfo().IsSigned;
	}

	public static bool IsPacked(this MemorySize memorySize)
	{
		return memorySize.GetInfo().IsPacked;
	}

	public static int GetElementCount(this MemorySize memorySize)
	{
		return memorySize.GetInfo().ElementCount;
	}
}
