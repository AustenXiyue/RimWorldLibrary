using System.Collections.Generic;
using System.Runtime.Serialization;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Navigation;

[Serializable]
internal sealed class Journal : ISerializable
{
	[NonSerialized]
	private EventHandler _backForwardStateChange;

	private JournalEntryFilter _filter;

	private JournalEntryBackStack _backStack;

	private JournalEntryForwardStack _forwardStack;

	private int _journalEntryId = SafeNativeMethods.GetTickCount();

	private List<JournalEntry> _journalEntryList = new List<JournalEntry>();

	private int _currentEntryIndex;

	private int _uncommittedCurrentIndex;

	private int _version;

	internal JournalEntry this[int index] => _journalEntryList[index];

	internal int TotalCount => _journalEntryList.Count;

	internal int CurrentIndex => _currentEntryIndex;

	internal JournalEntry CurrentEntry
	{
		get
		{
			if (_currentEntryIndex >= 0 && _currentEntryIndex < TotalCount)
			{
				return _journalEntryList[_currentEntryIndex];
			}
			return null;
		}
	}

	internal bool HasUncommittedNavigation => _uncommittedCurrentIndex != _currentEntryIndex;

	internal JournalEntryStack BackStack => _backStack;

	internal JournalEntryStack ForwardStack => _forwardStack;

	internal bool CanGoBack => GetGoBackEntry() != null;

	internal bool CanGoForward
	{
		get
		{
			GetGoForwardEntryIndex(out var index);
			return index != -1;
		}
	}

	internal int Version => _version;

