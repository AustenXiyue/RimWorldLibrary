using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Concurrent;

/// <summary>Represents a thread-safe collection of key/value pairs that can be accessed by multiple threads concurrently. </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(IDictionaryDebugView<, >))]
public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
	private sealed class Tables
	{
		internal readonly Node[] _buckets;

		internal readonly object[] _locks;

		internal volatile int[] _countPerLock;

		internal Tables(Node[] buckets, object[] locks, int[] countPerLock)
		{
			_buckets = buckets;
			_locks = locks;
			_countPerLock = countPerLock;
		}
	}

	[Serializable]
	private sealed class Node
	{
		internal readonly TKey _key;

		internal TValue _value;

		internal volatile Node _next;

		internal readonly int _hashcode;

		internal Node(TKey key, TValue value, int hashcode, Node next)
		{
			_key = key;
			_value = value;
			_next = next;
			_hashcode = hashcode;
		}
	}

	[Serializable]
	private sealed class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

		public DictionaryEntry Entry => new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);

		public object Key => _enumerator.Current.Key;

		public object Value => _enumerator.Current.Value;

		public object Current => Entry;

		internal DictionaryEnumerator(ConcurrentDictionary<TKey, TValue> dictionary)
		{
			_enumerator = dictionary.GetEnumerator();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator.Reset();
		}
	}

	private volatile Tables _tables;

	private IEqualityComparer<TKey> _comparer;

	private readonly bool _growLockArray;

	private int _budget;

	private const int DefaultCapacity = 31;

	private const int MaxLockNumber = 1024;

	private static readonly bool s_isValueWriteAtomic = IsValueWriteAtomic();

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value of the key/value pair at the specified index.</returns>
	/// <param name="key">The key of the value to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is  null.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
	public TValue this[TKey key]
	{
		get
		{
			if (!TryGetValue(key, out var value))
			{
				ThrowKeyNotFoundException();
			}
			return value;
		}
		set
		{
			if (key == null)
			{
				ThrowKeyNullException();
			}
			TryAddInternal(key, _comparer.GetHashCode(key), value, updateIfExists: true, acquireLock: true, out var _);
		}
	}

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	public int Count
	{
		get
		{
			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);
				return GetCountInternal();
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> is empty.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> is empty; otherwise, false.</returns>
	public bool IsEmpty
	{
		get
		{
			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);
				for (int i = 0; i < _tables._countPerLock.Length; i++)
				{
					if (_tables._countPerLock[i] != 0)
					{
						return false;
					}
				}
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
			return true;
		}
	}

	/// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>A collection of keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	public ICollection<TKey> Keys => GetKeys();

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => GetKeys();

	/// <summary>Gets a collection that contains the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>A collection that contains the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />. </returns>
	public ICollection<TValue> Values => GetValues();

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => GetValues();

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> has a fixed size; otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />, this property always returns false.</returns>
	bool IDictionary.IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only; otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />, this property always returns false.</returns>
	bool IDictionary.IsReadOnly => false;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys of the  <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
	/// <returns>An interface that contains the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	ICollection IDictionary.Keys => GetKeys();

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An interface that contains the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Values => GetValues();

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key, or  null if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is not assignable to the key type of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
	/// <param name="key">The key of the value to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is  null.</exception>
	/// <exception cref="T:System.ArgumentException">A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the key type or the value type of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</exception>
	object IDictionary.this[object key]
	{
		get
		{
			if (key == null)
			{
				ThrowKeyNullException();
			}
			if (key is TKey && TryGetValue((TKey)key, out var value))
			{
				return value;
			}
			return null;
		}
		set
		{
			if (key == null)
			{
				ThrowKeyNullException();
			}
			if (!(key is TKey))
			{
				throw new ArgumentException("The key was of an incorrect type for this dictionary.");
			}
			if (!(value is TValue))
			{
				throw new ArgumentException("The value was of an incorrect type for this dictionary.");
			}
			this[(TKey)key] = (TValue)value;
		}
	}

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. This property is not supported.</summary>
	/// <returns>Always returns null.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported.</exception>
	object ICollection.SyncRoot
	{
		get
		{
			throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections.");
		}
	}

	private static int DefaultConcurrencyLevel => PlatformHelper.ProcessorCount;

	private static bool IsValueWriteAtomic()
	{
		Type typeFromHandle = typeof(TValue);
		if (!typeFromHandle.IsValueType)
		{
			return true;
		}
		switch (Type.GetTypeCode(typeFromHandle))
		{
		case TypeCode.Boolean:
		case TypeCode.Char:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Single:
			return true;
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Double:
			return IntPtr.Size == 8;
		default:
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.</summary>
	public ConcurrentDictionary()
		: this(DefaultConcurrencyLevel, 31, growLockArray: true, (IEqualityComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the specified concurrency level and capacity, and uses the default comparer for the key type.</summary>
	/// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.</param>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="concurrencyLevel" /> is less than 1.-or-<paramref name="capacity" /> is less than 0.</exception>
	public ConcurrentDictionary(int concurrencyLevel, int capacity)
		: this(concurrencyLevel, capacity, growLockArray: false, (IEqualityComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" />, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.</summary>
	/// <param name="collection">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> or any of its keys is  null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="collection" /> contains one or more duplicate keys.</exception>
	public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
		: this(collection, (IEqualityComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the default concurrency level and capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="comparer" /> is null.</exception>
	public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
		: this(DefaultConcurrencyLevel, 31, growLockArray: true, comparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" /> has the default concurrency level, has the default initial capacity, and uses the specified  <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="collection">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> or <paramref name="comparer" /> is null.</exception>
	public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
		: this(comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		InitializeFromCollection(collection);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.</param>
	/// <param name="collection">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> or <paramref name="comparer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="concurrencyLevel" /> is less than 1.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="collection" /> contains one or more duplicate keys.</exception>
	public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
		: this(concurrencyLevel, 31, growLockArray: false, comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		InitializeFromCollection(collection);
	}

	private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
	{
		foreach (KeyValuePair<TKey, TValue> item in collection)
		{
			if (item.Key == null)
			{
				ThrowKeyNullException();
			}
			if (!TryAddInternal(item.Key, _comparer.GetHashCode(item.Key), item.Value, updateIfExists: false, acquireLock: false, out var _))
			{
				throw new ArgumentException("The source argument contains duplicate keys.");
			}
		}
		if (_budget == 0)
		{
			_budget = _tables._buckets.Length / _tables._locks.Length;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the specified concurrency level, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.</param>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="comparer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="concurrencyLevel" /> or <paramref name="capacity" /> is less than 1.</exception>
	public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
		: this(concurrencyLevel, capacity, growLockArray: false, comparer)
	{
	}

	internal ConcurrentDictionary(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<TKey> comparer)
	{
		if (concurrencyLevel < 1)
		{
			throw new ArgumentOutOfRangeException("concurrencyLevel", "The concurrencyLevel argument must be positive.");
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "The capacity argument must be greater than or equal to zero.");
		}
		if (capacity < concurrencyLevel)
		{
			capacity = concurrencyLevel;
		}
		object[] array = new object[concurrencyLevel];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new object();
		}
		int[] countPerLock = new int[array.Length];
		Node[] array2 = new Node[capacity];
		_tables = new Tables(array2, array, countPerLock);
		_comparer = comparer ?? EqualityComparer<TKey>.Default;
		_growLockArray = growLockArray;
		_budget = array2.Length / array.Length;
	}

	/// <summary>Attempts to add the specified key and value to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	/// <returns>true if the key/value pair was added to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> successfully; false if the key already exists.</returns>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be  null for reference types.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is  null.</exception>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	public bool TryAdd(TKey key, TValue value)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		TValue resultingValue;
		return TryAddInternal(key, _comparer.GetHashCode(key), value, updateIfExists: false, acquireLock: true, out resultingValue);
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> contains the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool ContainsKey(TKey key)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		TValue value;
		return TryGetValue(key, out value);
	}

	/// <summary>Attempts to remove and return the value that has the specified key from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	/// <returns>true if the object was removed successfully; otherwise, false.</returns>
	/// <param name="key">The key of the element to remove and return.</param>
	/// <param name="value">When this method returns, contains the object removed from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />, or the default value of  the TValue type if <paramref name="key" /> does not exist. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is  null.</exception>
	public bool TryRemove(TKey key, out TValue value)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		return TryRemoveInternal(key, out value, matchValue: false, default(TValue));
	}

	private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
	{
		int hashCode = _comparer.GetHashCode(key);
		while (true)
		{
			Tables tables = _tables;
			GetBucketAndLockNo(hashCode, out var bucketNo, out var lockNo, tables._buckets.Length, tables._locks.Length);
			lock (tables._locks[lockNo])
			{
				if (tables != _tables)
				{
					continue;
				}
				Node node = null;
				for (Node node2 = tables._buckets[bucketNo]; node2 != null; node2 = node2._next)
				{
					if (hashCode == node2._hashcode && _comparer.Equals(node2._key, key))
					{
						if (matchValue && !EqualityComparer<TValue>.Default.Equals(oldValue, node2._value))
						{
							value = default(TValue);
							return false;
						}
						if (node == null)
						{
							Volatile.Write(ref tables._buckets[bucketNo], node2._next);
						}
						else
						{
							node._next = node2._next;
						}
						value = node2._value;
						tables._countPerLock[lockNo]--;
						return true;
					}
					node = node2;
				}
				break;
			}
		}
		value = default(TValue);
		return false;
	}

	/// <summary>Attempts to get the value associated with the specified key from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	/// <returns>true if the key was found in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />; otherwise, false.</returns>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the object from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> that has the specified key, or the default value of , if the operation failed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is  null.</exception>
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		return TryGetValueInternal(key, _comparer.GetHashCode(key), out value);
	}

	private bool TryGetValueInternal(TKey key, int hashcode, out TValue value)
	{
		Tables tables = _tables;
		int bucket = GetBucket(hashcode, tables._buckets.Length);
		for (Node node = Volatile.Read(ref tables._buckets[bucket]); node != null; node = node._next)
		{
			if (hashcode == node._hashcode && _comparer.Equals(node._key, key))
			{
				value = node._value;
				return true;
			}
		}
		value = default(TValue);
		return false;
	}

	/// <summary>Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value.</summary>
	/// <returns>true if the value with <paramref name="key" /> was equal to <paramref name="comparisonValue" /> and was replaced with <paramref name="newValue" />; otherwise, false.</returns>
	/// <param name="key">The key whose value is compared with <paramref name="comparisonValue" /> and possibly replaced.</param>
	/// <param name="newValue">The value that replaces the value of the element that has the specified <paramref name="key" /> if the comparison results in equality.</param>
	/// <param name="comparisonValue">The value that is compared to the value of the element that has the specified <paramref name="key" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		return TryUpdateInternal(key, _comparer.GetHashCode(key), newValue, comparisonValue);
	}

	private bool TryUpdateInternal(TKey key, int hashcode, TValue newValue, TValue comparisonValue)
	{
		IEqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
		while (true)
		{
			Tables tables = _tables;
			GetBucketAndLockNo(hashcode, out var bucketNo, out var lockNo, tables._buckets.Length, tables._locks.Length);
			lock (tables._locks[lockNo])
			{
				if (tables != _tables)
				{
					continue;
				}
				Node node = null;
				for (Node node2 = tables._buckets[bucketNo]; node2 != null; node2 = node2._next)
				{
					if (hashcode == node2._hashcode && _comparer.Equals(node2._key, key))
					{
						if (@default.Equals(node2._value, comparisonValue))
						{
							if (s_isValueWriteAtomic)
							{
								node2._value = newValue;
							}
							else
							{
								Node node3 = new Node(node2._key, newValue, hashcode, node2._next);
								if (node == null)
								{
									Volatile.Write(ref tables._buckets[bucketNo], node3);
								}
								else
								{
									node._next = node3;
								}
							}
							return true;
						}
						return false;
					}
					node = node2;
				}
				return false;
			}
		}
	}

	/// <summary>Removes all keys and values from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	public void Clear()
	{
		int locksAcquired = 0;
		try
		{
			AcquireAllLocks(ref locksAcquired);
			Tables tables = (_tables = new Tables(new Node[31], _tables._locks, new int[_tables._countPerLock.Length]));
			_budget = Math.Max(1, tables._buckets.Length / tables._locks.Length);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "The index argument is less than zero.");
		}
		int locksAcquired = 0;
		try
		{
			AcquireAllLocks(ref locksAcquired);
			int num = 0;
			for (int i = 0; i < _tables._locks.Length; i++)
			{
				if (num < 0)
				{
					break;
				}
				num += _tables._countPerLock[i];
			}
			if (array.Length - num < index || num < 0)
			{
				throw new ArgumentException("The index is equal to or greater than the length of the array, or the number of elements in the dictionary is greater than the available space from index to the end of the destination array.");
			}
			CopyToPairs(array, index);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	/// <summary>Copies the key and value pairs stored in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> to a new array.</summary>
	/// <returns>A new array containing a snapshot of key and value pairs copied from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
	public KeyValuePair<TKey, TValue>[] ToArray()
	{
		int locksAcquired = 0;
		checked
		{
			try
			{
				AcquireAllLocks(ref locksAcquired);
				int num = 0;
				for (int i = 0; i < _tables._locks.Length; i++)
				{
					num += _tables._countPerLock[i];
				}
				if (num == 0)
				{
					return Array.Empty<KeyValuePair<TKey, TValue>>();
				}
				KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[num];
				CopyToPairs(array, 0);
				return array;
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
		}
	}

	private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
	{
		Node[] buckets = _tables._buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			for (Node node = buckets[i]; node != null; node = node._next)
			{
				array[index] = new KeyValuePair<TKey, TValue>(node._key, node._value);
				index++;
			}
		}
	}

	private void CopyToEntries(DictionaryEntry[] array, int index)
	{
		Node[] buckets = _tables._buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			for (Node node = buckets[i]; node != null; node = node._next)
			{
				array[index] = new DictionaryEntry(node._key, node._value);
				index++;
			}
		}
	}

	private void CopyToObjects(object[] array, int index)
	{
		Node[] buckets = _tables._buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			for (Node node = buckets[i]; node != null; node = node._next)
			{
				array[index] = new KeyValuePair<TKey, TValue>(node._key, node._value);
				index++;
			}
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	/// <returns>An enumerator for the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		Node[] buckets = _tables._buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			for (Node current = Volatile.Read(ref buckets[i]); current != null; current = current._next)
			{
				yield return new KeyValuePair<TKey, TValue>(current._key, current._value);
			}
		}
	}

	private bool TryAddInternal(TKey key, int hashcode, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
	{
		checked
		{
			Tables tables;
			bool flag;
			while (true)
			{
				tables = _tables;
				GetBucketAndLockNo(hashcode, out var bucketNo, out var lockNo, tables._buckets.Length, tables._locks.Length);
				flag = false;
				bool lockTaken = false;
				try
				{
					if (acquireLock)
					{
						Monitor.Enter(tables._locks[lockNo], ref lockTaken);
					}
					if (tables != _tables)
					{
						continue;
					}
					Node node = null;
					for (Node node2 = tables._buckets[bucketNo]; node2 != null; node2 = node2._next)
					{
						if (hashcode == node2._hashcode && _comparer.Equals(node2._key, key))
						{
							if (updateIfExists)
							{
								if (s_isValueWriteAtomic)
								{
									node2._value = value;
								}
								else
								{
									Node node3 = new Node(node2._key, value, hashcode, node2._next);
									if (node == null)
									{
										Volatile.Write(ref tables._buckets[bucketNo], node3);
									}
									else
									{
										node._next = node3;
									}
								}
								resultingValue = value;
							}
							else
							{
								resultingValue = node2._value;
							}
							return false;
						}
						node = node2;
					}
					Volatile.Write(ref tables._buckets[bucketNo], new Node(key, value, hashcode, tables._buckets[bucketNo]));
					tables._countPerLock[lockNo]++;
					if (tables._countPerLock[lockNo] > _budget)
					{
						flag = true;
					}
					break;
				}
				finally
				{
					if (lockTaken)
					{
						Monitor.Exit(tables._locks[lockNo]);
					}
				}
			}
			if (flag)
			{
				GrowTable(tables);
			}
			resultingValue = value;
			return true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ThrowKeyNotFoundException()
	{
		throw new KeyNotFoundException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ThrowKeyNullException()
	{
		throw new ArgumentNullException("key");
	}

	private int GetCountInternal()
	{
		int num = 0;
		for (int i = 0; i < _tables._countPerLock.Length; i++)
		{
			num += _tables._countPerLock[i];
		}
		return num;
	}

	/// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> by using the specified function, if the key does not already exist.</summary>
	/// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="valueFactory">The function used to generate a value for the key</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> or <paramref name="valueFactory" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		int hashCode = _comparer.GetHashCode(key);
		if (!TryGetValueInternal(key, hashCode, out var value))
		{
			TryAddInternal(key, hashCode, valueFactory(key), updateIfExists: false, acquireLock: true, out value);
		}
		return value;
	}

	public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		int hashCode = _comparer.GetHashCode(key);
		if (!TryGetValueInternal(key, hashCode, out var value))
		{
			TryAddInternal(key, hashCode, valueFactory(key, factoryArgument), updateIfExists: false, acquireLock: true, out value);
		}
		return value;
	}

	/// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist.</summary>
	/// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">the value to be added, if the key does not already exist</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	public TValue GetOrAdd(TKey key, TValue value)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		int hashCode = _comparer.GetHashCode(key);
		if (!TryGetValueInternal(key, hashCode, out var value2))
		{
			TryAddInternal(key, hashCode, value, updateIfExists: false, acquireLock: true, out value2);
		}
		return value2;
	}

	public TValue AddOrUpdate<TArg>(TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (addValueFactory == null)
		{
			throw new ArgumentNullException("addValueFactory");
		}
		if (updateValueFactory == null)
		{
			throw new ArgumentNullException("updateValueFactory");
		}
		int hashCode = _comparer.GetHashCode(key);
		TValue resultingValue;
		while (true)
		{
			if (TryGetValueInternal(key, hashCode, out var value))
			{
				TValue val = updateValueFactory(key, value, factoryArgument);
				if (TryUpdateInternal(key, hashCode, val, value))
				{
					return val;
				}
			}
			else if (TryAddInternal(key, hashCode, addValueFactory(key, factoryArgument), updateIfExists: false, acquireLock: true, out resultingValue))
			{
				break;
			}
		}
		return resultingValue;
	}

	/// <summary>Uses the specified functions to add a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist, or to update a key/value pair in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key already exists.</summary>
	/// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
	/// <param name="key">The key to be added or whose value should be updated</param>
	/// <param name="addValueFactory">The function used to generate a value for an absent key</param>
	/// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" />, <paramref name="addValueFactory" />, or <paramref name="updateValueFactory" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (addValueFactory == null)
		{
			throw new ArgumentNullException("addValueFactory");
		}
		if (updateValueFactory == null)
		{
			throw new ArgumentNullException("updateValueFactory");
		}
		int hashCode = _comparer.GetHashCode(key);
		TValue resultingValue;
		while (true)
		{
			if (TryGetValueInternal(key, hashCode, out var value))
			{
				TValue val = updateValueFactory(key, value);
				if (TryUpdateInternal(key, hashCode, val, value))
				{
					return val;
				}
			}
			else if (TryAddInternal(key, hashCode, addValueFactory(key), updateIfExists: false, acquireLock: true, out resultingValue))
			{
				break;
			}
		}
		return resultingValue;
	}

	/// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist, or updates a key/value pair in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> by using the specified function if the key already exists.</summary>
	/// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
	/// <param name="key">The key to be added or whose value should be updated</param>
	/// <param name="addValue">The value to be added for an absent key</param>
	/// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> or <paramref name="updateValueFactory" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (updateValueFactory == null)
		{
			throw new ArgumentNullException("updateValueFactory");
		}
		int hashCode = _comparer.GetHashCode(key);
		TValue resultingValue;
		while (true)
		{
			if (TryGetValueInternal(key, hashCode, out var value))
			{
				TValue val = updateValueFactory(key, value);
				if (TryUpdateInternal(key, hashCode, val, value))
				{
					return val;
				}
			}
			else if (TryAddInternal(key, hashCode, addValue, updateIfExists: false, acquireLock: true, out resultingValue))
			{
				break;
			}
		}
		return resultingValue;
	}

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		if (!TryAdd(key, value))
		{
			throw new ArgumentException("The key already existed in the dictionary.");
		}
	}

	bool IDictionary<TKey, TValue>.Remove(TKey key)
	{
		TValue value;
		return TryRemove(key, out value);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
	{
		((IDictionary<TKey, TValue>)this).Add(keyValuePair.Key, keyValuePair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		if (!TryGetValue(keyValuePair.Key, out var value))
		{
			return false;
		}
		return EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
	{
		if (keyValuePair.Key == null)
		{
			throw new ArgumentNullException("keyValuePair", "TKey is a reference type and item.Key is null.");
		}
		TValue value;
		return TryRemoveInternal(keyValuePair.Key, out value, matchValue: true, keyValuePair.Value);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
	/// <returns>An enumerator for the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds the specified key and value to the dictionary.</summary>
	/// <param name="key">The object to use as the key.</param>
	/// <param name="value">The object to use as the value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is of a type that is not assignable to the key type  of the <see cref="T:System.Collections.Generic.Dictionary`2" />. -or- <paramref name="value" /> is of a type that is not assignable to the type of values in the <see cref="T:System.Collections.Generic.Dictionary`2" />. -or-A value with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
	/// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
	void IDictionary.Add(object key, object value)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (!(key is TKey))
		{
			throw new ArgumentException("The key was of an incorrect type for this dictionary.");
		}
		TValue value2;
		try
		{
			value2 = (TValue)value;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException("The value was of an incorrect type for this dictionary.");
		}
		((IDictionary<TKey, TValue>)this).Add((TKey)key, value2);
	}

	/// <summary>Gets whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	bool IDictionary.Contains(object key)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (key is TKey)
		{
			return ContainsKey((TKey)key);
		}
		return false;
	}

	/// <summary>Provides a <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new DictionaryEnumerator(this);
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	void IDictionary.Remove(object key)
	{
		if (key == null)
		{
			ThrowKeyNullException();
		}
		if (key is TKey)
		{
			TryRemove((TKey)key, out var _);
		}
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.ICollection" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" />. -or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "The index argument is less than zero.");
		}
		int locksAcquired = 0;
		try
		{
			AcquireAllLocks(ref locksAcquired);
			Tables tables = _tables;
			int num = 0;
			for (int i = 0; i < tables._locks.Length; i++)
			{
				if (num < 0)
				{
					break;
				}
				num += tables._countPerLock[i];
			}
			if (array.Length - num < index || num < 0)
			{
				throw new ArgumentException("The index is equal to or greater than the length of the array, or the number of elements in the dictionary is greater than the available space from index to the end of the destination array.");
			}
			if (array is KeyValuePair<TKey, TValue>[] array2)
			{
				CopyToPairs(array2, index);
				return;
			}
			if (array is DictionaryEntry[] array3)
			{
				CopyToEntries(array3, index);
				return;
			}
			if (array is object[] array4)
			{
				CopyToObjects(array4, index);
				return;
			}
			throw new ArgumentException("The array is multidimensional, or the type parameter for the set cannot be cast automatically to the type of the destination array.", "array");
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	private void GrowTable(Tables tables)
	{
		int locksAcquired = 0;
		try
		{
			AcquireLocks(0, 1, ref locksAcquired);
			if (tables != _tables)
			{
				return;
			}
			long num = 0L;
			for (int i = 0; i < tables._countPerLock.Length; i++)
			{
				num += tables._countPerLock[i];
			}
			if (num < tables._buckets.Length / 4)
			{
				_budget = 2 * _budget;
				if (_budget < 0)
				{
					_budget = int.MaxValue;
				}
				return;
			}
			int j = 0;
			bool flag = false;
			try
			{
				for (j = checked(tables._buckets.Length * 2 + 1); j % 3 == 0 || j % 5 == 0 || j % 7 == 0; j = checked(j + 2))
				{
				}
				if (j > 2146435071)
				{
					flag = true;
				}
			}
			catch (OverflowException)
			{
				flag = true;
			}
			if (flag)
			{
				j = 2146435071;
				_budget = int.MaxValue;
			}
			AcquireLocks(1, tables._locks.Length, ref locksAcquired);
			object[] array = tables._locks;
			if (_growLockArray && tables._locks.Length < 1024)
			{
				array = new object[tables._locks.Length * 2];
				Array.Copy(tables._locks, 0, array, 0, tables._locks.Length);
				for (int k = tables._locks.Length; k < array.Length; k++)
				{
					array[k] = new object();
				}
			}
			Node[] array2 = new Node[j];
			int[] array3 = new int[array.Length];
			for (int l = 0; l < tables._buckets.Length; l++)
			{
				Node node = tables._buckets[l];
				checked
				{
					while (node != null)
					{
						Node next = node._next;
						GetBucketAndLockNo(node._hashcode, out var bucketNo, out var lockNo, array2.Length, array.Length);
						array2[bucketNo] = new Node(node._key, node._value, node._hashcode, array2[bucketNo]);
						array3[lockNo]++;
						node = next;
					}
				}
			}
			_budget = Math.Max(1, array2.Length / array.Length);
			_tables = new Tables(array2, array, array3);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	private static int GetBucket(int hashcode, int bucketCount)
	{
		return (hashcode & 0x7FFFFFFF) % bucketCount;
	}

	private static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
	{
		bucketNo = (hashcode & 0x7FFFFFFF) % bucketCount;
		lockNo = bucketNo % lockCount;
	}

	private void AcquireAllLocks(ref int locksAcquired)
	{
		if (CDSCollectionETWBCLProvider.Log.IsEnabled())
		{
			CDSCollectionETWBCLProvider.Log.ConcurrentDictionary_AcquiringAllLocks(_tables._buckets.Length);
		}
		AcquireLocks(0, 1, ref locksAcquired);
		AcquireLocks(1, _tables._locks.Length, ref locksAcquired);
	}

	private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
	{
		object[] locks = _tables._locks;
		for (int i = fromInclusive; i < toExclusive; i++)
		{
			bool lockTaken = false;
			try
			{
				Monitor.Enter(locks[i], ref lockTaken);
			}
			finally
			{
				if (lockTaken)
				{
					locksAcquired++;
				}
			}
		}
	}

	private void ReleaseLocks(int fromInclusive, int toExclusive)
	{
		for (int i = fromInclusive; i < toExclusive; i++)
		{
			Monitor.Exit(_tables._locks[i]);
		}
	}

	private ReadOnlyCollection<TKey> GetKeys()
	{
		int locksAcquired = 0;
		try
		{
			AcquireAllLocks(ref locksAcquired);
			int countInternal = GetCountInternal();
			if (countInternal < 0)
			{
				throw new OutOfMemoryException();
			}
			List<TKey> list = new List<TKey>(countInternal);
			for (int i = 0; i < _tables._buckets.Length; i++)
			{
				for (Node node = _tables._buckets[i]; node != null; node = node._next)
				{
					list.Add(node._key);
				}
			}
			return new ReadOnlyCollection<TKey>(list);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	private ReadOnlyCollection<TValue> GetValues()
	{
		int locksAcquired = 0;
		try
		{
			AcquireAllLocks(ref locksAcquired);
			int countInternal = GetCountInternal();
			if (countInternal < 0)
			{
				throw new OutOfMemoryException();
			}
			List<TValue> list = new List<TValue>(countInternal);
			for (int i = 0; i < _tables._buckets.Length; i++)
			{
				for (Node node = _tables._buckets[i]; node != null; node = node._next)
				{
					list.Add(node._value);
				}
			}
			return new ReadOnlyCollection<TValue>(list);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}
}
