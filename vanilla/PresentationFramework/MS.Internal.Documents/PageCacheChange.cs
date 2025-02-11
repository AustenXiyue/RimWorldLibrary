namespace MS.Internal.Documents;

internal class PageCacheChange
{
	private readonly int _start;

	private readonly int _count;

	private readonly PageCacheChangeType _type;

	public int Start => _start;

	public int Count => _count;

	public PageCacheChangeType Type => _type;

	public PageCacheChange(int start, int count, PageCacheChangeType type)
	{
		_start = start;
		_count = count;
		_type = type;
	}
}
