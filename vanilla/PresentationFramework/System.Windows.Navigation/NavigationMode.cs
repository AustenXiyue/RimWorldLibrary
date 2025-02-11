namespace System.Windows.Navigation;

/// <summary>Specifies the type of navigation that is taking place <see cref="T:System.Windows.Navigation.NavigationMode" /> is used by the <see cref="P:System.Windows.Navigation.NavigatingCancelEventArgs.NavigationMode" /> property.</summary>
public enum NavigationMode : byte
{
	/// <summary>Navigating to new content. This occurs when the Navigate method is called, or when Source property is set.</summary>
	New,
	/// <summary>Navigating back to the most recent content in back navigation history. This occurs when the GoBack method is called.</summary>
	Back,
	/// <summary>Navigating to the most recent content on forward navigation history. This occurs when the GoForward method is called.</summary>
	Forward,
	/// <summary>Reloading the current content. This occurs when the Refresh method is called.</summary>
	Refresh
}
