using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using MS.Internal.KnownBoxes;

namespace MS.Internal.AppModel;

internal class JournalNavigationScope : DependencyObject, INavigator, INavigatorBase
{
	private static readonly DependencyPropertyKey CanGoBackPropertyKey = DependencyProperty.RegisterReadOnly("CanGoBack", typeof(bool), typeof(JournalNavigationScope), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

	internal static readonly DependencyProperty CanGoBackProperty = CanGoBackPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey CanGoForwardPropertyKey = DependencyProperty.RegisterReadOnly("CanGoForward", typeof(bool), typeof(JournalNavigationScope), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

	internal static readonly DependencyProperty CanGoForwardProperty = CanGoForwardPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey BackStackPropertyKey = DependencyProperty.RegisterReadOnly("BackStack", typeof(IEnumerable), typeof(JournalNavigationScope), new FrameworkPropertyMetadata((object)null));

	internal static readonly DependencyProperty BackStackProperty = BackStackPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey ForwardStackPropertyKey = DependencyProperty.RegisterReadOnly("ForwardStack", typeof(IEnumerable), typeof(JournalNavigationScope), new FrameworkPropertyMetadata((object)null));

	internal static readonly DependencyProperty ForwardStackProperty = ForwardStackPropertyKey.DependencyProperty;

	private IJournalNavigationScopeHost _host;

	private NavigationService _rootNavSvc;

	private Journal _journal;

	public Uri Source
	{
		get
		{
			return _host.Source;
		}
		set
		{
			_host.Source = value;
		}
	}

	public Uri CurrentSource => _host.CurrentSource;

	public object Content
	{
		get
		{
			return _host.Content;
		}
		set
		{
			_host.Content = value;
		}
	}

	public bool CanGoForward
	{
		get
		{
			_host.VerifyContextAndObjectState();
			if (_journal != null && !InAppShutdown)
			{
				return _journal.CanGoForward;
			}
			return false;
		}
	}

	public bool CanGoBack
	{
		get
		{
			_host.VerifyContextAndObjectState();
			if (_journal != null && !InAppShutdown)
			{
				return _journal.CanGoBack;
			}
			return false;
		}
	}

	public IEnumerable BackStack
	{
		get
		{
			_host.VerifyContextAndObjectState();
			return Journal.BackStack;
		}
	}

	public IEnumerable ForwardStack
	{
		get
		{
			_host.VerifyContextAndObjectState();
			return Journal.ForwardStack;
		}
	}

	internal Journal Journal
	{
		get
		{
			if (_journal == null)
			{
				Journal = new Journal();
			}
			return _journal;
		}
		set
		{
			_journal = value;
			_journal.Filter = IsEntryNavigable;
			_journal.BackForwardStateChange += OnBackForwardStateChange;
			DependencyObject obj = (DependencyObject)_host;
			obj.SetValue(BackStackPropertyKey, _journal.BackStack);
			obj.SetValue(ForwardStackPropertyKey, _journal.ForwardStack);
			_host.OnJournalAvailable();
		}
	}

	internal NavigationService RootNavigationService => _rootNavSvc;

	internal INavigatorBase NavigatorHost => _host;

	private bool InAppShutdown => Application.IsShuttingDown;

	public event NavigatingCancelEventHandler Navigating
	{
		add
		{
			_host.Navigating += value;
		}
		remove
		{
			_host.Navigating -= value;
		}
	}

	public event NavigationProgressEventHandler NavigationProgress
	{
		add
		{
			_host.NavigationProgress += value;
		}
		remove
		{
			_host.NavigationProgress -= value;
		}
	}

	public event NavigationFailedEventHandler NavigationFailed
	{
		add
		{
			_host.NavigationFailed += value;
		}
		remove
		{
			_host.NavigationFailed -= value;
		}
	}

	public event NavigatedEventHandler Navigated
	{
		add
		{
			_host.Navigated += value;
		}
		remove
		{
			_host.Navigated -= value;
		}
	}

	public event LoadCompletedEventHandler LoadCompleted
	{
		add
		{
			_host.LoadCompleted += value;
		}
		remove
		{
			_host.LoadCompleted -= value;
		}
	}

	public event NavigationStoppedEventHandler NavigationStopped
	{
		add
		{
			_host.NavigationStopped += value;
		}
		remove
		{
			_host.NavigationStopped -= value;
		}
	}

	public event FragmentNavigationEventHandler FragmentNavigation
	{
		add
		{
			_host.FragmentNavigation += value;
		}
		remove
		{
			_host.FragmentNavigation -= value;
		}
	}

	internal JournalNavigationScope(IJournalNavigationScopeHost host)
	{
		_host = host;
		_rootNavSvc = host.NavigationService;
	}

	public bool Navigate(Uri source)
	{
		return _host.Navigate(source);
	}

	public bool Navigate(Uri source, object extraData)
	{
		return _host.Navigate(source, extraData);
	}

	public bool Navigate(object content)
	{
		return _host.Navigate(content);
	}

	public bool Navigate(object content, object extraData)
	{
		return _host.Navigate(content, extraData);
	}

	public void StopLoading()
	{
		_host.StopLoading();
	}

	public void Refresh()
	{
		_host.Refresh();
	}

	public void GoForward()
	{
		if (!CanGoForward)
		{
			throw new InvalidOperationException(SR.NoForwardEntry);
		}
		if (!_host.GoForwardOverride())
		{
			JournalEntry journalEntry = Journal.BeginForwardNavigation();
			if (journalEntry == null)
			{
				_rootNavSvc.StopLoading();
			}
			else
			{
				NavigateToEntry(journalEntry);
			}
		}
	}

	public void GoBack()
	{
		if (!CanGoBack)
		{
			throw new InvalidOperationException(SR.NoBackEntry);
		}
		if (!_host.GoBackOverride())
		{
			JournalEntry journalEntry = Journal.BeginBackNavigation();
			if (journalEntry == null)
			{
				_rootNavSvc.StopLoading();
			}
			else
			{
				NavigateToEntry(journalEntry);
			}
		}
	}

	public void AddBackEntry(CustomContentState state)
	{
		_host.VerifyContextAndObjectState();
		_rootNavSvc.AddBackEntry(state);
	}

	public JournalEntry RemoveBackEntry()
	{
		_host.VerifyContextAndObjectState();
		if (_journal != null)
		{
			return _journal.RemoveBackEntry();
		}
		return null;
	}

	JournalNavigationScope INavigator.GetJournal(bool create)
	{
		return this;
	}

	internal void EnsureJournal()
	{
		_ = Journal;
	}

	internal bool CanInvokeJournalEntry(int entryId)
	{
		if (_journal == null)
		{
			return false;
		}
		int num = _journal.FindIndexForEntryWithId(entryId);
		if (num == -1)
		{
			return false;
		}
		JournalEntry entry = _journal[num];
		return _journal.IsNavigable(entry);
	}

	internal bool NavigateToEntry(int index)
	{
		JournalEntry entry = Journal[index];
		return NavigateToEntry(entry);
	}

	internal bool NavigateToEntry(JournalEntry entry)
	{
		if (entry == null)
		{
			return false;
		}
		if (!Journal.IsNavigable(entry))
		{
			return false;
		}
		NavigationService navigationService = _rootNavSvc.FindTarget(entry.NavigationServiceId);
		NavigationMode navigationMode = Journal.GetNavigationMode(entry);
		bool flag = false;
		try
		{
			flag = entry.Navigate(navigationService.INavigatorHost, navigationMode);
		}
		finally
		{
			if (!flag)
			{
				AbortJournalNavigation();
			}
		}
		return flag;
	}

	internal void AbortJournalNavigation()
	{
		if (_journal != null)
		{
			_journal.AbortJournalNavigation();
		}
	}

	internal INavigatorBase FindTarget(string name)
	{
		return _rootNavSvc.FindTarget(name);
	}

	internal static void ClearDPValues(DependencyObject navigator)
	{
		navigator.SetValue(CanGoBackPropertyKey, BooleanBoxes.FalseBox);
		navigator.SetValue(CanGoForwardPropertyKey, BooleanBoxes.FalseBox);
		navigator.SetValue(BackStackPropertyKey, null);
		navigator.SetValue(ForwardStackPropertyKey, null);
	}

	private void OnBackForwardStateChange(object sender, EventArgs e)
	{
		DependencyObject dependencyObject = (DependencyObject)_host;
		bool flag = false;
		bool canGoBack = _journal.CanGoBack;
		if (canGoBack != (bool)dependencyObject.GetValue(CanGoBackProperty))
		{
			dependencyObject.SetValue(CanGoBackPropertyKey, BooleanBoxes.Box(canGoBack));
			flag = true;
		}
		canGoBack = _journal.CanGoForward;
		if (canGoBack != (bool)dependencyObject.GetValue(CanGoForwardProperty))
		{
			dependencyObject.SetValue(CanGoForwardPropertyKey, BooleanBoxes.Box(canGoBack));
			flag = true;
		}
		if (flag)
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private bool IsEntryNavigable(JournalEntry entry)
	{
		if (entry == null || !entry.IsNavigable())
		{
			return false;
		}
		NavigationService navigationService = _rootNavSvc.FindTarget(entry.NavigationServiceId);
		if (navigationService != null)
		{
			if (navigationService.ContentId != entry.ContentId)
			{
				return entry.JEGroupState.GroupExitEntry == entry;
			}
			return true;
		}
		return false;
	}
}
