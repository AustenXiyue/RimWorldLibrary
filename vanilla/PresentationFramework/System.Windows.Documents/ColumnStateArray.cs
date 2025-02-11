using System.Collections;

namespace System.Windows.Documents;

internal class ColumnStateArray : ArrayList
{
	internal ColumnStateArray()
		: base(20)
	{
	}

	internal ColumnState EntryAt(int i)
	{
		return (ColumnState)this[i];
	}

	internal int GetMinUnfilledRowIndex()
	{
		int num = -1;
		for (int i = 0; i < Count; i++)
		{
			ColumnState columnState = EntryAt(i);
			if (!columnState.IsFilled && (num < 0 || num > columnState.Row.Index) && !columnState.Row.FormatState.RowFormat.IsVMerge)
			{
				num = columnState.Row.Index;
			}
		}
		return num;
	}
}
