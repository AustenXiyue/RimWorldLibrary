using System;
using System.Collections.Generic;

namespace MS.Internal.Data;

internal class LiveShapingBlock : RBNode<LiveShapingItem>
{
	private LiveShapingBlock ParentBlock => base.Parent as LiveShapingBlock;

	private LiveShapingBlock LeftChildBlock => (LiveShapingBlock)base.LeftChild;

	private LiveShapingBlock RightChildBlock => (LiveShapingBlock)base.RightChild;

	internal LiveShapingList List => ((LiveShapingTree)GetRoot(this)).List;

	internal LiveShapingBlock()
	{
	}

	internal LiveShapingBlock(bool b)
		: base(b)
	{
	}

	public override LiveShapingItem SetItemAt(int offset, LiveShapingItem lsi)
	{
		base.SetItemAt(offset, lsi);
		if (lsi != null)
		{
			lsi.Block = this;
		}
		return lsi;
	}

	protected override void Copy(RBNode<LiveShapingItem> sourceNode, int sourceOffset, RBNode<LiveShapingItem> destNode, int destOffset, int count)
	{
		base.Copy(sourceNode, sourceOffset, destNode, destOffset, count);
		if (sourceNode != destNode)
		{
			LiveShapingBlock block = (LiveShapingBlock)destNode;
			int num = 0;
			while (num < count)
			{
				destNode.GetItemAt(destOffset).Block = block;
				num++;
				destOffset++;
			}
		}
	}

	internal RBFinger<LiveShapingItem> GetFinger(LiveShapingItem lsi)
	{
		int num = OffsetOf(lsi);
		GetRootAndIndex(this, out var index);
		RBFinger<LiveShapingItem> result = default(RBFinger<LiveShapingItem>);
		result.Node = this;
		result.Offset = num;
		result.Index = index + num;
		result.Found = true;
		return result;
	}

	internal void FindPosition(LiveShapingItem item, out RBFinger<LiveShapingItem> oldFinger, out RBFinger<LiveShapingItem> newFinger, Comparison<LiveShapingItem> comparison)
	{
		int size = base.Size;
		int num = -1;
		int num2 = -1;
		int i;
		for (i = 0; i < size; i++)
		{
			LiveShapingItem itemAt = GetItemAt(i);
			if (item == itemAt)
			{
				break;
			}
			if (!itemAt.IsSortDirty)
			{
				num2 = i;
				if (num < 0)
				{
					num = i;
				}
			}
		}
		int j;
		for (j = i + 1; j < size; j++)
		{
			LiveShapingItem itemAt = GetItemAt(j);
			if (!itemAt.IsSortDirty)
			{
				break;
			}
		}
		int num3 = j;
		for (int num4 = size - 1; num4 > num3; num4--)
		{
			LiveShapingItem itemAt = GetItemAt(num4);
			if (!itemAt.IsSortDirty)
			{
				num3 = num4;
			}
		}
		GetRootAndIndex(this, out var index);
		oldFinger = new RBFinger<LiveShapingItem>
		{
			Node = this,
			Offset = i,
			Index = index + i,
			Found = true
		};
		LiveShapingItem liveShapingItem = ((num2 >= 0) ? GetItemAt(num2) : null);
		LiveShapingItem liveShapingItem2 = ((j < size) ? GetItemAt(j) : null);
		int num5 = 0;
		int num6 = 0;
		if (liveShapingItem != null && (num5 = comparison(item, liveShapingItem)) < 0)
		{
			if (num != num2)
			{
				num5 = comparison(item, GetItemAt(num));
			}
			if (num5 >= 0)
			{
				newFinger = LocalSearch(item, num + 1, num2, comparison);
			}
			else
			{
				newFinger = SearchLeft(item, num, comparison);
			}
		}
		else if (liveShapingItem2 != null && (num6 = comparison(item, liveShapingItem2)) > 0)
		{
			if (num3 != j)
			{
				num6 = comparison(item, GetItemAt(num3));
			}
			if (num6 <= 0)
			{
				newFinger = LocalSearch(item, j + 1, num3, comparison);
			}
			else
			{
				newFinger = SearchRight(item, num3 + 1, comparison);
			}
		}
		else if (liveShapingItem != null)
		{
			if (liveShapingItem2 != null)
			{
				newFinger = oldFinger;
			}
			else
			{
				newFinger = SearchRight(item, i, comparison);
			}
		}
		else if (liveShapingItem2 != null)
		{
			newFinger = SearchLeft(item, i, comparison);
		}
		else
		{
			newFinger = SearchLeft(item, i, comparison);
			if (newFinger.Node == this)
			{
				newFinger = SearchRight(item, i, comparison);
			}
		}
	}

	private RBFinger<LiveShapingItem> LocalSearch(LiveShapingItem item, int left, int right, Comparison<LiveShapingItem> comparison)
	{
		int num2;
		while (right - left > 3)
		{
			int num = (left + right) / 2;
			num2 = num;
			while (num2 >= left && GetItemAt(num2).IsSortDirty)
			{
				num2--;
			}
			if (num2 < left || comparison(GetItemAt(num2), item) <= 0)
			{
				left = num + 1;
			}
			else
			{
				right = num2;
			}
		}
		for (num2 = left; num2 < right && (GetItemAt(num2).IsSortDirty || comparison(item, GetItemAt(num2)) > 0); num2++)
		{
		}
		GetRootAndIndex(this, out var index);
		RBFinger<LiveShapingItem> result = default(RBFinger<LiveShapingItem>);
		result.Node = this;
		result.Offset = num2;
		result.Index = index + num2;
		return result;
	}

