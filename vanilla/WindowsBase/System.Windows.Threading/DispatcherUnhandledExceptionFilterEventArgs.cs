namespace System.Windows.Threading;

/// <summary>Provides data for the <see cref="T:System.Windows.Threading.Dispatcher" /> <see cref="E:System.Windows.Threading.Dispatcher.UnhandledExceptionFilter" /> event.</summary>
public sealed class DispatcherUnhandledExceptionFilterEventArgs : DispatcherEventArgs
{
	private Exception _exception;

	private bool _requestCatch;

	/// <summary>Gets the exception that was raised when executing code by way of the dispatcher.</summary>
	/// <returns>The exception.</returns>
	public Exception Exception => _exception;

	/// <summary>Gets or sets whether the exception should be caught and the event handlers called. </summary>
	/// <returns>true if the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledException" /> should be raised; otherwise; false.  The default value is true.</returns>
	public bool RequestCatch
	{
		get
		{
			return _requestCatch;
		}
		set
		{
			if (!value)
			{
				_requestCatch = value;
			}
		}
	}

	internal DispatcherUnhandledExceptionFilterEventArgs(Dispatcher dispatcher)
		: base(dispatcher)
	{
	}

	internal void Initialize(Exception exception, bool requestCatch)
	{
		_exception = exception;
		_requestCatch = requestCatch;
	}
}
