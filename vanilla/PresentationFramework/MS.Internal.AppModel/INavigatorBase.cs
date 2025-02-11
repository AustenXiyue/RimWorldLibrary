using System;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal interface INavigatorBase
{
	Uri Source { get; set; }

	Uri CurrentSource { get; }

	object Content { get; set; }

	event NavigatingCancelEventHandler Navigating;

	event NavigationProgressEventHandler NavigationProgress;

	event NavigationFailedEventHandler NavigationFailed;

	event NavigatedEventHandler Navigated;

	event LoadCompletedEventHandler LoadCompleted;

	event NavigationStoppedEventHandler NavigationStopped;

	event FragmentNavigationEventHandler FragmentNavigation;

	bool Navigate(Uri source);

	bool Navigate(Uri source, object extraData);

	bool Navigate(object content);

	bool Navigate(object content, object extraData);

	void StopLoading();

	void Refresh();
}
