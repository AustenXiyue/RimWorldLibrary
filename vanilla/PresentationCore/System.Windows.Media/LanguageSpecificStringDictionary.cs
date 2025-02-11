using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a dictionary of strings that are used to represent the name of an object in different languages.</summary>
public sealed class LanguageSpecificStringDictionary : IDictionary<XmlLanguage, string>, ICollection<KeyValuePair<XmlLanguage, string>>, IEnumerable<KeyValuePair<XmlLanguage, string>>, IEnumerable, IDictionary, ICollection
{
	private class EntryEnumerator : IDictionaryEnumerator, IEnumerator
	{
		protected IDictionary<XmlLanguage, string> _innerDictionary;

		protected IEnumerator<KeyValuePair<XmlLanguage, string>> _enumerator;

		public virtual object Current => Entry;

		public DictionaryEntry Entry
		{
			get
			{
				KeyValuePair<XmlLanguage, string> currentEntry = GetCurrentEntry();
				return new DictionaryEntry(currentEntry.Key, currentEntry.Value);
			}
		}

		public object Key => GetCurrentEntry().Key;

		public object Value => GetCurrentEntry().Value;

		internal EntryEnumerator(IDictionary<XmlLanguage, string> names)
		{
			_innerDictionary = names;
			_enumerator = names.GetEnumerator();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator = _innerDictionary.GetEnumerator();
		}

		private KeyValuePair<XmlLanguage, string> GetCurrentEntry()
		{
			KeyValuePair<XmlLanguage, string> current = _enumerator.Current;
			if (current.Key == null)
			{
				throw new InvalidOperationException(SR.Enumerator_VerifyContext);
			}
			return current;
		}
	}

	private abstract class BaseCollection : ICollection, IEnumerable
	{
		protected IDictionary<XmlLanguage, string> _innerDictionary;

		public int Count => _innerDictionary.Count;

		public bool IsSynchronized => false;

		public object SyncRoot => _innerDictionary;

		internal BaseCollection(IDictionary<XmlLanguage, string> names)
		{
			_innerDictionary = names;
		}

		public void CopyTo(Array array, int index)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					array.SetValue(current, index++);
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public abstract IEnumerator GetEnumerator();
	}

	private class KeyCollection : BaseCollection
	{
		private class KeyEnumerator : EntryEnumerator
		{
			public override object Current => base.Key;

			internal KeyEnumerator(IDictionary<XmlLanguage, string> names)
				: base(names)
			{
			}
		}

		internal KeyCollection(IDictionary<XmlLanguage, string> names)
			: base(names)
		{
		}

		public override IEnumerator GetEnumerator()
		{
			return new KeyEnumerator(_innerDictionary);
		}
	}

	private class ValueCollection : BaseCollection
	{
		private class ValueEnumerator : EntryEnumerator
		{
			public override object Current => base.Value;

			internal ValueEnumerator(IDictionary<XmlLanguage, string> names)
				: base(names)
			{
			}
		}

		internal ValueCollection(IDictionary<XmlLanguage, string> names)
			: base(names)
		{
		}

		public override IEnumerator GetEnumerator()
		{
			return new ValueEnumerator(_innerDictionary);
		}
	}

	private IDictionary<XmlLanguage, string> _innerDictionary;

	/// <summary>Gets the number of strings in the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</summary>
	/// <returns>A value of type <see cref="T:System.Int32" /> representing the total number of strings.</returns>
	public int Count => _innerDictionary.Count;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> is read-only.</summary>
	/// <returns>true if the dictionary is read-only; otherwise, false.</returns>
	public bool IsReadOnly => _innerDictionary.IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</returns>
	object ICollection.SyncRoot => _innerDictionary;

	/// <summary>Gets or sets the string associated with the specified language.</summary>
	/// <returns>A value of type <see cref="T:System.String" />.</returns>
	/// <param name="key">A value of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	public string this[XmlLanguage key]
	{
		get
		{
			return _innerDictionary[key];
		}
		set
		{
			_innerDictionary[key] = ValidateValue(value);
		}
	}

	/// <summary>Gets a collection containing the keys, or <see cref="T:System.Windows.Markup.XmlLanguage" /> objects, in the dictionary.</summary>
	/// <returns>A collection of objects of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</returns>
	[CLSCompliant(false)]
	public ICollection<XmlLanguage> Keys => _innerDictionary.Keys;

