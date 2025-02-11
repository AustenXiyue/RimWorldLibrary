using System;

namespace MS.Internal.Data;

internal class AsyncDataRequest
{
	private AsyncRequestStatus _status;

	private object _result;

	private object _bindingState;

	private object[] _args;

	private Exception _exception;

	private AsyncRequestCallback _workCallback;

	private AsyncRequestCallback _completedCallback;

	private readonly object SyncRoot = new object();

	public object Result => _result;

	public AsyncRequestStatus Status => _status;

	public Exception Exception => _exception;

	internal object[] Args => _args;

	internal AsyncDataRequest(object bindingState, AsyncRequestCallback workCallback, AsyncRequestCallback completedCallback, params object[] args)
	{
		_bindingState = bindingState;
		_workCallback = workCallback;
		_completedCallback = completedCallback;
		_args = args;
	}

	public object DoWork()
	{
		if (DoBeginWork() && _workCallback != null)
		{
			return _workCallback(this);
		}
		return null;
	}

	public bool DoBeginWork()
	{
		return ChangeStatus(AsyncRequestStatus.Working);
	}

	public void Complete(object result)
	{
		if (ChangeStatus(AsyncRequestStatus.Completed))
		{
			_result = result;
			if (_completedCallback != null)
			{
				_completedCallback(this);
			}
		}
	}

	public void Cancel()
	{
		ChangeStatus(AsyncRequestStatus.Cancelled);
	}

	public void Fail(Exception exception)
	{
		if (ChangeStatus(AsyncRequestStatus.Failed))
		{
			_exception = exception;
			if (_completedCallback != null)
			{
				_completedCallback(this);
			}
		}
	}

	private bool ChangeStatus(AsyncRequestStatus newStatus)
	{
		bool flag = false;
		lock (SyncRoot)
		{
			switch (newStatus)
			{
			case AsyncRequestStatus.Working:
				flag = _status == AsyncRequestStatus.Waiting;
				break;
			case AsyncRequestStatus.Completed:
				flag = _status == AsyncRequestStatus.Working;
				break;
			case AsyncRequestStatus.Cancelled:
				flag = _status == AsyncRequestStatus.Waiting || _status == AsyncRequestStatus.Working;
				break;
			case AsyncRequestStatus.Failed:
				flag = _status == AsyncRequestStatus.Working;
				break;
			}
			if (flag)
			{
				_status = newStatus;
			}
		}
		return flag;
	}
}
