using System.Collections.Generic;
using System.Text;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.CopyingRowClipboardContent" /> event. </summary>
public class DataGridRowClipboardEventArgs : EventArgs
{
	private int _startColumnDisplayIndex;

	private int _endColumnDisplayIndex;

	private object _item;

	private bool _isColumnHeadersRow;

	private List<DataGridClipboardCellContent> _clipboardRowContent;

	private int _rowIndexHint = -1;

	/// <summary>Gets the data item for the row for which the event occurred.</summary>
	/// <returns>The data item for the row for which the event occurred.</returns>
	public object Item => _item;

	/// <summary>Gets a list of <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> values that represent the text values of the cells being copied. </summary>
	/// <returns>A list of <see cref="T:System.Windows.Controls.DataGridClipboardCellContent" /> values that represent the text values of the cells being copied. </returns>
	public List<DataGridClipboardCellContent> ClipboardRowContent
	{
		get
		{
			if (_clipboardRowContent == null)
			{
				_clipboardRowContent = new List<DataGridClipboardCellContent>();
			}
			return _clipboardRowContent;
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> value of the column that contains the first selected cell in the row.</summary>
	/// <returns>The <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> value of the column that contains the first selected cell in the row.</returns>
	public int StartColumnDisplayIndex => _startColumnDisplayIndex;

	/// <summary>Gets the <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> value of the column that contains the last selected cell in the row. </summary>
	/// <returns>The <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> value of the column that contains the last selected cell in the row. </returns>
	public int EndColumnDisplayIndex => _endColumnDisplayIndex;

	/// <summary>Gets a value that indicates whether the row for which the event occurred represents the column headers. </summary>
	/// <returns>true if the row represents the column headers; otherwise, false.</returns>
	public bool IsColumnHeadersRow => _isColumnHeadersRow;

	internal int RowIndexHint => _rowIndexHint;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridRowClipboardEventArgs" /> class. </summary>
	/// <param name="item">The data item for the row for which the event occurred.</param>
	/// <param name="startColumnDisplayIndex">The <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> value of the column that contains the first selected cell in the row.</param>
	/// <param name="endColumnDisplayIndex">The <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> value of the column that contains the last selected cell in the row. </param>
	/// <param name="isColumnHeadersRow">A value that indicates whether the row for which the event occurred represents the column headers. </param>
	public DataGridRowClipboardEventArgs(object item, int startColumnDisplayIndex, int endColumnDisplayIndex, bool isColumnHeadersRow)
	{
		_item = item;
		_startColumnDisplayIndex = startColumnDisplayIndex;
		_endColumnDisplayIndex = endColumnDisplayIndex;
		_isColumnHeadersRow = isColumnHeadersRow;
	}

	internal DataGridRowClipboardEventArgs(object item, int startColumnDisplayIndex, int endColumnDisplayIndex, bool isColumnHeadersRow, int rowIndexHint)
		: this(item, startColumnDisplayIndex, endColumnDisplayIndex, isColumnHeadersRow)
	{
		_rowIndexHint = rowIndexHint;
	}

	/// <summary>Returns the <see cref="P:System.Windows.Controls.DataGridRowClipboardEventArgs.ClipboardRowContent" /> values as a string in the specified format. </summary>
	/// <returns>The formatted string.</returns>
	/// <param name="format">The data format in which to serialize the cell values. </param>
	public string FormatClipboardCellValues(string format)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int count = ClipboardRowContent.Count;
		for (int i = 0; i < count; i++)
		{
			DataGridClipboardHelper.FormatCell(ClipboardRowContent[i].Content, i == 0, i == count - 1, stringBuilder, format);
		}
		return stringBuilder.ToString();
	}
}
