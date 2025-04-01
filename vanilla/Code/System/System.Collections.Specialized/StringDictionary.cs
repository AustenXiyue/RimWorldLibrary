using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Collections.Specialized;

/// <summary>Implements a hash table with the key and the value strongly typed to be strings rather than objects.</summary>
[Serializable]
[DesignerSerializer("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
public class StringDictionary : IEnumerable
{
	private class GenericAdapter : IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable
	{
		internal enum KeyOrValue
		{
			Key,
			Value
		}

		private class ICollectionToGenericCollectionAdapter : ICollection<string>, IEnumerable<string>, IEnumerable
		{
			private StringDictionary _internal;

			private KeyOrValue _keyOrValue;

			public int Count => _internal.Count;

			public bool IsReadOnly => true;

			public ICollectionToGenericCollectionAdapter(StringDictionary source, KeyOrValue keyOrValue)
			{
				if (source == null)
				{
					throw new ArgumentNullException("source");
				}
				_internal = source;
				_keyOrValue = keyOrValue;
			}

			public void Add(string item)
			{
				ThrowNotSupportedException();
			}

			public void Clear()
			{
				ThrowNotSupportedException();
			}

			public void ThrowNotSupportedException()
			{
				if (_keyOrValue == KeyOrValue.Key)
				{
					throw new NotSupportedException(global::SR.GetString("Mutating a key collection derived from a dictionary is not allowed."));
				}
				throw new NotSupportedException(global::SR.GetString("Mutating a value collection derived from a dictionary is not allowed."));
			}

			public bool Contains(string item)
			{
				if (_keyOrValue == KeyOrValue.Key)
				{
					return _internal.ContainsKey(item);
				}
				return _internal.ContainsValue(item);
			}

			public void CopyTo(string[] array, int arrayIndex)
			{
				GetUnderlyingCollection().CopyTo(array, arrayIndex);
			}

			public bool Remove(string item)
			{
				ThrowNotSupportedException();
				return false;
			}

			private ICollection GetUnderlyingCollection()
			{
				if (_keyOrValue == KeyOrValue.Key)
				{
					return _internal.Keys;
				}
				return _internal.Values;
			}

			public IEnumerator<string> GetEnumerator()
			{
				ICollection underlyingCollection = GetUnderlyingCollection();
				foreach (string item in underlyingCollection)
				{
					yield return item;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetUnderlyingCollection().GetEnumerator();
			}
		}

		private StringDictionary m_stringDictionary;

		private ICollectionToGenericCollectionAdapter _values;

		private ICollectionToGenericCollectionAdapter _keys;

		public int Count => m_stringDictionary.Count;

		public string this[string key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				if (!m_stringDictionary.ContainsKey(key))
				{
					throw new KeyNotFoundException();
				}
				return m_stringDictionary[key];
			}
			set
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				m_stringDictionary[key] = value;
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				if (_keys == null)
				{
					_keys = new ICollectionToGenericCollectionAdapter(m_stringDictionary, KeyOrValue.Key);
				}
				return _keys;
			}
		}

		public ICollection<string> Values
		{
			get
			{
				if (_values == null)
				{
					_values = new ICollectionToGenericCollectionAdapter(m_stringDictionary, KeyOrValue.Value);
				}
				return _values;
			}
		}

		bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

		internal GenericAdapter(StringDictionary stringDictionary)
		{
			m_stringDictionary = stringDictionary;
		}

		public void Add(string key, string value)
		{
			this[key] = value;
		}

		public bool ContainsKey(string key)
		{
			return m_stringDictionary.ContainsKey(key);
		}

		public void Clear()
		{
			m_stringDictionary.Clear();
		}

		public bool Remove(string key)
		{
			if (!m_stringDictionary.ContainsKey(key))
			{
				return false;
			}
			m_stringDictionary.Remove(key);
			return true;
		}

		public bool TryGetValue(string key, out string value)
		{
			if (!m_stringDictionary.ContainsKey(key))
			{
				value = null;
				return false;
			}
			value = m_stringDictionary[key];
			return true;
		}

		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
		{
			m_stringDictionary.Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
		{
			if (TryGetValue(item.Key, out var value))
			{
				return value.Equals(item.Value);
			}
			return false;
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", global::SR.GetString("Array cannot be null."));
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", global::SR.GetString("Non-negative number required."));
			}
			if (array.Length - arrayIndex < Count)
			{
				throw new ArgumentException(global::SR.GetString("Destination array is not long enough to copy all the items in the collection. Check array index and length."));
			}
			int num = arrayIndex;
			foreach (DictionaryEntry item in m_stringDictionary)
			{
				array[num++] = new KeyValuePair<string, string>((string)item.Key, (string)item.Value);
			}
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			if (!((ICollection<KeyValuePair<string, string>>)this).Contains(item))
			{
				return false;
			}
			m_stringDictionary.Remove(item.Key);
			return true;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			foreach (DictionaryEntry item in m_stringDictionary)
			{
				yield return new KeyValuePair<string, string>((string)item.Key, (string)item.Value);
			}
		}
	}

	internal Hashtable contents = new Hashtable();

	/// <summary>Gets the number of key/value pairs in the <see cref="T:System.Collections.Specialized.StringDictionary" />.</summary>
	/// <returns>The number of key/value pairs in the <see cref="T:System.Collections.Specialized.StringDictionary" />.Retrieving the value of this property is an O(1) operation.</returns>
	public virtual int Count => contents.Count;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Specialized.StringDictionary" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.Specialized.StringDictionary" /> is synchronized (thread safe); otherwise, false.</returns>
	public virtual bool IsSynchronized => contents.IsSynchronized;

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, Get returns null, and Set creates a new entry with the specified key.</returns>
	/// <param name="key">The key whose value to get or set. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public virtual string this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			return (string)contents[key.ToLower(CultureInfo.InvariantCulture)];
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			contents[key.ToLower(CultureInfo.InvariantCulture)] = value;
		}
	}

	/// <summary>Gets a collection of keys in the <see cref="T:System.Collections.Specialized.StringDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that provides the keys in the <see cref="T:System.Collections.Specialized.StringDictionary" />.</returns>
	public virtual ICollection Keys => contents.Keys;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.StringDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that can be used to synchronize access to the <see cref="T:System.Collections.Specialized.StringDictionary" />.</returns>
	public virtual object SyncRoot => contents.SyncRoot;

	/// <summary>Gets a collection of values in the <see cref="T:System.Collections.Specialized.StringDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that provides the values in the <see cref="T:System.Collections.Specialized.StringDictionary" />.</returns>
	public virtual ICollection Values => contents.Values;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.StringDictionary" /> class.</summary>
	public StringDictionary()
	{
	}

	/// <summary>Adds an entry with the specified key and value into the <see cref="T:System.Collections.Specialized.StringDictionary" />.</summary>
	/// <param name="key">The key of the entry to add. </param>
	/// <param name="value">The value of the entry to add. The value can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">An entry with the same key already exists in the <see cref="T:System.Collections.Specialized.StringDictionary" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.StringDictionary" /> is read-only. </exception>
	public virtual void Add(string key, string value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		contents.Add(key.ToLower(CultureInfo.InvariantCulture), value);
	}

	/// <summary>Removes all entries from the <see cref="T:System.Collections.Specialized.StringDictionary" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.StringDictionary" /> is read-only. </exception>
	public virtual void Clear()
	{
		contents.Clear();
	}

	/// <summary>Determines if the <see cref="T:System.Collections.Specialized.StringDictionary" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.StringDictionary" /> contains an entry with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Specialized.StringDictionary" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The key is null. </exception>
	public virtual bool ContainsKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return contents.ContainsKey(key.ToLower(CultureInfo.InvariantCulture));
	}

	/// <summary>Determines if the <see cref="T:System.Collections.Specialized.StringDictionary" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.StringDictionary" /> contains an element with the specified value; otherwise, false.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.Specialized.StringDictionary" />. The value can be null. </param>
	public virtual bool ContainsValue(string value)
	{
		return contents.ContainsValue(value);
	}

	/// <summary>Copies the string dictionary values to a one-dimensional <see cref="T:System.Array" /> instance at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the <see cref="T:System.Collections.Specialized.StringDictionary" />. </param>
	/// <param name="index">The index in the array where copying begins. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the <see cref="T:System.Collections.Specialized.StringDictionary" /> is greater than the available space from <paramref name="index" /> to the end of <paramref name="array" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />. </exception>
	public virtual void CopyTo(Array array, int index)
	{
		contents.CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that iterates through the string dictionary.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that iterates through the string dictionary.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		return contents.GetEnumerator();
	}

	/// <summary>Removes the entry with the specified key from the string dictionary.</summary>
	/// <param name="key">The key of the entry to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">The key is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Specialized.StringDictionary" /> is read-only. </exception>
	public virtual void Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		contents.Remove(key.ToLower(CultureInfo.InvariantCulture));
	}

	internal void ReplaceHashtable(Hashtable useThisHashtableInstead)
	{
		contents = useThisHashtableInstead;
	}

	internal IDictionary<string, string> AsGenericDictionary()
	{
		return new GenericAdapter(this);
	}
}
