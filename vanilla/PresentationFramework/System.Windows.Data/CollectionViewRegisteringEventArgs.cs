namespace System.Windows.Data;

/// <summary>Provides data for the <see cref="E:System.Windows.Data.BindingOperations.CollectionViewRegistering" /> event.</summary>
public class CollectionViewRegisteringEventArgs : EventArgs
{
	private CollectionView _view;

	/// <summary>Gets the collection view to be registered for cross-thread access.</summary>
	/// <returns>The collection view to be registered for cross-thread access.</returns>
	public CollectionView CollectionView => _view;

	internal CollectionViewRegisteringEventArgs(CollectionView view)
	{
		_view = view;
	}
}
