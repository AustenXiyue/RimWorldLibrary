using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using MS.Internal;
using MS.Internal.Text;

namespace System.Windows.Documents;

internal class RtfToXamlReader
{
	private enum EncodeType
	{
		Ansi,
		Unicode,
		ShiftJis
	}

	private byte[] _rtfBytes;

	private StringBuilder _outerXamlBuilder;

	private RtfToXamlLexer _lexer;

	private ConverterState _converterState;

	private bool _bForceParagraph;

	private WpfPayload _wpfPayload;

	private int _imageCount;

	private const int MAX_GROUP_DEPTH = 32;

	internal string Output => _outerXamlBuilder.ToString();

	internal bool ForceParagraph
	{
		get
		{
			return _bForceParagraph;
		}
		set
		{
			_bForceParagraph = value;
		}
	}

	internal ConverterState ConverterState => _converterState;

	internal WpfPayload WpfPayload
	{
		set
		{
			_wpfPayload = value;
		}
	}

	internal RtfToXamlReader(string rtfString)
	{
		_rtfBytes = Encoding.Default.GetBytes(rtfString);
		_bForceParagraph = false;
		Initialize();
	}

	private void Initialize()
	{
		_lexer = new RtfToXamlLexer(_rtfBytes);
		_converterState = new ConverterState();
		_converterState.RtfFormatStack.Push();
		_outerXamlBuilder = new StringBuilder();
	}

	internal RtfToXamlError Process()
	{
		RtfToXamlError rtfToXamlError = RtfToXamlError.None;
		RtfToken rtfToken = new RtfToken();
		bool flag = false;
		int count = _converterState.RtfFormatStack.Count;
		while (rtfToXamlError == RtfToXamlError.None)
		{
			rtfToXamlError = _lexer.Next(rtfToken, _converterState.TopFormatState);
			if (rtfToXamlError != 0)
			{
				break;
			}
			switch (rtfToken.Type)
			{
			case RtfTokenType.TokenGroupStart:
				_converterState.RtfFormatStack.Push();
				flag = false;
				break;
			case RtfTokenType.TokenGroupEnd:
				ProcessGroupEnd();
				flag = false;
				break;
			case RtfTokenType.TokenInvalid:
				rtfToXamlError = RtfToXamlError.InvalidFormat;
				break;
			case RtfTokenType.TokenEOF:
				while (_converterState.RtfFormatStack.Count > 2 && _converterState.RtfFormatStack.Count > count)
				{
					ProcessGroupEnd();
				}
				AppendDocument();
				return RtfToXamlError.None;
			case RtfTokenType.TokenDestination:
				flag = true;
				break;
			case RtfTokenType.TokenControl:
			{
				RtfControlWordInfo rtfControlWordInfo = rtfToken.RtfControlWordInfo;
				if (rtfControlWordInfo != null && !flag && (rtfControlWordInfo.Flags & 8) != 0)
				{
					flag = true;
				}
				if (flag)
				{
					if (rtfControlWordInfo != null && rtfControlWordInfo.Control == RtfControlWord.Ctrl_Unknown && _converterState.TopFormatState.RtfDestination == RtfDestination.DestFieldResult)
					{
						rtfControlWordInfo = null;
					}
					else
					{
						_converterState.TopFormatState.RtfDestination = RtfDestination.DestUnknown;
					}
					flag = false;
				}
				if (rtfControlWordInfo != null)
				{
					HandleControl(rtfToken, rtfControlWordInfo);
				}
				break;
			}
			case RtfTokenType.TokenText:
				ProcessText(rtfToken);
				break;
			case RtfTokenType.TokenTextSymbol:
				ProcessTextSymbol(rtfToken);
				break;
			case RtfTokenType.TokenPictureData:
				ProcessImage(_converterState.TopFormatState);
				break;
			}
		}
		return rtfToXamlError;
	}

