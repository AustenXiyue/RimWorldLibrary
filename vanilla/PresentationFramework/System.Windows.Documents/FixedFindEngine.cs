using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Markup;
using System.Xml;

namespace System.Windows.Documents;

internal sealed class FixedFindEngine
{
	private static string[] _predefinedNamespaces = new string[2] { "http://schemas.microsoft.com/xps/2005/06", "http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key" };

	internal static TextRange Find(ITextPointer start, ITextPointer end, string findPattern, CultureInfo cultureInfo, bool matchCase, bool matchWholeWord, bool matchLast, bool matchDiacritics, bool matchKashida, bool matchAlefHamza)
	{
		if (findPattern.Length == 0)
		{
			return null;
		}
		IDocumentPaginatorSource documentPaginatorSource = start.TextContainer.Parent as IDocumentPaginatorSource;
		DynamicDocumentPaginator dynamicDocumentPaginator = documentPaginatorSource.DocumentPaginator as DynamicDocumentPaginator;
		int num = -1;
		int num2 = -1;
		if (matchLast)
		{
			num2 = dynamicDocumentPaginator.GetPageNumber((ContentPosition)start);
			num = dynamicDocumentPaginator.GetPageNumber((ContentPosition)end);
		}
		else
		{
			num2 = dynamicDocumentPaginator.GetPageNumber((ContentPosition)end);
			num = dynamicDocumentPaginator.GetPageNumber((ContentPosition)start);
		}
		TextRange textRange = null;
		_ = cultureInfo.CompareInfo;
		bool replaceAlefWithAlefHamza = false;
		CompareOptions compareOptions = _InitializeSearch(cultureInfo, matchCase, matchAlefHamza, matchDiacritics, ref findPattern, out replaceAlefWithAlefHamza);
		int childPageNumber = num;
		FixedDocumentSequence fixedDocumentSequence = documentPaginatorSource as FixedDocumentSequence;
		DynamicDocumentPaginator childPaginator = null;
		fixedDocumentSequence?.TranslatePageNumber(num, out childPaginator, out childPageNumber);
		if (num - num2 != 0)
		{
			ITextPointer firstSearchPageStart = null;
			ITextPointer firstSearchPageEnd = null;
			_GetFirstPageSearchPointers(start, end, childPageNumber, matchLast, out firstSearchPageStart, out firstSearchPageEnd);
			textRange = TextFindEngine.InternalFind(firstSearchPageStart, firstSearchPageEnd, findPattern, cultureInfo, matchCase, matchWholeWord, matchLast, matchDiacritics, matchKashida, matchAlefHamza);
			if (textRange == null)
			{
				num = (matchLast ? (num - 1) : (num + 1));
				for (int num3 = ((!matchLast) ? 1 : (-1)); matchLast ? (num >= num2) : (num <= num2); num += num3)
				{
					FixedDocument fixedDocument = null;
					childPageNumber = num;
					childPaginator = null;
					if (fixedDocumentSequence != null)
					{
						fixedDocumentSequence.TranslatePageNumber(num, out childPaginator, out childPageNumber);
						fixedDocument = (FixedDocument)childPaginator.Source;
					}
					else
					{
						fixedDocument = documentPaginatorSource as FixedDocument;
					}
					string text = _GetPageString(fixedDocument, childPageNumber, replaceAlefWithAlefHamza);
					if (text == null)
					{
						return TextFindEngine.InternalFind(start, end, findPattern, cultureInfo, matchCase, matchWholeWord, matchLast, matchDiacritics, matchKashida, matchAlefHamza);
					}
					if (!_FoundOnPage(text, findPattern, cultureInfo, compareOptions))
					{
						continue;
					}
					if (fixedDocumentSequence != null)
					{
						ChildDocumentBlock childBlock = fixedDocumentSequence.TextContainer.FindChildBlock(fixedDocument.DocumentReference);
						if (matchLast)
						{
							end = new DocumentSequenceTextPointer(childBlock, new FixedTextPointer(mutable: false, LogicalDirection.Backward, fixedDocument.FixedContainer.FixedTextBuilder.GetPageEndFlowPosition(childPageNumber)));
							start = new DocumentSequenceTextPointer(childBlock, new FixedTextPointer(mutable: false, LogicalDirection.Forward, fixedDocument.FixedContainer.FixedTextBuilder.GetPageStartFlowPosition(childPageNumber)));
						}
						else
						{
							start = new DocumentSequenceTextPointer(childBlock, new FixedTextPointer(mutable: false, LogicalDirection.Forward, fixedDocument.FixedContainer.FixedTextBuilder.GetPageStartFlowPosition(childPageNumber)));
							end = new DocumentSequenceTextPointer(childBlock, new FixedTextPointer(mutable: false, LogicalDirection.Backward, fixedDocument.FixedContainer.FixedTextBuilder.GetPageEndFlowPosition(childPageNumber)));
						}
					}
					else
					{
						FixedTextBuilder fixedTextBuilder = ((FixedDocument)documentPaginatorSource).FixedContainer.FixedTextBuilder;
						if (matchLast)
						{
							end = new FixedTextPointer(mutable: false, LogicalDirection.Backward, fixedTextBuilder.GetPageEndFlowPosition(num));
							start = new FixedTextPointer(mutable: false, LogicalDirection.Forward, fixedTextBuilder.GetPageStartFlowPosition(num));
						}
						else
						{
							start = new FixedTextPointer(mutable: false, LogicalDirection.Forward, fixedTextBuilder.GetPageStartFlowPosition(num));
							end = new FixedTextPointer(mutable: false, LogicalDirection.Backward, fixedTextBuilder.GetPageEndFlowPosition(num));
						}
					}
					textRange = TextFindEngine.InternalFind(start, end, findPattern, cultureInfo, matchCase, matchWholeWord, matchLast, matchDiacritics, matchKashida, matchAlefHamza);
					if (textRange != null)
					{
						return textRange;
					}
				}
			}
		}
		else
		{
			string text2 = _GetPageString((childPaginator != null) ? (childPaginator.Source as FixedDocument) : (documentPaginatorSource as FixedDocument), childPageNumber, replaceAlefWithAlefHamza);
			if (text2 == null || _FoundOnPage(text2, findPattern, cultureInfo, compareOptions))
			{
				textRange = TextFindEngine.InternalFind(start, end, findPattern, cultureInfo, matchCase, matchWholeWord, matchLast, matchDiacritics, matchKashida, matchAlefHamza);
			}
		}
		return textRange;
	}

