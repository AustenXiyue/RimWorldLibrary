using MS.Internal.Documents;

namespace System.Windows.Documents;

internal class ColumnResizeUndoUnit : ParentUndoUnit
{
	private TextContainer _textContainer;

	private double[] _columnWidths;

	private int _cpTable;

	private int _columnIndex;

	private double _resizeAmount;

	internal ColumnResizeUndoUnit(TextPointer textPointerTable, int columnIndex, double[] columnWidths, double resizeAmount)
		: base("ColumnResize")
	{
		_textContainer = textPointerTable.TextContainer;
		_cpTable = _textContainer.Start.GetOffsetToPosition(textPointerTable);
		_columnWidths = columnWidths;
		_columnIndex = columnIndex;
		_resizeAmount = resizeAmount;
	}

	public override void Do()
	{
		UndoManager undoManager = base.TopContainer as UndoManager;
		IParentUndoUnit parentUndoUnit = null;
		TextPointer textPointer = new TextPointer(_textContainer.Start, _cpTable, LogicalDirection.Forward);
		Table table = (Table)textPointer.Parent;
		_columnWidths[_columnIndex] -= _resizeAmount;
		if (_columnIndex < table.ColumnCount - 1)
		{
			_columnWidths[_columnIndex + 1] += _resizeAmount;
		}
		if (undoManager != null && undoManager.IsEnabled)
		{
			parentUndoUnit = new ColumnResizeUndoUnit(textPointer, _columnIndex, _columnWidths, 0.0 - _resizeAmount);
			undoManager.Open(parentUndoUnit);
		}
		TextRangeEditTables.EnsureTableColumnsAreFixedSize(table, _columnWidths);
		if (parentUndoUnit != null)
		{
			undoManager.Close(parentUndoUnit, UndoCloseAction.Commit);
		}
	}
}
