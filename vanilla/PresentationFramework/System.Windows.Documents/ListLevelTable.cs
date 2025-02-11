using System.Collections;

namespace System.Windows.Documents;

internal class ListLevelTable : ArrayList
{
	internal ListLevel CurrentEntry
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

	internal ListLevelTable()
		: base(1)
	{
	}

	internal ListLevel EntryAt(int index)
	{
		if (index > Count)
		{
			index = Count - 1;
		}
		return (ListLevel)((Count > index && index >= 0) ? this[index] : null);
	}

	internal ListLevel AddEntry()
	{
		ListLevel listLevel = new ListLevel();
		Add(listLevel);
		return listLevel;
	}
}
