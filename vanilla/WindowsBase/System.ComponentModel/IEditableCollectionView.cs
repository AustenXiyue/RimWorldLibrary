namespace System.ComponentModel;

/// <summary>Defines methods and properties that a <see cref="T:System.Windows.Data.CollectionView" /> implements to provide editing capabilities to a collection.</summary>
public interface IEditableCollectionView
{
	/// <summary>Gets or sets the position of the new item placeholder in the collection view.</summary>
	/// <returns>One of the enumeration values that specifies the position of the new item placeholder in the collection view.</returns>
	NewItemPlaceholderPosition NewItemPlaceholderPosition { get; set; }

	/// <summary>Gets a value that indicates whether a new item can be added to the collection.</summary>
	/// <returns>true if a new item can be added to the collection; otherwise, false.</returns>
	bool CanAddNew { get; }

	/// <summary>Gets a value that indicates whether an add transaction is in progress.</summary>
	/// <returns>true if an add transaction is in progress; otherwise, false.</returns>
	bool IsAddingNew { get; }

	/// <summary>Gets the item that is being added during the current add transaction.</summary>
	/// <returns>The item that is being added if <see cref="P:System.ComponentModel.IEditableCollectionView.IsAddingNew" /> is true; otherwise, null.</returns>
	object CurrentAddItem { get; }

	/// <summary>Gets a value that indicates whether an item can be removed from the collection.</summary>
	/// <returns>true if an item can be removed from the collection; otherwise, false.</returns>
	bool CanRemove { get; }

	/// <summary>Gets a value that indicates whether the collection view can discard pending changes and restore the original values of an edited object.</summary>
	/// <returns>true if the collection view can discard pending changes and restore the original values of an edited object; otherwise, false.</returns>
	bool CanCancelEdit { get; }

	/// <summary>Gets a value that indicates whether an edit transaction is in progress.</summary>
	/// <returns>true if an edit transaction is in progress; otherwise, false.</returns>
	bool IsEditingItem { get; }

	/// <summary>Gets the item in the collection that is being edited.</summary>
	/// <returns>The item in the collection that is being edited if <see cref="P:System.ComponentModel.IEditableCollectionView.IsEditingItem" /> is true; otherwise, null.</returns>
	object CurrentEditItem { get; }

	/// <summary>Adds a new item to the collection.</summary>
	/// <returns>The new item that is added to the collection.</returns>
	object AddNew();

	/// <summary>Ends the add transaction and saves the pending new item.</summary>
	void CommitNew();

	/// <summary>Ends the add transaction and discards the pending new item.</summary>
	void CancelNew();

	/// <summary>Removes the item at the specified position from the collection.</summary>
	/// <param name="index">The position of the item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0 or greater than the number of items in the collection view.</exception>
	void RemoveAt(int index);

	/// <summary>Removes the specified item from the collection.</summary>
	/// <param name="item">The item to remove.</param>
	void Remove(object item);

	/// <summary>Begins an edit transaction of the specified item.</summary>
	/// <param name="item">The item to edit.</param>
	void EditItem(object item);

	/// <summary>Ends the edit transaction and saves the pending changes.</summary>
	void CommitEdit();

	/// <summary>Ends the edit transaction and, if possible, restores the original value to the item.</summary>
	void CancelEdit();
}
