using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace MS.Internal.Data;

internal class LiveShapingList : IList, ICollection, IEnumerable
{
	private class DPFromPath : Dictionary<string, DependencyProperty>
	{
		private List<string> _unusedKeys;

		private int _dpIndex;

		public void BeginReset()
		{
			_unusedKeys = new List<string>(base.Keys);
			_dpIndex = 0;
		}

		public void EndReset()
		{
			foreach (string unusedKey in _unusedKeys)
			{
				Remove(unusedKey);
			}
			_unusedKeys = null;
		}

		public DependencyProperty GetDP(string path)
		{
			if (TryGetValue(path, out var value))
			{
				_unusedKeys.Remove(path);
				return value;
			}
			ICollection<DependencyProperty> values = base.Values;
			while (_dpIndex < s_dpList.Count)
			{
				value = s_dpList[_dpIndex];
				if (!values.Contains(value))
				{
					base[path] = value;
					return value;
				}
				_dpIndex++;
			}
			lock (s_Sync)
			{
				value = DependencyProperty.RegisterAttached(string.Format(TypeConverterHelper.InvariantEnglishUS, "LiveSortingTargetProperty{0}", s_dpList.Count), typeof(object), typeof(LiveShapingList));
				s_dpList.Add(value);
			}
			base[path] = value;
			return value;
		}
	}

	private class ItemEnumerator : IEnumerator
	{
		private IEnumerator<LiveShapingItem> _ie;

		object IEnumerator.Current => _ie.Current.Item;

		public ItemEnumerator(IEnumerator<LiveShapingItem> ie)
		{
			_ie = ie;
		}

		void IEnumerator.Reset()
		{
			_ie.Reset();
		}

		bool IEnumerator.MoveNext()
		{
			return _ie.MoveNext();
		}
	}

	private ICollectionViewLiveShaping _view;

	private DPFromPath _dpFromPath;

	private LivePropertyInfo[] _compInfos;

	private LivePropertyInfo[] _sortInfos;

	private LivePropertyInfo[] _filterInfos;

	private LivePropertyInfo[] _groupInfos;

	private IComparer _comparer;

	private LiveShapingTree _root;

	private LiveShapingTree _filterRoot;

	private List<LiveShapingItem> _sortDirtyItems;

	private List<LiveShapingItem> _filterDirtyItems;

	private List<LiveShapingItem> _groupDirtyItems;

	private bool _isRestoringLiveSorting;

	private bool _isCustomSorting;

	private static List<DependencyProperty> s_dpList = new List<DependencyProperty>();

	private static readonly object s_Sync = new object();

	internal ICollectionViewLiveShaping View => _view;

	internal Dictionary<string, DependencyProperty> ObservedProperties => _dpFromPath;

	internal List<LiveShapingItem> SortDirtyItems => _sortDirtyItems;

	internal List<LiveShapingItem> FilterDirtyItems => _filterDirtyItems;

	internal List<LiveShapingItem> GroupDirtyItems => _groupDirtyItems;

	internal bool IsRestoringLiveSorting => _isRestoringLiveSorting;

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public object this[int index]
	{
		get
		{
			return _root[index].Item;
		}
		set
		{
			_root.ReplaceAt(index, value);
		}
	}

	public int Count => _root.Count;

	public bool IsSynchronized => false;

	public object SyncRoot => null;

	internal event EventHandler LiveShapingDirty;

	internal LiveShapingList(ICollectionViewLiveShaping view, LiveShapingFlags flags, IComparer comparer)
	{
		_view = view;
		_comparer = comparer;
		_isCustomSorting = !(comparer is SortFieldComparer);
		_dpFromPath = new DPFromPath();
		_root = new LiveShapingTree(this);
		if (comparer != null)
		{
			_root.Comparison = CompareLiveShapingItems;
		}
		_sortDirtyItems = new List<LiveShapingItem>();
		_filterDirtyItems = new List<LiveShapingItem>();
		_groupDirtyItems = new List<LiveShapingItem>();
		SetLiveShapingProperties(flags);
	}

