using System.Collections;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Represents a collection of <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects.</summary>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public class PropertyDescriptorCollection : ICollection, IEnumerable, IList, IDictionary
{
	private class PropertyDescriptorEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private PropertyDescriptorCollection owner;

		private int index = -1;

		public object Current => Entry;

		public DictionaryEntry Entry
		{
			get
			{
				PropertyDescriptor propertyDescriptor = owner[index];
				return new DictionaryEntry(propertyDescriptor.Name, propertyDescriptor);
			}
		}

		public object Key => owner[index].Name;

		public object Value => owner[index].Name;

		public PropertyDescriptorEnumerator(PropertyDescriptorCollection owner)
		{
			this.owner = owner;
		}

		public bool MoveNext()
		{
			if (index < owner.Count - 1)
			{
				index++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			index = -1;
		}
	}

	/// <summary>Specifies an empty collection that you can use instead of creating a new one with no items. This static field is read-only.</summary>
	public static readonly PropertyDescriptorCollection Empty = new PropertyDescriptorCollection(null, readOnly: true);

	private IDictionary cachedFoundProperties;

	private bool cachedIgnoreCase;

	private PropertyDescriptor[] properties;

	private int propCount;

	private string[] namedSort;

	private IComparer comparer;

	private bool propsOwned = true;

	private bool needSort;

	private bool readOnly;

	/// <summary>Gets the number of property descriptors in the collection.</summary>
	/// <returns>The number of property descriptors in the collection.</returns>
	public int Count => propCount;

	/// <summary>Gets or sets the <see cref="T:System.ComponentModel.PropertyDescriptor" /> at the specified index number.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor" /> with the specified index number.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to get or set. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="index" /> parameter is not a valid index for <see cref="P:System.ComponentModel.PropertyDescriptorCollection.Item(System.Int32)" />. </exception>
	public virtual PropertyDescriptor this[int index]
	{
		get
		{
			if (index >= propCount)
			{
				throw new IndexOutOfRangeException();
			}
			EnsurePropsOwned();
			return properties[index];
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.ComponentModel.PropertyDescriptor" /> with the specified name.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor" /> with the specified name, or null if the property does not exist.</returns>
	/// <param name="name">The name of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to get from the collection. </param>
	public virtual PropertyDescriptor this[string name] => Find(name, ignoreCase: false);

	/// <summary>Gets the number of elements contained in the collection.</summary>
	/// <returns>The number of elements contained in the collection.</returns>
	int ICollection.Count => Count;

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the collection is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	object ICollection.SyncRoot => null;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> has a fixed size; otherwise, false.</returns>
	bool IDictionary.IsFixedSize => readOnly;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> is read-only; otherwise, false.</returns>
	bool IDictionary.IsReadOnly => readOnly;

	/// <summary>Gets or sets the element with the specified key. </summary>
	/// <returns>The element with the specified key.</returns>
	/// <param name="key">The key of the element to get or set. </param>
	object IDictionary.this[object key]
	{
		get
		{
			if (key is string)
			{
				return this[(string)key];
			}
			return null;
		}
		set
		{
			if (readOnly)
			{
				throw new NotSupportedException();
			}
			if (value != null && !(value is PropertyDescriptor))
			{
				throw new ArgumentException("value");
			}
			int num = -1;
			if (key is int)
			{
				num = (int)key;
				if (num < 0 || num >= propCount)
				{
					throw new IndexOutOfRangeException();
				}
			}
			else
			{
				if (!(key is string))
				{
					throw new ArgumentException("key");
				}
				for (int i = 0; i < propCount; i++)
				{
					if (properties[i].Name.Equals((string)key))
					{
						num = i;
						break;
					}
				}
			}
			if (num == -1)
			{
				Add((PropertyDescriptor)value);
				return;
			}
			EnsurePropsOwned();
			properties[num] = (PropertyDescriptor)value;
			if (cachedFoundProperties != null && key is string)
			{
				cachedFoundProperties[key] = value;
			}
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Keys
	{
		get
		{
			string[] array = new string[propCount];
			for (int i = 0; i < propCount; i++)
			{
				array[i] = properties[i].Name;
			}
			return array;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Values
	{
		get
		{
			if (properties.Length != propCount)
			{
				PropertyDescriptor[] array = new PropertyDescriptor[propCount];
				Array.Copy(properties, 0, array, 0, propCount);
				return array;
			}
			return (ICollection)properties.Clone();
		}
	}

	/// <summary>Gets a value indicating whether the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => readOnly;

	/// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
	/// <returns>true if the collection has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => readOnly;

	/// <summary>Gets or sets an item from the collection at a specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the item to get or set.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.ComponentModel.PropertyDescriptor" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is less than 0. -or-<paramref name="index" /> is equal to or greater than <see cref="P:System.ComponentModel.EventDescriptorCollection.Count" />.</exception>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			if (readOnly)
			{
				throw new NotSupportedException();
			}
			if (index >= propCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (value != null && !(value is PropertyDescriptor))
			{
				throw new ArgumentException("value");
			}
			EnsurePropsOwned();
			properties[index] = (PropertyDescriptor)value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> class.</summary>
	/// <param name="properties">An array of type <see cref="T:System.ComponentModel.PropertyDescriptor" /> that provides the properties for this collection. </param>
	public PropertyDescriptorCollection(PropertyDescriptor[] properties)
	{
		this.properties = properties;
		if (properties == null)
		{
			this.properties = new PropertyDescriptor[0];
			propCount = 0;
		}
		else
		{
			propCount = properties.Length;
		}
		propsOwned = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> class, which is optionally read-only.</summary>
	/// <param name="properties">An array of type <see cref="T:System.ComponentModel.PropertyDescriptor" /> that provides the properties for this collection.</param>
	/// <param name="readOnly">If true, specifies that the collection cannot be modified.</param>
	public PropertyDescriptorCollection(PropertyDescriptor[] properties, bool readOnly)
		: this(properties)
	{
		this.readOnly = readOnly;
	}

	private PropertyDescriptorCollection(PropertyDescriptor[] properties, int propCount, string[] namedSort, IComparer comparer)
	{
		propsOwned = false;
		if (namedSort != null)
		{
			this.namedSort = (string[])namedSort.Clone();
		}
		this.comparer = comparer;
		this.properties = properties;
		this.propCount = propCount;
		needSort = true;
	}

	/// <summary>Adds the specified <see cref="T:System.ComponentModel.PropertyDescriptor" /> to the collection.</summary>
	/// <returns>The index of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> that was added to the collection.</returns>
	/// <param name="value">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to add to the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public int Add(PropertyDescriptor value)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		EnsureSize(propCount + 1);
		properties[propCount++] = value;
		return propCount - 1;
	}

	/// <summary>Removes all <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Clear()
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		propCount = 0;
		cachedFoundProperties = null;
	}

	/// <summary>Returns whether the collection contains the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.</summary>
	/// <returns>true if the collection contains the given <see cref="T:System.ComponentModel.PropertyDescriptor" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to find in the collection. </param>
	public bool Contains(PropertyDescriptor value)
	{
		return IndexOf(value) >= 0;
	}

	/// <summary>Copies the entire collection to an array, starting at the specified index number.</summary>
	/// <param name="array">An array of <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects to copy elements of the collection to. </param>
	/// <param name="index">The index of the <paramref name="array" /> parameter at which copying begins. </param>
	public void CopyTo(Array array, int index)
	{
		EnsurePropsOwned();
		Array.Copy(properties, 0, array, index, Count);
	}

	private void EnsurePropsOwned()
	{
		if (!propsOwned)
		{
			propsOwned = true;
			if (properties != null)
			{
				PropertyDescriptor[] destinationArray = new PropertyDescriptor[Count];
				Array.Copy(properties, 0, destinationArray, 0, Count);
				properties = destinationArray;
			}
		}
		if (needSort)
		{
			needSort = false;
			InternalSort(namedSort);
		}
	}

	private void EnsureSize(int sizeNeeded)
	{
		if (sizeNeeded > properties.Length)
		{
			if (properties == null || properties.Length == 0)
			{
				propCount = 0;
				properties = new PropertyDescriptor[sizeNeeded];
				return;
			}
			EnsurePropsOwned();
			PropertyDescriptor[] destinationArray = new PropertyDescriptor[Math.Max(sizeNeeded, properties.Length * 2)];
			Array.Copy(properties, 0, destinationArray, 0, propCount);
			properties = destinationArray;
		}
	}

	/// <summary>Returns the <see cref="T:System.ComponentModel.PropertyDescriptor" /> with the specified name, using a Boolean to indicate whether to ignore case.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptor" /> with the specified name, or null if the property does not exist.</returns>
	/// <param name="name">The name of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to return from the collection. </param>
	/// <param name="ignoreCase">true if you want to ignore the case of the property name; otherwise, false. </param>
	public virtual PropertyDescriptor Find(string name, bool ignoreCase)
	{
		lock (this)
		{
			PropertyDescriptor result = null;
			if (cachedFoundProperties == null || cachedIgnoreCase != ignoreCase)
			{
				cachedIgnoreCase = ignoreCase;
				cachedFoundProperties = new HybridDictionary(ignoreCase);
			}
			object obj = cachedFoundProperties[name];
			if (obj != null)
			{
				return (PropertyDescriptor)obj;
			}
			for (int i = 0; i < propCount; i++)
			{
				if (ignoreCase)
				{
					if (string.Equals(properties[i].Name, name, StringComparison.OrdinalIgnoreCase))
					{
						cachedFoundProperties[name] = properties[i];
						result = properties[i];
						break;
					}
				}
				else if (properties[i].Name.Equals(name))
				{
					cachedFoundProperties[name] = properties[i];
					result = properties[i];
					break;
				}
			}
			return result;
		}
	}

	/// <summary>Returns the index of the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.</summary>
	/// <returns>The index of the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.</returns>
	/// <param name="value">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to return the index of. </param>
	public int IndexOf(PropertyDescriptor value)
	{
		return Array.IndexOf(properties, value, 0, propCount);
	}

	/// <summary>Adds the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to the collection at the specified index number.</summary>
	/// <param name="index">The index at which to add the <paramref name="value" /> parameter to the collection. </param>
	/// <param name="value">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to add to the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Insert(int index, PropertyDescriptor value)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		EnsureSize(propCount + 1);
		if (index < propCount)
		{
			Array.Copy(properties, index, properties, index + 1, propCount - index);
		}
		properties[index] = value;
		propCount++;
	}

	/// <summary>Removes the specified <see cref="T:System.ComponentModel.PropertyDescriptor" /> from the collection.</summary>
	/// <param name="value">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to remove from the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Remove(PropertyDescriptor value)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		int num = IndexOf(value);
		if (num != -1)
		{
			RemoveAt(num);
		}
	}

	/// <summary>Removes the <see cref="T:System.ComponentModel.PropertyDescriptor" /> at the specified index from the collection.</summary>
	/// <param name="index">The index of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to remove from the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void RemoveAt(int index)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		if (index < propCount - 1)
		{
			Array.Copy(properties, index + 1, properties, index, propCount - index - 1);
		}
		properties[propCount - 1] = null;
		propCount--;
	}

	/// <summary>Sorts the members of this collection, using the default sort for this collection, which is usually alphabetical.</summary>
	/// <returns>A new <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that contains the sorted <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects.</returns>
	public virtual PropertyDescriptorCollection Sort()
	{
		return new PropertyDescriptorCollection(properties, propCount, namedSort, comparer);
	}

	/// <summary>Sorts the members of this collection. The specified order is applied first, followed by the default sort for this collection, which is usually alphabetical.</summary>
	/// <returns>A new <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that contains the sorted <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects.</returns>
	/// <param name="names">An array of strings describing the order in which to sort the <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects in this collection. </param>
	public virtual PropertyDescriptorCollection Sort(string[] names)
	{
		return new PropertyDescriptorCollection(properties, propCount, names, comparer);
	}

	/// <summary>Sorts the members of this collection. The specified order is applied first, followed by the sort using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <returns>A new <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that contains the sorted <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects.</returns>
	/// <param name="names">An array of strings describing the order in which to sort the <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects in this collection. </param>
	/// <param name="comparer">A comparer to use to sort the <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects in this collection. </param>
	public virtual PropertyDescriptorCollection Sort(string[] names, IComparer comparer)
	{
		return new PropertyDescriptorCollection(properties, propCount, names, comparer);
	}

	/// <summary>Sorts the members of this collection, using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <returns>A new <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that contains the sorted <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects.</returns>
	/// <param name="comparer">A comparer to use to sort the <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects in this collection. </param>
	public virtual PropertyDescriptorCollection Sort(IComparer comparer)
	{
		return new PropertyDescriptorCollection(properties, propCount, namedSort, comparer);
	}

	/// <summary>Sorts the members of this collection. The specified order is applied first, followed by the default sort for this collection, which is usually alphabetical.</summary>
	/// <param name="names">An array of strings describing the order in which to sort the <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects in this collection. </param>
	protected void InternalSort(string[] names)
	{
		if (properties == null || properties.Length == 0)
		{
			return;
		}
		InternalSort(comparer);
		if (names == null || names.Length == 0)
		{
			return;
		}
		ArrayList arrayList = new ArrayList(properties);
		int num = 0;
		int num2 = properties.Length;
		for (int i = 0; i < names.Length; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				PropertyDescriptor propertyDescriptor = (PropertyDescriptor)arrayList[j];
				if (propertyDescriptor != null && propertyDescriptor.Name.Equals(names[i]))
				{
					properties[num++] = propertyDescriptor;
					arrayList[j] = null;
					break;
				}
			}
		}
		for (int k = 0; k < num2; k++)
		{
			if (arrayList[k] != null)
			{
				properties[num++] = (PropertyDescriptor)arrayList[k];
			}
		}
	}

	/// <summary>Sorts the members of this collection, using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <param name="sorter">A comparer to use to sort the <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects in this collection. </param>
	protected void InternalSort(IComparer sorter)
	{
		if (sorter == null)
		{
			TypeDescriptor.SortDescriptorArray(this);
		}
		else
		{
			Array.Sort(properties, sorter);
		}
	}

	/// <summary>Returns an enumerator for this class.</summary>
	/// <returns>An enumerator of type <see cref="T:System.Collections.IEnumerator" />.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		EnsurePropsOwned();
		if (properties.Length != propCount)
		{
			PropertyDescriptor[] array = new PropertyDescriptor[propCount];
			Array.Copy(properties, 0, array, 0, propCount);
			return array.GetEnumerator();
		}
		return properties.GetEnumerator();
	}

	/// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to use as the key of the element to add.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to use as the value of the element to add.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	void IDictionary.Add(object key, object value)
	{
		if (!(value is PropertyDescriptor value2))
		{
			throw new ArgumentException("value");
		}
		Add(value2);
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.IDictionary" />. </summary>
	void IDictionary.Clear()
	{
		Clear();
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary" />.</param>
	bool IDictionary.Contains(object key)
	{
		if (key is string)
		{
			return this[(string)key] != null;
		}
		return false;
	}

	/// <summary>Returns an enumerator for this class.</summary>
	/// <returns>An enumerator of type <see cref="T:System.Collections.IEnumerator" />.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new PropertyDescriptorEnumerator(this);
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />. </summary>
	/// <param name="key">The key of the element to remove.</param>
	void IDictionary.Remove(object key)
	{
		if (key is string)
		{
			PropertyDescriptor propertyDescriptor = this[(string)key];
			if (propertyDescriptor != null)
			{
				((IList)this).Remove((object)propertyDescriptor);
			}
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The item to add to the collection.</param>
	int IList.Add(object value)
	{
		return Add((PropertyDescriptor)value);
	}

	/// <summary>Removes all items from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>Determines whether the collection contains a specific value.</summary>
	/// <returns>true if the item is found in the collection; otherwise, false.</returns>
	/// <param name="value">The item to locate in the collection.</param>
	bool IList.Contains(object value)
	{
		return Contains((PropertyDescriptor)value);
	}

	/// <summary>Determines the index of a specified item in the collection.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list, otherwise -1.</returns>
	/// <param name="value">The item to locate in the collection.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf((PropertyDescriptor)value);
	}

	/// <summary>Inserts an item into the collection at a specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The item to insert into the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.Insert(int index, object value)
	{
		Insert(index, (PropertyDescriptor)value);
	}

	/// <summary>Removes the first occurrence of a specified value from the collection.</summary>
	/// <param name="value">The item to remove from the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.Remove(object value)
	{
		Remove((PropertyDescriptor)value);
	}

	/// <summary>Removes the item at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}
}
