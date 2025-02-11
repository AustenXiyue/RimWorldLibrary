using System;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal class JournalEntryGroupState
{
	private Guid _navigationServiceId;

	private uint _contentId;

	private DataStreams _journalDataStreams;

	private JournalEntry _groupExitEntry;

	internal Guid NavigationServiceId
	{
		get
		{
			return _navigationServiceId;
		}
		set
		{
			_navigationServiceId = value;
		}
	}

	internal uint ContentId
	{
		get
		{
			return _contentId;
		}
		set
		{
			_contentId = value;
		}
	}

	internal DataStreams JournalDataStreams
	{
		get
		{
			return _journalDataStreams;
		}
		set
		{
			_journalDataStreams = value;
		}
	}

	internal JournalEntry GroupExitEntry
	{
		get
		{
			return _groupExitEntry;
		}
		set
		{
			_groupExitEntry = value;
		}
	}

	internal JournalEntryGroupState()
	{
	}

	internal JournalEntryGroupState(Guid navSvcId, uint contentId)
	{
		_navigationServiceId = navSvcId;
		_contentId = contentId;
	}
}
