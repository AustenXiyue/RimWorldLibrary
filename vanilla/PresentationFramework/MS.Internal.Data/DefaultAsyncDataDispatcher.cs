using System;
using System.Collections;
using System.Threading;
using System.Windows;

namespace MS.Internal.Data;

internal class DefaultAsyncDataDispatcher : IAsyncDataDispatcher
{
	private ArrayList _list = new ArrayList();

	void IAsyncDataDispatcher.AddRequest(AsyncDataRequest request)
	{
		lock (_list.SyncRoot)
		{
			_list.Add(request);
		}
		ThreadPool.QueueUserWorkItem(ProcessRequest, request);
	}

	void IAsyncDataDispatcher.CancelAllRequests()
	{
		lock (_list.SyncRoot)
		{
			for (int i = 0; i < _list.Count; i++)
			{
				((AsyncDataRequest)_list[i]).Cancel();
			}
			_list.Clear();
		}
	}

	private void ProcessRequest(object o)
	{
		AsyncDataRequest asyncDataRequest = (AsyncDataRequest)o;
		try
		{
			asyncDataRequest.Complete(asyncDataRequest.DoWork());
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
			asyncDataRequest.Fail(ex);
		}
		catch
		{
			asyncDataRequest.Fail(new InvalidOperationException(SR.Format(SR.NonCLSException, "processing an async data request")));
		}
		lock (_list.SyncRoot)
		{
			_list.Remove(asyncDataRequest);
		}
	}
}
