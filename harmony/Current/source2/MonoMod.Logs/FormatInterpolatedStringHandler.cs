using System;
using System.Runtime.CompilerServices;

namespace MonoMod.Logs;

[InterpolatedStringHandler]
internal ref struct FormatInterpolatedStringHandler
{
	private DebugLogInterpolatedStringHandler handler;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FormatInterpolatedStringHandler(int literalLen, int formattedCount)
	{
		handler = new DebugLogInterpolatedStringHandler(literalLen, formattedCount, enabled: true, recordHoles: false, out var _);
	}

	public override string ToString()
	{
		return handler.ToString();
	}

	public string ToStringAndClear()
	{
		return handler.ToStringAndClear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendLiteral(string s)
	{
		handler.AppendLiteral(s);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted(string? s)
	{
		handler.AppendFormatted(s);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted(string? s, int alignment = 0, string? format = null)
	{
		handler.AppendFormatted(s, alignment, format);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted(ReadOnlySpan<char> s)
	{
		handler.AppendFormatted(s);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string? format = null)
	{
		handler.AppendFormatted(s, alignment, format);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value)
	{
		handler.AppendFormatted(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value, int alignment)
	{
		handler.AppendFormatted(value, alignment);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value, string? format)
	{
		handler.AppendFormatted(value, format);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value, int alignment, string? format)
	{
		handler.AppendFormatted(value, alignment, format);
	}
}
