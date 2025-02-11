using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MS.Internal.Globalization;
using MS.Internal.Text;

namespace System.Windows.Documents;

internal class XamlToRtfWriter
{
	internal enum XamlTag
	{
		XTUnknown,
		XTBold,
		XTItalic,
		XTUnderline,
		XTHyperlink,
		XTInline,
		XTLineBreak,
		XTParagraph,
		XTInlineUIContainer,
		XTBlockUIContainer,
		XTImage,
		XTBitmapImage,
		XTList,
		XTListItem,
		XTTable,
		XTTableBody,
		XTTableRow,
		XTTableCell,
		XTTableColumn,
		XTSection,
		XTFloater,
		XTFigure,
		XTTextDecoration
	}

	internal class XamlIn : IXamlContentHandler, IXamlErrorHandler
	{
		private string _xaml;

		private XamlToRtfWriter _writer;

		private XamlToRtfParser _parser;

		private bool _bGenListTables;

		internal bool GenerateListTables => _bGenListTables;

		internal XamlIn(XamlToRtfWriter writer, string xaml)
		{
			_writer = writer;
			_xaml = xaml;
			_parser = new XamlToRtfParser(_xaml);
			_parser.SetCallbacks(this, this);
			_bGenListTables = true;
		}

		internal XamlToRtfError Parse()
		{
			return _parser.Parse();
		}

		XamlToRtfError IXamlContentHandler.Characters(string characters)
		{
			XamlToRtfError xamlToRtfError = XamlToRtfError.None;
			ConverterState converterState = _writer.ConverterState;
			DocumentNodeArray documentNodeArray = converterState.DocumentNodeArray;
			DocumentNode documentNode = documentNodeArray.TopPending();
			int i = 0;
			while (xamlToRtfError == XamlToRtfError.None && i < characters.Length)
			{
				for (; i < characters.Length && IsNewLine(characters[i]); i++)
				{
				}
				int j;
				for (j = i; j < characters.Length && !IsNewLine(characters[j]); j++)
				{
				}
				if (i != j)
				{
					string s = characters.Substring(i, j - i);
					DocumentNode documentNode2 = new DocumentNode(DocumentNodeType.dnText);
					if (documentNode != null)
					{
						documentNode2.InheritFormatState(documentNode.FormatState);
					}
					documentNodeArray.Push(documentNode2);
					documentNode2.IsPending = false;
					if (xamlToRtfError == XamlToRtfError.None)
					{
						int cp = converterState.FontTable.FindEntryByIndex((int)documentNode2.FormatState.Font)?.CodePage ?? 1252;
						XamlParserHelper.AppendRTFText(documentNode2.Content, s, cp);
					}
				}
				i = j;
			}
			return xamlToRtfError;
		}

		XamlToRtfError IXamlContentHandler.StartDocument()
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlContentHandler.EndDocument()
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlContentHandler.StartElement(string nameSpaceUri, string localName, string qName, IXamlAttributes attributes)
		{
			XamlToRtfError xamlToRtfError = XamlToRtfError.None;
			ConverterState converterState = _writer.ConverterState;
			DocumentNodeArray documentNodeArray = converterState.DocumentNodeArray;
			DocumentNodeType documentNodeType = DocumentNodeType.dnUnknown;
			DocumentNode documentNode = documentNodeArray.TopPending();
			DocumentNode documentNode2 = null;
			XamlTag xamlTag = XamlTag.XTUnknown;
			bool flag = true;
			if (!XamlParserHelper.ConvertToTag(converterState, localName, ref xamlTag))
			{
				return xamlToRtfError;
			}
			if (xamlTag == XamlTag.XTTextDecoration || xamlTag == XamlTag.XTTableColumn || xamlTag == XamlTag.XTBitmapImage)
			{
				if (documentNode == null)
				{
					return xamlToRtfError;
				}
				documentNode2 = documentNode;
				flag = false;
			}
			if (flag)
			{
				if (!XamlParserHelper.ConvertTagToNodeType(xamlTag, ref documentNodeType))
				{
					return xamlToRtfError;
				}
				documentNode2 = CreateDocumentNode(converterState, documentNodeType, documentNode, xamlTag);
			}
			if (attributes != null && documentNode2 != null)
			{
				xamlToRtfError = HandleAttributes(converterState, attributes, documentNode2, xamlTag, documentNodeArray);
			}
			if (xamlToRtfError == XamlToRtfError.None && documentNode2 != null && flag)
			{
				if (!documentNode2.IsInline)
				{
					XamlParserHelper.EnsureParagraphClosed(converterState);
				}
				documentNodeArray.Push(documentNode2);
			}
			return xamlToRtfError;
		}

		XamlToRtfError IXamlContentHandler.EndElement(string nameSpaceUri, string localName, string qName)
		{
			XamlToRtfError result = XamlToRtfError.None;
			ConverterState converterState = _writer.ConverterState;
			XamlTag xamlTag = XamlTag.XTUnknown;
			if (!XamlParserHelper.ConvertToTag(converterState, localName, ref xamlTag))
			{
				return result;
			}
			DocumentNodeType documentNodeType = DocumentNodeType.dnUnknown;
			if (!XamlParserHelper.ConvertTagToNodeType(xamlTag, ref documentNodeType))
			{
				return result;
			}
			DocumentNodeArray documentNodeArray = converterState.DocumentNodeArray;
			int num = documentNodeArray.FindPending(documentNodeType);
			if (num >= 0)
			{
				DocumentNode documentNode = documentNodeArray.EntryAt(num);
				if (documentNodeType != DocumentNodeType.dnParagraph && !documentNode.IsInline)
				{
					XamlParserHelper.EnsureParagraphClosed(converterState);
				}
				documentNodeArray.CloseAt(num);
			}
			return result;
		}

		XamlToRtfError IXamlContentHandler.IgnorableWhitespace(string xaml)
		{
			XamlToRtfError result = XamlToRtfError.None;
			ConverterState converterState = _writer.ConverterState;
			if (converterState.DocumentNodeArray.FindPending(DocumentNodeType.dnParagraph) >= 0 || converterState.DocumentNodeArray.FindPending(DocumentNodeType.dnInline) >= 0)
			{
				int num = 0;
				while (num < xaml.Length)
				{
					int num2 = num;
					int i = num;
					int num3 = -1;
					for (; i < xaml.Length; i++)
					{
						if (xaml[i] == '\r' || xaml[i] == '\n')
						{
							num3 = ((xaml[i] != '\r' || i + 1 >= xaml.Length || xaml[i + 1] != '\n') ? (i + 1) : (i + 2));
						}
					}
					if (num2 == 0 && i == xaml.Length)
					{
						return ((IXamlContentHandler)this).Characters(xaml);
					}
					if (i != num2)
					{
						string characters = xaml.Substring(num2, i - num2);
						result = ((IXamlContentHandler)this).Characters(characters);
						if (result != 0)
						{
							return result;
						}
					}
					result = ((IXamlContentHandler)this).StartElement((string)null, "LineBreak", (string)null, (IXamlAttributes)null);
					if (result != 0)
					{
						return result;
					}
					result = ((IXamlContentHandler)this).EndElement((string)null, "LineBreak", (string)null);
					if (result != 0)
					{
						return result;
					}
					num = ((i == xaml.Length) ? i : num3);
				}
				return ((IXamlContentHandler)this).Characters(xaml);
			}
			return result;
		}

