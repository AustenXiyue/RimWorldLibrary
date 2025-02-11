using System.Globalization;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal static class TextPointerBase
{
	private enum BorderingElementCategory
	{
		MergeableScopingInline,
		NonMergeableScopingInline,
		NotScopingInline
	}

	internal static char[] NextLineCharacters = new char[7] { '\n', '\r', '\v', '\f', '\u0085', '\u2028', '\u2029' };

	internal static ITextPointer Min(ITextPointer position1, ITextPointer position2)
	{
		if (position1.CompareTo(position2) > 0)
		{
			return position2;
		}
		return position1;
	}

	internal static ITextPointer Max(ITextPointer position1, ITextPointer position2)
	{
		if (position1.CompareTo(position2) < 0)
		{
			return position2;
		}
		return position1;
	}

	internal static string GetTextInRun(ITextPointer position, LogicalDirection direction)
	{
		int textRunLength = position.GetTextRunLength(direction);
		char[] array = new char[textRunLength];
		Invariant.Assert(position.GetTextInRun(direction, array, 0, textRunLength) == textRunLength, "textLengths returned from GetTextRunLength and GetTextInRun are innconsistent");
		return new string(array);
	}

	internal static int GetTextWithLimit(ITextPointer thisPointer, LogicalDirection direction, char[] textBuffer, int startIndex, int count, ITextPointer limit)
	{
		if (limit == null)
		{
			return thisPointer.GetTextInRun(direction, textBuffer, startIndex, count);
		}
		if (direction == LogicalDirection.Forward && limit.CompareTo(thisPointer) <= 0)
		{
			return 0;
		}
		if (direction == LogicalDirection.Backward && limit.CompareTo(thisPointer) >= 0)
		{
			return 0;
		}
		int val = ((direction != LogicalDirection.Forward) ? Math.Min(count, limit.GetOffsetToPosition(thisPointer)) : Math.Min(count, thisPointer.GetOffsetToPosition(limit)));
		val = Math.Min(count, val);
		return thisPointer.GetTextInRun(direction, textBuffer, startIndex, val);
	}

	internal static bool IsAtInsertionPosition(ITextPointer position)
	{
		return IsAtNormalizedPosition(position, respectCaretUnitBoundaries: true);
	}

	internal static bool IsAtPotentialRunPosition(ITextPointer position)
	{
		bool flag = IsAtPotentialRunPosition(position, position);
		if (!flag)
		{
			flag = IsAtPotentialParagraphPosition(position);
		}
		return flag;
	}

	internal static bool IsAtPotentialRunPosition(TextElement run)
	{
		if (run is Run && run.IsEmpty)
		{
			return IsAtPotentialRunPosition(run.ElementStart, run.ElementEnd);
		}
		return false;
	}

	private static bool IsAtPotentialRunPosition(ITextPointer backwardPosition, ITextPointer forwardPosition)
	{
		Invariant.Assert(backwardPosition.HasEqualScope(forwardPosition));
		if (TextSchema.IsValidChild(backwardPosition, typeof(Run)))
		{
			Type elementType = forwardPosition.GetElementType(LogicalDirection.Forward);
			Type elementType2 = backwardPosition.GetElementType(LogicalDirection.Backward);
			if (elementType != null && elementType2 != null)
			{
				TextPointerContext pointerContext = forwardPosition.GetPointerContext(LogicalDirection.Forward);
				TextPointerContext pointerContext2 = backwardPosition.GetPointerContext(LogicalDirection.Backward);
				if ((pointerContext2 == TextPointerContext.ElementStart && pointerContext == TextPointerContext.ElementEnd) || (pointerContext2 == TextPointerContext.ElementStart && TextSchema.IsNonFormattingInline(elementType) && !IsAtNonMergeableInlineStart(backwardPosition)) || (pointerContext == TextPointerContext.ElementEnd && TextSchema.IsNonFormattingInline(elementType2) && !IsAtNonMergeableInlineEnd(forwardPosition)) || (pointerContext2 == TextPointerContext.ElementEnd && pointerContext == TextPointerContext.ElementStart && TextSchema.IsNonFormattingInline(elementType2) && TextSchema.IsNonFormattingInline(elementType)) || (pointerContext2 == TextPointerContext.ElementEnd && typeof(Inline).IsAssignableFrom(elementType2) && !TextSchema.IsMergeableInline(elementType2) && !typeof(Run).IsAssignableFrom(elementType) && (pointerContext != TextPointerContext.ElementEnd || !IsAtNonMergeableInlineEnd(forwardPosition))) || (pointerContext == TextPointerContext.ElementStart && typeof(Inline).IsAssignableFrom(elementType) && !TextSchema.IsMergeableInline(elementType) && !typeof(Run).IsAssignableFrom(elementType2) && (pointerContext2 != TextPointerContext.ElementStart || !IsAtNonMergeableInlineStart(backwardPosition))))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool IsAtPotentialParagraphPosition(ITextPointer position)
	{
		Type parentType = position.ParentType;
		TextPointerContext pointerContext = position.GetPointerContext(LogicalDirection.Backward);
		TextPointerContext pointerContext2 = position.GetPointerContext(LogicalDirection.Forward);
		if (pointerContext == TextPointerContext.ElementStart && pointerContext2 == TextPointerContext.ElementEnd)
		{
			if (!typeof(ListItem).IsAssignableFrom(parentType))
			{
				return typeof(TableCell).IsAssignableFrom(parentType);
			}
			return true;
		}
		if (pointerContext == TextPointerContext.None && pointerContext2 == TextPointerContext.None)
		{
			if (!typeof(FlowDocumentView).IsAssignableFrom(parentType))
			{
				return typeof(FlowDocument).IsAssignableFrom(parentType);
			}
			return true;
		}
		return false;
	}

	internal static bool IsBeforeFirstTable(ITextPointer position)
	{
		TextPointerContext pointerContext = position.GetPointerContext(LogicalDirection.Forward);
		TextPointerContext pointerContext2 = position.GetPointerContext(LogicalDirection.Backward);
		if (pointerContext == TextPointerContext.ElementStart && (pointerContext2 == TextPointerContext.ElementStart || pointerContext2 == TextPointerContext.None))
		{
			return typeof(Table).IsAssignableFrom(position.GetElementType(LogicalDirection.Forward));
		}
		return false;
	}

	internal static bool IsInBlockUIContainer(ITextPointer position)
	{
		return typeof(BlockUIContainer).IsAssignableFrom(position.ParentType);
	}

	internal static bool IsAtBlockUIContainerStart(ITextPointer position)
	{
		if (IsInBlockUIContainer(position))
		{
			return position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart;
		}
		return false;
	}

	internal static bool IsAtBlockUIContainerEnd(ITextPointer position)
	{
		if (IsInBlockUIContainer(position))
		{
			return position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd;
		}
		return false;
	}

	private static bool IsInAncestorScope(ITextPointer position, Type allowedParentType, Type limitingType)
	{
		ITextPointer textPointer = position.CreatePointer();
		Type parentType = textPointer.ParentType;
		while (parentType != null && allowedParentType.IsAssignableFrom(parentType))
		{
			if (limitingType.IsAssignableFrom(parentType))
			{
				return true;
			}
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
			parentType = textPointer.ParentType;
		}
		return false;
	}

	internal static bool IsInAnchoredBlock(ITextPointer position)
	{
		return IsInAncestorScope(position, typeof(TextElement), typeof(AnchoredBlock));
	}

	internal static bool IsInHyperlinkScope(ITextPointer position)
	{
		return IsInAncestorScope(position, typeof(Inline), typeof(Hyperlink));
	}

	internal static ITextPointer GetFollowingNonMergeableInlineContentStart(ITextPointer position)
	{
		ITextPointer textPointer = position.CreatePointer();
		bool flag = false;
		Type elementType;
		while (true)
		{
			if (GetBorderingElementCategory(textPointer, LogicalDirection.Forward) == BorderingElementCategory.MergeableScopingInline)
			{
				do
				{
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				}
				while (GetBorderingElementCategory(textPointer, LogicalDirection.Forward) == BorderingElementCategory.MergeableScopingInline);
				flag = true;
			}
			elementType = textPointer.GetElementType(LogicalDirection.Forward);
			if (elementType == typeof(InlineUIContainer) || elementType == typeof(BlockUIContainer))
			{
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
			}
			else
			{
				if (!(textPointer.ParentType == typeof(InlineUIContainer)) && !(textPointer.ParentType == typeof(BlockUIContainer)))
				{
					break;
				}
				textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
			}
			elementType = textPointer.GetElementType(LogicalDirection.Forward);
			if (!(elementType == typeof(InlineUIContainer)) && !(elementType == typeof(BlockUIContainer)))
			{
				textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
			}
			flag = true;
		}
		if (typeof(Inline).IsAssignableFrom(elementType) && !TextSchema.IsMergeableInline(elementType))
		{
			do
			{
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			}
			while (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart);
			flag = true;
		}
		if (!flag)
		{
			return null;
		}
		return textPointer;
	}

	internal static bool IsAtNonMergeableInlineStart(ITextPointer position)
	{
		return IsAtNonMergeableInlineEdge(position, LogicalDirection.Backward);
	}

	internal static bool IsAtNonMergeableInlineEnd(ITextPointer position)
	{
		return IsAtNonMergeableInlineEdge(position, LogicalDirection.Forward);
	}

	internal static bool IsPositionAtNonMergeableInlineBoundary(ITextPointer position)
	{
		if (!IsAtNonMergeableInlineStart(position))
		{
			return IsAtNonMergeableInlineEnd(position);
		}
		return true;
	}

	internal static bool IsAtFormatNormalizedPosition(ITextPointer position, LogicalDirection direction)
	{
		return IsAtNormalizedPosition(position, direction, respectCaretUnitBoundaries: false);
	}

	internal static bool IsAtInsertionPosition(ITextPointer position, LogicalDirection direction)
	{
		return IsAtNormalizedPosition(position, direction, respectCaretUnitBoundaries: true);
	}

	internal static bool IsAtNormalizedPosition(ITextPointer position, LogicalDirection direction, bool respectCaretUnitBoundaries)
	{
		if (!IsAtNormalizedPosition(position, respectCaretUnitBoundaries))
		{
			return false;
		}
		if (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
		{
			return true;
		}
		if (TextSchema.IsFormattingType(position.GetElementType(direction)))
		{
			position = position.CreatePointer();
			while (TextSchema.IsFormattingType(position.GetElementType(direction)))
			{
				position.MoveToNextContextPosition(direction);
			}
			if (IsAtNormalizedPosition(position, respectCaretUnitBoundaries))
			{
				return false;
			}
		}
		return true;
	}

	internal static int GetOffset(ITextPointer thisPosition)
	{
		return thisPosition.TextContainer.Start.GetOffsetToPosition(thisPosition);
	}

	internal static bool IsAtWordBoundary(ITextPointer thisPosition, LogicalDirection insideWordDirection)
	{
		ITextPointer textPointer = thisPosition.CreatePointer();
		if (textPointer.GetPointerContext(insideWordDirection) != TextPointerContext.Text)
		{
			textPointer.MoveToInsertionPosition(insideWordDirection);
		}
		if (textPointer.GetPointerContext(insideWordDirection) == TextPointerContext.Text)
		{
			GetWordBreakerText(thisPosition, out var text, out var position);
			return SelectionWordBreaker.IsAtWordBoundary(text, position, insideWordDirection);
		}
		return true;
	}

	internal static TextSegment GetWordRange(ITextPointer thisPosition)
	{
		return GetWordRange(thisPosition, LogicalDirection.Forward);
	}

	internal static TextSegment GetWordRange(ITextPointer thisPosition, LogicalDirection direction)
	{
		if (!thisPosition.IsAtInsertionPosition)
		{
			thisPosition = thisPosition.GetInsertionPosition(direction);
		}
		if (!thisPosition.IsAtInsertionPosition)
		{
			return new TextSegment(thisPosition, thisPosition);
		}
		ITextPointer textPointer = thisPosition.CreatePointer();
		bool flag = MoveToNextWordBoundary(textPointer, direction);
		ITextPointer textPointer2 = textPointer;
		ITextPointer textPointer3;
		if (flag && IsAtWordBoundary(thisPosition, LogicalDirection.Forward))
		{
			textPointer3 = thisPosition;
		}
		else
		{
			ITextPointer textPointer4 = thisPosition.CreatePointer();
			MoveToNextWordBoundary(textPointer4, (direction == LogicalDirection.Backward) ? LogicalDirection.Forward : LogicalDirection.Backward);
			textPointer3 = textPointer4;
		}
		if (direction == LogicalDirection.Backward)
		{
			ITextPointer textPointer5 = textPointer3;
			textPointer3 = textPointer2;
			textPointer2 = textPointer5;
		}
		textPointer3 = RestrictWithinBlock(thisPosition, textPointer3, LogicalDirection.Backward);
		textPointer2 = RestrictWithinBlock(thisPosition, textPointer2, LogicalDirection.Forward);
		if (textPointer3.CompareTo(textPointer2) < 0)
		{
			textPointer3 = textPointer3.GetFrozenPointer(LogicalDirection.Backward);
			textPointer2 = textPointer2.GetFrozenPointer(LogicalDirection.Forward);
		}
		else
		{
			textPointer3 = textPointer2.GetFrozenPointer(LogicalDirection.Backward);
			textPointer2 = textPointer3;
		}
		Invariant.Assert(textPointer3.CompareTo(textPointer2) <= 0, "expecting wordStart <= wordEnd");
		return new TextSegment(textPointer3, textPointer2);
	}

	private static ITextPointer RestrictWithinBlock(ITextPointer position, ITextPointer limit, LogicalDirection direction)
	{
		Invariant.Assert(direction != 0 || position.CompareTo(limit) >= 0, "for backward direction position must be >= than limit");
		Invariant.Assert(direction != LogicalDirection.Forward || position.CompareTo(limit) <= 0, "for forward direcion position must be <= than linit");
		for (; (direction == LogicalDirection.Backward) ? (position.CompareTo(limit) > 0) : (position.CompareTo(limit) < 0); position = position.GetNextContextPosition(direction))
		{
			switch (position.GetPointerContext(direction))
			{
			case TextPointerContext.ElementStart:
			case TextPointerContext.ElementEnd:
			{
				Type elementType = position.GetElementType(direction);
				if (typeof(Inline).IsAssignableFrom(elementType))
				{
					continue;
				}
				limit = position;
				break;
			}
			case TextPointerContext.EmbeddedElement:
				limit = position;
				break;
			default:
				continue;
			}
			break;
		}
		return limit.GetInsertionPosition((direction == LogicalDirection.Backward) ? LogicalDirection.Forward : LogicalDirection.Backward);
	}

	internal static bool IsNextToPlainLineBreak(ITextPointer thisPosition, LogicalDirection direction)
	{
		char[] array = new char[2];
		int textInRun = thisPosition.GetTextInRun(direction, array, 0, 2);
		if (textInRun != 1 || !IsCharUnicodeNewLine(array[0]))
		{
			if (textInRun == 2)
			{
				if (direction != 0 || !IsCharUnicodeNewLine(array[1]))
				{
					if (direction == LogicalDirection.Forward)
					{
						return IsCharUnicodeNewLine(array[0]);
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	internal static bool IsCharUnicodeNewLine(char ch)
	{
		return Array.IndexOf(NextLineCharacters, ch) > -1;
	}

	internal static bool IsNextToRichLineBreak(ITextPointer thisPosition, LogicalDirection direction)
	{
		return IsNextToRichBreak(thisPosition, direction, typeof(LineBreak));
	}

	internal static bool IsNextToParagraphBreak(ITextPointer thisPosition, LogicalDirection direction)
	{
		return IsNextToRichBreak(thisPosition, direction, typeof(Paragraph));
	}

	internal static bool IsNextToAnyBreak(ITextPointer thisPosition, LogicalDirection direction)
	{
		if (!thisPosition.IsAtInsertionPosition)
		{
			thisPosition = thisPosition.GetInsertionPosition(direction);
		}
		if (!IsNextToPlainLineBreak(thisPosition, direction))
		{
			return IsNextToRichBreak(thisPosition, direction, null);
		}
		return true;
	}

	internal static bool IsAtLineWrappingPosition(ITextPointer position, ITextView textView)
	{
		Invariant.Assert(position != null, "null check: position");
		if (!position.HasValidLayout)
		{
			return false;
		}
		Invariant.Assert(textView != null, "textView cannot be null because the position has valid layout");
		TextSegment lineRange = textView.GetLineRange(position);
		if (lineRange.IsNull)
		{
			return false;
		}
		if (position.LogicalDirection != LogicalDirection.Forward)
		{
			return position.CompareTo(lineRange.End) == 0;
		}
		return position.CompareTo(lineRange.Start) == 0;
	}

	internal static bool IsAtRowEnd(ITextPointer thisPosition)
	{
		if (typeof(TableRow).IsAssignableFrom(thisPosition.ParentType) && thisPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
		{
			return thisPosition.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementStart;
		}
		return false;
	}

	internal static bool IsAfterLastParagraph(ITextPointer thisPosition)
	{
		if (thisPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.None && thisPosition.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementEnd)
		{
			return !typeof(Inline).IsAssignableFrom(thisPosition.GetElementType(LogicalDirection.Backward));
		}
		return false;
	}

	internal static bool IsAtParagraphOrBlockUIContainerStart(ITextPointer pointer)
	{
		if (IsAtPotentialParagraphPosition(pointer))
		{
			return true;
		}
		while (pointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			if (TextSchema.IsParagraphOrBlockUIContainer(pointer.ParentType))
			{
				return true;
			}
			pointer = pointer.GetNextContextPosition(LogicalDirection.Backward);
		}
		return false;
	}

	internal static ListItem GetListItem(TextPointer pointer)
	{
		if (pointer.Parent is ListItem)
		{
			return (ListItem)pointer.Parent;
		}
		Block paragraphOrBlockUIContainer = pointer.ParagraphOrBlockUIContainer;
		if (paragraphOrBlockUIContainer != null)
		{
			return paragraphOrBlockUIContainer.Parent as ListItem;
		}
		return null;
	}

	internal static ListItem GetImmediateListItem(TextPointer position)
	{
		if (position.Parent is ListItem)
		{
			return (ListItem)position.Parent;
		}
		Block paragraphOrBlockUIContainer = position.ParagraphOrBlockUIContainer;
		if (paragraphOrBlockUIContainer != null && paragraphOrBlockUIContainer.Parent is ListItem && paragraphOrBlockUIContainer.ElementStart.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			return (ListItem)paragraphOrBlockUIContainer.Parent;
		}
		return null;
	}

	internal static bool IsInEmptyListItem(TextPointer position)
	{
		if (position.Parent is ListItem listItem)
		{
			return listItem.IsEmpty;
		}
		return false;
	}

	internal static int MoveToLineBoundary(ITextPointer thisPointer, ITextView textView, int count)
	{
		return MoveToLineBoundary(thisPointer, textView, count, respectNonMeargeableInlineStart: false);
	}

	internal static int MoveToLineBoundary(ITextPointer thisPointer, ITextView textView, int count, bool respectNonMeargeableInlineStart)
	{
		Invariant.Assert(!thisPointer.IsFrozen, "Can't reposition a frozen pointer!");
		Invariant.Assert(textView != null, "Null TextView!");
		double newSuggestedX;
		ITextPointer positionAtNextLine = textView.GetPositionAtNextLine(thisPointer, double.NaN, count, out newSuggestedX, out count);
		if (!positionAtNextLine.IsAtInsertionPosition && (!respectNonMeargeableInlineStart || (!IsAtNonMergeableInlineStart(positionAtNextLine) && !IsAtNonMergeableInlineEnd(positionAtNextLine))))
		{
			positionAtNextLine.MoveToInsertionPosition(positionAtNextLine.LogicalDirection);
		}
		if (IsAtRowEnd(positionAtNextLine))
		{
			thisPointer.MoveToPosition(positionAtNextLine);
			thisPointer.SetLogicalDirection(positionAtNextLine.LogicalDirection);
		}
		else
		{
			TextSegment lineRange = textView.GetLineRange(positionAtNextLine);
			if (!lineRange.IsNull)
			{
				thisPointer.MoveToPosition(lineRange.Start);
				thisPointer.SetLogicalDirection(lineRange.Start.LogicalDirection);
			}
			else if (count > 0)
			{
				thisPointer.MoveToPosition(positionAtNextLine);
				thisPointer.SetLogicalDirection(positionAtNextLine.LogicalDirection);
			}
		}
		return count;
	}

	internal static Rect GetCharacterRect(ITextPointer thisPointer, LogicalDirection direction)
	{
		return GetCharacterRect(thisPointer, direction, transformToUiScope: true);
	}

	internal static Rect GetCharacterRect(ITextPointer thisPointer, LogicalDirection direction, bool transformToUiScope)
	{
		ITextView textView = thisPointer.TextContainer.TextView;
		Invariant.Assert(textView != null, "Null TextView!");
		Invariant.Assert(textView.RenderScope != null, "Null RenderScope");
		Invariant.Assert(thisPointer.TextContainer != null, "Null TextContainer");
		Invariant.Assert(thisPointer.TextContainer.Parent != null, "Null parent of TextContainer");
		if (!thisPointer.IsAtInsertionPosition)
		{
			ITextPointer insertionPosition = thisPointer.GetInsertionPosition(direction);
			if (insertionPosition != null)
			{
				thisPointer = insertionPosition;
			}
		}
		Rect rect = textView.GetRectangleFromTextPosition(thisPointer.CreatePointer(direction));
		if (transformToUiScope)
		{
			Visual visual;
			if (thisPointer.TextContainer.Parent is FlowDocument && textView.RenderScope is FlowDocumentView)
			{
				visual = ((FlowDocumentView)textView.RenderScope).TemplatedParent as Visual;
				if (visual == null && ((FlowDocumentView)textView.RenderScope).Parent is FrameworkElement)
				{
					visual = ((FrameworkElement)((FlowDocumentView)textView.RenderScope).Parent).TemplatedParent as Visual;
				}
			}
			else if (thisPointer.TextContainer.Parent is Visual)
			{
				Invariant.Assert(textView.RenderScope == thisPointer.TextContainer.Parent || ((Visual)thisPointer.TextContainer.Parent).IsAncestorOf(textView.RenderScope), "Unexpected location of RenderScope within visual tree");
				visual = (Visual)thisPointer.TextContainer.Parent;
			}
			else
			{
				visual = null;
			}
			if (visual != null && visual.IsAncestorOf(textView.RenderScope))
			{
				rect = textView.RenderScope.TransformToAncestor(visual).TransformBounds(rect);
			}
		}
		return rect;
	}

	internal static bool MoveToFormatNormalizedPosition(ITextPointer thisNavigator, LogicalDirection direction)
	{
		return NormalizePosition(thisNavigator, direction, respectCaretUnitBoundaries: false);
	}

	internal static bool MoveToInsertionPosition(ITextPointer thisNavigator, LogicalDirection direction)
	{
		return NormalizePosition(thisNavigator, direction, respectCaretUnitBoundaries: true);
	}

	internal static bool MoveToNextInsertionPosition(ITextPointer thisNavigator, LogicalDirection direction)
	{
		Invariant.Assert(!thisNavigator.IsFrozen, "Can't reposition a frozen pointer!");
		bool flag = true;
		int num = ((direction == LogicalDirection.Forward) ? 1 : (-1));
		ITextPointer textPointer = thisNavigator.CreatePointer();
		if (IsAtInsertionPosition(thisNavigator))
		{
			goto IL_0068;
		}
		if (!MoveToInsertionPosition(thisNavigator, direction))
		{
			flag = false;
		}
		else if ((direction != LogicalDirection.Forward || textPointer.CompareTo(thisNavigator) >= 0) && (direction != 0 || thisNavigator.CompareTo(textPointer) >= 0))
		{
			goto IL_0068;
		}
		goto IL_00e9;
		IL_00e9:
		if (flag)
		{
			if (direction == LogicalDirection.Forward)
			{
				Invariant.Assert(thisNavigator.CompareTo(textPointer) > 0, "thisNavigator is expected to be moved from initialPosition - 1");
			}
			else
			{
				Invariant.Assert(thisNavigator.CompareTo(textPointer) < 0, "thisNavigator is expected to be moved from initialPosition - 2");
			}
		}
		else
		{
			Invariant.Assert(thisNavigator.CompareTo(textPointer) == 0, "thisNavigator must stay at initial position");
		}
		return flag;
		IL_0068:
		while (TextSchema.IsFormattingType(thisNavigator.GetElementType(direction)))
		{
			thisNavigator.MoveByOffset(num);
		}
		while (true)
		{
			if (thisNavigator.GetPointerContext(direction) != 0)
			{
				thisNavigator.MoveByOffset(num);
				if (!IsAtInsertionPosition(thisNavigator))
				{
					continue;
				}
				if (direction != 0)
				{
					break;
				}
				while (TextSchema.IsFormattingType(thisNavigator.GetElementType(direction)))
				{
					thisNavigator.MoveByOffset(num);
				}
				TextPointerContext pointerContext = thisNavigator.GetPointerContext(direction);
				if (pointerContext == TextPointerContext.ElementStart || pointerContext == TextPointerContext.None)
				{
					num = -num;
					while (TextSchema.IsFormattingType(thisNavigator.GetElementType(LogicalDirection.Forward)) && !IsAtInsertionPosition(thisNavigator))
					{
						thisNavigator.MoveByOffset(num);
					}
				}
				break;
			}
			thisNavigator.MoveToPosition(textPointer);
			flag = false;
			break;
		}
		goto IL_00e9;
	}

	internal static bool MoveToNextWordBoundary(ITextPointer thisNavigator, LogicalDirection movingDirection)
	{
		int num = 0;
		Invariant.Assert(!thisNavigator.IsFrozen, "Can't reposition a frozen pointer!");
		ITextPointer position = thisNavigator.CreatePointer();
		while (thisNavigator.MoveToNextInsertionPosition(movingDirection))
		{
			num++;
			if (num > 64)
			{
				thisNavigator.MoveToPosition(position);
				thisNavigator.MoveToNextContextPosition(movingDirection);
				break;
			}
			if (IsAtWordBoundary(thisNavigator, LogicalDirection.Forward))
			{
				break;
			}
		}
		return num > 0;
	}

	internal static ITextPointer GetFrozenPointer(ITextPointer thisPointer, LogicalDirection logicalDirection)
	{
		ITextPointer textPointer;
		if (thisPointer.IsFrozen && thisPointer.LogicalDirection == logicalDirection)
		{
			textPointer = thisPointer;
		}
		else
		{
			textPointer = thisPointer.CreatePointer(logicalDirection);
			textPointer.Freeze();
		}
		return textPointer;
	}

	internal static bool ValidateLayout(ITextPointer thisPointer, ITextView textView)
	{
		return textView?.Validate(thisPointer) ?? false;
	}

	private static bool NormalizePosition(ITextPointer thisNavigator, LogicalDirection direction, bool respectCaretUnitBoundaries)
	{
		Invariant.Assert(!thisNavigator.IsFrozen, "Can't reposition a frozen pointer!");
		int num = 0;
		int num2;
		LogicalDirection direction2;
		TextPointerContext textPointerContext;
		TextPointerContext textPointerContext2;
		if (direction == LogicalDirection.Forward)
		{
			num2 = 1;
			direction2 = LogicalDirection.Backward;
			textPointerContext = TextPointerContext.ElementStart;
			textPointerContext2 = TextPointerContext.ElementEnd;
		}
		else
		{
			num2 = -1;
			direction2 = LogicalDirection.Forward;
			textPointerContext = TextPointerContext.ElementEnd;
			textPointerContext2 = TextPointerContext.ElementStart;
		}
		if (!IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries))
		{
			while (thisNavigator.GetPointerContext(direction) == textPointerContext && !typeof(Inline).IsAssignableFrom(thisNavigator.GetElementType(direction)) && !IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries))
			{
				thisNavigator.MoveToNextContextPosition(direction);
				num += num2;
			}
			while (thisNavigator.GetPointerContext(direction2) == textPointerContext2 && !typeof(Inline).IsAssignableFrom(thisNavigator.GetElementType(direction2)) && !IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries))
			{
				thisNavigator.MoveToNextContextPosition(direction2);
				num -= num2;
			}
		}
		num = LeaveNonMergeableInlineBoundary(thisNavigator, direction, num);
		if (respectCaretUnitBoundaries)
		{
			while (!IsAtCaretUnitBoundary(thisNavigator))
			{
				num += num2;
				thisNavigator.MoveByOffset(num2);
			}
		}
		while (TextSchema.IsMergeableInline(thisNavigator.GetElementType(direction)))
		{
			thisNavigator.MoveToNextContextPosition(direction);
			num += num2;
		}
		if (!IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries))
		{
			while (!IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries) && TextSchema.IsMergeableInline(thisNavigator.GetElementType(direction2)))
			{
				thisNavigator.MoveToNextContextPosition(direction2);
				num -= num2;
			}
			while (!IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries) && thisNavigator.MoveToNextContextPosition(direction))
			{
				num += num2;
			}
			while (!IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries) && thisNavigator.MoveToNextContextPosition(direction2))
			{
				num -= num2;
			}
			if (!IsAtNormalizedPosition(thisNavigator, respectCaretUnitBoundaries))
			{
				thisNavigator.MoveByOffset(-num);
			}
		}
		return num != 0;
	}

	private static int LeaveNonMergeableInlineBoundary(ITextPointer thisNavigator, LogicalDirection direction, int symbolCount)
	{
		if (IsAtNonMergeableInlineStart(thisNavigator))
		{
			symbolCount = ((direction != LogicalDirection.Forward || !IsAtNonMergeableInlineEnd(thisNavigator)) ? (symbolCount + LeaveNonMergeableAncestor(thisNavigator, LogicalDirection.Backward)) : (symbolCount + LeaveNonMergeableAncestor(thisNavigator, LogicalDirection.Forward)));
		}
		else if (IsAtNonMergeableInlineEnd(thisNavigator))
		{
			symbolCount = ((direction != 0 || !IsAtNonMergeableInlineStart(thisNavigator)) ? (symbolCount + LeaveNonMergeableAncestor(thisNavigator, LogicalDirection.Forward)) : (symbolCount + LeaveNonMergeableAncestor(thisNavigator, LogicalDirection.Backward)));
		}
		return symbolCount;
	}

	private static int LeaveNonMergeableAncestor(ITextPointer thisNavigator, LogicalDirection direction)
	{
		int num = 0;
		int num2 = ((direction == LogicalDirection.Forward) ? 1 : (-1));
		while (TextSchema.IsMergeableInline(thisNavigator.ParentType))
		{
			thisNavigator.MoveToNextContextPosition(direction);
			num += num2;
		}
		thisNavigator.MoveToNextContextPosition(direction);
		return num + num2;
	}

	private static bool IsAtNormalizedPosition(ITextPointer position, bool respectCaretUnitBoundaries)
	{
		if (IsPositionAtNonMergeableInlineBoundary(position))
		{
			return false;
		}
		if (TextSchema.IsValidChild(position, typeof(string)))
		{
			if (!respectCaretUnitBoundaries)
			{
				return true;
			}
			return IsAtCaretUnitBoundary(position);
		}
		if (!IsAtRowEnd(position) && !IsAtPotentialRunPosition(position) && !IsBeforeFirstTable(position))
		{
			return IsInBlockUIContainer(position);
		}
		return true;
	}

	private static bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		TextPointerContext pointerContext = position.GetPointerContext(LogicalDirection.Forward);
		if (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text && pointerContext == TextPointerContext.Text)
		{
			if (position.HasValidLayout)
			{
				return position.IsAtCaretUnitBoundary;
			}
			return !IsInsideCompoundSequence(position);
		}
		return true;
	}

	private static bool IsInsideCompoundSequence(ITextPointer position)
	{
		char[] array = new char[2];
		if (position.GetTextInRun(LogicalDirection.Backward, array, 0, 1) == 1 && position.GetTextInRun(LogicalDirection.Forward, array, 1, 1) == 1)
		{
			if (char.IsSurrogatePair(array[0], array[1]) || (array[0] == '\r' && array[1] == '\n'))
			{
				return true;
			}
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(array[1]);
			if (unicodeCategory == UnicodeCategory.SpacingCombiningMark || unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.EnclosingMark)
			{
				UnicodeCategory unicodeCategory2 = char.GetUnicodeCategory(array[0]);
				if (unicodeCategory2 != UnicodeCategory.Control && unicodeCategory2 != UnicodeCategory.Format && unicodeCategory2 != UnicodeCategory.OtherNotAssigned)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void GetWordBreakerText(ITextPointer pointer, out char[] text, out int position)
	{
		char[] array = new char[SelectionWordBreaker.MinContextLength];
		char[] array2 = new char[SelectionWordBreaker.MinContextLength];
		int num = 0;
		int num2 = 0;
		ITextPointer textPointer = pointer.CreatePointer();
		do
		{
			int num3 = Math.Min(textPointer.GetTextRunLength(LogicalDirection.Backward), SelectionWordBreaker.MinContextLength - num);
			num += num3;
			textPointer.MoveByOffset(-num3);
			textPointer.GetTextInRun(LogicalDirection.Forward, array, SelectionWordBreaker.MinContextLength - num, num3);
			if (num == SelectionWordBreaker.MinContextLength)
			{
				break;
			}
			textPointer.MoveToInsertionPosition(LogicalDirection.Backward);
		}
		while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text);
		textPointer.MoveToPosition(pointer);
		do
		{
			int num3 = Math.Min(textPointer.GetTextRunLength(LogicalDirection.Forward), SelectionWordBreaker.MinContextLength - num2);
			textPointer.GetTextInRun(LogicalDirection.Forward, array2, num2, num3);
			num2 += num3;
			if (num2 == SelectionWordBreaker.MinContextLength)
			{
				break;
			}
			textPointer.MoveByOffset(num3);
			textPointer.MoveToInsertionPosition(LogicalDirection.Forward);
		}
		while (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text);
		text = new char[num + num2];
		Array.Copy(array, SelectionWordBreaker.MinContextLength - num, text, 0, num);
		Array.Copy(array2, 0, text, num, num2);
		position = num;
	}

	private static bool IsAtNonMergeableInlineEdge(ITextPointer position, LogicalDirection direction)
	{
		BorderingElementCategory borderingElementCategory = GetBorderingElementCategory(position, direction);
		if (borderingElementCategory == BorderingElementCategory.MergeableScopingInline)
		{
			ITextPointer textPointer = position.CreatePointer();
			do
			{
				textPointer.MoveToNextContextPosition(direction);
			}
			while ((borderingElementCategory = GetBorderingElementCategory(textPointer, direction)) == BorderingElementCategory.MergeableScopingInline);
		}
		return borderingElementCategory == BorderingElementCategory.NonMergeableScopingInline;
	}

	private static BorderingElementCategory GetBorderingElementCategory(ITextPointer position, LogicalDirection direction)
	{
		TextPointerContext textPointerContext = ((direction == LogicalDirection.Forward) ? TextPointerContext.ElementEnd : TextPointerContext.ElementStart);
		if (position.GetPointerContext(direction) != textPointerContext || !typeof(Inline).IsAssignableFrom(position.ParentType))
		{
			return BorderingElementCategory.NotScopingInline;
		}
		if (TextSchema.IsMergeableInline(position.ParentType))
		{
			return BorderingElementCategory.MergeableScopingInline;
		}
		return BorderingElementCategory.NonMergeableScopingInline;
	}

	private static bool IsNextToRichBreak(ITextPointer thisPosition, LogicalDirection direction, Type lineBreakType)
	{
		Invariant.Assert(lineBreakType == null || lineBreakType == typeof(LineBreak) || lineBreakType == typeof(Paragraph));
		bool result = false;
		while (true)
		{
			Type elementType = thisPosition.GetElementType(direction);
			if (lineBreakType == null)
			{
				if (typeof(LineBreak).IsAssignableFrom(elementType) || typeof(Paragraph).IsAssignableFrom(elementType))
				{
					result = true;
					break;
				}
			}
			else if (lineBreakType.IsAssignableFrom(elementType))
			{
				result = true;
				break;
			}
			if (!TextSchema.IsFormattingType(elementType))
			{
				break;
			}
			thisPosition = thisPosition.GetNextContextPosition(direction);
		}
		return result;
	}
}
