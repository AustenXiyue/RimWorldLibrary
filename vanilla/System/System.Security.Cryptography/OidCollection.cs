using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography;

/// <summary>Represents a collection of <see cref="T:System.Security.Cryptography.Oid" /> objects. This class cannot be inherited.</summary>
public sealed class OidCollection : ICollection, IEnumerable
{
	private ArrayList m_list;

	/// <summary>Gets an <see cref="T:System.Security.Cryptography.Oid" /> object from the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" /> object.</returns>
	/// <param name="index">The location of the <see cref="T:System.Security.Cryptography.Oid" /> object in the collection.</param>
	public Oid this[int index] => m_list[index] as Oid;

	/// <summary>Gets the first <see cref="T:System.Security.Cryptography.Oid" /> object that contains a value of the <see cref="P:System.Security.Cryptography.Oid.Value" /> property or a value of the <see cref="P:System.Security.Cryptography.Oid.FriendlyName" /> property that matches the specified string value from the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" /> object.</returns>
	/// <param name="oid">A string that represents a <see cref="P:System.Security.Cryptography.Oid.Value" /> property or a <see cref="P:System.Security.Cryptography.Oid.FriendlyName" /> property.</param>
	public Oid this[string oid]
	{
		get
		{
			string text = X509Utils.FindOidInfoWithFallback(2u, oid, OidGroup.All);
			if (text == null)
			{
				text = oid;
			}
			foreach (Oid item in m_list)
			{
				if (item.Value == text)
				{
					return item;
				}
			}
			return null;
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Security.Cryptography.Oid" /> objects in a collection. </summary>
	/// <returns>The number of <see cref="T:System.Security.Cryptography.Oid" /> objects in a collection.</returns>
	public int Count => m_list.Count;

	/// <summary>Gets a value that indicates whether access to the <see cref="T:System.Security.Cryptography.OidCollection" /> object is thread safe.</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</returns>
	public object SyncRoot => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.OidCollection" /> class.</summary>
	public OidCollection()
	{
		m_list = new ArrayList();
	}

	/// <summary>Adds an <see cref="T:System.Security.Cryptography.Oid" /> object to the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>The index of the added <see cref="T:System.Security.Cryptography.Oid" /> object.</returns>
	/// <param name="oid">The <see cref="T:System.Security.Cryptography.Oid" /> object to add to the collection.</param>
	public int Add(Oid oid)
	{
		return m_list.Add(oid);
	}

	/// <summary>Returns an <see cref="T:System.Security.Cryptography.OidEnumerator" /> object that can be used to navigate the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.OidEnumerator" /> object.</returns>
	public OidEnumerator GetEnumerator()
	{
		return new OidEnumerator(this);
	}

	/// <summary>Returns an <see cref="T:System.Security.Cryptography.OidEnumerator" /> object that can be used to navigate the <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.OidEnumerator" /> object that can be used to navigate the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new OidEnumerator(this);
	}

	/// <summary>Copies the <see cref="T:System.Security.Cryptography.OidCollection" /> object into an array.</summary>
	/// <param name="array">The array to copy the <see cref="T:System.Security.Cryptography.OidCollection" /> object to.</param>
	/// <param name="index">The location where the copy operation starts.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> cannot be a multidimensional array.-or-The length of <paramref name="array" /> is an invalid offset length.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="index" /> is out range.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(global::SR.GetString("Only single dimensional arrays are supported for the requested action."));
		}
		if (index < 0 || index >= array.Length)
		{
			throw new ArgumentOutOfRangeException("index", global::SR.GetString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (index + Count > array.Length)
		{
			throw new ArgumentException(global::SR.GetString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		for (int i = 0; i < Count; i++)
		{
			array.SetValue(this[i], index);
			index++;
		}
	}

	/// <summary>Copies the <see cref="T:System.Security.Cryptography.OidCollection" /> object into an array.</summary>
	/// <param name="array">The array to copy the <see cref="T:System.Security.Cryptography.OidCollection" /> object into.</param>
	/// <param name="index">The location where the copy operation starts.</param>
	public void CopyTo(Oid[] array, int index)
	{
		((ICollection)this).CopyTo((Array)array, index);
	}
}
