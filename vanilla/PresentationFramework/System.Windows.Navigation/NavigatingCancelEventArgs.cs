using System.ComponentModel;
using System.Net;

namespace System.Windows.Navigation;

/// <summary>Provides data for the Navigating event.</summary>
public class NavigatingCancelEventArgs : CancelEventArgs
{
	private Uri _uri;

	private object _content;

	private CustomContentState _targetContentState;

	private CustomContentState _contentStateToSave;

	private object _extraData;

	private NavigationMode _navigationMode;

	private WebRequest _webRequest;

	private bool _isNavInitiator;

	private object _navigator;

	/// <summary>Gets the uniform resource identifier (URI) for the content being navigated to.</summary>
	/// <returns>The <see cref="T:System.Uri" /> for the content being navigated to. If navigating to an object, <see cref="P:System.Windows.Navigation.NavigatingCancelEventArgs.Uri" /> is null.</returns>
	public Uri Uri => _uri;

	/// <summary>Gets a reference to the content object that is being navigated to.</summary>
	/// <returns>A <see cref="T:System.Object" /> reference to the content object that is being navigated to; otherwise, null.</returns>
	public object Content => _content;

	/// <summary>Gets the <see cref="T:System.Windows.Navigation.CustomContentState" /> object that is to be applied to the content being navigated to.</summary>
	/// <returns>The <see cref="T:System.Windows.Navigation.CustomContentState" /> object that is to be applied to the content being navigated to.</returns>
	public CustomContentState TargetContentState => _targetContentState;

	/// <summary>Gets and sets the <see cref="T:System.Windows.Navigation.CustomContentState" /> object that is associated with the back navigation history entry for the page being navigated from.</summary>
	/// <returns>The <see cref="T:System.Windows.Navigation.CustomContentState" /> object that is associated with the back navigation history entry for the page being navigated from.</returns>
	public CustomContentState ContentStateToSave
	{
		get
		{
			return _contentStateToSave;
		}
		set
		{
			_contentStateToSave = value;
		}
	}

	/// <summary>Gets the optional data <see cref="T:System.Object" /> that was passed when navigation started.</summary>
	/// <returns>The optional data <see cref="T:System.Object" /> that was passed when navigation started.</returns>
	public object ExtraData => _extraData;

	/// <summary>Gets a <see cref="T:System.Windows.Navigation.NavigationMode" /> value that indicates the type of navigation that is occurring.</summary>
	/// <returns>A <see cref="T:System.Windows.Navigation.NavigationMode" /> value that indicates the type of navigation that is occurring.</returns>
	public NavigationMode NavigationMode => _navigationMode;

	/// <summary>Gets the <see cref="T:System.Net.WebRequest" /> object that is used to request the specified content.</summary>
	/// <returns>Gets the <see cref="T:System.Net.WebRequest" /> object that is used to request the specified content. </returns>
	public WebRequest WebRequest => _webRequest;

	/// <summary>Indicates whether the navigator (<see cref="T:System.Windows.Navigation.NavigationWindow" />, <see cref="T:System.Windows.Controls.Frame" />) that is specified by <see cref="P:System.Windows.Navigation.NavigatingCancelEventArgs.Navigator" /> is servicing this navigation, or whether a parent navigator is doing so.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value that is true if the navigator that is specified by <see cref="P:System.Windows.Navigation.NavigatingCancelEventArgs.Navigator" /> is servicing this navigation. Otherwise, false is returned, such as during a nested <see cref="T:System.Windows.Controls.Frame" /> navigation.</returns>
	public bool IsNavigationInitiator => _isNavInitiator;

	/// <summary>The navigator that raised this event.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is the navigator that raised this event</returns>
	public object Navigator => _navigator;

	internal NavigatingCancelEventArgs(Uri uri, object content, CustomContentState customContentState, object extraData, NavigationMode navigationMode, WebRequest request, object Navigator, bool isNavInitiator)
	{
		_uri = uri;
		_content = content;
		_targetContentState = customContentState;
		_navigationMode = navigationMode;
		_extraData = extraData;
		_webRequest = request;
		_isNavInitiator = isNavInitiator;
		_navigator = Navigator;
	}
}
