namespace MS.Internal;

internal struct SpanRider
{
	private SpanVector _spans;

	private SpanPosition _spanPosition;

	private int _cp;

	private int _cch;

	public int CurrentSpanStart => _spanPosition.CP;

	public int Length => _cch;

	public int CurrentPosition => _cp;

	public object CurrentElement
	{
		get
		{
			if (_spanPosition.Index < _spans.Count)
			{
				return _spans[_spanPosition.Index].element;
			}
			return _spans.Default;
		}
	}

	public int CurrentSpanIndex => _spanPosition.Index;

	public SpanPosition SpanPosition => _spanPosition;

	public SpanRider(SpanVector spans)
		: this(spans, default(SpanPosition), 0)
	{
	}

	public SpanRider(SpanVector spans, SpanPosition latestPosition)
		: this(spans, latestPosition, latestPosition.CP)
	{
	}

	public SpanRider(SpanVector spans, SpanPosition latestPosition, int cp)
	{
		_spans = spans;
		_spanPosition = default(SpanPosition);
		_cp = 0;
		_cch = 0;
		At(latestPosition, cp);
	}

	public bool At(int cp)
	{
		return At(_spanPosition, cp);
	}

	public bool At(SpanPosition latestPosition, int cp)
	{
		bool num = _spans.FindSpan(cp, latestPosition, out _spanPosition);
		if (num)
		{
			_cch = _spans[_spanPosition.Index].length - (cp - _spanPosition.CP);
			_cp = cp;
			return num;
		}
		_cch = int.MaxValue;
		_cp = _spanPosition.CP;
		return num;
	}
}
