using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Markup.Localizer;

/// <summary>Contains all the localizable resources in a BAML record. </summary>
public sealed class BamlLocalizationDictionary : IDictionary, ICollection, IEnumerable
{
	private IDictionary<BamlLocalizableResourceKey, BamlLocalizableResource> _dictionary;

	private BamlLocalizableResourceKey _rootElementKey;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object has a fixed size. </summary>
	/// <returns>Always returns false.</returns>
	public bool IsFixedSize => false;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object is read-only. </summary>
	/// <returns>Always returns false.</returns>
	public bool IsReadOnly => false;

	/// <summary>Gets the key of the root element, if it is localizable. </summary>
	/// <returns>The key of the root element, if it is localizable. Otherwise, the value is set to null.</returns>
	public BamlLocalizableResourceKey RootElementKey => _rootElementKey;

	/// <summary>Gets a collection that contains all the keys in the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object. </summary>
	/// <returns>A collection that contains all the keys in the object.</returns>
	public ICollection Keys => ((IDictionary)_dictionary).Keys;

	/// <summary>Gets a collection that contains all the values in the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />. </summary>
	/// <returns>A collection that contains all the values in the object.</returns>
	public ICollection Values => ((IDictionary)_dictionary).Values;

	/// <summary>Gets or sets a localizable resource specified by its key.</summary>
	/// <returns>The value of the resource.</returns>
	/// <param name="key">The key value of the resource.</param>
	public BamlLocalizableResource this[BamlLocalizableResourceKey key]
	{
		get
		{
			CheckNonNullParam(key, "key");
			return _dictionary[key];
		}
		set
		{
			CheckNonNullParam(key, "key");
			_dictionary[key] = value;
		}
	}

	/// <summary>Gets the number of localizable resources in the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</summary>
	/// <returns>The number of localizable resources.</returns>
	public int Count => _dictionary.Count;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Item(System.Object)" />.</summary>
	/// <returns>The item with the specified key.</returns>
	/// <param name="key">The key of the item to get or set. </param>
	object IDictionary.this[object key]
	{
		get
		{
			CheckNonNullParam(key, "key");
			return ((IDictionary)_dictionary)[key];
		}
		set
		{
			CheckNonNullParam(key, "key");
			((IDictionary)_dictionary)[key] = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.Count" />.</summary>
	/// <returns>The number of items in the collection.</returns>
	int ICollection.Count => Count;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</returns>
	object ICollection.SyncRoot => ((IDictionary)_dictionary).SyncRoot;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => ((IDictionary)_dictionary).IsSynchronized;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> class.</summary>
	public BamlLocalizationDictionary()
	{
		_dictionary = new Dictionary<BamlLocalizableResourceKey, BamlLocalizableResource>();
	}

	/// <summary>Adds an item with the provided key and value to the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</summary>
	/// <param name="key">A key for the resource.</param>
	/// <param name="value">An object that contains the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An item with the same key already exists.</exception>
	public void Add(BamlLocalizableResourceKey key, BamlLocalizableResource value)
	{
		CheckNonNullParam(key, "key");
		_dictionary.Add(key, value);
	}

	/// <summary>Deletes all resources from the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object.</summary>
	public void Clear()
	{
		_dictionary.Clear();
	}

	/// <summary>Removes a specified localizable resource from the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</summary>
	/// <param name="key">The key for the resource to be removed.</param>
	/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
	public void Remove(BamlLocalizableResourceKey key)
	{
		_dictionary.Remove(key);
	}

	/// <summary>Determines whether a <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object contains a resource with a specified key.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object contains a resource with the specified key; otherwise, false.</returns>
	/// <param name="key">The resource key to find.</param>
	/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
	public bool Contains(BamlLocalizableResourceKey key)
	{
		CheckNonNullParam(key, "key");
		return _dictionary.ContainsKey(key);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />. </summary>
	/// <returns>A specialized <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionaryEnumerator" /> that can iterate the contents of the dictionary. </returns>
	public BamlLocalizationDictionaryEnumerator GetEnumerator()
	{
		return new BamlLocalizationDictionaryEnumerator(((IDictionary)_dictionary).GetEnumerator());
	}

	/// <summary>Copies the contents of a <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object to a one-dimensional array of <see cref="T:System.Collections.DictionaryEntry" /> objects, starting at a specified index. </summary>
	/// <param name="array">An array of objects to hold the data.</param>
	/// <param name="arrayIndex">The starting index value.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="arrayIndex" /> exceeds the destination array length.-or-Copy cannot fit in the remaining array space between <paramref name="arrayIndex" /> and the destination array length.</exception>
	public void CopyTo(DictionaryEntry[] array, int arrayIndex)
	{
		CheckNonNullParam(array, "array");
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", SR.ParameterCannotBeNegative);
		}
		if (arrayIndex >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "arrayIndex", "array"), "arrayIndex");
		}
		if (Count > array.Length - arrayIndex)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, "arrayIndex", "array"));
		}
		foreach (KeyValuePair<BamlLocalizableResourceKey, BamlLocalizableResource> item in _dictionary)
		{
			DictionaryEntry dictionaryEntry = new DictionaryEntry(item.Key, item.Value);
			array[arrayIndex++] = dictionaryEntry;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />; otherwise, false.</returns>
	/// <param name="key">The key  to locate in the dictionary.</param>
	bool IDictionary.Contains(object key)
	{
		CheckNonNullParam(key, "key");
		return ((IDictionary)_dictionary).Contains(key);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Add(System.Object,System.Object)" />.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The object value to add to the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</param>
	void IDictionary.Add(object key, object value)
	{
		CheckNonNullParam(key, "key");
		((IDictionary)_dictionary).Add(key, value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Remove(System.Object)" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</param>
	void IDictionary.Remove(object key)
	{
		CheckNonNullParam(key, "key");
		((IDictionary)_dictionary).Remove(key);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.GetEnumerator" />.</summary>
	/// <returns>An enumerator object that can be used to iterate through the collection.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array != null && array.Rank != 1)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_ArrayCannotBeMultidimensional), "array");
		}
		CopyTo(array as DictionaryEntry[], index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal BamlLocalizationDictionary Copy()
	{
		BamlLocalizationDictionary bamlLocalizationDictionary = new BamlLocalizationDictionary();
		foreach (KeyValuePair<BamlLocalizableResourceKey, BamlLocalizableResource> item in _dictionary)
		{
			BamlLocalizableResource value = ((item.Value == null) ? null : new BamlLocalizableResource(item.Value));
			bamlLocalizationDictionary.Add(item.Key, value);
		}
		bamlLocalizationDictionary._rootElementKey = _rootElementKey;
		return bamlLocalizationDictionary;
	}

	internal void SetRootElementKey(BamlLocalizableResourceKey key)
	{
		_rootElementKey = key;
	}

	private void CheckNonNullParam(object param, string paramName)
	{
		if (param == null)
		{
			throw new ArgumentNullException(paramName);
		}
	}
}
