using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeInsertElementUndoUnit : TextTreeUndoUnit
{
	private readonly bool _deep;

	internal TextTreeInsertElementUndoUnit(TextContainer tree, int symbolOffset, bool deep)
		: base(tree, symbolOffset)
	{
		_deep = deep;
	}

	public override void DoCore()
	{
		VerifyTreeContentHashCode();
		TextPointer textPointer = new TextPointer(base.TextContainer, base.SymbolOffset, LogicalDirection.Forward);
		Invariant.Assert(textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart, "TextTree undo unit out of sync with TextTree.");
		TextElement adjacentElementFromOuterPosition = textPointer.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
		if (_deep)
		{
			TextPointer endPosition = new TextPointer(base.TextContainer, adjacentElementFromOuterPosition.TextElementNode, ElementEdge.AfterEnd);
			base.TextContainer.DeleteContentInternal(textPointer, endPosition);
		}
		else
		{
			base.TextContainer.ExtractElementInternal(adjacentElementFromOuterPosition);
		}
	}
}
