namespace System.Windows.Documents;

internal class ConverterState
{
	private RtfFormatStack _rtfFormatStack;

	private DocumentNodeArray _documentNodeArray;

	private FontTable _fontTable;

	private ColorTable _colorTable;

	private ListTable _listTable;

	private ListOverrideTable _listOverrideTable;

	private long _defaultFont;

	private long _defaultLang;

	private long _defaultLangFE;

	private int _codePage;

	private bool _bMarkerWhiteSpace;

	private bool _bMarkerPresent;

	private BorderFormat _border;

	internal RtfFormatStack RtfFormatStack => _rtfFormatStack;

	internal FontTable FontTable => _fontTable;

	internal ColorTable ColorTable => _colorTable;

	internal ListTable ListTable => _listTable;

	internal ListOverrideTable ListOverrideTable => _listOverrideTable;

	internal DocumentNodeArray DocumentNodeArray => _documentNodeArray;

	internal FormatState TopFormatState => _rtfFormatStack.Top();

	internal int CodePage
	{
		get
		{
			return _codePage;
		}
		set
		{
			_codePage = value;
		}
	}

	internal long DefaultFont
	{
		get
		{
			return _defaultFont;
		}
		set
		{
			_defaultFont = value;
		}
	}

	internal long DefaultLang
	{
		get
		{
			return _defaultLang;
		}
		set
		{
			_defaultLang = value;
		}
	}

	internal long DefaultLangFE
	{
		get
		{
			return _defaultLangFE;
		}
		set
		{
			_defaultLangFE = value;
		}
	}

	internal bool IsMarkerWhiteSpace
	{
		get
		{
			return _bMarkerWhiteSpace;
		}
		set
		{
			_bMarkerWhiteSpace = value;
		}
	}

	internal bool IsMarkerPresent
	{
		get
		{
			return _bMarkerPresent;
		}
		set
		{
			_bMarkerPresent = value;
		}
	}

	internal BorderFormat CurrentBorder
	{
		get
		{
			return _border;
		}
		set
		{
			_border = value;
		}
	}

	internal ConverterState()
	{
		_rtfFormatStack = new RtfFormatStack();
		_documentNodeArray = new DocumentNodeArray();
		_documentNodeArray.IsMain = true;
		_fontTable = new FontTable();
		_colorTable = new ColorTable();
		_listTable = new ListTable();
		_listOverrideTable = new ListOverrideTable();
		_defaultFont = -1L;
		_defaultLang = -1L;
		_defaultLangFE = -1L;
		_bMarkerWhiteSpace = false;
		_bMarkerPresent = false;
		_border = null;
	}

	internal FormatState PreviousTopFormatState(int fromTop)
	{
		return _rtfFormatStack.PrevTop(fromTop);
	}
}
