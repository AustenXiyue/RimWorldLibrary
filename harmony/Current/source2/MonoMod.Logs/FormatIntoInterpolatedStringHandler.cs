using System;
using System.Runtime.CompilerServices;

namespace MonoMod.Logs;

[InterpolatedStringHandler]
internal ref struct FormatIntoInterpolatedStringHandler
{
	private readonly Span<char> _chars;

	internal int pos;

	internal bool incomplete;

	public FormatIntoInterpolatedStringHandler(int literalLen, int numHoles, Span<char> into, out bool enabled)
	{
		_chars = into;
		pos = 0;
		if (into.Length < literalLen)
		{
			incomplete = true;
			enabled = false;
		}
		else
		{
			incomplete = false;
			enabled = true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool AppendLiteral(string value)
	{
		if (value.Length == 1)
		{
			Span<char> chars = _chars;
			int num = pos;
			if ((uint)num < (uint)chars.Length)
			{
				chars[num] = value[0];
				pos = num + 1;
				return true;
			}
			incomplete = true;
			return false;
		}
		if (value.Length == 2)
		{
			Span<char> chars2 = _chars;
			int num2 = pos;
			if ((uint)num2 < chars2.Length - 1)
			{
				value.AsSpan().CopyTo(chars2.Slice(num2));
				pos = num2 + 2;
				return true;
			}
			incomplete = true;
			return false;
		}
		return AppendStringDirect(value);
	}

	private bool AppendStringDirect(string value)
	{
		if (value.AsSpan().TryCopyTo(_chars.Slice(pos)))
		{
			pos += value.Length;
			return true;
		}
		incomplete = true;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool AppendFormatted(string? value)
	{
		if (value == null)
		{
			return true;
		}
		if (value.AsSpan().TryCopyTo(_chars.Slice(pos)))
		{
			pos += value.Length;
			return true;
		}
		incomplete = true;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool AppendFormatted(string? value, int alignment = 0, string? format = null)
	{
		return this.AppendFormatted<string>(value, alignment, format);
	}

	public bool AppendFormatted(ReadOnlySpan<char> value)
	{
		if (value.TryCopyTo(_chars.Slice(pos)))
		{
			pos += value.Length;
			return true;
		}
		incomplete = true;
		return false;
	}

	public bool AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
	{
		bool flag = false;
		if (alignment < 0)
		{
			flag = true;
			alignment = -alignment;
		}
		int num = alignment - value.Length;
		if (num <= 0)
		{
			return AppendFormatted(value);
		}
		if (_chars.Slice(pos).Length < value.Length + num)
		{
			incomplete = true;
			return false;
		}
		if (flag)
		{
			value.CopyTo(_chars.Slice(pos));
			pos += value.Length;
			_chars.Slice(pos, num).Fill(' ');
			pos += num;
		}
		else
		{
			_chars.Slice(pos, num).Fill(' ');
			pos += num;
			value.CopyTo(_chars.Slice(pos));
			pos += value.Length;
		}
		return true;
	}

	public bool AppendFormatted<T>(T value)
	{
		if (typeof(T) == typeof(IntPtr))
		{
			return AppendFormatted(Unsafe.As<T, IntPtr>(ref value));
		}
		if (typeof(T) == typeof(UIntPtr))
		{
			return AppendFormatted(Unsafe.As<T, UIntPtr>(ref value));
		}
		if (DebugFormatter.CanDebugFormat(in value, out object extraData))
		{
			if (!DebugFormatter.TryFormatInto(in value, extraData, _chars.Slice(pos), out var wrote))
			{
				incomplete = true;
				return false;
			}
			pos += wrote;
			return true;
		}
		string text = ((!(value is IFormattable)) ? value?.ToString() : ((IFormattable)(object)value).ToString(null, null));
		if (text != null)
		{
			return AppendStringDirect(text);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool AppendFormatted(IntPtr value)
	{
		if (IntPtr.Size == 4)
		{
			return AppendFormatted((int)value);
		}
		return AppendFormatted((long)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool AppendFormatted(IntPtr value, string? format)
	{
		if (IntPtr.Size == 4)
		{
			return AppendFormatted((int)value, format);
		}
		return AppendFormatted((long)value, format);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool AppendFormatted(UIntPtr value)
	{
		if (UIntPtr.Size == 4)
		{
			return AppendFormatted((uint)value);
		}
		return AppendFormatted((ulong)value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool AppendFormatted(UIntPtr value, string? format)
	{
		if (UIntPtr.Size == 4)
		{
			return AppendFormatted((uint)value, format);
		}
		return AppendFormatted((ulong)value, format);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool AppendFormatted<T>(T value, int alignment)
	{
		int startingPos = pos;
		if (!AppendFormatted(value))
		{
			return false;
		}
		if (alignment != 0)
		{
			return AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool AppendFormatted<T>(T value, string? format)
	{
		if (typeof(T) == typeof(IntPtr))
		{
			return AppendFormatted(Unsafe.As<T, IntPtr>(ref value), format);
		}
		if (typeof(T) == typeof(UIntPtr))
		{
			return AppendFormatted(Unsafe.As<T, UIntPtr>(ref value), format);
		}
		if (DebugFormatter.CanDebugFormat(in value, out object extraData))
		{
			if (!DebugFormatter.TryFormatInto(in value, extraData, _chars.Slice(pos), out var wrote))
			{
				incomplete = true;
				return false;
			}
			pos += wrote;
			return true;
		}
		string text = ((!(value is IFormattable)) ? value?.ToString() : ((IFormattable)(object)value).ToString(format, null));
		if (text != null)
		{
			return AppendStringDirect(text);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool AppendFormatted<T>(T value, int alignment, string? format)
	{
		int startingPos = pos;
		if (!AppendFormatted(value, format))
		{
			return false;
		}
		if (alignment != 0)
		{
			return AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
		}
		return true;
	}

	private bool AppendOrInsertAlignmentIfNeeded(int startingPos, int alignment)
	{
		int num = pos - startingPos;
		bool flag = false;
		if (alignment < 0)
		{
			flag = true;
			alignment = -alignment;
		}
		int num2 = alignment - num;
		if (num2 > 0)
		{
			if (_chars.Slice(pos).Length < num2)
			{
				incomplete = true;
				return false;
			}
			if (flag)
			{
				_chars.Slice(pos, num2).Fill(' ');
			}
			else
			{
				_chars.Slice(startingPos, num).CopyTo(_chars.Slice(startingPos + num2));
				_chars.Slice(startingPos, num2).Fill(' ');
			}
			pos += num2;
		}
		return true;
	}
}
