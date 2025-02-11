using System;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace MS.Internal.Data;

internal class LiveShapingTree : RBTree<LiveShapingItem>
{
	private LiveShapingList _list;

	private LiveShapingBlock _placeholderBlock;

	internal LiveShapingList List => _list;

	internal LiveShapingBlock PlaceholderBlock
	{
		get
		{
			if (_placeholderBlock == null)
			{
				_placeholderBlock = new LiveShapingBlock(b: false);
				_placeholderBlock.Parent = this;
			}
			return _placeholderBlock;
		}
	}

	internal LiveShapingTree(LiveShapingList list)
	{
		_list = list;
	}

	internal override RBNode<LiveShapingItem> NewNode()
	{
		return new LiveShapingBlock();
	}

	internal void Move(int oldIndex, int newIndex)
	{
		LiveShapingItem item = base[oldIndex];
		RemoveAt(oldIndex);
		Insert(newIndex, item);
	}

	internal void RestoreLiveSortingByInsertionSort(Action<NotifyCollectionChangedEventArgs, int, int> RaiseMoveEvent)
	{
		RBFinger<LiveShapingItem> oldFinger = FindIndex(0);
		while (oldFinger.Node != this)
		{
			LiveShapingItem item = oldFinger.Item;
			item.IsSortDirty = false;
			item.IsSortPendingClean = false;
			RBFinger<LiveShapingItem> newFinger = LocateItem(oldFinger, base.Comparison);
			int index = oldFinger.Index;
			int index2 = newFinger.Index;
			if (index != index2)
			{
				ReInsert(ref oldFinger, newFinger);
				RaiseMoveEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item.Item, index, index2), index, index2);
			}
			++oldFinger;
		}
	}

	internal void FindPosition(LiveShapingItem lsi, out int oldIndex, out int newIndex)
	{
		lsi.FindPosition(out var oldFinger, out var newFinger, base.Comparison);
		oldIndex = oldFinger.Index;
		newIndex = newFinger.Index;
	}

	internal void ReplaceAt(int index, object item)
	{
		RBFinger<LiveShapingItem> rBFinger = FindIndex(index);
		rBFinger.Item.Clear();
		rBFinger.Node.SetItemAt(rBFinger.Offset, new LiveShapingItem(item, List));
	}

	internal LiveShapingItem FindItem(object item)
	{
		RBFinger<LiveShapingItem> rBFinger = FindIndex(0);
		while (rBFinger.Node != this)
		{
			if (ItemsControl.EqualsEx(rBFinger.Item.Item, item))
			{
				return rBFinger.Item;
			}
			++rBFinger;
		}
		return null;
	}

	public override int IndexOf(LiveShapingItem lsi)
	{
		RBFinger<LiveShapingItem> finger = lsi.GetFinger();
		if (!finger.Found)
		{
			return -1;
		}
		return finger.Index;
	}
}
