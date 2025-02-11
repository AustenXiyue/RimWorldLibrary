using System.Net;

namespace System.Windows.Navigation;

/// <summary>Provides data for non-cancelable navigation events, including <see cref="E:System.Windows.Navigation.NavigationWindow.LoadCompleted" />, <see cref="E:System.Windows.Navigation.NavigationWindow.Navigated" />, and <see cref="E:System.Windows.Navigation.NavigationWindow.NavigationStopped" />.  </summary>
public class NavigationEventArgs : EventArgs
{
	private Uri _uri;

	private object _content;

	private object _extraData;

	private WebResponse _webResponse;

	private bool _isNavigationInitiator;

	private object _navigator;

	/// <summary>Gets the uniform resource identifier (URI) of the target page.</summary>
	/// <returns>The URI of the target page.</returns>
	public Uri Uri => _uri;

	/// <summary>Gets the root node of the target page's content. </summary>
	/// <returns>The root element of the target page's content.</returns>
	public object Content => _content;

	/// <summary>Gets a value that indicates whether the current navigator initiated the navigation.</summary>
	/// <returns>true if the navigation was initiated inside the current frame; false if the parent navigator is also navigating.</returns>
	public bool IsNavigationInitiator => _isNavigationInitiator;

	/// <summary>Gets an optional user-defined data object. </summary>
	/// <returns>The data object.</returns>
	public object ExtraData => _extraData;

	/// <summary>Gets the Web response to allow access to HTTP headers and other properties. </summary>
	/// <returns>The Web response.</returns>
	public WebResponse WebResponse => _webResponse;

	/// <summary>Gets the navigator that raised the event </summary>
	/// <returns>The navigator that raised the event.</returns>
	public object Navigator => _navigator;

	internal NavigationEventArgs(Uri uri, object content, object extraData, WebResponse response, object Navigator, bool isNavigationInitiator)
	{
		_uri = uri;
		_content = content;
		_extraData = extraData;
		_webResponse = response;
		_isNavigationInitiator = isNavigationInitiator;
		_navigator = Navigator;
	}
}
