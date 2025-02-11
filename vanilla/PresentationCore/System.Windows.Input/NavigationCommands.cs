using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides a standard set of navigation-related commands.</summary>
public static class NavigationCommands
{
	private enum CommandId : byte
	{
		BrowseBack = 1,
		BrowseForward,
		BrowseHome,
		BrowseStop,
		Refresh,
		Favorites,
		Search,
		IncreaseZoom,
		DecreaseZoom,
		Zoom,
		NextPage,
		PreviousPage,
		FirstPage,
		LastPage,
		GoToPage,
		NavigateJournal,
		Last
	}

	private static RoutedUICommand[] _internalCommands = new RoutedUICommand[17];

	private const string BrowseBackKey = "Alt+Left;Backspace";

	private const string BrowseForwardKey = "Alt+Right;Shift+Backspace";

	private const string BrowseHomeKey = "Alt+Home;BrowserHome";

	private const string BrowseStopKey = "Alt+Esc;BrowserStop";

	private const string FavoritesKey = "Ctrl+I";

	private const string RefreshKey = "F5";

	private const string SearchKey = "F3";

	/// <summary>Gets the value that represents the Browse Back command. </summary>
	/// <returns>The routed UI command.Default ValuesKey GestureALT+LEFTUI TextBack</returns>
	public static RoutedUICommand BrowseBack => _EnsureCommand(CommandId.BrowseBack);

	/// <summary>Gets the value that represents the Browse Forward command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureALT+RIGHTUI TextForward</returns>
	public static RoutedUICommand BrowseForward => _EnsureCommand(CommandId.BrowseForward);

	/// <summary>Gets the value that represents the Browse Home command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureALT+HOMEUI TextHome</returns>
	public static RoutedUICommand BrowseHome => _EnsureCommand(CommandId.BrowseHome);

	/// <summary>Gets the value that represents the Browse Stop command. </summary>
	/// <returns>The routed UI command.Default ValuesKey GestureALT+ESCUI TextStop</returns>
	public static RoutedUICommand BrowseStop => _EnsureCommand(CommandId.BrowseStop);

	/// <summary>Gets the value that represents the Refresh command. </summary>
	/// <returns>The routed UI command.Default ValuesKey GestureF5UI TextRefresh</returns>
	public static RoutedUICommand Refresh => _EnsureCommand(CommandId.Refresh);

	/// <summary>Gets the value that represents the Favorites command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureCTRL+IUI TextFavorites</returns>
	public static RoutedUICommand Favorites => _EnsureCommand(CommandId.Favorites);

	/// <summary>Gets the value that represents the Search command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureF3UI TextSearch</returns>
	public static RoutedUICommand Search => _EnsureCommand(CommandId.Search);

	/// <summary>Gets the value that represents the Increase Zoom command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextIncrease Zoom</returns>
	public static RoutedUICommand IncreaseZoom => _EnsureCommand(CommandId.IncreaseZoom);

	/// <summary>Gets the value that represents the Decrease Zoom command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextDecrease Zoom</returns>
	public static RoutedUICommand DecreaseZoom => _EnsureCommand(CommandId.DecreaseZoom);

	/// <summary>Gets the value that represents the Zoom command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextZoom</returns>
	public static RoutedUICommand Zoom => _EnsureCommand(CommandId.Zoom);

	/// <summary>Gets the value that represents the Next Page command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextNext Page</returns>
	public static RoutedUICommand NextPage => _EnsureCommand(CommandId.NextPage);

	/// <summary>Gets the value that represents the Previous Page command. </summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextPrevious Page</returns>
	public static RoutedUICommand PreviousPage => _EnsureCommand(CommandId.PreviousPage);

	/// <summary>Gets the value that represents the First Page command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextFirst Page</returns>
	public static RoutedUICommand FirstPage => _EnsureCommand(CommandId.FirstPage);

	/// <summary>Gets the value that represents the Last Page command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextLast Page</returns>
	public static RoutedUICommand LastPage => _EnsureCommand(CommandId.LastPage);

	/// <summary>Gets the value that represents the Go To Page command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextGo To Page</returns>
	public static RoutedUICommand GoToPage => _EnsureCommand(CommandId.GoToPage);

	/// <summary>Gets the value that represents the Navigate Journal command.</summary>
	/// <returns>The routed UI command.Default ValuesKey GestureN/AUI TextNavigation Journal</returns>
	public static RoutedUICommand NavigateJournal => _EnsureCommand(CommandId.NavigateJournal);

