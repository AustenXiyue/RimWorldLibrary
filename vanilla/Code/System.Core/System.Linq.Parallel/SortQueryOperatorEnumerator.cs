namespace System.Linq.Parallel;

internal class SortQueryOperatorEnumerator<TInputOutput, TKey, TSortKey> : QueryOperatorEnumerator<TInputOutput, TSortKey>
{
	private readonly QueryOperatorEnumerator<TInputOutput, TKey> _source;

	private readonly Func<TInputOutput, TSortKey> _keySelector;

	internal SortQueryOperatorEnumerator(QueryOperatorEnumerator<TInputOutput, TKey> source, Func<TInputOutput, TSortKey> keySelector)
	{
		_source = source;
		_keySelector = keySelector;
	}

	internal override bool MoveNext(ref TInputOutput currentElement, ref TSortKey currentKey)
	{
		TKey currentKey2 = default(TKey);
		if (!_source.MoveNext(ref currentElement, ref currentKey2))
		{
			return false;
		}
		currentKey = _keySelector(currentElement);
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		_source.Dispose();
	}
}
