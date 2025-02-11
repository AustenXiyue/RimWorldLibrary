using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MS.Internal.Data;

namespace MS.Internal.Controls;

internal sealed class InnerItemCollectionView : CollectionView, IList, ICollection, IEnumerable
{
	private SortDescriptionCollection _sort;

	private ArrayList _viewList;

	private ArrayList _rawList;

	private ItemCollection _itemCollection;

	private bool _isModified;

	private bool _currentElementWasRemoved;

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

	public override bool CanSort => true;

	public object this[int index]
	{
		get
		{
			return GetItemAt(index);
		}
		set
		{
			DependencyObject dependencyObject = AssertPristineModelChild(value);
			_ = CurrentPosition;
			object obj = _viewList[index];
			_viewList[index] = value;
			int num = -1;
			if (IsCachedMode)
			{
				num = _rawList.IndexOf(obj);
				_rawList[num] = value;
			}
			bool flag = true;
			if (dependencyObject != null)
			{
				flag = false;
				try
				{
					SetModelParent(value);
					flag = true;
				}
				finally
				{
					if (!flag)
					{
						_viewList[index] = obj;
						if (num > 0)
						{
							_rawList[num] = obj;
						}
					}
					else
					{
						ClearModelParent(obj);
					}
				}
			}
			if (flag)
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, obj, index));
				SetIsModified();
			}
		}
	}

	public bool IsReadOnly => false;

	public bool IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => _rawList.SyncRoot;

	public override IEnumerable SourceCollection => this;

	public override int Count => ViewCount;

	public override bool IsEmpty => ViewCount == 0;

	public override bool NeedsRefresh
	{
		get
		{
			if (!base.NeedsRefresh)
			{
				return _isModified;
			}
			return true;
		}
	}

	internal ItemCollection ItemCollection => _itemCollection;

	internal IEnumerator LogicalChildren => _rawList.GetEnumerator();

	internal int RawCount => _rawList.Count;

	private int ViewCount => _viewList.Count;

	private bool IsCachedMode => _viewList != _rawList;

	private FrameworkElement ModelParentFE => ItemCollection.ModelParentFE;

	private bool IsCurrentInView
	{
		get
		{
			if (0 <= CurrentPosition)
			{
				return CurrentPosition < ViewCount;
			}
			return false;
		}
	}

	public InnerItemCollectionView(int capacity, ItemCollection itemCollection)
		: base(EmptyEnumerable.Instance, shouldProcessCollectionChanged: false)
	{
		_rawList = (_viewList = new ArrayList(capacity));
		_itemCollection = itemCollection;
	}

	public override bool Contains(object item)
	{
		return _viewList.Contains(item);
	}

	public int Add(object item)
	{
		DependencyObject dependencyObject = AssertPristineModelChild(item);
		int num = _viewList.Add(item);
		int num2 = -1;
		if (IsCachedMode)
		{
			num2 = _rawList.Add(item);
		}
		bool flag = true;
		if (dependencyObject != null)
		{
			flag = false;
			try
			{
				SetModelParent(item);
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					_viewList.RemoveAt(num);
					if (num2 >= 0)
					{
						_rawList.RemoveAt(num2);
					}
					ClearModelParent(item);
					num = -1;
				}
			}
		}
		if (!flag)
		{
			return -1;
		}
		AdjustCurrencyForAdd(num);
		SetIsModified();
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, num));
		return num;
	}

	public void Clear()
	{
		try
		{
			for (int num = _rawList.Count - 1; num >= 0; num--)
			{
				ClearModelParent(_rawList[num]);
			}
		}
		finally
		{
			_rawList.Clear();
			RefreshOrDefer();
		}
	}

	public void Insert(int index, object item)
	{
		DependencyObject dependencyObject = AssertPristineModelChild(item);
		_viewList.Insert(index, item);
		int num = -1;
		if (IsCachedMode)
		{
			num = _rawList.Add(item);
		}
		bool flag = true;
		if (dependencyObject != null)
		{
			flag = false;
			try
			{
				SetModelParent(item);
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					_viewList.RemoveAt(index);
					if (num >= 0)
					{
						_rawList.RemoveAt(num);
					}
					ClearModelParent(item);
				}
			}
		}
		if (flag)
		{
			AdjustCurrencyForAdd(index);
			SetIsModified();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}
	}

	public void Remove(object item)
	{
		int index = _viewList.IndexOf(item);
		int indexR = -1;
		if (IsCachedMode)
		{
			indexR = _rawList.IndexOf(item);
		}
		_RemoveAt(index, indexR, item);
	}

	public void RemoveAt(int index)
	{
		if (0 <= index && index < ViewCount)
		{
			object obj = this[index];
			int indexR = -1;
			if (IsCachedMode)
			{
				indexR = _rawList.IndexOf(obj);
			}
			_RemoveAt(index, indexR, obj);
			return;
		}
		throw new ArgumentOutOfRangeException("index", SR.ItemCollectionRemoveArgumentOutOfRange);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		_viewList.CopyTo(array, index);
	}

	public override int IndexOf(object item)
	{
		return _viewList.IndexOf(item);
	}

	public override object GetItemAt(int index)
	{
		return _viewList[index];
	}

	public override bool MoveCurrentTo(object item)
	{
		if (ItemsControl.EqualsEx(CurrentItem, item) && (item != null || IsCurrentInView))
		{
			return IsCurrentInView;
		}
		return MoveCurrentToPosition(IndexOf(item));
	}

	public override bool MoveCurrentToPosition(int position)
	{
		if (position < -1 || position > ViewCount)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		if (position != CurrentPosition && OKToChangeCurrent())
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

	protected override void RefreshOverride()
	{
		_ = IsEmpty;
		object currentItem = CurrentItem;
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		int currentPosition = CurrentPosition;
		OnCurrentChanging();
		if (SortDescriptions.Count > 0 || Filter != null)
		{
			if (Filter == null)
			{
				_viewList = new ArrayList(_rawList);
			}
			else
			{
				_viewList = new ArrayList();
				for (int i = 0; i < _rawList.Count; i++)
				{
					if (Filter(_rawList[i]))
					{
						_viewList.Add(_rawList[i]);
					}
				}
			}
			if (_sort != null && _sort.Count > 0 && ViewCount > 0)
			{
				SortFieldComparer.SortHelper(_viewList, new SortFieldComparer(_sort, Culture));
			}
		}
		else
		{
			_viewList = _rawList;
		}
		if (IsEmpty || isCurrentBeforeFirst)
		{
			_MoveCurrentToPosition(-1);
		}
		else if (isCurrentAfterLast)
		{
			_MoveCurrentToPosition(ViewCount);
		}
		else if (currentItem != null)
		{
			int num = _viewList.IndexOf(currentItem);
			if (num < 0)
			{
				num = 0;
			}
			_MoveCurrentToPosition(num);
		}
		ClearIsModified();
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
		if (currentPosition != CurrentPosition)
		{
			OnPropertyChanged("CurrentPosition");
		}
		if (currentItem != CurrentItem)
		{
			OnPropertyChanged("CurrentItem");
		}
	}

	protected override IEnumerator GetEnumerator()
	{
		return _viewList.GetEnumerator();
	}

	private void SetIsModified()
	{
		if (IsCachedMode)
		{
			_isModified = true;
		}
	}

	private void ClearIsModified()
	{
		_isModified = false;
	}

	private void _RemoveAt(int index, int indexR, object item)
	{
		if (index >= 0)
		{
			_viewList.RemoveAt(index);
		}
		if (indexR >= 0)
		{
			_rawList.RemoveAt(indexR);
		}
		try
		{
			ClearModelParent(item);
		}
		finally
		{
			if (index >= 0)
			{
				AdjustCurrencyForRemove(index);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
				if (_currentElementWasRemoved)
				{
					MoveCurrencyOffDeletedElement();
				}
			}
		}
	}

	private DependencyObject AssertPristineModelChild(object item)
	{
		if (!(item is DependencyObject dependencyObject))
		{
			return null;
		}
		if (LogicalTreeHelper.GetParent(dependencyObject) != null)
		{
			throw new InvalidOperationException(SR.ReparentModelChildIllegal);
		}
		return dependencyObject;
	}

	private void SetModelParent(object item)
	{
		if (ModelParentFE != null && item is DependencyObject)
		{
			LogicalTreeHelper.AddLogicalChild(ModelParentFE, null, item);
		}
	}

	private void ClearModelParent(object item)
	{
		if (ModelParentFE != null && item is DependencyObject)
		{
			LogicalTreeHelper.RemoveLogicalChild(ModelParentFE, null, item);
		}
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
		RefreshOrDefer();
	}

	private void _MoveCurrentToPosition(int position)
	{
		if (position < 0)
		{
			SetCurrent(null, -1);
		}
		else if (position >= ViewCount)
		{
			SetCurrent(null, ViewCount);
		}
		else
		{
			SetCurrent(_viewList[position], position);
		}
	}

	private void AdjustCurrencyForAdd(int index)
	{
		if (index < 0)
		{
			return;
		}
		if (ViewCount == 1)
		{
			SetCurrent(null, -1);
		}
		else if (index <= CurrentPosition)
		{
			int num = CurrentPosition + 1;
			if (num < ViewCount)
			{
				SetCurrent(_viewList[num], num);
			}
			else
			{
				SetCurrent(null, ViewCount);
			}
		}
	}

	private void AdjustCurrencyForRemove(int index)
	{
		if (index >= 0)
		{
			if (index < CurrentPosition)
			{
				int num = CurrentPosition - 1;
				SetCurrent(_viewList[num], num);
			}
			else if (index == CurrentPosition)
			{
				_currentElementWasRemoved = true;
			}
		}
	}

	private void MoveCurrencyOffDeletedElement()
	{
		int num = ViewCount - 1;
		int position = ((CurrentPosition < num) ? CurrentPosition : num);
		_currentElementWasRemoved = false;
		OnCurrentChanging();
		_MoveCurrentToPosition(position);
		OnCurrentChanged();
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
