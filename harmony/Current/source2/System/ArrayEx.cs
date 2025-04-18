using System.Runtime.CompilerServices;

namespace System;

internal static class ArrayEx
{
	private static class TypeHolder<T>
	{
		public static readonly T[] Empty = new T[0];
	}

	public static int MaxLength => 1879048191;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Empty<T>()
	{
		return TypeHolder<T>.Empty;
	}
}
