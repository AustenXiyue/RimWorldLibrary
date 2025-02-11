namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.LoadingRow" /> and <see cref="E:System.Windows.Controls.DataGrid.UnloadingRow" /> events. </summary>
public class DataGridRowEventArgs : EventArgs
{
	/// <summary>Gets the row for which the event occurred. </summary>
	/// <returns>The row for which the event occurred. </returns>
	public DataGridRow Row { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridRowEventArgs" /> class. </summary>
	/// <param name="row">The row for which the event occurred. </param>
	public DataGridRowEventArgs(DataGridRow row)
	{
		Row = row;
	}
}
