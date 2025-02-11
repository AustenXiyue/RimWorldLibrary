using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections.Specialized;

/// <summary>Represents a collection of key/value pairs that are accessible by the key or index.</summary>
[Serializable]
public class OrderedDictionary : IOrderedDictionary, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
{
	private class OrderedDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private int _objectReturnType;

		internal const int Keys = 1;

		internal const int Values = 2;

		internal const int DictionaryEntry = 3;

		private IEnumerator arrayEnumerator;

		public object Current
		{
			get
			{
				if (_objectReturnType == 1)
				{
					return ((DictionaryEntry)arrayEnumerator.Current).Key;
				}
				if (_objectReturnType == 2)
				{
					return ((DictionaryEntry)arrayEnumerator.Current).Value;
				}
				return Entry;
			}
		}

		public DictionaryEntry Entry => new DictionaryEntry(((DictionaryEntry)arrayEnumerator.Current).Key, ((DictionaryEntry)arrayEnumerator.Current).Value);

		public object Key => ((DictionaryEntry)arrayEnumerator.Current).Key;

		public object Value => ((DictionaryEntry)arrayEnumerator.Current).Value;

		internal OrderedDictionaryEnumerator(ArrayList array, int objectReturnType)
		{
			arrayEnumerator = array.GetEnumerator();
			_objectReturnType = objectReturnType;
		}

		public bool MoveNext()
		{
			return arrayEnumerator.MoveNext();
		}

