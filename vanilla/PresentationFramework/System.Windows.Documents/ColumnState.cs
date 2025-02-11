namespace System.Windows.Documents;

internal class ColumnState
{
	private long _nCellX;

	private DocumentNode _row;

	private bool _fFilled;

	internal long CellX
	{
		get
		{
			return _nCellX;
		}
		set
		{
			_nCellX = value;
		}
	}

	internal DocumentNode Row
	{
		get
		{
			return _row;
		}
		set
		{
			_row = value;
		}
	}

	internal bool IsFilled
	{
		get
		{
			return _fFilled;
		}
		set
		{
			_fFilled = value;
		}
	}

	internal ColumnState()
	{
		_nCellX = 0L;
		_row = null;
		_fFilled = false;
	}
}
