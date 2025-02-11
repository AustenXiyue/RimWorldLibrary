namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGridColumn.CopyingCellClipboardContent" /> and <see cref="E:System.Windows.Controls.DataGridColumn.PastingCellClipboardContent" /> events.</summary>
public class DataGridCellClipboardEventArgs : EventArgs
{
	private object _content;

	private object _item;

	private DataGridColumn _column;

	/// <summary>Gets or sets the text value of the cell for which the event occurred.</summary>
	/// <returns>The text value of the cell for which the event occurred.</returns>
	public object Content
	{
		get
		{
			return _content;
		}
		set
		{
			_content = value;
		}
	}

	/// <summary>Gets the data item for the row that contains the cell for which the event occurred.</summary>
	/// <returns>The data item for the row that contains the cell for which the event occurred.</returns>
	public object Item => _item;

	/// <summary>Gets the column that contains the cell for which the event occurred. </summary>
	/// <returns>The column that contains the cell for which the event occurred. </returns>
	public DataGridColumn Column => _column;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCellClipboardEventArgs" /> class. </summary>
	/// <param name="item">The data item for the row that contains the cell for which the event occurred.</param>
	/// <param name="column">The column that contains the cell for which the event occurred. </param>
	/// <param name="content">The text value of the cell for which the event occurred. </param>
	public DataGridCellClipboardEventArgs(object item, DataGridColumn column, object content)
	{
		_item = item;
		_column = column;
		_content = content;
	}
}
