namespace System.Windows.Navigation;

internal class NavigateInfo
{
	private Uri _source;

	private NavigationMode _navigationMode;

	private JournalEntry _journalEntry;

	internal Uri Source => _source;

	internal NavigationMode NavigationMode => _navigationMode;

	internal JournalEntry JournalEntry => _journalEntry;

	internal bool IsConsistent
	{
		get
		{
			if (!((_navigationMode == NavigationMode.New) ^ (_journalEntry != null)))
			{
				return _navigationMode == NavigationMode.Refresh;
			}
			return true;
		}
	}

	internal NavigateInfo(Uri source)
	{
		_source = source;
	}

	internal NavigateInfo(Uri source, NavigationMode navigationMode)
	{
		_source = source;
		_navigationMode = navigationMode;
	}

	internal NavigateInfo(Uri source, NavigationMode navigationMode, JournalEntry journalEntry)
	{
		_source = source;
		_navigationMode = navigationMode;
		_journalEntry = journalEntry;
	}
}
