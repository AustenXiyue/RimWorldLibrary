using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;
using MS.Internal.Hashing.PresentationFramework;

namespace System.Windows.Data;

/// <summary>Represents a view for grouping, sorting, filtering, and navigating a data collection.</summary>
public class CollectionView : DispatcherObject, ICollectionView, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
{
	internal class PlaceholderAwareEnumerator : IEnumerator
	{
		private enum Position
		{
			BeforePlaceholder,
			OnPlaceholder,
			OnNewItem,
			AfterPlaceholder
		}

		private CollectionView _collectionView;

		private IEnumerator _baseEnumerator;

		private NewItemPlaceholderPosition _placeholderPosition;

		private Position _position;

		private object _newItem;

		private int _timestamp;

		public object Current
		{
			get
			{
				if (_position != Position.OnPlaceholder)
				{
					if (_position != Position.OnNewItem)
					{
						return _baseEnumerator.Current;
					}
					return _newItem;
				}
				return NewItemPlaceholder;
			}
		}

		public PlaceholderAwareEnumerator(CollectionView collectionView, IEnumerator baseEnumerator, NewItemPlaceholderPosition placeholderPosition, object newItem)
		{
			_collectionView = collectionView;
			_timestamp = collectionView.Timestamp;
			_baseEnumerator = baseEnumerator;
			_placeholderPosition = placeholderPosition;
			_newItem = newItem;
		}

		public bool MoveNext()
		{
			if (_timestamp != _collectionView.Timestamp)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			switch (_position)
			{
			case Position.BeforePlaceholder:
				if (_placeholderPosition == NewItemPlaceholderPosition.AtBeginning)
				{
					_position = Position.OnPlaceholder;
				}
				else if (!_baseEnumerator.MoveNext() || (_newItem != NoNewItem && _baseEnumerator.Current == _newItem && !_baseEnumerator.MoveNext()))
				{
					if (_newItem != NoNewItem)
					{
						_position = Position.OnNewItem;
					}
					else
					{
						if (_placeholderPosition == NewItemPlaceholderPosition.None)
						{
							return false;
						}
						_position = Position.OnPlaceholder;
					}
				}
				return true;
			case Position.OnPlaceholder:
				if (_newItem != NoNewItem && _placeholderPosition == NewItemPlaceholderPosition.AtBeginning)
				{
					_position = Position.OnNewItem;
					return true;
				}
				break;
			case Position.OnNewItem:
				if (_placeholderPosition == NewItemPlaceholderPosition.AtEnd)
				{
					_position = Position.OnPlaceholder;
					return true;
				}
				break;
			}
			_position = Position.AfterPlaceholder;
			if (_baseEnumerator.MoveNext())
			{
				if (_newItem != NoNewItem && _baseEnumerator.Current == _newItem)
				{
					return _baseEnumerator.MoveNext();
				}
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_position = Position.BeforePlaceholder;
			_baseEnumerator.Reset();
		}
	}

	private class DeferHelper : IDisposable
	{
		private CollectionView _collectionView;

		public DeferHelper(CollectionView collectionView)
		{
			_collectionView = collectionView;
		}

		public void Dispose()
		{
			if (_collectionView != null)
			{
				_collectionView.EndDefer();
				_collectionView = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	private class SimpleMonitor : IDisposable
	{
		private bool _entered;

		public bool Busy => _entered;

		public bool Enter()
		{
			if (_entered)
			{
				return false;
			}
			_entered = true;
			return true;
		}

		public void Dispose()
		{
			_entered = false;
			GC.SuppressFinalize(this);
		}
	}

	[Flags]
	private enum CollectionViewFlags
	{
		UpdatedOutsideDispatcher = 2,
		ShouldProcessCollectionChanged = 4,
		IsCurrentBeforeFirst = 8,
		IsCurrentAfterLast = 0x10,
		IsDynamic = 0x20,
		IsDataInGroupOrder = 0x40,
		NeedsRefresh = 0x80,
		AllowsCrossThreadChanges = 0x100,
		CachedIsEmpty = 0x200
	}

	private ArrayList _changeLog = new ArrayList();

	private ArrayList _tempChangeLog = EmptyArrayList;

	private DataBindOperation _databindOperation;

	private object _vmData;

	private IEnumerable _sourceCollection;

	private CultureInfo _culture;

	private SimpleMonitor _currentChangedMonitor = new SimpleMonitor();

	private int _deferLevel;

	private IndexedEnumerable _enumerableWrapper;

	private Predicate<object> _filter;

	private object _currentItem;

	private int _currentPosition;

	private CollectionViewFlags _flags = CollectionViewFlags.ShouldProcessCollectionChanged | CollectionViewFlags.NeedsRefresh;

	private bool _currentElementWasRemovedOrReplaced;

	private static object _newItemPlaceholder = new NamedObject("NewItemPlaceholder");

	private object _syncObject = new object();

	private DataBindEngine _engine;

	private int _timestamp;

	private static readonly ArrayList EmptyArrayList = new ArrayList();

	private static readonly string IEnumerableT = typeof(IEnumerable<>).Name;

	internal static readonly object NoNewItem = new NamedObject("NoNewItem");

	private static readonly CurrentChangingEventArgs uncancelableCurrentChangingEventArgs = new CurrentChangingEventArgs(isCancelable: false);

	internal const string CountPropertyName = "Count";

	internal const string IsEmptyPropertyName = "IsEmpty";

	internal const string CulturePropertyName = "Culture";

	internal const string CurrentPositionPropertyName = "CurrentPosition";

	internal const string CurrentItemPropertyName = "CurrentItem";

	internal const string IsCurrentBeforeFirstPropertyName = "IsCurrentBeforeFirst";

	internal const string IsCurrentAfterLastPropertyName = "IsCurrentAfterLast";

	/// <summary>Gets or sets the culture information to use during sorting.</summary>
	/// <returns>The culture information to use during sorting.</returns>
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public virtual CultureInfo Culture
	{
		get
		{
			return _culture;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_culture != value)
			{
				_culture = value;
				OnPropertyChanged("Culture");
			}
		}
	}

	/// <summary>Returns the underlying unfiltered collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerable" /> object that is the underlying collection.</returns>
	public virtual IEnumerable SourceCollection => _sourceCollection;

	/// <summary>Gets or sets a method used to determine if an item is suitable for inclusion in the view.</summary>
	/// <returns>A delegate that represents the method used to determine if an item is suitable for inclusion in the view.</returns>
	/// <exception cref="T:System.NotSupportedException">The current implementation does not support filtering. </exception>
	public virtual Predicate<object> Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			if (!CanFilter)
			{
				throw new NotSupportedException();
			}
			_filter = value;
			RefreshOrDefer();
		}
	}

