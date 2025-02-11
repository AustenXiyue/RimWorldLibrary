using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.ComponentModel;

/// <summary>Represents a collection of <see cref="T:System.ComponentModel.SortDescription" /> objects.</summary>
public class SortDescriptionCollection : Collection<SortDescription>, INotifyCollectionChanged
{
	private class EmptySortDescriptionCollection : SortDescriptionCollection, IList, ICollection, IEnumerable
	{
		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => true;

		protected override void ClearItems()
		{
			throw new NotSupportedException();
		}

		protected override void RemoveItem(int index)
		{
			throw new NotSupportedException();
		}

		protected override void InsertItem(int index, SortDescription item)
		{
			throw new NotSupportedException();
		}

		protected override void SetItem(int index, SortDescription item)
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets an empty and non-modifiable instance of <see cref="T:System.ComponentModel.SortDescriptionCollection" />. </summary>
	public static readonly SortDescriptionCollection Empty = new EmptySortDescriptionCollection();

	/// <summary>Occurs when an item is added or removed.</summary>
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

	/// <summary>Occurs when an item is added or removed.</summary>
	protected event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>Removes all items from the collection.</summary>
	protected override void ClearItems()
	{
		base.ClearItems();
		OnCollectionChanged(NotifyCollectionChangedAction.Reset);
	}

	/// <summary>Removes the item at the specified index in the collection.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	protected override void RemoveItem(int index)
	{
		SortDescription sortDescription = base[index];
		base.RemoveItem(index);
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, sortDescription, index);
	}

	/// <summary>Inserts an item into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index where the <paramref name="item" /> is inserted.</param>
	/// <param name="item">The object to insert.</param>
	protected override void InsertItem(int index, SortDescription item)
	{
		item.Seal();
		base.InsertItem(index, item);
		OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
	}

	/// <summary>Replaces the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to replace.</param>
	/// <param name="item">The new value for the element at the specified index.</param>
	protected override void SetItem(int index, SortDescription item)
	{
		item.Seal();
		SortDescription sortDescription = base[index];
		base.SetItem(index, item);
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, sortDescription, index);
		OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
		}
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.SortDescriptionCollection" /> class.</summary>
	public SortDescriptionCollection()
	{
	}
}
