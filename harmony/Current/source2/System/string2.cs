using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal static class string2
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty([_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(false)] string? value)
	{
		return string.IsNullOrEmpty(value);
	}
}
