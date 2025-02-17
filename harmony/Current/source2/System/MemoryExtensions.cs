using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

internal static class MemoryExtensions
{
	internal static readonly nint StringAdjustment = MeasureStringAdjustment();

	public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span)
	{
		return span.TrimStart().TrimEnd();
	}

	public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span)
	{
		int i;
		for (i = 0; i < span.Length && char.IsWhiteSpace(span[i]); i++)
		{
		}
		return span.Slice(i);
	}

	public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span)
	{
		int num = span.Length - 1;
		while (num >= 0 && char.IsWhiteSpace(span[num]))
		{
			num--;
		}
		return span.Slice(0, num + 1);
	}

	public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, char trimChar)
	{
		return span.TrimStart(trimChar).TrimEnd(trimChar);
	}

	public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, char trimChar)
	{
		int i;
		for (i = 0; i < span.Length && span[i] == trimChar; i++)
		{
		}
		return span.Slice(i);
	}

	public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, char trimChar)
	{
		int num = span.Length - 1;
		while (num >= 0 && span[num] == trimChar)
		{
			num--;
		}
		return span.Slice(0, num + 1);
	}

	public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
	{
		return span.TrimStart(trimChars).TrimEnd(trimChars);
	}

	public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
	{
		if (trimChars.IsEmpty)
		{
			return span.TrimStart();
		}
		int i;
		for (i = 0; i < span.Length; i++)
		{
			int num = 0;
			while (num < trimChars.Length)
			{
				if (span[i] != trimChars[num])
				{
					num++;
					continue;
				}
				goto IL_003c;
			}
			break;
			IL_003c:;
		}
		return span.Slice(i);
	}

	public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
	{
		if (trimChars.IsEmpty)
		{
			return span.TrimEnd();
		}
		int num;
		for (num = span.Length - 1; num >= 0; num--)
		{
			int num2 = 0;
			while (num2 < trimChars.Length)
			{
				if (span[num] != trimChars[num2])
				{
					num2++;
					continue;
				}
				goto IL_0044;
			}
			break;
			IL_0044:;
		}
		return span.Slice(0, num + 1);
	}

	public static bool IsWhiteSpace(this ReadOnlySpan<char> span)
	{
		for (int i = 0; i < span.Length; i++)
		{
			if (!char.IsWhiteSpace(span[i]))
			{
				return false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf<T>(this Span<T> span, T value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
		}
		if (typeof(T) == typeof(char))
		{
			return SpanHelpers.IndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
		}
		return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
		}
		return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOf<T>(this Span<T> span, T value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
		}
		if (typeof(T) == typeof(char))
		{
			return SpanHelpers.LastIndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
		}
		return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOf<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
		}
		return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SequenceEqual<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
	{
		int length = span.Length;
		if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
		{
			if (length == other.Length)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), (nuint)length * size);
			}
			return false;
		}
		if (length == other.Length)
		{
			return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other), length);
		}
		return false;
	}

	public static int SequenceCompareTo<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IComparable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), other.Length);
		}
		if (typeof(T) == typeof(char))
		{
			return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(other)), other.Length);
		}
		return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(other), other.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
		}
		if (typeof(T) == typeof(char))
		{
			return SpanHelpers.IndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
		}
		return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
		}
		return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
		}
		if (typeof(T) == typeof(char))
		{
			return SpanHelpers.LastIndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
		}
		return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
		}
		return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
		}
		return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
		}
		return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
		}
		return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
		}
		return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
		}
		return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
		}
		return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOfAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
		}
		return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOfAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
		}
		return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOfAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
		}
		return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
		}
		return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
		}
		return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
		}
		return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
	{
		int length = span.Length;
		if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
		{
			if (length == other.Length)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), (nuint)length * size);
			}
			return false;
		}
		if (length == other.Length)
		{
			return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other), length);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SequenceCompareTo<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IComparable<T>
	{
		if (typeof(T) == typeof(byte))
		{
			return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), other.Length);
		}
		if (typeof(T) == typeof(char))
		{
			return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(other)), other.Length);
		}
		return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(other), other.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool StartsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		int length = value.Length;
		if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
		{
			if (length <= span.Length)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (nuint)length * size);
			}
			return false;
		}
		if (length <= span.Length)
		{
			return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), length);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool StartsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		int length = value.Length;
		if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
		{
			if (length <= span.Length)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (nuint)length * size);
			}
			return false;
		}
		if (length <= span.Length)
		{
			return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), length);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EndsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		int length = span.Length;
		int length2 = value.Length;
		if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
		{
			if (length2 <= length)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (nuint)length2 * size);
			}
			return false;
		}
		if (length2 <= length)
		{
			return SpanHelpers.SequenceEqual(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2), ref MemoryMarshal.GetReference(value), length2);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EndsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
	{
		int length = span.Length;
		int length2 = value.Length;
		if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
		{
			if (length2 <= length)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (nuint)length2 * size);
			}
			return false;
		}
		if (length2 <= length)
		{
			return SpanHelpers.SequenceEqual(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2), ref MemoryMarshal.GetReference(value), length2);
		}
		return false;
	}

	public static void Reverse<T>(this Span<T> span)
	{
		if (span.Length > 1)
		{
			ref T reference = ref MemoryMarshal.GetReference(span);
			ref T reference2 = ref Unsafe.Add(ref Unsafe.Add(ref reference, span.Length), -1);
			do
			{
				T val = reference;
				reference = reference2;
				reference2 = val;
				reference = ref Unsafe.Add(ref reference, 1);
				reference2 = ref Unsafe.Add(ref reference2, -1);
			}
			while (Unsafe.IsAddressLessThan(ref reference, ref reference2));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this T[]? array)
	{
		return new Span<T>(array);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this T[]? array, int start, int length)
	{
		return new Span<T>(array, start, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this ArraySegment<T> segment)
	{
		return new Span<T>(segment.Array, segment.Offset, segment.Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this ArraySegment<T> segment, int start)
	{
		if ((uint)start > segment.Count)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		return new Span<T>(segment.Array, segment.Offset + start, segment.Count - start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this ArraySegment<T> segment, int start, int length)
	{
		if ((uint)start > segment.Count)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		if ((uint)length > segment.Count - start)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
		}
		return new Span<T>(segment.Array, segment.Offset + start, length);
	}

	public static Memory<T> AsMemory<T>(this T[]? array)
	{
		return new Memory<T>(array);
	}

	public static Memory<T> AsMemory<T>(this T[]? array, int start)
	{
		return new Memory<T>(array, start);
	}

	public static Memory<T> AsMemory<T>(this T[]? array, int start, int length)
	{
		return new Memory<T>(array, start, length);
	}

	public static Memory<T> AsMemory<T>(this ArraySegment<T> segment)
	{
		return new Memory<T>(segment.Array, segment.Offset, segment.Count);
	}

	public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start)
	{
		if ((uint)start > segment.Count)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		return new Memory<T>(segment.Array, segment.Offset + start, segment.Count - start);
	}

	public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start, int length)
	{
		if ((uint)start > segment.Count)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		if ((uint)length > segment.Count - start)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
		}
		return new Memory<T>(segment.Array, segment.Offset + start, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyTo<T>(this T[]? source, Span<T> destination)
	{
		new ReadOnlySpan<T>(source).CopyTo(destination);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyTo<T>(this T[]? source, Memory<T> destination)
	{
		source.CopyTo(destination.Span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Overlaps<T>(this Span<T> span, ReadOnlySpan<T> other)
	{
		return ((ReadOnlySpan<T>)span).Overlaps(other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Overlaps<T>(this Span<T> span, ReadOnlySpan<T> other, out int elementOffset)
	{
		return ((ReadOnlySpan<T>)span).Overlaps(other, out elementOffset);
	}

	public static bool Overlaps<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other)
	{
		if (span.IsEmpty || other.IsEmpty)
		{
			return false;
		}
		IntPtr intPtr = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other));
		if (Unsafe.SizeOf<IntPtr>() == 4)
		{
			if ((uint)(int)intPtr >= (uint)(span.Length * Unsafe.SizeOf<T>()))
			{
				return (uint)(int)intPtr > (uint)(-(other.Length * Unsafe.SizeOf<T>()));
			}
			return true;
		}
		if ((ulong)(long)intPtr >= (ulong)((long)span.Length * (long)Unsafe.SizeOf<T>()))
		{
			return (ulong)(long)intPtr > (ulong)(-((long)other.Length * (long)Unsafe.SizeOf<T>()));
		}
		return true;
	}

	public static bool Overlaps<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, out int elementOffset)
	{
		if (span.IsEmpty || other.IsEmpty)
		{
			elementOffset = 0;
			return false;
		}
		IntPtr intPtr = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other));
		if (Unsafe.SizeOf<IntPtr>() == 4)
		{
			if ((uint)(int)intPtr < (uint)(span.Length * Unsafe.SizeOf<T>()) || (uint)(int)intPtr > (uint)(-(other.Length * Unsafe.SizeOf<T>())))
			{
				if ((int)intPtr % Unsafe.SizeOf<T>() != 0)
				{
					System.ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch();
				}
				elementOffset = (int)intPtr / Unsafe.SizeOf<T>();
				return true;
			}
			elementOffset = 0;
			return false;
		}
		if ((ulong)(long)intPtr < (ulong)((long)span.Length * (long)Unsafe.SizeOf<T>()) || (ulong)(long)intPtr > (ulong)(-((long)other.Length * (long)Unsafe.SizeOf<T>())))
		{
			if ((long)intPtr % Unsafe.SizeOf<T>() != 0L)
			{
				System.ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch();
			}
			elementOffset = (int)((long)intPtr / Unsafe.SizeOf<T>());
			return true;
		}
		elementOffset = 0;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T>(this Span<T> span, IComparable<T> comparable)
	{
		return span.BinarySearch<T, IComparable<T>>(comparable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T, TComparable>(this Span<T> span, TComparable comparable) where TComparable : IComparable<T>
	{
		return BinarySearch((ReadOnlySpan<T>)span, comparable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T, TComparer>(this Span<T> span, T value, TComparer comparer) where TComparer : IComparer<T>
	{
		return ((ReadOnlySpan<T>)span).BinarySearch(value, comparer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T>(this ReadOnlySpan<T> span, IComparable<T> comparable)
	{
		return MemoryExtensions.BinarySearch<T, IComparable<T>>(span, comparable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) where TComparable : IComparable<T>
	{
		return SpanHelpers.BinarySearch(span, comparable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T, TComparer>(this ReadOnlySpan<T> span, T value, TComparer comparer) where TComparer : IComparer<T>
	{
		if (comparer == null)
		{
			System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.comparer);
		}
		SpanHelpers.ComparerComparable<T, TComparer> comparable = new SpanHelpers.ComparerComparable<T, TComparer>(value, comparer);
		return BinarySearch(span, comparable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsTypeComparableAsBytes<T>(out nuint size)
	{
		if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
		{
			size = 1u;
			return true;
		}
		if (typeof(T) == typeof(char) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort))
		{
			size = 2u;
			return true;
		}
		if (typeof(T) == typeof(int) || typeof(T) == typeof(uint))
		{
			size = 4u;
			return true;
		}
		if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
		{
			size = 8u;
			return true;
		}
		size = 0u;
		return false;
	}

	public static Span<T> AsSpan<T>(this T[]? array, int start)
	{
		return Span<T>.Create(array, start);
	}

	public static bool Contains(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
	{
		return span.IndexOf(value, comparisonType) >= 0;
	}

	public static bool Equals(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		switch (comparisonType)
		{
		case StringComparison.Ordinal:
			return span.SequenceEqual(other);
		case StringComparison.OrdinalIgnoreCase:
			if (span.Length != other.Length)
			{
				return false;
			}
			return EqualsOrdinalIgnoreCase(span, other);
		default:
			return span.ToString().Equals(other.ToString(), comparisonType);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool EqualsOrdinalIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> other)
	{
		if (other.Length == 0)
		{
			return true;
		}
		return CompareToOrdinalIgnoreCase(span, other) == 0;
	}

	public static int CompareTo(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		return comparisonType switch
		{
			StringComparison.Ordinal => span.SequenceCompareTo(other), 
			StringComparison.OrdinalIgnoreCase => CompareToOrdinalIgnoreCase(span, other), 
			_ => string.Compare(span.ToString(), other.ToString(), comparisonType), 
		};
	}

	private unsafe static int CompareToOrdinalIgnoreCase(ReadOnlySpan<char> strA, ReadOnlySpan<char> strB)
	{
		int num = Math.Min(strA.Length, strB.Length);
		int num2 = num;
		fixed (char* reference = &MemoryMarshal.GetReference(strA))
		{
			fixed (char* reference2 = &MemoryMarshal.GetReference(strB))
			{
				char* ptr = reference;
				char* ptr2 = reference2;
				while (num != 0 && *ptr <= '\u007f' && *ptr2 <= '\u007f')
				{
					int num3 = *ptr;
					int num4 = *ptr2;
					if (num3 == num4)
					{
						ptr++;
						ptr2++;
						num--;
						continue;
					}
					if ((uint)(num3 - 97) <= 25u)
					{
						num3 -= 32;
					}
					if ((uint)(num4 - 97) <= 25u)
					{
						num4 -= 32;
					}
					if (num3 != num4)
					{
						return num3 - num4;
					}
					ptr++;
					ptr2++;
					num--;
				}
				if (num == 0)
				{
					return strA.Length - strB.Length;
				}
				num2 -= num;
				return string.Compare(strA.Slice(num2).ToString(), strB.Slice(num2).ToString(), StringComparison.OrdinalIgnoreCase);
			}
		}
	}

	public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
	{
		if (comparisonType == StringComparison.Ordinal)
		{
			return span.IndexOf(value);
		}
		return span.ToString().IndexOf(value.ToString(), comparisonType);
	}

	public static int ToLower(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
	{
		System.ThrowHelper.ThrowIfArgumentNull(culture, System.ExceptionArgument.culture);
		if (destination.Length < source.Length)
		{
			return -1;
		}
		string str = source.ToString();
		culture.TextInfo.ToLower(str).AsSpan().CopyTo(destination);
		return source.Length;
	}

	public static int ToLowerInvariant(this ReadOnlySpan<char> source, Span<char> destination)
	{
		return source.ToLower(destination, CultureInfo.InvariantCulture);
	}

	public static int ToUpper(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
	{
		System.ThrowHelper.ThrowIfArgumentNull(culture, System.ExceptionArgument.culture);
		if (destination.Length < source.Length)
		{
			return -1;
		}
		string str = source.ToString();
		culture.TextInfo.ToUpper(str).AsSpan().CopyTo(destination);
		return source.Length;
	}

	public static int ToUpperInvariant(this ReadOnlySpan<char> source, Span<char> destination)
	{
		return source.ToUpper(destination, CultureInfo.InvariantCulture);
	}

	public static bool EndsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
	{
		switch (comparisonType)
		{
		case StringComparison.Ordinal:
			return span.EndsWith(value);
		case StringComparison.OrdinalIgnoreCase:
			if (value.Length <= span.Length)
			{
				return EqualsOrdinalIgnoreCase(span.Slice(span.Length - value.Length), value);
			}
			return false;
		default:
		{
			string text = span.ToString();
			string value2 = value.ToString();
			return text.EndsWith(value2, comparisonType);
		}
		}
	}

	public static bool StartsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
	{
		switch (comparisonType)
		{
		case StringComparison.Ordinal:
			return span.StartsWith(value);
		case StringComparison.OrdinalIgnoreCase:
			if (value.Length <= span.Length)
			{
				return EqualsOrdinalIgnoreCase(span.Slice(0, value.Length), value);
			}
			return false;
		default:
		{
			string text = span.ToString();
			string value2 = value.ToString();
			return text.StartsWith(value2, comparisonType);
		}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<char> AsSpan(this string? text)
	{
		if (text == null)
		{
			return default(ReadOnlySpan<char>);
		}
		return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment, text.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<char> AsSpan(this string? text, int start)
	{
		if (text == null)
		{
			if (start != 0)
			{
				System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
			}
			return default(ReadOnlySpan<char>);
		}
		if ((uint)start > (uint)text.Length)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment + start * 2, text.Length - start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<char> AsSpan(this string? text, int start, int length)
	{
		if (text == null)
		{
			if (start != 0 || length != 0)
			{
				System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
			}
			return default(ReadOnlySpan<char>);
		}
		if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment + start * 2, length);
	}

	public static ReadOnlyMemory<char> AsMemory(this string? text)
	{
		if (text == null)
		{
			return default(ReadOnlyMemory<char>);
		}
		return new ReadOnlyMemory<char>(text, 0, text.Length);
	}

	public static ReadOnlyMemory<char> AsMemory(this string? text, int start)
	{
		if (text == null)
		{
			if (start != 0)
			{
				System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
			}
			return default(ReadOnlyMemory<char>);
		}
		if ((uint)start > (uint)text.Length)
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		return new ReadOnlyMemory<char>(text, start, text.Length - start);
	}

	public static ReadOnlyMemory<char> AsMemory(this string? text, int start, int length)
	{
		if (text == null)
		{
			if (start != 0 || length != 0)
			{
				System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
			}
			return default(ReadOnlyMemory<char>);
		}
		if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
		}
		return new ReadOnlyMemory<char>(text, start, length);
	}

	private unsafe static nint MeasureStringAdjustment()
	{
		fixed (char* source = "a")
		{
			return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<char>>("a").Data, ref Unsafe.AsRef<char>(source));
		}
	}
}
