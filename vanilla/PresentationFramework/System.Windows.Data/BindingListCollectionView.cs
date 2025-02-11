using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Represents the <see cref="T:System.Windows.Data.CollectionView" /> for collections that implement <see cref="T:System.ComponentModel.IBindingList" />, such as Microsoft ActiveX Data Objects (ADO) data views.</summary>
public sealed class BindingListCollectionView : CollectionView, IComparer, IEditableCollectionView, ICollectionViewLiveShaping, IItemProperties
{
	private class BindingListSortDescriptionCollection : SortDescriptionCollection
	{
		private bool _allowMultipleDescriptions;

		internal BindingListSortDescriptionCollection(bool allowMultipleDescriptions)
		{
			_allowMultipleDescriptions = allowMultipleDescriptions;
		}

		protected override void InsertItem(int index, SortDescription item)
		{
			if (!_allowMultipleDescriptions && base.Count > 0)
			{
				throw new InvalidOperationException(SR.BindingListCanOnlySortByOneProperty);
			}
			base.InsertItem(index, item);
		}
	}

	private IBindingList _internalList;

	private CollectionViewGroupRoot _group;

	private bool _isGrouping;

	private IBindingListView _blv;

	private BindingListSortDescriptionCollection _sort;

	private IList _shadowList;

	private bool _isSorted;

	private IComparer _comparer;

	private string _customFilter;

	private bool _isFiltered;

	private bool _ignoreInnerRefresh;

	private bool? _itemsRaisePropertyChanged;

	private bool _isDataView;

	private object _newItem = CollectionView.NoNewItem;

	private object _editItem;

	private int _newItemIndex;

	private NewItemPlaceholderPosition _newItemPlaceholderPosition;

	private List<Action> _deferredActions;

	private bool _isRemoving;

	private bool? _isLiveGrouping = false;

	private bool _isLiveShapingDirty;

	private ObservableCollection<string> _liveSortingProperties;

	private ObservableCollection<string> _liveFilteringProperties;

	private ObservableCollection<string> _liveGroupingProperties;

	private IList _cachedList;

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describes how the items in the collection are sorted in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describe how the items in the collection are sorted in the view.</returns>
	public override SortDescriptionCollection SortDescriptions
	{
		get
		{
			if (InternalList.SupportsSorting)
			{
				if (_sort == null)
				{
					bool allowMultipleDescriptions = _blv != null && _blv.SupportsAdvancedSorting;
					_sort = new BindingListSortDescriptionCollection(allowMultipleDescriptions);
					((INotifyCollectionChanged)_sort).CollectionChanged += SortDescriptionsChanged;
				}
				return _sort;
			}
			return SortDescriptionCollection.Empty;
		}
	}

	/// <summary>Gets a value that indicates whether the collection supports sorting.</summary>
	/// <returns>For a default instance of <see cref="T:System.Windows.Data.BindingListCollectionView" /> this property always returns true.</returns>
	public override bool CanSort => InternalList.SupportsSorting;

	private IComparer ActiveComparer
	{
		get
		{
			return _comparer;
		}
		set
		{
			_comparer = value;
		}
	}

	/// <summary>Gets a value that indicates whether the view supports callback-based filtering.</summary>
	/// <returns>This property always returns false.</returns>
	public override bool CanFilter => false;

