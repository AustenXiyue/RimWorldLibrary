namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.Sorting" /> event. </summary>
public class DataGridSortingEventArgs : DataGridColumnEventArgs
{
	private bool _handled;

	/// <summary>Gets or sets a value that specifies whether the routed event is handled.</summary>
	/// <returns>true if the event has been handled; otherwise, false. The default is false.</returns>
	public bool Handled
	{
		get
		{
			return _handled;
		}
		set
		{
			_handled = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridSortingEventArgs" /> class.</summary>
	/// <param name="column">The column that is being sorted.</param>
	public DataGridSortingEventArgs(DataGridColumn column)
		: base(column)
	{
	}
}
