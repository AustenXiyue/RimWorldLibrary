using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Generic;

/// <summary>Represents a collection of key/value pairs that are sorted by key based on the associated <see cref="T:System.Collections.Generic.IComparer`1" /> implementation. </summary>
/// <typeparam name="TKey">The type of keys in the collection.</typeparam>
/// <typeparam name="TValue">The type of values in the collection.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(IDictionaryDebugView<, >))]
public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
	[Serializable]
	private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
	{
		private SortedList<TKey, TValue> _sortedList;

		private TKey _key;

		private TValue _value;

		private int _index;

		private int _version;

		private int _getEnumeratorRetType;

		internal const int KeyValuePair = 1;

		internal const int DictEntry = 2;

		object IDictionaryEnumerator.Key
		{
			get
			{
				if (_index == 0 || _index == _sortedList.Count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _key;
			}
		}

		DictionaryEntry IDictionaryEnumerator.Entry
		{
			get
			{
				if (_index == 0 || _index == _sortedList.Count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return new DictionaryEntry(_key, _value);
			}
		}

		public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_key, _value);

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _sortedList.Count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				if (_getEnumeratorRetType == 2)
				{
					return new DictionaryEntry(_key, _value);
				}
				return new KeyValuePair<TKey, TValue>(_key, _value);
			}
		}

		object IDictionaryEnumerator.Value
		{
			get
			{
				if (_index == 0 || _index == _sortedList.Count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _value;
			}
		}

		internal Enumerator(SortedList<TKey, TValue> sortedList, int getEnumeratorRetType)
		{
			_sortedList = sortedList;
			_index = 0;
			_version = _sortedList.version;
			_getEnumeratorRetType = getEnumeratorRetType;
			_key = default(TKey);
			_value = default(TValue);
		}

		public void Dispose()
		{
			_index = 0;
			_key = default(TKey);
			_value = default(TValue);
		}

		public bool MoveNext()
		{
			if (_version != _sortedList.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			if ((uint)_index < (uint)_sortedList.Count)
			{
				_key = _sortedList.keys[_index];
				_value = _sortedList.values[_index];
				_index++;
				return true;
			}
			_index = _sortedList.Count + 1;
			_key = default(TKey);
			_value = default(TValue);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _sortedList.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			_index = 0;
			_key = default(TKey);
			_value = default(TValue);
		}
	}

	[Serializable]
	private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IDisposable, IEnumerator
	{
		private SortedList<TKey, TValue> _sortedList;

		private int _index;

		private int _version;

		private TKey _currentKey;

		public TKey Current => _currentKey;

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _sortedList.Count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _currentKey;
			}
		}

		internal SortedListKeyEnumerator(SortedList<TKey, TValue> sortedList)
		{
			_sortedList = sortedList;
			_version = sortedList.version;
		}

		public void Dispose()
		{
			_index = 0;
			_currentKey = default(TKey);
		}

		public bool MoveNext()
		{
			if (_version != _sortedList.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			if ((uint)_index < (uint)_sortedList.Count)
			{
				_currentKey = _sortedList.keys[_index];
				_index++;
				return true;
			}
			_index = _sortedList.Count + 1;
			_currentKey = default(TKey);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _sortedList.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			_index = 0;
			_currentKey = default(TKey);
		}
	}

	[Serializable]
	private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IDisposable, IEnumerator
	{
		private SortedList<TKey, TValue> _sortedList;

		private int _index;

		private int _version;

		private TValue _currentValue;

		public TValue Current => _currentValue;

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _sortedList.Count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _currentValue;
			}
		}

		internal SortedListValueEnumerator(SortedList<TKey, TValue> sortedList)
		{
			_sortedList = sortedList;
			_version = sortedList.version;
		}

		public void Dispose()
		{
			_index = 0;
			_currentValue = default(TValue);
		}

		public bool MoveNext()
		{
			if (_version != _sortedList.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			if ((uint)_index < (uint)_sortedList.Count)
			{
				_currentValue = _sortedList.values[_index];
				_index++;
				return true;
			}
			_index = _sortedList.Count + 1;
			_currentValue = default(TValue);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _sortedList.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			_index = 0;
			_currentValue = default(TValue);
		}
	}

	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<, >))]
	private sealed class KeyList : IList<TKey>, ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection
	{
		private SortedList<TKey, TValue> _dict;

		public int Count => _dict._size;

		public bool IsReadOnly => true;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

		public TKey this[int index]
		{
			get
			{
				return _dict.GetKey(index);
			}
			set
			{
				throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
			}
		}

		internal KeyList(SortedList<TKey, TValue> dictionary)
		{
			_dict = dictionary;
		}

		public void Add(TKey key)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public void Clear()
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public bool Contains(TKey key)
		{
			return _dict.ContainsKey(key);
		}

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			Array.Copy(_dict.keys, 0, array, arrayIndex, _dict.Count);
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
			}
			try
			{
				Array.Copy(_dict.keys, 0, array, arrayIndex, _dict.Count);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
		}

		public void Insert(int index, TKey value)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			return new SortedListKeyEnumerator(_dict);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SortedListKeyEnumerator(_dict);
		}

		public int IndexOf(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			int num = Array.BinarySearch(_dict.keys, 0, _dict.Count, key, _dict.comparer);
			if (num >= 0)
			{
				return num;
			}
			return -1;
		}

		public bool Remove(TKey key)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}
	}

	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<, >))]
	private sealed class ValueList : IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection
	{
		private SortedList<TKey, TValue> _dict;

		public int Count => _dict._size;

		public bool IsReadOnly => true;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

		public TValue this[int index]
		{
			get
			{
				return _dict.GetByIndex(index);
			}
			set
			{
				throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
			}
		}

		internal ValueList(SortedList<TKey, TValue> dictionary)
		{
			_dict = dictionary;
		}

		public void Add(TValue key)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public void Clear()
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public bool Contains(TValue value)
		{
			return _dict.ContainsValue(value);
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			Array.Copy(_dict.values, 0, array, arrayIndex, _dict.Count);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
			}
			try
			{
				Array.Copy(_dict.values, 0, array, index, _dict.Count);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
		}

		public void Insert(int index, TValue value)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return new SortedListValueEnumerator(_dict);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SortedListValueEnumerator(_dict);
		}

		public int IndexOf(TValue value)
		{
			return Array.IndexOf(_dict.values, value, 0, _dict.Count);
		}

		public bool Remove(TValue value)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException("This operation is not supported on SortedList nested types because they require modifying the original SortedList.");
		}
	}

	private TKey[] keys;

	private TValue[] values;

	private int _size;

	private int version;

	private IComparer<TKey> comparer;

	private KeyList keyList;

	private ValueList valueList;

	[NonSerialized]
	private object _syncRoot;

	private const int DefaultCapacity = 4;

	private const int MaxArrayLength = 2146435071;

	/// <summary>Gets or sets the number of elements that the <see cref="T:System.Collections.Generic.SortedList`2" /> can contain.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Collections.Generic.SortedList`2" /> can contain.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Collections.Generic.SortedList`2.Capacity" /> is set to a value that is less than <see cref="P:System.Collections.Generic.SortedList`2.Count" />.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
	public int Capacity
	{
		get
		{
			return keys.Length;
		}
		set
		{
			if (value == keys.Length)
			{
				return;
			}
			if (value < _size)
			{
				throw new ArgumentOutOfRangeException("value", value, "capacity was less than the current size.");
			}
			if (value > 0)
			{
				TKey[] destinationArray = new TKey[value];
				TValue[] destinationArray2 = new TValue[value];
				if (_size > 0)
				{
					Array.Copy(keys, 0, destinationArray, 0, _size);
					Array.Copy(values, 0, destinationArray2, 0, _size);
				}
				keys = destinationArray;
				values = destinationArray2;
			}
			else
			{
				keys = Array.Empty<TKey>();
				values = Array.Empty<TValue>();
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Collections.Generic.IComparer`1" /> for the sorted list. </summary>
	/// <returns>The <see cref="T:System.IComparable`1" /> for the current <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	public IComparer<TKey> Comparer => comparer;

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	public int Count => _size;

	/// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IList`1" /> containing the keys in the <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	public IList<TKey> Keys => GetKeyListHelper();

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => GetKeyListHelper();

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Keys => GetKeyListHelper();

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => GetKeyListHelper();

	/// <summary>Gets a collection containing the values in the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IList`1" /> containing the values in the <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	public IList<TValue> Values => GetValueListHelper();

	ICollection<TValue> IDictionary<TKey, TValue>.Values => GetValueListHelper();

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Values => GetValueListHelper();

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => GetValueListHelper();

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedList`2" />, this property always returns false.</returns>
	bool IDictionary.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> has a fixed size; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedList`2" />, this property always returns false.</returns>
	bool IDictionary.IsFixedSize => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedList`2" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.SortedList`2" />, this property always returns the current instance.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" /> and a set operation creates a new element using the specified key.</returns>
	/// <param name="key">The key whose value to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
	public TValue this[TKey key]
	{
		get
		{
			int num = IndexOfKey(key);
			if (num >= 0)
			{
				return values[num];
			}
			throw new KeyNotFoundException();
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			int num = Array.BinarySearch(keys, 0, _size, key, comparer);
			if (num >= 0)
			{
				values[num] = value;
				version++;
			}
			else
			{
				Insert(~num, key, value);
			}
		}
	}

	/// <summary>Gets or sets the element with the specified key.</summary>
	/// <returns>The element with the specified key, or null if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	/// <param name="key">The key of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.SortedList`2" />.-or-A value is being assigned, and <paramref name="value" /> is of a type that is not assignable to the value type <paramref name="TValue" /> of the <see cref="T:System.Collections.Generic.SortedList`2" />.</exception>
	object IDictionary.this[object key]
	{
		get
		{
			if (IsCompatibleKey(key))
			{
				int num = IndexOfKey((TKey)key);
				if (num >= 0)
				{
					return values[num];
				}
			}
			return null;
		}
		set
		{
			if (!IsCompatibleKey(key))
			{
				throw new ArgumentNullException("key");
			}
			if (value == null && default(TValue) != null)
			{
				throw new ArgumentNullException("value");
			}
			TKey key2 = (TKey)key;
			try
			{
				this[key2] = (TValue)value;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(TValue)), "value");
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedList`2" /> class that is empty, has the default initial capacity, and uses the default <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
	public SortedList()
	{
		keys = Array.Empty<TKey>();
		values = Array.Empty<TValue>();
		_size = 0;
		comparer = Comparer<TKey>.Default;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedList`2" /> class that is empty, has the specified initial capacity, and uses the default <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.SortedList`2" /> can contain.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	public SortedList(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", capacity, "Non-negative number required.");
		}
		keys = new TKey[capacity];
		values = new TValue[capacity];
		comparer = Comparer<TKey>.Default;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedList`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing keys.-or-null to use the default <see cref="T:System.Collections.Generic.Comparer`1" /> for the type of the key.</param>
	public SortedList(IComparer<TKey> comparer)
		: this()
	{
		if (comparer != null)
		{
			this.comparer = comparer;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedList`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.SortedList`2" /> can contain.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing keys.-or-null to use the default <see cref="T:System.Collections.Generic.Comparer`1" /> for the type of the key.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	public SortedList(int capacity, IComparer<TKey> comparer)
		: this(comparer)
	{
		Capacity = capacity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedList`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" />, has sufficient capacity to accommodate the number of elements copied, and uses the default <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
	/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.SortedList`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
	public SortedList(IDictionary<TKey, TValue> dictionary)
		: this(dictionary, (IComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedList`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" />, has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
	/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.SortedList`2" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing keys.-or-null to use the default <see cref="T:System.Collections.Generic.Comparer`1" /> for the type of the key.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
	public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
		: this(dictionary?.Count ?? 0, comparer)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		int count = dictionary.Count;
		if (count != 0)
		{
			TKey[] array = keys;
			dictionary.Keys.CopyTo(array, 0);
			dictionary.Values.CopyTo(values, 0);
			if (count > 1)
			{
				comparer = Comparer;
				Array.Sort(array, values, comparer);
				for (int i = 1; i != array.Length; i++)
				{
					if (comparer.Compare(array[i - 1], array[i]) == 0)
					{
						throw new ArgumentException(global::SR.Format("An item with the same key has already been added. Key: {0}", array[i]));
					}
				}
			}
		}
		_size = count;
	}

	/// <summary>Adds an element with the specified key and value into the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.SortedList`2" />.</exception>
	public void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = Array.BinarySearch(keys, 0, _size, key, comparer);
		if (num >= 0)
		{
			throw new ArgumentException(global::SR.Format("An item with the same key has already been added. Key: {0}", key), "key");
		}
		Insert(~num, key, value);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
	{
		Add(keyValuePair.Key, keyValuePair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int num = IndexOfKey(keyValuePair.Key);
		if (num >= 0 && EqualityComparer<TValue>.Default.Equals(values[num], keyValuePair.Value))
		{
			return true;
		}
		return false;
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int num = IndexOfKey(keyValuePair.Key);
		if (num >= 0 && EqualityComparer<TValue>.Default.Equals(values[num], keyValuePair.Value))
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	/// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to use as the key of the element to add.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to use as the value of the element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.IDictionary" />.-or-<paramref name="value" /> is of a type that is not assignable to the value type <paramref name="TValue" /> of the <see cref="T:System.Collections.IDictionary" />.-or-An element with the same key already exists in the <see cref="T:System.Collections.IDictionary" />.</exception>
	void IDictionary.Add(object key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (value == null && default(TValue) != null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(key is TKey))
		{
			throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", key, typeof(TKey)), "key");
		}
		if (!(value is TValue) && value != null)
		{
			throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(TValue)), "value");
		}
		Add((TKey)key, (TValue)value);
	}

	private KeyList GetKeyListHelper()
	{
		if (keyList == null)
		{
			keyList = new KeyList(this);
		}
		return keyList;
	}

	private ValueList GetValueListHelper()
	{
		if (valueList == null)
		{
			valueList = new ValueList(this);
		}
		return valueList;
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	public void Clear()
	{
		version++;
		if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
		{
			Array.Clear(keys, 0, _size);
		}
		if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
		{
			Array.Clear(values, 0, _size);
		}
		_size = 0;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	bool IDictionary.Contains(object key)
	{
		if (IsCompatibleKey(key))
		{
			return ContainsKey((TKey)key);
		}
		return false;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.SortedList`2" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.SortedList`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.SortedList`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool ContainsKey(TKey key)
	{
		return IndexOfKey(key) >= 0;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.SortedList`2" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.SortedList`2" /> contains an element with the specified value; otherwise, false.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.SortedList`2" />. The value can be null for reference types.</param>
	public bool ContainsValue(TValue value)
	{
		return IndexOfValue(value) >= 0;
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0 || arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		for (int i = 0; i < Count; i++)
		{
			KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
			array[arrayIndex + i] = keyValuePair;
		}
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
		}
		if (array.GetLowerBound(0) != 0)
		{
			throw new ArgumentException("The lower bound of target array must be zero.", "array");
		}
		if (index < 0 || index > array.Length)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		if (array is KeyValuePair<TKey, TValue>[] array2)
		{
			for (int i = 0; i < Count; i++)
			{
				array2[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
			}
			return;
		}
		if (!(array is object[] array3))
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
		}
		try
		{
			for (int j = 0; j < Count; j++)
			{
				array3[j + index] = new KeyValuePair<TKey, TValue>(keys[j], values[j]);
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
		}
	}

	private void EnsureCapacity(int min)
	{
		int num = ((keys.Length == 0) ? 4 : (keys.Length * 2));
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

	private TValue GetByIndex(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		return values[index];
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> of type <see cref="T:System.Collections.Generic.KeyValuePair`2" /> for the <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return new Enumerator(this, 1);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(this, 1);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new Enumerator(this, 2);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this, 1);
	}

	private TKey GetKey(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		return keys[index];
	}

	/// <summary>Searches for the specified key and returns the zero-based index within the entire <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>The zero-based index of <paramref name="key" /> within the entire <see cref="T:System.Collections.Generic.SortedList`2" />, if found; otherwise, -1.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.SortedList`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public int IndexOfKey(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = Array.BinarySearch(keys, 0, _size, key, comparer);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	/// <summary>Searches for the specified value and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <see cref="T:System.Collections.Generic.SortedList`2" />, if found; otherwise, -1.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.SortedList`2" />.  The value can be null for reference types.</param>
	public int IndexOfValue(TValue value)
	{
		return Array.IndexOf(values, value, 0, _size);
	}

	private void Insert(int index, TKey key, TValue value)
	{
		if (_size == keys.Length)
		{
			EnsureCapacity(_size + 1);
		}
		if (index < _size)
		{
			Array.Copy(keys, index, keys, index + 1, _size - index);
			Array.Copy(values, index, values, index + 1, _size - index);
		}
		keys[index] = key;
		values[index] = value;
		_size++;
		version++;
	}

	/// <summary>Gets the value associated with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.SortedList`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key whose value to get.</param>
	/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = IndexOfKey(key);
		if (num >= 0)
		{
			value = values[num];
			return true;
		}
		value = default(TValue);
		return false;
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.SortedList`2.Count" />.</exception>
	public void RemoveAt(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		_size--;
		if (index < _size)
		{
			Array.Copy(keys, index + 1, keys, index, _size - index);
			Array.Copy(values, index + 1, values, index, _size - index);
		}
		if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
		{
			keys[_size] = default(TKey);
		}
		if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
		{
			values[_size] = default(TValue);
		}
		version++;
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.SortedList`2" />.</summary>
	/// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.SortedList`2" />.</returns>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool Remove(TKey key)
	{
		int num = IndexOfKey(key);
		if (num >= 0)
		{
			RemoveAt(num);
		}
		return num >= 0;
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	void IDictionary.Remove(object key)
	{
		if (IsCompatibleKey(key))
		{
			Remove((TKey)key);
		}
	}

	/// <summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.SortedList`2" />, if that number is less than 90 percent of current capacity.</summary>
	public void TrimExcess()
	{
		int num = (int)((double)keys.Length * 0.9);
		if (_size < num)
		{
			Capacity = _size;
		}
	}

	private static bool IsCompatibleKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return key is TKey;
	}
}
