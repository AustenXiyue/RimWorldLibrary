using System.Collections;

namespace System.Windows.Documents;

internal class ListOverrideTable : ArrayList
{
	internal ListOverride CurrentEntry
	{
		get
		{
			if (Count <= 0)
			{
				return null;
			}
			return EntryAt(Count - 1);
		}
	}

	internal ListOverrideTable()
		: base(20)
	{
	}

	internal ListOverride EntryAt(int index)
	{
		return (ListOverride)this[index];
	}

	internal ListOverride FindEntry(int index)
	{
		for (int i = 0; i < Count; i++)
		{
			ListOverride listOverride = EntryAt(i);
			if (listOverride.Index == index)
			{
				return listOverride;
			}
		}
		return null;
	}

	internal ListOverride AddEntry()
	{
		ListOverride listOverride = new ListOverride();
		Add(listOverride);
		return listOverride;
	}
}
