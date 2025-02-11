namespace System.Threading;

/// <summary>Encapsulates and propagates the host execution context across threads. </summary>
/// <filterpriority>2</filterpriority>
[MonoTODO("Useless until the runtime supports it")]
public class HostExecutionContext : IDisposable
{
	private object _state;

	/// <summary>Gets or sets the state of the host execution context.</summary>
	/// <returns>An object representing the host execution context state.</returns>
	protected internal object State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.HostExecutionContext" /> class. </summary>
	public HostExecutionContext()
	{
		_state = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.HostExecutionContext" /> class using the specified state. </summary>
	/// <param name="state">An object representing the host execution context state.</param>
	public HostExecutionContext(object state)
	{
		_state = state;
	}

	/// <summary>Creates a copy of the current host execution context.</summary>
	/// <returns>A <see cref="T:System.Threading.HostExecutionContext" /> object representing the host context for the current thread.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual HostExecutionContext CreateCopy()
	{
		return new HostExecutionContext(_state);
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.HostExecutionContext" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>When overridden in a derived class, releases the unmanaged resources used by the <see cref="T:System.Threading.WaitHandle" />, and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	public virtual void Dispose(bool disposing)
	{
	}
}
