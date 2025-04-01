using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections;

/// <summary>Represents a collection of key/value pairs that are organized based on the hash code of the key.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[ComVisible(true)]
[DebuggerTypeProxy(typeof(HashtableDebugView))]
public class Hashtable : IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback, ICloneable
{
	private struct bucket
	{
		public object key;

		public object val;

		public int hash_coll;
	}

	[Serializable]
	private class KeyCollection : ICollection, IEnumerable
	{
		private Hashtable _hashtable;

		public virtual bool IsSynchronized => _hashtable.IsSynchronized;

		public virtual object SyncRoot => _hashtable.SyncRoot;

		public virtual int Count => _hashtable.count;

		internal KeyCollection(Hashtable hashtable)
		{
			_hashtable = hashtable;
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("Non-negative number required."));
			}
			if (array.Length - arrayIndex < _hashtable.count)
			{
				throw new ArgumentException(Environment.GetResourceString("Destination array is not long enough to copy all the items in the collection. Check array index and length."));
			}
			_hashtable.CopyKeys(array, arrayIndex);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new HashtableEnumerator(_hashtable, 1);
		}
	}

	[Serializable]
	private class ValueCollection : ICollection, IEnumerable
	{
		private Hashtable _hashtable;

		public virtual bool IsSynchronized => _hashtable.IsSynchronized;

		public virtual object SyncRoot => _hashtable.SyncRoot;

		public virtual int Count => _hashtable.count;

		internal ValueCollection(Hashtable hashtable)
		{
			_hashtable = hashtable;
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("Non-negative number required."));
			}
			if (array.Length - arrayIndex < _hashtable.count)
			{
				throw new ArgumentException(Environment.GetResourceString("Destination array is not long enough to copy all the items in the collection. Check array index and length."));
			}
			_hashtable.CopyValues(array, arrayIndex);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new HashtableEnumerator(_hashtable, 2);
		}
	}

	[Serializable]
	private class SyncHashtable : Hashtable, IEnumerable
	{
		protected Hashtable _table;

		public override int Count => _table.Count;

		public override bool IsReadOnly => _table.IsReadOnly;

		public override bool IsFixedSize => _table.IsFixedSize;

		public override bool IsSynchronized => true;

		public override object this[object key]
		{
			get
			{
				return _table[key];
			}
			set
			{
				lock (_table.SyncRoot)
				{
					_table[key] = value;
				}
			}
		}

		public override object SyncRoot => _table.SyncRoot;

		public override ICollection Keys
		{
			get
			{
				lock (_table.SyncRoot)
				{
					return _table.Keys;
				}
			}
		}

		public override ICollection Values
		{
			get
			{
				lock (_table.SyncRoot)
				{
					return _table.Values;
				}
			}
		}

		internal SyncHashtable(Hashtable table)
			: base(trash: false)
		{
			_table = table;
		}

		internal SyncHashtable(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_table = (Hashtable)info.GetValue("ParentTable", typeof(Hashtable));
			if (_table == null)
			{
				throw new SerializationException(Environment.GetResourceString("Insufficient state to return the real object."));
			}
		}

		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			lock (_table.SyncRoot)
			{
				info.AddValue("ParentTable", _table, typeof(Hashtable));
			}
		}

		public override void Add(object key, object value)
		{
			lock (_table.SyncRoot)
			{
				_table.Add(key, value);
			}
		}

		public override void Clear()
		{
			lock (_table.SyncRoot)
			{
				_table.Clear();
			}
		}

		public override bool Contains(object key)
		{
			return _table.Contains(key);
		}

		public override bool ContainsKey(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
			}
			return _table.ContainsKey(key);
		}

		public override bool ContainsValue(object key)
		{
			lock (_table.SyncRoot)
			{
				return _table.ContainsValue(key);
			}
		}

		public override void CopyTo(Array array, int arrayIndex)
		{
			lock (_table.SyncRoot)
			{
				_table.CopyTo(array, arrayIndex);
			}
		}

		public override object Clone()
		{
			lock (_table.SyncRoot)
			{
				return Synchronized((Hashtable)_table.Clone());
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _table.GetEnumerator();
		}

		public override IDictionaryEnumerator GetEnumerator()
		{
			return _table.GetEnumerator();
		}

		public override void Remove(object key)
		{
			lock (_table.SyncRoot)
			{
				_table.Remove(key);
			}
		}

		public override void OnDeserialization(object sender)
		{
		}

		internal override KeyValuePairs[] ToKeyValuePairsArray()
		{
			return _table.ToKeyValuePairsArray();
		}
	}

	[Serializable]
	private class HashtableEnumerator : IDictionaryEnumerator, IEnumerator, ICloneable
	{
		private Hashtable hashtable;

		private int bucket;

		private int version;

		private bool current;

		private int getObjectRetType;

		private object currentKey;

		private object currentValue;

		internal const int Keys = 1;

		internal const int Values = 2;

		internal const int DictEntry = 3;

		public virtual object Key
		{
			get
			{
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
				}
				return currentKey;
			}
		}

		public virtual DictionaryEntry Entry
		{
			get
			{
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
				}
				return new DictionaryEntry(currentKey, currentValue);
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
					return currentKey;
				}
				if (getObjectRetType == 2)
				{
					return currentValue;
				}
				return new DictionaryEntry(currentKey, currentValue);
			}
		}

		public virtual object Value
		{
			get
			{
				if (!current)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
				}
				return currentValue;
			}
		}

		internal HashtableEnumerator(Hashtable hashtable, int getObjRetType)
		{
			this.hashtable = hashtable;
			bucket = hashtable.buckets.Length;
			version = hashtable.version;
			current = false;
			getObjectRetType = getObjRetType;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (version != hashtable.version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			while (bucket > 0)
			{
				bucket--;
				object key = hashtable.buckets[bucket].key;
				if (key != null && key != hashtable.buckets)
				{
					currentKey = key;
					currentValue = hashtable.buckets[bucket].val;
					current = true;
					return true;
				}
			}
			current = false;
			return false;
		}

		public virtual void Reset()
		{
			if (version != hashtable.version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			current = false;
			bucket = hashtable.buckets.Length;
			currentKey = null;
			currentValue = null;
		}
	}

	internal class HashtableDebugView
	{
		private Hashtable hashtable;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePairs[] Items => hashtable.ToKeyValuePairsArray();

		public HashtableDebugView(Hashtable hashtable)
		{
			if (hashtable == null)
			{
				throw new ArgumentNullException("hashtable");
			}
			this.hashtable = hashtable;
		}
	}

	internal const int HashPrime = 101;

	private const int InitialSize = 3;

	private const string LoadFactorName = "LoadFactor";

	private const string VersionName = "Version";

	private const string ComparerName = "Comparer";

	private const string HashCodeProviderName = "HashCodeProvider";

	private const string HashSizeName = "HashSize";

	private const string KeysName = "Keys";

	private const string ValuesName = "Values";

	private const string KeyComparerName = "KeyComparer";

	private bucket[] buckets;

	private int count;

	private int occupancy;

	private int loadsize;

	private float loadFactor;

	private volatile int version;

	private volatile bool isWriterInProgress;

	private ICollection keys;

	private ICollection values;

	private IEqualityComparer _keycomparer;

	private object _syncRoot;

	/// <summary>Gets or sets the object that can dispense hash codes.</summary>
	/// <returns>The object that can dispense hash codes.</returns>
	/// <exception cref="T:System.ArgumentException">The property is set to a value, but the hash table was created using an <see cref="T:System.Collections.IEqualityComparer" />. </exception>
	[Obsolete("Please use EqualityComparer property.")]
	protected IHashCodeProvider hcp
	{
		get
		{
			if (_keycomparer is CompatibleComparer)
			{
				return ((CompatibleComparer)_keycomparer).HashCodeProvider;
			}
			if (_keycomparer == null)
			{
				return null;
			}
			throw new ArgumentException(Environment.GetResourceString("The usage of IKeyComparer and IHashCodeProvider/IComparer interfaces cannot be mixed; use one or the other."));
		}
		set
		{
			if (_keycomparer is CompatibleComparer)
			{
				CompatibleComparer compatibleComparer = (CompatibleComparer)_keycomparer;
				_keycomparer = new CompatibleComparer(compatibleComparer.Comparer, value);
				return;
			}
			if (_keycomparer == null)
			{
				_keycomparer = new CompatibleComparer(null, value);
				return;
			}
			throw new ArgumentException(Environment.GetResourceString("The usage of IKeyComparer and IHashCodeProvider/IComparer interfaces cannot be mixed; use one or the other."));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Collections.IComparer" /> to use for the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>The <see cref="T:System.Collections.IComparer" /> to use for the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <exception cref="T:System.ArgumentException">The property is set to a value, but the hash table was created using an <see cref="T:System.Collections.IEqualityComparer" />. </exception>
	[Obsolete("Please use KeyComparer properties.")]
	protected IComparer comparer
	{
		get
		{
			if (_keycomparer is CompatibleComparer)
			{
				return ((CompatibleComparer)_keycomparer).Comparer;
			}
			if (_keycomparer == null)
			{
				return null;
			}
			throw new ArgumentException(Environment.GetResourceString("The usage of IKeyComparer and IHashCodeProvider/IComparer interfaces cannot be mixed; use one or the other."));
		}
		set
		{
			if (_keycomparer is CompatibleComparer)
			{
				CompatibleComparer compatibleComparer = (CompatibleComparer)_keycomparer;
				_keycomparer = new CompatibleComparer(value, compatibleComparer.HashCodeProvider);
				return;
			}
			if (_keycomparer == null)
			{
				_keycomparer = new CompatibleComparer(value, null);
				return;
			}
			throw new ArgumentException(Environment.GetResourceString("The usage of IKeyComparer and IHashCodeProvider/IComparer interfaces cannot be mixed; use one or the other."));
		}
	}

	/// <summary>Gets the <see cref="T:System.Collections.IEqualityComparer" /> to use for the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>The <see cref="T:System.Collections.IEqualityComparer" /> to use for the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <exception cref="T:System.ArgumentException">The property is set to a value, but the hash table was created using an <see cref="T:System.Collections.IHashCodeProvider" /> and an <see cref="T:System.Collections.IComparer" />. </exception>
	protected IEqualityComparer EqualityComparer => _keycomparer;

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new element using the specified key.</returns>
	/// <param name="key">The key whose value to get or set. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Hashtable" /> is read-only.-or- The property is set, <paramref name="key" /> does not exist in the collection, and the <see cref="T:System.Collections.Hashtable" /> has a fixed size. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual object this[object key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
			}
			bucket[] array = buckets;
			uint seed;
			uint incr;
			uint num = InitHash(key, array.Length, out seed, out incr);
			int num2 = 0;
			int num3 = (int)(seed % (uint)array.Length);
			bucket bucket;
			do
			{
				int num4 = 0;
				int num5;
				do
				{
					num5 = version;
					bucket = array[num3];
					if (++num4 % 8 == 0)
					{
						Thread.Sleep(1);
					}
				}
				while (isWriterInProgress || num5 != version);
				if (bucket.key == null)
				{
					return null;
				}
				if ((bucket.hash_coll & 0x7FFFFFFF) == num && KeyEquals(bucket.key, key))
				{
					return bucket.val;
				}
				num3 = (int)((num3 + incr) % (uint)array.Length);
			}
			while (bucket.hash_coll < 0 && ++num2 < array.Length);
			return null;
		}
		set
		{
			Insert(key, value, add: false);
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Hashtable" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Hashtable" /> is read-only; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Hashtable" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Hashtable" /> has a fixed size; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsFixedSize => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Hashtable" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.Hashtable" /> is synchronized (thread safe); otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsSynchronized => false;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys in the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys in the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual ICollection Keys
	{
		get
		{
			if (keys == null)
			{
				keys = new KeyCollection(this);
			}
			return keys;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual ICollection Values
	{
		get
		{
			if (values == null)
			{
				values = new ValueCollection(this);
			}
			return values;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Hashtable" />.</returns>
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

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual int Count => count;

	internal Hashtable(bool trash)
	{
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the default initial capacity, load factor, hash code provider, and comparer.</summary>
	public Hashtable()
		: this(0, 1f)
	{
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the specified initial capacity, and the default load factor, hash code provider, and comparer.</summary>
	/// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable" /> object can initially contain. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	public Hashtable(int capacity)
		: this(capacity, 1f)
	{
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the specified initial capacity and load factor, and the default hash code provider and comparer.</summary>
	/// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable" /> object can initially contain. </param>
	/// <param name="loadFactor">A number in the range from 0.1 through 1.0 that is multiplied by the default value which provides the best performance. The result is the maximum ratio of elements to buckets. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or- <paramref name="loadFactor" /> is less than 0.1.-or- <paramref name="loadFactor" /> is greater than 1.0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="capacity" /> is causing an overflow.</exception>
	public Hashtable(int capacity, float loadFactor)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("Non-negative number required."));
		}
		if (!(loadFactor >= 0.1f) || !(loadFactor <= 1f))
		{
			throw new ArgumentOutOfRangeException("loadFactor", Environment.GetResourceString("Load factor needs to be between 0.1 and 1.0.", 0.1, 1.0));
		}
		this.loadFactor = 0.72f * loadFactor;
		double num = (float)capacity / this.loadFactor;
		if (num > 2147483647.0)
		{
			throw new ArgumentException(Environment.GetResourceString("Hashtable's capacity overflowed and went negative. Check load factor, capacity and the current size of the table."));
		}
		int num2 = ((num > 3.0) ? HashHelpers.GetPrime((int)num) : 3);
		buckets = new bucket[num2];
		loadsize = (int)(this.loadFactor * (float)num2);
		isWriterInProgress = false;
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the specified initial capacity, load factor, hash code provider, and comparer.</summary>
	/// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable" /> object can initially contain. </param>
	/// <param name="loadFactor">A number in the range from 0.1 through 1.0 that is multiplied by the default value which provides the best performance. The result is the maximum ratio of elements to buckets.</param>
	/// <param name="hcp">The <see cref="T:System.Collections.IHashCodeProvider" /> object that supplies the hash codes for all keys in the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider, which is each key's implementation of <see cref="M:System.Object.GetHashCode" />. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> object to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or- <paramref name="loadFactor" /> is less than 0.1.-or- <paramref name="loadFactor" /> is greater than 1.0. </exception>
	[Obsolete("Please use Hashtable(int, float, IEqualityComparer) instead.")]
	public Hashtable(int capacity, float loadFactor, IHashCodeProvider hcp, IComparer comparer)
		: this(capacity, loadFactor)
	{
		if (hcp == null && comparer == null)
		{
			_keycomparer = null;
		}
		else
		{
			_keycomparer = new CompatibleComparer(comparer, hcp);
		}
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the specified initial capacity, load factor, and <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable" /> object can initially contain. </param>
	/// <param name="loadFactor">A number in the range from 0.1 through 1.0 that is multiplied by the default value which provides the best performance. The result is the maximum ratio of elements to buckets.</param>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object that defines the hash code provider and the comparer to use with the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider and the default comparer. The default hash code provider is each key's implementation of <see cref="M:System.Object.GetHashCode" /> and the default comparer is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or- <paramref name="loadFactor" /> is less than 0.1.-or- <paramref name="loadFactor" /> is greater than 1.0. </exception>
	public Hashtable(int capacity, float loadFactor, IEqualityComparer equalityComparer)
		: this(capacity, loadFactor)
	{
		_keycomparer = equalityComparer;
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the default initial capacity and load factor, and the specified hash code provider and comparer.</summary>
	/// <param name="hcp">The <see cref="T:System.Collections.IHashCodeProvider" /> object that supplies the hash codes for all keys in the <see cref="T:System.Collections.Hashtable" /> object.-or- null to use the default hash code provider, which is each key's implementation of <see cref="M:System.Object.GetHashCode" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> object to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />.</param>
	[Obsolete("Please use Hashtable(IEqualityComparer) instead.")]
	public Hashtable(IHashCodeProvider hcp, IComparer comparer)
		: this(0, 1f, hcp, comparer)
	{
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the default initial capacity and load factor, and the specified <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object that defines the hash code provider and the comparer to use with the <see cref="T:System.Collections.Hashtable" /> object.-or- null to use the default hash code provider and the default comparer. The default hash code provider is each key's implementation of <see cref="M:System.Object.GetHashCode" /> and the default comparer is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	public Hashtable(IEqualityComparer equalityComparer)
		: this(0, 1f, equalityComparer)
	{
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the specified initial capacity, hash code provider, comparer, and the default load factor.</summary>
	/// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable" /> object can initially contain. </param>
	/// <param name="hcp">The <see cref="T:System.Collections.IHashCodeProvider" /> object that supplies the hash codes for all keys in the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider, which is each key's implementation of <see cref="M:System.Object.GetHashCode" />. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> object to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	[Obsolete("Please use Hashtable(int, IEqualityComparer) instead.")]
	public Hashtable(int capacity, IHashCodeProvider hcp, IComparer comparer)
		: this(capacity, 1f, hcp, comparer)
	{
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class using the specified initial capacity and <see cref="T:System.Collections.IEqualityComparer" />, and the default load factor.</summary>
	/// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable" /> object can initially contain. </param>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object that defines the hash code provider and the comparer to use with the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider and the default comparer. The default hash code provider is each key's implementation of <see cref="M:System.Object.GetHashCode" /> and the default comparer is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	public Hashtable(int capacity, IEqualityComparer equalityComparer)
		: this(capacity, 1f, equalityComparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Hashtable" /> class by copying the elements from the specified dictionary to the new <see cref="T:System.Collections.Hashtable" /> object. The new <see cref="T:System.Collections.Hashtable" /> object has an initial capacity equal to the number of elements copied, and uses the default load factor, hash code provider, and comparer.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> object to copy to a new <see cref="T:System.Collections.Hashtable" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	public Hashtable(IDictionary d)
		: this(d, 1f)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Hashtable" /> class by copying the elements from the specified dictionary to the new <see cref="T:System.Collections.Hashtable" /> object. The new <see cref="T:System.Collections.Hashtable" /> object has an initial capacity equal to the number of elements copied, and uses the specified load factor, and the default hash code provider and comparer.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> object to copy to a new <see cref="T:System.Collections.Hashtable" /> object.</param>
	/// <param name="loadFactor">A number in the range from 0.1 through 1.0 that is multiplied by the default value which provides the best performance. The result is the maximum ratio of elements to buckets.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="loadFactor" /> is less than 0.1.-or- <paramref name="loadFactor" /> is greater than 1.0. </exception>
	public Hashtable(IDictionary d, float loadFactor)
		: this(d, loadFactor, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Hashtable" /> class by copying the elements from the specified dictionary to the new <see cref="T:System.Collections.Hashtable" /> object. The new <see cref="T:System.Collections.Hashtable" /> object has an initial capacity equal to the number of elements copied, and uses the default load factor, and the specified hash code provider and comparer. This API is obsolete. For an alternative, see <see cref="M:System.Collections.Hashtable.#ctor(System.Collections.IDictionary,System.Collections.IEqualityComparer)" />.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> object to copy to a new <see cref="T:System.Collections.Hashtable" /> object.</param>
	/// <param name="hcp">The <see cref="T:System.Collections.IHashCodeProvider" /> object that supplies the hash codes for all keys in the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider, which is each key's implementation of <see cref="M:System.Object.GetHashCode" />. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> object to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	[Obsolete("Please use Hashtable(IDictionary, IEqualityComparer) instead.")]
	public Hashtable(IDictionary d, IHashCodeProvider hcp, IComparer comparer)
		: this(d, 1f, hcp, comparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Hashtable" /> class by copying the elements from the specified dictionary to a new <see cref="T:System.Collections.Hashtable" /> object. The new <see cref="T:System.Collections.Hashtable" /> object has an initial capacity equal to the number of elements copied, and uses the default load factor and the specified <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> object to copy to a new <see cref="T:System.Collections.Hashtable" /> object.</param>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object that defines the hash code provider and the comparer to use with the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider and the default comparer. The default hash code provider is each key's implementation of <see cref="M:System.Object.GetHashCode" /> and the default comparer is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	public Hashtable(IDictionary d, IEqualityComparer equalityComparer)
		: this(d, 1f, equalityComparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Hashtable" /> class by copying the elements from the specified dictionary to the new <see cref="T:System.Collections.Hashtable" /> object. The new <see cref="T:System.Collections.Hashtable" /> object has an initial capacity equal to the number of elements copied, and uses the specified load factor, hash code provider, and comparer.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> object to copy to a new <see cref="T:System.Collections.Hashtable" /> object.</param>
	/// <param name="loadFactor">A number in the range from 0.1 through 1.0 that is multiplied by the default value which provides the best performance. The result is the maximum ratio of elements to buckets.</param>
	/// <param name="hcp">The <see cref="T:System.Collections.IHashCodeProvider" /> object that supplies the hash codes for all keys in the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider, which is each key's implementation of <see cref="M:System.Object.GetHashCode" />. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> object to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="loadFactor" /> is less than 0.1.-or- <paramref name="loadFactor" /> is greater than 1.0. </exception>
	[Obsolete("Please use Hashtable(IDictionary, float, IEqualityComparer) instead.")]
	public Hashtable(IDictionary d, float loadFactor, IHashCodeProvider hcp, IComparer comparer)
		: this(d?.Count ?? 0, loadFactor, hcp, comparer)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d", Environment.GetResourceString("Dictionary cannot be null."));
		}
		IDictionaryEnumerator enumerator = d.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Add(enumerator.Key, enumerator.Value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Hashtable" /> class by copying the elements from the specified dictionary to the new <see cref="T:System.Collections.Hashtable" /> object. The new <see cref="T:System.Collections.Hashtable" /> object has an initial capacity equal to the number of elements copied, and uses the specified load factor and <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="d">The <see cref="T:System.Collections.IDictionary" /> object to copy to a new <see cref="T:System.Collections.Hashtable" /> object.</param>
	/// <param name="loadFactor">A number in the range from 0.1 through 1.0 that is multiplied by the default value which provides the best performance. The result is the maximum ratio of elements to buckets.</param>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object that defines the hash code provider and the comparer to use with the <see cref="T:System.Collections.Hashtable" />.-or- null to use the default hash code provider and the default comparer. The default hash code provider is each key's implementation of <see cref="M:System.Object.GetHashCode" /> and the default comparer is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="loadFactor" /> is less than 0.1.-or- <paramref name="loadFactor" /> is greater than 1.0. </exception>
	public Hashtable(IDictionary d, float loadFactor, IEqualityComparer equalityComparer)
		: this(d?.Count ?? 0, loadFactor, equalityComparer)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d", Environment.GetResourceString("Dictionary cannot be null."));
		}
		IDictionaryEnumerator enumerator = d.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Add(enumerator.Key, enumerator.Value);
		}
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable" /> class that is serializable using the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Hashtable" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Hashtable" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	protected Hashtable(SerializationInfo info, StreamingContext context)
	{
		HashHelpers.SerializationInfoTable.Add(this, info);
	}

	private uint InitHash(object key, int hashsize, out uint seed, out uint incr)
	{
		uint result = (seed = (uint)(GetHash(key) & 0x7FFFFFFF));
		incr = 1 + seed * 101 % (uint)(hashsize - 1);
		return result;
	}

	/// <summary>Adds an element with the specified key and value into the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <param name="key">The key of the element to add. </param>
	/// <param name="value">The value of the element to add. The value can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Hashtable" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Hashtable" /> is read-only.-or- The <see cref="T:System.Collections.Hashtable" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Add(object key, object value)
	{
		Insert(key, value, add: true);
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Hashtable" /> is read-only. </exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public virtual void Clear()
	{
		if (count != 0 || occupancy != 0)
		{
			Thread.BeginCriticalRegion();
			isWriterInProgress = true;
			for (int i = 0; i < buckets.Length; i++)
			{
				buckets[i].hash_coll = 0;
				buckets[i].key = null;
				buckets[i].val = null;
			}
			count = 0;
			occupancy = 0;
			UpdateVersion();
			isWriterInProgress = false;
			Thread.EndCriticalRegion();
		}
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual object Clone()
	{
		bucket[] array = buckets;
		Hashtable hashtable = new Hashtable(count, _keycomparer);
		hashtable.version = version;
		hashtable.loadFactor = loadFactor;
		hashtable.count = 0;
		int num = array.Length;
		while (num > 0)
		{
			num--;
			object key = array[num].key;
			if (key != null && key != array)
			{
				hashtable[key] = array[num].val;
			}
		}
		return hashtable;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Hashtable" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Hashtable" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Hashtable" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual bool Contains(object key)
	{
		return ContainsKey(key);
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Hashtable" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Hashtable" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Hashtable" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual bool ContainsKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
		}
		bucket[] array = buckets;
		uint seed;
		uint incr;
		uint num = InitHash(key, array.Length, out seed, out incr);
		int num2 = 0;
		int num3 = (int)(seed % (uint)array.Length);
		bucket bucket;
		do
		{
			bucket = array[num3];
			if (bucket.key == null)
			{
				return false;
			}
			if ((bucket.hash_coll & 0x7FFFFFFF) == num && KeyEquals(bucket.key, key))
			{
				return true;
			}
			num3 = (int)((num3 + incr) % (uint)array.Length);
		}
		while (bucket.hash_coll < 0 && ++num2 < array.Length);
		return false;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Hashtable" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Hashtable" /> contains an element with the specified <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.Hashtable" />. The value can be null. </param>
	/// <filterpriority>1</filterpriority>
	public virtual bool ContainsValue(object value)
	{
		if (value == null)
		{
			int num = buckets.Length;
			while (--num >= 0)
			{
				if (buckets[num].key != null && buckets[num].key != buckets && buckets[num].val == null)
				{
					return true;
				}
			}
		}
		else
		{
			int num2 = buckets.Length;
			while (--num2 >= 0)
			{
				object val = buckets[num2].val;
				if (val != null && val.Equals(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void CopyKeys(Array array, int arrayIndex)
	{
		bucket[] array2 = buckets;
		int num = array2.Length;
		while (--num >= 0)
		{
			object key = array2[num].key;
			if (key != null && key != buckets)
			{
				array.SetValue(key, arrayIndex++);
			}
		}
	}

	private void CopyEntries(Array array, int arrayIndex)
	{
		bucket[] array2 = buckets;
		int num = array2.Length;
		while (--num >= 0)
		{
			object key = array2[num].key;
			if (key != null && key != buckets)
			{
				DictionaryEntry dictionaryEntry = new DictionaryEntry(key, array2[num].val);
				array.SetValue(dictionaryEntry, arrayIndex++);
			}
		}
	}

	/// <summary>Copies the <see cref="T:System.Collections.Hashtable" /> elements to a one-dimensional <see cref="T:System.Array" /> instance at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the <see cref="T:System.Collections.DictionaryEntry" /> objects copied from <see cref="T:System.Collections.Hashtable" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Hashtable" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Hashtable" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
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
		CopyEntries(array, arrayIndex);
	}

	internal virtual KeyValuePairs[] ToKeyValuePairsArray()
	{
		KeyValuePairs[] array = new KeyValuePairs[count];
		int num = 0;
		bucket[] array2 = buckets;
		int num2 = array2.Length;
		while (--num2 >= 0)
		{
			object key = array2[num2].key;
			if (key != null && key != buckets)
			{
				array[num++] = new KeyValuePairs(key, array2[num2].val);
			}
		}
		return array;
	}

	private void CopyValues(Array array, int arrayIndex)
	{
		bucket[] array2 = buckets;
		int num = array2.Length;
		while (--num >= 0)
		{
			object key = array2[num].key;
			if (key != null && key != buckets)
			{
				array.SetValue(array2[num].val, arrayIndex++);
			}
		}
	}

	private void expand()
	{
		int newsize = HashHelpers.ExpandPrime(buckets.Length);
		rehash(newsize, forceNewHashCode: false);
	}

	private void rehash()
	{
		rehash(buckets.Length, forceNewHashCode: false);
	}

	private void UpdateVersion()
	{
		version++;
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	private void rehash(int newsize, bool forceNewHashCode)
	{
		occupancy = 0;
		bucket[] newBuckets = new bucket[newsize];
		for (int i = 0; i < buckets.Length; i++)
		{
			bucket bucket = buckets[i];
			if (bucket.key != null && bucket.key != buckets)
			{
				int hashcode = (forceNewHashCode ? GetHash(bucket.key) : bucket.hash_coll) & 0x7FFFFFFF;
				putEntry(newBuckets, bucket.key, bucket.val, hashcode);
			}
		}
		Thread.BeginCriticalRegion();
		isWriterInProgress = true;
		buckets = newBuckets;
		loadsize = (int)(loadFactor * (float)newsize);
		UpdateVersion();
		isWriterInProgress = false;
		Thread.EndCriticalRegion();
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new HashtableEnumerator(this, 3);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> that iterates through the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return new HashtableEnumerator(this, 3);
	}

	/// <summary>Returns the hash code for the specified key.</summary>
	/// <returns>The hash code for <paramref name="key" />.</returns>
	/// <param name="key">The <see cref="T:System.Object" /> for which a hash code is to be returned. </param>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="key" /> is null. </exception>
	protected virtual int GetHash(object key)
	{
		if (_keycomparer != null)
		{
			return _keycomparer.GetHashCode(key);
		}
		return key.GetHashCode();
	}

	/// <summary>Compares a specific <see cref="T:System.Object" /> with a specific key in the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>true if <paramref name="item" /> and <paramref name="key" /> are equal; otherwise, false.</returns>
	/// <param name="item">The <see cref="T:System.Object" /> to compare with <paramref name="key" />. </param>
	/// <param name="key">The key in the <see cref="T:System.Collections.Hashtable" /> to compare with <paramref name="item" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="item" /> is null.-or- <paramref name="key" /> is null. </exception>
	protected virtual bool KeyEquals(object item, object key)
	{
		if (buckets == item)
		{
			return false;
		}
		if (item == key)
		{
			return true;
		}
		if (_keycomparer != null)
		{
			return _keycomparer.Equals(item, key);
		}
		return item?.Equals(key) ?? false;
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	private void Insert(object key, object nvalue, bool add)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
		}
		if (count >= loadsize)
		{
			expand();
		}
		else if (occupancy > loadsize && count > 100)
		{
			rehash();
		}
		uint seed;
		uint incr;
		uint num = InitHash(key, buckets.Length, out seed, out incr);
		int num2 = 0;
		int num3 = -1;
		int num4 = (int)(seed % (uint)buckets.Length);
		do
		{
			if (num3 == -1 && buckets[num4].key == buckets && buckets[num4].hash_coll < 0)
			{
				num3 = num4;
			}
			if (buckets[num4].key == null || (buckets[num4].key == buckets && (buckets[num4].hash_coll & 0x80000000u) == 0L))
			{
				if (num3 != -1)
				{
					num4 = num3;
				}
				Thread.BeginCriticalRegion();
				isWriterInProgress = true;
				buckets[num4].val = nvalue;
				buckets[num4].key = key;
				buckets[num4].hash_coll |= (int)num;
				count++;
				UpdateVersion();
				isWriterInProgress = false;
				Thread.EndCriticalRegion();
				return;
			}
			if ((buckets[num4].hash_coll & 0x7FFFFFFF) == num && KeyEquals(buckets[num4].key, key))
			{
				if (add)
				{
					throw new ArgumentException(Environment.GetResourceString("Item has already been added. Key in dictionary: '{0}'  Key being added: '{1}'", buckets[num4].key, key));
				}
				Thread.BeginCriticalRegion();
				isWriterInProgress = true;
				buckets[num4].val = nvalue;
				UpdateVersion();
				isWriterInProgress = false;
				Thread.EndCriticalRegion();
				return;
			}
			if (num3 == -1 && buckets[num4].hash_coll >= 0)
			{
				buckets[num4].hash_coll |= int.MinValue;
				occupancy++;
			}
			num4 = (int)((num4 + incr) % (uint)buckets.Length);
		}
		while (++num2 < buckets.Length);
		if (num3 != -1)
		{
			Thread.BeginCriticalRegion();
			isWriterInProgress = true;
			buckets[num3].val = nvalue;
			buckets[num3].key = key;
			buckets[num3].hash_coll |= (int)num;
			count++;
			UpdateVersion();
			isWriterInProgress = false;
			Thread.EndCriticalRegion();
			return;
		}
		throw new InvalidOperationException(Environment.GetResourceString("Hashtable insert failed. Load factor too high. The most common cause is multiple threads writing to the Hashtable simultaneously."));
	}

	private void putEntry(bucket[] newBuckets, object key, object nvalue, int hashcode)
	{
		uint num = 1 + (uint)(hashcode * 101) % (uint)(newBuckets.Length - 1);
		int num2 = (int)((uint)hashcode % (uint)newBuckets.Length);
		while (newBuckets[num2].key != null && newBuckets[num2].key != buckets)
		{
			if (newBuckets[num2].hash_coll >= 0)
			{
				newBuckets[num2].hash_coll |= int.MinValue;
				occupancy++;
			}
			num2 = (int)((num2 + num) % (uint)newBuckets.Length);
		}
		newBuckets[num2].val = nvalue;
		newBuckets[num2].key = key;
		newBuckets[num2].hash_coll |= hashcode;
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <param name="key">The key of the element to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Hashtable" /> is read-only.-or- The <see cref="T:System.Collections.Hashtable" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public virtual void Remove(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", Environment.GetResourceString("Key cannot be null."));
		}
		uint seed;
		uint incr;
		uint num = InitHash(key, buckets.Length, out seed, out incr);
		int num2 = 0;
		int num3 = (int)(seed % (uint)buckets.Length);
		bucket bucket;
		do
		{
			bucket = buckets[num3];
			if ((bucket.hash_coll & 0x7FFFFFFF) == num && KeyEquals(bucket.key, key))
			{
				Thread.BeginCriticalRegion();
				isWriterInProgress = true;
				buckets[num3].hash_coll &= int.MinValue;
				if (buckets[num3].hash_coll != 0)
				{
					buckets[num3].key = buckets;
				}
				else
				{
					buckets[num3].key = null;
				}
				buckets[num3].val = null;
				count--;
				UpdateVersion();
				isWriterInProgress = false;
				Thread.EndCriticalRegion();
				break;
			}
			num3 = (int)((num3 + incr) % (uint)buckets.Length);
		}
		while (bucket.hash_coll < 0 && ++num2 < buckets.Length);
	}

	/// <summary>Returns a synchronized (thread-safe) wrapper for the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <returns>A synchronized (thread-safe) wrapper for the <see cref="T:System.Collections.Hashtable" />.</returns>
	/// <param name="table">The <see cref="T:System.Collections.Hashtable" /> to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="table" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static Hashtable Synchronized(Hashtable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		return new SyncHashtable(table);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Hashtable" />.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Hashtable" />. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Hashtable" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		lock (SyncRoot)
		{
			int num = version;
			info.AddValue("LoadFactor", loadFactor);
			info.AddValue("Version", version);
			IEqualityComparer keycomparer = _keycomparer;
			if (keycomparer == null)
			{
				info.AddValue("Comparer", null, typeof(IComparer));
				info.AddValue("HashCodeProvider", null, typeof(IHashCodeProvider));
			}
			else if (keycomparer is CompatibleComparer)
			{
				CompatibleComparer compatibleComparer = keycomparer as CompatibleComparer;
				info.AddValue("Comparer", compatibleComparer.Comparer, typeof(IComparer));
				info.AddValue("HashCodeProvider", compatibleComparer.HashCodeProvider, typeof(IHashCodeProvider));
			}
			else
			{
				info.AddValue("KeyComparer", keycomparer, typeof(IEqualityComparer));
			}
			info.AddValue("HashSize", buckets.Length);
			object[] array = new object[count];
			object[] array2 = new object[count];
			CopyKeys(array, 0);
			CopyValues(array2, 0);
			info.AddValue("Keys", array, typeof(object[]));
			info.AddValue("Values", array2, typeof(object[]));
			if (version != num)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
		}
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event. </param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Hashtable" /> is invalid. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void OnDeserialization(object sender)
	{
		if (buckets != null)
		{
			return;
		}
		HashHelpers.SerializationInfoTable.TryGetValue(this, out var value);
		if (value == null)
		{
			throw new SerializationException(Environment.GetResourceString("OnDeserialization method was called while the object was not being deserialized."));
		}
		int num = 0;
		IComparer comparer = null;
		IHashCodeProvider hashCodeProvider = null;
		object[] array = null;
		object[] array2 = null;
		SerializationInfoEnumerator enumerator = value.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Name)
			{
			case "LoadFactor":
				loadFactor = value.GetSingle("LoadFactor");
				break;
			case "HashSize":
				num = value.GetInt32("HashSize");
				break;
			case "KeyComparer":
				_keycomparer = (IEqualityComparer)value.GetValue("KeyComparer", typeof(IEqualityComparer));
				break;
			case "Comparer":
				comparer = (IComparer)value.GetValue("Comparer", typeof(IComparer));
				break;
			case "HashCodeProvider":
				hashCodeProvider = (IHashCodeProvider)value.GetValue("HashCodeProvider", typeof(IHashCodeProvider));
				break;
			case "Keys":
				array = (object[])value.GetValue("Keys", typeof(object[]));
				break;
			case "Values":
				array2 = (object[])value.GetValue("Values", typeof(object[]));
				break;
			}
		}
		loadsize = (int)(loadFactor * (float)num);
		if (_keycomparer == null && (comparer != null || hashCodeProvider != null))
		{
			_keycomparer = new CompatibleComparer(comparer, hashCodeProvider);
		}
		buckets = new bucket[num];
		if (array == null)
		{
			throw new SerializationException(Environment.GetResourceString("The Keys for this Hashtable are missing."));
		}
		if (array2 == null)
		{
			throw new SerializationException(Environment.GetResourceString("The values for this dictionary are missing."));
		}
		if (array.Length != array2.Length)
		{
			throw new SerializationException(Environment.GetResourceString("The keys and values arrays have different sizes."));
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
			{
				throw new SerializationException(Environment.GetResourceString("One of the serialized keys is null."));
			}
			Insert(array[i], array2[i], add: true);
		}
		version = value.GetInt32("Version");
		HashHelpers.SerializationInfoTable.Remove(this);
	}
}
