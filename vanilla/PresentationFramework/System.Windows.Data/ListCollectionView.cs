using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Represents the collection view for collections that implement <see cref="T:System.Collections.IList" />. </summary>
public class ListCollectionView : CollectionView, IComparer, IEditableCollectionViewAddNewItem, IEditableCollectionView, ICollectionViewLiveShaping, IItemProperties
{
	private const double LiveSortingDensityThreshold = 0.8;

	private IList _internalList;

	private CollectionViewGroupRoot _group;

	private bool _isGrouping;

	private IComparer _activeComparer;

	private Predicate<object> _activeFilter;

	private SortDescriptionCollection _sort;

	private IComparer _customSort;

	private ArrayList _shadowCollection;

	private bool _currentElementWasRemoved;

	private object _newItem = CollectionView.NoNewItem;

	private object _editItem;

	private int _newItemIndex;

	private NewItemPlaceholderPosition _newItemPlaceholderPosition;

	private bool _isItemConstructorValid;

	private ConstructorInfo _itemConstructor;

	private List<Action> _deferredActions;

	private ObservableCollection<string> _liveSortingProperties;

	private ObservableCollection<string> _liveFilteringProperties;

	private ObservableCollection<string> _liveGroupingProperties;

	private bool? _isLiveSorting = false;

	private bool? _isLiveFiltering = false;

	private bool? _isLiveGrouping = false;

	private bool _isLiveShapingDirty;

	private bool _isRemoving;

	private const int _unknownIndex = -1;

	/// <summary>Gets a value that indicates whether the collection view supports grouping.</summary>
	/// <returns>true if the collection view supports grouping; otherwise, false.</returns>
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
			if (!IsGrouping)
			{
				return null;
			}
			return _group.Items;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describes how the items in the collection are sorted in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describe how the items in the collection are sorted in the view.</returns>
	public override SortDescriptionCollection SortDescriptions
	{
		get
		{
			if (_sort == null)
			{
				SetSortDescriptions(new SortDescriptionCollection());
			}
			return _sort;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view supports sorting.</summary>
	/// <returns>For a default instance of <see cref="T:System.Windows.Data.ListCollectionView" />, this property always returns true.</returns>
	public override bool CanSort => true;

	/// <summary>Gets a value that indicates whether the view supports callback-based filtering.</summary>
	/// <returns>For a default instance of <see cref="T:System.Windows.Data.ListCollectionView" />, this property always returns true.</returns>
	public override bool CanFilter => true;

	/// <summary>Gets or sets a method that is used to determine whether an item is suitable for inclusion in the view.</summary>
	/// <returns>A delegate that represents the method that is used to determine whether an item is suitable for inclusion in the view.</returns>
	public override Predicate<object> Filter
	{
		get
		{
			return base.Filter;
		}
		set
		{
			if (base.AllowsCrossThreadChanges)
			{
				VerifyAccess();
			}
			if (IsAddingNew || IsEditingItem)
			{
				throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Filter"));
			}
			base.Filter = value;
		}
	}

	/// <summary>Gets or sets a custom object that implements <see cref="T:System.Collections.IComparer" /> to sort items in the view.</summary>
	/// <returns>The sort criteria as an implementation of <see cref="T:System.Collections.IComparer" />.</returns>
	public IComparer CustomSort
	{
		get
		{
			return _customSort;
		}
		set
		{
			if (base.AllowsCrossThreadChanges)
			{
				VerifyAccess();
			}
			if (IsAddingNew || IsEditingItem)
			{
				throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "CustomSort"));
			}
			_customSort = value;
			SetSortDescriptions(null);
			RefreshOrDefer();
		}
	}

