using System.Collections.Generic;
using System.Windows.Documents.Internal;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.PtsHost;

namespace System.Windows.Documents;

internal static class TextRangeEditTables
{
	internal class TableColumnResizeInfo
	{
		private Rect _columnRect;

		private double _tableAutofitWidth;

		private double[] _columnWidths;

		private Table _table;

		private int _columnIndex;

		private double _dxl;

		private double _dxr;

		private ColumnResizeAdorner _tableColResizeAdorner;

		internal double LeftDragMax => _dxl;

		internal double RightDragMax => _dxr;

		internal Table Table => _table;

		internal TableColumnResizeInfo(ITextView textView, Table table, int columnIndex, Rect columnRect, double tableAutofitWidth, double[] columnWidths)
		{
			Invariant.Assert(table != null, "null check: table");
			Invariant.Assert(columnIndex >= 0 && columnIndex < table.ColumnCount, "ColumnIndex validity check");
			_table = table;
			_columnIndex = columnIndex;
			_columnRect = columnRect;
			_columnWidths = columnWidths;
			_tableAutofitWidth = tableAutofitWidth;
			_dxl = _columnWidths[columnIndex];
			if (columnIndex == table.ColumnCount - 1)
			{
				_dxr = _tableAutofitWidth;
				for (int i = 0; i < table.ColumnCount; i++)
				{
					_dxr -= _columnWidths[i] + table.InternalCellSpacing;
				}
				_dxr = Math.Max(_dxr, 0.0);
			}
			else
			{
				_dxr = _columnWidths[columnIndex + 1];
			}
			_tableColResizeAdorner = new ColumnResizeAdorner(textView.RenderScope);
			_tableColResizeAdorner.Initialize(textView.RenderScope, _columnRect.Left + _columnRect.Width / 2.0, _columnRect.Top, _columnRect.Height);
		}

		internal void UpdateAdorner(Point mouseMovePoint)
		{
			if (_tableColResizeAdorner != null)
			{
				double x = mouseMovePoint.X;
				x = Math.Max(x, _columnRect.Left - LeftDragMax);
				x = Math.Min(x, _columnRect.Right + RightDragMax);
				_tableColResizeAdorner.Update(x);
			}
		}

		internal void ResizeColumn(Point mousePoint)
		{
			double val = mousePoint.X - (_columnRect.X + _columnRect.Width / 2.0);
			val = Math.Max(val, 0.0 - LeftDragMax);
			val = Math.Min(val, RightDragMax);
			int columnIndex = _columnIndex;
			Table table = Table;
			Invariant.Assert(table != null, "Table is not expected to be null");
			Invariant.Assert(table.ColumnCount > 0, "ColumnCount is expected to be > 0");
			_columnWidths[columnIndex] += val;
			if (columnIndex < table.ColumnCount - 1)
			{
				_columnWidths[columnIndex + 1] -= val;
			}
			EnsureTableColumnsAreFixedSize(table, _columnWidths);
			UndoManager undoManager = table.TextContainer.UndoManager;
			if (undoManager != null && undoManager.IsEnabled)
			{
				IParentUndoUnit unit = new ColumnResizeUndoUnit(table.ContentStart, columnIndex, _columnWidths, val);
				undoManager.Open(unit);
				undoManager.Close(unit, UndoCloseAction.Commit);
			}
			DisposeAdorner();
		}

		internal void DisposeAdorner()
		{
			if (_tableColResizeAdorner != null)
			{
				_tableColResizeAdorner.Uninitialize();
				_tableColResizeAdorner = null;
			}
		}
	}

	internal static bool GetColumnRange(ITextRange range, Table table, out int firstColumnIndex, out int lastColumnIndex)
	{
		firstColumnIndex = -1;
		lastColumnIndex = -1;
		if (range == null || !range.IsTableCellRange)
		{
			return false;
		}
		if (!(range.Start is TextPointer))
		{
			return false;
		}
		if (table != GetTableFromPosition((TextPointer)range.TextSegments[0].Start))
		{
			return false;
		}
		TableCell tableCellFromPosition = GetTableCellFromPosition((TextPointer)range.TextSegments[0].Start);
		if (tableCellFromPosition == null)
		{
			return false;
		}
		TextPointer obj = (TextPointer)range.TextSegments[0].End.GetNextInsertionPosition(LogicalDirection.Backward);
		Invariant.Assert(obj != null, "lastCellPointer cannot be null here. Even empty table cells have a potential insertion position.");
		TableCell tableCellFromPosition2 = GetTableCellFromPosition(obj);
		if (tableCellFromPosition2 == null)
		{
			return false;
		}
		if (tableCellFromPosition.ColumnIndex < 0 || tableCellFromPosition2.ColumnIndex < 0)
		{
			return false;
		}
		firstColumnIndex = tableCellFromPosition.ColumnIndex;
		lastColumnIndex = tableCellFromPosition2.ColumnIndex + tableCellFromPosition2.ColumnSpan - 1;
		Invariant.Assert(firstColumnIndex <= lastColumnIndex, "expecting: firstColumnIndex <= lastColumnIndex. Actual values: " + firstColumnIndex + " and " + lastColumnIndex);
		return true;
	}

	internal static Table GetTableFromPosition(TextPointer position)
	{
		TextElement textElement = position.Parent as TextElement;
		while (textElement != null && !(textElement is Table))
		{
			textElement = textElement.Parent as TextElement;
		}
		return textElement as Table;
	}

	private static TableRow GetTableRowFromPosition(TextPointer position)
	{
		TextElement textElement = position.Parent as TextElement;
		while (textElement != null && !(textElement is TableRow))
		{
			textElement = textElement.Parent as TextElement;
		}
		return textElement as TableRow;
	}

	internal static TableCell GetTableCellFromPosition(TextPointer position)
	{
		TextElement textElement = position.Parent as TextElement;
		while (textElement != null && !(textElement is TableCell))
		{
			textElement = textElement.Parent as TextElement;
		}
		return textElement as TableCell;
	}

	internal static bool IsTableStructureCrossed(ITextPointer anchorPosition, ITextPointer movingPosition)
	{
		if (!(anchorPosition is TextPointer) || !(movingPosition is TextPointer))
		{
			return false;
		}
		TableCell anchorCell;
		TableCell movingCell;
		TableRow anchorRow;
		TableRow movingRow;
		TableRowGroup anchorRowGroup;
		TableRowGroup movingRowGroup;
		Table anchorTable;
		Table movingTable;
		return IdentifyTableElements((TextPointer)anchorPosition, (TextPointer)movingPosition, includeCellAtMovingPosition: true, out anchorCell, out movingCell, out anchorRow, out movingRow, out anchorRowGroup, out movingRowGroup, out anchorTable, out movingTable);
	}

	internal static bool IsTableCellRange(TextPointer anchorPosition, TextPointer movingPosition, bool includeCellAtMovingPosition, out TableCell anchorCell, out TableCell movingCell)
	{
		if (!IdentifyTableElements(anchorPosition, movingPosition, includeCellAtMovingPosition, out anchorCell, out movingCell, out var _, out var _, out var _, out var _, out var _, out var _))
		{
			return false;
		}
		if (anchorCell != null)
		{
			return movingCell != null;
		}
		return false;
	}

