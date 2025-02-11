using System.Net;

namespace System.Windows.Navigation;

/// <summary>Provides data for the NavigationFailed event.</summary>
public class NavigationFailedEventArgs : EventArgs
{
	private Uri _uri;

	private object _extraData;

	private object _navigator;

	private WebRequest _request;

	private WebResponse _response;

	private Exception _exception;

	private bool _handled;

	/// <summary>Gets the uniform resource identifier (URI) for the content that could not be navigated to.</summary>
	/// <returns>The <see cref="T:System.Uri" /> for the content that could not be navigated to.</returns>
	public Uri Uri => _uri;

	/// <summary>Gets the optional data <see cref="T:System.Object" /> that was passed when navigation commenced.</summary>
	/// <returns>The optional data <see cref="T:System.Object" /> that was passed when navigation commenced.</returns>
	public object ExtraData => _extraData;

	/// <summary>The navigator that raised this event.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is the navigator that raised this event</returns>
	public object Navigator => _navigator;

	/// <summary>Gets the web request that was used to request the specified content.</summary>
	/// <returns>Gets the <see cref="T:System.Net.WebRequest" /> object that was used to request the specified content. If navigating to an object, <see cref="P:System.Windows.Navigation.NavigationFailedEventArgs.WebRequest" /> is null.</returns>
	public WebRequest WebRequest => _request;

	/// <summary>Gets the web response that was returned after attempting to download the requested the specified content.</summary>
	/// <returns>The <see cref="T:System.Net.WebResponse" /> that was returned after attempting to download the requested the specified content. If the navigation failed, <see cref="P:System.Windows.Navigation.NavigationFailedEventArgs.WebResponse" /> is null.</returns>
	public WebResponse WebResponse => _response;

	/// <summary>Gets the <see cref="T:System.Exception" /> that was raised as the result of a failed navigation.</summary>
	/// <returns>The <see cref="T:System.Exception" /> that was raised as the result of a failed navigation.</returns>
	public Exception Exception => _exception;

	/// <summary>Gets or sets whether the failed navigation exception has been handled.</summary>
	/// <returns>true if the exception is handled; otherwise, false (default).</returns>
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

	internal NavigationFailedEventArgs(Uri uri, object extraData, object navigator, WebRequest request, WebResponse response, Exception e)
	{
		_uri = uri;
		_extraData = extraData;
		_navigator = navigator;
		_request = request;
		_response = response;
		_exception = e;
	}
}
