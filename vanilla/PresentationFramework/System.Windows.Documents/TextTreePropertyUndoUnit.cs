using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreePropertyUndoUnit : TextTreeUndoUnit
{
	private readonly PropertyRecord _propertyRecord;

	internal TextTreePropertyUndoUnit(TextContainer tree, int symbolOffset, PropertyRecord propertyRecord)
		: base(tree, symbolOffset)
	{
		_propertyRecord = propertyRecord;
	}

	public override void DoCore()
	{
		VerifyTreeContentHashCode();
		TextPointer textPointer = new TextPointer(base.TextContainer, base.SymbolOffset, LogicalDirection.Forward);
		Invariant.Assert(textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart, "TextTree undo unit out of sync with TextTree.");
		if (_propertyRecord.Value != DependencyProperty.UnsetValue)
		{
			base.TextContainer.SetValue(textPointer, _propertyRecord.Property, _propertyRecord.Value);
		}
		else
		{
			textPointer.Parent.ClearValue(_propertyRecord.Property);
		}
	}
}
