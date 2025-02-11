using System.Collections;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal interface INavigator : INavigatorBase
{
	bool CanGoForward { get; }

	bool CanGoBack { get; }

	IEnumerable BackStack { get; }

	IEnumerable ForwardStack { get; }

	JournalNavigationScope GetJournal(bool create);

	void GoForward();

	void GoBack();

	void AddBackEntry(CustomContentState state);

	JournalEntry RemoveBackEntry();
}
