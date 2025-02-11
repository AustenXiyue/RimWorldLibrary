namespace System.Threading.Tasks;

/// <summary>Provides data for the event that is raised when a faulted <see cref="T:System.Threading.Tasks.Task" />'s exception goes unobserved.</summary>
public class UnobservedTaskExceptionEventArgs : EventArgs
{
	private AggregateException m_exception;

	internal bool m_observed;

	/// <summary>Gets whether this exception has been marked as "observed."</summary>
	/// <returns>true if this exception has been marked as "observed"; otherwise false.</returns>
	public bool Observed => m_observed;

	/// <summary>The Exception that went unobserved.</summary>
	/// <returns>The Exception that went unobserved.</returns>
	public AggregateException Exception => m_exception;

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.UnobservedTaskExceptionEventArgs" /> class with the unobserved exception.</summary>
	/// <param name="exception">The Exception that has gone unobserved.</param>
	public UnobservedTaskExceptionEventArgs(AggregateException exception)
	{
		m_exception = exception;
	}

	/// <summary>Marks the <see cref="P:System.Threading.Tasks.UnobservedTaskExceptionEventArgs.Exception" /> as "observed," thus preventing it from triggering exception escalation policy which, by default, terminates the process.</summary>
	public void SetObserved()
	{
		m_observed = true;
	}
}
