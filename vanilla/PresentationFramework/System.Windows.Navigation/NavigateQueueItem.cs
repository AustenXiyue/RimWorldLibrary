using System.Windows.Threading;

namespace System.Windows.Navigation;

internal class NavigateQueueItem
{
	private Uri _source;

	private object _content;

	private object _navState;

	private NavigationService _nc;

	private NavigationMode _navigationMode;

	private DispatcherOperation _postedOp;

	internal Uri Source => _source;

	internal object NavState => _navState;

	internal NavigateQueueItem(Uri source, object content, NavigationMode mode, object navState, NavigationService nc)
	{
		_source = source;
		_content = content;
		_navState = navState;
		_nc = nc;
		_navigationMode = mode;
	}

	internal void PostNavigation()
	{
		_postedOp = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(Dispatch), null);
	}

	internal void Stop()
	{
		if (_postedOp != null)
		{
			_postedOp.Abort();
			_postedOp = null;
		}
	}

	private object Dispatch(object obj)
	{
		_postedOp = null;
		if (_content != null || _source == null)
		{
			_nc.DoNavigate(_content, _navigationMode, _navState);
		}
		else
		{
			_nc.DoNavigate(_source, _navigationMode, _navState);
		}
		return null;
	}
}
