using System.Collections;

namespace System.Windows.Navigation;

internal class JournalEntryStackEnumerator : IEnumerator
{
	private Journal _journal;

	private int _start;

	private int _delta;

	private int _next;

	private JournalEntry _current;

	private JournalEntryFilter _filter;

	private int _version;

	public object Current => _current;

	public JournalEntryStackEnumerator(Journal journal, int start, int delta, JournalEntryFilter filter)
	{
		_journal = journal;
		_version = journal.Version;
		_start = start;
		_delta = delta;
		_filter = filter;
		Reset();
	}

	public void Reset()
	{
		_next = _start;
		_current = null;
	}

	public bool MoveNext()
	{
		VerifyUnchanged();
		while (_next >= 0 && _next < _journal.TotalCount)
		{
			_current = _journal[_next];
			_next += _delta;
			if (_filter == null || _filter(_current))
			{
				return true;
			}
		}
		_current = null;
		return false;
	}

	protected void VerifyUnchanged()
	{
		if (_version != _journal.Version)
		{
			throw new InvalidOperationException(SR.EnumeratorVersionChanged);
		}
	}
}
