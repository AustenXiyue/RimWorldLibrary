using System.Net;
using System.Windows.Threading;

namespace System.Windows.Navigation;

internal class RequestState
{
	private WebRequest _request;

	private Uri _source;

	private object _navState;

	private Dispatcher _callbackDispatcher;

	internal WebRequest Request => _request;

	internal Uri Source => _source;

	internal object NavState => _navState;

	internal Dispatcher CallbackDispatcher => _callbackDispatcher;

	internal RequestState(WebRequest request, Uri source, object navState, Dispatcher callbackDispatcher)
	{
		_request = request;
		_source = source;
		_navState = navState;
		_callbackDispatcher = callbackDispatcher;
	}
}
