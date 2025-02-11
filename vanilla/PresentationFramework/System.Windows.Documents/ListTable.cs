using System.Collections;

namespace System.Windows.Documents;

internal class ListTable : ArrayList
{
	internal ListTableEntry CurrentEntry
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

	internal ListTable()
		: base(20)
	{
	}

	internal ListTableEntry EntryAt(int index)
	{
		return (ListTableEntry)this[index];
	}

	internal ListTableEntry FindEntry(long id)
	{
		for (int i = 0; i < Count; i++)
		{
			ListTableEntry listTableEntry = EntryAt(i);
			if (listTableEntry.ID == id)
			{
				return listTableEntry;
			}
		}
		return null;
	}

	internal ListTableEntry AddEntry()
	{
		ListTableEntry listTableEntry = new ListTableEntry();
		Add(listTableEntry);
		return listTableEntry;
	}
}
