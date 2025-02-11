namespace System.Windows.Documents;

internal class TextTreeExtractElementUndoUnit : TextTreeUndoUnit
{
	private readonly int _symbolCount;

	private readonly Type _type;

	private readonly PropertyRecord[] _localValues;

	private readonly ResourceDictionary _resources;

	private readonly TableColumn[] _columns;

	internal TextTreeExtractElementUndoUnit(TextContainer tree, TextTreeTextElementNode elementNode)
		: base(tree, elementNode.GetSymbolOffset(tree.Generation))
	{
		_symbolCount = elementNode.SymbolCount;
		_type = elementNode.TextElement.GetType();
		_localValues = TextTreeUndoUnit.GetPropertyRecordArray(elementNode.TextElement);
		_resources = elementNode.TextElement.Resources;
		if (elementNode.TextElement is Table)
		{
			_columns = TextTreeDeleteContentUndoUnit.SaveColumns((Table)elementNode.TextElement);
		}
	}

	public override void DoCore()
	{
		VerifyTreeContentHashCode();
		TextPointer start = new TextPointer(base.TextContainer, base.SymbolOffset, LogicalDirection.Forward);
		TextPointer textPointer = new TextPointer(base.TextContainer, base.SymbolOffset + _symbolCount - 2, LogicalDirection.Forward);
		TextElement textElement = (TextElement)Activator.CreateInstance(_type);
		textElement.Reposition(start, textPointer);
		textElement.Resources = _resources;
		textPointer.MoveToNextContextPosition(LogicalDirection.Backward);
		base.TextContainer.SetValues(textPointer, TextTreeUndoUnit.ArrayToLocalValueEnumerator(_localValues));
		if (textElement is Table)
		{
			TextTreeDeleteContentUndoUnit.RestoreColumns((Table)textElement, _columns);
		}
	}
}
