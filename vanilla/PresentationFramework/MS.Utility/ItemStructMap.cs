using System;

namespace MS.Utility;

internal struct ItemStructMap<T>
{
	public struct Entry
	{
		public int Key;

		public T Value;
	}

	private const int SearchTypeThreshold = 4;

	public Entry[] Entries;

	public int Count;

	private static Entry EmptyEntry;

	public int EnsureEntry(int key)
	{
		int num = Search(key);
		if (num < 0)
		{
			if (Entries == null)
			{
				Entries = new Entry[4];
			}
			num = ~num;
			Entry[] array = Entries;
			if (Count + 1 > Entries.Length)
			{
				array = new Entry[Entries.Length * 2];
				Array.Copy(Entries, 0, array, 0, num);
			}
			Array.Copy(Entries, num, array, num + 1, Count - num);
			Entries = array;
			Entries[num] = EmptyEntry;
			Entries[num].Key = key;
			Count++;
		}
		return num;
	}

	public int Search(int key)
	{
		int num = int.MaxValue;
		int num2 = 0;
		if (Count > 4)
		{
			int num3 = 0;
			int num4 = Count - 1;
			while (num3 <= num4)
			{
				num2 = (num4 + num3) / 2;
				num = Entries[num2].Key;
				if (key == num)
				{
					return num2;
				}
				if (key < num)
				{
					num4 = num2 - 1;
				}
				else
				{
					num3 = num2 + 1;
				}
			}
		}
		else
		{
			for (int i = 0; i < Count; i++)
			{
				num2 = i;
				num = Entries[num2].Key;
				if (key == num)
				{
					return num2;
				}
				if (key < num)
				{
					break;
				}
			}
		}
		if (key > num)
		{
			num2++;
		}
		return ~num2;
	}
}
