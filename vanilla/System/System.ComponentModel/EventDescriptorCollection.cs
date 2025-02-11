using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Represents a collection of <see cref="T:System.ComponentModel.EventDescriptor" /> objects.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public class EventDescriptorCollection : ICollection, IEnumerable, IList
{
	private EventDescriptor[] events;

	private string[] namedSort;

	private IComparer comparer;

	private bool eventsOwned = true;

	private bool needSort;

	private int eventCount;

	private bool readOnly;

	/// <summary>Specifies an empty collection to use, rather than creating a new one with no items. This static field is read-only.</summary>
	public static readonly EventDescriptorCollection Empty = new EventDescriptorCollection(null, readOnly: true);

	/// <summary>Gets the number of event descriptors in the collection.</summary>
	/// <returns>The number of event descriptors in the collection.</returns>
	public int Count => eventCount;

	/// <summary>Gets or sets the event with the specified index number.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.EventDescriptor" /> with the specified index number.</returns>
	/// <param name="index">The zero-based index number of the <see cref="T:System.ComponentModel.EventDescriptor" /> to get or set. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is not a valid index for <see cref="P:System.ComponentModel.EventDescriptorCollection.Item(System.Int32)" />. </exception>
	public virtual EventDescriptor this[int index]
	{
		get
		{
			if (index >= eventCount)
			{
				throw new IndexOutOfRangeException();
			}
			EnsureEventsOwned();
			return events[index];
		}
	}

	/// <summary>Gets or sets the event with the specified name.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.EventDescriptor" /> with the specified name, or null if the event does not exist.</returns>
	/// <param name="name">The name of the <see cref="T:System.ComponentModel.EventDescriptor" /> to get or set. </param>
	public virtual EventDescriptor this[string name] => Find(name, ignoreCase: false);

	/// <summary>Gets the number of elements contained in the collection.</summary>
	/// <returns>The number of elements contained in the collection.</returns>
	int ICollection.Count => Count;

	/// <summary>Gets a value indicating whether access to the collection is synchronized.</summary>
	/// <returns>true if access to the collection is synchronized; otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	object ICollection.SyncRoot => null;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
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
			if (index >= eventCount)
			{
				throw new IndexOutOfRangeException();
			}
			EnsureEventsOwned();
			events[index] = (EventDescriptor)value;
		}
	}

	/// <summary>Gets a value indicating whether the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => readOnly;

	/// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
	/// <returns>true if the collection has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => readOnly;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EventDescriptorCollection" /> class with the given array of <see cref="T:System.ComponentModel.EventDescriptor" /> objects.</summary>
	/// <param name="events">An array of type <see cref="T:System.ComponentModel.EventDescriptor" /> that provides the events for this collection. </param>
	public EventDescriptorCollection(EventDescriptor[] events)
	{
		this.events = events;
		if (events == null)
		{
			this.events = new EventDescriptor[0];
			eventCount = 0;
		}
		else
		{
			eventCount = this.events.Length;
		}
		eventsOwned = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EventDescriptorCollection" /> class with the given array of <see cref="T:System.ComponentModel.EventDescriptor" /> objects. The collection is optionally read-only.</summary>
	/// <param name="events">An array of type <see cref="T:System.ComponentModel.EventDescriptor" /> that provides the events for this collection. </param>
	/// <param name="readOnly">true to specify a read-only collection; otherwise, false.</param>
	public EventDescriptorCollection(EventDescriptor[] events, bool readOnly)
		: this(events)
	{
		this.readOnly = readOnly;
	}

	private EventDescriptorCollection(EventDescriptor[] events, int eventCount, string[] namedSort, IComparer comparer)
	{
		eventsOwned = false;
		if (namedSort != null)
		{
			this.namedSort = (string[])namedSort.Clone();
		}
		this.comparer = comparer;
		this.events = events;
		this.eventCount = eventCount;
		needSort = true;
	}

	/// <summary>Adds an <see cref="T:System.ComponentModel.EventDescriptor" /> to the end of the collection.</summary>
	/// <returns>The position of the <see cref="T:System.ComponentModel.EventDescriptor" /> within the collection.</returns>
	/// <param name="value">An <see cref="T:System.ComponentModel.EventDescriptor" /> to add to the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public int Add(EventDescriptor value)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		EnsureSize(eventCount + 1);
		events[eventCount++] = value;
		return eventCount - 1;
	}

	/// <summary>Removes all objects from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Clear()
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		eventCount = 0;
	}

	/// <summary>Returns whether the collection contains the given <see cref="T:System.ComponentModel.EventDescriptor" />.</summary>
	/// <returns>true if the collection contains the <paramref name="value" /> parameter given; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.ComponentModel.EventDescriptor" /> to find within the collection. </param>
	public bool Contains(EventDescriptor value)
	{
		return IndexOf(value) >= 0;
	}

	/// <summary>Copies the elements of the collection to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from collection. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		EnsureEventsOwned();
		Array.Copy(events, 0, array, index, Count);
	}

	private void EnsureEventsOwned()
	{
		if (!eventsOwned)
		{
			eventsOwned = true;
			if (events != null)
			{
				EventDescriptor[] destinationArray = new EventDescriptor[Count];
				Array.Copy(events, 0, destinationArray, 0, Count);
				events = destinationArray;
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
		if (sizeNeeded > events.Length)
		{
			if (events == null || events.Length == 0)
			{
				eventCount = 0;
				events = new EventDescriptor[sizeNeeded];
				return;
			}
			EnsureEventsOwned();
			EventDescriptor[] destinationArray = new EventDescriptor[Math.Max(sizeNeeded, events.Length * 2)];
			Array.Copy(events, 0, destinationArray, 0, eventCount);
			events = destinationArray;
		}
	}

	/// <summary>Gets the description of the event with the specified name in the collection.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.EventDescriptor" /> with the specified name, or null if the event does not exist.</returns>
	/// <param name="name">The name of the event to get from the collection. </param>
	/// <param name="ignoreCase">true if you want to ignore the case of the event; otherwise, false. </param>
	public virtual EventDescriptor Find(string name, bool ignoreCase)
	{
		EventDescriptor result = null;
		if (ignoreCase)
		{
			for (int i = 0; i < Count; i++)
			{
				if (string.Equals(events[i].Name, name, StringComparison.OrdinalIgnoreCase))
				{
					result = events[i];
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < Count; j++)
			{
				if (string.Equals(events[j].Name, name, StringComparison.Ordinal))
				{
					result = events[j];
					break;
				}
			}
		}
		return result;
	}

	/// <summary>Returns the index of the given <see cref="T:System.ComponentModel.EventDescriptor" />.</summary>
	/// <returns>The index of the given <see cref="T:System.ComponentModel.EventDescriptor" /> within the collection.</returns>
	/// <param name="value">The <see cref="T:System.ComponentModel.EventDescriptor" /> to find within the collection. </param>
	public int IndexOf(EventDescriptor value)
	{
		return Array.IndexOf(events, value, 0, eventCount);
	}

	/// <summary>Inserts an <see cref="T:System.ComponentModel.EventDescriptor" /> to the collection at a specified index.</summary>
	/// <param name="index">The index within the collection in which to insert the <paramref name="value" /> parameter. </param>
	/// <param name="value">An <see cref="T:System.ComponentModel.EventDescriptor" /> to insert into the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Insert(int index, EventDescriptor value)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		EnsureSize(eventCount + 1);
		if (index < eventCount)
		{
			Array.Copy(events, index, events, index + 1, eventCount - index);
		}
		events[index] = value;
		eventCount++;
	}

	/// <summary>Removes the specified <see cref="T:System.ComponentModel.EventDescriptor" /> from the collection.</summary>
	/// <param name="value">The <see cref="T:System.ComponentModel.EventDescriptor" /> to remove from the collection. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Remove(EventDescriptor value)
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

	/// <summary>Removes the <see cref="T:System.ComponentModel.EventDescriptor" /> at the specified index from the collection.</summary>
	/// <param name="index">The index of the <see cref="T:System.ComponentModel.EventDescriptor" /> to remove. </param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void RemoveAt(int index)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		if (index < eventCount - 1)
		{
			Array.Copy(events, index + 1, events, index, eventCount - index - 1);
		}
		events[eventCount - 1] = null;
		eventCount--;
	}

	/// <summary>Gets an enumerator for this <see cref="T:System.ComponentModel.EventDescriptorCollection" />.</summary>
	/// <returns>An enumerator that implements <see cref="T:System.Collections.IEnumerator" />.</returns>
	public IEnumerator GetEnumerator()
	{
		if (events.Length == eventCount)
		{
			return events.GetEnumerator();
		}
		return new ArraySubsetEnumerator(events, eventCount);
	}

	/// <summary>Sorts the members of this <see cref="T:System.ComponentModel.EventDescriptorCollection" />, using the default sort for this collection, which is usually alphabetical.</summary>
	/// <returns>The new <see cref="T:System.ComponentModel.EventDescriptorCollection" />.</returns>
	public virtual EventDescriptorCollection Sort()
	{
		return new EventDescriptorCollection(events, eventCount, namedSort, comparer);
	}

	/// <summary>Sorts the members of this <see cref="T:System.ComponentModel.EventDescriptorCollection" />, given a specified sort order.</summary>
	/// <returns>The new <see cref="T:System.ComponentModel.EventDescriptorCollection" />.</returns>
	/// <param name="names">An array of strings describing the order in which to sort the <see cref="T:System.ComponentModel.EventDescriptor" /> objects in the collection. </param>
	public virtual EventDescriptorCollection Sort(string[] names)
	{
		return new EventDescriptorCollection(events, eventCount, names, comparer);
	}

	/// <summary>Sorts the members of this <see cref="T:System.ComponentModel.EventDescriptorCollection" />, given a specified sort order and an <see cref="T:System.Collections.IComparer" />.</summary>
	/// <returns>The new <see cref="T:System.ComponentModel.EventDescriptorCollection" />.</returns>
	/// <param name="names">An array of strings describing the order in which to sort the <see cref="T:System.ComponentModel.EventDescriptor" /> objects in the collection. </param>
	/// <param name="comparer">An <see cref="T:System.Collections.IComparer" /> to use to sort the <see cref="T:System.ComponentModel.EventDescriptor" /> objects in this collection. </param>
	public virtual EventDescriptorCollection Sort(string[] names, IComparer comparer)
	{
		return new EventDescriptorCollection(events, eventCount, names, comparer);
	}

	/// <summary>Sorts the members of this <see cref="T:System.ComponentModel.EventDescriptorCollection" />, using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <returns>The new <see cref="T:System.ComponentModel.EventDescriptorCollection" />.</returns>
	/// <param name="comparer">An <see cref="T:System.Collections.IComparer" /> to use to sort the <see cref="T:System.ComponentModel.EventDescriptor" /> objects in this collection. </param>
	public virtual EventDescriptorCollection Sort(IComparer comparer)
	{
		return new EventDescriptorCollection(events, eventCount, namedSort, comparer);
	}

	/// <summary>Sorts the members of this <see cref="T:System.ComponentModel.EventDescriptorCollection" />. The specified order is applied first, followed by the default sort for this collection, which is usually alphabetical.</summary>
	/// <param name="names">An array of strings describing the order in which to sort the <see cref="T:System.ComponentModel.EventDescriptor" /> objects in this collection. </param>
	protected void InternalSort(string[] names)
	{
		if (events == null || events.Length == 0)
		{
			return;
		}
		InternalSort(comparer);
		if (names == null || names.Length == 0)
		{
			return;
		}
		ArrayList arrayList = new ArrayList(events);
		int num = 0;
		int num2 = events.Length;
		for (int i = 0; i < names.Length; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				EventDescriptor eventDescriptor = (EventDescriptor)arrayList[j];
				if (eventDescriptor != null && eventDescriptor.Name.Equals(names[i]))
				{
					events[num++] = eventDescriptor;
					arrayList[j] = null;
					break;
				}
			}
		}
		for (int k = 0; k < num2; k++)
		{
			if (arrayList[k] != null)
			{
				events[num++] = (EventDescriptor)arrayList[k];
			}
		}
	}

	/// <summary>Sorts the members of this <see cref="T:System.ComponentModel.EventDescriptorCollection" />, using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <param name="sorter">A comparer to use to sort the <see cref="T:System.ComponentModel.EventDescriptor" /> objects in this collection. </param>
	protected void InternalSort(IComparer sorter)
	{
		if (sorter == null)
		{
			TypeDescriptor.SortDescriptorArray(this);
		}
		else
		{
			Array.Sort(events, sorter);
		}
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds an item to the collection.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	int IList.Add(object value)
	{
		return Add((EventDescriptor)value);
	}

	/// <summary>Removes all the items from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>Determines whether the collection contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the collection.</param>
	bool IList.Contains(object value)
	{
		return Contains((EventDescriptor)value);
	}

	/// <summary>Determines the index of a specific item in the collection.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the collection.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf((EventDescriptor)value);
	}

	/// <summary>Inserts an item to the collection at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.Insert(int index, object value)
	{
		Insert(index, (EventDescriptor)value);
	}

	/// <summary>Removes the first occurrence of a specific object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.Remove(object value)
	{
		Remove((EventDescriptor)value);
	}

	/// <summary>Removes the item at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}
}
