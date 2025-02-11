namespace System.Windows.Documents;

internal class ListOverride
{
	private long _id;

	private long _index;

	private long _nStartIndex;

	private ListLevelTable _levels;

	internal long ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	internal long Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	internal ListLevelTable Levels
	{
		get
		{
			return _levels;
		}
		set
		{
			_levels = value;
		}
	}

	internal long StartIndex
	{
		get
		{
			return _nStartIndex;
		}
		set
		{
			_nStartIndex = value;
		}
	}

	internal ListOverride()
	{
		_id = 0L;
		_index = 0L;
		_levels = null;
		_nStartIndex = -1L;
	}
}
