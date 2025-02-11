namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.ColumnDisplayIndexChanged" /> and <see cref="E:System.Windows.Controls.DataGrid.ColumnReordered" /> events.</summary>
public class DataGridColumnEventArgs : EventArgs
{
	private DataGridColumn _column;

	/// <summary>Gets the column related to the event.</summary>
	/// <returns>An object that represents the column related to the event.</returns>
	public DataGridColumn Column => _column;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridColumnEventArgs" /> class.</summary>
	/// <param name="column">The column related to the event.</param>
	public DataGridColumnEventArgs(DataGridColumn column)
	{
		_column = column;
	}
}