		public void Reset()
		{
			arrayEnumerator.Reset();
		}
	}

	private class OrderedDictionaryKeyValueCollection : ICollection, IEnumerable
	{
		private ArrayList _objects;

		private bool isKeys;

		int ICollection.Count => _objects.Count;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => _objects.SyncRoot;

		public OrderedDictionaryKeyValueCollection(ArrayList array, bool isKeys)
		{
			_objects = array;
			this.isKeys = isKeys;
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			foreach (object @object in _objects)
			{
				array.SetValue(isKeys ? ((DictionaryEntry)@object).Key : ((DictionaryEntry)@object).Value, index);
				index++;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new OrderedDictionaryEnumerator(_objects, isKeys ? 1 : 2);
		}
	}

	private ArrayList _objectsArray;

	private Hashtable _objectsTable;

	private int _initialCapacity;

	private IEqualityComparer _comparer;

	private bool _readOnly;

	private object _syncRoot;

	private SerializationInfo _siInfo;

	private const string KeyComparerName = "KeyComparer";

	private const string ArrayListName = "ArrayList";

	private const string ReadOnlyName = "ReadOnly";

	private const string InitCapacityName = "InitialCapacity";

	/// <summary>Gets the number of key/values pairs contained in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</returns>
	public int Count => objectsArray.Count;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> has a fixed size; otherwise, false. The default is false.</returns>
	bool IDictionary.IsFixedSize => _readOnly;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only; otherwise, false. The default is false.</returns>
	public bool IsReadOnly => _readOnly;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> object is synchronized (thread-safe).</summary>
	/// <returns>This method always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> object containing the keys in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the keys in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</returns>
	public ICollection Keys => new OrderedDictionaryKeyValueCollection(objectsArray, isKeys: true);

	private ArrayList objectsArray
	{
		get
		{
			if (_objectsArray == null)
			{
				_objectsArray = new ArrayList(_initialCapacity);
			}
			return _objectsArray;
		}
	}

	private Hashtable objectsTable
	{
		get
		{
			if (_objectsTable == null)
			{
				_objectsTable = new Hashtable(_initialCapacity, _comparer);
			}
			return _objectsTable;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> object.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> object.</returns>
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

	/// <summary>Gets or sets the value at the specified index.</summary>
	/// <returns>The value of the item at the specified index. </returns>
	/// <param name="index">The zero-based index of the value to get or set.</param>
	/// <exception cref="T:System.NotSupportedException">The property is being set and the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Specialized.OrderedDictionary.Count" />.</exception>
	public object this[int index]
	{
		get
		{
			return ((DictionaryEntry)objectsArray[index]).Value;
		}
		set
		{
			if (_readOnly)
			{
				throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
			}
			if (index < 0 || index >= objectsArray.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			object key = ((DictionaryEntry)objectsArray[index]).Key;
			objectsArray[index] = new DictionaryEntry(key, value);
			objectsTable[key] = value;
		}
	}

	/// <summary>Gets or sets the value with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new element using the specified key.</returns>
	/// <param name="key">The key of the value to get or set.</param>
	/// <exception cref="T:System.NotSupportedException">The property is being set and the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</exception>
	public object this[object key]
	{
		get
		{
			return objectsTable[key];
		}
		set
		{
			if (_readOnly)
			{
				throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
			}
			if (objectsTable.Contains(key))
			{
				objectsTable[key] = value;
				objectsArray[IndexOfKey(key)] = new DictionaryEntry(key, value);
			}
			else
			{
				Add(key, value);
			}
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> object containing the values in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the values in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</returns>
	public ICollection Values => new OrderedDictionaryKeyValueCollection(objectsArray, isKeys: false);

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> class.</summary>
	public OrderedDictionary()
		: this(0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> class using the specified initial capacity.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection can contain.</param>
	public OrderedDictionary(int capacity)
		: this(capacity, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> class using the specified comparer.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />.</param>
	public OrderedDictionary(IEqualityComparer comparer)
		: this(0, comparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> class using the specified initial capacity and comparer.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection can contain.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />.</param>
	public OrderedDictionary(int capacity, IEqualityComparer comparer)
	{
		_initialCapacity = capacity;
		_comparer = comparer;
	}

	private OrderedDictionary(OrderedDictionary dictionary)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		_readOnly = true;
		_objectsArray = dictionary._objectsArray;
		_objectsTable = dictionary._objectsTable;
		_comparer = dictionary._comparer;
		_initialCapacity = dictionary._initialCapacity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> class that is serializable using the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Specialized.OrderedDictionary" />.</param>
	protected OrderedDictionary(SerializationInfo info, StreamingContext context)
	{
		_siInfo = info;
	}

	/// <summary>Adds an entry with the specified key and value into the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection with the lowest available index.</summary>
	/// <param name="key">The key of the entry to add.</param>
	/// <param name="value">The value of the entry to add. This value can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</exception>
	public void Add(object key, object value)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
		}
		objectsTable.Add(key, value);
		objectsArray.Add(new DictionaryEntry(key, value));
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</exception>
	public void Clear()
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
		}
		objectsTable.Clear();
		objectsArray.Clear();
	}

	/// <summary>Returns a read-only copy of the current <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <returns>A read-only copy of the current <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</returns>
	public OrderedDictionary AsReadOnly()
	{
		return new OrderedDictionary(this);
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</param>
	public bool Contains(object key)
	{
		return objectsTable.Contains(key);
	}

	/// <summary>Copies the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> elements to a one-dimensional <see cref="T:System.Array" /> object at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> object that is the destination of the <see cref="T:System.Collections.DictionaryEntry" /> objects copied from <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	public void CopyTo(Array array, int index)
	{
		objectsTable.CopyTo(array, index);
	}

	private int IndexOfKey(object key)
	{
		for (int i = 0; i < objectsArray.Count; i++)
		{
			object key2 = ((DictionaryEntry)objectsArray[i]).Key;
			if (_comparer != null)
			{
				if (_comparer.Equals(key2, key))
				{
					return i;
				}
			}
			else if (key2.Equals(key))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Inserts a new entry into the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection with the specified key and value at the specified index.</summary>
	/// <param name="index">The zero-based index at which the element should be inserted.</param>
	/// <param name="key">The key of the entry to add.</param>
	/// <param name="value">The value of the entry to add. The value can be null.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is out of range.</exception>
	/// <exception cref="T:System.NotSupportedException">This collection is read-only.</exception>
	public void Insert(int index, object key, object value)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
		}
		if (index > Count || index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		objectsTable.Add(key, value);
		objectsArray.Insert(index, new DictionaryEntry(key, value));
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and is called back by the deserialization event when deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is invalid.</exception>
	protected virtual void OnDeserialization(object sender)
	{
		if (_siInfo == null)
		{
			throw new SerializationException(global::SR.GetString("OnDeserialization method was called while the object was not being deserialized."));
		}
		_comparer = (IEqualityComparer)_siInfo.GetValue("KeyComparer", typeof(IEqualityComparer));
		_readOnly = _siInfo.GetBoolean("ReadOnly");
		_initialCapacity = _siInfo.GetInt32("InitialCapacity");
		object[] array = (object[])_siInfo.GetValue("ArrayList", typeof(object[]));
		if (array == null)
		{
			return;
		}
		object[] array2 = array;
		foreach (object obj in array2)
		{
			DictionaryEntry dictionaryEntry;
			try
			{
				dictionaryEntry = (DictionaryEntry)obj;
			}
			catch
			{
				throw new SerializationException(global::SR.GetString("There was an error deserializing the OrderedDictionary.  The ArrayList does not contain DictionaryEntries."));
			}
			objectsArray.Add(dictionaryEntry);
			objectsTable.Add(dictionaryEntry.Key, dictionaryEntry.Value);
		}
	}

	/// <summary>Removes the entry at the specified index from the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <param name="index">The zero-based index of the entry to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.- or -<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Specialized.OrderedDictionary.Count" />.</exception>
	public void RemoveAt(int index)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
		}
		if (index >= Count || index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		object key = ((DictionaryEntry)objectsArray[index]).Key;
		objectsArray.RemoveAt(index);
		objectsTable.Remove(key);
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <param name="key">The key of the entry to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public void Remove(object key)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("The OrderedDictionary is readonly and cannot be modified."));
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = IndexOfKey(key);
		if (num >= 0)
		{
			objectsTable.Remove(key);
			objectsArray.RemoveAt(num);
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object that iterates through the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</returns>
	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return new OrderedDictionaryEnumerator(objectsArray, 3);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object that iterates through the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new OrderedDictionaryEnumerator(objectsArray, 3);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Specialized.OrderedDictionary" /> collection.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Specialized.OrderedDictionary" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("KeyComparer", _comparer, typeof(IEqualityComparer));
		info.AddValue("ReadOnly", _readOnly);
		info.AddValue("InitialCapacity", _initialCapacity);
		object[] array = new object[Count];
		_objectsArray.CopyTo(array);
		info.AddValue("ArrayList", array);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and is called back by the deserialization event when deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event.</param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		OnDeserialization(sender);
	}
}
