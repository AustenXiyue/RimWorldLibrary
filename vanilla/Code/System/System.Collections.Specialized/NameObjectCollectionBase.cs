using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using Unity;

namespace System.Collections.Specialized;

/// <summary>Provides the abstract base class for a collection of associated <see cref="T:System.String" /> keys and <see cref="T:System.Object" /> values that can be accessed either with the key or with the index.</summary>
[Serializable]
public abstract class NameObjectCollectionBase : ICollection, IEnumerable, ISerializable, IDeserializationCallback
{
	internal class NameObjectEntry
	{
		internal string Key;

		internal object Value;

		internal NameObjectEntry(string name, object value)
		{
			Key = name;
			Value = value;
		}
	}

	[Serializable]
	internal class NameObjectKeysEnumerator : IEnumerator
	{
		private int _pos;

		private NameObjectCollectionBase _coll;

		private int _version;

		public object Current
		{
			get
			{
				if (_pos >= 0 && _pos < _coll.Count)
				{
					return _coll.BaseGetKey(_pos);
				}
				throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
			}
		}

		internal NameObjectKeysEnumerator(NameObjectCollectionBase coll)
		{
			_coll = coll;
			_version = _coll._version;
			_pos = -1;
		}

		public bool MoveNext()
		{
			if (_version != _coll._version)
			{
				throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
			}
			if (_pos < _coll.Count - 1)
			{
				_pos++;
				return true;
			}
			_pos = _coll.Count;
			return false;
		}

		public void Reset()
		{
			if (_version != _coll._version)
			{
				throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
			}
			_pos = -1;
		}
	}

	/// <summary>Represents a collection of the <see cref="T:System.String" /> keys of a collection.</summary>
	[Serializable]
	public class KeysCollection : ICollection, IEnumerable
	{
		private NameObjectCollectionBase _coll;

		/// <summary>Gets the entry at the specified index of the collection.</summary>
		/// <returns>The <see cref="T:System.String" /> key of the entry at the specified index of the collection.</returns>
		/// <param name="index">The zero-based index of the entry to locate in the collection. </param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
		public string this[int index] => Get(index);

		/// <summary>Gets the number of keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />.</summary>
		/// <returns>The number of keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />.</returns>
		public int Count => _coll.Count;

		/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />.</summary>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />.</returns>
		object ICollection.SyncRoot => ((ICollection)_coll).SyncRoot;

		/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> is synchronized (thread safe).</summary>
		/// <returns>true if access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> is synchronized (thread safe); otherwise, false. The default is false.</returns>
		bool ICollection.IsSynchronized => false;

		internal KeysCollection(NameObjectCollectionBase coll)
		{
			_coll = coll;
		}

		/// <summary>Gets the key at the specified index of the collection.</summary>
		/// <returns>A <see cref="T:System.String" /> that contains the key at the specified index of the collection.</returns>
		/// <param name="index">The zero-based index of the key to get from the collection. </param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
		public virtual string Get(int index)
		{
			return _coll.BaseGetKey(index);
		}

		/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />.</returns>
		public IEnumerator GetEnumerator()
		{
			return new NameObjectKeysEnumerator(_coll);
		}

