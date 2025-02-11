namespace System.Windows.Controls;

/// <summary>Defines constants that specify whether cells, rows, or both, are used for selection in a <see cref="T:System.Windows.Controls.DataGrid" /> control.</summary>
public enum DataGridSelectionUnit
{
	/// <summary>Only cells are selectable. Clicking a cell selects the cell. Clicking a row or column header does nothing.</summary>
	Cell,
	/// <summary>Only full rows are selectable. Clicking a cell or a row header selects the full row.</summary>
	FullRow,
	/// <summary>Cells and rows are selectable. Clicking a cell selects only the cell. Clicking a row header selects the full row.</summary>
	CellOrRowHeader
}