	internal static List<TextSegment> BuildTableRange(TextPointer anchorPosition, TextPointer movingPosition, bool includeCellAtMovingPosition, out bool isTableCellRange)
	{
		if (!IdentifyTableElements(anchorPosition, movingPosition, includeCellAtMovingPosition, out var anchorCell, out var movingCell, out var anchorRow, out var movingRow, out var anchorRowGroup, out var movingRowGroup, out var anchorTable, out var movingTable))
		{
			isTableCellRange = false;
			return null;
		}
		if (anchorCell != null && movingCell != null)
		{
			isTableCellRange = true;
			return BuildCellSelection(anchorCell, movingCell);
		}
		if (anchorRow != null || movingRow != null || anchorRowGroup != null || movingRowGroup != null || anchorTable != null || movingTable != null)
		{
			isTableCellRange = false;
			return BuildCrossTableSelection(anchorPosition, movingPosition, anchorRow, movingRow);
		}
		isTableCellRange = false;
		return null;
	}

	private static List<TextSegment> BuildCellSelection(TableCell anchorCell, TableCell movingCell)
	{
		TableRowGroup rowGroup = anchorCell.Row.RowGroup;
		int num = Math.Min(anchorCell.Row.Index, movingCell.Row.Index);
		int num2 = Math.Max(anchorCell.Row.Index + anchorCell.RowSpan - 1, movingCell.Row.Index + movingCell.RowSpan - 1);
		int num3 = Math.Min(anchorCell.ColumnIndex, movingCell.ColumnIndex);
		int num4 = Math.Max(anchorCell.ColumnIndex + anchorCell.ColumnSpan - 1, movingCell.ColumnIndex + movingCell.ColumnSpan - 1);
		List<TextSegment> list = new List<TextSegment>(num2 - num + 1);
		for (int i = num; i <= num2 && i < rowGroup.Rows.Count; i++)
		{
			TableCellCollection cells = rowGroup.Rows[i].Cells;
			TableCell tableCell = null;
			TableCell tableCell2 = null;
			for (int j = 0; j < cells.Count; j++)
			{
				TableCell tableCell3 = cells[j];
				if (num3 <= tableCell3.ColumnIndex && tableCell3.ColumnIndex + tableCell3.ColumnSpan - 1 <= num4)
				{
					if (tableCell == null)
					{
						tableCell = tableCell3;
					}
					tableCell2 = tableCell3;
				}
			}
			if (tableCell != null && tableCell2 != null)
			{
				Invariant.Assert(tableCell.Row == tableCell2.Row, "Inconsistent Rows for segmentStartCell and segmentEndCell");
				Invariant.Assert(tableCell.Index <= tableCell2.Index, "Index of segmentStartCell must be <= index of segentEndCell");
				list.Add(NewNormalizedCellSegment(tableCell, tableCell2));
			}
		}
		return list;
	}

	private static List<TextSegment> BuildCrossTableSelection(TextPointer anchorPosition, TextPointer movingPosition, TableRow anchorRow, TableRow movingRow)
	{
		List<TextSegment> list = new List<TextSegment>(1);
		if (anchorPosition.CompareTo(movingPosition) < 0)
		{
			list.Add(NewNormalizedTextSegment((anchorRow != null) ? anchorRow.ContentStart : anchorPosition, (movingRow != null) ? movingRow.ContentEnd : movingPosition));
		}
		else
		{
			list.Add(NewNormalizedTextSegment((movingRow != null) ? movingRow.ContentStart : movingPosition, (anchorRow != null) ? anchorRow.ContentEnd : anchorPosition));
		}
		return list;
	}

	internal static void IdentifyValidBoundaries(ITextRange range, out ITextPointer start, out ITextPointer end)
	{
		Invariant.Assert(range._IsTableCellRange, "Range must be in table cell range state");
		List<TextSegment> textSegments = range._TextSegments;
		start = null;
		end = null;
		for (int i = 0; i < textSegments.Count; i++)
		{
			TextSegment textSegment = textSegments[i];
			if (textSegment.Start.CompareTo(textSegment.End) != 0)
			{
				if (start == null)
				{
					start = textSegment.Start;
				}
				end = textSegment.End;
			}
		}
		if (start == null)
		{
			start = textSegments[0].Start;
			end = textSegments[textSegments.Count - 1].End;
		}
	}

	internal static TextPointer GetNextTableCellRangeInsertionPosition(TextSelection selection, LogicalDirection direction)
	{
		Invariant.Assert(selection.IsTableCellRange, "TextSelection call this method only if selection is in TableCellRange state");
		TextPointer textPointer = selection.MovingPosition;
		if (IsTableCellRange(selection.AnchorPosition, textPointer, includeCellAtMovingPosition: false, out var anchorCell, out var movingCell))
		{
			Invariant.Assert(anchorCell != null && movingCell != null, "anchorCell != null && movingCell != null");
			Invariant.Assert(anchorCell.Row.RowGroup == movingCell.Row.RowGroup, "anchorCell.Row.RowGroup == movingCell.Row.RowGroup");
			if (direction == LogicalDirection.Backward && movingCell == anchorCell)
			{
				textPointer = anchorCell.ContentEnd.GetInsertionPosition();
			}
			else if (direction == LogicalDirection.Forward && ((movingCell.Row == anchorCell.Row && movingCell.Index + 1 == anchorCell.Index) || (anchorCell.Index == 0 && movingCell.Index == movingCell.Row.Cells.Count - 1 && movingCell.Row.Index + 1 == anchorCell.Row.Index)))
			{
				textPointer = anchorCell.ContentStart.GetInsertionPosition();
			}
			else
			{
				TableRow row = movingCell.Row;
				TableCellCollection cells = row.Cells;
				TableRowCollection rows = row.RowGroup.Rows;
				if (direction == LogicalDirection.Forward)
				{
					if (movingCell.Index + 1 < cells.Count)
					{
						movingCell = cells[movingCell.Index + 1];
					}
					else
					{
						int i;
						for (i = row.Index + 1; i < rows.Count && rows[i].Cells.Count == 0; i++)
						{
						}
						movingCell = ((i >= rows.Count) ? null : rows[i].Cells[0]);
					}
				}
				else if (movingCell.Index > 0)
				{
					movingCell = cells[movingCell.Index - 1];
				}
				else
				{
					int num = row.Index - 1;
					while (num >= 0 && rows[num].Cells.Count == 0)
					{
						num--;
					}
					movingCell = ((num < 0) ? null : rows[num].Cells[rows[num].Cells.Count - 1]);
				}
				if (movingCell != null)
				{
					textPointer = ((movingCell.ColumnIndex < anchorCell.ColumnIndex) ? movingCell.ContentStart.GetInsertionPosition() : movingCell.ContentEnd.GetInsertionPosition().GetNextInsertionPosition(LogicalDirection.Forward));
				}
				else
				{
					textPointer = ((direction != LogicalDirection.Forward) ? anchorCell.Table.ContentStart : anchorCell.Table.ContentEnd);
					textPointer = textPointer.GetNextInsertionPosition(direction);
				}
			}
		}
		return textPointer;
	}