	internal void SetLiveShapingProperties(LiveShapingFlags flags)
	{
		_dpFromPath.BeginReset();
		SortDescriptionCollection sortDescriptions = ((ICollectionView)View).SortDescriptions;
		int count = sortDescriptions.Count;
		_compInfos = new LivePropertyInfo[count];
		for (int i = 0; i < count; i++)
		{
			string path = NormalizePath(sortDescriptions[i].PropertyName);
			_compInfos[i] = new LivePropertyInfo(path, _dpFromPath.GetDP(path));
		}
		if (TestLiveShapingFlag(flags, LiveShapingFlags.Sorting))
		{
			Collection<string> liveSortingProperties = View.LiveSortingProperties;
			if (liveSortingProperties.Count == 0)
			{
				_sortInfos = _compInfos;
			}
			else
			{
				count = liveSortingProperties.Count;
				_sortInfos = new LivePropertyInfo[count];
				for (int i = 0; i < count; i++)
				{
					string path = NormalizePath(liveSortingProperties[i]);
					_sortInfos[i] = new LivePropertyInfo(path, _dpFromPath.GetDP(path));
				}
			}
		}
		else
		{
			_sortInfos = Array.Empty<LivePropertyInfo>();
		}
		if (TestLiveShapingFlag(flags, LiveShapingFlags.Filtering))
		{
			Collection<string> liveFilteringProperties = View.LiveFilteringProperties;
			count = liveFilteringProperties.Count;
			_filterInfos = new LivePropertyInfo[count];
			for (int i = 0; i < count; i++)
			{
				string path = NormalizePath(liveFilteringProperties[i]);
				_filterInfos[i] = new LivePropertyInfo(path, _dpFromPath.GetDP(path));
			}
			_filterRoot = new LiveShapingTree(this);
		}
		else
		{
			_filterInfos = Array.Empty<LivePropertyInfo>();
			_filterRoot = null;
		}
		if (TestLiveShapingFlag(flags, LiveShapingFlags.Grouping))
		{
			Collection<string> collection = View.LiveGroupingProperties;
			if (collection.Count == 0)
			{
				collection = new Collection<string>();
				ObservableCollection<GroupDescription> observableCollection = ((View is ICollectionView collectionView) ? collectionView.GroupDescriptions : null);
				if (observableCollection != null)
				{
					foreach (GroupDescription item in observableCollection)
					{
						if (item is PropertyGroupDescription propertyGroupDescription)
						{
							collection.Add(propertyGroupDescription.PropertyName);
						}
					}
				}
			}
			count = collection.Count;
			_groupInfos = new LivePropertyInfo[count];
			for (int i = 0; i < count; i++)
			{
				string path = NormalizePath(collection[i]);
				_groupInfos[i] = new LivePropertyInfo(path, _dpFromPath.GetDP(path));
			}
		}
		else
		{
			_groupInfos = Array.Empty<LivePropertyInfo>();
		}
		_dpFromPath.EndReset();
	}

	private bool TestLiveShapingFlag(LiveShapingFlags flags, LiveShapingFlags flag)
	{
		return (flags & flag) != 0;
	}

	internal int Search(int index, int count, object value)
	{
		LiveShapingItem liveShapingItem = new LiveShapingItem(value, this, filtered: true, null, oneTime: true);
		RBFinger<LiveShapingItem> rBFinger = _root.BoundedSearch(liveShapingItem, index, index + count);
		ClearItem(liveShapingItem);
		if (!rBFinger.Found)
		{
			return ~rBFinger.Index;
		}
		return rBFinger.Index;
	}

	internal void Sort()
	{
		_root.Sort();
	}