	/// <summary>Gets a value that indicates whether the view supports filtering.</summary>
	/// <returns>true if the view supports filtering; otherwise, false. The default is true.</returns>
	public virtual bool CanFilter => true;

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.SortDescription" /> structures that describes how the items in the collection are sorted in the view.</summary>
	/// <returns>An empty <see cref="T:System.ComponentModel.SortDescriptionCollection" /> in all cases.</returns>
	public virtual SortDescriptionCollection SortDescriptions => SortDescriptionCollection.Empty;

	/// <summary>Gets a value that indicates whether the view supports sorting.</summary>
	/// <returns>false in all cases.</returns>
	public virtual bool CanSort => false;

	/// <summary>Gets a value that indicates whether the view supports grouping.</summary>
	/// <returns>false in all cases.</returns>
	public virtual bool CanGroup => false;

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describes how the items in the collection are grouped in the view.</summary>
	/// <returns>null in all cases.</returns>
	public virtual ObservableCollection<GroupDescription> GroupDescriptions => null;

	/// <summary>Gets a collection of the top-level groups that is constructed based on the <see cref="P:System.Windows.Data.CollectionView.GroupDescriptions" /> property.</summary>
	/// <returns>null in all cases.</returns>
	public virtual ReadOnlyObservableCollection<object> Groups => null;

	/// <summary>Gets the current item in the view.</summary>
	/// <returns>The current item of the view. By default, the first item of the collection starts as the current item.</returns>
	public virtual object CurrentItem
	{
		get
		{
			VerifyRefreshNotDeferred();
			return _currentItem;
		}
	}

