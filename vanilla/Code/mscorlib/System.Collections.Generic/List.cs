using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;

namespace System.Collections.Generic;

/// <summary>Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.</summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	[Serializable]
	internal class SynchronizedList : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private List<T> _list;

		private object _root;

		public int Count
		{
			get
			{
				lock (_root)
				{
					return _list.Count;
				}
			}
		}

		public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

		public T this[int index]
		{
			get
			{
				lock (_root)
				{
					return _list[index];
				}
			}
			set
			{
				lock (_root)
				{
					_list[index] = value;
				}
			}
		}

		internal SynchronizedList(List<T> list)
		{
			_list = list;
			_root = ((ICollection)list).SyncRoot;
		}

		public void Add(T item)
		{
			lock (_root)
			{
				_list.Add(item);
			}
		}

		public void Clear()
		{
			lock (_root)
			{
				_list.Clear();
			}
		}

		public bool Contains(T item)
		{
			lock (_root)
			{
				return _list.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_root)
			{
				_list.CopyTo(array, arrayIndex);
			}
		}

		public bool Remove(T item)
		{
			lock (_root)
			{
				return _list.Remove(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (_root)
			{
				return _list.GetEnumerator();
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			lock (_root)
			{
				return ((IEnumerable<T>)_list).GetEnumerator();
			}
		}

		public int IndexOf(T item)
		{
			lock (_root)
			{
				return _list.IndexOf(item);
			}
		}

		public void Insert(int index, T item)
		{
			lock (_root)
			{
				_list.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (_root)
			{
				_list.RemoveAt(index);
			}
		}
	}

	/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private List<T> list;

		private int index;

		private int version;

		private T current;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the <see cref="T:System.Collections.Generic.List`1" /> at the current position of the enumerator.</returns>
		public T Current => current;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the <see cref="T:System.Collections.Generic.List`1" /> at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IEnumerator.Current
		{
			get
			{
				if (index == 0 || index == list._size + 1)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
				}
				return Current;
			}
		}

		internal Enumerator(List<T> list)
		{
			this.list = list;
			index = 0;
			version = list._version;
			current = default(T);
		}

		/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.List`1.Enumerator" />.</summary>
		public void Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		public bool MoveNext()
		{
			List<T> list = this.list;
			if (version == list._version && (uint)index < (uint)list._size)
			{
				current = list._items[index];
				index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (version != list._version)
			{
				ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
			}
			index = list._size + 1;
			current = default(T);
			return false;
		}

		/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		void IEnumerator.Reset()
		{
			if (version != list._version)
			{
				ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
			}
			index = 0;
			current = default(T);
		}
	}

	private const int _defaultCapacity = 4;

	private T[] _items;

	private int _size;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private static readonly T[] _emptyArray = new T[0];

	/// <summary>Gets or sets the total number of elements the internal data structure can hold without resizing.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Collections.Generic.List`1" /> can contain before resizing is required.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Collections.Generic.List`1.Capacity" /> is set to a value that is less than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
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
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
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
				_items = _emptyArray;
			}
		}
	}

	/// <summary>Gets the number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.</returns>
	public int Count => _size;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.List`1" />, this property always returns false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.List`1" />, this property always returns false.</returns>
	bool ICollection<T>.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.List`1" />, this property always returns false.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.List`1" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.List`1" />, this property always returns the current instance.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
	public T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if ((uint)index >= (uint)_size)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException();
			}
			return Array.UnsafeLoad(_items, index);
		}
		set
		{
			if ((uint)index >= (uint)_size)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException();
			}
			_items[index] = value;
			_version++;
		}
	}

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
	/// <exception cref="T:System.ArgumentException">The property is set and <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
			try
			{
				this[index] = (T)value;
			}
			catch (InvalidCastException)
			{
				ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the default initial capacity.</summary>
	public List()
	{
		_items = _emptyArray;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the specified initial capacity.</summary>
	/// <param name="capacity">The number of elements that the new list can initially store.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than 0. </exception>
	public List(int capacity)
	{
		if (capacity < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (capacity == 0)
		{
			_items = _emptyArray;
		}
		else
		{
			_items = new T[capacity];
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.</summary>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public List(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
		}
		if (collection is ICollection<T> { Count: var count } collection2)
		{
			if (count == 0)
			{
				_items = _emptyArray;
				return;
			}
			_items = new T[count];
			collection2.CopyTo(_items, 0);
			_size = count;
			return;
		}
		_size = 0;
		_items = _emptyArray;
		foreach (T item in collection)
		{
			Add(item);
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

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	public void Add(T item)
	{
		if (_size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		_items[_size++] = item;
		_version++;
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="item">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="item" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	int IList.Add(object item)
	{
		ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
		try
		{
			Add((T)item);
		}
		catch (InvalidCastException)
		{
			ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
		}
		return Count - 1;
	}

	/// <summary>Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <param name="collection">The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The collection itself cannot be null, but it can contain elements that are null, if type <paramref name="T" /> is a reference type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public void AddRange(IEnumerable<T> collection)
	{
		InsertRange(_size, collection);
	}

	/// <summary>Returns a read-only <see cref="T:System.Collections.Generic.IList`1" /> wrapper for the current collection.</summary>
	/// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.List`1" />.</returns>
	public ReadOnlyCollection<T> AsReadOnly()
	{
		return new ReadOnlyCollection<T>(this);
	}

	/// <summary>Searches a range of elements in the sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
	/// <returns>The zero-based index of <paramref name="item" /> in the sorted <see cref="T:System.Collections.Generic.List`1" />, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count" />.</returns>
	/// <param name="index">The zero-based starting index of the range to search.</param>
	/// <param name="count">The length of the range to search.</param>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.</exception>
	public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		return Array.BinarySearch(_items, index, count, item, comparer);
	}

	/// <summary>Searches the entire sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the default comparer and returns the zero-based index of the element.</summary>
	/// <returns>The zero-based index of <paramref name="item" /> in the sorted <see cref="T:System.Collections.Generic.List`1" />, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count" />.</returns>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <exception cref="T:System.InvalidOperationException">The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.</exception>
	public int BinarySearch(T item)
	{
		return BinarySearch(0, Count, item, null);
	}

	/// <summary>Searches the entire sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
	/// <returns>The zero-based index of <paramref name="item" /> in the sorted <see cref="T:System.Collections.Generic.List`1" />, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count" />.</returns>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.-or-null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.</exception>
	public int BinarySearch(T item, IComparer<T> comparer)
	{
		return BinarySearch(0, Count, item, comparer);
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	public void Clear()
	{
		if (_size > 0)
		{
			Array.Clear(_items, 0, _size);
			_size = 0;
		}
		_version++;
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.List`1" />; otherwise, false.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
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

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="item">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	bool IList.Contains(object item)
	{
		if (IsCompatibleObject(item))
		{
			return Contains((T)item);
		}
		return false;
	}

	/// <summary>Converts the elements in the current <see cref="T:System.Collections.Generic.List`1" /> to another type, and returns a list containing the converted elements.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.List`1" /> of the target type containing the converted elements from the current <see cref="T:System.Collections.Generic.List`1" />.</returns>
	/// <param name="converter">A <see cref="T:System.Converter`2" /> delegate that converts each element from one type to another type.</param>
	/// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="converter" /> is null.</exception>
	public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
	{
		if (converter == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
		}
		List<TOutput> list = new List<TOutput>(_size);
		for (int i = 0; i < _size; i++)
		{
			list._items[i] = converter(_items[i]);
		}
		list._size = _size;
		return list;
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the number of elements that the destination <paramref name="array" /> can contain.</exception>
	public void CopyTo(T[] array)
	{
		CopyTo(array, 0);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array != null && array.Rank != 1)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
		}
		try
		{
			Array.Copy(_items, 0, array, arrayIndex, _size);
		}
		catch (ArrayTypeMismatchException)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
		}
	}

	/// <summary>Copies a range of elements from the <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
	/// <param name="index">The zero-based index in the source <see cref="T:System.Collections.Generic.List`1" /> at which copying begins.</param>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <param name="count">The number of elements to copy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="arrayIndex" /> is less than 0.-or-<paramref name="count" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> is equal to or greater than the <see cref="P:System.Collections.Generic.List`1.Count" /> of the source <see cref="T:System.Collections.Generic.List`1" />.-or-The number of elements from <paramref name="index" /> to the end of the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />. </exception>
	public void CopyTo(int index, T[] array, int arrayIndex, int count)
	{
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		Array.Copy(_items, index, array, arrayIndex, count);
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		Array.Copy(_items, 0, array, arrayIndex, _size);
	}

	private void EnsureCapacity(int min)
	{
		if (_items.Length < min)
		{
			int num = ((_items.Length == 0) ? 4 : (_items.Length * 2));
			if ((uint)num > 2146435071u)
			{
				num = 2146435071;
			}
			if (num < min)
			{
				num = min;
			}
			Capacity = num;
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.List`1" /> contains elements that match the conditions defined by the specified predicate.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.List`1" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public bool Exists(Predicate<T> match)
	{
		return FindIndex(match) != -1;
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public T Find(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		for (int i = 0; i < _size; i++)
		{
			if (match(_items[i]))
			{
				return _items[i];
			}
		}
		return default(T);
	}

	/// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.List`1" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List`1" />.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public List<T> FindAll(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		List<T> list = new List<T>();
		for (int i = 0; i < _size; i++)
		{
			if (match(_items[i]))
			{
				list.Add(_items[i]);
			}
		}
		return list;
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public int FindIndex(Predicate<T> match)
	{
		return FindIndex(0, _size, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the specified index to the last element.</summary>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public int FindIndex(int startIndex, Predicate<T> match)
	{
		return FindIndex(startIndex, _size - startIndex, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.-or-<paramref name="count" /> is less than 0.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		if ((uint)startIndex > (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (count < 0 || startIndex > _size - count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
		}
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		int num = startIndex + count;
		for (int i = startIndex; i < num; i++)
		{
			if (match(_items[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public T FindLast(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		for (int num = _size - 1; num >= 0; num--)
		{
			if (match(_items[num]))
			{
				return _items[num];
			}
		}
		return default(T);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public int FindLastIndex(Predicate<T> match)
	{
		return FindLastIndex(_size - 1, _size, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public int FindLastIndex(int startIndex, Predicate<T> match)
	{
		return FindLastIndex(startIndex, startIndex + 1, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.-or-<paramref name="count" /> is less than 0.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public int FindLastIndex(int startIndex, int count, Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		if (_size == 0)
		{
			if (startIndex != -1)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
		}
		else if ((uint)startIndex >= (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
		}
		int num = startIndex - count;
		for (int num2 = startIndex; num2 > num; num2--)
		{
			if (match(_items[num2]))
			{
				return num2;
			}
		}
		return -1;
	}

	/// <summary>Performs the specified action on each element of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <param name="action">The <see cref="T:System.Action`1" /> delegate to perform on each element of the <see cref="T:System.Collections.Generic.List`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="action" /> is null.</exception>
	public void ForEach(Action<T> action)
	{
		if (action == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		int version = _version;
		for (int i = 0; i < _size; i++)
		{
			if (version != _version && BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
			{
				break;
			}
			action(_items[i]);
		}
		if (version != _version && BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
		{
			ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.List`1.Enumerator" /> for the <see cref="T:System.Collections.Generic.List`1" />.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Creates a shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>A shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.</returns>
	/// <param name="index">The zero-based <see cref="T:System.Collections.Generic.List`1" /> index at which the range starts.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public List<T> GetRange(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		List<T> list = new List<T>(count);
		Array.Copy(_items, index, list._items, 0, count);
		list._size = count;
		return list;
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the entire <see cref="T:System.Collections.Generic.List`1" />, if found; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	public int IndexOf(T item)
	{
		return Array.IndexOf(_items, item, 0, _size);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="item" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	int IList.IndexOf(object item)
	{
		if (IsCompatibleObject(item))
		{
			return IndexOf((T)item);
		}
		return -1;
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the specified index to the last element.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from <paramref name="index" /> to the last element, if found; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public int IndexOf(T item, int index)
	{
		if (index > _size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
		}
		return Array.IndexOf(_items, item, index, _size - index);
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.-or-<paramref name="count" /> is less than 0.-or-<paramref name="index" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public int IndexOf(T item, int index, int count)
	{
		if (index > _size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (count < 0 || index > _size - count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
		}
		return Array.IndexOf(_items, item, index, count);
	}

	/// <summary>Inserts an element into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.</exception>
	public void Insert(int index, T item)
	{
		if ((uint)index > (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
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

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert into the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="item" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	void IList.Insert(int index, object item)
	{
		ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
		try
		{
			Insert(index, (T)item);
		}
		catch (InvalidCastException)
		{
			ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
		}
	}

	/// <summary>Inserts the elements of a collection into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
	/// <param name="collection">The collection whose elements should be inserted into the <see cref="T:System.Collections.Generic.List`1" />. The collection itself cannot be null, but it can contain elements that are null, if type <paramref name="T" /> is a reference type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.</exception>
	public void InsertRange(int index, IEnumerable<T> collection)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
		}
		if ((uint)index > (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (collection is ICollection<T> { Count: var count } collection2)
		{
			if (count > 0)
			{
				EnsureCapacity(_size + count);
				if (index < _size)
				{
					Array.Copy(_items, index, _items, index + count, _size - index);
				}
				if (this == collection2)
				{
					Array.Copy(_items, 0, _items, index, index);
					Array.Copy(_items, index + count, _items, index * 2, _size - index);
				}
				else
				{
					collection2.CopyTo(_items, index);
				}
				_size += count;
			}
		}
		else
		{
			using IEnumerator<T> enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Insert(index++, enumerator.Current);
			}
		}
		_version++;
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the entire the <see cref="T:System.Collections.Generic.List`1" />, if found; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	public int LastIndexOf(T item)
	{
		if (_size == 0)
		{
			return -1;
		}
		return LastIndexOf(item, _size - 1, _size);
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to <paramref name="index" />, if found; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />. </exception>
	public int LastIndexOf(T item, int index)
	{
		if (index >= _size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
		}
		return LastIndexOf(item, index, index + 1);
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains <paramref name="count" /> number of elements and ends at <paramref name="index" />, if found; otherwise, –1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.-or-<paramref name="count" /> is less than 0.-or-<paramref name="index" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />. </exception>
	public int LastIndexOf(T item, int index, int count)
	{
		if (Count != 0 && index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (Count != 0 && count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size == 0)
		{
			return -1;
		}
		if (index >= _size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
		}
		if (count > index + 1)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
		}
		return Array.LastIndexOf(_items, item, index, count);
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false.  This method also returns false if <paramref name="item" /> was not found in the <see cref="T:System.Collections.Generic.List`1" />.</returns>
	/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
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

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="item">The object to remove from the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="item" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	void IList.Remove(object item)
	{
		if (IsCompatibleObject(item))
		{
			Remove((T)item);
		}
	}

	/// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
	/// <returns>The number of elements removed from the <see cref="T:System.Collections.Generic.List`1" /> .</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public int RemoveAll(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		int i;
		for (i = 0; i < _size && !match(_items[i]); i++)
		{
		}
		if (i >= _size)
		{
			return 0;
		}
		int j = i + 1;
		while (j < _size)
		{
			for (; j < _size && match(_items[j]); j++)
			{
			}
			if (j < _size)
			{
				_items[i++] = _items[j++];
			}
		}
		Array.Clear(_items, i, _size - i);
		int result = _size - i;
		_size = i;
		_version++;
		return result;
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />.</exception>
	public void RemoveAt(int index)
	{
		if ((uint)index >= (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException();
		}
		_size--;
		if (index < _size)
		{
			Array.Copy(_items, index + 1, _items, index, _size - index);
		}
		_items[_size] = default(T);
		_version++;
	}

	/// <summary>Removes a range of elements from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
	/// <param name="count">The number of elements to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
	public void RemoveRange(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		if (count > 0)
		{
			_size -= count;
			if (index < _size)
			{
				Array.Copy(_items, index + count, _items, index, _size - index);
			}
			Array.Clear(_items, _size, count);
			_version++;
		}
	}

	/// <summary>Reverses the order of the elements in the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	public void Reverse()
	{
		Reverse(0, Count);
	}

	/// <summary>Reverses the order of the elements in the specified range.</summary>
	/// <param name="index">The zero-based starting index of the range to reverse.</param>
	/// <param name="count">The number of elements in the range to reverse.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />. </exception>
	public void Reverse(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		Array.Reverse(_items, index, count);
		_version++;
	}

	/// <summary>Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the default comparer.</summary>
	/// <exception cref="T:System.InvalidOperationException">The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.</exception>
	public void Sort()
	{
		Sort(0, Count, null);
	}

	/// <summary>Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified comparer.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.</exception>
	/// <exception cref="T:System.ArgumentException">The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	public void Sort(IComparer<T> comparer)
	{
		Sort(0, Count, comparer);
	}

	/// <summary>Sorts the elements in a range of elements in <see cref="T:System.Collections.Generic.List`1" /> using the specified comparer.</summary>
	/// <param name="index">The zero-based starting index of the range to sort.</param>
	/// <param name="count">The length of the range to sort.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not specify a valid range in the <see cref="T:System.Collections.Generic.List`1" />.-or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.</exception>
	public void Sort(int index, int count, IComparer<T> comparer)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		Array.Sort(_items, index, count, comparer);
		_version++;
	}

	/// <summary>Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
	/// <param name="comparison">The <see cref="T:System.Comparison`1" /> to use when comparing elements.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="comparison" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The implementation of <paramref name="comparison" /> caused an error during the sort. For example, <paramref name="comparison" /> might not return 0 when comparing an item with itself.</exception>
	public void Sort(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		if (_size > 0)
		{
			ArraySortHelper<T>.Sort(_items, 0, _size, comparison);
		}
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.List`1" /> to a new array.</summary>
	/// <returns>An array containing copies of the elements of the <see cref="T:System.Collections.Generic.List`1" />.</returns>
	public T[] ToArray()
	{
		T[] array = new T[_size];
		Array.Copy(_items, 0, array, 0, _size);
		return array;
	}

	/// <summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.List`1" />, if that number is less than a threshold value.</summary>
	public void TrimExcess()
	{
		int num = (int)((double)_items.Length * 0.9);
		if (_size < num)
		{
			Capacity = _size;
		}
	}

	/// <summary>Determines whether every element in the <see cref="T:System.Collections.Generic.List`1" /> matches the conditions defined by the specified predicate.</summary>
	/// <returns>true if every element in the <see cref="T:System.Collections.Generic.List`1" /> matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions to check against the elements.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public bool TrueForAll(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		for (int i = 0; i < _size; i++)
		{
			if (!match(_items[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static IList<T> Synchronized(List<T> list)
	{
		return new SynchronizedList(list);
	}
}
