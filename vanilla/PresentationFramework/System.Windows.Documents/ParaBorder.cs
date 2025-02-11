using System.Globalization;
using System.Text;

namespace System.Windows.Documents;

internal class ParaBorder
{
	private BorderFormat _bfLeft;

	private BorderFormat _bfTop;

	private BorderFormat _bfRight;

	private BorderFormat _bfBottom;

	private BorderFormat _bfAll;

	private long _nSpacing;

	internal BorderFormat BorderLeft
	{
		get
		{
			return _bfLeft;
		}
		set
		{
			_bfLeft = value;
		}
	}

	internal BorderFormat BorderTop
	{
		get
		{
			return _bfTop;
		}
		set
		{
			_bfTop = value;
		}
	}

	internal BorderFormat BorderRight
	{
		get
		{
			return _bfRight;
		}
		set
		{
			_bfRight = value;
		}
	}

	internal BorderFormat BorderBottom
	{
		get
		{
			return _bfBottom;
		}
		set
		{
			_bfBottom = value;
		}
	}

	internal BorderFormat BorderAll
	{
		get
		{
			return _bfAll;
		}
		set
		{
			_bfAll = value;
		}
	}

	internal long Spacing
	{
		get
		{
			return _nSpacing;
		}
		set
		{
			_nSpacing = value;
		}
	}

	internal long CF
	{
		get
		{
			return BorderLeft.CF;
		}
		set
		{
			BorderLeft.CF = value;
			BorderTop.CF = value;
			BorderRight.CF = value;
			BorderBottom.CF = value;
			BorderAll.CF = value;
		}
	}

	internal bool IsNone
	{
		get
		{
			if (BorderLeft.IsNone && BorderTop.IsNone && BorderRight.IsNone && BorderBottom.IsNone)
			{
				return BorderAll.IsNone;
			}
			return false;
		}
	}

	internal string RTFEncoding
	{
		get
		{
			if (IsNone)
			{
				return "\\brdrnil";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("\\brdrl");
			stringBuilder.Append(BorderLeft.RTFEncoding);
			if (BorderLeft.CF >= 0)
			{
				stringBuilder.Append("\\brdrcf");
				stringBuilder.Append(BorderLeft.CF.ToString(CultureInfo.InvariantCulture));
			}
			stringBuilder.Append("\\brdrt");
			stringBuilder.Append(BorderTop.RTFEncoding);
			if (BorderTop.CF >= 0)
			{
				stringBuilder.Append("\\brdrcf");
				stringBuilder.Append(BorderTop.CF.ToString(CultureInfo.InvariantCulture));
			}
			stringBuilder.Append("\\brdrr");
			stringBuilder.Append(BorderRight.RTFEncoding);
			if (BorderRight.CF >= 0)
			{
				stringBuilder.Append("\\brdrcf");
				stringBuilder.Append(BorderRight.CF.ToString(CultureInfo.InvariantCulture));
			}
			stringBuilder.Append("\\brdrb");
			stringBuilder.Append(BorderBottom.RTFEncoding);
			if (BorderBottom.CF >= 0)
			{
				stringBuilder.Append("\\brdrcf");
				stringBuilder.Append(BorderBottom.CF.ToString(CultureInfo.InvariantCulture));
			}
			stringBuilder.Append("\\brsp");
			stringBuilder.Append(Spacing.ToString(CultureInfo.InvariantCulture));
			return stringBuilder.ToString();
		}
	}

	internal ParaBorder()
	{
		BorderLeft = new BorderFormat();
		BorderTop = new BorderFormat();
		BorderRight = new BorderFormat();
		BorderBottom = new BorderFormat();
		BorderAll = new BorderFormat();
		Spacing = 0L;
	}

	internal ParaBorder(ParaBorder pb)
	{
		BorderLeft = new BorderFormat(pb.BorderLeft);
		BorderTop = new BorderFormat(pb.BorderTop);
		BorderRight = new BorderFormat(pb.BorderRight);
		BorderBottom = new BorderFormat(pb.BorderBottom);
		BorderAll = new BorderFormat(pb.BorderAll);
		Spacing = pb.Spacing;
	}

	internal string GetBorderAttributeString(ConverterState converterState)
	{
		if (IsNone)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(" BorderThickness=\"");
		if (!BorderAll.IsNone)
		{
			stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderAll.EffectiveWidth));
		}
		else
		{
			stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderLeft.EffectiveWidth));
			stringBuilder.Append(',');
			stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderTop.EffectiveWidth));
			stringBuilder.Append(',');
			stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderRight.EffectiveWidth));
			stringBuilder.Append(',');
			stringBuilder.Append(Converters.TwipToPositiveVisiblePxString(BorderBottom.EffectiveWidth));
		}
		stringBuilder.Append('"');
		ColorTableEntry colorTableEntry = null;
		if (CF >= 0)
		{
			colorTableEntry = converterState.ColorTable.EntryAt((int)CF);
		}
		if (colorTableEntry != null)
		{
			stringBuilder.Append(" BorderBrush=\"");
			stringBuilder.Append(colorTableEntry.Color.ToString());
			stringBuilder.Append('"');
		}
		else
		{
			stringBuilder.Append(" BorderBrush=\"#FF000000\"");
		}
		if (Spacing != 0L)
		{
			stringBuilder.Append(" Padding=\"");
			stringBuilder.Append(Converters.TwipToPositivePxString(Spacing));
			stringBuilder.Append('"');
		}
		return stringBuilder.ToString();
	}
}