	private static string GetPropertyName(CommandId commandId)
	{
		string result = string.Empty;
		switch (commandId)
		{
		case CommandId.BrowseBack:
			result = "BrowseBack";
			break;
		case CommandId.BrowseForward:
			result = "BrowseForward";
			break;
		case CommandId.BrowseHome:
			result = "BrowseHome";
			break;
		case CommandId.BrowseStop:
			result = "BrowseStop";
			break;
		case CommandId.Refresh:
			result = "Refresh";
			break;
		case CommandId.Favorites:
			result = "Favorites";
			break;
		case CommandId.Search:
			result = "Search";
			break;
		case CommandId.IncreaseZoom:
			result = "IncreaseZoom";
			break;
		case CommandId.DecreaseZoom:
			result = "DecreaseZoom";
			break;
		case CommandId.Zoom:
			result = "Zoom";
			break;
		case CommandId.NextPage:
			result = "NextPage";
			break;
		case CommandId.PreviousPage:
			result = "PreviousPage";
			break;
		case CommandId.FirstPage:
			result = "FirstPage";
			break;
		case CommandId.LastPage:
			result = "LastPage";
			break;
		case CommandId.GoToPage:
			result = "GoToPage";
			break;
		case CommandId.NavigateJournal:
			result = "NavigateJournal";
			break;
		}
		return result;
	}

	internal static string GetUIText(byte commandId)
	{
		string result = string.Empty;
		switch ((CommandId)commandId)
		{
		case CommandId.BrowseBack:
			result = SR.BrowseBackText;
			break;
		case CommandId.BrowseForward:
			result = SR.BrowseForwardText;
			break;
		case CommandId.BrowseHome:
			result = SR.BrowseHomeText;
			break;
		case CommandId.BrowseStop:
			result = SR.BrowseStopText;
			break;
		case CommandId.Refresh:
			result = SR.RefreshText;
			break;
		case CommandId.Favorites:
			result = SR.FavoritesText;
			break;
		case CommandId.Search:
			result = SR.SearchText;
			break;
		case CommandId.IncreaseZoom:
			result = SR.IncreaseZoomText;
			break;
		case CommandId.DecreaseZoom:
			result = SR.DecreaseZoomText;
			break;
		case CommandId.Zoom:
			result = SR.ZoomText;
			break;
		case CommandId.NextPage:
			result = SR.NextPageText;
			break;
		case CommandId.PreviousPage:
			result = SR.PreviousPageText;
			break;
		case CommandId.FirstPage:
			result = SR.FirstPageText;
			break;
		case CommandId.LastPage:
			result = SR.LastPageText;
			break;
		case CommandId.GoToPage:
			result = SR.GoToPageText;
			break;
		case CommandId.NavigateJournal:
			result = SR.NavigateJournalText;
			break;
		}
		return result;
	}

	internal static InputGestureCollection LoadDefaultGestureFromResource(byte commandId)
	{
		InputGestureCollection inputGestureCollection = new InputGestureCollection();
		switch ((CommandId)commandId)
		{
		case CommandId.BrowseBack:
			KeyGesture.AddGesturesFromResourceStrings("Alt+Left;Backspace", SR.BrowseBackKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.BrowseForward:
			KeyGesture.AddGesturesFromResourceStrings("Alt+Right;Shift+Backspace", SR.BrowseForwardKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.BrowseHome:
			KeyGesture.AddGesturesFromResourceStrings("Alt+Home;BrowserHome", SR.BrowseHomeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.BrowseStop:
			KeyGesture.AddGesturesFromResourceStrings("Alt+Esc;BrowserStop", SR.BrowseStopKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Refresh:
			KeyGesture.AddGesturesFromResourceStrings("F5", SR.RefreshKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Favorites:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+I", SR.FavoritesKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Search:
			KeyGesture.AddGesturesFromResourceStrings("F3", SR.SearchKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.IncreaseZoom:
			KeyGesture.AddGesturesFromResourceStrings(SR.IncreaseZoomKey, SR.IncreaseZoomKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.DecreaseZoom:
			KeyGesture.AddGesturesFromResourceStrings(SR.DecreaseZoomKey, SR.DecreaseZoomKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Zoom:
			KeyGesture.AddGesturesFromResourceStrings(SR.ZoomKey, SR.ZoomKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.NextPage:
			KeyGesture.AddGesturesFromResourceStrings(SR.NextPageKey, SR.NextPageKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.PreviousPage:
			KeyGesture.AddGesturesFromResourceStrings(SR.PreviousPageKey, SR.PreviousPageKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.FirstPage:
			KeyGesture.AddGesturesFromResourceStrings(SR.FirstPageKey, SR.FirstPageKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.LastPage:
			KeyGesture.AddGesturesFromResourceStrings(SR.LastPageKey, SR.LastPageKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.GoToPage:
			KeyGesture.AddGesturesFromResourceStrings(SR.GoToPageKey, SR.GoToPageKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.NavigateJournal:
			KeyGesture.AddGesturesFromResourceStrings(SR.NavigateJournalKey, SR.NavigateJournalKeyDisplayString, inputGestureCollection);
			break;
		}
		return inputGestureCollection;
	}

	private static RoutedUICommand _EnsureCommand(CommandId idCommand)
	{
		if ((int)idCommand >= 0 && (int)idCommand < 17)
		{
			lock (_internalCommands.SyncRoot)
			{
				if (_internalCommands[(uint)idCommand] == null)
				{
					RoutedUICommand routedUICommand = CommandLibraryHelper.CreateUICommand(GetPropertyName(idCommand), typeof(NavigationCommands), (byte)idCommand);
					_internalCommands[(uint)idCommand] = routedUICommand;
				}
			}
			return _internalCommands[(uint)idCommand];
		}
		return null;
	}
}
