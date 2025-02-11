using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices;

/// <summary>The builder for read only collection.</summary>
/// <typeparam name="T">The type of the collection element.</typeparam>
[Serializable]
public sealed class ReadOnlyCollectionBuilder<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
{
	[Serializable]
	private class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly ReadOnlyCollectionBuilder<T> _builder;

		private readonly int _version;

		private int _index;

		private T _current;

		public T Current => _current;

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index > _builder._size)
				{
					throw Error.EnumerationIsDone();
				}
				return _current;
			}
		}

		internal Enumerator(ReadOnlyCollectionBuilder<T> builder)
		{
			_builder = builder;
			_version = builder._version;
			_index = 0;
			_current = default(T);
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_version == _builder._version)
			{
				if (_index < _builder._size)
				{
					_current = _builder._items[_index++];
					return true;
				}
				_index = _builder._size + 1;
				_current = default(T);
				return false;
			}
			throw Error.CollectionModifiedWhileEnumerating();
		}

		void IEnumerator.Reset()
		{
			if (_version != _builder._version)
			{
				throw Error.CollectionModifiedWhileEnumerating();
			}
			_index = 0;
			_current = default(T);
		}
	}

	private const int DefaultCapacity = 4;

	private T[] _items;

	private int _size;

	private int _version;

	/// <summary>Gets and sets the capacity of this ReadOnlyCollectionBuilder.</summary>
	/// <returns>The capacity of this ReadOnlyCollectionBuilder.</returns>
	public int Capacity
	{
		get
		{
			return _items.Length;
		}
		set
		{
			if (value < _size)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value == _items.Length)
			{
				return;
			}
			if (value > 0)
			{
				T[] array = new T[value];
				if (_size > 0)
				{
					Array.Copy(_items, 0, array, 0, _size);
				}
				_items = array;
			}
			else
			{
				_items = Array.Empty<T>();
			}
		}
	}

	/// <summary>Returns number of elements in the ReadOnlyCollectionBuilder.</summary>
	/// <returns>The number of elements in the ReadOnlyCollectionBuilder.</returns>
	public int Count => _size;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	public T this[int index]
	{
		get
		{
			if (index >= _size)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return _items[index];
		}
		set
		{
			if (index >= _size)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			_items[index] = value;
			_version++;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
	bool ICollection<T>.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			ValidateNullValue(value, "value");
			try
			{
				this[index] = (T)value;
			}
			catch (InvalidCastException)
			{
				throw Error.InvalidTypeException(value, typeof(T), "value");
			}
		}
	}

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>Constructs a ReadOnlyCollectionBuilder.</summary>
	public ReadOnlyCollectionBuilder()
	{
		_items = Array.Empty<T>();
	}

	/// <summary>Constructs a ReadOnlyCollectionBuilder with a given initial capacity. The contents are empty but builder will have reserved room for the given number of elements before any reallocations are required.</summary>
	/// <param name="capacity">Initial capacity.</param>
	public ReadOnlyCollectionBuilder(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_items = new T[capacity];
	}

	/// <summary>Constructs a ReadOnlyCollectionBuilder, copying contents of the given collection.</summary>
	/// <param name="collection">Collection to copy elements from.</param>
	public ReadOnlyCollectionBuilder(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection is ICollection<T> { Count: var count } collection2)
		{
			_items = new T[count];
			collection2.CopyTo(_items, 0);
			_size = count;
			return;
		}
		_size = 0;
		_items = new T[4];
		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>Returns the index of the first occurrence of a given value in the builder.</summary>
	/// <returns>The index of the first occurrence of an item.</returns>
	/// <param name="item">An item to search for.</param>
	public int IndexOf(T item)
	{
		return Array.IndexOf(_items, item, 0, _size);
	}

	/// <summary>Inserts an item to the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which item should be inserted.</param>
	/// <param name="item">The object to insert into the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</param>
	public void Insert(int index, T item)
	{
		if (index > _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (_size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		if (index < _size)
		{
			Array.Copy(_items, index, _items, index + 1, _size - index);
		}
		_items[index] = item;
		_size++;
		_version++;
	}

	/// <summary>Removes the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" /> item at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	public void RemoveAt(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_size--;
		if (index < _size)
		{
			Array.Copy(_items, index + 1, _items, index, _size - index);
		}
		_items[_size] = default(T);
		_version++;
	}

	/// <summary>Adds an item to the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</summary>
	/// <param name="item">The object to add to the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</param>
	public void Add(T item)
	{
		if (_size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		_items[_size++] = item;
		_version++;
	}

	/// <summary>Removes all items from the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</summary>
	public void Clear()
	{
		if (_size > 0)
		{
			Array.Clear(_items, 0, _size);
			_size = 0;
		}
		_version++;
	}

	/// <summary>Determines whether the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" /> contains a specific value</summary>
	/// <returns>true if item is found in the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />; otherwise, false.</returns>
	/// <param name="item">the object to locate in the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</param>
	public bool Contains(T item)
	{
		if (item == null)
		{
			for (int i = 0; i < _size; i++)
			{
				if (_items[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		EqualityComparer<T> @default = EqualityComparer<T>.Default;
		for (int j = 0; j < _size; j++)
		{
			if (@default.Equals(_items[j], item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" /> to an <see cref="T:System.Array" />, starting at particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	public void CopyTo(T[] array, int arrayIndex)
	{
		Array.Copy(_items, 0, array, arrayIndex, _size);
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</summary>
	/// <returns>true if item was successfully removed from the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</returns>
	/// <param name="item">The object to remove from the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</param>
	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Collections.IList" />.</param>
	int IList.Add(object value)
	{
		ValidateNullValue(value, "value");
		try
		{
			Add((T)value);
		}
		catch (InvalidCastException)
		{
			throw Error.InvalidTypeException(value, typeof(T), "value");
		}
		return Count - 1;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	bool IList.Contains(object value)
	{
		if (IsCompatibleObject(value))
		{
			return Contains((T)value);
		}
		return false;
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, –1.</returns>
	/// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
	int IList.IndexOf(object value)
	{
		if (IsCompatibleObject(value))
		{
			return IndexOf((T)value);
		}
		return -1;
	}

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="value">The object to insert into the <see cref="T:System.Collections.IList" />.</param>
	void IList.Insert(int index, object value)
	{
		ValidateNullValue(value, "value");
		try
		{
			Insert(index, (T)value);
		}
		catch (InvalidCastException)
		{
			throw Error.InvalidTypeException(value, typeof(T), "value");
		}
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="value">The object to remove from the <see cref="T:System.Collections.IList" />.</param>
	void IList.Remove(object value)
	{
		if (IsCompatibleObject(value))
		{
			Remove((T)value);
		}
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException("array");
		}
		Array.Copy(_items, 0, array, index, _size);
	}

	/// <summary>Reverses the order of the elements in the entire <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</summary>
	public void Reverse()
	{
		Reverse(0, Count);
	}

	/// <summary>Reverses the order of the elements in the specified range.</summary>
	/// <param name="index">The zero-based starting index of the range to reverse.</param>
	/// <param name="count">The number of elements in the range to reverse.</param>
	public void Reverse(int index, int count)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		Array.Reverse(_items, index, count);
		_version++;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" /> to a new array.</summary>
	/// <returns>An array containing copies of the elements of the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />.</returns>
	public T[] ToArray()
	{
		T[] array = new T[_size];
		Array.Copy(_items, 0, array, 0, _size);
		return array;
	}

	/// <summary>Creates a <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> containing all of the elements of the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" />, avoiding copying the elements to the new array if possible. Resets the <see cref="T:System.Runtime.CompilerServices.ReadOnlyCollectionBuilder`1" /> after the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> has been created.</summary>
	/// <returns>A new instance of <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</returns>
	public ReadOnlyCollection<T> ToReadOnlyCollection()
	{
		T[] list = ((_size != _items.Length) ? ToArray() : _items);
		_items = Array.Empty<T>();
		_size = 0;
		_version++;
		return new TrueReadOnlyCollection<T>(list);
	}

	private void EnsureCapacity(int min)
	{
		if (_items.Length < min)
		{
			int num = 4;
			if (_items.Length != 0)
			{
				num = _items.Length * 2;
			}
			if (num < min)
			{
				num = min;
			}
			Capacity = num;
		}
	}

	private static bool IsCompatibleObject(object value)
	{
		if (!(value is T))
		{
			if (value == null)
			{
				return default(T) == null;
			}
			return false;
		}
		return true;
	}

	private static void ValidateNullValue(object value, string argument)
	{
		if (value == null && default(T) != null)
		{
			throw Error.InvalidNullValue(typeof(T), argument);
		}
	}
}
