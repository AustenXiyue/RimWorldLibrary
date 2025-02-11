namespace System.Windows.Documents;

internal class ListTableEntry
{
	private long _id;

	private long _templateID;

	private bool _simple;

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

	internal long TemplateID
	{
		set
		{
			_templateID = value;
		}
	}

	internal bool Simple
	{
		set
		{
			_simple = value;
		}
	}

	internal ListLevelTable Levels => _levels;

	internal ListTableEntry()
	{
		_id = 0L;
		_templateID = 0L;
		_levels = new ListLevelTable();
	}
}
