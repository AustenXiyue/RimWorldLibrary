using System.Threading;

namespace System;

/// <summary>Provides an <see cref="T:System.IProgress`1" /> that invokes callbacks for each reported progress value.</summary>
/// <typeparam name="T">Specifies the type of the progress report value.</typeparam>
public class Progress<T> : IProgress<T>
{
	private readonly SynchronizationContext m_synchronizationContext;

	private readonly Action<T> m_handler;

	private readonly SendOrPostCallback m_invokeHandlers;

	/// <summary>Raised for each reported progress value.</summary>
	public event EventHandler<T> ProgressChanged;

	/// <summary>Initializes the <see cref="T:System.Progress`1" /> object.</summary>
	public Progress()
	{
		m_synchronizationContext = SynchronizationContext.CurrentNoFlow ?? ProgressStatics.DefaultContext;
		m_invokeHandlers = InvokeHandlers;
	}

	/// <summary>Initializes the <see cref="T:System.Progress`1" /> object with the specified callback.</summary>
	/// <param name="handler">A handler to invoke for each reported progress value. This handler will be invoked in addition to any delegates registered with the <see cref="E:System.Progress`1.ProgressChanged" /> event. Depending on the <see cref="T:System.Threading.SynchronizationContext" /> instance captured by the <see cref="T:System.Progress`1" /> at construction, it is possible that this handler instance could be invoked concurrently with itself.</param>
	public Progress(Action<T> handler)
		: this()
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		m_handler = handler;
	}

	/// <summary>Reports a progress change.</summary>
	/// <param name="value">The value of the updated progress.</param>
	protected virtual void OnReport(T value)
	{
		Action<T> handler = m_handler;
		EventHandler<T> eventHandler = this.ProgressChanged;
		if (handler != null || eventHandler != null)
		{
			m_synchronizationContext.Post(m_invokeHandlers, value);
		}
	}

	/// <summary>Reports a progress change.</summary>
	/// <param name="value">The value of the updated progress.</param>
	void IProgress<T>.Report(T value)
	{
		OnReport(value);
	}

	private void InvokeHandlers(object state)
	{
		T val = (T)state;
		Action<T> handler = m_handler;
		EventHandler<T> eventHandler = this.ProgressChanged;
		handler?.Invoke(val);
		eventHandler?.Invoke(this, val);
	}
}
