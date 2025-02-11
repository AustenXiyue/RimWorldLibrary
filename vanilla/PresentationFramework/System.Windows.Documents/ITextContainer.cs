using MS.Internal.Documents;

namespace System.Windows.Documents;

internal interface ITextContainer
{
	bool IsReadOnly { get; }

	ITextPointer Start { get; }

	ITextPointer End { get; }

	DependencyObject Parent { get; }

	Highlights Highlights { get; }

	ITextSelection TextSelection { get; set; }

	UndoManager UndoManager { get; }

	ITextView TextView { get; set; }

	int SymbolCount { get; }

	int IMECharCount { get; }

	uint Generation { get; }

	event EventHandler Changing;

	event TextContainerChangeEventHandler Change;

	event TextContainerChangedEventHandler Changed;

	void BeginChange();

	void BeginChangeNoUndo();

	void EndChange();

	void EndChange(bool skipEvents);

	ITextPointer CreatePointerAtOffset(int offset, LogicalDirection direction);

	ITextPointer CreatePointerAtCharOffset(int charOffset, LogicalDirection direction);

	ITextPointer CreateDynamicTextPointer(StaticTextPointer position, LogicalDirection direction);

	StaticTextPointer CreateStaticPointerAtOffset(int offset);

	TextPointerContext GetPointerContext(StaticTextPointer pointer, LogicalDirection direction);

	int GetOffsetToPosition(StaticTextPointer position1, StaticTextPointer position2);

	int GetTextInRun(StaticTextPointer position, LogicalDirection direction, char[] textBuffer, int startIndex, int count);

	object GetAdjacentElement(StaticTextPointer position, LogicalDirection direction);

	DependencyObject GetParent(StaticTextPointer position);

	StaticTextPointer CreatePointer(StaticTextPointer position, int offset);

	StaticTextPointer GetNextContextPosition(StaticTextPointer position, LogicalDirection direction);

	int CompareTo(StaticTextPointer position1, StaticTextPointer position2);

	int CompareTo(StaticTextPointer position1, ITextPointer position2);

	object GetValue(StaticTextPointer position, DependencyProperty formattingProperty);
}
