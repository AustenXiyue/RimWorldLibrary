namespace System.Windows.Controls;

[Flags]
internal enum DataGridNotificationTarget
{
	None = 0,
	Cells = 1,
	CellsPresenter = 2,
	Columns = 4,
	ColumnCollection = 8,
	ColumnHeaders = 0x10,
	ColumnHeadersPresenter = 0x20,
	DataGrid = 0x40,
	DetailsPresenter = 0x80,
	RefreshCellContent = 0x100,
	RowHeaders = 0x200,
	Rows = 0x400,
	All = 0xFFF
}
