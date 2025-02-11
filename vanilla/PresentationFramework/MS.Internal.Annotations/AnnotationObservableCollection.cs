using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace MS.Internal.Annotations;

internal class AnnotationObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged2, IOwnedObject
{
	private readonly PropertyChangedEventHandler _listener;

	internal readonly string CountString = "Count";

	internal readonly string IndexerName = "Item[]";

	public AnnotationObservableCollection()
	{
		_listener = OnItemPropertyChanged;
	}

	public AnnotationObservableCollection(List<T> list)
		: base(list)
	{
		_listener = OnItemPropertyChanged;
	}

	protected override void ClearItems()
	{
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				INotifyPropertyChanged2 item = enumerator.Current;
				SetOwned(item, owned: false);
			}
		}
		ProtectedClearItems();
	}

	protected override void RemoveItem(int index)
	{
		T val = base[index];
		SetOwned(val, owned: false);
		base.RemoveItem(index);
	}

	protected override void InsertItem(int index, T item)
	{
		if (ItemOwned(item))
		{
			throw new ArgumentException(SR.AlreadyHasParent);
		}
		base.InsertItem(index, item);
		SetOwned(item, owned: true);
	}

	protected override void SetItem(int index, T item)
	{
		if (ItemOwned(item))
		{
			throw new ArgumentException(SR.AlreadyHasParent);
		}
		T val = base[index];
		SetOwned(val, owned: false);
		ProtectedSetItem(index, item);
		SetOwned(item, owned: true);
	}

	protected virtual void ProtectedClearItems()
	{
		base.ClearItems();
	}

	protected virtual void ProtectedSetItem(int index, T item)
	{
		base.Items[index] = item;
		OnPropertyChanged(new PropertyChangedEventArgs(CountString));
		OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected void ObservableCollectionSetItem(int index, T item)
	{
		base.SetItem(index, item);
	}

	protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	private bool ItemOwned(object item)
	{
		if (item != null)
		{
			return (item as IOwnedObject).Owned;
		}
		return false;
	}

	private void SetOwned(object item, bool owned)
	{
		if (item != null)
		{
			(item as IOwnedObject).Owned = owned;
			if (owned)
			{
				((INotifyPropertyChanged2)item).PropertyChanged += _listener;
			}
			else
			{
				((INotifyPropertyChanged2)item).PropertyChanged -= _listener;
			}
		}
	}
}
