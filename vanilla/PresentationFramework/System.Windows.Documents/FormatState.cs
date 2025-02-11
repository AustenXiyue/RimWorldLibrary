namespace System.Windows.Documents;

internal class FormatState
{
	private RtfDestination _dest;

	private bool _fBold;

	private bool _fItalic;

	private bool _fSuper;

	private bool _fSub;

	private bool _fOutline;

	private bool _fEngrave;

	private bool _fShadow;

	private bool _fScaps;

	private long _fs;

	private long _font;

	private int _codePage;

	private long _superOffset;

	private long _cf;

	private long _cb;

	private DirState _dirChar;

	private ULState _ul;

	private StrikeState _strike;

	private long _expand;

	private long _lang;

	private long _langFE;

	private long _langCur;

	private FontSlot _fontSlot;

	private long _sa;

	private long _sb;

	private long _li;

	private long _ri;

	private long _fi;

	private HAlign _align;

	private long _ils;

	private long _ilvl;

	private long _pnlvl;

	private long _itap;

	private DirState _dirPara;

	private long _cfPara;

	private long _cbPara;

	private long _nParaShading;

	private MarkerStyle _marker;

	private bool _fContinue;

	private long _nStartIndex;

	private long _nStartIndexDefault;

	private long _sl;

	private bool _slMult;

	private ParaBorder _pb;

	private bool _fInTable;

	private bool _fHidden;

	private int _stateSkip;

	private RowFormat _rowFormat;

	private static FormatState _fsEmptyState;

	private RtfImageFormat _imageFormat;

	private string _imageSource;

	private double _imageWidth;

	private double _imageHeight;

	private double _imageBaselineOffset;

	private bool _isIncludeImageBaselineOffset;

	private double _imageScaleWidth;

	private double _imageScaleHeight;

	private bool _isImageDataBinary;

	private string _imageStretch;

	private string _imageStretchDirection;

	private const int MAX_LIST_DEPTH = 32;

	private const int MAX_TABLE_DEPTH = 32;

	internal static FormatState EmptyFormatState
	{
		get
		{
			if (_fsEmptyState == null)
			{
				_fsEmptyState = new FormatState();
				_fsEmptyState.FontSize = -1L;
			}
			return _fsEmptyState;
		}
	}

	internal RtfDestination RtfDestination
	{
		get
		{
			return _dest;
		}
		set
		{
			_dest = value;
		}
	}

	internal bool IsHidden
	{
		get
		{
			return _fHidden;
		}
		set
		{
			_fHidden = value;
		}
	}

	internal bool IsContentDestination
	{
		get
		{
			if (_dest != 0 && _dest != RtfDestination.DestFieldResult && _dest != RtfDestination.DestShapeResult && _dest != RtfDestination.DestShape)
			{
				return _dest == RtfDestination.DestListText;
			}
			return true;
		}
	}

	internal bool Bold
	{
		get
		{
			return _fBold;
		}
		set
		{
			_fBold = value;
		}
	}

	internal bool Italic
	{
		get
		{
			return _fItalic;
		}
		set
		{
			_fItalic = value;
		}
	}

	internal bool Engrave
	{
		get
		{
			return _fEngrave;
		}
		set
		{
			_fEngrave = value;
		}
	}

	internal bool Shadow
	{
		get
		{
			return _fShadow;
		}
		set
		{
			_fShadow = value;
		}
	}

	internal bool SCaps
	{
		get
		{
			return _fScaps;
		}
		set
		{
			_fScaps = value;
		}
	}

	internal bool Outline
	{
		get
		{
			return _fOutline;
		}
		set
		{
			_fOutline = value;
		}
	}

	internal bool Sub
	{
		get
		{
			return _fSub;
		}
		set
		{
			_fSub = value;
		}
	}

	internal bool Super
	{
		get
		{
			return _fSuper;
		}
		set
		{
			_fSuper = value;
		}
	}

	internal long SuperOffset
	{
		get
		{
			return _superOffset;
		}
		set
		{
			_superOffset = value;
		}
	}

	internal long FontSize
	{
		get
		{
			return _fs;
		}
		set
		{
			_fs = value;
		}
	}

