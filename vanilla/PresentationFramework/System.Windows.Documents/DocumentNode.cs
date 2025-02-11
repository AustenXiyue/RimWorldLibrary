using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

internal class DocumentNode
{
	internal static string[] HtmlNames = new string[12]
	{
		"", "", "span", "br", "a", "p", "ul", "li", "table", "tbody",
		"tr", "td"
	};

	internal static int[] HtmlLengths = new int[12]
	{
		0, 0, 4, 2, 1, 1, 2, 2, 5, 6,
		2, 2
	};

	internal static string[] XamlNames = new string[20]
	{
		"", "", "Span", "LineBreak", "Hyperlink", "Paragraph", "InlineUIContainer", "BlockUIContainer", "Image", "List",
		"ListItem", "Table", "TableRowGroup", "TableRow", "TableCell", "Section", "Figure", "Floater", "Field", "ListText"
	};

	private bool _bPending;

	private bool _bTerminated;

	private DocumentNodeType _type;

	private FormatState _formatState;

	private string _xaml;

	private StringBuilder _contentBuilder;

	private int _childCount;

	private int _index;

	private DocumentNode _parent;

	private DocumentNodeArray _dna;

	private ColumnStateArray _csa;

	private int _nRowSpan;

	private int _nColSpan;

	private string _sCustom;

	private long _nVirtualListLevel;

	private bool _bHasMarkerContent;

	private bool _bMatched;

	internal bool IsInline
	{
		get
		{
			if (_type != DocumentNodeType.dnText && _type != DocumentNodeType.dnInline && _type != DocumentNodeType.dnImage && _type != DocumentNodeType.dnLineBreak && _type != DocumentNodeType.dnListText)
			{
				return _type == DocumentNodeType.dnHyperlink;
			}
			return true;
		}
	}

	internal bool IsBlock
	{
		get
		{
			if (_type != DocumentNodeType.dnParagraph && _type != DocumentNodeType.dnList && _type != DocumentNodeType.dnListItem && _type != DocumentNodeType.dnTable && _type != DocumentNodeType.dnTableBody && _type != DocumentNodeType.dnRow && _type != DocumentNodeType.dnCell && _type != DocumentNodeType.dnSection && _type != DocumentNodeType.dnFigure)
			{
				return _type == DocumentNodeType.dnFloater;
			}
			return true;
		}
	}

	internal bool IsEmptyNode => _type == DocumentNodeType.dnLineBreak;

	internal bool IsHidden
	{
		get
		{
			if (_type != DocumentNodeType.dnFieldBegin && _type != DocumentNodeType.dnFieldEnd && _type != DocumentNodeType.dnShape)
			{
				return _type == DocumentNodeType.dnListText;
			}
			return true;
		}
	}

	internal bool IsWhiteSpace
	{
		get
		{
			if (IsTerminated)
			{
				return false;
			}
			if (_type == DocumentNodeType.dnText)
			{
				return Xaml.Trim().Length == 0;
			}
			return false;
		}
	}

	internal bool IsPending
	{
		get
		{
			if (Index >= 0)
			{
				return _bPending;
			}
			return false;
		}
		set
		{
			_bPending = value;
		}
	}

	internal bool IsTerminated
	{
		get
		{
			return _bTerminated;
		}
		set
		{
			_bTerminated = value;
		}
	}

	internal bool IsMatched
	{
		get
		{
			if (Type == DocumentNodeType.dnFieldBegin)
			{
				return _bMatched;
			}
			return true;
		}
		set
		{
			_bMatched = value;
		}
	}

	internal bool IsTrackedAsOpen
	{
		get
		{
			if (Index < 0)
			{
				return false;
			}
			if (Type == DocumentNodeType.dnFieldEnd)
			{
				return false;
			}
			if (IsPending && !IsTerminated)
			{
				return true;
			}
			if (!IsMatched)
			{
				return true;
			}
			return false;
		}
	}

	internal bool HasMarkerContent
	{
		get
		{
			return _bHasMarkerContent;
		}
		set
		{
			_bHasMarkerContent = value;
		}
	}

