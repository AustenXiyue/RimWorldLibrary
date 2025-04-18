using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq.Expressions;

internal sealed class StackGuard
{
	private const int MaxExecutionStackCount = 1024;

	private int _executionStackCount;

	public bool TryEnterOnCurrentStack()
	{
		if (RuntimeHelpers.TryEnsureSufficientExecutionStack())
		{
			return true;
		}
		if (_executionStackCount < 1024)
		{
			return false;
		}
		throw new InsufficientExecutionStackException();
	}

	public void RunOnEmptyStack<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
	{
		RunOnEmptyStackCore(delegate(object s)
		{
			Tuple<Action<T1, T2>, T1, T2> tuple = (Tuple<Action<T1, T2>, T1, T2>)s;
			tuple.Item1(tuple.Item2, tuple.Item3);
			return (object)null;
		}, Tuple.Create(action, arg1, arg2));
	}

	public void RunOnEmptyStack<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
	{
		RunOnEmptyStackCore(delegate(object s)
		{
			Tuple<Action<T1, T2, T3>, T1, T2, T3> tuple = (Tuple<Action<T1, T2, T3>, T1, T2, T3>)s;
			tuple.Item1(tuple.Item2, tuple.Item3, tuple.Item4);
			return (object)null;
		}, Tuple.Create(action, arg1, arg2, arg3));
	}

	public R RunOnEmptyStack<T1, T2, R>(Func<T1, T2, R> action, T1 arg1, T2 arg2)
	{
		return RunOnEmptyStackCore(delegate(object s)
		{
			Tuple<Func<T1, T2, R>, T1, T2> tuple = (Tuple<Func<T1, T2, R>, T1, T2>)s;
			return tuple.Item1(tuple.Item2, tuple.Item3);
		}, Tuple.Create(action, arg1, arg2));
	}

	public R RunOnEmptyStack<T1, T2, T3, R>(Func<T1, T2, T3, R> action, T1 arg1, T2 arg2, T3 arg3)
	{
		return RunOnEmptyStackCore(delegate(object s)
		{
			Tuple<Func<T1, T2, T3, R>, T1, T2, T3> tuple = (Tuple<Func<T1, T2, T3, R>, T1, T2, T3>)s;
			return tuple.Item1(tuple.Item2, tuple.Item3, tuple.Item4);
		}, Tuple.Create(action, arg1, arg2, arg3));
	}

	private R RunOnEmptyStackCore<R>(Func<object, R> action, object state)
	{
		_executionStackCount++;
		try
		{
			Task<R> task = Task.Factory.StartNew(action, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
			TaskAwaiter<R> awaiter = task.GetAwaiter();
			if (!awaiter.IsCompleted)
			{
				((IAsyncResult)task).AsyncWaitHandle.WaitOne();
			}
			return awaiter.GetResult();
		}
		finally
		{
			_executionStackCount--;
		}
	}
}
