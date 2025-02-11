namespace System.Windows.Documents;

internal abstract class HighlightLayer
{
	internal abstract Type OwnerType { get; }

	internal abstract event HighlightChangedEventHandler Changed;

	internal abstract object GetHighlightValue(StaticTextPointer textPosition, LogicalDirection direction);

	internal abstract bool IsContentHighlighted(StaticTextPointer textPosition, LogicalDirection direction);

	internal abstract StaticTextPointer GetNextChangePosition(StaticTextPointer textPosition, LogicalDirection direction);
}
