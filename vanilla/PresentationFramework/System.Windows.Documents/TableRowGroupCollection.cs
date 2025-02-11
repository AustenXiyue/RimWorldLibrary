using System.Collections;
using System.Collections.Generic;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>Provides standard facilities for creating and managing a type-safe, ordered collection of <see cref="T:System.Windows.Documents.TableRowGroup" /> objects.</summary>
public sealed class TableRowGroupCollection : IList<TableRowGroup>, ICollection<TableRowGroup>, IEnumerable<TableRowGroup>, IEnumerable, IList, ICollection
{
	private TableTextElementCollectionInternal<Table, TableRowGroup> _rowGroupCollectionInternal;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Documents.TableRowGroupCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => ((IList)_rowGroupCollectionInternal).IsFixedSize;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Documents.TableRowGroupCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((IList)_rowGroupCollectionInternal).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			return ((IList)_rowGroupCollectionInternal)[index];
		}
		set
		{
			((IList)_rowGroupCollectionInternal)[index] = value;
		}
	}

	/// <summary>Gets the number of items currently contained by the collection.</summary>
	/// <returns>The number of items currently contained by the collection.</returns>
	public int Count => _rowGroupCollectionInternal.Count;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Currently, this property always returns false.</returns>
	public bool IsReadOnly => _rowGroupCollectionInternal.IsReadOnly;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Currently, this property always returns false.</returns>
	public bool IsSynchronized => _rowGroupCollectionInternal.IsSynchronized;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot => _rowGroupCollectionInternal.SyncRoot;

	/// <summary>Gets or sets the pre-allocated collection item capacity for this collection.</summary>
	/// <returns>The pre-allocated collection item capacity for this collection.  The default value is 8.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when an attempt is made to set <see cref="P:System.Windows.Documents.TableCellCollection.Capacity" /> to a value that is less than the current value of <see cref="P:System.Windows.Documents.TableCellCollection.Count" />.</exception>
	public int Capacity
	{
		get
		{
			return _rowGroupCollectionInternal.Capacity;
		}
		set
		{
			_rowGroupCollectionInternal.Capacity = value;
		}
	}

	/// <summary>Gets the collection item at a specified index.  This is an indexed property.</summary>
	/// <returns>The collection item at the specified index.</returns>
	/// <param name="index">A zero-based index specifying the position of the collection item to retrieve.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index is less than zero, or when index is greater than or equal to <see cref="P:System.Windows.Documents.TableRowGroupCollection.Count" />.</exception>
	public TableRowGroup this[int index]
	{
		get
		{
			return _rowGroupCollectionInternal[index];
		}
		set
		{
			_rowGroupCollectionInternal[index] = value;
		}
	}

	private int PrivateCapacity
	{
		get
		{
			return _rowGroupCollectionInternal.PrivateCapacity;
		}
		set
		{
			_rowGroupCollectionInternal.PrivateCapacity = value;
		}
	}

	internal TableRowGroupCollection(Table owner)
	{
		_rowGroupCollectionInternal = new TableTextElementCollectionInternal<Table, TableRowGroup>(owner);
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified array starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional array to which the collection contents will be copied.  This array must use zero-based indexing.</param>
	/// <param name="index">A zero-based index in <paramref name="array" /> specifying the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when array includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TableRowGroup" />, or if arrayIndex specifies a position that falls outside of the bounds of array.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when array is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when arrayIndex is less than 0.</exception>
	public void CopyTo(Array array, int index)
	{
		_rowGroupCollectionInternal.CopyTo(array, index);
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified <see cref="T:System.Windows.Documents.TableRowGroup" /> array of starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional <see cref="T:System.Windows.Documents.TableRowGroup" /> array to which the collection contents will be copied.  This array must use zero-based indexing.</param>
	/// <param name="index">A zero-based index in <paramref name="array" /> specifying the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when array includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TableRowGroup" />, or if arrayIndex specifies a position that falls outside of the bounds of array.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when array is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when arrayIndex is less than 0.</exception>
	public void CopyTo(TableRowGroup[] array, int index)
	{
		_rowGroupCollectionInternal.CopyTo(array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _rowGroupCollectionInternal.GetEnumerator();
	}

	IEnumerator<TableRowGroup> IEnumerable<TableRowGroup>.GetEnumerator()
	{
		return ((IEnumerable<TableRowGroup>)_rowGroupCollectionInternal).GetEnumerator();
	}

	/// <summary>Appends a specified item to the collection.</summary>
	/// <param name="item">An item to append to the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised when item already belongs to a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public void Add(TableRowGroup item)
	{
		_rowGroupCollectionInternal.Add(item);
	}

	/// <summary>Clears all items from the collection.</summary>
	public void Clear()
	{
		_rowGroupCollectionInternal.Clear();
	}

	/// <summary>Queries for the presence of a specified item in the collection.</summary>
	/// <returns>true if the specified item is present in the collection; otherwise, false.</returns>
	/// <param name="item">An item to query for the presence of in the collection.</param>
	public bool Contains(TableRowGroup item)
	{
		return _rowGroupCollectionInternal.Contains(item);
	}

	/// <summary>Returns the zero-based index of specified collection item.</summary>
	/// <returns>The zero-based index of the specified collection item, or -1 if the specified item is not a member of the collection.</returns>
	/// <param name="item">A collection item to return the index of.</param>
	public int IndexOf(TableRowGroup item)
	{
		return _rowGroupCollectionInternal.IndexOf(item);
	}

	/// <summary>Inserts a specified item in the collection at a specified index position.</summary>
	/// <param name="index">A zero-based index that specifies the position in the collection at which to insert <paramref name="item" />.</param>
	/// <param name="item">An item to insert into the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index is less than 0.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public void Insert(int index, TableRowGroup item)
	{
		_rowGroupCollectionInternal.Insert(index, item);
	}

	/// <summary>Removes a specified item from the collection.</summary>
	/// <returns>true if the specified item was found and removed; otherwise, false.</returns>
	/// <param name="item">An item to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised if item is not present in the collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public bool Remove(TableRowGroup item)
	{
		return _rowGroupCollectionInternal.Remove(item);
	}

	/// <summary>Removes an item, specified by index, from the collection.</summary>
	/// <param name="index">A zero-based index that specifies the collection item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index is less than zero, or when index is greater than or equal to <see cref="P:System.Windows.Documents.TableRowGroupCollection.Count" />.</exception>
	public void RemoveAt(int index)
	{
		_rowGroupCollectionInternal.RemoveAt(index);
	}

	/// <summary>Removes a range of items, specified by beginning index and count, from the collection.</summary>
	/// <param name="index">A zero-based index indicating the beginning of a range of items to remove.</param>
	/// <param name="count">The number of items to remove, beginning form the position specified by <paramref name="index" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index or count is less than zero, or when index is greater than or equal to <see cref="P:System.Windows.Documents.TableRowGroupCollection.Count" />.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when index and count do not specify a valid range in this collection.</exception>
	public void RemoveRange(int index, int count)
	{
		_rowGroupCollectionInternal.RemoveRange(index, count);
	}

	/// <summary>Optimizes memory consumption for the collection by setting the underlying collection <see cref="P:System.Windows.Documents.TableRowGroupCollection.Capacity" /> equal to the <see cref="P:System.Windows.Documents.TableRowGroupCollection.Count" /> of items currently in the collection.</summary>
	public void TrimToSize()
	{
		_rowGroupCollectionInternal.TrimToSize();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Documents.TableRowGroupCollection" />.</param>
	int IList.Add(object value)
	{
		return ((IList)_rowGroupCollectionInternal).Add(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Clear" />.</summary>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Documents.TableRowGroupCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Documents.TableRowGroupCollection" />.</param>
	bool IList.Contains(object value)
	{
		return ((IList)_rowGroupCollectionInternal).Contains(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Documents.TableRowGroupCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return ((IList)_rowGroupCollectionInternal).IndexOf(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Documents.TableRowGroupCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		((IList)_rowGroupCollectionInternal).Insert(index, value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Documents.TableRowGroupCollection" />.</param>
	void IList.Remove(object value)
	{
		((IList)_rowGroupCollectionInternal).Remove(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.RemoveAt(System.Int32)" />.</summary>
	/// <param name="index">The zero-based index of the item to remove. </param>
	void IList.RemoveAt(int index)
	{
		((IList)_rowGroupCollectionInternal).RemoveAt(index);
	}

	internal void InternalAdd(TableRowGroup item)
	{
		_rowGroupCollectionInternal.InternalAdd(item);
	}

	internal void InternalRemove(TableRowGroup item)
	{
		_rowGroupCollectionInternal.InternalRemove(item);
	}

	private void EnsureCapacity(int min)
	{
		_rowGroupCollectionInternal.EnsureCapacity(min);
	}

	private void PrivateConnectChild(int index, TableRowGroup item)
	{
		_rowGroupCollectionInternal.PrivateConnectChild(index, item);
	}

	private void PrivateDisconnectChild(TableRowGroup item)
	{
		_rowGroupCollectionInternal.PrivateDisconnectChild(item);
	}

	private bool BelongsToOwner(TableRowGroup item)
	{
		return _rowGroupCollectionInternal.BelongsToOwner(item);
	}

	private int FindInsertionIndex(TableRowGroup item)
	{
		return _rowGroupCollectionInternal.FindInsertionIndex(item);
	}
}
