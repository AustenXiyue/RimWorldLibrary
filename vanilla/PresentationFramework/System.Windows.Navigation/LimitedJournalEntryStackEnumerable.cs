using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Navigation;

internal class LimitedJournalEntryStackEnumerable : IEnumerable, INotifyCollectionChanged
{
	private const uint DefaultMaxMenuEntries = 9u;

	private IEnumerable _ieble;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	internal LimitedJournalEntryStackEnumerable(IEnumerable ieble)
	{
		_ieble = ieble;
		if (ieble is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged += PropogateCollectionChanged;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return new LimitedJournalEntryStackEnumerator(_ieble, 9u);
	}

	internal void PropogateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, e);
		}
	}
}