	internal int CompareLiveShapingItems(LiveShapingItem x, LiveShapingItem y)
	{
		if (x == y || ItemsControl.EqualsEx(x.Item, y.Item))
		{
			return 0;
		}
		int num = 0;
		if (!_isCustomSorting)
		{
			SortFieldComparer sortFieldComparer = _comparer as SortFieldComparer;
			SortDescriptionCollection sortDescriptions = ((ICollectionView)View).SortDescriptions;
			int num2 = _compInfos.Length;
			for (int i = 0; i < num2; i++)
			{
				object value = x.GetValue(_compInfos[i].Path, _compInfos[i].Property);
				object value2 = y.GetValue(_compInfos[i].Path, _compInfos[i].Property);
				num = sortFieldComparer.BaseComparer.Compare(value, value2);
				if (sortDescriptions[i].Direction == ListSortDirection.Descending)
				{
					num = -num;
				}
				if (num != 0)
				{
					break;
				}
			}
		}
		else
		{
			num = _comparer.Compare(x.Item, y.Item);
		}
		return num;
	}

	internal void Move(int oldIndex, int newIndex)
	{
		_root.Move(oldIndex, newIndex);
	}

	internal void RestoreLiveSortingByInsertionSort(Action<NotifyCollectionChangedEventArgs, int, int> RaiseMoveEvent)
	{
		_isRestoringLiveSorting = true;
		_root.RestoreLiveSortingByInsertionSort(RaiseMoveEvent);
		_isRestoringLiveSorting = false;
	}

	internal void AddFilteredItem(object item)
	{
		LiveShapingItem item2 = new LiveShapingItem(item, this, filtered: true)
		{
			FailsFilter = true
		};
		_filterRoot.Insert(_filterRoot.Count, item2);
	}

	internal void AddFilteredItem(LiveShapingItem lsi)
	{
		InitializeItem(lsi, lsi.Item, filtered: true, oneTime: false);
		lsi.FailsFilter = true;
		_filterRoot.Insert(_filterRoot.Count, lsi);
	}

	internal void SetStartingIndexForFilteredItem(object item, int value)
	{
		foreach (LiveShapingItem filterDirtyItem in _filterDirtyItems)
		{
			if (ItemsControl.EqualsEx(item, filterDirtyItem.Item))
			{
				filterDirtyItem.StartingIndex = value;
				break;
			}
		}
	}

	internal void RemoveFilteredItem(LiveShapingItem lsi)
	{
		_filterRoot.RemoveAt(_filterRoot.IndexOf(lsi));
		ClearItem(lsi);
	}

	internal void RemoveFilteredItem(object item)
	{
		LiveShapingItem liveShapingItem = _filterRoot.FindItem(item);
		if (liveShapingItem != null)
		{
			RemoveFilteredItem(liveShapingItem);
		}
	}

	internal void ReplaceFilteredItem(object oldItem, object newItem)
	{
		LiveShapingItem liveShapingItem = _filterRoot.FindItem(oldItem);
		if (liveShapingItem != null)
		{
			ClearItem(liveShapingItem);
			InitializeItem(liveShapingItem, newItem, filtered: true, oneTime: false);
		}
	}

	internal int IndexOf(LiveShapingItem lsi)
	{
		return _root.IndexOf(lsi);
	}

	internal void InitializeItem(LiveShapingItem lsi, object item, bool filtered, bool oneTime)
	{
		lsi.Item = item;
		LivePropertyInfo[] sortInfos;
		if (!filtered)
		{
			sortInfos = _sortInfos;
			for (int i = 0; i < sortInfos.Length; i++)
			{
				LivePropertyInfo livePropertyInfo = sortInfos[i];
				lsi.Block = _root.PlaceholderBlock;
				lsi.SetBinding(livePropertyInfo.Path, livePropertyInfo.Property, oneTime, enableXT: true);
			}
			sortInfos = _groupInfos;
			for (int i = 0; i < sortInfos.Length; i++)
			{
				LivePropertyInfo livePropertyInfo2 = sortInfos[i];
				lsi.SetBinding(livePropertyInfo2.Path, livePropertyInfo2.Property, oneTime);
			}
		}
		sortInfos = _filterInfos;
		for (int i = 0; i < sortInfos.Length; i++)
		{
			LivePropertyInfo livePropertyInfo3 = sortInfos[i];
			lsi.SetBinding(livePropertyInfo3.Path, livePropertyInfo3.Property, oneTime);
		}
		lsi.ForwardChanges = !oneTime;
	}