	private RBFinger<LiveShapingItem> SearchLeft(LiveShapingItem item, int offset, Comparison<LiveShapingItem> comparison)
	{
		LiveShapingBlock node = this;
		List<LiveShapingBlock> list = new List<LiveShapingBlock>();
		list.Add(LeftChildBlock);
		LiveShapingBlock liveShapingBlock = this;
		int first;
		int last;
		int size;
		for (LiveShapingBlock parentBlock = liveShapingBlock.ParentBlock; parentBlock != null; parentBlock = liveShapingBlock.ParentBlock)
		{
			if (parentBlock.RightChildBlock == liveShapingBlock)
			{
				parentBlock.GetFirstAndLastCleanItems(out first, out last, out size);
				if (first >= size)
				{
					list.Add(parentBlock.LeftChildBlock);
				}
				else
				{
					if (comparison(item, parentBlock.GetItemAt(last)) > 0)
					{
						break;
					}
					if (comparison(item, parentBlock.GetItemAt(first)) >= 0)
					{
						return parentBlock.LocalSearch(item, first + 1, last, comparison);
					}
					list.Clear();
					list.Add(parentBlock.LeftChildBlock);
					node = parentBlock;
					offset = first;
				}
			}
			liveShapingBlock = parentBlock;
		}
		Stack<LiveShapingBlock> stack = new Stack<LiveShapingBlock>(list);
		while (stack.Count > 0)
		{
			liveShapingBlock = stack.Pop();
			if (liveShapingBlock == null)
			{
				continue;
			}
			liveShapingBlock.GetFirstAndLastCleanItems(out first, out last, out size);
			if (first >= size)
			{
				stack.Push(liveShapingBlock.LeftChildBlock);
				stack.Push(liveShapingBlock.RightChildBlock);
				continue;
			}
			if (comparison(item, liveShapingBlock.GetItemAt(last)) > 0)
			{
				stack.Clear();
				stack.Push(liveShapingBlock.RightChildBlock);
				continue;
			}
			if (comparison(item, liveShapingBlock.GetItemAt(first)) >= 0)
			{
				return liveShapingBlock.LocalSearch(item, first + 1, last, comparison);
			}
			node = liveShapingBlock;
			offset = first;
			stack.Push(liveShapingBlock.LeftChildBlock);
		}
		GetRootAndIndex(node, out var index);
		RBFinger<LiveShapingItem> result = default(RBFinger<LiveShapingItem>);
		result.Node = node;
		result.Offset = offset;
		result.Index = index + offset;
		return result;
	}

	private RBFinger<LiveShapingItem> SearchRight(LiveShapingItem item, int offset, Comparison<LiveShapingItem> comparison)
	{
		LiveShapingBlock node = this;
		List<LiveShapingBlock> list = new List<LiveShapingBlock>();
		list.Add(RightChildBlock);
		LiveShapingBlock liveShapingBlock = this;
		int first;
		int last;
		int size;
		for (LiveShapingBlock parentBlock = liveShapingBlock.ParentBlock; parentBlock != null; parentBlock = liveShapingBlock.ParentBlock)
		{
			if (parentBlock.LeftChildBlock == liveShapingBlock)
			{
				parentBlock.GetFirstAndLastCleanItems(out first, out last, out size);
				if (first >= size)
				{
					list.Add(parentBlock.RightChildBlock);
				}
				else
				{
					if (comparison(item, parentBlock.GetItemAt(first)) < 0)
					{
						break;
					}
					if (comparison(item, parentBlock.GetItemAt(last)) <= 0)
					{
						return parentBlock.LocalSearch(item, first + 1, last, comparison);
					}
					list.Clear();
					list.Add(parentBlock.RightChildBlock);
					node = parentBlock;
					offset = last + 1;
				}
			}
			liveShapingBlock = parentBlock;
		}
		Stack<LiveShapingBlock> stack = new Stack<LiveShapingBlock>(list);
		while (stack.Count > 0)
		{
			liveShapingBlock = stack.Pop();
			if (liveShapingBlock == null)
			{
				continue;
			}
			liveShapingBlock.GetFirstAndLastCleanItems(out first, out last, out size);
			if (first >= size)
			{
				stack.Push(liveShapingBlock.RightChildBlock);
				stack.Push(liveShapingBlock.LeftChildBlock);
				continue;
			}
			if (comparison(item, liveShapingBlock.GetItemAt(first)) < 0)
			{
				stack.Clear();
				stack.Push(liveShapingBlock.LeftChildBlock);
				continue;
			}
			if (comparison(item, liveShapingBlock.GetItemAt(last)) <= 0)
			{
				return liveShapingBlock.LocalSearch(item, first + 1, last, comparison);
			}
			node = liveShapingBlock;
			offset = last + 1;
			stack.Push(liveShapingBlock.RightChildBlock);
		}
		GetRootAndIndex(node, out var index);
		RBFinger<LiveShapingItem> result = default(RBFinger<LiveShapingItem>);
		result.Node = node;
		result.Offset = offset;
		result.Index = index + offset;
		return result;
	}

	private void GetFirstAndLastCleanItems(out int first, out int last, out int size)
	{
		size = base.Size;
		first = 0;
		while (first < size && GetItemAt(first).IsSortDirty)
		{
			first++;
		}
		last = size - 1;
		while (last > first && GetItemAt(last).IsSortDirty)
		{
			last--;
		}
	}
}