		XamlToRtfError IXamlContentHandler.StartPrefixMapping(string prefix, string uri)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlContentHandler.ProcessingInstruction(string target, string data)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlContentHandler.SkippedEntity(string name)
		{
			XamlToRtfError result = XamlToRtfError.None;
			if (string.Compare(name, "&gt;", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ((IXamlContentHandler)this).Characters(">");
			}
			if (string.Compare(name, "&lt;", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ((IXamlContentHandler)this).Characters("<");
			}
			if (string.Compare(name, "&amp;", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ((IXamlContentHandler)this).Characters("&");
			}
			if (name.IndexOf("&#x", StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = XamlToRtfError.InvalidFormat;
				if (name.Length >= 5)
				{
					int i = 0;
					Converters.HexStringToInt(name.AsSpan(3, name.Length - 4), ref i);
					if (i >= 0 && i <= 65535)
					{
						string characters = char.ToString((char)i);
						return ((IXamlContentHandler)this).Characters(characters);
					}
				}
			}
			else if (name.IndexOf("&#", StringComparison.OrdinalIgnoreCase) == 0 && name.Length >= 4)
			{
				ReadOnlySpan<char> s = name.AsSpan(2, name.Length - 3);
				int i2 = 0;
				Converters.StringToInt(s, ref i2);
				if (i2 >= 0 && i2 <= 65535)
				{
					string characters2 = char.ToString((char)i2);
					return ((IXamlContentHandler)this).Characters(characters2);
				}
			}
			return result;
		}

		void IXamlErrorHandler.Error(string message, XamlToRtfError xamlToRtfError)
		{
		}

		void IXamlErrorHandler.FatalError(string message, XamlToRtfError xamlToRtfError)
		{
		}

		void IXamlErrorHandler.IgnorableWarning(string message, XamlToRtfError xamlToRtfError)
		{
		}

		private bool IsNewLine(char character)
		{
			if (character != '\r')
			{
				return character == '\n';
			}
			return true;
		}

		private DocumentNode CreateDocumentNode(ConverterState converterState, DocumentNodeType documentNodeType, DocumentNode dnTop, XamlTag xamlTag)
		{
			DocumentNode documentNode = new DocumentNode(documentNodeType);
			if (dnTop != null)
			{
				documentNode.InheritFormatState(dnTop.FormatState);
			}
			switch (xamlTag)
			{
			case XamlTag.XTBold:
				documentNode.FormatState.Bold = true;
				break;
			case XamlTag.XTHyperlink:
			{
				long colorIndex = 0L;
				documentNode.FormatState.UL = ULState.ULNormal;
				if (XamlParserHelper.ConvertToColor(converterState, "#FF0000FF", ref colorIndex))
				{
					documentNode.FormatState.CF = colorIndex;
				}
				break;
			}
			case XamlTag.XTItalic:
				documentNode.FormatState.Italic = true;
				break;
			case XamlTag.XTUnderline:
				documentNode.FormatState.UL = ULState.ULNormal;
				break;
			case XamlTag.XTList:
				documentNode.FormatState.Marker = MarkerStyle.MarkerBullet;
				documentNode.FormatState.StartIndex = 1L;
				documentNode.FormatState.LI = 720L;
				break;
			}
			return documentNode;
		}

		private XamlToRtfError HandleAttributes(ConverterState converterState, IXamlAttributes attributes, DocumentNode documentNode, XamlTag xamlTag, DocumentNodeArray dna)
		{
			int length = 0;
			XamlToRtfError xamlToRtfError = attributes.GetLength(ref length);
			if (xamlToRtfError == XamlToRtfError.None)
			{
				string uri = string.Empty;
				string localName = string.Empty;
				string qName = string.Empty;
				string value = string.Empty;
				FormatState formatState = documentNode.FormatState;
				XamlAttribute xamlAttribute = XamlAttribute.XAUnknown;
				long fontIndex = 0L;
				int num = 0;
				while (xamlToRtfError == XamlToRtfError.None && num < length)
				{
					xamlToRtfError = attributes.GetName(num, ref uri, ref localName, ref qName);
					if (xamlToRtfError == XamlToRtfError.None)
					{
						xamlToRtfError = attributes.GetValue(num, ref value);
						if (xamlToRtfError == XamlToRtfError.None && XamlParserHelper.ConvertToAttribute(converterState, localName, ref xamlAttribute))
						{
							switch (xamlAttribute)
							{
							case XamlAttribute.XAFontWeight:
								if (string.Compare(value, "Normal", StringComparison.OrdinalIgnoreCase) == 0)
								{
									formatState.Bold = false;
								}
								else if (string.Compare(value, "Bold", StringComparison.OrdinalIgnoreCase) == 0)
								{
									formatState.Bold = true;
								}
								break;
							case XamlAttribute.XAFontSize:
							{
								double d = 0.0;
								if (XamlParserHelper.ConvertToFontSize(converterState, value, ref d))
								{
									formatState.FontSize = (long)Math.Round(d);
								}
								break;
							}
							case XamlAttribute.XAFontStyle:
								if (string.Compare(value, "Italic", StringComparison.OrdinalIgnoreCase) == 0)
								{
									formatState.Italic = true;
								}
								break;
							case XamlAttribute.XAFontFamily:
								if (XamlParserHelper.ConvertToFont(converterState, value, ref fontIndex))
								{
									formatState.Font = fontIndex;
								}
								break;
							case XamlAttribute.XAFontStretch:
								if (XamlParserHelper.ConvertToFontStretch(converterState, value, ref fontIndex))
								{
									formatState.Expand = fontIndex;
								}
								break;
							case XamlAttribute.XABackground:
								if (XamlParserHelper.ConvertToColor(converterState, value, ref fontIndex))
								{
									if (documentNode.IsInline)
									{
										formatState.CB = fontIndex;
									}
									else
									{
										formatState.CBPara = fontIndex;
									}
								}
								break;
							case XamlAttribute.XAForeground:
								if (XamlParserHelper.ConvertToColor(converterState, value, ref fontIndex))
								{
									formatState.CF = fontIndex;
								}
								break;
							case XamlAttribute.XAFlowDirection:
							{
								DirState dirState = DirState.DirDefault;
								if (XamlParserHelper.ConvertToDir(converterState, value, ref dirState))
								{
									if (documentNode.IsInline)
									{
										formatState.DirChar = dirState;
									}
									else if (documentNode.Type == DocumentNodeType.dnTable)
									{
										formatState.RowFormat.Dir = dirState;
									}
									else
									{
										formatState.DirPara = dirState;
										formatState.DirChar = dirState;
									}
									if (documentNode.Type == DocumentNodeType.dnList && formatState.DirPara == DirState.DirRTL)
									{
										formatState.LI = 0L;
										formatState.RI = 720L;
									}
								}
								break;
							}
							case XamlAttribute.XATextDecorations:
							{
								ULState ulState2 = ULState.ULNormal;
								StrikeState strikeState2 = StrikeState.StrikeNormal;
								if (XamlParserHelper.ConvertToDecoration(converterState, value, ref ulState2, ref strikeState2))
								{
									if (ulState2 != 0)
									{
										formatState.UL = ulState2;
									}
									if (strikeState2 != 0)
									{
										formatState.Strike = strikeState2;
									}
								}
								break;
							}
							case XamlAttribute.XALocation:
							{
								ULState ulState = ULState.ULNormal;
								StrikeState strikeState = StrikeState.StrikeNormal;
								if (XamlParserHelper.ConvertToDecoration(converterState, value, ref ulState, ref strikeState))
								{
									if (ulState != 0)
									{
										formatState.UL = ulState;
									}
									if (strikeState != 0)
									{
										formatState.Strike = strikeState;
									}
								}
								break;
							}
							case XamlAttribute.XARowSpan:
							{
								int i3 = 0;
								if (Converters.StringToInt(value, ref i3) && documentNode.Type == DocumentNodeType.dnCell)
								{
									documentNode.RowSpan = i3;
								}
								break;
							}
							case XamlAttribute.XAColumnSpan:
							{
								int i2 = 0;
								if (Converters.StringToInt(value, ref i2) && documentNode.Type == DocumentNodeType.dnCell)
								{
									documentNode.ColSpan = i2;
								}
								break;
							}
							case XamlAttribute.XACellSpacing:
							{
								double d8 = 0.0;
								if (Converters.StringToDouble(value, ref d8) && documentNode.Type == DocumentNodeType.dnTable)
								{
									formatState.RowFormat.Trgaph = Converters.PxToTwipRounded(d8);
								}
								break;
							}
							case XamlAttribute.XANavigateUri:
								if (xamlTag == XamlTag.XTHyperlink && value.Length > 0)
								{
									StringBuilder stringBuilder = new StringBuilder();
									XamlParserHelper.AppendRTFText(stringBuilder, value, 0);
									documentNode.NavigateUri = stringBuilder.ToString();
								}
								break;
							case XamlAttribute.XAWidth:
								switch (xamlTag)
								{
								case XamlTag.XTTableColumn:
								{
									double d7 = 0.0;
									if (Converters.StringToDouble(value, ref d7))
									{
										int num2 = dna.FindPending(DocumentNodeType.dnTable);
										if (num2 >= 0)
										{
											CellFormat cellFormat = dna.EntryAt(num2).FormatState.RowFormat.NextCellFormat();
											cellFormat.Width.Type = WidthType.WidthTwips;
											cellFormat.Width.Value = Converters.PxToTwipRounded(d7);
										}
									}
									break;
								}
								case XamlTag.XTImage:
								{
									double d6 = 0.0;
									Converters.StringToDouble(value, ref d6);
									documentNode.FormatState.ImageWidth = d6;
									break;
								}
								}
								break;
							case XamlAttribute.XAHeight:
								if (xamlTag == XamlTag.XTImage)
								{
									double d5 = 0.0;
									Converters.StringToDouble(value, ref d5);
									documentNode.FormatState.ImageHeight = d5;
								}
								break;
							case XamlAttribute.XABaselineOffset:
								if (xamlTag == XamlTag.XTImage)
								{
									double d4 = 0.0;
									Converters.StringToDouble(value, ref d4);
									documentNode.FormatState.ImageBaselineOffset = d4;
									documentNode.FormatState.IncludeImageBaselineOffset = true;
								}
								break;
							case XamlAttribute.XASource:
								if (xamlTag == XamlTag.XTImage)
								{
									documentNode.FormatState.ImageSource = value;
								}
								break;
							case XamlAttribute.XAUriSource:
								if (xamlTag == XamlTag.XTBitmapImage)
								{
									documentNode.FormatState.ImageSource = value;
								}
								break;
							case XamlAttribute.XAStretch:
								if (xamlTag == XamlTag.XTImage)
								{
									documentNode.FormatState.ImageStretch = value;
								}
								break;
							case XamlAttribute.XAStretchDirection:
								if (xamlTag == XamlTag.XTImage)
								{
									documentNode.FormatState.ImageStretchDirection = value;
								}
								break;
							case XamlAttribute.XATypographyVariants:
							{
								RtfSuperSubscript ss = RtfSuperSubscript.None;
								if (XamlParserHelper.ConvertToSuperSub(converterState, value, ref ss))
								{
									switch (ss)
									{
									case RtfSuperSubscript.Super:
										formatState.Super = true;
										break;
									case RtfSuperSubscript.Sub:
										formatState.Sub = true;
										break;
									case RtfSuperSubscript.Normal:
										formatState.Sub = false;
										formatState.Super = false;
										break;
									}
								}
								break;
							}
							case XamlAttribute.XAMarkerStyle:
							{
								MarkerStyle ms = MarkerStyle.MarkerBullet;
								if (XamlParserHelper.ConvertToMarkerStyle(converterState, value, ref ms))
								{
									formatState.Marker = ms;
								}
								break;
							}
							case XamlAttribute.XAStartIndex:
							{
								int i = 0;
								if (XamlParserHelper.ConvertToStartIndex(converterState, value, ref i))
								{
									formatState.StartIndex = i;
								}
								break;
							}
							case XamlAttribute.XAMargin:
							{
								XamlThickness xthickness3 = new XamlThickness(0f, 0f, 0f, 0f);
								if (XamlParserHelper.ConvertToThickness(converterState, value, ref xthickness3))
								{
									formatState.LI = Converters.PxToTwipRounded(xthickness3.Left);
									formatState.RI = Converters.PxToTwipRounded(xthickness3.Right);
									formatState.SB = Converters.PxToTwipRounded(xthickness3.Top);
									formatState.SA = Converters.PxToTwipRounded(xthickness3.Bottom);
								}
								break;
							}
							case XamlAttribute.XAPadding:
							{
								XamlThickness xthickness2 = new XamlThickness(0f, 0f, 0f, 0f);
								if (XamlParserHelper.ConvertToThickness(converterState, value, ref xthickness2))
								{
									if (xamlTag == XamlTag.XTParagraph)
									{
										formatState.ParaBorder.Spacing = Converters.PxToTwipRounded(xthickness2.Left);
										break;
									}
									CellFormat rowCellFormat3 = formatState.RowFormat.RowCellFormat;
									rowCellFormat3.PaddingLeft = Converters.PxToTwipRounded(xthickness2.Left);
									rowCellFormat3.PaddingRight = Converters.PxToTwipRounded(xthickness2.Right);
									rowCellFormat3.PaddingTop = Converters.PxToTwipRounded(xthickness2.Top);
									rowCellFormat3.PaddingBottom = Converters.PxToTwipRounded(xthickness2.Bottom);
								}
								break;
							}
							case XamlAttribute.XABorderThickness:
							{
								XamlThickness xthickness = new XamlThickness(0f, 0f, 0f, 0f);
								if (XamlParserHelper.ConvertToThickness(converterState, value, ref xthickness))
								{
									if (xamlTag == XamlTag.XTParagraph)
									{
										ParaBorder paraBorder = formatState.ParaBorder;
										paraBorder.BorderLeft.Type = BorderType.BorderSingle;
										paraBorder.BorderLeft.Width = Converters.PxToTwipRounded(xthickness.Left);
										paraBorder.BorderRight.Type = BorderType.BorderSingle;
										paraBorder.BorderRight.Width = Converters.PxToTwipRounded(xthickness.Right);
										paraBorder.BorderTop.Type = BorderType.BorderSingle;
										paraBorder.BorderTop.Width = Converters.PxToTwipRounded(xthickness.Top);
										paraBorder.BorderBottom.Type = BorderType.BorderSingle;
										paraBorder.BorderBottom.Width = Converters.PxToTwipRounded(xthickness.Bottom);
									}
									else
									{
										CellFormat rowCellFormat2 = formatState.RowFormat.RowCellFormat;
										rowCellFormat2.BorderLeft.Type = BorderType.BorderSingle;
										rowCellFormat2.BorderLeft.Width = Converters.PxToTwipRounded(xthickness.Left);
										rowCellFormat2.BorderRight.Type = BorderType.BorderSingle;
										rowCellFormat2.BorderRight.Width = Converters.PxToTwipRounded(xthickness.Right);
										rowCellFormat2.BorderTop.Type = BorderType.BorderSingle;
										rowCellFormat2.BorderTop.Width = Converters.PxToTwipRounded(xthickness.Top);
										rowCellFormat2.BorderBottom.Type = BorderType.BorderSingle;
										rowCellFormat2.BorderBottom.Width = Converters.PxToTwipRounded(xthickness.Bottom);
									}
								}
								break;
							}
							case XamlAttribute.XABorderBrush:
								if (XamlParserHelper.ConvertToColor(converterState, value, ref fontIndex))
								{
									if (xamlTag == XamlTag.XTParagraph)
									{
										formatState.ParaBorder.CF = fontIndex;
										break;
									}
									CellFormat rowCellFormat = formatState.RowFormat.RowCellFormat;
									rowCellFormat.BorderLeft.CF = fontIndex;
									rowCellFormat.BorderRight.CF = fontIndex;
									rowCellFormat.BorderTop.CF = fontIndex;
									rowCellFormat.BorderBottom.CF = fontIndex;
								}
								break;
							case XamlAttribute.XATextIndent:
							{
								double d3 = 0.0;
								if (XamlParserHelper.ConvertToTextIndent(converterState, value, ref d3))
								{
									formatState.FI = Converters.PxToTwipRounded(d3);
								}
								break;
							}
							case XamlAttribute.XALineHeight:
							{
								double d2 = 0.0;
								if (XamlParserHelper.ConvertToLineHeight(converterState, value, ref d2))
								{
									formatState.SL = Converters.PxToTwipRounded(d2);
									formatState.SLMult = false;
								}
								break;
							}
							case XamlAttribute.XALang:
								try
								{
									CultureInfo cultureInfo = new CultureInfo(value);
									if (cultureInfo.LCID > 0)
									{
										formatState.Lang = (ushort)cultureInfo.LCID;
									}
								}
								catch (ArgumentException)
								{
								}
								break;
							case XamlAttribute.XATextAlignment:
							{
								HAlign align = HAlign.AlignDefault;
								if (XamlParserHelper.ConvertToHAlign(converterState, value, ref align))
								{
									formatState.HAlign = align;
								}
								break;
							}
							}
						}
					}
					num++;
				}
			}
			return xamlToRtfError;
		}
	}

	internal static class XamlParserHelper
	{
		internal struct LookupTableEntry
		{
			private string _name;

			private int _value;

			internal string Name => _name;

			internal int Value => _value;

			internal LookupTableEntry(string name, int value)
			{
				_name = name;
				_value = value;
			}
		}

		internal static LookupTableEntry[] TagTable = new LookupTableEntry[25]
		{
			new LookupTableEntry("", 0),
			new LookupTableEntry("", 0),
			new LookupTableEntry("Bold", 1),
			new LookupTableEntry("Italic", 2),
			new LookupTableEntry("Underline", 3),
			new LookupTableEntry("Hyperlink", 4),
			new LookupTableEntry("Span", 5),
			new LookupTableEntry("Run", 5),
			new LookupTableEntry("LineBreak", 6),
			new LookupTableEntry("Paragraph", 7),
			new LookupTableEntry("InlineUIContainer", 5),
			new LookupTableEntry("BlockUIContainer", 9),
			new LookupTableEntry("Image", 10),
			new LookupTableEntry("BitmapImage", 11),
			new LookupTableEntry("List", 12),
			new LookupTableEntry("ListItem", 13),
			new LookupTableEntry("Table", 14),
			new LookupTableEntry("TableRowGroup", 15),
			new LookupTableEntry("TableRow", 16),
			new LookupTableEntry("TableCell", 17),
			new LookupTableEntry("TableColumn", 18),
			new LookupTableEntry("Section", 19),
			new LookupTableEntry("Figure", 21),
			new LookupTableEntry("Floater", 20),
			new LookupTableEntry("TextDecoration", 22)
		};

		internal static LookupTableEntry[] AttributeTable = new LookupTableEntry[36]
		{
			new LookupTableEntry("", 0),
			new LookupTableEntry("FontWeight", 1),
			new LookupTableEntry("FontSize", 2),
			new LookupTableEntry("FontStyle", 3),
			new LookupTableEntry("FontFamily", 4),
			new LookupTableEntry("Background", 6),
			new LookupTableEntry("Foreground", 7),
			new LookupTableEntry("FlowDirection", 8),
			new LookupTableEntry("TextDecorations", 9),
			new LookupTableEntry("TextAlignment", 10),
			new LookupTableEntry("MarkerStyle", 11),
			new LookupTableEntry("TextIndent", 12),
			new LookupTableEntry("ColumnSpan", 13),
			new LookupTableEntry("RowSpan", 14),
			new LookupTableEntry("StartIndex", 15),
			new LookupTableEntry("MarkerOffset", 16),
			new LookupTableEntry("BorderThickness", 17),
			new LookupTableEntry("BorderBrush", 18),
			new LookupTableEntry("Padding", 19),
			new LookupTableEntry("Margin", 20),
			new LookupTableEntry("KeepTogether", 21),
			new LookupTableEntry("KeepWithNext", 22),
			new LookupTableEntry("BaselineAlignment", 23),
			new LookupTableEntry("BaselineOffset", 24),
			new LookupTableEntry("NavigateUri", 25),
			new LookupTableEntry("TargetName", 26),
			new LookupTableEntry("LineHeight", 27),
			new LookupTableEntry("xml:lang", 37),
			new LookupTableEntry("Height", 30),
			new LookupTableEntry("Source", 31),
			new LookupTableEntry("UriSource", 32),
			new LookupTableEntry("Stretch", 33),
			new LookupTableEntry("StretchDirection", 34),
			new LookupTableEntry("Location", 28),
			new LookupTableEntry("Width", 29),
			new LookupTableEntry("Typography.Variants", 36)
		};

		internal static LookupTableEntry[] MarkerStyleTable = new LookupTableEntry[13]
		{
			new LookupTableEntry("", 23),
			new LookupTableEntry("None", -1),
			new LookupTableEntry("Decimal", 0),
			new LookupTableEntry("UpperRoman", 1),
			new LookupTableEntry("LowerRoman", 2),
			new LookupTableEntry("UpperLatin", 3),
			new LookupTableEntry("LowerLatin", 4),
			new LookupTableEntry("Ordinal", 5),
			new LookupTableEntry("Decimal", 6),
			new LookupTableEntry("Disc", 23),
			new LookupTableEntry("Box", 23),
			new LookupTableEntry("Circle", 23),
			new LookupTableEntry("Square", 23)
		};

		internal static LookupTableEntry[] HAlignTable = new LookupTableEntry[5]
		{
			new LookupTableEntry("", 4),
			new LookupTableEntry("Left", 0),
			new LookupTableEntry("Right", 1),
			new LookupTableEntry("Center", 2),
			new LookupTableEntry("Justify", 3)
		};

		internal static LookupTableEntry[] FontStretchTable = new LookupTableEntry[10]
		{
			new LookupTableEntry("", 0),
			new LookupTableEntry("Normal", 0),
			new LookupTableEntry("UltraCondensed", -80),
			new LookupTableEntry("ExtraCondensed", -60),
			new LookupTableEntry("Condensed", -40),
			new LookupTableEntry("SemiCondensed", -20),
			new LookupTableEntry("SemiExpanded", 20),
			new LookupTableEntry("Expanded", 40),
			new LookupTableEntry("ExtraExpanded", 60),
			new LookupTableEntry("UltraExpanded", 80)
		};

		internal static LookupTableEntry[] TypographyVariantsTable = new LookupTableEntry[3]
		{
			new LookupTableEntry("Normal", 1),
			new LookupTableEntry("Superscript", 2),
			new LookupTableEntry("Subscript", 3)
		};

		internal static int BasicLookup(LookupTableEntry[] entries, string name)
		{
			for (int i = 0; i < entries.Length; i++)
			{
				if (string.Compare(entries[i].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return entries[i].Value;
				}
			}
			return 0;
		}

		internal static bool ConvertToTag(ConverterState converterState, string tagName, ref XamlTag xamlTag)
		{
			if (tagName.Length == 0)
			{
				return false;
			}
			xamlTag = (XamlTag)BasicLookup(TagTable, tagName);
			return xamlTag != XamlTag.XTUnknown;
		}

		internal static bool ConvertToSuperSub(ConverterState converterState, string s, ref RtfSuperSubscript ss)
		{
			if (s.Length == 0)
			{
				return false;
			}
			ss = (RtfSuperSubscript)BasicLookup(TypographyVariantsTable, s);
			return ss != RtfSuperSubscript.None;
		}

		internal static bool ConvertToAttribute(ConverterState converterState, string attributeName, ref XamlAttribute xamlAttribute)
		{
			if (attributeName.Length == 0)
			{
				return false;
			}
			xamlAttribute = (XamlAttribute)BasicLookup(AttributeTable, attributeName);
			return xamlAttribute != XamlAttribute.XAUnknown;
		}

		internal static bool ConvertToFont(ConverterState converterState, string attributeName, ref long fontIndex)
		{
			if (attributeName.Length == 0)
			{
				return false;
			}
			FontTable fontTable = converterState.FontTable;
			FontTableEntry fontTableEntry = fontTable.FindEntryByName(attributeName);
			if (fontTableEntry == null)
			{
				fontTableEntry = fontTable.DefineEntry(fontTable.Count + 1);
				if (fontTableEntry != null)
				{
					fontTableEntry.Name = attributeName;
					fontTableEntry.ComputePreferredCodePage();
				}
			}
			if (fontTableEntry == null)
			{
				return false;
			}
			fontIndex = fontTableEntry.Index;
			return true;
		}

		internal static bool ConvertToFontSize(ConverterState converterState, ReadOnlySpan<char> s, ref double d)
		{
			if (s.Length == 0)
			{
				return false;
			}
			int num = s.Length - 1;
			while (num >= 0 && (s[num] < '0' || s[num] > '9') && s[num] != '.')
			{
				num--;
			}
			ReadOnlySpan<char> readOnlySpan = default(ReadOnlySpan<char>);
			if (num < s.Length - 1)
			{
				readOnlySpan = s.Slice(num + 1);
				s = s.Slice(0, num + 1);
			}
			bool num2 = Converters.StringToDouble(s, ref d);
			if (num2)
			{
				if (readOnlySpan.IsEmpty)
				{
					d = Converters.PxToPt(d);
				}
				d *= 2.0;
			}
			if (num2)
			{
				return d > 0.0;
			}
			return false;
		}

		internal static bool ConvertToTextIndent(ConverterState converterState, string s, ref double d)
		{
			return Converters.StringToDouble(s, ref d);
		}

		internal static bool ConvertToLineHeight(ConverterState converterState, string s, ref double d)
		{
			return Converters.StringToDouble(s, ref d);
		}

		internal static bool ConvertToColor(ConverterState converterState, string brush, ref long colorIndex)
		{
			if (brush.Length == 0)
			{
				return false;
			}
			ColorTable colorTable = converterState.ColorTable;
			if (brush[0] == '#')
			{
				int i = 1;
				uint num = 0u;
				for (; i < brush.Length && i < 9; i++)
				{
					char c = brush[i];
					if (c >= '0' && c <= '9')
					{
						num = (uint)((num << 4) + (c - 48));
						continue;
					}
					if (c >= 'A' && c <= 'F')
					{
						num = (uint)((num << 4) + (c - 65 + 10));
						continue;
					}
					if (c < 'a' || c > 'f')
					{
						break;
					}
					num = (uint)((num << 4) + (c - 97 + 10));
				}
				Color color = Color.FromRgb((byte)((num & 0xFF0000) >> 16), (byte)((num & 0xFF00) >> 8), (byte)(num & 0xFF));
				colorIndex = colorTable.AddColor(color);
				return colorIndex >= 0;
			}
			try
			{
				Color color2 = (Color)ColorConverter.ConvertFromString(brush);
				colorIndex = colorTable.AddColor(color2);
				return colorIndex >= 0;
			}
			catch (NotSupportedException)
			{
				return false;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		internal static bool ConvertToDecoration(ConverterState converterState, string decoration, ref ULState ulState, ref StrikeState strikeState)
		{
			ulState = ULState.ULNone;
			strikeState = StrikeState.StrikeNone;
			if (decoration.IndexOf("Underline", StringComparison.OrdinalIgnoreCase) != -1)
			{
				ulState = ULState.ULNormal;
			}
			if (decoration.IndexOf("Strikethrough", StringComparison.OrdinalIgnoreCase) != -1)
			{
				strikeState = StrikeState.StrikeNormal;
			}
			if (ulState == ULState.ULNone)
			{
				return strikeState != StrikeState.StrikeNone;
			}
			return true;
		}

		internal static bool ConvertToDir(ConverterState converterState, string dirName, ref DirState dirState)
		{
			if (dirName.Length == 0)
			{
				return false;
			}
			if (string.Compare("RightToLeft", dirName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				dirState = DirState.DirRTL;
				return true;
			}
			if (string.Compare("LeftToRight", dirName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				dirState = DirState.DirLTR;
				return true;
			}
			return false;
		}

		internal static bool ConvertTagToNodeType(XamlTag xamlTag, ref DocumentNodeType documentNodeType)
		{
			documentNodeType = DocumentNodeType.dnUnknown;
			switch (xamlTag)
			{
			default:
				return false;
			case XamlTag.XTBold:
			case XamlTag.XTItalic:
			case XamlTag.XTUnderline:
			case XamlTag.XTInline:
				documentNodeType = DocumentNodeType.dnInline;
				break;
			case XamlTag.XTHyperlink:
				documentNodeType = DocumentNodeType.dnHyperlink;
				break;
			case XamlTag.XTLineBreak:
				documentNodeType = DocumentNodeType.dnLineBreak;
				break;
			case XamlTag.XTInlineUIContainer:
				documentNodeType = DocumentNodeType.dnInlineUIContainer;
				break;
			case XamlTag.XTBlockUIContainer:
				documentNodeType = DocumentNodeType.dnBlockUIContainer;
				break;
			case XamlTag.XTImage:
				documentNodeType = DocumentNodeType.dnImage;
				break;
			case XamlTag.XTParagraph:
				documentNodeType = DocumentNodeType.dnParagraph;
				break;
			case XamlTag.XTSection:
				documentNodeType = DocumentNodeType.dnSection;
				break;
			case XamlTag.XTList:
				documentNodeType = DocumentNodeType.dnList;
				break;
			case XamlTag.XTListItem:
				documentNodeType = DocumentNodeType.dnListItem;
				break;
			case XamlTag.XTTable:
				documentNodeType = DocumentNodeType.dnTable;
				break;
			case XamlTag.XTTableBody:
				documentNodeType = DocumentNodeType.dnTableBody;
				break;
			case XamlTag.XTTableRow:
				documentNodeType = DocumentNodeType.dnRow;
				break;
			case XamlTag.XTTableCell:
				documentNodeType = DocumentNodeType.dnCell;
				break;
			}
			return true;
		}

		internal static bool ConvertToMarkerStyle(ConverterState converterState, string styleName, ref MarkerStyle ms)
		{
			ms = MarkerStyle.MarkerBullet;
			if (styleName.Length == 0)
			{
				return false;
			}
			ms = (MarkerStyle)BasicLookup(MarkerStyleTable, styleName);
			return true;
		}

		internal static bool ConvertToStartIndex(ConverterState converterState, string s, ref int i)
		{
			bool result = true;
			try
			{
				i = Convert.ToInt32(s, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				result = false;
			}
			catch (FormatException)
			{
				result = false;
			}
			return result;
		}

		internal static bool ConvertToThickness(ConverterState converterState, ReadOnlySpan<char> thickness, ref XamlThickness xthickness)
		{
			int num = 0;
			int num2 = 0;
			while (num2 < thickness.Length)
			{
				int i;
				for (i = num2; i < thickness.Length && thickness[i] != ','; i++)
				{
				}
				ReadOnlySpan<char> s = thickness.Slice(num2, i - num2);
				if (s.Length > 0)
				{
					double d = 0.0;
					if (!Converters.StringToDouble(s, ref d))
					{
						return false;
					}
					switch (num)
					{
					case 0:
						xthickness.Left = (float)d;
						break;
					case 1:
						xthickness.Top = (float)d;
						break;
					case 2:
						xthickness.Right = (float)d;
						break;
					case 3:
						xthickness.Bottom = (float)d;
						break;
					default:
						return false;
					}
					num++;
				}
				num2 = i + 1;
			}
			if (num == 1)
			{
				xthickness.Top = xthickness.Left;
				xthickness.Right = xthickness.Left;
				xthickness.Bottom = xthickness.Left;
				num = 4;
			}
			return num == 4;
		}

		internal static bool ConvertToHAlign(ConverterState converterState, string alignName, ref HAlign align)
		{
			if (alignName.Length == 0)
			{
				return false;
			}
			align = (HAlign)BasicLookup(HAlignTable, alignName);
			return true;
		}

		internal static bool ConvertToFontStretch(ConverterState converterState, string stretchName, ref long twips)
		{
			if (stretchName.Length == 0)
			{
				return false;
			}
			twips = BasicLookup(HAlignTable, stretchName);
			return true;
		}

		internal static void AppendRTFText(StringBuilder sb, string s, int cp)
		{
			if (cp <= 0)
			{
				cp = 1252;
			}
			Encoding e = null;
			byte[] rgAnsi = new byte[20];
			char[] rgChar = new char[20];
			for (int i = 0; i < s.Length; i++)
			{
				AppendRtfChar(sb, s[i], cp, ref e, rgAnsi, rgChar);
			}
		}

		internal static void EnsureParagraphClosed(ConverterState converterState)
		{
			DocumentNodeArray documentNodeArray = converterState.DocumentNodeArray;
			int num = documentNodeArray.FindPending(DocumentNodeType.dnParagraph);
			if (num >= 0)
			{
				documentNodeArray.EntryAt(num);
				documentNodeArray.CloseAt(num);
			}
		}

		private static void AppendRtfChar(StringBuilder sb, char c, int cp, ref Encoding e, byte[] rgAnsi, char[] rgChar)
		{
			if (c == '{' || c == '}' || c == '\\')
			{
				sb.Append('\\');
			}
			if (c == '\t')
			{
				sb.Append("\\tab ");
				return;
			}
			if (c == '\f')
			{
				sb.Append("\\page ");
				return;
			}
			if (c < '\u0080')
			{
				sb.Append(c);
				return;
			}
			switch (c)
			{
			case '\u00a0':
				sb.Append("\\~");
				break;
			case '—':
				sb.Append("\\emdash ");
				break;
			case '–':
				sb.Append("\\endash ");
				break;
			case '\u2003':
				sb.Append("\\emspace ");
				break;
			case '\u2002':
				sb.Append("\\enspace ");
				break;
			case '\u2005':
				sb.Append("\\qmspace ");
				break;
			case '•':
				sb.Append("\\bullet ");
				break;
			case '‘':
				sb.Append("\\lquote ");
				break;
			case '’':
				sb.Append("\\rquote ");
				break;
			case '“':
				sb.Append("\\ldblquote ");
				break;
			case '”':
				sb.Append("\\rdblquote ");
				break;
			case '\u200d':
				sb.Append("\\zwj ");
				break;
			case '\u200c':
				sb.Append("\\zwnj ");
				break;
			case '\u200e':
				sb.Append("\\ltrmark ");
				break;
			case '\u200f':
				sb.Append("\\rtlmark ");
				break;
			case '‑':
				sb.Append("\\_");
				break;
			default:
				AppendRtfUnicodeChar(sb, c, cp, ref e, rgAnsi, rgChar);
				break;
			}
		}

		private static void AppendRtfUnicodeChar(StringBuilder sb, char c, int cp, ref Encoding e, byte[] rgAnsi, char[] rgChar)
		{
			if (e == null)
			{
				e = InternalEncoding.GetEncoding(cp);
			}
			int bytes = e.GetBytes(new char[1] { c }, 0, 1, rgAnsi, 0);
			if (e.GetChars(rgAnsi, 0, bytes, rgChar, 0) == 1 && rgChar[0] == c)
			{
				for (int i = 0; i < bytes; i++)
				{
					sb.Append("\\'");
					sb.Append(rgAnsi[i].ToString("x", CultureInfo.InvariantCulture));
				}
			}
			else
			{
				sb.Append("\\u");
				sb.Append(((short)c).ToString(CultureInfo.InvariantCulture));
				sb.Append('?');
			}
		}
	}

	internal struct XamlThickness
	{
		private float _left;

		private float _top;

		private float _right;

		private float _bottom;

		internal float Left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		internal float Top
		{
			get
			{
				return _top;
			}
			set
			{
				_top = value;
			}
		}

		internal float Right
		{
			get
			{
				return _right;
			}
			set
			{
				_right = value;
			}
		}

		internal float Bottom
		{
			get
			{
				return _bottom;
			}
			set
			{
				_bottom = value;
			}
		}

		internal XamlThickness(float l, float t, float r, float b)
		{
			_left = l;
			_top = t;
			_right = r;
			_bottom = b;
		}
	}

	private string _xaml;

	private StringBuilder _rtfBuilder;

	private ConverterState _converterState;

	private XamlIn _xamlIn;

	private WpfPayload _wpfPayload;

	private const int DefaultCellXAsTwips = 1440;

	internal string Output => _rtfBuilder.ToString();

	internal bool GenerateListTables => _xamlIn.GenerateListTables;

	internal WpfPayload WpfPayload
	{
		set
		{
			_wpfPayload = value;
		}
	}

	internal ConverterState ConverterState => _converterState;

	internal XamlToRtfWriter(string xaml)
	{
		_xaml = xaml;
		_rtfBuilder = new StringBuilder();
		_xamlIn = new XamlIn(this, xaml);
		_converterState = new ConverterState();
		ColorTable colorTable = _converterState.ColorTable;
		colorTable.AddColor(Color.FromArgb(byte.MaxValue, 0, 0, 0));
		colorTable.AddColor(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		FontTableEntry fontTableEntry = _converterState.FontTable.DefineEntry(0);
		fontTableEntry.Name = "Times New Roman";
		fontTableEntry.ComputePreferredCodePage();
	}

	internal XamlToRtfError Process()
	{
		XamlToRtfError result = _xamlIn.Parse();
		XamlParserHelper.EnsureParagraphClosed(_converterState);
		_converterState.DocumentNodeArray.EstablishTreeRelationships();
		WriteOutput();
		return result;
	}

	private void BuildListTable()
	{
		ListLevelTable[] array = new ListLevelTable[9];
		for (int i = 0; i < 9; i++)
		{
			array[i] = new ListLevelTable();
		}
		ArrayList openLists = new ArrayList();
		int num = BuildListStyles(array, openLists);
		ListOverrideTable listOverrideTable = _converterState.ListOverrideTable;
		for (int i = 0; i < num; i++)
		{
			ListOverride listOverride = listOverrideTable.AddEntry();
			listOverride.ID = i + 1;
			listOverride.Index = i + 1;
		}
		ListTable listTable = _converterState.ListTable;
		for (int i = 0; i < num; i++)
		{
			ListTableEntry listTableEntry = listTable.AddEntry();
			listTableEntry.ID = i + 1;
			ListLevelTable levels = listTableEntry.Levels;
			for (int j = 0; j < 9; j++)
			{
				ListLevel listLevel = levels.AddEntry();
				ListLevelTable listLevelTable = array[j];
				if (listLevelTable.Count > i)
				{
					ListLevel listLevel2 = listLevelTable.EntryAt(i);
					listLevel.Marker = listLevel2.Marker;
					listLevel.StartIndex = listLevel2.StartIndex;
				}
			}
		}
	}

	private int BuildListStyles(ListLevelTable[] levels, ArrayList openLists)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int num = -1;
		bool flag = false;
		int num2 = 0;
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			while (i == num)
			{
				if (openLists.Count > 0)
				{
					openLists.RemoveRange(openLists.Count - 1, 1);
					if (openLists.Count > 0)
					{
						DocumentNode documentNode = (DocumentNode)openLists[openLists.Count - 1];
						num = documentNode.Index + documentNode.ChildCount + 1;
					}
					else
					{
						num = -1;
					}
				}
				else
				{
					num = -1;
				}
			}
			DocumentNode documentNode2 = documentNodeArray.EntryAt(i);
			switch (documentNode2.Type)
			{
			case DocumentNodeType.dnList:
				openLists.Add(documentNode2);
				num = documentNode2.Index + documentNode2.ChildCount + 1;
				flag = true;
				break;
			case DocumentNodeType.dnListItem:
				flag = true;
				break;
			case DocumentNodeType.dnParagraph:
			{
				if (!flag || openLists.Count <= 0)
				{
					break;
				}
				flag = false;
				DocumentNode documentNode3 = (DocumentNode)openLists[openLists.Count - 1];
				int num3 = openLists.Count;
				MarkerStyle marker = documentNode3.FormatState.Marker;
				long num4 = documentNode3.FormatState.StartIndex;
				if (num4 < 0)
				{
					num4 = 1L;
				}
				if (num3 > 9)
				{
					num3 = 9;
				}
				ListLevelTable listLevelTable = levels[num3 - 1];
				int j;
				for (j = 0; j < listLevelTable.Count; j++)
				{
					ListLevel listLevel = listLevelTable.EntryAt(j);
					if (listLevel.Marker == marker && listLevel.StartIndex == num4)
					{
						break;
					}
				}
				if (j == listLevelTable.Count)
				{
					ListLevel listLevel = listLevelTable.AddEntry();
					listLevel.Marker = marker;
					listLevel.StartIndex = num4;
					if (listLevelTable.Count > num2)
					{
						num2 = listLevelTable.Count;
					}
				}
				if (num3 > 1)
				{
					documentNode2.FormatState.ILVL = num3 - 1;
				}
				documentNode2.FormatState.ILS = j + 1;
				for (j = 0; j < openLists.Count; j++)
				{
					documentNode3 = (DocumentNode)openLists[j];
					if (documentNode3.FormatState.PNLVL < num3)
					{
						documentNode3.FormatState.PNLVL = num3;
					}
					if (documentNode3.FormatState.ILS == -1)
					{
						documentNode3.FormatState.ILS = documentNode2.FormatState.ILS;
					}
					else if (documentNode3.FormatState.ILS != documentNode2.FormatState.ILS)
					{
						documentNode3.FormatState.ILS = 0L;
					}
				}
				break;
			}
			}
		}
		return num2;
	}

	private void MergeParagraphMargins()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.Type != DocumentNodeType.dnParagraph)
			{
				continue;
			}
			long num = documentNode.FormatState.LI;
			long num2 = documentNode.FormatState.RI;
			DocumentNode parent = documentNode.Parent;
			while (parent != null && parent.Type != DocumentNodeType.dnCell)
			{
				if (parent.Type == DocumentNodeType.dnListItem || parent.Type == DocumentNodeType.dnList)
				{
					num += parent.FormatState.LI;
					num2 += parent.FormatState.RI;
				}
				parent = parent.Parent;
			}
			documentNode.FormatState.LI = num;
			documentNode.FormatState.RI = num2;
		}
	}

	private void GenerateListLabels()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		ArrayList arrayList = new ArrayList();
		long[] array = new long[documentNodeArray.Count];
		long[] array2 = new long[documentNodeArray.Count];
		int num = -1;
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			while (i == num)
			{
				if (arrayList.Count > 0)
				{
					arrayList.RemoveRange(arrayList.Count - 1, 1);
					if (arrayList.Count > 0)
					{
						DocumentNode documentNode = (DocumentNode)arrayList[arrayList.Count - 1];
						num = documentNode.Index + documentNode.ChildCount + 1;
					}
					else
					{
						num = -1;
					}
				}
				else
				{
					num = -1;
				}
			}
			DocumentNode documentNode2 = documentNodeArray.EntryAt(i);
			switch (documentNode2.Type)
			{
			case DocumentNodeType.dnList:
				arrayList.Add(documentNode2);
				array[arrayList.Count - 1] = documentNode2.FormatState.StartIndex - 1;
				array2[arrayList.Count - 1] = documentNode2.FormatState.StartIndex;
				num = documentNode2.Index + documentNode2.ChildCount + 1;
				break;
			case DocumentNodeType.dnListItem:
				if (arrayList.Count > 0)
				{
					array[arrayList.Count - 1] = array[arrayList.Count - 1] + 1;
				}
				break;
			case DocumentNodeType.dnParagraph:
				if (documentNode2.FormatState.ListLevel > 0 && arrayList.Count > 0)
				{
					DocumentNode documentNode3 = (DocumentNode)arrayList[arrayList.Count - 1];
					long nCount = array[arrayList.Count - 1];
					long startIndex = array2[arrayList.Count - 1];
					documentNode2.FormatState.StartIndex = startIndex;
					documentNode2.ListLabel = Converters.MarkerCountToString(documentNode3.FormatState.Marker, nCount);
				}
				break;
			}
		}
	}

	private void SetParagraphStructureProperties()
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.Type != DocumentNodeType.dnParagraph)
			{
				continue;
			}
			long num = 0L;
			for (DocumentNode parent = documentNode.Parent; parent != null; parent = parent.Parent)
			{
				if (parent.Type == DocumentNodeType.dnCell)
				{
					num++;
				}
			}
			if (num > 1)
			{
				documentNode.FormatState.ITAP = num;
			}
			if (num != 0L)
			{
				documentNode.FormatState.IsInTable = true;
			}
		}
	}

