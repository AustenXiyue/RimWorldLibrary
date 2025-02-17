using System.Runtime.CompilerServices;

namespace System;

internal static class StringExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Replace(this string self, string oldValue, string newValue, StringComparison comparison)
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, System.ExceptionArgument.self);
		System.ThrowHelper.ThrowIfArgumentNull(oldValue, System.ExceptionArgument.oldValue);
		System.ThrowHelper.ThrowIfArgumentNull(newValue, System.ExceptionArgument.newValue);
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(oldValue.Length, 0);
		ReadOnlySpan<char> readOnlySpan = self.AsSpan();
		ReadOnlySpan<char> value = oldValue.AsSpan();
		while (true)
		{
			int num = readOnlySpan.IndexOf(value, comparison);
			if (num < 0)
			{
				break;
			}
			defaultInterpolatedStringHandler.AppendFormatted(readOnlySpan.Slice(0, num));
			defaultInterpolatedStringHandler.AppendLiteral(newValue);
			readOnlySpan = readOnlySpan.Slice(num + value.Length);
		}
		defaultInterpolatedStringHandler.AppendFormatted(readOnlySpan);
		return defaultInterpolatedStringHandler.ToStringAndClear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Contains(this string self, string value, StringComparison comparison)
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, System.ExceptionArgument.self);
		System.ThrowHelper.ThrowIfArgumentNull(value, System.ExceptionArgument.value);
		return self.IndexOf(value, comparison) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Contains(this string self, char value, StringComparison comparison)
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, System.ExceptionArgument.self);
		return self.IndexOf(value, comparison) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetHashCode(this string self, StringComparison comparison)
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, System.ExceptionArgument.self);
		return StringComparerEx.FromComparison(comparison).GetHashCode(self);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf(this string self, char value, StringComparison comparison)
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, System.ExceptionArgument.self);
		return self.IndexOf(new string(value, 1), comparison);
	}
}
