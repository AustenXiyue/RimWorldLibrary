using System.Globalization;
using System.Text;

namespace System.Windows.Documents;

internal class CellFormat
{
	private long _cb;

	private long _cf;

	private long _nShading;

	private long _padT;

	private long _padB;

	private long _padR;

	private long _padL;

	private long _spaceT;

	private long _spaceB;

	private long _spaceR;

	private long _spaceL;

	private long _nCellX;

	private CellWidth _width;

	private VAlign _valign;

	private BorderFormat _brdL;

	private BorderFormat _brdR;

	private BorderFormat _brdT;

	private BorderFormat _brdB;

	private bool _fPending;

	private bool _fHMerge;

	private bool _fHMergeFirst;

	private bool _fVMerge;

	private bool _fVMergeFirst;

	private bool _fCellXSet;

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

	internal long Shading
	{
		get
		{
			return _nShading;
		}
		set
		{
			_nShading = Validators.MakeValidShading(value);
		}
	}

	internal long PaddingLeft
	{
		get
		{
			return _padL;
		}
		set
		{
			_padL = value;
		}
	}

	internal long PaddingRight
	{
		get
		{
			return _padR;
		}
		set
		{
			_padR = value;
		}
	}

	internal long PaddingTop
	{
		get
		{
			return _padT;
		}
		set
		{
			_padT = value;
		}
	}

	internal long PaddingBottom
	{
		get
		{
			return _padB;
		}
		set
		{
			_padB = value;
		}
	}

	internal BorderFormat BorderTop
	{
		get
		{
			return _brdT;
		}
		set
		{
			_brdT = value;
		}
	}

	internal BorderFormat BorderBottom
	{
		get
		{
			return _brdB;
		}
		set
		{
			_brdB = value;
		}
	}

	internal BorderFormat BorderLeft
	{
		get
		{
			return _brdL;
		}
		set
		{
			_brdL = value;
		}
	}

	internal BorderFormat BorderRight
	{
		get
		{
			return _brdR;
		}
		set
		{
			_brdR = value;
		}
	}

