using System.Collections;

namespace System.Security.Cryptography;

/// <summary>Contains a set of <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> objects.</summary>
public sealed class CryptographicAttributeObjectCollection : ICollection, IEnumerable
{
	private ArrayList _list;

	/// <summary>Gets the number of items in the collection.</summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count => _list.Count;

	/// <summary>Gets a value that indicates whether access to the collection is synchronized, or thread safe.</summary>
	/// <returns>true if access to the collection is thread safe; otherwise false.</returns>
	public bool IsSynchronized => _list.IsSynchronized;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object at the specified index in the collection.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object at the specified index.</returns>
	/// <param name="index">An <see cref="T:System.Int32" /> value that represents the zero-based index of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to retrieve.</param>
	public CryptographicAttributeObject this[int index] => (CryptographicAttributeObject)_list[index];

	/// <summary>Gets an <see cref="T:System.Object" /> object used to synchronize access to the collection.</summary>
	/// <returns>An <see cref="T:System.Object" /> object used to synchronize access to the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	public object SyncRoot => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> class.</summary>
	public CryptographicAttributeObjectCollection()
	{
		_list = new ArrayList();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> class, adding a specified <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> to the collection.</summary>
	/// <param name="attribute">A <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object that is added to the collection.</param>
	public CryptographicAttributeObjectCollection(CryptographicAttributeObject attribute)
		: this()
	{
		_list.Add(attribute);
	}

	/// <summary>Adds the specified <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object to the collection.</summary>
	/// <returns>true if the method returns the zero-based index of the added item; otherwise, false.</returns>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asnEncodedData" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public int Add(AsnEncodedData asnEncodedData)
	{
		if (asnEncodedData == null)
		{
			throw new ArgumentNullException("asnEncodedData");
		}
		AsnEncodedDataCollection values = new AsnEncodedDataCollection(asnEncodedData);
		return Add(new CryptographicAttributeObject(asnEncodedData.Oid, values));
	}

	/// <summary>Adds the specified <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to the collection.</summary>
	/// <returns>true if the method returns the zero-based index of the added item; otherwise, false.</returns>
	/// <param name="attribute">The <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asnEncodedData" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The specified item already exists in the collection.</exception>
	public int Add(CryptographicAttributeObject attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		int num = -1;
		string value = attribute.Oid.Value;
		for (int i = 0; i < _list.Count; i++)
		{
			if ((_list[i] as CryptographicAttributeObject).Oid.Value == value)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			CryptographicAttributeObject cryptographicAttributeObject = this[num];
			AsnEncodedDataEnumerator enumerator = attribute.Values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				AsnEncodedData current = enumerator.Current;
				cryptographicAttributeObject.Values.Add(current);
			}
			return num;
		}
		return _list.Add(attribute);
	}

	/// <summary>Copies the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection to an array of <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> objects.</summary>
	/// <param name="array">An array of <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> objects that the collection is copied to.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> to which the collection is to be copied.</param>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">null was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public void CopyTo(CryptographicAttributeObject[] array, int index)
	{
		_list.CopyTo(array, index);
	}

	/// <summary>Copies the elements of this <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection to an <see cref="T:System.Array" /> array, starting at a particular index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> array that is the destination of the elements copied from this <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" />. The <see cref="T:System.Array" /> array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		_list.CopyTo(array, index);
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectEnumerator" /> object for the collection.</summary>
	/// <returns>true if the method returns a <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectEnumerator" /> object that can be used to enumerate the collection; otherwise, false.</returns>
	public CryptographicAttributeObjectEnumerator GetEnumerator()
	{
		return new CryptographicAttributeObjectEnumerator(_list);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new CryptographicAttributeObjectEnumerator(_list);
	}

	/// <summary>Removes the specified <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object from the collection.</summary>
	/// <param name="attribute">The <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attribute" /> is null.</exception>
	public void Remove(CryptographicAttributeObject attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		_list.Remove(attribute);
	}
}
