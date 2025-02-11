namespace System.Windows.Data;

/// <summary>Provides information and event data that is associated with the <see cref="E:System.Windows.Data.CollectionViewSource.Filter" /> event.</summary>
public class FilterEventArgs : EventArgs
{
	private object _item;

	private bool _accepted;

	/// <summary>Gets the object that the filter should test.</summary>
	/// <returns>The object that the filter should test. The default is null.</returns>
	public object Item => _item;

	/// <summary>Gets or sets a value that indicates whether the item passes the filter.</summary>
	/// <returns>true if the item passes the filter; otherwise, false. The default is false.</returns>
	public bool Accepted
	{
		get
		{
			return _accepted;
		}
		set
		{
			_accepted = value;
		}
	}

	internal FilterEventArgs(object item)
	{
		_item = item;
		_accepted = true;
	}
}
