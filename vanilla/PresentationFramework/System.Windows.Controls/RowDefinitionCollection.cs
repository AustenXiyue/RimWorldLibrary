using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>Provides access to an ordered, strongly typed collection of <see cref="T:System.Windows.Controls.RowDefinition" /> objects.</summary>
public sealed class RowDefinitionCollection : IList<RowDefinition>, ICollection<RowDefinition>, IEnumerable<RowDefinition>, IEnumerable, IList, ICollection
{
	internal struct Enumerator : IEnumerator<RowDefinition>, IEnumerator, IDisposable
	{
		private RowDefinitionCollection _collection;

		private int _index;

		private int _version;

		private object _currentElement;

		object IEnumerator.Current
		{
			get
			{
				if (_currentElement == _collection)
				{
					if (_index == -1)
					{
						throw new InvalidOperationException(SR.EnumeratorNotStarted);
					}
					throw new InvalidOperationException(SR.EnumeratorReachedEnd);
				}
				return _currentElement;
			}
		}

		public RowDefinition Current
		{
			get
			{
				if (_currentElement == _collection)
				{
					if (_index == -1)
					{
						throw new InvalidOperationException(SR.EnumeratorNotStarted);
					}
					throw new InvalidOperationException(SR.EnumeratorReachedEnd);
				}
				return (RowDefinition)_currentElement;
			}
		}

		internal Enumerator(RowDefinitionCollection collection)
		{
			_collection = collection;
			_index = -1;
			_version = ((_collection != null) ? _collection._version : (-1));
			_currentElement = collection;
		}

		public bool MoveNext()
		{
			if (_collection == null)
			{
				return false;
			}
			PrivateValidate();
			if (_index < _collection._size - 1)
			{
				_index++;
				_currentElement = _collection[_index];
				return true;
			}
			_currentElement = _collection;
			_index = _collection._size;
			return false;
		}

		public void Reset()
		{
			if (_collection != null)
			{
				PrivateValidate();
				_currentElement = _collection;
				_index = -1;
			}
		}

		public void Dispose()
		{
			_currentElement = null;
		}

		private void PrivateValidate()
		{
			if (_currentElement == null)
			{
				throw new InvalidOperationException(SR.EnumeratorCollectionDisposed);
			}
			if (_version != _collection._version)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
		}
	}

	private readonly Grid _owner;

	private DefinitionBase[] _items;

	private int _size;

	private int _version;

	private const int c_defaultCapacity = 4;

	/// <summary>Gets the total number of items within this instance of <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <returns>The total number of items in the collection. This property has no default value.</returns>
	public int Count => _size;

	/// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize
	{
		get
		{
			if (!_owner.MeasureOverrideInProgress)
			{
				return _owner.ArrangeOverrideInProgress;
			}
			return true;
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> is read-only. </summary>
	/// <returns>true if the collection is read-only; otherwise false. This property has no default value.</returns>
	public bool IsReadOnly
	{
		get
		{
			if (!_owner.MeasureOverrideInProgress)
			{
				return _owner.ArrangeOverrideInProgress;
			}
			return true;
		}
	}

	/// <summary>Gets a value that indicates whether access to this <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> is synchronized (thread-safe).</summary>
	/// <returns>true if access to this collection is synchronized; otherwise, false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</returns>
	public object SyncRoot => this;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index position in the list.</exception>
	object IList.this[int index]
	{
		get
		{
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
			}
			return _items[index];
		}
		set
		{
			PrivateVerifyWriteAccess();
			PrivateValidateValueForAddition(value);
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
			}
			PrivateDisconnectChild(_items[index]);
			PrivateConnectChild(index, value as RowDefinition);
		}
	}

