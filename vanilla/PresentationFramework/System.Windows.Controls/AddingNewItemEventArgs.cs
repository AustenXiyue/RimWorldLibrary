namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.AddingNewItem" /> event.</summary>
public class AddingNewItemEventArgs : EventArgs
{
	private object _newItem;

	/// <summary>Gets or sets the item that will be added.</summary>
	/// <returns>The item that will be added.</returns>
	public object NewItem
	{
		get
		{
			return _newItem;
		}
		set
		{
			_newItem = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.AddingNewItemEventArgs" /> class.</summary>
	public AddingNewItemEventArgs()
	{
	}
}
