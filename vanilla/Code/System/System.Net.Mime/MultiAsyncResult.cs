using System.Threading;

namespace System.Net.Mime;

internal class MultiAsyncResult : LazyAsyncResult
{
	private int outstanding;

	private object context;

	internal object Context => context;

	internal MultiAsyncResult(object context, AsyncCallback callback, object state)
		: base(context, state, callback)
	{
		this.context = context;
	}

	internal void Enter()
	{
		Increment();
	}

	internal void Leave()
	{
		Decrement();
	}

	internal void Leave(object result)
	{
		base.Result = result;
		Decrement();
	}

	private void Decrement()
	{
		if (Interlocked.Decrement(ref outstanding) == -1)
		{
			InvokeCallback(base.Result);
		}
	}

	private void Increment()
	{
		Interlocked.Increment(ref outstanding);
	}

	internal void CompleteSequence()
	{
		Decrement();
	}

	internal static object End(IAsyncResult result)
	{
		MultiAsyncResult obj = (MultiAsyncResult)result;
		obj.InternalWaitForCompletion();
		return obj.Result;
	}
}
