using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MS.Internal.Data;

internal class CollectionViewGroupInternal : CollectionViewGroup
{
	internal class IListComparer : IComparer
	{
		private int _index;

		private IList _list;

		internal IListComparer(IList list)
		{
			ResetList(list);
		}

		internal void Reset()
		{
			_index = 0;
		}

		internal void ResetList(IList list)
		{
			_list = list;
			_index = 0;
		}

		public int Compare(object x, object y)
		{
			if (ItemsControl.EqualsEx(x, y))
			{
				return 0;
			}
			int num = ((_list != null) ? _list.Count : 0);
			while (_index < num)
			{
				object o = _list[_index];
				if (ItemsControl.EqualsEx(x, o))
				{
					return -1;
				}
				if (ItemsControl.EqualsEx(y, o))
				{
					return 1;
				}
				_index++;
			}
			return 1;
		}
	}

	private class LeafEnumerator : IEnumerator
	{
		private CollectionViewGroupInternal _group;

		private int _version;

		private int _index;

		private IEnumerator _subEnum;

		private object _current;

		object IEnumerator.Current
		{
			get
			{
				if (_index < 0 || _index >= _group.Items.Count)
				{
					throw new InvalidOperationException();
				}
				return _current;
			}
		}

		public LeafEnumerator(CollectionViewGroupInternal group)
		{
			_group = group;
			DoReset();
		}

		void IEnumerator.Reset()
		{
			DoReset();
		}

		private void DoReset()
		{
			_version = _group._version;
			_index = -1;
			_subEnum = null;
		}

		bool IEnumerator.MoveNext()
		{
			if (_group._version != _version)
			{
				throw new InvalidOperationException();
			}
			while (_subEnum == null || !_subEnum.MoveNext())
			{
				_index++;
				if (_index >= _group.Items.Count)
				{
					return false;
				}
				if (!(_group.Items[_index] is CollectionViewGroupInternal collectionViewGroupInternal))
				{
					_current = _group.Items[_index];
					_subEnum = null;
					return true;
				}
				_subEnum = collectionViewGroupInternal.GetLeafEnumerator();
			}
			_current = _subEnum.Current;
			return true;
		}
	}

	private class EmptyGroupRemover : IDisposable
	{
		private List<CollectionViewGroupInternal> _toRemove;

		public static EmptyGroupRemover Create(bool isNeeded)
		{
			if (!isNeeded)
			{
				return null;
			}
			return new EmptyGroupRemover();
		}

		public void RemoveEmptyGroup(CollectionViewGroupInternal group)
		{
			if (_toRemove == null)
			{
				_toRemove = new List<CollectionViewGroupInternal>();
			}
			_toRemove.Add(group);
		}

		public void Dispose()
		{
			if (_toRemove == null)
			{
				return;
			}
			foreach (CollectionViewGroupInternal item in _toRemove)
			{
				CollectionViewGroupInternal parent = item.Parent;
				if (parent != null && !item.IsExplicit)
				{
					parent.Remove(item, returnLeafIndex: false);
				}
			}
		}
	}

	private GroupDescription _groupBy;

	private CollectionViewGroupInternal _parentGroup;

	private IComparer _groupComparer;

	private int _fullCount = 1;

	private int _lastIndex;

	private int _version;

	private Hashtable _nameToGroupMap;

	private bool _mapCleanupScheduled;

	private bool _isExplicit;

	private static NamedObject _nullGroupNameKey = new NamedObject("NullGroupNameKey");

	public override bool IsBottomLevel => _groupBy == null;

