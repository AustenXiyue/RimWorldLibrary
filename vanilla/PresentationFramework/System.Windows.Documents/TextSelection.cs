using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

/// <summary>Encapsulates the selection state for the <see cref="T:System.Windows.Controls.RichTextBox" /> control.</summary>
public sealed class TextSelection : TextRange, ITextSelection, ITextRange
{
	private enum MovingEdge
	{
		Start,
		StartInner,
		EndInner,
		End,
		None
	}

	private TextEditor _textEditor;

	private TextSelectionHighlightLayer _highlightLayer;

	private DependencyObject _springloadFormatting;

	private ITextPointer _anchorPosition;

	private MovingEdge _movingPositionEdge;

	private LogicalDirection _movingPositionDirection;

	private ITextPointer _previousCursorPosition;

	private ITextPointer _reenterPosition;

	private bool _anchorWordRangeHasBeenCrossedOnce;

	private bool _allowWordExpansionOnAnchorEnd;

	private const int FONTSIGNATURE_SIZE = 16;

	private const int FONTSIGNATURE_BIDI_INDEX = 7;

	private const int FONTSIGNATURE_BIDI = 2048;

	private CaretScrollMethod _caretScrollMethod;

	private bool _pendingCaretNavigation;

	private CaretElement _caretElement;

	private bool _pendingUpdateCaretStateCallback;

	bool ITextRange._IsChanged
	{
		get
		{
			return base._IsChanged;
		}
		set
		{
			if (!base._IsChanged && value)
			{
				if (TextStore != null)
				{
					TextStore.OnSelectionChange();
				}
				if (ImmComposition != null)
				{
					ImmComposition.OnSelectionChange();
				}
			}
			base._IsChanged = value;
		}
	}

	string ITextRange.Text
	{
		get
		{
			return TextRangeBase.GetText(this);
		}
		set
		{
			TextRangeBase.BeginChange(this);
			try
			{
				TextRangeBase.SetText(this, value);
				if (base.IsEmpty)
				{
					((ITextSelection)this).SetCaretToPosition(((ITextRange)this).End, LogicalDirection.Forward, allowStopAtLineEnd: false, allowStopNearSpace: false);
				}
				Invariant.Assert(!base.IsTableCellRange);
				SetActivePositions(((ITextRange)this).Start, ((ITextRange)this).End);
			}
			finally
			{
				TextRangeBase.EndChange(this);
			}
		}
	}

	ITextPointer ITextSelection.AnchorPosition
	{
		get
		{
			Invariant.Assert(base.IsEmpty || _anchorPosition != null);
			Invariant.Assert(_anchorPosition == null || _anchorPosition.IsFrozen);
			if (!base.IsEmpty)
			{
				return _anchorPosition;
			}
			return ((ITextRange)this).Start;
		}
	}

	ITextPointer ITextSelection.MovingPosition
	{
		get
		{
			if (base.IsEmpty)
			{
				return ((ITextRange)this).Start;
			}
			ITextPointer textPointer;
			switch (_movingPositionEdge)
			{
			case MovingEdge.Start:
				textPointer = ((ITextRange)this).Start;
				break;
			case MovingEdge.StartInner:
				textPointer = ((ITextRange)this).TextSegments[0].End;
				break;
			case MovingEdge.EndInner:
				textPointer = ((ITextRange)this).TextSegments[((ITextRange)this).TextSegments.Count - 1].Start;
				break;
			case MovingEdge.End:
				textPointer = ((ITextRange)this).End;
				break;
			default:
				Invariant.Assert(condition: false, "MovingEdge should never be None with non-empty TextSelection!");
				textPointer = null;
				break;
			}
			return textPointer.GetFrozenPointer(_movingPositionDirection);
		}
	}

	internal bool IsInterimSelection
	{
		get
		{
			if (TextStore != null)
			{
				return TextStore.IsInterimSelection;
			}
			return false;
		}
	}

	bool ITextSelection.IsInterimSelection => IsInterimSelection;

	internal TextPointer AnchorPosition => (TextPointer)((ITextSelection)this).AnchorPosition;

	internal TextPointer MovingPosition => (TextPointer)((ITextSelection)this).MovingPosition;

	internal CaretElement CaretElement => _caretElement;

	CaretElement ITextSelection.CaretElement => CaretElement;

	bool ITextSelection.CoversEntireContent
	{
		get
		{
			if (((ITextRange)this).Start.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text && ((ITextRange)this).End.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text && ((ITextRange)this).Start.GetNextInsertionPosition(LogicalDirection.Backward) == null)
			{
				return ((ITextRange)this).End.GetNextInsertionPosition(LogicalDirection.Forward) == null;
			}
			return false;
		}
	}

	TextEditor ITextSelection.TextEditor => _textEditor;

	ITextView ITextSelection.TextView => _textEditor.TextView;

	private ITextView TextView => ((ITextSelection)this).TextView;

	private TextStore TextStore => _textEditor.TextStore;

	private ImmComposition ImmComposition => _textEditor.ImmComposition;

	private FrameworkElement UiScope => _textEditor.UiScope;

