namespace System.Windows.Threading;

/// <summary>Represents an awaitable object that asynchronously yields control back to the current dispatcher and provides an opportunity for the dispatcher to process other events</summary>
public struct DispatcherPriorityAwaitable
{
	private readonly Dispatcher _dispatcher;

	private readonly DispatcherPriority _priority;

	internal DispatcherPriorityAwaitable(Dispatcher dispatcher, DispatcherPriority priority)
	{
		_dispatcher = dispatcher;
		_priority = priority;
	}

	/// <summary>Returns an object that waits for the completion of an asynchronous task.</summary>
	/// <returns>An object that waits for the completion of an asynchronous task.</returns>
	public DispatcherPriorityAwaiter GetAwaiter()
	{
		return new DispatcherPriorityAwaiter(_dispatcher, _priority);
	}
}
