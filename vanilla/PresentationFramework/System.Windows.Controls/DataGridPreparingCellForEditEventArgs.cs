namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.PreparingCellForEdit" /> event.</summary>
public class DataGridPreparingCellForEditEventArgs : EventArgs
{
	private DataGridColumn _dataGridColumn;

	private DataGridRow _dataGridRow;

	private RoutedEventArgs _editingEventArgs;

	private FrameworkElement _editingElement;

	/// <summary>Gets the column that contains the cell to be edited. </summary>
	/// <returns>The column that contains the cell to be edited. </returns>
	public DataGridColumn Column => _dataGridColumn;

	/// <summary>Gets the row that contains the cell to be edited. </summary>
	/// <returns>The row that contains the cell to be edited. </returns>
	public DataGridRow Row => _dataGridRow;

	/// <summary>Gets information about the user gesture that caused the cell to enter edit mode.</summary>
	/// <returns>Information about the user gesture that caused the cell to enter edit mode.</returns>
	public RoutedEventArgs EditingEventArgs => _editingEventArgs;

	/// <summary>Gets the element that the column displays for a cell in editing mode.</summary>
	/// <returns>The element that the column displays for a cell in editing mode.</returns>
	public FrameworkElement EditingElement => _editingElement;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridPreparingCellForEditEventArgs" /> class. </summary>
	/// <param name="column">The column that contains the cell to be edited. </param>
	/// <param name="row">The row that contains the cell to be edited. </param>
	/// <param name="editingEventArgs">Information about the user gesture that caused the cell to enter edit mode.</param>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	public DataGridPreparingCellForEditEventArgs(DataGridColumn column, DataGridRow row, RoutedEventArgs editingEventArgs, FrameworkElement editingElement)
	{
		_dataGridColumn = column;
		_dataGridRow = row;
		_editingEventArgs = editingEventArgs;
		_editingElement = editingElement;
	}
}
