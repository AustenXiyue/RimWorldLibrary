using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections;

/// <summary>Represents a collection of key/value pairs that are sorted by the keys and are accessible by key and by index.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(SortedListDebugView))]
public class SortedList : IDictionary, ICollection, IEnumerable, ICloneable
{
	[Serializable]
	private class SyncSortedList : SortedList
	{
		private SortedList _list;

		private object _root;

		public override int Count
		{
			get
			{
				lock (_root)
				{
					return _list.Count;
				}
			}
		}

		public override object SyncRoot => _root;

		public override bool IsReadOnly => _list.IsReadOnly;

		public override bool IsFixedSize => _list.IsFixedSize;

		public override bool IsSynchronized => true;

		public override object this[object key]
		{
			get
			{
				lock (_root)
				{
					return _list[key];
				}
			}
			set
			{
				lock (_root)
				{
					_list[key] = value;
				}
			}
		}

		public override int Capacity
		{
			get
			{
				lock (_root)
				{
					return _list.Capacity;
				}
			}
		}

		internal SyncSortedList(SortedList list)
		{
			_list = list;
			_root = list.SyncRoot;
		}

		public override void Add(object key, object value)
		{
			lock (_root)
			{
				_list.Add(key, value);
			}
		}

		public override void Clear()
		{
			lock (_root)
			{
				_list.Clear();
			}
		}

		public override object Clone()
		{
			lock (_root)
			{
				return _list.Clone();
			}
		}

		public override bool Contains(object key)
		{
			lock (_root)
			{
				return _list.Contains(key);
			}
		}

		public override bool ContainsKey(object key)
		{
			lock (_root)
			{
				return _list.ContainsKey(key);
			}
		}

		public override bool ContainsValue(object key)
		{
			lock (_root)
			{
				return _list.ContainsValue(key);
			}
		}

		public override void CopyTo(Array array, int index)
		{
			lock (_root)
			{
				_list.CopyTo(array, index);
			}
		}

		public override object GetByIndex(int index)
		{
			lock (_root)
			{
				return _list.GetByIndex(index);
			}
		}

		public override IDictionaryEnumerator GetEnumerator()
		{
			lock (_root)
			{
				return _list.GetEnumerator();
			}
		}

		public override object GetKey(int index)
		{
			lock (_root)
			{
				return _list.GetKey(index);
			}
		}

		public override IList GetKeyList()
		{
			lock (_root)
			{
				return _list.GetKeyList();
			}
		}

		public override IList GetValueList()
		{
			lock (_root)
			{
				return _list.GetValueList();
			}
		}

		public override int IndexOfKey(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
			}
			lock (_root)
			{
				return _list.IndexOfKey(key);
			}
		}

		public override int IndexOfValue(object value)
		{
			lock (_root)
			{
				return _list.IndexOfValue(value);
			}
		}

		public override void RemoveAt(int index)
		{
			lock (_root)
			{
				_list.RemoveAt(index);
			}
		}

		public override void Remove(object key)
		{
			lock (_root)
			{
				_list.Remove(key);
			}
		}

		public override void SetByIndex(int index, object value)
		{
			lock (_root)
			{
				_list.SetByIndex(index, value);
			}
		}

		internal override KeyValuePairs[] ToKeyValuePairsArray()
		{
			return _list.ToKeyValuePairsArray();
		}

