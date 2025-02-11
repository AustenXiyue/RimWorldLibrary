using System.Threading;

namespace System.Collections.Specialized;

/// <summary>Implements IDictionary using a singly linked list. Recommended for collections that typically include fewer than 10 items.</summary>
[Serializable]
public class ListDictionary : IDictionary, ICollection, IEnumerable
{
	private class NodeEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private ListDictionary list;

		private DictionaryNode current;

		private int version;

		private bool start;

		public object Current => Entry;

		public DictionaryEntry Entry
		{
			get
			{
				if (current == null)
				{
					throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
				}
				return new DictionaryEntry(current.key, current.value);
			}
		}

		public object Key
		{
			get
			{
				if (current == null)
				{
					throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
				}
				return current.key;
			}
		}

		public object Value
		{
			get
			{
				if (current == null)
				{
					throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
				}
				return current.value;
			}
		}

		public NodeEnumerator(ListDictionary list)
		{
			this.list = list;
			version = list.version;
			start = true;
			current = null;
		}

		public bool MoveNext()
		{
			if (version != list.version)
			{
				throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
			}
			if (start)
			{
				current = list.head;
				start = false;
			}
			else if (current != null)
			{
				current = current.next;
			}
			return current != null;
		}

		public void Reset()
		{
			if (version != list.version)
			{
				throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
			}
			start = true;
			current = null;
		}
	}

	private class NodeKeyValueCollection : ICollection, IEnumerable
	{
		private class NodeKeyValueEnumerator : IEnumerator
		{
			private ListDictionary list;

			private DictionaryNode current;

			private int version;

			private bool isKeys;

			private bool start;

			public object Current
			{
				get
				{
					if (current == null)
					{
						throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
					}
					if (!isKeys)
					{
						return current.value;
					}
					return current.key;
				}
			}

			public NodeKeyValueEnumerator(ListDictionary list, bool isKeys)
			{
				this.list = list;
				this.isKeys = isKeys;
				version = list.version;
				start = true;
				current = null;
			}

			public bool MoveNext()
			{
				if (version != list.version)
				{
					throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
				}
				if (start)
				{
					current = list.head;
					start = false;
				}
				else if (current != null)
				{
					current = current.next;
				}
				return current != null;
			}

			public void Reset()
			{
				if (version != list.version)
				{
					throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
				}
				start = true;
				current = null;
			}
		}

		private ListDictionary list;

		private bool isKeys;

		int ICollection.Count
		{
			get
			{
				int num = 0;
				for (DictionaryNode dictionaryNode = list.head; dictionaryNode != null; dictionaryNode = dictionaryNode.next)
				{
					num++;
				}
				return num;
			}
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => list.SyncRoot;

		public NodeKeyValueCollection(ListDictionary list, bool isKeys)
		{
			this.list = list;
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
				throw new ArgumentOutOfRangeException("index", global::SR.GetString("Non-negative number required."));
			}
			for (DictionaryNode dictionaryNode = list.head; dictionaryNode != null; dictionaryNode = dictionaryNode.next)
			{
				array.SetValue(isKeys ? dictionaryNode.key : dictionaryNode.value, index);
				index++;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new NodeKeyValueEnumerator(list, isKeys);
		}
	}

	[Serializable]
	private class DictionaryNode
	{
		public object key;

		public object value;

		public DictionaryNode next;
	}

	private DictionaryNode head;

	private int version;

	private int count;

	private IComparer comparer;

	[NonSerialized]
	private object _syncRoot;

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new entry using the specified key.</returns>
	/// <param name="key">The key whose value to get or set. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public object this[object key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
			}
			DictionaryNode next = head;
			if (comparer == null)
			{
				while (next != null)
				{
					object key2 = next.key;
					if (key2 != null && key2.Equals(key))
					{
						return next.value;
					}
					next = next.next;
				}
			}
			else
			{
				while (next != null)
				{
					object key3 = next.key;
					if (key3 != null && comparer.Compare(key3, key) == 0)
					{
						return next.value;
					}
					next = next.next;
				}
			}
			return null;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
			}
			version++;
			DictionaryNode dictionaryNode = null;
			DictionaryNode next;
			for (next = head; next != null; next = next.next)
			{
				object key2 = next.key;
				if ((comparer == null) ? key2.Equals(key) : (comparer.Compare(key2, key) == 0))
				{
					break;
				}
				dictionaryNode = next;
			}
			if (next != null)
			{
				next.value = value;
				return;
			}
			DictionaryNode dictionaryNode2 = new DictionaryNode();
			dictionaryNode2.key = key;
			dictionaryNode2.value = value;
			if (dictionaryNode != null)
			{
				dictionaryNode.next = dictionaryNode2;
			}
			else
			{
				head = dictionaryNode2;
			}
			count++;
		}
	}

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.ListDictionary" />.</returns>
	public int Count => count;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys in the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys in the <see cref="T:System.Collections.Specialized.ListDictionary" />.</returns>
	public ICollection Keys => new NodeKeyValueCollection(this, isKeys: true);

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.ListDictionary" /> is read-only.</summary>
	/// <returns>This property always returns false.</returns>
	public bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.ListDictionary" /> has a fixed size.</summary>
	/// <returns>This property always returns false.</returns>
	public bool IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.ListDictionary" /> is synchronized (thread safe).</summary>
	/// <returns>This property always returns false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.ListDictionary" />.</returns>
	public object SyncRoot
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

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.Specialized.ListDictionary" />.</returns>
	public ICollection Values => new NodeKeyValueCollection(this, isKeys: false);

	/// <summary>Creates an empty <see cref="T:System.Collections.Specialized.ListDictionary" /> using the default comparer.</summary>
	public ListDictionary()
	{
	}

	/// <summary>Creates an empty <see cref="T:System.Collections.Specialized.ListDictionary" /> using the specified comparer.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.-or- null to use the default comparer, which is each key's implementation of <see cref="M:System.Object.Equals(System.Object)" />. </param>
	public ListDictionary(IComparer comparer)
	{
		this.comparer = comparer;
	}

	/// <summary>Adds an entry with the specified key and value into the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <param name="key">The key of the entry to add. </param>
	/// <param name="value">The value of the entry to add. The value can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">An entry with the same key already exists in the <see cref="T:System.Collections.Specialized.ListDictionary" />. </exception>
	public void Add(object key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
		}
		version++;
		DictionaryNode dictionaryNode = null;
		for (DictionaryNode next = head; next != null; next = next.next)
		{
			object key2 = next.key;
			if ((comparer == null) ? key2.Equals(key) : (comparer.Compare(key2, key) == 0))
			{
				throw new ArgumentException(global::SR.GetString("An item with the same key has already been added. Key: {0}"));
			}
			dictionaryNode = next;
		}
		DictionaryNode dictionaryNode2 = new DictionaryNode();
		dictionaryNode2.key = key;
		dictionaryNode2.value = value;
		if (dictionaryNode != null)
		{
			dictionaryNode.next = dictionaryNode2;
		}
		else
		{
			head = dictionaryNode2;
		}
		count++;
	}

	/// <summary>Removes all entries from the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	public void Clear()
	{
		count = 0;
		head = null;
		version++;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Specialized.ListDictionary" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.ListDictionary" /> contains an entry with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Specialized.ListDictionary" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public bool Contains(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
		}
		for (DictionaryNode next = head; next != null; next = next.next)
		{
			object key2 = next.key;
			if ((comparer == null) ? key2.Equals(key) : (comparer.Compare(key2, key) == 0))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Specialized.ListDictionary" /> entries to a one-dimensional <see cref="T:System.Array" /> instance at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the <see cref="T:System.Collections.DictionaryEntry" /> objects copied from <see cref="T:System.Collections.Specialized.ListDictionary" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Specialized.ListDictionary" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Specialized.ListDictionary" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	public void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", global::SR.GetString("Non-negative number required."));
		}
		if (array.Length - index < count)
		{
			throw new ArgumentException(global::SR.GetString("Insufficient space in the target location to copy the information."));
		}
		for (DictionaryNode next = head; next != null; next = next.next)
		{
			array.SetValue(new DictionaryEntry(next.key, next.value), index);
			index++;
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> that iterates through the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Specialized.ListDictionary" />.</returns>
	public IDictionaryEnumerator GetEnumerator()
	{
		return new NodeEnumerator(this);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that iterates through the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Specialized.ListDictionary" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new NodeEnumerator(this);
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Collections.Specialized.ListDictionary" />.</summary>
	/// <param name="key">The key of the entry to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public void Remove(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
		}
		version++;
		DictionaryNode dictionaryNode = null;
		DictionaryNode next;
		for (next = head; next != null; next = next.next)
		{
			object key2 = next.key;
			if ((comparer == null) ? key2.Equals(key) : (comparer.Compare(key2, key) == 0))
			{
				break;
			}
			dictionaryNode = next;
		}
		if (next != null)
		{
			if (next == head)
			{
				head = next.next;
			}
			else
			{
				dictionaryNode.next = next.next;
			}
			count--;
		}
	}
}
