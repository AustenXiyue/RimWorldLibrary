using System;

namespace MS.Internal.Generic;

internal struct SpanRider<T>
{
	private const int MaxCch = int.MaxValue;

	private SpanVector<T> _vector;

	private Span<T> _defaultSpan;

	private int _current;

	private int _cp;

	private int _dcp;

	private int _cch;

	internal int CurrentSpanStart => _dcp;

	internal int Length => _cch;

	internal int CurrentPosition => _cp;

	internal T CurrentValue
	{
		get
		{
			if (_current < _vector.Count)
			{
				return _vector[_current].Value;
			}
			return _defaultSpan.Value;
		}
	}

	internal SpanRider(SpanVector<T> vector)
	{
		_defaultSpan = new Span<T>(vector.DefaultValue, int.MaxValue);
		_vector = vector;
		_current = 0;
		_cp = 0;
		_dcp = 0;
		_cch = 0;
		At(0);
	}

	internal bool At(int cp)
	{
		if (cp < _dcp)
		{
			_cp = (_dcp = (_current = (_cch = 0)));
		}
		Span<T> span = default(Span<T>);
		while (_current < _vector.Count)
		{
			int dcp = _dcp;
			Span<T> span2 = (span = _vector[_current]);
			if (dcp + span2.Length > cp)
			{
				break;
			}
			_dcp += span.Length;
			_current++;
		}
		if (_current < _vector.Count)
		{
			_cch = _vector[_current].Length - cp + _dcp;
			_cp = cp;
			return true;
		}
		_cch = _defaultSpan.Length;
		_cp = Math.Min(cp, _dcp);
		return false;
	}
}
