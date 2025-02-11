namespace System.Windows.Controls;

/// <summary>Encapsulates the value and location of a <see cref="T:System.Windows.Controls.DataGrid" /> cell for use when copying content to the Clipboard.</summary>
public struct DataGridClipboardCellContent
{
	private object _item;

	private DataGridColumn _column;

	private object _content;

	/// <summary>Gets the data item for the row that contains the cell being copied. </summary>
	/// <returns>The data item for the row that contains the cell being copied. </returns>
	public object Item => _item;

	/// <summary>Gets the column that contains the cell being copied. </summary>
	/// <returns>The column that contains the cell being copied. </returns>
	public DataGridColumn Column => _column;

	/// <summary>Gets the text value of the cell being copied.</summary>
	/// <returns>The text value of the cell being copied.</returns>
	public object Content => _content;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> structure. </summary>
	/// <param name="item">The data item for the row that contains the cell being copied. </param>
	/// <param name="column">The column that contains the cell being copied. </param>
	/// <param name="content">The text value of the cell being copied. </param>
	public DataGridClipboardCellContent(object item, DataGridColumn column, object content)
	{
		_item = item;
		_column = column;
		_content = content;
	}

	/// <summary>Indicates whether the current and specified <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instances are equivalent.</summary>
	/// <returns>true if the current and specified <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instances have the same <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Item" />, <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Column" />, and <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Content" /> property values; otherwise, false. </returns>
	/// <param name="data">The <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance to compare with the current <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance.</param>
	public override bool Equals(object data)
	{
		if (data is DataGridClipboardCellContent dataGridClipboardCellContent)
		{
			if (_column == dataGridClipboardCellContent._column && _content == dataGridClipboardCellContent._content)
			{
				return _item == dataGridClipboardCellContent._item;
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance.</summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance.</returns>
	public override int GetHashCode()
	{
		return ((_column != null) ? _column.GetHashCode() : 0) ^ ((_content != null) ? _content.GetHashCode() : 0) ^ ((_item != null) ? _item.GetHashCode() : 0);
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instances are equivalent.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instances have the same <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Item" />, <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Column" />, and <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Content" /> property values; otherwise, false. </returns>
	/// <param name="clipboardCellContent1">The first <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance to be compared.</param>
	/// <param name="clipboardCellContent2">The second <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance to be compared.</param>
	public static bool operator ==(DataGridClipboardCellContent clipboardCellContent1, DataGridClipboardCellContent clipboardCellContent2)
	{
		if (clipboardCellContent1._column == clipboardCellContent2._column && clipboardCellContent1._content == clipboardCellContent2._content)
		{
			return clipboardCellContent1._item == clipboardCellContent2._item;
		}
		return false;
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instances are not equivalent.</summary>
	/// <returns>true if the current and specified <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instances do not have the same <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Item" />, <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Column" />, and <see cref="P:System.Windows.Controls.DataGridClipboardCellContent.Content" /> property values; otherwise, false. </returns>
	/// <param name="clipboardCellContent1">The first <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance to be compared.</param>
	/// <param name="clipboardCellContent2">The second <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> instance to be compared.</param>
	public static bool operator !=(DataGridClipboardCellContent clipboardCellContent1, DataGridClipboardCellContent clipboardCellContent2)
	{
		if (clipboardCellContent1._column == clipboardCellContent2._column && clipboardCellContent1._content == clipboardCellContent2._content)
		{
			return clipboardCellContent1._item != clipboardCellContent2._item;
		}
		return true;
	}
}