		/// <summary>Copies the entire <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero. </exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
		/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(global::SR.GetString("Multi dimension array is not supported on this operation."));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", global::SR.GetString("Index {0} is out of range.", index.ToString(CultureInfo.CurrentCulture)));
			}
			if (array.Length - index < _coll.Count)
			{
				throw new ArgumentException(global::SR.GetString("Insufficient space in the target location to copy the information."));
			}
			IEnumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				array.SetValue(enumerator.Current, index++);
			}
		}

		internal KeysCollection()
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	private const string ReadOnlyName = "ReadOnly";

	private const string CountName = "Count";

	private const string ComparerName = "Comparer";

	private const string HashCodeProviderName = "HashProvider";

	private const string KeysName = "Keys";

	private const string ValuesName = "Values";

	private const string KeyComparerName = "KeyComparer";

	private const string VersionName = "Version";

	private bool _readOnly;

	private ArrayList _entriesArray;

	private IEqualityComparer _keyComparer;

	private volatile Hashtable _entriesTable;

	private volatile NameObjectEntry _nullKeyEntry;

	private KeysCollection _keys;

	private SerializationInfo _serializationInfo;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private static StringComparer defaultComparer = StringComparer.InvariantCultureIgnoreCase;

	internal IEqualityComparer Comparer
	{
		get
		{
			return _keyComparer;
		}
		set
		{
			_keyComparer = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance is read-only; otherwise, false.</returns>
	protected bool IsReadOnly
	{
		get
		{
			return _readOnly;
		}
		set
		{
			_readOnly = value;
		}
	}

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</returns>
	public virtual int Count => _entriesArray.Count;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> object.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> object.</returns>
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

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> object is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> object is synchronized (thread safe); otherwise, false. The default is false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets a <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> instance that contains all the keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> instance that contains all the keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</returns>
	public virtual KeysCollection Keys
	{
		get
		{
			if (_keys == null)
			{
				_keys = new KeysCollection(this);
			}
			return _keys;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is empty.</summary>
	protected NameObjectCollectionBase()
		: this(defaultComparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object to use to determine whether two keys are equal and to generate hash codes for the keys in the collection.</param>
	protected NameObjectCollectionBase(IEqualityComparer equalityComparer)
	{
		IEqualityComparer keyComparer;
		if (equalityComparer != null)
		{
			keyComparer = equalityComparer;
		}
		else
		{
			IEqualityComparer equalityComparer2 = defaultComparer;
			keyComparer = equalityComparer2;
		}
		_keyComparer = keyComparer;
		Reset();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="capacity">The approximate number of entries that the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> object can initially contain.</param>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object to use to determine whether two keys are equal and to generate hash codes for the keys in the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	protected NameObjectCollectionBase(int capacity, IEqualityComparer equalityComparer)
		: this(equalityComparer)
	{
		Reset(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is empty, has the default initial capacity, and uses the specified hash code provider and the specified comparer.</summary>
	/// <param name="hashProvider">The <see cref="T:System.Collections.IHashCodeProvider" /> that will supply the hash codes for all keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.</param>
	[Obsolete("Please use NameObjectCollectionBase(IEqualityComparer) instead.")]
	protected NameObjectCollectionBase(IHashCodeProvider hashProvider, IComparer comparer)
	{
		_keyComparer = new CompatibleComparer(comparer, hashProvider);
		Reset();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is empty, has the specified initial capacity and uses the specified hash code provider and the specified comparer.</summary>
	/// <param name="capacity">The approximate number of entries that the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance can initially contain.</param>
	/// <param name="hashProvider">The <see cref="T:System.Collections.IHashCodeProvider" /> that will supply the hash codes for all keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	[Obsolete("Please use NameObjectCollectionBase(Int32, IEqualityComparer) instead.")]
	protected NameObjectCollectionBase(int capacity, IHashCodeProvider hashProvider, IComparer comparer)
	{
		_keyComparer = new CompatibleComparer(comparer, hashProvider);
		Reset(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is empty, has the specified initial capacity, and uses the default hash code provider and the default comparer.</summary>
	/// <param name="capacity">The approximate number of entries that the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance can initially contain.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	protected NameObjectCollectionBase(int capacity)
	{
		_keyComparer = StringComparer.InvariantCultureIgnoreCase;
		Reset(capacity);
	}

	internal NameObjectCollectionBase(DBNull dummy)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> class that is serializable and uses the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the new <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the new <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</param>
	protected NameObjectCollectionBase(SerializationInfo info, StreamingContext context)
	{
		_serializationInfo = info;
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("ReadOnly", _readOnly);
		if (_keyComparer == defaultComparer)
		{
			info.AddValue("HashProvider", CompatibleComparer.DefaultHashCodeProvider, typeof(IHashCodeProvider));
			info.AddValue("Comparer", CompatibleComparer.DefaultComparer, typeof(IComparer));
		}
		else if (_keyComparer == null)
		{
			info.AddValue("HashProvider", null, typeof(IHashCodeProvider));
			info.AddValue("Comparer", null, typeof(IComparer));
		}
		else if (_keyComparer is CompatibleComparer)
		{
			CompatibleComparer compatibleComparer = (CompatibleComparer)_keyComparer;
			info.AddValue("HashProvider", compatibleComparer.HashCodeProvider, typeof(IHashCodeProvider));
			info.AddValue("Comparer", compatibleComparer.Comparer, typeof(IComparer));
		}
		else
		{
			info.AddValue("KeyComparer", _keyComparer, typeof(IEqualityComparer));
		}
		int count = _entriesArray.Count;
		info.AddValue("Count", count);
		string[] array = new string[count];
		object[] array2 = new object[count];
		for (int i = 0; i < count; i++)
		{
			NameObjectEntry nameObjectEntry = (NameObjectEntry)_entriesArray[i];
			array[i] = nameObjectEntry.Key;
			array2[i] = nameObjectEntry.Value;
		}
		info.AddValue("Keys", array, typeof(string[]));
		info.AddValue("Values", array2, typeof(object[]));
		info.AddValue("Version", _version);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance is invalid.</exception>
	public virtual void OnDeserialization(object sender)
	{
		if (_keyComparer != null)
		{
			return;
		}
		if (_serializationInfo == null)
		{
			throw new SerializationException();
		}
		SerializationInfo serializationInfo = _serializationInfo;
		_serializationInfo = null;
		bool readOnly = false;
		int num = 0;
		string[] array = null;
		object[] array2 = null;
		IHashCodeProvider hashCodeProvider = null;
		IComparer comparer = null;
		bool flag = false;
		int version = 0;
		SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Name)
			{
			case "ReadOnly":
				readOnly = serializationInfo.GetBoolean("ReadOnly");
				break;
			case "HashProvider":
				hashCodeProvider = (IHashCodeProvider)serializationInfo.GetValue("HashProvider", typeof(IHashCodeProvider));
				break;
			case "Comparer":
				comparer = (IComparer)serializationInfo.GetValue("Comparer", typeof(IComparer));
				break;
			case "KeyComparer":
				_keyComparer = (IEqualityComparer)serializationInfo.GetValue("KeyComparer", typeof(IEqualityComparer));
				break;
			case "Count":
				num = serializationInfo.GetInt32("Count");
				break;
			case "Keys":
				array = (string[])serializationInfo.GetValue("Keys", typeof(string[]));
				break;
			case "Values":
				array2 = (object[])serializationInfo.GetValue("Values", typeof(object[]));
				break;
			case "Version":
				flag = true;
				version = serializationInfo.GetInt32("Version");
				break;
			}
		}
		if (_keyComparer == null)
		{
			if (comparer == null || hashCodeProvider == null)
			{
				throw new SerializationException();
			}
			_keyComparer = new CompatibleComparer(comparer, hashCodeProvider);
		}
		if (array == null || array2 == null)
		{
			throw new SerializationException();
		}
		Reset(num);
		for (int i = 0; i < num; i++)
		{
			BaseAdd(array[i], array2[i]);
		}
		_readOnly = readOnly;
		if (flag)
		{
			_version = version;
		}
	}

	private void Reset()
	{
		_entriesArray = new ArrayList();
		_entriesTable = new Hashtable(_keyComparer);
		_nullKeyEntry = null;
		_version++;
	}

	private void Reset(int capacity)
	{
		_entriesArray = new ArrayList(capacity);
		_entriesTable = new Hashtable(capacity, _keyComparer);
		_nullKeyEntry = null;
		_version++;
	}

	private NameObjectEntry FindEntry(string key)
	{
		if (key != null)
		{
			return (NameObjectEntry)_entriesTable[key];
		}
		return _nullKeyEntry;
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance contains entries whose keys are not null.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance contains entries whose keys are not null; otherwise, false.</returns>
	protected bool BaseHasKeys()
	{
		return _entriesTable.Count > 0;
	}

	/// <summary>Adds an entry with the specified key and value into the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to add. The key can be null.</param>
	/// <param name="value">The <see cref="T:System.Object" /> value of the entry to add. The value can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only. </exception>
	protected void BaseAdd(string name, object value)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		NameObjectEntry nameObjectEntry = new NameObjectEntry(name, value);
		if (name != null)
		{
			if (_entriesTable[name] == null)
			{
				_entriesTable.Add(name, nameObjectEntry);
			}
		}
		else if (_nullKeyEntry == null)
		{
			_nullKeyEntry = nameObjectEntry;
		}
		_entriesArray.Add(nameObjectEntry);
		_version++;
	}

	/// <summary>Removes the entries with the specified key from the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="name">The <see cref="T:System.String" /> key of the entries to remove. The key can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only. </exception>
	protected void BaseRemove(string name)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		if (name != null)
		{
			_entriesTable.Remove(name);
			for (int num = _entriesArray.Count - 1; num >= 0; num--)
			{
				if (_keyComparer.Equals(name, BaseGetKey(num)))
				{
					_entriesArray.RemoveAt(num);
				}
			}
		}
		else
		{
			_nullKeyEntry = null;
			for (int num2 = _entriesArray.Count - 1; num2 >= 0; num2--)
			{
				if (BaseGetKey(num2) == null)
				{
					_entriesArray.RemoveAt(num2);
				}
			}
		}
		_version++;
	}

	/// <summary>Removes the entry at the specified index of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index of the entry to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection.</exception>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	protected void BaseRemoveAt(int index)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		string text = BaseGetKey(index);
		if (text != null)
		{
			_entriesTable.Remove(text);
		}
		else
		{
			_nullKeyEntry = null;
		}
		_entriesArray.RemoveAt(index);
		_version++;
	}

	/// <summary>Removes all entries from the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	protected void BaseClear()
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		Reset();
	}

	/// <summary>Gets the value of the first entry with the specified key from the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the value of the first entry with the specified key, if found; otherwise, null.</returns>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to get. The key can be null.</param>
	protected object BaseGet(string name)
	{
		return FindEntry(name)?.Value;
	}

	/// <summary>Sets the value of the first entry with the specified key in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance, if found; otherwise, adds an entry with the specified key and value into the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to set. The key can be null.</param>
	/// <param name="value">The <see cref="T:System.Object" /> that represents the new value of the entry to set. The value can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only. </exception>
	protected void BaseSet(string name, object value)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		NameObjectEntry nameObjectEntry = FindEntry(name);
		if (nameObjectEntry != null)
		{
			nameObjectEntry.Value = value;
			_version++;
		}
		else
		{
			BaseAdd(name, value);
		}
	}

	/// <summary>Gets the value of the entry at the specified index of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the value of the entry at the specified index.</returns>
	/// <param name="index">The zero-based index of the value to get.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
	protected object BaseGet(int index)
	{
		return ((NameObjectEntry)_entriesArray[index]).Value;
	}

	/// <summary>Gets the key of the entry at the specified index of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the key of the entry at the specified index.</returns>
	/// <param name="index">The zero-based index of the key to get.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
	protected string BaseGetKey(int index)
	{
		return ((NameObjectEntry)_entriesArray[index]).Key;
	}

	/// <summary>Sets the value of the entry at the specified index of the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index of the entry to set.</param>
	/// <param name="value">The <see cref="T:System.Object" /> that represents the new value of the entry to set. The value can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection.</exception>
	protected void BaseSet(int index, object value)
	{
		if (_readOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		((NameObjectEntry)_entriesArray[index]).Value = value;
		_version++;
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		return new NameObjectKeysEnumerator(this);
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(global::SR.GetString("Multi dimension array is not supported on this operation."));
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", global::SR.GetString("Index {0} is out of range.", index.ToString(CultureInfo.CurrentCulture)));
		}
		if (array.Length - index < _entriesArray.Count)
		{
			throw new ArgumentException(global::SR.GetString("Insufficient space in the target location to copy the information."));
		}
		IEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			array.SetValue(enumerator.Current, index++);
		}
	}

	/// <summary>Returns a <see cref="T:System.String" /> array that contains all the keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>A <see cref="T:System.String" /> array that contains all the keys in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</returns>
	protected string[] BaseGetAllKeys()
	{
		int count = _entriesArray.Count;
		string[] array = new string[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BaseGetKey(i);
		}
		return array;
	}

	/// <summary>Returns an <see cref="T:System.Object" /> array that contains all the values in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>An <see cref="T:System.Object" /> array that contains all the values in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</returns>
	protected object[] BaseGetAllValues()
	{
		int count = _entriesArray.Count;
		object[] array = new object[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BaseGet(i);
		}
		return array;
	}

	/// <summary>Returns an array of the specified type that contains all the values in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <returns>An array of the specified type that contains all the values in the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of array to return.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Type" />. </exception>
	protected object[] BaseGetAllValues(Type type)
	{
		int count = _entriesArray.Count;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		object[] array = (object[])SecurityUtils.ArrayCreateInstance(type, count);
		for (int i = 0; i < count; i++)
		{
			array[i] = BaseGet(i);
		}
		return array;
	}
}
