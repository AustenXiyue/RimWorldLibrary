using System.Threading.Tasks;

namespace System.Windows.Threading;

internal abstract class DispatcherOperationTaskSource
{
	public abstract void Initialize(DispatcherOperation operation);

	public abstract Task GetTask();

	public abstract void SetCanceled();

	public abstract void SetResult(object result);

	public abstract void SetException(Exception exception);
}
internal class DispatcherOperationTaskSource<TResult> : DispatcherOperationTaskSource
{
	private TaskCompletionSource<TResult> _taskCompletionSource;

	public override void Initialize(DispatcherOperation operation)
	{
		if (_taskCompletionSource != null)
		{
			throw new InvalidOperationException();
		}
		_taskCompletionSource = new TaskCompletionSource<TResult>(new DispatcherOperationTaskMapping(operation));
	}

	public override Task GetTask()
	{
		if (_taskCompletionSource == null)
		{
			throw new InvalidOperationException();
		}
		return _taskCompletionSource.Task;
	}

	public override void SetCanceled()
	{
		if (_taskCompletionSource == null)
		{
			throw new InvalidOperationException();
		}
		_taskCompletionSource.SetCanceled();
	}

	public override void SetResult(object result)
	{
		if (_taskCompletionSource == null)
		{
			throw new InvalidOperationException();
		}
		_taskCompletionSource.SetResult((TResult)result);
	}

	public override void SetException(Exception exception)
	{
		if (_taskCompletionSource == null)
		{
			throw new InvalidOperationException();
		}
		_taskCompletionSource.SetException(exception);
	}
}
