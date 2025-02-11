namespace System.Windows.Documents;

internal class MarkerListEntry
{
	private MarkerStyle _marker;

	private long _nStartIndexOverride;

	private long _nStartIndexDefault;

	private long _nVirtualListLevel;

	private long _nILS;

	internal MarkerStyle Marker
	{
		get
		{
			return _marker;
		}
		set
		{
			_marker = value;
		}
	}

	internal long StartIndexOverride
	{
		get
		{
			return _nStartIndexOverride;
		}
		set
		{
			_nStartIndexOverride = value;
		}
	}

	internal long StartIndexDefault
	{
		get
		{
			return _nStartIndexDefault;
		}
		set
		{
			_nStartIndexDefault = value;
		}
	}

	internal long VirtualListLevel
	{
		get
		{
			return _nVirtualListLevel;
		}
		set
		{
			_nVirtualListLevel = value;
		}
	}

	internal long StartIndexToUse
	{
		get
		{
			if (_nStartIndexOverride <= 0)
			{
				return _nStartIndexDefault;
			}
			return _nStartIndexOverride;
		}
	}

	internal long ILS
	{
		get
		{
			return _nILS;
		}
		set
		{
			_nILS = value;
		}
	}

	internal MarkerListEntry()
	{
		_marker = MarkerStyle.MarkerBullet;
		_nILS = -1L;
		_nStartIndexOverride = -1L;
		_nStartIndexDefault = -1L;
		_nVirtualListLevel = -1L;
	}
}