	internal JournalEntryFilter Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
			BackStack.Filter = _filter;
			ForwardStack.Filter = _filter;
		}
	}

	internal event EventHandler BackForwardStateChange
	{
		add
		{
			_backForwardStateChange = (EventHandler)Delegate.Combine(_backForwardStateChange, value);
		}
		remove
		{
			_backForwardStateChange = (EventHandler)Delegate.Remove(_backForwardStateChange, value);
		}
	}

	internal Journal()
	{
		_Initialize();
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("_journalEntryList", _journalEntryList);
		info.AddValue("_currentEntryIndex", _currentEntryIndex);
		info.AddValue("_journalEntryId", _journalEntryId);
	}

	private Journal(SerializationInfo info, StreamingContext context)
	{
		_Initialize();
		_journalEntryList = (List<JournalEntry>)info.GetValue("_journalEntryList", typeof(List<JournalEntry>));
		_currentEntryIndex = info.GetInt32("_currentEntryIndex");
		_uncommittedCurrentIndex = _currentEntryIndex;
		_journalEntryId = info.GetInt32("_journalEntryId");
	}

	internal JournalEntry RemoveBackEntry()
	{
		int num = _currentEntryIndex;
		do
		{
			if (--num < 0)
			{
				return null;
			}
		}
		while (!IsNavigable(_journalEntryList[num]));
		JournalEntry result = RemoveEntryInternal(num);
		UpdateView();
		return result;
	}

	internal void UpdateCurrentEntry(JournalEntry journalEntry)
	{
		if (journalEntry == null)
		{
			throw new ArgumentNullException("journalEntry");
		}
		if (_currentEntryIndex > -1 && _currentEntryIndex < TotalCount)
		{
			JournalEntry journalEntry2 = _journalEntryList[_currentEntryIndex];
			journalEntry.Id = journalEntry2.Id;
			_journalEntryList[_currentEntryIndex] = journalEntry;
		}
		else
		{
			journalEntry.Id = ++_journalEntryId;
			_journalEntryList.Add(journalEntry);
		}
		_version++;
		journalEntry.JEGroupState.GroupExitEntry = journalEntry;
	}

	internal void RecordNewNavigation()
	{
		Invariant.Assert(ValidateIndexes());
		_currentEntryIndex++;
		_uncommittedCurrentIndex = _currentEntryIndex;
		if (!ClearForwardStack())
		{
			UpdateView();
		}
	}

	internal bool ClearForwardStack()
	{
		if (_currentEntryIndex >= TotalCount)
		{
			return false;
		}
		if (_uncommittedCurrentIndex > _currentEntryIndex)
		{
			throw new InvalidOperationException(SR.InvalidOperation_CannotClearFwdStack);
		}
		_journalEntryList.RemoveRange(_currentEntryIndex, _journalEntryList.Count - _currentEntryIndex);
		UpdateView();
		return true;
	}

	internal void CommitJournalNavigation(JournalEntry navigated)
	{
		NavigateTo(navigated);
	}

	internal void AbortJournalNavigation()
	{
		_uncommittedCurrentIndex = _currentEntryIndex;
		UpdateView();
	}

	internal JournalEntry BeginBackNavigation()
	{
		Invariant.Assert(ValidateIndexes());
		int index;
		JournalEntry goBackEntry = GetGoBackEntry(out index);
		if (goBackEntry == null)
		{
			throw new InvalidOperationException(SR.NoBackEntry);
		}
		_uncommittedCurrentIndex = index;
		UpdateView();
		if (_uncommittedCurrentIndex == _currentEntryIndex)
		{
			return null;
		}
		return goBackEntry;
	}

	internal JournalEntry BeginForwardNavigation()
	{
		Invariant.Assert(ValidateIndexes());
		GetGoForwardEntryIndex(out var index);
		if (index == -1)
		{
			throw new InvalidOperationException(SR.NoForwardEntry);
		}
		_uncommittedCurrentIndex = index;
		UpdateView();
		if (index == _currentEntryIndex)
		{
			return null;
		}
		return _journalEntryList[index];
	}

	internal NavigationMode GetNavigationMode(JournalEntry entry)
	{
		if (_journalEntryList.IndexOf(entry) <= _currentEntryIndex)
		{
			return NavigationMode.Back;
		}
		return NavigationMode.Forward;
	}

	internal void NavigateTo(JournalEntry target)
	{
		int num = _journalEntryList.IndexOf(target);
		if (num > -1)
		{
			_currentEntryIndex = num;
			_uncommittedCurrentIndex = _currentEntryIndex;
			UpdateView();
		}
	}

	internal int FindIndexForEntryWithId(int id)
	{
		for (int i = 0; i < TotalCount; i++)
		{
			if (this[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	internal void PruneKeepAliveEntries()
	{
		for (int num = TotalCount - 1; num >= 0; num--)
		{
			JournalEntry journalEntry = _journalEntryList[num];
			if (journalEntry.IsAlive())
			{
				RemoveEntryInternal(num);
			}
			else
			{
				journalEntry.JEGroupState.JournalDataStreams?.PrepareForSerialization();
				if (journalEntry.RootViewerState != null)
				{
					journalEntry.RootViewerState.PrepareForSerialization();
				}
			}
		}
	}

	internal JournalEntry RemoveEntryInternal(int index)
	{
		JournalEntry result = _journalEntryList[index];
		_version++;
		_journalEntryList.RemoveAt(index);
		if (_currentEntryIndex > index)
		{
			_currentEntryIndex--;
		}
		if (_uncommittedCurrentIndex > index)
		{
			_uncommittedCurrentIndex--;
		}
		return result;
	}

	internal void RemoveEntries(Guid navSvcId)
	{
		for (int num = TotalCount - 1; num >= 0; num--)
		{
			if (num != _currentEntryIndex && _journalEntryList[num].NavigationServiceId == navSvcId)
			{
				RemoveEntryInternal(num);
			}
		}
		UpdateView();
	}

	internal void UpdateView()
	{
		BackStack.OnCollectionChanged();
		ForwardStack.OnCollectionChanged();
		if (_backForwardStateChange != null)
		{
			_backForwardStateChange(this, EventArgs.Empty);
		}
	}

	internal JournalEntry GetGoBackEntry(out int index)
	{
		for (index = _uncommittedCurrentIndex - 1; index >= 0; index--)
		{
			JournalEntry journalEntry = _journalEntryList[index];
			if (IsNavigable(journalEntry))
			{
				return journalEntry;
			}
		}
		return null;
	}

	internal JournalEntry GetGoBackEntry()
	{
		int index;
		return GetGoBackEntry(out index);
	}

	internal void GetGoForwardEntryIndex(out int index)
	{
		index = _uncommittedCurrentIndex;
		do
		{
			index++;
			if (index == _currentEntryIndex)
			{
				break;
			}
			if (index >= TotalCount)
			{
				index = -1;
				break;
			}
		}
		while (!IsNavigable(_journalEntryList[index]));
	}

	private bool ValidateIndexes()
	{
		if (_currentEntryIndex >= 0 && _currentEntryIndex <= TotalCount && _uncommittedCurrentIndex >= 0)
		{
			return _uncommittedCurrentIndex <= TotalCount;
		}
		return false;
	}

	private void _Initialize()
	{
		_backStack = new JournalEntryBackStack(this);
		_forwardStack = new JournalEntryForwardStack(this);
	}

	internal bool IsNavigable(JournalEntry entry)
	{
		if (entry == null)
		{
			return false;
		}
		if (Filter == null)
		{
			return entry.IsNavigable();
		}
		return Filter(entry);
	}
}
