using System.Threading;

namespace System.Windows.Threading;

internal class ExceptionWrapper
{
	public delegate bool CatchHandler(object source, Exception e);

	public delegate bool FilterHandler(object source, Exception e);

	public event CatchHandler Catch;

	public event FilterHandler Filter;

	internal ExceptionWrapper()
	{
	}

	public object TryCatchWhen(object source, Delegate callback, object args, int numArgs, Delegate catchHandler)
	{
		object result = null;
		try
		{
			result = InternalRealCall(callback, args, numArgs);
		}
		catch (Exception e) when (FilterException(source, e))
		{
			if (!CatchException(source, e, catchHandler))
			{
				throw;
			}
		}
		return result;
	}

	private object InternalRealCall(Delegate callback, object args, int numArgs)
	{
		object result = null;
		int num = numArgs;
		object obj = args;
		if (numArgs == -1)
		{
			object[] array = (object[])args;
			if (array == null || array.Length == 0)
			{
				num = 0;
			}
			else if (array.Length == 1)
			{
				num = 1;
				obj = array[0];
			}
		}
		switch (num)
		{
		case 0:
			if (callback is Action action)
			{
				action();
			}
			else if (callback is Dispatcher.ShutdownCallback shutdownCallback)
			{
				shutdownCallback();
			}
			else
			{
				result = callback.DynamicInvoke();
			}
			break;
		case 1:
			if (callback is DispatcherOperationCallback dispatcherOperationCallback)
			{
				result = dispatcherOperationCallback(obj);
			}
			else if (!(callback is SendOrPostCallback sendOrPostCallback))
			{
				result = ((numArgs != -1) ? callback.DynamicInvoke(args) : callback.DynamicInvoke((object[])args));
			}
			else
			{
				sendOrPostCallback(obj);
			}
			break;
		default:
			result = callback.DynamicInvoke((object[])args);
			break;
		}
		return result;
	}

	private bool FilterException(object source, Exception e)
	{
		bool result = this.Catch != null;
		if (this.Filter != null)
		{
			result = this.Filter(source, e);
		}
		return result;
	}

	private bool CatchException(object source, Exception e, Delegate catchHandler)
	{
		if ((object)catchHandler != null)
		{
			if (catchHandler is DispatcherOperationCallback)
			{
				((DispatcherOperationCallback)catchHandler)(null);
			}
			else
			{
				catchHandler.DynamicInvoke(null);
			}
		}
		if (this.Catch != null)
		{
			return this.Catch(source, e);
		}
		return false;
	}
}
