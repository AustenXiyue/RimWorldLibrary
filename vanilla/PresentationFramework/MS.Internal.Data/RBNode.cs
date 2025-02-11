using System;
using System.ComponentModel;

namespace MS.Internal.Data;

internal class RBNode<T> : INotifyPropertyChanged
{
	protected const int MaxSize = 64;

	protected const int BinarySearchThreshold = 3;

	private int _size;

	private int _leftSize;

	private T[] _data;

	public RBNode<T> LeftChild { get; set; }

	public RBNode<T> RightChild { get; set; }

	public RBNode<T> Parent { get; set; }

	public bool IsRed { get; set; }

	public virtual bool HasData => true;

	public int Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
			OnPropertyChanged("Size");
		}
	}

	public int LeftSize
	{
		get
		{
			return _leftSize;
		}
		set
		{
			_leftSize = value;
			OnPropertyChanged("LeftSize");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public RBNode()
	{
		_data = new T[64];
	}

	protected RBNode(bool b)
	{
	}

	public T GetItemAt(int offset)
	{
		return _data[offset];
	}

	public virtual T SetItemAt(int offset, T x)
	{
		_data[offset] = x;
		return x;
	}

	public int OffsetOf(T x)
	{
		return Array.IndexOf(_data, x);
	}

	internal RBNode<T> GetSuccessor()
	{
		RBNode<T> parent;
		if (RightChild == null)
		{
			RBNode<T> rBNode = this;
			parent = rBNode.Parent;
			while (parent.RightChild == rBNode)
			{
				rBNode = parent;
				parent = rBNode.Parent;
			}
			return parent;
		}
		parent = RightChild;
		for (RBNode<T> rBNode = parent.LeftChild; rBNode != null; rBNode = parent.LeftChild)
		{
			parent = rBNode;
		}
		return parent;
	}

	internal RBNode<T> GetPredecessor()
	{
		RBNode<T> parent;
		if (LeftChild == null)
		{
			RBNode<T> rBNode = this;
			parent = rBNode.Parent;
			while (parent != null && parent.LeftChild == rBNode)
			{
				rBNode = parent;
				parent = rBNode.Parent;
			}
			return parent;
		}
		parent = LeftChild;
		for (RBNode<T> rBNode = parent.RightChild; rBNode != null; rBNode = parent.RightChild)
		{
			parent = rBNode;
		}
		return parent;
	}

	protected RBFinger<T> FindIndex(int index, bool exists = true)
	{
		int num = (exists ? 1 : 0);
		RBFinger<T> result;
		if (index + num <= LeftSize)
		{
			if (LeftChild == null)
			{
				RBFinger<T> rBFinger = default(RBFinger<T>);
				rBFinger.Node = this;
				rBFinger.Offset = 0;
				rBFinger.Index = 0;
				rBFinger.Found = false;
				result = rBFinger;
			}
			else
			{
				result = LeftChild.FindIndex(index, exists);
			}
		}
		else if (index < LeftSize + Size)
		{
			RBFinger<T> rBFinger = default(RBFinger<T>);
			rBFinger.Node = this;
			rBFinger.Offset = index - LeftSize;
			rBFinger.Index = index;
			rBFinger.Found = true;
			result = rBFinger;
		}
		else if (RightChild == null)
		{
			RBFinger<T> rBFinger = default(RBFinger<T>);
			rBFinger.Node = this;
			rBFinger.Offset = Size;
			rBFinger.Index = LeftSize + Size;
			rBFinger.Found = false;
			result = rBFinger;
		}
		else
		{
			result = RightChild.FindIndex(index - LeftSize - Size, exists);
			result.Index += LeftSize + Size;
		}
		return result;
	}

	protected RBFinger<T> Find(T x, Comparison<T> comparison)
	{
		int num = ((_data != null) ? comparison(x, GetItemAt(0)) : (-1));
		RBFinger<T> result;
		int compHigh;
		if (num <= 0)
		{
			if (LeftChild == null)
			{
				RBFinger<T> rBFinger = default(RBFinger<T>);
				rBFinger.Node = this;
				rBFinger.Offset = 0;
				rBFinger.Index = 0;
				rBFinger.Found = num == 0;
				result = rBFinger;
			}
			else
			{
				result = LeftChild.Find(x, comparison);
				if (num == 0 && !result.Found)
				{
					RBFinger<T> rBFinger = default(RBFinger<T>);
					rBFinger.Node = this;
					rBFinger.Offset = 0;
					rBFinger.Index = LeftSize;
					rBFinger.Found = true;
					result = rBFinger;
				}
			}
		}
		else if ((compHigh = comparison(x, GetItemAt(Size - 1))) <= 0)
		{
			bool found;
			int num2 = BinarySearch(x, 1, Size - 1, comparison, compHigh, out found);
			RBFinger<T> rBFinger = default(RBFinger<T>);
			rBFinger.Node = this;
			rBFinger.Offset = num2;
			rBFinger.Index = LeftSize + num2;
			rBFinger.Found = found;
			result = rBFinger;
		}
		else if (RightChild == null)
		{
			RBFinger<T> rBFinger = default(RBFinger<T>);
			rBFinger.Node = this;
			rBFinger.Offset = Size;
			rBFinger.Index = LeftSize + Size;
			result = rBFinger;
		}
		else
		{
			result = RightChild.Find(x, comparison);
			result.Index += LeftSize + Size;
		}
		return result;
	}

	protected RBFinger<T> BoundedSearch(T x, int low, int high, Comparison<T> comparison)
	{
		RBNode<T> rBNode = LeftChild;
		RBNode<T> rBNode2 = RightChild;
		int num = 0;
		int num2 = Size;
		int num3;
		if (high <= LeftSize)
		{
			num3 = -1;
		}
		else
		{
			if (low >= LeftSize)
			{
				rBNode = null;
				num = low - LeftSize;
			}
			num3 = ((num >= Size) ? 1 : comparison(x, GetItemAt(num)));
		}
		RBFinger<T> rBFinger;
		if (num3 <= 0)
		{
			RBFinger<T> result;
			if (rBNode == null)
			{
				rBFinger = default(RBFinger<T>);
				rBFinger.Node = this;
				rBFinger.Offset = num;
				rBFinger.Index = num;
				rBFinger.Found = num3 == 0;
				result = rBFinger;
			}
			else
			{
				result = rBNode.BoundedSearch(x, low, high, comparison);
				if (num3 == 0 && !result.Found)
				{
					rBFinger = default(RBFinger<T>);
					rBFinger.Node = this;
					rBFinger.Offset = 0;
					rBFinger.Index = LeftSize;
					rBFinger.Found = true;
					result = rBFinger;
				}
			}
			return result;
		}
		int num4;
		if (LeftSize + Size <= low)
		{
			num4 = 1;
		}
		else
		{
			if (LeftSize + Size >= high)
			{
				rBNode2 = null;
				num2 = high - LeftSize;
			}
			num4 = comparison(x, GetItemAt(num2 - 1));
		}
		if (num4 > 0)
		{
			RBFinger<T> result;
			if (rBNode2 == null)
			{
				rBFinger = default(RBFinger<T>);
				rBFinger.Node = this;
				rBFinger.Offset = num2;
				rBFinger.Index = LeftSize + num2;
				rBFinger.Found = false;
				result = rBFinger;
			}
			else
			{
				int num5 = LeftSize + Size;
				result = rBNode2.BoundedSearch(x, low - num5, high - num5, comparison);
				result.Index += num5;
			}
			return result;
		}
		bool found;
		int num6 = BinarySearch(x, num + 1, num2 - 1, comparison, num4, out found);
		rBFinger = default(RBFinger<T>);
		rBFinger.Node = this;
		rBFinger.Offset = num6;
		rBFinger.Index = LeftSize + num6;
		rBFinger.Found = found;
		return rBFinger;
	}

	private int BinarySearch(T x, int low, int high, Comparison<T> comparison, int compHigh, out bool found)
	{
		while (high - low > 3)
		{
			int num = (high + low) / 2;
			int num2 = comparison(x, GetItemAt(num));
			if (num2 <= 0)
			{
				compHigh = num2;
				high = num;
			}
			else
			{
				low = num + 1;
			}
		}
		int num3 = 0;
		while (low < high)
		{
			num3 = comparison(x, GetItemAt(low));
			if (num3 <= 0)
			{
				break;
			}
			low++;
		}
		if (low == high)
		{
			num3 = compHigh;
		}
		found = num3 == 0;
		return low;
	}

	protected RBFinger<T> LocateItem(RBFinger<T> finger, Comparison<T> comparison)
	{
		RBNode<T> rBNode = finger.Node;
		int num = finger.Index - finger.Offset;
		T itemAt = rBNode.GetItemAt(finger.Offset);
		RBFinger<T> result;
		for (int num2 = finger.Offset - 1; num2 >= 0; num2--)
		{
			if (comparison(itemAt, rBNode.GetItemAt(num2)) >= 0)
			{
				result = default(RBFinger<T>);
				result.Node = rBNode;
				result.Offset = num2 + 1;
				result.Index = num + num2 + 1;
				return result;
			}
		}
		RBNode<T> rBNode2 = rBNode;
		for (RBNode<T> parent = rBNode2.Parent; parent != null; parent = rBNode2.Parent)
		{
			while (parent != null && rBNode2 == parent.LeftChild)
			{
				rBNode2 = parent;
				parent = rBNode2.Parent;
			}
			if (parent == null || comparison(itemAt, parent.GetItemAt(parent.Size - 1)) >= 0)
			{
				break;
			}
			num = num - rBNode.LeftSize - parent.Size;
			if (comparison(itemAt, parent.GetItemAt(0)) >= 0)
			{
				bool found;
				int num3 = parent.BinarySearch(itemAt, 1, parent.Size - 1, comparison, -1, out found);
				result = default(RBFinger<T>);
				result.Node = parent;
				result.Offset = num3;
				result.Index = num + num3;
				return result;
			}
			rBNode = (rBNode2 = parent);
		}
		if (rBNode.LeftChild != null)
		{
			RBFinger<T> result2 = rBNode.LeftChild.Find(itemAt, comparison);
			if (result2.Offset == result2.Node.Size)
			{
				result = default(RBFinger<T>);
				result.Node = result2.Node.GetSuccessor();
				result.Offset = 0;
				result.Index = result2.Index;
				result2 = result;
			}
			return result2;
		}
		result = default(RBFinger<T>);
		result.Node = rBNode;
		result.Offset = 0;
		result.Index = num;
		return result;
	}

	protected virtual void Copy(RBNode<T> sourceNode, int sourceOffset, RBNode<T> destNode, int destOffset, int count)
	{
		Array.Copy(sourceNode._data, sourceOffset, destNode._data, destOffset, count);
	}

	protected void ReInsert(ref RBFinger<T> oldFinger, RBFinger<T> newFinger)
	{
		RBNode<T> node = oldFinger.Node;
		RBNode<T> node2 = newFinger.Node;
		int offset = oldFinger.Offset;
		int offset2 = newFinger.Offset;
		T itemAt = node.GetItemAt(oldFinger.Offset);
		if (node == node2)
		{
			int num = offset - offset2;
			if (num != 0)
			{
				Copy(node, offset2, node, offset2 + 1, num);
				node.SetItemAt(offset2, itemAt);
			}
			return;
		}
		if (node2.Size < 64)
		{
			node2.InsertAt(offset2, itemAt);
			node.RemoveAt(ref oldFinger);
			return;
		}
		RBNode<T> successor = node2.GetSuccessor();
		if (successor == node)
		{
			T itemAt2 = node2.GetItemAt(63);
			Copy(node2, offset2, node2, offset2 + 1, 64 - offset2 - 1);
			node2.SetItemAt(offset2, itemAt);
			Copy(node, 0, node, 1, offset);
			node.SetItemAt(0, itemAt2);
			return;
		}
		if (successor.Size < 64)
		{
			node2.InsertAt(offset2, itemAt, successor);
		}
		else
		{
			RBNode<T> succsucc = successor;
			successor = InsertNodeAfter(node2);
			node2.InsertAt(offset2, itemAt, successor, succsucc);
		}
		node.RemoveAt(ref oldFinger);
	}

	protected void RemoveAt(ref RBFinger<T> finger)
	{
		RBNode<T> node = finger.Node;
		int offset = finger.Offset;
		Copy(node, offset + 1, node, offset, node.Size - offset - 1);
		node.ChangeSize(-1);
		node.SetItemAt(node.Size, default(T));
		if (node.Size == 0)
		{
			finger.Node = node.GetSuccessor();
			finger.Offset = 0;
			node.GetRootAndIndex(node, out var index).RemoveNode(index);
		}
		finger.Offset--;
	}

	protected RBNode<T> InsertNodeAfter(RBNode<T> node)
	{
		int index;
		return GetRootAndIndex(node, out index).InsertNode(index + node.Size);
	}

	protected RBTree<T> GetRoot(RBNode<T> node)
	{
		for (RBNode<T> parent = node.Parent; parent != null; parent = node.Parent)
		{
			node = parent;
		}
		return (RBTree<T>)node;
	}

	protected RBTree<T> GetRootAndIndex(RBNode<T> node, out int index)
	{
		index = node.LeftSize;
		for (RBNode<T> parent = node.Parent; parent != null; parent = node.Parent)
		{
			if (node == parent.RightChild)
			{
				index += parent.LeftSize + parent.Size;
			}
			node = parent;
		}
		return (RBTree<T>)node;
	}

	internal void InsertAt(int offset, T x, RBNode<T> successor = null, RBNode<T> succsucc = null)
	{
		if (Size < 64)
		{
			Copy(this, offset, this, offset + 1, Size - offset);
			SetItemAt(offset, x);
			ChangeSize(1);
		}
		else if (successor.Size == 0)
		{
			if (succsucc == null)
			{
				if (offset < 64)
				{
					successor.InsertAt(0, GetItemAt(63));
					Copy(this, offset, this, offset + 1, 64 - offset - 1);
					SetItemAt(offset, x);
				}
				else
				{
					successor.InsertAt(0, x);
				}
				return;
			}
			int num = 21;
			Copy(successor, 0, successor, num, successor.Size);
			Copy(this, 64 - num, successor, 0, num);
			Copy(succsucc, 0, successor, num + successor.Size, num);
			Copy(succsucc, num, succsucc, 0, 64 - num);
			if (offset <= 64 - num)
			{
				Copy(this, offset, this, offset + 1, 64 - num - offset);
				SetItemAt(offset, x);
				ChangeSize(1 - num);
				successor.ChangeSize(num + num);
			}
			else
			{
				Copy(successor, offset - (64 - num), successor, offset - (64 - num) + 1, successor.Size + num + num - (offset - (64 - num)));
				successor.SetItemAt(offset - (64 - num), x);
				ChangeSize(-num);
				successor.ChangeSize(num + num + 1);
			}
			succsucc.ChangeSize(-num);
		}
		else
		{
			int num2 = (Size + successor.Size + 1) / 2;
			if (offset < num2)
			{
				Copy(successor, 0, successor, 64 - num2 + 1, successor.Size);
				Copy(this, num2 - 1, successor, 0, 64 - num2 + 1);
				Copy(this, offset, this, offset + 1, num2 - 1 - offset);
				SetItemAt(offset, x);
			}
			else
			{
				Copy(successor, 0, successor, 64 - num2, successor.Size);
				Copy(this, num2, successor, 0, 64 - num2);
				Copy(successor, offset - num2, successor, offset - num2 + 1, successor.Size + 64 - offset);
				successor.SetItemAt(offset - num2, x);
			}
			ChangeSize(num2 - 64);
			successor.ChangeSize(64 - num2 + 1);
		}
	}

	protected RBNode<T> InsertNode(RBTree<T> root, RBNode<T> parent, RBNode<T> node, int index, out RBNode<T> newNode)
	{
		if (node == null)
		{
			newNode = root.NewNode();
			newNode.Parent = parent;
			newNode.IsRed = true;
			return newNode;
		}
		if (index <= node.LeftSize)
		{
			node.LeftChild = InsertNode(root, node, node.LeftChild, index, out newNode);
		}
		else
		{
			node.RightChild = InsertNode(root, node, node.RightChild, index - node.LeftSize - node.Size, out newNode);
		}
		node = Fixup(node);
		return node;
	}

	protected void ChangeSize(int delta)
	{
		if (delta == 0)
		{
			return;
		}
		for (int i = Size + delta; i < Size; i++)
		{
			_data[i] = default(T);
		}
		Size += delta;
		RBNode<T> rBNode = this;
		for (RBNode<T> parent = rBNode.Parent; parent != null; parent = rBNode.Parent)
		{
			if (parent.LeftChild == rBNode)
			{
				parent.LeftSize += delta;
			}
			rBNode = parent;
		}
	}

	private RBNode<T> Substitute(RBNode<T> node, RBNode<T> sub, RBNode<T> parent)
	{
		sub.LeftChild = node.LeftChild;
		sub.RightChild = node.RightChild;
		sub.LeftSize = node.LeftSize;
		sub.Parent = node.Parent;
		sub.IsRed = node.IsRed;
		if (sub.LeftChild != null)
		{
			sub.LeftChild.Parent = sub;
		}
		if (sub.RightChild != null)
		{
			sub.RightChild.Parent = sub;
		}
		return sub;
	}

	protected RBNode<T> DeleteNode(RBNode<T> parent, RBNode<T> node, int index)
	{
		if (index < node.LeftSize || (index == node.LeftSize && node.Size > 0))
		{
			if (!IsNodeRed(node.LeftChild) && !IsNodeRed(node.LeftChild.LeftChild))
			{
				node = MoveRedLeft(node);
			}
			node.LeftChild = DeleteNode(node, node.LeftChild, index);
		}
		else
		{
			bool flag = index == node.LeftSize;
			if (IsNodeRed(node.LeftChild))
			{
				node = node.RotateRight();
				flag = false;
			}
			if (flag && node.RightChild == null)
			{
				return null;
			}
			if (!IsNodeRed(node.RightChild) && !IsNodeRed(node.RightChild.LeftChild))
			{
				RBNode<T> rBNode = node;
				node = MoveRedRight(node);
				flag = flag && rBNode == node;
			}
			if (flag)
			{
				node.RightChild = DeleteLeftmost(node.RightChild, out var leftmost);
				node = Substitute(node, leftmost, parent);
			}
			else
			{
				node.RightChild = DeleteNode(node, node.RightChild, index - node.LeftSize - node.Size);
			}
		}
		return Fixup(node);
	}

	private RBNode<T> DeleteLeftmost(RBNode<T> node, out RBNode<T> leftmost)
	{
		if (node.LeftChild == null)
		{
			leftmost = node;
			return null;
		}
		if (!IsNodeRed(node.LeftChild) && !IsNodeRed(node.LeftChild.LeftChild))
		{
			node = MoveRedLeft(node);
		}
		node.LeftChild = DeleteLeftmost(node.LeftChild, out leftmost);
		node.LeftSize -= leftmost.Size;
		return Fixup(node);
	}

	private bool IsNodeRed(RBNode<T> node)
	{
		return node?.IsRed ?? false;
	}

	private RBNode<T> RotateLeft()
	{
		RBNode<T> rightChild = RightChild;
		rightChild.LeftSize += LeftSize + Size;
		rightChild.IsRed = IsRed;
		rightChild.Parent = Parent;
		RightChild = rightChild.LeftChild;
		if (RightChild != null)
		{
			RightChild.Parent = this;
		}
		rightChild.LeftChild = this;
		IsRed = true;
		Parent = rightChild;
		return rightChild;
	}

	private RBNode<T> RotateRight()
	{
		RBNode<T> leftChild = LeftChild;
		LeftSize -= leftChild.LeftSize + leftChild.Size;
		leftChild.IsRed = IsRed;
		leftChild.Parent = Parent;
		LeftChild = leftChild.RightChild;
		if (LeftChild != null)
		{
			LeftChild.Parent = this;
		}
		leftChild.RightChild = this;
		IsRed = true;
		Parent = leftChild;
		return leftChild;
	}

	private void ColorFlip()
	{
		IsRed = !IsRed;
		LeftChild.IsRed = !LeftChild.IsRed;
		RightChild.IsRed = !RightChild.IsRed;
	}

	private RBNode<T> Fixup(RBNode<T> node)
	{
		if (!IsNodeRed(node.LeftChild) && IsNodeRed(node.RightChild))
		{
			node = node.RotateLeft();
		}
		if (IsNodeRed(node.LeftChild) && IsNodeRed(node.LeftChild.LeftChild))
		{
			node = node.RotateRight();
		}
		if (IsNodeRed(node.LeftChild) && IsNodeRed(node.RightChild))
		{
			node.ColorFlip();
		}
		return node;
	}

	private RBNode<T> MoveRedRight(RBNode<T> node)
	{
		node.ColorFlip();
		if (IsNodeRed(node.LeftChild.LeftChild))
		{
			node = node.RotateRight();
			node.ColorFlip();
		}
		return node;
	}

	private RBNode<T> MoveRedLeft(RBNode<T> node)
	{
		node.ColorFlip();
		if (IsNodeRed(node.RightChild.LeftChild))
		{
			node.RightChild = node.RightChild.RotateRight();
			node = node.RotateLeft();
			node.ColorFlip();
		}
		return node;
	}

	protected void OnPropertyChanged(string name)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
