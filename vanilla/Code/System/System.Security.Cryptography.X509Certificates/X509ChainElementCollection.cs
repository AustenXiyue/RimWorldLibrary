using System.Collections;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents a collection of <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElement" /> objects. This class cannot be inherited.</summary>
public sealed class X509ChainElementCollection : ICollection, IEnumerable
{
	private ArrayList _list;

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>An integer representing the number of elements in the collection.</returns>
	public int Count => _list.Count;

	/// <summary>Gets a value indicating whether the collection of chain elements is synchronized.</summary>
	/// <returns>Always returns false.</returns>
	public bool IsSynchronized => _list.IsSynchronized;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElement" /> object at the specified index.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElement" /> object.</returns>
	/// <param name="index">An integer value. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is greater than or equal to the length of the collection. </exception>
	public X509ChainElement this[int index] => (X509ChainElement)_list[index];

	/// <summary>Gets an object that can be used to synchronize access to an <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementCollection" /> object.</summary>
	/// <returns>A pointer reference to the current object.</returns>
	public object SyncRoot => _list.SyncRoot;

	internal X509ChainElementCollection()
	{
		_list = new ArrayList();
	}

	/// <summary>Copies an <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementCollection" /> object into an array, starting at the specified index.</summary>
	/// <param name="array">An array of <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElement" /> objects. </param>
	/// <param name="index">An integer representing the index value. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified <paramref name="index" /> is less than zero, or greater than or equal to the length of the array. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> plus the current count is greater than the length of the array. </exception>
	public void CopyTo(X509ChainElement[] array, int index)
	{
		_list.CopyTo(array, index);
	}

	/// <summary>Copies an <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementCollection" /> object into an array, starting at the specified index.</summary>
	/// <param name="array">An array to copy the <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementCollection" /> object to.</param>
	/// <param name="index">The index of <paramref name="array" /> at which to start copying.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified <paramref name="index" /> is less than zero, or greater than or equal to the length of the array. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> plus the current count is greater than the length of the array. </exception>
	void ICollection.CopyTo(Array array, int index)
	{
		_list.CopyTo(array, index);
	}

	/// <summary>Gets an <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementEnumerator" /> object that can be used to navigate through a collection of chain elements.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementEnumerator" /> object.</returns>
	public X509ChainElementEnumerator GetEnumerator()
	{
		return new X509ChainElementEnumerator(_list);
	}

	/// <summary>Gets an <see cref="T:System.Collections.IEnumerator" /> object that can be used to navigate a collection of chain elements.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new X509ChainElementEnumerator(_list);
	}

	internal void Add(X509Certificate2 certificate)
	{
		_list.Add(new X509ChainElement(certificate));
	}

	internal void Clear()
	{
		_list.Clear();
	}

	internal bool Contains(X509Certificate2 certificate)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			if (certificate.Equals((_list[i] as X509ChainElement).Certificate))
			{
				return true;
			}
		}
		return false;
	}
}
