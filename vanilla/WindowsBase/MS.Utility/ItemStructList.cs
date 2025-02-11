using System;
using MS.Internal.WindowsBase;

namespace MS.Utility;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct ItemStructList<T>
{
	public T[] List;

	public int Count;

	public ItemStructList(int capacity)
	{
		List = new T[capacity];
		Count = 0;
	}

	public void EnsureIndex(int index)
	{
		int num = index + 1 - Count;
		if (num > 0)
		{
			Add(num);
		}
	}

	public bool IsValidIndex(int index)
	{
		if (index >= 0)
		{
			return index < Count;
		}
		return false;
	}

	public int IndexOf(T value)
	{
		int result = -1;
		for (int i = 0; i < Count; i++)
		{
			if (List[i].Equals(value))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public bool Contains(T value)
	{
		return IndexOf(value) != -1;
	}

	public void Add(T item)
	{
		int num = Add(1, incrCount: false);
		List[num] = item;
		Count++;
	}

	public void Add(ref T item)
	{
		int num = Add(1, incrCount: false);
		List[num] = item;
		Count++;
	}

	public int Add()
	{
		return Add(1, incrCount: true);
	}

	public int Add(int delta)
	{
		return Add(delta, incrCount: true);
	}

	private int Add(int delta, bool incrCount)
	{
		if (List != null)
		{
			if (Count + delta > List.Length)
			{
				T[] array = new T[Math.Max(List.Length * 2, Count + delta)];
				List.CopyTo(array, 0);
				List = array;
			}
		}
		else
		{
			List = new T[Math.Max(delta, 2)];
		}
		int count = Count;
		if (incrCount)
		{
			Count += delta;
		}
		return count;
	}

	public void Sort()
	{
		if (List != null)
		{
			Array.Sort(List, 0, Count);
		}
	}

	public void AppendTo(ref ItemStructList<T> destinationList)
	{
		for (int i = 0; i < Count; i++)
		{
			destinationList.Add(ref List[i]);
		}
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		Array.Copy(List, 0, array, 0, Count);
		return array;
	}

	public void Clear()
	{
		Array.Clear(List, 0, Count);
		Count = 0;
	}

	public void Remove(T value)
	{
		int num = IndexOf(value);
		if (num != -1)
		{
			Array.Copy(List, num + 1, List, num, Count - num - 1);
			Array.Clear(List, Count - 1, 1);
			Count--;
		}
	}
}