	/// <summary>Gets or sets a delegate to select the <see cref="T:System.ComponentModel.GroupDescription" /> as a function of the parent group and its level. </summary>
	/// <returns>A method that provides the logic for the selection of the <see cref="T:System.ComponentModel.GroupDescription" /> as a function of the parent group and its level. The default value is null.</returns>
	[DefaultValue(null)]
	public virtual GroupDescriptionSelectorCallback GroupBySelector
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
				throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Grouping"));
			}
			_group.GroupBySelector = value;
			RefreshOrDefer();
		}
	}

	/// <summary>Gets the estimated number of records.</summary>
	/// <returns>One of the following:ValueMeaning-1Could not determine the count of the collection. This might be returned by a "virtualizing" view, where the view deliberately does not account for all items in the underlying collection because the view is trying to increase efficiency and minimize dependence on always having the whole collection available.any other integerThe count of the collection.</returns>
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
	public override bool IsEmpty => InternalCount == 0;

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

	/// <summary>Gets or sets the position of the new item placeholder in the <see cref="T:System.Windows.Data.ListCollectionView" />.</summary>
	/// <returns>One of the enumeration values that specifies the position of the new item placeholder in the <see cref="T:System.Windows.Data.ListCollectionView" />.</returns>
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
			if (!IsGrouping)
			{
				ProcessCollectionChangedWithAdjustedIndex(notifyCollectionChangedEventArgs, num, num2);
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
			if (!IsEditingItem && !SourceList.IsFixedSize)
			{
				return CanConstructItem;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether a specified object can be added to the collection.</summary>
	/// <returns>true if a specified object can be added to the collection; otherwise, false.</returns>
	public bool CanAddNewItem
	{
		get
		{
			if (!IsEditingItem)
			{
				return !SourceList.IsFixedSize;
			}
			return false;
		}
	}

	private bool CanConstructItem
	{
		get
		{
			if (!_isItemConstructorValid)
			{
				EnsureItemConstructor();
			}
			return _itemConstructor != null;
		}
	}

	/// <summary>Gets a value that indicates whether an add transaction is in progress.</summary>
	/// <returns>true if an add transaction is in progress; otherwise, false.</returns>
	public bool IsAddingNew => _newItem != CollectionView.NoNewItem;

	/// <summary>Gets the item that is being added during the current add transaction.</summary>
	/// <returns>The item that is being added if <see cref="P:System.Windows.Data.ListCollectionView.IsAddingNew" /> is true; otherwise, null.</returns>
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
				return !SourceList.IsFixedSize;
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

	/// <summary>Gets a value that indicates whether the collection view supports turning sorting data in real time on or off.</summary>
	/// <returns>true in all cases.</returns>
	public bool CanChangeLiveSorting => true;

	/// <summary>Gets a value that indicates whether the collection view supports turning filtering data in real time on or off.</summary>
	/// <returns>true in all cases.</returns>
	public bool CanChangeLiveFiltering => true;

	/// <summary>Gets a value that indicates whether the collection view supports turning grouping data in real time on or off.</summary>
	/// <returns>true in all cases.</returns>
	public bool CanChangeLiveGrouping => true;

	/// <summary>Gets or sets a value that indicates whether sorting in real time is enabled.</summary>
	/// <returns>true if sorting data in real time is enabled; false if live sorting is not enabled; null if it cannot be determined whether the collection view implements live sorting.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Windows.Data.ListCollectionView.IsLiveFiltering" /> cannot be set to null.</exception>
	public bool? IsLiveSorting
	{
		get
		{
			return _isLiveSorting;
		}
		set
		{
			if (!value.HasValue)
			{
				throw new ArgumentNullException("value");
			}
			if (value != _isLiveSorting)
			{
				_isLiveSorting = value;
				RebuildLocalArray();
				OnPropertyChanged("IsLiveSorting");
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether filtering data in real time is enabled.</summary>
	/// <returns>true if filtering data in real time is enabled; false if live filtering is not enabled; null if it cannot be determined whether the collection view implements live filtering.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Windows.Data.ListCollectionView.IsLiveFiltering" /> cannot be set to null.</exception>
	public bool? IsLiveFiltering
	{
		get
		{
			return _isLiveFiltering;
		}
		set
		{
			if (!value.HasValue)
			{
				throw new ArgumentNullException("value");
			}
			if (value != _isLiveFiltering)
			{
				_isLiveFiltering = value;
				RebuildLocalArray();
				OnPropertyChanged("IsLiveFiltering");
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether grouping data in real time is enabled.</summary>
	/// <returns>true if grouping data in real time is enabled; false if live grouping is not enabled; null if it cannot be determined whether the collection view implements live grouping.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Windows.Data.ListCollectionView.IsLiveGrouping" /> cannot be set to null.</exception>
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
				RebuildLocalArray();
				OnPropertyChanged("IsLiveGrouping");
			}
		}
	}

	private bool IsLiveShaping
	{
		get
		{
			if (IsLiveSorting != true && IsLiveFiltering != true)
			{
				return IsLiveGrouping == true;
			}
			return true;
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
				_liveSortingProperties.CollectionChanged += OnLivePropertyListChanged;
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
				_liveFilteringProperties.CollectionChanged += OnLivePropertyListChanged;
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

	/// <summary>Gets a value that indicates whether a private copy of the data is needed for sorting and filtering.</summary>
	/// <returns>true if a private copy of the data is needed for sorting and filtering; otherwise, false. The default implementation returns true if there is an <see cref="P:System.Windows.Data.ListCollectionView.ActiveFilter" /> or <see cref="P:System.Windows.Data.ListCollectionView.ActiveComparer" />, or both.</returns>
	protected bool UsesLocalArray
	{
		get
		{
			if (ActiveComparer == null && ActiveFilter == null)
			{
				if (IsGrouping)
				{
					return IsLiveGrouping == true;
				}
				return false;
			}
			return true;
		}
	}

	/// <summary>Gets the complete and unfiltered underlying collection.</summary>
	/// <returns>The underlying collection, which must implement <see cref="T:System.Collections.IList" />.</returns>
	protected IList InternalList => _internalList;

	/// <summary>Gets or sets the current active comparer that is used in sorting.</summary>
	/// <returns>An <see cref="T:System.Collections.IComparer" /> object that is the active comparer.</returns>
	protected IComparer ActiveComparer
	{
		get
		{
			return _activeComparer;
		}
		set
		{
			_activeComparer = value;
		}
	}

	/// <summary>Gets or sets the current active <see cref="P:System.Windows.Data.CollectionView.Filter" /> callback.</summary>
	/// <returns>The active <see cref="P:System.Windows.Data.CollectionView.Filter" /> callback.</returns>
	protected Predicate<object> ActiveFilter
	{
		get
		{
			return _activeFilter;
		}
		set
		{
			_activeFilter = value;
		}
	}

	/// <summary>Gets a value that indicates whether there are groups in the view.</summary>
	/// <returns>true if there are groups in the view; otherwise, false.</returns>
	protected bool IsGrouping => _isGrouping;

	/// <summary>Gets the number of records in the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />.</summary>
	/// <returns>The number of records in the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />.</returns>
	protected int InternalCount
	{
		get
		{
			if (IsGrouping)
			{
				return _group.ItemCount;
			}
			int num = ((NewItemPlaceholderPosition != NewItemPlaceholderPosition.None) ? 1 : 0);
			if (UsesLocalArray && IsAddingNew)
			{
				num++;
			}
			return num + InternalList.Count;
		}
	}

	internal ArrayList ShadowCollection
	{
		get
		{
			return _shadowCollection;
		}
		set
		{
			_shadowCollection = value;
		}
	}

	internal bool HasSortDescriptions
	{
		get
		{
			if (_sort != null)
			{
				return _sort.Count > 0;
			}
			return false;
		}
	}

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

	private bool CanGroupNamesChange => true;

	private IList SourceList => SourceCollection as IList;

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ListCollectionView" /> class, using a supplied collection that implements <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="list">The underlying collection, which must implement <see cref="T:System.Collections.IList" />.</param>
	public ListCollectionView(IList list)
		: base(list)
	{
		if (base.AllowsCrossThreadChanges)
		{
			BindingOperations.AccessCollection(list, delegate
			{
				ClearPendingChanges();
				ShadowCollection = new ArrayList((ICollection)SourceCollection);
				_internalList = ShadowCollection;
			}, writeAccess: false);
		}
		else
		{
			_internalList = list;
		}
		if (InternalList.Count == 0)
		{
			SetCurrent(null, -1, 0);
		}
		else
		{
			SetCurrent(InternalList[0], 0, 1);
		}
		_group = new CollectionViewGroupRoot(this);
		_group.GroupDescriptionChanged += OnGroupDescriptionChanged;
		((INotifyCollectionChanged)_group).CollectionChanged += OnGroupChanged;
		((INotifyCollectionChanged)_group.GroupDescriptions).CollectionChanged += OnGroupByChanged;
	}

	/// <summary>Recreates the view.</summary>
	protected override void RefreshOverride()
	{
		if (base.AllowsCrossThreadChanges)
		{
			BindingOperations.AccessCollection(SourceCollection, delegate
			{
				lock (base.SyncRoot)
				{
					ClearPendingChanges();
					ShadowCollection = new ArrayList((ICollection)SourceCollection);
				}
			}, writeAccess: false);
		}
		object currentItem = CurrentItem;
		int num = (IsEmpty ? (-1) : CurrentPosition);
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		OnCurrentChanging();
		PrepareLocalArray();
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

	/// <summary>Returns a value that indicates whether a given item belongs to the collection view.</summary>
	/// <returns>true if the item belongs to the collection view; otherwise, false.</returns>
	/// <param name="item">The object to check.</param>
	public override bool Contains(object item)
	{
		VerifyRefreshNotDeferred();
		return InternalContains(item);
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
		if (position != CurrentPosition || !base.IsCurrentInSync)
		{
			object obj = ((0 <= position && position < InternalCount) ? InternalItemAt(position) : null);
			if (obj != CollectionView.NewItemPlaceholder && OKToChangeCurrent())
			{
				bool isCurrentAfterLast = IsCurrentAfterLast;
				bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
				SetCurrent(obj, position);
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
		return IsCurrentInView;
	}

	/// <summary>Returns a value that indicates whether the specified item in the underlying collection belongs to the view.</summary>
	/// <returns>true if the specified item belongs to the view or if there is not filter set on the collection view; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	public override bool PassesFilter(object item)
	{
		if (ActiveFilter != null)
		{
			return ActiveFilter(item);
		}
		return true;
	}

	/// <summary>Returns the index where the given data item belongs in the collection, or -1 if the index of that item is unknown. </summary>
	/// <returns>The index of the item in the collection, or -1 if the item does not exist in the collection.</returns>
	/// <param name="item">The object to check for in the collection.</param>
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

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A value that is less than zero means <paramref name="o1" /> is less than <paramref name="o2;" /> a value of zero means the objects are equal; and a value that is over zero means <paramref name="o1" /> is greater than <paramref name="o2" />.</returns>
	/// <param name="o1">The first object to compare.</param>
	/// <param name="o2">The second object to compare.</param>
	int IComparer.Compare(object o1, object o2)
	{
		return Compare(o1, o2);
	}

	/// <summary>Compares two objects and returns a value that indicates whether one is less than, equal to, or greater than the other.</summary>
	/// <returns>Less than zero if <paramref name="o1" /> is less than <paramref name="o2" />, zero if <paramref name="o1" /> and <paramref name="o2" /> are equal, or greater than zero if <paramref name="o1" /> is greater than <paramref name="o2" />.</returns>
	/// <param name="o1">The first object to compare.</param>
	/// <param name="o2">The second object to compare.</param>
	protected virtual int Compare(object o1, object o2)
	{
		if (!IsGrouping)
		{
			if (ActiveComparer != null)
			{
				return ActiveComparer.Compare(o1, o2);
			}
			int num = InternalList.IndexOf(o1);
			int num2 = InternalList.IndexOf(o2);
			return num - num2;
		}
		int num3 = InternalIndexOf(o1);
		int num4 = InternalIndexOf(o2);
		return num3 - num4;
	}

	/// <summary>Returns an object that you can use to enumerate the items in the view.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that you can use to enumerate the items in the view.</returns>
	protected override IEnumerator GetEnumerator()
	{
		VerifyRefreshNotDeferred();
		return InternalGetEnumerator();
	}

	private void EnsureItemConstructor()
	{
		if (!_isItemConstructorValid)
		{
			Type itemType = GetItemType(useRepresentativeItem: true);
			if (itemType != null)
			{
				_itemConstructor = itemType.GetConstructor(Type.EmptyTypes);
				_isItemConstructorValid = true;
			}
		}
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
		return AddNewCommon(_itemConstructor.Invoke(null));
	}

	/// <summary>Adds the specified object to the collection.</summary>
	/// <returns>The object that was added to the collection.</returns>
	/// <param name="newItem">The object to add to the collection.</param>
	/// <exception cref="T:System.InvalidOperationException">An object cannot be added to the <see cref="T:System.Windows.Data.ListCollectionView" /> by using the <see cref="M:System.Windows.Data.ListCollectionView.AddNewItem(System.Object)" /> method.</exception>
	public object AddNewItem(object newItem)
	{
		VerifyRefreshNotDeferred();
		if (IsEditingItem)
		{
			CommitEdit();
		}
		CommitNew();
		if (!CanAddNewItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "AddNewItem"));
		}
		return AddNewCommon(newItem);
	}

	private object AddNewCommon(object newItem)
	{
		BindingOperations.AccessCollection(SourceList, delegate
		{
			ProcessPendingChanges();
			_newItemIndex = -2;
			int index = SourceList.Add(newItem);
			if (!(SourceList is INotifyCollectionChanged))
			{
				if (!ItemsControl.EqualsEx(newItem, SourceList[index]))
				{
					index = SourceList.IndexOf(newItem);
				}
				BeginAddNew(newItem, index);
			}
		}, writeAccess: true);
		MoveCurrentTo(newItem);
		if (newItem is ISupportInitialize supportInitialize)
		{
			supportInitialize.BeginInit();
		}
		if (newItem is IEditableObject editableObject)
		{
			editableObject.BeginEdit();
		}
		return newItem;
	}

	private void BeginAddNew(object newItem, int index)
	{
		SetNewItem(newItem);
		_newItemIndex = index;
		int num = -1;
		switch (NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.None:
			num = (UsesLocalArray ? (InternalCount - 1) : _newItemIndex);
			break;
		case NewItemPlaceholderPosition.AtBeginning:
			num = 1;
			break;
		case NewItemPlaceholderPosition.AtEnd:
			num = InternalCount - 2;
			break;
		}
		ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, num), -1, num);
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
		if (IsGrouping)
		{
			CommitNewForGrouping();
			return;
		}
		int num = 0;
		switch (NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.None:
			num = (UsesLocalArray ? (InternalCount - 1) : _newItemIndex);
			break;
		case NewItemPlaceholderPosition.AtBeginning:
			num = 1;
			break;
		case NewItemPlaceholderPosition.AtEnd:
			num = InternalCount - 2;
			break;
		}
		object obj = EndAddNew(cancel: false);
		int num2 = AdjustBefore(NotifyCollectionChangedAction.Add, obj, _newItemIndex);
		if (num2 < 0)
		{
			ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, num), num, -1);
		}
		else if (num == num2)
		{
			if (UsesLocalArray)
			{
				if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
				{
					num2--;
				}
				InternalList.Insert(num2, obj);
			}
		}
		else
		{
			ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, obj, num2, num), num, num2);
		}
	}

	private void CommitNewForGrouping()
	{
		int index = NewItemPlaceholderPosition switch
		{
			NewItemPlaceholderPosition.AtBeginning => 1, 
			NewItemPlaceholderPosition.AtEnd => _group.Items.Count - 2, 
			_ => _group.Items.Count - 1, 
		};
		int newItemIndex = _newItemIndex;
		object obj = EndAddNew(cancel: false);
		_group.RemoveSpecialItem(index, obj, loading: false);
		ProcessCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, obj, newItemIndex));
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
		BindingOperations.AccessCollection(SourceList, delegate
		{
			ProcessPendingChanges();
			SourceList.RemoveAt(_newItemIndex);
			if (_newItem != CollectionView.NoNewItem)
			{
				int num = AdjustBefore(NotifyCollectionChangedAction.Remove, _newItem, _newItemIndex);
				object changedItem = EndAddNew(cancel: true);
				ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem, num), num, -1);
			}
		}, writeAccess: true);
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
		BindingOperations.AccessCollection(SourceList, delegate
		{
			ProcessPendingChanges();
			if (index >= InternalCount || !ItemsControl.EqualsEx(item, GetItemAt(index)))
			{
				index = InternalIndexOf(item);
				if (index < 0)
				{
					return;
				}
			}
			int num = ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? 1 : 0);
			int index2 = index - num;
			bool flag = !(SourceList is INotifyCollectionChanged);
			try
			{
				_isRemoving = true;
				if (UsesLocalArray || IsGrouping)
				{
					if (flag)
					{
						index2 = SourceList.IndexOf(item);
						SourceList.RemoveAt(index2);
					}
					else
					{
						SourceList.Remove(item);
					}
				}
				else
				{
					SourceList.RemoveAt(index2);
				}
				if (flag)
				{
					ProcessCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index2));
				}
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
		object editItem = _editItem;
		IEditableObject editableObject = _editItem as IEditableObject;
		SetEditItem(null);
		editableObject?.EndEdit();
		int num = InternalIndexOf(editItem);
		bool flag = num >= 0;
		bool flag2 = (flag ? PassesFilter(editItem) : (SourceList.Contains(editItem) && PassesFilter(editItem)));
		if (IsGrouping)
		{
			if (flag)
			{
				RemoveItemFromGroups(editItem);
			}
			if (flag2)
			{
				LiveShapingItem lsi = ((!(InternalList is LiveShapingList liveShapingList)) ? null : liveShapingList.ItemAt(liveShapingList.IndexOf(editItem)));
				AddItemToGroups(editItem, lsi);
			}
		}
		else
		{
			if (!UsesLocalArray)
			{
				return;
			}
			IList internalList = InternalList;
			int num2 = ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? 1 : 0);
			int num3 = -1;
			if (flag)
			{
				if (!flag2)
				{
					ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, editItem, num), num, -1);
				}
				else
				{
					if (ActiveComparer == null)
					{
						return;
					}
					int num4 = num - num2;
					if (num4 > 0 && ActiveComparer.Compare(internalList[num4 - 1], editItem) > 0)
					{
						num3 = internalList.Search(0, num4, editItem, ActiveComparer);
						if (num3 < 0)
						{
							num3 = ~num3;
						}
					}
					else if (num4 < internalList.Count - 1 && ActiveComparer.Compare(editItem, internalList[num4 + 1]) > 0)
					{
						num3 = internalList.Search(num4 + 1, internalList.Count - num4 - 1, editItem, ActiveComparer);
						if (num3 < 0)
						{
							num3 = ~num3;
						}
						num3--;
					}
					if (num3 >= 0)
					{
						ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, editItem, num3 + num2, num), num, num3 + num2);
					}
				}
			}
			else if (flag2)
			{
				num3 = AdjustBefore(NotifyCollectionChangedAction.Add, editItem, SourceList.IndexOf(editItem));
				ProcessCollectionChangedWithAdjustedIndex(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, editItem, num3 + num2), -1, num3 + num2);
			}
		}
	}

	/// <summary>Ends the edit transaction, and if possible, restores the original value to the item.</summary>
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
			OnPropertyChanged("CanAddNewItem");
			OnPropertyChanged("CanRemove");
		}
	}

	private void OnLivePropertyListChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsLiveShaping)
		{
			RebuildLocalArray();
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Data.CollectionView.AllowsCrossThreadChanges" /> property changes.</summary>
	protected override void OnAllowsCrossThreadChangesChanged()
	{
		if (base.AllowsCrossThreadChanges)
		{
			BindingOperations.AccessCollection(SourceCollection, delegate
			{
				lock (base.SyncRoot)
				{
					ClearPendingChanges();
					ShadowCollection = new ArrayList((ICollection)SourceCollection);
					if (!UsesLocalArray)
					{
						_internalList = ShadowCollection;
					}
				}
			}, writeAccess: false);
		}
		else
		{
			ShadowCollection = null;
			if (!UsesLocalArray)
			{
				_internalList = SourceList;
			}
		}
	}

	/// <summary>Called by the base class to notify the derived class that a <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event has been posted to the message queue.</summary>
	/// <param name="args">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> object that is added to the change log.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="args" /> is null. </exception>
	[Obsolete("Replaced by OnAllowsCrossThreadChangesChanged")]
	protected override void OnBeginChangeLogging(NotifyCollectionChangedEventArgs args)
	{
	}

	/// <summary>Handles <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> events.</summary>
	/// <param name="args">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> object to process.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="args" /> is null. </exception>
	protected override void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		ValidateCollectionChangedEventArgs(args);
		if (!_isItemConstructorValid)
		{
			switch (args.Action)
			{
			case NotifyCollectionChangedAction.Add:
			case NotifyCollectionChangedAction.Replace:
			case NotifyCollectionChangedAction.Reset:
				OnPropertyChanged("CanAddNew");
				break;
			}
		}
		int num = -1;
		int num2 = -1;
		if (base.AllowsCrossThreadChanges && args.Action != NotifyCollectionChangedAction.Reset)
		{
			if ((args.Action != NotifyCollectionChangedAction.Remove && args.NewStartingIndex < 0) || (args.Action != 0 && args.OldStartingIndex < 0))
			{
				return;
			}
			AdjustShadowCopy(args);
		}
		if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			if (IsEditingItem)
			{
				ImplicitlyCancelEdit();
			}
			if (IsAddingNew)
			{
				_newItemIndex = SourceList.IndexOf(_newItem);
				if (_newItemIndex < 0)
				{
					EndAddNew(cancel: true);
				}
			}
			RefreshOrDefer();
			return;
		}
		if (args.Action == NotifyCollectionChangedAction.Add && _newItemIndex == -2)
		{
			BeginAddNew(args.NewItems[0], args.NewStartingIndex);
			return;
		}
		if (args.Action != NotifyCollectionChangedAction.Remove)
		{
			num2 = AdjustBefore(NotifyCollectionChangedAction.Add, args.NewItems[0], args.NewStartingIndex);
		}
		if (args.Action != 0)
		{
			num = AdjustBefore(NotifyCollectionChangedAction.Remove, args.OldItems[0], args.OldStartingIndex);
			if (UsesLocalArray && num >= 0 && num < num2)
			{
				num2--;
			}
		}
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (IsAddingNew && args.NewStartingIndex <= _newItemIndex)
			{
				_newItemIndex++;
			}
			break;
		case NotifyCollectionChangedAction.Remove:
		{
			if (IsAddingNew && args.OldStartingIndex < _newItemIndex)
			{
				_newItemIndex--;
			}
			object obj = args.OldItems[0];
			if (obj == CurrentEditItem)
			{
				ImplicitlyCancelEdit();
			}
			else if (obj == CurrentAddItem)
			{
				EndAddNew(cancel: true);
			}
			break;
		}
		case NotifyCollectionChangedAction.Move:
			if (IsAddingNew)
			{
				if (args.OldStartingIndex == _newItemIndex)
				{
					_newItemIndex = args.NewStartingIndex;
				}
				else if (args.OldStartingIndex < _newItemIndex && _newItemIndex <= args.NewStartingIndex)
				{
					_newItemIndex--;
				}
				else if (args.NewStartingIndex <= _newItemIndex && _newItemIndex < args.OldStartingIndex)
				{
					_newItemIndex++;
				}
			}
			if (ActiveComparer != null && num == num2)
			{
				return;
			}
			break;
		}
		ProcessCollectionChangedWithAdjustedIndex(args, num, num2);
	}

	private void ProcessCollectionChangedWithAdjustedIndex(NotifyCollectionChangedEventArgs args, int adjustedOldIndex, int adjustedNewIndex)
	{
		NotifyCollectionChangedAction notifyCollectionChangedAction = args.Action;
		if (adjustedOldIndex == adjustedNewIndex && adjustedOldIndex >= 0)
		{
			notifyCollectionChangedAction = NotifyCollectionChangedAction.Replace;
		}
		else if (adjustedOldIndex == -1)
		{
			if (adjustedNewIndex < 0 && args.Action != 0)
			{
				notifyCollectionChangedAction = NotifyCollectionChangedAction.Remove;
			}
		}
		else if (adjustedOldIndex >= -1)
		{
			notifyCollectionChangedAction = ((adjustedNewIndex < 0) ? NotifyCollectionChangedAction.Remove : NotifyCollectionChangedAction.Move);
		}
		else if (adjustedNewIndex >= 0)
		{
			notifyCollectionChangedAction = NotifyCollectionChangedAction.Add;
		}
		else if (notifyCollectionChangedAction == NotifyCollectionChangedAction.Move)
		{
			return;
		}
		int num = ((!IsGrouping) ? ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? ((!IsAddingNew) ? 1 : 2) : 0) : 0);
		int currentPosition = CurrentPosition;
		int currentPosition2 = CurrentPosition;
		object currentItem = CurrentItem;
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		object obj = ((args.OldItems != null && args.OldItems.Count > 0) ? args.OldItems[0] : null);
		object obj2 = ((args.NewItems != null && args.NewItems.Count > 0) ? args.NewItems[0] : null);
		LiveShapingList liveShapingList = InternalList as LiveShapingList;
		NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null;
		switch (notifyCollectionChangedAction)
		{
		case NotifyCollectionChangedAction.Add:
		{
			if (adjustedNewIndex == -2)
			{
				if (liveShapingList != null && IsLiveFiltering == true)
				{
					liveShapingList.AddFilteredItem(obj2);
				}
				return;
			}
			bool flag2 = obj2 == CollectionView.NewItemPlaceholder || (IsAddingNew && ItemsControl.EqualsEx(_newItem, obj2));
			if (UsesLocalArray && !flag2)
			{
				InternalList.Insert(adjustedNewIndex - num, obj2);
			}
			if (!IsGrouping)
			{
				AdjustCurrencyForAdd(adjustedNewIndex);
				args = new NotifyCollectionChangedEventArgs(notifyCollectionChangedAction, obj2, adjustedNewIndex);
			}
			else
			{
				LiveShapingItem lsi = ((liveShapingList == null || flag2) ? null : liveShapingList.ItemAt(adjustedNewIndex - num));
				AddItemToGroups(obj2, lsi);
			}
			break;
		}
		case NotifyCollectionChangedAction.Remove:
			if (adjustedOldIndex == -2)
			{
				if (liveShapingList != null && IsLiveFiltering == true)
				{
					liveShapingList.RemoveFilteredItem(obj);
				}
				return;
			}
			if (UsesLocalArray)
			{
				int num4 = adjustedOldIndex - num;
				if (num4 < InternalList.Count && ItemsControl.EqualsEx(ItemFrom(InternalList[num4]), obj))
				{
					InternalList.RemoveAt(num4);
				}
			}
			if (!IsGrouping)
			{
				AdjustCurrencyForRemove(adjustedOldIndex);
				args = new NotifyCollectionChangedEventArgs(notifyCollectionChangedAction, args.OldItems[0], adjustedOldIndex);
			}
			else
			{
				RemoveItemFromGroups(obj);
			}
			break;
		case NotifyCollectionChangedAction.Replace:
			if (adjustedOldIndex == -2)
			{
				if (liveShapingList != null && IsLiveFiltering == true)
				{
					liveShapingList.ReplaceFilteredItem(obj, obj2);
				}
				return;
			}
			if (UsesLocalArray)
			{
				InternalList[adjustedOldIndex - num] = obj2;
			}
			if (!IsGrouping)
			{
				AdjustCurrencyForReplace(adjustedOldIndex);
				args = new NotifyCollectionChangedEventArgs(notifyCollectionChangedAction, args.NewItems[0], args.OldItems[0], adjustedOldIndex);
			}
			else
			{
				LiveShapingItem lsi = liveShapingList?.ItemAt(adjustedNewIndex - num);
				RemoveItemFromGroups(obj);
				AddItemToGroups(obj2, lsi);
			}
			break;
		case NotifyCollectionChangedAction.Move:
		{
			bool flag = ItemsControl.EqualsEx(obj, obj2);
			if (UsesLocalArray && (liveShapingList == null || !liveShapingList.IsRestoringLiveSorting))
			{
				int num2 = adjustedOldIndex - num;
				int num3 = adjustedNewIndex - num;
				if (num2 < InternalList.Count && ItemsControl.EqualsEx(InternalList[num2], obj))
				{
					if (CollectionView.NewItemPlaceholder != obj2)
					{
						InternalList.Move(num2, num3);
						if (!flag)
						{
							InternalList[num3] = obj2;
						}
					}
					else
					{
						InternalList.RemoveAt(num2);
					}
				}
				else if (CollectionView.NewItemPlaceholder != obj2)
				{
					InternalList.Insert(num3, obj2);
				}
			}
			if (!IsGrouping)
			{
				AdjustCurrencyForMove(adjustedOldIndex, adjustedNewIndex);
				if (flag)
				{
					args = new NotifyCollectionChangedEventArgs(notifyCollectionChangedAction, args.OldItems[0], adjustedNewIndex, adjustedOldIndex);
					break;
				}
				notifyCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems, adjustedNewIndex);
				args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems, adjustedOldIndex);
			}
			else
			{
				LiveShapingItem lsi = liveShapingList?.ItemAt(adjustedNewIndex);
				if (flag)
				{
					MoveItemWithinGroups(obj, lsi, adjustedOldIndex, adjustedNewIndex);
					break;
				}
				RemoveItemFromGroups(obj);
				AddItemToGroups(obj2, lsi);
			}
			break;
		}
		default:
			Invariant.Assert(condition: false, SR.Format(SR.UnexpectedCollectionChangeAction, notifyCollectionChangedAction));
			break;
		}
		bool flag3 = IsCurrentAfterLast != isCurrentAfterLast;
		bool flag4 = IsCurrentBeforeFirst != isCurrentBeforeFirst;
		bool flag5 = CurrentPosition != currentPosition2;
		bool flag6 = CurrentItem != currentItem;
		isCurrentAfterLast = IsCurrentAfterLast;
		isCurrentBeforeFirst = IsCurrentBeforeFirst;
		currentPosition2 = CurrentPosition;
		currentItem = CurrentItem;
		if (!IsGrouping)
		{
			OnCollectionChanged(args);
			if (notifyCollectionChangedEventArgs != null)
			{
				OnCollectionChanged(notifyCollectionChangedEventArgs);
			}
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
		if (_currentElementWasRemoved)
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

	/// <summary>Returns the index of the specified item in the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />.</summary>
	/// <returns>The index of the specified item in the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />.</returns>
	/// <param name="item">The item to return an index for.</param>
	protected int InternalIndexOf(object item)
	{
		if (IsGrouping)
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
			case NewItemPlaceholderPosition.None:
				if (UsesLocalArray)
				{
					return InternalCount - 1;
				}
				break;
			case NewItemPlaceholderPosition.AtBeginning:
				return 1;
			case NewItemPlaceholderPosition.AtEnd:
				return InternalCount - 2;
			}
		}
		int num = InternalList.IndexOf(item);
		if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning && num >= 0)
		{
			num += ((!IsAddingNew) ? 1 : 2);
		}
		return num;
	}

	/// <summary>Returns the item at the given index in the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />.</summary>
	/// <returns>The item at the specified zero-based index in the view.</returns>
	/// <param name="index">The index at which the item is located.</param>
	protected object InternalItemAt(int index)
	{
		if (IsGrouping)
		{
			return _group.LeafAt(index);
		}
		switch (NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.None:
			if (IsAddingNew && UsesLocalArray && index == InternalCount - 1)
			{
				return _newItem;
			}
			break;
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
				if (UsesLocalArray || index <= _newItemIndex)
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
			if (IsAddingNew)
			{
				if (index == InternalCount - 2)
				{
					return _newItem;
				}
				if (!UsesLocalArray && index >= _newItemIndex)
				{
					index++;
				}
			}
			break;
		}
		return InternalList[index];
	}

	/// <summary>Return a value that indicates whether the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" /> contains the item.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" /> contains the item; otherwise, false.</returns>
	/// <param name="item">The item to locate.</param>
	protected bool InternalContains(object item)
	{
		if (item == CollectionView.NewItemPlaceholder)
		{
			return NewItemPlaceholderPosition != NewItemPlaceholderPosition.None;
		}
		if (IsGrouping)
		{
			return _group.LeafIndexOf(item) >= 0;
		}
		return InternalList.Contains(item);
	}

	/// <summary>Returns an enumerator for the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />. </summary>
	/// <returns>An enumerator for the <see cref="P:System.Windows.Data.ListCollectionView.InternalList" />.</returns>
	protected IEnumerator InternalGetEnumerator()
	{
		if (!IsGrouping)
		{
			return new PlaceholderAwareEnumerator(this, InternalList.GetEnumerator(), NewItemPlaceholderPosition, _newItem);
		}
		return _group.GetLeafEnumerator();
	}

	internal void AdjustShadowCopy(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (e.NewStartingIndex > -1)
			{
				ShadowCollection.Insert(e.NewStartingIndex, e.NewItems[0]);
			}
			else
			{
				ShadowCollection.Add(e.NewItems[0]);
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (e.OldStartingIndex > -1)
			{
				ShadowCollection.RemoveAt(e.OldStartingIndex);
			}
			else
			{
				ShadowCollection.Remove(e.OldItems[0]);
			}
			break;
		case NotifyCollectionChangedAction.Replace:
		{
			if (e.OldStartingIndex > -1)
			{
				ShadowCollection[e.OldStartingIndex] = e.NewItems[0];
				break;
			}
			int num = ShadowCollection.IndexOf(e.OldItems[0]);
			ShadowCollection[num] = e.NewItems[0];
			break;
		}
		case NotifyCollectionChangedAction.Move:
		{
			int num = e.OldStartingIndex;
			if (num < 0)
			{
				num = ShadowCollection.IndexOf(e.NewItems[0]);
			}
			ShadowCollection.RemoveAt(num);
			ShadowCollection.Insert(e.NewStartingIndex, e.NewItems[0]);
			break;
		}
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		}
	}

	internal static IComparer PrepareComparer(IComparer customSort, SortDescriptionCollection sort, Func<object, CollectionView> lazyGetCollectionView, object state)
	{
		if (customSort != null)
		{
			return customSort;
		}
		if (sort != null && sort.Count > 0)
		{
			CollectionView collectionView = lazyGetCollectionView(state);
			if (collectionView.SourceCollection != null)
			{
				IComparer comparer = SystemXmlHelper.PrepareXmlComparer(collectionView.SourceCollection, sort, collectionView.Culture);
				if (comparer != null)
				{
					return comparer;
				}
			}
			return new SortFieldComparer(sort, collectionView.Culture);
		}
		return null;
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

	private void PrepareLocalArray()
	{
		PrepareShaping();
		if (_internalList is LiveShapingList liveShapingList)
		{
			liveShapingList.LiveShapingDirty -= OnLiveShapingDirty;
			liveShapingList.Clear();
		}
		object obj;
		if (!base.AllowsCrossThreadChanges)
		{
			obj = SourceCollection as IList;
		}
		else
		{
			IList shadowCollection = ShadowCollection;
			obj = shadowCollection;
		}
		IList list = (IList)obj;
		if (!UsesLocalArray)
		{
			_internalList = list;
		}
		else
		{
			int count = list.Count;
			IList list2;
			if (!IsLiveShaping)
			{
				IList shadowCollection = new ArrayList(count);
				list2 = shadowCollection;
			}
			else
			{
				IList shadowCollection = new LiveShapingList(this, GetLiveShapingFlags(), ActiveComparer);
				list2 = shadowCollection;
			}
			IList list3 = list2;
			LiveShapingList liveShapingList2 = list3 as LiveShapingList;
			for (int i = 0; i < count; i++)
			{
				if (!IsAddingNew || i != _newItemIndex)
				{
					object obj2 = list[i];
					if (ActiveFilter == null || ActiveFilter(obj2))
					{
						list3.Add(obj2);
					}
					else if (IsLiveFiltering == true)
					{
						liveShapingList2.AddFilteredItem(obj2);
					}
				}
			}
			if (ActiveComparer != null)
			{
				list3.Sort(ActiveComparer);
			}
			if (liveShapingList2 != null)
			{
				liveShapingList2.LiveShapingDirty += OnLiveShapingDirty;
			}
			_internalList = list3;
		}
		PrepareGroups();
	}

	private void OnLiveShapingDirty(object sender, EventArgs e)
	{
		IsLiveShapingDirty = true;
	}

	private void RebuildLocalArray()
	{
		if (base.IsRefreshDeferred)
		{
			RefreshOrDefer();
		}
		else
		{
			PrepareLocalArray();
		}
	}

	private void MoveCurrencyOffDeletedElement(int oldCurrentPosition)
	{
		int num = InternalCount - 1;
		int num2 = ((oldCurrentPosition < num) ? oldCurrentPosition : num);
		_currentElementWasRemoved = false;
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

	private int AdjustBefore(NotifyCollectionChangedAction action, object item, int index)
	{
		if (action == NotifyCollectionChangedAction.Reset)
		{
			return -1;
		}
		if (item == CollectionView.NewItemPlaceholder)
		{
			if (NewItemPlaceholderPosition != NewItemPlaceholderPosition.AtBeginning)
			{
				return InternalCount - 1;
			}
			return 0;
		}
		if (IsAddingNew && NewItemPlaceholderPosition != 0 && ItemsControl.EqualsEx(item, _newItem))
		{
			if (NewItemPlaceholderPosition != NewItemPlaceholderPosition.AtBeginning)
			{
				if (!UsesLocalArray)
				{
					return index;
				}
				return InternalCount - 2;
			}
			return 1;
		}
		int num = ((!IsGrouping) ? ((NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) ? ((!IsAddingNew) ? 1 : 2) : 0) : 0);
		IEnumerable enumerable;
		if (!base.AllowsCrossThreadChanges)
		{
			enumerable = SourceCollection;
		}
		else
		{
			IEnumerable shadowCollection = ShadowCollection;
			enumerable = shadowCollection;
		}
		IList list = enumerable as IList;
		if (index < -1 || index > list.Count)
		{
			throw new InvalidOperationException(SR.Format(SR.CollectionChangeIndexOutOfRange, index, list.Count));
		}
		if (action == NotifyCollectionChangedAction.Add)
		{
			if (index >= 0)
			{
				if (!ItemsControl.EqualsEx(item, list[index]))
				{
					throw new InvalidOperationException(SR.Format(SR.AddedItemNotAtIndex, index));
				}
			}
			else
			{
				index = list.IndexOf(item);
				if (index < 0)
				{
					throw new InvalidOperationException(SR.AddedItemNotInCollection);
				}
			}
		}
		if (!UsesLocalArray)
		{
			if (IsAddingNew && NewItemPlaceholderPosition != 0 && index > _newItemIndex)
			{
				index--;
			}
			if (index >= 0)
			{
				return index + num;
			}
			return index;
		}
		switch (action)
		{
		case NotifyCollectionChangedAction.Add:
			if (!PassesFilter(item))
			{
				return -2;
			}
			if (!UsesLocalArray)
			{
				index = -1;
			}
			else if (ActiveComparer != null)
			{
				index = InternalList.Search(item, ActiveComparer);
				if (index < 0)
				{
					index = ~index;
				}
			}
			else
			{
				index = MatchingSearch(item, index, list, InternalList);
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (!IsAddingNew || item != _newItem)
			{
				index = InternalList.IndexOf(item);
				if (index < 0)
				{
					return -2;
				}
				break;
			}
			switch (NewItemPlaceholderPosition)
			{
			case NewItemPlaceholderPosition.None:
				return InternalCount - 1;
			case NewItemPlaceholderPosition.AtBeginning:
				return 1;
			case NewItemPlaceholderPosition.AtEnd:
				return InternalCount - 2;
			}
			break;
		default:
			index = -1;
			break;
		}
		if (index >= 0)
		{
			return index + num;
		}
		return index;
	}

	private int MatchingSearch(object item, int index, IList ilFull, IList ilPartial)
	{
		int num = 0;
		int num2 = 0;
		while (num < index && num2 < InternalList.Count)
		{
			if (ItemsControl.EqualsEx(ilFull[num], ilPartial[num2]))
			{
				num++;
				num2++;
			}
			else if (ItemsControl.EqualsEx(item, ilPartial[num2]))
			{
				num2++;
			}
			else
			{
				num++;
			}
		}
		return num2;
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

	private void AdjustCurrencyForRemove(int index)
	{
		if (index < CurrentPosition)
		{
			SetCurrent(CurrentItem, CurrentPosition - 1);
		}
		else if (index == CurrentPosition)
		{
			_currentElementWasRemoved = true;
		}
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

	private void AdjustCurrencyForReplace(int index)
	{
		if (index == CurrentPosition)
		{
			_currentElementWasRemoved = true;
		}
	}

	private void PrepareShaping()
	{
		ActiveComparer = PrepareComparer(_customSort, _sort, (object state) => (ListCollectionView)state, this);
		ActiveFilter = Filter;
		_group.Clear();
		_group.Initialize();
		_isGrouping = _group.GroupBy != null;
	}

	private void SetSortDescriptions(SortDescriptionCollection descriptions)
	{
		if (_sort != null)
		{
			((INotifyCollectionChanged)_sort).CollectionChanged -= SortDescriptionsChanged;
		}
		_sort = descriptions;
		if (_sort != null)
		{
			Invariant.Assert(_sort.Count == 0, "must be empty SortDescription collection");
			((INotifyCollectionChanged)_sort).CollectionChanged += SortDescriptionsChanged;
		}
	}

	private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsAddingNew || IsEditingItem)
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Sorting"));
		}
		if (_sort.Count > 0)
		{
			_customSort = null;
		}
		RefreshOrDefer();
	}

	private void PrepareGroups()
	{
		if (!_isGrouping)
		{
			return;
		}
		IComparer activeComparer = ActiveComparer;
		if (activeComparer != null)
		{
			_group.ActiveComparer = activeComparer;
		}
		else if (_group.ActiveComparer is CollectionViewGroupInternal.IListComparer listComparer)
		{
			listComparer.ResetList(InternalList);
		}
		else
		{
			_group.ActiveComparer = new CollectionViewGroupInternal.IListComparer(InternalList);
		}
		if (NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
		{
			_group.InsertSpecialItem(0, CollectionView.NewItemPlaceholder, loading: true);
			if (IsAddingNew)
			{
				_group.InsertSpecialItem(1, _newItem, loading: true);
			}
		}
		_ = IsLiveGrouping == true;
		LiveShapingList liveShapingList = InternalList as LiveShapingList;
		int i = 0;
		for (int count = InternalList.Count; i < count; i++)
		{
			object obj = InternalList[i];
			LiveShapingItem lsi = liveShapingList?.ItemAt(i);
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

	private void AddItemToGroups(object item, LiveShapingItem lsi)
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
			_group.AddToSubgroups(item, lsi, loading: false);
		}
	}

	private void RemoveItemFromGroups(object item)
	{
		if (CanGroupNamesChange || _group.RemoveFromSubgroups(item))
		{
			_group.RemoveItemFromSubgroupsByExhaustiveSearch(item);
		}
	}

	private void MoveItemWithinGroups(object item, LiveShapingItem lsi, int oldIndex, int newIndex)
	{
		_group.MoveWithinSubgroups(item, lsi, InternalList, oldIndex, newIndex);
	}

	private LiveShapingFlags GetLiveShapingFlags()
	{
		LiveShapingFlags liveShapingFlags = (LiveShapingFlags)0;
		if (IsLiveSorting == true)
		{
			liveShapingFlags |= LiveShapingFlags.Sorting;
		}
		if (IsLiveFiltering == true)
		{
			liveShapingFlags |= LiveShapingFlags.Filtering;
		}
		if (IsLiveGrouping == true)
		{
			liveShapingFlags |= LiveShapingFlags.Grouping;
		}
		return liveShapingFlags;
	}

	internal void RestoreLiveShaping()
	{
		if (!(InternalList is LiveShapingList liveShapingList))
		{
			return;
		}
		if (ActiveComparer != null)
		{
			if ((double)liveShapingList.SortDirtyItems.Count / (double)(liveShapingList.Count + 1) < 0.8)
			{
				foreach (LiveShapingItem sortDirtyItem in liveShapingList.SortDirtyItems)
				{
					if (!sortDirtyItem.IsSortDirty || sortDirtyItem.IsDeleted || !sortDirtyItem.ForwardChanges)
					{
						continue;
					}
					sortDirtyItem.IsSortDirty = false;
					sortDirtyItem.IsSortPendingClean = false;
					liveShapingList.FindPosition(sortDirtyItem, out var oldIndex, out var newIndex);
					if (oldIndex != newIndex)
					{
						if (oldIndex < newIndex)
						{
							newIndex--;
						}
						ProcessLiveShapingCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, sortDirtyItem.Item, oldIndex, newIndex), oldIndex, newIndex);
					}
				}
			}
			else
			{
				liveShapingList.RestoreLiveSortingByInsertionSort(ProcessLiveShapingCollectionChange);
			}
		}
		liveShapingList.SortDirtyItems.Clear();
		if (ActiveFilter != null)
		{
			foreach (LiveShapingItem filterDirtyItem in liveShapingList.FilterDirtyItems)
			{
				if (!filterDirtyItem.IsFilterDirty || !filterDirtyItem.ForwardChanges)
				{
					continue;
				}
				object item = filterDirtyItem.Item;
				bool failsFilter = filterDirtyItem.FailsFilter;
				bool flag = !PassesFilter(item);
				if (failsFilter != flag)
				{
					if (flag)
					{
						int num = liveShapingList.IndexOf(filterDirtyItem);
						ProcessLiveShapingCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, num), num, -1);
						liveShapingList.AddFilteredItem(filterDirtyItem);
					}
					else
					{
						liveShapingList.RemoveFilteredItem(filterDirtyItem);
						int num;
						if (ActiveComparer != null)
						{
							num = liveShapingList.Search(0, liveShapingList.Count, item);
							if (num < 0)
							{
								num = ~num;
							}
						}
						else
						{
							IEnumerable enumerable;
							if (!base.AllowsCrossThreadChanges)
							{
								enumerable = SourceCollection;
							}
							else
							{
								IEnumerable shadowCollection = ShadowCollection;
								enumerable = shadowCollection;
							}
							IList list = enumerable as IList;
							for (num = filterDirtyItem.GetAndClearStartingIndex(); num < list.Count && !ItemsControl.EqualsEx(item, list[num]); num++)
							{
							}
							liveShapingList.SetStartingIndexForFilteredItem(item, num + 1);
							num = MatchingSearch(item, num, list, liveShapingList);
						}
						ProcessLiveShapingCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, num), -1, num);
					}
				}
				filterDirtyItem.IsFilterDirty = false;
			}
		}
		liveShapingList.FilterDirtyItems.Clear();
		if (IsGrouping)
		{
			List<AbandonedGroupItem> deleteList = new List<AbandonedGroupItem>();
			foreach (LiveShapingItem groupDirtyItem in liveShapingList.GroupDirtyItems)
			{
				if (groupDirtyItem.IsGroupDirty && !groupDirtyItem.IsDeleted && groupDirtyItem.ForwardChanges)
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

	private void ProcessLiveShapingCollectionChange(NotifyCollectionChangedEventArgs args, int oldIndex, int newIndex)
	{
		if (!IsGrouping && NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
		{
			if (oldIndex >= 0)
			{
				oldIndex++;
			}
			if (newIndex >= 0)
			{
				newIndex++;
			}
		}
		ProcessCollectionChangedWithAdjustedIndex(args, oldIndex, newIndex);
	}

	private object ItemFrom(object o)
	{
		if (o is LiveShapingItem liveShapingItem)
		{
			return liveShapingItem.Item;
		}
		return o;
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