	/// <summary>Gets a value that indicates the current item within a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />. </summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The current item in the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index position in the collection.</exception>
	public RowDefinition this[int index]
	{
		get
		{
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
			}
			return (RowDefinition)_items[index];
		}
		set
		{
			PrivateVerifyWriteAccess();
			PrivateValidateValueForAddition(value);
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
			}
			PrivateDisconnectChild(_items[index]);
			PrivateConnectChild(index, value);
		}
	}

	internal int InternalCount => _size;

	internal DefinitionBase[] InternalItems => _items;

	internal RowDefinitionCollection(Grid owner)
	{
		_owner = owner;
		PrivateOnModified();
	}

	/// <summary>Copies the elements of the collection to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.GridCollection_DestArrayInvalidRank);
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException(SR.Format(SR.GridCollection_DestArrayInvalidLowerBound, "index"));
		}
		if (array.Length - index < _size)
		{
			throw new ArgumentException(SR.Format(SR.GridCollection_DestArrayInvalidLength, "array"));
		}
		if (_size > 0)
		{
			Array.Copy(_items, 0, array, index, _size);
		}
	}

	/// <summary>Copies an array of <see cref="T:System.Windows.Controls.RowDefinition" /> objects to a given index position within a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <param name="array">An array of <see cref="T:System.Windows.Controls.RowDefinition" /> objects.</param>
	/// <param name="index">Identifies the index position within <paramref name="array" /> to which the <see cref="T:System.Windows.Controls.RowDefinition" /> objects are copied.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from index to the end of the destination array. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	public void CopyTo(RowDefinition[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException(SR.Format(SR.GridCollection_DestArrayInvalidLowerBound, "index"));
		}
		if (array.Length - index < _size)
		{
			throw new ArgumentException(SR.Format(SR.GridCollection_DestArrayInvalidLength, "array"));
		}
		if (_size > 0)
		{
			Array.Copy(_items, 0, array, index, _size);
		}
	}

	/// <summary>Adds an item to the collection.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</param>
	int IList.Add(object value)
	{
		PrivateVerifyWriteAccess();
		PrivateValidateValueForAddition(value);
		PrivateInsert(_size, value as RowDefinition);
		return _size - 1;
	}

	/// <summary>Adds a <see cref="T:System.Windows.Controls.RowDefinition" /> element to a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <param name="value">Identifies the <see cref="T:System.Windows.Controls.RowDefinition" /> to add to the collection.</param>
	public void Add(RowDefinition value)
	{
		PrivateVerifyWriteAccess();
		PrivateValidateValueForAddition(value);
		PrivateInsert(_size, value);
	}

	/// <summary>Clears the content of the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	public void Clear()
	{
		PrivateVerifyWriteAccess();
		PrivateOnModified();
		for (int i = 0; i < _size; i++)
		{
			PrivateDisconnectChild(_items[i]);
			_items[i] = null;
		}
		_size = 0;
	}

	/// <summary>Determines whether the collection contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</param>
	bool IList.Contains(object value)
	{
		if (value is RowDefinition rowDefinition && rowDefinition.Parent == _owner)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether a given <see cref="T:System.Windows.Controls.RowDefinition" /> exists within a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.RowDefinition" /> exists within the collection; otherwise false.</returns>
	/// <param name="value">Identifies the <see cref="T:System.Windows.Controls.RowDefinition" /> that is being tested.</param>
	public bool Contains(RowDefinition value)
	{
		if (value != null && value.Parent == _owner)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines the index of a specific item in the collection.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as RowDefinition);
	}

	/// <summary>Returns the index position of a given <see cref="T:System.Windows.Controls.RowDefinition" /> within a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the collection; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Controls.RowDefinition" /> whose index position is desired.</param>
	public int IndexOf(RowDefinition value)
	{
		if (value == null || value.Parent != _owner)
		{
			return -1;
		}
		return value.Index;
	}

	/// <summary>Inserts an item to the collection at the specified index.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		PrivateVerifyWriteAccess();
		if (index < 0 || index > _size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		PrivateValidateValueForAddition(value);
		PrivateInsert(index, value as RowDefinition);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Controls.RowDefinition" /> at the specified index position within a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />. </summary>
	/// <param name="index">The position within the collection where the item is inserted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Controls.RowDefinition" /> to insert.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />. </exception>
	public void Insert(int index, RowDefinition value)
	{
		PrivateVerifyWriteAccess();
		if (index < 0 || index > _size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		PrivateValidateValueForAddition(value);
		PrivateInsert(index, value);
	}

	/// <summary>Removes the first occurrence of a specific object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</param>
	void IList.Remove(object value)
	{
		PrivateVerifyWriteAccess();
		if (PrivateValidateValueForRemoval(value))
		{
			PrivateRemove(value as RowDefinition);
		}
	}

	/// <summary>Removes a <see cref="T:System.Windows.Controls.RowDefinition" /> from a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.RowDefinition" /> was found in the collection and removed; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Controls.RowDefinition" /> to remove from the collection.</param>
	public bool Remove(RowDefinition value)
	{
		bool num = PrivateValidateValueForRemoval(value);
		if (num)
		{
			PrivateRemove(value);
		}
		return num;
	}

	/// <summary>Removes a <see cref="T:System.Windows.Controls.RowDefinition" /> from a <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> at the specified index position.</summary>
	/// <param name="index">The position within the collection at which the <see cref="T:System.Windows.Controls.RowDefinition" /> is removed.</param>
	public void RemoveAt(int index)
	{
		PrivateVerifyWriteAccess();
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		PrivateRemove(_items[index]);
	}

	/// <summary>Removes a range of <see cref="T:System.Windows.Controls.RowDefinition" /> objects from a <see cref="T:System.Windows.Controls.RowDefinitionCollection" />. </summary>
	/// <param name="index">The position within the collection at which the first <see cref="T:System.Windows.Controls.RowDefinition" /> is removed.</param>
	/// <param name="count">The total number of <see cref="T:System.Windows.Controls.RowDefinition" /> objects to remove from the collection.</param>
	public void RemoveRange(int index, int count)
	{
		PrivateVerifyWriteAccess();
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionCountNeedNonNegNum);
		}
		if (_size - index < count)
		{
			throw new ArgumentException(SR.TableCollectionRangeOutOfRange);
		}
		PrivateOnModified();
		if (count > 0)
		{
			for (int num = index + count - 1; num >= index; num--)
			{
				PrivateDisconnectChild(_items[num]);
			}
			_size -= count;
			for (int i = index; i < _size; i++)
			{
				_items[i] = _items[i + count];
				_items[i].Index = i;
				_items[i + count] = null;
			}
		}
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<RowDefinition> IEnumerable<RowDefinition>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	internal void InternalTrimToSize()
	{
		PrivateSetCapacity(_size);
	}

	private void PrivateVerifyWriteAccess()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.Format(SR.GridCollection_CannotModifyReadOnly, "RowDefinitionCollection"));
		}
	}

	private void PrivateValidateValueForAddition(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (((value as RowDefinition) ?? throw new ArgumentException(SR.Format(SR.GridCollection_MustBeCertainType, "RowDefinitionCollection", "RowDefinition"))).Parent != null)
		{
			throw new ArgumentException(SR.Format(SR.GridCollection_InOtherCollection, "value", "RowDefinitionCollection"));
		}
	}

	private bool PrivateValidateValueForRemoval(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ((value as RowDefinition) ?? throw new ArgumentException(SR.Format(SR.GridCollection_MustBeCertainType, "RowDefinitionCollection", "RowDefinition"))).Parent == _owner;
	}

	private void PrivateConnectChild(int index, DefinitionBase value)
	{
		_items[index] = value;
		value.Index = index;
		_owner.AddLogicalChild(value);
		value.OnEnterParentTree();
	}

	private void PrivateDisconnectChild(DefinitionBase value)
	{
		value.OnExitParentTree();
		_items[value.Index] = null;
		value.Index = -1;
		_owner.RemoveLogicalChild(value);
	}

	private void PrivateInsert(int index, DefinitionBase value)
	{
		PrivateOnModified();
		if (_items == null)
		{
			PrivateSetCapacity(4);
		}
		else if (_size == _items.Length)
		{
			PrivateSetCapacity(Math.Max(_items.Length * 2, 4));
		}
		for (int num = _size - 1; num >= index; num--)
		{
			_items[num + 1] = _items[num];
			_items[num].Index = num + 1;
		}
		_items[index] = null;
		_size++;
		PrivateConnectChild(index, value);
	}

	private void PrivateRemove(DefinitionBase value)
	{
		PrivateOnModified();
		int index = value.Index;
		PrivateDisconnectChild(value);
		_size--;
		for (int i = index; i < _size; i++)
		{
			_items[i] = _items[i + 1];
			_items[i].Index = i;
		}
		_items[_size] = null;
	}

	private void PrivateOnModified()
	{
		_version++;
		_owner.RowDefinitionCollectionDirty = true;
		_owner.Invalidate();
	}

	private void PrivateSetCapacity(int value)
	{
		if (value <= 0)
		{
			_items = null;
		}
		else if (_items == null || value != _items.Length)
		{
			RowDefinition[] array = new RowDefinition[value];
			if (_size > 0)
			{
				Array.Copy(_items, 0, array, 0, _size);
			}
			DefinitionBase[] items = array;
			_items = items;
		}
	}
}
