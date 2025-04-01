using System.Collections;
using System.Collections.Generic;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeNamespaceImport" /> objects.</summary>
[Serializable]
public class CodeNamespaceImportCollection : IList, ICollection, IEnumerable
{
	private readonly ArrayList _data = new ArrayList();

	private readonly Dictionary<string, CodeNamespaceImport> _keys = new Dictionary<string, CodeNamespaceImport>(StringComparer.OrdinalIgnoreCase);

	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeNamespaceImport" /> object at the specified index in the collection.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeNamespaceImport" /> object at each valid index.</returns>
	/// <param name="index">The index of the collection to access. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is outside the valid range of indexes for the collection. </exception>
	public CodeNamespaceImport this[int index]
	{
		get
		{
			return (CodeNamespaceImport)_data[index];
		}
		set
		{
			_data[index] = value;
			SyncKeys();
		}
	}

	/// <summary>Gets the number of namespaces in the collection.</summary>
	/// <returns>The number of namespaces in the collection.</returns>
	public int Count => _data.Count;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.  This property always returns false.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.  This property always returns false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (CodeNamespaceImport)value;
			SyncKeys();
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection" />.</returns>
	int ICollection.Count => Count;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false. This property always returns false. </returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  This property always returns null.</returns>
	object ICollection.SyncRoot => null;

	/// <summary>Adds a <see cref="T:System.CodeDom.CodeNamespaceImport" /> object to the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespaceImport" /> object to add to the collection. </param>
	public void Add(CodeNamespaceImport value)
	{
		if (!_keys.ContainsKey(value.Namespace))
		{
			_keys[value.Namespace] = value;
			_data.Add(value);
		}
	}

	/// <summary>Adds a set of <see cref="T:System.CodeDom.CodeNamespaceImport" /> objects to the collection.</summary>
	/// <param name="value">An array of type <see cref="T:System.CodeDom.CodeNamespaceImport" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeNamespaceImport[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		foreach (CodeNamespaceImport value2 in value)
		{
			Add(value2);
		}
	}

	/// <summary>Clears the collection of members.</summary>
	public void Clear()
	{
		_data.Clear();
		_keys.Clear();
	}

	private void SyncKeys()
	{
		_keys.Clear();
		foreach (CodeNamespaceImport datum in _data)
		{
			_keys[datum.Namespace] = datum;
		}
	}

	/// <summary>Gets an enumerator that enumerates the collection members.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that indicates the collection members.</returns>
	public IEnumerator GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.ICollection" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		_data.CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that can iterate through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds an object to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Collections.IList" />.</param>
	int IList.Add(object value)
	{
		return _data.Add((CodeNamespaceImport)value);
	}

	/// <summary>Removes all items from the <see cref="T:System.Collections.IList" />.</summary>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
	/// <returns>true if the value is in the list; otherwise, false. </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	bool IList.Contains(object value)
	{
		return _data.Contains(value);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />. </summary>
	/// <returns>The index of <paramref name="value" /> if it is found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	int IList.IndexOf(object value)
	{
		return _data.IndexOf((CodeNamespaceImport)value);
	}

	/// <summary>Inserts an item in the <see cref="T:System.Collections.IList" /> at the specified position. </summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Collections.IList" />.</param>
	void IList.Insert(int index, object value)
	{
		_data.Insert(index, (CodeNamespaceImport)value);
		SyncKeys();
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />. </summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Collections.IList" />.</param>
	void IList.Remove(object value)
	{
		_data.Remove((CodeNamespaceImport)value);
		SyncKeys();
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.IList" />. </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	void IList.RemoveAt(int index)
	{
		_data.RemoveAt(index);
		SyncKeys();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespaceImportCollection" /> class. </summary>
	public CodeNamespaceImportCollection()
	{
	}
}