	internal long Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
		}
	}

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

	internal long CF
	{
		get
		{
			return _cf;
		}
		set
		{
			_cf = value;
		}
	}

	internal long CB
	{
		get
		{
			return _cb;
		}
		set
		{
			_cb = value;
		}
	}

	internal DirState DirChar
	{
		get
		{
			return _dirChar;
		}
		set
		{
			_dirChar = value;
		}
	}

	internal ULState UL
	{
		get
		{
			return _ul;
		}
		set
		{
			_ul = value;
		}
	}

	internal StrikeState Strike
	{
		get
		{
			return _strike;
		}
		set
		{
			_strike = value;
		}
	}

	internal long Expand
	{
		get
		{
			return _expand;
		}
		set
		{
			_expand = value;
		}
	}

	internal long Lang
	{
		get
		{
			return _lang;
		}
		set
		{
			_lang = value;
		}
	}

	internal long LangFE
	{
		get
		{
			return _langFE;
		}
		set
		{
			_langFE = value;
		}
	}

	internal long LangCur
	{
		get
		{
			return _langCur;
		}
		set
		{
			_langCur = value;
		}
	}

	internal FontSlot FontSlot
	{
		get
		{
			return _fontSlot;
		}
		set
		{
			_fontSlot = value;
		}
	}

	internal long SB
	{
		get
		{
			return _sb;
		}
		set
		{
			_sb = value;
		}
	}

	internal long SA
	{
		get
		{
			return _sa;
		}
		set
		{
			_sa = value;
		}
	}

	internal long FI
	{
		get
		{
			return _fi;
		}
		set
		{
			_fi = value;
		}
	}

	internal long RI
	{
		get
		{
			return _ri;
		}
		set
		{
			_ri = value;
		}
	}

	internal long LI
	{
		get
		{
			return _li;
		}
		set
		{
			_li = value;
		}
	}

	internal HAlign HAlign
	{
		get
		{
			return _align;
		}
		set
		{
			_align = value;
		}
	}

	internal long ILVL
	{
		get
		{
			return _ilvl;
		}
		set
		{
			if (value >= 0 && value <= 32)
			{
				_ilvl = value;
			}
		}
	}

	internal long PNLVL
	{
		get
		{
			return _pnlvl;
		}
		set
		{
			_pnlvl = value;
		}
	}

	internal long ITAP
	{
		get
		{
			return _itap;
		}
		set
		{
			if (value >= 0 && value <= 32)
			{
				_itap = value;
			}
		}
	}

	internal long ILS
	{
		get
		{
			return _ils;
		}
		set
		{
			_ils = value;
		}
	}

	internal DirState DirPara
	{
		get
		{
			return _dirPara;
		}
		set
		{
			_dirPara = value;
		}
	}

	internal long CFPara
	{
		get
		{
			return _cfPara;
		}
		set
		{
			_cfPara = value;
		}
	}

	internal long CBPara
	{
		get
		{
			return _cbPara;
		}
		set
		{
			_cbPara = value;
		}
	}

	internal long ParaShading
	{
		get
		{
			return _nParaShading;
		}
		set
		{
			_nParaShading = Validators.MakeValidShading(value);
		}
	}

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

	internal bool IsContinue
	{
		get
		{
			return _fContinue;
		}
		set
		{
			_fContinue = value;
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

	internal long SL
	{
		get
		{
			return _sl;
		}
		set
		{
			_sl = value;
		}
	}

	internal bool SLMult
	{
		get
		{
			return _slMult;
		}
		set
		{
			_slMult = value;
		}
	}

	internal bool IsInTable
	{
		get
		{
			return _fInTable;
		}
		set
		{
			_fInTable = value;
		}
	}

	internal long TableLevel
	{
		get
		{
			if (_fInTable || _itap > 0)
			{
				if (_itap <= 0)
				{
					return 1L;
				}
				return _itap;
			}
			return 0L;
		}
	}

	internal long ListLevel
	{
		get
		{
			if (_ils >= 0 || _ilvl > 0)
			{
				if (_ilvl <= 0)
				{
					return 1L;
				}
				return _ilvl + 1;
			}
			if (PNLVL > 0)
			{
				return PNLVL;
			}
			if (_marker != MarkerStyle.MarkerNone)
			{
				return 1L;
			}
			return 0L;
		}
	}

	internal int UnicodeSkip
	{
		get
		{
			return _stateSkip;
		}
		set
		{
			if (value >= 0 && value < 65535)
			{
				_stateSkip = value;
			}
		}
	}

	internal RowFormat RowFormat
	{
		get
		{
			if (_rowFormat == null)
			{
				_rowFormat = new RowFormat();
			}
			return _rowFormat;
		}
		set
		{
			_rowFormat = value;
		}
	}

	internal bool HasRowFormat => _rowFormat != null;

	internal ParaBorder ParaBorder
	{
		get
		{
			if (_pb == null)
			{
				_pb = new ParaBorder();
			}
			return _pb;
		}
	}

	internal bool HasParaBorder
	{
		get
		{
			if (_pb != null)
			{
				return !_pb.IsNone;
			}
			return false;
		}
	}

	internal RtfImageFormat ImageFormat
	{
		get
		{
			return _imageFormat;
		}
		set
		{
			_imageFormat = value;
		}
	}

	internal string ImageSource
	{
		get
		{
			return _imageSource;
		}
		set
		{
			_imageSource = value;
		}
	}

	internal double ImageWidth
	{
		get
		{
			return _imageWidth;
		}
		set
		{
			_imageWidth = value;
		}
	}

	internal double ImageHeight
	{
		get
		{
			return _imageHeight;
		}
		set
		{
			_imageHeight = value;
		}
	}

	internal double ImageBaselineOffset
	{
		get
		{
			return _imageBaselineOffset;
		}
		set
		{
			_imageBaselineOffset = value;
		}
	}

	internal bool IncludeImageBaselineOffset
	{
		get
		{
			return _isIncludeImageBaselineOffset;
		}
		set
		{
			_isIncludeImageBaselineOffset = value;
		}
	}

	internal double ImageScaleWidth
	{
		get
		{
			return _imageScaleWidth;
		}
		set
		{
			_imageScaleWidth = value;
		}
	}

	internal double ImageScaleHeight
	{
		get
		{
			return _imageScaleHeight;
		}
		set
		{
			_imageScaleHeight = value;
		}
	}

	internal bool IsImageDataBinary
	{
		get
		{
			return _isImageDataBinary;
		}
		set
		{
			_isImageDataBinary = value;
		}
	}

	internal string ImageStretch
	{
		get
		{
			return _imageStretch;
		}
		set
		{
			_imageStretch = value;
		}
	}

	internal string ImageStretchDirection
	{
		get
		{
			return _imageStretchDirection;
		}
		set
		{
			_imageStretchDirection = value;
		}
	}

	internal FormatState()
	{
		_dest = RtfDestination.DestNormal;
		_stateSkip = 1;
		SetCharDefaults();
		SetParaDefaults();
		SetRowDefaults();
	}

	internal FormatState(FormatState formatState)
	{
		Bold = formatState.Bold;
		Italic = formatState.Italic;
		Engrave = formatState.Engrave;
		Shadow = formatState.Shadow;
		SCaps = formatState.SCaps;
		Outline = formatState.Outline;
		Super = formatState.Super;
		Sub = formatState.Sub;
		SuperOffset = formatState.SuperOffset;
		FontSize = formatState.FontSize;
		Font = formatState.Font;
		CodePage = formatState.CodePage;
		CF = formatState.CF;
		CB = formatState.CB;
		DirChar = formatState.DirChar;
		UL = formatState.UL;
		Strike = formatState.Strike;
		Expand = formatState.Expand;
		Lang = formatState.Lang;
		LangFE = formatState.LangFE;
		LangCur = formatState.LangCur;
		FontSlot = formatState.FontSlot;
		SB = formatState.SB;
		SA = formatState.SA;
		FI = formatState.FI;
		RI = formatState.RI;
		LI = formatState.LI;
		SL = formatState.SL;
		SLMult = formatState.SLMult;
		HAlign = formatState.HAlign;
		ILVL = formatState.ILVL;
		ITAP = formatState.ITAP;
		ILS = formatState.ILS;
		DirPara = formatState.DirPara;
		CFPara = formatState.CFPara;
		CBPara = formatState.CBPara;
		ParaShading = formatState.ParaShading;
		Marker = formatState.Marker;
		IsContinue = formatState.IsContinue;
		StartIndex = formatState.StartIndex;
		StartIndexDefault = formatState.StartIndexDefault;
		IsInTable = formatState.IsInTable;
		_pb = (formatState.HasParaBorder ? new ParaBorder(formatState.ParaBorder) : null);
		RowFormat = formatState._rowFormat;
		RtfDestination = formatState.RtfDestination;
		IsHidden = formatState.IsHidden;
		_stateSkip = formatState.UnicodeSkip;
	}

	internal void SetCharDefaults()
	{
		_fBold = false;
		_fItalic = false;
		_fEngrave = false;
		_fShadow = false;
		_fScaps = false;
		_fOutline = false;
		_fSub = false;
		_fSuper = false;
		_superOffset = 0L;
		_fs = 24L;
		_font = -1L;
		_codePage = -1;
		_cf = -1L;
		_cb = -1L;
		_dirChar = DirState.DirLTR;
		_ul = ULState.ULNone;
		_strike = StrikeState.StrikeNone;
		_expand = 0L;
		_fHidden = false;
		_lang = -1L;
		_langFE = -1L;
		_langCur = -1L;
		_fontSlot = FontSlot.LOCH;
	}

	internal void SetParaDefaults()
	{
		_sb = 0L;
		_sa = 0L;
		_fi = 0L;
		_ri = 0L;
		_li = 0L;
		_align = HAlign.AlignDefault;
		_ilvl = 0L;
		_pnlvl = 0L;
		_itap = 0L;
		_ils = -1L;
		_dirPara = DirState.DirLTR;
		_cbPara = -1L;
		_nParaShading = -1L;
		_cfPara = -1L;
		_marker = MarkerStyle.MarkerNone;
		_fContinue = false;
		_nStartIndex = -1L;
		_nStartIndexDefault = -1L;
		_sl = 0L;
		_slMult = false;
		_pb = null;
		_fInTable = false;
	}

	internal void SetRowDefaults()
	{
		RowFormat = null;
	}

	internal bool IsEqual(FormatState formatState)
	{
		if (Bold == formatState.Bold && Italic == formatState.Italic && Engrave == formatState.Engrave && Shadow == formatState.Shadow && SCaps == formatState.SCaps && Outline == formatState.Outline && Super == formatState.Super && Sub == formatState.Sub && SuperOffset == formatState.SuperOffset && FontSize == formatState.FontSize && Font == formatState.Font && CodePage == formatState.CodePage && CF == formatState.CF && CB == formatState.CB && DirChar == formatState.DirChar && UL == formatState.UL && Strike == formatState.Strike && Expand == formatState.Expand && Lang == formatState.Lang && LangFE == formatState.LangFE && LangCur == formatState.LangCur && FontSlot == formatState.FontSlot && SB == formatState.SB && SA == formatState.SA && FI == formatState.FI && RI == formatState.RI && LI == formatState.LI && HAlign == formatState.HAlign && ILVL == formatState.ILVL && ITAP == formatState.ITAP && ILS == formatState.ILS && DirPara == formatState.DirPara && CFPara == formatState.CFPara && CBPara == formatState.CBPara && ParaShading == formatState.ParaShading && Marker == formatState.Marker && IsContinue == formatState.IsContinue && StartIndex == formatState.StartIndex && StartIndexDefault == formatState.StartIndexDefault && SL == formatState.SL && SLMult == formatState.SLMult && IsInTable == formatState.IsInTable && RtfDestination == formatState.RtfDestination && IsHidden == formatState.IsHidden)
		{
			return UnicodeSkip == formatState.UnicodeSkip;
		}
		return false;
	}

	internal string GetBorderAttributeString(ConverterState converterState)
	{
		if (HasParaBorder)
		{
			return ParaBorder.GetBorderAttributeString(converterState);
		}
		return string.Empty;
	}
}
