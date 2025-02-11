using System.Collections;

namespace System.Windows.Data;

/// <summary>Provides data for the <see cref="E:System.Windows.Data.BindingOperations.CollectionRegistering" /> event.</summary>
public class CollectionRegisteringEventArgs : EventArgs
{
	private IEnumerable _collection;

	private object _parent;

	/// <summary>Gets the collection to be registered for cross-thread access.</summary>
	/// <returns>The collection to be registered for cross-thread access.</returns>
	public IEnumerable Collection => _collection;

	/// <summary>Gets the parent of the collection to register.</summary>
	/// <returns>The parent of the collection to register.</returns>
	public object Parent => _parent;

	internal CollectionRegisteringEventArgs(IEnumerable collection, object parent = null)
	{
		_collection = collection;
		_parent = parent;
	}
}