	internal CellWidth Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
		}
	}

	internal long CellX
	{
		get
		{
			return _nCellX;
		}
		set
		{
			_nCellX = value;
			_fCellXSet = true;
		}
	}

	internal bool IsCellXSet
	{
		get
		{
			return _fCellXSet;
		}
		set
		{
			_fCellXSet = value;
		}
	}

	internal VAlign VAlign
	{
		set
		{
			_valign = value;
		}
	}

	internal long SpacingTop
	{
		get
		{
			return _spaceT;
		}
		set
		{
			_spaceT = value;
		}
	}

	internal long SpacingLeft
	{
		get
		{
			return _spaceL;
		}
		set
		{
			_spaceL = value;
		}
	}

	internal long SpacingBottom
	{
		get
		{
			return _spaceB;
		}
		set
		{
			_spaceB = value;
		}
	}

	internal long SpacingRight
	{
		get
		{
			return _spaceR;
		}
		set
		{
			_spaceR = value;
		}
	}

	internal bool IsPending
	{
		get
		{
			return _fPending;
		}
		set
		{
			_fPending = value;
		}
	}

	internal bool IsHMerge
	{
		get
		{
			return _fHMerge;
		}
		set
		{
			_fHMerge = value;
		}
	}

	internal bool IsHMergeFirst
	{
		get
		{
			return _fHMergeFirst;
		}
		set
		{
			_fHMergeFirst = value;
		}
	}

	internal bool IsVMerge
	{
		get
		{
			return _fVMerge;
		}
		set
		{
			_fVMerge = value;
		}
	}

	internal bool IsVMergeFirst
	{
		get
		{
			return _fVMergeFirst;
		}
		set
		{
			_fVMergeFirst = value;
		}
	}

	internal bool HasBorder
	{
		get
		{
			if (BorderLeft.EffectiveWidth <= 0 && BorderRight.EffectiveWidth <= 0 && BorderTop.EffectiveWidth <= 0)
			{
				return BorderBottom.EffectiveWidth > 0;
			}
			return true;
		}
	}

	internal string RTFEncodingForWidth
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("\\clftsWidth");
			stringBuilder.Append(((int)Width.Type).ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append("\\clwWidth");
			stringBuilder.Append(Width.Value.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append("\\cellx");
			stringBuilder.Append(CellX.ToString(CultureInfo.InvariantCulture));
			return stringBuilder.ToString();
		}
	}

	internal CellFormat()
	{
		BorderLeft = new BorderFormat();
		BorderRight = new BorderFormat();
		BorderBottom = new BorderFormat();
		BorderTop = new BorderFormat();
		Width = new CellWidth();
		SetDefaults();
		IsPending = true;
	}

	internal CellFormat(CellFormat cf)
	{
		CellX = cf.CellX;
		IsCellXSet = cf.IsCellXSet;
		Width = new CellWidth(cf.Width);
		CB = cf.CB;
		CF = cf.CF;
		Shading = cf.Shading;
		PaddingTop = cf.PaddingTop;
		PaddingBottom = cf.PaddingBottom;
		PaddingRight = cf.PaddingRight;
		PaddingLeft = cf.PaddingLeft;
		BorderLeft = new BorderFormat(cf.BorderLeft);
		BorderRight = new BorderFormat(cf.BorderRight);
		BorderBottom = new BorderFormat(cf.BorderBottom);
		BorderTop = new BorderFormat(cf.BorderTop);
		SpacingTop = cf.SpacingTop;
		SpacingBottom = cf.SpacingBottom;
		SpacingRight = cf.SpacingRight;
		SpacingLeft = cf.SpacingLeft;
		VAlign = VAlign.AlignTop;
		IsPending = true;
		IsHMerge = cf.IsHMerge;
		IsHMergeFirst = cf.IsHMergeFirst;
		IsVMerge = cf.IsVMerge;
		IsVMergeFirst = cf.IsVMergeFirst;
	}

	internal void SetDefaults()
	{
		CellX = -1L;
		IsCellXSet = false;
		Width.SetDefaults();
		CB = -1L;
		CF = -1L;
		Shading = -1L;
		PaddingTop = 0L;
		PaddingBottom = 0L;
		PaddingRight = 0L;
		PaddingLeft = 0L;
		BorderLeft.SetDefaults();
		BorderRight.SetDefaults();
		BorderBottom.SetDefaults();
		BorderTop.SetDefaults();
		SpacingTop = 0L;
		SpacingBottom = 0L;
		SpacingRight = 0L;
		SpacingLeft = 0L;
		VAlign = VAlign.AlignTop;
		IsHMerge = false;
		IsHMergeFirst = false;
		IsVMerge = false;
		IsVMergeFirst = false;
	}

	internal string GetBorderAttributeString(ConverterState converterState)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(" BorderThickness=\"");
		stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderLeft.EffectiveWidth));
		stringBuilder.Append(',');
		stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderTop.EffectiveWidth));
		stringBuilder.Append(',');
		stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderRight.EffectiveWidth));
		stringBuilder.Append(',');
		stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderBottom.EffectiveWidth));
		stringBuilder.Append('"');
		ColorTableEntry colorTableEntry = null;
		if (BorderLeft.CF >= 0)
		{
			colorTableEntry = converterState.ColorTable.EntryAt((int)BorderLeft.CF);
		}
		if (colorTableEntry != null)
		{
			stringBuilder.Append(" BorderBrush=\"");
			stringBuilder.Append(colorTableEntry.Color.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append('"');
		}
		else
		{
			stringBuilder.Append(" BorderBrush=\"#FF000000\"");
		}
		return stringBuilder.ToString();
	}

	internal string GetPaddingAttributeString()
	{
		return $" Padding=\"{Converters.TwipToPositivePxString(PaddingLeft)},{Converters.TwipToPositivePxString(PaddingTop)},{Converters.TwipToPositivePxString(PaddingRight)},{Converters.TwipToPositivePxString(PaddingBottom)}\"";
	}
}