	internal GroupDescription GroupBy
	{
		get
		{
			return _groupBy;
		}
		set
		{
			bool isBottomLevel = IsBottomLevel;
			if (_groupBy != null)
			{
				PropertyChangedEventManager.RemoveHandler(_groupBy, OnGroupByChanged, string.Empty);
			}
			_groupBy = value;
			if (_groupBy != null)
			{
				PropertyChangedEventManager.AddHandler(_groupBy, OnGroupByChanged, string.Empty);
			}
			_groupComparer = ((_groupBy == null) ? null : ListCollectionView.PrepareComparer(_groupBy.CustomSort, _groupBy.SortDescriptionsInternal, delegate(object state)
			{
				for (CollectionViewGroupInternal collectionViewGroupInternal = (CollectionViewGroupInternal)state; collectionViewGroupInternal != null; collectionViewGroupInternal = collectionViewGroupInternal.Parent)
				{
					if (collectionViewGroupInternal is CollectionViewGroupRoot collectionViewGroupRoot)
					{
						return collectionViewGroupRoot.View;
					}
				}
				return (CollectionView)null;
			}, this));
			if (isBottomLevel != IsBottomLevel)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("IsBottomLevel"));
			}
		}
	}

	internal int FullCount
	{
		get
		{
			return _fullCount;
		}
		set
		{
			_fullCount = value;
		}
	}

	internal int LastIndex
	{
		get
		{
			return _lastIndex;
		}
		set
		{
			_lastIndex = value;
		}
	}

	internal object SeedItem
	{
		get
		{
			if (base.ItemCount > 0 && (GroupBy == null || GroupBy.GroupNames.Count == 0))
			{
				int i = 0;
				for (int count = base.Items.Count; i < count; i++)
				{
					if (!(base.Items[i] is CollectionViewGroupInternal collectionViewGroupInternal))
					{
						return base.Items[i];
					}
					if (collectionViewGroupInternal.ItemCount > 0)
					{
						return collectionViewGroupInternal.SeedItem;
					}
				}
				return DependencyProperty.UnsetValue;
			}
			return DependencyProperty.UnsetValue;
		}
	}

	internal CollectionViewGroupInternal Parent => _parentGroup;

	private bool IsExplicit => _isExplicit;

	internal CollectionViewGroupInternal(object name, CollectionViewGroupInternal parent, bool isExplicit = false)
		: base(name)
	{
		_parentGroup = parent;
		_isExplicit = isExplicit;
	}

	internal void Add(object item)
	{
		if (_groupComparer == null)
		{
			ChangeCounts(item, 1);
			base.ProtectedItems.Add(item);
		}
		else
		{
			Insert(item, null, null);
		}
	}

	internal int Remove(object item, bool returnLeafIndex)
	{
		int result = -1;
		int num = base.ProtectedItems.IndexOf(item);
		if (num >= 0)
		{
			if (returnLeafIndex)
			{
				result = LeafIndexFromItem(null, num);
			}
			if (item is CollectionViewGroupInternal collectionViewGroupInternal)
			{
				collectionViewGroupInternal.Clear();
				RemoveSubgroupFromMap(collectionViewGroupInternal);
			}
			ChangeCounts(item, -1);
			if (base.ProtectedItems.Count > 0)
			{
				base.ProtectedItems.RemoveAt(num);
			}
		}
		return result;
	}

	internal void Clear()
	{
		FullCount = 1;
		base.ProtectedItemCount = 0;
		if (_groupBy != null)
		{
			PropertyChangedEventManager.RemoveHandler(_groupBy, OnGroupByChanged, string.Empty);
			_groupBy = null;
			int i = 0;
			for (int count = base.ProtectedItems.Count; i < count; i++)
			{
				if (base.ProtectedItems[i] is CollectionViewGroupInternal collectionViewGroupInternal)
				{
					collectionViewGroupInternal.Clear();
				}
			}
		}
		base.ProtectedItems.Clear();
		if (_nameToGroupMap != null)
		{
			_nameToGroupMap.Clear();
		}
	}

	internal int LeafIndexOf(object item)
	{
		int num = 0;
		int i = 0;
		for (int count = base.Items.Count; i < count; i++)
		{
			if (base.Items[i] is CollectionViewGroupInternal collectionViewGroupInternal)
			{
				int num2 = collectionViewGroupInternal.LeafIndexOf(item);
				if (num2 >= 0)
				{
					return num + num2;
				}
				num += collectionViewGroupInternal.ItemCount;
			}
			else
			{
				if (ItemsControl.EqualsEx(item, base.Items[i]))
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	internal int LeafIndexFromItem(object item, int index)
	{
		int num = 0;
		CollectionViewGroupInternal collectionViewGroupInternal = this;
		while (collectionViewGroupInternal != null)
		{
			int i = 0;
			for (int count = collectionViewGroupInternal.Items.Count; i < count && (index >= 0 || !ItemsControl.EqualsEx(item, collectionViewGroupInternal.Items[i])) && index != i; i++)
			{
				num += (collectionViewGroupInternal.Items[i] as CollectionViewGroupInternal)?.ItemCount ?? 1;
			}
			item = collectionViewGroupInternal;
			collectionViewGroupInternal = collectionViewGroupInternal.Parent;
			index = -1;
		}
		return num;
	}

	internal object LeafAt(int index)
	{
		int i = 0;
		for (int count = base.Items.Count; i < count; i++)
		{
			if (base.Items[i] is CollectionViewGroupInternal collectionViewGroupInternal)
			{
				if (index < collectionViewGroupInternal.ItemCount)
				{
					return collectionViewGroupInternal.LeafAt(index);
				}
				index -= collectionViewGroupInternal.ItemCount;
			}
			else
			{
				if (index == 0)
				{
					return base.Items[i];
				}
				index--;
			}
		}
		throw new ArgumentOutOfRangeException("index");
	}

	internal IEnumerator GetLeafEnumerator()
	{
		return new LeafEnumerator(this);
	}

	internal int Insert(object item, object seed, IComparer comparer)
	{
		int low = 0;
		if (_groupComparer == null && GroupBy != null)
		{
			low = GroupBy.GroupNames.Count;
		}
		int num = FindIndex(item, seed, comparer, low, base.ProtectedItems.Count);
		ChangeCounts(item, 1);
		base.ProtectedItems.Insert(num, item);
		return num;
	}

	protected virtual int FindIndex(object item, object seed, IComparer comparer, int low, int high)
	{
		int i;
		if (_groupComparer == null)
		{
			if (comparer != null)
			{
				if (comparer is IListComparer listComparer)
				{
					listComparer.Reset();
				}
				for (i = low; i < high; i++)
				{
					object obj = ((base.ProtectedItems[i] is CollectionViewGroupInternal collectionViewGroupInternal) ? collectionViewGroupInternal.SeedItem : base.ProtectedItems[i]);
					if (obj != DependencyProperty.UnsetValue && comparer.Compare(seed, obj) < 0)
					{
						break;
					}
				}
			}
			else
			{
				i = high;
			}
		}
		else
		{
			for (i = low; i < high && _groupComparer.Compare(item, base.ProtectedItems[i]) >= 0; i++)
			{
			}
		}
		return i;
	}

	internal bool Move(object item, IList list, ref int oldIndex, ref int newIndex)
	{
		int num = -1;
		int num2 = -1;
		int num3 = 0;
		int count = base.ProtectedItems.Count;
		int num4 = 0;
		while (true)
		{
			if (num4 == oldIndex)
			{
				num = num3;
				if (num2 >= 0)
				{
					break;
				}
				num3++;
			}
			if (num4 == newIndex)
			{
				num2 = num3;
				if (num >= 0)
				{
					num2--;
					break;
				}
				num4++;
				oldIndex++;
			}
			if (num3 < count && ItemsControl.EqualsEx(base.ProtectedItems[num3], list[num4]))
			{
				num3++;
			}
			num4++;
		}
		if (num == num2)
		{
			return false;
		}
		int num5 = 0;
		int num6;
		int num7;
		int num8;
		if (num < num2)
		{
			num6 = num + 1;
			num7 = num2 + 1;
			num8 = LeafIndexFromItem(null, num);
		}
		else
		{
			num6 = num2;
			num7 = num;
			num8 = LeafIndexFromItem(null, num2);
		}
		for (int i = num6; i < num7; i++)
		{
			num5 += (base.Items[i] as CollectionViewGroupInternal)?.ItemCount ?? 1;
		}
		if (num < num2)
		{
			oldIndex = num8;
			newIndex = oldIndex + num5;
		}
		else
		{
			newIndex = num8;
			oldIndex = newIndex + num5;
		}
		base.ProtectedItems.Move(num, num2);
		return true;
	}

	protected virtual void OnGroupByChanged()
	{
		if (Parent != null)
		{
			Parent.OnGroupByChanged();
		}
	}

	internal void AddSubgroupToMap(object nameKey, CollectionViewGroupInternal subgroup)
	{
		if (nameKey == null)
		{
			nameKey = _nullGroupNameKey;
		}
		if (_nameToGroupMap == null)
		{
			_nameToGroupMap = new Hashtable();
		}
		_nameToGroupMap[nameKey] = new WeakReference(subgroup);
		ScheduleMapCleanup();
	}

	private void RemoveSubgroupFromMap(CollectionViewGroupInternal subgroup)
	{
		if (_nameToGroupMap == null)
		{
			return;
		}
		object obj = null;
		foreach (object key in _nameToGroupMap.Keys)
		{
			if (_nameToGroupMap[key] is WeakReference weakReference && weakReference.Target == subgroup)
			{
				obj = key;
				break;
			}
		}
		if (obj != null)
		{
			_nameToGroupMap.Remove(obj);
		}
		ScheduleMapCleanup();
	}

	internal CollectionViewGroupInternal GetSubgroupFromMap(object nameKey)
	{
		if (_nameToGroupMap != null)
		{
			if (nameKey == null)
			{
				nameKey = _nullGroupNameKey;
			}
			if (_nameToGroupMap[nameKey] is WeakReference weakReference)
			{
				return weakReference.Target as CollectionViewGroupInternal;
			}
		}
		return null;
	}

	private void ScheduleMapCleanup()
	{
		if (_mapCleanupScheduled)
		{
			return;
		}
		_mapCleanupScheduled = true;
		Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
		{
			_mapCleanupScheduled = false;
			if (_nameToGroupMap != null)
			{
				ArrayList arrayList = new ArrayList();
				foreach (object key in _nameToGroupMap.Keys)
				{
					if (!(_nameToGroupMap[key] is WeakReference { IsAlive: not false }))
					{
						arrayList.Add(key);
					}
				}
				foreach (object item in arrayList)
				{
					_nameToGroupMap.Remove(item);
				}
			}
		}, DispatcherPriority.ContextIdle);
	}

	protected void ChangeCounts(object item, int delta)
	{
		bool flag = !(item is CollectionViewGroup);
		using (EmptyGroupRemover emptyGroupRemover = EmptyGroupRemover.Create(flag && delta < 0))
		{
			for (CollectionViewGroupInternal collectionViewGroupInternal = this; collectionViewGroupInternal != null; collectionViewGroupInternal = collectionViewGroupInternal._parentGroup)
			{
				collectionViewGroupInternal.FullCount += delta;
				if (flag)
				{
					collectionViewGroupInternal.ProtectedItemCount += delta;
					if (collectionViewGroupInternal.ProtectedItemCount == 0)
					{
						emptyGroupRemover.RemoveEmptyGroup(collectionViewGroupInternal);
					}
				}
			}
		}
		_version++;
	}

	private void OnGroupByChanged(object sender, PropertyChangedEventArgs e)
	{
		OnGroupByChanged();
	}
}