	private ITextPointer PropertyPosition
	{
		get
		{
			ITextPointer textPointer = null;
			if (!((ITextRange)this).IsEmpty)
			{
				textPointer = TextPointerBase.GetFollowingNonMergeableInlineContentStart(((ITextRange)this).Start);
			}
			if (textPointer == null)
			{
				textPointer = ((ITextRange)this).Start;
			}
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal TextSelection(TextEditor textEditor)
		: base(textEditor.TextContainer.Start, textEditor.TextContainer.Start)
	{
		Invariant.Assert(textEditor.UiScope != null);
		_textEditor = textEditor;
		SetActivePositions(((ITextRange)this).Start, ((ITextRange)this).End);
		((ITextSelection)this).UpdateCaretAndHighlight();
	}

	void ITextRange.Select(ITextPointer anchorPosition, ITextPointer movingPosition)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			TextRangeBase.Select(this, anchorPosition, movingPosition);
			SetActivePositions(anchorPosition, movingPosition);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	void ITextRange.SelectWord(ITextPointer position)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			TextRangeBase.SelectWord(this, position);
			SetActivePositions(((ITextRange)this).Start, ((ITextRange)this).End);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	void ITextRange.SelectParagraph(ITextPointer position)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			TextRangeBase.SelectParagraph(this, position);
			SetActivePositions(position, ((ITextRange)this).End);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	void ITextRange.ApplyTypingHeuristics(bool overType)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			TextRangeBase.ApplyInitialTypingHeuristics(this);
			if (!base.IsEmpty && _textEditor.AcceptsRichContent)
			{
				SpringloadCurrentFormatting();
			}
			TextRangeBase.ApplyFinalTypingHeuristics(this, overType);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	object ITextRange.GetPropertyValue(DependencyProperty formattingProperty)
	{
		if (base.IsEmpty && TextSchema.IsCharacterProperty(formattingProperty))
		{
			return GetCurrentValue(formattingProperty);
		}
		return TextRangeBase.GetPropertyValue(this, formattingProperty);
	}

	void ITextRange.NotifyChanged(bool disableScroll, bool skipEvents)
	{
		if (TextStore != null)
		{
			TextStore.OnSelectionChanged();
		}
		if (ImmComposition != null)
		{
			ImmComposition.OnSelectionChanged();
		}
		if (!skipEvents)
		{
			TextRangeBase.NotifyChanged(this, disableScroll);
		}
		if (!disableScroll)
		{
			ITextPointer movingPosition = ((ITextSelection)this).MovingPosition;
			if (TextView != null && TextView.IsValid && !TextView.Contains(movingPosition))
			{
				movingPosition.ValidateLayout();
			}
		}
		UpdateCaretState((!disableScroll) ? CaretScrollMethod.Simple : CaretScrollMethod.None);
	}

	void ITextSelection.UpdateCaretAndHighlight()
	{
		FrameworkElement uiScope = UiScope;
		FrameworkElement ownerElement = CaretElement.GetOwnerElement(uiScope);
		bool flag = false;
		bool isBlinkEnabled = false;
		bool flag2 = false;
		if (uiScope.IsEnabled && TextView != null)
		{
			if (uiScope.IsKeyboardFocused)
			{
				flag = true;
				isBlinkEnabled = true;
				flag2 = true;
			}
			else if (uiScope.IsFocused && ((IsRootElement(FocusManager.GetFocusScope(uiScope)) && IsFocusWithinRoot()) || _textEditor.IsContextMenuOpen))
			{
				flag = true;
				isBlinkEnabled = false;
				flag2 = true;
			}
			else if (!base.IsEmpty && (bool)ownerElement.GetValue(TextBoxBase.IsInactiveSelectionHighlightEnabledProperty))
			{
				flag = true;
				isBlinkEnabled = false;
				flag2 = false;
			}
		}
		ownerElement.SetValue(TextBoxBase.IsSelectionActivePropertyKey, flag2);
		if (flag)
		{
			if (flag2)
			{
				SetThreadSelection();
			}
			EnsureCaret(isBlinkEnabled, flag2, CaretScrollMethod.None);
			Highlight();
		}
		else
		{
			ClearThreadSelection();
			DetachCaretFromVisualTree();
			Unhighlight();
		}
	}

	void ITextSelection.SetCaretToPosition(ITextPointer caretPosition, LogicalDirection direction, bool allowStopAtLineEnd, bool allowStopNearSpace)
	{
		caretPosition = caretPosition.CreatePointer(direction);
		caretPosition.MoveToInsertionPosition(direction);
		ITextPointer position = caretPosition.CreatePointer((direction != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
		if (!allowStopAtLineEnd && ((TextPointerBase.IsAtLineWrappingPosition(caretPosition, TextView) && TextPointerBase.IsAtLineWrappingPosition(position, TextView)) || TextPointerBase.IsNextToPlainLineBreak(caretPosition, LogicalDirection.Backward) || TextSchema.IsBreak(caretPosition.GetElementType(LogicalDirection.Backward))))
		{
			caretPosition.SetLogicalDirection(LogicalDirection.Forward);
		}
		else if ((caretPosition.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text || caretPosition.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) && !allowStopNearSpace)
		{
			char[] array = new char[1];
			if (caretPosition.GetPointerContext(direction) == TextPointerContext.Text && caretPosition.GetTextInRun(direction, array, 0, 1) == 1 && char.IsWhiteSpace(array[0]))
			{
				LogicalDirection logicalDirection = ((direction != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
				FlowDirection flowDirection = (FlowDirection)caretPosition.GetValue(FrameworkElement.FlowDirectionProperty);
				if (caretPosition.MoveToInsertionPosition(logicalDirection) && flowDirection == (FlowDirection)caretPosition.GetValue(FrameworkElement.FlowDirectionProperty) && (caretPosition.GetPointerContext(logicalDirection) != TextPointerContext.Text || caretPosition.GetTextInRun(logicalDirection, array, 0, 1) != 1 || !char.IsWhiteSpace(array[0])))
				{
					direction = logicalDirection;
					caretPosition.SetLogicalDirection(direction);
				}
			}
		}
		TextRangeBase.BeginChange(this);
		try
		{
			TextRangeBase.Select(this, caretPosition, caretPosition);
			Invariant.Assert(((ITextRange)this).Start.LogicalDirection == caretPosition.LogicalDirection);
			Invariant.Assert(base.IsEmpty);
			SetActivePositions(null, null);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	void ITextSelection.ExtendToPosition(ITextPointer position)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			ITextPointer anchorPosition = ((ITextSelection)this).AnchorPosition;
			TextRangeBase.Select(this, anchorPosition, position);
			SetActivePositions(anchorPosition, position);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	bool ITextSelection.ExtendToNextInsertionPosition(LogicalDirection direction)
	{
		bool result = false;
		TextRangeBase.BeginChange(this);
		try
		{
			ITextPointer anchorPosition = ((ITextSelection)this).AnchorPosition;
			ITextPointer movingPosition = ((ITextSelection)this).MovingPosition;
			ITextPointer textPointer = (base.IsTableCellRange ? TextRangeEditTables.GetNextTableCellRangeInsertionPosition(this, direction) : ((movingPosition is TextPointer && TextPointerBase.IsAtRowEnd(movingPosition)) ? TextRangeEditTables.GetNextRowEndMovingPosition(this, direction) : ((!(movingPosition is TextPointer) || !TextRangeEditTables.MovingPositionCrossesCellBoundary(this)) ? GetNextTextSegmentInsertionPosition(direction) : TextRangeEditTables.GetNextRowStartMovingPosition(this, direction))));
			if (textPointer == null && direction == LogicalDirection.Forward && movingPosition.CompareTo(movingPosition.TextContainer.End) != 0)
			{
				textPointer = movingPosition.TextContainer.End;
			}
			if (textPointer != null)
			{
				result = true;
				TextRangeBase.Select(this, anchorPosition, textPointer);
				LogicalDirection logicalDirection = ((anchorPosition.CompareTo(textPointer) > 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
				textPointer = textPointer.GetFrozenPointer(logicalDirection);
				SetActivePositions(anchorPosition, textPointer);
			}
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
		return result;
	}

	private ITextPointer GetNextTextSegmentInsertionPosition(LogicalDirection direction)
	{
		return ((ITextSelection)this).MovingPosition.GetNextInsertionPosition(direction);
	}

	bool ITextSelection.Contains(Point point)
	{
		if (((ITextRange)this).IsEmpty)
		{
			return false;
		}
		if (TextView == null || !TextView.IsValid)
		{
			return false;
		}
		bool flag = false;
		ITextPointer textPositionFromPoint = TextView.GetTextPositionFromPoint(point, snapToText: false);
		if (textPositionFromPoint != null && ((ITextRange)this).Contains(textPositionFromPoint))
		{
			textPositionFromPoint = textPositionFromPoint.GetNextInsertionPosition(textPositionFromPoint.LogicalDirection);
			if (textPositionFromPoint != null && ((ITextRange)this).Contains(textPositionFromPoint))
			{
				flag = true;
			}
		}
		if (!flag && _caretElement != null && _caretElement.SelectionGeometry != null && _caretElement.SelectionGeometry.FillContains(point))
		{
			flag = true;
		}
		return flag;
	}

	void ITextSelection.OnDetach()
	{
		((ITextSelection)this).UpdateCaretAndHighlight();
		if (_highlightLayer != null && ((ITextRange)this).Start.TextContainer.Highlights.GetLayer(typeof(TextSelection)) == _highlightLayer)
		{
			((ITextRange)this).Start.TextContainer.Highlights.RemoveLayer(_highlightLayer);
		}
		_highlightLayer = null;
		_textEditor = null;
	}

	void ITextSelection.OnTextViewUpdated()
	{
		if (UiScope.IsKeyboardFocused || UiScope.IsFocused)
		{
			_caretElement?.OnTextViewUpdated();
		}
		if (_pendingUpdateCaretStateCallback)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(UpdateCaretStateWorker), null);
		}
	}

	void ITextSelection.DetachFromVisualTree()
	{
		DetachCaretFromVisualTree();
	}

	void ITextSelection.RefreshCaret()
	{
		RefreshCaret(_textEditor, _textEditor.Selection);
	}

	void ITextSelection.OnInterimSelectionChanged(bool interimSelection)
	{
		UpdateCaretState(CaretScrollMethod.None);
	}

	void ITextSelection.SetSelectionByMouse(ITextPointer cursorPosition, Point cursorMousePoint)
	{
		if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
		{
			((ITextSelection)this).ExtendSelectionByMouse(cursorPosition, forceWordSelection: false, forceParagraphSelection: false);
		}
		else
		{
			MoveSelectionByMouse(cursorPosition, cursorMousePoint);
		}
	}

	void ITextSelection.ExtendSelectionByMouse(ITextPointer cursorPosition, bool forceWordSelection, bool forceParagraphSelection)
	{
		if (forceParagraphSelection || (_previousCursorPosition != null && cursorPosition.CompareTo(_previousCursorPosition) == 0))
		{
			return;
		}
		((ITextRange)this).BeginChange();
		try
		{
			if (BeginMouseSelectionProcess(cursorPosition))
			{
				ITextPointer anchorPosition = ((ITextSelection)this).AnchorPosition;
				IdentifyWordsOnSelectionEnds(anchorPosition, cursorPosition, forceWordSelection, out var anchorWordRange, out var cursorWordRange);
				ITextPointer frozenPointer;
				ITextPointer frozenPointer2;
				if (anchorWordRange.Start.CompareTo(cursorWordRange.Start) <= 0)
				{
					frozenPointer = anchorWordRange.Start.GetFrozenPointer(LogicalDirection.Forward);
					frozenPointer2 = cursorWordRange.End.GetFrozenPointer(LogicalDirection.Backward);
				}
				else
				{
					frozenPointer = anchorWordRange.End.GetFrozenPointer(LogicalDirection.Backward);
					frozenPointer2 = cursorWordRange.Start.GetFrozenPointer(LogicalDirection.Forward);
				}
				TextRangeBase.Select(this, frozenPointer, frozenPointer2, includeCellAtMovingPosition: true);
				SetActivePositions(anchorPosition, frozenPointer2);
				_previousCursorPosition = cursorPosition.CreatePointer();
				Invariant.Assert(((ITextRange)this).Contains(((ITextSelection)this).AnchorPosition));
			}
		}
		finally
		{
			((ITextRange)this).EndChange();
		}
	}

	private bool BeginMouseSelectionProcess(ITextPointer cursorPosition)
	{
		if (_previousCursorPosition == null)
		{
			_anchorWordRangeHasBeenCrossedOnce = false;
			_allowWordExpansionOnAnchorEnd = true;
			_reenterPosition = null;
			if (GetUIElementSelected() != null)
			{
				_previousCursorPosition = cursorPosition.CreatePointer();
				return false;
			}
		}
		return true;
	}

	private void IdentifyWordsOnSelectionEnds(ITextPointer anchorPosition, ITextPointer cursorPosition, bool forceWordSelection, out TextSegment anchorWordRange, out TextSegment cursorWordRange)
	{
		if (forceWordSelection)
		{
			anchorWordRange = TextPointerBase.GetWordRange(anchorPosition);
			cursorWordRange = TextPointerBase.GetWordRange(cursorPosition, cursorPosition.LogicalDirection);
			return;
		}
		if (!_textEditor.AutoWordSelection || ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 && (Keyboard.Modifiers & ModifierKeys.Control) != 0))
		{
			anchorWordRange = new TextSegment(anchorPosition, anchorPosition);
			cursorWordRange = new TextSegment(cursorPosition, cursorPosition);
			return;
		}
		anchorWordRange = TextPointerBase.GetWordRange(anchorPosition);
		if (_previousCursorPosition != null && ((anchorPosition.CompareTo(cursorPosition) < 0 && cursorPosition.CompareTo(_previousCursorPosition) < 0) || (_previousCursorPosition.CompareTo(cursorPosition) < 0 && cursorPosition.CompareTo(anchorPosition) < 0)))
		{
			_reenterPosition = cursorPosition.CreatePointer();
			if (_anchorWordRangeHasBeenCrossedOnce && anchorWordRange.Contains(cursorPosition))
			{
				_allowWordExpansionOnAnchorEnd = false;
			}
		}
		else if (_reenterPosition != null && !TextPointerBase.GetWordRange(_reenterPosition).Contains(cursorPosition))
		{
			_reenterPosition = null;
		}
		if (anchorWordRange.Contains(cursorPosition) || anchorWordRange.Contains(cursorPosition.GetInsertionPosition(LogicalDirection.Forward)) || anchorWordRange.Contains(cursorPosition.GetInsertionPosition(LogicalDirection.Backward)))
		{
			anchorWordRange = new TextSegment(anchorPosition, anchorPosition);
			cursorWordRange = new TextSegment(cursorPosition, cursorPosition);
			return;
		}
		_anchorWordRangeHasBeenCrossedOnce = true;
		if (!_allowWordExpansionOnAnchorEnd || TextPointerBase.IsAtWordBoundary(anchorPosition, LogicalDirection.Forward))
		{
			anchorWordRange = new TextSegment(anchorPosition, anchorPosition);
		}
		if (TextPointerBase.IsAfterLastParagraph(cursorPosition) || TextPointerBase.IsAtWordBoundary(cursorPosition, LogicalDirection.Forward))
		{
			cursorWordRange = new TextSegment(cursorPosition, cursorPosition);
		}
		else if (_reenterPosition == null)
		{
			cursorWordRange = TextPointerBase.GetWordRange(cursorPosition, cursorPosition.LogicalDirection);
		}
		else
		{
			cursorWordRange = new TextSegment(cursorPosition, cursorPosition);
		}
	}

	bool ITextSelection.ExtendToNextTableRow(LogicalDirection direction)
	{
		if (!base.IsTableCellRange)
		{
			return false;
		}
		Invariant.Assert(!base.IsEmpty);
		Invariant.Assert(_anchorPosition != null);
		Invariant.Assert(_movingPositionEdge != MovingEdge.None);
		if (!TextRangeEditTables.IsTableCellRange((TextPointer)_anchorPosition, (TextPointer)((ITextSelection)this).MovingPosition, includeCellAtMovingPosition: false, out var anchorCell, out var movingCell))
		{
			return false;
		}
		Invariant.Assert(anchorCell != null && movingCell != null);
		TableRowGroup rowGroup = movingCell.Row.RowGroup;
		TableCell tableCell = null;
		if (direction == LogicalDirection.Forward)
		{
			int num = movingCell.Row.Index + movingCell.RowSpan;
			if (num < rowGroup.Rows.Count)
			{
				tableCell = FindCellAtColumnIndex(rowGroup.Rows[num].Cells, movingCell.ColumnIndex);
			}
		}
		else
		{
			for (int num = movingCell.Row.Index - 1; num >= 0; num--)
			{
				tableCell = FindCellAtColumnIndex(rowGroup.Rows[num].Cells, movingCell.ColumnIndex);
				if (tableCell != null)
				{
					break;
				}
			}
		}
		if (tableCell != null)
		{
			ITextPointer textPointer = tableCell.ContentEnd.CreatePointer();
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
			TextRangeBase.Select(this, _anchorPosition, textPointer);
			SetActivePositions(_anchorPosition, textPointer);
			return true;
		}
		return false;
	}

	internal void SetCaretToPosition(TextPointer caretPosition, LogicalDirection direction, bool allowStopAtLineEnd, bool allowStopNearSpace)
	{
		((ITextSelection)this).SetCaretToPosition((ITextPointer)caretPosition, direction, allowStopAtLineEnd, allowStopNearSpace);
	}

	internal bool ExtendToNextInsertionPosition(LogicalDirection direction)
	{
		return ((ITextSelection)this).ExtendToNextInsertionPosition(direction);
	}

	internal static void OnInputLanguageChanged(CultureInfo cultureInfo)
	{
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		if (IsBidiInputLanguage(cultureInfo))
		{
			threadLocalStore.Bidi = true;
		}
		else
		{
			threadLocalStore.Bidi = false;
		}
		if (threadLocalStore.FocusedTextSelection != null)
		{
			((ITextSelection)threadLocalStore.FocusedTextSelection).RefreshCaret();
		}
	}

	internal bool Contains(Point point)
	{
		return ((ITextSelection)this).Contains(point);
	}

	internal override void InsertEmbeddedUIElementVirtual(FrameworkElement embeddedElement)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			base.InsertEmbeddedUIElementVirtual(embeddedElement);
			ClearSpringloadFormatting();
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal override void ApplyPropertyToTextVirtual(DependencyProperty formattingProperty, object value, bool applyToParagraphs, PropertyValueAction propertyValueAction)
	{
		if (!TextSchema.IsParagraphProperty(formattingProperty) && !TextSchema.IsCharacterProperty(formattingProperty))
		{
			return;
		}
		if (base.IsEmpty && TextSchema.IsCharacterProperty(formattingProperty) && !applyToParagraphs && formattingProperty != FrameworkElement.FlowDirectionProperty)
		{
			TextSegment autoWord = TextRangeBase.GetAutoWord(this);
			if (autoWord.IsNull)
			{
				if (_springloadFormatting == null)
				{
					_springloadFormatting = new DependencyObject();
				}
				_springloadFormatting.SetValue(formattingProperty, value);
			}
			else
			{
				new TextRange(autoWord.Start, autoWord.End).ApplyPropertyValue(formattingProperty, value);
			}
		}
		else
		{
			base.ApplyPropertyToTextVirtual(formattingProperty, value, applyToParagraphs, propertyValueAction);
			ClearSpringloadFormatting();
		}
	}

	internal override void ClearAllPropertiesVirtual()
	{
		if (base.IsEmpty)
		{
			ClearSpringloadFormatting();
			return;
		}
		TextRangeBase.BeginChange(this);
		try
		{
			base.ClearAllPropertiesVirtual();
			ClearSpringloadFormatting();
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal override void SetXmlVirtual(TextElement fragment)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			base.SetXmlVirtual(fragment);
			ClearSpringloadFormatting();
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal override void LoadVirtual(Stream stream, string dataFormat)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			base.LoadVirtual(stream, dataFormat);
			ClearSpringloadFormatting();
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal override Table InsertTableVirtual(int rowCount, int columnCount)
	{
		using (DeclareChangeBlock())
		{
			Table table = base.InsertTableVirtual(rowCount, columnCount);
			if (table != null)
			{
				TextPointer contentStart = table.RowGroups[0].Rows[0].Cells[0].ContentStart;
				SetCaretToPosition(contentStart, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: false);
			}
			return table;
		}
	}

	internal object GetCurrentValue(DependencyProperty formattingProperty)
	{
		object obj = DependencyProperty.UnsetValue;
		if (((ITextRange)this).Start is TextPointer && _springloadFormatting != null && base.IsEmpty)
		{
			obj = _springloadFormatting.ReadLocalValue(formattingProperty);
			if (obj == DependencyProperty.UnsetValue)
			{
				obj = base.Start.Parent.GetValue(formattingProperty);
			}
		}
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = PropertyPosition.GetValue(formattingProperty);
		}
		return obj;
	}

	internal void SpringloadCurrentFormatting()
	{
		if (((ITextRange)this).Start is TextPointer)
		{
			TextPointer textPointer = base.Start;
			Inline nonMergeableInlineAncestor = textPointer.GetNonMergeableInlineAncestor();
			if (nonMergeableInlineAncestor != null && base.End.GetNonMergeableInlineAncestor() != nonMergeableInlineAncestor)
			{
				textPointer = nonMergeableInlineAncestor.ElementEnd;
			}
			if (_springloadFormatting == null)
			{
				SpringloadCurrentFormatting(textPointer.Parent);
			}
		}
	}

	private void SpringloadCurrentFormatting(DependencyObject parent)
	{
		_springloadFormatting = new DependencyObject();
		if (parent == null)
		{
			return;
		}
		DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(Inline));
		DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(typeof(Span));
		DependencyObject dependencyObject = parent;
		while (dependencyObject is Inline)
		{
			if (((TextElementEditingBehaviorAttribute)Attribute.GetCustomAttribute(dependencyObject.GetType(), typeof(TextElementEditingBehaviorAttribute))).IsTypographicOnly)
			{
				for (int i = 0; i < inheritableProperties.Length; i++)
				{
					if (_springloadFormatting.ReadLocalValue(inheritableProperties[i]) == DependencyProperty.UnsetValue && inheritableProperties[i] != FrameworkElement.LanguageProperty && inheritableProperties[i] != FrameworkElement.FlowDirectionProperty && DependencyPropertyHelper.GetValueSource(dependencyObject, inheritableProperties[i]).BaseValueSource != BaseValueSource.Inherited)
					{
						object value = parent.GetValue(inheritableProperties[i]);
						_springloadFormatting.SetValue(inheritableProperties[i], value);
					}
				}
				for (int j = 0; j < noninheritableProperties.Length; j++)
				{
					if (_springloadFormatting.ReadLocalValue(noninheritableProperties[j]) == DependencyProperty.UnsetValue && noninheritableProperties[j] != TextElement.TextEffectsProperty && DependencyPropertyHelper.GetValueSource(dependencyObject, noninheritableProperties[j]).BaseValueSource != BaseValueSource.Inherited)
					{
						object value2 = parent.GetValue(noninheritableProperties[j]);
						_springloadFormatting.SetValue(noninheritableProperties[j], value2);
					}
				}
			}
			dependencyObject = ((TextElement)dependencyObject).Parent;
		}
	}

	internal void ClearSpringloadFormatting()
	{
		if (((ITextRange)this).Start is TextPointer)
		{
			_springloadFormatting = null;
			((ITextSelection)this).RefreshCaret();
		}
	}

	internal void ApplySpringloadFormatting()
	{
		if (((ITextRange)this).Start is TextPointer && !base.IsEmpty && _springloadFormatting != null)
		{
			Invariant.Assert(base.Start.LogicalDirection == LogicalDirection.Backward);
			Invariant.Assert(base.End.LogicalDirection == LogicalDirection.Forward);
			LocalValueEnumerator localValueEnumerator = _springloadFormatting.GetLocalValueEnumerator();
			while (!base.IsEmpty && localValueEnumerator.MoveNext())
			{
				LocalValueEntry current = localValueEnumerator.Current;
				Invariant.Assert(TextSchema.IsCharacterProperty(current.Property));
				ApplyPropertyValue(current.Property, current.Value);
			}
			ClearSpringloadFormatting();
		}
	}

	internal void UpdateCaretState(CaretScrollMethod caretScrollMethod)
	{
		Invariant.Assert(caretScrollMethod != CaretScrollMethod.Unset);
		if (_pendingCaretNavigation)
		{
			caretScrollMethod = CaretScrollMethod.Navigation;
			_pendingCaretNavigation = false;
		}
		if (_caretScrollMethod == CaretScrollMethod.Unset)
		{
			_caretScrollMethod = caretScrollMethod;
			if (_textEditor.TextView != null && _textEditor.TextView.IsValid)
			{
				UpdateCaretStateWorker(null);
			}
			else
			{
				_pendingUpdateCaretStateCallback = true;
			}
		}
		else if (caretScrollMethod != CaretScrollMethod.None)
		{
			_caretScrollMethod = caretScrollMethod;
		}
	}

	internal static Brush GetCaretBrush(TextEditor textEditor)
	{
		Brush brush = (Brush)textEditor.UiScope.GetValue(TextBoxBase.CaretBrushProperty);
		if (brush != null)
		{
			return brush;
		}
		object value = textEditor.UiScope.GetValue(Panel.BackgroundProperty);
		Color color = ((value == null || value == DependencyProperty.UnsetValue || !(value is SolidColorBrush)) ? SystemColors.WindowColor : ((SolidColorBrush)value).Color);
		ITextSelection selection = textEditor.Selection;
		if (selection is TextSelection)
		{
			value = ((TextSelection)selection).GetCurrentValue(TextElement.BackgroundProperty);
			if (value != null && value != DependencyProperty.UnsetValue && value is SolidColorBrush)
			{
				color = ((SolidColorBrush)value).Color;
			}
		}
		byte r = (byte)(~color.R);
		byte g = (byte)(~color.G);
		byte b = (byte)(~color.B);
		brush = new SolidColorBrush(Color.FromRgb(r, g, b));
		brush.Freeze();
		return brush;
	}

	internal static bool IsBidiInputLanguageInstalled()
	{
		bool result = false;
		int keyboardLayoutList = SafeNativeMethods.GetKeyboardLayoutList(0, null);
		if (keyboardLayoutList > 0)
		{
			nint[] array = new nint[keyboardLayoutList];
			keyboardLayoutList = SafeNativeMethods.GetKeyboardLayoutList(keyboardLayoutList, array);
			for (int i = 0; i < array.Length && i < keyboardLayoutList; i++)
			{
				if (IsBidiInputLanguage(new CultureInfo((short)array[i])))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	void ITextSelection.ValidateLayout()
	{
		((ITextSelection)this).MovingPosition.ValidateLayout();
	}

	private void SetThreadSelection()
	{
		TextEditor._ThreadLocalStore.FocusedTextSelection = this;
	}

	private void ClearThreadSelection()
	{
		if (TextEditor._ThreadLocalStore.FocusedTextSelection == this)
		{
			TextEditor._ThreadLocalStore.FocusedTextSelection = null;
		}
	}

	private void Highlight()
	{
		ITextContainer textContainer = ((ITextRange)this).Start.TextContainer;
		if (!FrameworkAppContextSwitches.UseAdornerForTextboxSelectionRendering || (!(textContainer is TextContainer) && !(textContainer is PasswordTextContainer)))
		{
			if (_highlightLayer == null)
			{
				_highlightLayer = new TextSelectionHighlightLayer(this);
			}
			if (textContainer.Highlights.GetLayer(typeof(TextSelection)) == null)
			{
				textContainer.Highlights.AddLayer(_highlightLayer);
			}
		}
	}

	private void Unhighlight()
	{
		ITextContainer textContainer = ((ITextRange)this).Start.TextContainer;
		if (textContainer.Highlights.GetLayer(typeof(TextSelection)) is TextSelectionHighlightLayer highlightLayer)
		{
			textContainer.Highlights.RemoveLayer(highlightLayer);
			Invariant.Assert(textContainer.Highlights.GetLayer(typeof(TextSelection)) == null);
		}
	}

	private void SetActivePositions(ITextPointer anchorPosition, ITextPointer movingPosition)
	{
		_previousCursorPosition = null;
		if (base.IsEmpty)
		{
			_anchorPosition = null;
			_movingPositionEdge = MovingEdge.None;
			return;
		}
		Invariant.Assert(anchorPosition != null);
		_anchorPosition = anchorPosition.GetInsertionPosition(anchorPosition.LogicalDirection);
		if (_anchorPosition.CompareTo(((ITextRange)this).Start) < 0)
		{
			_anchorPosition = ((ITextRange)this).Start.GetFrozenPointer(_anchorPosition.LogicalDirection);
		}
		else if (_anchorPosition.CompareTo(((ITextRange)this).End) > 0)
		{
			_anchorPosition = ((ITextRange)this).End.GetFrozenPointer(_anchorPosition.LogicalDirection);
		}
		_movingPositionEdge = ConvertToMovingEdge(anchorPosition, movingPosition);
		_movingPositionDirection = movingPosition.LogicalDirection;
	}

	private MovingEdge ConvertToMovingEdge(ITextPointer anchorPosition, ITextPointer movingPosition)
	{
		if (((ITextRange)this).IsEmpty)
		{
			return MovingEdge.None;
		}
		if (((ITextRange)this).TextSegments.Count < 2)
		{
			return (anchorPosition.CompareTo(movingPosition) <= 0) ? MovingEdge.End : MovingEdge.Start;
		}
		if (movingPosition.CompareTo(((ITextRange)this).Start) == 0)
		{
			return MovingEdge.Start;
		}
		if (movingPosition.CompareTo(((ITextRange)this).End) == 0)
		{
			return MovingEdge.End;
		}
		if (movingPosition.CompareTo(((ITextRange)this).TextSegments[0].End) == 0)
		{
			return MovingEdge.StartInner;
		}
		if (movingPosition.CompareTo(((ITextRange)this).TextSegments[((ITextRange)this).TextSegments.Count - 1].Start) == 0)
		{
			return MovingEdge.EndInner;
		}
		return (anchorPosition.CompareTo(movingPosition) <= 0) ? MovingEdge.End : MovingEdge.Start;
	}

	private void MoveSelectionByMouse(ITextPointer cursorPosition, Point cursorMousePoint)
	{
		if (TextView == null)
		{
			return;
		}
		Invariant.Assert(TextView.IsValid);
		ITextPointer textPointer = null;
		if (cursorPosition.GetPointerContext(cursorPosition.LogicalDirection) == TextPointerContext.EmbeddedElement)
		{
			Rect rectangleFromTextPosition = TextView.GetRectangleFromTextPosition(cursorPosition);
			if (!_textEditor.IsReadOnly && ShouldSelectEmbeddedObject(cursorPosition, cursorMousePoint, rectangleFromTextPosition))
			{
				textPointer = cursorPosition.GetNextContextPosition(cursorPosition.LogicalDirection);
			}
		}
		if (textPointer == null)
		{
			((ITextSelection)this).SetCaretToPosition(cursorPosition, cursorPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: false);
		}
		else
		{
			((ITextRange)this).Select(cursorPosition, textPointer);
		}
	}

	private bool ShouldSelectEmbeddedObject(ITextPointer cursorPosition, Point cursorMousePoint, Rect objectEdgeRect)
	{
		if (!objectEdgeRect.IsEmpty && cursorMousePoint.Y >= objectEdgeRect.Y && cursorMousePoint.Y < objectEdgeRect.Y + objectEdgeRect.Height)
		{
			FlowDirection num = (FlowDirection)TextView.RenderScope.GetValue(Block.FlowDirectionProperty);
			FlowDirection flowDirection = (FlowDirection)cursorPosition.GetValue(Block.FlowDirectionProperty);
			if (num == FlowDirection.LeftToRight)
			{
				if (flowDirection == FlowDirection.LeftToRight && ((cursorPosition.LogicalDirection == LogicalDirection.Forward && objectEdgeRect.X < cursorMousePoint.X) || (cursorPosition.LogicalDirection == LogicalDirection.Backward && cursorMousePoint.X < objectEdgeRect.X)))
				{
					return true;
				}
				if (flowDirection == FlowDirection.RightToLeft && ((cursorPosition.LogicalDirection == LogicalDirection.Forward && objectEdgeRect.X > cursorMousePoint.X) || (cursorPosition.LogicalDirection == LogicalDirection.Backward && cursorMousePoint.X > objectEdgeRect.X)))
				{
					return true;
				}
			}
			else
			{
				if (flowDirection == FlowDirection.LeftToRight && ((cursorPosition.LogicalDirection == LogicalDirection.Forward && objectEdgeRect.X > cursorMousePoint.X) || (cursorPosition.LogicalDirection == LogicalDirection.Backward && cursorMousePoint.X > objectEdgeRect.X)))
				{
					return true;
				}
				if (flowDirection == FlowDirection.RightToLeft && ((cursorPosition.LogicalDirection == LogicalDirection.Forward && objectEdgeRect.X < cursorMousePoint.X) || (cursorPosition.LogicalDirection == LogicalDirection.Backward && cursorMousePoint.X < objectEdgeRect.X)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void RefreshCaret(TextEditor textEditor, ITextSelection textSelection)
	{
		if (textSelection != null && textSelection.CaretElement != null)
		{
			object currentValue = ((TextSelection)textSelection).GetCurrentValue(TextElement.FontStyleProperty);
			bool italic = textEditor.AcceptsRichContent && currentValue != DependencyProperty.UnsetValue && (FontStyle)currentValue == FontStyles.Italic;
			textSelection.CaretElement.RefreshCaret(italic);
		}
	}

	internal void OnCaretNavigation()
	{
		_pendingCaretNavigation = true;
	}

	void ITextSelection.OnCaretNavigation()
	{
		OnCaretNavigation();
	}

	private object UpdateCaretStateWorker(object o)
	{
		_pendingUpdateCaretStateCallback = false;
		if (_textEditor == null)
		{
			return null;
		}
		TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
		CaretScrollMethod caretScrollMethod = _caretScrollMethod;
		_caretScrollMethod = CaretScrollMethod.Unset;
		CaretElement caretElement = _caretElement;
		if (caretElement == null)
		{
			return null;
		}
		if (threadLocalStore.FocusedTextSelection == null)
		{
			if (!base.IsEmpty)
			{
				caretElement.Hide();
			}
			return null;
		}
		if (_textEditor.TextView == null || !_textEditor.TextView.IsValid)
		{
			return null;
		}
		if (!VerifyAdornerLayerExists())
		{
			caretElement.Hide();
		}
		ITextPointer textPointer = IdentifyCaretPosition(this);
		if (textPointer.HasValidLayout)
		{
			bool italic = false;
			bool visible = base.IsEmpty && (!_textEditor.IsReadOnly || _textEditor.IsReadOnlyCaretVisible);
			Rect caretRectangle;
			if (!IsInterimSelection)
			{
				caretRectangle = CalculateCaretRectangle(this, textPointer);
				if (base.IsEmpty)
				{
					object propertyValue = GetPropertyValue(TextElement.FontStyleProperty);
					italic = _textEditor.AcceptsRichContent && propertyValue != DependencyProperty.UnsetValue && (FontStyle)propertyValue == FontStyles.Italic;
				}
			}
			else
			{
				caretRectangle = CalculateInterimCaretRectangle(this);
				visible = true;
			}
			Brush caretBrush = GetCaretBrush(_textEditor);
			double scrollToOriginPosition = CalculateScrollToOriginPosition(_textEditor, textPointer, caretRectangle.X);
			caretElement.Update(visible, caretRectangle, caretBrush, 1.0, italic, caretScrollMethod, scrollToOriginPosition);
		}
		if (TextView.IsValid && !TextView.RendersOwnSelection)
		{
			caretElement.UpdateSelection();
		}
		return null;
	}

	private static ITextPointer IdentifyCaretPosition(ITextSelection currentTextSelection)
	{
		ITextPointer textPointer = currentTextSelection.MovingPosition;
		if (!currentTextSelection.IsEmpty && ((textPointer.LogicalDirection == LogicalDirection.Backward && textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart) || TextPointerBase.IsAfterLastParagraph(textPointer)))
		{
			textPointer = textPointer.CreatePointer();
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward);
			textPointer.SetLogicalDirection(LogicalDirection.Forward);
		}
		if (textPointer.LogicalDirection == LogicalDirection.Backward && textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && (textPointer.GetNextInsertionPosition(LogicalDirection.Backward) == null || TextPointerBase.IsNextToAnyBreak(textPointer, LogicalDirection.Backward)))
		{
			textPointer = textPointer.CreatePointer();
			textPointer.SetLogicalDirection(LogicalDirection.Forward);
		}
		return textPointer;
	}

	private static Rect CalculateCaretRectangle(ITextSelection currentTextSelection, ITextPointer caretPosition)
	{
		Rect rawRectangleFromTextPosition = currentTextSelection.TextView.GetRawRectangleFromTextPosition(caretPosition, out var transform);
		if (rawRectangleFromTextPosition.IsEmpty)
		{
			return Rect.Empty;
		}
		rawRectangleFromTextPosition = transform.TransformBounds(rawRectangleFromTextPosition);
		rawRectangleFromTextPosition.Width = 0.0;
		if (currentTextSelection.IsEmpty)
		{
			double num = (double)currentTextSelection.GetPropertyValue(TextElement.FontSizeProperty);
			double num2 = ((FontFamily)currentTextSelection.GetPropertyValue(TextElement.FontFamilyProperty)).LineSpacing * num;
			if (num2 < rawRectangleFromTextPosition.Height)
			{
				rawRectangleFromTextPosition.Y += rawRectangleFromTextPosition.Height - num2;
				rawRectangleFromTextPosition.Height = num2;
			}
			if (!transform.IsIdentity)
			{
				Point result = new Point(rawRectangleFromTextPosition.X, rawRectangleFromTextPosition.Y);
				Point result2 = new Point(rawRectangleFromTextPosition.X, rawRectangleFromTextPosition.Y + rawRectangleFromTextPosition.Height);
				transform.TryTransform(result, out result);
				transform.TryTransform(result2, out result2);
				rawRectangleFromTextPosition.Y += rawRectangleFromTextPosition.Height - Math.Abs(result2.Y - result.Y);
				rawRectangleFromTextPosition.Height = Math.Abs(result2.Y - result.Y);
			}
		}
		return rawRectangleFromTextPosition;
	}

	private static Rect CalculateInterimCaretRectangle(ITextSelection focusedTextSelection)
	{
		Rect rectangleFromTextPosition;
		Rect rectangleFromTextPosition2;
		if ((FlowDirection)focusedTextSelection.Start.GetValue(FrameworkElement.FlowDirectionProperty) != FlowDirection.RightToLeft)
		{
			ITextPointer textPointer = focusedTextSelection.Start.CreatePointer(LogicalDirection.Forward);
			rectangleFromTextPosition = focusedTextSelection.TextView.GetRectangleFromTextPosition(textPointer);
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
			textPointer.SetLogicalDirection(LogicalDirection.Backward);
			rectangleFromTextPosition2 = focusedTextSelection.TextView.GetRectangleFromTextPosition(textPointer);
		}
		else
		{
			ITextPointer textPointer = focusedTextSelection.End.CreatePointer(LogicalDirection.Backward);
			rectangleFromTextPosition = focusedTextSelection.TextView.GetRectangleFromTextPosition(textPointer);
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward);
			textPointer.SetLogicalDirection(LogicalDirection.Forward);
			rectangleFromTextPosition2 = focusedTextSelection.TextView.GetRectangleFromTextPosition(textPointer);
		}
		if (!rectangleFromTextPosition.IsEmpty && !rectangleFromTextPosition2.IsEmpty && rectangleFromTextPosition2.Left > rectangleFromTextPosition.Left)
		{
			rectangleFromTextPosition.Width = rectangleFromTextPosition2.Left - rectangleFromTextPosition.Left;
		}
		return rectangleFromTextPosition;
	}

	private static double CalculateScrollToOriginPosition(TextEditor textEditor, ITextPointer caretPosition, double horizontalCaretPosition)
	{
		double result = double.NaN;
		if (textEditor.UiScope is TextBoxBase)
		{
			double viewportWidth = ((TextBoxBase)textEditor.UiScope).ViewportWidth;
			double extentWidth = ((TextBoxBase)textEditor.UiScope).ExtentWidth;
			if (viewportWidth != 0.0 && extentWidth != 0.0 && viewportWidth < extentWidth)
			{
				bool flag = false;
				if (horizontalCaretPosition < 0.0 || horizontalCaretPosition >= viewportWidth)
				{
					flag = true;
				}
				if (flag)
				{
					result = 0.0;
					FlowDirection flowDirection = (FlowDirection)textEditor.UiScope.GetValue(FrameworkElement.FlowDirectionProperty);
					Block block = ((caretPosition is TextPointer) ? ((TextPointer)caretPosition).ParagraphOrBlockUIContainer : null);
					if (block != null)
					{
						FlowDirection flowDirection2 = block.FlowDirection;
						if (flowDirection != flowDirection2)
						{
							result = extentWidth;
						}
					}
					result -= ((TextBoxBase)textEditor.UiScope).HorizontalOffset;
				}
			}
		}
		return result;
	}

	private CaretElement EnsureCaret(bool isBlinkEnabled, bool isSelectionActive, CaretScrollMethod scrollMethod)
	{
		_ = TextEditor._ThreadLocalStore;
		if (_caretElement == null)
		{
			_caretElement = new CaretElement(_textEditor, isBlinkEnabled);
			_caretElement.IsSelectionActive = isSelectionActive;
			if (IsBidiInputLanguage(InputLanguageManager.Current.CurrentInputLanguage))
			{
				TextEditor._ThreadLocalStore.Bidi = true;
			}
			else
			{
				TextEditor._ThreadLocalStore.Bidi = false;
			}
		}
		else
		{
			_caretElement.IsSelectionActive = isSelectionActive;
			_caretElement.SetBlinking(isBlinkEnabled);
		}
		UpdateCaretState(scrollMethod);
		return _caretElement;
	}

	private bool VerifyAdornerLayerExists()
	{
		DependencyObject dependencyObject = TextView.RenderScope;
		while (dependencyObject != _textEditor.UiScope && dependencyObject != null)
		{
			if (dependencyObject is AdornerDecorator || dependencyObject is ScrollContentPresenter)
			{
				return true;
			}
			dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		}
		return false;
	}

	private static bool IsBidiInputLanguage(CultureInfo cultureInfo)
	{
		bool result = false;
		string text = new string(new char[16]);
		if (MS.Win32.UnsafeNativeMethods.GetLocaleInfoW(cultureInfo.LCID, 88, text, 16) != 0 && (text[7] & 0x800) != 0)
		{
			result = true;
		}
		return result;
	}

	private static TableCell FindCellAtColumnIndex(TableCellCollection cells, int columnIndex)
	{
		for (int i = 0; i < cells.Count; i++)
		{
			TableCell tableCell = cells[i];
			int columnIndex2 = tableCell.ColumnIndex;
			int num = columnIndex2 + tableCell.ColumnSpan - 1;
			if (columnIndex2 <= columnIndex && columnIndex <= num)
			{
				return tableCell;
			}
		}
		return null;
	}

	private static bool IsRootElement(DependencyObject element)
	{
		return GetParentElement(element) == null;
	}

	private bool IsFocusWithinRoot()
	{
		DependencyObject dependencyObject = UiScope;
		for (DependencyObject dependencyObject2 = UiScope; dependencyObject2 != null; dependencyObject2 = GetParentElement(dependencyObject))
		{
			dependencyObject = dependencyObject2;
		}
		if (dependencyObject is UIElement && ((UIElement)dependencyObject).IsKeyboardFocusWithin)
		{
			return true;
		}
		return false;
	}

	private static DependencyObject GetParentElement(DependencyObject element)
	{
		DependencyObject dependencyObject;
		if (!(element is FrameworkElement) && !(element is FrameworkContentElement))
		{
			dependencyObject = ((!(element is Visual)) ? null : VisualTreeHelper.GetParent(element));
		}
		else
		{
			dependencyObject = LogicalTreeHelper.GetParent(element);
			if (dependencyObject == null && element is FrameworkElement)
			{
				dependencyObject = ((FrameworkElement)element).TemplatedParent;
				if (dependencyObject == null && element is Visual)
				{
					dependencyObject = VisualTreeHelper.GetParent(element);
				}
			}
		}
		return dependencyObject;
	}

	private void DetachCaretFromVisualTree()
	{
		if (_caretElement != null)
		{
			_caretElement.DetachFromView();
			_caretElement = null;
		}
	}
}
