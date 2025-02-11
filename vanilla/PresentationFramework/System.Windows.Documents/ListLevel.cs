namespace System.Windows.Documents;

internal class ListLevel
{
	private long _nStartIndex;

	private MarkerStyle _numberType;

	private FormatState _formatState;

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

	internal MarkerStyle Marker
	{
		get
		{
			return _numberType;
		}
		set
		{
			_numberType = value;
		}
	}

	internal FormatState FormatState
	{
		set
		{
			_formatState = value;
		}
	}

	internal ListLevel()
	{
		_nStartIndex = 1L;
		_numberType = MarkerStyle.MarkerArabic;
	}
}
