namespace MS.Internal;

internal struct SpanPosition
{
	private int _spanIndex;

	private int _spanCP;

	internal int Index => _spanIndex;

	internal int CP => _spanCP;

	internal SpanPosition(int spanIndex, int spanCP)
	{
		_spanIndex = spanIndex;
		_spanCP = spanCP;
	}
}
