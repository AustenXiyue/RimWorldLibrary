using System.Collections;

namespace System.Windows.Navigation;

internal class JournalEntryBackStack : JournalEntryStack
{
	public JournalEntryBackStack(Journal journal)
		: base(journal)
	{
	}

	public override IEnumerator GetEnumerator()
	{
		return new JournalEntryStackEnumerator(_journal, _journal.CurrentIndex - 1, -1, base.Filter);
	}
}
