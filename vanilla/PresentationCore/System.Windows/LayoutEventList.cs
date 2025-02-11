namespace System.Windows;

internal class LayoutEventList
{
	internal class ListItem : WeakReference
	{
		internal ListItem Next;

		internal ListItem Prev;

		internal bool InUse;

		internal ListItem()
			: base(null)
		{
		}
	}

	private const int PocketCapacity = 153;

	private ListItem _head;

	private ListItem _pocket;

	private int _pocketSize;

	private int _count;

	internal int Count => _count;

	internal LayoutEventList()
	{
		for (int i = 0; i < 153; i++)
		{
			_pocket = new ListItem
			{
				Next = _pocket
			};
		}
		_pocketSize = 153;
	}

	internal ListItem Add(object target)
	{
		ListItem newListItem = getNewListItem(target);
		newListItem.Next = _head;
		if (_head != null)
		{
			_head.Prev = newListItem;
		}
		_head = newListItem;
		_count++;
		return newListItem;
	}

	internal void Remove(ListItem t)
	{
		if (t.InUse)
		{
			if (t.Prev == null)
			{
				_head = t.Next;
			}
			else
			{
				t.Prev.Next = t.Next;
			}
			if (t.Next != null)
			{
				t.Next.Prev = t.Prev;
			}
			reuseListItem(t);
			_count--;
		}
	}

	private ListItem getNewListItem(object target)
	{
		ListItem listItem;
		if (_pocket != null)
		{
			listItem = _pocket;
			_pocket = listItem.Next;
			_pocketSize--;
			listItem.Next = (listItem.Prev = null);
		}
		else
		{
			listItem = new ListItem();
		}
		listItem.Target = target;
		listItem.InUse = true;
		return listItem;
	}

	private void reuseListItem(ListItem t)
	{
		t.Target = null;
		t.Next = (t.Prev = null);
		t.InUse = false;
		if (_pocketSize < 153)
		{
			t.Next = _pocket;
			_pocket = t;
			_pocketSize++;
		}
	}

	internal ListItem[] CopyToArray()
	{
		ListItem[] array = new ListItem[_count];
		ListItem listItem = _head;
		int num = 0;
		while (listItem != null)
		{
			array[num++] = listItem;
			listItem = listItem.Next;
		}
		return array;
	}
}
