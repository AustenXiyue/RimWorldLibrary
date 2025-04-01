using System.Runtime.InteropServices;

namespace System.Collections;

/// <summary>Provides the abstract base class for a strongly typed collection.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class CollectionBase : IList, ICollection, IEnumerable
{
	private ArrayList list;

	/// <summary>Gets an <see cref="T:System.Collections.ArrayList" /> containing the list of elements in the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> representing the <see cref="T:System.Collections.CollectionBase" /> instance itself.Retrieving the value of this property is an O(1) operation.</returns>
	protected ArrayList InnerList
	{
		get
		{
			if (list == null)
			{
				list = new ArrayList();
			}
			return list;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.IList" /> containing the list of elements in the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> representing the <see cref="T:System.Collections.CollectionBase" /> instance itself.</returns>
	protected IList List => this;

	/// <summary>Gets or sets the number of elements that the <see cref="T:System.Collections.CollectionBase" /> can contain.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Collections.CollectionBase" /> can contain.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Collections.CollectionBase.Capacity" /> is set to a value that is less than <see cref="P:System.Collections.CollectionBase.Count" />.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public int Capacity
	{
		get
		{
			return InnerList.Capacity;
		}
		set
		{
			InnerList.Capacity = value;
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.CollectionBase" /> instance. This property cannot be overridden.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.CollectionBase" /> instance.Retrieving the value of this property is an O(1) operation.</returns>
	/// <filterpriority>2</filterpriority>
	public int Count
	{
		get
		{
			if (list != null)
			{
				return list.Count;
			}
			return 0;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.CollectionBase" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.CollectionBase" /> is read-only; otherwise, false. The default is false.</returns>
	bool IList.IsReadOnly => InnerList.IsReadOnly;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.CollectionBase" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.CollectionBase" /> has a fixed size; otherwise, false. The default is false.</returns>
	bool IList.IsFixedSize => InnerList.IsFixedSize;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.CollectionBase" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.CollectionBase" /> is synchronized (thread safe); otherwise, false. The default is false.</returns>
	bool ICollection.IsSynchronized => InnerList.IsSynchronized;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.CollectionBase" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.CollectionBase" />.</returns>
	object ICollection.SyncRoot => InnerList.SyncRoot;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.CollectionBase.Count" />.</exception>
	object IList.this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			return InnerList[index];
		}
		set
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			OnValidate(value);
			object obj = InnerList[index];
			OnSet(index, obj, value);
			InnerList[index] = value;
			try
			{
				OnSetComplete(index, obj, value);
			}
			catch
			{
				InnerList[index] = obj;
				throw;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.CollectionBase" /> class with the default initial capacity.</summary>
	protected CollectionBase()
	{
		list = new ArrayList();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.CollectionBase" /> class with the specified capacity.</summary>
	/// <param name="capacity">The number of elements that the new list can initially store.</param>
	protected CollectionBase(int capacity)
	{
		list = new ArrayList(capacity);
	}

	/// <summary>Removes all objects from the <see cref="T:System.Collections.CollectionBase" /> instance. This method cannot be overridden.</summary>
	/// <filterpriority>2</filterpriority>
	public void Clear()
	{
		OnClear();
		InnerList.Clear();
		OnClearComplete();
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.CollectionBase" /> instance. This method is not overridable.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.CollectionBase.Count" />.</exception>
	/// <filterpriority>2</filterpriority>
	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		object value = InnerList[index];
		OnValidate(value);
		OnRemove(index, value);
		InnerList.RemoveAt(index);
		try
		{
			OnRemoveComplete(index, value);
		}
		catch
		{
			InnerList.Insert(index, value);
			throw;
		}
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.CollectionBase" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.CollectionBase" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.CollectionBase" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.CollectionBase" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		InnerList.CopyTo(array, index);
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.CollectionBase" /> contains a specific element.</summary>
	/// <returns>true if the <see cref="T:System.Collections.CollectionBase" /> contains the specified <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.CollectionBase" />.</param>
	bool IList.Contains(object value)
	{
		return InnerList.Contains(value);
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.CollectionBase" />.</summary>
	/// <returns>The <see cref="T:System.Collections.CollectionBase" /> index at which the <paramref name="value" /> has been added.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be added to the end of the <see cref="T:System.Collections.CollectionBase" />.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.CollectionBase" /> is read-only.-or-The <see cref="T:System.Collections.CollectionBase" /> has a fixed size.</exception>
	int IList.Add(object value)
	{
		OnValidate(value);
		OnInsert(InnerList.Count, value);
		int num = InnerList.Add(value);
		try
		{
			OnInsertComplete(num, value);
			return num;
		}
		catch
		{
			InnerList.RemoveAt(num);
			throw;
		}
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.CollectionBase" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Collections.CollectionBase" />.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> parameter was not found in the <see cref="T:System.Collections.CollectionBase" /> object.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.CollectionBase" /> is read-only.-or-The <see cref="T:System.Collections.CollectionBase" /> has a fixed size.</exception>
	void IList.Remove(object value)
	{
		OnValidate(value);
		int num = InnerList.IndexOf(value);
		if (num < 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Cannot remove the specified item because it was not found in the specified Collection."));
		}
		OnRemove(num, value);
		InnerList.RemoveAt(num);
		try
		{
			OnRemoveComplete(num, value);
		}
		catch
		{
			InnerList.Insert(num, value);
			throw;
		}
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.CollectionBase" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <see cref="T:System.Collections.CollectionBase" />, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.CollectionBase" />.</param>
	int IList.IndexOf(object value)
	{
		return InnerList.IndexOf(value);
	}

	/// <summary>Inserts an element into the <see cref="T:System.Collections.CollectionBase" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.CollectionBase.Count" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.CollectionBase" /> is read-only.-or-The <see cref="T:System.Collections.CollectionBase" /> has a fixed size.</exception>
	void IList.Insert(int index, object value)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		OnValidate(value);
		OnInsert(index, value);
		InnerList.Insert(index, value);
		try
		{
			OnInsertComplete(index, value);
		}
		catch
		{
			InnerList.RemoveAt(index);
			throw;
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.CollectionBase" /> instance.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerator GetEnumerator()
	{
		return InnerList.GetEnumerator();
	}

	/// <summary>Performs additional custom processes before setting a value in the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index at which <paramref name="oldValue" /> can be found.</param>
	/// <param name="oldValue">The value to replace with <paramref name="newValue" />.</param>
	/// <param name="newValue">The new value of the element at <paramref name="index" />.</param>
	protected virtual void OnSet(int index, object oldValue, object newValue)
	{
	}

	/// <summary>Performs additional custom processes before inserting a new element into the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="value" />.</param>
	/// <param name="value">The new value of the element at <paramref name="index" />.</param>
	protected virtual void OnInsert(int index, object value)
	{
	}

	/// <summary>Performs additional custom processes when clearing the contents of the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	protected virtual void OnClear()
	{
	}

	/// <summary>Performs additional custom processes when removing an element from the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> can be found.</param>
	/// <param name="value">The value of the element to remove from <paramref name="index" />.</param>
	protected virtual void OnRemove(int index, object value)
	{
	}

	/// <summary>Performs additional custom processes when validating a value.</summary>
	/// <param name="value">The object to validate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	protected virtual void OnValidate(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
	}

	/// <summary>Performs additional custom processes after setting a value in the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index at which <paramref name="oldValue" /> can be found.</param>
	/// <param name="oldValue">The value to replace with <paramref name="newValue" />.</param>
	/// <param name="newValue">The new value of the element at <paramref name="index" />.</param>
	protected virtual void OnSetComplete(int index, object oldValue, object newValue)
	{
	}

	/// <summary>Performs additional custom processes after inserting a new element into the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="value" />.</param>
	/// <param name="value">The new value of the element at <paramref name="index" />.</param>
	protected virtual void OnInsertComplete(int index, object value)
	{
	}

	/// <summary>Performs additional custom processes after clearing the contents of the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	protected virtual void OnClearComplete()
	{
	}

	/// <summary>Performs additional custom processes after removing an element from the <see cref="T:System.Collections.CollectionBase" /> instance.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> can be found.</param>
	/// <param name="value">The value of the element to remove from <paramref name="index" />.</param>
	protected virtual void OnRemoveComplete(int index, object value)
	{
	}
}
