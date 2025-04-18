using System;
using System.Runtime.CompilerServices;

namespace Iced.Intel;

internal static class TupleTypeTable
{
	private static ReadOnlySpan<byte> tupleTypeData => new byte[38]
	{
		1, 1, 2, 2, 4, 4, 8, 8, 16, 16,
		32, 32, 64, 64, 8, 4, 16, 4, 32, 4,
		64, 4, 16, 8, 32, 8, 64, 8, 4, 2,
		8, 2, 16, 2, 32, 2, 64, 2
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint GetDisp8N(TupleType tupleType, bool bcst)
	{
		int index = ((int)tupleType << 1) | (bcst ? 1 : 0);
		return tupleTypeData[index];
	}
}
