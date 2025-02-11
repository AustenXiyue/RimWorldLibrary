using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace System.Windows.Documents;

internal sealed class PageContentAsyncResult : IAsyncResult
{
	internal enum GetPageStatus
	{
		Loading,
		Cancelled,
		Finished
	}

	private object _asyncState;

	private bool _isCompleted;

	private bool _completedSynchronously;

	private AsyncCallback _callback;

	private Exception _exception;

	private GetPageStatus _getpageStatus;

	private Uri _baseUri;

	private Uri _source;

	private FixedPage _child;

	private Dispatcher _dispatcher;

	private FixedPage _result;

	private Stream _pendingStream;

	private DispatcherOperation _dispatcherOperation;

	public object AsyncState => _asyncState;

	public WaitHandle AsyncWaitHandle => null;

	public bool CompletedSynchronously => _completedSynchronously;

	public bool IsCompleted => _isCompleted;

	internal Exception Exception => _exception;

	internal bool IsCancelled => _getpageStatus == GetPageStatus.Cancelled;

	internal DispatcherOperation DispatcherOperation
	{
		set
		{
			_dispatcherOperation = value;
		}
	}

	internal FixedPage Result => _result;

	internal PageContentAsyncResult(AsyncCallback callback, object state, Dispatcher dispatcher, Uri baseUri, Uri source, FixedPage child)
	{
		_dispatcher = dispatcher;
		_isCompleted = false;
		_completedSynchronously = false;
		_callback = callback;
		_asyncState = state;
		_getpageStatus = GetPageStatus.Loading;
		_child = child;
		_baseUri = baseUri;
		_source = source;
	}

	internal object Dispatch(object arg)
	{
		if (_exception != null)
		{
			_getpageStatus = GetPageStatus.Finished;
		}
		switch (_getpageStatus)
		{
		case GetPageStatus.Loading:
			try
			{
				if (_child != null)
				{
					_completedSynchronously = true;
					_result = _child;
					_getpageStatus = GetPageStatus.Finished;
				}
				else
				{
					PageContent._LoadPageImpl(_baseUri, _source, out _result, out var pageStream);
					if (_result == null || _result.IsInitialized)
					{
						pageStream.Close();
					}
					else
					{
						_pendingStream = pageStream;
						_result.Initialized += _OnPaserFinished;
					}
					_getpageStatus = GetPageStatus.Finished;
				}
			}
			catch (ApplicationException exception)
			{
				_exception = exception;
			}
			goto case GetPageStatus.Cancelled;
		case GetPageStatus.Cancelled:
		case GetPageStatus.Finished:
			_isCompleted = true;
			if (_callback != null)
			{
				_callback(this);
			}
			break;
		}
		return null;
	}

	internal void Cancel()
	{
		_getpageStatus = GetPageStatus.Cancelled;
	}

	internal void Wait()
	{
		_dispatcherOperation.Wait();
	}

	private void _OnPaserFinished(object sender, EventArgs args)
	{
		if (_pendingStream != null)
		{
			_pendingStream.Close();
			_pendingStream = null;
		}
	}
}
