using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Navigation;

internal abstract class JournalEntryStack : IEnumerable, INotifyCollectionChanged
{
	private LimitedJournalEntryStackEnumerable _ljese;

	protected JournalEntryFilter _filter;

	protected Journal _journal;

	internal JournalEntryFilter Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	internal JournalEntryStack(Journal journal)
	{
		_journal = journal;
	}

	internal void OnCollectionChanged()
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}

	internal IEnumerable GetLimitedJournalEntryStackEnumerable()
	{
		if (_ljese == null)
		{
			_ljese = new LimitedJournalEntryStackEnumerable(this);
		}
		return _ljese;
	}

	public abstract IEnumerator GetEnumerator();
}
