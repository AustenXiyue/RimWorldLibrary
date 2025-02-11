using System;
using System.Collections;
using System.Windows;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal class TableTextElementCollectionInternal<TParent, TElementType> : ContentElementCollection<TParent, TElementType> where TParent : TextElement, IAcceptInsertion where TElementType : TextElement, IIndexedChild<TParent>
{
	public override TElementType this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
			}
			return base.Items[index];
		}
		set
		{
			if (index < 0 || index >= base.Size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Parent != null)
			{
				throw new ArgumentException(SR.TableCollectionInOtherCollection);
			}
			RemoveAt(index);
			Insert(index, value);
		}
	}

	internal TableTextElementCollectionInternal(TParent owner)
		: base(owner)
	{
	}

	public override void Add(TElementType item)
	{
		base.Version++;
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.Parent != null)
		{
			throw new ArgumentException(SR.TableCollectionInOtherCollection);
		}
		base.Owner.InsertionIndex = base.Size;
		item.RepositionWithContent(base.Owner.ContentEnd);
		base.Owner.InsertionIndex = -1;
	}

	public override void Clear()
	{
		base.Version++;
		for (int num = base.Size - 1; num >= 0; num--)
		{
			Remove(base.Items[num]);
		}
		base.Size = 0;
	}

	public override void Insert(int index, TElementType item)
	{
		base.Version++;
		if (index < 0 || index > base.Size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.Parent != null)
		{
			throw new ArgumentException(SR.TableCollectionInOtherCollection);
		}
		base.Owner.InsertionIndex = index;
		if (index == base.Size)
		{
			item.RepositionWithContent(base.Owner.ContentEnd);
		}
		else
		{
			TextPointer textPosition = new TextPointer(base.Items[index].ContentStart, -1);
			item.RepositionWithContent(textPosition);
		}
		base.Owner.InsertionIndex = -1;
	}

	public override bool Remove(TElementType item)
	{
		base.Version++;
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (!BelongsToOwner(item))
		{
			return false;
		}
		TextPointer startPosition = new TextPointer(item.TextContainer, item.TextElementNode, ElementEdge.BeforeStart, LogicalDirection.Backward);
		TextPointer endPosition = new TextPointer(item.TextContainer, item.TextElementNode, ElementEdge.AfterEnd, LogicalDirection.Backward);
		base.Owner.TextContainer.BeginChange();
		try
		{
			base.Owner.TextContainer.DeleteContentInternal(startPosition, endPosition);
		}
		finally
		{
			base.Owner.TextContainer.EndChange();
		}
		return true;
	}

	public override void RemoveAt(int index)
	{
		base.Version++;
		if (index < 0 || index >= base.Size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		Remove(base.Items[index]);
	}

	public override void RemoveRange(int index, int count)
	{
		base.Version++;
		if (index < 0 || index >= base.Size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionCountNeedNonNegNum);
		}
		if (base.Size - index < count)
		{
			throw new ArgumentException(SR.TableCollectionRangeOutOfRange);
		}
		if (count > 0)
		{
			for (int num = index + count - 1; num >= index; num--)
			{
				Remove(base.Items[num]);
			}
		}
	}

	internal override void PrivateConnectChild(int index, TElementType item)
	{
		if (item.Parent is DummyProxy && LogicalTreeHelper.GetParent(item.Parent) != base.Owner)
		{
			throw new ArgumentException(SR.TableCollectionWrongProxyParent);
		}
		base.Items[index] = item;
		item.Index = index;
		item.OnEnterParentTree();
	}

	internal override void PrivateDisconnectChild(TElementType item)
	{
		int index = item.Index;
		item.OnExitParentTree();
		base.Items[item.Index] = null;
		item.Index = -1;
		int size = base.Size - 1;
		base.Size = size;
		for (int i = index; i < base.Size; i++)
		{
			base.Items[i] = base.Items[i + 1];
			base.Items[i].Index = i;
		}
		base.Items[base.Size] = null;
		item.OnAfterExitParentTree(base.Owner);
	}

	internal int FindInsertionIndex(TElementType item)
	{
		int num = 0;
		object obj = item;
		if (item.Parent is DummyProxy)
		{
			obj = item.Parent;
		}
		IEnumerator enumerator = (base.Owner.IsEmpty ? new RangeContentEnumerator(null, null) : new RangeContentEnumerator(base.Owner.ContentStart, base.Owner.ContentEnd));
		while (enumerator.MoveNext())
		{
			if (obj == enumerator.Current)
			{
				return num;
			}
			if (enumerator.Current is TElementType || enumerator.Current is DummyProxy)
			{
				num++;
			}
		}
		Invariant.Assert(condition: false);
		return -1;
	}

	internal void InternalAdd(TElementType item)
	{
		if (base.Size == base.Items.Length)
		{
			EnsureCapacity(base.Size + 1);
		}
		int num = base.Owner.InsertionIndex;
		if (num == -1)
		{
			num = FindInsertionIndex(item);
		}
		for (int num2 = base.Size - 1; num2 >= num; num2--)
		{
			base.Items[num2 + 1] = base.Items[num2];
			base.Items[num2].Index = num2 + 1;
		}
		base.Items[num] = null;
		base.Size++;
		PrivateConnectChild(num, item);
	}

	internal void InternalRemove(TElementType item)
	{
		PrivateDisconnectChild(item);
	}
}
