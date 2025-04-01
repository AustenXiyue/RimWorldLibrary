using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Collections.Generic;

/// <summary>Represents a set of values.</summary>
/// <typeparam name="T">The type of elements in the hash set.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(System.Collections.Generic.ICollectionDebugView<>))]
public class HashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
{
	internal struct ElementCount
	{
		internal int uniqueCount;

		internal int unfoundCount;
	}

	internal struct Slot
	{
		internal int hashCode;

		internal int next;

		internal T value;
	}

	/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	/// <filterpriority>2</filterpriority>
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private HashSet<T> _set;

		private int _index;

		private int _version;

		private T _current;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the <see cref="T:System.Collections.Generic.HashSet`1" /> collection at the current position of the enumerator.</returns>
		public T Current => _current;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the collection at the current position of the enumerator, as an <see cref="T:System.Object" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _set._lastIndex + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return Current;
			}
		}

		internal Enumerator(HashSet<T> set)
		{
			_set = set;
			_index = 0;
			_version = set._version;
			_current = default(T);
		}

		/// <summary>Releases all resources used by a <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object.</summary>
		public void Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		public bool MoveNext()
		{
			if (_version != _set._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			while (_index < _set._lastIndex)
			{
				if (_set._slots[_index].hashCode >= 0)
				{
					_current = _set._slots[_index].value;
					_index++;
					return true;
				}
				_index++;
			}
			_index = _set._lastIndex + 1;
			_current = default(T);
			return false;
		}

		/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		void IEnumerator.Reset()
		{
			if (_version != _set._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			_index = 0;
			_current = default(T);
		}
	}

	private const int Lower31BitMask = int.MaxValue;

	private const int StackAllocThreshold = 100;

	private const int ShrinkThreshold = 3;

	private const string CapacityName = "Capacity";

	private const string ElementsName = "Elements";

	private const string ComparerName = "Comparer";

	private const string VersionName = "Version";

	private int[] _buckets;

	private Slot[] _slots;

	private int _count;

	private int _lastIndex;

	private int _freeList;

	private IEqualityComparer<T> _comparer;

	private int _version;

	private SerializationInfo _siInfo;

	/// <summary>Gets the number of elements that are contained in a set.</summary>
	/// <returns>The number of elements that are contained in the set.</returns>
	public int Count => _count;

	/// <summary>Gets a value indicating whether a collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	bool ICollection<T>.IsReadOnly => false;

	/// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> object that is used to determine equality for the values in the set.</summary>
	/// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> object that is used to determine equality for the values in the set.</returns>
	public IEqualityComparer<T> Comparer => _comparer;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the default equality comparer for the set type.</summary>
	public HashSet()
		: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.</param>
	public HashSet(IEqualityComparer<T> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<T>.Default;
		}
		_comparer = comparer;
		_lastIndex = 0;
		_count = 0;
		_freeList = -1;
		_version = 0;
	}

	public HashSet(int capacity)
		: this(capacity, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the default equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
	/// <param name="collection">The collection whose elements are copied to the new set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public HashSet(IEnumerable<T> collection)
		: this(collection, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
	/// <param name="collection">The collection whose elements are copied to the new set.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
		: this(comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
		{
			CopyFrom(hashSet);
			return;
		}
		Initialize((collection is ICollection<T> collection2) ? collection2.Count : 0);
		UnionWith(collection);
		if (_count > 0 && _slots.Length / _count > 3)
		{
			TrimExcess();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class with serialized data.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	protected HashSet(SerializationInfo info, StreamingContext context)
	{
		_siInfo = info;
	}

	private void CopyFrom(HashSet<T> source)
	{
		int count = source._count;
		if (count == 0)
		{
			return;
		}
		int num = source._buckets.Length;
		if (HashHelpers.ExpandPrime(count + 1) >= num)
		{
			_buckets = (int[])source._buckets.Clone();
			_slots = (Slot[])source._slots.Clone();
			_lastIndex = source._lastIndex;
			_freeList = source._freeList;
		}
		else
		{
			int lastIndex = source._lastIndex;
			Slot[] slots = source._slots;
			Initialize(count);
			int num2 = 0;
			for (int i = 0; i < lastIndex; i++)
			{
				int hashCode = slots[i].hashCode;
				if (hashCode >= 0)
				{
					AddValue(num2, hashCode, slots[i].value);
					num2++;
				}
			}
			_lastIndex = num2;
		}
		_count = count;
	}

	public HashSet(int capacity, IEqualityComparer<T> comparer)
		: this(comparer)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		if (capacity > 0)
		{
			Initialize(capacity);
		}
	}

	/// <summary>Adds an item to an <see cref="T:System.Collections.Generic.ICollection`1" /> object.</summary>
	/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" /> object.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
	void ICollection<T>.Add(T item)
	{
		AddIfNotPresent(item);
	}

	/// <summary>Removes all elements from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	public void Clear()
	{
		if (_lastIndex > 0)
		{
			Array.Clear(_slots, 0, _lastIndex);
			Array.Clear(_buckets, 0, _buckets.Length);
			_lastIndex = 0;
			_count = 0;
			_freeList = -1;
		}
		_version++;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element; otherwise, false.</returns>
	/// <param name="item">The element to locate in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	public bool Contains(T item)
	{
		if (_buckets != null)
		{
			int num = InternalGetHashCode(item);
			for (int num2 = _buckets[num % _buckets.Length] - 1; num2 >= 0; num2 = _slots[num2].next)
			{
				if (_slots[num2].hashCode == num && _comparer.Equals(_slots[num2].value, item))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		CopyTo(array, arrayIndex, _count);
	}

	/// <summary>Removes the specified element from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	/// <returns>true if the element is successfully found and removed; otherwise, false.  This method returns false if <paramref name="item" /> is not found in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
	/// <param name="item">The element to remove.</param>
	public bool Remove(T item)
	{
		if (_buckets != null)
		{
			int num = InternalGetHashCode(item);
			int num2 = num % _buckets.Length;
			int num3 = -1;
			for (int num4 = _buckets[num2] - 1; num4 >= 0; num4 = _slots[num4].next)
			{
				if (_slots[num4].hashCode == num && _comparer.Equals(_slots[num4].value, item))
				{
					if (num3 < 0)
					{
						_buckets[num2] = _slots[num4].next + 1;
					}
					else
					{
						_slots[num3].next = _slots[num4].next;
					}
					_slots[num4].hashCode = -1;
					if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
					{
						_slots[num4].value = default(T);
					}
					_slots[num4].next = _freeList;
					_count--;
					_version++;
					if (_count == 0)
					{
						_lastIndex = 0;
						_freeList = -1;
					}
					else
					{
						_freeList = num4;
					}
					return true;
				}
				num3 = num4;
			}
		}
		return false;
	}

	/// <summary>Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object for the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> object that can be used to iterate through the collection.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Version", _version);
		info.AddValue("Comparer", _comparer, typeof(IComparer<T>));
		info.AddValue("Capacity", (_buckets != null) ? _buckets.Length : 0);
		if (_buckets != null)
		{
			T[] array = new T[_count];
			CopyTo(array);
			info.AddValue("Elements", array, typeof(T[]));
		}
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.HashSet`1" /> object is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void OnDeserialization(object sender)
	{
		if (_siInfo == null)
		{
			return;
		}
		int @int = _siInfo.GetInt32("Capacity");
		_comparer = (IEqualityComparer<T>)_siInfo.GetValue("Comparer", typeof(IEqualityComparer<T>));
		_freeList = -1;
		if (@int != 0)
		{
			_buckets = new int[@int];
			_slots = new Slot[@int];
			T[] array = (T[])_siInfo.GetValue("Elements", typeof(T[]));
			if (array == null)
			{
				throw new SerializationException("The Keys for this dictionary are missing.");
			}
			for (int i = 0; i < array.Length; i++)
			{
				AddIfNotPresent(array[i]);
			}
		}
		else
		{
			_buckets = null;
		}
		_version = _siInfo.GetInt32("Version");
		_siInfo = null;
	}

	/// <summary>Adds the specified element to a set.</summary>
	/// <returns>true if the element is added to the <see cref="T:System.Collections.Generic.HashSet`1" /> object; false if the element is already present.</returns>
	/// <param name="item">The element to add to the set.</param>
	public bool Add(T item)
	{
		return AddIfNotPresent(item);
	}

	public bool TryGetValue(T equalValue, out T actualValue)
	{
		if (_buckets != null)
		{
			int num = InternalIndexOf(equalValue);
			if (num >= 0)
			{
				actualValue = _slots[num].value;
				return true;
			}
		}
		actualValue = default(T);
		return false;
	}

	/// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain all elements that are present in itself, the specified collection, or both.</summary>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public void UnionWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (T item in other)
		{
			AddIfNotPresent(item);
		}
	}

	/// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present in that object and in the specified collection.</summary>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public void IntersectWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_count == 0 || other == this)
		{
			return;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				Clear();
				return;
			}
			if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
			{
				IntersectWithHashSetWithSameEC(hashSet);
				return;
			}
		}
		IntersectWithEnumerable(other);
	}

	/// <summary>Removes all elements in the specified collection from the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	/// <param name="other">The collection of items to remove from the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public void ExceptWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_count == 0)
		{
			return;
		}
		if (other == this)
		{
			Clear();
			return;
		}
		foreach (T item in other)
		{
			Remove(item);
		}
	}

	/// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present either in that object or in the specified collection, but not both.</summary>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_count == 0)
		{
			UnionWith(other);
		}
		else if (other == this)
		{
			Clear();
		}
		else if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
		{
			SymmetricExceptWithUniqueHashSet(hashSet);
		}
		else
		{
			SymmetricExceptWithEnumerable(other);
		}
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of the specified collection.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of <paramref name="other" />; otherwise, false.</returns>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_count == 0)
		{
			return true;
		}
		if (other == this)
		{
			return true;
		}
		if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
		{
			if (_count > hashSet.Count)
			{
				return false;
			}
			return IsSubsetOfHashSetWithSameEC(hashSet);
		}
		ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
		if (elementCount.uniqueCount == _count)
		{
			return elementCount.unfoundCount >= 0;
		}
		return false;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of the specified collection.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of <paramref name="other" />; otherwise, false.</returns>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other == this)
		{
			return false;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				return false;
			}
			if (_count == 0)
			{
				return collection.Count > 0;
			}
			if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
			{
				if (_count >= hashSet.Count)
				{
					return false;
				}
				return IsSubsetOfHashSetWithSameEC(hashSet);
			}
		}
		ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
		if (elementCount.uniqueCount == _count)
		{
			return elementCount.unfoundCount > 0;
		}
		return false;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of the specified collection.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of <paramref name="other" />; otherwise, false.</returns>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other == this)
		{
			return true;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				return true;
			}
			if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet) && hashSet.Count > _count)
			{
				return false;
			}
		}
		return ContainsAllElements(other);
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of the specified collection.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of <paramref name="other" />; otherwise, false.</returns>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_count == 0)
		{
			return false;
		}
		if (other == this)
		{
			return false;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				return true;
			}
			if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
			{
				if (hashSet.Count >= _count)
				{
					return false;
				}
				return ContainsAllElements(hashSet);
			}
		}
		ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
		if (elementCount.uniqueCount < _count)
		{
			return elementCount.unfoundCount == 0;
		}
		return false;
	}

	/// <summary>Determines whether the current <see cref="T:System.Collections.Generic.HashSet`1" /> object and a specified collection share common elements.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object and <paramref name="other" /> share at least one common element; otherwise, false.</returns>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public bool Overlaps(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_count == 0)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		foreach (T item in other)
		{
			if (Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object and the specified collection contain the same elements.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is equal to <paramref name="other" />; otherwise, false.</returns>
	/// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="other" /> is null.</exception>
	public bool SetEquals(IEnumerable<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other == this)
		{
			return true;
		}
		if (other is HashSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
		{
			if (_count != hashSet.Count)
			{
				return false;
			}
			return ContainsAllElements(hashSet);
		}
		if (other is ICollection<T> collection && _count == 0 && collection.Count > 0)
		{
			return false;
		}
		ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
		if (elementCount.uniqueCount == _count)
		{
			return elementCount.unfoundCount == 0;
		}
		return false;
	}

	/// <summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	public void CopyTo(T[] array)
	{
		CopyTo(array, 0, _count);
	}

	/// <summary>Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <param name="count">The number of elements to copy to <paramref name="array" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.-or-<paramref name="count" /> is greater than the available space from the <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int arrayIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Non negative number is required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", count, "Non negative number is required.");
		}
		if (arrayIndex > array.Length || count > array.Length - arrayIndex)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		int num = 0;
		for (int i = 0; i < _lastIndex; i++)
		{
			if (num >= count)
			{
				break;
			}
			if (_slots[i].hashCode >= 0)
			{
				array[arrayIndex + num] = _slots[i].value;
				num++;
			}
		}
	}

	/// <summary>Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
	/// <returns>The number of elements that were removed from the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</returns>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public int RemoveWhere(Predicate<T> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		int num = 0;
		for (int i = 0; i < _lastIndex; i++)
		{
			if (_slots[i].hashCode >= 0)
			{
				T value = _slots[i].value;
				if (match(value) && Remove(value))
				{
					num++;
				}
			}
		}
		return num;
	}

	/// <summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.</summary>
	public void TrimExcess()
	{
		if (_count == 0)
		{
			_buckets = null;
			_slots = null;
			_version++;
			return;
		}
		int prime = HashHelpers.GetPrime(_count);
		Slot[] array = new Slot[prime];
		int[] array2 = new int[prime];
		int num = 0;
		for (int i = 0; i < _lastIndex; i++)
		{
			if (_slots[i].hashCode >= 0)
			{
				array[num] = _slots[i];
				int num2 = array[num].hashCode % prime;
				array[num].next = array2[num2] - 1;
				array2[num2] = num + 1;
				num++;
			}
		}
		_lastIndex = num;
		_slots = array;
		_buckets = array2;
		_freeList = -1;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for deep equality testing of the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
	public static IEqualityComparer<HashSet<T>> CreateSetComparer()
	{
		return new HashSetEqualityComparer<T>();
	}

	private void Initialize(int capacity)
	{
		int prime = HashHelpers.GetPrime(capacity);
		_buckets = new int[prime];
		_slots = new Slot[prime];
	}

	private void IncreaseCapacity()
	{
		int num = HashHelpers.ExpandPrime(_count);
		if (num <= _count)
		{
			throw new ArgumentException("HashSet capacity is too big.");
		}
		SetCapacity(num);
	}

	private void SetCapacity(int newSize)
	{
		Slot[] array = new Slot[newSize];
		if (_slots != null)
		{
			Array.Copy(_slots, 0, array, 0, _lastIndex);
		}
		int[] array2 = new int[newSize];
		for (int i = 0; i < _lastIndex; i++)
		{
			int num = array[i].hashCode % newSize;
			array[i].next = array2[num] - 1;
			array2[num] = i + 1;
		}
		_slots = array;
		_buckets = array2;
	}

	private bool AddIfNotPresent(T value)
	{
		if (_buckets == null)
		{
			Initialize(0);
		}
		int num = InternalGetHashCode(value);
		int num2 = num % _buckets.Length;
		for (int num3 = _buckets[num2] - 1; num3 >= 0; num3 = _slots[num3].next)
		{
			if (_slots[num3].hashCode == num && _comparer.Equals(_slots[num3].value, value))
			{
				return false;
			}
		}
		int num4;
		if (_freeList >= 0)
		{
			num4 = _freeList;
			_freeList = _slots[num4].next;
		}
		else
		{
			if (_lastIndex == _slots.Length)
			{
				IncreaseCapacity();
				num2 = num % _buckets.Length;
			}
			num4 = _lastIndex;
			_lastIndex++;
		}
		_slots[num4].hashCode = num;
		_slots[num4].value = value;
		_slots[num4].next = _buckets[num2] - 1;
		_buckets[num2] = num4 + 1;
		_count++;
		_version++;
		return true;
	}

	private void AddValue(int index, int hashCode, T value)
	{
		int num = hashCode % _buckets.Length;
		_slots[index].hashCode = hashCode;
		_slots[index].value = value;
		_slots[index].next = _buckets[num] - 1;
		_buckets[num] = index + 1;
	}

	private bool ContainsAllElements(IEnumerable<T> other)
	{
		foreach (T item in other)
		{
			if (!Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsSubsetOfHashSetWithSameEC(HashSet<T> other)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (!other.Contains(current))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void IntersectWithHashSetWithSameEC(HashSet<T> other)
	{
		for (int i = 0; i < _lastIndex; i++)
		{
			if (_slots[i].hashCode >= 0)
			{
				T value = _slots[i].value;
				if (!other.Contains(value))
				{
					Remove(value);
				}
			}
		}
	}

	private unsafe void IntersectWithEnumerable(IEnumerable<T> other)
	{
		int lastIndex = _lastIndex;
		int num = System.Collections.Generic.BitHelper.ToIntArrayLength(lastIndex);
		System.Collections.Generic.BitHelper bitHelper = ((num > 100) ? new System.Collections.Generic.BitHelper(new int[num], num) : new System.Collections.Generic.BitHelper(stackalloc int[num], num));
		foreach (T item in other)
		{
			int num2 = InternalIndexOf(item);
			if (num2 >= 0)
			{
				bitHelper.MarkBit(num2);
			}
		}
		for (int i = 0; i < lastIndex; i++)
		{
			if (_slots[i].hashCode >= 0 && !bitHelper.IsMarked(i))
			{
				Remove(_slots[i].value);
			}
		}
	}

	private int InternalIndexOf(T item)
	{
		int num = InternalGetHashCode(item);
		for (int num2 = _buckets[num % _buckets.Length] - 1; num2 >= 0; num2 = _slots[num2].next)
		{
			if (_slots[num2].hashCode == num && _comparer.Equals(_slots[num2].value, item))
			{
				return num2;
			}
		}
		return -1;
	}

	private void SymmetricExceptWithUniqueHashSet(HashSet<T> other)
	{
		foreach (T item in other)
		{
			if (!Remove(item))
			{
				AddIfNotPresent(item);
			}
		}
	}

	private unsafe void SymmetricExceptWithEnumerable(IEnumerable<T> other)
	{
		int lastIndex = _lastIndex;
		int num = System.Collections.Generic.BitHelper.ToIntArrayLength(lastIndex);
		System.Collections.Generic.BitHelper bitHelper;
		System.Collections.Generic.BitHelper bitHelper2;
		if (num <= 50)
		{
			bitHelper = new System.Collections.Generic.BitHelper(stackalloc int[num], num);
			bitHelper2 = new System.Collections.Generic.BitHelper(stackalloc int[num], num);
		}
		else
		{
			bitHelper = new System.Collections.Generic.BitHelper(new int[num], num);
			bitHelper2 = new System.Collections.Generic.BitHelper(new int[num], num);
		}
		foreach (T item in other)
		{
			int location = 0;
			if (AddOrGetLocation(item, out location))
			{
				bitHelper2.MarkBit(location);
			}
			else if (location < lastIndex && !bitHelper2.IsMarked(location))
			{
				bitHelper.MarkBit(location);
			}
		}
		for (int i = 0; i < lastIndex; i++)
		{
			if (bitHelper.IsMarked(i))
			{
				Remove(_slots[i].value);
			}
		}
	}

	private bool AddOrGetLocation(T value, out int location)
	{
		int num = InternalGetHashCode(value);
		int num2 = num % _buckets.Length;
		for (int num3 = _buckets[num2] - 1; num3 >= 0; num3 = _slots[num3].next)
		{
			if (_slots[num3].hashCode == num && _comparer.Equals(_slots[num3].value, value))
			{
				location = num3;
				return false;
			}
		}
		int num4;
		if (_freeList >= 0)
		{
			num4 = _freeList;
			_freeList = _slots[num4].next;
		}
		else
		{
			if (_lastIndex == _slots.Length)
			{
				IncreaseCapacity();
				num2 = num % _buckets.Length;
			}
			num4 = _lastIndex;
			_lastIndex++;
		}
		_slots[num4].hashCode = num;
		_slots[num4].value = value;
		_slots[num4].next = _buckets[num2] - 1;
		_buckets[num2] = num4 + 1;
		_count++;
		_version++;
		location = num4;
		return true;
	}

	private unsafe ElementCount CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
	{
		ElementCount result = default(ElementCount);
		if (_count == 0)
		{
			int num = 0;
			using (IEnumerator<T> enumerator = other.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					_ = enumerator.Current;
					num++;
				}
			}
			result.uniqueCount = 0;
			result.unfoundCount = num;
			return result;
		}
		int num2 = System.Collections.Generic.BitHelper.ToIntArrayLength(_lastIndex);
		System.Collections.Generic.BitHelper bitHelper = ((num2 > 100) ? new System.Collections.Generic.BitHelper(new int[num2], num2) : new System.Collections.Generic.BitHelper(stackalloc int[num2], num2));
		int num3 = 0;
		int num4 = 0;
		foreach (T item in other)
		{
			int num5 = InternalIndexOf(item);
			if (num5 >= 0)
			{
				if (!bitHelper.IsMarked(num5))
				{
					bitHelper.MarkBit(num5);
					num4++;
				}
			}
			else
			{
				num3++;
				if (returnIfUnfound)
				{
					break;
				}
			}
		}
		result.uniqueCount = num4;
		result.unfoundCount = num3;
		return result;
	}

	internal static bool HashSetEquals(HashSet<T> set1, HashSet<T> set2, IEqualityComparer<T> comparer)
	{
		if (set1 == null)
		{
			return set2 == null;
		}
		if (set2 == null)
		{
			return false;
		}
		if (AreEqualityComparersEqual(set1, set2))
		{
			if (set1.Count != set2.Count)
			{
				return false;
			}
			foreach (T item in set2)
			{
				if (!set1.Contains(item))
				{
					return false;
				}
			}
			return true;
		}
		foreach (T item2 in set2)
		{
			bool flag = false;
			foreach (T item3 in set1)
			{
				if (comparer.Equals(item2, item3))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private static bool AreEqualityComparersEqual(HashSet<T> set1, HashSet<T> set2)
	{
		return set1.Comparer.Equals(set2.Comparer);
	}

	private int InternalGetHashCode(T item)
	{
		if (item == null)
		{
			return 0;
		}
		return _comparer.GetHashCode(item) & 0x7FFFFFFF;
	}
}
