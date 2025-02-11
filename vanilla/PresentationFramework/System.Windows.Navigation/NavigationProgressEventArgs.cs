namespace System.Windows.Navigation;

/// <summary>Provides data for the <see cref="E:System.Windows.Application.NavigationProgress" /> and <see cref="E:System.Windows.Navigation.NavigationWindow.NavigationProgress" /> events. </summary>
public class NavigationProgressEventArgs : EventArgs
{
	private Uri _uri;

	private long _bytesRead;

	private long _maxBytes;

	private object _navigator;

	/// <summary>Gets the uniform resource identifier (URI) of the target page. </summary>
	/// <returns>The URI of the target page.</returns>
	public Uri Uri => _uri;

	/// <summary>Gets the number of bytes that have been read. </summary>
	/// <returns>The number of bytes that have been read.</returns>
	public long BytesRead => _bytesRead;

	/// <summary>Gets the maximum number of bytes. </summary>
	/// <returns>The maximum number of bytes.</returns>
	public long MaxBytes => _maxBytes;

	/// <summary>Gets the navigator that raised the event </summary>
	/// <returns>The navigator that raised the event</returns>
	public object Navigator => _navigator;

	internal NavigationProgressEventArgs(Uri uri, long bytesRead, long maxBytes, object Navigator)
	{
		_uri = uri;
		_bytesRead = bytesRead;
		_maxBytes = maxBytes;
		_navigator = Navigator;
	}
}