	private static bool _FoundOnPage(string pageString, string findPattern, CultureInfo cultureInfo, CompareOptions compareOptions)
	{
		CompareInfo compareInfo = cultureInfo.CompareInfo;
		string[] array = findPattern.Split((char[]?)null);
		if (array != null)
		{
			string[] array2 = array;
			foreach (string value in array2)
			{
				if (!string.IsNullOrEmpty(value) && compareInfo.IndexOf(pageString, value, compareOptions) == -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static CompareOptions _InitializeSearch(CultureInfo cultureInfo, bool matchCase, bool matchAlefHamza, bool matchDiacritics, ref string findPattern, out bool replaceAlefWithAlefHamza)
	{
		CompareOptions compareOptions = CompareOptions.None;
		replaceAlefWithAlefHamza = false;
		if (!matchCase)
		{
			compareOptions |= CompareOptions.IgnoreCase;
		}
		TextFindEngine.InitializeBidiFlags(findPattern, out var stringContainedBidiCharacter, out var stringContainedAlefCharacter);
		if (stringContainedAlefCharacter && !matchAlefHamza)
		{
			findPattern = TextFindEngine.ReplaceAlefHamzaWithAlef(findPattern);
			replaceAlefWithAlefHamza = true;
		}
		if (!matchDiacritics && stringContainedBidiCharacter)
		{
			compareOptions |= CompareOptions.IgnoreNonSpace;
		}
		return compareOptions;
	}

	private static void _GetFirstPageSearchPointers(ITextPointer start, ITextPointer end, int pageNumber, bool matchLast, out ITextPointer firstSearchPageStart, out ITextPointer firstSearchPageEnd)
	{
		if (matchLast)
		{
			if (end is DocumentSequenceTextPointer documentSequenceTextPointer)
			{
				FlowPosition pageStartFlowPosition = ((FixedTextContainer)documentSequenceTextPointer.ChildBlock.ChildContainer).FixedTextBuilder.GetPageStartFlowPosition(pageNumber);
				firstSearchPageStart = new DocumentSequenceTextPointer(documentSequenceTextPointer.ChildBlock, new FixedTextPointer(mutable: false, LogicalDirection.Forward, pageStartFlowPosition));
			}
			else
			{
				FixedTextPointer fixedTextPointer = end as FixedTextPointer;
				firstSearchPageStart = new FixedTextPointer(mutable: false, LogicalDirection.Forward, fixedTextPointer.FixedTextContainer.FixedTextBuilder.GetPageStartFlowPosition(pageNumber));
			}
			firstSearchPageEnd = end;
		}
		else
		{
			if (start is DocumentSequenceTextPointer documentSequenceTextPointer2)
			{
				FlowPosition pageEndFlowPosition = ((FixedTextContainer)documentSequenceTextPointer2.ChildBlock.ChildContainer).FixedTextBuilder.GetPageEndFlowPosition(pageNumber);
				firstSearchPageEnd = new DocumentSequenceTextPointer(documentSequenceTextPointer2.ChildBlock, new FixedTextPointer(mutable: false, LogicalDirection.Backward, pageEndFlowPosition));
			}
			else
			{
				FixedTextPointer fixedTextPointer2 = start as FixedTextPointer;
				firstSearchPageEnd = new FixedTextPointer(mutable: false, LogicalDirection.Backward, fixedTextPointer2.FixedTextContainer.FixedTextBuilder.GetPageEndFlowPosition(pageNumber));
			}
			firstSearchPageStart = start;
		}
	}

	private static string _GetPageString(FixedDocument doc, int translatedPageNo, bool replaceAlefWithAlefHamza)
	{
		string text = null;
		using Stream stream = doc.Pages[translatedPageNo].GetPageStream();
		bool reverseRTL = true;
		if (doc.HasExplicitStructure)
		{
			reverseRTL = false;
		}
		if (stream != null)
		{
			text = _ConstructPageString(stream, reverseRTL);
			if (replaceAlefWithAlefHamza)
			{
				text = TextFindEngine.ReplaceAlefHamzaWithAlef(text);
			}
		}
		return text;
	}

	private static string _ConstructPageString(Stream pageStream, bool reverseRTL)
	{
		XmlReader reader = new XmlCompatibilityReader(new XmlTextReader(pageStream), _predefinedNamespaces);
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreWhitespace = true;
		xmlReaderSettings.IgnoreComments = true;
		xmlReaderSettings.ProhibitDtd = true;
		reader = XmlReader.Create(reader, xmlReaderSettings);
		reader.MoveToContent();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		string text = null;
		while (reader.Read())
		{
			if (reader.NodeType != XmlNodeType.Element || !(reader.Name == "Glyphs"))
			{
				continue;
			}
			text = reader.GetAttribute("UnicodeString");
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string attribute = reader.GetAttribute("IsSideways");
			flag = false;
			if (attribute != null && string.Compare(attribute, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
			{
				flag = true;
			}
			if (reverseRTL)
			{
				string attribute2 = reader.GetAttribute("BidiLevel");
				int num = 0;
				if (!string.IsNullOrEmpty(attribute2))
				{
					try
					{
						num = Convert.ToInt32(attribute2, CultureInfo.InvariantCulture);
					}
					catch (Exception)
					{
					}
				}
				string attribute3 = reader.GetAttribute("CaretStops");
				if (num == 0 && !flag && string.IsNullOrEmpty(attribute3) && FixedTextBuilder.MostlyRTL(text))
				{
					char[] array = text.ToCharArray();
					Array.Reverse(array);
					text = new string(array);
				}
			}
			stringBuilder.Append(text);
		}
		return stringBuilder.ToString();
	}

	private FixedFindEngine()
	{
	}
}
