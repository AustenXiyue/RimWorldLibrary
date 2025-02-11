using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel;

/// <summary>Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.</summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
[Serializable]
[TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
	[Serializable]
	[TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
	private class SimpleMonitor : IDisposable
	{
		private int _busyCount;

		public bool Busy => _busyCount > 0;

		public void Enter()
		{
			_busyCount++;
		}

		public void Dispose()
		{
			_busyCount--;
		}
	}

	private const string CountString = "Count";

	private const string IndexerName = "Item[]";

	private SimpleMonitor _monitor = new SimpleMonitor();

	/// <summary>Occurs when a property value changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	/// <summary>Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.</summary>
	public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>Occurs when a property value changes.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> class.</summary>
	public ObservableCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> class that contains elements copied from the specified list.</summary>
	/// <param name="list">The list from which the elements are copied.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="list" /> parameter cannot be null.</exception>
	public ObservableCollection(List<T> list)
		: base((IList<T>)((list != null) ? new List<T>(list.Count) : list))
	{
		CopyFrom(list);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> class that contains elements copied from the specified collection.</summary>
	/// <param name="collection">The collection from which the elements are copied.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> parameter cannot be null.</exception>
	public ObservableCollection(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		CopyFrom(collection);
	}

	private void CopyFrom(IEnumerable<T> collection)
	{
		IList<T> list = base.Items;
		if (collection == null || list == null)
		{
			return;
		}
		foreach (T item in collection)
		{
			list.Add(item);
		}
	}

	/// <summary>Moves the item at the specified index to a new location in the collection.</summary>
	/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
	/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
	public void Move(int oldIndex, int newIndex)
	{
		MoveItem(oldIndex, newIndex);
	}

	/// <summary>Removes all items from the collection.</summary>
	protected override void ClearItems()
	{
		CheckReentrancy();
		base.ClearItems();
		OnPropertyChanged("Count");
		OnPropertyChanged("Item[]");
		OnCollectionReset();
	}

	/// <summary>Removes the item at the specified index of the collection.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	protected override void RemoveItem(int index)
	{
		CheckReentrancy();
		T val = base[index];
		base.RemoveItem(index);
		OnPropertyChanged("Count");
		OnPropertyChanged("Item[]");
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, val, index);
	}

	/// <summary>Inserts an item into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert.</param>
	protected override void InsertItem(int index, T item)
	{
		CheckReentrancy();
		base.InsertItem(index, item);
		OnPropertyChanged("Count");
		OnPropertyChanged("Item[]");
		OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
	}

	/// <summary>Replaces the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to replace.</param>
	/// <param name="item">The new value for the element at the specified index.</param>
	protected override void SetItem(int index, T item)
	{
		CheckReentrancy();
		T val = base[index];
		base.SetItem(index, item);
		OnPropertyChanged("Item[]");
		OnCollectionChanged(NotifyCollectionChangedAction.Replace, val, item, index);
	}

	/// <summary>Moves the item at the specified index to a new location in the collection.</summary>
	/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
	/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
	protected virtual void MoveItem(int oldIndex, int newIndex)
	{
		CheckReentrancy();
		T val = base[oldIndex];
		base.RemoveItem(oldIndex);
		base.InsertItem(newIndex, val);
		OnPropertyChanged("Item[]");
		OnCollectionChanged(NotifyCollectionChangedAction.Move, val, newIndex, oldIndex);
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.PropertyChanged" /> event with the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event with the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (this.CollectionChanged != null)
		{
			using (BlockReentrancy())
			{
				this.CollectionChanged(this, e);
			}
		}
	}

	/// <summary>Disallows reentrant attempts to change this collection.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that can be used to dispose of the object.</returns>
	protected IDisposable BlockReentrancy()
	{
		_monitor.Enter();
		return _monitor;
	}

	/// <summary>Checks for reentrant attempts to change this collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">If there was a call to <see cref="M:System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy" /> of which the <see cref="T:System.IDisposable" /> return value has not yet been disposed of. Typically, this means when there are additional attempts to change this collection during a <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event. However, it depends on when derived classes choose to call <see cref="M:System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy" />.</exception>
	protected void CheckReentrancy()
	{
		if (_monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot change ObservableCollection during a CollectionChanged event."));
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
	}

	private void OnCollectionReset()
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}
}
