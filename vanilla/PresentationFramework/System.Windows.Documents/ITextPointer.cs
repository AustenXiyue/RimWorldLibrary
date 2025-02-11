namespace System.Windows.Documents;

internal interface ITextPointer
{
	ITextContainer TextContainer { get; }

	bool HasValidLayout { get; }

	bool IsAtCaretUnitBoundary { get; }

	LogicalDirection LogicalDirection { get; }

	Type ParentType { get; }

	bool IsAtInsertionPosition { get; }

	bool IsFrozen { get; }

	int Offset { get; }

	int CharOffset { get; }

	ITextPointer CreatePointer();

	StaticTextPointer CreateStaticPointer();

	ITextPointer CreatePointer(int offset);

	ITextPointer CreatePointer(LogicalDirection gravity);

	ITextPointer CreatePointer(int offset, LogicalDirection gravity);

	void SetLogicalDirection(LogicalDirection direction);

	int CompareTo(ITextPointer position);

	int CompareTo(StaticTextPointer position);

	bool HasEqualScope(ITextPointer position);

	TextPointerContext GetPointerContext(LogicalDirection direction);

	int GetOffsetToPosition(ITextPointer position);

	int GetTextRunLength(LogicalDirection direction);

	string GetTextInRun(LogicalDirection direction);

	int GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count);

	object GetAdjacentElement(LogicalDirection direction);

	void MoveToPosition(ITextPointer position);

	int MoveByOffset(int offset);

	bool MoveToNextContextPosition(LogicalDirection direction);

	ITextPointer GetNextContextPosition(LogicalDirection direction);

	bool MoveToInsertionPosition(LogicalDirection direction);

	ITextPointer GetInsertionPosition(LogicalDirection direction);

	ITextPointer GetFormatNormalizedPosition(LogicalDirection direction);

	bool MoveToNextInsertionPosition(LogicalDirection direction);

	ITextPointer GetNextInsertionPosition(LogicalDirection direction);

	void MoveToElementEdge(ElementEdge edge);

	int MoveToLineBoundary(int count);

	Rect GetCharacterRect(LogicalDirection direction);

	void Freeze();

	ITextPointer GetFrozenPointer(LogicalDirection logicalDirection);

	void InsertTextInRun(string textData);

	void DeleteContentToPosition(ITextPointer limit);

	Type GetElementType(LogicalDirection direction);

	object GetValue(DependencyProperty formattingProperty);

	object ReadLocalValue(DependencyProperty formattingProperty);

	LocalValueEnumerator GetLocalValueEnumerator();

	bool ValidateLayout();
}
