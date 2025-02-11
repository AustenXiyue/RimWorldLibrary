using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Windows.Documents;

/// <summary>Defines an ordered list of <see cref="T:System.Windows.Documents.DocumentReference" /> elements.</summary>
[CLSCompliant(false)]
public sealed class DocumentReferenceCollection : IEnumerable<DocumentReference>, IEnumerable, INotifyCollectionChanged
{
	private List<DocumentReference> _internalList;

	/// <summary>Gets the number of elements that are in the collection.</summary>
	/// <returns>The number of items that the collection contains.</returns>
	public int Count => _InternalList.Count;

	/// <summary>Gets the element that is at the specified index.</summary>
	/// <returns>The collection element that is at the specified <paramref name="index" />.</returns>
	/// <param name="index">The zero-based index of the element in the collection to get.</param>
	public DocumentReference this[int index] => _InternalList[index];

	private IList<DocumentReference> _InternalList
	{
		get
		{
			if (_internalList == null)
			{
				_internalList = new List<DocumentReference>();
			}
			return _internalList;
		}
	}

	/// <summary>Occurs when an element is added or removed.</summary>
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	internal DocumentReferenceCollection()
	{
	}

	/// <summary>Returns an enumerator for iterating through the collection.</summary>
	/// <returns>An enumerator that you can use to iterate through the collection.</returns>
	public IEnumerator<DocumentReference> GetEnumerator()
	{
		return _InternalList.GetEnumerator();
	}

	/// <summary>Returns an enumerator that iterates through a collection.  Use the type-safe <see cref="M:System.Windows.Documents.DocumentReferenceCollection.GetEnumerator" /> method instead. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<DocumentReference>)this).GetEnumerator();
	}

	/// <summary>Adds an element to the end of the collection.</summary>
	/// <param name="item">The element to add to the end of the collection.</param>
	public void Add(DocumentReference item)
	{
		int count = _InternalList.Count;
		_InternalList.Add(item);
		OnCollectionChanged(NotifyCollectionChangedAction.Add, item, count);
	}

	/// <summary>Copies the whole collection to an array that starts at a given array index.</summary>
	/// <param name="array">The destination array to which the elements from the collection should be copied.</param>
	/// <param name="arrayIndex">The zero-based starting index within the array where the collection elements are to be copied.</param>
	public void CopyTo(DocumentReference[] array, int arrayIndex)
	{
		_InternalList.CopyTo(array, arrayIndex);
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		if (this.CollectionChanged != null)
		{
			NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(action, item, index);
			this.CollectionChanged(this, e);
		}
	}
}
