namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.LoadingRowDetails" />, <see cref="E:System.Windows.Controls.DataGrid.UnloadingRowDetails" />, and <see cref="E:System.Windows.Controls.DataGrid.RowDetailsVisibilityChanged" /> events.</summary>
public class DataGridRowDetailsEventArgs : EventArgs
{
	/// <summary>Gets the row details section as a framework element. </summary>
	/// <returns>The row details section as a framework element. </returns>
	public FrameworkElement DetailsElement { get; private set; }

	/// <summary>Gets the row for which the event occurred.</summary>
	/// <returns>The row for which the event occurred.</returns>
	public DataGridRow Row { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridRowDetailsEventArgs" /> class. </summary>
	/// <param name="row">The row for which the event occurred.</param>
	/// <param name="detailsElement">The row details section as a framework element. </param>
	public DataGridRowDetailsEventArgs(DataGridRow row, FrameworkElement detailsElement)
	{
		Row = row;
		DetailsElement = detailsElement;
	}
}
