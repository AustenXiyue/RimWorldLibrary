namespace System.Windows.Documents;

internal interface ITextSelection : ITextRange
{
	TextEditor TextEditor { get; }

	ITextView TextView { get; }

	bool IsInterimSelection { get; }

	ITextPointer AnchorPosition { get; }

	ITextPointer MovingPosition { get; }

	CaretElement CaretElement { get; }

	bool CoversEntireContent { get; }

	void SetCaretToPosition(ITextPointer caretPosition, LogicalDirection direction, bool allowStopAtLineEnd, bool allowStopNearSpace);

	void ExtendToPosition(ITextPointer textPosition);

	bool ExtendToNextInsertionPosition(LogicalDirection direction);

	bool Contains(Point point);

	void OnDetach();

	void UpdateCaretAndHighlight();

	void OnTextViewUpdated();

	void DetachFromVisualTree();

	void RefreshCaret();

	void OnInterimSelectionChanged(bool interimSelection);

	void SetSelectionByMouse(ITextPointer cursorPosition, Point cursorMousePoint);

	void ExtendSelectionByMouse(ITextPointer cursorPosition, bool forceWordSelection, bool forceParagraphSelection);

	bool ExtendToNextTableRow(LogicalDirection direction);

	void OnCaretNavigation();

	void ValidateLayout();
}
