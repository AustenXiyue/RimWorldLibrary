using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MS.Internal;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorSelection
{
	private const string KeyMoveDownByLine = "Down";

	private const string KeyMoveDownByPage = "PageDown";

	private const string KeyMoveDownByParagraph = "Ctrl+Down";

	private const string KeyMoveLeftByCharacter = "Left";

	private const string KeyMoveLeftByWord = "Ctrl+Left";

	private const string KeyMoveRightByCharacter = "Right";

	private const string KeyMoveRightByWord = "Ctrl+Right";

	private const string KeyMoveToColumnEnd = "Alt+PageDown";

	private const string KeyMoveToColumnStart = "Alt+PageUp";

	private const string KeyMoveToDocumentEnd = "Ctrl+End";

	private const string KeyMoveToDocumentStart = "Ctrl+Home";

	private const string KeyMoveToLineEnd = "End";

	private const string KeyMoveToLineStart = "Home";

	private const string KeyMoveToWindowBottom = "Alt+Ctrl+PageDown";

	private const string KeyMoveToWindowTop = "Alt+Ctrl+PageUp";

	private const string KeyMoveUpByLine = "Up";

	private const string KeyMoveUpByPage = "PageUp";

	private const string KeyMoveUpByParagraph = "Ctrl+Up";

	private const string KeySelectAll = "Ctrl+A";

	private const string KeySelectDownByLine = "Shift+Down";

	private const string KeySelectDownByPage = "Shift+PageDown";

	private const string KeySelectDownByParagraph = "Ctrl+Shift+Down";

	private const string KeySelectLeftByCharacter = "Shift+Left";

	private const string KeySelectLeftByWord = "Ctrl+Shift+Left";

	private const string KeySelectRightByCharacter = "Shift+Right";

	private const string KeySelectRightByWord = "Ctrl+Shift+Right";

	private const string KeySelectToColumnEnd = "Alt+Shift+PageDown";

	private const string KeySelectToColumnStart = "Alt+Shift+PageUp";

	private const string KeySelectToDocumentEnd = "Ctrl+Shift+End";

	private const string KeySelectToDocumentStart = "Ctrl+Shift+Home";

	private const string KeySelectToLineEnd = "Shift+End";

	private const string KeySelectToLineStart = "Shift+Home";

	private const string KeySelectToWindowBottom = "Alt+Ctrl+Shift+PageDown";

	private const string KeySelectToWindowTop = "Alt+Ctrl+Shift+PageUp";

	private const string KeySelectUpByLine = "Shift+Up";

	private const string KeySelectUpByPage = "Shift+PageUp";

	private const string KeySelectUpByParagraph = "Ctrl+Shift+Up";

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = OnNYICommand;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryStatusCaretNavigation;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler2 = OnQueryStatusKeyboardSelection;
		CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.SelectAll, OnSelectAll, canExecuteRoutedEventHandler2, "Ctrl+A", "KeySelectAllDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveRightByCharacter, OnMoveRightByCharacter, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Right", "KeyMoveRightByCharacterDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveLeftByCharacter, OnMoveLeftByCharacter, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Left", "KeyMoveLeftByCharacterDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveRightByWord, OnMoveRightByWord, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Right", "KeyMoveRightByWordDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveLeftByWord, OnMoveLeftByWord, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Left", "KeyMoveLeftByWordDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveDownByLine, OnMoveDownByLine, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Down", "KeyMoveDownByLineDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveUpByLine, OnMoveUpByLine, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Up", "KeyMoveUpByLineDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveDownByParagraph, OnMoveDownByParagraph, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Down", "KeyMoveDownByParagraphDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveUpByParagraph, OnMoveUpByParagraph, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Up", "KeyMoveUpByParagraphDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveDownByPage, OnMoveDownByPage, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("PageDown", "KeyMoveDownByPageDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveUpByPage, OnMoveUpByPage, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("PageUp", "KeyMoveUpByPageDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToLineStart, OnMoveToLineStart, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Home", "KeyMoveToLineStartDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToLineEnd, OnMoveToLineEnd, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("End", "KeyMoveToLineEndDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToColumnStart, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+PageUp", "KeyMoveToColumnStartDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToColumnEnd, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+PageDown", "KeyMoveToColumnEndDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToWindowTop, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+PageUp", "KeyMoveToWindowTopDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToWindowBottom, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+PageDown", "KeyMoveToWindowBottomDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToDocumentStart, OnMoveToDocumentStart, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Home", "KeyMoveToDocumentStartDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MoveToDocumentEnd, OnMoveToDocumentEnd, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+End", "KeyMoveToDocumentEndDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectRightByCharacter, OnSelectRightByCharacter, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+Right", "KeySelectRightByCharacterDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectLeftByCharacter, OnSelectLeftByCharacter, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+Left", "KeySelectLeftByCharacterDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectRightByWord, OnSelectRightByWord, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+Right", "KeySelectRightByWordDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectLeftByWord, OnSelectLeftByWord, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+Left", "KeySelectLeftByWordDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectDownByLine, OnSelectDownByLine, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+Down", "KeySelectDownByLineDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectUpByLine, OnSelectUpByLine, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+Up", "KeySelectUpByLineDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectDownByParagraph, OnSelectDownByParagraph, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+Down", "KeySelectDownByParagraphDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectUpByParagraph, OnSelectUpByParagraph, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+Up", "KeySelectUpByParagraphDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectDownByPage, OnSelectDownByPage, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+PageDown", "KeySelectDownByPageDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectUpByPage, OnSelectUpByPage, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+PageUp", "KeySelectUpByPageDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToLineStart, OnSelectToLineStart, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+Home", "KeySelectToLineStartDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToLineEnd, OnSelectToLineEnd, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Shift+End", "KeySelectToLineEndDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToColumnStart, executedRoutedEventHandler, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Alt+Shift+PageUp", "KeySelectToColumnStartDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToColumnEnd, executedRoutedEventHandler, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Alt+Shift+PageDown", "KeySelectToColumnEndDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToWindowTop, executedRoutedEventHandler, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+PageUp", "KeySelectToWindowTopDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToWindowBottom, executedRoutedEventHandler, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+PageDown", "KeySelectToWindowBottomDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToDocumentStart, OnSelectToDocumentStart, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+Home", "KeySelectToDocumentStartDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SelectToDocumentEnd, OnSelectToDocumentEnd, canExecuteRoutedEventHandler2, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+End", "KeySelectToDocumentEndDisplayString"));
	}

	internal static void _ClearSuggestedX(TextEditor This)
	{
		This._suggestedX = double.NaN;
		This._NextLineAdvanceMovingPosition = null;
	}

	internal static TextSegment GetNormalizedLineRange(ITextView textView, ITextPointer position)
	{
		TextSegment lineRange = textView.GetLineRange(position);
		if (lineRange.IsNull)
		{
			if (!typeof(BlockUIContainer).IsAssignableFrom(position.ParentType))
			{
				return lineRange;
			}
			ITextPointer textPointer = position.CreatePointer(LogicalDirection.Forward);
			textPointer.MoveToElementEdge(ElementEdge.AfterStart);
			ITextPointer textPointer2 = position.CreatePointer(LogicalDirection.Backward);
			textPointer2.MoveToElementEdge(ElementEdge.BeforeEnd);
			return new TextSegment(textPointer, textPointer2);
		}
		ITextRange textRange = new TextRange(lineRange.Start, lineRange.End);
		return new TextSegment(textRange.Start, textRange.End);
	}

	internal static bool IsPaginated(ITextView textview)
	{
		return !(textview is TextBoxView);
	}

	private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock(disableScroll: true))
		{
			textEditor.Selection.Select(textEditor.TextContainer.Start, textEditor.TextContainer.End);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnMoveRightByCharacter(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = ((!IsFlowDirectionRightToLeftThenTopToBottom(textEditor)) ? LogicalDirection.Forward : LogicalDirection.Backward);
			MoveToCharacterLogicalDirection(textEditor, direction, extend: false);
		}
	}

	private static void OnMoveLeftByCharacter(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = (IsFlowDirectionRightToLeftThenTopToBottom(textEditor) ? LogicalDirection.Forward : LogicalDirection.Backward);
			MoveToCharacterLogicalDirection(textEditor, direction, extend: false);
		}
	}

	private static void OnMoveRightByWord(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = ((!IsFlowDirectionRightToLeftThenTopToBottom(textEditor)) ? LogicalDirection.Forward : LogicalDirection.Backward);
			NavigateWordLogicalDirection(textEditor, direction);
		}
	}

	private static void OnMoveLeftByWord(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = (IsFlowDirectionRightToLeftThenTopToBottom(textEditor) ? LogicalDirection.Forward : LogicalDirection.Backward);
			NavigateWordLogicalDirection(textEditor, direction);
		}
	}

	private static void OnMoveDownByLine(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!textEditor.Selection.End.ValidateLayout())
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (!textEditor.Selection.IsEmpty)
			{
				ITextPointer endInner = GetEndInner(textEditor);
				textEditor.Selection.SetCaretToPosition(endInner, endInner.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				_ClearSuggestedX(textEditor);
			}
			Invariant.Assert(textEditor.Selection.IsEmpty);
			AdjustCaretAtTableRowEnd(textEditor);
			ITextPointer innerMovingPosition;
			double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
			if (innerMovingPosition != null)
			{
				double newSuggestedX;
				int linesMoved;
				ITextPointer positionAtNextLine = textEditor.TextView.GetPositionAtNextLine(textEditor.Selection.MovingPosition, suggestedX, 1, out newSuggestedX, out linesMoved);
				Invariant.Assert(positionAtNextLine != null);
				if (linesMoved != 0)
				{
					UpdateSuggestedXOnColumnOrPageBoundary(textEditor, newSuggestedX);
					textEditor.Selection.SetCaretToPosition(positionAtNextLine, positionAtNextLine.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				}
				else if (TextPointerBase.IsInAnchoredBlock(innerMovingPosition))
				{
					ITextPointer positionAtLineEnd = GetPositionAtLineEnd(innerMovingPosition);
					ITextPointer nextInsertionPosition = positionAtLineEnd.GetNextInsertionPosition(LogicalDirection.Forward);
					textEditor.Selection.SetCaretToPosition((nextInsertionPosition != null) ? nextInsertionPosition : positionAtLineEnd, innerMovingPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				}
				else if (IsPaginated(textEditor.TextView))
				{
					textEditor.TextView.BringLineIntoViewCompleted += HandleMoveByLineCompleted;
					textEditor.TextView.BringLineIntoViewAsync(positionAtNextLine, newSuggestedX, 1, textEditor);
				}
				TextEditorTyping._BreakTypingSequence(textEditor);
				ClearSpringloadFormatting(textEditor);
			}
		}
	}

	private static void OnMoveUpByLine(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!textEditor.Selection.Start.ValidateLayout())
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (!textEditor.Selection.IsEmpty)
			{
				ITextPointer startInner = GetStartInner(textEditor);
				textEditor.Selection.SetCaretToPosition(startInner, startInner.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				_ClearSuggestedX(textEditor);
			}
			Invariant.Assert(textEditor.Selection.IsEmpty);
			AdjustCaretAtTableRowEnd(textEditor);
			ITextPointer innerMovingPosition;
			double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
			if (innerMovingPosition != null)
			{
				double newSuggestedX;
				int linesMoved;
				ITextPointer positionAtNextLine = textEditor.TextView.GetPositionAtNextLine(textEditor.Selection.MovingPosition, suggestedX, -1, out newSuggestedX, out linesMoved);
				Invariant.Assert(positionAtNextLine != null);
				if (linesMoved != 0)
				{
					UpdateSuggestedXOnColumnOrPageBoundary(textEditor, newSuggestedX);
					textEditor.Selection.SetCaretToPosition(positionAtNextLine, positionAtNextLine.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				}
				else if (TextPointerBase.IsInAnchoredBlock(innerMovingPosition))
				{
					ITextPointer positionAtLineStart = GetPositionAtLineStart(innerMovingPosition);
					ITextPointer nextInsertionPosition = positionAtLineStart.GetNextInsertionPosition(LogicalDirection.Backward);
					textEditor.Selection.SetCaretToPosition((nextInsertionPosition != null) ? nextInsertionPosition : positionAtLineStart, innerMovingPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				}
				else if (IsPaginated(textEditor.TextView))
				{
					textEditor.TextView.BringLineIntoViewCompleted += HandleMoveByLineCompleted;
					textEditor.TextView.BringLineIntoViewAsync(positionAtNextLine, newSuggestedX, -1, textEditor);
				}
				TextEditorTyping._BreakTypingSequence(textEditor);
				ClearSpringloadFormatting(textEditor);
			}
		}
	}

	private static void OnMoveDownByParagraph(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
			if (!textEditor.Selection.IsEmpty)
			{
				ITextPointer endInner = GetEndInner(textEditor);
				textEditor.Selection.SetCaretToPosition(endInner, endInner.LogicalDirection, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
			ITextPointer textPointer = textEditor.Selection.MovingPosition.CreatePointer();
			ITextRange textRange = new TextRange(textPointer, textPointer);
			textRange.SelectParagraph(textPointer);
			textPointer.MoveToPosition(textRange.End);
			if (textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward))
			{
				textRange.SelectParagraph(textPointer);
				textEditor.Selection.SetCaretToPosition(textRange.Start, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
			else
			{
				textEditor.Selection.SetCaretToPosition(textRange.End, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
		}
	}

	private static void OnMoveUpByParagraph(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
			if (!textEditor.Selection.IsEmpty)
			{
				ITextPointer startInner = GetStartInner(textEditor);
				textEditor.Selection.SetCaretToPosition(startInner, startInner.LogicalDirection, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
			ITextPointer textPointer = textEditor.Selection.MovingPosition.CreatePointer();
			ITextRange textRange = new TextRange(textPointer, textPointer);
			textRange.SelectParagraph(textPointer);
			if (textEditor.Selection.Start.CompareTo(textRange.Start) > 0)
			{
				textEditor.Selection.SetCaretToPosition(textRange.Start, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
				return;
			}
			textPointer.MoveToPosition(textRange.Start);
			if (textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward))
			{
				textRange.SelectParagraph(textPointer);
				textEditor.Selection.SetCaretToPosition(textRange.Start, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
		}
	}

	private static void OnMoveDownByPage(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!textEditor.Selection.End.ValidateLayout())
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (!textEditor.Selection.IsEmpty)
			{
				ITextPointer endInner = GetEndInner(textEditor);
				textEditor.Selection.SetCaretToPosition(endInner, endInner.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
			}
			ITextPointer innerMovingPosition;
			double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
			if (innerMovingPosition == null)
			{
				return;
			}
			double num = (double)textEditor.UiScope.GetValue(TextEditor.PageHeightProperty);
			if (num == 0.0)
			{
				if (IsPaginated(textEditor.TextView))
				{
					double suggestedYFromPosition = GetSuggestedYFromPosition(textEditor, innerMovingPosition);
					Point newSuggestedOffset;
					int pagesMoved;
					ITextPointer positionAtNextPage = textEditor.TextView.GetPositionAtNextPage(innerMovingPosition, new Point(GetViewportXOffset(textEditor.TextView, suggestedX), suggestedYFromPosition), 1, out newSuggestedOffset, out pagesMoved);
					double x = newSuggestedOffset.X;
					Invariant.Assert(positionAtNextPage != null);
					if (pagesMoved != 0)
					{
						UpdateSuggestedXOnColumnOrPageBoundary(textEditor, x);
						textEditor.Selection.SetCaretToPosition(positionAtNextPage, positionAtNextPage.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: false);
					}
					else if (IsPaginated(textEditor.TextView))
					{
						textEditor.TextView.BringPageIntoViewCompleted += HandleMoveByPageCompleted;
						textEditor.TextView.BringPageIntoViewAsync(positionAtNextPage, newSuggestedOffset, 1, textEditor);
					}
				}
			}
			else
			{
				Rect rectangleFromTextPosition = textEditor.TextView.GetRectangleFromTextPosition(innerMovingPosition);
				Point point = new Point(GetViewportXOffset(textEditor.TextView, suggestedX), rectangleFromTextPosition.Top + num);
				ITextPointer positionAtNextPage = textEditor.TextView.GetTextPositionFromPoint(point, snapToText: true);
				if (positionAtNextPage == null)
				{
					return;
				}
				if (positionAtNextPage.CompareTo(innerMovingPosition) <= 0)
				{
					positionAtNextPage = textEditor.TextContainer.End;
					_ClearSuggestedX(textEditor);
				}
				ScrollBar.PageDownCommand.Execute(null, textEditor.TextView.RenderScope);
				textEditor.TextView.RenderScope.UpdateLayout();
				textEditor.Selection.SetCaretToPosition(positionAtNextPage, positionAtNextPage.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: false);
			}
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnMoveUpByPage(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!textEditor.Selection.Start.ValidateLayout())
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (!textEditor.Selection.IsEmpty)
			{
				ITextPointer startInner = GetStartInner(textEditor);
				textEditor.Selection.SetCaretToPosition(startInner, startInner.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
			}
			ITextPointer innerMovingPosition;
			double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
			if (innerMovingPosition == null)
			{
				return;
			}
			double num = (double)textEditor.UiScope.GetValue(TextEditor.PageHeightProperty);
			if (num == 0.0)
			{
				if (IsPaginated(textEditor.TextView))
				{
					double suggestedYFromPosition = GetSuggestedYFromPosition(textEditor, innerMovingPosition);
					Point newSuggestedOffset;
					int pagesMoved;
					ITextPointer positionAtNextPage = textEditor.TextView.GetPositionAtNextPage(innerMovingPosition, new Point(GetViewportXOffset(textEditor.TextView, suggestedX), suggestedYFromPosition), -1, out newSuggestedOffset, out pagesMoved);
					double x = newSuggestedOffset.X;
					Invariant.Assert(positionAtNextPage != null);
					if (pagesMoved != 0)
					{
						UpdateSuggestedXOnColumnOrPageBoundary(textEditor, x);
						textEditor.Selection.SetCaretToPosition(positionAtNextPage, positionAtNextPage.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: false);
					}
					else if (IsPaginated(textEditor.TextView))
					{
						textEditor.TextView.BringPageIntoViewCompleted += HandleMoveByPageCompleted;
						textEditor.TextView.BringPageIntoViewAsync(positionAtNextPage, newSuggestedOffset, -1, textEditor);
					}
				}
			}
			else
			{
				Rect rectangleFromTextPosition = textEditor.TextView.GetRectangleFromTextPosition(innerMovingPosition);
				Point point = new Point(GetViewportXOffset(textEditor.TextView, suggestedX), rectangleFromTextPosition.Bottom - num);
				ITextPointer positionAtNextPage = textEditor.TextView.GetTextPositionFromPoint(point, snapToText: true);
				if (positionAtNextPage == null)
				{
					return;
				}
				if (positionAtNextPage.CompareTo(innerMovingPosition) >= 0)
				{
					positionAtNextPage = textEditor.TextContainer.Start;
					_ClearSuggestedX(textEditor);
				}
				ScrollBar.PageUpCommand.Execute(null, textEditor.TextView.RenderScope);
				textEditor.TextView.RenderScope.UpdateLayout();
				textEditor.Selection.SetCaretToPosition(positionAtNextPage, positionAtNextPage.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: false);
			}
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnMoveToLineStart(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		ITextPointer startInner = GetStartInner(textEditor);
		if (!startInner.ValidateLayout())
		{
			return;
		}
		TextSegment normalizedLineRange = GetNormalizedLineRange(textEditor.TextView, startInner);
		if (normalizedLineRange.IsNull)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ITextPointer frozenPointer = normalizedLineRange.Start.GetFrozenPointer(LogicalDirection.Forward);
			textEditor.Selection.SetCaretToPosition(frozenPointer, LogicalDirection.Forward, allowStopAtLineEnd: true, allowStopNearSpace: true);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnMoveToLineEnd(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		ITextPointer endInner = GetEndInner(textEditor);
		if (!endInner.ValidateLayout())
		{
			return;
		}
		TextSegment normalizedLineRange = GetNormalizedLineRange(textEditor.TextView, endInner);
		if (normalizedLineRange.IsNull)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			LogicalDirection logicalDirection = (TextPointerBase.IsNextToPlainLineBreak(normalizedLineRange.End, LogicalDirection.Backward) ? LogicalDirection.Forward : LogicalDirection.Backward);
			ITextPointer frozenPointer = normalizedLineRange.End.GetFrozenPointer(logicalDirection);
			textEditor.Selection.SetCaretToPosition(frozenPointer, logicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnMoveToDocumentStart(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			textEditor.Selection.SetCaretToPosition(textEditor.TextContainer.Start, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnMoveToDocumentEnd(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			textEditor.Selection.SetCaretToPosition(textEditor.TextContainer.End, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectRightByCharacter(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = ((!IsFlowDirectionRightToLeftThenTopToBottom(textEditor)) ? LogicalDirection.Forward : LogicalDirection.Backward);
			MoveToCharacterLogicalDirection(textEditor, direction, extend: true);
		}
	}

	private static void OnSelectLeftByCharacter(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = (IsFlowDirectionRightToLeftThenTopToBottom(textEditor) ? LogicalDirection.Forward : LogicalDirection.Backward);
			MoveToCharacterLogicalDirection(textEditor, direction, extend: true);
		}
	}

	private static void OnSelectRightByWord(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = ((!IsFlowDirectionRightToLeftThenTopToBottom(textEditor)) ? LogicalDirection.Forward : LogicalDirection.Backward);
			ExtendWordLogicalDirection(textEditor, direction);
		}
	}

	private static void OnSelectLeftByWord(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && textEditor._IsSourceInScope(args.Source))
		{
			LogicalDirection direction = (IsFlowDirectionRightToLeftThenTopToBottom(textEditor) ? LogicalDirection.Forward : LogicalDirection.Backward);
			ExtendWordLogicalDirection(textEditor, direction);
		}
	}

	private static void OnSelectDownByLine(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (!textEditor.Selection.ExtendToNextTableRow(LogicalDirection.Forward))
			{
				ITextPointer innerMovingPosition;
				double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
				if (innerMovingPosition == null)
				{
					return;
				}
				if (textEditor._NextLineAdvanceMovingPosition != null && textEditor._IsNextLineAdvanceMovingPositionAtDocumentHead)
				{
					ExtendSelectionAndBringIntoView(textEditor._NextLineAdvanceMovingPosition, textEditor);
					textEditor._NextLineAdvanceMovingPosition = null;
				}
				else
				{
					ITextPointer position = AdjustPositionAtTableRowEnd(innerMovingPosition);
					position = textEditor.TextView.GetPositionAtNextLine(position, suggestedX, 1, out var newSuggestedX, out var linesMoved);
					Invariant.Assert(position != null);
					if (linesMoved != 0)
					{
						UpdateSuggestedXOnColumnOrPageBoundary(textEditor, newSuggestedX);
						AdjustMovingPositionForSelectDownByLine(textEditor, position, innerMovingPosition, newSuggestedX);
					}
					else if (TextPointerBase.IsInAnchoredBlock(innerMovingPosition))
					{
						ITextPointer positionAtLineEnd = GetPositionAtLineEnd(innerMovingPosition);
						ITextPointer nextInsertionPosition = positionAtLineEnd.GetNextInsertionPosition(LogicalDirection.Forward);
						ExtendSelectionAndBringIntoView((nextInsertionPosition != null) ? nextInsertionPosition : positionAtLineEnd, textEditor);
					}
					else if (IsPaginated(textEditor.TextView))
					{
						textEditor.TextView.BringLineIntoViewCompleted += HandleSelectByLineCompleted;
						textEditor.TextView.BringLineIntoViewAsync(position, newSuggestedX, 1, textEditor);
					}
					else
					{
						if (textEditor._NextLineAdvanceMovingPosition == null)
						{
							textEditor._NextLineAdvanceMovingPosition = innerMovingPosition;
							textEditor._IsNextLineAdvanceMovingPositionAtDocumentHead = false;
						}
						ExtendSelectionAndBringIntoView(GetPositionAtLineEnd(position), textEditor);
					}
				}
			}
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void AdjustMovingPositionForSelectDownByLine(TextEditor This, ITextPointer newMovingPosition, ITextPointer originalMovingPosition, double suggestedX)
	{
		int num = newMovingPosition.CompareTo(originalMovingPosition);
		if (num > 0 || (num == 0 && newMovingPosition.LogicalDirection != originalMovingPosition.LogicalDirection))
		{
			if (TextPointerBase.IsNextToAnyBreak(newMovingPosition, LogicalDirection.Forward) || newMovingPosition.GetNextInsertionPosition(LogicalDirection.Forward) == null)
			{
				double absoluteXOffset = GetAbsoluteXOffset(This.TextView, newMovingPosition);
				FlowDirection scopingParagraphFlowDirection = GetScopingParagraphFlowDirection(newMovingPosition);
				FlowDirection flowDirection = This.UiScope.FlowDirection;
				if ((scopingParagraphFlowDirection == flowDirection && absoluteXOffset < suggestedX) || (scopingParagraphFlowDirection != flowDirection && absoluteXOffset > suggestedX))
				{
					newMovingPosition = newMovingPosition.GetInsertionPosition(LogicalDirection.Forward);
					newMovingPosition = newMovingPosition.GetNextInsertionPosition(LogicalDirection.Forward);
					if (newMovingPosition == null)
					{
						newMovingPosition = originalMovingPosition.TextContainer.End;
					}
					newMovingPosition = newMovingPosition.GetFrozenPointer(LogicalDirection.Backward);
				}
			}
			ExtendSelectionAndBringIntoView(newMovingPosition, This);
		}
		else
		{
			if (This._NextLineAdvanceMovingPosition == null)
			{
				This._NextLineAdvanceMovingPosition = originalMovingPosition;
				This._IsNextLineAdvanceMovingPositionAtDocumentHead = false;
			}
			newMovingPosition = GetPositionAtLineEnd(originalMovingPosition);
			if (newMovingPosition.GetNextInsertionPosition(LogicalDirection.Forward) == null)
			{
				newMovingPosition = newMovingPosition.TextContainer.End;
			}
			ExtendSelectionAndBringIntoView(newMovingPosition, This);
		}
	}

	private static void OnSelectUpByLine(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (!textEditor.Selection.ExtendToNextTableRow(LogicalDirection.Backward))
			{
				ITextPointer innerMovingPosition;
				double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
				if (innerMovingPosition == null)
				{
					return;
				}
				if (textEditor._NextLineAdvanceMovingPosition != null && !textEditor._IsNextLineAdvanceMovingPositionAtDocumentHead)
				{
					ExtendSelectionAndBringIntoView(textEditor._NextLineAdvanceMovingPosition, textEditor);
					textEditor._NextLineAdvanceMovingPosition = null;
				}
				else
				{
					ITextPointer position = AdjustPositionAtTableRowEnd(innerMovingPosition);
					position = textEditor.TextView.GetPositionAtNextLine(position, suggestedX, -1, out var newSuggestedX, out var linesMoved);
					Invariant.Assert(position != null);
					if (linesMoved != 0)
					{
						UpdateSuggestedXOnColumnOrPageBoundary(textEditor, newSuggestedX);
						int num = position.CompareTo(innerMovingPosition);
						if (num < 0 || (num == 0 && position.LogicalDirection != innerMovingPosition.LogicalDirection))
						{
							ExtendSelectionAndBringIntoView(position, textEditor);
						}
						else
						{
							ExtendSelectionAndBringIntoView(GetPositionAtLineStart(innerMovingPosition), textEditor);
						}
					}
					else if (TextPointerBase.IsInAnchoredBlock(innerMovingPosition))
					{
						ITextPointer positionAtLineStart = GetPositionAtLineStart(innerMovingPosition);
						ITextPointer nextInsertionPosition = positionAtLineStart.GetNextInsertionPosition(LogicalDirection.Backward);
						ExtendSelectionAndBringIntoView((nextInsertionPosition != null) ? nextInsertionPosition : positionAtLineStart, textEditor);
					}
					else if (IsPaginated(textEditor.TextView))
					{
						textEditor.TextView.BringLineIntoViewCompleted += HandleSelectByLineCompleted;
						textEditor.TextView.BringLineIntoViewAsync(position, newSuggestedX, -1, textEditor);
					}
					else
					{
						if (textEditor._NextLineAdvanceMovingPosition == null)
						{
							textEditor._NextLineAdvanceMovingPosition = innerMovingPosition;
							textEditor._IsNextLineAdvanceMovingPositionAtDocumentHead = true;
						}
						ExtendSelectionAndBringIntoView(GetPositionAtLineStart(position), textEditor);
					}
				}
			}
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectDownByParagraph(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
			ITextPointer textPointer = textEditor.Selection.MovingPosition.CreatePointer();
			ITextRange textRange = new TextRange(textPointer, textPointer);
			textRange.SelectParagraph(textPointer);
			textPointer.MoveToPosition(textRange.End);
			if (textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward))
			{
				textRange.SelectParagraph(textPointer);
				ExtendSelectionAndBringIntoView(textRange.Start, textEditor);
			}
			else
			{
				ExtendSelectionAndBringIntoView(textRange.End, textEditor);
			}
		}
	}

	private static void OnSelectUpByParagraph(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
			ITextPointer textPointer = textEditor.Selection.MovingPosition.CreatePointer();
			ITextRange textRange = new TextRange(textPointer, textPointer);
			textRange.SelectParagraph(textPointer);
			if (textPointer.CompareTo(textRange.Start) > 0)
			{
				ExtendSelectionAndBringIntoView(textRange.Start, textEditor);
				return;
			}
			textPointer.MoveToPosition(textRange.Start);
			if (textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward))
			{
				textRange.SelectParagraph(textPointer);
				ExtendSelectionAndBringIntoView(textRange.Start, textEditor);
			}
		}
	}

	private static void OnSelectDownByPage(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		ITextPointer innerMovingPosition;
		double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
		if (innerMovingPosition == null)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			double num = (double)textEditor.UiScope.GetValue(TextEditor.PageHeightProperty);
			if (num == 0.0)
			{
				if (IsPaginated(textEditor.TextView))
				{
					double suggestedYFromPosition = GetSuggestedYFromPosition(textEditor, innerMovingPosition);
					Point newSuggestedOffset;
					int pagesMoved;
					ITextPointer positionAtNextPage = textEditor.TextView.GetPositionAtNextPage(innerMovingPosition, new Point(GetViewportXOffset(textEditor.TextView, suggestedX), suggestedYFromPosition), 1, out newSuggestedOffset, out pagesMoved);
					double x = newSuggestedOffset.X;
					Invariant.Assert(positionAtNextPage != null);
					if (pagesMoved != 0)
					{
						UpdateSuggestedXOnColumnOrPageBoundary(textEditor, x);
						ExtendSelectionAndBringIntoView(positionAtNextPage, textEditor);
					}
					else if (IsPaginated(textEditor.TextView))
					{
						textEditor.TextView.BringPageIntoViewCompleted += HandleSelectByPageCompleted;
						textEditor.TextView.BringPageIntoViewAsync(positionAtNextPage, newSuggestedOffset, 1, textEditor);
					}
					else
					{
						ExtendSelectionAndBringIntoView(positionAtNextPage.TextContainer.End, textEditor);
					}
				}
			}
			else
			{
				Rect rectangleFromTextPosition = textEditor.TextView.GetRectangleFromTextPosition(innerMovingPosition);
				Point point = new Point(GetViewportXOffset(textEditor.TextView, suggestedX), rectangleFromTextPosition.Top + num);
				ITextPointer positionAtNextPage = textEditor.TextView.GetTextPositionFromPoint(point, snapToText: true);
				if (positionAtNextPage == null)
				{
					return;
				}
				if (positionAtNextPage.CompareTo(innerMovingPosition) <= 0)
				{
					positionAtNextPage = textEditor.TextContainer.End;
				}
				ExtendSelectionAndBringIntoView(positionAtNextPage, textEditor);
				ScrollBar.PageDownCommand.Execute(null, textEditor.TextView.RenderScope);
			}
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectUpByPage(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		ITextPointer innerMovingPosition;
		double suggestedX = GetSuggestedX(textEditor, out innerMovingPosition);
		if (innerMovingPosition == null)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			double num = (double)textEditor.UiScope.GetValue(TextEditor.PageHeightProperty);
			if (num == 0.0)
			{
				if (IsPaginated(textEditor.TextView))
				{
					double suggestedYFromPosition = GetSuggestedYFromPosition(textEditor, innerMovingPosition);
					Point newSuggestedOffset;
					int pagesMoved;
					ITextPointer positionAtNextPage = textEditor.TextView.GetPositionAtNextPage(innerMovingPosition, new Point(GetViewportXOffset(textEditor.TextView, suggestedX), suggestedYFromPosition), -1, out newSuggestedOffset, out pagesMoved);
					double x = newSuggestedOffset.X;
					Invariant.Assert(positionAtNextPage != null);
					if (pagesMoved != 0)
					{
						UpdateSuggestedXOnColumnOrPageBoundary(textEditor, x);
						ExtendSelectionAndBringIntoView(positionAtNextPage, textEditor);
					}
					else if (IsPaginated(textEditor.TextView))
					{
						textEditor.TextView.BringPageIntoViewCompleted += HandleSelectByPageCompleted;
						textEditor.TextView.BringPageIntoViewAsync(positionAtNextPage, newSuggestedOffset, -1, textEditor);
					}
					else
					{
						ExtendSelectionAndBringIntoView(positionAtNextPage.TextContainer.Start, textEditor);
					}
				}
			}
			else
			{
				Rect rectangleFromTextPosition = textEditor.TextView.GetRectangleFromTextPosition(innerMovingPosition);
				Point point = new Point(GetViewportXOffset(textEditor.TextView, suggestedX), rectangleFromTextPosition.Bottom - num);
				ITextPointer positionAtNextPage = textEditor.TextView.GetTextPositionFromPoint(point, snapToText: true);
				if (positionAtNextPage == null)
				{
					return;
				}
				if (positionAtNextPage.CompareTo(innerMovingPosition) >= 0)
				{
					positionAtNextPage = textEditor.TextContainer.Start;
				}
				ExtendSelectionAndBringIntoView(positionAtNextPage, textEditor);
				ScrollBar.PageUpCommand.Execute(null, textEditor.TextView.RenderScope);
			}
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectToLineStart(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		ITextPointer movingPositionInner = GetMovingPositionInner(textEditor);
		if (!movingPositionInner.ValidateLayout())
		{
			return;
		}
		TextSegment normalizedLineRange = GetNormalizedLineRange(textEditor.TextView, movingPositionInner);
		if (normalizedLineRange.IsNull)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ExtendSelectionAndBringIntoView(normalizedLineRange.Start.CreatePointer(LogicalDirection.Forward), textEditor);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectToLineEnd(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		ITextPointer movingPositionInner = GetMovingPositionInner(textEditor);
		if (!movingPositionInner.ValidateLayout())
		{
			return;
		}
		TextSegment normalizedLineRange = GetNormalizedLineRange(textEditor.TextView, movingPositionInner);
		if (normalizedLineRange.IsNull || normalizedLineRange.End.CompareTo(textEditor.Selection.End) < 0)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ITextPointer textPointer = normalizedLineRange.End;
			if (TextPointerBase.IsNextToPlainLineBreak(textPointer, LogicalDirection.Forward) || TextPointerBase.IsNextToRichLineBreak(textPointer, LogicalDirection.Forward))
			{
				if (textEditor.Selection.AnchorPosition.ValidateLayout())
				{
					TextSegment normalizedLineRange2 = GetNormalizedLineRange(textEditor.TextView, textEditor.Selection.AnchorPosition);
					if (!normalizedLineRange.IsNull && normalizedLineRange2.Start.CompareTo(textEditor.Selection.AnchorPosition) == 0)
					{
						textPointer = textPointer.GetNextInsertionPosition(LogicalDirection.Forward);
					}
				}
			}
			else if (TextPointerBase.IsNextToParagraphBreak(textPointer, LogicalDirection.Forward) && TextPointerBase.IsNextToParagraphBreak(textEditor.Selection.AnchorPosition, LogicalDirection.Backward))
			{
				ITextPointer nextInsertionPosition = textPointer.GetNextInsertionPosition(LogicalDirection.Forward);
				textPointer = ((nextInsertionPosition != null) ? nextInsertionPosition : textPointer.TextContainer.End);
			}
			textPointer = textPointer.GetFrozenPointer(LogicalDirection.Backward);
			ExtendSelectionAndBringIntoView(textPointer, textEditor);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectToDocumentStart(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ExtendSelectionAndBringIntoView(textEditor.TextContainer.Start, textEditor);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void OnSelectToDocumentEnd(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || !textEditor._IsSourceInScope(args.Source))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ExtendSelectionAndBringIntoView(textEditor.TextContainer.End, textEditor);
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void HandleMoveByLineCompleted(object sender, BringLineIntoViewCompletedEventArgs e)
	{
		Invariant.Assert(sender is ITextView);
		((ITextView)sender).BringLineIntoViewCompleted -= HandleMoveByLineCompleted;
		if (e != null && !e.Cancelled && e.Error == null && e.UserState is TextEditor { _IsEnabled: not false } textEditor)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			using (textEditor.Selection.DeclareChangeBlock())
			{
				UpdateSuggestedXOnColumnOrPageBoundary(textEditor, e.NewSuggestedX);
				textEditor.Selection.SetCaretToPosition(e.NewPosition, e.NewPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
			}
		}
	}

	private static void HandleMoveByPageCompleted(object sender, BringPageIntoViewCompletedEventArgs e)
	{
		Invariant.Assert(sender is ITextView);
		((ITextView)sender).BringPageIntoViewCompleted -= HandleMoveByPageCompleted;
		if (e != null && !e.Cancelled && e.Error == null && e.UserState is TextEditor { _IsEnabled: not false } textEditor)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			using (textEditor.Selection.DeclareChangeBlock())
			{
				UpdateSuggestedXOnColumnOrPageBoundary(textEditor, e.NewSuggestedOffset.X);
				textEditor.Selection.SetCaretToPosition(e.NewPosition, e.NewPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
			}
		}
	}

	private static void HandleSelectByLineCompleted(object sender, BringLineIntoViewCompletedEventArgs e)
	{
		Invariant.Assert(sender is ITextView);
		((ITextView)sender).BringLineIntoViewCompleted -= HandleSelectByLineCompleted;
		if (e == null || e.Cancelled || e.Error != null || !(e.UserState is TextEditor { _IsEnabled: not false } textEditor))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			UpdateSuggestedXOnColumnOrPageBoundary(textEditor, e.NewSuggestedX);
			int num = e.NewPosition.CompareTo(e.Position);
			if (e.Count < 0)
			{
				if (num < 0 || (num == 0 && e.NewPosition.LogicalDirection != e.Position.LogicalDirection))
				{
					ExtendSelectionAndBringIntoView(e.NewPosition, textEditor);
					return;
				}
				if (textEditor._NextLineAdvanceMovingPosition == null)
				{
					textEditor._NextLineAdvanceMovingPosition = e.Position;
					textEditor._IsNextLineAdvanceMovingPositionAtDocumentHead = true;
				}
				ExtendSelectionAndBringIntoView(GetPositionAtLineStart(e.NewPosition), textEditor);
			}
			else
			{
				AdjustMovingPositionForSelectDownByLine(textEditor, e.NewPosition, e.Position, e.NewSuggestedX);
			}
		}
	}

	private static void HandleSelectByPageCompleted(object sender, BringPageIntoViewCompletedEventArgs e)
	{
		Invariant.Assert(sender is ITextView);
		((ITextView)sender).BringPageIntoViewCompleted -= HandleSelectByPageCompleted;
		if (e == null || e.Cancelled || e.Error != null || !(e.UserState is TextEditor { _IsEnabled: not false } textEditor))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			UpdateSuggestedXOnColumnOrPageBoundary(textEditor, e.NewSuggestedOffset.X);
			int num = e.NewPosition.CompareTo(e.Position);
			if (e.Count < 0)
			{
				if (num < 0)
				{
					ExtendSelectionAndBringIntoView(e.NewPosition, textEditor);
				}
				else
				{
					ExtendSelectionAndBringIntoView(e.NewPosition.TextContainer.Start, textEditor);
				}
			}
			else if (num > 0)
			{
				ExtendSelectionAndBringIntoView(e.NewPosition, textEditor);
			}
			else
			{
				ExtendSelectionAndBringIntoView(e.NewPosition.TextContainer.End, textEditor);
			}
		}
	}

	private static void OnQueryStatusKeyboardSelection(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled)
		{
			args.CanExecute = true;
		}
	}

	private static void OnQueryStatusCaretNavigation(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && (!textEditor.IsReadOnly || textEditor.IsReadOnlyCaretVisible))
		{
			args.CanExecute = true;
		}
	}

	private static void OnNYICommand(object source, ExecutedRoutedEventArgs args)
	{
	}

	private static void ClearSpringloadFormatting(TextEditor This)
	{
		if (This.Selection is TextSelection)
		{
			((TextSelection)This.Selection).ClearSpringloadFormatting();
		}
	}

	private static bool IsFlowDirectionRightToLeftThenTopToBottom(TextEditor textEditor)
	{
		Invariant.Assert(textEditor != null);
		ITextPointer textPointer = textEditor.Selection.MovingPosition.CreatePointer();
		while (TextSchema.IsFormattingType(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
		}
		return (FlowDirection)textPointer.GetValue(FlowDocument.FlowDirectionProperty) == FlowDirection.RightToLeft;
	}

	private static void MoveToCharacterLogicalDirection(TextEditor textEditor, LogicalDirection direction, bool extend)
	{
		Invariant.Assert(textEditor != null);
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			if (extend)
			{
				textEditor.Selection.ExtendToNextInsertionPosition(direction);
				BringIntoView(textEditor.Selection.MovingPosition, textEditor);
			}
			else
			{
				ITextPointer textPointer = ((direction == LogicalDirection.Forward) ? textEditor.Selection.End : textEditor.Selection.Start);
				if (textEditor.Selection.IsEmpty)
				{
					textPointer = textPointer.GetNextInsertionPosition(direction);
				}
				if (textPointer != null)
				{
					LogicalDirection direction2 = ((direction != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
					textPointer = textPointer.GetInsertionPosition(direction2);
					textEditor.Selection.SetCaretToPosition(textPointer, direction2, allowStopAtLineEnd: false, allowStopNearSpace: false);
				}
			}
			textEditor.Selection.OnCaretNavigation();
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void NavigateWordLogicalDirection(TextEditor textEditor, LogicalDirection direction)
	{
		Invariant.Assert(textEditor != null);
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
			if (direction == LogicalDirection.Forward)
			{
				if (!textEditor.Selection.IsEmpty && TextPointerBase.IsAtWordBoundary(textEditor.Selection.End, LogicalDirection.Forward))
				{
					textEditor.Selection.SetCaretToPosition(textEditor.Selection.End, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
				}
				else
				{
					ITextPointer textPointer = textEditor.Selection.End.CreatePointer();
					TextPointerBase.MoveToNextWordBoundary(textPointer, LogicalDirection.Forward);
					textEditor.Selection.SetCaretToPosition(textPointer, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
				}
			}
			else if (!textEditor.Selection.IsEmpty && TextPointerBase.IsAtWordBoundary(textEditor.Selection.Start, LogicalDirection.Forward))
			{
				textEditor.Selection.SetCaretToPosition(textEditor.Selection.Start, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
			else
			{
				ITextPointer textPointer2 = textEditor.Selection.Start.CreatePointer();
				TextPointerBase.MoveToNextWordBoundary(textPointer2, LogicalDirection.Backward);
				textEditor.Selection.SetCaretToPosition(textPointer2, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
			textEditor.Selection.OnCaretNavigation();
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static void ExtendWordLogicalDirection(TextEditor textEditor, LogicalDirection direction)
	{
		Invariant.Assert(textEditor != null);
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
			ITextPointer textPointer = textEditor.Selection.MovingPosition.CreatePointer();
			TextPointerBase.MoveToNextWordBoundary(textPointer, direction);
			textPointer.SetLogicalDirection((direction != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
			ExtendSelectionAndBringIntoView(textPointer, textEditor);
			textEditor.Selection.OnCaretNavigation();
			_ClearSuggestedX(textEditor);
			TextEditorTyping._BreakTypingSequence(textEditor);
			ClearSpringloadFormatting(textEditor);
		}
	}

	private static double GetSuggestedX(TextEditor This, out ITextPointer innerMovingPosition)
	{
		innerMovingPosition = GetMovingPositionInner(This);
		if (!innerMovingPosition.ValidateLayout())
		{
			innerMovingPosition = null;
			return double.NaN;
		}
		if (double.IsNaN(This._suggestedX))
		{
			This._suggestedX = GetAbsoluteXOffset(This.TextView, innerMovingPosition);
			if (This.Selection.MovingPosition.CompareTo(innerMovingPosition) > 0)
			{
				double num = (double)innerMovingPosition.GetValue(TextElement.FontSizeProperty) * 0.5;
				FlowDirection scopingParagraphFlowDirection = GetScopingParagraphFlowDirection(innerMovingPosition);
				FlowDirection flowDirection = This.UiScope.FlowDirection;
				if (scopingParagraphFlowDirection != flowDirection)
				{
					num = 0.0 - num;
				}
				This._suggestedX += num;
			}
		}
		return This._suggestedX;
	}

	private static double GetSuggestedYFromPosition(TextEditor This, ITextPointer position)
	{
		double result = double.NaN;
		if (position != null)
		{
			result = This.TextView.GetRectangleFromTextPosition(position).Y;
		}
		return result;
	}

	private static void UpdateSuggestedXOnColumnOrPageBoundary(TextEditor This, double newSuggestedX)
	{
		if (This._suggestedX != newSuggestedX)
		{
			This._suggestedX = newSuggestedX;
		}
	}

	private static ITextPointer GetMovingPositionInner(TextEditor This)
	{
		ITextPointer textPointer = This.Selection.MovingPosition;
		if (!(textPointer is DocumentSequenceTextPointer) && !(textPointer is FixedTextPointer) && textPointer.LogicalDirection == LogicalDirection.Backward && This.Selection.Start.CompareTo(textPointer) < 0 && TextPointerBase.IsNextToAnyBreak(textPointer, LogicalDirection.Backward))
		{
			textPointer = textPointer.GetNextInsertionPosition(LogicalDirection.Backward);
			if (TextPointerBase.IsNextToPlainLineBreak(textPointer, LogicalDirection.Backward))
			{
				textPointer = textPointer.GetFrozenPointer(LogicalDirection.Forward);
			}
		}
		else if (TextPointerBase.IsAfterLastParagraph(textPointer))
		{
			textPointer = textPointer.GetInsertionPosition(textPointer.LogicalDirection);
		}
		return textPointer;
	}

	private static ITextPointer GetStartInner(TextEditor This)
	{
		if (!This.Selection.IsEmpty)
		{
			return This.Selection.Start.GetFrozenPointer(LogicalDirection.Forward);
		}
		return This.Selection.Start;
	}

	private static ITextPointer GetEndInner(TextEditor This)
	{
		ITextPointer textPointer = This.Selection.End;
		if (textPointer.CompareTo(This.Selection.MovingPosition) == 0)
		{
			textPointer = GetMovingPositionInner(This);
		}
		return textPointer;
	}

	private static ITextPointer GetPositionAtLineStart(ITextPointer position)
	{
		TextSegment lineRange = position.TextContainer.TextView.GetLineRange(position);
		if (!lineRange.IsNull)
		{
			return lineRange.Start;
		}
		return position;
	}

	private static ITextPointer GetPositionAtLineEnd(ITextPointer position)
	{
		TextSegment lineRange = position.TextContainer.TextView.GetLineRange(position);
		if (!lineRange.IsNull)
		{
			return lineRange.End;
		}
		return position;
	}

	private static void ExtendSelectionAndBringIntoView(ITextPointer position, TextEditor textEditor)
	{
		textEditor.Selection.ExtendToPosition(position);
		BringIntoView(position, textEditor);
	}

	private static void BringIntoView(ITextPointer position, TextEditor textEditor)
	{
		if ((double)textEditor.UiScope.GetValue(TextEditor.PageHeightProperty) == 0.0 && textEditor.TextView != null && textEditor.TextView.IsValid && !textEditor.TextView.Contains(position) && IsPaginated(textEditor.TextView))
		{
			textEditor.TextView.BringPositionIntoViewAsync(position, textEditor);
		}
	}

	private static void AdjustCaretAtTableRowEnd(TextEditor This)
	{
		if (This.Selection.IsEmpty && TextPointerBase.IsAtRowEnd(This.Selection.Start))
		{
			ITextPointer nextInsertionPosition = This.Selection.Start.GetNextInsertionPosition(LogicalDirection.Backward);
			if (nextInsertionPosition != null)
			{
				This.Selection.SetCaretToPosition(nextInsertionPosition, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
		}
	}

	private static ITextPointer AdjustPositionAtTableRowEnd(ITextPointer position)
	{
		if (TextPointerBase.IsAtRowEnd(position))
		{
			ITextPointer nextInsertionPosition = position.GetNextInsertionPosition(LogicalDirection.Backward);
			if (nextInsertionPosition != null)
			{
				position = nextInsertionPosition;
			}
		}
		return position;
	}

	private static FlowDirection GetScopingParagraphFlowDirection(ITextPointer position)
	{
		ITextPointer textPointer = position.CreatePointer();
		while (typeof(Inline).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		}
		return (FlowDirection)textPointer.GetValue(FrameworkElement.FlowDirectionProperty);
	}

	private static double GetAbsoluteXOffset(ITextView textview, ITextPointer position)
	{
		double num = textview.GetRectangleFromTextPosition(position).X;
		if (textview is TextBoxView && textview is IScrollInfo scrollInfo)
		{
			num += scrollInfo.HorizontalOffset;
		}
		return num;
	}

	private static double GetViewportXOffset(ITextView textview, double suggestedX)
	{
		if (textview is TextBoxView && textview is IScrollInfo scrollInfo)
		{
			suggestedX -= scrollInfo.HorizontalOffset;
		}
		return suggestedX;
	}
}