	/// <summary>Gets a collection containing the values, or strings, in the dictionary.</summary>
	/// <returns>A collection containing the strings in the dictionary.</returns>
	[CLSCompliant(false)]
	public ICollection<string> Values => _innerDictionary.Values;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> has a fixed size; otherwise, false.</returns>
	bool IDictionary.IsFixedSize => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Item(System.Object)" />.</summary>
	/// <returns>The element with the specified key.</returns>
	/// <param name="key">The key of the element to get or set. </param>
	object IDictionary.this[object key]
	{
		get
		{
			XmlLanguage xmlLanguage = TryConvertKey(key);
			if (xmlLanguage == null)
			{
				return null;
			}
			return _innerDictionary[xmlLanguage];
		}
		set
		{
			_innerDictionary[ConvertKey(key)] = ConvertValue(value);
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Keys" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the keys of the <see cref="T:System.Collections.IDictionary" /> object.</returns>
	ICollection IDictionary.Keys => new KeyCollection(_innerDictionary);

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Values" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the values in the <see cref="T:System.Collections.IDictionary" /> object.</returns>
	ICollection IDictionary.Values => new ValueCollection(_innerDictionary);

	internal LanguageSpecificStringDictionary(IDictionary<XmlLanguage, string> innerDictionary)
	{
		_innerDictionary = innerDictionary;
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that iterates through the collection.</returns>
	[CLSCompliant(false)]
	public IEnumerator<KeyValuePair<XmlLanguage, string>> GetEnumerator()
	{
		return _innerDictionary.GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new EntryEnumerator(_innerDictionary);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new EntryEnumerator(_innerDictionary);
	}

	/// <summary>Retrieves the string value in the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> for a specified key, or language.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> contains an entry for <paramref name="key" />; otherwise false.</returns>
	/// <param name="key">A value of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	/// <param name="value">A value of type <see cref="T:System.String" />.</param>
	public bool TryGetValue(XmlLanguage key, out string value)
	{
		return _innerDictionary.TryGetValue(key, out value);
	}

	/// <summary>Adds a key/value pair to the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</summary>
	/// <param name="item">An array of key/value pairs. The key is an object of type <see cref="T:System.Windows.Markup.XmlLanguage" />. The value is an associated string.</param>
	[CLSCompliant(false)]
	public void Add(KeyValuePair<XmlLanguage, string> item)
	{
		Add(item.Key, item.Value);
	}

	/// <summary>Removes all elements from the collection.</summary>
	public void Clear()
	{
		_innerDictionary.Clear();
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> contains the key/value pair.</summary>
	/// <returns>true if the collection contains the key/value pair; otherwise, false.</returns>
	/// <param name="item">The key/pair to locate. The key is an object of type <see cref="T:System.Windows.Markup.XmlLanguage" />. The value is an associated string.</param>
	[CLSCompliant(false)]
	public bool Contains(KeyValuePair<XmlLanguage, string> item)
	{
		return _innerDictionary.Contains(item);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> to an array, starting at a specific array index.</summary>
	/// <param name="array">The destination array to copy to.</param>
	/// <param name="index">The index within the source <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> to start copying from.</param>
	[CLSCompliant(false)]
	public void CopyTo(KeyValuePair<XmlLanguage, string>[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (index >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "index", "array"));
		}
		if (_innerDictionary.Count > array.Length - index)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, index, "array"));
		}
		_innerDictionary.CopyTo(array, index);
	}

	/// <summary>Removes the element with the specified key/value pair from the collection.</summary>
	/// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> was not found in the original collection.</returns>
	/// <param name="item">The key/value pair of the element to remove.</param>
	[CLSCompliant(false)]
	public bool Remove(KeyValuePair<XmlLanguage, string> item)
	{
		return _innerDictionary.Remove(item);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
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
		if (index >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "index", "array"));
		}
		if (_innerDictionary.Count > array.Length - index)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, index, "array"));
		}
		if (array is DictionaryEntry[] array2)
		{
			{
				foreach (KeyValuePair<XmlLanguage, string> item in _innerDictionary)
				{
					array2[index++] = new DictionaryEntry(item.Key, item.Value);
				}
				return;
			}
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_CopyTo_ArrayCannotBeMultidimensional);
		}
		Type elementType = array.GetType().GetElementType();
		if (!elementType.IsAssignableFrom(typeof(DictionaryEntry)))
		{
			throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(DictionaryEntry), elementType));
		}
		foreach (KeyValuePair<XmlLanguage, string> item2 in _innerDictionary)
		{
			array.SetValue(new DictionaryEntry(item2.Key, item2.Value), index++);
		}
	}

	/// <summary>Adds a language and associated string to the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</summary>
	/// <param name="key">A value of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	/// <param name="value">A value of type <see cref="T:System.String" />.</param>
	public void Add(XmlLanguage key, string value)
	{
		_innerDictionary.Add(key, ValidateValue(value));
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> contains the specified language.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> contains <paramref name="key" />; otherwise, false.</returns>
	/// <param name="key">A value of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	public bool ContainsKey(XmlLanguage key)
	{
		return _innerDictionary.ContainsKey(key);
	}

	/// <summary>Removes the element from <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> based on the specified key value.</summary>
	/// <returns>true if the element referenced by <paramref name="key" /> was successfully deleted; otherwise false.</returns>
	/// <param name="key">A value of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	public bool Remove(XmlLanguage key)
	{
		return _innerDictionary.Remove(key);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Add(System.Object,System.Object)" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to use as the key of the element to add.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</param>
	void IDictionary.Add(object key, object value)
	{
		_innerDictionary.Add(ConvertKey(key), ConvertValue(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />; otherwise, false.</returns>
	/// <param name="key">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</param>
	bool IDictionary.Contains(object key)
	{
		XmlLanguage xmlLanguage = TryConvertKey(key);
		if (xmlLanguage == null)
		{
			return false;
		}
		return _innerDictionary.ContainsKey(xmlLanguage);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Remove(System.Object)" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" />.</param>
	void IDictionary.Remove(object key)
	{
		XmlLanguage xmlLanguage = TryConvertKey(key);
		if (xmlLanguage != null)
		{
			_innerDictionary.Remove(xmlLanguage);
		}
	}

	private string ValidateValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return value;
	}

	private string ConvertValue(object value)
	{
		string obj = value as string;
		if (obj == null)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(string)), "value");
		}
		return obj;
	}

	private XmlLanguage ConvertKey(object key)
	{
		XmlLanguage xmlLanguage = TryConvertKey(key);
		if (xmlLanguage == null)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			throw new ArgumentException(SR.Format(SR.CannotConvertType, key.GetType(), typeof(XmlLanguage)), "key");
		}
		return xmlLanguage;
	}

	private XmlLanguage TryConvertKey(object key)
	{
		if (key is XmlLanguage result)
		{
			return result;
		}
		if (key is string ietfLanguageTag)
		{
			return XmlLanguage.GetLanguage(ietfLanguageTag);
		}
		return null;
	}
}
