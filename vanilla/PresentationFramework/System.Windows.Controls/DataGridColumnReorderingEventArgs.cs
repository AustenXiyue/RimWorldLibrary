namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.ColumnReordering" /> event.</summary>
public class DataGridColumnReorderingEventArgs : DataGridColumnEventArgs
{
	private bool _cancel;

	private Control _dropLocationIndicator;

	private Control _dragIndicator;

	/// <summary>Gets or sets a value that indicates whether the reordering operation is stopped before completion.</summary>
	/// <returns>true if the reordering operation is stopped before completion; otherwise, false. The default is false.</returns>
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

	/// <summary>Gets or sets the control that is used to display the visual indicator of the current drop location during a column drag operation.</summary>
	/// <returns>The control that is used to display the drop location indicator during a column drag operation.</returns>
	public Control DropLocationIndicator
	{
		get
		{
			return _dropLocationIndicator;
		}
		set
		{
			_dropLocationIndicator = value;
		}
	}

	/// <summary>Gets or sets the control that is used to display the visual indicator of the header for the column that is being dragged.</summary>
	/// <returns>The control that is used to display a dragged column header.</returns>
	public Control DragIndicator
	{
		get
		{
			return _dragIndicator;
		}
		set
		{
			_dragIndicator = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridColumnReorderingEventArgs" /> class.</summary>
	/// <param name="dataGridColumn">The column that is being moved.</param>
	public DataGridColumnReorderingEventArgs(DataGridColumn dataGridColumn)
		: base(dataGridColumn)
	{
	}
}
