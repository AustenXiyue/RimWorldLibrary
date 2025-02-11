namespace System.Windows.Threading;

/// <summary>Provides event data for <see cref="T:System.Windows.Threading.Dispatcher" /> related events. </summary>
public class DispatcherEventArgs : EventArgs
{
	private Dispatcher _dispatcher;

	/// <summary>The <see cref="T:System.Windows.Threading.Dispatcher" /> associated with this event. </summary>
	/// <returns>The dispatcher.</returns>
	public Dispatcher Dispatcher => _dispatcher;

	internal DispatcherEventArgs(Dispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}
}
