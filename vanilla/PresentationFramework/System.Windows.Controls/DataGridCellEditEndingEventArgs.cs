namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.CellEditEnding" /> event. </summary>
public class DataGridCellEditEndingEventArgs : EventArgs
{
	private bool _cancel;

	private DataGridColumn _dataGridColumn;

	private DataGridRow _dataGridRow;

	private FrameworkElement _editingElement;

	private DataGridEditAction _editAction;

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

	/// <summary>Gets the column that contains the cell for which the event occurred. </summary>
	/// <returns>The column that contains the cell for which the event occurred. </returns>
	public DataGridColumn Column => _dataGridColumn;

	/// <summary>Gets the row that contains the cell for which the event occurred. </summary>
	/// <returns>The row that contains the cell for which the event occurred. </returns>
	public DataGridRow Row => _dataGridRow;

	/// <summary>Gets the element that the cell displays in editing mode.</summary>
	/// <returns>The element that the cell displays in editing mode.</returns>
	public FrameworkElement EditingElement => _editingElement;

	/// <summary>Gets a value that indicates whether the edit was canceled or committed. </summary>
	/// <returns>A value that indicates whether the edit was canceled or committed. </returns>
	public DataGridEditAction EditAction => _editAction;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCellEditEndingEventArgs" /> class. </summary>
	/// <param name="column">The column that contains the cell for which the event occurred. </param>
	/// <param name="row">The row that contains the cell for which the event occurred. </param>
	/// <param name="editingElement">The element that the cell displays in editing mode.</param>
	/// <param name="editAction">A value that indicates whether the edit was canceled or committed. </param>
	public DataGridCellEditEndingEventArgs(DataGridColumn column, DataGridRow row, FrameworkElement editingElement, DataGridEditAction editAction)
	{
		_dataGridColumn = column;
		_dataGridRow = row;
		_editingElement = editingElement;
		_editAction = editAction;
	}
}
