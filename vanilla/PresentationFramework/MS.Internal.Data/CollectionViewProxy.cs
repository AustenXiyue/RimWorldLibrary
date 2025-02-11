using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class CollectionViewProxy : CollectionView, IEditableCollectionViewAddNewItem, IEditableCollectionView, ICollectionViewLiveShaping, IItemProperties
{
	private ICollectionView _view;

	private IndexedEnumerable _indexer;

	private ObservableCollection<string> _liveSortingProperties;

	private ObservableCollection<string> _liveFilteringProperties;

	private ObservableCollection<string> _liveGroupingProperties;

	public override CultureInfo Culture
	{
		get
		{
			return ProxiedView.Culture;
		}
		set
		{
			ProxiedView.Culture = value;
		}
	}

	public override IEnumerable SourceCollection => base.SourceCollection;

	public override Predicate<object> Filter
	{
		get
		{
			return ProxiedView.Filter;
		}
		set
		{
			ProxiedView.Filter = value;
		}
	}

	public override bool CanFilter => ProxiedView.CanFilter;

	public override SortDescriptionCollection SortDescriptions => ProxiedView.SortDescriptions;

	public override bool CanSort => ProxiedView.CanSort;

	public override bool CanGroup => ProxiedView.CanGroup;

	public override ObservableCollection<GroupDescription> GroupDescriptions => ProxiedView.GroupDescriptions;

	public override ReadOnlyObservableCollection<object> Groups => ProxiedView.Groups;

	public override object CurrentItem => ProxiedView.CurrentItem;

	public override int CurrentPosition => ProxiedView.CurrentPosition;

	public override bool IsCurrentAfterLast => ProxiedView.IsCurrentAfterLast;

	public override bool IsCurrentBeforeFirst => ProxiedView.IsCurrentBeforeFirst;

	public override int Count => EnumerableWrapper.Count;

	public override bool IsEmpty => ProxiedView.IsEmpty;

	public ICollectionView ProxiedView => _view;

	NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.NewItemPlaceholderPosition;
			}
			return NewItemPlaceholderPosition.None;
		}
		set
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				editableCollectionView.NewItemPlaceholderPosition = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "NewItemPlaceholderPosition"));
		}
	}

	bool IEditableCollectionView.CanAddNew
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CanAddNew;
			}
			return false;
		}
	}

	bool IEditableCollectionView.IsAddingNew
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.IsAddingNew;
			}
			return false;
		}
	}

	object IEditableCollectionView.CurrentAddItem
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CurrentAddItem;
			}
			return null;
		}
	}

	bool IEditableCollectionView.CanRemove
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CanRemove;
			}
			return false;
		}
	}

	bool IEditableCollectionView.CanCancelEdit
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CanCancelEdit;
			}
			return false;
		}
	}

	bool IEditableCollectionView.IsEditingItem
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.IsEditingItem;
			}
			return false;
		}
	}

	object IEditableCollectionView.CurrentEditItem
	{
		get
		{
			if (ProxiedView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CurrentEditItem;
			}
			return null;
		}
	}

	bool IEditableCollectionViewAddNewItem.CanAddNewItem
	{
		get
		{
			if (ProxiedView is IEditableCollectionViewAddNewItem editableCollectionViewAddNewItem)
			{
				return editableCollectionViewAddNewItem.CanAddNewItem;
			}
			return false;
		}
	}

	bool ICollectionViewLiveShaping.CanChangeLiveSorting
	{
		get
		{
			if (!(ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return false;
			}
			return collectionViewLiveShaping.CanChangeLiveSorting;
		}
	}

	bool ICollectionViewLiveShaping.CanChangeLiveFiltering
	{
		get
		{
			if (!(ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return false;
			}
			return collectionViewLiveShaping.CanChangeLiveFiltering;
		}
	}

	bool ICollectionViewLiveShaping.CanChangeLiveGrouping
	{
		get
		{
			if (!(ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return false;
			}
			return collectionViewLiveShaping.CanChangeLiveGrouping;
		}
	}

	bool? ICollectionViewLiveShaping.IsLiveSorting
	{
		get
		{
			if (!(ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return null;
			}
			return collectionViewLiveShaping.IsLiveSorting;
		}
		set
		{
			if (ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping)
			{
				collectionViewLiveShaping.IsLiveSorting = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.CannotChangeLiveShaping, "IsLiveSorting", "CanChangeLiveSorting"));
		}
	}

	bool? ICollectionViewLiveShaping.IsLiveFiltering
	{
		get
		{
			if (!(ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return null;
			}
			return collectionViewLiveShaping.IsLiveFiltering;
		}
		set
		{
			if (ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping)
			{
				collectionViewLiveShaping.IsLiveFiltering = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.CannotChangeLiveShaping, "IsLiveFiltering", "CanChangeLiveFiltering"));
		}
	}

	bool? ICollectionViewLiveShaping.IsLiveGrouping
	{
		get
		{
			if (!(ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return null;
			}
			return collectionViewLiveShaping.IsLiveGrouping;
		}
		set
		{
			if (ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping)
			{
				collectionViewLiveShaping.IsLiveGrouping = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.CannotChangeLiveShaping, "IsLiveGrouping", "CanChangeLiveGrouping"));
		}
	}

	ObservableCollection<string> ICollectionViewLiveShaping.LiveSortingProperties
	{
		get
		{
			if (ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping)
			{
				return collectionViewLiveShaping.LiveSortingProperties;
			}
			if (_liveSortingProperties == null)
			{
				_liveSortingProperties = new ObservableCollection<string>();
			}
			return _liveSortingProperties;
		}
	}

	ObservableCollection<string> ICollectionViewLiveShaping.LiveFilteringProperties
	{
		get
		{
			if (ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping)
			{
				return collectionViewLiveShaping.LiveFilteringProperties;
			}
			if (_liveFilteringProperties == null)
			{
				_liveFilteringProperties = new ObservableCollection<string>();
			}
			return _liveFilteringProperties;
		}
	}

	ObservableCollection<string> ICollectionViewLiveShaping.LiveGroupingProperties
	{
		get
		{
			if (ProxiedView is ICollectionViewLiveShaping collectionViewLiveShaping)
			{
				return collectionViewLiveShaping.LiveGroupingProperties;
			}
			if (_liveGroupingProperties == null)
			{
				_liveGroupingProperties = new ObservableCollection<string>();
			}
			return _liveGroupingProperties;
		}
	}

	ReadOnlyCollection<ItemPropertyInfo> IItemProperties.ItemProperties
	{
		get
		{
			if (ProxiedView is IItemProperties itemProperties)
			{
				return itemProperties.ItemProperties;
			}
			return null;
		}
	}

	private IndexedEnumerable EnumerableWrapper
	{
		get
		{
			if (_indexer == null)
			{
				IndexedEnumerable value = new IndexedEnumerable(ProxiedView, PassesFilter);
				Interlocked.CompareExchange(ref _indexer, value, null);
			}
			return _indexer;
		}
	}

	public override event CurrentChangingEventHandler CurrentChanging
	{
		add
		{
			PrivateCurrentChanging += value;
		}
		remove
		{
			PrivateCurrentChanging -= value;
		}
	}

	public override event EventHandler CurrentChanged
	{
		add
		{
			PrivateCurrentChanged += value;
		}
		remove
		{
			PrivateCurrentChanged -= value;
		}
	}

	private event CurrentChangingEventHandler PrivateCurrentChanging;

	private event EventHandler PrivateCurrentChanged;

	internal CollectionViewProxy(ICollectionView view)
		: base(view.SourceCollection, shouldProcessCollectionChanged: false)
	{
		_view = view;
		view.CollectionChanged += _OnViewChanged;
		view.CurrentChanging += _OnCurrentChanging;
		view.CurrentChanged += _OnCurrentChanged;
		if (view is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += _OnPropertyChanged;
		}
	}

	public override bool Contains(object item)
	{
		return ProxiedView.Contains(item);
	}

	public override void Refresh()
	{
		Interlocked.Exchange(ref _indexer, null)?.Invalidate();
		ProxiedView.Refresh();
	}

	public override IDisposable DeferRefresh()
	{
		return ProxiedView.DeferRefresh();
	}

	public override bool MoveCurrentToFirst()
	{
		return ProxiedView.MoveCurrentToFirst();
	}

	public override bool MoveCurrentToPrevious()
	{
		return ProxiedView.MoveCurrentToPrevious();
	}

	public override bool MoveCurrentToNext()
	{
		return ProxiedView.MoveCurrentToNext();
	}

	public override bool MoveCurrentToLast()
	{
		return ProxiedView.MoveCurrentToLast();
	}

	public override bool MoveCurrentTo(object item)
	{
		return ProxiedView.MoveCurrentTo(item);
	}

	public override bool MoveCurrentToPosition(int position)
	{
		return ProxiedView.MoveCurrentToPosition(position);
	}

	public override int IndexOf(object item)
	{
		return EnumerableWrapper.IndexOf(item);
	}

	public override bool PassesFilter(object item)
	{
		if (ProxiedView.CanFilter && ProxiedView.Filter != null && item != CollectionView.NewItemPlaceholder && item != ((IEditableCollectionView)this).CurrentAddItem)
		{
			return ProxiedView.Filter(item);
		}
		return true;
	}

	public override object GetItemAt(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return EnumerableWrapper[index];
	}

	public override void DetachFromSourceCollection()
	{
		if (_view != null)
		{
			_view.CollectionChanged -= _OnViewChanged;
			_view.CurrentChanging -= _OnCurrentChanging;
			_view.CurrentChanged -= _OnCurrentChanged;
			if (_view is INotifyPropertyChanged notifyPropertyChanged)
			{
				notifyPropertyChanged.PropertyChanged -= _OnPropertyChanged;
			}
			_view = null;
		}
		base.DetachFromSourceCollection();
	}

	object IEditableCollectionView.AddNew()
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			return editableCollectionView.AddNew();
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "AddNew"));
	}

	void IEditableCollectionView.CommitNew()
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CommitNew();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CommitNew"));
	}

	void IEditableCollectionView.CancelNew()
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CancelNew();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CancelNew"));
	}

	void IEditableCollectionView.RemoveAt(int index)
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.RemoveAt(index);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "RemoveAt"));
	}

	void IEditableCollectionView.Remove(object item)
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.Remove(item);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "Remove"));
	}

	void IEditableCollectionView.EditItem(object item)
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.EditItem(item);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "EditItem"));
	}

	void IEditableCollectionView.CommitEdit()
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CommitEdit();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CommitEdit"));
	}

	void IEditableCollectionView.CancelEdit()
	{
		if (ProxiedView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CancelEdit();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CancelEdit"));
	}

	object IEditableCollectionViewAddNewItem.AddNewItem(object newItem)
	{
		if (ProxiedView is IEditableCollectionViewAddNewItem editableCollectionViewAddNewItem)
		{
			return editableCollectionViewAddNewItem.AddNewItem(newItem);
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "AddNewItem"));
	}

	protected override IEnumerator GetEnumerator()
	{
		return ProxiedView.GetEnumerator();
	}

	internal override void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, false, sources);
		if (_view != null)
		{
			format(level + 1, _view, true, sources);
			object sourceCollection = _view.SourceCollection;
			if (sourceCollection != null)
			{
				format(level + 2, sourceCollection, null, sources);
			}
		}
	}

	private void _OnPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		OnPropertyChanged(args);
	}

	private void _OnViewChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		OnCollectionChanged(args);
	}

	private void _OnCurrentChanging(object sender, CurrentChangingEventArgs args)
	{
		if (this.PrivateCurrentChanging != null)
		{
			this.PrivateCurrentChanging(this, args);
		}
	}

	private void _OnCurrentChanged(object sender, EventArgs args)
	{
		if (this.PrivateCurrentChanged != null)
		{
			this.PrivateCurrentChanged(this, args);
		}
	}
}
