using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;ReferenceList&gt; element used in XML encryption. This class cannot be inherited.</summary>
public sealed class ReferenceList : IList, ICollection, IEnumerable
{
	private ArrayList _references;

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object.</returns>
	public int Count => _references.Count;

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object at the specified index.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object at the specified index.</returns>
	/// <param name="index">The index of the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to return.</param>
	[IndexerName("ItemOf")]
	public EncryptedReference this[int index]
	{
		get
		{
			return Item(index);
		}
		set
		{
			((IList)this)[index] = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
	object IList.this[int index]
	{
		get
		{
			return _references[index];
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is DataReference) && !(value is KeyReference))
			{
				throw new ArgumentException("Type of input object is invalid.", "value");
			}
			_references[index] = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => _references.IsFixedSize;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => _references.IsReadOnly;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object.</returns>
	public object SyncRoot => _references.SyncRoot;

	/// <summary>Gets a value that indicates whether access to the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object is synchronized (thread safe); otherwise, false. </returns>
	public bool IsSynchronized => _references.IsSynchronized;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> class. </summary>
	public ReferenceList()
	{
		_references = new ArrayList();
	}

	/// <summary>Returns an enumerator that iterates through a <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through a <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</returns>
	public IEnumerator GetEnumerator()
	{
		return _references.GetEnumerator();
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</summary>
	/// <returns>The position at which the new element was inserted.</returns>
	/// <param name="value">A <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to add to the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> parameter is not a <see cref="T:System.Security.Cryptography.Xml.DataReference" />  object.-or-The <paramref name="value" /> parameter is not a <see cref="T:System.Security.Cryptography.Xml.KeyReference" />  object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	public int Add(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is DataReference) && !(value is KeyReference))
		{
			throw new ArgumentException("Type of input object is invalid.", "value");
		}
		return _references.Add(value);
	}

	/// <summary>Removes all items from the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</summary>
	public void Clear()
	{
		_references.Clear();
	}

	/// <summary>Determines whether the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection contains a specific <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object.</summary>
	/// <returns>true if the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object is found in the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection; otherwise, false. </returns>
	/// <param name="value">The <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to locate in the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection. </param>
	public bool Contains(object value)
	{
		return _references.Contains(value);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the collection; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to locate in the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</param>
	public int IndexOf(object value)
	{
		return _references.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object into the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection at the specified position.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">A <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to insert into the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> parameter is not a <see cref="T:System.Security.Cryptography.Xml.DataReference" />  object.-or-The <paramref name="value" /> parameter is not a <see cref="T:System.Security.Cryptography.Xml.KeyReference" />  object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	public void Insert(int index, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is DataReference) && !(value is KeyReference))
		{
			throw new ArgumentException("Type of input object is invalid.", "value");
		}
		_references.Insert(index, value);
	}

	/// <summary>Removes the first occurrence of a specific <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object from the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</summary>
	/// <param name="value">The <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to remove from the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> collection.</param>
	public void Remove(object value)
	{
		_references.Remove(value);
	}

	/// <summary>Removes the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object at the specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to remove.</param>
	public void RemoveAt(int index)
	{
		_references.RemoveAt(index);
	}

	/// <summary>Returns the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object at the specified index.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object at the specified index.</returns>
	/// <param name="index">The index of the <see cref="T:System.Security.Cryptography.Xml.DataReference" /> or <see cref="T:System.Security.Cryptography.Xml.KeyReference" /> object to return.</param>
	public EncryptedReference Item(int index)
	{
		return (EncryptedReference)_references[index];
	}

	/// <summary>Copies the elements of the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object to an array, starting at a specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> object that is the destination of the elements copied from the <see cref="T:System.Security.Cryptography.Xml.ReferenceList" /> object. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	public void CopyTo(Array array, int index)
	{
		_references.CopyTo(array, index);
	}
}
