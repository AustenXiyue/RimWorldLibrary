using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using MS.Internal.Controls;
using MS.Internal.Data;
using MS.Internal.KnownBoxes;
using MS.Internal.Utility;

namespace System.Windows.Controls;

/// <summary>Holds the list of items that constitute the content of an <see cref="T:System.Windows.Controls.ItemsControl" />.</summary>
[Localizability(LocalizationCategory.Ignore)]
public sealed class ItemCollection : CollectionView, IList, ICollection, IEnumerable, IEditableCollectionViewAddNewItem, IEditableCollectionView, ICollectionViewLiveShaping, IItemProperties, IWeakEventListener
{
	private class ShapingStorage
	{
		public bool _isSortingSet;

		public bool _isGroupingSet;

		public bool _isLiveSortingSet;

		public bool _isLiveFilteringSet;

		public bool _isLiveGroupingSet;

		public SortDescriptionCollection _sort;

		public Predicate<object> _filter;

		public ObservableCollection<GroupDescription> _groupBy;

		public bool? _isLiveSorting;

		public bool? _isLiveFiltering;

		public bool? _isLiveGrouping;

		public ObservableCollection<string> _liveSortingProperties;

		public ObservableCollection<string> _liveFilteringProperties;

		public ObservableCollection<string> _liveGroupingProperties;

		public MonitorWrapper _sortDescriptionsMonitor;

		public MonitorWrapper _groupDescriptionsMonitor;

		public MonitorWrapper _liveSortingMonitor;

		public MonitorWrapper _liveFilteringMonitor;

		public MonitorWrapper _liveGroupingMonitor;
	}

	private class DeferHelper : IDisposable
	{
		private ItemCollection _itemCollection;

		public DeferHelper(ItemCollection itemCollection)
		{
			_itemCollection = itemCollection;
		}

		public void Dispose()
		{
			if (_itemCollection != null)
			{
				_itemCollection.EndDefer();
				_itemCollection = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	private InnerItemCollectionView _internalView;

	private IEnumerable _itemsSource;

	private CollectionView _collectionView;

	private int _defaultCapacity = 16;

	private bool _isUsingItemsSource;

	private bool _isInitializing;

	private int _deferLevel;

	private IDisposable _deferInnerRefresh;

	private ShapingStorage _shapingStorage;

	private WeakReference _modelParent;

	/// <summary>Gets the number of records in the collection. </summary>
	/// <returns>The number of items in the collection or 0 if the collection is uninitialized or if there is no collection in the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode.</returns>
	public override int Count
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return 0;
			}
			VerifyRefreshNotDeferred();
			return _collectionView.Count;
		}
	}

