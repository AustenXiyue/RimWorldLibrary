namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.BeginningEdit" /> event. </summary>
public class DataGridBeginningEditEventArgs : EventArgs
{
	private bool _cancel;

	private DataGridColumn _dataGridColumn;

	private DataGridRow _dataGridRow;

	private RoutedEventArgs _editingEventArgs;

	/// <summary>Gets or sets a value that indicates whether the event should be canceled. </summary>
	/// <returns>true if the event should be canceled; otherwise, false. The default is false. </returns>
	public bool Cancel
	{
		get
		{
			return _cancel;
		}
		set
		{
			_cancel = value;
		}
	}

	/// <summary>Gets the column that contains the cell to be edited. </summary>
	/// <returns>The column that contains the cell to be edited. </returns>
	public DataGridColumn Column => _dataGridColumn;

	/// <summary>Gets the row that contains the cell to be edited. </summary>
	/// <returns>The row that contains the cell to be edited. </returns>
	public DataGridRow Row => _dataGridRow;

	/// <summary>Gets information about the user gesture that caused the cell to enter edit mode.</summary>
	/// <returns>Information about the user gesture that caused the cell to enter edit mode.</returns>
	public RoutedEventArgs EditingEventArgs => _editingEventArgs;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridBeginningEditEventArgs" /> class. </summary>
	/// <param name="column">The column that contains the cell to be edited. </param>
	/// <param name="row">The row that contains the cell to be edited. </param>
	/// <param name="editingEventArgs">Information about the user gesture that caused the cell to enter edit mode.</param>
	public DataGridBeginningEditEventArgs(DataGridColumn column, DataGridRow row, RoutedEventArgs editingEventArgs)
	{
		_dataGridColumn = column;
		_dataGridRow = row;
		_editingEventArgs = editingEventArgs;
	}
}