	/// <summary>Gets the ordinal position of the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> within the (optionally sorted and filtered) view.</summary>
	/// <returns>The ordinal position of the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> within the (optionally sorted and filtered) view.</returns>
	public virtual int CurrentPosition
	{
		get
		{
			VerifyRefreshNotDeferred();
			return _currentPosition;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> of the view is beyond the end of the collection.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> of the view is beyond the end of the collection; otherwise, false.</returns>
	public virtual bool IsCurrentAfterLast
	{
		get
		{
			VerifyRefreshNotDeferred();
			return CheckFlag(CollectionViewFlags.IsCurrentAfterLast);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> of the view is before the beginning of the collection.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> of the view is before the beginning of the collection; otherwise, false.</returns>
	public virtual bool IsCurrentBeforeFirst
	{
		get
		{
			VerifyRefreshNotDeferred();
			return CheckFlag(CollectionViewFlags.IsCurrentBeforeFirst);
		}
	}

	/// <summary>Gets the number of records in the view.</summary>
	/// <returns>The number of records in the view, or –1 if the number of records is unknown.</returns>
	public virtual int Count
	{
		get
		{
			VerifyRefreshNotDeferred();
			return EnumerableWrapper.Count;
		}
	}

	/// <summary>Gets a value that indicates whether the resulting (filtered) view is empty.</summary>
	/// <returns>true if the resulting view is empty; otherwise, false.</returns>
	public virtual bool IsEmpty => EnumerableWrapper.IsEmpty;

	/// <summary>Returns an object that you can use to compare items in the view.</summary>
	/// <returns>An <see cref="T:System.Collections.IComparer" /> object that you can use to compare items in the view.</returns>
	public virtual IComparer Comparer => this as IComparer;

	/// <summary>Gets a value that indicates whether the view needs to be refreshed.</summary>
	/// <returns>true if the view needs to be refreshed; otherwise, false.</returns>
	public virtual bool NeedsRefresh => CheckFlag(CollectionViewFlags.NeedsRefresh);

	/// <summary>Gets a value that indicates whether any object is subscribing to the events of this <see cref="T:System.Windows.Data.CollectionView" />.</summary>
	/// <returns>true if any object is subscribing to the events of this <see cref="T:System.Windows.Data.CollectionView" />; otherwise, false.</returns>
	public virtual bool IsInUse
	{
		get
		{
			if (this.CollectionChanged == null && this.PropertyChanged == null && this.CurrentChanged == null)
			{
				return this.CurrentChanging != null;
			}
			return true;
		}
	}

	/// <summary>Gets the object that is in the collection to represent a new item.</summary>
	/// <returns>The object that is in the collection to represent a new item.</returns>
	public static object NewItemPlaceholder => _newItemPlaceholder;

	/// <summary>Gets a value that indicates whether the underlying collection provides change notifications.</summary>
	/// <returns>true if the underlying collection provides change notifications; otherwise, false.</returns>
	protected bool IsDynamic => CheckFlag(CollectionViewFlags.IsDynamic);

	/// <summary>Gets a value that indicates whether a thread other than the one that created the <see cref="T:System.Windows.Data.CollectionView" /> can change the <see cref="P:System.Windows.Data.CollectionView.SourceCollection" />. </summary>
	/// <returns>true if a thread other than the one that created the <see cref="T:System.Windows.Data.CollectionView" /> can change the <see cref="P:System.Windows.Data.CollectionView.SourceCollection" />; otherwise, false.</returns>
	protected bool AllowsCrossThreadChanges => CheckFlag(CollectionViewFlags.AllowsCrossThreadChanges);

	/// <summary>Gets a value that indicates whether it has been necessary to update the change log because a <see cref="E:System.Windows.Data.CollectionView.CollectionChanged" /> notification has been received on a different thread without first entering the user interface (UI) thread dispatcher.</summary>
	/// <returns>true if it has been necessary to update the change log because a <see cref="E:System.Windows.Data.CollectionView.CollectionChanged" /> notification has been received on a different thread without first entering the user interface (UI) thread dispatcher; otherwise, false.</returns>
	protected bool UpdatedOutsideDispatcher => AllowsCrossThreadChanges;

	/// <summary>Gets a value that indicates whether there is an outstanding <see cref="M:System.Windows.Data.CollectionView.DeferRefresh" /> in use.</summary>
	/// <returns>true if there is an outstanding <see cref="M:System.Windows.Data.CollectionView.DeferRefresh" /> in use; otherwise, false.</returns>
	protected bool IsRefreshDeferred => _deferLevel > 0;

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is at the <see cref="P:System.Windows.Data.CollectionView.CurrentPosition" />.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is in the view and at the <see cref="P:System.Windows.Data.CollectionView.CurrentPosition" />; otherwise, false.</returns>
	protected bool IsCurrentInSync
	{
		get
		{
			if (IsCurrentInView)
			{
				return GetItemAt(CurrentPosition) == CurrentItem;
			}
			return CurrentItem == null;
		}
	}

	internal object SyncRoot => _syncObject;

	internal int Timestamp => _timestamp;

	private bool IsCurrentInView
	{
		get
		{
			VerifyRefreshNotDeferred();
			if (0 <= CurrentPosition)
			{
				return CurrentPosition < Count;
			}
			return false;
		}
	}

	private IndexedEnumerable EnumerableWrapper
	{
		get
		{
			if (_enumerableWrapper == null)
			{
				IndexedEnumerable value = new IndexedEnumerable(SourceCollection, PassesFilter);
				Interlocked.CompareExchange(ref _enumerableWrapper, value, null);
			}
			return _enumerableWrapper;
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is changing.</summary>
	public virtual event CurrentChangingEventHandler CurrentChanging;

	/// <summary>Occurs after the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> has changed.</summary>
	public virtual event EventHandler CurrentChanged;

	/// <summary>Occurs when the view has changed.</summary>
	protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>Occurs when the view has changed.</summary>
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

	/// <summary>Occurs when a property value has changed.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.CollectionView" /> class that represents a view of the specified collection. </summary>
	/// <param name="collection">The underlying collection.</param>
	public CollectionView(IEnumerable collection)
		: this(collection, 0)
	{
	}

	internal CollectionView(IEnumerable collection, int moveToFirst)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (GetType() == typeof(CollectionView) && TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.CollectionViewIsUnsupported);
		}
		_engine = DataBindEngine.CurrentDataBindEngine;
		if (!_engine.IsShutDown)
		{
			SetFlag(CollectionViewFlags.AllowsCrossThreadChanges, _engine.ViewManager.GetSynchronizationInfo(collection).IsSynchronized);
		}
		else
		{
			moveToFirst = -1;
		}
		_sourceCollection = collection;
		if (collection is INotifyCollectionChanged notifyCollectionChanged)
		{
			if (!(this is BindingListCollectionView) || collection is IBindingList { SupportsChangeNotification: false })
			{
				notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
			}
			SetFlag(CollectionViewFlags.IsDynamic, value: true);
		}
		object currentItem = null;
		int currentPosition = -1;
		if (moveToFirst >= 0)
		{
			BindingOperations.AccessCollection(collection, delegate
			{
				IEnumerator enumerator = collection.GetEnumerator();
				if (enumerator.MoveNext())
				{
					currentItem = enumerator.Current;
					currentPosition = 0;
				}
				if (enumerator is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}, writeAccess: false);
		}
		_currentItem = currentItem;
		_currentPosition = currentPosition;
		SetFlag(CollectionViewFlags.IsCurrentBeforeFirst, _currentPosition < 0);
		SetFlag(CollectionViewFlags.IsCurrentAfterLast, _currentPosition < 0);
		SetFlag(CollectionViewFlags.CachedIsEmpty, _currentPosition < 0);
	}

	internal CollectionView(IEnumerable collection, bool shouldProcessCollectionChanged)
		: this(collection)
	{
		SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, shouldProcessCollectionChanged);
	}

	/// <summary>Returns a value that indicates whether the specified item belongs to the view.</summary>
	/// <returns>true if the item belongs to the view; otherwise, false.</returns>
	/// <param name="item">The object to check.</param>
	public virtual bool Contains(object item)
	{
		VerifyRefreshNotDeferred();
		return IndexOf(item) >= 0;
	}

	/// <summary>Re-creates the view.</summary>
	public virtual void Refresh()
	{
		if (this is IEditableCollectionView editableCollectionView && (editableCollectionView.IsAddingNew || editableCollectionView.IsEditingItem))
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "Refresh"));
		}
		RefreshInternal();
	}

	internal void RefreshInternal()
	{
		if (AllowsCrossThreadChanges)
		{
			VerifyAccess();
		}
		RefreshOverride();
		SetFlag(CollectionViewFlags.NeedsRefresh, value: false);
	}

	/// <summary>Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that you can use to dispose of the calling object.</returns>
	public virtual IDisposable DeferRefresh()
	{
		if (AllowsCrossThreadChanges)
		{
			VerifyAccess();
		}
		if (this is IEditableCollectionView editableCollectionView && (editableCollectionView.IsAddingNew || editableCollectionView.IsEditingItem))
		{
			throw new InvalidOperationException(SR.Format(SR.MemberNotAllowedDuringAddOrEdit, "DeferRefresh"));
		}
		_deferLevel++;
		return new DeferHelper(this);
	}

	/// <summary>Sets the first item in the view as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public virtual bool MoveCurrentToFirst()
	{
		VerifyRefreshNotDeferred();
		int position = 0;
		if (this is IEditableCollectionView { NewItemPlaceholderPosition: NewItemPlaceholderPosition.AtBeginning })
		{
			position = 1;
		}
		return MoveCurrentToPosition(position);
	}

	/// <summary>Sets the last item in the view as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public virtual bool MoveCurrentToLast()
	{
		VerifyRefreshNotDeferred();
		int num = Count - 1;
		if (this is IEditableCollectionView { NewItemPlaceholderPosition: NewItemPlaceholderPosition.AtEnd })
		{
			num--;
		}
		return MoveCurrentToPosition(num);
	}

	/// <summary>Sets the item after the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> in the view as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public virtual bool MoveCurrentToNext()
	{
		VerifyRefreshNotDeferred();
		int num = CurrentPosition + 1;
		int count = Count;
		IEditableCollectionView editableCollectionView = this as IEditableCollectionView;
		if (editableCollectionView != null && num == 0 && editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
		{
			num = 1;
		}
		if (editableCollectionView != null && num == count - 1 && editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)
		{
			num = count;
		}
		if (num <= count)
		{
			return MoveCurrentToPosition(num);
		}
		return false;
	}

	/// <summary>Sets the item before the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> in the view as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	public virtual bool MoveCurrentToPrevious()
	{
		VerifyRefreshNotDeferred();
		int num = CurrentPosition - 1;
		int count = Count;
		IEditableCollectionView editableCollectionView = this as IEditableCollectionView;
		if (editableCollectionView != null && num == count - 1 && editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)
		{
			num = count - 2;
		}
		if (editableCollectionView != null && num == 0 && editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
		{
			num = -1;
		}
		if (num >= -1)
		{
			return MoveCurrentToPosition(num);
		}
		return false;
	}

	/// <summary>Sets the specified item to be the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> in the view.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is within the view; otherwise, false.</returns>
	/// <param name="item">The item to set as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</param>
	public virtual bool MoveCurrentTo(object item)
	{
		VerifyRefreshNotDeferred();
		if ((ItemsControl.EqualsEx(CurrentItem, item) || ItemsControl.EqualsEx(NewItemPlaceholder, item)) && (item != null || IsCurrentInView))
		{
			return IsCurrentInView;
		}
		int position = -1;
		if ((this is IEditableCollectionView { IsAddingNew: not false } editableCollectionView && ItemsControl.EqualsEx(item, editableCollectionView.CurrentAddItem)) || PassesFilter(item))
		{
			position = IndexOf(item);
		}
		return MoveCurrentToPosition(position);
	}

	/// <summary>Sets the item at the specified index to be the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> in the view.</summary>
	/// <returns>true if the resulting <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	/// <param name="position">The index to set the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> to.</param>
	public virtual bool MoveCurrentToPosition(int position)
	{
		VerifyRefreshNotDeferred();
		if (position < -1 || position > Count)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		if (this is IEditableCollectionView editableCollectionView && ((position == 0 && editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning) || (position == Count - 1 && editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)))
		{
			return IsCurrentInView;
		}
		if ((position != CurrentPosition || !IsCurrentInSync) && OKToChangeCurrent())
		{
			bool isCurrentAfterLast = IsCurrentAfterLast;
			bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
			_MoveCurrentToPosition(position);
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
		return IsCurrentInView;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> object that you can use to enumerate the items in the view.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that you can use to enumerate the items in the view.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Returns a value that indicates whether the specified item in the underlying collection belongs to the view.</summary>
	/// <returns>true if the specified item belongs to the view or if there is not filter set on the collection view; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	public virtual bool PassesFilter(object item)
	{
		if (CanFilter && Filter != null)
		{
			return Filter(item);
		}
		return true;
	}

	/// <summary>Returns the index at which the specified item is located.</summary>
	/// <returns>The index at which the specified item is located, or –1 if the item is unknown.</returns>
	/// <param name="item">The item to locate.</param>
	public virtual int IndexOf(object item)
	{
		VerifyRefreshNotDeferred();
		return EnumerableWrapper.IndexOf(item);
	}

	/// <summary>Retrieves the item at the specified zero-based index in the view.</summary>
	/// <returns>The item at the specified zero-based index in the view.</returns>
	/// <param name="index">The zero-based index of the item to retrieve.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0. </exception>
	public virtual object GetItemAt(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return EnumerableWrapper[index];
	}

	/// <summary>Removes the reference to the underlying collection from the <see cref="T:System.Windows.Data.CollectionView" />.</summary>
	public virtual void DetachFromSourceCollection()
	{
		if (_sourceCollection is INotifyCollectionChanged notifyCollectionChanged && (!(this is BindingListCollectionView) || _sourceCollection is IBindingList { SupportsChangeNotification: false }))
		{
			notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
		}
		_sourceCollection = null;
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event using the specified arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	/// <summary>Re-creates the view.</summary>
	protected virtual void RefreshOverride()
	{
		if (SortDescriptions.Count > 0)
		{
			throw new InvalidOperationException(SR.Format(SR.ImplementOtherMembersWithSort, "Refresh()"));
		}
		object currentItem = _currentItem;
		bool flag = CheckFlag(CollectionViewFlags.IsCurrentAfterLast);
		bool flag2 = CheckFlag(CollectionViewFlags.IsCurrentBeforeFirst);
		int currentPosition = _currentPosition;
		OnCurrentChanging();
		InvalidateEnumerableWrapper();
		if (IsEmpty || flag2)
		{
			_MoveCurrentToPosition(-1);
		}
		else if (flag)
		{
			_MoveCurrentToPosition(Count);
		}
		else if (currentItem != null)
		{
			int num = EnumerableWrapper.IndexOf(currentItem);
			if (num < 0)
			{
				num = 0;
			}
			_MoveCurrentToPosition(num);
		}
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		OnCurrentChanged();
		if (IsCurrentAfterLast != flag)
		{
			OnPropertyChanged("IsCurrentAfterLast");
		}
		if (IsCurrentBeforeFirst != flag2)
		{
			OnPropertyChanged("IsCurrentBeforeFirst");
		}
		if (currentPosition != CurrentPosition)
		{
			OnPropertyChanged("CurrentPosition");
		}
		if (currentItem != CurrentItem)
		{
			OnPropertyChanged("CurrentItem");
		}
	}

	/// <summary>Returns an object that you can use to enumerate the items in the view.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that you can use to enumerate the items in the view.</returns>
	protected virtual IEnumerator GetEnumerator()
	{
		VerifyRefreshNotDeferred();
		if (SortDescriptions.Count > 0)
		{
			throw new InvalidOperationException(SR.Format(SR.ImplementOtherMembersWithSort, "GetEnumerator()"));
		}
		return EnumerableWrapper.GetEnumerator();
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.CollectionView.CollectionChanged" /> event. </summary>
	/// <param name="args">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> object to pass to the event handler.</param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		_timestamp++;
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, args);
		}
		if (args.Action != NotifyCollectionChangedAction.Replace && args.Action != NotifyCollectionChangedAction.Move)
		{
			OnPropertyChanged("Count");
		}
		bool isEmpty = IsEmpty;
		if (isEmpty != CheckFlag(CollectionViewFlags.CachedIsEmpty))
		{
			SetFlag(CollectionViewFlags.CachedIsEmpty, isEmpty);
			OnPropertyChanged("IsEmpty");
		}
	}

	/// <summary>Sets the specified item and index as the values of the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> and <see cref="P:System.Windows.Data.CollectionView.CurrentPosition" /> properties.</summary>
	/// <param name="newItem">The item to set as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</param>
	/// <param name="newPosition">The value to set as the <see cref="P:System.Windows.Data.CollectionView.CurrentPosition" /> property value.</param>
	protected void SetCurrent(object newItem, int newPosition)
	{
		int count = ((newItem == null) ? ((!IsEmpty) ? Count : 0) : 0);
		SetCurrent(newItem, newPosition, count);
	}

	/// <summary>Sets the specified item and index as the values of the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" /> and <see cref="P:System.Windows.Data.CollectionView.CurrentPosition" /> properties. This method can be called from a constructor of a derived class.</summary>
	/// <param name="newItem">The item to set as the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</param>
	/// <param name="newPosition">The value to set as the <see cref="P:System.Windows.Data.CollectionView.CurrentPosition" /> property value.</param>
	/// <param name="count">The number of items in the <see cref="T:System.Windows.Data.CollectionView" />. </param>
	protected void SetCurrent(object newItem, int newPosition, int count)
	{
		if (newItem != null)
		{
			SetFlag(CollectionViewFlags.IsCurrentBeforeFirst, value: false);
			SetFlag(CollectionViewFlags.IsCurrentAfterLast, value: false);
		}
		else if (count == 0)
		{
			SetFlag(CollectionViewFlags.IsCurrentBeforeFirst, value: true);
			SetFlag(CollectionViewFlags.IsCurrentAfterLast, value: true);
			newPosition = -1;
		}
		else
		{
			SetFlag(CollectionViewFlags.IsCurrentBeforeFirst, newPosition < 0);
			SetFlag(CollectionViewFlags.IsCurrentAfterLast, newPosition >= count);
		}
		_currentItem = newItem;
		_currentPosition = newPosition;
	}

	/// <summary>Returns a value that indicates whether the view can change which item is the <see cref="P:System.Windows.Data.CollectionView.CurrentItem" />.</summary>
	/// <returns>false if a listener cancels the change; otherwise, true.</returns>
	protected bool OKToChangeCurrent()
	{
		CurrentChangingEventArgs currentChangingEventArgs = new CurrentChangingEventArgs();
		OnCurrentChanging(currentChangingEventArgs);
		return !currentChangingEventArgs.Cancel;
	}

	/// <summary>Raises a <see cref="E:System.Windows.Data.CollectionView.CurrentChanging" /> event that is not cancelable.</summary>
	protected void OnCurrentChanging()
	{
		_currentPosition = -1;
		OnCurrentChanging(uncancelableCurrentChangingEventArgs);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.CollectionView.CurrentChanging" /> event with the specified arguments.</summary>
	/// <param name="args">Information about the event.</param>
	protected virtual void OnCurrentChanging(CurrentChangingEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (_currentChangedMonitor.Busy)
		{
			if (args.IsCancelable)
			{
				args.Cancel = true;
			}
		}
		else if (this.CurrentChanging != null)
		{
			this.CurrentChanging(this, args);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.CollectionView.CurrentChanged" /> event.</summary>
	protected virtual void OnCurrentChanged()
	{
		if (this.CurrentChanged != null && _currentChangedMonitor.Enter())
		{
			using (_currentChangedMonitor)
			{
				this.CurrentChanged(this, EventArgs.Empty);
			}
		}
	}

	/// <summary>When overridden in a derived class, processes a single change on the UI thread.</summary>
	/// <param name="args">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> object to process.</param>
	protected virtual void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		ValidateCollectionChangedEventArgs(args);
		object currentItem = _currentItem;
		bool flag = CheckFlag(CollectionViewFlags.IsCurrentAfterLast);
		bool flag2 = CheckFlag(CollectionViewFlags.IsCurrentBeforeFirst);
		int currentPosition = _currentPosition;
		bool flag3 = false;
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (PassesFilter(args.NewItems[0]))
			{
				flag3 = true;
				AdjustCurrencyForAdd(args.NewStartingIndex);
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (PassesFilter(args.OldItems[0]))
			{
				flag3 = true;
				AdjustCurrencyForRemove(args.OldStartingIndex);
			}
			break;
		case NotifyCollectionChangedAction.Replace:
			if (PassesFilter(args.OldItems[0]) || PassesFilter(args.NewItems[0]))
			{
				flag3 = true;
				AdjustCurrencyForReplace(args.OldStartingIndex);
			}
			break;
		case NotifyCollectionChangedAction.Move:
			if (PassesFilter(args.NewItems[0]))
			{
				flag3 = true;
				AdjustCurrencyForMove(args.OldStartingIndex, args.NewStartingIndex);
			}
			break;
		case NotifyCollectionChangedAction.Reset:
			RefreshOrDefer();
			return;
		}
		if (flag3)
		{
			OnCollectionChanged(args);
		}
		if (_currentElementWasRemovedOrReplaced)
		{
			MoveCurrencyOffDeletedElement();
			_currentElementWasRemovedOrReplaced = false;
		}
		if (IsCurrentAfterLast != flag)
		{
			OnPropertyChanged("IsCurrentAfterLast");
		}
		if (IsCurrentBeforeFirst != flag2)
		{
			OnPropertyChanged("IsCurrentBeforeFirst");
		}
		if (_currentPosition != currentPosition)
		{
			OnPropertyChanged("CurrentPosition");
		}
		if (_currentItem != currentItem)
		{
			OnPropertyChanged("CurrentItem");
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.CollectionView.CollectionChanged" /> event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="args">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> object to pass to the event handler.</param>
	protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (!CheckFlag(CollectionViewFlags.ShouldProcessCollectionChanged))
		{
			return;
		}
		if (!AllowsCrossThreadChanges)
		{
			if (!CheckAccess())
			{
				throw new NotSupportedException(SR.MultiThreadedCollectionChangeNotSupported);
			}
			ProcessCollectionChanged(args);
		}
		else
		{
			PostChange(args);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Data.CollectionView.AllowsCrossThreadChanges" /> property changes.</summary>
	protected virtual void OnAllowsCrossThreadChangesChanged()
	{
	}

	/// <summary>Clears unprocessed changed to the collection.</summary>
	protected void ClearPendingChanges()
	{
		lock (_changeLog.SyncRoot)
		{
			_changeLog.Clear();
			_tempChangeLog.Clear();
		}
	}

	/// <summary>Ensures that all pending changes to the collection have been committed.</summary>
	protected void ProcessPendingChanges()
	{
		lock (_changeLog.SyncRoot)
		{
			ProcessChangeLog(_changeLog, processAll: true);
			_changeLog.Clear();
		}
	}

	/// <summary>Called by the base class to notify the derived class that an <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event has been posted to the message queue.</summary>
	/// <param name="args">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> object that is added to the change log.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="args" /> is null. </exception>
	[Obsolete("Replaced by OnAllowsCrossThreadChangesChanged")]
	protected virtual void OnBeginChangeLogging(NotifyCollectionChangedEventArgs args)
	{
	}

	/// <summary>Clears any pending changes from the change log.</summary>
	[Obsolete("Replaced by ClearPendingChanges")]
	protected void ClearChangeLog()
	{
		ClearPendingChanges();
	}

	/// <summary>Refreshes the view or specifies that the view needs to be refreshed when the defer cycle completes.</summary>
	protected void RefreshOrDefer()
	{
		if (IsRefreshDeferred)
		{
			SetFlag(CollectionViewFlags.NeedsRefresh, value: true);
		}
		else
		{
			RefreshInternal();
		}
	}

	internal void SetAllowsCrossThreadChanges(bool value)
	{
		if (CheckFlag(CollectionViewFlags.AllowsCrossThreadChanges) != value)
		{
			SetFlag(CollectionViewFlags.AllowsCrossThreadChanges, value);
			OnAllowsCrossThreadChangesChanged();
		}
	}

	internal void SetViewManagerData(object value)
	{
		if (_vmData == null)
		{
			_vmData = value;
		}
		else if (!(_vmData is object[] array))
		{
			_vmData = new object[2] { _vmData, value };
		}
		else
		{
			object[] array2 = new object[array.Length + 1];
			array.CopyTo(array2, 0);
			array2[array.Length] = value;
			_vmData = array2;
		}
	}

	internal virtual bool HasReliableHashCodes()
	{
		if (!IsEmpty)
		{
			return HashHelper.HasReliableHashCode(GetItemAt(0));
		}
		return true;
	}

	internal void VerifyRefreshNotDeferred()
	{
		if (AllowsCrossThreadChanges)
		{
			VerifyAccess();
		}
		if (IsRefreshDeferred)
		{
			throw new InvalidOperationException(SR.NoCheckOrChangeWhenDeferred);
		}
	}

	internal void InvalidateEnumerableWrapper()
	{
		Interlocked.Exchange(ref _enumerableWrapper, null)?.Invalidate();
	}

	internal ReadOnlyCollection<ItemPropertyInfo> GetItemProperties()
	{
		IEnumerable sourceCollection = SourceCollection;
		if (sourceCollection == null)
		{
			return null;
		}
		IEnumerable enumerable = null;
		Type itemType;
		object representativeItem;
		if (sourceCollection is ITypedList typedList)
		{
			enumerable = typedList.GetItemProperties(null);
		}
		else if ((itemType = GetItemType(useRepresentativeItem: false)) != null)
		{
			enumerable = TypeDescriptor.GetProperties(itemType);
		}
		else if ((representativeItem = GetRepresentativeItem()) != null)
		{
			enumerable = ((representativeItem is ICustomTypeProvider customTypeProvider) ? ((IList)customTypeProvider.GetCustomType().GetProperties()) : ((IList)TypeDescriptor.GetProperties(representativeItem)));
		}
		if (enumerable == null)
		{
			return null;
		}
		List<ItemPropertyInfo> list = new List<ItemPropertyInfo>();
		foreach (object item in enumerable)
		{
			PropertyInfo propertyInfo;
			if (item is PropertyDescriptor propertyDescriptor)
			{
				list.Add(new ItemPropertyInfo(propertyDescriptor.Name, propertyDescriptor.PropertyType, propertyDescriptor));
			}
			else if ((propertyInfo = item as PropertyInfo) != null)
			{
				list.Add(new ItemPropertyInfo(propertyInfo.Name, propertyInfo.PropertyType, propertyInfo));
			}
		}
		return new ReadOnlyCollection<ItemPropertyInfo>(list);
	}

	internal Type GetItemType(bool useRepresentativeItem)
	{
		Type[] interfaces = SourceCollection.GetType().GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (!(type.Name == IEnumerableT))
			{
				continue;
			}
			Type[] genericArguments = type.GetGenericArguments();
			if (genericArguments.Length == 1)
			{
				Type type2 = genericArguments[0];
				if (typeof(ICustomTypeProvider).IsAssignableFrom(type2))
				{
					break;
				}
				if (!(type2 == typeof(object)))
				{
					return type2;
				}
			}
		}
		if (useRepresentativeItem)
		{
			return ReflectionHelper.GetReflectionType(GetRepresentativeItem());
		}
		return null;
	}

	internal object GetRepresentativeItem()
	{
		if (IsEmpty)
		{
			return null;
		}
		object result = null;
		IEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			if (current != null && current != NewItemPlaceholder)
			{
				result = current;
				break;
			}
		}
		if (enumerator is IDisposable disposable)
		{
			disposable.Dispose();
		}
		return result;
	}

	internal virtual void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, null, sources);
		if (_sourceCollection != null)
		{
			format(level + 1, _sourceCollection, null, sources);
		}
	}

	private void _MoveCurrentToPosition(int position)
	{
		if (position < 0)
		{
			SetFlag(CollectionViewFlags.IsCurrentBeforeFirst, value: true);
			SetCurrent(null, -1);
		}
		else if (position >= Count)
		{
			SetFlag(CollectionViewFlags.IsCurrentAfterLast, value: true);
			SetCurrent(null, Count);
		}
		else
		{
			SetFlag(CollectionViewFlags.IsCurrentBeforeFirst | CollectionViewFlags.IsCurrentAfterLast, value: false);
			SetCurrent(EnumerableWrapper[position], position);
		}
	}

	private void MoveCurrencyOffDeletedElement()
	{
		int num = Count - 1;
		int position = ((_currentPosition < num) ? _currentPosition : num);
		OnCurrentChanging();
		_MoveCurrentToPosition(position);
		OnCurrentChanged();
	}

	private void EndDefer()
	{
		_deferLevel--;
		if (_deferLevel == 0 && CheckFlag(CollectionViewFlags.NeedsRefresh))
		{
			Refresh();
		}
	}

	private void DeferProcessing(ICollection changeLog)
	{
		lock (SyncRoot)
		{
			lock (_changeLog.SyncRoot)
			{
				if (_changeLog == null)
				{
					_changeLog = new ArrayList(changeLog);
				}
				else
				{
					_changeLog.InsertRange(0, changeLog);
				}
				if (_databindOperation != null)
				{
					_engine.ChangeCost(_databindOperation, changeLog.Count);
				}
				else
				{
					_databindOperation = _engine.Marshal(ProcessInvoke, null, changeLog.Count);
				}
			}
		}
	}

	private ICollection ProcessChangeLog(ArrayList changeLog, bool processAll = false)
	{
		int i = 0;
		bool flag = false;
		long ticks = DateTime.Now.Ticks;
		_ = changeLog.Count;
		for (; i < changeLog.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (changeLog[i] is NotifyCollectionChangedEventArgs args)
			{
				ProcessCollectionChanged(args);
			}
			if (!processAll)
			{
				flag = DateTime.Now.Ticks - ticks > 50000;
			}
		}
		if (flag && i < changeLog.Count)
		{
			changeLog.RemoveRange(0, i);
			return changeLog;
		}
		return null;
	}

	private bool CheckFlag(CollectionViewFlags flags)
	{
		return (_flags & flags) != 0;
	}

	private void SetFlag(CollectionViewFlags flags, bool value)
	{
		if (value)
		{
			_flags |= flags;
		}
		else
		{
			_flags &= ~flags;
		}
	}

	private void PostChange(NotifyCollectionChangedEventArgs args)
	{
		lock (SyncRoot)
		{
			lock (_changeLog.SyncRoot)
			{
				if (args.Action == NotifyCollectionChangedAction.Reset)
				{
					_changeLog.Clear();
				}
				if (_changeLog.Count == 0 && CheckAccess())
				{
					ProcessCollectionChanged(args);
					return;
				}
				_changeLog.Add(args);
				if (_databindOperation == null)
				{
					_databindOperation = _engine.Marshal(ProcessInvoke, null, _changeLog.Count);
				}
			}
		}
	}

	private object ProcessInvoke(object arg)
	{
		lock (SyncRoot)
		{
			lock (_changeLog.SyncRoot)
			{
				_databindOperation = null;
				_tempChangeLog = _changeLog;
				_changeLog = new ArrayList();
			}
		}
		ICollection collection = ProcessChangeLog(_tempChangeLog);
		if (collection != null && collection.Count > 0)
		{
			DeferProcessing(collection);
		}
		_tempChangeLog = EmptyArrayList;
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
			if (e.OldStartingIndex < 0)
			{
				throw new InvalidOperationException(SR.RemovedItemNotFound);
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

	private void AdjustCurrencyForAdd(int index)
	{
		if (Count == 1)
		{
			_currentPosition = -1;
		}
		else if (index <= _currentPosition)
		{
			_currentPosition++;
			if (_currentPosition < Count)
			{
				_currentItem = EnumerableWrapper[_currentPosition];
			}
		}
	}

	private void AdjustCurrencyForRemove(int index)
	{
		if (index < _currentPosition)
		{
			_currentPosition--;
		}
		else if (index == _currentPosition)
		{
			_currentElementWasRemovedOrReplaced = true;
		}
	}

	private void AdjustCurrencyForMove(int oldIndex, int newIndex)
	{
		if ((oldIndex >= CurrentPosition || newIndex >= CurrentPosition) && (oldIndex <= CurrentPosition || newIndex <= CurrentPosition))
		{
			if (oldIndex <= CurrentPosition)
			{
				AdjustCurrencyForRemove(oldIndex);
			}
			else if (newIndex <= CurrentPosition)
			{
				AdjustCurrencyForAdd(newIndex);
			}
		}
	}

	private void AdjustCurrencyForReplace(int index)
	{
		if (index == _currentPosition)
		{
			_currentElementWasRemovedOrReplaced = true;
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