	internal bool IsNonEmpty
	{
		get
		{
			if (ChildCount <= 0)
			{
				return Xaml != null;
			}
			return true;
		}
	}

	internal string ListLabel
	{
		get
		{
			return _sCustom;
		}
		set
		{
			_sCustom = value;
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

	internal string NavigateUri
	{
		get
		{
			return _sCustom;
		}
		set
		{
			_sCustom = value;
		}
	}

	internal DocumentNodeType Type => _type;

	internal FormatState FormatState
	{
		get
		{
			return _formatState;
		}
		set
		{
			_formatState = value;
		}
	}

	internal FormatState ParentFormatStateForFont
	{
		get
		{
			DocumentNode parent = Parent;
			if (parent != null && parent.Type == DocumentNodeType.dnHyperlink)
			{
				parent = parent.Parent;
			}
			if (Type == DocumentNodeType.dnParagraph || parent == null)
			{
				return FormatState.EmptyFormatState;
			}
			return parent.FormatState;
		}
	}

	internal int ChildCount
	{
		get
		{
			return _childCount;
		}
		set
		{
			if (value >= 0)
			{
				_childCount = value;
			}
		}
	}

	internal int Index
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

	internal DocumentNodeArray DNA
	{
		get
		{
			return _dna;
		}
		set
		{
			_dna = value;
		}
	}

	internal int LastChildIndex => Index + ChildCount;

	internal DocumentNode ClosedParent => _parent;

	internal DocumentNode Parent
	{
		get
		{
			if (_parent == null && DNA != null)
			{
				return DNA.GetOpenParentWhileParsing(this);
			}
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	internal string Xaml
	{
		get
		{
			return _xaml;
		}
		set
		{
			_xaml = value;
		}
	}

	internal StringBuilder Content => _contentBuilder;

	internal int RowSpan
	{
		get
		{
			return _nRowSpan;
		}
		set
		{
			_nRowSpan = value;
		}
	}

	internal int ColSpan
	{
		get
		{
			return _nColSpan;
		}
		set
		{
			_nColSpan = value;
		}
	}

	internal ColumnStateArray ColumnStateArray
	{
		get
		{
			return _csa;
		}
		set
		{
			_csa = value;
		}
	}

	internal DirState XamlDir
	{
		get
		{
			if (IsInline)
			{
				return FormatState.DirChar;
			}
			if (Type == DocumentNodeType.dnTable)
			{
				if (FormatState.HasRowFormat)
				{
					return FormatState.RowFormat.Dir;
				}
				return ParentXamlDir;
			}
			if (Type == DocumentNodeType.dnList || Type == DocumentNodeType.dnParagraph)
			{
				return FormatState.DirPara;
			}
			for (DocumentNode parent = Parent; parent != null; parent = parent.Parent)
			{
				DocumentNodeType type = parent.Type;
				if (type == DocumentNodeType.dnParagraph || type == DocumentNodeType.dnList || type == DocumentNodeType.dnTable)
				{
					return parent.XamlDir;
				}
			}
			return DirState.DirLTR;
		}
	}

	internal DirState ParentXamlDir
	{
		get
		{
			if (Parent != null)
			{
				return Parent.XamlDir;
			}
			return DirState.DirLTR;
		}
	}

	internal bool RequiresXamlDir => XamlDir != ParentXamlDir;

	internal long NearMargin
	{
		get
		{
			if (ParentXamlDir != DirState.DirLTR)
			{
				return FormatState.RI;
			}
			return FormatState.LI;
		}
		set
		{
			if (ParentXamlDir == DirState.DirLTR)
			{
				FormatState.LI = value;
			}
			else
			{
				FormatState.RI = value;
			}
		}
	}

	internal long FarMargin
	{
		get
		{
			if (ParentXamlDir != DirState.DirLTR)
			{
				return FormatState.LI;
			}
			return FormatState.RI;
		}
	}

	internal DocumentNode(DocumentNodeType documentNodeType)
	{
		_type = documentNodeType;
		_bPending = true;
		_childCount = 0;
		_index = -1;
		_dna = null;
		_parent = null;
		_bTerminated = false;
		_bMatched = false;
		_bHasMarkerContent = false;
		_sCustom = null;
		_nRowSpan = 1;
		_nColSpan = 1;
		_nVirtualListLevel = -1L;
		_csa = null;
		_formatState = new FormatState();
		_contentBuilder = new StringBuilder();
	}

	internal void InheritFormatState(FormatState formatState)
	{
		_formatState = new FormatState(formatState);
		_formatState.LI = 0L;
		_formatState.RI = 0L;
		_formatState.SB = 0L;
		_formatState.SA = 0L;
		_formatState.FI = 0L;
		_formatState.Marker = MarkerStyle.MarkerNone;
		_formatState.CBPara = -1L;
	}

	internal string GetTagName()
	{
		return XamlNames[(int)Type];
	}

	internal DocumentNode GetParentOfType(DocumentNodeType parentType)
	{
		DocumentNode parent = Parent;
		while (parent != null && parent.Type != parentType)
		{
			parent = parent.Parent;
		}
		return parent;
	}

	internal int GetTableDepth()
	{
		DocumentNode parent = Parent;
		int num = 0;
		while (parent != null)
		{
			if (parent.Type == DocumentNodeType.dnTable)
			{
				num++;
			}
			parent = parent.Parent;
		}
		return num;
	}

	internal int GetListDepth()
	{
		DocumentNode parent = Parent;
		int num = 0;
		while (parent != null)
		{
			if (parent.Type == DocumentNodeType.dnList)
			{
				num++;
			}
			else if (parent.Type == DocumentNodeType.dnCell)
			{
				break;
			}
			parent = parent.Parent;
		}
		return num;
	}

	internal void Terminate(ConverterState converterState)
	{
		if (!IsTerminated)
		{
			string text = StripInvalidChars(Xaml);
			AppendXamlPrefix(converterState);
			Xaml += text;
			AppendXamlPostfix(converterState);
			IsTerminated = true;
		}
	}

	internal void ConstrainFontPropagation(FormatState fsOrig)
	{
		FormatState.SetCharDefaults();
		FormatState.Font = fsOrig.Font;
		FormatState.FontSize = fsOrig.FontSize;
		FormatState.Bold = fsOrig.Bold;
		FormatState.Italic = fsOrig.Italic;
	}

	internal bool RequiresXamlFontProperties()
	{
		FormatState formatState = FormatState;
		FormatState parentFormatStateForFont = ParentFormatStateForFont;
		if (formatState.Strike == parentFormatStateForFont.Strike && formatState.UL == parentFormatStateForFont.UL && (formatState.Font == parentFormatStateForFont.Font || formatState.Font < 0) && (formatState.FontSize == parentFormatStateForFont.FontSize || formatState.FontSize < 0) && formatState.CF == parentFormatStateForFont.CF && formatState.Bold == parentFormatStateForFont.Bold && formatState.Italic == parentFormatStateForFont.Italic)
		{
			return formatState.LangCur != parentFormatStateForFont.LangCur;
		}
		return true;
	}

	internal void AppendXamlFontProperties(ConverterState converterState, StringBuilder sb)
	{
		FormatState formatState = FormatState;
		FormatState parentFormatStateForFont = ParentFormatStateForFont;
		bool flag = formatState.Strike != parentFormatStateForFont.Strike;
		bool flag2 = formatState.UL != parentFormatStateForFont.UL;
		if (flag || flag2)
		{
			sb.Append(" TextDecorations=\"");
			if (flag2)
			{
				sb.Append("Underline");
			}
			if (flag2 && flag)
			{
				sb.Append(", ");
			}
			if (flag)
			{
				sb.Append("Strikethrough");
			}
			sb.Append('"');
		}
		if (formatState.Font != parentFormatStateForFont.Font && formatState.Font >= 0)
		{
			FontTableEntry fontTableEntry = converterState.FontTable.FindEntryByIndex((int)formatState.Font);
			if (fontTableEntry != null && fontTableEntry.Name != null && !fontTableEntry.Name.Equals(string.Empty))
			{
				sb.Append(" FontFamily=\"");
				if (fontTableEntry.Name.Length > 32)
				{
					sb.Append(fontTableEntry.Name, 0, 32);
				}
				else
				{
					sb.Append(fontTableEntry.Name);
				}
				sb.Append('"');
			}
		}
		if (formatState.FontSize != parentFormatStateForFont.FontSize && formatState.FontSize >= 0)
		{
			sb.Append(" FontSize=\"");
			double num = formatState.FontSize;
			if (num <= 1.0)
			{
				num = 2.0;
			}
			sb.Append((num / 2.0).ToString(CultureInfo.InvariantCulture));
			sb.Append("pt\"");
		}
		if (formatState.Bold != parentFormatStateForFont.Bold)
		{
			if (formatState.Bold)
			{
				sb.Append(" FontWeight=\"Bold\"");
			}
			else
			{
				sb.Append(" FontWeight=\"Normal\"");
			}
		}
		if (formatState.Italic != parentFormatStateForFont.Italic)
		{
			if (formatState.Italic)
			{
				sb.Append(" FontStyle=\"Italic\"");
			}
			else
			{
				sb.Append(" FontStyle=\"Normal\"");
			}
		}
		if (formatState.CF != parentFormatStateForFont.CF)
		{
			ColorTableEntry colorTableEntry = converterState.ColorTable.EntryAt((int)formatState.CF);
			if (colorTableEntry != null && !colorTableEntry.IsAuto)
			{
				sb.Append(" Foreground=\"");
				sb.Append(colorTableEntry.Color.ToString());
				sb.Append('"');
			}
		}
		if (formatState.LangCur != parentFormatStateForFont.LangCur && formatState.LangCur > 0 && formatState.LangCur != 1024)
		{
			try
			{
				CultureInfo cultureInfo = new CultureInfo((int)formatState.LangCur);
				sb.Append(" xml:lang=\"");
				sb.Append(cultureInfo.Name);
				sb.Append('"');
			}
			catch (ArgumentException)
			{
			}
		}
	}

	internal string StripInvalidChars(string text)
	{
		if (text == null || text.Length == 0)
		{
			return text;
		}
		StringBuilder stringBuilder = null;
		for (int i = 0; i < text.Length; i++)
		{
			int num = i;
			for (; i < text.Length; i++)
			{
				if ((text[i] & 0xF800) == 55296)
				{
					if (i + 1 == text.Length || (text[i] & 0xFC00) == 56320 || (text[i + 1] & 0xFC00) != 56320)
					{
						break;
					}
					i++;
				}
			}
			if (num != 0 || i != text.Length)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				if (i != num)
				{
					stringBuilder.Append(text, num, i - num);
				}
			}
		}
		if (stringBuilder != null)
		{
			return stringBuilder.ToString();
		}
		return text;
	}

	internal void AppendXamlEncoded(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(Xaml);
		int num = 0;
		while (num < text.Length)
		{
			int i;
			for (i = num; i < text.Length && (text[i] >= ' ' || text[i] == '\t') && text[i] != '&' && text[i] != '>' && text[i] != '<' && text[i] != 0; i++)
			{
			}
			if (i != num)
			{
				ReadOnlySpan<char> value = text.AsSpan(num, i - num);
				stringBuilder.Append(value);
			}
			if (i < text.Length)
			{
				if (text[i] < ' ' && text[i] != '\t')
				{
					if (text[i] == '\f')
					{
						stringBuilder.Append("&#x");
						stringBuilder.Append(((int)text[i]).ToString("x", CultureInfo.InvariantCulture));
						stringBuilder.Append(';');
					}
				}
				else
				{
					switch (text[i])
					{
					case '&':
						stringBuilder.Append("&amp;");
						break;
					case '<':
						stringBuilder.Append("&lt;");
						break;
					case '>':
						stringBuilder.Append("&gt;");
						break;
					}
				}
			}
			num = i + 1;
		}
		Xaml = stringBuilder.ToString();
	}

	internal void AppendXamlPrefix(ConverterState converterState)
	{
		DocumentNodeArray documentNodeArray = converterState.DocumentNodeArray;
		if (IsHidden)
		{
			return;
		}
		if (Type == DocumentNodeType.dnImage)
		{
			AppendImageXamlPrefix();
			return;
		}
		if (Type == DocumentNodeType.dnText || Type == DocumentNodeType.dnInline)
		{
			AppendInlineXamlPrefix(converterState);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (IsEmptyNode && RequiresXamlFontProperties())
		{
			stringBuilder.Append('<');
			stringBuilder.Append(XamlNames[2]);
			AppendXamlFontProperties(converterState, stringBuilder);
			stringBuilder.Append('>');
		}
		stringBuilder.Append('<');
		stringBuilder.Append(GetTagName());
		switch (Type)
		{
		case DocumentNodeType.dnTable:
			AppendXamlPrefixTableProperties(stringBuilder);
			break;
		case DocumentNodeType.dnCell:
			AppendXamlPrefixCellProperties(stringBuilder, documentNodeArray, converterState);
			break;
		case DocumentNodeType.dnParagraph:
			AppendXamlPrefixParagraphProperties(stringBuilder, converterState);
			break;
		case DocumentNodeType.dnListItem:
			AppendXamlPrefixListItemProperties(stringBuilder);
			break;
		case DocumentNodeType.dnList:
			AppendXamlPrefixListProperties(stringBuilder);
			break;
		case DocumentNodeType.dnHyperlink:
			AppendXamlPrefixHyperlinkProperties(stringBuilder);
			break;
		}
		if (IsEmptyNode)
		{
			stringBuilder.Append(" /");
		}
		stringBuilder.Append('>');
		if (IsEmptyNode && RequiresXamlFontProperties())
		{
			stringBuilder.Append("</");
			stringBuilder.Append(XamlNames[2]);
			stringBuilder.Append('>');
		}
		if (Type == DocumentNodeType.dnTable)
		{
			AppendXamlTableColumnsAfterStartTag(stringBuilder);
		}
		Xaml = stringBuilder.ToString();
	}

	private void AppendXamlPrefixTableProperties(StringBuilder xamlStringBuilder)
	{
		if (FormatState.HasRowFormat)
		{
			if (FormatState.RowFormat.Dir == DirState.DirRTL)
			{
				xamlStringBuilder.Append(" FlowDirection=\"RightToLeft\"");
			}
			RowFormat rowFormat = FormatState.RowFormat;
			CellFormat rowCellFormat = rowFormat.RowCellFormat;
			xamlStringBuilder.Append(" CellSpacing=\"");
			xamlStringBuilder.Append(Converters.TwipToPositiveVisiblePxString(rowCellFormat.SpacingLeft));
			xamlStringBuilder.Append('"');
			xamlStringBuilder.Append(" Margin=\"");
			xamlStringBuilder.Append(Converters.TwipToPositivePxString(rowFormat.Trleft));
			xamlStringBuilder.Append(",0,0,0\"");
		}
		else
		{
			xamlStringBuilder.Append(" CellSpacing=\"0\" Margin=\"0,0,0,0\"");
		}
	}

	private void AppendXamlPrefixCellProperties(StringBuilder xamlStringBuilder, DocumentNodeArray dna, ConverterState converterState)
	{
		Color c = Color.FromArgb(byte.MaxValue, 0, 0, 0);
		DocumentNode parentOfType = GetParentOfType(DocumentNodeType.dnRow);
		if (parentOfType != null && parentOfType.FormatState.HasRowFormat)
		{
			int cellColumn = GetCellColumn();
			CellFormat cellFormat = parentOfType.FormatState.RowFormat.NthCellFormat(cellColumn);
			if (Converters.ColorToUse(converterState, cellFormat.CB, cellFormat.CF, cellFormat.Shading, ref c))
			{
				xamlStringBuilder.Append(" Background=\"");
				xamlStringBuilder.Append(c.ToString(CultureInfo.InvariantCulture));
				xamlStringBuilder.Append('"');
			}
			if (cellFormat.HasBorder)
			{
				xamlStringBuilder.Append(cellFormat.GetBorderAttributeString(converterState));
			}
			xamlStringBuilder.Append(cellFormat.GetPaddingAttributeString());
		}
		else
		{
			xamlStringBuilder.Append(" BorderBrush=\"#FF000000\" BorderThickness=\"1,1,1,1\"");
		}
		if (ColSpan > 1)
		{
			xamlStringBuilder.Append(" ColumnSpan=\"");
			xamlStringBuilder.Append(ColSpan.ToString(CultureInfo.InvariantCulture));
			xamlStringBuilder.Append('"');
		}
		if (RowSpan > 1)
		{
			xamlStringBuilder.Append(" RowSpan=\"");
			xamlStringBuilder.Append(RowSpan.ToString(CultureInfo.InvariantCulture));
			xamlStringBuilder.Append('"');
		}
	}

	private void AppendXamlDir(StringBuilder xamlStringBuilder)
	{
		if (RequiresXamlDir)
		{
			if (XamlDir == DirState.DirLTR)
			{
				xamlStringBuilder.Append(" FlowDirection=\"LeftToRight\"");
			}
			else
			{
				xamlStringBuilder.Append(" FlowDirection=\"RightToLeft\"");
			}
		}
	}

	private void AppendXamlPrefixParagraphProperties(StringBuilder xamlStringBuilder, ConverterState converterState)
	{
		Color c = Color.FromArgb(byte.MaxValue, 0, 0, 0);
		FormatState formatState = FormatState;
		if (Converters.ColorToUse(converterState, formatState.CBPara, formatState.CFPara, formatState.ParaShading, ref c))
		{
			xamlStringBuilder.Append(" Background=\"");
			xamlStringBuilder.Append(c.ToString(CultureInfo.InvariantCulture));
			xamlStringBuilder.Append('"');
		}
		AppendXamlDir(xamlStringBuilder);
		xamlStringBuilder.Append(" Margin=\"");
		xamlStringBuilder.Append(Converters.TwipToPositivePxString(NearMargin));
		xamlStringBuilder.Append(',');
		xamlStringBuilder.Append(Converters.TwipToPositivePxString(formatState.SB));
		xamlStringBuilder.Append(',');
		xamlStringBuilder.Append(Converters.TwipToPositivePxString(FarMargin));
		xamlStringBuilder.Append(',');
		xamlStringBuilder.Append(Converters.TwipToPositivePxString(formatState.SA));
		xamlStringBuilder.Append('"');
		AppendXamlFontProperties(converterState, xamlStringBuilder);
		if (formatState.FI != 0L)
		{
			xamlStringBuilder.Append(" TextIndent=\"");
			xamlStringBuilder.Append(Converters.TwipToPxString(formatState.FI));
			xamlStringBuilder.Append('"');
		}
		if (formatState.HAlign != HAlign.AlignDefault)
		{
			xamlStringBuilder.Append(" TextAlignment=\"");
			xamlStringBuilder.Append(Converters.AlignmentToString(formatState.HAlign, formatState.DirPara));
			xamlStringBuilder.Append('"');
		}
		if (formatState.HasParaBorder)
		{
			xamlStringBuilder.Append(formatState.GetBorderAttributeString(converterState));
		}
	}

	private void AppendXamlPrefixListItemProperties(StringBuilder xamlStringBuilder)
	{
		long num = NearMargin;
		if (num < 360 && GetListDepth() == 1)
		{
			DocumentNode parent = Parent;
			if (parent != null && parent.FormatState.Marker != MarkerStyle.MarkerHidden)
			{
				num = 360L;
			}
		}
		xamlStringBuilder.Append(" Margin=\"");
		xamlStringBuilder.Append(Converters.TwipToPositivePxString(num));
		xamlStringBuilder.Append(",0,0,0\"");
		AppendXamlDir(xamlStringBuilder);
	}

	private void AppendXamlPrefixListProperties(StringBuilder xamlStringBuilder)
	{
		xamlStringBuilder.Append(" Margin=\"0,0,0,0\"");
		xamlStringBuilder.Append(" Padding=\"0,0,0,0\"");
		xamlStringBuilder.Append(" MarkerStyle=\"");
		xamlStringBuilder.Append(Converters.MarkerStyleToString(FormatState.Marker));
		xamlStringBuilder.Append('"');
		if (FormatState.StartIndex > 0 && FormatState.StartIndex != 1)
		{
			xamlStringBuilder.Append(" StartIndex=\"");
			xamlStringBuilder.Append(FormatState.StartIndex.ToString(CultureInfo.InvariantCulture));
			xamlStringBuilder.Append('"');
		}
		AppendXamlDir(xamlStringBuilder);
	}

	private void AppendXamlPrefixHyperlinkProperties(StringBuilder xamlStringBuilder)
	{
		if (NavigateUri != null && NavigateUri.Length > 0)
		{
			xamlStringBuilder.Append(" NavigateUri=\"");
			xamlStringBuilder.Append(Converters.StringToXMLAttribute(NavigateUri));
			xamlStringBuilder.Append('"');
		}
	}

	private void AppendXamlTableColumnsAfterStartTag(StringBuilder xamlStringBuilder)
	{
		if (ColumnStateArray == null || ColumnStateArray.Count <= 0)
		{
			return;
		}
		xamlStringBuilder.Append("<Table.Columns>");
		long num = 0L;
		if (FormatState.HasRowFormat)
		{
			num = FormatState.RowFormat.Trleft;
		}
		for (int i = 0; i < ColumnStateArray.Count; i++)
		{
			ColumnState columnState = ColumnStateArray.EntryAt(i);
			long num2 = columnState.CellX - num;
			if (num2 <= 0)
			{
				num2 = 1L;
			}
			num = columnState.CellX;
			xamlStringBuilder.Append("<TableColumn Width=\"");
			xamlStringBuilder.Append(Converters.TwipToPxString(num2));
			xamlStringBuilder.Append("\" />");
		}
		xamlStringBuilder.Append("</Table.Columns>");
	}

	internal void AppendXamlPostfix(ConverterState converterState)
	{
		if (!IsHidden && !IsEmptyNode)
		{
			if (Type == DocumentNodeType.dnImage)
			{
				AppendImageXamlPostfix();
				return;
			}
			if (Type == DocumentNodeType.dnText || Type == DocumentNodeType.dnInline)
			{
				AppendInlineXamlPostfix(converterState);
				return;
			}
			Xaml = $"{Xaml}</{GetTagName()}>{(IsBlock ? "\r\n" : "")}";
		}
	}

	internal void AppendInlineXamlPrefix(ConverterState converterState)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatState formatState = FormatState;
		FormatState parentFormatStateForFont = ParentFormatStateForFont;
		stringBuilder.Append("<Span");
		AppendXamlDir(stringBuilder);
		if (formatState.CB != parentFormatStateForFont.CB)
		{
			ColorTableEntry colorTableEntry = converterState.ColorTable.EntryAt((int)formatState.CB);
			if (colorTableEntry != null && !colorTableEntry.IsAuto)
			{
				stringBuilder.Append(" Background=\"");
				stringBuilder.Append(colorTableEntry.Color.ToString());
				stringBuilder.Append('"');
			}
		}
		AppendXamlFontProperties(converterState, stringBuilder);
		if (formatState.Super != parentFormatStateForFont.Super)
		{
			stringBuilder.Append(" Typography.Variants=\"Superscript\"");
		}
		if (formatState.Sub != parentFormatStateForFont.Sub)
		{
			stringBuilder.Append(" Typography.Variants=\"Subscript\"");
		}
		stringBuilder.Append('>');
		Xaml = stringBuilder.ToString();
	}

	internal void AppendInlineXamlPostfix(ConverterState converterState)
	{
		Xaml += "</Span>";
	}

	internal void AppendImageXamlPrefix()
	{
		Xaml = "<InlineUIContainer>";
	}

	internal void AppendImageXamlPostfix()
	{
		Xaml += "</InlineUIContainer>";
	}

	internal bool IsAncestorOf(DocumentNode documentNode)
	{
		int index = Index;
		int num = Index + ChildCount;
		if (documentNode.Index > index)
		{
			return documentNode.Index <= num;
		}
		return false;
	}

	internal bool IsLastParagraphInCell()
	{
		DocumentNodeArray dNA = DNA;
		if (Type != DocumentNodeType.dnParagraph)
		{
			return false;
		}
		DocumentNode parentOfType = GetParentOfType(DocumentNodeType.dnCell);
		if (parentOfType == null)
		{
			return false;
		}
		int num = parentOfType.Index + 1;
		int num2 = parentOfType.Index + parentOfType.ChildCount;
		while (num <= num2)
		{
			DocumentNode documentNode = dNA.EntryAt(num2);
			if (documentNode == this)
			{
				return true;
			}
			if (documentNode.IsBlock)
			{
				return false;
			}
			num2--;
		}
		return false;
	}

	internal DocumentNodeArray GetTableRows()
	{
		DocumentNodeArray dNA = DNA;
		DocumentNodeArray documentNodeArray = new DocumentNodeArray();
		if (Type == DocumentNodeType.dnTable)
		{
			int i = Index + 1;
			for (int num = Index + ChildCount; i <= num; i++)
			{
				DocumentNode documentNode = dNA.EntryAt(i);
				if (documentNode.Type == DocumentNodeType.dnRow && this == documentNode.GetParentOfType(DocumentNodeType.dnTable))
				{
					documentNodeArray.Push(documentNode);
				}
			}
		}
		return documentNodeArray;
	}

	internal DocumentNodeArray GetRowsCells()
	{
		DocumentNodeArray dNA = DNA;
		DocumentNodeArray documentNodeArray = new DocumentNodeArray();
		if (Type == DocumentNodeType.dnRow)
		{
			int i = Index + 1;
			for (int num = Index + ChildCount; i <= num; i++)
			{
				DocumentNode documentNode = dNA.EntryAt(i);
				if (documentNode.Type == DocumentNodeType.dnCell && this == documentNode.GetParentOfType(DocumentNodeType.dnRow))
				{
					documentNodeArray.Push(documentNode);
				}
			}
		}
		return documentNodeArray;
	}

	internal int GetCellColumn()
	{
		DocumentNodeArray dNA = DNA;
		int num = 0;
		if (Type == DocumentNodeType.dnCell)
		{
			DocumentNode parentOfType = GetParentOfType(DocumentNodeType.dnRow);
			if (parentOfType != null)
			{
				int i = parentOfType.Index + 1;
				for (int num2 = parentOfType.Index + parentOfType.ChildCount; i <= num2; i++)
				{
					DocumentNode documentNode = dNA.EntryAt(i);
					if (documentNode == this)
					{
						break;
					}
					if (documentNode.Type == DocumentNodeType.dnCell && documentNode.GetParentOfType(DocumentNodeType.dnRow) == parentOfType)
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	internal ColumnStateArray ComputeColumns()
	{
		_ = DNA;
		DocumentNodeArray tableRows = GetTableRows();
		ColumnStateArray columnStateArray = new ColumnStateArray();
		for (int i = 0; i < tableRows.Count; i++)
		{
			DocumentNode documentNode = tableRows.EntryAt(i);
			RowFormat rowFormat = documentNode.FormatState.RowFormat;
			long num = 0L;
			for (int j = 0; j < rowFormat.CellCount; j++)
			{
				CellFormat cellFormat = rowFormat.NthCellFormat(j);
				bool flag = false;
				long num2 = 0L;
				if (cellFormat.IsHMerge)
				{
					continue;
				}
				for (int k = 0; k < columnStateArray.Count; k++)
				{
					ColumnState columnState = (ColumnState)columnStateArray[k];
					if (columnState.CellX == cellFormat.CellX)
					{
						if (!columnState.IsFilled && num2 == num)
						{
							columnState.IsFilled = true;
						}
						flag = true;
						break;
					}
					if (columnState.CellX > cellFormat.CellX)
					{
						ColumnState columnState2 = new ColumnState();
						columnState2.Row = documentNode;
						columnState2.CellX = cellFormat.CellX;
						columnState2.IsFilled = num2 == num;
						columnStateArray.Insert(k, columnState2);
						flag = true;
						break;
					}
					num2 = columnState.CellX;
				}
				if (!flag)
				{
					ColumnState columnState3 = new ColumnState();
					columnState3.Row = documentNode;
					columnState3.CellX = cellFormat.CellX;
					columnState3.IsFilled = num2 == num;
					columnStateArray.Add(columnState3);
				}
				num = cellFormat.CellX;
			}
		}
		return columnStateArray;
	}
}
