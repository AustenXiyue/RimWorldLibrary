using System.Runtime.CompilerServices;
using MS.Internal.WindowsBase;

namespace System.Windows.Threading;

/// <summary>Represents an object that waits for the completion of an asynchronous task.</summary>
public struct DispatcherPriorityAwaiter : INotifyCompletion
{
	private readonly Dispatcher _dispatcher;

	private readonly DispatcherPriority _priority;

	/// <summary>Gets a value that indicates whether the asynchronous task has completed.</summary>
	/// <returns>false in all cases.</returns>
	public bool IsCompleted => false;

	internal DispatcherPriorityAwaiter(Dispatcher dispatcher, DispatcherPriority priority)
	{
		_dispatcher = dispatcher;
		_priority = priority;
	}

	/// <summary>Ends the wait for the completion of the asynchronous task.</summary>
	public void GetResult()
	{
	}

	/// <summary>Sets the action to perform when the <see cref="T:System.Windows.Threading.DispatcherPriorityAwaiter" /> object stops waiting for the asynchronous task to complete.</summary>
	/// <param name="continuation">The action to perform when the wait operation completes.</param>
	public void OnCompleted(Action continuation)
	{
		if (_dispatcher == null)
		{
			throw new InvalidOperationException(SR.DispatcherPriorityAwaiterInvalid);
		}
		_dispatcher.InvokeAsync(continuation, _priority);
	}
}
