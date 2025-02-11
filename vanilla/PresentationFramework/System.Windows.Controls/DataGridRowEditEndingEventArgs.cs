namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.RowEditEnding" /> event. </summary>
public class DataGridRowEditEndingEventArgs : EventArgs
{
	private bool _cancel;

	private DataGridRow _dataGridRow;

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

	/// <summary>Gets the row for which the event occurred. </summary>
	/// <returns>The row for which the event occurred. </returns>
	public DataGridRow Row => _dataGridRow;

	/// <summary>Gets a value that indicates whether the edit was canceled or committed. </summary>
	/// <returns>A value that indicates whether the edit was canceled or committed. </returns>
	public DataGridEditAction EditAction => _editAction;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridRowEditEndingEventArgs" /> class. </summary>
	/// <param name="row">The row for which the event occurred. </param>
	/// <param name="editAction">A value that indicates whether the edit was canceled or committed. </param>
	public DataGridRowEditEndingEventArgs(DataGridRow row, DataGridEditAction editAction)
	{
		_dataGridRow = row;
		_editAction = editAction;
	}
}
