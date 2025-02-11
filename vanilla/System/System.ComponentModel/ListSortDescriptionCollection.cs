using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Represents a collection of <see cref="T:System.ComponentModel.ListSortDescription" /> objects.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class ListSortDescriptionCollection : IList, ICollection, IEnumerable
{
	private ArrayList sorts = new ArrayList();

	/// <summary>Gets or sets the specified <see cref="T:System.ComponentModel.ListSortDescription" />.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ListSortDescription" /> with the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.ComponentModel.ListSortDescription" />  to get or set in the collection. </param>
	/// <exception cref="T:System.InvalidOperationException">An item is set in the <see cref="T:System.ComponentModel.ListSortDescriptionCollection" />, which is read-only.</exception>
	public ListSortDescription this[int index]
	{
		get
		{
			return (ListSortDescription)sorts[index];
		}
		set
		{
			throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
		}
	}

	/// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
	/// <returns>true in all cases.</returns>
	bool IList.IsFixedSize => true;

	/// <summary>Gets a value indicating whether the collection is read-only.</summary>
	/// <returns>true in all cases.</returns>
	bool IList.IsReadOnly => true;

	/// <summary>Gets the specified <see cref="T:System.ComponentModel.ListSortDescription" />.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ListSortDescription" /> with the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.ComponentModel.ListSortDescription" />  to get in the collection </param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
		}
	}

	/// <summary>Gets the number of items in the collection.</summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count => sorts.Count;

	/// <summary>Gets a value indicating whether access to the collection is thread safe.</summary>
	/// <returns>true in all cases.</returns>
	bool ICollection.IsSynchronized => true;

	/// <summary>Gets the current instance that can be used to synchronize access to the collection.</summary>
	/// <returns>The current instance of the <see cref="T:System.ComponentModel.ListSortDescriptionCollection" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> class. </summary>
	public ListSortDescriptionCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> class with the specified array of <see cref="T:System.ComponentModel.ListSortDescription" /> objects.</summary>
	/// <param name="sorts">The array of <see cref="T:System.ComponentModel.ListSortDescription" /> objects to be contained in the collection.</param>
	public ListSortDescriptionCollection(ListSortDescription[] sorts)
	{
		if (sorts != null)
		{
			for (int i = 0; i < sorts.Length; i++)
			{
				this.sorts.Add(sorts[i]);
			}
		}
	}

	/// <summary>Adds an item to the collection.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The item to add to the collection.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	int IList.Add(object value)
	{
		throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
	}

	/// <summary>Removes all items from the collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	void IList.Clear()
	{
		throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
	}

	/// <summary>Determines if the <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the collection; otherwise, false. </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the collection.</param>
	public bool Contains(object value)
	{
		return ((IList)sorts).Contains(value);
	}

	/// <summary>Returns the index of the specified item in the collection.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the collection.</param>
	public int IndexOf(object value)
	{
		return ((IList)sorts).IndexOf(value);
	}

	/// <summary>Inserts an item into the collection at a specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.ComponentModel.ListSortDescription" />  to get or set in the collection</param>
	/// <param name="value">The item to insert into the collection.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	void IList.Insert(int index, object value)
	{
		throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
	}

	/// <summary>Removes the first occurrence of an item from the collection.</summary>
	/// <param name="value">The item to remove from the collection.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	void IList.Remove(object value)
	{
		throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
	}

	/// <summary>Removes an item from the collection at a specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.ComponentModel.ListSortDescription" />  to remove from the collection</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	void IList.RemoveAt(int index)
	{
		throw new InvalidOperationException(global::SR.GetString("Once a ListSortDescriptionCollection has been created it can't be modified."));
	}

	/// <summary>Copies the contents of the collection to the specified array, starting at the specified destination array index.</summary>
	/// <param name="array">The destination array for the items copied from the collection.</param>
	/// <param name="index">The index of the destination array at which copying begins.</param>
	public void CopyTo(Array array, int index)
	{
		sorts.CopyTo(array, index);
	}

	/// <summary>Gets a <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return sorts.GetEnumerator();
	}
}
