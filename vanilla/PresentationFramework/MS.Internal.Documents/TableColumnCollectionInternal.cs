using System;
using System.Windows;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal class TableColumnCollectionInternal : ContentElementCollection<Table, TableColumn>
{
	public override TableColumn this[int index]
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
			PrivateDisconnectChild(base.Items[index]);
			PrivateConnectChild(index, value);
		}
	}

	internal TableColumnCollectionInternal(Table owner)
		: base(owner)
	{
	}

	public override void Add(TableColumn item)
	{
		base.Version++;
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (base.Size == base.Items.Length)
		{
			EnsureCapacity(base.Size + 1);
		}
		PrivateConnectChild(base.Size++, item);
	}

	public override void Clear()
	{
		base.Version++;
		for (int i = 0; i < base.Size; i++)
		{
			PrivateDisconnectChild(base.Items[i]);
			base.Items[i] = null;
		}
		base.Size = 0;
	}

	public override void Insert(int index, TableColumn item)
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
		if (base.Size == base.Items.Length)
		{
			EnsureCapacity(base.Size + 1);
		}
		for (int num = base.Size - 1; num >= index; num--)
		{
			base.Items[num + 1] = base.Items[num];
			base.Items[num].Index = num + 1;
		}
		base.Items[index] = null;
		base.Size++;
		PrivateConnectChild(index, item);
	}

	internal override void PrivateConnectChild(int index, TableColumn item)
	{
		if (item.Parent is DummyProxy)
		{
			if (LogicalTreeHelper.GetParent(item.Parent) != base.Owner)
			{
				throw new ArgumentException(SR.TableCollectionWrongProxyParent);
			}
		}
		else
		{
			if (item.Parent != null)
			{
				throw new ArgumentException(SR.TableCollectionInOtherCollection);
			}
			base.Owner.AddLogicalChild(item);
		}
		base.Items[index] = item;
		item.Index = index;
		item.OnEnterParentTree();
	}

	internal override void PrivateDisconnectChild(TableColumn item)
	{
		item.OnExitParentTree();
		base.Items[item.Index] = null;
		item.Index = -1;
		if (!(item.Parent is DummyProxy))
		{
			base.Owner.RemoveLogicalChild(item);
		}
	}

	public override bool Remove(TableColumn item)
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
		PrivateRemove(item);
		return true;
	}

	public override void RemoveAt(int index)
	{
		base.Version++;
		if (index < 0 || index >= base.Size)
		{
			throw new ArgumentOutOfRangeException(SR.TableCollectionOutOfRange);
		}
		PrivateRemove(base.Items[index]);
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
				PrivateDisconnectChild(base.Items[num]);
			}
			base.Size -= count;
			for (int i = index; i < base.Size; i++)
			{
				base.Items[i] = base.Items[i + count];
				base.Items[i].Index = i;
				base.Items[i + count] = null;
			}
		}
	}
}
