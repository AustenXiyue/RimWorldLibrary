using System.Collections;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> class represents a collection of <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> objects. <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> implements the <see cref="T:System.Collections.ICollection" /> interface. </summary>
public sealed class SignerInfoCollection : ICollection, IEnumerable
{
	private ArrayList _list;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.Count" /> property retrieves the number of items in the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>An int value that represents the number of items in the collection.</returns>
	public int Count => _list.Count;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.IsSynchronized" /> property retrieves whether access to the collection is synchronized, or thread safe. This property always returns false, which means the collection is not thread safe.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value of false, which means the collection is not thread safe.</returns>
	public bool IsSynchronized => false;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.Item(System.Int32)" /> property retrieves the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object at the specified index in the collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object  at the specified index.</returns>
	/// <param name="index">An int value that represents the index in the collection. The index is zero based.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public SignerInfo this[int index] => (SignerInfo)_list[index];

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.SyncRoot" /> property retrieves an <see cref="T:System.Object" /> object is used to synchronize access to the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>An <see cref="T:System.Object" /> object is used to synchronize access to the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</returns>
	public object SyncRoot => _list.SyncRoot;

	internal SignerInfoCollection()
	{
		_list = new ArrayList();
	}

	internal void Add(SignerInfo signer)
	{
		_list.Add(signer);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.CopyTo(System.Array,System.Int32)" /> method copies the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection to an array.</summary>
	/// <param name="array">An <see cref="T:System.Array" /> object to which the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is to be copied.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> where the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is copied.</param>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index >= array.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_list.CopyTo(array, index);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.CopyTo(System.Security.Cryptography.Pkcs.SignerInfo[],System.Int32)" /> method copies the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection to a <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> array.</summary>
	/// <param name="array">An array of <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> objects where the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is to be copied.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> where the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is copied.</param>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public void CopyTo(SignerInfo[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index >= array.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_list.CopyTo(array, index);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.GetEnumerator" /> method returns a <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object for the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object that can be used to enumerate the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</returns>
	public SignerInfoEnumerator GetEnumerator()
	{
		return new SignerInfoEnumerator(_list);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.System#Collections#IEnumerable#GetEnumerator" /> method returns a <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object for the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object that can be used to enumerate the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new SignerInfoEnumerator(_list);
	}
}
