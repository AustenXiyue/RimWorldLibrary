namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.InitializingNewItem" /> event.</summary>
public class InitializingNewItemEventArgs : EventArgs
{
	private object _newItem;

	/// <summary>Gets the new item added to the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The new item added to the grid.</returns>
	public object NewItem => _newItem;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.InitializingNewItemEventArgs" /> class. </summary>
	/// <param name="newItem">The new item added to the <see cref="T:System.Windows.Controls.DataGrid" />.</param>
	public InitializingNewItemEventArgs(object newItem)
	{
		_newItem = newItem;
	}
}
