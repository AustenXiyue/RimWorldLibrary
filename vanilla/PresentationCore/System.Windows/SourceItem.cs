namespace System.Windows;

internal struct SourceItem
{
	private int _startIndex;

	private object _source;

	internal int StartIndex => _startIndex;

	internal object Source => _source;

	internal SourceItem(int startIndex, object source)
	{
		_startIndex = startIndex;
		_source = source;
	}

	public override bool Equals(object o)
	{
		return Equals((SourceItem)o);
	}

	public bool Equals(SourceItem sourceItem)
	{
		if (sourceItem._startIndex == _startIndex)
		{
			return sourceItem._source == _source;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(SourceItem sourceItem1, SourceItem sourceItem2)
	{
		return sourceItem1.Equals(sourceItem2);
	}

	public static bool operator !=(SourceItem sourceItem1, SourceItem sourceItem2)
	{
		return !sourceItem1.Equals(sourceItem2);
	}
}
