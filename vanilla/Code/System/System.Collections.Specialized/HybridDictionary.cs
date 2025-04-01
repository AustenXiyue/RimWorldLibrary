namespace System.Collections.Specialized;

/// <summary>Implements IDictionary by using a <see cref="T:System.Collections.Specialized.ListDictionary" /> while the collection is small, and then switching to a <see cref="T:System.Collections.Hashtable" /> when the collection gets large.</summary>
[Serializable]
public class HybridDictionary : IDictionary, ICollection, IEnumerable
{
	private const int CutoverPoint = 9;

	private const int InitialHashtableSize = 13;

	private const int FixedSizeCutoverPoint = 6;

	private ListDictionary list;

	private Hashtable hashtable;

	private bool caseInsensitive;

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new entry using the specified key.</returns>
	/// <param name="key">The key whose value to get or set. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public object this[object key]
	{
		get
		{
			ListDictionary listDictionary = list;
			if (hashtable != null)
			{
				return hashtable[key];
			}
			if (listDictionary != null)
			{
				return listDictionary[key];
			}
			if (key == null)
			{
				throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
			}
			return null;
		}
		set
		{
			if (hashtable != null)
			{
				hashtable[key] = value;
			}
			else if (list != null)
			{
				if (list.Count >= 8)
				{
					ChangeOver();
					hashtable[key] = value;
				}
				else
				{
					list[key] = value;
				}
			}
			else
			{
				list = new ListDictionary(caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
				list[key] = value;
			}
		}
	}

	private ListDictionary List
	{
		get
		{
			if (list == null)
			{
				list = new ListDictionary(caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
			}
			return list;
		}
	}

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Specialized.HybridDictionary" />.Retrieving the value of this property is an O(1) operation.</returns>
	public int Count
	{
		get
		{
			ListDictionary listDictionary = list;
			if (hashtable != null)
			{
				return hashtable.Count;
			}
			return listDictionary?.Count ?? 0;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys in the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys in the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</returns>
	public ICollection Keys
	{
		get
		{
			if (hashtable != null)
			{
				return hashtable.Keys;
			}
			return List.Keys;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.HybridDictionary" /> is read-only.</summary>
	/// <returns>This property always returns false.</returns>
	public bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.HybridDictionary" /> has a fixed size.</summary>
	/// <returns>This property always returns false.</returns>
	public bool IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.HybridDictionary" /> is synchronized (thread safe).</summary>
	/// <returns>This property always returns false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</returns>
	public object SyncRoot => this;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</returns>
	public ICollection Values
	{
		get
		{
			if (hashtable != null)
			{
				return hashtable.Values;
			}
			return List.Values;
		}
	}

	/// <summary>Creates an empty case-sensitive <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	public HybridDictionary()
	{
	}

	/// <summary>Creates a case-sensitive <see cref="T:System.Collections.Specialized.HybridDictionary" /> with the specified initial size.</summary>
	/// <param name="initialSize">The approximate number of entries that the <see cref="T:System.Collections.Specialized.HybridDictionary" /> can initially contain. </param>
	public HybridDictionary(int initialSize)
		: this(initialSize, caseInsensitive: false)
	{
	}

	/// <summary>Creates an empty <see cref="T:System.Collections.Specialized.HybridDictionary" /> with the specified case sensitivity.</summary>
	/// <param name="caseInsensitive">A Boolean that denotes whether the <see cref="T:System.Collections.Specialized.HybridDictionary" /> is case-insensitive. </param>
	public HybridDictionary(bool caseInsensitive)
	{
		this.caseInsensitive = caseInsensitive;
	}

	/// <summary>Creates a <see cref="T:System.Collections.Specialized.HybridDictionary" /> with the specified initial size and case sensitivity.</summary>
	/// <param name="initialSize">The approximate number of entries that the <see cref="T:System.Collections.Specialized.HybridDictionary" /> can initially contain. </param>
	/// <param name="caseInsensitive">A Boolean that denotes whether the <see cref="T:System.Collections.Specialized.HybridDictionary" /> is case-insensitive. </param>
	public HybridDictionary(int initialSize, bool caseInsensitive)
	{
		this.caseInsensitive = caseInsensitive;
		if (initialSize >= 6)
		{
			if (caseInsensitive)
			{
				hashtable = new Hashtable(initialSize, StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				hashtable = new Hashtable(initialSize);
			}
		}
	}

	private void ChangeOver()
	{
		IDictionaryEnumerator enumerator = list.GetEnumerator();
		Hashtable hashtable = ((!caseInsensitive) ? new Hashtable(13) : new Hashtable(13, StringComparer.OrdinalIgnoreCase));
		while (enumerator.MoveNext())
		{
			hashtable.Add(enumerator.Key, enumerator.Value);
		}
		this.hashtable = hashtable;
		list = null;
	}

	/// <summary>Adds an entry with the specified key and value into the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <param name="key">The key of the entry to add. </param>
	/// <param name="value">The value of the entry to add. The value can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">An entry with the same key already exists in the <see cref="T:System.Collections.Specialized.HybridDictionary" />. </exception>
	public void Add(object key, object value)
	{
		if (hashtable != null)
		{
			hashtable.Add(key, value);
		}
		else if (list == null)
		{
			list = new ListDictionary(caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
			list.Add(key, value);
		}
		else if (list.Count + 1 >= 9)
		{
			ChangeOver();
			hashtable.Add(key, value);
		}
		else
		{
			list.Add(key, value);
		}
	}

	/// <summary>Removes all entries from the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	public void Clear()
	{
		if (hashtable != null)
		{
			Hashtable obj = hashtable;
			hashtable = null;
			obj.Clear();
		}
		if (list != null)
		{
			ListDictionary listDictionary = list;
			list = null;
			listDictionary.Clear();
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Specialized.HybridDictionary" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.HybridDictionary" /> contains an entry with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Specialized.HybridDictionary" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public bool Contains(object key)
	{
		ListDictionary listDictionary = list;
		if (hashtable != null)
		{
			return hashtable.Contains(key);
		}
		if (listDictionary != null)
		{
			return listDictionary.Contains(key);
		}
		if (key == null)
		{
			throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
		}
		return false;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Specialized.HybridDictionary" /> entries to a one-dimensional <see cref="T:System.Array" /> instance at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the <see cref="T:System.Collections.DictionaryEntry" /> objects copied from <see cref="T:System.Collections.Specialized.HybridDictionary" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Specialized.HybridDictionary" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Specialized.HybridDictionary" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	public void CopyTo(Array array, int index)
	{
		if (hashtable != null)
		{
			hashtable.CopyTo(array, index);
		}
		else
		{
			List.CopyTo(array, index);
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> that iterates through the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</returns>
	public IDictionaryEnumerator GetEnumerator()
	{
		if (hashtable != null)
		{
			return hashtable.GetEnumerator();
		}
		if (list == null)
		{
			list = new ListDictionary(caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
		}
		return list.GetEnumerator();
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that iterates through the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		if (hashtable != null)
		{
			return hashtable.GetEnumerator();
		}
		if (list == null)
		{
			list = new ListDictionary(caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
		}
		return list.GetEnumerator();
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Collections.Specialized.HybridDictionary" />.</summary>
	/// <param name="key">The key of the entry to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public void Remove(object key)
	{
		if (hashtable != null)
		{
			hashtable.Remove(key);
		}
		else if (list != null)
		{
			list.Remove(key);
		}
		else if (key == null)
		{
			throw new ArgumentNullException("key", global::SR.GetString("Key cannot be null."));
		}
	}
}
