namespace System.Windows.Navigation;

/// <summary>Provides data for the FragmentNavigation event.</summary>
public class FragmentNavigationEventArgs : EventArgs
{
	private string _fragment;

	private bool _handled;

	private object _navigator;

	/// <summary>Gets the uniform resource identifier (URI) fragment.</summary>
	/// <returns>The URI fragment. If you set the property to an empty string, the top of the content will be navigated to by default.</returns>
	public string Fragment => _fragment;

	/// <summary>Gets or sets a value that indicates whether the fragment navigation has been handled. </summary>
	/// <returns>true if the navigation has been handled; otherwise, false.</returns>
	public bool Handled
	{
		get
		{
			return _handled;
		}
		set
		{
			_handled = value;
		}
	}

	/// <summary>The navigator that raised the <see cref="E:System.Windows.Navigation.NavigationService.FragmentNavigation" /> event.</summary>
	/// <returns>A <see cref="T:System.Object" /> that refers to the navigator (Internet Explorer, <see cref="T:System.Windows.Navigation.NavigationWindow" />, <see cref="T:System.Windows.Controls.Frame" />.) that is navigating to the content fragment.</returns>
	public object Navigator => _navigator;

	internal FragmentNavigationEventArgs(string fragment, object Navigator)
	{
		_fragment = fragment;
		_navigator = Navigator;
	}
}
