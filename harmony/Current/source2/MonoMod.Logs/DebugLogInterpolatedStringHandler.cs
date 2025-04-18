using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MonoMod.Logs;

[InterpolatedStringHandler]
internal ref struct DebugLogInterpolatedStringHandler
{
	private const int GuessedLengthPerHole = 11;

	private const int MinimumArrayPoolLength = 256;

	private char[]? _arrayToReturnToPool;

	private Span<char> _chars;

	private int _pos;

	private int holeBegin;

	private int holePos;

	private Memory<MessageHole> holes;

	internal readonly bool enabled;

	internal ReadOnlySpan<char> Text => _chars.Slice(0, _pos);

	public DebugLogInterpolatedStringHandler(int literalLength, int formattedCount, bool enabled, bool recordHoles, out bool isEnabled)
	{
		_pos = (holeBegin = (holePos = 0));
		this.enabled = (isEnabled = enabled);
		if (enabled)
		{
			_chars = (_arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount)));
			if (recordHoles)
			{
				holes = new MessageHole[formattedCount];
			}
			else
			{
				holes = default(Memory<MessageHole>);
			}
		}
		else
		{
			_chars = (_arrayToReturnToPool = null);
			holes = default(Memory<MessageHole>);
		}
	}

	public DebugLogInterpolatedStringHandler(int literalLength, int formattedCount, out bool isEnabled)
	{
		DebugLog instance = DebugLog.Instance;
		_pos = (holeBegin = (holePos = 0));
		if (instance.ShouldLog)
		{
			enabled = (isEnabled = true);
			_chars = (_arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount)));
			if (instance.RecordHoles)
			{
				holes = new MessageHole[formattedCount];
			}
			else
			{
				holes = default(Memory<MessageHole>);
			}
		}
		else
		{
			enabled = (isEnabled = false);
			_chars = (_arrayToReturnToPool = null);
			holes = default(Memory<MessageHole>);
		}
	}

	public DebugLogInterpolatedStringHandler(int literalLength, int formattedCount, LogLevel level, out bool isEnabled)
	{
		DebugLog instance = DebugLog.Instance;
		_pos = (holeBegin = (holePos = 0));
		if (instance.ShouldLogLevel(level))
		{
			enabled = (isEnabled = true);
			_chars = (_arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount)));
			if (instance.ShouldLevelRecordHoles(level))
			{
				holes = new MessageHole[formattedCount];
			}
			else
			{
				holes = default(Memory<MessageHole>);
			}
		}
		else
		{
			enabled = (isEnabled = false);
			_chars = (_arrayToReturnToPool = null);
			holes = default(Memory<MessageHole>);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetDefaultLength(int literalLength, int formattedCount)
	{
		return Math.Max(256, literalLength + formattedCount * 11);
	}

	public override string ToString()
	{
		return Text.ToString();
	}

	public string ToStringAndClear()
	{
		string result = Text.ToString();
		Clear();
		return result;
	}

	internal string ToStringAndClear(out ReadOnlyMemory<MessageHole> holes)
	{
		holes = this.holes;
		return ToStringAndClear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Clear()
	{
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		this = default(DebugLogInterpolatedStringHandler);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendLiteral(string value)
	{
		if (value.Length == 1)
		{
			Span<char> chars = _chars;
			int pos = _pos;
			if ((uint)pos < (uint)chars.Length)
			{
				chars[pos] = value[0];
				_pos = pos + 1;
			}
			else
			{
				GrowThenCopyString(value);
			}
		}
		else if (value.Length == 2)
		{
			Span<char> chars2 = _chars;
			int pos2 = _pos;
			if ((uint)pos2 < chars2.Length - 1)
			{
				value.AsSpan().CopyTo(chars2.Slice(pos2));
				_pos = pos2 + 2;
			}
			else
			{
				GrowThenCopyString(value);
			}
		}
		else
		{
			AppendStringDirect(value);
		}
	}

	private void AppendStringDirect(string value)
	{
		if (value.AsSpan().TryCopyTo(_chars.Slice(_pos)))
		{
			_pos += value.Length;
		}
		else
		{
			GrowThenCopyString(value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void BeginHole()
	{
		holeBegin = _pos;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EndHole(object? obj, bool reprd)
	{
		EndHole(in obj, reprd);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void EndHole<T>(in T obj, bool reprd)
	{
		if (!holes.IsEmpty)
		{
			holes.Span[holePos++] = (reprd ? new MessageHole(holeBegin, _pos, obj) : new MessageHole(holeBegin, _pos));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted(string? value)
	{
		BeginHole();
		if (value != null && value.AsSpan().TryCopyTo(_chars.Slice(_pos)))
		{
			_pos += value.Length;
		}
		else
		{
			AppendFormattedSlow(value);
		}
		EndHole(in value, reprd: true);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AppendFormattedSlow(string? value)
	{
		if (value != null)
		{
			EnsureCapacityForAdditionalChars(value.Length);
			value.AsSpan().CopyTo(_chars.Slice(_pos));
			_pos += value.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted(string? value, int alignment = 0, string? format = null)
	{
		this.AppendFormatted<string>(value, alignment, format);
	}

	public void AppendFormatted(ReadOnlySpan<char> value)
	{
		BeginHole();
		if (value.TryCopyTo(_chars.Slice(_pos)))
		{
			_pos += value.Length;
		}
		else
		{
			GrowThenCopySpan(value);
		}
		EndHole(null, reprd: false);
	}

	public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
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
			AppendFormatted(value);
			return;
		}
		BeginHole();
		EnsureCapacityForAdditionalChars(value.Length + num);
		if (flag)
		{
			value.CopyTo(_chars.Slice(_pos));
			_pos += value.Length;
			_chars.Slice(_pos, num).Fill(' ');
			_pos += num;
		}
		else
		{
			_chars.Slice(_pos, num).Fill(' ');
			_pos += num;
			value.CopyTo(_chars.Slice(_pos));
			_pos += value.Length;
		}
		EndHole(null, reprd: false);
	}

	public void AppendFormatted<T>(T value)
	{
		if (typeof(T) == typeof(IntPtr))
		{
			AppendFormatted(Unsafe.As<T, IntPtr>(ref value));
			return;
		}
		if (typeof(T) == typeof(UIntPtr))
		{
			AppendFormatted(Unsafe.As<T, UIntPtr>(ref value));
			return;
		}
		BeginHole();
		if (DebugFormatter.CanDebugFormat(in value, out object extraData))
		{
			int wrote;
			while (!DebugFormatter.TryFormatInto(in value, extraData, _chars.Slice(_pos), out wrote))
			{
				Grow();
			}
			_pos += wrote;
			return;
		}
		string text = ((!(value is IFormattable)) ? value?.ToString() : ((IFormattable)(object)value).ToString(null, null));
		if (text != null)
		{
			AppendStringDirect(text);
		}
		EndHole(in value, reprd: true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AppendFormatted(IntPtr value)
	{
		if (IntPtr.Size == 4)
		{
			AppendFormatted((int)value);
		}
		else
		{
			AppendFormatted((long)value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AppendFormatted(IntPtr value, string? format)
	{
		if (IntPtr.Size == 4)
		{
			AppendFormatted((int)value, format);
		}
		else
		{
			AppendFormatted((long)value, format);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AppendFormatted(UIntPtr value)
	{
		if (UIntPtr.Size == 4)
		{
			AppendFormatted((uint)value);
		}
		else
		{
			AppendFormatted((ulong)value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AppendFormatted(UIntPtr value, string? format)
	{
		if (UIntPtr.Size == 4)
		{
			AppendFormatted((uint)value, format);
		}
		else
		{
			AppendFormatted((ulong)value, format);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value, int alignment)
	{
		int pos = _pos;
		AppendFormatted(value);
		if (alignment != 0)
		{
			AppendOrInsertAlignmentIfNeeded(pos, alignment);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value, string? format)
	{
		if (typeof(T) == typeof(IntPtr))
		{
			AppendFormatted(Unsafe.As<T, IntPtr>(ref value), format);
			return;
		}
		if (typeof(T) == typeof(UIntPtr))
		{
			AppendFormatted(Unsafe.As<T, UIntPtr>(ref value), format);
			return;
		}
		BeginHole();
		if (DebugFormatter.CanDebugFormat(in value, out object extraData))
		{
			int wrote;
			while (!DebugFormatter.TryFormatInto(in value, extraData, _chars.Slice(_pos), out wrote))
			{
				Grow();
			}
			_pos += wrote;
			return;
		}
		string text = ((!(value is IFormattable)) ? value?.ToString() : ((IFormattable)(object)value).ToString(format, null));
		if (text != null)
		{
			AppendStringDirect(text);
		}
		EndHole(in value, reprd: true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value, int alignment, string? format)
	{
		int pos = _pos;
		AppendFormatted(value, format);
		if (alignment != 0)
		{
			AppendOrInsertAlignmentIfNeeded(pos, alignment);
		}
	}

	private void AppendOrInsertAlignmentIfNeeded(int startingPos, int alignment)
	{
		int num = _pos - startingPos;
		bool flag = false;
		if (alignment < 0)
		{
			flag = true;
			alignment = -alignment;
		}
		int num2 = alignment - num;
		if (num2 > 0)
		{
			EnsureCapacityForAdditionalChars(num2);
			if (flag)
			{
				_chars.Slice(_pos, num2).Fill(' ');
			}
			else
			{
				_chars.Slice(startingPos, num).CopyTo(_chars.Slice(startingPos + num2));
				_chars.Slice(startingPos, num2).Fill(' ');
			}
			_pos += num2;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureCapacityForAdditionalChars(int additionalChars)
	{
		if (_chars.Length - _pos < additionalChars)
		{
			Grow(additionalChars);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowThenCopyString(string value)
	{
		Grow(value.Length);
		value.AsSpan().CopyTo(_chars.Slice(_pos));
		_pos += value.Length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowThenCopySpan(ReadOnlySpan<char> value)
	{
		Grow(value.Length);
		value.CopyTo(_chars.Slice(_pos));
		_pos += value.Length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow(int additionalChars)
	{
		GrowCore((uint)(_pos + additionalChars));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow()
	{
		GrowCore((uint)(_chars.Length + 1));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GrowCore(uint requiredMinCapacity)
	{
		int minimumLength = (int)MathEx.Clamp(Math.Max(requiredMinCapacity, Math.Min((uint)(_chars.Length * 2), uint.MaxValue)), 256u, 2147483647u);
		char[] array = ArrayPool<char>.Shared.Rent(minimumLength);
		_chars.Slice(0, _pos).CopyTo(array);
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		_chars = (_arrayToReturnToPool = array);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}
}