	internal bool TreeContainsBlock()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.Type == DocumentNodeType.dnParagraph || documentNode.Type == DocumentNodeType.dnList || documentNode.Type == DocumentNodeType.dnTable)
			{
				return true;
			}
		}
		return false;
	}

	internal void AppendDocument()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		while (documentNodeArray.Count > 0)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(documentNodeArray.Count - 1);
			if (!documentNode.IsInline || !documentNode.IsWhiteSpace)
			{
				break;
			}
			documentNodeArray.Excise(documentNodeArray.Count - 1, 1);
		}
		if ((ForceParagraph || TreeContainsBlock()) && documentNodeArray.Count > 0 && documentNodeArray.EntryAt(documentNodeArray.Count - 1).IsInline)
		{
			FormatState topFormatState = _converterState.TopFormatState;
			if (topFormatState != null)
			{
				HandlePara(null, topFormatState);
			}
		}
		documentNodeArray.CloseAll();
		documentNodeArray.CoalesceAll(_converterState);
		bool flag = ForceParagraph || TreeContainsBlock();
		if (flag)
		{
			_outerXamlBuilder.Append("<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\" >\r\n");
		}
		else
		{
			_outerXamlBuilder.Append("<Span xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\">");
		}
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			DocumentNode documentNode2 = documentNodeArray.EntryAt(i);
			_outerXamlBuilder.Append(documentNode2.Xaml);
		}
		if (flag)
		{
			_outerXamlBuilder.Append("</Section>");
		}
		else
		{
			_outerXamlBuilder.Append("</Span>");
		}
	}

	internal void ProcessField()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		DocumentNode documentNode = null;
		DocumentNode documentNode2 = null;
		DocumentNode documentNode3 = null;
		DocumentNode documentNode4 = null;
		DocumentNode documentNode5 = null;
		DocumentNode documentNode6 = null;
		int num = documentNodeArray.Count - 1;
		while (num >= 0 && documentNode == null)
		{
			DocumentNode documentNode7 = documentNodeArray.EntryAt(num);
			if (documentNode7.Type == DocumentNodeType.dnFieldBegin)
			{
				switch (documentNode7.FormatState.RtfDestination)
				{
				case RtfDestination.DestFieldInstruction:
					documentNode3 = documentNode7;
					break;
				case RtfDestination.DestFieldResult:
					documentNode5 = documentNode7;
					break;
				case RtfDestination.DestField:
					documentNode = documentNode7;
					break;
				}
			}
			else if (documentNode7.Type == DocumentNodeType.dnFieldEnd)
			{
				switch (documentNode7.FormatState.RtfDestination)
				{
				case RtfDestination.DestFieldInstruction:
					documentNode4 = documentNode7;
					break;
				case RtfDestination.DestFieldResult:
					documentNode6 = documentNode7;
					break;
				case RtfDestination.DestField:
					documentNode2 = documentNode7;
					break;
				}
			}
			num--;
		}
		if (documentNode == null || documentNode2 == null)
		{
			return;
		}
		DocumentNode documentNode8 = null;
		string text = null;
		if (documentNode3 != null && documentNode4 != null && documentNode4.Index > documentNode3.Index + 1)
		{
			string text2 = string.Empty;
			for (int i = documentNode3.Index + 1; i < documentNode4.Index; i++)
			{
				DocumentNode documentNode9 = documentNodeArray.EntryAt(i);
				if (documentNode9.Type == DocumentNodeType.dnText)
				{
					text2 += documentNode9.Xaml;
				}
			}
			if (text2.Length > 0 && text2[0] == ' ')
			{
				text2 = text2.Substring(1);
			}
			if (text2.IndexOf("HYPERLINK", StringComparison.Ordinal) == 0)
			{
				documentNode8 = ProcessHyperlinkField(text2);
			}
			else if (text2.IndexOf("SYMBOL", StringComparison.Ordinal) == 0)
			{
				documentNode8 = ProcessSymbolField(text2);
			}
			else if (text2.IndexOf("INCLUDEPICTURE", StringComparison.Ordinal) == 0)
			{
				text = GetIncludePictureUri(text2);
			}
		}
		if (documentNode3 != null && documentNode4 != null)
		{
			int nExcise = documentNode4.Index - documentNode3.Index + 1;
			documentNodeArray.Excise(documentNode3.Index, nExcise);
		}
		if (documentNode5 != null)
		{
			documentNodeArray.Excise(documentNode5.Index, 1);
		}
		if (documentNode6 != null)
		{
			documentNodeArray.Excise(documentNode6.Index, 1);
		}
		int index = documentNode.Index;
		int num2 = documentNode2.Index - documentNode.Index - 1;
		DocumentNode closedParent = documentNode.ClosedParent;
		documentNodeArray.Excise(documentNode.Index, 1);
		documentNodeArray.Excise(documentNode2.Index, 1);
		if (text != null && num2 != 0)
		{
			DocumentNode documentNode10 = documentNodeArray.EntryAt(index);
			if (documentNode10.Type == DocumentNodeType.dnImage)
			{
				int num3 = documentNode10.Xaml.IndexOf("UriSource=", StringComparison.Ordinal);
				int num4 = documentNode10.Xaml.IndexOf('"', num3 + 11);
				string text3 = documentNode10.Xaml.Substring(0, num3);
				text3 = text3 + "UriSource=\"" + text + "\"";
				text3 += documentNode10.Xaml.Substring(num4 + 1);
				documentNode10.Xaml = text3;
			}
		}
		if (documentNode8 != null)
		{
			bool flag = true;
			if (documentNode8.IsInline)
			{
				int num5 = index;
				while (flag && num5 < index + num2)
				{
					if (documentNodeArray.EntryAt(num5).IsBlock)
					{
						flag = false;
					}
					num5++;
				}
			}
			if (flag)
			{
				if (documentNode8.Type == DocumentNodeType.dnText || num2 != 0)
				{
					documentNodeArray.InsertChildAt(closedParent, documentNode8, index, num2);
				}
			}
			else if (documentNode8.Type == DocumentNodeType.dnHyperlink)
			{
				documentNode8.AppendXamlPrefix(_converterState);
				for (int j = index; j < index + num2; j++)
				{
					DocumentNode documentNode11 = documentNodeArray.EntryAt(j);
					if (documentNode11.Type == DocumentNodeType.dnParagraph && !documentNode11.IsTerminated)
					{
						documentNode11.Xaml = documentNode8.Xaml + documentNode11.Xaml + "</Hyperlink>";
					}
				}
				int num6 = 0;
				int num7 = index + num2 - 1;
				while (num7 >= index && documentNodeArray.EntryAt(num7).IsInline)
				{
					num6++;
					num7--;
				}
				if (num6 > 0)
				{
					WrapInlineInParagraph(num7 + 1, num6);
					DocumentNode documentNode12 = documentNodeArray.EntryAt(num7 + 1);
					if (documentNode12.Type == DocumentNodeType.dnParagraph && !documentNode12.IsTerminated)
					{
						documentNode12.Xaml = documentNode8.Xaml + documentNode12.Xaml + "</Hyperlink>";
					}
				}
			}
		}
		bool flag2 = false;
		int num8 = index;
		while (!flag2 && num8 < documentNodeArray.Count)
		{
			flag2 = documentNodeArray.EntryAt(num8).IsBlock;
			num8++;
		}
		if (!flag2)
		{
			return;
		}
		int num9 = 0;
		num2 = 0;
		for (int num10 = index - 1; num10 >= 0; num10--)
		{
			DocumentNode documentNode13 = documentNodeArray.EntryAt(num10);
			if (documentNode13.IsInline || documentNode13.IsHidden)
			{
				num2++;
				if (!documentNode13.IsHidden)
				{
					num9++;
				}
			}
			if (documentNode13.IsBlock)
			{
				num2 -= documentNode13.ChildCount;
				break;
			}
		}
		if (num9 > 0)
		{
			WrapInlineInParagraph(index - num2, num2);
		}
	}

	private int HexToInt(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c - 48;
		}
		if (c >= 'a' && c <= 'f')
		{
			return 10 + c - 97;
		}
		if (c >= 'A' && c <= 'F')
		{
			return 10 + c - 65;
		}
		return 0;
	}

	private int DecToInt(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c - 48;
		}
		return 0;
	}

	private string GetIncludePictureUri(string instructionName)
	{
		string text = null;
		int num = instructionName.IndexOf("http:", StringComparison.OrdinalIgnoreCase);
		if (num != -1)
		{
			text = instructionName.Substring(num, instructionName.Length - num - 1);
			int num2 = text.IndexOf('"');
			if (num2 != -1)
			{
				text = text.Substring(0, num2);
			}
			if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
			{
				text = null;
			}
		}
		return text;
	}

	internal DocumentNode ProcessHyperlinkField(string instr)
	{
		DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnHyperlink);
		documentNode.FormatState = new FormatState(_converterState.PreviousTopFormatState(0));
		string text = null;
		string text2 = null;
		bool flag = false;
		bool flag2 = false;
		int num = 10;
		while (num < instr.Length)
		{
			if (instr[num] == ' ')
			{
				num++;
			}
			else if (instr[num] == '"')
			{
				num++;
				if (num < instr.Length)
				{
					int num2 = num;
					int i;
					for (i = num; i < instr.Length && instr[i] != '"'; i++)
					{
					}
					string text3 = instr.Substring(num2, i - num2);
					if (flag)
					{
						flag = false;
					}
					else if (flag2)
					{
						text2 = text3;
						flag2 = false;
					}
					else if (text == null)
					{
						text = text3;
					}
					num = i + 1;
				}
			}
			else if (instr[num] == '\\')
			{
				num++;
				if (num < instr.Length)
				{
					switch (instr[num])
					{
					case 'l':
						flag2 = true;
						flag = false;
						break;
					case 't':
						flag2 = false;
						flag = true;
						break;
					}
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (text != null)
		{
			stringBuilder.Append(text);
		}
		if (text2 != null)
		{
			stringBuilder.Append('#');
			stringBuilder.Append(text2);
		}
		for (int j = 0; j < stringBuilder.Length; j++)
		{
			if (stringBuilder[j] == '\\' && j + 1 < stringBuilder.Length && stringBuilder[j + 1] == '\\')
			{
				stringBuilder.Remove(j + 1, 1);
			}
		}
		documentNode.NavigateUri = stringBuilder.ToString();
		if (documentNode.NavigateUri.Length <= 0)
		{
			return null;
		}
		return documentNode;
	}

	internal DocumentNode ProcessSymbolField(string instr)
	{
		DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnText);
		documentNode.FormatState = new FormatState(_converterState.PreviousTopFormatState(0));
		int num = -1;
		EncodeType encodeType = EncodeType.Ansi;
		int i = 7;
		while (i < instr.Length)
		{
			if (instr[i] == ' ')
			{
				i++;
			}
			else if (num == -1 && instr[i] >= '0' && instr[i] <= '9')
			{
				if (instr[i] == '0' && i < instr.Length - 1 && instr[i + 1] == 'x')
				{
					i += 2;
					num = 0;
					for (; i < instr.Length && instr[i] != ' ' && instr[i] != '\\'; i++)
					{
						if (num < 65535)
						{
							num = num * 16 + HexToInt(instr[i]);
						}
					}
					continue;
				}
				num = 0;
				for (; i < instr.Length && instr[i] != ' ' && instr[i] != '\\'; i++)
				{
					if (num < 65535)
					{
						num = num * 10 + DecToInt(instr[i]);
					}
				}
			}
			else if (instr[i] == '\\')
			{
				i++;
				if (i < instr.Length)
				{
					ProcessSymbolFieldInstruction(documentNode, instr, ref i, ref encodeType);
				}
			}
			else
			{
				i++;
			}
		}
		if (num == -1)
		{
			return null;
		}
		ConvertSymbolCharValueToText(documentNode, num, encodeType);
		if (documentNode.Xaml == null || documentNode.Xaml.Length <= 0)
		{
			return null;
		}
		return documentNode;
	}

	private void ProcessSymbolFieldInstruction(DocumentNode dn, string instr, ref int i, ref EncodeType encodeType)
	{
		int num = 0;
		switch (instr[i++])
		{
		case 'a':
			encodeType = EncodeType.Ansi;
			break;
		case 'u':
			encodeType = EncodeType.Unicode;
			break;
		case 'j':
			encodeType = EncodeType.ShiftJis;
			break;
		case 's':
		{
			if (i < instr.Length && instr[i] == ' ')
			{
				i++;
			}
			num = i;
			while (i < instr.Length && instr[i] != ' ')
			{
				i++;
			}
			ReadOnlySpan<char> s = instr.AsSpan(num, i - num);
			bool flag = true;
			double num2 = 0.0;
			try
			{
				num2 = double.Parse(s, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				flag = false;
			}
			catch (FormatException)
			{
				flag = false;
			}
			if (flag)
			{
				dn.FormatState.FontSize = (long)(num2 * 2.0 + 0.5);
			}
			break;
		}
		case 'f':
		{
			if (i < instr.Length && instr[i] == ' ')
			{
				i++;
			}
			if (i < instr.Length && instr[i] == '"')
			{
				i++;
			}
			num = i;
			while (i < instr.Length && instr[i] != '"')
			{
				i++;
			}
			string text = instr.Substring(num, i - num);
			i++;
			if (text != null && text.Length > 0)
			{
				dn.FormatState.Font = _converterState.FontTable.DefineEntryByName(text);
			}
			break;
		}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void ProcessImage(FormatState formatState)
	{
		string text;
		switch (formatState.ImageFormat)
		{
		case RtfImageFormat.Png:
		case RtfImageFormat.Wmf:
			text = "image/png";
			break;
		case RtfImageFormat.Jpeg:
			text = "image/jpeg";
			break;
		default:
			text = string.Empty;
			break;
		}
		bool flag = formatState.ImageScaleWidth < 0.0 || formatState.ImageScaleHeight < 0.0;
		if (_wpfPayload != null && text != string.Empty && !flag)
		{
			string imagePartUriString;
			Stream stream = _wpfPayload.CreateImageStream(_imageCount, text, out imagePartUriString);
			using (stream)
			{
				if (formatState.ImageFormat != RtfImageFormat.Wmf)
				{
					_lexer.WriteImageData(stream, formatState.IsImageDataBinary);
				}
				else
				{
					MemoryStream memoryStream = new MemoryStream();
					using (memoryStream)
					{
						_lexer.WriteImageData(memoryStream, formatState.IsImageDataBinary);
						memoryStream.Position = 0L;
						SystemDrawingHelper.SaveMetafileToImageStream(memoryStream, stream);
					}
				}
			}
			_imageCount++;
			formatState.ImageSource = imagePartUriString;
			DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnImage);
			documentNode.FormatState = formatState;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<Image ");
			stringBuilder.Append(" Width=\"");
			stringBuilder.Append(((formatState.ImageScaleWidth == 0.0) ? formatState.ImageWidth : (formatState.ImageWidth * (formatState.ImageScaleWidth / 100.0))).ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append('"');
			stringBuilder.Append(" Height=\"");
			double num = formatState.ImageHeight * (formatState.ImageScaleHeight / 100.0);
			num = ((formatState.ImageScaleHeight == 0.0) ? formatState.ImageHeight : (formatState.ImageHeight * (formatState.ImageScaleHeight / 100.0)));
			stringBuilder.Append(num.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append('"');
			if (formatState.IncludeImageBaselineOffset)
			{
				double num2 = num - formatState.ImageBaselineOffset;
				stringBuilder.Append(" TextBlock.BaselineOffset=\"");
				stringBuilder.Append(num2.ToString(CultureInfo.InvariantCulture));
				stringBuilder.Append('"');
			}
			stringBuilder.Append(" Stretch=\"Fill");
			stringBuilder.Append('"');
			stringBuilder.Append('>');
			stringBuilder.Append("<Image.Source>");
			stringBuilder.Append("<BitmapImage ");
			stringBuilder.Append("UriSource=\"");
			stringBuilder.Append(imagePartUriString);
			stringBuilder.Append("\" ");
			stringBuilder.Append("CacheOption=\"OnLoad\" ");
			stringBuilder.Append("/>");
			stringBuilder.Append("</Image.Source>");
			stringBuilder.Append("</Image>");
			documentNode.Xaml = stringBuilder.ToString();
			DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
			documentNodeArray.Push(documentNode);
			documentNodeArray.CloseAt(documentNodeArray.Count - 1);
		}
		else
		{
			_lexer.AdvanceForImageData();
		}
	}

	private void ConvertSymbolCharValueToText(DocumentNode dn, int nChar, EncodeType encodeType)
	{
		switch (encodeType)
		{
		case EncodeType.Unicode:
			if (nChar < 65535)
			{
				dn.AppendXamlEncoded(char.ToString((char)nChar));
			}
			break;
		case EncodeType.ShiftJis:
			if (nChar < 65535)
			{
				Encoding encoding = InternalEncoding.GetEncoding(932);
				int num = ((nChar <= 256) ? 1 : 2);
				byte[] array = new byte[2];
				if (num == 1)
				{
					array[0] = (byte)nChar;
				}
				else
				{
					array[0] = (byte)((nChar >> 8) & 0xFF);
					array[1] = (byte)(nChar & 0xFF);
				}
				dn.AppendXamlEncoded(encoding.GetString(array, 0, num));
			}
			break;
		default:
			if (nChar < 256)
			{
				char c = (char)nChar;
				dn.AppendXamlEncoded(new string(c, 1));
			}
			break;
		}
	}

	internal void ProcessListText()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int num = documentNodeArray.FindPending(DocumentNodeType.dnListText);
		if (num < 0)
		{
			return;
		}
		DocumentNode documentNode = documentNodeArray.EntryAt(num);
		documentNodeArray.CloseAt(num);
		bool flag = true;
		if (documentNode.HasMarkerContent)
		{
			flag = false;
		}
		else
		{
			int num2 = num + documentNode.ChildCount;
			int num3 = num + 1;
			while (flag && num3 <= num2)
			{
				if (!documentNodeArray.EntryAt(num3).IsWhiteSpace)
				{
					flag = false;
				}
				num3++;
			}
		}
		documentNodeArray.CoalesceChildren(_converterState, num);
		if (flag)
		{
			_converterState.IsMarkerWhiteSpace = true;
		}
		documentNode.Xaml = string.Empty;
		_converterState.IsMarkerPresent = true;
	}

	internal void ProcessShapeResult()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int num = documentNodeArray.FindPending(DocumentNodeType.dnShape);
		if (num >= 0)
		{
			FormatState topFormatState = _converterState.TopFormatState;
			if (topFormatState != null)
			{
				WrapPendingInlineInParagraph(null, topFormatState);
			}
			documentNodeArray.CloseAt(num);
		}
	}

	private void ProcessRtfDestination(FormatState fsCur)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		switch (fsCur.RtfDestination)
		{
		case RtfDestination.DestField:
		{
			int num = documentNodeArray.FindUnmatched(DocumentNodeType.dnFieldBegin);
			if (num >= 0)
			{
				DocumentNode documentNode2 = new DocumentNode(DocumentNodeType.dnFieldEnd);
				documentNode2.FormatState = new FormatState(fsCur);
				documentNode2.IsPending = false;
				documentNodeArray.Push(documentNode2);
				documentNodeArray.EntryAt(num).IsMatched = true;
				ProcessField();
			}
			break;
		}
		case RtfDestination.DestFieldInstruction:
		case RtfDestination.DestFieldResult:
		case RtfDestination.DestFieldPrivate:
		{
			int num = documentNodeArray.FindUnmatched(DocumentNodeType.dnFieldBegin);
			if (num >= 0)
			{
				DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnFieldEnd);
				documentNode.FormatState = new FormatState(fsCur);
				documentNode.IsPending = false;
				documentNodeArray.Push(documentNode);
				documentNodeArray.EntryAt(num).IsMatched = true;
			}
			break;
		}
		case RtfDestination.DestShapeResult:
			ProcessShapeResult();
			break;
		case RtfDestination.DestListText:
			ProcessListText();
			break;
		case RtfDestination.DestListLevel:
		{
			ListTableEntry currentEntry2 = _converterState.ListTable.CurrentEntry;
			if (currentEntry2 != null)
			{
				currentEntry2.Levels.CurrentEntry.FormatState = new FormatState(fsCur);
			}
			break;
		}
		case RtfDestination.DestFontName:
		{
			FontTableEntry currentEntry = _converterState.FontTable.CurrentEntry;
			if (currentEntry != null)
			{
				currentEntry.IsNameSealed = true;
				currentEntry.IsPending = false;
			}
			break;
		}
		case RtfDestination.DestFontTable:
			_converterState.FontTable.MapFonts();
			break;
		case RtfDestination.DestListTable:
		case RtfDestination.DestListOverrideTable:
		case RtfDestination.DestList:
		case RtfDestination.DestListOverride:
		case RtfDestination.DestListPicture:
		case RtfDestination.DestUPR:
		case RtfDestination.DestShape:
		case RtfDestination.DestShapeInstruction:
			break;
		}
	}

	internal void ProcessGroupEnd()
	{
		if (_converterState.RtfFormatStack.Count <= 2)
		{
			return;
		}
		_ = _converterState.DocumentNodeArray;
		FormatState formatState = _converterState.PreviousTopFormatState(0);
		FormatState formatState2 = _converterState.PreviousTopFormatState(1);
		if (formatState.RtfDestination != formatState2.RtfDestination)
		{
			ProcessRtfDestination(formatState);
		}
		else if (formatState2.RtfDestination == RtfDestination.DestFontTable)
		{
			FontTableEntry currentEntry = _converterState.FontTable.CurrentEntry;
			if (currentEntry != null)
			{
				currentEntry.IsPending = false;
			}
		}
		_converterState.RtfFormatStack.Pop();
		if (formatState2.CodePage == -1)
		{
			_lexer.CodePage = _converterState.CodePage;
		}
		else
		{
			_lexer.CodePage = formatState2.CodePage;
		}
		if (formatState.RtfDestination == RtfDestination.DestFontTable && _converterState.DefaultFont >= 0)
		{
			SelectFont(_converterState.DefaultFont);
		}
		else if (formatState.Font < 0 && _converterState.DefaultFont >= 0)
		{
			SelectFont(_converterState.DefaultFont);
		}
	}

	internal void SelectFont(long nFont)
	{
		FormatState topFormatState = _converterState.TopFormatState;
		if (topFormatState == null)
		{
			return;
		}
		topFormatState.Font = nFont;
		FontTableEntry fontTableEntry = _converterState.FontTable.FindEntryByIndex((int)topFormatState.Font);
		if (fontTableEntry != null)
		{
			if (fontTableEntry.CodePage == -1)
			{
				topFormatState.CodePage = _converterState.CodePage;
			}
			else
			{
				topFormatState.CodePage = fontTableEntry.CodePage;
			}
			_lexer.CodePage = topFormatState.CodePage;
		}
	}

	internal void HandleControl(RtfToken token, RtfControlWordInfo controlWordInfo)
	{
		FormatState topFormatState = _converterState.TopFormatState;
		if (topFormatState == null)
		{
			return;
		}
		RtfControlWord control = controlWordInfo.Control;
		if (control <= RtfControlWord.Ctrl_PCA)
		{
			switch (control)
			{
			case RtfControlWord.Ctrl_DELETED:
				topFormatState.IsHidden = true;
				return;
			case RtfControlWord.Ctrl_B:
				topFormatState.Bold = token.ToggleValue > 0;
				return;
			case RtfControlWord.Ctrl_I:
				topFormatState.Italic = token.ToggleValue > 0;
				return;
			case RtfControlWord.Ctrl_IMPR:
				topFormatState.Engrave = token.ToggleValue > 0;
				return;
			case RtfControlWord.Ctrl_OUTL:
				topFormatState.Outline = token.ToggleValue > 0;
				return;
			case RtfControlWord.Ctrl_FS:
				if (Validators.IsValidFontSize(token.Parameter))
				{
					topFormatState.FontSize = token.Parameter;
				}
				return;
			case RtfControlWord.Ctrl_EXPND:
			case RtfControlWord.Ctrl_EXPNDTW:
				topFormatState.Expand = token.Parameter;
				return;
			case RtfControlWord.Ctrl_F:
				break;
			case RtfControlWord.Ctrl_DBCH:
				topFormatState.FontSlot = FontSlot.DBCH;
				return;
			case RtfControlWord.Ctrl_LOCH:
				topFormatState.FontSlot = FontSlot.LOCH;
				return;
			case RtfControlWord.Ctrl_HICH:
				topFormatState.FontSlot = FontSlot.HICH;
				return;
			case RtfControlWord.Ctrl_LANG:
				topFormatState.Lang = token.Parameter;
				topFormatState.LangCur = token.Parameter;
				return;
			case RtfControlWord.Ctrl_LANGFE:
				topFormatState.LangFE = token.Parameter;
				return;
			case RtfControlWord.Ctrl_DEFLANG:
				_converterState.DefaultLang = token.Parameter;
				return;
			case RtfControlWord.Ctrl_DEFLANGFE:
				_converterState.DefaultLangFE = token.Parameter;
				return;
			case RtfControlWord.Ctrl_DEFF:
				_converterState.DefaultFont = token.Parameter;
				return;
			case RtfControlWord.Ctrl_FNAME:
				if (_converterState.PreviousTopFormatState(1).RtfDestination == RtfDestination.DestFontTable)
				{
					topFormatState.RtfDestination = RtfDestination.DestFontName;
					FontTableEntry currentEntry = _converterState.FontTable.CurrentEntry;
					if (currentEntry != null)
					{
						currentEntry.Name = null;
					}
				}
				return;
			case RtfControlWord.Ctrl_FCHARSET:
				if (topFormatState.RtfDestination == RtfDestination.DestFontTable)
				{
					HandleFontTableTokens(token);
				}
				return;
			case RtfControlWord.Ctrl_HIGHLIGHT:
				topFormatState.CB = token.Parameter;
				return;
			case RtfControlWord.Ctrl_CB:
				topFormatState.CB = token.Parameter;
				return;
			case RtfControlWord.Ctrl_CF:
				topFormatState.CF = token.Parameter;
				return;
			case RtfControlWord.Ctrl_NOSUPERSUB:
				topFormatState.Sub = false;
				topFormatState.Super = false;
				topFormatState.SuperOffset = 0L;
				return;
			case RtfControlWord.Ctrl_PARD:
				topFormatState.SetParaDefaults();
				return;
			case RtfControlWord.Ctrl_FI:
				if (token.Parameter > 0)
				{
					topFormatState.FI = token.Parameter;
				}
				return;
			case RtfControlWord.Ctrl_LI:
				topFormatState.LI = token.Parameter;
				return;
			case RtfControlWord.Ctrl_CFPAT:
				topFormatState.CFPara = token.Parameter;
				return;
			case RtfControlWord.Ctrl_CBPAT:
				topFormatState.CBPara = token.Parameter;
				return;
			case RtfControlWord.Ctrl_LINE:
				ProcessHardLine(token, topFormatState);
				return;
			case RtfControlWord.Ctrl_LTRCH:
				topFormatState.DirChar = DirState.DirLTR;
				return;
			case RtfControlWord.Ctrl_LTRDOC:
			case RtfControlWord.Ctrl_LTRPAR:
			case RtfControlWord.Ctrl_LTRSECT:
				topFormatState.DirPara = DirState.DirLTR;
				return;
			case RtfControlWord.Ctrl_LTRMARK:
				ProcessText(new string('\u200e', 1));
				return;
			case RtfControlWord.Ctrl_PAR:
				goto IL_0dde;
			case RtfControlWord.Ctrl_PAGE:
				HandlePage(token, topFormatState);
				return;
			case RtfControlWord.Ctrl_CELL:
			case RtfControlWord.Ctrl_NESTCELL:
			case RtfControlWord.Ctrl_NESTROW:
			case RtfControlWord.Ctrl_NESTTABLEPROPS:
				goto IL_0df0;
			case RtfControlWord.Ctrl_BOX:
			case RtfControlWord.Ctrl_BRDRART:
			case RtfControlWord.Ctrl_BRDRB:
			case RtfControlWord.Ctrl_BRDRBAR:
			case RtfControlWord.Ctrl_BRDRBTW:
			case RtfControlWord.Ctrl_BRDRCF:
			case RtfControlWord.Ctrl_BRDRDASH:
			case RtfControlWord.Ctrl_BRDRDASHD:
			case RtfControlWord.Ctrl_BRDRDASHDD:
			case RtfControlWord.Ctrl_BRDRDASHDOTSTR:
			case RtfControlWord.Ctrl_BRDRDASHSM:
			case RtfControlWord.Ctrl_BRDRDB:
			case RtfControlWord.Ctrl_BRDRDOT:
			case RtfControlWord.Ctrl_BRDREMBOSS:
			case RtfControlWord.Ctrl_BRDRENGRAVE:
			case RtfControlWord.Ctrl_BRDRFRAME:
			case RtfControlWord.Ctrl_BRDRHAIR:
			case RtfControlWord.Ctrl_BRDRINSET:
			case RtfControlWord.Ctrl_BRDRL:
			case RtfControlWord.Ctrl_BRDROUTSET:
			case RtfControlWord.Ctrl_BRDRNIL:
			case RtfControlWord.Ctrl_BRDRNONE:
			case RtfControlWord.Ctrl_BRDRTBL:
			case RtfControlWord.Ctrl_BRDRR:
			case RtfControlWord.Ctrl_BRDRS:
			case RtfControlWord.Ctrl_BRDRSH:
			case RtfControlWord.Ctrl_BRDRT:
			case RtfControlWord.Ctrl_BRDRTH:
			case RtfControlWord.Ctrl_BRDRTHTNLG:
			case RtfControlWord.Ctrl_BRDRTHTNMG:
			case RtfControlWord.Ctrl_BRDRTHTNSG:
			case RtfControlWord.Ctrl_BRDRTNTHLG:
			case RtfControlWord.Ctrl_BRDRTNTHMG:
			case RtfControlWord.Ctrl_BRDRTNTHSG:
			case RtfControlWord.Ctrl_BRDRTNTHTNLG:
			case RtfControlWord.Ctrl_BRDRTNTHTNMG:
			case RtfControlWord.Ctrl_BRDRTNTHTNSG:
			case RtfControlWord.Ctrl_BRDRTRIPLE:
			case RtfControlWord.Ctrl_BRDRW:
			case RtfControlWord.Ctrl_BRDRWAVY:
			case RtfControlWord.Ctrl_BRDRWAVYDB:
			case RtfControlWord.Ctrl_BRSP:
			case RtfControlWord.Ctrl_CELLX:
			case RtfControlWord.Ctrl_CLBRDRB:
			case RtfControlWord.Ctrl_CLBRDRL:
			case RtfControlWord.Ctrl_CLBRDRR:
			case RtfControlWord.Ctrl_CLBRDRT:
			case RtfControlWord.Ctrl_CLCBPAT:
			case RtfControlWord.Ctrl_CLCFPAT:
			case RtfControlWord.Ctrl_CLFTSWIDTH:
			case RtfControlWord.Ctrl_CLMGF:
			case RtfControlWord.Ctrl_CLMRG:
			case RtfControlWord.Ctrl_CLPADB:
			case RtfControlWord.Ctrl_CLPADFB:
			case RtfControlWord.Ctrl_CLPADFL:
			case RtfControlWord.Ctrl_CLPADFR:
			case RtfControlWord.Ctrl_CLPADFT:
			case RtfControlWord.Ctrl_CLPADL:
			case RtfControlWord.Ctrl_CLPADR:
			case RtfControlWord.Ctrl_CLPADT:
			case RtfControlWord.Ctrl_CLSHDNG:
			case RtfControlWord.Ctrl_CLSHDRAWNIL:
			case RtfControlWord.Ctrl_CLVERTALB:
			case RtfControlWord.Ctrl_CLVERTALC:
			case RtfControlWord.Ctrl_CLVERTALT:
			case RtfControlWord.Ctrl_CLVMGF:
			case RtfControlWord.Ctrl_CLVMRG:
			case RtfControlWord.Ctrl_CLWWIDTH:
			case RtfControlWord.Ctrl_LTRROW:
				goto IL_0df9;
			case RtfControlWord.Ctrl_EMDASH:
				ProcessText("—");
				return;
			case RtfControlWord.Ctrl_ENDASH:
				ProcessText("–");
				return;
			case RtfControlWord.Ctrl_EMSPACE:
				ProcessText("\u2003");
				return;
			case RtfControlWord.Ctrl_ENSPACE:
				ProcessText("\u2002");
				return;
			case RtfControlWord.Ctrl_BULLET:
				ProcessText("•");
				return;
			case RtfControlWord.Ctrl_LQUOTE:
				ProcessText("‘");
				return;
			case RtfControlWord.Ctrl_LDBLQUOTE:
				ProcessText("“");
				return;
			case RtfControlWord.Ctrl_ANSI:
			case RtfControlWord.Ctrl_MAC:
			case RtfControlWord.Ctrl_PC:
			case RtfControlWord.Ctrl_PCA:
				goto IL_0e9e;
			case RtfControlWord.Ctrl_FIELD:
			case RtfControlWord.Ctrl_FLDINST:
			case RtfControlWord.Ctrl_FLDPRIV:
			case RtfControlWord.Ctrl_FLDRSLT:
				HandleFieldTokens(token, topFormatState);
				return;
			case RtfControlWord.Ctrl_ILVL:
				topFormatState.ILVL = token.Parameter;
				return;
			case RtfControlWord.Ctrl_INTBL:
				topFormatState.IsInTable = token.FlagValue;
				return;
			case RtfControlWord.Ctrl_LS:
				goto IL_0ee6;
			case RtfControlWord.Ctrl_ITAP:
				topFormatState.ITAP = token.Parameter;
				return;
			case RtfControlWord.Ctrl_BIN:
				HandleBinControl(token, topFormatState);
				return;
			case RtfControlWord.Ctrl_LEVELFOLLOW:
			case RtfControlWord.Ctrl_LEVELINDENT:
			case RtfControlWord.Ctrl_LEVELJC:
			case RtfControlWord.Ctrl_LEVELJCN:
			case RtfControlWord.Ctrl_LEVELNFC:
			case RtfControlWord.Ctrl_LEVELNFCN:
			case RtfControlWord.Ctrl_LEVELNUMBERS:
			case RtfControlWord.Ctrl_LEVELSPACE:
			case RtfControlWord.Ctrl_LEVELSTARTAT:
			case RtfControlWord.Ctrl_LEVELTEMPLATEID:
			case RtfControlWord.Ctrl_LEVELTEXT:
			case RtfControlWord.Ctrl_LIST:
			case RtfControlWord.Ctrl_LISTHYBRID:
			case RtfControlWord.Ctrl_LISTID:
			case RtfControlWord.Ctrl_LISTLEVEL:
			case RtfControlWord.Ctrl_LISTOVERRIDE:
			case RtfControlWord.Ctrl_LISTSIMPLE:
			case RtfControlWord.Ctrl_LISTTEMPLATEID:
			case RtfControlWord.Ctrl_LISTTEXT:
				HandleListTokens(token, topFormatState);
				return;
			case RtfControlWord.Ctrl_LISTPICTURE:
				topFormatState.RtfDestination = RtfDestination.DestListPicture;
				return;
			case RtfControlWord.Ctrl_DO:
			case RtfControlWord.Ctrl_DPTXBXTEXT:
			case RtfControlWord.Ctrl_NONSHPPICT:
				goto IL_0f36;
			case RtfControlWord.Ctrl_FONTTBL:
				topFormatState.RtfDestination = RtfDestination.DestFontTable;
				return;
			case RtfControlWord.Ctrl_COLORTBL:
				topFormatState.RtfDestination = RtfDestination.DestColorTable;
				return;
			case RtfControlWord.Ctrl_LISTTABLE:
				topFormatState.RtfDestination = RtfDestination.DestListTable;
				return;
			case RtfControlWord.Ctrl_LISTOVERRIDETABLE:
				topFormatState.RtfDestination = RtfDestination.DestListOverrideTable;
				return;
			case RtfControlWord.Ctrl_GREEN:
				if (topFormatState.RtfDestination == RtfDestination.DestColorTable)
				{
					_converterState.ColorTable.NewGreen = (byte)token.Parameter;
				}
				return;
			case RtfControlWord.Ctrl_BLUE:
				if (topFormatState.RtfDestination == RtfDestination.DestColorTable)
				{
					_converterState.ColorTable.NewBlue = (byte)token.Parameter;
				}
				return;
			case RtfControlWord.Ctrl_JPEGBLIP:
				if (topFormatState.RtfDestination == RtfDestination.DestPicture)
				{
					topFormatState.ImageFormat = RtfImageFormat.Jpeg;
				}
				return;
			case RtfControlWord.Ctrl_DN:
				if (topFormatState.RtfDestination == RtfDestination.DestPicture)
				{
					topFormatState.ImageBaselineOffset = Converters.HalfPointToPositivePx(token.Parameter);
					topFormatState.IncludeImageBaselineOffset = true;
				}
				return;
			default:
				return;
			}
			if (!token.HasParameter)
			{
				return;
			}
			if (topFormatState.RtfDestination == RtfDestination.DestFontTable)
			{
				_converterState.FontTable.DefineEntry((int)token.Parameter);
				return;
			}
			SelectFont(token.Parameter);
			if (topFormatState.FontSlot == FontSlot.DBCH)
			{
				if (topFormatState.LangFE > 0)
				{
					topFormatState.LangCur = topFormatState.LangFE;
				}
				else if (_converterState.DefaultLangFE > 0)
				{
					topFormatState.LangCur = _converterState.DefaultLangFE;
				}
			}
			else if (topFormatState.Lang > 0)
			{
				topFormatState.LangCur = topFormatState.Lang;
			}
			else if (_converterState.DefaultLang > 0)
			{
				topFormatState.LangCur = _converterState.DefaultLang;
			}
			return;
		}
		switch (control)
		{
		case RtfControlWord.Ctrl_V:
			topFormatState.IsHidden = token.ToggleValue > 0;
			return;
		case RtfControlWord.Ctrl_SHAD:
			topFormatState.Shadow = token.ToggleValue > 0;
			return;
		case RtfControlWord.Ctrl_SCAPS:
			topFormatState.SCaps = token.ToggleValue > 0;
			return;
		case RtfControlWord.Ctrl_SUB:
			topFormatState.Sub = token.FlagValue;
			if (topFormatState.Sub)
			{
				topFormatState.Super = false;
			}
			return;
		case RtfControlWord.Ctrl_SUPER:
			topFormatState.Super = token.FlagValue;
			if (topFormatState.Super)
			{
				topFormatState.Sub = false;
			}
			return;
		case RtfControlWord.Ctrl_UP:
			topFormatState.SuperOffset = token.Parameter;
			return;
		case RtfControlWord.Ctrl_UL:
		case RtfControlWord.Ctrl_ULD:
		case RtfControlWord.Ctrl_ULDASH:
		case RtfControlWord.Ctrl_ULDASHD:
		case RtfControlWord.Ctrl_ULDASHDD:
		case RtfControlWord.Ctrl_ULDB:
		case RtfControlWord.Ctrl_ULHAIR:
		case RtfControlWord.Ctrl_ULHWAVE:
		case RtfControlWord.Ctrl_ULLDASH:
		case RtfControlWord.Ctrl_ULTH:
		case RtfControlWord.Ctrl_ULTHD:
		case RtfControlWord.Ctrl_ULTHDASH:
		case RtfControlWord.Ctrl_ULTHDASHD:
		case RtfControlWord.Ctrl_ULTHDASHDD:
		case RtfControlWord.Ctrl_ULTHLDASH:
		case RtfControlWord.Ctrl_ULULDBWAVE:
		case RtfControlWord.Ctrl_ULW:
		case RtfControlWord.Ctrl_ULWAVE:
			topFormatState.UL = ((token.ToggleValue > 0) ? ULState.ULNormal : ULState.ULNone);
			return;
		case RtfControlWord.Ctrl_ULNONE:
			topFormatState.UL = ULState.ULNone;
			return;
		case RtfControlWord.Ctrl_STRIKE:
			topFormatState.Strike = ((token.ToggleValue > 0) ? StrikeState.StrikeNormal : StrikeState.StrikeNone);
			return;
		case RtfControlWord.Ctrl_STRIKED:
			topFormatState.Strike = ((token.ToggleValue > 0) ? StrikeState.StrikeDouble : StrikeState.StrikeNone);
			return;
		case RtfControlWord.Ctrl_PLAIN:
			topFormatState.SetCharDefaults();
			if (_converterState.DefaultFont >= 0)
			{
				SelectFont(_converterState.DefaultFont);
			}
			return;
		case RtfControlWord.Ctrl_SB:
			topFormatState.SB = token.Parameter;
			return;
		case RtfControlWord.Ctrl_SA:
			topFormatState.SA = token.Parameter;
			return;
		case RtfControlWord.Ctrl_RI:
			topFormatState.RI = token.Parameter;
			return;
		case RtfControlWord.Ctrl_QC:
			topFormatState.HAlign = HAlign.AlignCenter;
			return;
		case RtfControlWord.Ctrl_QJ:
			topFormatState.HAlign = HAlign.AlignJustify;
			return;
		case RtfControlWord.Ctrl_QL:
			topFormatState.HAlign = HAlign.AlignLeft;
			return;
		case RtfControlWord.Ctrl_QR:
			topFormatState.HAlign = HAlign.AlignRight;
			return;
		case RtfControlWord.Ctrl_SHADING:
			topFormatState.ParaShading = token.Parameter;
			return;
		case RtfControlWord.Ctrl_SL:
			topFormatState.SL = token.Parameter;
			return;
		case RtfControlWord.Ctrl_SLMULT:
			topFormatState.SLMult = token.ToggleValue > 0;
			return;
		case RtfControlWord.Ctrl_RTLCH:
			topFormatState.DirChar = DirState.DirRTL;
			return;
		case RtfControlWord.Ctrl_RTLDOC:
		case RtfControlWord.Ctrl_RTLPAR:
		case RtfControlWord.Ctrl_RTLSECT:
			topFormatState.DirPara = DirState.DirRTL;
			if (topFormatState.HAlign == HAlign.AlignDefault)
			{
				topFormatState.HAlign = HAlign.AlignLeft;
			}
			return;
		case RtfControlWord.Ctrl_RTLMARK:
			ProcessText(new string('\u200f', 1));
			return;
		case RtfControlWord.Ctrl_SECT:
			break;
		case RtfControlWord.Ctrl_ROW:
		case RtfControlWord.Ctrl_TROWD:
			goto IL_0df0;
		case RtfControlWord.Ctrl_RTLROW:
		case RtfControlWord.Ctrl_TRAUTOFIT:
		case RtfControlWord.Ctrl_TRBRDRB:
		case RtfControlWord.Ctrl_TRBRDRH:
		case RtfControlWord.Ctrl_TRBRDRL:
		case RtfControlWord.Ctrl_TRBRDRR:
		case RtfControlWord.Ctrl_TRBRDRT:
		case RtfControlWord.Ctrl_TRBRDRV:
		case RtfControlWord.Ctrl_TRFTSWIDTHA:
		case RtfControlWord.Ctrl_TRFTSWIDTHB:
		case RtfControlWord.Ctrl_TRFTSWIDTH:
		case RtfControlWord.Ctrl_TRGAPH:
		case RtfControlWord.Ctrl_TRLEFT:
		case RtfControlWord.Ctrl_TRPADDB:
		case RtfControlWord.Ctrl_TRPADDFB:
		case RtfControlWord.Ctrl_TRPADDFL:
		case RtfControlWord.Ctrl_TRPADDFR:
		case RtfControlWord.Ctrl_TRPADDFT:
		case RtfControlWord.Ctrl_TRPADDL:
		case RtfControlWord.Ctrl_TRPADDR:
		case RtfControlWord.Ctrl_TRPADDT:
		case RtfControlWord.Ctrl_TRQC:
		case RtfControlWord.Ctrl_TRQL:
		case RtfControlWord.Ctrl_TRQR:
		case RtfControlWord.Ctrl_TRSPDB:
		case RtfControlWord.Ctrl_TRSPDFB:
		case RtfControlWord.Ctrl_TRSPDFL:
		case RtfControlWord.Ctrl_TRSPDFR:
		case RtfControlWord.Ctrl_TRSPDFT:
		case RtfControlWord.Ctrl_TRSPDL:
		case RtfControlWord.Ctrl_TRSPDR:
		case RtfControlWord.Ctrl_TRSPDT:
		case RtfControlWord.Ctrl_TRWWIDTHA:
		case RtfControlWord.Ctrl_TRWWIDTHB:
		case RtfControlWord.Ctrl_TRWWIDTH:
			goto IL_0df9;
		case RtfControlWord.Ctrl_TAB:
			ProcessText("\t");
			return;
		case RtfControlWord.Ctrl_QMSPACE:
			ProcessText("\u2005");
			return;
		case RtfControlWord.Ctrl_RQUOTE:
			ProcessText("’");
			return;
		case RtfControlWord.Ctrl_RDBLQUOTE:
			ProcessText("”");
			return;
		case RtfControlWord.Ctrl_ZWJ:
			ProcessText("\u200d");
			return;
		case RtfControlWord.Ctrl_ZWNJ:
			ProcessText("\u200c");
			return;
		case RtfControlWord.Ctrl_UC:
		case RtfControlWord.Ctrl_UD:
		case RtfControlWord.Ctrl_UPR:
			goto IL_0e9e;
		case RtfControlWord.Ctrl_U:
			HandleCodePageTokens(token, topFormatState);
			_lexer.AdvanceForUnicode(topFormatState.UnicodeSkip);
			return;
		case RtfControlWord.Ctrl_PN:
		case RtfControlWord.Ctrl_PNBIDIA:
		case RtfControlWord.Ctrl_PNBIDIB:
		case RtfControlWord.Ctrl_PNCARD:
		case RtfControlWord.Ctrl_PNDEC:
		case RtfControlWord.Ctrl_PNLCLTR:
		case RtfControlWord.Ctrl_PNLCRM:
		case RtfControlWord.Ctrl_PNLVL:
		case RtfControlWord.Ctrl_PNLVLBLT:
		case RtfControlWord.Ctrl_PNLVLBODY:
		case RtfControlWord.Ctrl_PNLVLCONT:
		case RtfControlWord.Ctrl_PNORD:
		case RtfControlWord.Ctrl_PNORDT:
		case RtfControlWord.Ctrl_PNSTART:
		case RtfControlWord.Ctrl_PNTEXT:
		case RtfControlWord.Ctrl_PNTXTA:
		case RtfControlWord.Ctrl_PNTXTB:
		case RtfControlWord.Ctrl_PNUCLTR:
		case RtfControlWord.Ctrl_PNUCRM:
			HandleOldListTokens(token, topFormatState);
			return;
		case RtfControlWord.Ctrl_SHPPICT:
		case RtfControlWord.Ctrl_SHPRSLT:
			goto IL_0f36;
		case RtfControlWord.Ctrl_RED:
			if (topFormatState.RtfDestination == RtfDestination.DestColorTable)
			{
				_converterState.ColorTable.NewRed = (byte)token.Parameter;
			}
			return;
		case RtfControlWord.Ctrl_SHPINST:
			topFormatState.RtfDestination = RtfDestination.DestShapeInstruction;
			return;
		case RtfControlWord.Ctrl_PICT:
		{
			FormatState formatState = _converterState.PreviousTopFormatState(1);
			if (formatState.RtfDestination == RtfDestination.DestShapePicture || formatState.RtfDestination == RtfDestination.DestShapeInstruction || (formatState.RtfDestination != RtfDestination.DestNoneShapePicture && formatState.RtfDestination != RtfDestination.DestShape && formatState.RtfDestination != RtfDestination.DestListPicture))
			{
				topFormatState.RtfDestination = RtfDestination.DestPicture;
			}
			return;
		}
		case RtfControlWord.Ctrl_PNGBLIP:
			if (topFormatState.RtfDestination == RtfDestination.DestPicture)
			{
				topFormatState.ImageFormat = RtfImageFormat.Png;
			}
			return;
		case RtfControlWord.Ctrl_WMETAFILE:
			if (topFormatState.RtfDestination == RtfDestination.DestPicture)
			{
				topFormatState.ImageFormat = RtfImageFormat.Wmf;
			}
			return;
		case RtfControlWord.Ctrl_PICHGOAL:
			if (topFormatState.RtfDestination == RtfDestination.DestPicture)
			{
				topFormatState.ImageHeight = Converters.TwipToPositivePx(token.Parameter);
			}
			return;
		case RtfControlWord.Ctrl_PICWGOAL:
			if (topFormatState.RtfDestination == RtfDestination.DestPicture)
			{
				topFormatState.ImageWidth = Converters.TwipToPositivePx(token.Parameter);
			}
			return;
		case RtfControlWord.Ctrl_PICSCALEX:
			if (topFormatState.RtfDestination == RtfDestination.DestPicture)
			{
				topFormatState.ImageScaleWidth = token.Parameter;
			}
			return;
		case RtfControlWord.Ctrl_PICSCALEY:
			if (topFormatState.RtfDestination == RtfDestination.DestPicture)
			{
				topFormatState.ImageScaleHeight = token.Parameter;
			}
			return;
		default:
			return;
		}
		goto IL_0dde;
		IL_0f36:
		HandleShapeTokens(token, topFormatState);
		return;
		IL_0e9e:
		HandleCodePageTokens(token, topFormatState);
		return;
		IL_0df9:
		HandleTableProperties(token, topFormatState);
		return;
		IL_0ee6:
		if (topFormatState.RtfDestination == RtfDestination.DestListOverride)
		{
			HandleListTokens(token, topFormatState);
		}
		else
		{
			topFormatState.ILS = token.Parameter;
		}
		return;
		IL_0df0:
		HandleTableTokens(token, topFormatState);
		return;
		IL_0dde:
		HandlePara(token, topFormatState);
	}

	internal void ProcessText(RtfToken token)
	{
		FormatState topFormatState = _converterState.TopFormatState;
		if (!topFormatState.IsHidden)
		{
			switch (topFormatState.RtfDestination)
			{
			case RtfDestination.DestNormal:
			case RtfDestination.DestListText:
			case RtfDestination.DestFieldResult:
			case RtfDestination.DestShapeResult:
				HandleNormalText(token.Text, topFormatState);
				break;
			case RtfDestination.DestFontTable:
			case RtfDestination.DestFontName:
				ProcessFontTableText(token);
				break;
			case RtfDestination.DestColorTable:
				ProcessColorTableText(token);
				break;
			case RtfDestination.DestField:
			case RtfDestination.DestFieldInstruction:
			case RtfDestination.DestFieldPrivate:
				ProcessFieldText(token);
				break;
			case RtfDestination.DestListTable:
			case RtfDestination.DestListOverrideTable:
			case RtfDestination.DestList:
			case RtfDestination.DestListLevel:
			case RtfDestination.DestListOverride:
			case RtfDestination.DestListPicture:
			case RtfDestination.DestUPR:
			case RtfDestination.DestShape:
			case RtfDestination.DestShapeInstruction:
				break;
			}
		}
	}

	internal void ProcessTextSymbol(RtfToken token)
	{
		if (token.Text.Length != 0)
		{
			SetTokenTextWithControlCharacter(token);
			switch (_converterState.TopFormatState.RtfDestination)
			{
			case RtfDestination.DestNormal:
			case RtfDestination.DestListText:
			case RtfDestination.DestFieldResult:
			case RtfDestination.DestShapeResult:
				HandleNormalText(token.Text, _converterState.TopFormatState);
				break;
			case RtfDestination.DestFontTable:
				ProcessFontTableText(token);
				break;
			case RtfDestination.DestColorTable:
				ProcessColorTableText(token);
				break;
			case RtfDestination.DestField:
			case RtfDestination.DestFieldInstruction:
			case RtfDestination.DestFieldPrivate:
				ProcessFieldText(token);
				break;
			}
		}
	}

	internal void HandleBinControl(RtfToken token, FormatState formatState)
	{
		if (token.Parameter > 0)
		{
			if (formatState.RtfDestination == RtfDestination.DestPicture)
			{
				formatState.IsImageDataBinary = true;
			}
			else
			{
				_lexer.AdvanceForBinary((int)token.Parameter);
			}
		}
	}

	internal void HandlePara(RtfToken token, FormatState formatState)
	{
		if (formatState.IsContentDestination && !formatState.IsHidden)
		{
			HandleParagraphFromText(formatState);
			HandleTableNesting(formatState);
			HandleListNesting(formatState);
		}
	}

	internal void WrapPendingInlineInParagraph(RtfToken token, FormatState formatState)
	{
		if (!formatState.IsContentDestination || formatState.IsHidden)
		{
			return;
		}
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int num = documentNodeArray.Count;
		int num2;
		for (num2 = documentNodeArray.Count; num2 > 0; num2--)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(num2 - 1);
			if (!documentNode.IsInline || documentNode.ClosedParent != null || !documentNode.IsMatched)
			{
				break;
			}
			if (documentNode.Type == DocumentNodeType.dnListText && !documentNode.IsPending && num2 + documentNode.ChildCount == documentNodeArray.Count)
			{
				num = num2 - 1;
			}
		}
		if (num2 != num)
		{
			HandlePara(token, formatState);
		}
	}

	internal void HandlePage(RtfToken token, FormatState formatState)
	{
		WrapPendingInlineInParagraph(token, formatState);
	}

	internal void HandleParagraphFromText(FormatState formatState)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int num;
		DocumentNode documentNode;
		for (num = documentNodeArray.Count; num > 0; num--)
		{
			documentNode = documentNodeArray.EntryAt(num - 1);
			if (!documentNode.IsInline || (documentNode.ClosedParent != null && !documentNode.ClosedParent.IsInline) || !documentNode.IsMatched)
			{
				break;
			}
		}
		documentNode = new DocumentNode(DocumentNodeType.dnParagraph);
		documentNode.FormatState = new FormatState(formatState);
		documentNode.ConstrainFontPropagation(formatState);
		documentNodeArray.InsertNode(num, documentNode);
		documentNodeArray.CloseAt(num);
		documentNodeArray.CoalesceOnlyChildren(_converterState, num);
	}

	internal void WrapInlineInParagraph(int nInsertAt, int nChildren)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		DocumentNode documentNode = documentNodeArray.EntryAt(nInsertAt + nChildren - 1);
		DocumentNode documentNode2 = new DocumentNode(DocumentNodeType.dnParagraph);
		documentNode2.FormatState = new FormatState(documentNode.FormatState);
		documentNode2.ConstrainFontPropagation(documentNode2.FormatState);
		DocumentNode dnParent = null;
		if (documentNode.ClosedParent != null && documentNode.ClosedParent.Index < nInsertAt && documentNode.ClosedParent.Index > nInsertAt + nChildren - 1)
		{
			dnParent = documentNode.ClosedParent;
		}
		documentNodeArray.InsertChildAt(dnParent, documentNode2, nInsertAt, nChildren);
		documentNodeArray.CoalesceOnlyChildren(_converterState, nInsertAt);
	}

	internal void ProcessPendingTextAtRowEnd()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int num = 0;
		for (int num2 = documentNodeArray.Count - 1; num2 >= 0; num2--)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(num2);
			if (!documentNode.IsInline || documentNode.ClosedParent != null)
			{
				break;
			}
			num++;
		}
		if (num > 0)
		{
			documentNodeArray.Excise(documentNodeArray.Count - num, num);
		}
	}

	internal void HandleTableTokens(RtfToken token, FormatState formatState)
	{
		FormatState formatState2 = _converterState.PreviousTopFormatState(0);
		FormatState formatState3 = _converterState.PreviousTopFormatState(1);
		if (formatState2 == null || formatState3 == null)
		{
			return;
		}
		if (token.RtfControlWordInfo.Control == RtfControlWord.Ctrl_NESTTABLEPROPS)
		{
			formatState2.RtfDestination = formatState3.RtfDestination;
		}
		if (!formatState.IsContentDestination)
		{
			return;
		}
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		bool isHidden = formatState.IsHidden;
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_CELL:
		{
			formatState.IsInTable = true;
			formatState.ITAP = 1L;
			formatState.IsHidden = false;
			HandlePara(token, formatState);
			formatState.IsHidden = isHidden;
			int num3 = documentNodeArray.FindPending(DocumentNodeType.dnCell);
			if (num3 >= 0)
			{
				documentNodeArray.CloseAt(num3);
			}
			break;
		}
		case RtfControlWord.Ctrl_NESTCELL:
		{
			formatState.IsHidden = false;
			HandlePara(token, formatState);
			formatState.IsHidden = isHidden;
			int num4 = documentNodeArray.CountOpenCells();
			if (documentNodeArray.GetTableScope() != DocumentNodeType.dnCell || num4 < 2)
			{
				HandlePara(token, formatState);
			}
			int num3 = documentNodeArray.FindPending(DocumentNodeType.dnCell);
			if (num3 >= 0)
			{
				documentNodeArray.CloseAt(num3);
			}
			break;
		}
		case RtfControlWord.Ctrl_TROWD:
			formatState.IsHidden = false;
			formatState.SetRowDefaults();
			formatState.IsHidden = isHidden;
			break;
		case RtfControlWord.Ctrl_NESTROW:
		case RtfControlWord.Ctrl_ROW:
		{
			formatState.IsHidden = false;
			int num = documentNodeArray.FindPending(DocumentNodeType.dnRow);
			if (num >= 0)
			{
				DocumentNode documentNode = documentNodeArray.EntryAt(num);
				if (formatState.RowFormat != null)
				{
					documentNode.FormatState.RowFormat = new RowFormat(formatState.RowFormat);
					documentNode.FormatState.RowFormat.CanonicalizeWidthsFromRTF();
					int num2 = documentNodeArray.FindPendingFrom(DocumentNodeType.dnTable, num - 1, -1);
					if (num2 >= 0)
					{
						DocumentNode documentNode2 = documentNodeArray.EntryAt(num2);
						if (!documentNode2.FormatState.HasRowFormat)
						{
							documentNode2.FormatState.RowFormat = documentNode.FormatState.RowFormat;
						}
					}
				}
				ProcessPendingTextAtRowEnd();
				documentNodeArray.CloseAt(num);
			}
			formatState.IsHidden = isHidden;
			break;
		}
		}
	}

	internal ListOverride GetControllingListOverride()
	{
		_ = _converterState.ListTable;
		ListOverrideTable listOverrideTable = _converterState.ListOverrideTable;
		RtfFormatStack rtfFormatStack = _converterState.RtfFormatStack;
		for (int num = rtfFormatStack.Count - 1; num >= 0; num--)
		{
			FormatState formatState = rtfFormatStack.EntryAt(num);
			if (formatState.RtfDestination == RtfDestination.DestListOverride)
			{
				return listOverrideTable.CurrentEntry;
			}
			if (formatState.RtfDestination == RtfDestination.DestListTable)
			{
				return null;
			}
		}
		return null;
	}

	internal ListLevelTable GetControllingLevelTable()
	{
		ListTable listTable = _converterState.ListTable;
		ListOverrideTable listOverrideTable = _converterState.ListOverrideTable;
		RtfFormatStack rtfFormatStack = _converterState.RtfFormatStack;
		for (int num = rtfFormatStack.Count - 1; num >= 0; num--)
		{
			FormatState formatState = rtfFormatStack.EntryAt(num);
			if (formatState.RtfDestination == RtfDestination.DestListOverride)
			{
				ListOverride currentEntry = listOverrideTable.CurrentEntry;
				if (currentEntry.Levels == null)
				{
					currentEntry.Levels = new ListLevelTable();
				}
				return currentEntry.Levels;
			}
			if (formatState.RtfDestination == RtfDestination.DestListTable)
			{
				ListTableEntry currentEntry2 = listTable.CurrentEntry;
				if (currentEntry2 != null)
				{
					return currentEntry2.Levels;
				}
			}
		}
		return null;
	}

	internal void HandleListTokens(RtfToken token, FormatState formatState)
	{
		ListTable listTable = _converterState.ListTable;
		ListOverrideTable listOverrideTable = _converterState.ListOverrideTable;
		FormatState formatState2 = _converterState.PreviousTopFormatState(0);
		FormatState formatState3 = _converterState.PreviousTopFormatState(1);
		if (formatState2 == null || formatState3 == null)
		{
			return;
		}
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_LIST:
			if (formatState.RtfDestination == RtfDestination.DestListTable)
			{
				listTable.AddEntry();
			}
			break;
		case RtfControlWord.Ctrl_LISTTEMPLATEID:
		{
			ListTableEntry currentEntry3 = listTable.CurrentEntry;
			if (currentEntry3 != null)
			{
				currentEntry3.TemplateID = token.Parameter;
			}
			break;
		}
		case RtfControlWord.Ctrl_LISTHYBRID:
		case RtfControlWord.Ctrl_LISTSIMPLE:
		{
			ListTableEntry currentEntry7 = listTable.CurrentEntry;
			if (currentEntry7 != null)
			{
				currentEntry7.Simple = token.RtfControlWordInfo.Control == RtfControlWord.Ctrl_LISTSIMPLE;
			}
			break;
		}
		case RtfControlWord.Ctrl_LISTLEVEL:
			formatState.RtfDestination = RtfDestination.DestListLevel;
			GetControllingLevelTable()?.AddEntry();
			break;
		case RtfControlWord.Ctrl_LISTTEXT:
			if (formatState3.IsContentDestination || formatState.IsHidden)
			{
				formatState.RtfDestination = RtfDestination.DestListText;
				_converterState.DocumentNodeArray.Push(new DocumentNode(DocumentNodeType.dnListText)
				{
					FormatState = new FormatState(formatState)
				});
			}
			break;
		case RtfControlWord.Ctrl_LEVELNFC:
		case RtfControlWord.Ctrl_LEVELNFCN:
		{
			ListLevelTable controllingLevelTable2 = GetControllingLevelTable();
			if (controllingLevelTable2 != null)
			{
				ListLevel currentEntry6 = controllingLevelTable2.CurrentEntry;
				if (currentEntry6 != null)
				{
					currentEntry6.Marker = (MarkerStyle)token.Parameter;
				}
			}
			break;
		}
		case RtfControlWord.Ctrl_LEVELSTARTAT:
		{
			ListLevelTable controllingLevelTable = GetControllingLevelTable();
			if (controllingLevelTable == null)
			{
				break;
			}
			ListLevel currentEntry2 = controllingLevelTable.CurrentEntry;
			if (currentEntry2 != null)
			{
				currentEntry2.StartIndex = token.Parameter;
				break;
			}
			ListOverride controllingListOverride = GetControllingListOverride();
			if (controllingListOverride != null)
			{
				controllingListOverride.StartIndex = token.Parameter;
			}
			break;
		}
		case RtfControlWord.Ctrl_LISTID:
			if (formatState.RtfDestination == RtfDestination.DestListOverride)
			{
				ListOverride currentEntry4 = listOverrideTable.CurrentEntry;
				if (currentEntry4 != null)
				{
					currentEntry4.ID = token.Parameter;
				}
			}
			else
			{
				ListTableEntry currentEntry5 = listTable.CurrentEntry;
				if (currentEntry5 != null)
				{
					currentEntry5.ID = token.Parameter;
				}
			}
			break;
		case RtfControlWord.Ctrl_LISTOVERRIDE:
			if (_converterState.PreviousTopFormatState(1).RtfDestination == RtfDestination.DestListOverrideTable)
			{
				formatState.RtfDestination = RtfDestination.DestListOverride;
				listOverrideTable.AddEntry();
			}
			break;
		case RtfControlWord.Ctrl_LS:
			if (formatState.RtfDestination == RtfDestination.DestListOverride)
			{
				ListOverride currentEntry = listOverrideTable.CurrentEntry;
				if (currentEntry != null)
				{
					currentEntry.Index = token.Parameter;
				}
			}
			break;
		}
	}

	internal void HandleShapeTokens(RtfToken token, FormatState formatState)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		FormatState formatState2 = _converterState.PreviousTopFormatState(0);
		FormatState formatState3 = _converterState.PreviousTopFormatState(1);
		if (formatState2 == null || formatState3 == null)
		{
			return;
		}
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_DO:
			formatState2.RtfDestination = formatState3.RtfDestination;
			break;
		case RtfControlWord.Ctrl_SHPRSLT:
			if (formatState3.IsContentDestination)
			{
				formatState2.RtfDestination = RtfDestination.DestShape;
			}
			break;
		case RtfControlWord.Ctrl_DPTXBXTEXT:
		{
			if (!formatState3.IsContentDestination)
			{
				break;
			}
			formatState2.RtfDestination = RtfDestination.DestShapeResult;
			WrapPendingInlineInParagraph(token, formatState);
			switch (documentNodeArray.GetTableScope())
			{
			case DocumentNodeType.dnTableBody:
			{
				int num2 = documentNodeArray.FindPending(DocumentNodeType.dnTable);
				if (num2 >= 0)
				{
					documentNodeArray.CloseAt(num2);
					documentNodeArray.CoalesceChildren(_converterState, num2);
				}
				break;
			}
			default:
				documentNodeArray.OpenLastCell();
				break;
			case DocumentNodeType.dnParagraph:
				break;
			}
			DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnShape);
			formatState.SetParaDefaults();
			formatState.SetCharDefaults();
			documentNode.FormatState = new FormatState(formatState);
			documentNodeArray.Push(documentNode);
			break;
		}
		case RtfControlWord.Ctrl_SHPPICT:
		{
			int num = documentNodeArray.FindPending(DocumentNodeType.dnListText);
			if (num >= 0)
			{
				documentNodeArray.EntryAt(num).HasMarkerContent = true;
			}
			if (formatState3.RtfDestination == RtfDestination.DestListPicture)
			{
				formatState.RtfDestination = RtfDestination.DestListPicture;
			}
			else
			{
				formatState.RtfDestination = RtfDestination.DestShapePicture;
			}
			break;
		}
		case RtfControlWord.Ctrl_NONSHPPICT:
			formatState.RtfDestination = RtfDestination.DestNoneShapePicture;
			break;
		}
	}

	internal void HandleOldListTokens(RtfToken token, FormatState formatState)
	{
		FormatState formatState2 = _converterState.PreviousTopFormatState(0);
		FormatState formatState3 = _converterState.PreviousTopFormatState(1);
		if (formatState2 == null || formatState3 == null)
		{
			return;
		}
		if (formatState.RtfDestination == RtfDestination.DestPN)
		{
			formatState = formatState3;
		}
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_PNLVL:
			formatState.PNLVL = token.Parameter;
			break;
		case RtfControlWord.Ctrl_PNLVLBLT:
			formatState.Marker = MarkerStyle.MarkerBullet;
			formatState.IsContinue = false;
			break;
		case RtfControlWord.Ctrl_PNLVLBODY:
			formatState.Marker = MarkerStyle.MarkerArabic;
			formatState.IsContinue = false;
			break;
		case RtfControlWord.Ctrl_PNLVLCONT:
			formatState.IsContinue = true;
			break;
		case RtfControlWord.Ctrl_PNCARD:
			formatState.Marker = MarkerStyle.MarkerCardinal;
			break;
		case RtfControlWord.Ctrl_PNDEC:
			formatState.Marker = MarkerStyle.MarkerArabic;
			break;
		case RtfControlWord.Ctrl_PNUCLTR:
			formatState.Marker = MarkerStyle.MarkerUpperAlpha;
			break;
		case RtfControlWord.Ctrl_PNUCRM:
			formatState.Marker = MarkerStyle.MarkerUpperRoman;
			break;
		case RtfControlWord.Ctrl_PNLCLTR:
			formatState.Marker = MarkerStyle.MarkerLowerAlpha;
			break;
		case RtfControlWord.Ctrl_PNLCRM:
			formatState.Marker = MarkerStyle.MarkerLowerRoman;
			break;
		case RtfControlWord.Ctrl_PNORD:
			formatState.Marker = MarkerStyle.MarkerOrdinal;
			break;
		case RtfControlWord.Ctrl_PNORDT:
			formatState.Marker = MarkerStyle.MarkerOrdinal;
			break;
		case RtfControlWord.Ctrl_PNBIDIA:
			formatState.Marker = MarkerStyle.MarkerArabic;
			break;
		case RtfControlWord.Ctrl_PNBIDIB:
			formatState.Marker = MarkerStyle.MarkerArabic;
			break;
		case RtfControlWord.Ctrl_PN:
			formatState.RtfDestination = RtfDestination.DestPN;
			formatState3.Marker = MarkerStyle.MarkerBullet;
			break;
		case RtfControlWord.Ctrl_PNTEXT:
			if (formatState3.IsContentDestination || formatState.IsHidden)
			{
				formatState2.RtfDestination = RtfDestination.DestListText;
				_converterState.DocumentNodeArray.Push(new DocumentNode(DocumentNodeType.dnListText)
				{
					FormatState = new FormatState(formatState)
				});
			}
			break;
		case RtfControlWord.Ctrl_PNSTART:
			formatState.StartIndex = token.Parameter;
			break;
		default:
			formatState.Marker = MarkerStyle.MarkerBullet;
			break;
		case RtfControlWord.Ctrl_PNTXTA:
		case RtfControlWord.Ctrl_PNTXTB:
			break;
		}
	}

	internal void HandleTableProperties(RtfToken token, FormatState formatState)
	{
		if (!formatState.IsContentDestination)
		{
			return;
		}
		CellFormat cellFormat = null;
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_TRLEFT:
			formatState.RowFormat.Trleft = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRPADDL:
			formatState.RowFormat.RowCellFormat.PaddingLeft = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRPADDR:
			formatState.RowFormat.RowCellFormat.PaddingRight = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRPADDB:
			formatState.RowFormat.RowCellFormat.PaddingBottom = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRPADDT:
			formatState.RowFormat.RowCellFormat.PaddingTop = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRSPDFB:
			if (token.Parameter == 0L)
			{
				formatState.RowFormat.RowCellFormat.SpacingBottom = 0L;
			}
			break;
		case RtfControlWord.Ctrl_TRSPDFL:
			if (token.Parameter == 0L)
			{
				formatState.RowFormat.RowCellFormat.SpacingLeft = 0L;
			}
			break;
		case RtfControlWord.Ctrl_TRSPDFR:
			if (token.Parameter == 0L)
			{
				formatState.RowFormat.RowCellFormat.SpacingRight = 0L;
			}
			break;
		case RtfControlWord.Ctrl_TRSPDFT:
			if (token.Parameter == 0L)
			{
				formatState.RowFormat.RowCellFormat.SpacingTop = 0L;
			}
			break;
		case RtfControlWord.Ctrl_TRSPDB:
			formatState.RowFormat.RowCellFormat.SpacingBottom = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRSPDL:
			formatState.RowFormat.RowCellFormat.SpacingLeft = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRSPDR:
			formatState.RowFormat.RowCellFormat.SpacingRight = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRSPDT:
			formatState.RowFormat.RowCellFormat.SpacingTop = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRWWIDTH:
			formatState.RowFormat.WidthRow.Value = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRFTSWIDTH:
			if (Validators.IsValidWidthType(token.Parameter))
			{
				formatState.RowFormat.WidthRow.Type = (WidthType)token.Parameter;
			}
			break;
		case RtfControlWord.Ctrl_TRWWIDTHA:
			formatState.RowFormat.WidthA.Value = token.Parameter;
			break;
		case RtfControlWord.Ctrl_TRFTSWIDTHA:
			if (Validators.IsValidWidthType(token.Parameter))
			{
				formatState.RowFormat.WidthA.Type = (WidthType)token.Parameter;
			}
			break;
		case RtfControlWord.Ctrl_TRAUTOFIT:
			if (token.ToggleValue > 0)
			{
				formatState.RowFormat.WidthRow.SetDefaults();
			}
			break;
		case RtfControlWord.Ctrl_CLWWIDTH:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.Width.Value = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLFTSWIDTH:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			if (Validators.IsValidWidthType(token.Parameter))
			{
				cellFormat.Width.Type = (WidthType)token.Parameter;
			}
			break;
		case RtfControlWord.Ctrl_TRBRDRT:
			ConverterState.CurrentBorder = formatState.RowFormat.RowCellFormat.BorderTop;
			break;
		case RtfControlWord.Ctrl_TRBRDRB:
			ConverterState.CurrentBorder = formatState.RowFormat.RowCellFormat.BorderBottom;
			break;
		case RtfControlWord.Ctrl_TRBRDRR:
			ConverterState.CurrentBorder = formatState.RowFormat.RowCellFormat.BorderRight;
			break;
		case RtfControlWord.Ctrl_TRBRDRL:
			ConverterState.CurrentBorder = formatState.RowFormat.RowCellFormat.BorderLeft;
			break;
		case RtfControlWord.Ctrl_TRBRDRV:
			ConverterState.CurrentBorder = formatState.RowFormat.RowCellFormat.BorderLeft;
			break;
		case RtfControlWord.Ctrl_TRBRDRH:
			ConverterState.CurrentBorder = formatState.RowFormat.RowCellFormat.BorderTop;
			break;
		case RtfControlWord.Ctrl_CLVERTALT:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.VAlign = VAlign.AlignTop;
			break;
		case RtfControlWord.Ctrl_CLVERTALB:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.VAlign = VAlign.AlignBottom;
			break;
		case RtfControlWord.Ctrl_CLVERTALC:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.VAlign = VAlign.AlignCenter;
			break;
		case RtfControlWord.Ctrl_CLSHDNG:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.Shading = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLSHDRAWNIL:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.Shading = -1L;
			cellFormat.CB = -1L;
			cellFormat.CF = -1L;
			break;
		case RtfControlWord.Ctrl_CLBRDRB:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			ConverterState.CurrentBorder = cellFormat.BorderBottom;
			break;
		case RtfControlWord.Ctrl_CLBRDRR:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			ConverterState.CurrentBorder = cellFormat.BorderRight;
			break;
		case RtfControlWord.Ctrl_CLBRDRT:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			ConverterState.CurrentBorder = cellFormat.BorderTop;
			break;
		case RtfControlWord.Ctrl_CLBRDRL:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			ConverterState.CurrentBorder = cellFormat.BorderLeft;
			break;
		case RtfControlWord.Ctrl_CLCBPAT:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.CB = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLCFPAT:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.CF = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLPADL:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.PaddingLeft = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLPADR:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.PaddingRight = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLPADB:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.PaddingBottom = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CLPADT:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.PaddingTop = token.Parameter;
			break;
		case RtfControlWord.Ctrl_CELLX:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.CellX = token.Parameter;
			cellFormat.IsPending = false;
			break;
		case RtfControlWord.Ctrl_RTLROW:
			formatState.RowFormat.Dir = DirState.DirRTL;
			break;
		case RtfControlWord.Ctrl_LTRROW:
			formatState.RowFormat.Dir = DirState.DirLTR;
			break;
		case RtfControlWord.Ctrl_CLMGF:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.IsHMergeFirst = true;
			break;
		case RtfControlWord.Ctrl_CLMRG:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.IsHMerge = true;
			break;
		case RtfControlWord.Ctrl_CLVMGF:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.IsVMergeFirst = true;
			break;
		case RtfControlWord.Ctrl_CLVMRG:
			cellFormat = formatState.RowFormat.CurrentCellFormat();
			cellFormat.IsVMerge = true;
			break;
		case RtfControlWord.Ctrl_BRDRL:
			ConverterState.CurrentBorder = formatState.ParaBorder.BorderLeft;
			break;
		case RtfControlWord.Ctrl_BRDRR:
			ConverterState.CurrentBorder = formatState.ParaBorder.BorderRight;
			break;
		case RtfControlWord.Ctrl_BRDRT:
			ConverterState.CurrentBorder = formatState.ParaBorder.BorderTop;
			break;
		case RtfControlWord.Ctrl_BRDRB:
			ConverterState.CurrentBorder = formatState.ParaBorder.BorderBottom;
			break;
		case RtfControlWord.Ctrl_BOX:
			ConverterState.CurrentBorder = formatState.ParaBorder.BorderAll;
			break;
		case RtfControlWord.Ctrl_BRDRNIL:
			ConverterState.CurrentBorder = null;
			break;
		case RtfControlWord.Ctrl_BRSP:
			formatState.ParaBorder.Spacing = token.Parameter;
			break;
		case RtfControlWord.Ctrl_BRDRTBL:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderNone;
			}
			break;
		case RtfControlWord.Ctrl_BRDRART:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRCF:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.CF = token.Parameter;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDASH:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDASHD:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDASHDD:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDASHDOTSTR:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDASHSM:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDB:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderDouble;
			}
			break;
		case RtfControlWord.Ctrl_BRDRDOT:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDREMBOSS:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRENGRAVE:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRFRAME:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRHAIR:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRINSET:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDROUTSET:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRS:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRSH:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTH:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderDouble;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTHTNLG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTHTNMG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTHTNSG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTNTHLG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTNTHMG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTNTHSG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTNTHTNLG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTNTHTNMG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTNTHTNSG:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRTRIPLE:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRW:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Width = token.Parameter;
			}
			break;
		case RtfControlWord.Ctrl_BRDRNONE:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.SetDefaults();
			}
			break;
		case RtfControlWord.Ctrl_BRDRWAVY:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderSingle;
			}
			break;
		case RtfControlWord.Ctrl_BRDRWAVYDB:
			if (ConverterState.CurrentBorder != null)
			{
				ConverterState.CurrentBorder.Type = BorderType.BorderDouble;
			}
			break;
		}
	}

	internal void HandleFieldTokens(RtfToken token, FormatState formatState)
	{
		FormatState formatState2 = _converterState.PreviousTopFormatState(0);
		FormatState formatState3 = _converterState.PreviousTopFormatState(1);
		if (formatState2 == null || formatState3 == null)
		{
			return;
		}
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_FIELD:
			if (!formatState3.IsContentDestination || formatState.IsHidden)
			{
				return;
			}
			formatState.RtfDestination = RtfDestination.DestField;
			break;
		case RtfControlWord.Ctrl_FLDRSLT:
			if (formatState3.RtfDestination != RtfDestination.DestField)
			{
				return;
			}
			formatState.RtfDestination = RtfDestination.DestFieldResult;
			break;
		case RtfControlWord.Ctrl_FLDPRIV:
			if (formatState3.RtfDestination != RtfDestination.DestField)
			{
				return;
			}
			formatState.RtfDestination = RtfDestination.DestFieldPrivate;
			break;
		case RtfControlWord.Ctrl_FLDINST:
			if (formatState3.RtfDestination != RtfDestination.DestField)
			{
				return;
			}
			formatState.RtfDestination = RtfDestination.DestFieldInstruction;
			break;
		default:
			return;
		}
		_converterState.DocumentNodeArray.Push(new DocumentNode(DocumentNodeType.dnFieldBegin)
		{
			FormatState = new FormatState(formatState),
			IsPending = false,
			IsTerminated = true
		});
	}

	internal void HandleTableNesting(FormatState formatState)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		if (!formatState.IsContentDestination || formatState.IsHidden)
		{
			return;
		}
		int i = documentNodeArray.CountOpenNodes(DocumentNodeType.dnTable);
		int num = (int)formatState.TableLevel;
		if (i == num && i == 0)
		{
			return;
		}
		if (i > num)
		{
			DocumentNode documentNode = documentNodeArray.Pop();
			bool flag = documentNodeArray.FindUnmatched(DocumentNodeType.dnFieldBegin) >= 0;
			while (i > num)
			{
				int num2 = documentNodeArray.FindPending(DocumentNodeType.dnTable);
				if (num2 >= 0)
				{
					documentNodeArray.CloseAt(num2);
					if (!flag)
					{
						documentNodeArray.CoalesceChildren(_converterState, num2);
					}
				}
				i--;
			}
			documentNodeArray.Push(documentNode);
		}
		else
		{
			if (i < num)
			{
				int num3 = documentNodeArray.FindPending(DocumentNodeType.dnList);
				if (num3 >= 0)
				{
					DocumentNode documentNode2 = documentNodeArray.Pop();
					while (num3 >= 0)
					{
						documentNodeArray.CloseAt(num3);
						num3 = documentNodeArray.FindPending(DocumentNodeType.dnList);
					}
					documentNodeArray.Push(documentNode2);
				}
			}
			int num4 = documentNodeArray.Count - 1;
			int num5 = documentNodeArray.FindPending(DocumentNodeType.dnTable);
			if (num5 >= 0)
			{
				int num6 = documentNodeArray.FindPending(DocumentNodeType.dnRow, num5);
				if (num6 == -1)
				{
					DocumentNode dn = new DocumentNode(DocumentNodeType.dnRow);
					documentNodeArray.InsertNode(num4++, dn);
					num6 = num4 - 1;
				}
				if (documentNodeArray.FindPending(DocumentNodeType.dnCell, num6) == -1)
				{
					DocumentNode dn2 = new DocumentNode(DocumentNodeType.dnCell);
					documentNodeArray.InsertNode(num4, dn2);
				}
			}
			num4 = documentNodeArray.Count - 1;
			for (; i < num; i++)
			{
				DocumentNode dn3 = new DocumentNode(DocumentNodeType.dnTable);
				DocumentNode dn4 = new DocumentNode(DocumentNodeType.dnTableBody);
				DocumentNode dn5 = new DocumentNode(DocumentNodeType.dnRow);
				DocumentNode dn6 = new DocumentNode(DocumentNodeType.dnCell);
				documentNodeArray.InsertNode(num4, dn6);
				documentNodeArray.InsertNode(num4, dn5);
				documentNodeArray.InsertNode(num4, dn4);
				documentNodeArray.InsertNode(num4, dn3);
			}
		}
		documentNodeArray.AssertTreeSemanticInvariants();
	}

	internal MarkerList GetMarkerStylesOfParagraph(MarkerList mlHave, FormatState fs, bool bMarkerPresent)
	{
		MarkerList markerList = new MarkerList();
		long listLevel = fs.ListLevel;
		long nStartIndexOverride = -1L;
		if (listLevel < 1)
		{
			return markerList;
		}
		for (int i = 0; i < mlHave.Count && (mlHave.EntryAt(i).VirtualListLevel < listLevel || fs.IsContinue); i++)
		{
			MarkerListEntry markerListEntry = mlHave.EntryAt(i);
			markerList.AddEntry(markerListEntry.Marker, markerListEntry.ILS, -1L, markerListEntry.StartIndexDefault, markerListEntry.VirtualListLevel);
		}
		if (fs.IsContinue)
		{
			return markerList;
		}
		ListOverride listOverride = _converterState.ListOverrideTable.FindEntry((int)fs.ILS);
		if (listOverride != null)
		{
			ListLevelTable levels = listOverride.Levels;
			if (levels == null || levels.Count == 0)
			{
				ListTableEntry listTableEntry = _converterState.ListTable.FindEntry(listOverride.ID);
				if (listTableEntry != null)
				{
					levels = listTableEntry.Levels;
				}
				if (listOverride.StartIndex > 0)
				{
					nStartIndexOverride = listOverride.StartIndex;
				}
			}
			if (levels != null)
			{
				ListLevel listLevel2 = levels.EntryAt((int)listLevel - 1);
				if (listLevel2 != null)
				{
					MarkerStyle markerStyle = listLevel2.Marker;
					if (markerStyle == MarkerStyle.MarkerHidden && bMarkerPresent)
					{
						markerStyle = MarkerStyle.MarkerBullet;
					}
					markerList.AddEntry(markerStyle, fs.ILS, nStartIndexOverride, listLevel2.StartIndex, listLevel);
					return markerList;
				}
			}
		}
		markerList.AddEntry(fs.Marker, fs.ILS, nStartIndexOverride, fs.StartIndex, listLevel);
		return markerList;
	}

	internal void HandleListNesting(FormatState formatState)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		DocumentNode documentNode = documentNodeArray.EntryAt(documentNodeArray.Count - 1);
		bool isMarkerPresent = _converterState.IsMarkerPresent;
		if (_converterState.IsMarkerPresent)
		{
			_converterState.IsMarkerPresent = false;
		}
		if (!formatState.IsContentDestination || formatState.IsHidden)
		{
			return;
		}
		if (!isMarkerPresent && formatState.ListLevel > 0)
		{
			formatState = new FormatState(formatState);
			if (formatState.ILVL > 0 && formatState.ILS < 0)
			{
				formatState.ILVL = 0L;
			}
			else
			{
				formatState.IsContinue = true;
			}
		}
		MarkerList markerList = documentNodeArray.GetOpenMarkerStyles();
		MarkerList markerList2 = GetMarkerStylesOfParagraph(markerList, formatState, isMarkerPresent);
		int count = markerList.Count;
		int count2 = markerList2.Count;
		if ((count == count2 && count == 0) || (count == 0 && count2 == 1 && formatState.IsContinue))
		{
			return;
		}
		if (_converterState.IsMarkerWhiteSpace)
		{
			_converterState.IsMarkerWhiteSpace = false;
			if (count2 > 0)
			{
				markerList2.EntryAt(count2 - 1).Marker = MarkerStyle.MarkerHidden;
			}
		}
		int num = GetMatchedMarkList(formatState, markerList, markerList2);
		if (num == 0)
		{
			MarkerList lastMarkerStyles = documentNodeArray.GetLastMarkerStyles(markerList, markerList2);
			MarkerList markerStylesOfParagraph = GetMarkerStylesOfParagraph(lastMarkerStyles, formatState, isMarkerPresent);
			num = GetMatchedMarkList(formatState, lastMarkerStyles, markerStylesOfParagraph);
			if (num < lastMarkerStyles.Count && markerStylesOfParagraph.Count > num)
			{
				num = 0;
			}
			if (num > 0)
			{
				markerList = lastMarkerStyles;
				markerList2 = markerStylesOfParagraph;
				documentNodeArray.OpenLastList();
			}
		}
		EnsureListAndListItem(formatState, documentNodeArray, markerList, markerList2, num);
		if (documentNodeArray.Count > 1 && documentNodeArray.EntryAt(documentNodeArray.Count - 2).Type == DocumentNodeType.dnListItem)
		{
			documentNode.FormatState.FI = 0L;
		}
		documentNodeArray.AssertTreeSemanticInvariants();
	}

	internal void HandleCodePageTokens(RtfToken token, FormatState formatState)
	{
		switch (token.RtfControlWordInfo.Control)
		{
		case RtfControlWord.Ctrl_ANSI:
			_converterState.CodePage = 1252;
			_lexer.CodePage = _converterState.CodePage;
			break;
		case RtfControlWord.Ctrl_MAC:
			_converterState.CodePage = 10000;
			_lexer.CodePage = _converterState.CodePage;
			break;
		case RtfControlWord.Ctrl_PC:
			_converterState.CodePage = 437;
			_lexer.CodePage = _converterState.CodePage;
			break;
		case RtfControlWord.Ctrl_PCA:
			_converterState.CodePage = 850;
			_lexer.CodePage = _converterState.CodePage;
			break;
		case RtfControlWord.Ctrl_UPR:
			formatState.RtfDestination = RtfDestination.DestUPR;
			break;
		case RtfControlWord.Ctrl_U:
			ProcessText(char.ToString((char)token.Parameter));
			break;
		case RtfControlWord.Ctrl_UD:
		{
			FormatState formatState2 = _converterState.PreviousTopFormatState(1);
			FormatState formatState3 = _converterState.PreviousTopFormatState(2);
			if (formatState2 != null && formatState3 != null && formatState.RtfDestination == RtfDestination.DestUPR && formatState2.RtfDestination == RtfDestination.DestUnknown)
			{
				formatState.RtfDestination = formatState3.RtfDestination;
			}
			break;
		}
		case RtfControlWord.Ctrl_UC:
			formatState.UnicodeSkip = (int)token.Parameter;
			break;
		}
	}

	internal void ProcessFieldText(RtfToken token)
	{
		switch (_converterState.TopFormatState.RtfDestination)
		{
		case RtfDestination.DestFieldInstruction:
			HandleNormalText(token.Text, _converterState.TopFormatState);
			break;
		case RtfDestination.DestFieldResult:
			HandleNormalText(token.Text, _converterState.TopFormatState);
			break;
		case RtfDestination.DestField:
		case RtfDestination.DestFieldPrivate:
			break;
		}
	}

	internal void ProcessFontTableText(RtfToken token)
	{
		string text = token.Text;
		text = text.Replace("\r\n", "");
		text = text.Replace(";", "");
		FontTableEntry currentEntry = _converterState.FontTable.CurrentEntry;
		if (currentEntry != null && text.Length > 0 && !currentEntry.IsNameSealed)
		{
			if (currentEntry.Name == null)
			{
				currentEntry.Name = text;
			}
			else
			{
				currentEntry.Name += text;
			}
		}
	}

	internal void HandleFontTableTokens(RtfToken token)
	{
		FontTableEntry currentEntry = _converterState.FontTable.CurrentEntry;
		FormatState topFormatState = _converterState.TopFormatState;
		if (currentEntry != null && token.RtfControlWordInfo.Control == RtfControlWord.Ctrl_FCHARSET)
		{
			currentEntry.CodePageFromCharSet = (int)token.Parameter;
			if (currentEntry.CodePage == -1)
			{
				topFormatState.CodePage = _converterState.CodePage;
			}
			else
			{
				topFormatState.CodePage = currentEntry.CodePage;
			}
			_lexer.CodePage = topFormatState.CodePage;
		}
	}

	internal void ProcessColorTableText(RtfToken token)
	{
		_converterState.ColorTable.FinishColor();
	}

	internal void ProcessText(string text)
	{
		FormatState topFormatState = _converterState.TopFormatState;
		if (topFormatState.IsContentDestination && !topFormatState.IsHidden && text != string.Empty)
		{
			HandleNormalTextRaw(text, topFormatState);
		}
	}

	internal void HandleNormalText(string text, FormatState formatState)
	{
		int num = 0;
		while (num < text.Length)
		{
			int i;
			for (i = num; i < text.Length && text[i] != '\r' && text[i] != '\n'; i++)
			{
			}
			if (num == 0 && i == text.Length)
			{
				HandleNormalTextRaw(text, formatState);
			}
			else if (i > num)
			{
				string text2 = text.Substring(num, i - num);
				HandleNormalTextRaw(text2, formatState);
			}
			while (i < text.Length && (text[i] == '\r' || text[i] == '\n'))
			{
				ProcessNormalHardLine(formatState);
				i = ((i + 1 >= text.Length || text[i] != '\r' || text[i] != '\n') ? (i + 1) : (i + 2));
			}
			num = i;
		}
	}

	internal void HandleNormalTextRaw(string text, FormatState formatState)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		DocumentNode documentNode = documentNodeArray.Top;
		if (documentNode != null && documentNode.Type == DocumentNodeType.dnText && !documentNode.FormatState.IsEqual(formatState))
		{
			documentNodeArray.CloseAt(documentNodeArray.Count - 1);
			documentNode = null;
		}
		if (documentNode == null || documentNode.Type != DocumentNodeType.dnText)
		{
			documentNode = new DocumentNode(DocumentNodeType.dnText);
			documentNode.FormatState = new FormatState(formatState);
			documentNodeArray.Push(documentNode);
		}
		documentNode.AppendXamlEncoded(text);
		documentNode.IsPending = false;
	}

	internal void ProcessNormalHardLine(FormatState formatState)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		if (documentNodeArray.TestTop(DocumentNodeType.dnText))
		{
			documentNodeArray.CloseAt(documentNodeArray.Count - 1);
		}
		DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnLineBreak);
		documentNode.FormatState = new FormatState(formatState);
		documentNodeArray.Push(documentNode);
		documentNodeArray.CloseAt(documentNodeArray.Count - 1);
		documentNodeArray.CoalesceChildren(_converterState, documentNodeArray.Count - 1);
	}

	internal void ProcessHardLine(RtfToken token, FormatState formatState)
	{
		switch (_converterState.TopFormatState.RtfDestination)
		{
		case RtfDestination.DestNormal:
		case RtfDestination.DestListText:
		case RtfDestination.DestFieldResult:
		case RtfDestination.DestShapeResult:
			ProcessNormalHardLine(formatState);
			break;
		case RtfDestination.DestFieldInstruction:
		case RtfDestination.DestFieldPrivate:
			ProcessNormalHardLine(formatState);
			break;
		case RtfDestination.DestColorTable:
		case RtfDestination.DestFontTable:
		case RtfDestination.DestFontName:
		case RtfDestination.DestListTable:
		case RtfDestination.DestListOverrideTable:
		case RtfDestination.DestList:
		case RtfDestination.DestListLevel:
		case RtfDestination.DestListOverride:
		case RtfDestination.DestListPicture:
		case RtfDestination.DestUPR:
		case RtfDestination.DestField:
		case RtfDestination.DestShape:
		case RtfDestination.DestShapeInstruction:
			break;
		}
	}

	private void SetTokenTextWithControlCharacter(RtfToken token)
	{
		switch (token.Text[0])
		{
		case '\0':
		case '\u0001':
		case '\u0002':
		case '\u0003':
		case '\u0004':
		case '\u0005':
		case '\u0006':
		case '\a':
		case '\b':
		case '\t':
		case '\n':
		case '\v':
		case '\f':
		case '\r':
		case '\u000e':
		case '\u000f':
		case '\u0010':
		case '\u0011':
		case '\u0012':
		case '\u0013':
		case '\u0014':
		case '\u0015':
		case '\u0016':
		case '\u0017':
		case '\u0018':
		case '\u0019':
		case '\u001a':
		case '\u001b':
		case '\u001c':
		case '\u001d':
		case '\u001e':
		case '\u001f':
		case ' ':
		case '!':
		case '"':
		case '#':
		case '$':
		case '%':
		case '&':
		case '\'':
		case '(':
		case ')':
		case '*':
		case '+':
		case ',':
		case '.':
		case '/':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
		case ':':
			_ = 58;
			break;
		case '~':
			token.Text = new string('\u00a0', 1);
			break;
		case '-':
			token.Text = string.Empty;
			break;
		case '_':
			token.Text = new string('‑', 1);
			break;
		}
	}

	private int GetMatchedMarkList(FormatState formatState, MarkerList mlHave, MarkerList mlWant)
	{
		int i;
		for (i = 0; i < mlHave.Count && i < mlWant.Count; i++)
		{
			if (!formatState.IsContinue)
			{
				MarkerListEntry markerListEntry = mlHave.EntryAt(i);
				MarkerListEntry markerListEntry2 = mlWant.EntryAt(i);
				if (markerListEntry.Marker != markerListEntry2.Marker || markerListEntry.ILS != markerListEntry2.ILS || markerListEntry.StartIndexDefault != markerListEntry2.StartIndexDefault || markerListEntry2.StartIndexOverride >= 0)
				{
					break;
				}
			}
		}
		return i;
	}

	private void EnsureListAndListItem(FormatState formatState, DocumentNodeArray dna, MarkerList mlHave, MarkerList mlWant, int nMatch)
	{
		bool flag = false;
		int i = mlHave.Count;
		int num = mlWant.Count;
		dna.FindUnmatched(DocumentNodeType.dnFieldBegin);
		if (i > nMatch)
		{
			DocumentNode documentNode = dna.Pop();
			while (i > nMatch)
			{
				int num2 = dna.FindPending(DocumentNodeType.dnList);
				if (num2 >= 0)
				{
					dna.CloseAt(num2);
				}
				i--;
				mlHave.RemoveRange(mlHave.Count - 1, 1);
			}
			dna.Push(documentNode);
		}
		int nAt;
		if (i < num)
		{
			if (num != i + 1)
			{
				if (num <= mlWant.Count)
				{
					mlWant[i] = mlWant[mlWant.Count - 1];
				}
				num = i + 1;
			}
			nAt = dna.Count - 1;
			for (; i < num; i++)
			{
				flag = true;
				DocumentNode documentNode2 = new DocumentNode(DocumentNodeType.dnList);
				DocumentNode dn = new DocumentNode(DocumentNodeType.dnListItem);
				dna.InsertNode(nAt, dn);
				dna.InsertNode(nAt, documentNode2);
				MarkerListEntry markerListEntry = mlWant.EntryAt(i);
				documentNode2.FormatState.Marker = markerListEntry.Marker;
				documentNode2.FormatState.StartIndex = markerListEntry.StartIndexToUse;
				documentNode2.FormatState.StartIndexDefault = markerListEntry.StartIndexDefault;
				documentNode2.VirtualListLevel = markerListEntry.VirtualListLevel;
				documentNode2.FormatState.ILS = markerListEntry.ILS;
			}
		}
		nAt = dna.Count - 1;
		int num3 = dna.FindPending(DocumentNodeType.dnList);
		if (num3 >= 0)
		{
			int num4 = dna.FindPending(DocumentNodeType.dnListItem, num3);
			if (num4 >= 0 && !flag && !formatState.IsContinue)
			{
				DocumentNode documentNode3 = dna.Pop();
				dna.CloseAt(num4);
				dna.Push(documentNode3);
				num4 = -1;
				nAt = dna.Count - 1;
			}
			if (num4 == -1)
			{
				DocumentNode dn2 = new DocumentNode(DocumentNodeType.dnListItem);
				dna.InsertNode(nAt, dn2);
			}
		}
	}
}
