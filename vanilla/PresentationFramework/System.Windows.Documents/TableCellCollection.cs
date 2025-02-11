using System.Collections;
using System.Collections.Generic;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>Provides standard facilities for creating and managing a type-safe, ordered collection of <see cref="T:System.Windows.Documents.TableCell" /> objects.</summary>
public sealed class TableCellCollection : IList<TableCell>, ICollection<TableCell>, IEnumerable<TableCell>, IEnumerable, IList, ICollection
{
	private TableTextElementCollectionInternal<TableRow, TableCell> _cellCollectionInternal;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Documents.TableCellCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => ((IList)_cellCollectionInternal).IsFixedSize;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Documents.TableCellCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((IList)_cellCollectionInternal).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			return ((IList)_cellCollectionInternal)[index];
		}
		set
		{
			((IList)_cellCollectionInternal)[index] = value;
		}
	}

	/// <summary>Gets the number of items currently contained by the collection.</summary>
	/// <returns>The number of items currently contained by the collection.</returns>
	public int Count => _cellCollectionInternal.Count;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Currently, this property always returns false.</returns>
	public bool IsReadOnly => ((IList)_cellCollectionInternal).IsReadOnly;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Currently, this property always returns false.</returns>
	public bool IsSynchronized => ((ICollection)_cellCollectionInternal).IsSynchronized;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot => ((ICollection)_cellCollectionInternal).SyncRoot;

	/// <summary>Gets or sets the preallocated collection item capacity for this collection.</summary>
	/// <returns>The preallocated collection item capacity for this collection. The default value is 8.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when an attempt is made to set <see cref="P:System.Windows.Documents.TableCellCollection.Capacity" /> to a value that is less than the current value of <see cref="P:System.Windows.Documents.TableCellCollection.Count" />.</exception>
	public int Capacity
	{
		get
		{
			return _cellCollectionInternal.Capacity;
		}
		set
		{
			_cellCollectionInternal.Capacity = value;
		}
	}

	/// <summary>Gets or sets the collection item at a specified index. This is an indexed property.</summary>
	/// <returns>The collection item at the specified index.</returns>
	/// <param name="index">A zero-based index that specifies the position of the collection item.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when <paramref name="index" /> is less than zero, or when <paramref name="index" /> is greater than or equal to <see cref="P:System.Windows.Documents.TableCellCollection.Count" />.</exception>
	public TableCell this[int index]
	{
		get
		{
			return _cellCollectionInternal[index];
		}
		set
		{
			_cellCollectionInternal[index] = value;
		}
	}

	private int PrivateCapacity
	{
		get
		{
			return _cellCollectionInternal.PrivateCapacity;
		}
		set
		{
			_cellCollectionInternal.PrivateCapacity = value;
		}
	}

	internal TableCellCollection(TableRow owner)
	{
		_cellCollectionInternal = new TableTextElementCollectionInternal<TableRow, TableCell>(owner);
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified array starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional array to which the collection contents will be copied. This array must use zero-based indexing.</param>
	/// <param name="index">A zero-based index in <paramref name="array" /> specifying the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="array" /> includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TableCell" />, or if <paramref name="index" /> specifies a position that falls outside the bounds of <paramref name="array" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when <paramref name="index" /> is less than 0.</exception>
	public void CopyTo(Array array, int index)
	{
		_cellCollectionInternal.CopyTo(array, index);
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified <see cref="T:System.Windows.Documents.TableCell" /> array of starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional <see cref="T:System.Windows.Documents.TableCell" /> array to which the collection contents will be copied. This array must use zero-based indexing.</param>
	/// <param name="index">A zero-based index in <paramref name="array" /> that specifies the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="array" /> includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TableCell" />, or if <paramref name="index" /> specifies a position that falls outside of the bounds of <paramref name="array" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when <paramref name="index" /> is less than 0.</exception>
	public void CopyTo(TableCell[] array, int index)
	{
		_cellCollectionInternal.CopyTo(array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _cellCollectionInternal.GetEnumerator();
	}

	IEnumerator<TableCell> IEnumerable<TableCell>.GetEnumerator()
	{
		return ((IEnumerable<TableCell>)_cellCollectionInternal).GetEnumerator();
	}

	/// <summary>Appends a specified <see cref="T:System.Windows.Documents.TableCell" /> to the collection of table cells.</summary>
	/// <param name="item">The <see cref="T:System.Windows.Documents.TableCell" /> to append to the collection of table cells.</param>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="item" /> already belongs to a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="item" /> is null.</exception>
	public void Add(TableCell item)
	{
		_cellCollectionInternal.Add(item);
	}

	/// <summary>Clears all items from the collection.</summary>
	public void Clear()
	{
		_cellCollectionInternal.Clear();
	}

	/// <summary>Queries for the presence of a specified item in the collection.</summary>
	/// <returns>true if the specified <paramref name="item" /> is present in the collection; otherwise, false.</returns>
	/// <param name="item">An item to query for the presence of in the collection.</param>
	public bool Contains(TableCell item)
	{
		return _cellCollectionInternal.Contains(item);
	}

	/// <summary>Returns the zero-based index of specified collection item.</summary>
	/// <returns>The zero-based index of the specified collection item, or -1 if the specified item is not a member of the collection.</returns>
	/// <param name="item">A collection item to return the index of.</param>
	public int IndexOf(TableCell item)
	{
		return _cellCollectionInternal.IndexOf(item);
	}

	/// <summary>Inserts a specified item in the collection at a specified index position.</summary>
	/// <param name="index">A zero-based index that specifies the position in the collection at which to insert <paramref name="item" />.</param>
	/// <param name="item">An item to insert into the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when <paramref name="index" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="item" /> is null.</exception>
	public void Insert(int index, TableCell item)
	{
		_cellCollectionInternal.Insert(index, item);
	}

	/// <summary>Removes a specified item from the collection.</summary>
	/// <returns>true if the specified item was found and removed; otherwise, false.</returns>
	/// <param name="item">An item to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised if <paramref name="item" /> is not present in the collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="item" /> is null.</exception>
	public bool Remove(TableCell item)
	{
		return _cellCollectionInternal.Remove(item);
	}

	/// <summary>Removes an item, specified by index, from the collection.</summary>
	/// <param name="index">A zero-based index that specifies the collection item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when <paramref name="index" /> is less than zero, or when <paramref name="index" /> is greater than or equal to <see cref="P:System.Windows.Documents.TableCellCollection.Count" />.</exception>
	public void RemoveAt(int index)
	{
		_cellCollectionInternal.RemoveAt(index);
	}

	/// <summary>Removes a range of items, specified by beginning index and count, from the collection.</summary>
	/// <param name="index">A zero-based index that indicates the beginning of a range of items to remove.</param>
	/// <param name="count">The number of items to remove, beginning from the position specified by <paramref name="index" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when <paramref name="index" /> or <paramref name="count" /> is less than zero, or when <paramref name="index" /> is greater than or equal to <see cref="P:System.Windows.Documents.TableCellCollection.Count" />.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="index" /> and <paramref name="count" /> do not specify a valid range in this collection.</exception>
	public void RemoveRange(int index, int count)
	{
		_cellCollectionInternal.RemoveRange(index, count);
	}

	/// <summary>Optimizes memory consumption for the collection by setting the underlying collection <see cref="P:System.Windows.Documents.TableCellCollection.Capacity" /> equal to the <see cref="P:System.Windows.Documents.TableCellCollection.Count" /> of items currently in the collection.</summary>
	public void TrimToSize()
	{
		_cellCollectionInternal.TrimToSize();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Documents.TableCellCollection" />.</param>
	int IList.Add(object value)
	{
		return ((IList)_cellCollectionInternal).Add(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Clear" />.</summary>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Documents.TableCellCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Documents.TableCellCollection" />.</param>
	bool IList.Contains(object value)
	{
		return ((IList)_cellCollectionInternal).Contains(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Documents.TableCellCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return ((IList)_cellCollectionInternal).IndexOf(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Documents.TableCellCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		((IList)_cellCollectionInternal).Insert(index, value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Documents.TableCellCollection" />.</param>
	void IList.Remove(object value)
	{
		((IList)_cellCollectionInternal).Remove(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.RemoveAt(System.Int32)" />.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	void IList.RemoveAt(int index)
	{
		((IList)_cellCollectionInternal).RemoveAt(index);
	}

	internal void InternalAdd(TableCell item)
	{
		_cellCollectionInternal.InternalAdd(item);
	}

	internal void InternalRemove(TableCell item)
	{
		_cellCollectionInternal.InternalRemove(item);
	}

	private void EnsureCapacity(int min)
	{
		_cellCollectionInternal.EnsureCapacity(min);
	}

	private void PrivateConnectChild(int index, TableCell item)
	{
		_cellCollectionInternal.PrivateConnectChild(index, item);
	}

	private void PrivateDisconnectChild(TableCell item)
	{
		_cellCollectionInternal.PrivateDisconnectChild(item);
	}

	private bool BelongsToOwner(TableCell item)
	{
		return _cellCollectionInternal.BelongsToOwner(item);
	}

	private int FindInsertionIndex(TableCell item)
	{
		return _cellCollectionInternal.FindInsertionIndex(item);
	}
}