	internal static TextPointer GetNextRowEndMovingPosition(TextSelection selection, LogicalDirection direction)
	{
		Invariant.Assert(!selection.IsTableCellRange);
		Invariant.Assert(TextPointerBase.IsAtRowEnd(selection.MovingPosition));
		TableRow tableRow = (TableRow)selection.MovingPosition.Parent;
		if (direction != LogicalDirection.Forward)
		{
			return tableRow.ContentStart.GetNextInsertionPosition(LogicalDirection.Backward);
		}
		return tableRow.ContentEnd.GetNextInsertionPosition(LogicalDirection.Forward);
	}

	internal static bool MovingPositionCrossesCellBoundary(TextSelection selection)
	{
		Invariant.Assert(((ITextRange)selection).Start is TextPointer);
		TableCell tableCellFromPosition = GetTableCellFromPosition(selection.MovingPosition);
		if (tableCellFromPosition != null)
		{
			return !tableCellFromPosition.Contains(selection.AnchorPosition);
		}
		return false;
	}

	internal static TextPointer GetNextRowStartMovingPosition(TextSelection selection, LogicalDirection direction)
	{
		Invariant.Assert(((ITextRange)selection).Start is TextPointer);
		Invariant.Assert(!selection.IsTableCellRange);
		TableCell tableCellFromPosition = GetTableCellFromPosition(selection.MovingPosition);
		Invariant.Assert(tableCellFromPosition != null);
		TableRow row = tableCellFromPosition.Row;
		if (direction != LogicalDirection.Forward)
		{
			return row.ContentStart.GetNextInsertionPosition(LogicalDirection.Backward);
		}
		return row.ContentEnd.GetNextInsertionPosition(LogicalDirection.Forward);
	}

	internal static Table InsertTable(TextPointer insertionPosition, int rowCount, int columnCount)
	{
		for (TextElement textElement = insertionPosition.Parent as TextElement; textElement != null; textElement = textElement.Parent as TextElement)
		{
			if (textElement is List || (textElement is Inline && !TextSchema.IsMergeableInline(textElement.GetType())))
			{
				return null;
			}
		}
		insertionPosition = EnsureInsertionPosition(insertionPosition);
		Paragraph paragraph = insertionPosition.Paragraph;
		if (paragraph == null)
		{
			return null;
		}
		insertionPosition = insertionPosition.InsertParagraphBreak();
		paragraph = insertionPosition.Paragraph;
		Invariant.Assert(paragraph != null, "Expecting non-null paragraph at insertionPosition");
		Table table = new Table();
		table.CellSpacing = 0.0;
		TableRowGroup tableRowGroup = new TableRowGroup();
		for (int i = 0; i < rowCount; i++)
		{
			TableRow tableRow = new TableRow();
			for (int j = 0; j < columnCount; j++)
			{
				TableCell tableCell = new TableCell(new Paragraph());
				tableCell.BorderThickness = GetCellBorder(1.0, i, j, 1, 1, rowCount, columnCount);
				tableCell.BorderBrush = Brushes.Black;
				tableRow.Cells.Add(tableCell);
			}
			tableRowGroup.Rows.Add(tableRow);
		}
		table.RowGroups.Add(tableRowGroup);
		paragraph.SiblingBlocks.InsertBefore(paragraph, table);
		return table;
	}

	private static Thickness GetCellBorder(double thickness, int rowIndex, int columnIndex, int rowSpan, int columnSpan, int rowCount, int columnCount)
	{
		return new Thickness(thickness, thickness, (columnIndex + columnSpan < columnCount) ? 0.0 : thickness, (rowIndex + rowSpan < rowCount) ? 0.0 : thickness);
	}

	internal static TextPointer EnsureInsertionPosition(TextPointer position)
	{
		Invariant.Assert(position != null, "null check: position");
		position = position.GetInsertionPosition(position.LogicalDirection);
		if (!TextPointerBase.IsAtInsertionPosition(position))
		{
			position = CreateInsertionPositionInIncompleteContent(position);
		}
		else
		{
			if (position.IsAtRowEnd)
			{
				Table tableFromPosition = GetTableFromPosition(position);
				position = GetAdjustedRowEndPosition(tableFromPosition, position);
				if (position.CompareTo(tableFromPosition.ElementEnd) == 0)
				{
					position = CreateImplicitParagraph(tableFromPosition.ElementEnd);
				}
			}
			Invariant.Assert(!position.IsAtRowEnd, "position is not expected to be at RowEnd anymore");
			if (TextPointerBase.IsInBlockUIContainer(position))
			{
				BlockUIContainer blockUIContainer = (BlockUIContainer)position.Parent;
				position = ((position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart) ? CreateImplicitParagraph(blockUIContainer.ElementStart) : CreateImplicitParagraph(blockUIContainer.ElementEnd));
				if (blockUIContainer.IsEmpty)
				{
					blockUIContainer.RepositionWithContent(null);
				}
			}
			else if (TextPointerBase.IsBeforeFirstTable(position) || TextPointerBase.IsAtPotentialParagraphPosition(position))
			{
				position = CreateImplicitParagraph(position);
			}
			else if (TextPointerBase.IsAtPotentialRunPosition(position))
			{
				position = CreateImplicitRun(position);
			}
		}
		Invariant.Assert(TextSchema.IsInTextContent(position), "position must be in text content now");
		return position;
	}

	internal static TextPointer GetAdjustedRowEndPosition(Table currentTable, TextPointer rowEndPosition)
	{
		TextPointer textPointer = rowEndPosition;
		while (textPointer != null && textPointer.IsAtRowEnd && currentTable == GetTableFromPosition(textPointer))
		{
			textPointer = textPointer.GetNextInsertionPosition(LogicalDirection.Forward);
		}
		if (textPointer != null && currentTable == GetTableFromPosition(textPointer))
		{
			return textPointer;
		}
		return currentTable.ElementEnd;
	}

	private static TextPointer CreateInsertionPositionInIncompleteContent(TextPointer position)
	{
		while (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
		{
			position = position.GetNextContextPosition(LogicalDirection.Forward);
		}
		while (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementEnd)
		{
			position = position.GetNextContextPosition(LogicalDirection.Backward);
		}
		DependencyObject parent = position.Parent;
		if (parent != null)
		{
			if (parent is Table)
			{
				TableRowGroup tableRowGroup = new TableRowGroup();
				tableRowGroup.Reposition(position, position);
				position = tableRowGroup.ContentStart;
				parent = position.Parent;
			}
			if (parent is TableRowGroup)
			{
				TableRow tableRow = new TableRow();
				tableRow.Reposition(position, position);
				position = tableRow.ContentStart;
				parent = position.Parent;
			}
			if (parent is TableRow)
			{
				TableCell tableCell = new TableCell();
				tableCell.Reposition(position, position);
				position = tableCell.ContentStart;
				parent = position.Parent;
			}
			if (parent is List)
			{
				ListItem listItem = new ListItem();
				listItem.Reposition(position, position);
				position = listItem.ContentStart;
				parent = position.Parent;
			}
			if (parent is LineBreak || parent is InlineUIContainer)
			{
				position = ((Inline)parent).ElementStart;
				parent = position.Parent;
			}
		}
		if (parent == null)
		{
			throw new InvalidOperationException(SR.TextSchema_CannotInsertContentInThisPosition);
		}
		if (TextSchema.IsValidChild(position, typeof(Inline)))
		{
			return CreateImplicitRun(position);
		}
		Invariant.Assert(TextSchema.IsValidChild(position, typeof(Block)), "Expecting valid parent-child relationship");
		return CreateImplicitParagraph(position);
	}

	private static TextPointer CreateImplicitRun(TextPointer position)
	{
		TextPointer textPointer;
		if (position.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward) is Run)
		{
			textPointer = position.CreatePointer();
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			textPointer.Freeze();
		}
		else if (position.GetAdjacentElementFromOuterPosition(LogicalDirection.Backward) is Run)
		{
			textPointer = position.CreatePointer();
			textPointer.MoveToNextContextPosition(LogicalDirection.Backward);
			textPointer.Freeze();
		}
		else
		{
			Run run = Inline.CreateImplicitRun(position.Parent);
			run.Reposition(position, position);
			textPointer = run.ContentStart.GetFrozenPointer(position.LogicalDirection);
		}
		return textPointer;
	}

