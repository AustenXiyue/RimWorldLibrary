using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Enables multiple collections and items to be displayed as a single list.</summary>
[Localizability(LocalizationCategory.Ignore)]
public class CompositeCollection : IList, ICollection, IEnumerable, INotifyCollectionChanged, ICollectionViewFactory, IWeakEventListener
{
	private ArrayList _internalList;

	/// <summary>Gets the number of items stored in this collection.</summary>
	/// <returns>The number of items stored in this collection.</returns>
	public int Count => InternalList.Count;

	/// <summary>Indexer property that retrieves or replaces the item at the given zero-based offset in the collection. </summary>
	/// <returns>The item at the specified zero-based offset.</returns>
	/// <param name="itemIndex">The zero-based offset of the item to retrieve or replace.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">If the index is out of range.</exception>
	public object this[int itemIndex]
	{
		get
		{
			return InternalList[itemIndex];
		}
		set
		{
			object obj = InternalList[itemIndex];
			if (obj is CollectionContainer cc)
			{
				RemoveCollectionContainer(cc);
			}
			if (value is CollectionContainer cc2)
			{
				AddCollectionContainer(cc2);
			}
			InternalList[itemIndex] = value;
			OnCollectionChanged(NotifyCollectionChangedAction.Replace, obj, value, itemIndex);
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => InternalList.IsSynchronized;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => InternalList.SyncRoot;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => InternalList.IsFixedSize;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => InternalList.IsReadOnly;

	private ArrayList InternalList => _internalList;

	/// <summary>Occurs when the collection has changed.</summary>
	event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
	{
		add
		{
			CollectionChanged += value;
		}
		remove
		{
			CollectionChanged -= value;
		}
	}

	/// <summary>Occurs when the collection changes, either by adding or removing an item. </summary>
	protected event NotifyCollectionChangedEventHandler CollectionChanged;

	internal event NotifyCollectionChangedEventHandler ContainedCollectionChanged;

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Data.CompositeCollection" /> class that is empty and has default initial capacity. </summary>
	public CompositeCollection()
	{
		Initialize(new ArrayList());
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Data.CompositeCollection" /> class that is empty and has a specified initial capacity. </summary>
	/// <param name="capacity">The number of items that the new list is initially capable of storing.</param>
	public CompositeCollection(int capacity)
	{
		Initialize(new ArrayList(capacity));
	}

	/// <summary>Returns an enumerator.</summary>
	/// <returns>An IEnumerator object.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return InternalList.GetEnumerator();
	}

	/// <summary>Makes a shallow copy of object references from this collection to the given array.</summary>
	/// <param name="array">The array that is the destination of the copy operation.</param>
	/// <param name="index">Zero-based index in the target array at which the copying starts.</param>
	public void CopyTo(Array array, int index)
	{
		InternalList.CopyTo(array, index);
	}

	/// <summary>Adds the specified item to this collection.</summary>
	/// <returns>Zero-based index where the new item is added.</returns>
	/// <param name="newItem">New item to add to the collection.</param>
	public int Add(object newItem)
	{
		if (newItem is CollectionContainer cc)
		{
			AddCollectionContainer(cc);
		}
		int num = InternalList.Add(newItem);
		OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem, num);
		return num;
	}

	/// <summary>Clears the collection.</summary>
	public void Clear()
	{
		int i = 0;
		for (int count = InternalList.Count; i < count; i++)
		{
			if (this[i] is CollectionContainer cc)
			{
				RemoveCollectionContainer(cc);
			}
		}
		InternalList.Clear();
		OnCollectionChanged(NotifyCollectionChangedAction.Reset);
	}

	/// <summary>Checks to see if a given item is in this collection.</summary>
	/// <returns>true if the collection contains the given item; otherwise, false.</returns>
	/// <param name="containItem">The item to check.</param>
	public bool Contains(object containItem)
	{
		return InternalList.Contains(containItem);
	}

	/// <summary>Returns the index in this collection where the given item is found. </summary>
	/// <returns>If the item appears in the collection, then the zero-based index in the collection where the given item is found; otherwise, -1.</returns>
	/// <param name="indexItem">The item to retrieve the index for.</param>
	public int IndexOf(object indexItem)
	{
		return InternalList.IndexOf(indexItem);
	}

	/// <summary>Inserts an item in the collection at a given index. All items after the given position are moved down by one. </summary>
	/// <param name="insertIndex">The index to insert the item at.</param>
	/// <param name="insertItem">The item reference to add to the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">If index is out of range.</exception>
	public void Insert(int insertIndex, object insertItem)
	{
		if (insertItem is CollectionContainer cc)
		{
			AddCollectionContainer(cc);
		}
		InternalList.Insert(insertIndex, insertItem);
		OnCollectionChanged(NotifyCollectionChangedAction.Add, insertItem, insertIndex);
	}

	/// <summary>Removes the given item reference from the collection. All remaining items move up by one. </summary>
	/// <param name="removeItem">The item to remove.</param>
	public void Remove(object removeItem)
	{
		int num = InternalList.IndexOf(removeItem);
		if (num >= 0)
		{
			RemoveAt(num);
		}
	}

	/// <summary>Removes an item from the collection at the given index. All remaining items move up by one. </summary>
	/// <param name="removeIndex">The index at which to remove an item.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">If index is out of range.</exception>
	public void RemoveAt(int removeIndex)
	{
		if (0 <= removeIndex && removeIndex < Count)
		{
			object obj = this[removeIndex];
			if (obj is CollectionContainer cc)
			{
				RemoveCollectionContainer(cc);
			}
			InternalList.RemoveAt(removeIndex);
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, obj, removeIndex);
			return;
		}
		throw new ArgumentOutOfRangeException("removeIndex", SR.ItemCollectionRemoveArgumentOutOfRange);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The view created.</returns>
	ICollectionView ICollectionViewFactory.CreateView()
	{
		return new CompositeCollectionView(this);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Collections.Specialized.CollectionChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return ReceiveWeakEvent(managerType, sender, e);
	}

	/// <summary>Handles events from the centralized event table.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Collections.Specialized.CollectionChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private void OnContainedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (this.ContainedCollectionChanged != null)
		{
			this.ContainedCollectionChanged(sender, e);
		}
	}

	private void Initialize(ArrayList internalList)
	{
		_internalList = internalList;
	}

	private void AddCollectionContainer(CollectionContainer cc)
	{
		if (InternalList.Contains(cc))
		{
			throw new ArgumentException(SR.CollectionContainerMustBeUniqueForComposite, "cc");
		}
		CollectionChangedEventManager.AddHandler(cc, OnContainedCollectionChanged);
	}

	private void RemoveCollectionContainer(CollectionContainer cc)
	{
		CollectionChangedEventManager.RemoveHandler(cc, OnContainedCollectionChanged);
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
		}
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
		}
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}
	}

	internal void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, false, sources);
		foreach (object @internal in InternalList)
		{
			if (@internal is CollectionContainer collectionContainer)
			{
				collectionContainer.GetCollectionChangedSources(level + 1, format, sources);
			}
		}
	}
}
