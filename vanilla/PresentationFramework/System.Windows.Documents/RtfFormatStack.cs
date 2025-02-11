using System.Collections;
using MS.Internal;

namespace System.Windows.Documents;

internal class RtfFormatStack : ArrayList
{
	internal RtfFormatStack()
		: base(20)
	{
	}

	internal void Push()
	{
		FormatState formatState = Top();
		FormatState value = ((formatState != null) ? new FormatState(formatState) : new FormatState());
		Add(value);
	}

	internal void Pop()
	{
		Invariant.Assert(Count != 0);
		if (Count > 0)
		{
			RemoveAt(Count - 1);
		}
	}

	internal FormatState Top()
	{
		if (Count <= 0)
		{
			return null;
		}
		return EntryAt(Count - 1);
	}

	internal FormatState PrevTop(int fromTop)
	{
		int num = Count - 1 - fromTop;
		if (num < 0 || num >= Count)
		{
			return null;
		}
		return EntryAt(num);
	}

	internal FormatState EntryAt(int index)
	{
		return (FormatState)this[index];
	}
}
