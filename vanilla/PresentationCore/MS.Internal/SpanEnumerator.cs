using System.Collections;

namespace MS.Internal;

internal sealed class SpanEnumerator : IEnumerator
{
	private SpanVector _spans;

	private int _current;

	public object Current => _spans[_current];

	internal SpanEnumerator(SpanVector spans)
	{
		_spans = spans;
		_current = -1;
	}

	public bool MoveNext()
	{
		_current++;
		return _current < _spans.Count;
	}

	public void Reset()
	{
		_current = -1;
	}
}
