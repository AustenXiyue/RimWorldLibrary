using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class CollectionViewGroupRoot : CollectionViewGroupInternal, INotifyCollectionChanged
{
	private class GroupTreeNode
	{
		public GroupTreeNode FirstChild { get; set; }

		public GroupTreeNode Sibling { get; set; }

		public CollectionViewGroupInternal Group { get; set; }

		public bool ContainsItem { get; set; }

		public bool ContainsItemDirectly { get; set; }
	}

	private class TopLevelGroupDescription : GroupDescription
	{
		public override object GroupNameFromItem(object item, int level, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	private CollectionView _view;

	private IComparer _comparer;

	private bool _isDataInGroupOrder;

	private ObservableCollection<GroupDescription> _groupBy = new ObservableCollection<GroupDescription>();

	private GroupDescriptionSelectorCallback _groupBySelector;

	private static GroupDescription _topLevelGroupDescription;

	private static readonly object UseAsItemDirectly = new NamedObject("UseAsItemDirectly");

	public virtual ObservableCollection<GroupDescription> GroupDescriptions => _groupBy;

	public virtual GroupDescriptionSelectorCallback GroupBySelector
	{
		get
		{
			return _groupBySelector;
		}
		set
		{
			_groupBySelector = value;
		}
	}

	internal IComparer ActiveComparer
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

	internal CultureInfo Culture => _view.Culture;

	internal bool IsDataInGroupOrder
	{
		get
		{
			return _isDataInGroupOrder;
		}
		set
		{
			_isDataInGroupOrder = value;
		}
	}

	internal CollectionView View => _view;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	internal event EventHandler GroupDescriptionChanged;

	internal CollectionViewGroupRoot(CollectionView view)
		: base("Root", null)
	{
		_view = view;
	}

	public void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, args);
		}
	}

	protected override void OnGroupByChanged()
	{
		if (this.GroupDescriptionChanged != null)
		{
			this.GroupDescriptionChanged(this, EventArgs.Empty);
		}
	}

	internal void Initialize()
	{
		if (_topLevelGroupDescription == null)
		{
			_topLevelGroupDescription = new TopLevelGroupDescription();
		}
		InitializeGroup(this, _topLevelGroupDescription, 0);
	}

	internal void AddToSubgroups(object item, LiveShapingItem lsi, bool loading)
	{
		AddToSubgroups(item, lsi, this, 0, loading);
	}

	internal bool RemoveFromSubgroups(object item)
	{
		return RemoveFromSubgroups(item, this, 0);
	}

	internal void RemoveItemFromSubgroupsByExhaustiveSearch(object item)
	{
		RemoveItemFromSubgroupsByExhaustiveSearch(this, item);
	}

	internal void InsertSpecialItem(int index, object item, bool loading)
	{
		ChangeCounts(item, 1);
		base.ProtectedItems.Insert(index, item);
		if (!loading)
		{
			int index2 = LeafIndexFromItem(item, index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index2));
		}
	}

	internal void RemoveSpecialItem(int index, object item, bool loading)
	{
		int index2 = -1;
		if (!loading)
		{
			index2 = LeafIndexFromItem(item, index);
		}
		ChangeCounts(item, -1);
		base.ProtectedItems.RemoveAt(index);
		if (!loading)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index2));
		}
	}

	internal void MoveWithinSubgroups(object item, LiveShapingItem lsi, IList list, int oldIndex, int newIndex)
	{
		if (lsi == null)
		{
			MoveWithinSubgroups(item, this, 0, list, oldIndex, newIndex);
			return;
		}
		CollectionViewGroupInternal parentGroup = lsi.ParentGroup;
		if (parentGroup != null)
		{
			MoveWithinSubgroup(item, parentGroup, list, oldIndex, newIndex);
			return;
		}
		foreach (CollectionViewGroupInternal parentGroup2 in lsi.ParentGroups)
		{
			MoveWithinSubgroup(item, parentGroup2, list, oldIndex, newIndex);
		}
	}

	protected override int FindIndex(object item, object seed, IComparer comparer, int low, int high)
	{
		if (_view is IEditableCollectionView editableCollectionView)
		{
			if (editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
			{
				low++;
				if (editableCollectionView.IsAddingNew)
				{
					low++;
				}
			}
			else
			{
				if (editableCollectionView.IsAddingNew)
				{
					high--;
				}
				if (editableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)
				{
					high--;
				}
			}
		}
		return base.FindIndex(item, seed, comparer, low, high);
	}

	internal void RestoreGrouping(LiveShapingItem lsi, List<AbandonedGroupItem> deleteList)
	{
		GroupTreeNode groupTreeNode = BuildGroupTree(lsi);
		groupTreeNode.ContainsItem = true;
		RestoreGrouping(lsi, groupTreeNode, 0, deleteList);
	}

	private void RestoreGrouping(LiveShapingItem lsi, GroupTreeNode node, int level, List<AbandonedGroupItem> deleteList)
	{
		if (node.ContainsItem)
		{
			object obj = GetGroupName(lsi.Item, node.Group.GroupBy, level);
			if (obj != UseAsItemDirectly)
			{
				ArrayList arrayList = ((!(obj is ICollection c)) ? null : new ArrayList(c));
				for (GroupTreeNode groupTreeNode = node.FirstChild; groupTreeNode != null; groupTreeNode = groupTreeNode.Sibling)
				{
					if (arrayList == null)
					{
						if (object.Equals(obj, groupTreeNode.Group.Name))
						{
							groupTreeNode.ContainsItem = true;
							obj = DependencyProperty.UnsetValue;
							break;
						}
					}
					else if (arrayList.Contains(groupTreeNode.Group.Name))
					{
						groupTreeNode.ContainsItem = true;
						arrayList.Remove(groupTreeNode.Group.Name);
					}
				}
				if (arrayList == null)
				{
					if (obj != DependencyProperty.UnsetValue)
					{
						AddToSubgroup(lsi.Item, lsi, node.Group, level, obj, loading: false);
					}
				}
				else
				{
					foreach (object item in arrayList)
					{
						AddToSubgroup(lsi.Item, lsi, node.Group, level, item, loading: false);
					}
				}
			}
		}
		else if (node.ContainsItemDirectly)
		{
			deleteList.Add(new AbandonedGroupItem(lsi, node.Group));
		}
		for (GroupTreeNode groupTreeNode2 = node.FirstChild; groupTreeNode2 != null; groupTreeNode2 = groupTreeNode2.Sibling)
		{
			RestoreGrouping(lsi, groupTreeNode2, level + 1, deleteList);
		}
	}

	private GroupTreeNode BuildGroupTree(LiveShapingItem lsi)
	{
		CollectionViewGroupInternal collectionViewGroupInternal = lsi.ParentGroup;
		if (collectionViewGroupInternal != null)
		{
			GroupTreeNode groupTreeNode = new GroupTreeNode
			{
				Group = collectionViewGroupInternal,
				ContainsItemDirectly = true
			};
			while (true)
			{
				collectionViewGroupInternal = collectionViewGroupInternal.Parent;
				if (collectionViewGroupInternal == null)
				{
					break;
				}
				groupTreeNode = new GroupTreeNode
				{
					Group = collectionViewGroupInternal,
					FirstChild = groupTreeNode
				};
			}
			return groupTreeNode;
		}
		List<CollectionViewGroupInternal> parentGroups = lsi.ParentGroups;
		List<GroupTreeNode> list = new List<GroupTreeNode>(parentGroups.Count + 1);
		GroupTreeNode result = null;
		foreach (CollectionViewGroupInternal item in parentGroups)
		{
			GroupTreeNode groupTreeNode = new GroupTreeNode
			{
				Group = item,
				ContainsItemDirectly = true
			};
			list.Add(groupTreeNode);
		}
		for (int i = 0; i < list.Count; i++)
		{
			GroupTreeNode groupTreeNode = list[i];
			collectionViewGroupInternal = groupTreeNode.Group.Parent;
			GroupTreeNode groupTreeNode2 = null;
			if (collectionViewGroupInternal == null)
			{
				result = groupTreeNode;
				continue;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].Group == collectionViewGroupInternal)
				{
					groupTreeNode2 = list[num];
					break;
				}
			}
			if (groupTreeNode2 == null)
			{
				groupTreeNode2 = new GroupTreeNode
				{
					Group = collectionViewGroupInternal,
					FirstChild = groupTreeNode
				};
				list.Add(groupTreeNode2);
			}
			else
			{
				groupTreeNode.Sibling = groupTreeNode2.FirstChild;
				groupTreeNode2.FirstChild = groupTreeNode;
			}
		}
		return result;
	}

	internal void DeleteAbandonedGroupItems(List<AbandonedGroupItem> deleteList)
	{
		foreach (AbandonedGroupItem delete in deleteList)
		{
			RemoveFromGroupDirectly(delete.Group, delete.Item.Item);
			delete.Item.RemoveParentGroup(delete.Group);
		}
	}

	private void InitializeGroup(CollectionViewGroupInternal group, GroupDescription parentDescription, int level)
	{
		GroupDescription groupDescription2 = (group.GroupBy = GetGroupDescription(group, parentDescription, level));
		ObservableCollection<object> observableCollection = groupDescription2?.GroupNames;
		if (observableCollection != null)
		{
			int i = 0;
			for (int count = observableCollection.Count; i < count; i++)
			{
				CollectionViewGroupInternal collectionViewGroupInternal = new CollectionViewGroupInternal(observableCollection[i], group, isExplicit: true);
				InitializeGroup(collectionViewGroupInternal, groupDescription2, level + 1);
				group.Add(collectionViewGroupInternal);
			}
		}
		group.LastIndex = 0;
	}

	private GroupDescription GetGroupDescription(CollectionViewGroup group, GroupDescription parentDescription, int level)
	{
		GroupDescription groupDescription = null;
		if (group == this)
		{
			group = null;
		}
		if (groupDescription == null && GroupBySelector != null)
		{
			groupDescription = GroupBySelector(group, level);
		}
		if (groupDescription == null && level < GroupDescriptions.Count)
		{
			groupDescription = GroupDescriptions[level];
		}
		return groupDescription;
	}

	private void AddToSubgroups(object item, LiveShapingItem lsi, CollectionViewGroupInternal group, int level, bool loading)
	{
		object groupName = GetGroupName(item, group.GroupBy, level);
		if (groupName == UseAsItemDirectly)
		{
			lsi?.AddParentGroup(group);
			if (loading)
			{
				group.Add(item);
				return;
			}
			int index = group.Insert(item, item, ActiveComparer);
			int index2 = group.LeafIndexFromItem(item, index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index2));
			return;
		}
		if (!(groupName is ICollection collection))
		{
			AddToSubgroup(item, lsi, group, level, groupName, loading);
			return;
		}
		foreach (object item2 in collection)
		{
			AddToSubgroup(item, lsi, group, level, item2, loading);
		}
	}

	private void AddToSubgroup(object item, LiveShapingItem lsi, CollectionViewGroupInternal group, int level, object name, bool loading)
	{
		int i = ((loading && IsDataInGroupOrder) ? group.LastIndex : 0);
		object groupNameKey = GetGroupNameKey(name, group);
		CollectionViewGroupInternal subgroupFromMap;
		if ((subgroupFromMap = group.GetSubgroupFromMap(groupNameKey)) != null && group.GroupBy.NamesMatch(subgroupFromMap.Name, name))
		{
			group.LastIndex = ((group.Items[i] == subgroupFromMap) ? i : 0);
			AddToSubgroups(item, lsi, subgroupFromMap, level + 1, loading);
			return;
		}
		for (int count = group.Items.Count; i < count; i++)
		{
			if (group.Items[i] is CollectionViewGroupInternal collectionViewGroupInternal && group.GroupBy.NamesMatch(collectionViewGroupInternal.Name, name))
			{
				group.LastIndex = i;
				group.AddSubgroupToMap(groupNameKey, collectionViewGroupInternal);
				AddToSubgroups(item, lsi, collectionViewGroupInternal, level + 1, loading);
				return;
			}
		}
		subgroupFromMap = new CollectionViewGroupInternal(name, group);
		InitializeGroup(subgroupFromMap, group.GroupBy, level + 1);
		if (loading)
		{
			group.Add(subgroupFromMap);
			group.LastIndex = i;
		}
		else
		{
			group.Insert(subgroupFromMap, item, ActiveComparer);
		}
		group.AddSubgroupToMap(groupNameKey, subgroupFromMap);
		AddToSubgroups(item, lsi, subgroupFromMap, level + 1, loading);
	}

	private void MoveWithinSubgroups(object item, CollectionViewGroupInternal group, int level, IList list, int oldIndex, int newIndex)
	{
		object groupName = GetGroupName(item, group.GroupBy, level);
		if (groupName == UseAsItemDirectly)
		{
			MoveWithinSubgroup(item, group, list, oldIndex, newIndex);
			return;
		}
		if (!(groupName is ICollection collection))
		{
			MoveWithinSubgroup(item, group, level, groupName, list, oldIndex, newIndex);
			return;
		}
		foreach (object item2 in collection)
		{
			MoveWithinSubgroup(item, group, level, item2, list, oldIndex, newIndex);
		}
	}

	private void MoveWithinSubgroup(object item, CollectionViewGroupInternal group, int level, object name, IList list, int oldIndex, int newIndex)
	{
		object groupNameKey = GetGroupNameKey(name, group);
		CollectionViewGroupInternal subgroupFromMap;
		if ((subgroupFromMap = group.GetSubgroupFromMap(groupNameKey)) != null && group.GroupBy.NamesMatch(subgroupFromMap.Name, name))
		{
			MoveWithinSubgroups(item, subgroupFromMap, level + 1, list, oldIndex, newIndex);
			return;
		}
		int i = 0;
		for (int count = group.Items.Count; i < count; i++)
		{
			if (group.Items[i] is CollectionViewGroupInternal collectionViewGroupInternal && group.GroupBy.NamesMatch(collectionViewGroupInternal.Name, name))
			{
				group.AddSubgroupToMap(groupNameKey, collectionViewGroupInternal);
				MoveWithinSubgroups(item, collectionViewGroupInternal, level + 1, list, oldIndex, newIndex);
				break;
			}
		}
	}

	private void MoveWithinSubgroup(object item, CollectionViewGroupInternal group, IList list, int oldIndex, int newIndex)
	{
		if (group.Move(item, list, ref oldIndex, ref newIndex))
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
		}
	}

	private object GetGroupNameKey(object name, CollectionViewGroupInternal group)
	{
		object result = name;
		if (group.GroupBy is PropertyGroupDescription propertyGroupDescription)
		{
			string text = name as string;
			if (text != null)
			{
				if (propertyGroupDescription.StringComparison == StringComparison.OrdinalIgnoreCase || propertyGroupDescription.StringComparison == StringComparison.InvariantCultureIgnoreCase)
				{
					text = text.ToUpperInvariant();
				}
				else if (propertyGroupDescription.StringComparison == StringComparison.CurrentCultureIgnoreCase)
				{
					text = text.ToUpper(CultureInfo.CurrentCulture);
				}
				result = text;
			}
		}
		return result;
	}

	private bool RemoveFromSubgroups(object item, CollectionViewGroupInternal group, int level)
	{
		bool result = false;
		object groupName = GetGroupName(item, group.GroupBy, level);
		if (groupName == UseAsItemDirectly)
		{
			result = RemoveFromGroupDirectly(group, item);
		}
		else if (!(groupName is ICollection collection))
		{
			if (RemoveFromSubgroup(item, group, level, groupName))
			{
				result = true;
			}
		}
		else
		{
			foreach (object item2 in collection)
			{
				if (RemoveFromSubgroup(item, group, level, item2))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private bool RemoveFromSubgroup(object item, CollectionViewGroupInternal group, int level, object name)
	{
		object groupNameKey = GetGroupNameKey(name, group);
		CollectionViewGroupInternal subgroupFromMap;
		if ((subgroupFromMap = group.GetSubgroupFromMap(groupNameKey)) != null && group.GroupBy.NamesMatch(subgroupFromMap.Name, name))
		{
			return RemoveFromSubgroups(item, subgroupFromMap, level + 1);
		}
		int i = 0;
		for (int count = group.Items.Count; i < count; i++)
		{
			if (group.Items[i] is CollectionViewGroupInternal collectionViewGroupInternal && group.GroupBy.NamesMatch(collectionViewGroupInternal.Name, name))
			{
				return RemoveFromSubgroups(item, collectionViewGroupInternal, level + 1);
			}
		}
		return true;
	}

	private bool RemoveFromGroupDirectly(CollectionViewGroupInternal group, object item)
	{
		int num = group.Remove(item, returnLeafIndex: true);
		if (num >= 0)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, num));
			return false;
		}
		return true;
	}

	private void RemoveItemFromSubgroupsByExhaustiveSearch(CollectionViewGroupInternal group, object item)
	{
		if (!RemoveFromGroupDirectly(group, item))
		{
			return;
		}
		for (int num = group.Items.Count - 1; num >= 0; num--)
		{
			if (group.Items[num] is CollectionViewGroupInternal group2)
			{
				RemoveItemFromSubgroupsByExhaustiveSearch(group2, item);
			}
		}
	}

	private object GetGroupName(object item, GroupDescription groupDescription, int level)
	{
		if (groupDescription != null)
		{
			return groupDescription.GroupNameFromItem(item, level, Culture);
		}
		return UseAsItemDirectly;
	}
}
