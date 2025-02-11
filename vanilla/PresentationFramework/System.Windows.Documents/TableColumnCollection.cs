using System.Collections;
using System.Collections.Generic;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>Provides standard facilities for creating and managing a type-safe, ordered collection of <see cref="T:System.Windows.Documents.TableColumn" /> objects.</summary>
public sealed class TableColumnCollection : IList<TableColumn>, ICollection<TableColumn>, IEnumerable<TableColumn>, IEnumerable, IList, ICollection
{
	private TableColumnCollectionInternal _columnCollection;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Documents.TableCellCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => ((IList)_columnCollection).IsFixedSize;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Documents.TableColumnCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((IList)_columnCollection).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />. Use the type-safe <see cref="P:System.Windows.Documents.TableCellCollection.Item(System.Int32)" /> property instead.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			return ((IList)_columnCollection)[index];
		}
		set
		{
			((IList)_columnCollection)[index] = value;
		}
	}

	/// <summary>Gets the number of items currently contained by the collection.</summary>
	/// <returns>The number of items currently contained by the collection.</returns>
	public int Count => _columnCollection.Count;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Currently, this property always returns false.</returns>
	public bool IsReadOnly => _columnCollection.IsReadOnly;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Currently, this property always returns false.</returns>
	public bool IsSynchronized => _columnCollection.IsSynchronized;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot => _columnCollection.SyncRoot;

	/// <summary>Gets or sets the pre-allocated collection item capacity for this collection.</summary>
	/// <returns>The pre-allocated collection item capacity for this collection.  The default value is 8.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when an attempt is made to set <see cref="P:System.Windows.Documents.TableCellCollection.Capacity" /> to a value that is less than the current value of <see cref="P:System.Windows.Documents.TableCellCollection.Count" />.</exception>
	public int Capacity
	{
		get
		{
			return _columnCollection.PrivateCapacity;
		}
		set
		{
			_columnCollection.PrivateCapacity = value;
		}
	}

	/// <summary>Gets the collection item at a specified index.  This is an indexed property.</summary>
	/// <returns>The collection item at the specified index.</returns>
	/// <param name="index">A zero-based index specifying the position of the collection item to retrieve.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index is less than zero, or when index is greater than or equal to <see cref="P:System.Windows.Documents.TableColumnCollection.Count" />.</exception>
	public TableColumn this[int index]
	{
		get
		{
			return _columnCollection[index];
		}
		set
		{
			_columnCollection[index] = value;
		}
	}

	internal TableColumnCollection(Table owner)
	{
		_columnCollection = new TableColumnCollectionInternal(owner);
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified array starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional array to which the collection contents will be copied.  This array must use zero-based indexing.</param>
	/// <param name="index">A zero-based index in <paramref name="array" /> specifying the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when array includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TableColumn" />, or if arrayIndex specifies a position that falls outside of the bounds of array.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when array is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when arrayIndex is less than 0.</exception>
	public void CopyTo(Array array, int index)
	{
		_columnCollection.CopyTo(array, index);
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified <see cref="T:System.Windows.Documents.TableColumn" /> array of starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional <see cref="T:System.Windows.Documents.TableColumn" /> array to which the collection contents will be copied.  This array must use zero-based indexing.</param>
	/// <param name="index">A zero-based index in <paramref name="array" /> specifying the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when array includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TableColumn" />, or if arrayIndex specifies a position that falls outside of the bounds of array.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when array is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when arrayIndex is less than 0.</exception>
	public void CopyTo(TableColumn[] array, int index)
	{
		_columnCollection.CopyTo(array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _columnCollection.GetEnumerator();
	}

	IEnumerator<TableColumn> IEnumerable<TableColumn>.GetEnumerator()
	{
		return ((IEnumerable<TableColumn>)_columnCollection).GetEnumerator();
	}

	/// <summary>Appends a specified item to the collection.</summary>
	/// <param name="item">A table column to append to the collection of columns.</param>
	/// <exception cref="T:System.ArgumentException">Raised when item already belongs to a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public void Add(TableColumn item)
	{
		_columnCollection.Add(item);
	}

	/// <summary>Clears all items from the collection.</summary>
	public void Clear()
	{
		_columnCollection.Clear();
	}

	/// <summary>Queries for the presence of a specified item in the collection.</summary>
	/// <returns>true if the specified item is present in the collection; otherwise, false.</returns>
	/// <param name="item">An item to query for the presence of in the collection.</param>
	public bool Contains(TableColumn item)
	{
		return _columnCollection.Contains(item);
	}

	/// <summary>Returns the zero-based index of specified collection item.</summary>
	/// <returns>The zero-based index of the specified collection item, or -1 if the specified item is not a member of the collection.</returns>
	/// <param name="item">A collection item to return the index of.</param>
	public int IndexOf(TableColumn item)
	{
		return _columnCollection.IndexOf(item);
	}

	/// <summary>Inserts a specified item in the collection at a specified index position.</summary>
	/// <param name="index">A zero-based index that specifies the position in the collection at which to insert <paramref name="item" />.</param>
	/// <param name="item">An item to insert into the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index is less than 0.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public void Insert(int index, TableColumn item)
	{
		_columnCollection.Insert(index, item);
	}

	/// <summary>Removes a specified item from the collection.</summary>
	/// <returns>true if the specified item was found and removed; otherwise, false.</returns>
	/// <param name="item">An item to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised if item is not present in the collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public bool Remove(TableColumn item)
	{
		return _columnCollection.Remove(item);
	}

	/// <summary>Removes an item, specified by index, from the collection.</summary>
	/// <param name="index">A zero-based index that specifies the collection item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index is less than zero, or when index is greater than or equal to <see cref="P:System.Windows.Documents.TableColumnCollection.Count" />.</exception>
	public void RemoveAt(int index)
	{
		_columnCollection.RemoveAt(index);
	}

	/// <summary>Removes a range of items, specified by beginning index and count, from the collection.</summary>
	/// <param name="index">A zero-based index indicating the beginning of a range of items to remove.</param>
	/// <param name="count">The number of items to remove, beginning from the position specified by <paramref name="index" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when index or count is less than zero, or when index is greater than or equal to <see cref="P:System.Windows.Documents.TableColumnCollection.Count" />.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when index and count do not specify a valid range in this collection.</exception>
	public void RemoveRange(int index, int count)
	{
		_columnCollection.RemoveRange(index, count);
	}

	/// <summary>Optimizes memory consumption for the collection by setting the underlying collection <see cref="P:System.Windows.Documents.TableColumnCollection.Capacity" /> equal to the <see cref="P:System.Windows.Documents.TableColumnCollection.Count" /> of items currently in the collection.</summary>
	public void TrimToSize()
	{
		_columnCollection.TrimToSize();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.Add(System.Windows.Documents.TableColumn)" /> method instead.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Documents.TableColumnCollection" />.</param>
	int IList.Add(object value)
	{
		if (!(value is TableColumn))
		{
			throw new ArgumentException(SR.Format(SR.TableCollectionElementTypeExpected, typeof(TableColumn).Name), "value");
		}
		return ((IList)_columnCollection).Add(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Clear" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.Clear" /> method instead.</summary>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.Contains(System.Windows.Documents.TableColumn)" /> method instead.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Documents.TableColumnCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Documents.TableColumnCollection" />.</param>
	bool IList.Contains(object value)
	{
		return ((IList)_columnCollection).Contains(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.IndexOf(System.Windows.Documents.TableColumn)" /> method instead.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Documents.TableColumnCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return ((IList)_columnCollection).IndexOf(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.Insert(System.Int32,System.Windows.Documents.TableColumn)" /> method instead.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Documents.TableColumnCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		((IList)_columnCollection).Insert(index, value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.Remove(System.Windows.Documents.TableColumn)" />, <see cref="M:System.Windows.Documents.TableColumnCollection.RemoveAt(System.Int32)" />, or <see cref="M:System.Windows.Documents.TableColumnCollection.RemoveRange(System.Int32,System.Int32)" /> methods instead.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Documents.TableColumnCollection" />.</param>
	void IList.Remove(object value)
	{
		((IList)_columnCollection).Remove(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.RemoveAt(System.Int32)" />. Use the type-safe <see cref="M:System.Windows.Documents.TableColumnCollection.Remove(System.Windows.Documents.TableColumn)" />, <see cref="M:System.Windows.Documents.TableColumnCollection.RemoveAt(System.Int32)" />, or <see cref="M:System.Windows.Documents.TableColumnCollection.RemoveRange(System.Int32,System.Int32)" /> methods instead.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	void IList.RemoveAt(int index)
	{
		((IList)_columnCollection).RemoveAt(index);
	}
}
