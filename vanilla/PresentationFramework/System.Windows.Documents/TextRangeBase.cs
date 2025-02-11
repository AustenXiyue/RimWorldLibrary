using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using MS.Internal;

namespace System.Windows.Documents;

internal static class TextRangeBase
{
	private const char NumberSuffix = '.';

	private const string DecimalNumerics = "0123456789";

	private const string LowerLatinNumerics = "abcdefghijklmnopqrstuvwxyz";

	private const string UpperLatinNumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private static string[][] RomanNumerics = new string[2][]
	{
		new string[4] { "m??", "cdm", "xlc", "ivx" },
		new string[4] { "M??", "CDM", "XLC", "IVX" }
	};

	internal static bool Contains(ITextRange thisRange, ITextPointer textPointer)
	{
		NormalizeRange(thisRange);
		if (textPointer == null)
		{
			throw new ArgumentNullException("textPointer");
		}
		if (textPointer.TextContainer != thisRange.Start.TextContainer)
		{
			throw new ArgumentException(SR.NotInAssociatedTree, "textPointer");
		}
		if (textPointer.CompareTo(thisRange.Start) < 0)
		{
			textPointer = textPointer.GetFormatNormalizedPosition(LogicalDirection.Forward);
		}
		else if (textPointer.CompareTo(thisRange.End) > 0)
		{
			textPointer = textPointer.GetFormatNormalizedPosition(LogicalDirection.Backward);
		}
		for (int i = 0; i < thisRange._TextSegments.Count; i++)
		{
			if (thisRange._TextSegments[i].Contains(textPointer))
			{
				return true;
			}
		}
		return false;
	}

	internal static void Select(ITextRange thisRange, ITextPointer position1, ITextPointer position2)
	{
		Select(thisRange, position1, position2, includeCellAtMovingPosition: false);
	}

	internal static void Select(ITextRange thisRange, ITextPointer position1, ITextPointer position2, bool includeCellAtMovingPosition)
	{
		if (thisRange._TextSegments == null)
		{
			SelectPrivate(thisRange, position1, position2, includeCellAtMovingPosition, markRangeChanged: false);
			return;
		}
		ValidationHelper.VerifyPosition(thisRange.Start.TextContainer, position1, "position1");
		ValidationHelper.VerifyPosition(thisRange.Start.TextContainer, position2, "position2");
		BeginChange(thisRange);
		try
		{
			SelectPrivate(thisRange, position1, position2, includeCellAtMovingPosition, markRangeChanged: true);
		}
		finally
		{
			EndChange(thisRange);
		}
	}

