namespace MS.Internal.Documents;

internal class RowCacheChange
{
	private readonly int _start;

	private readonly int _count;

	public int Start => _start;

	public int Count => _count;

	public RowCacheChange(int start, int count)
	{
		_start = start;
		_count = count;
	}
}
