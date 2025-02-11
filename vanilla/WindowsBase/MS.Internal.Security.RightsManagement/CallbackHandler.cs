using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MS.Internal.Security.RightsManagement;

internal sealed class CallbackHandler : IDisposable
{
	private CallbackDelegate _callbackDelegate;

	private AutoResetEvent _resetEvent;

	private string _callbackData;

	private int _hr;

	private Exception _exception;

	private const uint S_DRM_COMPLETED = 315140u;

	internal CallbackDelegate CallbackDelegate => _callbackDelegate;

	internal string CallbackData => _callbackData;

	internal CallbackHandler()
	{
		_resetEvent = new AutoResetEvent(initialState: false);
		_callbackDelegate = OnStatus;
	}

	internal void WaitForCompletion()
	{
		_resetEvent.WaitOne();
		if (_exception != null)
		{
			throw _exception;
		}
		Errors.ThrowOnErrorCode(_hr);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private int OnStatus(StatusMessage status, int hr, nint pvParam, nint pvContext)
	{
		if ((long)hr == 315140 || hr < 0)
		{
			_exception = null;
			try
			{
				_hr = hr;
				if (pvParam != IntPtr.Zero)
				{
					_callbackData = Marshal.PtrToStringUni(pvParam);
				}
			}
			catch (Exception exception)
			{
				_exception = exception;
			}
			finally
			{
				_resetEvent.Set();
			}
		}
		return 0;
	}

	private void Dispose(bool disposing)
	{
		if (disposing && _resetEvent != null)
		{
			_resetEvent.Set();
			((IDisposable)_resetEvent).Dispose();
		}
	}
}