	private void WriteProlog()
	{
		_rtfBuilder.Append("{\\rtf1\\ansi\\ansicpg1252\\uc1\\htmautsp");
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		for (int i = 0; i < documentNodeArray.Count; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.FormatState.Font >= 0)
			{
				_rtfBuilder.Append("\\deff");
				_rtfBuilder.Append(documentNode.FormatState.Font.ToString(CultureInfo.InvariantCulture));
				break;
			}
		}
	}

	private void WriteHeaderTables()
	{
		WriteFontTable();
		WriteColorTable();
		if (GenerateListTables)
		{
			WriteListTable();
		}
	}

	private void WriteFontTable()
	{
		FontTable fontTable = _converterState.FontTable;
		_rtfBuilder.Append("{\\fonttbl");
		for (int i = 0; i < fontTable.Count; i++)
		{
			FontTableEntry fontTableEntry = fontTable.EntryAt(i);
			_rtfBuilder.Append('{');
			_rtfBuilder.Append("\\f");
			_rtfBuilder.Append(fontTableEntry.Index.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\fcharset");
			_rtfBuilder.Append(fontTableEntry.CharSet.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append(' ');
			XamlParserHelper.AppendRTFText(_rtfBuilder, fontTableEntry.Name, fontTableEntry.CodePage);
			_rtfBuilder.Append(";}");
		}
		_rtfBuilder.Append('}');
	}

	private void WriteColorTable()
	{
		ColorTable colorTable = _converterState.ColorTable;
		_rtfBuilder.Append("{\\colortbl");
		for (int i = 0; i < colorTable.Count; i++)
		{
			Color color = colorTable.ColorAt(i);
			_rtfBuilder.Append("\\red");
			_rtfBuilder.Append(color.R.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\green");
			_rtfBuilder.Append(color.G.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\blue");
			_rtfBuilder.Append(color.B.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append(';');
		}
		_rtfBuilder.Append('}');
	}

	private void WriteListTable()
	{
		ListTable listTable = _converterState.ListTable;
		if (listTable.Count > 0)
		{
			_rtfBuilder.Append("\r\n{\\*\\listtable");
			int num = 5;
			for (int i = 0; i < listTable.Count; i++)
			{
				ListTableEntry listTableEntry = listTable.EntryAt(i);
				_rtfBuilder.Append("\r\n{\\list");
				_rtfBuilder.Append("\\listtemplateid");
				_rtfBuilder.Append(listTableEntry.ID.ToString(CultureInfo.InvariantCulture));
				_rtfBuilder.Append("\\listhybrid");
				ListLevelTable levels = listTableEntry.Levels;
				for (int j = 0; j < levels.Count; j++)
				{
					ListLevel listLevel = levels.EntryAt(j);
					long num2 = (long)listLevel.Marker;
					_rtfBuilder.Append("\r\n{\\listlevel");
					_rtfBuilder.Append("\\levelnfc");
					_rtfBuilder.Append(num2.ToString(CultureInfo.InvariantCulture));
					_rtfBuilder.Append("\\levelnfcn");
					_rtfBuilder.Append(num2.ToString(CultureInfo.InvariantCulture));
					_rtfBuilder.Append("\\leveljc0");
					_rtfBuilder.Append("\\leveljcn0");
					_rtfBuilder.Append("\\levelfollow0");
					_rtfBuilder.Append("\\levelstartat");
					_rtfBuilder.Append(listLevel.StartIndex);
					_rtfBuilder.Append("\\levelspace0");
					_rtfBuilder.Append("\\levelindent0");
					_rtfBuilder.Append("{\\leveltext");
					_rtfBuilder.Append("\\leveltemplateid");
					_rtfBuilder.Append(num.ToString(CultureInfo.InvariantCulture));
					num++;
					if (listLevel.Marker == MarkerStyle.MarkerBullet)
					{
						_rtfBuilder.Append("\\'01\\'b7}");
						_rtfBuilder.Append("{\\levelnumbers;}");
					}
					else
					{
						_rtfBuilder.Append("\\'02\\'0");
						_rtfBuilder.Append(j.ToString(CultureInfo.InvariantCulture));
						_rtfBuilder.Append(".;}");
						_rtfBuilder.Append("{\\levelnumbers\\'01;}");
					}
					_rtfBuilder.Append("\\fi-360");
					_rtfBuilder.Append("\\li");
					string value = ((j + 1) * 720).ToString(CultureInfo.InvariantCulture);
					_rtfBuilder.Append(value);
					_rtfBuilder.Append("\\lin");
					_rtfBuilder.Append(value);
					_rtfBuilder.Append("\\jclisttab\\tx");
					_rtfBuilder.Append(value);
					_rtfBuilder.Append('}');
				}
				_rtfBuilder.Append("\r\n{\\listname ;}");
				_rtfBuilder.Append("\\listid");
				_rtfBuilder.Append(listTableEntry.ID.ToString(CultureInfo.InvariantCulture));
				_rtfBuilder.Append('}');
			}
			_rtfBuilder.Append("}\r\n");
		}
		ListOverrideTable listOverrideTable = _converterState.ListOverrideTable;
		if (listOverrideTable.Count <= 0)
		{
			return;
		}
		_rtfBuilder.Append("{\\*\\listoverridetable");
		for (int k = 0; k < listOverrideTable.Count; k++)
		{
			ListOverride listOverride = listOverrideTable.EntryAt(k);
			_rtfBuilder.Append("\r\n{\\listoverride");
			_rtfBuilder.Append("\\listid");
			_rtfBuilder.Append(listOverride.ID.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\listoverridecount0");
			if (listOverride.StartIndex > 0)
			{
				_rtfBuilder.Append("\\levelstartat");
				_rtfBuilder.Append(listOverride.StartIndex.ToString(CultureInfo.InvariantCulture));
			}
			_rtfBuilder.Append("\\ls");
			_rtfBuilder.Append(listOverride.Index.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append('}');
		}
		_rtfBuilder.Append("\r\n}\r\n");
	}

	private void WriteEmptyChild(DocumentNode documentNode)
	{
		if (documentNode.Type == DocumentNodeType.dnLineBreak)
		{
			_rtfBuilder.Append("\\line ");
		}
	}

	private void WriteInlineChild(DocumentNode documentNode)
	{
		if (documentNode.IsEmptyNode)
		{
			WriteEmptyChild(documentNode);
			return;
		}
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		FormatState formatState = documentNode.FormatState;
		FormatState formatState2 = ((documentNode.Parent != null) ? documentNode.Parent.FormatState : FormatState.EmptyFormatState);
		bool num = formatState.Font != formatState2.Font;
		bool flag = formatState.Bold != formatState2.Bold;
		bool flag2 = formatState.Italic != formatState2.Italic;
		bool flag3 = formatState.UL != formatState2.UL;
		bool flag4 = formatState.FontSize != formatState2.FontSize;
		bool flag5 = formatState.CF != formatState2.CF;
		bool flag6 = formatState.CB != formatState2.CB;
		bool flag7 = formatState.Strike != formatState2.Strike;
		bool flag8 = formatState.Super != formatState2.Super;
		bool flag9 = formatState.Sub != formatState2.Sub;
		bool flag10 = formatState.Lang != formatState2.Lang && formatState.Lang > 0;
		bool flag11 = formatState.DirChar != 0 && (documentNode.Parent == null || !documentNode.Parent.IsInline || formatState.Lang != formatState2.Lang);
		bool flag12 = num || flag || flag2 || flag3 || flag10 || flag11 || flag4 || flag5 || flag6 || flag7 || flag8 || flag9;
		if (flag12)
		{
			_rtfBuilder.Append('{');
		}
		if (flag10)
		{
			_rtfBuilder.Append("\\lang");
			_rtfBuilder.Append(formatState.Lang.ToString(CultureInfo.InvariantCulture));
		}
		if (num)
		{
			_rtfBuilder.Append("\\loch");
			_rtfBuilder.Append("\\f");
			_rtfBuilder.Append(formatState.Font.ToString(CultureInfo.InvariantCulture));
		}
		if (flag)
		{
			if (formatState.Bold)
			{
				_rtfBuilder.Append("\\b");
			}
			else
			{
				_rtfBuilder.Append("\\b0");
			}
		}
		if (flag2)
		{
			if (formatState.Italic)
			{
				_rtfBuilder.Append("\\i");
			}
			else
			{
				_rtfBuilder.Append("\\i0");
			}
		}
		if (flag3)
		{
			if (formatState.UL != 0)
			{
				_rtfBuilder.Append("\\ul");
			}
			else
			{
				_rtfBuilder.Append("\\ul0");
			}
		}
		if (flag7)
		{
			if (formatState.Strike != 0)
			{
				_rtfBuilder.Append("\\strike");
			}
			else
			{
				_rtfBuilder.Append("\\strike0");
			}
		}
		if (flag4)
		{
			_rtfBuilder.Append("\\fs");
			_rtfBuilder.Append(formatState.FontSize.ToString(CultureInfo.InvariantCulture));
		}
		if (flag5)
		{
			_rtfBuilder.Append("\\cf");
			_rtfBuilder.Append(formatState.CF.ToString(CultureInfo.InvariantCulture));
		}
		if (flag6)
		{
			_rtfBuilder.Append("\\highlight");
			_rtfBuilder.Append(formatState.CB.ToString(CultureInfo.InvariantCulture));
		}
		if (flag8)
		{
			if (formatState.Super)
			{
				_rtfBuilder.Append("\\super");
			}
			else
			{
				_rtfBuilder.Append("\\super0");
			}
		}
		if (flag9)
		{
			if (formatState.Sub)
			{
				_rtfBuilder.Append("\\sub");
			}
			else
			{
				_rtfBuilder.Append("\\sub0");
			}
		}
		if (flag11)
		{
			if (formatState.DirChar == DirState.DirLTR)
			{
				_rtfBuilder.Append("\\ltrch");
			}
			else
			{
				_rtfBuilder.Append("\\rtlch");
			}
		}
		if (flag12)
		{
			_rtfBuilder.Append(' ');
		}
		if (documentNode.Type == DocumentNodeType.dnHyperlink && !string.IsNullOrEmpty(documentNode.NavigateUri))
		{
			_rtfBuilder.Append("{\\field{\\*\\fldinst { HYPERLINK \"");
			documentNode.NavigateUri = BamlResourceContentUtil.UnescapeString(documentNode.NavigateUri);
			for (int i = 0; i < documentNode.NavigateUri.Length; i++)
			{
				if (documentNode.NavigateUri[i] == '\\')
				{
					_rtfBuilder.Append("\\\\");
				}
				else
				{
					_rtfBuilder.Append(documentNode.NavigateUri[i]);
				}
			}
			_rtfBuilder.Append("\" }}{\\fldrslt {");
		}
		else
		{
			_rtfBuilder.Append(documentNode.Content);
		}
		if (documentNode.Type == DocumentNodeType.dnImage)
		{
			WriteImage(documentNode);
		}
		int index = documentNode.Index;
		for (int j = index + 1; j <= index + documentNode.ChildCount; j++)
		{
			DocumentNode documentNode2 = documentNodeArray.EntryAt(j);
			if (documentNode2.Parent == documentNode)
			{
				WriteInlineChild(documentNode2);
			}
		}
		if (documentNode.Type == DocumentNodeType.dnHyperlink && !string.IsNullOrEmpty(documentNode.NavigateUri))
		{
			_rtfBuilder.Append("}}}");
		}
		if (flag12)
		{
			_rtfBuilder.Append('}');
		}
	}

	private void WriteUIContainerChild(DocumentNode documentNode)
	{
		_rtfBuilder.Append('{');
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		int index = documentNode.Index;
		for (int i = index + 1; i <= index + documentNode.ChildCount; i++)
		{
			DocumentNode documentNode2 = documentNodeArray.EntryAt(i);
			if (documentNode2.Parent == documentNode && documentNode2.Type == DocumentNodeType.dnImage)
			{
				WriteImage(documentNode2);
			}
		}
		if (documentNode.Type == DocumentNodeType.dnBlockUIContainer)
		{
			_rtfBuilder.Append("\\par");
		}
		_rtfBuilder.Append('}');
		_rtfBuilder.Append("\r\n");
	}

	private void WriteSection(DocumentNode dnThis)
	{
		int index = dnThis.Index;
		int num = index + 1;
		FormatState formatState = dnThis.FormatState;
		FormatState formatState2 = ((dnThis.Parent != null) ? dnThis.Parent.FormatState : FormatState.EmptyFormatState);
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		_rtfBuilder.Append('{');
		if (formatState.Lang != formatState2.Lang && formatState.Lang > 0)
		{
			_rtfBuilder.Append("\\lang");
			_rtfBuilder.Append(formatState.Lang.ToString(CultureInfo.InvariantCulture));
		}
		if (formatState.DirPara == DirState.DirRTL)
		{
			_rtfBuilder.Append("\\rtlpar");
		}
		if (WriteParagraphFontInfo(dnThis, formatState, formatState2))
		{
			_rtfBuilder.Append(' ');
		}
		if (formatState.CF != formatState2.CF)
		{
			_rtfBuilder.Append("\\cf");
			_rtfBuilder.Append(formatState.CF.ToString(CultureInfo.InvariantCulture));
		}
		switch (formatState.HAlign)
		{
		case HAlign.AlignLeft:
			if (formatState.DirPara != DirState.DirRTL)
			{
				_rtfBuilder.Append("\\ql");
			}
			else
			{
				_rtfBuilder.Append("\\qr");
			}
			break;
		case HAlign.AlignRight:
			if (formatState.DirPara != DirState.DirRTL)
			{
				_rtfBuilder.Append("\\qr");
			}
			else
			{
				_rtfBuilder.Append("\\ql");
			}
			break;
		case HAlign.AlignCenter:
			_rtfBuilder.Append("\\qc");
			break;
		case HAlign.AlignJustify:
			_rtfBuilder.Append("\\qj");
			break;
		}
		if (formatState.SL != 0L)
		{
			_rtfBuilder.Append("\\sl");
			_rtfBuilder.Append(formatState.SL.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\slmult0");
		}
		for (int i = num; i <= index + dnThis.ChildCount; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.Parent == dnThis)
			{
				WriteStructure(documentNode);
			}
		}
		_rtfBuilder.Append('}');
		_rtfBuilder.Append("\r\n");
	}

	private void WriteParagraph(DocumentNode dnThis)
	{
		int index = dnThis.Index;
		int num = index + 1;
		FormatState formatState = dnThis.FormatState;
		FormatState fsParent = ((dnThis.Parent != null) ? dnThis.Parent.FormatState : FormatState.EmptyFormatState);
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		_rtfBuilder.Append('{');
		bool flag = WriteParagraphFontInfo(dnThis, formatState, fsParent);
		if (formatState.IsInTable)
		{
			_rtfBuilder.Append("\\intbl");
			flag = true;
		}
		if (flag)
		{
			_rtfBuilder.Append(' ');
		}
		if (WriteParagraphListInfo(dnThis, formatState))
		{
			_rtfBuilder.Append(' ');
		}
		if (formatState.DirPara == DirState.DirRTL)
		{
			_rtfBuilder.Append("\\rtlpar");
		}
		for (int i = num; i <= index + dnThis.ChildCount; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.Parent == dnThis)
			{
				WriteInlineChild(documentNode);
			}
		}
		if (formatState.ITAP > 1)
		{
			_rtfBuilder.Append("\\itap");
			_rtfBuilder.Append(formatState.ITAP.ToString(CultureInfo.InvariantCulture));
		}
		_rtfBuilder.Append("\\li");
		_rtfBuilder.Append(formatState.LI.ToString(CultureInfo.InvariantCulture));
		_rtfBuilder.Append("\\ri");
		_rtfBuilder.Append(formatState.RI.ToString(CultureInfo.InvariantCulture));
		_rtfBuilder.Append("\\sa");
		_rtfBuilder.Append(formatState.SA.ToString(CultureInfo.InvariantCulture));
		_rtfBuilder.Append("\\sb");
		_rtfBuilder.Append(formatState.SB.ToString(CultureInfo.InvariantCulture));
		if (formatState.HasParaBorder)
		{
			_rtfBuilder.Append(formatState.ParaBorder.RTFEncoding);
		}
		if (dnThis.ListLabel != null)
		{
			_rtfBuilder.Append("\\jclisttab\\tx");
			_rtfBuilder.Append(formatState.LI.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\fi-360");
		}
		else
		{
			_rtfBuilder.Append("\\fi");
			_rtfBuilder.Append(formatState.FI.ToString(CultureInfo.InvariantCulture));
		}
		switch (formatState.HAlign)
		{
		case HAlign.AlignLeft:
			if (formatState.DirPara != DirState.DirRTL)
			{
				_rtfBuilder.Append("\\ql");
			}
			else
			{
				_rtfBuilder.Append("\\qr");
			}
			break;
		case HAlign.AlignRight:
			if (formatState.DirPara != DirState.DirRTL)
			{
				_rtfBuilder.Append("\\qr");
			}
			else
			{
				_rtfBuilder.Append("\\ql");
			}
			break;
		case HAlign.AlignCenter:
			_rtfBuilder.Append("\\qc");
			break;
		case HAlign.AlignJustify:
			_rtfBuilder.Append("\\qj");
			break;
		}
		if (formatState.CBPara >= 0)
		{
			_rtfBuilder.Append("\\cbpat");
			_rtfBuilder.Append(formatState.CBPara.ToString(CultureInfo.InvariantCulture));
		}
		if (formatState.SL != 0L)
		{
			_rtfBuilder.Append("\\sl");
			_rtfBuilder.Append(formatState.SL.ToString(CultureInfo.InvariantCulture));
			_rtfBuilder.Append("\\slmult0");
		}
		if (dnThis.IsLastParagraphInCell())
		{
			dnThis.GetParentOfType(DocumentNodeType.dnCell).IsTerminated = true;
			if (formatState.ITAP > 1)
			{
				_rtfBuilder.Append("\\nestcell");
				_rtfBuilder.Append("{\\nonesttables\\par}");
			}
			else
			{
				_rtfBuilder.Append("\\cell");
			}
			_rtfBuilder.Append("\r\n");
		}
		else
		{
			_rtfBuilder.Append("\\par");
		}
		_rtfBuilder.Append('}');
		_rtfBuilder.Append("\r\n");
	}

	private bool WriteParagraphFontInfo(DocumentNode dnThis, FormatState fsThis, FormatState fsParent)
	{
		int index = dnThis.Index;
		int num = index + 1;
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		bool result = false;
		long num2 = -2L;
		long num3 = -2L;
		for (int i = num; i <= index + dnThis.ChildCount; i++)
		{
			DocumentNode documentNode = documentNodeArray.EntryAt(i);
			if (documentNode.Parent == dnThis)
			{
				if (num2 == -2)
				{
					num2 = documentNode.FormatState.FontSize;
				}
				else if (num2 != documentNode.FormatState.FontSize)
				{
					num2 = -3L;
				}
				if (num3 == -2)
				{
					num3 = documentNode.FormatState.Font;
				}
				else if (num3 != documentNode.FormatState.Font)
				{
					num3 = -3L;
				}
			}
		}
		if (num2 >= 0)
		{
			fsThis.FontSize = num2;
		}
		if (num3 >= 0)
		{
			fsThis.Font = num3;
		}
		bool flag = dnThis.Type == DocumentNodeType.dnParagraph && dnThis.Parent != null && dnThis.Parent.Type == DocumentNodeType.dnSection && dnThis.Parent.Parent == null;
		if (fsThis.FontSize != fsParent.FontSize)
		{
			_rtfBuilder.Append("\\fs");
			_rtfBuilder.Append(fsThis.FontSize.ToString(CultureInfo.InvariantCulture));
			result = true;
		}
		if (fsThis.Font != fsParent.Font || flag)
		{
			_rtfBuilder.Append("\\f");
			_rtfBuilder.Append(fsThis.Font.ToString(CultureInfo.InvariantCulture));
			result = true;
		}
		if (fsThis.Bold != fsParent.Bold)
		{
			_rtfBuilder.Append("\\b");
			result = true;
		}
		if (fsThis.Italic != fsParent.Italic)
		{
			_rtfBuilder.Append("\\i");
			result = true;
		}
		if (fsThis.UL != fsParent.UL)
		{
			_rtfBuilder.Append("\\ul");
			result = true;
		}
		if (fsThis.Strike != fsParent.Strike)
		{
			_rtfBuilder.Append("\\strike");
			result = true;
		}
		if (fsThis.CF != fsParent.CF)
		{
			_rtfBuilder.Append("\\cf");
			_rtfBuilder.Append(fsThis.CF.ToString(CultureInfo.InvariantCulture));
			result = true;
		}
		return result;
	}

	private bool WriteParagraphListInfo(DocumentNode dnThis, FormatState fsThis)
	{
		bool result = false;
		bool flag = GenerateListTables;
		if (dnThis.ListLabel != null)
		{
			DocumentNode parentOfType = dnThis.GetParentOfType(DocumentNodeType.dnList);
			if (parentOfType != null)
			{
				if (flag && parentOfType.FormatState.PNLVL == 1)
				{
					flag = false;
				}
				if (flag)
				{
					_rtfBuilder.Append("{\\listtext ");
					_rtfBuilder.Append(dnThis.ListLabel);
					if (parentOfType.FormatState.Marker != MarkerStyle.MarkerBullet && parentOfType.FormatState.Marker != MarkerStyle.MarkerNone)
					{
						_rtfBuilder.Append('.');
					}
					_rtfBuilder.Append("\\tab}");
					if (fsThis.ILS > 0)
					{
						_rtfBuilder.Append("\\ls");
						_rtfBuilder.Append(fsThis.ILS.ToString(CultureInfo.InvariantCulture));
						result = true;
					}
					if (fsThis.ILVL > 0)
					{
						_rtfBuilder.Append("\\ilvl");
						_rtfBuilder.Append(fsThis.ILVL.ToString(CultureInfo.InvariantCulture));
						result = true;
					}
				}
				else
				{
					_rtfBuilder.Append("{\\pntext ");
					_rtfBuilder.Append(dnThis.ListLabel);
					if (parentOfType.FormatState.Marker != MarkerStyle.MarkerBullet && parentOfType.FormatState.Marker != MarkerStyle.MarkerNone)
					{
						_rtfBuilder.Append('.');
					}
					_rtfBuilder.Append("\\tab}{\\*\\pn");
					_rtfBuilder.Append(Converters.MarkerStyleToOldRTFString(parentOfType.FormatState.Marker));
					if (fsThis.ListLevel > 0 && parentOfType.FormatState.PNLVL > 1)
					{
						_rtfBuilder.Append("\\pnlvl");
						_rtfBuilder.Append(fsThis.ListLevel.ToString(CultureInfo.InvariantCulture));
					}
					if (fsThis.FI > 0)
					{
						_rtfBuilder.Append("\\pnhang");
					}
					if (fsThis.StartIndex >= 0)
					{
						_rtfBuilder.Append("\\pnstart");
						_rtfBuilder.Append(fsThis.StartIndex.ToString(CultureInfo.InvariantCulture));
					}
					if (parentOfType.FormatState.Marker == MarkerStyle.MarkerBullet)
					{
						_rtfBuilder.Append("{\\pntxtb\\'B7}}");
					}
					else if (parentOfType.FormatState.Marker == MarkerStyle.MarkerNone)
					{
						_rtfBuilder.Append("{\\pntxta }{\\pntxtb }}");
					}
					else
					{
						_rtfBuilder.Append("{\\pntxta .}}");
					}
					result = false;
				}
			}
		}
		return result;
	}

	private void WriteRow(DocumentNode dnRow)
	{
		int tableDepth = dnRow.GetTableDepth();
		_rtfBuilder.Append("\r\n");
		_rtfBuilder.Append('{');
		if (tableDepth == 1)
		{
			WriteRowStart(dnRow);
			WriteRowSettings(dnRow);
			WriteRowsCellProperties(dnRow);
		}
		else if (tableDepth > 1)
		{
			_rtfBuilder.Append("\\intbl\\itap");
			_rtfBuilder.Append(tableDepth.ToString(CultureInfo.InvariantCulture));
		}
		WriteRowsCellContents(dnRow);
		if (tableDepth > 1)
		{
			_rtfBuilder.Append("\\intbl\\itap");
			_rtfBuilder.Append(tableDepth.ToString(CultureInfo.InvariantCulture));
		}
		_rtfBuilder.Append('{');
		if (tableDepth > 1)
		{
			_rtfBuilder.Append("\\*\\nesttableprops");
		}
		WriteRowStart(dnRow);
		WriteRowSettings(dnRow);
		WriteRowsCellProperties(dnRow);
		if (tableDepth > 1)
		{
			_rtfBuilder.Append("\\nestrow");
		}
		else
		{
			_rtfBuilder.Append("\\row");
		}
		_rtfBuilder.Append("}}");
		_rtfBuilder.Append("\r\n");
	}

	private void WriteRowStart(DocumentNode dnRow)
	{
		_rtfBuilder.Append("\\trowd");
	}

	private void WriteRowSettings(DocumentNode dnRow)
	{
		DocumentNode parentOfType = dnRow.GetParentOfType(DocumentNodeType.dnTable);
		DirState num = parentOfType?.XamlDir ?? DirState.DirLTR;
		DirState dirState = parentOfType?.ParentXamlDir ?? DirState.DirLTR;
		if (parentOfType != null)
		{
			string value = ((dirState == DirState.DirLTR) ? parentOfType.FormatState.LI : parentOfType.FormatState.RI).ToString(CultureInfo.InvariantCulture);
			_rtfBuilder.Append("\\trleft");
			_rtfBuilder.Append(value);
			_rtfBuilder.Append("\\trgaph-");
			_rtfBuilder.Append(value);
		}
		else
		{
			_rtfBuilder.Append("\\trgaph0");
			_rtfBuilder.Append("\\trleft0");
		}
		WriteRowBorders(dnRow);
		WriteRowDimensions(dnRow);
		WriteRowPadding(dnRow);
		_rtfBuilder.Append("\\trql");
		if (num == DirState.DirRTL)
		{
			_rtfBuilder.Append("\\rtlrow");
		}
		else
		{
			_rtfBuilder.Append("\\ltrrow");
		}
	}

	private void WriteRowBorders(DocumentNode dnRow)
	{
		DocumentNodeArray rowsCells = dnRow.GetRowsCells();
		if (rowsCells.Count > 0)
		{
			DocumentNode documentNode = rowsCells.EntryAt(0);
			if (documentNode.FormatState.HasRowFormat)
			{
				CellFormat rowCellFormat = documentNode.FormatState.RowFormat.RowCellFormat;
				WriteBorder("\\trbrdrt", rowCellFormat.BorderTop);
				WriteBorder("\\trbrdrb", rowCellFormat.BorderBottom);
				WriteBorder("\\trbrdrr", rowCellFormat.BorderRight);
				WriteBorder("\\trbrdrl", rowCellFormat.BorderLeft);
				WriteBorder("\\trbrdrv", rowCellFormat.BorderLeft);
				WriteBorder("\\trbrdrh", rowCellFormat.BorderTop);
			}
		}
	}

	private void WriteRowDimensions(DocumentNode dnRow)
	{
		_rtfBuilder.Append("\\trftsWidth1");
		_rtfBuilder.Append("\\trftsWidthB3");
	}

	private void WriteRowPadding(DocumentNode dnRow)
	{
		_rtfBuilder.Append("\\trpaddl10");
		_rtfBuilder.Append("\\trpaddr10");
		_rtfBuilder.Append("\\trpaddb10");
		_rtfBuilder.Append("\\trpaddt10");
		_rtfBuilder.Append("\\trpaddfl3");
		_rtfBuilder.Append("\\trpaddfr3");
		_rtfBuilder.Append("\\trpaddft3");
		_rtfBuilder.Append("\\trpaddfb3");
	}

	private void WriteRowsCellProperties(DocumentNode dnRow)
	{
		DocumentNodeArray rowsCells = dnRow.GetRowsCells();
		int num = 0;
		long lastCellX = 0L;
		for (int i = 0; i < rowsCells.Count; i++)
		{
			DocumentNode documentNode = rowsCells.EntryAt(i);
			lastCellX = WriteCellProperties(documentNode, num, lastCellX);
			num += documentNode.ColSpan;
		}
	}

	private void WriteRowsCellContents(DocumentNode dnRow)
	{
		DocumentNodeArray rowsCells = dnRow.GetRowsCells();
		_rtfBuilder.Append('{');
		for (int i = 0; i < rowsCells.Count; i++)
		{
			DocumentNode dnThis = rowsCells.EntryAt(i);
			WriteStructure(dnThis);
		}
		_rtfBuilder.Append('}');
	}

	private long WriteCellProperties(DocumentNode dnCell, int nCol, long lastCellX)
	{
		WriteCellColor(dnCell);
		if (dnCell.FormatState.HasRowFormat)
		{
			if (dnCell.FormatState.RowFormat.RowCellFormat.IsVMergeFirst)
			{
				_rtfBuilder.Append("\\clvmgf");
			}
			else if (dnCell.FormatState.RowFormat.RowCellFormat.IsVMerge)
			{
				_rtfBuilder.Append("\\clvmrg");
			}
		}
		WriteCellVAlignment(dnCell);
		WriteCellBorders(dnCell);
		WriteCellPadding(dnCell);
		return WriteCellDimensions(dnCell, nCol, lastCellX);
	}

	private void WriteCellVAlignment(DocumentNode dnCell)
	{
		_rtfBuilder.Append("\\clvertalt");
	}

	private void WriteCellBorders(DocumentNode dnCell)
	{
		if (dnCell.FormatState.HasRowFormat)
		{
			CellFormat rowCellFormat = dnCell.FormatState.RowFormat.RowCellFormat;
			WriteBorder("\\clbrdrt", rowCellFormat.BorderTop);
			WriteBorder("\\clbrdrl", rowCellFormat.BorderLeft);
			WriteBorder("\\clbrdrb", rowCellFormat.BorderBottom);
			WriteBorder("\\clbrdrr", rowCellFormat.BorderRight);
		}
		else
		{
			WriteBorder("\\clbrdrt", BorderFormat.EmptyBorderFormat);
			WriteBorder("\\clbrdrl", BorderFormat.EmptyBorderFormat);
			WriteBorder("\\clbrdrb", BorderFormat.EmptyBorderFormat);
			WriteBorder("\\clbrdrr", BorderFormat.EmptyBorderFormat);
		}
	}

	private void WriteCellPadding(DocumentNode dnCell)
	{
	}

	private void WriteCellColor(DocumentNode dnCell)
	{
		FormatState formatState = null;
		if (dnCell.FormatState.CBPara >= 0)
		{
			formatState = dnCell.FormatState;
		}
		else if (dnCell.Parent != null && dnCell.Parent.FormatState.CBPara >= 0)
		{
			formatState = dnCell.Parent.FormatState;
		}
		if (formatState != null)
		{
			_rtfBuilder.Append("\\clcbpat");
			_rtfBuilder.Append(formatState.CBPara.ToString(CultureInfo.InvariantCulture));
		}
	}

	private long WriteCellDimensions(DocumentNode dnCell, int nCol, long lastCellX)
	{
		DocumentNode parentOfType = dnCell.GetParentOfType(DocumentNodeType.dnTable);
		if (parentOfType.FormatState.HasRowFormat)
		{
			RowFormat rowFormat = parentOfType.FormatState.RowFormat;
			CellFormat cellFormat = rowFormat.NthCellFormat(nCol);
			if (dnCell.ColSpan > 1)
			{
				CellFormat cellFormat2 = new CellFormat(cellFormat);
				for (int i = 1; i < dnCell.ColSpan; i++)
				{
					cellFormat = rowFormat.NthCellFormat(nCol + i);
					cellFormat2.Width.Value += cellFormat.Width.Value;
					cellFormat2.CellX = cellFormat.CellX;
				}
				if (cellFormat2.CellX == -1 || rowFormat.CellCount == 0)
				{
					cellFormat2.CellX = lastCellX + dnCell.ColSpan * 1440 + GetDefaultAllTablesWidthFromCell(dnCell);
				}
				_rtfBuilder.Append(cellFormat2.RTFEncodingForWidth);
				lastCellX = cellFormat2.CellX;
			}
			else
			{
				if (cellFormat.CellX == -1 || rowFormat.CellCount == 0)
				{
					cellFormat.CellX = lastCellX + 1440 + GetDefaultAllTablesWidthFromCell(dnCell);
				}
				_rtfBuilder.Append(cellFormat.RTFEncodingForWidth);
				lastCellX = cellFormat.CellX;
			}
		}
		else
		{
			_rtfBuilder.Append("\\clftsWidth1");
			_rtfBuilder.Append("\\cellx");
			long num = lastCellX + dnCell.ColSpan * 1440;
			_rtfBuilder.Append(num.ToString(CultureInfo.InvariantCulture));
			lastCellX = num;
		}
		return lastCellX;
	}

	private long GetDefaultAllTablesWidthFromCell(DocumentNode dnCell)
	{
		long num = 0L;
		for (int i = dnCell.Index + 1; i <= dnCell.Index + dnCell.ChildCount; i++)
		{
			DocumentNode documentNode = _converterState.DocumentNodeArray.EntryAt(i);
			if (documentNode.Type == DocumentNodeType.dnTable)
			{
				num += CalculateDefaultTableWidth(documentNode);
			}
		}
		return num;
	}

	private long CalculateDefaultTableWidth(DocumentNode dnTable)
	{
		long num = 0L;
		long num2 = 0L;
		for (int i = dnTable.Index + 1; i <= dnTable.Index + dnTable.ChildCount; i++)
		{
			DocumentNode documentNode = _converterState.DocumentNodeArray.EntryAt(i);
			if (documentNode.Type == DocumentNodeType.dnRow)
			{
				num = 0L;
				DocumentNodeArray rowsCells = documentNode.GetRowsCells();
				for (int j = 0; j < rowsCells.Count; j++)
				{
					DocumentNode documentNode2 = rowsCells.EntryAt(j);
					num += documentNode2.ColSpan * 1440;
				}
			}
			else if (documentNode.Type == DocumentNodeType.dnTable)
			{
				i += documentNode.ChildCount;
			}
			num2 = Math.Max(num2, num);
		}
		return num2;
	}

	private void WriteBorder(string borderControlWord, BorderFormat bf)
	{
		_rtfBuilder.Append(borderControlWord);
		_rtfBuilder.Append(bf.RTFEncoding);
	}

	private void PatchVerticallyMergedCells(DocumentNode dnThis)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		DocumentNodeArray tableRows = dnThis.GetTableRows();
		DocumentNodeArray documentNodeArray2 = new DocumentNodeArray();
		ArrayList arrayList = new ArrayList();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < tableRows.Count; i++)
		{
			DocumentNode documentNode = tableRows.EntryAt(i);
			DocumentNodeArray rowsCells = documentNode.GetRowsCells();
			int j = 0;
			for (int k = 0; k < rowsCells.Count; k++)
			{
				DocumentNode documentNode2 = rowsCells.EntryAt(k);
				DocumentNode documentNode4;
				for (num2 = j; num2 < arrayList.Count && (int)arrayList[num2] > 0; num2 += documentNode4.ColSpan)
				{
					DocumentNode documentNode3 = documentNodeArray2.EntryAt(num2);
					documentNode4 = new DocumentNode(DocumentNodeType.dnCell);
					documentNodeArray.InsertChildAt(documentNode, documentNode4, documentNode2.Index, 0);
					documentNode4.FormatState = new FormatState(documentNode3.FormatState);
					if (documentNode3.FormatState.HasRowFormat)
					{
						documentNode4.FormatState.RowFormat = new RowFormat(documentNode3.FormatState.RowFormat);
					}
					documentNode4.FormatState.RowFormat.RowCellFormat.IsVMergeFirst = false;
					documentNode4.FormatState.RowFormat.RowCellFormat.IsVMerge = true;
					documentNode4.ColSpan = documentNode3.ColSpan;
				}
				for (; j < arrayList.Count && (int)arrayList[j] > 0; j++)
				{
					arrayList[j] = (int)arrayList[j] - 1;
					if ((int)arrayList[j] == 0)
					{
						documentNodeArray2[j] = null;
					}
				}
				for (int l = 0; l < documentNode2.ColSpan; l++)
				{
					if (j < arrayList.Count)
					{
						arrayList[j] = documentNode2.RowSpan - 1;
						documentNodeArray2[j] = ((documentNode2.RowSpan > 1) ? documentNode2 : null);
					}
					else
					{
						arrayList.Add(documentNode2.RowSpan - 1);
						documentNodeArray2.Add((documentNode2.RowSpan > 1) ? documentNode2 : null);
					}
					j++;
				}
				if (documentNode2.RowSpan > 1)
				{
					documentNode2.FormatState.RowFormat.RowCellFormat.IsVMergeFirst = true;
				}
			}
			num2 = j;
			while (num2 < arrayList.Count)
			{
				if ((int)arrayList[num2] > 0)
				{
					DocumentNode documentNode5 = documentNodeArray2.EntryAt(num2);
					DocumentNode documentNode6 = new DocumentNode(DocumentNodeType.dnCell);
					documentNodeArray.InsertChildAt(documentNode, documentNode6, documentNode.Index + documentNode.ChildCount + 1, 0);
					documentNode6.FormatState = new FormatState(documentNode5.FormatState);
					if (documentNode5.FormatState.HasRowFormat)
					{
						documentNode6.FormatState.RowFormat = new RowFormat(documentNode5.FormatState.RowFormat);
					}
					documentNode6.FormatState.RowFormat.RowCellFormat.IsVMergeFirst = false;
					documentNode6.FormatState.RowFormat.RowCellFormat.IsVMerge = true;
					documentNode6.ColSpan = documentNode5.ColSpan;
					num2 += documentNode6.ColSpan;
				}
				else
				{
					num2++;
				}
			}
			for (; j < arrayList.Count; j++)
			{
				if ((int)arrayList[j] > 0)
				{
					arrayList[j] = (int)arrayList[j] - 1;
					if ((int)arrayList[j] == 0)
					{
						documentNodeArray2[j] = null;
					}
				}
			}
			if (j > num)
			{
				num = j;
			}
		}
	}

	private void WriteStructure(DocumentNode dnThis)
	{
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		bool flag = dnThis.GetParentOfType(DocumentNodeType.dnCell) != null;
		switch (dnThis.Type)
		{
		default:
			return;
		case DocumentNodeType.dnSection:
			WriteSection(dnThis);
			return;
		case DocumentNodeType.dnParagraph:
			WriteParagraph(dnThis);
			return;
		case DocumentNodeType.dnInline:
			WriteInlineChild(dnThis);
			return;
		case DocumentNodeType.dnInlineUIContainer:
		case DocumentNodeType.dnBlockUIContainer:
			WriteUIContainerChild(dnThis);
			return;
		case DocumentNodeType.dnTable:
			if (dnThis.FormatState.HasRowFormat)
			{
				dnThis.FormatState.RowFormat.Trleft = dnThis.FormatState.LI;
				dnThis.FormatState.RowFormat.CanonicalizeWidthsFromXaml();
			}
			PatchVerticallyMergedCells(dnThis);
			break;
		case DocumentNodeType.dnRow:
			WriteRow(dnThis);
			break;
		case DocumentNodeType.dnLineBreak:
		case DocumentNodeType.dnHyperlink:
		case DocumentNodeType.dnImage:
			return;
		case DocumentNodeType.dnList:
		case DocumentNodeType.dnListItem:
		case DocumentNodeType.dnTableBody:
		case DocumentNodeType.dnCell:
			break;
		}
		if (dnThis.Type != DocumentNodeType.dnRow)
		{
			int index = dnThis.Index;
			for (int i = index + 1; i <= index + dnThis.ChildCount; i++)
			{
				DocumentNode documentNode = documentNodeArray.EntryAt(i);
				if (documentNode.Parent == dnThis)
				{
					WriteStructure(documentNode);
				}
			}
		}
		switch (dnThis.Type)
		{
		case DocumentNodeType.dnCell:
			if (!dnThis.IsTerminated)
			{
				_rtfBuilder.Append(flag ? "\\nestcell" : "\\cell");
				_rtfBuilder.Append("\r\n");
			}
			break;
		case DocumentNodeType.dnList:
		case DocumentNodeType.dnListItem:
		case DocumentNodeType.dnTable:
		case DocumentNodeType.dnTableBody:
		case DocumentNodeType.dnRow:
			break;
		}
	}

	private void WriteDocumentContents()
	{
		_rtfBuilder.Append("\\loch\\hich\\dbch\\pard\\plain\\ltrpar\\itap0");
		DocumentNodeArray documentNodeArray = _converterState.DocumentNodeArray;
		DocumentNode documentNode;
		for (int i = 0; i < documentNodeArray.Count; i += documentNode.ChildCount + 1)
		{
			documentNode = documentNodeArray.EntryAt(i);
			WriteStructure(documentNode);
		}
	}

	private void WriteEpilog()
	{
		_rtfBuilder.Append('}');
	}

	private void WriteOutput()
	{
		BuildListTable();
		SetParagraphStructureProperties();
		MergeParagraphMargins();
		GenerateListLabels();
		WriteProlog();
		WriteHeaderTables();
		WriteDocumentContents();
		WriteEpilog();
	}

	private void WriteImage(DocumentNode documentNode)
	{
		if (_wpfPayload == null)
		{
			return;
		}
		using Stream imageStream = _wpfPayload.GetImageStream(documentNode.FormatState.ImageSource);
		RtfImageFormat imageFormatFromImageSourceName = GetImageFormatFromImageSourceName(documentNode.FormatState.ImageSource);
		WriteShapeImage(documentNode, imageStream, imageFormatFromImageSourceName);
	}

	private void WriteShapeImage(DocumentNode documentNode, Stream imageStream, RtfImageFormat imageFormat)
	{
		_rtfBuilder.Append("{\\*\\shppict{\\pict");
		Size availableSize = new Size(documentNode.FormatState.ImageWidth, documentNode.FormatState.ImageHeight);
		BitmapSource bitmapSource = BitmapFrame.Create(imageStream);
		Size contentSize = ((bitmapSource == null) ? new Size(availableSize.Width, availableSize.Height) : new Size(bitmapSource.Width, bitmapSource.Height));
		Stretch imageStretch = GetImageStretch(documentNode.FormatState.ImageStretch);
		StretchDirection imageStretchDirection = GetImageStretchDirection(documentNode.FormatState.ImageStretchDirection);
		if (availableSize.Width == 0.0)
		{
			if (availableSize.Height == 0.0)
			{
				availableSize.Width = contentSize.Width;
			}
			else
			{
				availableSize.Width = contentSize.Width * (availableSize.Height / contentSize.Height);
			}
		}
		if (availableSize.Height == 0.0)
		{
			if (availableSize.Width == 0.0)
			{
				availableSize.Height = contentSize.Height;
			}
			else
			{
				availableSize.Height = contentSize.Height * (availableSize.Width / contentSize.Width);
			}
		}
		Size size = Viewbox.ComputeScaleFactor(availableSize, contentSize, imageStretch, imageStretchDirection);
		if (documentNode.FormatState.IncludeImageBaselineOffset)
		{
			_rtfBuilder.Append("\\dn");
			_rtfBuilder.Append(Converters.PxToHalfPointRounded(contentSize.Height * size.Height - documentNode.FormatState.ImageBaselineOffset));
		}
		_rtfBuilder.Append("\\picwgoal");
		_rtfBuilder.Append(Converters.PxToTwipRounded(contentSize.Width * size.Width).ToString(CultureInfo.InvariantCulture));
		_rtfBuilder.Append("\\pichgoal");
		_rtfBuilder.Append(Converters.PxToTwipRounded(contentSize.Height * size.Height).ToString(CultureInfo.InvariantCulture));
		switch (imageFormat)
		{
		case RtfImageFormat.Bmp:
		case RtfImageFormat.Dib:
		case RtfImageFormat.Gif:
		case RtfImageFormat.Png:
		case RtfImageFormat.Tif:
			_rtfBuilder.Append("\\pngblip");
			break;
		case RtfImageFormat.Jpeg:
			_rtfBuilder.Append("\\jpegblip");
			break;
		}
		_rtfBuilder.Append("\r\n");
		if (imageFormat != 0)
		{
			string value = ConvertToImageHexDataString(imageStream);
			_rtfBuilder.Append(value);
		}
		_rtfBuilder.Append("}}");
	}

	private string ConvertToImageHexDataString(Stream imageStream)
	{
		byte[] array = new byte[imageStream.Length * 2];
		imageStream.Position = 0L;
		for (int i = 0; i < imageStream.Length; i++)
		{
			Converters.ByteToHex((byte)imageStream.ReadByte(), out array[i * 2], out array[i * 2 + 1]);
		}
		return InternalEncoding.GetEncoding(1252).GetString(array);
	}

	private RtfImageFormat GetImageFormatFromImageSourceName(string imageName)
	{
		RtfImageFormat result = RtfImageFormat.Unknown;
		int num = imageName.LastIndexOf('.');
		if (num >= 0)
		{
			string strB = imageName.Substring(num);
			if (string.Compare(".png", strB, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = RtfImageFormat.Png;
			}
			if (string.Compare(".jpeg", strB, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(".jpg", strB, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = RtfImageFormat.Jpeg;
			}
			if (string.Compare(".gif", strB, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = RtfImageFormat.Gif;
			}
			if (string.Compare(".tif", strB, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(".tiff", strB, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = RtfImageFormat.Tif;
			}
			if (string.Compare(".bmp", strB, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = RtfImageFormat.Bmp;
			}
			if (string.Compare(".dib", strB, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = RtfImageFormat.Dib;
			}
		}
		return result;
	}

	private Stretch GetImageStretch(string imageStretch)
	{
		if (string.Compare("Fill", imageStretch, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return Stretch.Fill;
		}
		if (string.Compare("UniformToFill", imageStretch, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return Stretch.UniformToFill;
		}
		return Stretch.Uniform;
	}

	private StretchDirection GetImageStretchDirection(string imageStretchDirection)
	{
		if (string.Compare("UpOnly", imageStretchDirection, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return StretchDirection.UpOnly;
		}
		if (string.Compare("DownOnly", imageStretchDirection, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return StretchDirection.DownOnly;
		}
		return StretchDirection.Both;
	}
}
