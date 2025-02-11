using System.Buffers;
using System.Runtime.CompilerServices;

namespace System.Text;

internal ref struct ValueStringBuilder
{
	private char[] _arrayToReturnToPool;

	private Span<char> _chars;

	private int _pos;

	public int Length => _pos;

	public ref char this[int index] => ref _chars[index];

	public ValueStringBuilder(Span<char> initialBuffer)
	{
		_arrayToReturnToPool = null;
		_chars = initialBuffer;
		_pos = 0;
	}

	public void EnsureCapacity(int capacity)
	{
		if ((uint)capacity > (uint)_chars.Length)
		{
			Grow(capacity - _pos);
		}
	}

	public override string ToString()
	{
		string result = _chars.Slice(0, _pos).ToString();
		Dispose();
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(char c)
	{
		int pos = _pos;
		Span<char> chars = _chars;
		if ((uint)pos < (uint)chars.Length)
		{
			chars[pos] = c;
			_pos = pos + 1;
		}
		else
		{
			GrowAndAppend(c);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string s)
	{
		if (s != null)
		{
			int pos = _pos;
			if (s.Length == 1 && (uint)pos < (uint)_chars.Length)
			{
				_chars[pos] = s[0];
				_pos = pos + 1;
			}
			else
			{
				AppendSlow(s);
			}
		}
	}

	private void AppendSlow(string s)
	{
		int pos = _pos;
		if (pos > _chars.Length - s.Length)
		{
			Grow(s.Length);
		}
		s.CopyTo(_chars.Slice(pos));
		_pos += s.Length;
	}

	public void Append(ReadOnlySpan<char> value)
	{
		int pos = _pos;
		if (pos > _chars.Length - value.Length)
		{
			Grow(value.Length);
		}
		value.CopyTo(_chars.Slice(_pos));
		_pos += value.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		int pos = _pos;
		if (pos > _chars.Length - length)
		{
			Grow(length);
		}
		_pos = pos + length;
		return _chars.Slice(pos, length);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowAndAppend(char c)
	{
		Grow(1);
		Append(c);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow(int additionalCapacityBeyondPos)
	{
		int minimumLength = (int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), Math.Min((uint)(_chars.Length * 2), 2147483591u));
		char[] array = ArrayPool<char>.Shared.Rent(minimumLength);
		_chars.Slice(0, _pos).CopyTo(array);
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		_chars = (_arrayToReturnToPool = array);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		this = default(System.Text.ValueStringBuilder);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}

	internal void AppendSpanFormattable<T>(T value, string format = null, IFormatProvider provider = null) where T : ISpanFormattable
	{
		ref T reference = ref value;
		T val = default(T);
		if (val == null)
		{
			val = reference;
			reference = ref val;
		}
		if (reference.TryFormat(_chars.Slice(_pos), out var charsWritten, format, provider))
		{
			_pos += charsWritten;
		}
		else
		{
			Append(value.ToString(format, provider));
		}
	}
}