		public override void TrimToSize()
		{
			lock (_root)
			{
				_list.TrimToSize();
			}
		}
	}

	[Serializable]
	private class SortedListEnumerator : IDictionaryEnumerator, IEnumerator, ICloneable
	{
		private SortedList sortedList;

		private object key;

		private object value;

		private int index;

		private int startIndex;

		private int endIndex;

		private int version;

		private bool current;

		private int getObjectRetType;

		internal const int Keys = 1;

		internal const int Values = 2;

		internal const int DictEntry = 3;

		public virtual object Key
		{
			get
			{
				if (version != sortedList.version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
				}
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
				}
				return key;
			}
		}

		public virtual DictionaryEntry Entry
		{
			get
			{
				if (version != sortedList.version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
				}
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
				}
				return new DictionaryEntry(key, value);
			}
		}

		public virtual object Current
		{
			get
			{
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
				}
				if (getObjectRetType == 1)
				{
					return key;
				}
				if (getObjectRetType == 2)
				{
					return value;
				}
				return new DictionaryEntry(key, value);
			}
		}

		public virtual object Value
		{
			get
			{
				if (version != sortedList.version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
				}
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
				}
				return value;
			}
		}

		internal SortedListEnumerator(SortedList sortedList, int index, int count, int getObjRetType)
		{
			this.sortedList = sortedList;
			this.index = index;
			startIndex = index;
			endIndex = index + count;
			version = sortedList.version;
			getObjectRetType = getObjRetType;
			current = false;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (version != sortedList.version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (index < endIndex)
			{
				key = sortedList.keys[index];
				value = sortedList.values[index];
				index++;
				current = true;
				return true;
			}
			key = null;
			value = null;
			current = false;
			return false;
		}

		public virtual void Reset()
		{
			if (version != sortedList.version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			index = startIndex;
			current = false;
			key = null;
			value = null;
		}
	}

	[Serializable]
	private class KeyList : IList, ICollection, IEnumerable
	{
		private SortedList sortedList;

		public virtual int Count => sortedList._size;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => sortedList.IsSynchronized;

		public virtual object SyncRoot => sortedList.SyncRoot;

		public virtual object this[int index]
		{
			get
			{
				return sortedList.GetKey(index);
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("Mutating a key collection derived from a dictionary is not allowed."));
			}
		}

		internal KeyList(SortedList sortedList)
		{
			this.sortedList = sortedList;
		}

		public virtual int Add(object key)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual bool Contains(object key)
		{
			return sortedList.Contains(key);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			Array.Copy(sortedList.keys, 0, array, arrayIndex, sortedList.Count);
		}

		public virtual void Insert(int index, object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new SortedListEnumerator(sortedList, 0, sortedList.Count, 1);
		}

		public virtual int IndexOf(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
			}
			int num = Array.BinarySearch(sortedList.keys, 0, sortedList.Count, key, sortedList.comparer);
			if (num >= 0)
			{
				return num;
			}
			return -1;
		}

		public virtual void Remove(object key)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}
	}

	[Serializable]
	private class ValueList : IList, ICollection, IEnumerable
	{
		private SortedList sortedList;

		public virtual int Count => sortedList._size;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => sortedList.IsSynchronized;

		public virtual object SyncRoot => sortedList.SyncRoot;

		public virtual object this[int index]
		{
			get
			{
				return sortedList.GetByIndex(index);
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
			}
		}

		internal ValueList(SortedList sortedList)
		{
			this.sortedList = sortedList;
		}

		public virtual int Add(object key)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual bool Contains(object value)
		{
			return sortedList.ContainsValue(value);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			Array.Copy(sortedList.values, 0, array, arrayIndex, sortedList.Count);
		}

		public virtual void Insert(int index, object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new SortedListEnumerator(sortedList, 0, sortedList.Count, 2);
		}

		public virtual int IndexOf(object value)
		{
			return Array.IndexOf(sortedList.values, value, 0, sortedList.Count);
		}

		public virtual void Remove(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("This operation is not supported on SortedList nested types because they require modifying the original SortedList."));
		}
	}

	internal class SortedListDebugView
	{
		private SortedList sortedList;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePairs[] Items => sortedList.ToKeyValuePairsArray();

		public SortedListDebugView(SortedList sortedList)
		{
			if (sortedList == null)
			{
				throw new ArgumentNullException("sortedList");
			}
			this.sortedList = sortedList;
		}
	}

	private object[] keys;

	private object[] values;

	private int _size;

	private int version;

	private IComparer comparer;

	private KeyList keyList;

	private ValueList valueList;

	[NonSerialized]
	private object _syncRoot;

	private const int _defaultCapacity = 16;

	private static object[] emptyArray = EmptyArray<object>.Value;

	/// <summary>Gets or sets the capacity of a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Collections.SortedList" /> object can contain.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value assigned is less than the current number of elements in the <see cref="T:System.Collections.SortedList" /> object.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual int Capacity
	{
		get
		{
			return keys.Length;
		}
		set
		{
			if (value < Count)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
			}
			if (value == keys.Length)
			{
				return;
			}
			if (value > 0)
			{
				object[] destinationArray = new object[value];
				object[] destinationArray2 = new object[value];
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
				keys = emptyArray;
				values = emptyArray;
			}
		}
	}

	/// <summary>Gets the number of elements contained in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual int Count => _size;

	/// <summary>Gets the keys in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the keys in the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual ICollection Keys => GetKeyList();

	/// <summary>Gets the values in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the values in the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual ICollection Values => GetValueList();

	/// <summary>Gets a value indicating whether a <see cref="T:System.Collections.SortedList" /> object is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.SortedList" /> object is read-only; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether a <see cref="T:System.Collections.SortedList" /> object has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.SortedList" /> object has a fixed size; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsFixedSize => false;

	/// <summary>Gets a value indicating whether access to a <see cref="T:System.Collections.SortedList" /> object is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.SortedList" /> object is synchronized (thread safe); otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object SyncRoot
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

	/// <summary>Gets and sets the value associated with a specific key in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The value associated with the <paramref name="key" /> parameter in the <see cref="T:System.Collections.SortedList" /> object, if <paramref name="key" /> is found; otherwise, null.</returns>
	/// <param name="key">The key associated with the value to get or set. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.SortedList" /> object is read-only.-or- The property is set, <paramref name="key" /> does not exist in the collection, and the <see cref="T:System.Collections.SortedList" /> has a fixed size. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough available memory to add the element to the <see cref="T:System.Collections.SortedList" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The comparer throws an exception. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual object this[object key]
	{
		get
		{
			int num = IndexOfKey(key);
			if (num >= 0)
			{
				return values[num];
			}
			return null;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.SortedList" /> class that is empty, has the default initial capacity, and is sorted according to the <see cref="T:System.IComparable" /> interface implemented by each key added to the <see cref="T:System.Collections.SortedList" /> object.</summary>
	public SortedList()
	{
		Init();
	}

	private void Init()
	{
		keys = emptyArray;
		values = emptyArray;
		_size = 0;
		comparer = new Comparer(CultureInfo.CurrentCulture);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.SortedList" /> class that is empty, has the specified initial capacity, and is sorted according to the <see cref="T:System.IComparable" /> interface implemented by each key added to the <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <param name="initialCapacity">The initial number of elements that the <see cref="T:System.Collections.SortedList" /> object can contain. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="initialCapacity" /> is less than zero. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough available memory to create a <see cref="T:System.Collections.SortedList" /> object with the specified <paramref name="initialCapacity" />.</exception>
	public SortedList(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity", Environment.GetResourceString("Non-negative number required."));
		}
		keys = new object[initialCapacity];
		values = new object[initialCapacity];
		comparer = new Comparer(CultureInfo.CurrentCulture);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.SortedList" /> class that is empty, has the default initial capacity, and is sorted according to the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing keys.-or- null to use the <see cref="T:System.IComparable" /> implementation of each key. </param>
	public SortedList(IComparer comparer)
		: this()
	{
		if (comparer != null)
		{
			this.comparer = comparer;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.SortedList" /> class that is empty, has the specified initial capacity, and is sorted according to the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing keys.-or- null to use the <see cref="T:System.IComparable" /> implementation of each key. </param>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.SortedList" /> object can contain. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough available memory to create a <see cref="T:System.Collections.SortedList" /> object with the specified <paramref name="capacity" />.</exception>
	public SortedList(IComparer comparer, int capacity)
		: this(comparer)
	{
		Capacity = capacity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.SortedList" /> class that contains elements copied from the specified dictionary, has the same initial capacity as the number of elements copied, and is sorted according to the <see cref="T:System.IComparable" /> interface implemented by each key.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> implementation to copy to a new <see cref="T:System.Collections.SortedList" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">One or more elements in <paramref name="d" /> do not implement the <see cref="T:System.IComparable" /> interface. </exception>
	public SortedList(IDictionary d)
		: this(d, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.SortedList" /> class that contains elements copied from the specified dictionary, has the same initial capacity as the number of elements copied, and is sorted according to the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> implementation to copy to a new <see cref="T:System.Collections.SortedList" /> object.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing keys.-or- null to use the <see cref="T:System.IComparable" /> implementation of each key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="comparer" /> is null, and one or more elements in <paramref name="d" /> do not implement the <see cref="T:System.IComparable" /> interface. </exception>
	public SortedList(IDictionary d, IComparer comparer)
		: this(comparer, d?.Count ?? 0)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d", Environment.GetResourceString("Dictionary cannot be null."));
		}
		d.Keys.CopyTo(keys, 0);
		d.Values.CopyTo(values, 0);
		Array.Sort(keys, values, comparer);
		_size = d.Count;
	}

	/// <summary>Adds an element with the specified key and value to a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <param name="key">The key of the element to add. </param>
	/// <param name="value">The value of the element to add. The value can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">An element with the specified <paramref name="key" /> already exists in the <see cref="T:System.Collections.SortedList" /> object.-or- The <see cref="T:System.Collections.SortedList" /> is set to use the <see cref="T:System.IComparable" /> interface, and <paramref name="key" /> does not implement the <see cref="T:System.IComparable" /> interface. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.SortedList" /> is read-only.-or- The <see cref="T:System.Collections.SortedList" /> has a fixed size. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough available memory to add the element to the <see cref="T:System.Collections.SortedList" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The comparer throws an exception. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Add(object key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
		}
		int num = Array.BinarySearch(keys, 0, _size, key, comparer);
		if (num >= 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Item has already been added. Key in dictionary: '{0}'  Key being added: '{1}'", GetKey(num), key));
		}
		Insert(~num, key, value);
	}

	/// <summary>Removes all elements from a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.SortedList" /> object is read-only.-or- The <see cref="T:System.Collections.SortedList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Clear()
	{
		version++;
		Array.Clear(keys, 0, _size);
		Array.Clear(values, 0, _size);
		_size = 0;
	}

	/// <summary>Creates a shallow copy of a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public virtual object Clone()
	{
		SortedList sortedList = new SortedList(_size);
		Array.Copy(keys, 0, sortedList.keys, 0, _size);
		Array.Copy(values, 0, sortedList.values, 0, _size);
		sortedList._size = _size;
		sortedList.version = version;
		sortedList.comparer = comparer;
		return sortedList;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.SortedList" /> object contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.SortedList" /> object contains an element with the specified <paramref name="key" />; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.SortedList" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The comparer throws an exception. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual bool Contains(object key)
	{
		return IndexOfKey(key) >= 0;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.SortedList" /> object contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.SortedList" /> object contains an element with the specified <paramref name="key" />; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.SortedList" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The comparer throws an exception. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual bool ContainsKey(object key)
	{
		return IndexOfKey(key) >= 0;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.SortedList" /> object contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Collections.SortedList" /> object contains an element with the specified <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.SortedList" /> object. The value can be null. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool ContainsValue(object value)
	{
		return IndexOfValue(value) >= 0;
	}

	/// <summary>Copies <see cref="T:System.Collections.SortedList" /> elements to a one-dimensional <see cref="T:System.Array" /> object, starting at the specified index in the array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> object that is the destination of the <see cref="T:System.Collections.DictionaryEntry" /> objects copied from <see cref="T:System.Collections.SortedList" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.SortedList" /> object is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.SortedList" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void CopyTo(Array array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", Environment.GetResourceString("Array cannot be null."));
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException(Environment.GetResourceString("Destination array is not long enough to copy all the items in the collection. Check array index and length."));
		}
		for (int i = 0; i < Count; i++)
		{
			DictionaryEntry dictionaryEntry = new DictionaryEntry(keys[i], values[i]);
			array.SetValue(dictionaryEntry, i + arrayIndex);
		}
	}

	internal virtual KeyValuePairs[] ToKeyValuePairsArray()
	{
		KeyValuePairs[] array = new KeyValuePairs[Count];
		for (int i = 0; i < Count; i++)
		{
			array[i] = new KeyValuePairs(keys[i], values[i]);
		}
		return array;
	}

	private void EnsureCapacity(int min)
	{
		int num = ((keys.Length == 0) ? 16 : (keys.Length * 2));
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

	/// <summary>Gets the value at the specified index of a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The value at the specified index of the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <param name="index">The zero-based index of the value to get. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.SortedList" /> object. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual object GetByIndex(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return values[index];
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that iterates through the <see cref="T:System.Collections.SortedList" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.SortedList" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new SortedListEnumerator(this, 0, _size, 3);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object that iterates through a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return new SortedListEnumerator(this, 0, _size, 3);
	}

	/// <summary>Gets the key at the specified index of a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The key at the specified index of the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <param name="index">The zero-based index of the key to get. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.SortedList" /> object.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual object GetKey(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return keys[index];
	}

	/// <summary>Gets the keys in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> object containing the keys in the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IList GetKeyList()
	{
		if (keyList == null)
		{
			keyList = new KeyList(this);
		}
		return keyList;
	}

	/// <summary>Gets the values in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> object containing the values in the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IList GetValueList()
	{
		if (valueList == null)
		{
			valueList = new ValueList(this);
		}
		return valueList;
	}

	/// <summary>Returns the zero-based index of the specified key in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The zero-based index of the <paramref name="key" /> parameter, if <paramref name="key" /> is found in the <see cref="T:System.Collections.SortedList" /> object; otherwise, -1.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.SortedList" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The comparer throws an exception. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int IndexOfKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
		}
		int num = Array.BinarySearch(keys, 0, _size, key, comparer);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	/// <summary>Returns the zero-based index of the first occurrence of the specified value in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>The zero-based index of the first occurrence of the <paramref name="value" /> parameter, if <paramref name="value" /> is found in the <see cref="T:System.Collections.SortedList" /> object; otherwise, -1.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.SortedList" /> object. The value can be null. </param>
	/// <filterpriority>1</filterpriority>
	public virtual int IndexOfValue(object value)
	{
		return Array.IndexOf(values, value, 0, _size);
	}

	private void Insert(int index, object key, object value)
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

	/// <summary>Removes the element at the specified index of a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <param name="index">The zero-based index of the element to remove. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.SortedList" /> object. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.SortedList" /> is read-only.-or- The <see cref="T:System.Collections.SortedList" /> has a fixed size. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		_size--;
		if (index < _size)
		{
			Array.Copy(keys, index + 1, keys, index, _size - index);
			Array.Copy(values, index + 1, values, index, _size - index);
		}
		keys[_size] = null;
		values[_size] = null;
		version++;
	}

	/// <summary>Removes the element with the specified key from a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <param name="key">The key of the element to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.SortedList" /> object is read-only.-or- The <see cref="T:System.Collections.SortedList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Remove(object key)
	{
		int num = IndexOfKey(key);
		if (num >= 0)
		{
			RemoveAt(num);
		}
	}

	/// <summary>Replaces the value at a specific index in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <param name="index">The zero-based index at which to save <paramref name="value" />. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to save into the <see cref="T:System.Collections.SortedList" /> object. The value can be null. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.SortedList" /> object. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void SetByIndex(int index, object value)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		values[index] = value;
		version++;
	}

	/// <summary>Returns a synchronized (thread-safe) wrapper for a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <returns>A synchronized (thread-safe) wrapper for the <see cref="T:System.Collections.SortedList" /> object.</returns>
	/// <param name="list">The <see cref="T:System.Collections.SortedList" /> object to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static SortedList Synchronized(SortedList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new SyncSortedList(list);
	}

	/// <summary>Sets the capacity to the actual number of elements in a <see cref="T:System.Collections.SortedList" /> object.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.SortedList" /> object is read-only.-or- The <see cref="T:System.Collections.SortedList" /> has a fixed size. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void TrimToSize()
	{
		Capacity = _size;
	}
}