	/// <summary>Gets or sets a custom filter.</summary>
	/// <returns>A string that specifies how the items are filtered.</returns>
	public string CustomFilter
	{
		get
		{
			return _customFilter;
		}
		set
		{
			if (!CanCustomFilter)
			{
				throw new NotSupportedException(SR.BindingListCannotCustomFilter);
			}
			if (IsAddingNew || IsEditingItem)
			{
				throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "CustomFilter"));
			}
			if (base.AllowsCrossThreadChanges)
			{
				VerifyAccess();
			}
			_customFilter = value;
			RefreshOrDefer();
		}
	}

	/// <summary>Gets a value that indicates whether the view supports custom filtering.</summary>
	/// <returns>true if the view supports custom filtering; otherwise, false.</returns>
	public bool CanCustomFilter
	{
		get
		{
			if (_blv != null)
			{
				return _blv.SupportsFiltering;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the view supports grouping.</summary>
	/// <returns>For a default instance of <see cref="T:System.Windows.Data.BindingListCollectionView" /> this property always returns true.</returns>
	public override bool CanGroup => true;

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describe how the items in the collection are grouped in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describe how the items in the collection are grouped in the view.</returns>
	public override ObservableCollection<GroupDescription> GroupDescriptions => _group.GroupDescriptions;

	/// <summary>Gets the top-level groups.</summary>
	/// <returns>A read-only collection of the top-level groups, or null if there are no groups.</returns>
	public override ReadOnlyObservableCollection<object> Groups
	{
		get
		{
			if (!_isGrouping)
			{
				return null;
			}
			return _group.Items;
		}
	}

	/// <summary>Gets or sets a delegate to select the <see cref="T:System.ComponentModel.GroupDescription" /> as a function of the parent group and its level. </summary>
	/// <returns>A method that provides the logic for the selection of the <see cref="T:System.ComponentModel.GroupDescription" /> as a function of the parent group and its level. The default is null.</returns>
	[DefaultValue(null)]
	public GroupDescriptionSelectorCallback GroupBySelector
	{
		get
		{
			return _group.GroupBySelector;
		}
		set
		{
			if (!CanGroup)
			{
				throw new NotSupportedException();
			}
			if (IsAddingNew || IsEditingItem)
			{
				throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "GroupBySelector"));
			}
			_group.GroupBySelector = value;
			RefreshOrDefer();
		}
	}

	/// <summary>Gets the estimated number of records in the collection. </summary>
	/// <returns>One of the following:ValueMeaning-1Could not determine the count of the collection. This might be returned by a "virtualizing" view, where the view deliberately does not account for all items in the underlying collection because the view is attempting to increase efficiency and minimize dependence on always having the entire collection available.any other integerThe count of the collection.</returns>
	public override int Count
	{
		get
		{
			VerifyRefreshNotDeferred();
			return InternalCount;
		}
	}

	/// <summary>Returns a value that indicates whether the resulting (filtered) view is empty.</summary>
	/// <returns>true if the resulting view is empty; otherwise, false.</returns>
	public override bool IsEmpty
	{
		get
		{
			if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.None)
			{
				return CollectionProxy.Count == 0;
			}
			return false;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the list of items (after applying the sort and filters, if any) is already in the correct order for grouping.</summary>
	/// <returns>true if the list of items is already in the correct order for grouping; otherwise, false.</returns>
	public bool IsDataInGroupOrder
	{
		get
		{
			return _group.IsDataInGroupOrder;
		}
		set
		{
			_group.IsDataInGroupOrder = value;
		}
	}

	/// <summary>Gets or sets the position of the new item placeholder in the <see cref="T:System.Windows.Data.BindingListCollectionView" />.</summary>
	/// <returns>One of the enumeration values that specifies the position of the new item placeholder in the <see cref="T:System.Windows.Data.BindingListCollectionView" />.</returns>
	public NewItemPlaceholderPosition NewItemPlaceholderPosition
	{
		get
		{
			return _newItemPlaceholderPosition;
		}
		set
		{
			VerifyRefreshNotDeferred();
			if (value != _newItemPlaceholderPosition && IsAddingNew)
			{
				throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringTransaction, "NewItemPlaceholderPosition", "AddNew"));
			}
			if (value != _newItemPlaceholderPosition && _isRemoving)
			{
				DeferAction(delegate
				{
					NewItemPlaceholderPosition = value;
				});
				return;
			}
			NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null;
			int num = -1;
			int num2 = -1;
			switch (value)
			{
			case NewItemPlaceholderPosition.None:
				switch (_newItemPlaceholderPosition)
				{
				case NewItemPlaceholderPosition.AtBeginning:
					num = 0;
					notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, CollectionView.NewItemPlaceholder, num);
					break;
				case NewItemPlaceholderPosition.AtEnd:
					num = InternalCount - 1;
					notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, CollectionView.NewItemPlaceholder, num);
					break;
				}
				break;
			case NewItemPlaceholderPosition.AtBeginning:
				switch (_newItemPlaceholderPosition)
				{
				case NewItemPlaceholderPosition.None:
					num2 = 0;
					notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, CollectionView.NewItemPlaceholder, num2);
					break;
				case NewItemPlaceholderPosition.AtEnd:
					num = InternalCount - 1;
					num2 = 0;
					notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, CollectionView.NewItemPlaceholder, num2, num);
					break;
				}
				break;
			case NewItemPlaceholderPosition.AtEnd:
				switch (_newItemPlaceholderPosition)
				{
				case NewItemPlaceholderPosition.None:
					num2 = InternalCount;
					notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, CollectionView.NewItemPlaceholder, num2);
					break;
				case NewItemPlaceholderPosition.AtBeginning:
					num = 0;
					num2 = InternalCount - 1;
					notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, CollectionView.NewItemPlaceholder, num2, num);
					break;
				}
				break;
			}
			if (notifyCollectionChangedEventArgs == null)
			{
				return;
			}
			_newItemPlaceholderPosition = value;
			if (!_isGrouping)
			{
				OnCollectionChanged(null, notifyCollectionChangedEventArgs);
			}
			else
			{
				if (num >= 0)
				{
					int index = ((num != 0) ? (_group.Items.Count - 1) : 0);
					_group.RemoveSpecialItem(index, CollectionView.NewItemPlaceholder, loading: false);
				}
				if (num2 >= 0)
				{
					int index2 = ((num2 != 0) ? _group.Items.Count : 0);
					_group.InsertSpecialItem(index2, CollectionView.NewItemPlaceholder, loading: false);
				}
			}
			OnPropertyChanged("NewItemPlaceholderPosition");
		}
	}

	/// <summary>Gets a value that indicates whether a new item can be added to the collection.</summary>
	/// <returns>true if a new item can be added to the collection; otherwise, false.</returns>
	public bool CanAddNew
	{
		get
		{
			if (!IsEditingItem)
			{
				return InternalList.AllowNew;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether an add transaction is in progress.</summary>
	/// <returns>true if an add transaction is in progress; otherwise, false.</returns>
	public bool IsAddingNew => _newItem != CollectionView.NoNewItem;

	/// <summary>Gets the item that is being added during the current add transaction.</summary>
	/// <returns>The item that is being added if <see cref="P:System.Windows.Data.BindingListCollectionView.IsAddingNew" /> is true; otherwise, null.</returns>
	public object CurrentAddItem
	{
		get
		{
			if (!IsAddingNew)
			{
				return null;
			}
			return _newItem;
		}
	}

	/// <summary>Gets a value that indicates whether an item can be removed from the collection.</summary>
	/// <returns>true if an item can be removed from the collection; otherwise, false.</returns>
	public bool CanRemove
	{
		get
		{
			if (!IsEditingItem && !IsAddingNew)
			{
				return InternalList.AllowRemove;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view can discard pending changes and restore the original values of an edited object.</summary>
	/// <returns>true if the collection view can discard pending changes and restore the original values of an edited object; otherwise, false.</returns>
	public bool CanCancelEdit => _editItem is IEditableObject;

	/// <summary>Gets a value that indicates whether an edit transaction is in progress.</summary>
	/// <returns>true if an edit transaction is in progress; otherwise, false.</returns>
	public bool IsEditingItem => _editItem != null;

	/// <summary>Gets the item in the collection that is being edited.</summary>
	/// <returns>The item in the collection that is being edited if <see cref="P:System.Windows.Data.ListCollectionView.IsEditingItem" /> is true; otherwise, null.</returns>
	public object CurrentEditItem => _editItem;

	/// <summary>Gets a value that indicates whether this view supports turning sorting data in real time on or off.</summary>
	/// <returns>false in all cases.</returns>
	public bool CanChangeLiveSorting => false;

	/// <summary>Gets a value that indicates whether this view supports turning filtering data in real time on or off.</summary>
	/// <returns>false in all cases.</returns>
	public bool CanChangeLiveFiltering => false;

	/// <summary>Gets a value that indicates whether this view supports turning grouping data in real time on or off.</summary>
	/// <returns>True in all cases.</returns>
	public bool CanChangeLiveGrouping => true;

	/// <summary>Gets or sets a value that indicates whether sorting data in real time is enabled.</summary>
	/// <returns>true if sorting data in real time is enabled; false if live sorting is not enabled; null if it cannot be determined whether the collection view implements live sorting.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Data.BindingListCollectionView.IsLiveSorting" /> cannot be set.</exception>
	public bool? IsLiveSorting
	{
		get
		{
			if (!IsDataView)
			{
				return null;
			}
			return true;
		}
		set
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeLiveShaping, "IsLiveSorting", "CanChangeLiveSorting"));
		}
	}

	/// <summary>Gets or sets a value that indicates whether filtering data in real time is enabled.</summary>
	/// <returns>true if filtering data in real time is enabled; false if live filtering is not enabled; null if it cannot be determined whether the collection view implements live filtering.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Data.BindingListCollectionView.IsLiveFiltering" /> cannot be set.</exception>
	public bool? IsLiveFiltering
	{
		get
		{
			if (!IsDataView)
			{
				return null;
			}
			return true;
		}
		set
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeLiveShaping, "IsLiveFiltering", "CanChangeLiveFiltering"));
		}
	}

	/// <summary>Gets or sets a value that indicates whether grouping data in real time is enabled.</summary>
	/// <returns>true if grouping data in real time is enabled; false if live grouping is not enabled; null if it cannot be determined whether the collection view implements live grouping.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Windows.Data.BindingListCollectionView.IsLiveGrouping" /> cannot be set to null.</exception>
	public bool? IsLiveGrouping
	{
		get
		{
			return _isLiveGrouping;
		}
		set
		{
			if (!value.HasValue)
			{
				throw new ArgumentNullException("value");
			}
			if (value != _isLiveGrouping)
			{
				_isLiveGrouping = value;
				RefreshOrDefer();
				OnPropertyChanged("IsLiveGrouping");
			}
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in sorting data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in sorting data in real time.</returns>
	public ObservableCollection<string> LiveSortingProperties
	{
		get
		{
			if (_liveSortingProperties == null)
			{
				_liveSortingProperties = new ObservableCollection<string>();
			}
			return _liveSortingProperties;
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in filtering data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in filtering data in real time.</returns>
	public ObservableCollection<string> LiveFilteringProperties
	{
		get
		{
			if (_liveFilteringProperties == null)
			{
				_liveFilteringProperties = new ObservableCollection<string>();
			}
			return _liveFilteringProperties;
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in grouping data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in grouping data in real time.</returns>
	public ObservableCollection<string> LiveGroupingProperties
	{
		get
		{
			if (_liveGroupingProperties == null)
			{
				_liveGroupingProperties = new ObservableCollection<string>();
				_liveGroupingProperties.CollectionChanged += OnLivePropertyListChanged;
			}
			return _liveGroupingProperties;
		}
	}

	/// <summary>Gets a collection of objects that describes the properties of the items in the collection.</summary>
	/// <returns>A collection of objects that describes the properties of the items in the collection.</returns>
	public ReadOnlyCollection<ItemPropertyInfo> ItemProperties => GetItemProperties();

	private int InternalCount
	{
		get
		{
			if (_isGrouping)
			{
				return _group.ItemCount;
			}
			return ((NewItemPlaceholderPosition != NewItemPlaceholderPosition.None) ? 1 : 0) + CollectionProxy.Count;
		}
	}

	private bool IsDataView => _isDataView;

	private bool IsCurrentInView
	{
		get
		{
			if (0 <= CurrentPosition)
			{
				return CurrentPosition < InternalCount;
			}
			return false;
		}
	}

	private IList CollectionProxy
	{
		get
		{
			if (_shadowList != null)
			{
				return _shadowList;
			}
			return InternalList;
		}
	}

	private IBindingList InternalList
	{
		get
		{
			return _internalList;
		}
		set
		{
			_internalList = value;
		}
	}

	private bool IsCustomFilterSet
	{
		get
		{
			if (_blv != null)
			{
				return !string.IsNullOrEmpty(_customFilter);
			}
			return false;
		}
	}

	private bool CanGroupNamesChange => true;

	internal bool IsLiveShapingDirty
	{
		get
		{
			return _isLiveShapingDirty;
		}
		set
		{
			if (value != _isLiveShapingDirty)
			{
				_isLiveShapingDirty = value;
				if (value)
				{
					base.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(RestoreLiveShaping));
				}
			}
		}
	}

	/// <summary>Initializes an instance of <see cref="T:System.Windows.Data.BindingListCollectionView" /> over the given list.</summary>
	/// <param name="list">The underlying <see cref="T:System.ComponentModel.IBindingList" />.</param>
	public BindingListCollectionView(IBindingList list)
		: base(list)
	{
		InternalList = list;
		_blv = list as IBindingListView;
		_isDataView = SystemDataHelper.IsDataView(list);
		SubscribeToChanges();
		_group = new CollectionViewGroupRoot(this);
		_group.GroupDescriptionChanged += OnGroupDescriptionChanged;
		((INotifyCollectionChanged)_group).CollectionChanged += OnGroupChanged;
		((INotifyCollectionChanged)_group.GroupDescriptions).CollectionChanged += OnGroupByChanged;
	}

	/// <summary>Returns a value that indicates whether the specified item in the underlying collection belongs to the view.</summary>
	/// <returns>true if the specified item belongs to the view or if there is not filter set on the collection view; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	public override bool PassesFilter(object item)
	{
		if (IsCustomFilterSet)
		{
			return Contains(item);
		}
		return true;
	}

	/// <summary>Returns a value that indicates whether a given item belongs to the collection view.</summary>
	/// <returns>true if the item belongs to the collection view; otherwise, false.</returns>
	/// <param name="item">The object to check.</param>
	public override bool Contains(object item)
	{
		VerifyRefreshNotDeferred();
		if (item != CollectionView.NewItemPlaceholder)
		{
			return CollectionProxy.Contains(item);
		}
		return NewItemPlaceholderPosition != NewItemPlaceholderPosition.None;
	}

	/// <summary>Sets the item at the specified index to be the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> in the view.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	/// <param name="position">The index to set the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> to.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the index is out of range. </exception>
	public override bool MoveCurrentToPosition(int position)
	{
		VerifyRefreshNotDeferred();
		if (position < -1 || position > InternalCount)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		_MoveTo(position);
		return IsCurrentInView;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Less than zero means <paramref name="o1" /> is less than <paramref name="o2" />, a value of zero means they are equal, and over zero means <paramref name="o1" /> is greater than <paramref name="o2" />.</returns>
	/// <param name="o1">  First object to compare.</param>
	/// <param name="o2">  Second object to compare.</param>
	int IComparer.Compare(object o1, object o2)
	{
		int num = InternalIndexOf(o1);
		int num2 = InternalIndexOf(o2);
		return num - num2;
	}

	/// <summary>Returns the index at which the given item belongs in the collection view.</summary>
	/// <returns>The index of the item in the collection, or -1 if the item does not exist in the collection view.</returns>
	/// <param name="item">The object to look for in the collection.</param>
	public override int IndexOf(object item)
	{
		VerifyRefreshNotDeferred();
		return InternalIndexOf(item);
	}

	/// <summary>Retrieves the item at the specified position in the view.</summary>
	/// <returns>The item at the specified position in the view.</returns>
	/// <param name="index">The zero-based index at which the item is located.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">If <paramref name="index" /> is out of range.</exception>
	public override object GetItemAt(int index)
	{
		VerifyRefreshNotDeferred();
		return InternalItemAt(index);
	}

	protected override IEnumerator GetEnumerator()
	{
		VerifyRefreshNotDeferred();
		return InternalGetEnumerator();
	}

	/// <summary>Detaches the underlying collection from this collection view to enable the collection view to be garbage collected.</summary>
	public override void DetachFromSourceCollection()
	{
		if (InternalList != null && InternalList.SupportsChangeNotification)
		{
			InternalList.ListChanged -= OnListChanged;
		}
		InternalList = null;
		base.DetachFromSourceCollection();
	}

	/// <summary>Starts an add transaction and returns the pending new item.</summary>
	/// <returns>The pending new item.</returns>
	public object AddNew()
	{
		VerifyRefreshNotDeferred();
		if (IsEditingItem)
		{
			CommitEdit();
		}
		CommitNew();
		if (!CanAddNew)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "AddNew"));
		}
		object newItem = null;
		BindingOperations.AccessCollection(InternalList, delegate
		{
			ProcessPendingChanges();
			_newItemIndex = -2;
			newItem = InternalList.AddNew();
		}, writeAccess: true);
		MoveCurrentTo(newItem);
		if (newItem is ISupportInitialize supportInitialize)
		{
			supportInitialize.BeginInit();
		}
		if (!IsDataView && newItem is IEditableObject editableObject)
		{
			editableObject.BeginEdit();
		}
		return newItem;
	}

	private void BeginAddNew(object newItem, int index)
	{
		SetNewItem(newItem);
		_newItemIndex = index;
		int index2 = index;
		if (!_isGrouping)
		{
			switch (NewItemPlaceholderPosition)
			{
			case NewItemPlaceholderPosition.AtBeginning:
				_newItemIndex--;
				index2 = 1;
				break;
			case NewItemPlaceholderPosition.AtEnd:
				index2 = InternalCount - 2;
				break;
			}
		}
		ProcessCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, index2));
	}

	/// <summary>Ends the add transaction and saves the pending new item.</summary>
	public void CommitNew()
	{
		if (IsEditingItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringTransaction, "CommitNew", "EditItem"));
		}
		VerifyRefreshNotDeferred();
		if (_newItem == CollectionView.NoNewItem)
		{
			return;
		}
		ICancelAddNew ican = InternalList as ICancelAddNew;
		IEditableObject ieo;
		BindingOperations.AccessCollection(InternalList, delegate
		{
			ProcessPendingChanges();
			if (ican != null)
			{
				ican.EndNew(_newItemIndex);
			}
			else if ((ieo = _newItem as IEditableObject) != null)
			{
				ieo.EndEdit();
			}
		}, writeAccess: true);
		if (_newItem != CollectionView.NoNewItem)
		{
			int num = ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? 1 : 0);
			NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = ProcessCommitNew(_newItemIndex, _newItemIndex + num);
			if (notifyCollectionChangedEventArgs != null)
			{
				OnCollectionChanged(InternalList, notifyCollectionChangedEventArgs);
			}
		}
	}

	/// <summary>Ends the add transaction and discards the pending new item.</summary>
	public void CancelNew()
	{
		if (IsEditingItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringTransaction, "CancelNew", "EditItem"));
		}
		VerifyRefreshNotDeferred();
		if (_newItem == CollectionView.NoNewItem)
		{
			return;
		}
		ICancelAddNew ican = InternalList as ICancelAddNew;
		IEditableObject ieo;
		BindingOperations.AccessCollection(InternalList, delegate
		{
			ProcessPendingChanges();
			if (ican != null)
			{
				ican.CancelNew(_newItemIndex);
			}
			else if ((ieo = _newItem as IEditableObject) != null)
			{
				ieo.CancelEdit();
			}
		}, writeAccess: true);
		_ = _newItem;
		_ = CollectionView.NoNewItem;
	}

	private object EndAddNew(bool cancel)
	{
		object newItem = _newItem;
		SetNewItem(CollectionView.NoNewItem);
		if (newItem is IEditableObject editableObject)
		{
			if (cancel)
			{
				editableObject.CancelEdit();
			}
			else
			{
				editableObject.EndEdit();
			}
		}
		if (newItem is ISupportInitialize supportInitialize)
		{
			supportInitialize.EndInit();
		}
		return newItem;
	}

	private NotifyCollectionChangedEventArgs ProcessCommitNew(int fromIndex, int toIndex)
	{
		if (_isGrouping)
		{
			CommitNewForGrouping();
			return null;
		}
		switch (NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.AtBeginning:
			fromIndex = 1;
			break;
		case NewItemPlaceholderPosition.AtEnd:
			fromIndex = InternalCount - 2;
			break;
		}
		object changedItem = EndAddNew(cancel: false);
		NotifyCollectionChangedEventArgs result = null;
		if (fromIndex != toIndex)
		{
			result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItem, toIndex, fromIndex);
		}
		return result;
	}

	private void CommitNewForGrouping()
	{
		int index = NewItemPlaceholderPosition switch
		{
			NewItemPlaceholderPosition.AtBeginning => 1, 
			NewItemPlaceholderPosition.AtEnd => _group.Items.Count - 2, 
			_ => _group.Items.Count - 1, 
		};
		object item = EndAddNew(cancel: false);
		_group.RemoveSpecialItem(index, item, loading: false);
		AddItemToGroups(item);
	}

	private void SetNewItem(object item)
	{
		if (!ItemsControl.EqualsEx(item, _newItem))
		{
			_newItem = item;
			OnPropertyChanged("CurrentAddItem");
			OnPropertyChanged("IsAddingNew");
			OnPropertyChanged("CanRemove");
		}
	}

	/// <summary>Removes the item at the specified position from the collection.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0 or greater than the number of items in the collection view.</exception>
	public void RemoveAt(int index)
	{
		if (IsEditingItem || IsAddingNew)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "RemoveAt"));
		}
		VerifyRefreshNotDeferred();
		RemoveImpl(GetItemAt(index), index);
	}

	/// <summary>Removes the specified item from the collection.</summary>
	/// <param name="item">The item to remove.</param>
	public void Remove(object item)
	{
		if (IsEditingItem || IsAddingNew)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Remove"));
		}
		VerifyRefreshNotDeferred();
		int num = InternalIndexOf(item);
		if (num >= 0)
		{
			RemoveImpl(item, num);
		}
	}

	private void RemoveImpl(object item, int index)
	{
		if (item == CollectionView.NewItemPlaceholder)
		{
			throw new InvalidOperationException(SR.RemovingPlaceholder);
		}
		BindingOperations.AccessCollection(InternalList, delegate
		{
			ProcessPendingChanges();
			if (index >= InternalList.Count || !ItemsControl.EqualsEx(item, GetItemAt(index)))
			{
				index = InternalList.IndexOf(item);
				if (index < 0)
				{
					return;
				}
			}
			if (_isGrouping)
			{
				index = InternalList.IndexOf(item);
			}
			else
			{
				int num = ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? 1 : 0);
				index -= num;
			}
			try
			{
				_isRemoving = true;
				InternalList.RemoveAt(index);
			}
			finally
			{
				_isRemoving = false;
				DoDeferredActions();
			}
		}, writeAccess: true);
	}

	/// <summary>Begins an edit transaction of the specified item.</summary>
	/// <param name="item">The item to edit.</param>
	public void EditItem(object item)
	{
		VerifyRefreshNotDeferred();
		if (item == CollectionView.NewItemPlaceholder)
		{
			throw new ArgumentException(SR.CannotEditPlaceholder, "item");
		}
		if (IsAddingNew)
		{
			if (ItemsControl.EqualsEx(item, _newItem))
			{
				return;
			}
			CommitNew();
		}
		CommitEdit();
		SetEditItem(item);
		if (item is IEditableObject editableObject)
		{
			editableObject.BeginEdit();
		}
	}

	/// <summary>Ends the edit transaction and saves the pending changes.</summary>
	public void CommitEdit()
	{
		if (IsAddingNew)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringTransaction, "CommitEdit", "AddNew"));
		}
		VerifyRefreshNotDeferred();
		if (_editItem == null)
		{
			return;
		}
		IEditableObject ieo = _editItem as IEditableObject;
		object editItem = _editItem;
		SetEditItem(null);
		if (ieo != null)
		{
			BindingOperations.AccessCollection(InternalList, delegate
			{
				ProcessPendingChanges();
				ieo.EndEdit();
			}, writeAccess: true);
		}
		if (_isGrouping)
		{
			RemoveItemFromGroups(editItem);
			AddItemToGroups(editItem);
		}
	}

	/// <summary>Ends the edit transaction and, if possible, restores the original value to the item.</summary>
	public void CancelEdit()
	{
		if (IsAddingNew)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringTransaction, "CancelEdit", "AddNew"));
		}
		VerifyRefreshNotDeferred();
		if (_editItem != null)
		{
			IEditableObject editableObject = _editItem as IEditableObject;
			SetEditItem(null);
			if (editableObject == null)
			{
				throw new InvalidOperationException(SR.CancelEditNotSupported);
			}
			editableObject.CancelEdit();
		}
	}

	private void ImplicitlyCancelEdit()
	{
		IEditableObject editableObject = _editItem as IEditableObject;
		SetEditItem(null);
		editableObject?.CancelEdit();
	}

	private void SetEditItem(object item)
	{
		if (!ItemsControl.EqualsEx(item, _editItem))
		{
			_editItem = item;
			OnPropertyChanged("CurrentEditItem");
			OnPropertyChanged("IsEditingItem");
			OnPropertyChanged("CanCancelEdit");
			OnPropertyChanged("CanAddNew");
			OnPropertyChanged("CanRemove");
		}
	}

	private void OnLivePropertyListChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsLiveGrouping == true)
		{
			RefreshOrDefer();
		}
	}

	protected override void RefreshOverride()
	{
		object currentItem = CurrentItem;
		int num = ((!IsEmpty) ? CurrentPosition : 0);
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		OnCurrentChanging();
		_ignoreInnerRefresh = true;
		if (IsCustomFilterSet || _isFiltered)
		{
			BindingOperations.AccessCollection(InternalList, delegate
			{
				if (IsCustomFilterSet)
				{
					_isFiltered = true;
					_blv.Filter = _customFilter;
				}
				else if (_isFiltered)
				{
					_isFiltered = false;
					_blv.RemoveFilter();
				}
			}, writeAccess: true);
		}
		if (_sort != null && _sort.Count > 0 && CollectionProxy != null && CollectionProxy.Count > 0)
		{
			ListSortDescriptionCollection sorts = ConvertSortDescriptionCollection(_sort);
			if (sorts.Count > 0)
			{
				_isSorted = true;
				BindingOperations.AccessCollection(InternalList, delegate
				{
					if (_blv == null)
					{
						InternalList.ApplySort(sorts[0].PropertyDescriptor, sorts[0].SortDirection);
					}
					else
					{
						_blv.ApplySort(sorts);
					}
				}, writeAccess: true);
			}
			ActiveComparer = new SortFieldComparer(_sort, Culture);
		}
		else if (_isSorted)
		{
			_isSorted = false;
			BindingOperations.AccessCollection(InternalList, delegate
			{
				InternalList.RemoveSort();
			}, writeAccess: true);
			ActiveComparer = null;
		}
		InitializeGrouping();
		PrepareCachedList();
		PrepareGroups();
		if (isCurrentBeforeFirst || IsEmpty)
		{
			SetCurrent(null, -1);
		}
		else if (isCurrentAfterLast)
		{
			SetCurrent(null, InternalCount);
		}
		else
		{
			int num2 = InternalIndexOf(currentItem);
			if (num2 < 0)
			{
				num2 = ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? 1 : 0);
				object newItem;
				if (num2 < InternalCount && (newItem = InternalItemAt(num2)) != CollectionView.NewItemPlaceholder)
				{
					SetCurrent(newItem, num2);
				}
				else
				{
					SetCurrent(null, -1);
				}
			}
			else
			{
				SetCurrent(currentItem, num2);
			}
		}
		_ignoreInnerRefresh = false;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		OnCurrentChanged();
		if (IsCurrentAfterLast != isCurrentAfterLast)
		{
			OnPropertyChanged("IsCurrentAfterLast");
		}
		if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
		{
			OnPropertyChanged("IsCurrentBeforeFirst");
		}
		if (num != CurrentPosition)
		{
			OnPropertyChanged("CurrentPosition");
		}
		if (currentItem != CurrentItem)
		{
			OnPropertyChanged("CurrentItem");
		}
	}

	protected override void OnAllowsCrossThreadChangesChanged()
	{
		PrepareCachedList();
	}

	private void PrepareCachedList()
	{
		if (base.AllowsCrossThreadChanges)
		{
			BindingOperations.AccessCollection(InternalList, delegate
			{
				RebuildLists();
			}, writeAccess: false);
		}
		else
		{
			RebuildListsCore();
		}
	}

	private void RebuildLists()
	{
		lock (base.SyncRoot)
		{
			ClearPendingChanges();
			RebuildListsCore();
		}
	}

	private void RebuildListsCore()
	{
		_cachedList = new ArrayList(InternalList);
		if (_shadowList is LiveShapingList liveShapingList)
		{
			liveShapingList.LiveShapingDirty -= OnLiveShapingDirty;
		}
		if (_isGrouping && IsLiveGrouping == true)
		{
			LiveShapingList liveShapingList2 = (LiveShapingList)(_shadowList = new LiveShapingList(this, GetLiveShapingFlags(), ActiveComparer));
			foreach (object @internal in InternalList)
			{
				liveShapingList2.Add(@internal);
			}
			liveShapingList2.LiveShapingDirty += OnLiveShapingDirty;
		}
		else if (base.AllowsCrossThreadChanges)
		{
			_shadowList = new ArrayList(InternalList);
		}
		else
		{
			_shadowList = null;
		}
	}

	[Obsolete("Replaced by OnAllowsCrossThreadChangesChanged")]
	protected override void OnBeginChangeLogging(NotifyCollectionChangedEventArgs args)
	{
	}

	protected override void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		bool flag = false;
		ValidateCollectionChangedEventArgs(args);
		int currentPosition = CurrentPosition;
		int currentPosition2 = CurrentPosition;
		object currentItem = CurrentItem;
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		bool flag2 = false;
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (_newItemIndex == -2)
			{
				BeginAddNew(args.NewItems[0], args.NewStartingIndex);
				return;
			}
			if (_isGrouping)
			{
				AddItemToGroups(args.NewItems[0]);
				break;
			}
			AdjustCurrencyForAdd(args.NewStartingIndex);
			flag = true;
			break;
		case NotifyCollectionChangedAction.Remove:
			if (_isGrouping)
			{
				RemoveItemFromGroups(args.OldItems[0]);
				break;
			}
			flag2 = AdjustCurrencyForRemove(args.OldStartingIndex);
			flag = true;
			break;
		case NotifyCollectionChangedAction.Replace:
			if (_isGrouping)
			{
				RemoveItemFromGroups(args.OldItems[0]);
				AddItemToGroups(args.NewItems[0]);
			}
			else
			{
				flag2 = AdjustCurrencyForReplace(args.NewStartingIndex);
				flag = true;
			}
			break;
		case NotifyCollectionChangedAction.Move:
			if (!_isGrouping)
			{
				AdjustCurrencyForMove(args.OldStartingIndex, args.NewStartingIndex);
				flag = true;
			}
			else
			{
				_group.MoveWithinSubgroups(args.OldItems[0], null, InternalList, args.OldStartingIndex, args.NewStartingIndex);
			}
			break;
		case NotifyCollectionChangedAction.Reset:
			if (_isGrouping)
			{
				RefreshOrDefer();
			}
			else
			{
				flag = true;
			}
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
		}
		if (base.AllowsCrossThreadChanges)
		{
			AdjustShadowCopy(args);
		}
		bool flag3 = IsCurrentAfterLast != isCurrentAfterLast;
		bool flag4 = IsCurrentBeforeFirst != isCurrentBeforeFirst;
		bool flag5 = CurrentPosition != currentPosition2;
		bool flag6 = CurrentItem != currentItem;
		isCurrentAfterLast = IsCurrentAfterLast;
		isCurrentBeforeFirst = IsCurrentBeforeFirst;
		currentPosition2 = CurrentPosition;
		currentItem = CurrentItem;
		if (flag)
		{
			OnCollectionChanged(args);
			if (IsCurrentAfterLast != isCurrentAfterLast)
			{
				flag3 = false;
				isCurrentAfterLast = IsCurrentAfterLast;
			}
			if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
			{
				flag4 = false;
				isCurrentBeforeFirst = IsCurrentBeforeFirst;
			}
			if (CurrentPosition != currentPosition2)
			{
				flag5 = false;
				currentPosition2 = CurrentPosition;
			}
			if (CurrentItem != currentItem)
			{
				flag6 = false;
				currentItem = CurrentItem;
			}
		}
		if (flag2)
		{
			MoveCurrencyOffDeletedElement(currentPosition);
			flag3 = flag3 || IsCurrentAfterLast != isCurrentAfterLast;
			flag4 = flag4 || IsCurrentBeforeFirst != isCurrentBeforeFirst;
			flag5 = flag5 || CurrentPosition != currentPosition2;
			flag6 = flag6 || CurrentItem != currentItem;
		}
		if (flag3)
		{
			OnPropertyChanged("IsCurrentAfterLast");
		}
		if (flag4)
		{
			OnPropertyChanged("IsCurrentBeforeFirst");
		}
		if (flag5)
		{
			OnPropertyChanged("CurrentPosition");
		}
		if (flag6)
		{
			OnPropertyChanged("CurrentItem");
		}
	}

	private int InternalIndexOf(object item)
	{
		if (_isGrouping)
		{
			return _group.LeafIndexOf(item);
		}
		if (item == CollectionView.NewItemPlaceholder)
		{
			switch (NewItemPlaceholderPosition)
			{
			case NewItemPlaceholderPosition.None:
				return -1;
			case NewItemPlaceholderPosition.AtBeginning:
				return 0;
			case NewItemPlaceholderPosition.AtEnd:
				return InternalCount - 1;
			}
		}
		else if (IsAddingNew && ItemsControl.EqualsEx(item, _newItem))
		{
			switch (NewItemPlaceholderPosition)
			{
			case NewItemPlaceholderPosition.AtBeginning:
				return 1;
			case NewItemPlaceholderPosition.AtEnd:
				return InternalCount - 2;
			}
		}
		int num = CollectionProxy.IndexOf(item);
		if (num >= CollectionProxy.Count)
		{
			num = -1;
		}
		if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning && num >= 0)
		{
			num += ((!IsAddingNew) ? 1 : 2);
		}
		return num;
	}

	private object InternalItemAt(int index)
	{
		if (_isGrouping)
		{
			return _group.LeafAt(index);
		}
		switch (NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.AtBeginning:
			if (index == 0)
			{
				return CollectionView.NewItemPlaceholder;
			}
			index--;
			if (IsAddingNew)
			{
				if (index == 0)
				{
					return _newItem;
				}
				if (index <= _newItemIndex + 1)
				{
					index--;
				}
			}
			break;
		case NewItemPlaceholderPosition.AtEnd:
			if (index == InternalCount - 1)
			{
				return CollectionView.NewItemPlaceholder;
			}
			if (IsAddingNew && index == InternalCount - 2)
			{
				return _newItem;
			}
			break;
		}
		return CollectionProxy[index];
	}

	private bool InternalContains(object item)
	{
		if (item == CollectionView.NewItemPlaceholder)
		{
			return NewItemPlaceholderPosition != NewItemPlaceholderPosition.None;
		}
		if (_isGrouping)
		{
			return _group.LeafIndexOf(item) >= 0;
		}
		return CollectionProxy.Contains(item);
	}

	private IEnumerator InternalGetEnumerator()
	{
		if (!_isGrouping)
		{
			return new PlaceholderAwareEnumerator(this, CollectionProxy.GetEnumerator(), NewItemPlaceholderPosition, _newItem);
		}
		return _group.GetLeafEnumerator();
	}

	private void AdjustShadowCopy(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			_shadowList.Insert(e.NewStartingIndex, e.NewItems[0]);
			break;
		case NotifyCollectionChangedAction.Remove:
			_shadowList.RemoveAt(e.OldStartingIndex);
			break;
		case NotifyCollectionChangedAction.Replace:
			_shadowList[e.OldStartingIndex] = e.NewItems[0];
			break;
		case NotifyCollectionChangedAction.Move:
			_shadowList.Move(e.OldStartingIndex, e.NewStartingIndex);
			break;
		}
	}

	private void _MoveTo(int proposed)
	{
		if (proposed == CurrentPosition || IsEmpty)
		{
			return;
		}
		object obj = ((0 <= proposed && proposed < InternalCount) ? GetItemAt(proposed) : null);
		if (obj != CollectionView.NewItemPlaceholder && OKToChangeCurrent())
		{
			bool isCurrentAfterLast = IsCurrentAfterLast;
			bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
			SetCurrent(obj, proposed);
			OnCurrentChanged();
			if (IsCurrentAfterLast != isCurrentAfterLast)
			{
				OnPropertyChanged("IsCurrentAfterLast");
			}
			if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
			{
				OnPropertyChanged("IsCurrentBeforeFirst");
			}
			OnPropertyChanged("CurrentPosition");
			OnPropertyChanged("CurrentItem");
		}
	}

	private void SubscribeToChanges()
	{
		if (InternalList.SupportsChangeNotification)
		{
			BindingOperations.AccessCollection(InternalList, delegate
			{
				InternalList.ListChanged += OnListChanged;
				RebuildLists();
			}, writeAccess: false);
		}
	}

	private void OnListChanged(object sender, ListChangedEventArgs args)
	{
		if (_ignoreInnerRefresh && args.ListChangedType == ListChangedType.Reset)
		{
			return;
		}
		NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null;
		object obj = null;
		int num = ((!_isGrouping && NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? 1 : 0);
		int num2 = args.NewIndex;
		switch (args.ListChangedType)
		{
		case ListChangedType.ItemAdded:
			if (InternalList.Count == _cachedList.Count)
			{
				if (IsAddingNew && num2 == _newItemIndex)
				{
					notifyCollectionChangedEventArgs = ProcessCommitNew(num2 + num, num2 + num);
				}
				break;
			}
			obj = InternalList[num2];
			notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, obj, num2 + num);
			_cachedList.Insert(num2, obj);
			if (InternalList.Count != _cachedList.Count)
			{
				throw new InvalidOperationException(SR.Format(SR.InconsistentBindingList, InternalList, args.ListChangedType));
			}
			if (num2 <= _newItemIndex)
			{
				_newItemIndex++;
			}
			break;
		case ListChangedType.ItemDeleted:
			obj = _cachedList[num2];
			_cachedList.RemoveAt(num2);
			if (InternalList.Count != _cachedList.Count)
			{
				throw new InvalidOperationException(SR.Format(SR.InconsistentBindingList, InternalList, args.ListChangedType));
			}
			if (num2 < _newItemIndex)
			{
				_newItemIndex--;
			}
			if (obj == CurrentEditItem)
			{
				ImplicitlyCancelEdit();
			}
			if (obj == CurrentAddItem)
			{
				EndAddNew(cancel: true);
				switch (NewItemPlaceholderPosition)
				{
				case NewItemPlaceholderPosition.AtBeginning:
					num2 = 0;
					break;
				case NewItemPlaceholderPosition.AtEnd:
					num2 = InternalCount - 1;
					break;
				}
			}
			notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, num2 + num);
			break;
		case ListChangedType.ItemMoved:
			if (IsAddingNew && args.OldIndex == _newItemIndex)
			{
				obj = _newItem;
				notifyCollectionChangedEventArgs = ProcessCommitNew(args.OldIndex, num2 + num);
			}
			else
			{
				obj = InternalList[num2];
				notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, obj, num2 + num, args.OldIndex + num);
				if (args.OldIndex < _newItemIndex && _newItemIndex < args.NewIndex)
				{
					_newItemIndex--;
				}
				else if (args.NewIndex <= _newItemIndex && _newItemIndex < args.OldIndex)
				{
					_newItemIndex++;
				}
			}
			_cachedList.RemoveAt(args.OldIndex);
			_cachedList.Insert(args.NewIndex, obj);
			if (InternalList.Count != _cachedList.Count)
			{
				throw new InvalidOperationException(SR.Format(SR.InconsistentBindingList, InternalList, args.ListChangedType));
			}
			break;
		case ListChangedType.ItemChanged:
			if (!_itemsRaisePropertyChanged.HasValue)
			{
				obj = InternalList[args.NewIndex];
				_itemsRaisePropertyChanged = obj is INotifyPropertyChanged;
			}
			if (_itemsRaisePropertyChanged.Value)
			{
				break;
			}
			goto case ListChangedType.Reset;
		case ListChangedType.Reset:
		case ListChangedType.PropertyDescriptorAdded:
		case ListChangedType.PropertyDescriptorDeleted:
		case ListChangedType.PropertyDescriptorChanged:
			if (IsEditingItem)
			{
				ImplicitlyCancelEdit();
			}
			if (IsAddingNew)
			{
				_newItemIndex = InternalList.IndexOf(_newItem);
				if (_newItemIndex < 0)
				{
					EndAddNew(cancel: true);
				}
			}
			RefreshOrDefer();
			break;
		}
		if (notifyCollectionChangedEventArgs != null)
		{
			OnCollectionChanged(sender, notifyCollectionChangedEventArgs);
		}
	}

	private void AdjustCurrencyForAdd(int index)
	{
		if (InternalCount == 1)
		{
			SetCurrent(null, -1);
		}
		else if (index <= CurrentPosition)
		{
			int num = CurrentPosition + 1;
			if (num < InternalCount)
			{
				SetCurrent(GetItemAt(num), num);
			}
			else
			{
				SetCurrent(null, InternalCount);
			}
		}
	}

	private bool AdjustCurrencyForRemove(int index)
	{
		bool result = index == CurrentPosition;
		if (index < CurrentPosition)
		{
			SetCurrent(CurrentItem, CurrentPosition - 1);
		}
		return result;
	}

	private void AdjustCurrencyForMove(int oldIndex, int newIndex)
	{
		if (oldIndex == CurrentPosition)
		{
			SetCurrent(GetItemAt(newIndex), newIndex);
		}
		else if (oldIndex < CurrentPosition && CurrentPosition <= newIndex)
		{
			SetCurrent(CurrentItem, CurrentPosition - 1);
		}
		else if (newIndex <= CurrentPosition && CurrentPosition < oldIndex)
		{
			SetCurrent(CurrentItem, CurrentPosition + 1);
		}
	}

	private bool AdjustCurrencyForReplace(int index)
	{
		bool num = index == CurrentPosition;
		if (num)
		{
			SetCurrent(GetItemAt(index), index);
		}
		return num;
	}

	private void MoveCurrencyOffDeletedElement(int oldCurrentPosition)
	{
		int num = InternalCount - 1;
		int num2 = ((oldCurrentPosition < num) ? oldCurrentPosition : num);
		OnCurrentChanging();
		if (num2 < 0)
		{
			SetCurrent(null, num2);
		}
		else
		{
			SetCurrent(InternalItemAt(num2), num2);
		}
		OnCurrentChanged();
	}

	private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsAddingNew || IsEditingItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Sorting"));
		}
		RefreshOrDefer();
	}

	private ListSortDescriptionCollection ConvertSortDescriptionCollection(SortDescriptionCollection sorts)
	{
		ITypedList typedList;
		Type itemType;
		PropertyDescriptorCollection propertyDescriptorCollection = (((typedList = InternalList as ITypedList) != null) ? typedList.GetItemProperties(null) : ((!((itemType = GetItemType(useRepresentativeItem: true)) != null)) ? null : TypeDescriptor.GetProperties(itemType)));
		if (propertyDescriptorCollection == null || propertyDescriptorCollection.Count == 0)
		{
			throw new ArgumentException(SR.CannotDetermineSortByPropertiesForCollection);
		}
		ListSortDescription[] array = new ListSortDescription[sorts.Count];
		for (int i = 0; i < sorts.Count; i++)
		{
			ListSortDescription listSortDescription = new ListSortDescription(propertyDescriptorCollection.Find(sorts[i].PropertyName, ignoreCase: true) ?? throw new ArgumentException(SR.Format(p1: typedList.GetListName(null), resourceFormat: SR.PropertyToSortByNotFoundOnType, p2: sorts[i].PropertyName)), sorts[i].Direction);
			array[i] = listSortDescription;
		}
		return new ListSortDescriptionCollection(array);
	}

	private void InitializeGrouping()
	{
		_group.Clear();
		_group.Initialize();
		_isGrouping = _group.GroupBy != null;
	}

	private void PrepareGroups()
	{
		if (!_isGrouping)
		{
			return;
		}
		IList collectionProxy = CollectionProxy;
		IComparer activeComparer = ActiveComparer;
		if (activeComparer != null)
		{
			_group.ActiveComparer = activeComparer;
		}
		else if (_group.ActiveComparer is CollectionViewGroupInternal.IListComparer listComparer)
		{
			listComparer.ResetList(collectionProxy);
		}
		else
		{
			_group.ActiveComparer = new CollectionViewGroupInternal.IListComparer(collectionProxy);
		}
		if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
		{
			_group.InsertSpecialItem(0, CollectionView.NewItemPlaceholder, loading: true);
			if (IsAddingNew)
			{
				_group.InsertSpecialItem(1, _newItem, loading: true);
			}
		}
		bool valueOrDefault = IsLiveGrouping == true;
		LiveShapingList liveShapingList = collectionProxy as LiveShapingList;
		int i = 0;
		for (int count = collectionProxy.Count; i < count; i++)
		{
			object obj = collectionProxy[i];
			LiveShapingItem lsi = (valueOrDefault ? liveShapingList.ItemAt(i) : null);
			if (!IsAddingNew || !ItemsControl.EqualsEx(_newItem, obj))
			{
				_group.AddToSubgroups(obj, lsi, loading: true);
			}
		}
		if (IsAddingNew && NewItemPlaceholderPosition != NewItemPlaceholderPosition.AtBeginning)
		{
			_group.InsertSpecialItem(_group.Items.Count, _newItem, loading: true);
		}
		if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)
		{
			_group.InsertSpecialItem(_group.Items.Count, CollectionView.NewItemPlaceholder, loading: true);
		}
	}

	private void OnGroupChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add)
		{
			AdjustCurrencyForAdd(e.NewStartingIndex);
		}
		else if (e.Action == NotifyCollectionChangedAction.Remove)
		{
			AdjustCurrencyForRemove(e.OldStartingIndex);
		}
		OnCollectionChanged(e);
	}

	private void OnGroupByChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsAddingNew || IsEditingItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Grouping"));
		}
		RefreshOrDefer();
	}

	private void OnGroupDescriptionChanged(object sender, EventArgs e)
	{
		if (IsAddingNew || IsEditingItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Grouping"));
		}
		RefreshOrDefer();
	}

	private void AddItemToGroups(object item)
	{
		if (IsAddingNew && item == _newItem)
		{
			int index = NewItemPlaceholderPosition switch
			{
				NewItemPlaceholderPosition.AtBeginning => 1, 
				NewItemPlaceholderPosition.AtEnd => _group.Items.Count - 1, 
				_ => _group.Items.Count, 
			};
			_group.InsertSpecialItem(index, item, loading: false);
		}
		else
		{
			_group.AddToSubgroups(item, null, loading: false);
		}
	}

	private void RemoveItemFromGroups(object item)
	{
		if (CanGroupNamesChange || _group.RemoveFromSubgroups(item))
		{
			_group.RemoveItemFromSubgroupsByExhaustiveSearch(item);
		}
	}

	private LiveShapingFlags GetLiveShapingFlags()
	{
		LiveShapingFlags liveShapingFlags = (LiveShapingFlags)0;
		if (IsLiveGrouping == true)
		{
			liveShapingFlags |= LiveShapingFlags.Grouping;
		}
		return liveShapingFlags;
	}

	internal void RestoreLiveShaping()
	{
		if (!(CollectionProxy is LiveShapingList liveShapingList))
		{
			return;
		}
		if (_isGrouping)
		{
			List<AbandonedGroupItem> deleteList = new List<AbandonedGroupItem>();
			foreach (LiveShapingItem groupDirtyItem in liveShapingList.GroupDirtyItems)
			{
				if (!groupDirtyItem.IsDeleted)
				{
					_group.RestoreGrouping(groupDirtyItem, deleteList);
					groupDirtyItem.IsGroupDirty = false;
				}
			}
			_group.DeleteAbandonedGroupItems(deleteList);
		}
		liveShapingList.GroupDirtyItems.Clear();
		IsLiveShapingDirty = false;
	}

	private void OnLiveShapingDirty(object sender, EventArgs e)
	{
		IsLiveShapingDirty = true;
	}

	private void ValidateCollectionChangedEventArgs(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (e.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (e.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			break;
		case NotifyCollectionChangedAction.Replace:
			if (e.NewItems.Count != 1 || e.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			break;
		case NotifyCollectionChangedAction.Move:
			if (e.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			if (e.NewStartingIndex < 0)
			{
				throw new InvalidOperationException(SR.CannotMoveToUnknownPosition);
			}
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		case NotifyCollectionChangedAction.Reset:
			break;
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	private void DeferAction(Action action)
	{
		if (_deferredActions == null)
		{
			_deferredActions = new List<Action>();
		}
		_deferredActions.Add(action);
	}

	private void DoDeferredActions()
	{
		if (_deferredActions == null)
		{
			return;
		}
		List<Action> deferredActions = _deferredActions;
		_deferredActions = null;
		foreach (Action item in deferredActions)
		{
			item();
		}
	}
}
