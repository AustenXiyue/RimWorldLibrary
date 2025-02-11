using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal interface IJournalNavigationScopeHost : INavigatorBase
{
	NavigationService NavigationService { get; }

	void VerifyContextAndObjectState();

	void OnJournalAvailable();

	bool GoBackOverride();

	bool GoForwardOverride();
}
