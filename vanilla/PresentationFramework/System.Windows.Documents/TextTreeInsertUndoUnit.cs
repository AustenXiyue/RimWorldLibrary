using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeInsertUndoUnit : TextTreeUndoUnit
{
	private readonly int _symbolCount;

	internal TextTreeInsertUndoUnit(TextContainer tree, int symbolOffset, int symbolCount)
		: base(tree, symbolOffset)
	{
		Invariant.Assert(symbolCount > 0, "Creating no-op insert undo unit!");
		_symbolCount = symbolCount;
	}

	public override void DoCore()
	{
		VerifyTreeContentHashCode();
		TextPointer startPosition = new TextPointer(base.TextContainer, base.SymbolOffset, LogicalDirection.Forward);
		TextPointer endPosition = new TextPointer(base.TextContainer, base.SymbolOffset + _symbolCount, LogicalDirection.Forward);
		base.TextContainer.DeleteContentInternal(startPosition, endPosition);
	}
}