	private static TextPointer CreateImplicitParagraph(TextPointer position)
	{
		Paragraph paragraph = new Paragraph();
		paragraph.Reposition(position, position);
		Run run = Inline.CreateImplicitRun(paragraph);
		paragraph.Inlines.Add(run);
		return run.ContentStart.GetFrozenPointer(position.LogicalDirection);
	}

	internal static void DeleteContent(TextPointer start, TextPointer end)
	{
		if (start.CompareTo(end) > 0)
		{
			TextPointer textPointer = end;
			end = start;
			start = textPointer;
		}
		TableCell anchorCell;
		TableCell movingCell;
		TableRow anchorRow;
		TableRow movingRow;
		TableRowGroup anchorRowGroup;
		TableRowGroup movingRowGroup;
		Table anchorTable;
		Table movingTable;
		while (start.CompareTo(end) < 0 && IdentifyTableElements(start, end, includeCellAtMovingPosition: false, out anchorCell, out movingCell, out anchorRow, out movingRow, out anchorRowGroup, out movingRowGroup, out anchorTable, out movingTable))
		{
			if ((anchorTable == null && movingTable == null) || anchorTable == movingTable)
			{
				bool isTableCellRange;
				List<TextSegment> list = BuildTableRange(start, end, includeCellAtMovingPosition: false, out isTableCellRange);
				if (isTableCellRange && list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						ClearTableCells(list[i]);
					}
					end = start;
					continue;
				}
				if (anchorCell != null)
				{
					anchorRow = anchorCell.Row;
				}
				else if (anchorRow == null && anchorRowGroup != null)
				{
					anchorRow = anchorRowGroup.Rows[0];
				}
				if (movingCell != null)
				{
					movingRow = anchorCell.Row;
				}
				else if (movingRow == null && movingRowGroup != null)
				{
					movingRow = movingRowGroup.Rows[movingRowGroup.Rows.Count - 1];
				}
				Invariant.Assert(anchorRow != null && movingRow != null, "startRow and endRow cannot be null, since our range is within one table");
				DeleteRows(new TextRange(anchorRow.ContentStart, movingRow.ContentEnd));
			}
			else
			{
				if (anchorRow != null)
				{
					start = anchorRow.Table.ElementEnd;
					DeleteRows(new TextRange(anchorRow.ContentStart, anchorRow.Table.ContentEnd));
				}
				if (movingRow != null)
				{
					end = movingRow.Table.ElementStart;
					DeleteRows(new TextRange(movingRow.Table.ContentStart, movingRow.ContentEnd));
				}
			}
		}
		if (start.CompareTo(end) < 0)
		{
			TextRangeEdit.DeleteParagraphContent(start, end);
		}
	}

	private static void ClearTableCells(TextSegment textSegment)
	{
		TableCell tableCell = GetTableCellFromPosition((TextPointer)textSegment.Start);
		TextPointer nextInsertionPosition = ((TextPointer)textSegment.End).GetNextInsertionPosition(LogicalDirection.Backward);
		while (tableCell != null)
		{
			tableCell.Blocks.Clear();
			tableCell.Blocks.Add(new Paragraph());
			TextPointer elementEnd = tableCell.ElementEnd;
			tableCell = ((elementEnd.CompareTo(nextInsertionPosition) >= 0 || elementEnd.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart) ? null : ((TableCell)elementEnd.GetAdjacentElement(LogicalDirection.Forward)));
		}
	}

	internal static TextRange InsertRows(TextRange textRange, int rowCount)
	{
		Invariant.Assert(textRange != null, "null check: textRange");
		TableRow tableRowFromPosition = GetTableRowFromPosition((rowCount > 0) ? textRange.End : textRange.Start);
		if (tableRowFromPosition == null || rowCount == 0)
		{
			return new TextRange(textRange.Start, textRange.Start);
		}
		TableCell[] spannedCells = tableRowFromPosition.SpannedCells;
		if (spannedCells != null)
		{
			foreach (TableCell tableCell in spannedCells)
			{
				tableCell.ContentStart.TextContainer.SetValue(tableCell.ContentStart, TableCell.RowSpanProperty, tableCell.RowSpan + ((rowCount > 0) ? rowCount : (-rowCount)));
			}
		}
		TableRowGroup rowGroup = tableRowFromPosition.RowGroup;
		int num = rowGroup.Rows.IndexOf(tableRowFromPosition);
		if (rowCount > 0)
		{
			num++;
		}
		TableRow tableRow = null;
		TableRow tableRow2 = null;
		while (rowCount != 0)
		{
			TableRow tableRow3 = CopyRow(tableRowFromPosition);
			if (tableRow == null)
			{
				tableRow = tableRow3;
			}
			tableRow2 = tableRow3;
			TableCellCollection cells = tableRowFromPosition.Cells;
			for (int j = 0; j < cells.Count; j++)
			{
				TableCell tableCell2 = cells[j];
				if (rowCount < 0 || tableCell2.RowSpan == 1)
				{
					AddCellCopy(tableRow3, tableCell2, -1, copyRowSpan: false, copyColumnSpan: true);
				}
			}
			rowGroup.Rows.Insert(num, tableRow3);
			if (rowCount > 0)
			{
				num++;
			}
			rowCount -= ((rowCount > 0) ? 1 : (-1));
		}
		CorrectBorders(rowGroup.Rows);
		if (rowCount <= 0)
		{
			return new TextRange(tableRow2.ContentStart, tableRow.ContentEnd);
		}
		return new TextRange(tableRow.ContentStart, tableRow2.ContentEnd);
	}

	internal static bool DeleteRows(TextRange textRange)
	{
		Invariant.Assert(textRange != null, "null check: textRange");
		TableRow tableRowFromPosition = GetTableRowFromPosition(textRange.Start);
		TableRow tableRowFromPosition2 = GetTableRowFromPosition(textRange.End);
		if (tableRowFromPosition == null || tableRowFromPosition2 == null || tableRowFromPosition.RowGroup != tableRowFromPosition2.RowGroup)
		{
			return false;
		}
		TableRowCollection rows = tableRowFromPosition.RowGroup.Rows;
		int num = tableRowFromPosition2.Index - tableRowFromPosition.Index + 1;
		if (num == rows.Count)
		{
			tableRowFromPosition.Table.RepositionWithContent(null);
		}
		else
		{
			if (tableRowFromPosition2.Index != rows.Count - 1)
			{
				CorrectRowSpansOnDeleteRows(rows[tableRowFromPosition2.Index + 1], num);
			}
			rows.RemoveRange(tableRowFromPosition.Index, tableRowFromPosition2.Index - tableRowFromPosition.Index + 1);
			Invariant.Assert(rows.Count > 0);
			CorrectBorders(rows);
		}
		textRange.Select(textRange.Start, textRange.Start);
		return true;
	}

	private static void CorrectBorders(TableRowCollection rows)
	{
		Table table = rows[0].Table;
		if (table.CellSpacing > 0.0)
		{
			return;
		}
		int columnCount = table.ColumnCount;
		int count = rows.Count;
		for (int i = 0; i < count; i++)
		{
			TableCellCollection cells = rows[i].Cells;
			for (int j = 0; j < cells.Count; j++)
			{
				TableCell tableCell = cells[j];
				Thickness borderThickness = tableCell.BorderThickness;
				double num = ((tableCell.ColumnIndex + tableCell.ColumnSpan < columnCount) ? 0.0 : borderThickness.Left);
				double num2 = ((i + tableCell.RowSpan < count) ? 0.0 : borderThickness.Top);
				if (borderThickness.Right != num || borderThickness.Bottom != num2)
				{
					borderThickness.Right = num;
					borderThickness.Bottom = num2;
					tableCell.BorderThickness = borderThickness;
				}
			}
		}
	}

	private static void CorrectRowSpansOnDeleteRows(TableRow nextRow, int deletedRowsCount)
	{
		Invariant.Assert(nextRow != null, "null check: nextRow");
		Invariant.Assert(nextRow.Index >= deletedRowsCount, "nextRow.Index is expected to be >= deletedRowsCount");
		TableCellCollection cells = nextRow.Cells;
		TableCell[] spannedCells = nextRow.SpannedCells;
		if (spannedCells == null)
		{
			return;
		}
		int i = 0;
		foreach (TableCell tableCell in spannedCells)
		{
			int index = tableCell.Row.Index;
			if (index >= nextRow.Index)
			{
				continue;
			}
			if (index < nextRow.Index - deletedRowsCount)
			{
				Invariant.Assert(tableCell.RowSpan > deletedRowsCount, "spannedCell.RowSpan is expected to be > deletedRowsCount");
				tableCell.ContentStart.TextContainer.SetValue(tableCell.ContentStart, TableCell.RowSpanProperty, tableCell.RowSpan - deletedRowsCount);
				continue;
			}
			for (int columnIndex = tableCell.ColumnIndex; i < cells.Count && cells[i].ColumnIndex < columnIndex; i++)
			{
			}
			TableCell tableCell2 = AddCellCopy(nextRow, tableCell, i, copyRowSpan: false, copyColumnSpan: true);
			Invariant.Assert(tableCell.RowSpan - (nextRow.Index - tableCell.Row.Index) > 0, "expecting: spannedCell.RowSpan - (nextRow.Index - spannedCell.Row.Index) > 0");
			tableCell2.ContentStart.TextContainer.SetValue(tableCell2.ContentStart, TableCell.RowSpanProperty, tableCell.RowSpan - (nextRow.Index - tableCell.Row.Index));
			i++;
		}
	}

	private static void InsertColumn(int colIndex, Table table)
	{
		for (int i = 0; i < table.RowGroups.Count; i++)
		{
			TableRowGroup tableRowGroup = table.RowGroups[i];
			for (int j = 0; j < tableRowGroup.Rows.Count; j++)
			{
				TableRow tableRow = tableRowGroup.Rows[j];
				if (colIndex == -1)
				{
					if (tableRow.Cells[0].ColumnIndex == 0)
					{
						AddCellCopy(tableRow, tableRow.Cells[0], 0, copyRowSpan: true, copyColumnSpan: false);
					}
					continue;
				}
				TableCell tableCell = null;
				for (int k = 0; k < tableRow.Cells.Count; k++)
				{
					TableCell tableCell2 = tableRow.Cells[k];
					if (tableCell2.ColumnIndex + tableCell2.ColumnSpan > colIndex)
					{
						if (tableCell2.ColumnIndex <= colIndex)
						{
							tableCell = tableCell2;
						}
						break;
					}
				}
				if (tableCell != null)
				{
					if (tableCell.ColumnSpan == 1)
					{
						AddCellCopy(tableRow, tableCell, tableRow.Cells.IndexOf(tableCell) + 1, copyRowSpan: true, copyColumnSpan: true);
					}
					else
					{
						tableCell.ContentStart.TextContainer.SetValue(tableCell.ContentStart, TableCell.ColumnSpanProperty, tableCell.ColumnSpan + 1);
					}
				}
			}
			CorrectBorders(tableRowGroup.Rows);
		}
	}

	internal static TextRange InsertColumns(TextRange textRange, int columnCount)
	{
		int num = Math.Abs(columnCount);
		Invariant.Assert(textRange != null, "null check: textRange");
		if (!IdentifyTableElements(textRange.Start, textRange.End, includeCellAtMovingPosition: false, out var anchorCell, out var movingCell, out var _, out var _, out var _, out var _, out var _, out var _))
		{
			if (textRange.IsTableCellRange)
			{
				return null;
			}
			anchorCell = GetTableCellFromPosition(textRange.Start);
			movingCell = GetTableCellFromPosition(textRange.End);
			if (anchorCell == null || anchorCell != movingCell)
			{
				return null;
			}
		}
		int num2 = movingCell.ColumnIndex + movingCell.ColumnSpan - 1;
		for (int i = 0; i < num; i++)
		{
			if (columnCount < 0)
			{
				InsertColumn(num2 - 1, movingCell.Table);
			}
			else
			{
				InsertColumn(num2, movingCell.Table);
			}
		}
		return null;
	}

	internal static void DeleteColumn(int colIndex, Table table)
	{
		for (int i = 0; i < table.RowGroups.Count; i++)
		{
			TableRowGroup tableRowGroup = table.RowGroups[i];
			for (int j = 0; j < tableRowGroup.Rows.Count; j++)
			{
				TableRow tableRow = tableRowGroup.Rows[j];
				TableCell tableCell = null;
				for (int k = 0; k < tableRow.Cells.Count; k++)
				{
					TableCell tableCell2 = tableRow.Cells[k];
					if (tableCell2.ColumnIndex + tableCell2.ColumnSpan > colIndex)
					{
						if (tableCell2.ColumnIndex <= colIndex)
						{
							tableCell = tableCell2;
						}
						break;
					}
				}
				if (tableCell != null)
				{
					if (tableCell.ColumnSpan == 1)
					{
						tableRow.Cells.Remove(tableCell);
					}
					else
					{
						tableCell.ContentStart.TextContainer.SetValue(tableCell.ContentStart, TableCell.ColumnSpanProperty, tableCell.ColumnSpan - 1);
					}
				}
			}
			CorrectBorders(tableRowGroup.Rows);
		}
	}

	internal static bool DeleteColumns(TextRange textRange)
	{
		Invariant.Assert(textRange != null, "null check: textRange");
		if (!IsTableCellRange(textRange.Start, textRange.End, includeCellAtMovingPosition: false, out var anchorCell, out var movingCell))
		{
			return false;
		}
		int columnIndex = anchorCell.ColumnIndex;
		int num = movingCell.ColumnIndex - anchorCell.ColumnIndex + 1;
		if (num == 0 || num == anchorCell.Table.ColumnCount)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			DeleteColumn(columnIndex, movingCell.Table);
		}
		return true;
	}

	internal static bool TableBorderHitTest(ITextView textView, Point pt)
	{
		Table table;
		int columnIndex;
		Rect columnRect;
		double tableAutofitWidth;
		double[] columnWidths;
		return TableBorderHitTest(textView, pt, out table, out columnIndex, out columnRect, out tableAutofitWidth, out columnWidths);
	}

	private static bool TableBorderHitTest(ITextView textView, Point point, out Table table, out int columnIndex, out Rect columnRect, out double tableAutofitWidth, out double[] columnWidths)
	{
		table = null;
		columnIndex = -1;
		columnRect = Rect.Empty;
		tableAutofitWidth = 0.0;
		columnWidths = null;
		if (!(textView is TextDocumentView))
		{
			return false;
		}
		CellInfo cellInfoFromPoint = ((TextDocumentView)textView).GetCellInfoFromPoint(point, null);
		if (cellInfoFromPoint == null)
		{
			return false;
		}
		if (point.Y < cellInfoFromPoint.TableArea.Top || point.Y > cellInfoFromPoint.TableArea.Bottom)
		{
			return false;
		}
		double num = 1.0;
		TableCell cell = cellInfoFromPoint.Cell;
		if (cell.ColumnIndex != 0 && point.X < cellInfoFromPoint.CellArea.Left + num)
		{
			columnIndex = cellInfoFromPoint.Cell.ColumnIndex - 1;
			columnRect = new Rect(cellInfoFromPoint.CellArea.Left, cellInfoFromPoint.TableArea.Top, 1.0, cellInfoFromPoint.TableArea.Height);
		}
		if (cell.ColumnIndex + cell.ColumnSpan <= cell.Table.ColumnCount && point.X > cellInfoFromPoint.CellArea.Right - num && (!IsLastCellInRow(cell) || point.X < cellInfoFromPoint.CellArea.Right + num))
		{
			columnIndex = cell.ColumnIndex + cell.ColumnSpan - 1;
			columnRect = new Rect(cellInfoFromPoint.CellArea.Right, cellInfoFromPoint.TableArea.Top, 1.0, cellInfoFromPoint.TableArea.Height);
		}
		if (columnIndex == -1)
		{
			return false;
		}
		table = cell.Table;
		tableAutofitWidth = cellInfoFromPoint.TableAutofitWidth;
		columnWidths = cellInfoFromPoint.TableColumnWidths;
		return true;
	}

	internal static TableColumnResizeInfo StartColumnResize(ITextView textView, Point pt)
	{
		if (TableBorderHitTest(textView, pt, out var table, out var columnIndex, out var columnRect, out var tableAutofitWidth, out var columnWidths))
		{
			return new TableColumnResizeInfo(textView, table, columnIndex, columnRect, tableAutofitWidth, columnWidths);
		}
		return null;
	}

	internal static void EnsureTableColumnsAreFixedSize(Table table, double[] columnWidths)
	{
		while (table.Columns.Count < columnWidths.Length)
		{
			table.Columns.Add(new TableColumn());
		}
		for (int i = 0; i < table.ColumnCount; i++)
		{
			table.Columns[i].Width = new GridLength(columnWidths[i]);
		}
	}

	internal static TextRange MergeCells(TextRange textRange)
	{
		Invariant.Assert(textRange != null, "null check: textRange");
		if (!IdentifyTableElements(textRange.Start, textRange.End, includeCellAtMovingPosition: false, out var anchorCell, out var movingCell, out var _, out var _, out var _, out var _, out var _, out var _))
		{
			return null;
		}
		if (anchorCell == null || movingCell == null)
		{
			return null;
		}
		Invariant.Assert(anchorCell.Row.RowGroup == movingCell.Row.RowGroup, "startCell and endCell must belong to the same RowGroup");
		Invariant.Assert(anchorCell.Row.Index <= movingCell.Row.Index, "startCell.Row.Index must be <= endCell.Row.Index");
		Invariant.Assert(anchorCell.ColumnIndex <= movingCell.ColumnIndex + movingCell.ColumnSpan - 1, "startCell.ColumnIndex must be <= an index+span of an endCell");
		TextRange textRange2 = MergeCellRange(anchorCell.Row.RowGroup, anchorCell.Row.Index, movingCell.Row.Index + movingCell.RowSpan - 1, anchorCell.ColumnIndex, movingCell.ColumnIndex + movingCell.ColumnSpan - 1);
		if (textRange2 != null)
		{
			textRange.Select(textRange.Start, textRange.End);
		}
		return textRange2;
	}

	internal static TextRange SplitCell(TextRange textRange, int splitCountHorizontal, int splitCountVertical)
	{
		Invariant.Assert(textRange != null, "null check: textRange");
		if (!IdentifyTableElements(textRange.Start, textRange.End, includeCellAtMovingPosition: false, out var anchorCell, out var movingCell, out var _, out var _, out var _, out var _, out var _, out var _))
		{
			return null;
		}
		if (anchorCell == null || anchorCell != movingCell)
		{
			return null;
		}
		if (anchorCell.ColumnSpan == 1 && anchorCell.RowSpan == 1)
		{
			return null;
		}
		TableRowGroup rowGroup = anchorCell.Row.RowGroup;
		_ = anchorCell.Row.Cells;
		int index = anchorCell.Index;
		if (splitCountHorizontal > anchorCell.ColumnSpan - 1)
		{
			splitCountHorizontal = anchorCell.ColumnSpan - 1;
		}
		Invariant.Assert(splitCountHorizontal >= 0, "expecting: splitCountHorizontal >= 0");
		if (splitCountVertical > anchorCell.RowSpan - 1)
		{
			splitCountVertical = anchorCell.RowSpan - 1;
		}
		Invariant.Assert(splitCountVertical >= 0, "expecting; splitCoutVertical >= 0");
		while (splitCountHorizontal > 0)
		{
			AddCellCopy(anchorCell.Row, anchorCell, index + 1, copyRowSpan: true, copyColumnSpan: false);
			anchorCell.ContentStart.TextContainer.SetValue(anchorCell.ContentStart, TableCell.ColumnSpanProperty, anchorCell.ColumnSpan - 1);
			if (anchorCell.ColumnSpan == 1)
			{
				anchorCell.ClearValue(TableCell.ColumnSpanProperty);
			}
			splitCountHorizontal--;
		}
		CorrectBorders(rowGroup.Rows);
		return new TextRange(anchorCell.ContentStart, anchorCell.ContentStart);
	}

	private static TextSegment NewNormalizedTextSegment(TextPointer startPosition, TextPointer endPosition)
	{
		startPosition = startPosition.GetInsertionPosition(LogicalDirection.Forward);
		if (!TextPointerBase.IsAfterLastParagraph(endPosition))
		{
			endPosition = endPosition.GetInsertionPosition(LogicalDirection.Backward);
		}
		if (startPosition.CompareTo(endPosition) < 0)
		{
			return new TextSegment(startPosition, endPosition);
		}
		return new TextSegment(startPosition, startPosition);
	}

	private static TextSegment NewNormalizedCellSegment(TableCell startCell, TableCell endCell)
	{
		Invariant.Assert(startCell.Row == endCell.Row, "startCell and endCell must be in the same Row");
		Invariant.Assert(startCell.Index <= endCell.Index, "insed of a startCell mustbe <= an index of an endCell");
		TextPointer insertionPosition = startCell.ContentStart.GetInsertionPosition(LogicalDirection.Forward);
		TextPointer nextInsertionPosition = endCell.ContentEnd.GetNextInsertionPosition(LogicalDirection.Forward);
		Invariant.Assert(GetTableRowFromPosition(nextInsertionPosition) == GetTableRowFromPosition(endCell.ContentEnd), "Inconsistent Rows on end");
		Invariant.Assert(insertionPosition.CompareTo(nextInsertionPosition) < 0, "The end must be in the beginning of the next cell (or at row end).");
		Invariant.Assert(GetTableRowFromPosition(insertionPosition) == GetTableRowFromPosition(nextInsertionPosition), "Inconsistent Rows for start and end");
		return new TextSegment(insertionPosition, nextInsertionPosition);
	}

	private static bool IdentifyTableElements(TextPointer anchorPosition, TextPointer movingPosition, bool includeCellAtMovingPosition, out TableCell anchorCell, out TableCell movingCell, out TableRow anchorRow, out TableRow movingRow, out TableRowGroup anchorRowGroup, out TableRowGroup movingRowGroup, out Table anchorTable, out Table movingTable)
	{
		anchorPosition = anchorPosition.GetInsertionPosition(LogicalDirection.Forward);
		if (!TextPointerBase.IsAfterLastParagraph(movingPosition))
		{
			movingPosition = movingPosition.GetInsertionPosition(LogicalDirection.Backward);
		}
		if (!FindTableElements(anchorPosition, movingPosition, out anchorCell, out movingCell, out anchorRow, out movingRow, out anchorRowGroup, out movingRowGroup, out anchorTable, out movingTable))
		{
			return false;
		}
		if (anchorTable != null || movingTable != null)
		{
			anchorCell = null;
			movingCell = null;
		}
		else if (anchorCell != null && movingCell != null)
		{
			if (!includeCellAtMovingPosition && movingPosition.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && movingCell.ColumnIndex > anchorCell.ColumnIndex + anchorCell.ColumnSpan - 1 && movingCell.Index > 0)
			{
				movingCell = movingCell.Row.Cells[movingCell.Index - 1];
			}
		}
		else if (anchorCell != null && movingCell == null && movingPosition.IsAtRowEnd)
		{
			TableRow tableRow = movingPosition.Parent as TableRow;
			movingCell = tableRow.Cells[tableRow.Cells.Count - 1];
		}
		else
		{
			anchorCell = null;
			movingCell = null;
		}
		if (anchorCell == null && movingCell == null && anchorRow == null && movingRow == null && anchorRowGroup == null && movingRowGroup == null && anchorTable == null)
		{
			return movingTable != null;
		}
		return true;
	}

	private static bool FindTableElements(TextPointer anchorPosition, TextPointer movingPosition, out TableCell anchorCell, out TableCell movingCell, out TableRow anchorRow, out TableRow movingRow, out TableRowGroup anchorRowGroup, out TableRowGroup movingRowGroup, out Table anchorTable, out Table movingTable)
	{
		if (anchorPosition.Parent == movingPosition.Parent)
		{
			anchorCell = null;
			movingCell = null;
			anchorRow = null;
			movingRow = null;
			anchorRowGroup = null;
			movingRowGroup = null;
			anchorTable = null;
			movingTable = null;
			return false;
		}
		TextElement textElement = anchorPosition.Parent as TextElement;
		while (textElement != null && !textElement.Contains(movingPosition))
		{
			textElement = textElement.Parent as TextElement;
		}
		FindTableElements(textElement, anchorPosition, out anchorCell, out anchorRow, out anchorRowGroup, out anchorTable);
		FindTableElements(textElement, movingPosition, out movingCell, out movingRow, out movingRowGroup, out movingTable);
		if (anchorCell == null && movingCell == null && anchorRow == null && movingRow == null && anchorRowGroup == null && movingRowGroup == null && anchorTable == null)
		{
			return movingTable != null;
		}
		return true;
	}

	private static void FindTableElements(TextElement commonAncestor, TextPointer position, out TableCell cell, out TableRow row, out TableRowGroup rowGroup, out Table table)
	{
		cell = null;
		row = null;
		rowGroup = null;
		table = null;
		for (TextElement textElement = position.Parent as TextElement; textElement != commonAncestor; textElement = textElement.Parent as TextElement)
		{
			Invariant.Assert(textElement != null, "Not expecting null for element: otherwise it must hit commonAncestor which must be null in this case...");
			if (textElement is TableCell)
			{
				cell = (TableCell)textElement;
				row = null;
				rowGroup = null;
				table = null;
			}
			else if (textElement is TableRow)
			{
				row = (TableRow)textElement;
				rowGroup = null;
				table = null;
			}
			else if (textElement is TableRowGroup)
			{
				rowGroup = (TableRowGroup)textElement;
				table = null;
			}
			else if (textElement is Table)
			{
				table = (Table)textElement;
			}
		}
	}

	private static TableRow CopyRow(TableRow currentRow)
	{
		Invariant.Assert(currentRow != null, "null check: currentRow");
		TableRow tableRow = new TableRow();
		LocalValueEnumerator localValueEnumerator = currentRow.GetLocalValueEnumerator();
		while (localValueEnumerator.MoveNext())
		{
			LocalValueEntry current = localValueEnumerator.Current;
			if (!current.Property.ReadOnly)
			{
				tableRow.SetValue(current.Property, current.Value);
			}
		}
		return tableRow;
	}

	private static TableCell AddCellCopy(TableRow newRow, TableCell currentCell, int cellInsertionIndex, bool copyRowSpan, bool copyColumnSpan)
	{
		Invariant.Assert(currentCell != null, "null check: currentCell");
		TableCell tableCell = new TableCell();
		if (cellInsertionIndex < 0)
		{
			newRow.Cells.Add(tableCell);
		}
		else
		{
			newRow.Cells.Insert(cellInsertionIndex, tableCell);
		}
		LocalValueEnumerator localValueEnumerator = currentCell.GetLocalValueEnumerator();
		while (localValueEnumerator.MoveNext())
		{
			LocalValueEntry current = localValueEnumerator.Current;
			if ((current.Property != TableCell.RowSpanProperty || copyRowSpan) && (current.Property != TableCell.ColumnSpanProperty || copyColumnSpan) && !current.Property.ReadOnly)
			{
				tableCell.SetValue(current.Property, current.Value);
			}
		}
		if (currentCell.Blocks.FirstBlock != null)
		{
			Paragraph paragraph = new Paragraph();
			if (currentCell.Blocks.FirstBlock is Paragraph paragraph2)
			{
				DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(Paragraph));
				DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(typeof(Paragraph));
				foreach (DependencyProperty dp in noninheritableProperties)
				{
					object obj = paragraph2.ReadLocalValue(dp);
					if (obj != DependencyProperty.UnsetValue)
					{
						paragraph.SetValue(dp, obj);
					}
				}
				foreach (DependencyProperty dp2 in inheritableProperties)
				{
					object obj2 = paragraph2.ReadLocalValue(dp2);
					if (obj2 != DependencyProperty.UnsetValue)
					{
						paragraph.SetValue(dp2, obj2);
					}
				}
			}
			tableCell.Blocks.Add(paragraph);
		}
		return tableCell;
	}

	private static TextRange MergeCellRange(TableRowGroup rowGroup, int topRow, int bottomRow, int leftColumn, int rightColumn)
	{
		Invariant.Assert(rowGroup != null, "null check: rowGroup");
		Invariant.Assert(topRow >= 0, "topRow must be >= 0");
		Invariant.Assert(bottomRow >= 0, "bottomRow must be >= 0");
		Invariant.Assert(leftColumn >= 0, "leftColumn must be >= 0");
		Invariant.Assert(rightColumn >= 0, "rightColumn must be >= 0");
		Invariant.Assert(topRow <= bottomRow, "topRow must be <= bottomRow");
		Invariant.Assert(leftColumn <= rightColumn, "leftColumn must be <= rightColumn");
		if (!CanMergeCellRange(rowGroup, topRow, bottomRow, leftColumn, rightColumn))
		{
			return null;
		}
		return DoMergeCellRange(rowGroup, topRow, bottomRow, leftColumn, rightColumn);
	}

	private static bool CanMergeCellRange(TableRowGroup rowGroup, int topRow, int bottomRow, int leftColumn, int rightColumn)
	{
		bool result = false;
		if (topRow >= rowGroup.Rows.Count || bottomRow >= rowGroup.Rows.Count)
		{
			return result;
		}
		if (rowGroup.Rows[topRow].ColumnCount != rowGroup.Rows[bottomRow].ColumnCount)
		{
			return result;
		}
		if (leftColumn >= rowGroup.Rows[topRow].ColumnCount || rightColumn >= rowGroup.Rows[bottomRow].ColumnCount)
		{
			return result;
		}
		TableCell[] spannedCells = rowGroup.Rows[topRow].SpannedCells;
		for (int i = 0; i < spannedCells.Length; i++)
		{
			if (spannedCells[i].Row.Index < topRow)
			{
				int columnIndex = spannedCells[i].ColumnIndex;
				int num = columnIndex + spannedCells[i].ColumnSpan - 1;
				if (columnIndex <= rightColumn && num >= leftColumn)
				{
					return result;
				}
			}
		}
		for (int j = topRow; j <= bottomRow; j++)
		{
			if (!GetBoundaryCells(rowGroup.Rows[j], bottomRow, leftColumn, rightColumn, out var firstCell, out var _))
			{
				return result;
			}
			if (j == topRow && (firstCell == null || firstCell.ColumnIndex != leftColumn))
			{
				return result;
			}
		}
		return true;
	}

	private static TextRange DoMergeCellRange(TableRowGroup rowGroup, int topRow, int bottomRow, int leftColumn, int rightColumn)
	{
		TextRange result = null;
		for (int num = bottomRow; num >= topRow; num--)
		{
			TableRow tableRow = rowGroup.Rows[num];
			GetBoundaryCells(tableRow, bottomRow, leftColumn, rightColumn, out var firstCell, out var lastCell);
			if (num == topRow)
			{
				Invariant.Assert(firstCell != null, "firstCell is not expected to be null");
				Invariant.Assert(lastCell != null, "lastCell is not expected to be null");
				Invariant.Assert(firstCell.ColumnIndex == leftColumn, "expecting: firstCell.ColumnIndex == leftColumn");
				int num2 = bottomRow - topRow + 1;
				int num3 = rightColumn - leftColumn + 1;
				if (num2 == 1)
				{
					firstCell.ClearValue(TableCell.RowSpanProperty);
				}
				else
				{
					firstCell.ContentStart.TextContainer.SetValue(firstCell.ContentStart, TableCell.RowSpanProperty, num2);
				}
				firstCell.ContentStart.TextContainer.SetValue(firstCell.ContentStart, TableCell.ColumnSpanProperty, num3);
				result = new TextRange(firstCell.ContentStart, firstCell.ContentStart);
				if (firstCell != lastCell)
				{
					tableRow.Cells.RemoveRange(firstCell.Index + 1, lastCell.Index - firstCell.Index + 1 - 1);
				}
			}
			else if (firstCell != null)
			{
				Invariant.Assert(lastCell != null, "lastCell is not expected to be null");
				if (firstCell.Index == 0 && lastCell.Index == lastCell.Row.Cells.Count - 1)
				{
					TableCell[] spannedCells = tableRow.SpannedCells;
					foreach (TableCell tableCell in spannedCells)
					{
						if (tableCell.ColumnIndex < firstCell.ColumnIndex || tableCell.ColumnIndex > lastCell.ColumnIndex)
						{
							int num4 = tableCell.RowSpan - 1;
							if (num4 == 1)
							{
								tableCell.ClearValue(TableCell.RowSpanProperty);
							}
							else
							{
								tableCell.ContentStart.TextContainer.SetValue(tableCell.ContentStart, TableCell.RowSpanProperty, num4);
							}
						}
					}
					tableRow.RowGroup.Rows.Remove(tableRow);
					bottomRow--;
				}
				else
				{
					tableRow.Cells.RemoveRange(firstCell.Index, lastCell.Index - firstCell.Index + 1);
				}
			}
		}
		CorrectBorders(rowGroup.Rows);
		return result;
	}

	private static bool GetBoundaryCells(TableRow row, int bottomRow, int leftColumn, int rightColumn, out TableCell firstCell, out TableCell lastCell)
	{
		firstCell = null;
		lastCell = null;
		bool flag = false;
		for (int i = 0; i < row.Cells.Count; i++)
		{
			TableCell tableCell = row.Cells[i];
			int columnIndex = tableCell.ColumnIndex;
			int num = columnIndex + tableCell.ColumnSpan - 1;
			if (columnIndex <= rightColumn && num >= leftColumn)
			{
				if (row.Index + tableCell.RowSpan - 1 > bottomRow)
				{
					flag = true;
				}
				if (firstCell == null)
				{
					firstCell = tableCell;
				}
				lastCell = tableCell;
			}
		}
		if (!flag && (firstCell == null || (firstCell.ColumnIndex >= leftColumn && firstCell.ColumnIndex + firstCell.ColumnSpan - 1 <= rightColumn)))
		{
			if (lastCell != null)
			{
				if (lastCell.ColumnIndex >= leftColumn)
				{
					return lastCell.ColumnIndex + lastCell.ColumnSpan - 1 <= rightColumn;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool IsLastCellInRow(TableCell cell)
	{
		return cell.ColumnIndex + cell.ColumnSpan == cell.Table.ColumnCount;
	}
}