	internal void ClearItem(LiveShapingItem lsi)
	{
		lsi.ForwardChanges = false;
		foreach (DependencyProperty value in ObservedProperties.Values)
		{
			BindingOperations.ClearBinding(lsi, value);
		}
	}

	private string NormalizePath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return path;
		}
		return string.Empty;
	}

	internal void OnItemPropertyChanged(LiveShapingItem lsi, DependencyProperty dp)
	{
		if (ContainsDP(_sortInfos, dp) && !lsi.FailsFilter && !lsi.IsSortPendingClean)
		{
			lsi.IsSortDirty = true;
			lsi.IsSortPendingClean = true;
			_sortDirtyItems.Add(lsi);
			OnLiveShapingDirty();
		}
		if (ContainsDP(_filterInfos, dp) && !lsi.IsFilterDirty)
		{
			lsi.IsFilterDirty = true;
			_filterDirtyItems.Add(lsi);
			OnLiveShapingDirty();
		}
		if (ContainsDP(_groupInfos, dp) && !lsi.FailsFilter && !lsi.IsGroupDirty)
		{
			lsi.IsGroupDirty = true;
			_groupDirtyItems.Add(lsi);
			OnLiveShapingDirty();
		}
	}

	internal void OnItemPropertyChangedCrossThread(LiveShapingItem lsi, DependencyProperty dp)
	{
		if (_isCustomSorting && ContainsDP(_sortInfos, dp) && !lsi.FailsFilter)
		{
			lsi.IsSortDirty = true;
		}
	}

	private void OnLiveShapingDirty()
	{
		if (this.LiveShapingDirty != null)
		{
			this.LiveShapingDirty(this, EventArgs.Empty);
		}
	}

	private bool ContainsDP(LivePropertyInfo[] infos, DependencyProperty dp)
	{
		for (int i = 0; i < infos.Length; i++)
		{
			if (infos[i].Property == dp || (dp == null && string.IsNullOrEmpty(infos[i].Path)))
			{
				return true;
			}
		}
		return false;
	}

	internal void FindPosition(LiveShapingItem lsi, out int oldIndex, out int newIndex)
	{
		_root.FindPosition(lsi, out oldIndex, out newIndex);
	}

	internal LiveShapingItem ItemAt(int index)
	{
		return _root[index];
	}

	public int Add(object value)
	{
		Insert(Count, value);
		return Count;
	}

	public void Clear()
	{
		ForEach(delegate(LiveShapingItem x)
		{
			ClearItem(x);
		});
		_root = new LiveShapingTree(this);
	}

	public bool Contains(object value)
	{
		return IndexOf(value) >= 0;
	}

	public int IndexOf(object value)
	{
		int result = 0;
		ForEachUntil(delegate(LiveShapingItem x)
		{
			if (ItemsControl.EqualsEx(value, x.Item))
			{
				return true;
			}
			int num = result + 1;
			result = num;
			return false;
		});
		if (result >= Count)
		{
			return -1;
		}
		return result;
	}

	public void Insert(int index, object value)
	{
		_root.Insert(index, new LiveShapingItem(value, this));
	}

	public void Remove(object value)
	{
		int num = IndexOf(value);
		if (num >= 0)
		{
			RemoveAt(num);
		}
	}

	public void RemoveAt(int index)
	{
		LiveShapingItem liveShapingItem = _root[index];
		_root.RemoveAt(index);
		ClearItem(liveShapingItem);
		liveShapingItem.IsDeleted = true;
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}

	public IEnumerator GetEnumerator()
	{
		return new ItemEnumerator(_root.GetEnumerator());
	}

	private void ForEach(Action<LiveShapingItem> action)
	{
		_root.ForEach(action);
	}

	private void ForEachUntil(Func<LiveShapingItem, bool> action)
	{
		_root.ForEachUntil(action);
	}

	internal bool VerifyLiveSorting(LiveShapingItem lsi)
	{
		return true;
	}
}
