using System.ComponentModel;

namespace System.Windows.Navigation;

internal class BPReadyEventArgs : CancelEventArgs
{
	private object _content;

	private Uri _uri;

	internal object Content => _content;

	internal Uri Uri => _uri;

	internal BPReadyEventArgs(object content, Uri uri)
	{
		_content = content;
		_uri = uri;
	}
}