	/// <summary>Gets a value that indicates whether the resulting (filtered) view is empty.</summary>
	/// <returns>true if the resulting view is empty; otherwise, false.</returns>
	public override bool IsEmpty
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return true;
			}
			VerifyRefreshNotDeferred();
			return _collectionView.IsEmpty;
		}
	}

	/// <summary>Gets or sets the item at the given zero-based index.</summary>
	/// <returns>The object retrieved or the object that is being set to the specified index.</returns>
	/// <param name="index">The zero-based index of the item.</param>
	/// <exception cref="T:System.InvalidOperationException">The collection is uninitialized, or the item to set already has a different logical parent, or the collection is in <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index is out of range.</exception>
	public object this[int index]
	{
		get
		{
			return GetItemAt(index);
		}
		set
		{
			CheckIsUsingInnerView();
			if (index < 0 || index >= _internalView.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			_internalView[index] = value;
		}
	}

	/// <summary>Gets the unsorted and unfiltered collection that underlies this collection view. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerable" /> object that is the underlying collection or the user-provided <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> collection.</returns>
	public override IEnumerable SourceCollection
	{
		get
		{
			if (IsUsingItemsSource)
			{
				return ItemsSource;
			}
			EnsureInternalView();
			return this;
		}
	}

	/// <summary>Gets a value that indicates whether the collection needs to be refreshed.</summary>
	/// <returns>true if the collection needs to be refreshed; otherwise, false.</returns>
	public override bool NeedsRefresh
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return false;
			}
			return _collectionView.NeedsRefresh;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describe how the items in the collection are sorted in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describe how the items in the collection are sorted in the view.</returns>
	public override SortDescriptionCollection SortDescriptions
	{
		get
		{
			if (MySortDescriptions == null)
			{
				MySortDescriptions = new SortDescriptionCollection();
				if (_collectionView != null)
				{
					CloneList(MySortDescriptions, _collectionView.SortDescriptions);
				}
				((INotifyCollectionChanged)MySortDescriptions).CollectionChanged += SortDescriptionsChanged;
			}
			return MySortDescriptions;
		}
	}

	/// <summary>Gets a value that indicates whether this collection view supports sorting.</summary>
	/// <returns>true if this view support sorting; otherwise, false. The default value is true.</returns>
	public override bool CanSort
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return true;
			}
			return _collectionView.CanSort;
		}
	}

	/// <summary>Gets or sets a callback used to determine if an item is suitable for inclusion in the view.</summary>
	/// <returns>A method used to determine if an item is suitable for inclusion in the view.</returns>
	/// <exception cref="T:System.NotSupportedException">Filtering is not supported.</exception>
	public override Predicate<object> Filter
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return MyFilter;
			}
			return _collectionView.Filter;
		}
		set
		{
			MyFilter = value;
			if (_collectionView != null)
			{
				_collectionView.Filter = value;
			}
		}
	}

	/// <summary>Gets a value that indicates whether this collection view supports filtering.</summary>
	/// <returns>true if this view supports filtering; otherwise, false. The default value is true.</returns>
	public override bool CanFilter
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return true;
			}
			return _collectionView.CanFilter;
		}
	}

	/// <summary>Gets a value that indicates whether this collection view supports grouping.</summary>
	/// <returns>true if the collection supports grouping; otherwise, false. The default value is false.</returns>
	public override bool CanGroup
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return false;
			}
			return _collectionView.CanGroup;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that defines how to group the items.</summary>
	/// <returns>An <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> of <see cref="T:System.ComponentModel.GroupDescription" /> objects. The collection is indexed by the group levels.</returns>
	public override ObservableCollection<GroupDescription> GroupDescriptions
	{
		get
		{
			if (MyGroupDescriptions == null)
			{
				MyGroupDescriptions = new ObservableCollection<GroupDescription>();
				if (_collectionView != null)
				{
					CloneList(MyGroupDescriptions, _collectionView.GroupDescriptions);
				}
				((INotifyCollectionChanged)MyGroupDescriptions).CollectionChanged += GroupDescriptionsChanged;
			}
			return MyGroupDescriptions;
		}
	}

	/// <summary>Gets the top-level groups that are constructed according to the <see cref="P:System.Windows.Controls.ItemCollection.GroupDescriptions" />.</summary>
	/// <returns>The top-level groups that are constructed according to the <see cref="P:System.Windows.Controls.ItemCollection.GroupDescriptions" />. The default value is null.</returns>
	public override ReadOnlyObservableCollection<object> Groups
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return null;
			}
			return _collectionView.Groups;
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (IsUsingItemsSource)
			{
				throw new NotSupportedException(SR.ItemCollectionShouldUseInnerSyncRoot);
			}
			return _internalView.SyncRoot;
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => IsUsingItemsSource;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read only; otherwise, false.</returns>
	bool IList.IsReadOnly => IsUsingItemsSource;

	/// <summary>Gets the ordinal position of the current item within the view.</summary>
	/// <returns>The ordinal position of the current item within the view or -1 if the collection is uninitialized or if there is no collection in the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode.</returns>
	public override int CurrentPosition
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return -1;
			}
			VerifyRefreshNotDeferred();
			return _collectionView.CurrentPosition;
		}
	}

	/// <summary>Gets the current item in the view.</summary>
	/// <returns>The current object in the view or null if the collection is uninitialized or if there is no collection in the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode.By default, the first item of the collection starts as the current item.</returns>
	public override object CurrentItem
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return null;
			}
			VerifyRefreshNotDeferred();
			return _collectionView.CurrentItem;
		}
	}

	/// <summary>Gets a value that indicates whether the current item of the view is beyond the end of the collection.</summary>
	/// <returns>true if the current item of the view is beyond the end of the collection; otherwise, false.</returns>
	public override bool IsCurrentAfterLast
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return false;
			}
			VerifyRefreshNotDeferred();
			return _collectionView.IsCurrentAfterLast;
		}
	}

	/// <summary>Gets a value that indicates whether the current item of the view is beyond the beginning of the collection.</summary>
	/// <returns>true if the current item of the view is beyond the beginning of the collection; otherwise, false.</returns>
	public override bool IsCurrentBeforeFirst
	{
		get
		{
			if (!EnsureCollectionView())
			{
				return false;
			}
			VerifyRefreshNotDeferred();
			return _collectionView.IsCurrentBeforeFirst;
		}
	}

	/// <summary>Gets or sets the position of the new item placeholder in the collection view.</summary>
	/// <returns>One of the enumeration values that specifies the position of the new item placeholder in the collection view.</returns>
	NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.NewItemPlaceholderPosition;
			}
			return NewItemPlaceholderPosition.None;
		}
		set
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				editableCollectionView.NewItemPlaceholderPosition = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "NewItemPlaceholderPosition"));
		}
	}

	/// <summary>Gets a value that indicates whether a new item can be added to the collection.</summary>
	/// <returns>true if a new item can be added to the collection; otherwise, false.</returns>
	bool IEditableCollectionView.CanAddNew
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CanAddNew;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether an add transaction is in progress.</summary>
	/// <returns>true if an add transaction is in progress; otherwise, false.</returns>
	bool IEditableCollectionView.IsAddingNew
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.IsAddingNew;
			}
			return false;
		}
	}

	/// <summary>Gets the item that is being added during the current add transaction.</summary>
	/// <returns>The item that is being added if <see cref="P:System.ComponentModel.IEditableCollectionView.IsAddingNew" /> is true; otherwise, null.</returns>
	object IEditableCollectionView.CurrentAddItem
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CurrentAddItem;
			}
			return null;
		}
	}

	/// <summary>Gets a value that indicates whether an item can be removed from the collection.</summary>
	/// <returns>true if an item can be removed from the collection; otherwise, false.</returns>
	bool IEditableCollectionView.CanRemove
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CanRemove;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view can discard pending changes and restore the original values of an edited object.</summary>
	/// <returns>true if the collection view can discard pending changes and restore the original values of an edited object; otherwise, false.</returns>
	bool IEditableCollectionView.CanCancelEdit
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CanCancelEdit;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether an edit transaction is in progress.</summary>
	/// <returns>true if an edit transaction is in progress; otherwise, false.</returns>
	bool IEditableCollectionView.IsEditingItem
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.IsEditingItem;
			}
			return false;
		}
	}

	/// <summary>Gets the item in the collection that is being edited.</summary>
	/// <returns>The item in the collection that is being edited if <see cref="P:System.ComponentModel.IEditableCollectionView.IsEditingItem" /> is true; otherwise, null.</returns>
	object IEditableCollectionView.CurrentEditItem
	{
		get
		{
			if (_collectionView is IEditableCollectionView editableCollectionView)
			{
				return editableCollectionView.CurrentEditItem;
			}
			return null;
		}
	}

	/// <summary>Gets a value that indicates whether a specified object can be added to the collection.</summary>
	/// <returns>true if a specified object can be added to the collection; otherwise, false.</returns>
	bool IEditableCollectionViewAddNewItem.CanAddNewItem
	{
		get
		{
			if (_collectionView is IEditableCollectionViewAddNewItem editableCollectionViewAddNewItem)
			{
				return editableCollectionViewAddNewItem.CanAddNewItem;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view supports turning sorting data in real time on or off.</summary>
	/// <returns>true if the collection view supports turning live sorting on or off; otherwise, false.</returns>
	public bool CanChangeLiveSorting
	{
		get
		{
			if (!(_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return false;
			}
			return collectionViewLiveShaping.CanChangeLiveSorting;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view supports turning filtering data in real time on or off.</summary>
	/// <returns>true if the collection view supports turning live filtering on or off; otherwise, false.</returns>
	public bool CanChangeLiveFiltering
	{
		get
		{
			if (!(_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return false;
			}
			return collectionViewLiveShaping.CanChangeLiveFiltering;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view supports turning grouping data in real time on or off.</summary>
	/// <returns>true if the collection view supports turning live grouping on or off; otherwise, false.</returns>
	public bool CanChangeLiveGrouping
	{
		get
		{
			if (!(_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return false;
			}
			return collectionViewLiveShaping.CanChangeLiveGrouping;
		}
	}

	/// <summary>Gets or sets a value that indicates whether sorting in real time is enabled.</summary>
	/// <returns>true if sorting data in real time is enabled; false if live sorting is not enabled; null if it cannot be determined whether the collection view implements live sorting.</returns>
	public bool? IsLiveSorting
	{
		get
		{
			if (!(_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return null;
			}
			return collectionViewLiveShaping.IsLiveSorting;
		}
		set
		{
			MyIsLiveSorting = value;
			if (_collectionView is ICollectionViewLiveShaping { CanChangeLiveSorting: not false } collectionViewLiveShaping)
			{
				collectionViewLiveShaping.IsLiveSorting = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether filtering data in real time is enabled.</summary>
	/// <returns>true if filtering data in real time is enabled; false if live filtering is not enabled; null if it cannot be determined whether the collection view implements live filtering.</returns>
	public bool? IsLiveFiltering
	{
		get
		{
			if (!(_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return null;
			}
			return collectionViewLiveShaping.IsLiveFiltering;
		}
		set
		{
			MyIsLiveFiltering = value;
			if (_collectionView is ICollectionViewLiveShaping { CanChangeLiveFiltering: not false } collectionViewLiveShaping)
			{
				collectionViewLiveShaping.IsLiveFiltering = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether grouping data in real time is enabled.</summary>
	/// <returns>true if grouping data in real time is enabled; false if live grouping is not enabled; null if it cannot be determined whether the collection view implements live grouping.</returns>
	public bool? IsLiveGrouping
	{
		get
		{
			if (!(_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping))
			{
				return null;
			}
			return collectionViewLiveShaping.IsLiveGrouping;
		}
		set
		{
			MyIsLiveGrouping = value;
			if (_collectionView is ICollectionViewLiveShaping { CanChangeLiveGrouping: not false } collectionViewLiveShaping)
			{
				collectionViewLiveShaping.IsLiveGrouping = value;
			}
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in sorting data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in sorting data in real time.</returns>
	public ObservableCollection<string> LiveSortingProperties
	{
		get
		{
			if (MyLiveSortingProperties == null)
			{
				MyLiveSortingProperties = new ObservableCollection<string>();
				if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
				{
					CloneList(MyLiveSortingProperties, collectionViewLiveShaping.LiveSortingProperties);
				}
				((INotifyCollectionChanged)MyLiveSortingProperties).CollectionChanged += LiveSortingChanged;
			}
			return MyLiveSortingProperties;
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in filtering data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in filtering data in real time.</returns>
	public ObservableCollection<string> LiveFilteringProperties
	{
		get
		{
			if (MyLiveFilteringProperties == null)
			{
				MyLiveFilteringProperties = new ObservableCollection<string>();
				if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
				{
					CloneList(MyLiveFilteringProperties, collectionViewLiveShaping.LiveFilteringProperties);
				}
				((INotifyCollectionChanged)MyLiveFilteringProperties).CollectionChanged += LiveFilteringChanged;
			}
			return MyLiveFilteringProperties;
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in grouping data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in grouping data in real time.</returns>
	public ObservableCollection<string> LiveGroupingProperties
	{
		get
		{
			if (MyLiveGroupingProperties == null)
			{
				MyLiveGroupingProperties = new ObservableCollection<string>();
				if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
				{
					CloneList(MyLiveGroupingProperties, collectionViewLiveShaping.LiveGroupingProperties);
				}
				((INotifyCollectionChanged)MyLiveGroupingProperties).CollectionChanged += LiveGroupingChanged;
			}
			return MyLiveGroupingProperties;
		}
	}

	/// <summary>Gets a collection that contains information about the properties that are available on the items in a collection.</summary>
	/// <returns>A collection that contains information about the properties that are available on the items in a collection.</returns>
	ReadOnlyCollection<ItemPropertyInfo> IItemProperties.ItemProperties
	{
		get
		{
			if (_collectionView is IItemProperties itemProperties)
			{
				return itemProperties.ItemProperties;
			}
			return null;
		}
	}

	internal DependencyObject ModelParent => (DependencyObject)_modelParent.Target;

	internal FrameworkElement ModelParentFE => ModelParent as FrameworkElement;

	internal IEnumerable ItemsSource => _itemsSource;

	internal bool IsUsingItemsSource => _isUsingItemsSource;

	internal CollectionView CollectionView => _collectionView;

	internal IEnumerator LogicalChildren
	{
		get
		{
			EnsureInternalView();
			return _internalView.LogicalChildren;
		}
	}

	private new bool IsRefreshDeferred => _deferLevel > 0;

	private bool IsShapingActive => _shapingStorage != null;

	private SortDescriptionCollection MySortDescriptions
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._sort;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._sort = value;
		}
	}

	private bool IsSortingSet
	{
		get
		{
			if (!IsShapingActive)
			{
				return false;
			}
			return _shapingStorage._isSortingSet;
		}
		set
		{
			_shapingStorage._isSortingSet = value;
		}
	}

	private MonitorWrapper SortDescriptionsMonitor
	{
		get
		{
			if (_shapingStorage._sortDescriptionsMonitor == null)
			{
				_shapingStorage._sortDescriptionsMonitor = new MonitorWrapper();
			}
			return _shapingStorage._sortDescriptionsMonitor;
		}
	}

	private Predicate<object> MyFilter
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._filter;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._filter = value;
		}
	}

	private ObservableCollection<GroupDescription> MyGroupDescriptions
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._groupBy;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._groupBy = value;
		}
	}

	private bool IsGroupingSet
	{
		get
		{
			if (!IsShapingActive)
			{
				return false;
			}
			return _shapingStorage._isGroupingSet;
		}
		set
		{
			if (IsShapingActive)
			{
				_shapingStorage._isGroupingSet = value;
			}
		}
	}

	private MonitorWrapper GroupDescriptionsMonitor
	{
		get
		{
			if (_shapingStorage._groupDescriptionsMonitor == null)
			{
				_shapingStorage._groupDescriptionsMonitor = new MonitorWrapper();
			}
			return _shapingStorage._groupDescriptionsMonitor;
		}
	}

	private bool? MyIsLiveSorting
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._isLiveSorting;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._isLiveSorting = value;
		}
	}

	private ObservableCollection<string> MyLiveSortingProperties
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._liveSortingProperties;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._liveSortingProperties = value;
		}
	}

	private bool IsLiveSortingSet
	{
		get
		{
			if (!IsShapingActive)
			{
				return false;
			}
			return _shapingStorage._isLiveSortingSet;
		}
		set
		{
			_shapingStorage._isLiveSortingSet = value;
		}
	}

	private MonitorWrapper LiveSortingMonitor
	{
		get
		{
			if (_shapingStorage._liveSortingMonitor == null)
			{
				_shapingStorage._liveSortingMonitor = new MonitorWrapper();
			}
			return _shapingStorage._liveSortingMonitor;
		}
	}

	private bool? MyIsLiveFiltering
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._isLiveFiltering;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._isLiveFiltering = value;
		}
	}

	private ObservableCollection<string> MyLiveFilteringProperties
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._liveFilteringProperties;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._liveFilteringProperties = value;
		}
	}

	private bool IsLiveFilteringSet
	{
		get
		{
			if (!IsShapingActive)
			{
				return false;
			}
			return _shapingStorage._isLiveFilteringSet;
		}
		set
		{
			_shapingStorage._isLiveFilteringSet = value;
		}
	}

	private MonitorWrapper LiveFilteringMonitor
	{
		get
		{
			if (_shapingStorage._liveFilteringMonitor == null)
			{
				_shapingStorage._liveFilteringMonitor = new MonitorWrapper();
			}
			return _shapingStorage._liveFilteringMonitor;
		}
	}

	private bool? MyIsLiveGrouping
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._isLiveGrouping;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._isLiveGrouping = value;
		}
	}

	private ObservableCollection<string> MyLiveGroupingProperties
	{
		get
		{
			if (!IsShapingActive)
			{
				return null;
			}
			return _shapingStorage._liveGroupingProperties;
		}
		set
		{
			EnsureShapingStorage();
			_shapingStorage._liveGroupingProperties = value;
		}
	}

	private bool IsLiveGroupingSet
	{
		get
		{
			if (!IsShapingActive)
			{
				return false;
			}
			return _shapingStorage._isLiveGroupingSet;
		}
		set
		{
			_shapingStorage._isLiveGroupingSet = value;
		}
	}

	private MonitorWrapper LiveGroupingMonitor
	{
		get
		{
			if (_shapingStorage._liveGroupingMonitor == null)
			{
				_shapingStorage._liveGroupingMonitor = new MonitorWrapper();
			}
			return _shapingStorage._liveGroupingMonitor;
		}
	}

	internal ItemCollection(DependencyObject modelParent)
		: base(EmptyEnumerable.Instance, shouldProcessCollectionChanged: false)
	{
		_modelParent = new WeakReference(modelParent);
	}

	internal ItemCollection(FrameworkElement modelParent, int capacity)
		: base(EmptyEnumerable.Instance, shouldProcessCollectionChanged: false)
	{
		_defaultCapacity = capacity;
		_modelParent = new WeakReference(modelParent);
	}

	/// <summary>Sets the first item in the view as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</summary>
	/// <returns>true to indicate that the resulting <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public override bool MoveCurrentToFirst()
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.MoveCurrentToFirst();
	}

	/// <summary>Sets the item after the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> in the view as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</summary>
	/// <returns>true to indicate that the resulting <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public override bool MoveCurrentToNext()
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.MoveCurrentToNext();
	}

	/// <summary>Sets the item before the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> in the view as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</summary>
	/// <returns>true  to indicate that the resulting <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public override bool MoveCurrentToPrevious()
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.MoveCurrentToPrevious();
	}

	/// <summary>Sets the last item in the view as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</summary>
	/// <returns>true to indicate that the resulting <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public override bool MoveCurrentToLast()
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.MoveCurrentToLast();
	}

	/// <summary>Sets the specified item in the collection as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</summary>
	/// <returns>true to indicate that the resulting <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	/// <param name="item">The item to set as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</param>
	public override bool MoveCurrentTo(object item)
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.MoveCurrentTo(item);
	}

	/// <summary>Sets the item at the specified index to be the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> in the view.</summary>
	/// <returns>true to indicate that the resulting <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	/// <param name="position">The zero-based index of the item to set as the <see cref="P:System.Windows.Controls.ItemCollection.CurrentItem" />.</param>
	public override bool MoveCurrentToPosition(int position)
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.MoveCurrentToPosition(position);
	}

	protected override IEnumerator GetEnumerator()
	{
		if (!EnsureCollectionView())
		{
			return EmptyEnumerator.Instance;
		}
		return ((IEnumerable)_collectionView).GetEnumerator();
	}

	/// <summary>Adds an item to the <see cref="T:System.Windows.Controls.ItemCollection" />.</summary>
	/// <returns>The zero-based index at which the object is added or -1 if the item cannot be added.</returns>
	/// <param name="newItem">The item to add to the collection.</param>
	/// <exception cref="T:System.InvalidOperationException">The item to add already has a different logical parent. </exception>
	/// <exception cref="T:System.InvalidOperationException">The collection is in ItemsSource mode.</exception>
	public int Add(object newItem)
	{
		CheckIsUsingInnerView();
		int result = _internalView.Add(newItem);
		ModelParent.SetValue(ItemsControl.HasItemsPropertyKey, BooleanBoxes.TrueBox);
		return result;
	}

	/// <summary>Clears the collection and releases the references on all items currently in the collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.ItemCollection" /> is in <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode. (When the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> property is set, the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection will be made read-only and fixed-size.)</exception>
	public void Clear()
	{
		VerifyRefreshNotDeferred();
		if (IsUsingItemsSource)
		{
			throw new InvalidOperationException(SR.ItemsSourceInUse);
		}
		if (_internalView != null)
		{
			_internalView.Clear();
		}
		ModelParent.ClearValue(ItemsControl.HasItemsPropertyKey);
	}

	/// <summary>Returns a value that indicates whether the specified item is in this view.</summary>
	/// <returns>true to indicate that the item belongs to this collection and passes the active filter; otherwise, false.</returns>
	/// <param name="containItem">The object to check.</param>
	public override bool Contains(object containItem)
	{
		if (!EnsureCollectionView())
		{
			return false;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.Contains(containItem);
	}

	/// <summary>Copies the elements of the collection to an array, starting at a particular array index. </summary>
	/// <param name="array">The destination array to copy to.</param>
	/// <param name="index">The zero-based index in the destination array.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="array" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The destination <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is less than 0.</exception>
	public void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank > 1)
		{
			throw new ArgumentException(SR.BadTargetArray, "array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (EnsureCollectionView())
		{
			VerifyRefreshNotDeferred();
			IndexedEnumerable.CopyTo(_collectionView, array, index);
		}
	}

	/// <summary>Returns the index in this collection where the specified item is located. </summary>
	/// <returns>The index of the item in the collection, or -1 if the item does not exist in the collection.</returns>
	/// <param name="item">The object to look for in the collection.</param>
	public override int IndexOf(object item)
	{
		if (!EnsureCollectionView())
		{
			return -1;
		}
		VerifyRefreshNotDeferred();
		return _collectionView.IndexOf(item);
	}

	/// <summary>Returns the item at the specified zero-based index in this view.</summary>
	/// <returns>The item at the specified zero-based index in this view.</returns>
	/// <param name="index">The zero-based index at which the item is located.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is out of range.</exception>
	/// <exception cref="T:System.InvalidOperationException">The collection is uninitialized or the binding on <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> supplied a null value.</exception>
	public override object GetItemAt(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		VerifyRefreshNotDeferred();
		if (!EnsureCollectionView())
		{
			throw new InvalidOperationException(SR.ItemCollectionHasNoCollection);
		}
		if (_collectionView == _internalView && index >= _internalView.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _collectionView.GetItemAt(index);
	}

	/// <summary> Inserts an element into the collection at the specified index. </summary>
	/// <param name="insertIndex">The zero-based index at which to insert the item.</param>
	/// <param name="insertItem">The item to insert.</param>
	/// <exception cref="T:System.InvalidOperationException">The item to insert already has a different logical parent. </exception>
	/// <exception cref="T:System.InvalidOperationException">The collection is in ItemsSource mode.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index is out of range. </exception>
	public void Insert(int insertIndex, object insertItem)
	{
		CheckIsUsingInnerView();
		_internalView.Insert(insertIndex, insertItem);
		ModelParent.SetValue(ItemsControl.HasItemsPropertyKey, BooleanBoxes.TrueBox);
	}

	/// <summary>Removes the specified item reference from the collection or view.</summary>
	/// <param name="removeItem">The object to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.ItemCollection" /> is read-only because it is in <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode or if DeferRefresh is in effect.</exception>
	public void Remove(object removeItem)
	{
		CheckIsUsingInnerView();
		_internalView.Remove(removeItem);
		if (IsEmpty)
		{
			ModelParent.ClearValue(ItemsControl.HasItemsPropertyKey);
		}
	}

	/// <summary>Removes the item at the specified index of the collection or view.</summary>
	/// <param name="removeIndex">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.ItemCollection" /> is read-only because it is in <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> mode or if DeferRefresh is in effect.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index is out of range.</exception>
	public void RemoveAt(int removeIndex)
	{
		CheckIsUsingInnerView();
		_internalView.RemoveAt(removeIndex);
		if (IsEmpty)
		{
			ModelParent.ClearValue(ItemsControl.HasItemsPropertyKey);
		}
	}

	/// <summary>Returns a value that indicates whether the specified item belongs to this view.</summary>
	/// <returns>true to indicate that the specified item belongs to this view or there is no filter set on this collection view; otherwise, false.</returns>
	/// <param name="item">The object to test.</param>
	public override bool PassesFilter(object item)
	{
		if (!EnsureCollectionView())
		{
			return true;
		}
		return _collectionView.PassesFilter(item);
	}

	protected override void RefreshOverride()
	{
		if (_collectionView != null)
		{
			if (_collectionView.NeedsRefresh)
			{
				_collectionView.Refresh();
			}
			else
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}
	}

	/// <summary>Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that you can use to dispose of the calling object.</returns>
	public override IDisposable DeferRefresh()
	{
		if (_deferLevel == 0 && _collectionView != null)
		{
			_deferInnerRefresh = _collectionView.DeferRefresh();
		}
		_deferLevel++;
		return new DeferHelper(this);
	}

	/// <summary>Adds a new item to the collection.</summary>
	/// <returns>The new item that is added to the collection.</returns>
	object IEditableCollectionView.AddNew()
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			return editableCollectionView.AddNew();
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "AddNew"));
	}

	/// <summary>Ends the add transaction and saves the pending new item.</summary>
	void IEditableCollectionView.CommitNew()
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CommitNew();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CommitNew"));
	}

	/// <summary>Ends the add transaction and discards the pending new item.</summary>
	void IEditableCollectionView.CancelNew()
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CancelNew();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CancelNew"));
	}

	/// <summary>Removes the item at the specified position from the collection.</summary>
	/// <param name="index">The position of the item to remove.</param>
	void IEditableCollectionView.RemoveAt(int index)
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.RemoveAt(index);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "RemoveAt"));
	}

	/// <summary>Removes the specified item from the collection.</summary>
	/// <param name="item">The item to remove.</param>
	void IEditableCollectionView.Remove(object item)
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.Remove(item);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "Remove"));
	}

	/// <summary>Begins an edit transaction of the specified item.</summary>
	/// <param name="item">The item to edit.</param>
	void IEditableCollectionView.EditItem(object item)
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.EditItem(item);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "EditItem"));
	}

	/// <summary>Ends the edit transaction and saves the pending changes.</summary>
	void IEditableCollectionView.CommitEdit()
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CommitEdit();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CommitEdit"));
	}

	/// <summary>Ends the edit transaction and, if possible, restores the original value to the item.</summary>
	void IEditableCollectionView.CancelEdit()
	{
		if (_collectionView is IEditableCollectionView editableCollectionView)
		{
			editableCollectionView.CancelEdit();
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "CancelEdit"));
	}

	/// <summary>Adds the specified object to the collection.</summary>
	/// <returns>The object that was added to the collection.</returns>
	/// <param name="newItem">The object to add to the collection.</param>
	object IEditableCollectionViewAddNewItem.AddNewItem(object newItem)
	{
		if (_collectionView is IEditableCollectionViewAddNewItem editableCollectionViewAddNewItem)
		{
			return editableCollectionViewAddNewItem.AddNewItem(newItem);
		}
		throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedForView, "AddNewItem"));
	}

	internal void SetItemsSource(IEnumerable value, Func<object, object> GetSourceItem = null)
	{
		if (!IsUsingItemsSource && _internalView != null && _internalView.RawCount > 0)
		{
			throw new InvalidOperationException(SR.CannotUseItemsSource);
		}
		_itemsSource = value;
		_isUsingItemsSource = true;
		SetCollectionView(CollectionViewSource.GetDefaultCollectionView(_itemsSource, ModelParent, GetSourceItem));
	}

	internal void ClearItemsSource()
	{
		if (IsUsingItemsSource)
		{
			_itemsSource = null;
			_isUsingItemsSource = false;
			SetCollectionView(_internalView);
		}
	}

	internal void BeginInit()
	{
		_isInitializing = true;
		if (_collectionView != null)
		{
			UnhookCollectionView(_collectionView);
		}
	}

	internal void EndInit()
	{
		EnsureCollectionView();
		_isInitializing = false;
		if (_collectionView != null)
		{
			HookCollectionView(_collectionView);
			Refresh();
		}
	}

	internal override void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, false, sources);
		if (_collectionView != null)
		{
			_collectionView.GetCollectionChangedSources(level + 1, format, sources);
		}
	}

	private bool EnsureCollectionView()
	{
		if (_collectionView == null && !IsUsingItemsSource && _internalView != null)
		{
			if (_internalView.IsEmpty)
			{
				bool isInitializing = _isInitializing;
				_isInitializing = true;
				SetCollectionView(_internalView);
				_isInitializing = isInitializing;
			}
			else
			{
				SetCollectionView(_internalView);
			}
			if (!_isInitializing)
			{
				HookCollectionView(_collectionView);
			}
		}
		return _collectionView != null;
	}

	private void EnsureInternalView()
	{
		if (_internalView == null)
		{
			_internalView = new InnerItemCollectionView(_defaultCapacity, this);
		}
	}

	private void SetCollectionView(CollectionView view)
	{
		if (_collectionView == view)
		{
			return;
		}
		if (_collectionView != null)
		{
			if (!_isInitializing)
			{
				UnhookCollectionView(_collectionView);
			}
			if (IsRefreshDeferred)
			{
				_deferInnerRefresh.Dispose();
				_deferInnerRefresh = null;
			}
		}
		bool flag = false;
		_collectionView = view;
		InvalidateEnumerableWrapper();
		if (_collectionView != null)
		{
			_deferInnerRefresh = _collectionView.DeferRefresh();
			ApplySortFilterAndGroup();
			if (!_isInitializing)
			{
				HookCollectionView(_collectionView);
			}
			if (!IsRefreshDeferred)
			{
				flag = !_collectionView.NeedsRefresh;
				_deferInnerRefresh.Dispose();
				_deferInnerRefresh = null;
			}
		}
		else if (!IsRefreshDeferred)
		{
			flag = true;
		}
		if (flag)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
		OnPropertyChanged(new PropertyChangedEventArgs("IsLiveSorting"));
		OnPropertyChanged(new PropertyChangedEventArgs("IsLiveFiltering"));
		OnPropertyChanged(new PropertyChangedEventArgs("IsLiveGrouping"));
	}

	private void ApplySortFilterAndGroup()
	{
		if (!IsShapingActive)
		{
			return;
		}
		if (_collectionView.CanSort)
		{
			SortDescriptionCollection master = (IsSortingSet ? MySortDescriptions : _collectionView.SortDescriptions);
			SortDescriptionCollection clone = (IsSortingSet ? _collectionView.SortDescriptions : MySortDescriptions);
			using (SortDescriptionsMonitor.Enter())
			{
				CloneList(clone, master);
			}
		}
		if (_collectionView.CanFilter && MyFilter != null)
		{
			_collectionView.Filter = MyFilter;
		}
		if (_collectionView.CanGroup)
		{
			ObservableCollection<GroupDescription> master2 = (IsGroupingSet ? MyGroupDescriptions : _collectionView.GroupDescriptions);
			ObservableCollection<GroupDescription> clone2 = (IsGroupingSet ? _collectionView.GroupDescriptions : MyGroupDescriptions);
			using (GroupDescriptionsMonitor.Enter())
			{
				CloneList(clone2, master2);
			}
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			if (MyIsLiveSorting.HasValue && collectionViewLiveShaping.CanChangeLiveSorting)
			{
				collectionViewLiveShaping.IsLiveSorting = MyIsLiveSorting;
			}
			if (MyIsLiveFiltering.HasValue && collectionViewLiveShaping.CanChangeLiveFiltering)
			{
				collectionViewLiveShaping.IsLiveFiltering = MyIsLiveFiltering;
			}
			if (MyIsLiveGrouping.HasValue && collectionViewLiveShaping.CanChangeLiveGrouping)
			{
				collectionViewLiveShaping.IsLiveGrouping = MyIsLiveGrouping;
			}
		}
	}

	private void HookCollectionView(CollectionView view)
	{
		CollectionChangedEventManager.AddHandler(view, OnViewCollectionChanged);
		CurrentChangingEventManager.AddHandler(view, OnCurrentChanging);
		CurrentChangedEventManager.AddHandler(view, OnCurrentChanged);
		PropertyChangedEventManager.AddHandler(view, OnViewPropertyChanged, string.Empty);
		SortDescriptionCollection sortDescriptions = view.SortDescriptions;
		if (sortDescriptions != null && sortDescriptions != SortDescriptionCollection.Empty)
		{
			CollectionChangedEventManager.AddHandler(sortDescriptions, OnInnerSortDescriptionsChanged);
		}
		ObservableCollection<GroupDescription> groupDescriptions = view.GroupDescriptions;
		if (groupDescriptions != null)
		{
			CollectionChangedEventManager.AddHandler(groupDescriptions, OnInnerGroupDescriptionsChanged);
		}
		if (view is ICollectionViewLiveShaping { LiveSortingProperties: var liveSortingProperties } collectionViewLiveShaping)
		{
			if (liveSortingProperties != null)
			{
				CollectionChangedEventManager.AddHandler(liveSortingProperties, OnInnerLiveSortingChanged);
			}
			ObservableCollection<string> liveFilteringProperties = collectionViewLiveShaping.LiveFilteringProperties;
			if (liveFilteringProperties != null)
			{
				CollectionChangedEventManager.AddHandler(liveFilteringProperties, OnInnerLiveFilteringChanged);
			}
			ObservableCollection<string> liveGroupingProperties = collectionViewLiveShaping.LiveGroupingProperties;
			if (liveGroupingProperties != null)
			{
				CollectionChangedEventManager.AddHandler(liveGroupingProperties, OnInnerLiveGroupingChanged);
			}
		}
	}

	private void UnhookCollectionView(CollectionView view)
	{
		CollectionChangedEventManager.RemoveHandler(view, OnViewCollectionChanged);
		CurrentChangingEventManager.RemoveHandler(view, OnCurrentChanging);
		CurrentChangedEventManager.RemoveHandler(view, OnCurrentChanged);
		PropertyChangedEventManager.RemoveHandler(view, OnViewPropertyChanged, string.Empty);
		SortDescriptionCollection sortDescriptions = view.SortDescriptions;
		if (sortDescriptions != null && sortDescriptions != SortDescriptionCollection.Empty)
		{
			CollectionChangedEventManager.RemoveHandler(sortDescriptions, OnInnerSortDescriptionsChanged);
		}
		ObservableCollection<GroupDescription> groupDescriptions = view.GroupDescriptions;
		if (groupDescriptions != null)
		{
			CollectionChangedEventManager.RemoveHandler(groupDescriptions, OnInnerGroupDescriptionsChanged);
		}
		if (view is ICollectionViewLiveShaping { LiveSortingProperties: var liveSortingProperties } collectionViewLiveShaping)
		{
			if (liveSortingProperties != null)
			{
				CollectionChangedEventManager.RemoveHandler(liveSortingProperties, OnInnerLiveSortingChanged);
			}
			ObservableCollection<string> liveFilteringProperties = collectionViewLiveShaping.LiveFilteringProperties;
			if (liveFilteringProperties != null)
			{
				CollectionChangedEventManager.RemoveHandler(liveFilteringProperties, OnInnerLiveFilteringChanged);
			}
			ObservableCollection<string> liveGroupingProperties = collectionViewLiveShaping.LiveGroupingProperties;
			if (liveGroupingProperties != null)
			{
				CollectionChangedEventManager.RemoveHandler(liveGroupingProperties, OnInnerLiveGroupingChanged);
			}
		}
		if (!(_collectionView is IEditableCollectionView editableCollectionView))
		{
			return;
		}
		if (editableCollectionView.IsAddingNew)
		{
			editableCollectionView.CancelNew();
		}
		if (editableCollectionView.IsEditingItem)
		{
			if (editableCollectionView.CanCancelEdit)
			{
				editableCollectionView.CancelEdit();
			}
			else
			{
				editableCollectionView.CommitEdit();
			}
		}
	}

	private void OnViewCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		InvalidateEnumerableWrapper();
		OnCollectionChanged(e);
	}

	private void OnCurrentChanged(object sender, EventArgs e)
	{
		OnCurrentChanged();
	}

	private void OnCurrentChanging(object sender, CurrentChangingEventArgs e)
	{
		OnCurrentChanging(e);
	}

	private void OnViewPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnPropertyChanged(e);
	}

	/// <summary>Receives events from the centralized event manager.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private void CheckIsUsingInnerView()
	{
		if (IsUsingItemsSource)
		{
			throw new InvalidOperationException(SR.ItemsSourceInUse);
		}
		EnsureInternalView();
		EnsureCollectionView();
		VerifyRefreshNotDeferred();
	}

	private void EndDefer()
	{
		_deferLevel--;
		if (_deferLevel == 0)
		{
			if (_deferInnerRefresh != null)
			{
				IDisposable deferInnerRefresh = _deferInnerRefresh;
				_deferInnerRefresh = null;
				deferInnerRefresh.Dispose();
			}
			else
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}
	}

	private new void VerifyRefreshNotDeferred()
	{
		if (IsRefreshDeferred)
		{
			throw new InvalidOperationException(SR.NoCheckOrChangeWhenDeferred);
		}
	}

	private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (SortDescriptionsMonitor.Busy)
		{
			return;
		}
		if (_collectionView != null && _collectionView.CanSort)
		{
			using (SortDescriptionsMonitor.Enter())
			{
				SynchronizeCollections(e, MySortDescriptions, _collectionView.SortDescriptions);
			}
		}
		IsSortingSet = true;
	}

	private void OnInnerSortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsShapingActive && !SortDescriptionsMonitor.Busy)
		{
			using (SortDescriptionsMonitor.Enter())
			{
				SynchronizeCollections(e, _collectionView.SortDescriptions, MySortDescriptions);
			}
			IsSortingSet = false;
		}
	}

	private void GroupDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (GroupDescriptionsMonitor.Busy)
		{
			return;
		}
		if (_collectionView != null && _collectionView.CanGroup)
		{
			using (GroupDescriptionsMonitor.Enter())
			{
				SynchronizeCollections(e, MyGroupDescriptions, _collectionView.GroupDescriptions);
			}
		}
		IsGroupingSet = true;
	}

	private void OnInnerGroupDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (IsShapingActive && !GroupDescriptionsMonitor.Busy)
		{
			using (GroupDescriptionsMonitor.Enter())
			{
				SynchronizeCollections(e, _collectionView.GroupDescriptions, MyGroupDescriptions);
			}
			IsGroupingSet = false;
		}
	}

	private void LiveSortingChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (LiveSortingMonitor.Busy)
		{
			return;
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			using (LiveSortingMonitor.Enter())
			{
				SynchronizeCollections(e, MyLiveSortingProperties, collectionViewLiveShaping.LiveSortingProperties);
			}
		}
		IsLiveSortingSet = true;
	}

	private void OnInnerLiveSortingChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (!IsShapingActive || LiveSortingMonitor.Busy)
		{
			return;
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			using (LiveSortingMonitor.Enter())
			{
				SynchronizeCollections(e, collectionViewLiveShaping.LiveSortingProperties, MyLiveSortingProperties);
			}
		}
		IsLiveSortingSet = false;
	}

	private void LiveFilteringChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (LiveFilteringMonitor.Busy)
		{
			return;
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			using (LiveFilteringMonitor.Enter())
			{
				SynchronizeCollections(e, MyLiveFilteringProperties, collectionViewLiveShaping.LiveFilteringProperties);
			}
		}
		IsLiveFilteringSet = true;
	}

	private void OnInnerLiveFilteringChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (!IsShapingActive || LiveFilteringMonitor.Busy)
		{
			return;
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			using (LiveFilteringMonitor.Enter())
			{
				SynchronizeCollections(e, collectionViewLiveShaping.LiveFilteringProperties, MyLiveFilteringProperties);
			}
		}
		IsLiveFilteringSet = false;
	}

	private void LiveGroupingChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (LiveGroupingMonitor.Busy)
		{
			return;
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			using (LiveGroupingMonitor.Enter())
			{
				SynchronizeCollections(e, MyLiveGroupingProperties, collectionViewLiveShaping.LiveGroupingProperties);
			}
		}
		IsLiveGroupingSet = true;
	}

	private void OnInnerLiveGroupingChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (!IsShapingActive || LiveGroupingMonitor.Busy)
		{
			return;
		}
		if (_collectionView is ICollectionViewLiveShaping collectionViewLiveShaping)
		{
			using (LiveGroupingMonitor.Enter())
			{
				SynchronizeCollections(e, collectionViewLiveShaping.LiveGroupingProperties, MyLiveGroupingProperties);
			}
		}
		IsLiveGroupingSet = false;
	}

	private void SynchronizeCollections<T>(NotifyCollectionChangedEventArgs e, Collection<T> origin, Collection<T> clone)
	{
		if (clone == null)
		{
			return;
		}
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (clone.Count + e.NewItems.Count == origin.Count)
			{
				for (int m = 0; m < e.NewItems.Count; m++)
				{
					clone.Insert(e.NewStartingIndex + m, (T)e.NewItems[m]);
				}
				break;
			}
			goto case NotifyCollectionChangedAction.Reset;
		case NotifyCollectionChangedAction.Remove:
			if (clone.Count - e.OldItems.Count == origin.Count)
			{
				for (int k = 0; k < e.OldItems.Count; k++)
				{
					clone.RemoveAt(e.OldStartingIndex);
				}
				break;
			}
			goto case NotifyCollectionChangedAction.Reset;
		case NotifyCollectionChangedAction.Replace:
			if (clone.Count == origin.Count)
			{
				for (int l = 0; l < e.OldItems.Count; l++)
				{
					clone[e.OldStartingIndex + l] = (T)e.NewItems[l];
				}
				break;
			}
			goto case NotifyCollectionChangedAction.Reset;
		case NotifyCollectionChangedAction.Move:
			if (clone.Count == origin.Count)
			{
				if (e.NewItems.Count == 1)
				{
					clone.RemoveAt(e.OldStartingIndex);
					clone.Insert(e.NewStartingIndex, (T)e.NewItems[0]);
					break;
				}
				for (int i = 0; i < e.OldItems.Count; i++)
				{
					clone.RemoveAt(e.OldStartingIndex);
				}
				for (int j = 0; j < e.NewItems.Count; j++)
				{
					clone.Insert(e.NewStartingIndex + j, (T)e.NewItems[j]);
				}
				break;
			}
			goto case NotifyCollectionChangedAction.Reset;
		case NotifyCollectionChangedAction.Reset:
			CloneList(clone, origin);
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		}
	}

	private void CloneList(IList clone, IList master)
	{
		if (clone != null && master != null)
		{
			if (clone.Count > 0)
			{
				clone.Clear();
			}
			int i = 0;
			for (int count = master.Count; i < count; i++)
			{
				clone.Add(master[i]);
			}
		}
	}

	private void EnsureShapingStorage()
	{
		if (!IsShapingActive)
		{
			_shapingStorage = new ShapingStorage();
		}
	}
}