	internal static void SelectWord(ITextRange thisRange, ITextPointer position)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		ITextPointer textPointer = position.CreatePointer();
		textPointer.MoveToInsertionPosition(LogicalDirection.Backward);
		TextSegment wordRange = TextPointerBase.GetWordRange(textPointer);
		Select(thisRange, wordRange.Start, wordRange.End);
	}

	internal static TextSegment GetAutoWord(ITextRange thisRange)
	{
		TextSegment result = TextSegment.Null;
		if (thisRange.IsEmpty && !TextPointerBase.IsAtWordBoundary(thisRange.Start, LogicalDirection.Forward) && !TextPointerBase.IsAtWordBoundary(thisRange.Start, LogicalDirection.Backward))
		{
			result = TextPointerBase.GetWordRange(thisRange.Start);
			string text = GetTextInternal(result.Start, result.End).TrimEnd(' ');
			if (GetTextInternal(result.Start, thisRange.Start).Length >= text.Length)
			{
				result = TextSegment.Null;
			}
		}
		return result;
	}

	internal static void SelectParagraph(ITextRange thisRange, ITextPointer position)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		FindParagraphOrListItemBoundaries(position, out var start, out var end);
		Select(thisRange, start, end);
	}

	internal static void ApplyInitialTypingHeuristics(ITextRange thisRange)
	{
		if (thisRange.IsTableCellRange)
		{
			TableCell tableCellFromPosition;
			if (thisRange.Start is TextPointer && (tableCellFromPosition = TextRangeEditTables.GetTableCellFromPosition((TextPointer)thisRange.Start)) != null)
			{
				thisRange.Select(tableCellFromPosition.ContentStart, tableCellFromPosition.ContentEnd);
			}
			else
			{
				thisRange.Select(thisRange.Start, thisRange.Start);
			}
		}
	}

	internal static void ApplyFinalTypingHeuristics(ITextRange thisRange, bool overType)
	{
		if (overType && thisRange.IsEmpty && !TextPointerBase.IsNextToAnyBreak(thisRange.End, LogicalDirection.Forward))
		{
			ITextPointer textPointer = thisRange.End.CreatePointer();
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
			if (!TextRangeEditTables.IsTableStructureCrossed(thisRange.Start, textPointer))
			{
				TextRange textRange = new TextRange(thisRange.Start, textPointer);
				Invariant.Assert(!textRange.IsTableCellRange);
				textRange.Text = string.Empty;
			}
		}
		if (!thisRange.IsEmpty && (TextPointerBase.IsNextToAnyBreak(thisRange.End, LogicalDirection.Backward) || TextPointerBase.IsAfterLastParagraph(thisRange.End)))
		{
			ITextPointer nextInsertionPosition = thisRange.End.GetNextInsertionPosition(LogicalDirection.Backward);
			thisRange.Select(thisRange.Start, nextInsertionPosition);
		}
	}

	internal static void ApplyTypingHeuristics(ITextRange thisRange, bool overType)
	{
		BeginChange(thisRange);
		try
		{
			ApplyInitialTypingHeuristics(thisRange);
			ApplyFinalTypingHeuristics(thisRange, overType);
		}
		finally
		{
			EndChange(thisRange);
		}
	}

	internal static void FindParagraphOrListItemBoundaries(ITextPointer position, out ITextPointer start, out ITextPointer end)
	{
		start = position.CreatePointer();
		end = position.CreatePointer();
		SkipParagraphContent(start, LogicalDirection.Backward);
		SkipParagraphContent(end, LogicalDirection.Forward);
	}

	private static void SkipParagraphContent(ITextPointer navigator, LogicalDirection direction)
	{
		TextPointerContext pointerContext = navigator.GetPointerContext(direction);
		while (pointerContext != 0 && (((pointerContext != TextPointerContext.ElementStart || direction != LogicalDirection.Forward) && (pointerContext != TextPointerContext.ElementEnd || direction != 0)) || typeof(Inline).IsAssignableFrom(navigator.GetElementType(direction))) && (((pointerContext != TextPointerContext.ElementEnd || direction != LogicalDirection.Forward) && (pointerContext != TextPointerContext.ElementStart || direction != 0)) || typeof(Inline).IsAssignableFrom(navigator.ParentType)) && navigator.MoveToNextContextPosition(direction))
		{
			pointerContext = navigator.GetPointerContext(direction);
		}
	}

	internal static object GetPropertyValue(ITextRange thisRange, DependencyProperty formattingProperty)
	{
		if (TextSchema.IsCharacterProperty(formattingProperty))
		{
			return GetCharacterPropertyValue(thisRange, formattingProperty);
		}
		Invariant.Assert(TextSchema.IsParagraphProperty(formattingProperty), "The property is expected to be one of either character or paragraph formatting one");
		return GetParagraphPropertyValue(thisRange, formattingProperty);
	}

	private static object GetCharacterPropertyValue(ITextRange thisRange, DependencyProperty formattingProperty)
	{
		object characterValueFromPosition = GetCharacterValueFromPosition(thisRange.Start, formattingProperty);
		for (int i = 0; i < thisRange._TextSegments.Count; i++)
		{
			TextSegment textSegment = thisRange._TextSegments[i];
			ITextPointer textPointer = textSegment.Start.CreatePointer();
			bool flag = true;
			while (flag && textPointer.CompareTo(textSegment.End) < 0)
			{
				if (!TextSchema.ValuesAreEqual(GetCharacterValueFromPosition(textPointer, formattingProperty), characterValueFromPosition))
				{
					return DependencyProperty.UnsetValue;
				}
				if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
				{
					flag = textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				}
				flag = textPointer.MoveToInsertionPosition(LogicalDirection.Forward);
				if (!flag)
				{
					flag = textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
				}
			}
		}
		return characterValueFromPosition;
	}

	private static object GetCharacterValueFromPosition(ITextPointer pointer, DependencyProperty formattingProperty)
	{
		object obj = null;
		if (formattingProperty != Inline.TextDecorationsProperty)
		{
			obj = pointer.GetValue(formattingProperty);
		}
		else if (pointer is TextPointer)
		{
			DependencyObject dependencyObject = ((TextPointer)pointer).Parent as TextElement;
			while (obj == null && (dependencyObject is Inline || dependencyObject is Paragraph || dependencyObject is TextBlock))
			{
				obj = dependencyObject.GetValue(formattingProperty);
				dependencyObject = ((dependencyObject is TextElement) ? ((TextElement)dependencyObject).Parent : null);
			}
		}
		return obj;
	}

	private static object GetParagraphPropertyValue(ITextRange thisRange, DependencyProperty formattingProperty)
	{
		object obj = null;
		for (int i = 0; i < thisRange._TextSegments.Count; i++)
		{
			TextSegment textSegment = thisRange._TextSegments[i];
			ITextPointer textPointer = textSegment.Start.CreatePointer();
			while (!typeof(Paragraph).IsAssignableFrom(textPointer.ParentType) && textPointer.MoveToNextContextPosition(LogicalDirection.Backward))
			{
			}
			for (bool flag = true; flag && textPointer.CompareTo(textSegment.End) <= 0; flag = textPointer.MoveToNextContextPosition(LogicalDirection.Forward))
			{
				if (typeof(Paragraph).IsAssignableFrom(textPointer.ParentType))
				{
					object value = textPointer.GetValue(formattingProperty);
					if (obj == null)
					{
						obj = value;
					}
					if (!TextSchema.ValuesAreEqual(value, obj))
					{
						return DependencyProperty.UnsetValue;
					}
					textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
				}
			}
		}
		if (obj == null)
		{
			obj = thisRange.Start.GetValue(formattingProperty);
		}
		return obj;
	}

	internal static bool IsParagraphBoundaryCrossed(ITextRange thisRange)
	{
		ITextPointer textPointer = thisRange.Start.CreatePointer();
		ITextPointer textPointer2 = thisRange.End.CreatePointer();
		if (TextPointerBase.IsAfterLastParagraph(textPointer2))
		{
			textPointer2.MoveToInsertionPosition(LogicalDirection.Backward);
		}
		while (typeof(Inline).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
		}
		while (typeof(Inline).IsAssignableFrom(textPointer2.ParentType))
		{
			textPointer2.MoveToElementEdge(ElementEdge.AfterEnd);
		}
		return !textPointer.HasEqualScope(textPointer2);
	}

	internal static void BeginChange(ITextRange thisRange)
	{
		BeginChangeWorker(thisRange, string.Empty);
	}

	internal static void BeginChangeNoUndo(ITextRange thisRange)
	{
		BeginChangeWorker(thisRange, null);
	}

	internal static void EndChange(ITextRange thisRange)
	{
		EndChange(thisRange, disableScroll: false, skipEvents: false);
	}

	internal static void EndChange(ITextRange thisRange, bool disableScroll, bool skipEvents)
	{
		Invariant.Assert(thisRange._ChangeBlockLevel > 0, "Unmatched EndChange call!");
		ITextContainer textContainer = thisRange.Start.TextContainer;
		try
		{
			bool isChanged;
			try
			{
				textContainer.EndChange(skipEvents);
			}
			finally
			{
				thisRange._ChangeBlockLevel--;
				isChanged = thisRange._IsChanged;
				if (thisRange._ChangeBlockLevel == 0)
				{
					thisRange._IsChanged = false;
				}
			}
			if (thisRange._ChangeBlockLevel == 0 && isChanged)
			{
				thisRange.NotifyChanged(disableScroll, skipEvents);
			}
		}
		finally
		{
			ChangeBlockUndoRecord changeBlockUndoRecord = thisRange._ChangeBlockUndoRecord;
			if (changeBlockUndoRecord != null && thisRange._ChangeBlockLevel == 0)
			{
				try
				{
					changeBlockUndoRecord.OnEndChange();
				}
				finally
				{
					thisRange._ChangeBlockUndoRecord = null;
				}
			}
		}
	}

	internal static void NotifyChanged(ITextRange thisRange, bool disableScroll)
	{
		thisRange.FireChanged();
	}

	internal static string GetTextInternal(ITextPointer startPosition, ITextPointer endPosition)
	{
		char[] charArray = null;
		return GetTextInternal(startPosition, endPosition, ref charArray);
	}

	internal static string GetTextInternal(ITextPointer startPosition, ITextPointer endPosition, ref char[] charArray)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Stack<int> listItemCounter = null;
		ITextPointer textPointer = startPosition.CreatePointer();
		Invariant.Assert(startPosition.CompareTo(endPosition) <= 0, "expecting: startPosition <= endPosition");
		while (textPointer.CompareTo(endPosition) < 0)
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.Text:
				PlainConvertTextRun(stringBuilder, textPointer, endPosition, ref charArray);
				break;
			case TextPointerContext.ElementEnd:
			{
				Type elementType = textPointer.ParentType;
				if (typeof(Paragraph).IsAssignableFrom(elementType) || typeof(BlockUIContainer).IsAssignableFrom(elementType))
				{
					PlainConvertParagraphEnd(stringBuilder, textPointer);
				}
				else if (typeof(LineBreak).IsAssignableFrom(elementType))
				{
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					stringBuilder.Append(Environment.NewLine);
				}
				else if (typeof(List).IsAssignableFrom(elementType))
				{
					PlainConvertListEnd(textPointer, ref listItemCounter);
				}
				else
				{
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				}
				break;
			}
			case TextPointerContext.EmbeddedElement:
				stringBuilder.Append(' ');
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			case TextPointerContext.ElementStart:
			{
				Type elementType = textPointer.GetElementType(LogicalDirection.Forward);
				if (typeof(AnchoredBlock).IsAssignableFrom(elementType))
				{
					stringBuilder.Append(Environment.NewLine);
				}
				else if (typeof(List).IsAssignableFrom(elementType) && textPointer is TextPointer)
				{
					PlainConvertListStart(textPointer, ref listItemCounter);
				}
				else if (typeof(ListItem).IsAssignableFrom(elementType))
				{
					PlainConvertListItemStart(stringBuilder, textPointer, ref listItemCounter);
				}
				else
				{
					PlainConvertAccessKey(stringBuilder, textPointer);
				}
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			}
			default:
				Invariant.Assert(condition: false, "Unexpected vlue for TextPointerContext");
				break;
			}
		}
		return stringBuilder.ToString();
	}

	private static void PlainConvertTextRun(StringBuilder textBuffer, ITextPointer navigator, ITextPointer endPosition, ref char[] charArray)
	{
		int textRunLength = navigator.GetTextRunLength(LogicalDirection.Forward);
		charArray = EnsureCharArraySize(charArray, textRunLength);
		textRunLength = TextPointerBase.GetTextWithLimit(navigator, LogicalDirection.Forward, charArray, 0, textRunLength, endPosition);
		textBuffer.Append(charArray, 0, textRunLength);
		navigator.MoveToNextContextPosition(LogicalDirection.Forward);
	}

	private static void PlainConvertParagraphEnd(StringBuilder textBuffer, ITextPointer navigator)
	{
		navigator.MoveToElementEdge(ElementEdge.BeforeStart);
		bool num = navigator.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart;
		navigator.MoveToNextContextPosition(LogicalDirection.Forward);
		navigator.MoveToElementEdge(ElementEdge.AfterEnd);
		TextPointerContext pointerContext = navigator.GetPointerContext(LogicalDirection.Forward);
		if (num && pointerContext == TextPointerContext.ElementEnd && typeof(TableCell).IsAssignableFrom(navigator.ParentType))
		{
			navigator.MoveToNextContextPosition(LogicalDirection.Forward);
			pointerContext = navigator.GetPointerContext(LogicalDirection.Forward);
			if (pointerContext == TextPointerContext.ElementStart)
			{
				textBuffer.Append('\t');
			}
			else
			{
				textBuffer.Append(Environment.NewLine);
			}
		}
		else
		{
			textBuffer.Append(Environment.NewLine);
		}
	}

	private static void PlainConvertListStart(ITextPointer navigator, ref Stack<int> listItemCounter)
	{
		_ = (List)navigator.GetAdjacentElement(LogicalDirection.Forward);
		if (listItemCounter == null)
		{
			listItemCounter = new Stack<int>(1);
		}
		listItemCounter.Push(0);
	}

	private static void PlainConvertListEnd(ITextPointer navigator, ref Stack<int> listItemCounter)
	{
		if (listItemCounter != null && listItemCounter.Count > 0)
		{
			listItemCounter.Pop();
		}
		navigator.MoveToNextContextPosition(LogicalDirection.Forward);
	}

	private static void PlainConvertListItemStart(StringBuilder textBuffer, ITextPointer navigator, ref Stack<int> listItemCounter)
	{
		if (navigator is TextPointer)
		{
			List list = (List)((TextPointer)navigator).Parent;
			ListItem listItem = (ListItem)navigator.GetAdjacentElement(LogicalDirection.Forward);
			if (listItemCounter == null)
			{
				listItemCounter = new Stack<int>(1);
			}
			if (listItemCounter.Count == 0)
			{
				listItemCounter.Push(((IList)listItem.SiblingListItems).IndexOf((object?)listItem));
			}
			Invariant.Assert(listItemCounter.Count > 0, "expectinng listItemCounter.Count > 0");
			int num = listItemCounter.Pop();
			int num2 = list?.StartIndex ?? 0;
			TextMarkerStyle listMarkerStyle = list?.MarkerStyle ?? TextMarkerStyle.Disc;
			WriteListMarker(textBuffer, listMarkerStyle, num + num2);
			num++;
			listItemCounter.Push(num);
		}
	}

	private static void PlainConvertAccessKey(StringBuilder textBuffer, ITextPointer navigator)
	{
		if (AccessText.HasCustomSerialization(navigator.GetAdjacentElement(LogicalDirection.Forward)))
		{
			textBuffer.Append(AccessText.AccessKeyMarker);
		}
	}

	private static char[] EnsureCharArraySize(char[] charArray, int textLength)
	{
		if (charArray == null)
		{
			charArray = new char[textLength + 10];
		}
		else if (charArray.Length < textLength)
		{
			int num = charArray.Length * 2;
			if (num < textLength)
			{
				num = textLength + 10;
			}
			charArray = new char[num];
		}
		return charArray;
	}

	private static void WriteListMarker(StringBuilder textBuffer, TextMarkerStyle listMarkerStyle, int listItemNumber)
	{
		string text = null;
		char[] array = null;
		switch (listMarkerStyle)
		{
		case TextMarkerStyle.None:
			text = "";
			break;
		case TextMarkerStyle.Disc:
			text = "•";
			break;
		case TextMarkerStyle.Circle:
			text = "○";
			break;
		case TextMarkerStyle.Square:
			text = "□";
			break;
		case TextMarkerStyle.Box:
			text = "■";
			break;
		case TextMarkerStyle.Decimal:
			array = ConvertNumberToString(listItemNumber, oneBased: false, "0123456789");
			break;
		case TextMarkerStyle.LowerLatin:
			array = ConvertNumberToString(listItemNumber, oneBased: true, "abcdefghijklmnopqrstuvwxyz");
			break;
		case TextMarkerStyle.UpperLatin:
			array = ConvertNumberToString(listItemNumber, oneBased: true, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			break;
		case TextMarkerStyle.LowerRoman:
			text = ConvertNumberToRomanString(listItemNumber, uppercase: false);
			break;
		case TextMarkerStyle.UpperRoman:
			text = ConvertNumberToRomanString(listItemNumber, uppercase: true);
			break;
		}
		if (text != null)
		{
			textBuffer.Append(text);
		}
		else if (array != null)
		{
			textBuffer.Append(array, 0, array.Length);
		}
		textBuffer.Append('\t');
	}

	private static char[] ConvertNumberToString(int number, bool oneBased, string numericSymbols)
	{
		if (oneBased)
		{
			number--;
		}
		Invariant.Assert(number >= 0, "expecting: number >= 0");
		int length = numericSymbols.Length;
		char[] array;
		if (number < length)
		{
			array = new char[2]
			{
				numericSymbols[number],
				'.'
			};
		}
		else
		{
			int num = (oneBased ? 1 : 0);
			int num2 = 1;
			int num3 = length;
			int num4 = length;
			while (number >= num3)
			{
				num4 *= length;
				num3 = num4 + num3 * num;
				num2++;
			}
			array = new char[num2 + 1];
			array[num2] = '.';
			for (int num5 = num2 - 1; num5 >= 0; num5--)
			{
				array[num5] = numericSymbols[number % length];
				number = number / length - num;
			}
		}
		return array;
	}

	private static string ConvertNumberToRomanString(int number, bool uppercase)
	{
		if (number > 3999)
		{
			return number.ToString(CultureInfo.InvariantCulture);
		}
		StringBuilder stringBuilder = new StringBuilder();
		AddRomanNumeric(stringBuilder, number / 1000, RomanNumerics[uppercase ? 1u : 0u][0]);
		number %= 1000;
		AddRomanNumeric(stringBuilder, number / 100, RomanNumerics[uppercase ? 1u : 0u][1]);
		number %= 100;
		AddRomanNumeric(stringBuilder, number / 10, RomanNumerics[uppercase ? 1u : 0u][2]);
		number %= 10;
		AddRomanNumeric(stringBuilder, number, RomanNumerics[uppercase ? 1u : 0u][3]);
		stringBuilder.Append('.');
		return stringBuilder.ToString();
	}

	private static void AddRomanNumeric(StringBuilder builder, int number, string oneFiveTen)
	{
		Invariant.Assert(number >= 0 && number <= 9, "expecting: number >= 0 && number <= 9");
		switch (number)
		{
		case 4:
		case 9:
			builder.Append(oneFiveTen[0]);
			break;
		case 1:
		case 2:
		case 3:
		case 5:
		case 6:
		case 7:
		case 8:
			break;
		default:
			return;
		}
		if (number == 9)
		{
			builder.Append(oneFiveTen[2]);
			return;
		}
		if (number >= 4)
		{
			builder.Append(oneFiveTen[1]);
		}
		int num = number % 5;
		while (num > 0 && num < 4)
		{
			builder.Append(oneFiveTen[0]);
			num--;
		}
	}

	internal static ITextPointer GetStart(ITextRange thisRange)
	{
		NormalizeRange(thisRange);
		Invariant.Assert(thisRange._TextSegments != null && thisRange._TextSegments.Count > 0, "expecting nonempty _TextSegments array for Start position");
		return thisRange._TextSegments[0].Start;
	}

	internal static ITextPointer GetEnd(ITextRange thisRange)
	{
		NormalizeRange(thisRange);
		Invariant.Assert(thisRange._TextSegments != null && thisRange._TextSegments.Count > 0, "expecting nonempty _TextSegments array for End position");
		return thisRange._TextSegments[thisRange._TextSegments.Count - 1].End;
	}

	internal static bool GetIsEmpty(ITextRange thisRange)
	{
		NormalizeRange(thisRange);
		Invariant.Assert((thisRange._TextSegments.Count == 1 && thisRange._TextSegments[0].Start == thisRange._TextSegments[0].End) == (thisRange.Start.CompareTo(thisRange.End) == 0), "Range emptiness assumes using one instance of TextPointer for both start and end");
		if (thisRange._TextSegments.Count == 1)
		{
			return thisRange._TextSegments[0].Start == thisRange._TextSegments[0].End;
		}
		return false;
	}

	internal static List<TextSegment> GetTextSegments(ITextRange thisRange)
	{
		return thisRange._TextSegments;
	}

	internal static string GetText(ITextRange thisRange)
	{
		NormalizeRange(thisRange);
		if (!thisRange.IsTableCellRange)
		{
			ITextPointer textPointer = thisRange.Start;
			while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && !typeof(AnchoredBlock).IsAssignableFrom(textPointer.ParentType))
			{
				textPointer = textPointer.GetNextContextPosition(LogicalDirection.Backward);
			}
			return GetTextInternal(textPointer, thisRange.End);
		}
		string text = string.Empty;
		for (int i = 0; i < thisRange._TextSegments.Count; i++)
		{
			TextSegment textSegment = thisRange._TextSegments[i];
			text += GetTextInternal(textSegment.Start, textSegment.End);
		}
		return text;
	}

	internal static void SetText(ITextRange thisRange, string textData)
	{
		NormalizeRange(thisRange);
		if (textData == null)
		{
			throw new ArgumentNullException("textData");
		}
		ITextPointer textPointer = null;
		BeginChange(thisRange);
		try
		{
			if (!thisRange.IsEmpty)
			{
				if (thisRange.Start is TextPointer && ((TextPointer)thisRange.Start).Parent == ((TextPointer)thisRange.End).Parent && ((TextPointer)thisRange.Start).Parent is Run && textData.Length > 0)
				{
					if (thisRange.Start.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text && thisRange.End.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
					{
						textPointer = thisRange.Start;
					}
					((TextPointer)thisRange.Start).TextContainer.DeleteContentInternal((TextPointer)thisRange.Start, (TextPointer)thisRange.End);
				}
				else
				{
					thisRange.Start.DeleteContentToPosition(thisRange.End);
				}
				if (thisRange.Start is TextPointer)
				{
					TextRangeEdit.MergeFlowDirection((TextPointer)thisRange.Start);
				}
				thisRange.Select(thisRange.Start, thisRange.Start);
			}
			if (textData.Length <= 0)
			{
				return;
			}
			ITextPointer textPointer2 = ((textPointer == null) ? thisRange.Start : textPointer);
			bool flag = textData.EndsWith("\n", StringComparison.Ordinal);
			bool flag2 = textPointer2 is TextPointer && TextSchema.IsValidChild(textPointer2, typeof(Block)) && (textPointer2.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None || textPointer2.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart) && (textPointer2.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.None || textPointer2.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd);
			if (textPointer2 is TextPointer && textPointer == null)
			{
				TextPointer textPointer3 = TextRangeEditTables.EnsureInsertionPosition((TextPointer)textPointer2);
				thisRange.Select(textPointer3, textPointer3);
				textPointer2 = thisRange.Start;
			}
			Invariant.Assert(TextSchema.IsInTextContent(textPointer2), "range.Start is expected to be in text content");
			ITextPointer frozenPointer = textPointer2.GetFrozenPointer(LogicalDirection.Backward);
			ITextPointer textPointer4 = textPointer2.CreatePointer(LogicalDirection.Forward);
			if (frozenPointer is TextPointer && ((TextPointer)frozenPointer).Paragraph != null)
			{
				TextPointer textPointer5 = (TextPointer)frozenPointer.CreatePointer(LogicalDirection.Forward);
				string[] array = textData.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
				int num = array.Length;
				if (flag2 && flag)
				{
					num--;
				}
				for (int i = 0; i < num; i++)
				{
					textPointer5.InsertTextInRun(array[i]);
					if (i < num - 1)
					{
						if (textPointer5.HasNonMergeableInlineAncestor)
						{
							textPointer5.InsertTextInRun(" ");
						}
						else
						{
							textPointer5 = textPointer5.InsertParagraphBreak();
						}
						textPointer4 = textPointer5;
					}
				}
				if (flag2 && flag)
				{
					textPointer4 = textPointer4.GetNextInsertionPosition(LogicalDirection.Forward);
					if (textPointer4 == null)
					{
						textPointer4 = frozenPointer.TextContainer.End;
					}
				}
			}
			else
			{
				frozenPointer.InsertTextInRun(textData);
			}
			SelectPrivate(thisRange, frozenPointer, textPointer4, includeCellAtMovingPosition: false, markRangeChanged: true);
		}
		finally
		{
			EndChange(thisRange);
		}
	}

	internal static string GetXml(ITextRange thisRange)
	{
		NormalizeRange(thisRange);
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		TextRangeSerialization.WriteXaml(new XmlTextWriter(stringWriter), thisRange, useFlowDocumentAsRoot: false, null);
		return stringWriter.ToString();
	}

	internal static bool CanSave(ITextRange thisRange, string dataFormat)
	{
		NormalizeRange(thisRange);
		if (!(dataFormat == DataFormats.Text) && !(dataFormat == DataFormats.Xaml))
		{
			if (!(dataFormat == DataFormats.XamlPackage))
			{
				return dataFormat == DataFormats.Rtf;
			}
			return true;
		}
		return true;
	}

	internal static bool CanLoad(ITextRange thisRange, string dataFormat)
	{
		NormalizeRange(thisRange);
		if (!(dataFormat == DataFormats.Text) && !(dataFormat == DataFormats.Xaml))
		{
			if (!(dataFormat == DataFormats.XamlPackage))
			{
				return dataFormat == DataFormats.Rtf;
			}
			return true;
		}
		return true;
	}

	internal static void Save(ITextRange thisRange, Stream stream, string dataFormat, bool preserveTextElements)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (dataFormat == null)
		{
			throw new ArgumentNullException("dataFormat");
		}
		NormalizeRange(thisRange);
		if (dataFormat == DataFormats.Text)
		{
			string text = thisRange.Text;
			StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(text);
			streamWriter.Flush();
			return;
		}
		if (dataFormat == DataFormats.Xaml)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(new StreamWriter(stream));
			TextRangeSerialization.WriteXaml(xmlTextWriter, thisRange, useFlowDocumentAsRoot: false, null, preserveTextElements);
			xmlTextWriter.Flush();
			return;
		}
		if (dataFormat == DataFormats.XamlPackage)
		{
			WpfPayload.SaveRange(thisRange, ref stream, useFlowDocumentAsRoot: false, preserveTextElements);
			return;
		}
		if (dataFormat == DataFormats.Rtf)
		{
			Stream stream2 = null;
			string value = TextEditorCopyPaste.ConvertXamlToRtf(WpfPayload.SaveRange(thisRange, ref stream2, useFlowDocumentAsRoot: false), stream2);
			StreamWriter streamWriter2 = new StreamWriter(stream);
			streamWriter2.Write(value);
			streamWriter2.Flush();
			return;
		}
		throw new ArgumentException(SR.Format(SR.TextRange_UnsupportedDataFormat, dataFormat), "dataFormat");
	}

	internal static void Load(TextRange thisRange, Stream stream, string dataFormat)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (dataFormat == null)
		{
			throw new ArgumentNullException("dataFormat");
		}
		NormalizeRange(thisRange);
		if (stream.CanSeek)
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}
		if (dataFormat == DataFormats.Text)
		{
			string text = new StreamReader(stream).ReadToEnd();
			thisRange.Text = text;
			return;
		}
		if (dataFormat == DataFormats.Xaml)
		{
			string xml = new StreamReader(stream).ReadToEnd();
			thisRange.Xml = xml;
			return;
		}
		if (dataFormat == DataFormats.XamlPackage)
		{
			object obj = WpfPayload.LoadElement(stream);
			if (!(obj is Section) && !(obj is Span))
			{
				throw new ArgumentException(SR.Format(SR.TextRange_UnrecognizedStructureInDataFormat, dataFormat), "stream");
			}
			thisRange.SetXmlVirtual((TextElement)obj);
			return;
		}
		if (dataFormat == DataFormats.Rtf)
		{
			TextElement textElement = WpfPayload.LoadElement(TextEditorCopyPaste.ConvertRtfToXaml(new StreamReader(stream).ReadToEnd()) ?? throw new ArgumentException(SR.Format(SR.TextRange_UnrecognizedStructureInDataFormat, dataFormat), "stream")) as TextElement;
			if (!(textElement is Section) && !(textElement is Span))
			{
				throw new ArgumentException(SR.Format(SR.TextRange_UnrecognizedStructureInDataFormat, dataFormat), "stream");
			}
			thisRange.SetXmlVirtual(textElement);
			return;
		}
		throw new ArgumentException(SR.Format(SR.TextRange_UnsupportedDataFormat, dataFormat), "dataFormat");
	}

	internal static int GetChangeBlockLevel(ITextRange thisRange)
	{
		return thisRange._ChangeBlockLevel;
	}

	internal static UIElement GetUIElementSelected(ITextRange range)
	{
		ITextPointer textPointer = range.Start.CreatePointer();
		TextPointerContext pointerContext = textPointer.GetPointerContext(LogicalDirection.Forward);
		while (true)
		{
			switch (pointerContext)
			{
			case TextPointerContext.ElementStart:
			case TextPointerContext.ElementEnd:
				goto IL_0016;
			case TextPointerContext.EmbeddedElement:
			{
				ITextPointer textPointer2 = range.End.CreatePointer();
				pointerContext = textPointer2.GetPointerContext(LogicalDirection.Backward);
				while (true)
				{
					switch (pointerContext)
					{
					case TextPointerContext.ElementStart:
					case TextPointerContext.ElementEnd:
						goto IL_0048;
					case TextPointerContext.EmbeddedElement:
						if (textPointer.GetOffsetToPosition(textPointer2) == 1)
						{
							return textPointer.GetAdjacentElement(LogicalDirection.Forward) as UIElement;
						}
						break;
					}
					break;
					IL_0048:
					textPointer2.MoveToNextContextPosition(LogicalDirection.Backward);
					pointerContext = textPointer2.GetPointerContext(LogicalDirection.Backward);
				}
				break;
			}
			}
			break;
			IL_0016:
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			pointerContext = textPointer.GetPointerContext(LogicalDirection.Forward);
		}
		return null;
	}

	internal static bool GetIsTableCellRange(ITextRange thisRange)
	{
		NormalizeRange(thisRange);
		return thisRange._IsTableCellRange;
	}

	private static void BeginChangeWorker(ITextRange thisRange, string description)
	{
		ITextContainer textContainer = thisRange.Start.TextContainer;
		if (description != null && thisRange._ChangeBlockUndoRecord == null && thisRange._ChangeBlockLevel == 0)
		{
			thisRange._ChangeBlockUndoRecord = new ChangeBlockUndoRecord(textContainer, description);
		}
		Invariant.Assert(thisRange._ChangeBlockLevel > 0 || !thisRange._IsChanged, "_changed must be false on new move sequence");
		thisRange._ChangeBlockLevel++;
		if (description != null)
		{
			textContainer.BeginChange();
		}
		else
		{
			textContainer.BeginChangeNoUndo();
		}
	}

	private static void CreateNormalizedTextSegment(ITextRange thisRange, ITextPointer start, ITextPointer end)
	{
		ValidationHelper.VerifyPositionPair(start, end);
		if (start.CompareTo(end) == 0)
		{
			if (!IsAtNormalizedPosition(thisRange, start, start.LogicalDirection))
			{
				start = GetNormalizedPosition(thisRange, start, start.LogicalDirection);
				end = start;
			}
		}
		else
		{
			start = GetNormalizedPosition(thisRange, start, LogicalDirection.Forward);
			if (!TextPointerBase.IsAfterLastParagraph(end))
			{
				end = GetNormalizedPosition(thisRange, end, LogicalDirection.Backward);
			}
			if (start.CompareTo(end) >= 0)
			{
				if (start.LogicalDirection == LogicalDirection.Backward)
				{
					start = end.GetFrozenPointer(LogicalDirection.Backward);
				}
				end = start;
			}
			else
			{
				if (start is TextPointer)
				{
					TextPointer start2 = (TextPointer)start;
					TextPointer end2 = (TextPointer)end;
					NormalizeAnchoredBlockBoundaries(ref start2, ref end2);
					start = start2;
					end = end2;
				}
				Invariant.Assert(start.CompareTo(end) <= 0, "expecting start <= end");
				if (start.CompareTo(end) == 0 && !IsAtNormalizedPosition(thisRange, start, start.LogicalDirection))
				{
					start = GetNormalizedPosition(thisRange, start, start.LogicalDirection);
					end = start;
				}
			}
		}
		thisRange._TextSegments = new List<TextSegment>(1);
		thisRange._TextSegments.Add(new TextSegment(start, end));
		thisRange._IsTableCellRange = false;
	}

	private static bool IsAtNormalizedPosition(ITextRange thisRange, ITextPointer position, LogicalDirection direction)
	{
		if (thisRange.IgnoreTextUnitBoundaries)
		{
			return TextPointerBase.IsAtFormatNormalizedPosition(position, direction);
		}
		return TextPointerBase.IsAtInsertionPosition(position, direction);
	}

	private static ITextPointer GetNormalizedPosition(ITextRange thisRange, ITextPointer position, LogicalDirection direction)
	{
		if (thisRange.IgnoreTextUnitBoundaries)
		{
			return position.GetFormatNormalizedPosition(direction);
		}
		return position.GetInsertionPosition(direction);
	}

	internal static void NormalizeAnchoredBlockBoundaries(ref TextPointer start, ref TextPointer end)
	{
		TextElement textElement = start.Parent as TextElement;
		while (textElement != null)
		{
			while (textElement != null && !typeof(AnchoredBlock).IsAssignableFrom(textElement.GetType()))
			{
				textElement = textElement.Parent as TextElement;
			}
			if (textElement == null)
			{
				continue;
			}
			AnchoredBlock anchoredBlock = null;
			TextElement textElement2 = end.Parent as TextElement;
			while (textElement2 != null && textElement2 != textElement)
			{
				if (textElement2 is AnchoredBlock)
				{
					anchoredBlock = (AnchoredBlock)textElement2;
				}
				textElement2 = textElement2.Parent as TextElement;
			}
			if (textElement2 == textElement)
			{
				if (anchoredBlock != null)
				{
					end = anchoredBlock.ElementEnd;
				}
				return;
			}
			start = textElement.ElementStart;
			textElement = textElement.Parent as TextElement;
		}
		textElement = end.Parent as TextElement;
		while (textElement != null)
		{
			while (textElement != null && !typeof(AnchoredBlock).IsAssignableFrom(textElement.GetType()))
			{
				textElement = textElement.Parent as TextElement;
			}
			if (textElement == null)
			{
				continue;
			}
			AnchoredBlock anchoredBlock2 = null;
			TextElement textElement3 = start.Parent as TextElement;
			while (textElement3 != null && textElement3 != textElement)
			{
				if (textElement3 is AnchoredBlock)
				{
					anchoredBlock2 = (AnchoredBlock)textElement3;
				}
				textElement3 = textElement3.Parent as TextElement;
			}
			if (textElement3 == textElement)
			{
				if (anchoredBlock2 != null)
				{
					start = anchoredBlock2.ElementStart;
				}
				break;
			}
			end = textElement.ElementEnd;
			textElement = textElement.Parent as TextElement;
		}
	}

	private static void NormalizeRange(ITextRange thisRange)
	{
		if (thisRange._ContentGeneration == thisRange._TextSegments[0].Start.TextContainer.Generation)
		{
			return;
		}
		ITextPointer start = thisRange._TextSegments[0].Start;
		ITextPointer end = thisRange._TextSegments[thisRange._TextSegments.Count - 1].End;
		if (thisRange._IsTableCellRange)
		{
			Invariant.Assert(thisRange._TextSegments[0].Start is TextPointer);
			TextRangeEditTables.IdentifyValidBoundaries(thisRange, out start, out end);
			SelectPrivate(thisRange, start, end, includeCellAtMovingPosition: false, markRangeChanged: false);
		}
		else
		{
			bool flag = false;
			if (start == end)
			{
				if (!TextPointerBase.IsAtInsertionPosition(start, start.LogicalDirection))
				{
					flag = true;
				}
			}
			else if (start.CompareTo(end) == 0)
			{
				flag = true;
			}
			else if (!TextPointerBase.IsAtInsertionPosition(start, LogicalDirection.Forward) || !TextPointerBase.IsAtInsertionPosition(end, LogicalDirection.Backward))
			{
				flag = true;
			}
			if (flag)
			{
				CreateNormalizedTextSegment(thisRange, start, end);
			}
		}
		thisRange._ContentGeneration = thisRange._TextSegments[0].Start.TextContainer.Generation;
	}

	private static void SelectPrivate(ITextRange thisRange, ITextPointer position1, ITextPointer position2, bool includeCellAtMovingPosition, bool markRangeChanged)
	{
		Invariant.Assert(position1 != null, "null check: position1");
		Invariant.Assert(position2 != null, "null check: position2");
		bool isTableCellRange;
		List<TextSegment> list;
		if (position1 is TextPointer)
		{
			list = TextRangeEditTables.BuildTableRange((TextPointer)position1, (TextPointer)position2, includeCellAtMovingPosition, out isTableCellRange);
		}
		else
		{
			Invariant.Assert(!thisRange._IsTableCellRange, "range is not expected to be in IsTableCellRange state - 1");
			list = null;
			isTableCellRange = false;
		}
		if (list != null)
		{
			thisRange._TextSegments = list;
			thisRange._IsTableCellRange = isTableCellRange;
		}
		else
		{
			ITextPointer textPointer = position1;
			ITextPointer textPointer2 = position2;
			if (position1.CompareTo(position2) > 0)
			{
				textPointer = position2;
				textPointer2 = position1;
			}
			CreateNormalizedTextSegment(thisRange, textPointer, textPointer2);
			Invariant.Assert(!thisRange._IsTableCellRange, "Expecting that the range is in text segment state now - must be set by CreateNOrmalizedTextSegment");
			if (position1 is TextPointer)
			{
				ITextPointer start = thisRange._TextSegments[0].Start;
				ITextPointer end = thisRange._TextSegments[thisRange._TextSegments.Count - 1].End;
				if (start.CompareTo(textPointer) != 0 || end.CompareTo(textPointer2) != 0)
				{
					list = TextRangeEditTables.BuildTableRange((TextPointer)start, (TextPointer)end, includeCellAtMovingPosition: false, out isTableCellRange);
					if (list != null)
					{
						thisRange._TextSegments = list;
						thisRange._IsTableCellRange = isTableCellRange;
					}
				}
			}
		}
		thisRange._ContentGeneration = thisRange._TextSegments[0].Start.TextContainer.Generation;
		if (markRangeChanged)
		{
			MarkRangeChanged(thisRange);
		}
	}

	private static void MarkRangeChanged(ITextRange thisRange)
	{
		Invariant.Assert(thisRange._ChangeBlockLevel > 0, "changeBlockLevel > 0 is expected");
		thisRange._IsChanged = true;
	}
}
