using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Navigation;

internal class UnifiedJournalEntryStackEnumerable : IEnumerable, INotifyCollectionChanged
{
	private LimitedJournalEntryStackEnumerable _backStack;

	private LimitedJournalEntryStackEnumerable _forwardStack;

	private ArrayList _items;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	internal UnifiedJournalEntryStackEnumerable(LimitedJournalEntryStackEnumerable backStack, LimitedJournalEntryStackEnumerable forwardStack)
	{
		_backStack = backStack;
		_backStack.CollectionChanged += StacksChanged;
		_forwardStack = forwardStack;
		_forwardStack.CollectionChanged += StacksChanged;
	}

	public IEnumerator GetEnumerator()
	{
		if (_items == null)
		{
			_items = new ArrayList(19);
			foreach (JournalEntry item in _forwardStack)
			{
				_items.Insert(0, item);
				JournalEntryUnifiedViewConverter.SetJournalEntryPosition(item, JournalEntryPosition.Forward);
			}
			DependencyObject dependencyObject = new DependencyObject();
			dependencyObject.SetValue(JournalEntry.NameProperty, SR.NavWindowMenuCurrentPage);
			_items.Add(dependencyObject);
			foreach (JournalEntry item2 in _backStack)
			{
				_items.Add(item2);
				JournalEntryUnifiedViewConverter.SetJournalEntryPosition(item2, JournalEntryPosition.Back);
			}
		}
		return _items.GetEnumerator();
	}

	internal void StacksChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		_items = null;
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}
