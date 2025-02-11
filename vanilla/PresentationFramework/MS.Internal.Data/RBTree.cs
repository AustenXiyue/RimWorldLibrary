using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MS.Internal.Data;

internal class RBTree<T> : RBNode<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private const int QuickSortThreshold = 15;

	private Comparison<T> _comparison;

	public override bool HasData => false;

	public Comparison<T> Comparison
	{
		get
		{
			return _comparison;
		}
		set
		{
			_comparison = value;
		}
	}

	public T this[int index]
	{
		get
		{
			VerifyIndex(index);
			RBFinger<T> rBFinger = FindIndex(index);
			return rBFinger.Node.GetItemAt(rBFinger.Offset);
		}
		set
		{
			VerifyIndex(index);
			RBFinger<T> rBFinger = FindIndex(index);
			rBFinger.Node.SetItemAt(rBFinger.Offset, value);
		}
	}

	public int Count => base.LeftSize;

	public bool IsReadOnly => false;

	public RBTree()
		: base(b: false)
	{
		base.Size = 64;
	}

	public RBFinger<T> BoundedSearch(T x, int low, int high)
	{
		return BoundedSearch(x, low, high, Comparison);
	}

	public void Insert(T x)
	{
		RBFinger<T> finger = Find(x, Comparison);
		Insert(finger, x, checkSort: true);
	}

	private void Insert(RBFinger<T> finger, T x, bool checkSort = false)
	{
		RBNode<T> node = finger.Node;
		if (node == this)
		{
			node = InsertNode(0);
			node.InsertAt(0, x);
		}
		else if (node.Size < 64)
		{
			node.InsertAt(finger.Offset, x);
		}
		else
		{
			RBNode<T> rBNode = node.GetSuccessor();
			RBNode<T> succsucc = null;
			if (rBNode.Size >= 64)
			{
				if (rBNode != this)
				{
					succsucc = rBNode;
				}
				rBNode = InsertNode(finger.Index + node.Size - finger.Offset);
			}
			node.InsertAt(finger.Offset, x, rBNode, succsucc);
		}
		base.LeftChild.IsRed = false;
	}

	public void Sort()
	{
		try
		{
			QuickSort();
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, innerException);
		}
	}

	public void QuickSort()
	{
		if (Count > 1)
		{
			RBFinger<T> low = FindIndex(0, exists: false);
			RBFinger<T> high = FindIndex(Count, exists: false);
			QuickSort3(low, high);
			InsertionSortImpl();
		}
	}

	public void InsertionSort()
	{
		if (Count > 1)
		{
			InsertionSortImpl();
		}
	}

	private void QuickSort3(RBFinger<T> low, RBFinger<T> high)
	{
		while (high - low > 15)
		{
			RBFinger<T> rBFinger = low;
			RBFinger<T> rBFinger2 = low + 1;
			RBFinger<T> rBFinger3 = high - 1;
			RBFinger<T> rBFinger4 = high;
			RBFinger<T> rBFinger5 = FindIndex((low.Index + high.Index) / 2);
			int num = Comparison(low.Item, rBFinger5.Item);
			if (num < 0)
			{
				num = Comparison(rBFinger5.Item, rBFinger3.Item);
				if (num >= 0)
				{
					if (num == 0)
					{
						rBFinger4 = rBFinger3;
					}
					else
					{
						num = Comparison(low.Item, rBFinger3.Item);
						if (num < 0)
						{
							Exchange(rBFinger5, rBFinger3);
						}
						else if (num == 0)
						{
							Exchange(rBFinger5, rBFinger3);
							rBFinger = rBFinger2;
						}
						else
						{
							Exchange(low, rBFinger5);
							Exchange(low, rBFinger3);
						}
					}
				}
			}
			else if (num == 0)
			{
				num = Comparison(low.Item, rBFinger3.Item);
				if (num < 0)
				{
					rBFinger = rBFinger2;
				}
				else if (num == 0)
				{
					rBFinger = rBFinger2;
					rBFinger4 = rBFinger3;
				}
				else
				{
					Exchange(low, rBFinger3);
					rBFinger4 = rBFinger3;
				}
			}
			else
			{
				num = Comparison(low.Item, rBFinger3.Item);
				if (num < 0)
				{
					Exchange(low, rBFinger5);
				}
				else if (num == 0)
				{
					Exchange(low, rBFinger5);
					rBFinger4 = rBFinger3;
				}
				else
				{
					num = Comparison(rBFinger5.Item, rBFinger3.Item);
					if (num < 0)
					{
						Exchange(low, rBFinger5);
						Exchange(rBFinger5, rBFinger3);
					}
					else if (num == 0)
					{
						Exchange(low, rBFinger3);
						rBFinger = rBFinger2;
					}
					else
					{
						Exchange(low, rBFinger3);
					}
				}
			}
			T item = rBFinger5.Item;
			RBFinger<T> rBFinger6 = rBFinger2;
			RBFinger<T> rBFinger7 = rBFinger3;
			while (true)
			{
				if (rBFinger6 < rBFinger7)
				{
					num = Comparison(rBFinger6.Item, item);
					if (num < 0)
					{
						Trade(rBFinger, rBFinger2, rBFinger6);
						rBFinger += rBFinger6 - rBFinger2;
						rBFinger2 = ++rBFinger6;
						continue;
					}
					if (num == 0)
					{
						++rBFinger6;
						continue;
					}
				}
				while (rBFinger6 < rBFinger7)
				{
					RBFinger<T> rBFinger8 = rBFinger7 - 1;
					num = Comparison(rBFinger8.Item, item);
					if (num < 0)
					{
						break;
					}
					if (num == 0)
					{
						--rBFinger7;
						continue;
					}
					Trade(rBFinger7, rBFinger3, rBFinger4);
					rBFinger4 -= rBFinger3 - rBFinger7;
					rBFinger3 = --rBFinger7;
				}
				switch (rBFinger7 - rBFinger6)
				{
				case 1:
					continue;
				case 0:
					Trade(low, rBFinger, rBFinger2);
					rBFinger2 += low - rBFinger;
					Trade(rBFinger3, rBFinger4, high);
					rBFinger3 += high - rBFinger4;
					break;
				case 2:
					Trade(low, rBFinger, rBFinger2);
					rBFinger2 += low - rBFinger + 1;
					Exchange(rBFinger2 - 1, rBFinger6 + 1);
					if (rBFinger2 > rBFinger6)
					{
						++rBFinger6;
					}
					Trade(rBFinger3, rBFinger4, high);
					rBFinger3 += high - rBFinger4 - 1;
					Exchange(rBFinger6, rBFinger3);
					break;
				default:
					Exchange(rBFinger6, rBFinger7 - 1);
					Trade(rBFinger, rBFinger2, rBFinger6);
					rBFinger += rBFinger6 - rBFinger2;
					rBFinger2 = ++rBFinger6;
					Trade(rBFinger7, rBFinger3, rBFinger4);
					rBFinger4 -= rBFinger3 - rBFinger7;
					rBFinger3 = --rBFinger7;
					continue;
				}
				break;
			}
			if (rBFinger2 - low < high - rBFinger3)
			{
				QuickSort3(low, rBFinger2);
				low = rBFinger3;
			}
			else
			{
				QuickSort3(rBFinger3, high);
				high = rBFinger2;
			}
		}
	}

	private void Trade(RBFinger<T> left, RBFinger<T> mid, RBFinger<T> right)
	{
		int num = Math.Min(mid - left, right - mid);
		for (int i = 0; i < num; i++)
		{
			--right;
			Exchange(left, right);
			++left;
		}
	}

	private void Exchange(RBFinger<T> f1, RBFinger<T> f2)
	{
		T item = f1.Item;
		f1.SetItem(f2.Item);
		f2.SetItem(item);
	}

	private void InsertionSortImpl()
	{
		RBFinger<T> oldFinger = FindIndex(1);
		while (oldFinger.Node != this)
		{
			RBFinger<T> newFinger = LocateItem(oldFinger, Comparison);
			ReInsert(ref oldFinger, newFinger);
			++oldFinger;
		}
	}

	internal RBNode<T> InsertNode(int index)
	{
		base.LeftChild = InsertNode(this, this, base.LeftChild, index, out var newNode);
		return newNode;
	}

	internal void RemoveNode(int index)
	{
		base.LeftChild = DeleteNode(this, base.LeftChild, index);
		if (base.LeftChild != null)
		{
			base.LeftChild.IsRed = false;
		}
	}

	internal virtual RBNode<T> NewNode()
	{
		return new RBNode<T>();
	}

	internal void ForEach(Action<T> action)
	{
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			action(current);
		}
	}

	internal void ForEachUntil(Func<T, bool> action)
	{
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (action(current))
			{
				break;
			}
		}
	}

	internal int IndexOf(T item, Func<T, T, bool> AreEqual)
	{
		if (Comparison != null)
		{
			RBFinger<T> rBFinger = Find(item, Comparison);
			while (rBFinger.Found && !AreEqual(rBFinger.Item, item))
			{
				++rBFinger;
				rBFinger.Found = rBFinger.IsValid && Comparison(rBFinger.Item, item) == 0;
			}
			if (!rBFinger.Found)
			{
				return -1;
			}
			return rBFinger.Index;
		}
		int result = 0;
		ForEachUntil(delegate(T x)
		{
			if (AreEqual(x, item))
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

	public virtual int IndexOf(T item)
	{
		return IndexOf(item, (T x, T y) => ItemsControl.EqualsEx(x, y));
	}

	public void Insert(int index, T item)
	{
		VerifyIndex(index, 1);
		RBFinger<T> finger = FindIndex(index, exists: false);
		Insert(finger, item);
	}

	public void RemoveAt(int index)
	{
		VerifyIndex(index);
		SaveTree();
		int leftSize = base.LeftSize;
		RBFinger<T> finger = FindIndex(index);
		RemoveAt(ref finger);
		if (base.LeftChild != null)
		{
			base.LeftChild.IsRed = false;
		}
		Verify(leftSize - 1);
	}

	public void Add(T item)
	{
		SaveTree();
		int leftSize = base.LeftSize;
		RBNode<T> rBNode = base.LeftChild;
		if (rBNode == null)
		{
			rBNode = InsertNode(0);
			rBNode.InsertAt(0, item);
		}
		else
		{
			while (rBNode.RightChild != null)
			{
				rBNode = rBNode.RightChild;
			}
			if (rBNode.Size < 64)
			{
				rBNode.InsertAt(rBNode.Size, item);
			}
			else
			{
				rBNode = InsertNode(base.LeftSize);
				rBNode.InsertAt(0, item);
			}
		}
		base.LeftChild.IsRed = false;
		Verify(leftSize + 1, checkSort: false);
	}

	public void Clear()
	{
		base.LeftChild = null;
		base.LeftSize = 0;
	}

	public bool Contains(T item)
	{
		return Find(item, Comparison).Found;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (arrayIndex + Count > array.Length)
		{
			throw new ArgumentException(SR.Argument_InvalidOffLen);
		}
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array[arrayIndex] = current;
			arrayIndex++;
		}
	}

	public bool Remove(T item)
	{
		RBFinger<T> finger = Find(item, Comparison);
		if (finger.Found)
		{
			RemoveAt(ref finger);
		}
		if (base.LeftChild != null)
		{
			base.LeftChild.IsRed = false;
		}
		return finger.Found;
	}

	public IEnumerator<T> GetEnumerator()
	{
		RBFinger<T> finger = FindIndex(0);
		while (finger.Node != this)
		{
			yield return finger.Node.GetItemAt(finger.Offset);
			RBFinger<T> rBFinger = finger;
			++rBFinger;
			finger = rBFinger;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		RBFinger<T> finger = FindIndex(0);
		while (finger.Node != this)
		{
			yield return finger.Node.GetItemAt(finger.Offset);
			RBFinger<T> rBFinger = finger;
			++rBFinger;
			finger = rBFinger;
		}
	}

	private void VerifyIndex(int index, int delta = 0)
	{
		if (index < 0 || index >= Count + delta)
		{
			throw new ArgumentOutOfRangeException("index");
		}
	}

	private void Verify(int expectedSize, bool checkSort = true)
	{
	}

	private void SaveTree()
	{
	}

	public void LoadTree(string s)
	{
	}
}
